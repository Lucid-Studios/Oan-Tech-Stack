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

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json
$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$schedulerReconciliationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.schedulerReconciliationStatePath)
$consolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeConsolidationStatePath)
$seededGovernanceOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceOutputRoot)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$schedulerReconciliationState = Read-JsonFileOrNull -Path $schedulerReconciliationStatePath

$consecutiveAcceptedCount = 0
if (Test-Path -LiteralPath $seededGovernanceOutputRoot -PathType Container) {
    $seedBundles = @(Get-ChildItem -LiteralPath $seededGovernanceOutputRoot -Directory | Sort-Object Name -Descending)
    foreach ($bundle in $seedBundles) {
        $bundleJson = Read-JsonFileOrNull -Path (Join-Path $bundle.FullName 'seeded-governance.json')
        if ($null -eq $bundleJson) {
            continue
        }

        if ([string] $bundleJson.disposition -eq 'Accepted') {
            $consecutiveAcceptedCount += 1
            continue
        }

        break
    }
}

$latestStatus = if ($null -ne $cycleState) { [string] $cycleState.lastKnownStatus } else { 'unknown' }
$latestDisposition = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.disposition } else { 'none' }
$schedulerAligned = if ($null -ne $schedulerReconciliationState) { [bool] $schedulerReconciliationState.aligned } else { $false }
$consolidationState = 'Exploratory'
$reasonCode = 'consolidation-exploratory'

if ($latestStatus -eq [string] $cyclePolicy.blockedStatus) {
    $consolidationState = 'Exploratory'
    $reasonCode = 'consolidation-automation-blocked'
} elseif ($latestDisposition -eq 'Accepted' -and $schedulerAligned -and $consecutiveAcceptedCount -ge 2) {
    $consolidationState = 'Crystallizing'
    $reasonCode = 'consolidation-crystallizing-seed-braided'
} elseif ($latestDisposition -eq 'Accepted' -and $schedulerAligned) {
    $consolidationState = 'Braided'
    $reasonCode = 'consolidation-braided-seed-accepted'
} elseif ($latestDisposition -in @('Deferred', 'Rejected')) {
    $consolidationState = 'SeedAssisted'
    $reasonCode = 'consolidation-seed-assisted'
}

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    latestAutomationStatus = $latestStatus
    latestSeedDisposition = $latestDisposition
    schedulerAligned = $schedulerAligned
    consecutiveAcceptedCount = $consecutiveAcceptedCount
    consolidationState = $consolidationState
    reasonCode = $reasonCode
}

Write-JsonFile -Path $consolidationStatePath -Value $statePayload

$markdownPath = [System.IO.Path]::ChangeExtension($consolidationStatePath, '.md')
$markdownLines = @(
    '# CME Formalization Consolidation State',
    '',
    ('- Generated at (UTC): `{0}`' -f $statePayload.generatedAtUtc),
    ('- Latest automation status: `{0}`' -f $statePayload.latestAutomationStatus),
    ('- Latest seed disposition: `{0}`' -f $statePayload.latestSeedDisposition),
    ('- Scheduler aligned: `{0}`' -f $statePayload.schedulerAligned),
    ('- Consecutive accepted seed runs: `{0}`' -f $statePayload.consecutiveAcceptedCount),
    ('- Consolidation state: `{0}`' -f $statePayload.consolidationState),
    ('- Reason code: `{0}`' -f $statePayload.reasonCode)
)

Set-Content -LiteralPath $markdownPath -Value $markdownLines -Encoding utf8
Write-Host ('[cme-formalization-consolidation] State: {0}' -f $consolidationStatePath)
$consolidationStatePath
