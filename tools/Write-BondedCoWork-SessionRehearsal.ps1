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
$agentiCoreActualUtilitySurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.agentiCoreActualUtilitySurfaceStatePath)
$reachDuplexRealizationSeamStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.reachDuplexRealizationSeamStatePath)
$bondedParticipationLocalityLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedParticipationLocalityLedgerStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.bondedCoWorkSessionRehearsalStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the bonded co-work session rehearsal writer can run.'
}

$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$agentiCoreActualUtilitySurfaceState = Read-JsonFileOrNull -Path $agentiCoreActualUtilitySurfaceStatePath
$reachDuplexRealizationSeamState = Read-JsonFileOrNull -Path $reachDuplexRealizationSeamStatePath
$bondedParticipationLocalityLedgerState = Read-JsonFileOrNull -Path $bondedParticipationLocalityLedgerStatePath

$sessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionLedgerState')
$utilitySurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $agentiCoreActualUtilitySurfaceState -PropertyName 'utilityState')
$reachSeamState = [string] (Get-ObjectPropertyValueOrNull -InputObject $reachDuplexRealizationSeamState -PropertyName 'seamState')
$localityLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $bondedParticipationLocalityLedgerState -PropertyName 'ledgerState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'agenticore-actual-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$rehearsalProjectionBound = $contractsSource.IndexOf('CreateBondedCoWorkSessionRehearsal', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('bonded-cowork-session-rehearsal-bound', [System.StringComparison]::Ordinal) -ge 0
$rehearsalKeyBound = $keysSource.IndexOf('CreateBondedCoWorkSessionRehearsalHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateBondedCoWorkSessionRehearsal', [System.StringComparison]::Ordinal) -ge 0

$rehearsalReceiptState = 'awaiting-session-ledger'
$reasonCode = 'bonded-cowork-session-rehearsal-awaiting-session-ledger'
$nextAction = 'emit-runtime-workbench-session-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $rehearsalReceiptState = 'blocked'
    $reasonCode = 'bonded-cowork-session-rehearsal-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($sessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $rehearsalReceiptState = 'awaiting-session-ledger'
    $reasonCode = 'bonded-cowork-session-rehearsal-session-ledger-not-ready'
    $nextAction = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($utilitySurfaceState -ne 'agenticore-actual-utility-ready') {
    $rehearsalReceiptState = 'awaiting-utility-surface'
    $reasonCode = 'bonded-cowork-session-rehearsal-utility-surface-not-ready'
    $nextAction = if ($null -ne $agentiCoreActualUtilitySurfaceState) { [string] $agentiCoreActualUtilitySurfaceState.nextAction } else { 'emit-agenticore-actual-utility-surface' }
} elseif ($reachSeamState -ne 'reach-duplex-realization-ready') {
    $rehearsalReceiptState = 'awaiting-reach-realization'
    $reasonCode = 'bonded-cowork-session-rehearsal-reach-seam-not-ready'
    $nextAction = if ($null -ne $reachDuplexRealizationSeamState) { [string] $reachDuplexRealizationSeamState.nextAction } else { 'emit-reach-duplex-realization-seam' }
} elseif ($localityLedgerState -ne 'bonded-locality-ledger-ready') {
    $rehearsalReceiptState = 'awaiting-bonded-locality-ledger'
    $reasonCode = 'bonded-cowork-session-rehearsal-locality-ledger-not-ready'
    $nextAction = if ($null -ne $bondedParticipationLocalityLedgerState) { [string] $bondedParticipationLocalityLedgerState.nextAction } else { 'emit-bonded-participation-locality-ledger' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $rehearsalProjectionBound -or -not $rehearsalKeyBound -or -not $serviceBindingBound) {
    $rehearsalReceiptState = 'awaiting-cowork-binding'
    $reasonCode = 'bonded-cowork-session-rehearsal-source-missing'
    $nextAction = 'bind-bonded-cowork-session-rehearsal'
} else {
    $rehearsalReceiptState = 'bonded-cowork-session-rehearsal-ready'
    $reasonCode = 'bonded-cowork-session-rehearsal-bound'
    $nextAction = 'emit-reach-return-dissolution-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'bonded-cowork-session-rehearsal.json'
$bundleMarkdownPath = Join-Path $bundlePath 'bonded-cowork-session-rehearsal.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    rehearsalReceiptState = $rehearsalReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sessionLedgerState = $sessionLedgerState
    utilitySurfaceState = $utilitySurfaceState
    reachSeamState = $reachSeamState
    localityLedgerState = $localityLedgerState
    rehearsalState = 'bounded-cowork-rehearsal-ready'
    sharedWorkLoopCount = 3
    duplexPredicateLaneCount = 2
    withheldLaneCount = 3
    remoteControlDenied = $true
    localityCollapseDenied = $true
    rehearsalProjectionBound = $rehearsalProjectionBound
    rehearsalKeyBound = $rehearsalKeyBound
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
    '# Bonded Co-Work Session Rehearsal',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Rehearsal state: `{0}`' -f $payload.rehearsalReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Utility-surface state: `{0}`' -f $(if ($payload.utilitySurfaceState) { $payload.utilitySurfaceState } else { 'missing' })),
    ('- Reach seam state: `{0}`' -f $(if ($payload.reachSeamState) { $payload.reachSeamState } else { 'missing' })),
    ('- Locality-ledger state: `{0}`' -f $(if ($payload.localityLedgerState) { $payload.localityLedgerState } else { 'missing' })),
    ('- Shared work-loop count: `{0}`' -f $payload.sharedWorkLoopCount),
    ('- Duplex predicate-lane count: `{0}`' -f $payload.duplexPredicateLaneCount),
    ('- Withheld lane count: `{0}`' -f $payload.withheldLaneCount),
    ('- Remote control denied: `{0}`' -f [bool] $payload.remoteControlDenied),
    ('- Locality collapse denied: `{0}`' -f [bool] $payload.localityCollapseDenied),
    ('- Rehearsal projection bound: `{0}`' -f [bool] $payload.rehearsalProjectionBound),
    ('- Rehearsal key bound: `{0}`' -f [bool] $payload.rehearsalKeyBound),
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
    rehearsalReceiptState = $payload.rehearsalReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    sessionLedgerState = $payload.sessionLedgerState
    utilitySurfaceState = $payload.utilitySurfaceState
    reachSeamState = $payload.reachSeamState
    localityLedgerState = $payload.localityLedgerState
    rehearsalState = $payload.rehearsalState
    sharedWorkLoopCount = $payload.sharedWorkLoopCount
    duplexPredicateLaneCount = $payload.duplexPredicateLaneCount
    withheldLaneCount = $payload.withheldLaneCount
    remoteControlDenied = $payload.remoteControlDenied
    localityCollapseDenied = $payload.localityCollapseDenied
    rehearsalProjectionBound = $payload.rehearsalProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[bonded-cowork-session-rehearsal] Bundle: {0}' -f $bundlePath)
$bundlePath
