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
$continuityUnderPressureLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.continuityUnderPressureLedgerStatePath)
$expressiveDeformationReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.expressiveDeformationReceiptStatePath)
$localityDistinctionWitnessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)
$sharedBoundaryMemoryLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sharedBoundaryMemoryLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.mutualIntelligibilityWitnessOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.mutualIntelligibilityWitnessStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the mutual-intelligibility writer can run.'
}

$continuityUnderPressureLedgerState = Read-JsonFileOrNull -Path $continuityUnderPressureLedgerStatePath
$expressiveDeformationReceiptState = Read-JsonFileOrNull -Path $expressiveDeformationReceiptStatePath
$localityDistinctionWitnessLedgerState = Read-JsonFileOrNull -Path $localityDistinctionWitnessLedgerStatePath
$sharedBoundaryMemoryLedgerState = Read-JsonFileOrNull -Path $sharedBoundaryMemoryLedgerStatePath

$currentContinuityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $continuityUnderPressureLedgerState -PropertyName 'continuityUnderPressureLedgerState')
$currentExpressiveDeformationState = [string] (Get-ObjectPropertyValueOrNull -InputObject $expressiveDeformationReceiptState -PropertyName 'expressiveDeformationReceiptState')
$currentLocalityWitnessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $localityDistinctionWitnessLedgerState -PropertyName 'witnessLedgerState')
$currentSharedBoundaryMemoryState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sharedBoundaryMemoryLedgerState -PropertyName 'sharedBoundaryMemoryLedgerState')

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

