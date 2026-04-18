param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json',
    [string] $FederationPolicyPath = 'OAN Mortalis V1.1.1/Automation/source-bucket-federation.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $RepoRoot = Split-Path -Parent $PSScriptRoot
    } else {
        $RepoRoot = (Get-Location).Path
    }
}

$seededGovernanceAdmissionHelperPath = Join-Path $PSScriptRoot 'Seeded-GovernanceAdmission.ps1'
. $seededGovernanceAdmissionHelperPath

function Resolve-PathFromRepo {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Read-JsonFile {
    param([string] $Path)

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

    if ($null -eq $InputObject) {
        return $null
    }

    $property = $InputObject.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function New-UniqueStringArray {
    param([object[]] $Values)

    $items = New-Object System.Collections.Generic.List[string]
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($value in @($Values)) {
        if ($null -eq $value) {
            continue
        }

        $stringValue = [string] $value
        if ([string]::IsNullOrWhiteSpace($stringValue)) {
            continue
        }

        if ($seen.Add($stringValue)) {
            [void] $items.Add($stringValue)
        }
    }

    return [string[]] $items.ToArray()
}

function Get-ChildScriptOutputTail {
    param([object[]] $Output)

    return @($Output | ForEach-Object { "$_".Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedFederationPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $FederationPolicyPath

$cyclePolicy = Read-JsonFile -Path $resolvedCyclePolicyPath
$federationPolicy = Read-JsonFile -Path $resolvedFederationPolicyPath
$requestContractPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.requestContractPath)
$requestContract = Read-JsonFile -Path $requestContractPath
$matrixPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.versionedTouchPointMatrixPath)
$matrix = Read-JsonFile -Path $matrixPath

$masterThreadStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.readOnlyUpstreamTelemetryPaths[0])
$taskStatusStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.readOnlyUpstreamTelemetryPaths[1])
$v111EnrichmentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $federationPolicy.readOnlyUpstreamTelemetryPaths[2])
$runIsolatedPathwayStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runIsolatedBuildPathwayStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)

$masterThreadState = Read-JsonFileOrNull -Path $masterThreadStatePath
$taskStatus = Read-JsonFileOrNull -Path $taskStatusStatePath
$v111EnrichmentState = Read-JsonFileOrNull -Path $v111EnrichmentStatePath
$runIsolatedPathwayState = Read-JsonFileOrNull -Path $runIsolatedPathwayStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$seededGovernanceAdmission = Get-SeededGovernanceBuildAdmission -SeededGovernanceState $seededGovernanceState -CyclePolicy $cyclePolicy

$seedDisposition = [string] $seededGovernanceAdmission.disposition
$seedReadyState = [string] $seededGovernanceAdmission.readyState
$seededGovernanceBuildAdmitted = [bool] $seededGovernanceAdmission.buildAdmissionIsAdmitted
$seededGovernanceClarifyRequired = [bool] $seededGovernanceAdmission.buildAdmissionClarifyRequired
$currentPosture = Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'currentPosture'
$gateReconciliation = Get-ObjectPropertyValueOrNull -InputObject $masterThreadState -PropertyName 'gateReconciliation'
$gateMismatchDetected = [bool] (Get-ObjectPropertyValueOrNull -InputObject $gateReconciliation -PropertyName 'gateMismatchDetected')
$taskStatusValue = [string] (Get-ObjectPropertyValueOrNull -InputObject $taskStatus -PropertyName 'lastKnownStatus')
if ([string]::IsNullOrWhiteSpace($taskStatusValue)) {
    $taskStatusValue = [string] (Get-ObjectPropertyValueOrNull -InputObject $currentPosture -PropertyName 'status')
}
$v111PathwayState = [string] (Get-ObjectPropertyValueOrNull -InputObject $v111EnrichmentState -PropertyName 'pathwayState')
$runIsolatedPathwayStateValue = [string] (Get-ObjectPropertyValueOrNull -InputObject $runIsolatedPathwayState -PropertyName 'pathwayState')

$allowRequestPublishing = $true
if ($taskStatusValue -eq 'blocked' -or $gateMismatchDetected) {
    $allowRequestPublishing = $false
}

$touchPointGroups = @{}
foreach ($touchPointProperty in $matrix.touchPoints.PSObject.Properties) {
    $touchPointId = [string] $touchPointProperty.Name
    $touchPoint = $touchPointProperty.Value
    if ([string] (Get-ObjectPropertyValueOrNull -InputObject $touchPoint -PropertyName 'status') -ne 'research-handoff') {
        continue
    }

    $bucketLabel = [string] (Get-ObjectPropertyValueOrNull -InputObject $touchPoint -PropertyName 'researchBucketLabel')
    if ([string]::IsNullOrWhiteSpace($bucketLabel)) {
        continue
    }

    if (-not $touchPointGroups.ContainsKey($bucketLabel)) {
        $touchPointGroups[$bucketLabel] = New-Object System.Collections.Generic.List[object]
    }

    [void] $touchPointGroups[$bucketLabel].Add([pscustomobject]@{
        touchPointId = $touchPointId
        researchReason = [string] (Get-ObjectPropertyValueOrNull -InputObject $touchPoint -PropertyName 'researchReason')
    })
}

