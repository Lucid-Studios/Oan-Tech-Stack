param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json',
    [string] $TaskingPolicyPath = 'OAN Mortalis V1.0/build/local-automation-tasking.json'
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

    $Value | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $Path -Encoding utf8
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$resolvedTaskingPolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $TaskingPolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$taskingPolicy = Get-Content -Raw -LiteralPath $resolvedTaskingPolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$activeRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $taskingPolicy.activeLongFormRunStatePath)
$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$unattendedProofCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedProofCollapseStatePath)
$dormantWindowLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dormantWindowLedgerStatePath)
$silentCadenceIntegrityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.silentCadenceIntegrityStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormPhaseWitnessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.longFormPhaseWitnessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$activeRun = Read-JsonFileOrNull -Path $activeRunStatePath
if ($null -eq $cycleState -or $null -eq $activeRun) {
    throw 'Cycle state and active long-form run state are required before long-form phase witness can run.'
}

$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$unattendedProofCollapseState = Read-JsonFileOrNull -Path $unattendedProofCollapseStatePath
$dormantWindowLedgerState = Read-JsonFileOrNull -Path $dormantWindowLedgerStatePath
$silentCadenceIntegrityState = Read-JsonFileOrNull -Path $silentCadenceIntegrityStatePath

$witnessState = 'candidate-only'
$reasonCode = 'long-form-phase-witness-candidate-only'
$nextAction = 'continue-candidate-automation'
$targetPhaseId = 'structure-01'
$targetPhaseLabel = 'Exploratory Structure 1'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $witnessState = 'blocked'
    $reasonCode = 'long-form-phase-witness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($null -eq $unattendedProofCollapseState -or $null -eq $dormantWindowLedgerState -or $null -eq $silentCadenceIntegrityState) {
    $witnessState = 'awaiting-evidence'
    $reasonCode = 'long-form-phase-witness-evidence-missing'
    $nextAction = 'complete-map-11-prerequisites'
} elseif ([string] $unattendedProofCollapseState.collapseState -like 'collapsed-*') {
    $witnessState = 'final-collapsed-proof'
    $reasonCode = 'long-form-phase-witness-collapsed-proof'
    $nextAction = 'collapse-active-run'
    $targetPhaseId = 'structure-04'
    $targetPhaseLabel = 'Final Collapsed Structure 4'
} elseif ([int] $dormantWindowLedgerState.consecutiveDormantWindows -ge 2) {
    $witnessState = 'deep-dormant-exploration'
    $reasonCode = 'long-form-phase-witness-deep-dormant'
    $nextAction = 'advance-exploratory-phase'
    $targetPhaseId = 'structure-03'
    $targetPhaseLabel = 'Exploratory Structure 3'
} elseif ([int] $dormantWindowLedgerState.consecutiveDormantWindows -ge 1) {
    $witnessState = 'dormant-exploration'
    $reasonCode = 'long-form-phase-witness-dormant'
    $nextAction = 'advance-exploratory-phase'
    $targetPhaseId = 'structure-02'
    $targetPhaseLabel = 'Exploratory Structure 2'
} elseif ([string] $schedulerExecutionReceiptState.receiptState -eq 'awaiting-scheduler-run') {
    $witnessState = 'initial-exploration'
    $reasonCode = 'long-form-phase-witness-initial'
    $nextAction = 'allow-scheduled-cycle-to-fire'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$runKey = if (-not [string]::IsNullOrWhiteSpace([string] $activeRun.runId)) { [string] $activeRun.runId } else { 'no-run' }
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $runKey)
$bundleJsonPath = Join-Path $bundlePath 'long-form-phase-witness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'long-form-phase-witness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    witnessState = $witnessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    activeRunId = [string] $activeRun.runId
    currentPhaseId = [string] $activeRun.currentPhaseId
    currentPhaseLabel = [string] $activeRun.currentPhaseLabel
    targetPhaseId = $targetPhaseId
    targetPhaseLabel = $targetPhaseLabel
    unattendedProofCollapseState = if ($null -ne $unattendedProofCollapseState) { [string] $unattendedProofCollapseState.collapseState } else { $null }
    dormantWindowCount = if ($null -ne $dormantWindowLedgerState) { [int] $dormantWindowLedgerState.consecutiveDormantWindows } else { 0 }
    silentCadenceIntegrityState = if ($null -ne $silentCadenceIntegrityState) { [string] $silentCadenceIntegrityState.integrityState } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Long-Form Phase Witness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Witness state: `{0}`' -f $payload.witnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Active run: `{0}`' -f $payload.activeRunId),
    ('- Current phase: `{0}`' -f $payload.currentPhaseLabel),
    ('- Target phase: `{0}`' -f $payload.targetPhaseLabel),
    ('- Unattended proof collapse state: `{0}`' -f $(if ($payload.unattendedProofCollapseState) { $payload.unattendedProofCollapseState } else { 'missing' })),
    ('- Dormant window count: `{0}`' -f $payload.dormantWindowCount),
    ('- Silent cadence integrity state: `{0}`' -f $(if ($payload.silentCadenceIntegrityState) { $payload.silentCadenceIntegrityState } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    witnessState = $payload.witnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    targetPhaseId = $payload.targetPhaseId
    targetPhaseLabel = $payload.targetPhaseLabel
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[long-form-phase-witness] Bundle: {0}' -f $bundlePath)
$bundlePath
