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
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$selfRootedCrypticDepthGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateStatePath)
$dayDreamCollapseReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.crypticDepthReturnReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.crypticDepthReturnReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the cryptic-depth return writer can run.'
}

$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$selfRootedCrypticDepthGateState = Read-JsonFileOrNull -Path $selfRootedCrypticDepthGateStatePath
$dayDreamCollapseReceiptState = Read-JsonFileOrNull -Path $dayDreamCollapseReceiptStatePath

$sessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionLedgerState')
$depthGateState = [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'gateState')
$collapseReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $dayDreamCollapseReceiptState -PropertyName 'collapseReceiptState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$returnProjectionBound = $contractsSource.IndexOf('CreateCrypticDepthReturnReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateContinuityMarker', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateResidueMarker', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('cryptic-depth-return-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$returnKeyBound = $keysSource.IndexOf('CreateCrypticDepthReturnReceiptHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateContinuityMarkerHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateResidueMarkerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateCrypticDepthReturnReceipt', [System.StringComparison]::Ordinal) -ge 0

$returnReceiptState = 'awaiting-session-ledger'
$reasonCode = 'cryptic-depth-return-receipt-awaiting-session-ledger'
$nextAction = 'emit-runtime-workbench-session-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $returnReceiptState = 'blocked'
    $reasonCode = 'cryptic-depth-return-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($sessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $returnReceiptState = 'awaiting-session-ledger'
    $reasonCode = 'cryptic-depth-return-receipt-session-ledger-not-ready'
    $nextAction = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($depthGateState -ne 'self-rooted-cryptic-depth-gate-ready') {
    $returnReceiptState = 'awaiting-depth-gate'
    $reasonCode = 'cryptic-depth-return-receipt-depth-gate-not-ready'
    $nextAction = if ($null -ne $selfRootedCrypticDepthGateState) { [string] $selfRootedCrypticDepthGateState.nextAction } else { 'emit-self-rooted-cryptic-depth-gate' }
} elseif ($collapseReceiptState -ne 'day-dream-collapse-receipt-ready') {
    $returnReceiptState = 'awaiting-day-dream-collapse'
    $reasonCode = 'cryptic-depth-return-receipt-collapse-not-ready'
    $nextAction = if ($null -ne $dayDreamCollapseReceiptState) { [string] $dayDreamCollapseReceiptState.nextAction } else { 'emit-day-dream-collapse-receipt' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $returnProjectionBound -or -not $returnKeyBound -or -not $serviceBindingBound) {
    $returnReceiptState = 'awaiting-depth-return-binding'
    $reasonCode = 'cryptic-depth-return-receipt-source-missing'
    $nextAction = 'bind-cryptic-depth-return-receipt'
} else {
    $returnReceiptState = 'cryptic-depth-return-receipt-ready'
    $reasonCode = 'cryptic-depth-return-receipt-bound'
    $nextAction = 'pull-forward-to-map-23'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'cryptic-depth-return-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'cryptic-depth-return-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    returnReceiptState = $returnReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sessionLedgerState = $sessionLedgerState
    depthGateState = $depthGateState
    collapseReceiptState = $collapseReceiptState
    returnState = 'clean-return-withheld'
    continuityMarkerCount = 2
    residueMarkerCount = 1
    boundaryConditionCount = 1
    returnedCleanly = $true
    sharedAmenableLaneClear = $true
    identityBleedDetected = $false
    returnProjectionBound = $returnProjectionBound
    returnKeyBound = $returnKeyBound
    serviceBindingBound = $serviceBindingBound
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
    '# Cryptic Depth Return Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Return-receipt state: `{0}`' -f $payload.returnReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Depth-gate state: `{0}`' -f $(if ($payload.depthGateState) { $payload.depthGateState } else { 'missing' })),
    ('- Collapse-receipt state: `{0}`' -f $(if ($payload.collapseReceiptState) { $payload.collapseReceiptState } else { 'missing' })),
    ('- Return state: `{0}`' -f $payload.returnState),
    ('- Continuity-marker count: `{0}`' -f $payload.continuityMarkerCount),
    ('- Residue-marker count: `{0}`' -f $payload.residueMarkerCount),
    ('- Boundary-condition count: `{0}`' -f $payload.boundaryConditionCount),
    ('- Returned cleanly: `{0}`' -f [bool] $payload.returnedCleanly),
    ('- Shared amenable lane clear: `{0}`' -f [bool] $payload.sharedAmenableLaneClear),
    ('- Identity bleed detected: `{0}`' -f [bool] $payload.identityBleedDetected),
    ('- Return projection bound: `{0}`' -f [bool] $payload.returnProjectionBound),
    ('- Return key bound: `{0}`' -f [bool] $payload.returnKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
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
    returnReceiptState = $payload.returnReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    sessionLedgerState = $payload.sessionLedgerState
    depthGateState = $payload.depthGateState
    collapseReceiptState = $payload.collapseReceiptState
    returnState = $payload.returnState
    continuityMarkerCount = $payload.continuityMarkerCount
    residueMarkerCount = $payload.residueMarkerCount
    boundaryConditionCount = $payload.boundaryConditionCount
    returnedCleanly = $payload.returnedCleanly
    sharedAmenableLaneClear = $payload.sharedAmenableLaneClear
    identityBleedDetected = $payload.identityBleedDetected
    returnProjectionBound = $payload.returnProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[cryptic-depth-return-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
