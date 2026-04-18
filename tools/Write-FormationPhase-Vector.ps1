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
$engramIntentFieldLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramIntentFieldLedgerStatePath)
$intentConstraintAlignmentReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.intentConstraintAlignmentReceiptStatePath)
$warmReactivationDispositionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.warmReactivationDispositionReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.formationPhaseVectorStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the formation phase-vector writer can run.'
}

$intentFieldState = Read-JsonFileOrNull -Path $engramIntentFieldLedgerStatePath
$alignmentState = Read-JsonFileOrNull -Path $intentConstraintAlignmentReceiptStatePath
$warmState = Read-JsonFileOrNull -Path $warmReactivationDispositionReceiptStatePath

$currentIntentFieldState = if ($null -ne $intentFieldState) { [string] $intentFieldState.engramIntentFieldLedgerState } else { $null }
$currentAlignmentState = if ($null -ne $alignmentState) { [string] $alignmentState.intentConstraintAlignmentReceiptState } else { $null }
$currentWarmState = if ($null -ne $warmState) { [string] $warmState.warmReactivationDispositionReceiptState } else { $null }

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$projectionBound = $contractsSource.IndexOf('FormationPhaseVectorReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateFormationPhaseVectorReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('formation-phase-vector-bound', [System.StringComparison]::Ordinal) -ge 0
$keyBound = $keysSource.IndexOf('CreateFormationPhaseVectorReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateFormationPhaseVectorReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateFormationPhaseBrittlenessAndDurability_WitnessCandidatePositionAcrossFieldPressure', [System.StringComparison]::Ordinal) -ge 0

$formationPhaseVectorState = 'awaiting-engram-intent-field-ledger'
$reasonCode = 'formation-phase-vector-awaiting-intent-field'
$nextAction = 'emit-engram-intent-field-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $formationPhaseVectorState = 'blocked'
    $reasonCode = 'formation-phase-vector-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentIntentFieldState -ne 'engram-intent-field-ledger-ready') {
    $formationPhaseVectorState = 'awaiting-engram-intent-field-ledger'
    $reasonCode = 'formation-phase-vector-intent-field-not-ready'
    $nextAction = if ($null -ne $intentFieldState) { [string] $intentFieldState.nextAction } else { 'emit-engram-intent-field-ledger' }
} elseif ($currentAlignmentState -ne 'intent-constraint-alignment-receipt-ready') {
    $formationPhaseVectorState = 'awaiting-intent-constraint-alignment-receipt'
    $reasonCode = 'formation-phase-vector-alignment-not-ready'
    $nextAction = if ($null -ne $alignmentState) { [string] $alignmentState.nextAction } else { 'emit-intent-constraint-alignment-receipt' }
} elseif ($currentWarmState -ne 'warm-reactivation-disposition-receipt-ready') {
    $formationPhaseVectorState = 'awaiting-warm-reactivation-disposition-receipt'
    $reasonCode = 'formation-phase-vector-warm-disposition-not-ready'
    $nextAction = if ($null -ne $warmState) { [string] $warmState.nextAction } else { 'emit-warm-reactivation-disposition-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $projectionBound -or -not $keyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $formationPhaseVectorState = 'awaiting-formation-phase-vector-binding'
    $reasonCode = 'formation-phase-vector-source-missing'
    $nextAction = 'bind-formation-phase-vector'
} else {
    $formationPhaseVectorState = 'formation-phase-vector-ready'
    $reasonCode = 'formation-phase-vector-bound'
    $nextAction = 'emit-brittleness-witness'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'formation-phase-vector.json'
$bundleMarkdownPath = Join-Path $bundlePath 'formation-phase-vector.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    formationPhaseVectorState = $formationPhaseVectorState
    reasonCode = $reasonCode
    nextAction = $nextAction
    engramIntentFieldLedgerState = $currentIntentFieldState
    intentConstraintAlignmentReceiptState = $currentAlignmentState
    warmReactivationDispositionReceiptState = $currentWarmState
    phaseAxisCount = 6
    stabilityAxisCount = 3
    thermalRegionCount = 3
    formationRegion = 'warm-governed-phase-space'
    warmGovernanceDominant = $true
    coolingEligible = $false
    reheatingSensitive = $true
    projectionBound = $projectionBound
    keyBound = $keyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Formation Phase Vector',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Receipt state: `{0}`' -f $payload.formationPhaseVectorState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Engram intent-field state: `{0}`' -f $(if ($payload.engramIntentFieldLedgerState) { $payload.engramIntentFieldLedgerState } else { 'missing' })),
    ('- Intent-constraint alignment state: `{0}`' -f $(if ($payload.intentConstraintAlignmentReceiptState) { $payload.intentConstraintAlignmentReceiptState } else { 'missing' })),
    ('- Warm reactivation-disposition state: `{0}`' -f $(if ($payload.warmReactivationDispositionReceiptState) { $payload.warmReactivationDispositionReceiptState } else { 'missing' })),
    ('- Phase-axis count: `{0}`' -f $payload.phaseAxisCount),
    ('- Stability-axis count: `{0}`' -f $payload.stabilityAxisCount),
    ('- Thermal-region count: `{0}`' -f $payload.thermalRegionCount),
    ('- Formation region: `{0}`' -f $payload.formationRegion),
    ('- Warm governance dominant: `{0}`' -f [bool] $payload.warmGovernanceDominant),
    ('- Cooling eligible: `{0}`' -f [bool] $payload.coolingEligible),
    ('- Reheating sensitive: `{0}`' -f [bool] $payload.reheatingSensitive),
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
    formationPhaseVectorState = $payload.formationPhaseVectorState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    engramIntentFieldLedgerState = $payload.engramIntentFieldLedgerState
    intentConstraintAlignmentReceiptState = $payload.intentConstraintAlignmentReceiptState
    warmReactivationDispositionReceiptState = $payload.warmReactivationDispositionReceiptState
    phaseAxisCount = $payload.phaseAxisCount
    stabilityAxisCount = $payload.stabilityAxisCount
    thermalRegionCount = $payload.thermalRegionCount
    formationRegion = $payload.formationRegion
    warmGovernanceDominant = $payload.warmGovernanceDominant
    coolingEligible = $payload.coolingEligible
    reheatingSensitive = $payload.reheatingSensitive
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
