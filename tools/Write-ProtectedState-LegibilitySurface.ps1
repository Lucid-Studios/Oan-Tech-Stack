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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$reachAccessTopologyLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedStateLegibilitySurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedStateLegibilitySurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before protected-state legibility can run.'
}

$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$reachAccessTopologyLedgerState = Read-JsonFileOrNull -Path $reachAccessTopologyLedgerStatePath

$legibilityState = 'awaiting-runtime-legibility'
$reasonCode = 'protected-state-legibility-awaiting-runtime-legibility'
$nextAction = 'emit-runtime-work-surface-admissibility'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $legibilityState = 'blocked'
    $reasonCode = 'protected-state-legibility-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $runtimeWorkSurfaceAdmissibilityState) {
    $legibilityState = 'awaiting-runtime-legibility'
    $reasonCode = 'protected-state-legibility-runtime-surface-missing'
    $nextAction = 'emit-runtime-work-surface-admissibility'
} else {
    $legibilityState = 'bounded-legibility-ready'
    $reasonCode = 'protected-state-legibility-bounded-runtime-receipts-present'
    $nextAction = 'bind-singular-nexus-portal-for-centralized-legibility'
}

$visibleSignals = @(
    [ordered]@{
        signalName = 'Sanctuary Runtime Readiness'
        value = if ($null -ne $sanctuaryRuntimeReadinessState) { [string] $sanctuaryRuntimeReadinessState.readinessState } else { 'missing' }
        disclosureClass = 'governed-legibility'
    },
    [ordered]@{
        signalName = 'Runtime Work Surface Admissibility'
        value = if ($null -ne $runtimeWorkSurfaceAdmissibilityState) { [string] $runtimeWorkSurfaceAdmissibilityState.admissibilityState } else { 'missing' }
        disclosureClass = 'governed-legibility'
    },
    [ordered]@{
        signalName = 'Reach Access Topology Ledger'
        value = if ($null -ne $reachAccessTopologyLedgerState) { [string] $reachAccessTopologyLedgerState.ledgerState } else { 'missing' }
        disclosureClass = 'governed-legibility'
    },
    [ordered]@{
        signalName = 'Protected Interiority'
        value = 'withheld'
        disclosureClass = 'protected-boundary'
    }
)

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'protected-state-legibility-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'protected-state-legibility-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    legibilityState = $legibilityState
    reasonCode = $reasonCode
    nextAction = $nextAction
    protectedInteriorityExposure = 'withheld'
    visibleSignals = $visibleSignals
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Protected State Legibility Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Legibility state: `{0}`' -f $payload.legibilityState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Protected interiority exposure: `{0}`' -f $payload.protectedInteriorityExposure),
    '',
    '## Visible Signals',
    ''
)

foreach ($signal in $visibleSignals) {
    $markdownLines += @(
        ('### {0}' -f [string] $signal.signalName),
        ('- Value: `{0}`' -f [string] $signal.value),
        ('- Disclosure class: `{0}`' -f [string] $signal.disclosureClass),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    legibilityState = $payload.legibilityState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    protectedInteriorityExposure = $payload.protectedInteriorityExposure
    visibleSignalCount = @($visibleSignals).Count
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[protected-state-legibility-surface] Bundle: {0}' -f $bundlePath)
$bundlePath
