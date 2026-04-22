param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/Automation/local-automation-cycle.json'
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

$automationCascadePromptHelperPath = Join-Path $PSScriptRoot 'Automation-CascadePrompt.ps1'
. $automationCascadePromptHelperPath
$resolverHelperPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
. $resolverHelperPath
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

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    if (Test-Path -LiteralPath $resolvedBase -PathType Leaf) {
        $resolvedBase = Split-Path -Parent $resolvedBase
    }

    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri(($resolvedBase.TrimEnd('\') + '\'))
    $targetUri = New-Object System.Uri($resolvedTarget)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace('\', '/')
}

function Read-JsonFileOrNull {
    param([string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }

    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Write-JsonFile {
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
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

function Test-PathUnderRoot {
    param(
        [string] $Path,
        [string] $RootPath
    )

    if ([string]::IsNullOrWhiteSpace($Path) -or [string]::IsNullOrWhiteSpace($RootPath)) {
        return $false
    }

    $resolvedPath = [System.IO.Path]::GetFullPath($Path)
    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd('\')
    return (
        [string]::Equals($resolvedPath, $resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        $resolvedPath.StartsWith($resolvedRoot + '\', [System.StringComparison]::OrdinalIgnoreCase)
    )
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$v111EnrichmentPathwayStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.v111EnrichmentPathwayStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runIsolatedBuildPathwayOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runIsolatedBuildPathwayStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the run-isolated build pathway can run.'
}

$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$v111EnrichmentPathwayState = Read-JsonFileOrNull -Path $v111EnrichmentPathwayStatePath
$seededGovernanceAdmission = Get-SeededGovernanceBuildAdmission -SeededGovernanceState $seededGovernanceState -CyclePolicy $cyclePolicy
$matrix = Read-OanWorkspaceTouchPointMatrix -BasePath $resolvedRepoRoot -CyclePolicy $cyclePolicy
if ($null -eq $matrix -or $null -eq $matrix.touchPoints) {
    throw 'Versioned touch-point matrix is required before the run-isolated build pathway can run.'
}

$context = Get-OanWorkspaceContext -BasePath $resolvedRepoRoot -CyclePolicy $cyclePolicy
$activeBuildRootPath = Join-OanWorkspacePath -BasePath $resolvedRepoRoot -CandidatePath $context.ActiveBuildRoot
$legacyWorkspaceRootPath = Join-OanWorkspacePath -BasePath $resolvedRepoRoot -CandidatePath $context.LegacyWorkspaceRoot

$touchPointSummaries = @()
$remainingLegacyTouchPoints = @()
$unresolvedTouchPoints = @()
$lawfulExclusions = @()
$researchHandOffTouchPoints = @()

foreach ($touchPointProperty in $matrix.touchPoints.PSObject.Properties) {
    $touchPointId = [string] $touchPointProperty.Name
    $laneClass = Get-OanWorkspaceTouchPointLaneClass -BasePath $resolvedRepoRoot -TouchPointId $touchPointId -CyclePolicy $cyclePolicy
    $resolution = Get-OanWorkspaceTouchPointResolution -BasePath $resolvedRepoRoot -TouchPointId $touchPointId -CyclePolicy $cyclePolicy
    $selectedPath = Resolve-OanWorkspaceTouchPoint -BasePath $resolvedRepoRoot -TouchPointId $touchPointId -CyclePolicy $cyclePolicy
    $selectedExists = $false
    if (-not [string]::IsNullOrWhiteSpace($selectedPath)) {
        $selectedExists = Test-Path -LiteralPath $selectedPath
    }

    $selectedDisposition = 'unresolved'
    switch ($laneClass) {
        'policy-lane' {
            if ($selectedExists) {
                $selectedDisposition = 'policy-lane'
            }
        }
        'documentation-lane' {
            if ($selectedExists) {
                $selectedDisposition = 'documentation-lane'
            }
        }
        'research-lane' {
            $selectedDisposition = if ($selectedExists) { 'research-materialized' } else { 'research-handoff' }
        }
        default {
            if ($selectedExists) {
                if (Test-PathUnderRoot -Path $selectedPath -RootPath $activeBuildRootPath) {
                    $selectedDisposition = 'active-build'
                } elseif (Test-PathUnderRoot -Path $selectedPath -RootPath $legacyWorkspaceRootPath) {
                    $selectedDisposition = 'legacy-fallback'
                }
            }
        }
    }

    $summary = [ordered]@{
        touchPointId = $touchPointId
        laneClass = $laneClass
        selectedDisposition = $selectedDisposition
        selectedPath = if ($selectedExists) { Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $selectedPath } else { $null }
        selectedPathExists = $selectedExists
        touchPointStatus = if ($null -ne $resolution) { [string] $resolution.TouchPointStatus } else { $null }
        researchBucketLabel = if ($null -ne $resolution) { [string] $resolution.ResearchBucketLabel } else { $null }
        researchReason = if ($null -ne $resolution) { [string] $resolution.ResearchReason } else { $null }
        fallbackPath = if ($null -ne $resolution -and -not [string]::IsNullOrWhiteSpace([string] $resolution.FallbackPath)) {
            Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath ([string] $resolution.FallbackPath)
        } else {
            $null
        }
        candidatePaths = @(
            if ($null -ne $resolution) {
                foreach ($candidate in @($resolution.Candidates)) {
                    if (-not [string]::IsNullOrWhiteSpace([string] $candidate)) {
                        Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath ([string] $candidate)
                    }
                }
            }
        )
    }
    $touchPointSummaries += [pscustomobject] $summary

    switch ($selectedDisposition) {
        'legacy-fallback' {
            $remainingLegacyTouchPoints += [pscustomobject] $summary
        }
        'unresolved' {
            $unresolvedTouchPoints += [pscustomobject] $summary
        }
        'policy-lane' {
            $lawfulExclusions += [pscustomobject] $summary
        }
        'documentation-lane' {
            $lawfulExclusions += [pscustomobject] $summary
        }
        'research-handoff' {
            $researchHandOffTouchPoints += [pscustomobject] $summary
        }
        'research-materialized' {
            $researchHandOffTouchPoints += [pscustomobject] $summary
        }
    }
}

$buildLaneTouchPoints = @($touchPointSummaries | Where-Object { [string] $_.laneClass -eq 'build-lane' })
$activeBuildTouchPoints = @($buildLaneTouchPoints | Where-Object { [string] $_.selectedDisposition -eq 'active-build' })
$policyLaneTouchPointCount = @($lawfulExclusions | Where-Object { [string] $_.laneClass -eq 'policy-lane' }).Count
$documentationLaneTouchPointCount = @($lawfulExclusions | Where-Object { [string] $_.laneClass -eq 'documentation-lane' }).Count
$researchHandOffTouchPointCount = @($researchHandOffTouchPoints).Count
$remainingLegacyTouchPointCount = @($remainingLegacyTouchPoints).Count
$unresolvedTouchPointCount = @($unresolvedTouchPoints).Count
$legacyFree = ($remainingLegacyTouchPointCount -eq 0 -and $unresolvedTouchPointCount -eq 0)
$seedDisposition = [string] $seededGovernanceAdmission.disposition
$seedReadyState = [string] $seededGovernanceAdmission.readyState
$seededGovernanceBuildAdmissionState = [string] $seededGovernanceAdmission.buildAdmissionState
$seededGovernanceBuildAdmissionReason = [string] $seededGovernanceAdmission.buildAdmissionReason
$seededGovernanceBuildAdmitted = [bool] $seededGovernanceAdmission.buildAdmissionIsAdmitted
$seededGovernanceClarifyRequired = [bool] $seededGovernanceAdmission.buildAdmissionClarifyRequired

$pathwayState = 'awaiting-v111-enrichment-pathway'
$reasonCode = 'run-isolated-build-pathway-v111-missing'
$nextAction = 'emit-v111-enrichment-pathway'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $pathwayState = 'blocked'
    $reasonCode = 'run-isolated-build-pathway-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($seededGovernanceClarifyRequired) {
    $pathwayState = 'clarify-seeded-governance-admission'
    $reasonCode = 'run-isolated-build-pathway-build-admission-unresolved'
    $nextAction = 'clarify-seeded-governance-admission-before-continuing'
} elseif ($null -eq $v111EnrichmentPathwayState) {
    $pathwayState = 'awaiting-v111-enrichment-pathway'
    $reasonCode = 'run-isolated-build-pathway-v111-missing'
    $nextAction = 'emit-v111-enrichment-pathway'
} elseif ([string] $v111EnrichmentPathwayState.pathwayState -ne 'v111-enrichment-path-open') {
    $pathwayState = 'awaiting-v111-enrichment-open'
    $reasonCode = 'run-isolated-build-pathway-v111-not-open'
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $v111EnrichmentPathwayState -PropertyName 'nextAction')
    if ([string]::IsNullOrWhiteSpace($nextAction)) {
        $nextAction = 'continue-v111-enrichment-admission'
    }
} elseif ($unresolvedTouchPointCount -gt 0) {
    $pathwayState = 'touchpoint-resolution-incomplete'
    $reasonCode = 'run-isolated-build-pathway-touchpoints-unresolved'
    $nextAction = 'classify-and-resolve-unresolved-touchpoints'
} elseif ($remainingLegacyTouchPointCount -gt 0) {
    $pathwayState = 'legacy-fallbacks-remain'
    $reasonCode = 'run-isolated-build-pathway-legacy-fallbacks-remain'
    $nextAction = 'migrate-remaining-build-touchpoints-to-v111'
} else {
    $pathwayState = 'run-isolated-build-ready'
    $reasonCode = 'run-isolated-build-pathway-ready'
    $nextAction = 'continue-run-isolated-v111-build'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'run-isolated-build-pathway.json'
$bundleMarkdownPath = Join-Path $bundlePath 'run-isolated-build-pathway.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    pathwayState = $pathwayState
    reasonCode = $reasonCode
    nextAction = $nextAction
    legacyFree = $legacyFree
    activeBuildRoot = $context.ActiveBuildRoot
    automationPolicyRoot = $context.AutomationPolicyRoot
    seededGovernanceDisposition = $seedDisposition
    seededGovernanceReadyState = $seedReadyState
    seededGovernanceBuildAdmissionState = $seededGovernanceBuildAdmissionState
    seededGovernanceBuildAdmissionReason = $seededGovernanceBuildAdmissionReason
    seededGovernanceBuildAdmitted = $seededGovernanceBuildAdmitted
    seededGovernanceClarifyRequired = $seededGovernanceClarifyRequired
    v111EnrichmentPathwayState = if ($null -ne $v111EnrichmentPathwayState) { [string] $v111EnrichmentPathwayState.pathwayState } else { $null }
    v111EnrichmentNextAction = if ($null -ne $v111EnrichmentPathwayState) { [string] $v111EnrichmentPathwayState.nextAction } else { $null }
    buildLaneTouchPointCount = @($buildLaneTouchPoints).Count
    activeBuildTouchPointCount = @($activeBuildTouchPoints).Count
    remainingLegacyTouchPointCount = $remainingLegacyTouchPointCount
    unresolvedTouchPointCount = $unresolvedTouchPointCount
    lawfulExclusionTouchPointCount = @($lawfulExclusions).Count
    researchHandOffTouchPointCount = $researchHandOffTouchPointCount
    lawfulExclusionBreakdown = [ordered]@{
        policyLane = $policyLaneTouchPointCount
        documentationLane = $documentationLaneTouchPointCount
    }
    remainingLegacyTouchPointIds = @($remainingLegacyTouchPoints | ForEach-Object { [string] $_.touchPointId })
    unresolvedTouchPointIds = @($unresolvedTouchPoints | ForEach-Object { [string] $_.touchPointId })
    researchHandOffTouchPointIds = @($researchHandOffTouchPoints | ForEach-Object { [string] $_.touchPointId })
    remainingLegacyTouchPoints = $remainingLegacyTouchPoints
    unresolvedTouchPoints = $unresolvedTouchPoints
    researchHandOffTouchPoints = $researchHandOffTouchPoints
    lawfulExclusions = $lawfulExclusions
    touchPoints = $touchPointSummaries
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $payload | Out-Null

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Run-Isolated Build Pathway',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Pathway state: `{0}`' -f $payload.pathwayState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Legacy free: `{0}`' -f [bool] $payload.legacyFree),
    ('- Active build root: `{0}`' -f $payload.activeBuildRoot),
    ('- Automation policy root: `{0}`' -f $payload.automationPolicyRoot),
    ('- Seeded governance disposition: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'missing' })),
    ('- Seeded governance ready state: `{0}`' -f $(if ($payload.seededGovernanceReadyState) { $payload.seededGovernanceReadyState } else { 'missing' })),
    ('- Seeded governance build admission state: `{0}`' -f $(if ($payload.seededGovernanceBuildAdmissionState) { $payload.seededGovernanceBuildAdmissionState } else { 'missing' })),
    ('- Seeded governance build admission reason: `{0}`' -f $(if ($payload.seededGovernanceBuildAdmissionReason) { $payload.seededGovernanceBuildAdmissionReason } else { 'missing' })),
    ('- Seeded governance build admitted: `{0}`' -f [bool] $payload.seededGovernanceBuildAdmitted),
    ('- Seeded governance clarify required: `{0}`' -f [bool] $payload.seededGovernanceClarifyRequired),
    ('- V1.1.1 enrichment pathway state: `{0}`' -f $(if ($payload.v111EnrichmentPathwayState) { $payload.v111EnrichmentPathwayState } else { 'missing' })),
    ('- Build-lane touchpoints: `{0}`' -f $payload.buildLaneTouchPointCount),
    ('- Active-build touchpoints: `{0}`' -f $payload.activeBuildTouchPointCount),
    ('- Remaining legacy fallbacks: `{0}`' -f $payload.remainingLegacyTouchPointCount),
    ('- Unresolved touchpoints: `{0}`' -f $payload.unresolvedTouchPointCount),
    ('- Research handoffs: `{0}`' -f $payload.researchHandOffTouchPointCount),
    ('- Lawful exclusions: `{0}`' -f $payload.lawfulExclusionTouchPointCount),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' }))
)

if (@($remainingLegacyTouchPoints).Count -gt 0) {
    $markdownLines += @(
        '',
        '## Remaining Legacy Fallbacks',
        ''
    )

    foreach ($touchPoint in @($remainingLegacyTouchPoints)) {
        $markdownLines += ('- `{0}` -> `{1}`' -f [string] $touchPoint.touchPointId, [string] $touchPoint.selectedPath)
    }
}

if (@($researchHandOffTouchPoints).Count -gt 0) {
    $markdownLines += @(
        '',
        '## Research HandOffs',
        ''
    )

    foreach ($touchPoint in @($researchHandOffTouchPoints)) {
        $markdownLines += ('- `{0}` -> bucket `{1}` ({2})' -f [string] $touchPoint.touchPointId, [string] $touchPoint.researchBucketLabel, [string] $touchPoint.researchReason)
    }
}

if (@($lawfulExclusions).Count -gt 0) {
    $markdownLines += @(
        '',
        '## Lawful Exclusions',
        '',
        ('- Policy-lane exclusions: `{0}`' -f $policyLaneTouchPointCount),
        ('- Documentation-lane exclusions: `{0}`' -f $documentationLaneTouchPointCount)
    )
}

if (@($unresolvedTouchPoints).Count -gt 0) {
    $markdownLines += @(
        '',
        '## Unresolved Touchpoints',
        ''
    )

    foreach ($touchPoint in @($unresolvedTouchPoints)) {
        $markdownLines += ('- `{0}`' -f [string] $touchPoint.touchPointId)
    }
}

$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines
Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    pathwayState = $payload.pathwayState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    legacyFree = $payload.legacyFree
    seededGovernanceDisposition = $payload.seededGovernanceDisposition
    seededGovernanceReadyState = $payload.seededGovernanceReadyState
    seededGovernanceBuildAdmissionState = $payload.seededGovernanceBuildAdmissionState
    seededGovernanceBuildAdmissionReason = $payload.seededGovernanceBuildAdmissionReason
    seededGovernanceBuildAdmitted = $payload.seededGovernanceBuildAdmitted
    seededGovernanceClarifyRequired = $payload.seededGovernanceClarifyRequired
    buildLaneTouchPointCount = $payload.buildLaneTouchPointCount
    activeBuildTouchPointCount = $payload.activeBuildTouchPointCount
    remainingLegacyTouchPointCount = $payload.remainingLegacyTouchPointCount
    unresolvedTouchPointCount = $payload.unresolvedTouchPointCount
    researchHandOffTouchPointCount = $payload.researchHandOffTouchPointCount
    lawfulExclusionTouchPointCount = $payload.lawfulExclusionTouchPointCount
    remainingLegacyTouchPointIds = $payload.remainingLegacyTouchPointIds
    unresolvedTouchPointIds = $payload.unresolvedTouchPointIds
    researchHandOffTouchPointIds = $payload.researchHandOffTouchPointIds
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statePayload | Out-Null

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[run-isolated-build-pathway] Bundle: {0}' -f $bundlePath)
$bundlePath
