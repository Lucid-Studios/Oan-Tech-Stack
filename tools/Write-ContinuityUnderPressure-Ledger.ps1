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
$activeTaskMapRunStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath '.audit/state/local-automation-active-task-map-run.json'
$bondedCrucibleSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCrucibleSessionRehearsalStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)
$coherenceGainWitnessReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.coherenceGainWitnessReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the continuity-under-pressure writer can run.'
}

$activeTaskMapRunState = Read-JsonFileOrNull -Path $activeTaskMapRunStatePath
$bondedCrucibleSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCrucibleSessionRehearsalStatePath
$sharedBoundaryMemoryLedgerState = Read-JsonFileOrNull -Path $sharedBoundaryMemoryLedgerStatePath
$coherenceGainWitnessReceiptState = Read-JsonFileOrNull -Path $coherenceGainWitnessReceiptStatePath

$currentActiveTaskMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $activeTaskMapRunState -PropertyName 'mapId')
$currentBondedCrucibleState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'bondedCrucibleSessionRehearsalState')
$currentSharedBoundaryMemoryState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryLedgerState')
$currentCoherenceWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $coherenceGainWitnessReceiptState -PropertyName 'coherenceWitnessState')
$currentActiveTaskMapOrdinal = 0
if ($currentActiveTaskMapId -match 'automation-maturation-map-(\d+)$') {
    $currentActiveTaskMapOrdinal = [int] $Matches[1]
}

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$ledgerProjectionBound = $contractsSource.IndexOf('ContinuityUnderPressureLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateContinuityUnderPressureLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('continuity-under-pressure-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$ledgerKeyBound = $keysSource.IndexOf('CreateContinuityUnderPressureLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateContinuityUnderPressureLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateContinuityUnderPressureLedger_RetainsWhatHeldUnderSharedUncertainty', [System.StringComparison]::Ordinal) -ge 0

$continuityUnderPressureLedgerState = 'awaiting-map-28-activation'
$reasonCode = 'continuity-under-pressure-ledger-awaiting-map-28-activation'
$nextAction = 'pull-forward-to-map-28'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $continuityUnderPressureLedgerState = 'blocked'
    $reasonCode = 'continuity-under-pressure-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentActiveTaskMapOrdinal -lt 28) {
    $continuityUnderPressureLedgerState = 'awaiting-map-28-activation'
    $reasonCode = 'continuity-under-pressure-ledger-map-not-active'
    $nextAction = 'pull-forward-to-map-28'
} elseif ($currentBondedCrucibleState -ne 'bonded-crucible-session-rehearsal-ready') {
    $continuityUnderPressureLedgerState = 'awaiting-bonded-crucible-session-rehearsal'
    $reasonCode = 'continuity-under-pressure-ledger-crucible-not-ready'
    $nextAction = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] $bondedCrucibleSessionRehearsalState.nextAction } else { 'emit-bonded-crucible-session-rehearsal' }
} elseif ($currentSharedBoundaryMemoryState -ne 'shared-boundary-memory-ledger-ready') {
    $continuityUnderPressureLedgerState = 'awaiting-shared-boundary-memory-ledger'
    $reasonCode = 'continuity-under-pressure-ledger-boundary-memory-not-ready'
    $nextAction = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.nextAction } else { 'emit-shared-boundary-memory-ledger' }
} elseif ($currentCoherenceWitnessState -ne 'coherence-gain-witness-receipt-ready') {
    $continuityUnderPressureLedgerState = 'awaiting-coherence-gain-witness-receipt'
    $reasonCode = 'continuity-under-pressure-ledger-coherence-witness-not-ready'
    $nextAction = if ($null -ne $coherenceGainWitnessReceiptState) { [string] $coherenceGainWitnessReceiptState.nextAction } else { 'emit-coherence-gain-witness-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $ledgerProjectionBound -or -not $ledgerKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $continuityUnderPressureLedgerState = 'awaiting-continuity-under-pressure-binding'
    $reasonCode = 'continuity-under-pressure-ledger-source-missing'
    $nextAction = 'bind-continuity-under-pressure-ledger'
} else {
    $continuityUnderPressureLedgerState = 'continuity-under-pressure-ledger-ready'
    $reasonCode = 'continuity-under-pressure-ledger-bound'
    $nextAction = 'emit-expressive-deformation-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'continuity-under-pressure-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'continuity-under-pressure-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    continuityUnderPressureLedgerState = $continuityUnderPressureLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    activeTaskMapId = $currentActiveTaskMapId
    bondedCrucibleSessionRehearsalState = $currentBondedCrucibleState
    sharedBoundaryMemoryLedgerState = $currentSharedBoundaryMemoryState
    coherenceGainWitnessReceiptState = $currentCoherenceWitnessState
    pressureState = 'continuity-under-pressure-retained'
    heldContinuityCount = 3
    partialContinuityCount = 3
    requiredPreservationCount = 3
    boundaryPressureCount = 3
    fluentSuccessDenied = $true
    ledgerProjectionBound = $ledgerProjectionBound
    ledgerKeyBound = $ledgerKeyBound
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
    '# Continuity Under Pressure Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.continuityUnderPressureLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Active task map: `{0}`' -f $(if ($payload.activeTaskMapId) { $payload.activeTaskMapId } else { 'missing' })),
    ('- Bonded crucible state: `{0}`' -f $(if ($payload.bondedCrucibleSessionRehearsalState) { $payload.bondedCrucibleSessionRehearsalState } else { 'missing' })),
    ('- Shared boundary-memory state: `{0}`' -f $(if ($payload.sharedBoundaryMemoryLedgerState) { $payload.sharedBoundaryMemoryLedgerState } else { 'missing' })),
    ('- Coherence-witness state: `{0}`' -f $(if ($payload.coherenceGainWitnessReceiptState) { $payload.coherenceGainWitnessReceiptState } else { 'missing' })),
    ('- Pressure state: `{0}`' -f $payload.pressureState),
    ('- Held continuity count: `{0}`' -f $payload.heldContinuityCount),
    ('- Partial continuity count: `{0}`' -f $payload.partialContinuityCount),
    ('- Required preservation count: `{0}`' -f $payload.requiredPreservationCount),
    ('- Boundary-pressure count: `{0}`' -f $payload.boundaryPressureCount),
    ('- Fluent success denied: `{0}`' -f [bool] $payload.fluentSuccessDenied),
    ('- Ledger projection bound: `{0}`' -f [bool] $payload.ledgerProjectionBound),
    ('- Ledger key bound: `{0}`' -f [bool] $payload.ledgerKeyBound),
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
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    activeTaskMapId = $payload.activeTaskMapId
    bondedCrucibleSessionRehearsalState = $payload.bondedCrucibleSessionRehearsalState
    sharedBoundaryMemoryLedgerState = $payload.sharedBoundaryMemoryLedgerState
    coherenceGainWitnessReceiptState = $payload.coherenceGainWitnessReceiptState
    pressureState = $payload.pressureState
    heldContinuityCount = $payload.heldContinuityCount
    partialContinuityCount = $payload.partialContinuityCount
    requiredPreservationCount = $payload.requiredPreservationCount
    boundaryPressureCount = $payload.boundaryPressureCount
    fluentSuccessDenied = $payload.fluentSuccessDenied
    ledgerProjectionBound = $payload.ledgerProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[continuity-under-pressure-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
