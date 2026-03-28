param(
    [string] $RepoRoot,
    [string] $CyclePolicyPath = 'OAN Mortalis V1.0/build/local-automation-cycle.json'
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
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$schedulerExecutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerExecutionReceiptStatePath)
$unattendedIntervalConcordanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedIntervalConcordanceStatePath)
$unattendedProofCollapseStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.unattendedProofCollapseStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerProofHarvestOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerProofHarvestStatePath)

$schedulerExecutionReceiptState = Read-JsonFileOrNull -Path $schedulerExecutionReceiptStatePath
$unattendedIntervalConcordanceState = Read-JsonFileOrNull -Path $unattendedIntervalConcordanceStatePath
$unattendedProofCollapseState = Read-JsonFileOrNull -Path $unattendedProofCollapseStatePath

$harvestState = 'candidate-only'
$reasonCode = 'scheduler-proof-harvest-candidate-only'
$nextAction = 'continue-candidate-automation'

if ($null -eq $schedulerExecutionReceiptState -or
    $null -eq $unattendedIntervalConcordanceState -or
    $null -eq $unattendedProofCollapseState) {
    $harvestState = 'awaiting-evidence'
    $reasonCode = 'scheduler-proof-harvest-evidence-missing'
    $nextAction = 'complete-scheduler-proof-prerequisites'
} elseif ([string] $unattendedProofCollapseState.collapseState -eq 'contradiction-hold') {
    $harvestState = 'contradiction-hold'
    $reasonCode = 'scheduler-proof-harvest-contradiction-hold'
    $nextAction = 'investigate-scheduler-proof-chain'
} elseif ([string] $schedulerExecutionReceiptState.receiptState -ne 'receipt-captured') {
    $harvestState = 'awaiting-scheduler-proof'
    $reasonCode = 'scheduler-proof-harvest-scheduler-run-pending'
    $nextAction = [string] $schedulerExecutionReceiptState.nextAction
} elseif ([string] $unattendedProofCollapseState.collapseState -eq 'collapsed-unattended-proof') {
    $harvestState = 'scheduler-proof-harvested'
    $reasonCode = 'scheduler-proof-harvest-collapsed'
    $nextAction = 'continue-with-observed-cadence'
} elseif ([string] $unattendedProofCollapseState.collapseState -eq 'collapsed-with-manual-overhang') {
    $harvestState = 'scheduler-proof-harvested-with-manual-overhang'
    $reasonCode = 'scheduler-proof-harvest-manual-overhang'
    $nextAction = 'reconcile-manual-overhang'
} else {
    $harvestState = 'awaiting-proof-collapse'
    $reasonCode = 'scheduler-proof-harvest-collapse-pending'
    $nextAction = [string] $unattendedProofCollapseState.nextAction
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundlePath = Join-Path $outputRoot $timestamp
$bundleJsonPath = Join-Path $bundlePath 'scheduler-proof-harvest.json'
$bundleMarkdownPath = Join-Path $bundlePath 'scheduler-proof-harvest.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    harvestState = $harvestState
    reasonCode = $reasonCode
    nextAction = $nextAction
    schedulerReceiptState = if ($null -ne $schedulerExecutionReceiptState) { [string] $schedulerExecutionReceiptState.receiptState } else { $null }
    unattendedConcordanceState = if ($null -ne $unattendedIntervalConcordanceState) { [string] $unattendedIntervalConcordanceState.concordanceState } else { $null }
    unattendedProofCollapseState = if ($null -ne $unattendedProofCollapseState) { [string] $unattendedProofCollapseState.collapseState } else { $null }
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Scheduler Proof Harvest',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Harvest state: `{0}`' -f $payload.harvestState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Scheduler receipt state: `{0}`' -f $(if ($payload.schedulerReceiptState) { $payload.schedulerReceiptState } else { 'missing' })),
    ('- Unattended concordance state: `{0}`' -f $(if ($payload.unattendedConcordanceState) { $payload.unattendedConcordanceState } else { 'missing' })),
    ('- Unattended proof collapse state: `{0}`' -f $(if ($payload.unattendedProofCollapseState) { $payload.unattendedProofCollapseState } else { 'missing' }))
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    harvestState = $payload.harvestState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[scheduler-proof-harvest] Bundle: {0}' -f $bundlePath)
$bundlePath
