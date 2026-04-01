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
$coolingPressureWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coolingPressureWitnessStatePath)
$warmClockDispositionStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmClockDispositionStatePath)
$ripeningStalenessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.ripeningStalenessLedgerStatePath)
$durabilityWitnessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.durabilityWitnessStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coldAdmissionEligibilityGateOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coldAdmissionEligibilityGateStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the cold admission eligibility gate writer can run.'
}

$coolingPressureState = Read-JsonFileOrNull -Path $coolingPressureWitnessStatePath
$warmClockState = Read-JsonFileOrNull -Path $warmClockDispositionStatePath
$ripeningState = Read-JsonFileOrNull -Path $ripeningStalenessLedgerStatePath
$durabilityState = Read-JsonFileOrNull -Path $durabilityWitnessStatePath
$alignmentState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath

$currentCoolingPressureState = if ($null -ne $coolingPressureState) { [string] $coolingPressureState.coolingPressureWitnessState } else { $null }
$currentWarmClockState = if ($null -ne $warmClockState) { [string] $warmClockState.warmClockDispositionState } else { $null }
$currentRipeningState = if ($null -ne $ripeningState) { [string] $ripeningState.ripeningStalenessLedgerState } else { $null }
$currentDurabilityState = if ($null -ne $durabilityState) { [string] $durabilityState.durabilityWitnessState } else { $null }
$currentAlignmentState = if ($null -ne $alignmentState) { [string] $alignmentState.intentConstraintAlignmentReceiptState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('ColdAdmissionEligibilityGateReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateColdAdmissionEligibilityGateReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('cold-admission-eligibility-gate-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateColdAdmissionEligibilityGateHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateColdAdmissionEligibilityGateReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateWarmExitLaw_ReheatColdAndArchiveRoutesRemainDistinct', [System.StringComparison]::Ordinal) -ge 0

$gateState = 'awaiting-cooling-pressure-witness'
$reasonCode = 'cold-admission-eligibility-gate-awaiting-cooling-pressure-witness'
$nextAction = 'emit-cooling-pressure-witness'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $gateState = 'blocked'
    $reasonCode = 'cold-admission-eligibility-gate-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentCoolingPressureState -ne 'cooling-pressure-witness-ready') {
    $gateState = 'awaiting-cooling-pressure-witness'
    $reasonCode = 'cold-admission-eligibility-gate-cooling-pressure-not-ready'
    $nextAction = if ($null -ne $coolingPressureState) { [string] $coolingPressureState.nextAction } else { 'emit-cooling-pressure-witness' }
} elseif ($currentWarmClockState -ne 'warm-clock-disposition-ready') {
    $gateState = 'awaiting-warm-clock-disposition'
    $reasonCode = 'cold-admission-eligibility-gate-warm-clock-not-ready'
    $nextAction = if ($null -ne $warmClockState) { [string] $warmClockState.nextAction } else { 'emit-warm-clock-disposition' }
} elseif ($currentRipeningState -ne 'ripening-staleness-ledger-ready') {
    $gateState = 'awaiting-ripening-staleness-ledger'
    $reasonCode = 'cold-admission-eligibility-gate-ripening-not-ready'
    $nextAction = if ($null -ne $ripeningState) { [string] $ripeningState.nextAction } else { 'emit-ripening-staleness-ledger' }
} elseif ($currentDurabilityState -ne 'durability-witness-ready') {
    $gateState = 'awaiting-durability-witness'
    $reasonCode = 'cold-admission-eligibility-gate-durability-not-ready'
    $nextAction = if ($null -ne $durabilityState) { [string] $durabilityState.nextAction } else { 'emit-durability-witness' }
} elseif ($currentAlignmentState -ne 'intent-constraint-alignment-receipt-ready') {
    $gateState = 'awaiting-intent-constraint-alignment-receipt'
    $reasonCode = 'cold-admission-eligibility-gate-alignment-not-ready'
    $nextAction = if ($null -ne $alignmentState) { [string] $alignmentState.nextAction } else { 'emit-intent-constraint-alignment-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $gateState = 'awaiting-cold-admission-eligibility-gate-binding'
    $reasonCode = 'cold-admission-eligibility-gate-source-missing'
    $nextAction = 'bind-cold-admission-eligibility-gate'
} else {
    $gateState = 'cold-admission-eligibility-gate-ready'
    $reasonCode = 'cold-admission-eligibility-gate-bound'
    $nextAction = 'emit-archive-disposition-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'cold-admission-eligibility-gate.json'
$bundleMarkdownPath = Join-Path $bundlePath 'cold-admission-eligibility-gate.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    coldAdmissionEligibilityGateState = $gateState
    reasonCode = $reasonCode
    nextAction = $nextAction
    coolingPressureWitnessState = $currentCoolingPressureState
    warmClockDispositionState = $currentWarmClockState
    ripeningStalenessLedgerState = $currentRipeningState
    durabilityWitnessState = $currentDurabilityState
    intentConstraintAlignmentReceiptState = $currentAlignmentState
    eligibilitySignalCount = 5
    remainingBarrierCount = 4
    eligibilityDisposition = 'cold-candidacy-withheld'
    coldApproachLawful = $false
    preFreezeOnly = $true
    finalInheritanceStillWithheld = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Cold Admission Eligibility Gate',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Gate state: `{0}`' -f $payload.coldAdmissionEligibilityGateState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Cooling-pressure witness state: `{0}`' -f $(if ($payload.coolingPressureWitnessState) { $payload.coolingPressureWitnessState } else { 'missing' })),
    ('- Warm-clock disposition state: `{0}`' -f $(if ($payload.warmClockDispositionState) { $payload.warmClockDispositionState } else { 'missing' })),
    ('- Ripening-staleness ledger state: `{0}`' -f $(if ($payload.ripeningStalenessLedgerState) { $payload.ripeningStalenessLedgerState } else { 'missing' })),
    ('- Durability-witness state: `{0}`' -f $(if ($payload.durabilityWitnessState) { $payload.durabilityWitnessState } else { 'missing' })),
    ('- Intent-constraint alignment state: `{0}`' -f $(if ($payload.intentConstraintAlignmentReceiptState) { $payload.intentConstraintAlignmentReceiptState } else { 'missing' })),
    ('- Eligibility-signal count: `{0}`' -f $payload.eligibilitySignalCount),
    ('- Remaining-barrier count: `{0}`' -f $payload.remainingBarrierCount),
    ('- Eligibility disposition: `{0}`' -f $payload.eligibilityDisposition),
    ('- Cold approach lawful: `{0}`' -f [bool] $payload.coldApproachLawful),
    ('- Pre-freeze only: `{0}`' -f [bool] $payload.preFreezeOnly),
    ('- Final inheritance still withheld: `{0}`' -f [bool] $payload.finalInheritanceStillWithheld),
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
    coldAdmissionEligibilityGateState = $payload.coldAdmissionEligibilityGateState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    coolingPressureWitnessState = $payload.coolingPressureWitnessState
    warmClockDispositionState = $payload.warmClockDispositionState
    ripeningStalenessLedgerState = $payload.ripeningStalenessLedgerState
    durabilityWitnessState = $payload.durabilityWitnessState
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    eligibilitySignalCount = $payload.eligibilitySignalCount
    remainingBarrierCount = $payload.remainingBarrierCount
    eligibilityDisposition = $payload.eligibilityDisposition
    coldApproachLawful = $payload.coldApproachLawful
    preFreezeOnly = $payload.preFreezeOnly
    finalInheritanceStillWithheld = $payload.finalInheritanceStillWithheld
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
