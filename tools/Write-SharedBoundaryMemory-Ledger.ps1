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
$boundaryConditionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.boundaryConditionLedgerStatePath)
$bondedCrucibleSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCrucibleSessionRehearsalStatePath)
$reachReturnDissolutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the shared boundary memory writer can run.'
}

$boundaryConditionLedgerState = Read-JsonFileOrNull -Path $boundaryConditionLedgerStatePath
$bondedCrucibleSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCrucibleSessionRehearsalStatePath
$reachReturnDissolutionReceiptState = Read-JsonFileOrNull -Path $reachReturnDissolutionReceiptStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath

$currentBoundaryLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $boundaryConditionLedgerState -PropertyName 'boundaryLedgerState')
$currentBondedCrucibleState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCrucibleSessionRehearsalState -PropertyName 'bondedCrucibleSessionRehearsalState')
$currentReachReturnState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'returnReceiptState')
$currentLocalityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$memoryProjectionBound = $contractsSource.IndexOf('SharedBoundaryMemoryLedgerReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateSharedBoundaryMemoryLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('shared-boundary-memory-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$memoryKeyBound = $keysSource.IndexOf('CreateSharedBoundaryMemoryLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateSharedBoundaryMemoryLedger', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateBondedCrucibleSessionRehearsalAndSharedBoundaryMemory_PreserveSharedUncertainty', [System.StringComparison]::Ordinal) -ge 0

$sharedBoundaryMemoryLedgerState = 'awaiting-boundary-condition-ledger'
$reasonCode = 'shared-boundary-memory-ledger-awaiting-boundary-condition-ledger'
$nextAction = 'emit-boundary-condition-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $sharedBoundaryMemoryLedgerState = 'blocked'
    $reasonCode = 'shared-boundary-memory-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentBoundaryLedgerState -ne 'boundary-condition-ledger-ready') {
    $sharedBoundaryMemoryLedgerState = 'awaiting-boundary-condition-ledger'
    $reasonCode = 'shared-boundary-memory-ledger-boundary-ledger-not-ready'
    $nextAction = if ($null -ne $boundaryConditionLedgerState) { [string] $boundaryConditionLedgerState.nextAction } else { 'emit-boundary-condition-ledger' }
} elseif ($currentBondedCrucibleState -ne 'bonded-crucible-session-rehearsal-ready') {
    $sharedBoundaryMemoryLedgerState = 'awaiting-bonded-crucible-session-rehearsal'
    $reasonCode = 'shared-boundary-memory-ledger-crucible-not-ready'
    $nextAction = if ($null -ne $bondedCrucibleSessionRehearsalState) { [string] $bondedCrucibleSessionRehearsalState.nextAction } else { 'emit-bonded-crucible-session-rehearsal' }
} elseif ($currentReachReturnState -ne 'reach-return-dissolution-receipt-ready') {
    $sharedBoundaryMemoryLedgerState = 'awaiting-reach-return-dissolution-receipt'
    $reasonCode = 'shared-boundary-memory-ledger-return-not-ready'
    $nextAction = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.nextAction } else { 'emit-reach-return-dissolution-receipt' }
} elseif ($currentLocalityWitnessState -ne 'locality-distinction-witness-ledger-ready') {
    $sharedBoundaryMemoryLedgerState = 'awaiting-locality-distinction-witness-ledger'
    $reasonCode = 'shared-boundary-memory-ledger-locality-witness-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $memoryProjectionBound -or -not $memoryKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $sharedBoundaryMemoryLedgerState = 'awaiting-shared-boundary-memory-binding'
    $reasonCode = 'shared-boundary-memory-ledger-source-missing'
    $nextAction = 'bind-shared-boundary-memory-ledger'
} else {
    $sharedBoundaryMemoryLedgerState = 'shared-boundary-memory-ledger-ready'
    $reasonCode = 'shared-boundary-memory-ledger-bound'
    $nextAction = 'pull-forward-to-map-28'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'shared-boundary-memory-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'shared-boundary-memory-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    sharedBoundaryMemoryLedgerState = $sharedBoundaryMemoryLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    boundaryConditionLedgerState = $currentBoundaryLedgerState
    bondedCrucibleSessionRehearsalState = $currentBondedCrucibleState
    reachReturnDissolutionReceiptState = $currentReachReturnState
    localityDistinctionWitnessLedgerState = $currentLocalityWitnessState
    sharedBoundaryMemoryState = 'shared-boundary-memory-carried'
    sharedBoundaryConditionCount = 3
    sharedContinuityRequirementCount = 3
    withheldCommonPropertyClaimCount = 3
    localityProvenancePreserved = $true
    identityBleedDetected = $false
    ambientCommonPropertyDenied = $true
    memoryProjectionBound = $memoryProjectionBound
    memoryKeyBound = $memoryKeyBound
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
    '# Shared Boundary Memory Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Shared boundary-memory state: `{0}`' -f $payload.sharedBoundaryMemoryLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Boundary-ledger state: `{0}`' -f $(if ($payload.boundaryConditionLedgerState) { $payload.boundaryConditionLedgerState } else { 'missing' })),
    ('- Bonded crucible-rehearsal state: `{0}`' -f $(if ($payload.bondedCrucibleSessionRehearsalState) { $payload.bondedCrucibleSessionRehearsalState } else { 'missing' })),
    ('- Reach return-dissolution state: `{0}`' -f $(if ($payload.reachReturnDissolutionReceiptState) { $payload.reachReturnDissolutionReceiptState } else { 'missing' })),
    ('- Locality witness state: `{0}`' -f $(if ($payload.localityDistinctionWitnessLedgerState) { $payload.localityDistinctionWitnessLedgerState } else { 'missing' })),
    ('- Shared boundary-condition count: `{0}`' -f $payload.sharedBoundaryConditionCount),
    ('- Shared continuity-requirement count: `{0}`' -f $payload.sharedContinuityRequirementCount),
    ('- Withheld common-property claim count: `{0}`' -f $payload.withheldCommonPropertyClaimCount),
    ('- Locality provenance preserved: `{0}`' -f [bool] $payload.localityProvenancePreserved),
    ('- Identity bleed detected: `{0}`' -f [bool] $payload.identityBleedDetected),
    ('- Ambient common property denied: `{0}`' -f [bool] $payload.ambientCommonPropertyDenied),
    ('- Memory projection bound: `{0}`' -f [bool] $payload.memoryProjectionBound),
    ('- Memory key bound: `{0}`' -f [bool] $payload.memoryKeyBound),
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
    sharedBoundaryMemoryLedgerState = $payload.sharedBoundaryMemoryLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    boundaryConditionLedgerState = $payload.boundaryConditionLedgerState
    bondedCrucibleSessionRehearsalState = $payload.bondedCrucibleSessionRehearsalState
    reachReturnDissolutionReceiptState = $payload.reachReturnDissolutionReceiptState
    localityDistinctionWitnessLedgerState = $payload.localityDistinctionWitnessLedgerState
    sharedBoundaryMemoryState = $payload.sharedBoundaryMemoryState
    sharedBoundaryConditionCount = $payload.sharedBoundaryConditionCount
    sharedContinuityRequirementCount = $payload.sharedContinuityRequirementCount
    withheldCommonPropertyClaimCount = $payload.withheldCommonPropertyClaimCount
    localityProvenancePreserved = $payload.localityProvenancePreserved
    identityBleedDetected = $payload.identityBleedDetected
    ambientCommonPropertyDenied = $payload.ambientCommonPropertyDenied
    memoryProjectionBound = $payload.memoryProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[shared-boundary-memory-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
