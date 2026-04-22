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

    if (-not (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue)) {
        $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
        if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
            . $oanWorkspaceResolverPath
        }
    }

    if (Get-Command -Name Resolve-OanWorkspacePath -ErrorAction SilentlyContinue) {
        return Resolve-OanWorkspacePath -BasePath $BasePath -CandidatePath $CandidatePath
    }

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param([string] $BasePath, [string] $TargetPath)

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
    param([string] $Path, [object] $Value)

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$warmClockDispositionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmClockDispositionStatePath)
$coolingPressureWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coolingPressureWitnessStatePath)
$brittlenessWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.hotReactivationTriggerReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.hotReactivationTriggerReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the hot reactivation trigger receipt writer can run.'
}

$warmClockState = Read-JsonFileOrNull -Path $warmClockDispositionStatePath
$coolingPressureState = Read-JsonFileOrNull -Path $coolingPressureWitnessStatePath
$brittlenessState = Read-JsonFileOrNull -Path $brittlenessWitnessStatePath

$currentWarmClockState = if ($null -ne $warmClockState) { [string] $warmClockState.warmClockDispositionState } else { $null }
$currentCoolingPressureState = if ($null -ne $coolingPressureState) { [string] $coolingPressureState.coolingPressureWitnessState } else { $null }
$currentBrittlenessState = if ($null -ne $brittlenessState) { [string] $brittlenessState.brittlenessWitnessState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('HotReactivationTriggerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateHotReactivationTriggerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('hot-reactivation-trigger-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateHotReactivationTriggerReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateHotReactivationTriggerReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmExitLaw_ReheatColdAndArchiveRoutesRemainDistinct', [System.StringComparison]::Ordinal) -ge 0

$receiptState = 'awaiting-warm-clock-disposition'
$reasonCode = 'hot-reactivation-trigger-receipt-awaiting-warm-clock-disposition'
$nextAction = 'emit-warm-clock-disposition'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $receiptState = 'blocked'
    $reasonCode = 'hot-reactivation-trigger-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentWarmClockState -ne 'warm-clock-disposition-ready') {
    $receiptState = 'awaiting-warm-clock-disposition'
    $reasonCode = 'hot-reactivation-trigger-receipt-warm-clock-not-ready'
    $nextAction = if ($null -ne $warmClockState) { [string] $warmClockState.nextAction } else { 'emit-warm-clock-disposition' }
} elseif ($currentCoolingPressureState -ne 'cooling-pressure-witness-ready') {
    $receiptState = 'awaiting-cooling-pressure-witness'
    $reasonCode = 'hot-reactivation-trigger-receipt-cooling-pressure-not-ready'
    $nextAction = if ($null -ne $coolingPressureState) { [string] $coolingPressureState.nextAction } else { 'emit-cooling-pressure-witness' }
} elseif ($currentBrittlenessState -ne 'brittleness-witness-ready') {
    $receiptState = 'awaiting-brittleness-witness'
    $reasonCode = 'hot-reactivation-trigger-receipt-brittleness-not-ready'
    $nextAction = if ($null -ne $brittlenessState) { [string] $brittlenessState.nextAction } else { 'emit-brittleness-witness' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $receiptState = 'awaiting-hot-reactivation-trigger-receipt-binding'
    $reasonCode = 'hot-reactivation-trigger-receipt-source-missing'
    $nextAction = 'bind-hot-reactivation-trigger-receipt'
} else {
    $receiptState = 'hot-reactivation-trigger-receipt-ready'
    $reasonCode = 'hot-reactivation-trigger-receipt-bound'
    $nextAction = 'emit-cold-admission-eligibility-gate'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'hot-reactivation-trigger-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'hot-reactivation-trigger-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    hotReactivationTriggerReceiptState = $receiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    warmClockDispositionState = $currentWarmClockState
    coolingPressureWitnessState = $currentCoolingPressureState
    brittlenessWitnessState = $currentBrittlenessState
    reactivationTriggerCount = 4
    failedInvariantCount = 4
    reactivationDisposition = 'return-to-hot-required'
    hotReturnLawful = $true
    warmHoldingInsufficient = $true
    reentryAsFormationPreserved = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Hot Reactivation Trigger Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.hotReactivationTriggerReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Warm-clock disposition state: `{0}`' -f $(if ($payload.warmClockDispositionState) { $payload.warmClockDispositionState } else { 'missing' })),
    ('- Cooling-pressure witness state: `{0}`' -f $(if ($payload.coolingPressureWitnessState) { $payload.coolingPressureWitnessState } else { 'missing' })),
    ('- Brittleness-witness state: `{0}`' -f $(if ($payload.brittlenessWitnessState) { $payload.brittlenessWitnessState } else { 'missing' })),
    ('- Reactivation-trigger count: `{0}`' -f $payload.reactivationTriggerCount),
    ('- Failed-invariant count: `{0}`' -f $payload.failedInvariantCount),
    ('- Reactivation disposition: `{0}`' -f $payload.reactivationDisposition),
    ('- Hot return lawful: `{0}`' -f [bool] $payload.hotReturnLawful),
    ('- Warm holding insufficient: `{0}`' -f [bool] $payload.warmHoldingInsufficient),
    ('- Re-entry as formation preserved: `{0}`' -f [bool] $payload.reentryAsFormationPreserved),
    ('- Projection bound: `{0}`' -f [bool] $payload.projectionBound),
    ('- Key bound: `{0}`' -f [bool] $payload.keyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Test binding bound: `{0}`' -f [bool] $payload.testBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount)
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    hotReactivationTriggerReceiptState = $payload.hotReactivationTriggerReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    warmClockDispositionState = $payload.warmClockDispositionState
    coolingPressureWitnessState = $payload.coolingPressureWitnessState
    brittlenessWitnessState = $payload.brittlenessWitnessState
    reactivationTriggerCount = $payload.reactivationTriggerCount
    failedInvariantCount = $payload.failedInvariantCount
    reactivationDisposition = $payload.reactivationDisposition
    hotReturnLawful = $payload.hotReturnLawful
    warmHoldingInsufficient = $payload.warmHoldingInsufficient
    reentryAsFormationPreserved = $payload.reentryAsFormationPreserved
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
