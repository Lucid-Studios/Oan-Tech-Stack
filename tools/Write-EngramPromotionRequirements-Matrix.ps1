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

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$engramDistanceClassificationLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramDistanceClassificationLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramPromotionRequirementsMatrixOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.engramPromotionRequirementsMatrixStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the engram-promotion requirements writer can run.'
}

$engramDistanceClassificationLedgerState = Read-JsonFileOrNull -Path $engramDistanceClassificationLedgerStatePath
$currentEngramDistanceClassificationLedgerState = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.engramDistanceClassificationLedgerState } else { $null }

$sourceFiles = @(
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationContracts.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/Oan.Common/AgentiActualizationKeys.cs'),
    (Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath 'OAN Mortalis V1.0/src/AgentiCore/Services/GovernedReachRealizationService.cs')
)
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }

$matrixProjectionBound = $contractsSource.IndexOf('EngramPromotionRequirementsMatrixReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateEngramPromotionRequirementsMatrix', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('engram-promotion-requirements-matrix-bound', [System.StringComparison]::Ordinal) -ge 0
$matrixKeyBound = $keysSource.IndexOf('CreateEngramPromotionRequirementsMatrixHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateEngramPromotionRequirementsMatrix', [System.StringComparison]::Ordinal) -ge 0

$engramPromotionRequirementsMatrixState = 'awaiting-engram-distance-classification-ledger'
$reasonCode = 'engram-promotion-requirements-matrix-awaiting-classification'
$nextAction = 'emit-engram-distance-classification-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $engramPromotionRequirementsMatrixState = 'blocked'
    $reasonCode = 'engram-promotion-requirements-matrix-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentEngramDistanceClassificationLedgerState -ne 'engram-distance-classification-ledger-ready') {
    $engramPromotionRequirementsMatrixState = 'awaiting-engram-distance-classification-ledger'
    $reasonCode = 'engram-promotion-requirements-matrix-classification-not-ready'
    $nextAction = if ($null -ne $engramDistanceClassificationLedgerState) { [string] $engramDistanceClassificationLedgerState.nextAction } else { 'emit-engram-distance-classification-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $matrixProjectionBound -or -not $matrixKeyBound -or -not $serviceBindingBound) {
    $engramPromotionRequirementsMatrixState = 'awaiting-engram-promotion-requirements-binding'
    $reasonCode = 'engram-promotion-requirements-matrix-source-missing'
    $nextAction = 'bind-engram-promotion-requirements-matrix'
} else {
    $engramPromotionRequirementsMatrixState = 'engram-promotion-requirements-matrix-ready'
    $reasonCode = 'engram-promotion-requirements-matrix-bound'
    $nextAction = 'emit-distance-weighted-questioning-admission-surface'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'engram-promotion-requirements-matrix.json'
$bundleMarkdownPath = Join-Path $bundlePath 'engram-promotion-requirements-matrix.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    engramPromotionRequirementsMatrixState = $engramPromotionRequirementsMatrixState
    reasonCode = $reasonCode
    nextAction = $nextAction
    engramDistanceClassificationLedgerState = $currentEngramDistanceClassificationLedgerState
    requirementEntryCount = 4
    burdenScalingPreserved = $true
    portableInheritanceRequiresVariation = $true
    dominantAdjacentPromotionCeiling = 'GuardedCandidateReview'
    dominantAdjacentUnknownTolerance = 1
    dominantAdjacentRequiredReentryDepth = 1
    matrixProjectionBound = $matrixProjectionBound
    matrixKeyBound = $matrixKeyBound
    serviceBindingBound = $serviceBindingBound
    sourceFileCount = @($sourceFiles).Count
    missingSourceFileCount = @($missingSourceFiles).Count
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# Engram Promotion Requirements Matrix',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Matrix state: `{0}`' -f $payload.engramPromotionRequirementsMatrixState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Distance-classification ledger state: `{0}`' -f $(if ($payload.engramDistanceClassificationLedgerState) { $payload.engramDistanceClassificationLedgerState } else { 'missing' })),
    ('- Requirement entry count: `{0}`' -f $payload.requirementEntryCount),
    ('- Burden scaling preserved: `{0}`' -f [bool] $payload.burdenScalingPreserved),
    ('- Portable inheritance requires variation: `{0}`' -f [bool] $payload.portableInheritanceRequiresVariation),
    ('- Adjacent-root promotion ceiling: `{0}`' -f $payload.dominantAdjacentPromotionCeiling),
    ('- Adjacent-root unknown tolerance: `{0}`' -f $payload.dominantAdjacentUnknownTolerance),
    ('- Adjacent-root required re-entry depth: `{0}`' -f $payload.dominantAdjacentRequiredReentryDepth),
    ('- Matrix projection bound: `{0}`' -f [bool] $payload.matrixProjectionBound),
    ('- Matrix key bound: `{0}`' -f [bool] $payload.matrixKeyBound),
    ('- Service binding bound: `{0}`' -f [bool] $payload.serviceBindingBound),
    ('- Source files present: `{0}/{1}`' -f ($payload.sourceFileCount - $payload.missingSourceFileCount), $payload.sourceFileCount)
)

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    engramPromotionRequirementsMatrixState = $payload.engramPromotionRequirementsMatrixState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    engramDistanceClassificationLedgerState = $payload.engramDistanceClassificationLedgerState
    requirementEntryCount = $payload.requirementEntryCount
    burdenScalingPreserved = $payload.burdenScalingPreserved
    portableInheritanceRequiresVariation = $payload.portableInheritanceRequiresVariation
    dominantAdjacentPromotionCeiling = $payload.dominantAdjacentPromotionCeiling
    dominantAdjacentUnknownTolerance = $payload.dominantAdjacentUnknownTolerance
    dominantAdjacentRequiredReentryDepth = $payload.dominantAdjacentRequiredReentryDepth
}

Write-JsonFile -Path $statePath -Value $statePayload
$bundlePath
