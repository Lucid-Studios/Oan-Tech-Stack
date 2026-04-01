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
$ripeningStalenessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ripeningStalenessLedgerStatePath)
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)
$formationPhaseVectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coolingPressureWitnessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coolingPressureWitnessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the cooling pressure witness writer can run.'
}

$warmClockState = Read-JsonFileOrNull -Path $warmClockDispositionStatePath
$ripeningState = Read-JsonFileOrNull -Path $ripeningStalenessLedgerStatePath
$durabilityState = Read-JsonFileOrNull -Path $durabilityWitnessStatePath
$formationPhaseState = Read-JsonFileOrNull -Path $formationPhaseVectorStatePath
$alignmentState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath

$currentWarmClockState = if ($null -ne $warmClockState) { [string] $warmClockState.warmClockDispositionState } else { $null }
$currentRipeningState = if ($null -ne $ripeningState) { [string] $ripeningState.ripeningStalenessLedgerState } else { $null }
$currentDurabilityState = if ($null -ne $durabilityState) { [string] $durabilityState.durabilityWitnessState } else { $null }
$currentFormationPhaseState = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.formationPhaseVectorState } else { $null }
$currentAlignmentState = if ($null -ne $alignmentState) { [string] $alignmentState.intentConstraintAlignmentReceiptState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('CoolingPressureWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateCoolingPressureWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('cooling-pressure-witness-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateCoolingPressureWitnessReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateCoolingPressureWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmClockRipeningAndCoolingPressure_WitnessTemporalLawInsideWarmLane', [System.StringComparison]::Ordinal) -ge 0

$coolingPressureWitnessState = 'awaiting-warm-clock-disposition'
$reasonCode = 'cooling-pressure-witness-awaiting-warm-clock-disposition'
$nextAction = 'emit-warm-clock-disposition'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $coolingPressureWitnessState = 'blocked'
    $reasonCode = 'cooling-pressure-witness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentWarmClockState -ne 'warm-clock-disposition-ready') {
    $coolingPressureWitnessState = 'awaiting-warm-clock-disposition'
    $reasonCode = 'cooling-pressure-witness-warm-clock-not-ready'
    $nextAction = if ($null -ne $warmClockState) { [string] $warmClockState.nextAction } else { 'emit-warm-clock-disposition' }
} elseif ($currentRipeningState -ne 'ripening-staleness-ledger-ready') {
    $coolingPressureWitnessState = 'awaiting-ripening-staleness-ledger'
    $reasonCode = 'cooling-pressure-witness-ripening-ledger-not-ready'
    $nextAction = if ($null -ne $ripeningState) { [string] $ripeningState.nextAction } else { 'emit-ripening-staleness-ledger' }
} elseif ($currentDurabilityState -ne 'durability-witness-ready') {
    $coolingPressureWitnessState = 'awaiting-durability-witness'
    $reasonCode = 'cooling-pressure-witness-durability-not-ready'
    $nextAction = if ($null -ne $durabilityState) { [string] $durabilityState.nextAction } else { 'emit-durability-witness' }
} elseif ($currentFormationPhaseState -ne 'formation-phase-vector-ready') {
    $coolingPressureWitnessState = 'awaiting-formation-phase-vector'
    $reasonCode = 'cooling-pressure-witness-phase-vector-not-ready'
    $nextAction = if ($null -ne $formationPhaseState) { [string] $formationPhaseState.nextAction } else { 'emit-formation-phase-vector' }
} elseif ($currentAlignmentState -ne 'intent-constraint-alignment-receipt-ready') {
    $coolingPressureWitnessState = 'awaiting-intent-constraint-alignment-receipt'
    $reasonCode = 'cooling-pressure-witness-alignment-not-ready'
    $nextAction = if ($null -ne $alignmentState) { [string] $alignmentState.nextAction } else { 'emit-intent-constraint-alignment-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $coolingPressureWitnessState = 'awaiting-cooling-pressure-witness-binding'
    $reasonCode = 'cooling-pressure-witness-source-missing'
    $nextAction = 'bind-cooling-pressure-witness'
} else {
    $coolingPressureWitnessState = 'cooling-pressure-witness-ready'
    $reasonCode = 'cooling-pressure-witness-bound'
    $nextAction = 'continue-warm-temporal-review'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'cooling-pressure-witness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'cooling-pressure-witness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    coolingPressureWitnessState = $coolingPressureWitnessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    warmClockDispositionState = $currentWarmClockState
    ripeningStalenessLedgerState = $currentRipeningState
    durabilityWitnessState = $currentDurabilityState
    formationPhaseVectorState = $currentFormationPhaseState
    intentConstraintAlignmentReceiptState = $currentAlignmentState
    coolingForceCount = 3
    coolingBarrierCount = 3
    pressureDisposition = 'pressure-emergent-but-withheld'
    coolingPressureEmergent = $true
    coldApproachLawful = $false
    reheatingOrArchivePressureStillStronger = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Cooling Pressure Witness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.coolingPressureWitnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Warm-clock disposition state: `{0}`' -f $(if ($payload.warmClockDispositionState) { $payload.warmClockDispositionState } else { 'missing' })),
    ('- Ripening-staleness ledger state: `{0}`' -f $(if ($payload.ripeningStalenessLedgerState) { $payload.ripeningStalenessLedgerState } else { 'missing' })),
    ('- Durability-witness state: `{0}`' -f $(if ($payload.durabilityWitnessState) { $payload.durabilityWitnessState } else { 'missing' })),
    ('- Formation phase-vector state: `{0}`' -f $(if ($payload.formationPhaseVectorState) { $payload.formationPhaseVectorState } else { 'missing' })),
    ('- Intent-constraint alignment state: `{0}`' -f $(if ($payload.intentConstraintAlignmentReceiptState) { $payload.intentConstraintAlignmentReceiptState } else { 'missing' })),
    ('- Cooling-force count: `{0}`' -f $payload.coolingForceCount),
    ('- Cooling-barrier count: `{0}`' -f $payload.coolingBarrierCount),
    ('- Pressure disposition: `{0}`' -f $payload.pressureDisposition),
    ('- Cooling pressure emergent: `{0}`' -f [bool] $payload.coolingPressureEmergent),
    ('- Cold approach lawful: `{0}`' -f [bool] $payload.coldApproachLawful),
    ('- Reheating or archive pressure still stronger: `{0}`' -f [bool] $payload.reheatingOrArchivePressureStillStronger),
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
    coolingPressureWitnessState = $payload.coolingPressureWitnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    warmClockDispositionState = $payload.warmClockDispositionState
    ripeningStalenessLedgerState = $payload.ripeningStalenessLedgerState
    durabilityWitnessState = $payload.durabilityWitnessState
    formationPhaseVectorState = $payload.formationPhaseVectorState
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    coolingForceCount = $payload.coolingForceCount
    coolingBarrierCount = $payload.coolingBarrierCount
    pressureDisposition = $payload.pressureDisposition
    coolingPressureEmergent = $payload.coolingPressureEmergent
    coldApproachLawful = $payload.coldApproachLawful
    reheatingOrArchivePressureStillStronger = $payload.reheatingOrArchivePressureStillStronger
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
