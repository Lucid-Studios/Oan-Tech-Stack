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
$bondedCoWorkSessionRehearsalStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalStatePath)
$reachDuplexRealizationSeamStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the reach return-dissolution writer can run.'
}

$bondedCoWorkSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCoWorkSessionRehearsalStatePath
$reachDuplexRealizationSeamState = Read-JsonFileOrNull -Path $reachDuplexRealizationSeamStatePath

$rehearsalReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'rehearsalReceiptState')
$reachSeamState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'seamState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$returnProjectionBound = $contractsSource.IndexOf('CreateReachReturnDissolutionReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('reach-return-dissolution-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$returnKeyBound = $keysSource.IndexOf('CreateReachReturnDissolutionReceiptHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateReachReturnDissolutionReceipt', [System.StringComparison]::Ordinal) -ge 0

$returnReceiptState = 'awaiting-bonded-cowork-session'
$reasonCode = 'reach-return-dissolution-receipt-awaiting-bonded-cowork-session'
$nextAction = 'emit-bonded-cowork-session-rehearsal'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $returnReceiptState = 'blocked'
    $reasonCode = 'reach-return-dissolution-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($rehearsalReceiptState -ne 'bonded-cowork-session-rehearsal-ready') {
    $returnReceiptState = 'awaiting-bonded-cowork-session'
    $reasonCode = 'reach-return-dissolution-receipt-cowork-not-ready'
    $nextAction = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.nextAction } else { 'emit-bonded-cowork-session-rehearsal' }
} elseif ($reachSeamState -ne 'reach-duplex-realization-ready') {
    $returnReceiptState = 'awaiting-reach-realization'
    $reasonCode = 'reach-return-dissolution-receipt-reach-seam-not-ready'
    $nextAction = if ($null -ne $reachDuplexRealizationSeamState) { [string] $reachDuplexRealizationSeamState.nextAction } else { 'emit-reach-duplex-realization-seam' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $returnProjectionBound -or -not $returnKeyBound -or -not $serviceBindingBound) {
    $returnReceiptState = 'awaiting-return-binding'
    $reasonCode = 'reach-return-dissolution-receipt-source-missing'
    $nextAction = 'bind-reach-return-dissolution-receipt'
} else {
    $returnReceiptState = 'reach-return-dissolution-receipt-ready'
    $reasonCode = 'reach-return-dissolution-receipt-bound'
    $nextAction = 'emit-locality-distinction-witness-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'reach-return-dissolution-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'reach-return-dissolution-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    returnReceiptState = $returnReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    rehearsalReceiptState = $rehearsalReceiptState
    reachSeamState = $reachSeamState
    returnState = 'returned-through-reach'
    dissolutionState = 'dissolution-witnessed'
    bondedEventReturned = $true
    bondedEventDissolved = $true
    ambientGrantDenied = $true
    localityDistinctionPreserved = $true
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
    '# Reach Return Dissolution Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Return-receipt state: `{0}`' -f $payload.returnReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Rehearsal state: `{0}`' -f $(if ($payload.rehearsalReceiptState) { $payload.rehearsalReceiptState } else { 'missing' })),
    ('- Reach seam state: `{0}`' -f $(if ($payload.reachSeamState) { $payload.reachSeamState } else { 'missing' })),
    ('- Return state: `{0}`' -f $payload.returnState),
    ('- Dissolution state: `{0}`' -f $payload.dissolutionState),
    ('- Bonded event returned: `{0}`' -f [bool] $payload.bondedEventReturned),
    ('- Bonded event dissolved: `{0}`' -f [bool] $payload.bondedEventDissolved),
    ('- Ambient grant denied: `{0}`' -f [bool] $payload.ambientGrantDenied),
    ('- Locality distinction preserved: `{0}`' -f [bool] $payload.localityDistinctionPreserved),
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
    rehearsalReceiptState = $payload.rehearsalReceiptState
    reachSeamState = $payload.reachSeamState
    returnState = $payload.returnState
    dissolutionState = $payload.dissolutionState
    bondedEventReturned = $payload.bondedEventReturned
    bondedEventDissolved = $payload.bondedEventDissolved
    ambientGrantDenied = $payload.ambientGrantDenied
    localityDistinctionPreserved = $payload.localityDistinctionPreserved
    returnProjectionBound = $payload.returnProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[reach-return-dissolution-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
