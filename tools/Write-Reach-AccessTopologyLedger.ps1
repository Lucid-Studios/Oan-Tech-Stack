param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.1.1/build/local-automation-cycle.json'
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

function Read-BundleJsonFromStateOrNull {
    param(
        [string] $RepoRootPath,
        [object] $State,
        [string] $JsonFileName
    )

    if ($null -eq $State) {
        return $null
    }

    $bundlePath = [string] (Get-ObjectPropertyValueOrNull -InputObject $State -PropertyName 'bundlePath')
    if ([string]::IsNullOrWhiteSpace($bundlePath)) {
        return $null
    }

    $resolvedBundlePath = Resolve-PathFromRepo -BasePath $RepoRootPath -CandidatePath $bundlePath
    $bundleJsonPath = Join-Path $resolvedBundlePath $JsonFileName
    return Read-JsonFileOrNull -Path $bundleJsonPath
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$runtimeDeployabilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeStatePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$masterThreadOrchestrationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.masterThreadOrchestrationStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before reach access-topology ledger can run.'
}

$runtimeDeployabilityState = Read-JsonFileOrNull -Path $runtimeDeployabilityStatePath
$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$masterThreadOrchestrationState = Read-JsonFileOrNull -Path $masterThreadOrchestrationStatePath

$runtimeDeployabilityBundle = Read-BundleJsonFromStateOrNull -RepoRootPath $resolvedRepoRoot -State $runtimeDeployabilityState -JsonFileName 'runtime-deployability-envelope.json'
$runtimeWorkBundle = Read-BundleJsonFromStateOrNull -RepoRootPath $resolvedRepoRoot -State $runtimeWorkSurfaceAdmissibilityState -JsonFileName 'runtime-work-surface-admissibility.json'

$ledgerState = 'awaiting-runtime-legibility'
$reasonCode = 'reach-access-topology-ledger-awaiting-runtime-legibility'
$nextAction = 'emit-runtime-work-surface-admissibility'

$readinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'readinessState')
$runtimeAdmissibilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'admissibilityState')

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ledgerState = 'blocked'
    $reasonCode = 'reach-access-topology-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $runtimeWorkSurfaceAdmissibilityState -or $null -eq $runtimeWorkBundle) {
    $ledgerState = 'awaiting-runtime-legibility'
    $reasonCode = 'reach-access-topology-ledger-runtime-surface-missing'
    $nextAction = 'emit-runtime-work-surface-admissibility'
} elseif ($readinessState -ne 'bounded-working-state-ready') {
    $ledgerState = 'awaiting-runtime-legibility'
    $reasonCode = 'reach-access-topology-ledger-runtime-readiness-not-earned'
    $nextAction = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.nextAction } else { 'emit-sanctuary-runtime-readiness' }
} else {
    $ledgerState = 'provisional-reach-legibility'
    $reasonCode = 'reach-access-topology-ledger-provisional-runtime-legibility'
    $nextAction = 'bind-reach-duplex-realization-seam'
}

$disclosedSurfaces = @(
    foreach ($surface in @($runtimeWorkBundle.admissibleSurfaces)) {
        [ordered]@{
            surfaceName = [string] $surface.surfaceName
            locality = [string] $surface.locality
            disclosureClass = 'disclosure-only'
            witnessSurface = [string] $surface.witnessSurface
            requiredState = [string] $surface.requiredState
        }
    }
)

$deniedSurfaces = @(
    foreach ($surface in @($runtimeWorkBundle.deniedSurfaces)) {
        [ordered]@{
            surfaceName = [string] $surface.surfaceName
            denialReason = [string] $surface.denialReason
            requiredOfficeOrState = [string] $surface.requiredOfficeOrState
        }
    }
)

$inventoryEntries = @(
    foreach ($candidate in @($runtimeDeployabilityBundle.deployableCandidates)) {
        [ordered]@{
            name = [string] $candidate.name
            family = [string] $candidate.family
            artifactPresent = [bool] $candidate.artifactPresent
            promotable = [bool] $candidate.promotable
            publishLane = [string] $candidate.publishLane
        }
    }
)

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'reach-access-topology-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'reach-access-topology-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    ledgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    runtimeAdmissibilityState = $runtimeAdmissibilityState
    sanctuaryRuntimeReadinessState = $readinessState
    accessGrantAuthority = 'host-law-and-nexus-only'
    eligibleTargetBucketLabels = if ($null -ne $masterThreadOrchestrationState) { @($masterThreadOrchestrationState.eligibleTargetBucketLabels) } else { @() }
    disclosedSurfaces = $disclosedSurfaces
    deniedSurfaces = $deniedSurfaces
    inventoryEntries = $inventoryEntries
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Reach Access Topology Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.ledgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Runtime admissibility state: `{0}`' -f $(if ($payload.runtimeAdmissibilityState) { $payload.runtimeAdmissibilityState } else { 'missing' })),
    ('- Sanctuary runtime readiness state: `{0}`' -f $(if ($payload.sanctuaryRuntimeReadinessState) { $payload.sanctuaryRuntimeReadinessState } else { 'missing' })),
    ('- Access grant authority: `{0}`' -f $payload.accessGrantAuthority),
    ('- Eligible target buckets: `{0}`' -f $(if (@($payload.eligibleTargetBucketLabels).Count -gt 0) { (@($payload.eligibleTargetBucketLabels) -join ', ') } else { 'none' })),
    '',
    '## Disclosed Surfaces',
    ''
)

foreach ($surface in $disclosedSurfaces) {
    $markdownLines += @(
        ('### {0}' -f [string] $surface.surfaceName),
        ('- Locality: `{0}`' -f [string] $surface.locality),
        ('- Disclosure class: `{0}`' -f [string] $surface.disclosureClass),
        ('- Required state: `{0}`' -f [string] $surface.requiredState),
        ('- Witness surface: `{0}`' -f [string] $surface.witnessSurface),
        ''
    )
}

if (@($disclosedSurfaces).Count -eq 0) {
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

$markdownLines += @(
    '## Inventory',
    ''
)

foreach ($entry in $inventoryEntries) {
    $markdownLines += @(
        ('### {0}' -f [string] $entry.name),
        ('- Family: `{0}`' -f [string] $entry.family),
        ('- Publish lane: `{0}`' -f [string] $entry.publishLane),
        ('- Promotable: `{0}`' -f [bool] $entry.promotable),
        ('- Artifact present: `{0}`' -f [bool] $entry.artifactPresent),
        ''
    )
}

if (@($inventoryEntries).Count -eq 0) {
    $markdownLines += @(
        '- none-yet',
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    ledgerState = $payload.ledgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    runtimeAdmissibilityState = $payload.runtimeAdmissibilityState
    sanctuaryRuntimeReadinessState = $payload.sanctuaryRuntimeReadinessState
    disclosedSurfaceCount = @($disclosedSurfaces).Count
    deniedSurfaceCount = @($deniedSurfaces).Count
    inventoryEntryCount = @($inventoryEntries).Count
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[reach-access-topology-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