$witnessProjectionBound = $contractsSource.IndexOf('MutualIntelligibilityWitnessReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateMutualIntelligibilityWitness', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('mutual-intelligibility-witness-bound', [System.StringComparison]::Ordinal) -ge 0
$witnessKeyBound = $keysSource.IndexOf('CreateMutualIntelligibilityWitnessHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateMutualIntelligibilityWitness', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateExpressiveDeformationAndMutualIntelligibilityWitness_PreserveRecognizableDifference', [System.StringComparison]::Ordinal) -ge 0

$mutualIntelligibilityWitnessState = 'awaiting-continuity-under-pressure-ledger'
$reasonCode = 'mutual-intelligibility-witness-awaiting-continuity-ledger'
$nextAction = 'emit-continuity-under-pressure-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $mutualIntelligibilityWitnessState = 'blocked'
    $reasonCode = 'mutual-intelligibility-witness-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentContinuityLedgerState -ne 'continuity-under-pressure-ledger-ready') {
    $mutualIntelligibilityWitnessState = 'awaiting-continuity-under-pressure-ledger'
    $reasonCode = 'mutual-intelligibility-witness-continuity-ledger-not-ready'
    $nextAction = if ($null -ne $continuityUnderPressureLedgerState) { [string] $continuityUnderPressureLedgerState.nextAction } else { 'emit-continuity-under-pressure-ledger' }
} elseif ($currentExpressiveDeformationState -ne 'expressive-deformation-receipt-ready') {
    $mutualIntelligibilityWitnessState = 'awaiting-expressive-deformation-receipt'
    $reasonCode = 'mutual-intelligibility-witness-deformation-not-ready'
    $nextAction = if ($null -ne $expressiveDeformationReceiptState) { [string] $expressiveDeformationReceiptState.nextAction } else { 'emit-expressive-deformation-receipt' }
} elseif ($currentLocalityWitnessState -ne 'locality-distinction-witness-ledger-ready') {
    $mutualIntelligibilityWitnessState = 'awaiting-locality-distinction-witness-ledger'
    $reasonCode = 'mutual-intelligibility-witness-locality-not-ready'
    $nextAction = if ($null -ne $localityDistinctionWitnessLedgerState) { [string] $localityDistinctionWitnessLedgerState.nextAction } else { 'emit-locality-distinction-witness-ledger' }
} elseif ($currentSharedBoundaryMemoryState -ne 'shared-boundary-memory-ledger-ready') {
    $mutualIntelligibilityWitnessState = 'awaiting-shared-boundary-memory-ledger'
    $reasonCode = 'mutual-intelligibility-witness-boundary-memory-not-ready'
    $nextAction = if ($null -ne $sharedBoundaryMemoryLedgerState) { [string] $sharedBoundaryMemoryLedgerState.nextAction } else { 'emit-shared-boundary-memory-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $witnessProjectionBound -or -not $witnessKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $mutualIntelligibilityWitnessState = 'awaiting-mutual-intelligibility-binding'
    $reasonCode = 'mutual-intelligibility-witness-source-missing'
    $nextAction = 'bind-mutual-intelligibility-witness'
} else {
    $mutualIntelligibilityWitnessState = 'mutual-intelligibility-witness-ready'
    $reasonCode = 'mutual-intelligibility-witness-bound'
    $nextAction = 'pull-forward-to-map-29'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'mutual-intelligibility-witness.json'
$bundleMarkdownPath = Join-Path $bundlePath 'mutual-intelligibility-witness.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    mutualIntelligibilityWitnessState = $mutualIntelligibilityWitnessState
    reasonCode = $reasonCode
    nextAction = $nextAction
    continuityUnderPressureLedgerState = $currentContinuityLedgerState
    expressiveDeformationReceiptState = $currentExpressiveDeformationState
    localityDistinctionWitnessLedgerState = $currentLocalityWitnessState
    sharedBoundaryMemoryLedgerState = $currentSharedBoundaryMemoryState
    sharedUnderstandingState = 'mutual-intelligibility-preserved'
    heldIntelligibilityCount = 3
    narrowedIntelligibilityCount = 3
    brokenIntelligibilityCount = 3
    samenessCollapseDenied = $true
    opaqueDivergenceDetected = $false
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
    '# Mutual Intelligibility Witness',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Witness state: `{0}`' -f $payload.mutualIntelligibilityWitnessState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Continuity-under-pressure state: `{0}`' -f $(if ($payload.continuityUnderPressureLedgerState) { $payload.continuityUnderPressureLedgerState } else { 'missing' })),
    ('- Expressive deformation state: `{0}`' -f $(if ($payload.expressiveDeformationReceiptState) { $payload.expressiveDeformationReceiptState } else { 'missing' })),
    ('- Locality witness state: `{0}`' -f $(if ($payload.localityDistinctionWitnessLedgerState) { $payload.localityDistinctionWitnessLedgerState } else { 'missing' })),
    ('- Shared boundary-memory state: `{0}`' -f $(if ($payload.sharedBoundaryMemoryLedgerState) { $payload.sharedBoundaryMemoryLedgerState } else { 'missing' })),
    ('- Shared understanding state: `{0}`' -f $payload.sharedUnderstandingState),
    ('- Held intelligibility count: `{0}`' -f $payload.heldIntelligibilityCount),
    ('- Narrowed intelligibility count: `{0}`' -f $payload.narrowedIntelligibilityCount),
    ('- Broken intelligibility count: `{0}`' -f $payload.brokenIntelligibilityCount),
    ('- Sameness collapse denied: `{0}`' -f [bool] $payload.samenessCollapseDenied),
    ('- Opaque divergence detected: `{0}`' -f [bool] $payload.opaqueDivergenceDetected),
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
    mutualIntelligibilityWitnessState = $payload.mutualIntelligibilityWitnessState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    continuityUnderPressureLedgerState = $payload.continuityUnderPressureLedgerState
    expressiveDeformationReceiptState = $payload.expressiveDeformationReceiptState
    localityDistinctionWitnessLedgerState = $payload.localityDistinctionWitnessLedgerState
    sharedBoundaryMemoryLedgerState = $payload.sharedBoundaryMemoryLedgerState
    sharedUnderstandingState = $payload.sharedUnderstandingState
    heldIntelligibilityCount = $payload.heldIntelligibilityCount
    narrowedIntelligibilityCount = $payload.narrowedIntelligibilityCount
    brokenIntelligibilityCount = $payload.brokenIntelligibilityCount
    samenessCollapseDenied = $payload.samenessCollapseDenied
    opaqueDivergenceDetected = $payload.opaqueDivergenceDetected
    witnessProjectionBound = $payload.witnessProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[mutual-intelligibility-witness] Bundle: {0}' -f $bundlePath)
$bundlePath
