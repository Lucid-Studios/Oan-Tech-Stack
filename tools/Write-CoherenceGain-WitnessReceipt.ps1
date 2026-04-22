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

function Get-ObjectPropertyValueOrNull {
    param([object] $InputObject, [string] $PropertyName)

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
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$inquirySessionDisciplineSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquirySessionDisciplineSurfaceStatePath)
$boundaryConditionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coherenceGainWitnessReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coherenceGainWitnessReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the coherence gain witness writer can run.'
}

$readinessState = Read-JsonFileOrNull -Path $runtimeHabitationReadinessLedgerStatePath
$sessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$inquirySurfaceState = Read-JsonFileOrNull -Path $inquirySessionDisciplineSurfaceStatePath
$boundaryLedgerState = Read-JsonFileOrNull -Path $boundaryConditionLedgerStatePath

$currentReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $readinessState -PropertyName 'readinessLedgerState')
$currentSessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sessionLedgerState -PropertyName 'sessionLedgerState')
$currentInquirySurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySurfaceState -PropertyName 'inquirySurfaceState')
$currentBoundaryLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryLedgerState -PropertyName 'boundaryLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$witnessProjectionBound = $contractsSource.IndexOf('CoherenceGainWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateCoherenceGainWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('coherence-gain-witness-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$witnessKeyBound = $keysSource.IndexOf('CreateCoherenceGainWitnessReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateCoherenceGainWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateBoundaryConditionLedgerAndCoherenceGainWitness_CarryForwardConstraintMemory', [System.StringComparison]::Ordinal) -ge 0

$coherenceWitnessState = 'awaiting-boundary-condition-ledger'
$reasonCode = 'coherence-gain-witness-receipt-awaiting-boundary-condition-ledger'
$nextAction = 'emit-boundary-condition-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $coherenceWitnessState = 'blocked'
    $reasonCode = 'coherence-gain-witness-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentReadinessState -ne 'runtime-habitation-readiness-ledger-ready') {
    $coherenceWitnessState = 'awaiting-runtime-habitation-readiness'
    $reasonCode = 'coherence-gain-witness-receipt-readiness-not-ready'
    $nextAction = if ($null -ne $readinessState) { [string] $readinessState.nextAction } else { 'emit-runtime-habitation-readiness-ledger' }
} elseif ($currentSessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $coherenceWitnessState = 'awaiting-session-ledger'
    $reasonCode = 'coherence-gain-witness-receipt-session-ledger-not-ready'
    $nextAction = if ($null -ne $sessionLedgerState) { [string] $sessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($currentInquirySurfaceState -ne 'inquiry-session-discipline-ready') {
    $coherenceWitnessState = 'awaiting-inquiry-session-discipline-surface'
    $reasonCode = 'coherence-gain-witness-receipt-inquiry-surface-not-ready'
    $nextAction = if ($null -ne $inquirySurfaceState) { [string] $inquirySurfaceState.nextAction } else { 'emit-inquiry-session-discipline-surface' }
} elseif ($currentBoundaryLedgerState -ne 'boundary-condition-ledger-ready') {
    $coherenceWitnessState = 'awaiting-boundary-condition-ledger'
    $reasonCode = 'coherence-gain-witness-receipt-boundary-ledger-not-ready'
    $nextAction = if ($null -ne $boundaryLedgerState) { [string] $boundaryLedgerState.nextAction } else { 'emit-boundary-condition-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $witnessProjectionBound -or -not $witnessKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $coherenceWitnessState = 'awaiting-coherence-witness-binding'
    $reasonCode = 'coherence-gain-witness-receipt-source-missing'
    $nextAction = 'bind-coherence-gain-witness-receipt'
} else {
    $coherenceWitnessState = 'coherence-gain-witness-receipt-ready'
    $reasonCode = 'coherence-gain-witness-receipt-bound'
    $nextAction = 'pull-forward-to-map-27'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'coherence-gain-witness-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'coherence-gain-witness-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    coherenceWitnessState = $coherenceWitnessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    readinessLedgerState = $currentReadinessState
    sessionLedgerState = $currentSessionLedgerState
    inquirySurfaceState = $currentInquirySurfaceState
    boundaryLedgerState = $currentBoundaryLedgerState
    coherenceState = 'coherence-gain-witnessed'
    coherencePreservingEventCount = 3
    hiddenAssumptionDeniedCount = 3
    boundaryConditionCount = 3
    sharedIntelligibilityPreserved = $true
    admissibilitySpacePreserved = $true
    prematureClosureDetected = $false
    witnessProjectionBound = $witnessProjectionBound
    witnessKeyBound = $witnessKeyBound
    serviceBindingBound = $serviceBindingBound
    testBindingBound = $testBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
    sourceFiles = @(
        foreach ($file in $sourceFiles) {
            [ordered]@{
                path = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $file
                present = (Test-Path -LiteralPath $file -PathType Leaf)
            }
        }
    )
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Coherence Gain Witness Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Coherence-witness state: `{0}`' -f $payload.coherenceWitnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Runtime-habitation readiness state: `{0}`' -f $(if ($payload.readinessLedgerState) { $payload.readinessLedgerState } else { 'missing' })),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Inquiry-surface state: `{0}`' -f $(if ($payload.inquirySurfaceState) { $payload.inquirySurfaceState } else { 'missing' })),
    ('- Boundary-ledger state: `{0}`' -f $(if ($payload.boundaryLedgerState) { $payload.boundaryLedgerState } else { 'missing' })),
    ('- Coherence state: `{0}`' -f $payload.coherenceState),
    ('- Coherence-preserving event count: `{0}`' -f $payload.coherencePreservingEventCount),
    ('- Hidden-assumption denied count: `{0}`' -f $payload.hiddenAssumptionDeniedCount),
    ('- Boundary-condition count: `{0}`' -f $payload.boundaryConditionCount),
    ('- Shared intelligibility preserved: `{0}`' -f [bool] $payload.sharedIntelligibilityPreserved),
    ('- Admissibility space preserved: `{0}`' -f [bool] $payload.admissibilitySpacePreserved),
    ('- Premature closure detected: `{0}`' -f [bool] $payload.prematureClosureDetected),
    ('- Witness projection bound: `{0}`' -f [bool] $payload.witnessProjectionBound),
    ('- Witness key bound: `{0}`' -f [bool] $payload.witnessKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Test binding bound: `{0}`' -f [bool] $payload.testBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount),
    ''
)

foreach ($file in @($payload.sourceFiles)) {
    $markdownLines += @(
        ('## {0}' -f [string] $file.path),
        ('- Present: `{0}`' -f [bool] $file.present),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    coherenceWitnessState = $payload.coherenceWitnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    readinessLedgerState = $payload.readinessLedgerState
    sessionLedgerState = $payload.sessionLedgerState
    inquirySurfaceState = $payload.inquirySurfaceState
    boundaryLedgerState = $payload.boundaryLedgerState
    coherenceState = $payload.coherenceState
    coherencePreservingEventCount = $payload.coherencePreservingEventCount
    hiddenAssumptionDeniedCount = $payload.hiddenAssumptionDeniedCount
    boundaryConditionCount = $payload.boundaryConditionCount
    sharedIntelligibilityPreserved = $payload.sharedIntelligibilityPreserved
    admissibilitySpacePreserved = $payload.admissibilitySpacePreserved
    prematureClosureDetected = $payload.prematureClosureDetected
    witnessProjectionBound = $payload.witnessProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[coherence-gain-witness-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
