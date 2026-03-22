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
$agentiCoreActualUtilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.agentiCoreActualUtilitySurfaceStatePath)
$reachAccessTopologyLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachAccessTopologyLedgerStatePath)
$protectedStateLegibilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.protectedStateLegibilitySurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the reach duplex-realization seam writer can run.'
}

$agentiCoreActualUtilitySurfaceState = Read-JsonFileOrNull -Path $agentiCoreActualUtilitySurfaceStatePath
$reachAccessTopologyLedgerState = Read-JsonFileOrNull -Path $reachAccessTopologyLedgerStatePath
$protectedStateLegibilitySurfaceState = Read-JsonFileOrNull -Path $protectedStateLegibilitySurfaceStatePath

$utilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $agentiCoreActualUtilitySurfaceState -PropertyName 'utilityState')
$reachLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachAccessTopologyLedgerState -PropertyName 'ledgerState')
$protectedLegibilityState = [string] (Get-ObjectPropertyValueOrNull -InputObject $protectedStateLegibilitySurfaceState -PropertyName 'legibilityState')

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore.Runtime/ReachDuplexRealizationSurfaceContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore.Runtime/AgentiRuntime.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedReachRealizationService.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationContracts.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$runtimeSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$commonSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }
$contractProjectionBound = $contractsSource.IndexOf('CreateEnvelope', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreatePacket', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('reach-duplex-dispatch-accepted', [System.StringComparison]::Ordinal) -ge 0
$runtimeDispatchBound = $runtimeSource.IndexOf('SendReachDuplexRealizationAsync', [System.StringComparison]::Ordinal) -ge 0
$serviceEnvelopeBindingBound = $serviceSource.IndexOf('CreateReachDuplexRealizationEnvelope', [System.StringComparison]::Ordinal) -ge 0 -and
    $serviceSource.IndexOf('CreateReachDuplexRealizationReceipt', [System.StringComparison]::Ordinal) -ge 0
$commonReceiptBound = $commonSource.IndexOf('CreateReachDuplexRealizationReceipt', [System.StringComparison]::Ordinal) -ge 0

$seamState = 'awaiting-agenticore-actual-utility'
$reasonCode = 'reach-duplex-realization-awaiting-agenticore-actual-utility'
$nextAction = 'emit-agenticore-actual-utility-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $seamState = 'blocked'
    $reasonCode = 'reach-duplex-realization-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($utilityState -ne 'agenticore-actual-utility-ready') {
    $seamState = 'awaiting-agenticore-actual-utility'
    $reasonCode = 'reach-duplex-realization-utility-not-ready'
    $nextAction = if ($null -ne $agentiCoreActualUtilitySurfaceState) { [string] $agentiCoreActualUtilitySurfaceState.nextAction } else { 'emit-agenticore-actual-utility-surface' }
} elseif ([string]::IsNullOrWhiteSpace($reachLedgerState) -or $reachLedgerState -eq 'blocked') {
    $seamState = 'awaiting-reach-legibility'
    $reasonCode = 'reach-duplex-realization-ledger-not-ready'
    $nextAction = if ($null -ne $reachAccessTopologyLedgerState) { [string] $reachAccessTopologyLedgerState.nextAction } else { 'emit-reach-access-topology-ledger' }
} elseif ($protectedLegibilityState -ne 'bounded-legibility-ready') {
    $seamState = 'awaiting-protected-legibility'
    $reasonCode = 'reach-duplex-realization-protected-legibility-not-ready'
    $nextAction = if ($null -ne $protectedStateLegibilitySurfaceState) { [string] $protectedStateLegibilitySurfaceState.nextAction } else { 'emit-protected-state-legibility-surface' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $contractProjectionBound -or -not $runtimeDispatchBound -or -not $serviceEnvelopeBindingBound -or -not $commonReceiptBound) {
    $seamState = 'awaiting-reach-binding'
    $reasonCode = 'reach-duplex-realization-source-missing'
    $nextAction = 'bind-reach-duplex-realization-seam'
} else {
    $seamState = 'reach-duplex-realization-ready'
    $reasonCode = 'reach-duplex-realization-seam-bound'
    $nextAction = 'emit-bonded-participation-locality-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'reach-duplex-realization-seam.json'
$bundleMarkdownPath = Join-Path $bundlePath 'reach-duplex-realization-seam.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    seamState = $seamState
    reasonCode = $reasonCode
    nextAction = $nextAction
    utilityState = $utilityState
    reachAccessTopologyState = $reachLedgerState
    protectedLegibilityState = $protectedLegibilityState
    bondedSpaceHandleClass = 'bonded-space://'
    grantImplied = $false
    contractProjectionBound = $contractProjectionBound
    runtimeDispatchBound = $runtimeDispatchBound
    serviceEnvelopeBindingBound = $serviceEnvelopeBindingBound
    commonReceiptBound = $commonReceiptBound
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
    '# Reach Duplex Realization Seam',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Seam state: `{0}`' -f $payload.seamState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Utility state: `{0}`' -f $(if ($payload.utilityState) { $payload.utilityState } else { 'missing' })),
    ('- Reach access topology state: `{0}`' -f $(if ($payload.reachAccessTopologyState) { $payload.reachAccessTopologyState } else { 'missing' })),
    ('- Protected legibility state: `{0}`' -f $(if ($payload.protectedLegibilityState) { $payload.protectedLegibilityState } else { 'missing' })),
    ('- Bonded-space handle class: `{0}`' -f $payload.bondedSpaceHandleClass),
    ('- Grant implied: `{0}`' -f [bool] $payload.grantImplied),
    ('- Contract projection bound: `{0}`' -f [bool] $payload.contractProjectionBound),
    ('- Runtime dispatch bound: `{0}`' -f [bool] $payload.runtimeDispatchBound),
    ('- Service envelope binding bound: `{0}`' -f [bool] $payload.serviceEnvelopeBindingBound),
    ('- Common receipt bound: `{0}`' -f [bool] $payload.commonReceiptBound),
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
    seamState = $payload.seamState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    utilityState = $payload.utilityState
    reachAccessTopologyState = $payload.reachAccessTopologyState
    protectedLegibilityState = $payload.protectedLegibilityState
    grantImplied = $payload.grantImplied
    runtimeDispatchBound = $payload.runtimeDispatchBound
    serviceEnvelopeBindingBound = $payload.serviceEnvelopeBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[reach-duplex-realization-seam] Bundle: {0}' -f $bundlePath)
$bundlePath
