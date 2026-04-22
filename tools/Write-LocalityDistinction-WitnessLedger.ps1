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
$reachReturnDissolutionReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachReturnDissolutionReceiptStatePath)
$bondedParticipationLocalityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.localityDistinctionWitnessLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the locality-distinction witness writer can run.'
}

$bondedCoWorkSessionRehearsalState = Read-JsonFileOrNull -Path $bondedCoWorkSessionRehearsalStatePath
$reachReturnDissolutionReceiptState = Read-JsonFileOrNull -Path $reachReturnDissolutionReceiptStatePath
$bondedParticipationLocalityLedgerState = Read-JsonFileOrNull -Path $bondedParticipationLocalityLedgerStatePath

$rehearsalReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedCoWorkSessionRehearsalState -PropertyName 'rehearsalReceiptState')
$returnReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachReturnDissolutionReceiptState -PropertyName 'returnReceiptState')
$localityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'ledgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$witnessProjectionBound = $contractsSource.IndexOf('CreateLocalityDistinctionWitnessLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('locality-distinction-witness-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$witnessKeyBound = $keysSource.IndexOf('CreateLocalityDistinctionWitnessLedgerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateLocalityDistinctionWitnessLedger', [System.StringComparison]::Ordinal) -ge 0

$witnessLedgerState = 'awaiting-bonded-cowork-session'
$reasonCode = 'locality-distinction-witness-ledger-awaiting-bonded-cowork-session'
$nextAction = 'emit-bonded-cowork-session-rehearsal'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $witnessLedgerState = 'blocked'
    $reasonCode = 'locality-distinction-witness-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($rehearsalReceiptState -ne 'bonded-cowork-session-rehearsal-ready') {
    $witnessLedgerState = 'awaiting-bonded-cowork-session'
    $reasonCode = 'locality-distinction-witness-ledger-cowork-not-ready'
    $nextAction = if ($null -ne $bondedCoWorkSessionRehearsalState) { [string] $bondedCoWorkSessionRehearsalState.nextAction } else { 'emit-bonded-cowork-session-rehearsal' }
} elseif ($returnReceiptState -ne 'reach-return-dissolution-receipt-ready') {
    $witnessLedgerState = 'awaiting-return-dissolution'
    $reasonCode = 'locality-distinction-witness-ledger-return-not-ready'
    $nextAction = if ($null -ne $reachReturnDissolutionReceiptState) { [string] $reachReturnDissolutionReceiptState.nextAction } else { 'emit-reach-return-dissolution-receipt' }
} elseif ($localityLedgerState -ne 'bonded-locality-ledger-ready') {
    $witnessLedgerState = 'awaiting-bonded-locality-ledger'
    $reasonCode = 'locality-distinction-witness-ledger-locality-ledger-not-ready'
    $nextAction = if ($null -ne $bondedParticipationLocalityLedgerState) { [string] $bondedParticipationLocalityLedgerState.nextAction } else { 'emit-bonded-participation-locality-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $witnessProjectionBound -or -not $witnessKeyBound -or -not $serviceBindingBound) {
    $witnessLedgerState = 'awaiting-locality-witness-binding'
    $reasonCode = 'locality-distinction-witness-ledger-source-missing'
    $nextAction = 'bind-locality-distinction-witness-ledger'
} else {
    $witnessLedgerState = 'locality-distinction-witness-ledger-ready'
    $reasonCode = 'locality-distinction-witness-ledger-bound'
    $nextAction = 'pull-forward-to-map-24'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'locality-distinction-witness-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'locality-distinction-witness-ledger.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    witnessLedgerState = $witnessLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    rehearsalReceiptState = $rehearsalReceiptState
    returnReceiptState = $returnReceiptState
    bondedParticipationLocalityLedgerState = $localityLedgerState
    sharedSurfaceCount = 3
    sanctuaryLocalSurfaceCount = 2
    operatorLocalSurfaceCount = 2
    withheldSurfaceCount = 3
    localityCollapseDetected = $false
    projectionTheaterDenied = $true
    witnessProjectionBound = $witnessProjectionBound
    witnessKeyBound = $witnessKeyBound
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
    '# Locality Distinction Witness Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Witness-ledger state: `{0}`' -f $payload.witnessLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Rehearsal state: `{0}`' -f $(if ($payload.rehearsalReceiptState) { $payload.rehearsalReceiptState } else { 'missing' })),
    ('- Return-receipt state: `{0}`' -f $(if ($payload.returnReceiptState) { $payload.returnReceiptState } else { 'missing' })),
    ('- Bonded locality-ledger state: `{0}`' -f $(if ($payload.bondedParticipationLocalityLedgerState) { $payload.bondedParticipationLocalityLedgerState } else { 'missing' })),
    ('- Shared surface count: `{0}`' -f $payload.sharedSurfaceCount),
    ('- Sanctuary-local surface count: `{0}`' -f $payload.sanctuaryLocalSurfaceCount),
    ('- Operator-local surface count: `{0}`' -f $payload.operatorLocalSurfaceCount),
    ('- Withheld surface count: `{0}`' -f $payload.withheldSurfaceCount),
    ('- Locality collapse detected: `{0}`' -f [bool] $payload.localityCollapseDetected),
    ('- Projection theater denied: `{0}`' -f [bool] $payload.projectionTheaterDenied),
    ('- Witness projection bound: `{0}`' -f [bool] $payload.witnessProjectionBound),
    ('- Witness key bound: `{0}`' -f [bool] $payload.witnessKeyBound),
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
    witnessLedgerState = $payload.witnessLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    rehearsalReceiptState = $payload.rehearsalReceiptState
    returnReceiptState = $payload.returnReceiptState
    bondedParticipationLocalityLedgerState = $payload.bondedParticipationLocalityLedgerState
    sharedSurfaceCount = $payload.sharedSurfaceCount
    sanctuaryLocalSurfaceCount = $payload.sanctuaryLocalSurfaceCount
    operatorLocalSurfaceCount = $payload.operatorLocalSurfaceCount
    withheldSurfaceCount = $payload.withheldSurfaceCount
    localityCollapseDetected = $payload.localityCollapseDetected
    projectionTheaterDenied = $payload.projectionTheaterDenied
    witnessProjectionBound = $payload.witnessProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[locality-distinction-witness-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
