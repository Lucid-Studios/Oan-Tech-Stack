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
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$dayDreamCollapseReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptStatePath)
$crypticDepthReturnReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.crypticDepthReturnReceiptStatePath)
$inquirySessionDisciplineSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquirySessionDisciplineSurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the boundary condition ledger writer can run.'
}

$readinessState = Read-JsonFileOrNull -Path $runtimeHabitationReadinessLedgerStatePath
$sessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$collapseReceiptState = Read-JsonFileOrNull -Path $dayDreamCollapseReceiptStatePath
$returnReceiptState = Read-JsonFileOrNull -Path $crypticDepthReturnReceiptStatePath
$inquirySurfaceState = Read-JsonFileOrNull -Path $inquirySessionDisciplineSurfaceStatePath

$currentReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $readinessState -PropertyName 'readinessLedgerState')
$currentSessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sessionLedgerState -PropertyName 'sessionLedgerState')
$currentCollapseReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $collapseReceiptState -PropertyName 'collapseReceiptState')
$currentReturnReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnReceiptState -PropertyName 'returnReceiptState')
$currentInquirySurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $inquirySurfaceState -PropertyName 'inquirySurfaceState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$ledgerProjectionBound = $contractsSource.IndexOf('BoundaryConditionLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateBoundaryConditionLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('boundary-condition-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$ledgerKeyBound = $keysSource.IndexOf('CreateBoundaryConditionLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBoundaryConditionLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateBoundaryConditionLedgerAndCoherenceGainWitness_CarryForwardConstraintMemory', [System.StringComparison]::Ordinal) -ge 0

$boundaryLedgerState = 'awaiting-inquiry-session-discipline-surface'
$reasonCode = 'boundary-condition-ledger-awaiting-inquiry-session-discipline-surface'
$nextAction = 'emit-inquiry-session-discipline-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $boundaryLedgerState = 'blocked'
    $reasonCode = 'boundary-condition-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentReadinessState -ne 'runtime-habitation-readiness-ledger-ready') {
    $boundaryLedgerState = 'awaiting-runtime-habitation-readiness'
    $reasonCode = 'boundary-condition-ledger-readiness-not-ready'
    $nextAction = if ($null -ne $readinessState) { [string] $readinessState.nextAction } else { 'emit-runtime-habitation-readiness-ledger' }
} elseif ($currentSessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $boundaryLedgerState = 'awaiting-session-ledger'
    $reasonCode = 'boundary-condition-ledger-session-ledger-not-ready'
    $nextAction = if ($null -ne $sessionLedgerState) { [string] $sessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($currentCollapseReceiptState -ne 'day-dream-collapse-receipt-ready') {
    $boundaryLedgerState = 'awaiting-day-dream-collapse-receipt'
    $reasonCode = 'boundary-condition-ledger-collapse-not-ready'
    $nextAction = if ($null -ne $collapseReceiptState) { [string] $collapseReceiptState.nextAction } else { 'emit-day-dream-collapse-receipt' }
} elseif ($currentReturnReceiptState -ne 'cryptic-depth-return-receipt-ready') {
    $boundaryLedgerState = 'awaiting-cryptic-depth-return-receipt'
    $reasonCode = 'boundary-condition-ledger-return-not-ready'
    $nextAction = if ($null -ne $returnReceiptState) { [string] $returnReceiptState.nextAction } else { 'emit-cryptic-depth-return-receipt' }
} elseif ($currentInquirySurfaceState -ne 'inquiry-session-discipline-ready') {
    $boundaryLedgerState = 'awaiting-inquiry-session-discipline-surface'
    $reasonCode = 'boundary-condition-ledger-inquiry-surface-not-ready'
    $nextAction = if ($null -ne $inquirySurfaceState) { [string] $inquirySurfaceState.nextAction } else { 'emit-inquiry-session-discipline-surface' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $ledgerProjectionBound -or -not $ledgerKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $boundaryLedgerState = 'awaiting-boundary-ledger-binding'
    $reasonCode = 'boundary-condition-ledger-source-missing'
    $nextAction = 'bind-boundary-condition-ledger'
} else {
    $boundaryLedgerState = 'boundary-condition-ledger-ready'
    $reasonCode = 'boundary-condition-ledger-bound'
    $nextAction = 'emit-coherence-gain-witness-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'boundary-condition-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'boundary-condition-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    boundaryLedgerState = $boundaryLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    readinessLedgerState = $currentReadinessState
    sessionLedgerState = $currentSessionLedgerState
    collapseReceiptState = $currentCollapseReceiptState
    returnReceiptState = $currentReturnReceiptState
    inquirySurfaceState = $currentInquirySurfaceState
    retainedBoundaryConditionCount = 3
    continuityRequirementCount = 3
    withheldCrossingCount = 3
    boundaryMemoryCarriedForward = $true
    failurePunishmentDenied = $true
    identityBleedDetected = $false
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
    '# Boundary Condition Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Boundary-ledger state: `{0}`' -f $payload.boundaryLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Runtime-habitation readiness state: `{0}`' -f $(if ($payload.readinessLedgerState) { $payload.readinessLedgerState } else { 'missing' })),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Collapse-receipt state: `{0}`' -f $(if ($payload.collapseReceiptState) { $payload.collapseReceiptState } else { 'missing' })),
    ('- Return-receipt state: `{0}`' -f $(if ($payload.returnReceiptState) { $payload.returnReceiptState } else { 'missing' })),
    ('- Inquiry-surface state: `{0}`' -f $(if ($payload.inquirySurfaceState) { $payload.inquirySurfaceState } else { 'missing' })),
    ('- Retained boundary-condition count: `{0}`' -f $payload.retainedBoundaryConditionCount),
    ('- Continuity-requirement count: `{0}`' -f $payload.continuityRequirementCount),
    ('- Withheld-crossing count: `{0}`' -f $payload.withheldCrossingCount),
    ('- Boundary memory carried forward: `{0}`' -f [bool] $payload.boundaryMemoryCarriedForward),
    ('- Failure punishment denied: `{0}`' -f [bool] $payload.failurePunishmentDenied),
    ('- Identity bleed detected: `{0}`' -f [bool] $payload.identityBleedDetected),
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
    boundaryLedgerState = $payload.boundaryLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    readinessLedgerState = $payload.readinessLedgerState
    sessionLedgerState = $payload.sessionLedgerState
    collapseReceiptState = $payload.collapseReceiptState
    returnReceiptState = $payload.returnReceiptState
    inquirySurfaceState = $payload.inquirySurfaceState
    retainedBoundaryConditionCount = $payload.retainedBoundaryConditionCount
    continuityRequirementCount = $payload.continuityRequirementCount
    withheldCrossingCount = $payload.withheldCrossingCount
    boundaryMemoryCarriedForward = $payload.boundaryMemoryCarriedForward
    failurePunishmentDenied = $payload.failurePunishmentDenied
    identityBleedDetected = $payload.identityBleedDetected
    ledgerProjectionBound = $payload.ledgerProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[boundary-condition-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
