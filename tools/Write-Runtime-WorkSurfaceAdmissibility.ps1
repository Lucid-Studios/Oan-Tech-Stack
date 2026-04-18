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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$cmeFormationAndOfficeLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeFormationAndOfficeLedgerStatePath)
$masterThreadOrchestrationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.masterThreadOrchestrationStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before runtime work-surface admissibility can run.'
}

$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$cmeFormationAndOfficeLedgerState = Read-JsonFileOrNull -Path $cmeFormationAndOfficeLedgerStatePath
$masterThreadOrchestrationState = Read-JsonFileOrNull -Path $masterThreadOrchestrationStatePath

$admissibilityState = 'withheld'
$reasonCode = 'runtime-work-surface-admissibility-withheld'
$nextAction = 'derive-runtime-readiness'

$readinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'readinessState')
$officeLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cmeFormationAndOfficeLedgerState -PropertyName 'officeLedgerState')
$orchestrationState = [string] (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'orchestrationState')
$eligibleTargetBucketLabels = if ($null -ne $masterThreadOrchestrationState) { @($masterThreadOrchestrationState.eligibleTargetBucketLabels) } else { @() }

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $admissibilityState = 'withheld'
    $reasonCode = 'runtime-work-surface-admissibility-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $sanctuaryRuntimeReadinessState) {
    $admissibilityState = 'withheld'
    $reasonCode = 'runtime-work-surface-admissibility-readiness-missing'
    $nextAction = 'emit-sanctuary-runtime-readiness'
} elseif ($readinessState -ne 'bounded-working-state-ready') {
    $admissibilityState = 'withheld'
    $reasonCode = 'runtime-work-surface-admissibility-readiness-not-earned'
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'nextAction')
} elseif ($officeLedgerState -in @('Provisional', 'Open')) {
    $admissibilityState = 'provisional-runtime-work'
    $reasonCode = 'runtime-work-surface-admissibility-provisional-office'
    $nextAction = 'continue-bounded-runtime-work'
} else {
    $admissibilityState = 'bounded-internal-work-only'
    $reasonCode = 'runtime-work-surface-admissibility-bounded-internal'
    $nextAction = 'limit-work-to-bounded-internal-surfaces'
}

$admissibleSurfaces = @()
if ($admissibilityState -in @('bounded-internal-work-only', 'provisional-runtime-work')) {
    $admissibleSurfaces = @(
        [ordered]@{
            surfaceName = 'Sanctuary Candidate Runtime Session'
            locality = 'Sanctuary.actual'
            admissibility = 'Admissible'
            requiredState = 'bounded-working-state-ready'
            witnessSurface = 'sanctuary-runtime-readiness-receipt'
        },
        [ordered]@{
            surfaceName = 'Headless Runtime Host Verification'
            locality = 'Oan.Runtime.Headless'
            admissibility = 'Admissible'
            requiredState = 'deployable-candidate-ready'
            witnessSurface = 'runtime-deployability-envelope'
        },
        [ordered]@{
            surfaceName = 'SLI Nexus Witness Work'
            locality = 'SLI/Nexus'
            admissibility = 'Admissible'
            requiredState = 'provisional-or-open-office'
            witnessSurface = 'cme-formation-office-ledger'
        },
        [ordered]@{
            surfaceName = 'Bucket-Scoped Automation And Orchestration'
            locality = 'Automation/Infrastructure'
            admissibility = 'Admissible'
            requiredState = 'weighted-or-polling-or-better'
            witnessSurface = 'master-thread-orchestration'
        }
    )
}

$deniedSurfaces = @(
    [ordered]@{
        surfaceName = 'Bonded Operator Actual Runtime'
        denialReason = 'operator-actual-not-yet-co-realized'
        requiredOfficeOrState = 'ratified-reach-and-operator-actual-contract'
    },
    [ordered]@{
        surfaceName = 'Deep Self-Rooted Cryptic Descent'
        denialReason = 'deep-cryptic-descent-not-yet-self-rooted'
        requiredOfficeOrState = 'cryptic-biad-self-instantiation'
    },
    [ordered]@{
        surfaceName = 'Reach Co-Realized Participation Surface'
        denialReason = 'reach-contract-not-yet-bound'
        requiredOfficeOrState = 'bounded-reach-realization-contract'
    },
    [ordered]@{
        surfaceName = 'Ratified Publication Lane'
        denialReason = 'publication-remains-hitl-ratified'
        requiredOfficeOrState = 'publication-ratification'
    },
    [ordered]@{
        surfaceName = 'Ambient Cross-Locality Agentic Access'
        denialReason = 'ambient-cross-locality-access-prohibited'
        requiredOfficeOrState = 'explicit-gate-grant-through-host-law'
    }
)

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'runtime-work-surface-admissibility.json'
$bundleMarkdownPath = Join-Path $bundlePath 'runtime-work-surface-admissibility.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    admissibilityState = $admissibilityState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sanctuaryRuntimeReadinessState = $readinessState
    cmeOfficeLedgerState = $officeLedgerState
    orchestrationState = $orchestrationState
    eligibleTargetBucketLabels = $eligibleTargetBucketLabels
    admissibleSurfaces = $admissibleSurfaces
    deniedSurfaces = $deniedSurfaces
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Runtime Work Surface Admissibility',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Admissibility state: `{0}`' -f $payload.admissibilityState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Sanctuary runtime readiness state: `{0}`' -f $(if ($payload.sanctuaryRuntimeReadinessState) { $payload.sanctuaryRuntimeReadinessState } else { 'missing' })),
    ('- CME office ledger state: `{0}`' -f $(if ($payload.cmeOfficeLedgerState) { $payload.cmeOfficeLedgerState } else { 'missing' })),
    ('- Orchestration state: `{0}`' -f $(if ($payload.orchestrationState) { $payload.orchestrationState } else { 'missing' })),
    ('- Eligible target buckets: `{0}`' -f $(if (@($eligibleTargetBucketLabels).Count -gt 0) { (@($eligibleTargetBucketLabels) -join ', ') } else { 'none' })),
    '',
    '## Admissible Surfaces',
    ''
)

foreach ($surface in $admissibleSurfaces) {
    $markdownLines += @(
        ('### {0}' -f [string] $surface.surfaceName),
        ('- Locality: `{0}`' -f [string] $surface.locality),
        ('- Admissibility: `{0}`' -f [string] $surface.admissibility),
        ('- Required state: `{0}`' -f [string] $surface.requiredState),
        ('- Witness surface: `{0}`' -f [string] $surface.witnessSurface),
        ''
    )
}

if (@($admissibleSurfaces).Count -eq 0) {
    $markdownLines += @(
        '- none-yet',
        ''
    )
}

$markdownLines += @(
    '## Denied Surfaces',
    ''
)

foreach ($surface in $deniedSurfaces) {
    $markdownLines += @(
        ('### {0}' -f [string] $surface.surfaceName),
        ('- Denial reason: `{0}`' -f [string] $surface.denialReason),
        ('- Required office or state: `{0}`' -f [string] $surface.requiredOfficeOrState),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    admissibilityState = $payload.admissibilityState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    sanctuaryRuntimeReadinessState = $payload.sanctuaryRuntimeReadinessState
    cmeOfficeLedgerState = $payload.cmeOfficeLedgerState
    orchestrationState = $payload.orchestrationState
    admissibleSurfaceCount = @($admissibleSurfaces).Count
    deniedSurfaceCount = @($deniedSurfaces).Count
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[runtime-work-surface-admissibility] Bundle: {0}' -f $bundlePath)
$bundlePath
