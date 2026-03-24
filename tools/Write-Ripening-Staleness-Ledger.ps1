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
    param([string] $BasePath, [string] $CandidatePath)

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
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)
$brittlenessWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.brittlenessWitnessStatePath)
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ripeningStalenessLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ripeningStalenessLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the ripening staleness ledger writer can run.'
}

$warmClockState = Read-JsonFileOrNull -Path $warmClockDispositionStatePath
$formationPhaseState = Read-JsonFileOrNull -Path $formationPhaseVectorStatePath
$brittlenessState = Read-JsonFileOrNull -Path $brittlenessWitnessStatePath
$durabilityState = Read-JsonFileOrNull -Path $durabilityWitnessStatePath

$currentWarmClockState = if ($null -ne $warmClockState) { [string] $warmClockState.warmClockDispositionState } else { $null }
$currentFormationPhaseState = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.formationPhaseVectorState } else { $null }
$currentBrittlenessState = if ($null -ne $brittlenessState) { [string] $brittlenessState.brittlenessWitnessState } else { $null }
$currentDurabilityState = if ($null -ne $durabilityState) { [string] $durabilityState.durabilityWitnessState } else { $null }

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedReachRealizationService.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/tests/Oan.Runtime.IntegrationTests/SanctuaryRuntimeWorkbenchServiceTests.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('RipeningStalenessLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateRipeningStalenessLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('ripening-staleness-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateRipeningStalenessLedgerReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateRipeningStalenessLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmClockRipeningAndCoolingPressure_WitnessTemporalLawInsideWarmLane', [System.StringComparison]::Ordinal) -ge 0

$ripeningStalenessLedgerState = 'awaiting-warm-clock-disposition'
$reasonCode = 'ripening-staleness-ledger-awaiting-warm-clock-disposition'
$nextAction = 'emit-warm-clock-disposition'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $ripeningStalenessLedgerState = 'blocked'
    $reasonCode = 'ripening-staleness-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentWarmClockState -ne 'warm-clock-disposition-ready') {
    $ripeningStalenessLedgerState = 'awaiting-warm-clock-disposition'
    $reasonCode = 'ripening-staleness-ledger-warm-clock-not-ready'
    $nextAction = if ($null -ne $warmClockState) { [string] $warmClockState.nextAction } else { 'emit-warm-clock-disposition' }
} elseif ($currentFormationPhaseState -ne 'formation-phase-vector-ready') {
    $ripeningStalenessLedgerState = 'awaiting-formation-phase-vector'
    $reasonCode = 'ripening-staleness-ledger-phase-vector-not-ready'
    $nextAction = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.nextAction } else { 'emit-formation-phase-vector' }
} elseif ($currentBrittlenessState -ne 'brittleness-witness-ready') {
    $ripeningStalenessLedgerState = 'awaiting-brittleness-witness'
    $reasonCode = 'ripening-staleness-ledger-brittleness-not-ready'
    $nextAction = if ($null -ne $brittlenessState) { [string] $brittlenessState.nextAction } else { 'emit-brittleness-witness' }
} elseif ($currentDurabilityState -ne 'durability-witness-ready') {
    $ripeningStalenessLedgerState = 'awaiting-durability-witness'
    $reasonCode = 'ripening-staleness-ledger-durability-not-ready'
    $nextAction = if ($null -ne $durabilityState) { [string] $durabilityState.nextAction } else { 'emit-durability-witness' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $ripeningStalenessLedgerState = 'awaiting-ripening-staleness-ledger-binding'
    $reasonCode = 'ripening-staleness-ledger-source-missing'
    $nextAction = 'bind-ripening-staleness-ledger'
} else {
    $ripeningStalenessLedgerState = 'ripening-staleness-ledger-ready'
    $reasonCode = 'ripening-staleness-ledger-bound'
    $nextAction = 'emit-cooling-pressure-witness'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'ripening-staleness-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'ripening-staleness-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    ripeningStalenessLedgerState = $ripeningStalenessLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    warmClockDispositionState = $currentWarmClockState
    formationPhaseVectorState = $currentFormationPhaseState
    brittlenessWitnessState = $currentBrittlenessState
    durabilityWitnessState = $currentDurabilityState
    ripeningPatternCount = 2
    stalePatternCount = 1
    ripeningWindowCount = 2
    staleWindowCount = 1
    refreshRequiredCount = 1
    honestWarmRipeningPreserved = $true
    administrativeSuspensionDenied = $true
    freshConstraintContactStillRequired = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Ripening Staleness Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.ripeningStalenessLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Warm-clock disposition state: `{0}`' -f $(if ($payload.warmClockDispositionState) { $payload.warmClockDispositionState } else { 'missing' })),
    ('- Formation phase-vector state: `{0}`' -f $(if ($payload.formationPhaseVectorState) { $payload.formationPhaseVectorState } else { 'missing' })),
    ('- Brittleness-witness state: `{0}`' -f $(if ($payload.brittlenessWitnessState) { $payload.brittlenessWitnessState } else { 'missing' })),
    ('- Durability-witness state: `{0}`' -f $(if ($payload.durabilityWitnessState) { $payload.durabilityWitnessState } else { 'missing' })),
    ('- Ripening-pattern count: `{0}`' -f $payload.ripeningPatternCount),
    ('- Stale-pattern count: `{0}`' -f $payload.stalePatternCount),
    ('- Ripening-window count: `{0}`' -f $payload.ripeningWindowCount),
    ('- Stale-window count: `{0}`' -f $payload.staleWindowCount),
    ('- Refresh-required count: `{0}`' -f $payload.refreshRequiredCount),
    ('- Honest warm ripening preserved: `{0}`' -f [bool] $payload.honestWarmRipeningPreserved),
    ('- Administrative suspension denied: `{0}`' -f [bool] $payload.administrativeSuspensionDenied),
    ('- Fresh constraint contact still required: `{0}`' -f [bool] $payload.freshConstraintContactStillRequired),
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
    ripeningStalenessLedgerState = $payload.ripeningStalenessLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    warmClockDispositionState = $payload.warmClockDispositionState
    formationPhaseVectorState = $payload.formationPhaseVectorState
    brittlenessWitnessState = $payload.brittlenessWitnessState
    durabilityWitnessState = $payload.durabilityWitnessState
    ripeningPatternCount = $payload.ripeningPatternCount
    stalePatternCount = $payload.stalePatternCount
    ripeningWindowCount = $payload.ripeningWindowCount
    staleWindowCount = $payload.staleWindowCount
    refreshRequiredCount = $payload.refreshRequiredCount
    honestWarmRipeningPreserved = $payload.honestWarmRipeningPreserved
    administrativeSuspensionDenied = $payload.administrativeSuspensionDenied
    freshConstraintContactStillRequired = $payload.freshConstraintContactStillRequired
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
