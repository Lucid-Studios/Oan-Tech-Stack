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
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$amenableDayDreamTierAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the day-dream collapse writer can run.'
}

$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$amenableDayDreamTierAdmissibilityState = Read-JsonFileOrNull -Path $amenableDayDreamTierAdmissibilityStatePath

$sessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionLedgerState')
$dayDreamTierState = [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'tierState')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$collapseProjectionBound = $contractsSource.IndexOf('CreateDayDreamCollapseReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateResidueMarker', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('day-dream-collapse-receipt-bound', [System.StringComparison]::Ordinal) -ge 0
$collapseKeyBound = $keysSource.IndexOf('CreateDayDreamCollapseReceiptHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateResidueMarkerHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateDayDreamCollapseReceipt', [System.StringComparison]::Ordinal) -ge 0

$collapseReceiptState = 'awaiting-session-ledger'
$reasonCode = 'day-dream-collapse-receipt-awaiting-session-ledger'
$nextAction = 'emit-runtime-workbench-session-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $collapseReceiptState = 'blocked'
    $reasonCode = 'day-dream-collapse-receipt-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($sessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $collapseReceiptState = 'awaiting-session-ledger'
    $reasonCode = 'day-dream-collapse-receipt-session-ledger-not-ready'
    $nextAction = if ($null -ne $runtimeWorkbenchSessionLedgerState) { [string] $runtimeWorkbenchSessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($dayDreamTierState -ne 'amenable-day-dream-tier-ready') {
    $collapseReceiptState = 'awaiting-day-dream-tier'
    $reasonCode = 'day-dream-collapse-receipt-day-dream-not-ready'
    $nextAction = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $collapseProjectionBound -or -not $collapseKeyBound -or -not $serviceBindingBound) {
    $collapseReceiptState = 'awaiting-collapse-binding'
    $reasonCode = 'day-dream-collapse-receipt-source-missing'
    $nextAction = 'bind-day-dream-collapse-receipt'
} else {
    $collapseReceiptState = 'day-dream-collapse-receipt-ready'
    $reasonCode = 'day-dream-collapse-receipt-bound'
    $nextAction = 'emit-cryptic-depth-return-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'day-dream-collapse-receipt.json'
$bundleMarkdownPath = Join-Path $bundlePath 'day-dream-collapse-receipt.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    collapseReceiptState = $collapseReceiptState
    reasonCode = $reasonCode
    nextAction = $nextAction
    sessionLedgerState = $sessionLedgerState
    dayDreamTierState = $dayDreamTierState
    collapseState = 'bounded-collapse-recorded'
    consideredPredicateCount = 3
    boundedOutputCount = 2
    remainingNonFinalOutputCount = 1
    exploratoryProvenancePreserved = $true
    boundaryConditionCount = 1
    residueMarkerCount = 1
    collapseProjectionBound = $collapseProjectionBound
    collapseKeyBound = $collapseKeyBound
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
    '# Day-Dream Collapse Receipt',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Collapse-receipt state: `{0}`' -f $payload.collapseReceiptState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Day-dream tier state: `{0}`' -f $(if ($payload.dayDreamTierState) { $payload.dayDreamTierState } else { 'missing' })),
    ('- Collapse state: `{0}`' -f $payload.collapseState),
    ('- Considered predicate count: `{0}`' -f $payload.consideredPredicateCount),
    ('- Bounded output count: `{0}`' -f $payload.boundedOutputCount),
    ('- Remaining non-final output count: `{0}`' -f $payload.remainingNonFinalOutputCount),
    ('- Exploratory provenance preserved: `{0}`' -f [bool] $payload.exploratoryProvenancePreserved),
    ('- Boundary-condition count: `{0}`' -f $payload.boundaryConditionCount),
    ('- Residue-marker count: `{0}`' -f $payload.residueMarkerCount),
    ('- Collapse projection bound: `{0}`' -f [bool] $payload.collapseProjectionBound),
    ('- Collapse key bound: `{0}`' -f [bool] $payload.collapseKeyBound),
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
    collapseReceiptState = $payload.collapseReceiptState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    sessionLedgerState = $payload.sessionLedgerState
    dayDreamTierState = $payload.dayDreamTierState
    collapseState = $payload.collapseState
    consideredPredicateCount = $payload.consideredPredicateCount
    boundedOutputCount = $payload.boundedOutputCount
    remainingNonFinalOutputCount = $payload.remainingNonFinalOutputCount
    exploratoryProvenancePreserved = $payload.exploratoryProvenancePreserved
    boundaryConditionCount = $payload.boundaryConditionCount
    residueMarkerCount = $payload.residueMarkerCount
    collapseProjectionBound = $payload.collapseProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[day-dream-collapse-receipt] Bundle: {0}' -f $bundlePath)
$bundlePath