$newRequestScriptPath = Join-Path $resolvedRepoRoot 'tools\New-SourceBucket-WorkRequest.ps1'
$statusScriptPath = Join-Path $resolvedRepoRoot 'tools\Write-SourceBucket-FederationStatus.ps1'

if ($allowRequestPublishing) {
    foreach ($bucketLabel in @($requestContract.targetBucketLabels)) {
        $bucketKey = [string] $bucketLabel
        if (-not $touchPointGroups.ContainsKey($bucketKey)) {
            continue
        }

        $touchPoints = @($touchPointGroups[$bucketKey] | ForEach-Object { $_ })
        if ($touchPoints.Count -eq 0) {
            continue
        }

        $touchPointIds = New-UniqueStringArray -Values @($touchPoints | ForEach-Object { [string] $_.touchPointId })
        $buildSurface = 'OAN Mortalis V1.1.1 carry-forward research handoff'
        $subject = 'V1.1.1 build-directed research condensate'
        $predicate = 'return build-facing refinement without widening runtime or CME authority'
        $actions = @(
            'Review the linked V1.1.1 research-handoff touchpoints as one bounded source-bucket seam.'
            'Return only receipted doctrine, contracts, packet summaries, or refinement guidance that survives current build law.'
            'Keep live runtime, CME, ListeningFrame, and tool-authority widening withheld.'
        )
        $neededReturnClass = 'spec-now'

        switch ($bucketLabel) {
            'IUTT SLI & Lisp' {
                $buildSurface = 'V1.1.1 cognition-runtime condensate'
                $subject = 'IUTT SLI & Lisp actualization and nexus refinement'
                $predicate = 'condense research-handoff runtime law into bounded build-facing spec'
                $actions = @(
                    'Review the linked actualization, reach, runtime, and nexus touchpoints as one bounded cognition-runtime seam.'
                    'Return only receipted spec-now or frame-now condensate that can guide later V1.1.1 admission.'
                    'Do not imply live Agenti runtime, local LLM preflight authority, or CME embodiment from this return.'
                )
            }
            'Trivium Forum' {
                $buildSurface = 'V1.1.1 operator-workflow condensate'
                $subject = 'Trivium Forum workbench and worker governance refinement'
                $predicate = 'condense workflow and conference governance law into bounded build-facing spec'
                $actions = @(
                    'Review the linked workbench, worker-thread, and workflow governance touchpoints as one bounded operator-workflow seam.'
                    'Return only receipted spec-now or frame-now condensate that clarifies future build admission surfaces.'
                    'Do not imply live workbench runtime, worker authority, or cross-repo execution from this return.'
                )
            }
        }

        $withholdRules = @(
            'No direct runtime widening from source-bucket output.',
            'No CME placement, ListeningFrame widening, or tool-authority widening.',
            'No raw external repo drift treated as build authority.',
            'HITL remains required before any promotion or runtime admission boundary is crossed.'
        )

        if ((-not $seededGovernanceBuildAdmitted) -and ($seededGovernanceClarifyRequired -or $v111PathwayState -eq 'clarify-seeded-governance-admission' -or $runIsolatedPathwayStateValue -eq 'clarify-seeded-governance-admission')) {
            $withholdRules += 'Seeded-governance admission remains clarified but unpromoted; keep the return on frame-now/spec-now surfaces only.'
        }

        $evidenceLinks = @(
            [string] $cyclePolicy.versionedTouchPointMatrixPath,
            [string] $federationPolicy.formalSurfaceMarkdownPath,
            [string] $federationPolicy.buildReadinessMarkdownPath,
            [string] $federationPolicy.enrichmentPathwayMarkdownPath
        )

        $requestOutput = & $newRequestScriptPath `
            -RepoRoot $resolvedRepoRoot `
            -RequestContractPath $requestContractPath `
            -TargetBucketLabel $bucketLabel `
            -BuildSurface $buildSurface `
            -Subject $subject `
            -Predicate $predicate `
            -Actions $actions `
            -NeededReturnClass $neededReturnClass `
            -EvidenceLinks $evidenceLinks `
            -AdmissibilityClass 'review_required' `
            -HitlState 'required_before_promotion' `
            -WithholdRules $withholdRules `
            -SourceTouchPointIds $touchPointIds `
            -RequestKey ('research-handoff::{0}::{1}' -f $bucketLabel, (@($touchPointIds) -join ','))

        $null = Get-ChildScriptOutputTail -Output $requestOutput
    }
}

$statusOutput = & $statusScriptPath -RepoRoot $resolvedRepoRoot -CyclePolicyPath $resolvedCyclePolicyPath -FederationPolicyPath $resolvedFederationPolicyPath
$statusPath = Get-ChildScriptOutputTail -Output $statusOutput
if (-not [string]::IsNullOrWhiteSpace($statusPath)) {
    Write-Host ('[source-bucket-federation-cycle] Status: {0}' -f $statusPath)
}

$statusPath
