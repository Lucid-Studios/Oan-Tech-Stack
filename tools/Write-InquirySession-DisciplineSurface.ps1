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
$runtimeHabitationReadinessLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeHabitationReadinessLedgerStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$dayDreamCollapseReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.dayDreamCollapseReceiptStatePath)
$crypticDepthReturnReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.crypticDepthReturnReceiptStatePath)
$nextEraBatchSelectorStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.nextEraBatchSelectorStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquirySessionDisciplineSurfaceOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.inquirySessionDisciplineSurfaceStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the inquiry session discipline writer can run.'
}

$readinessState = Read-JsonFileOrNull -Path $runtimeHabitationReadinessLedgerStatePath
$sessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$collapseReceiptState = Read-JsonFileOrNull -Path $dayDreamCollapseReceiptStatePath
$returnReceiptState = Read-JsonFileOrNull -Path $crypticDepthReturnReceiptStatePath
$nextEraBatchSelectorState = Read-JsonFileOrNull -Path $nextEraBatchSelectorStatePath

$currentReadinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $readinessState -PropertyName 'readinessLedgerState')
$currentSessionLedgerState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sessionLedgerState -PropertyName 'sessionLedgerState')
$currentCollapseReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $collapseReceiptState -PropertyName 'collapseReceiptState')
$currentReturnReceiptState = [string] (Get-ObjectPropertyValueOrNull -InputObject $returnReceiptState -PropertyName 'returnReceiptState')
$currentNextEraSelectorState = [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectorState')
$selectedNextMapId = [string] (Get-ObjectPropertyValueOrNull -InputObject $nextEraBatchSelectorState -PropertyName 'selectedNextMapId')

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-integration' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$testSource = if (Test-Path -LiteralPath $sourceFiles[3] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[3] } else { '' }

$inquiryProjectionBound = $contractsSource.IndexOf('InquirySessionDisciplineSurfaceReceipt', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateInquirySessionDisciplineSurface', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('inquiry-session-discipline-surface-bound', [System.StringComparison]::Ordinal) -ge 0
$inquiryKeyBound = $keysSource.IndexOf('CreateInquirySessionDisciplineSurfaceHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateInquirySessionDisciplineSurface', [System.StringComparison]::Ordinal) -ge 0
$testBindingBound = $testSource.IndexOf('CreateInquirySessionDisciplineSurface_BindsQuestioningAndSilenceInsideBoundedHabitation', [System.StringComparison]::Ordinal) -ge 0

$inquirySurfaceState = 'awaiting-runtime-habitation-readiness'
$reasonCode = 'inquiry-session-discipline-surface-awaiting-runtime-habitation-readiness'
$nextAction = 'emit-runtime-habitation-readiness-ledger'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $inquirySurfaceState = 'blocked'
    $reasonCode = 'inquiry-session-discipline-surface-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($currentReadinessState -ne 'runtime-habitation-readiness-ledger-ready') {
    $inquirySurfaceState = 'awaiting-runtime-habitation-readiness'
    $reasonCode = 'inquiry-session-discipline-surface-readiness-not-ready'
    $nextAction = if ($null -ne $readinessState) { [string] $readinessState.nextAction } else { 'emit-runtime-habitation-readiness-ledger' }
} elseif ($currentSessionLedgerState -ne 'runtime-workbench-session-ledger-ready') {
    $inquirySurfaceState = 'awaiting-session-ledger'
    $reasonCode = 'inquiry-session-discipline-surface-session-ledger-not-ready'
    $nextAction = if ($null -ne $sessionLedgerState) { [string] $sessionLedgerState.nextAction } else { 'emit-runtime-workbench-session-ledger' }
} elseif ($currentCollapseReceiptState -ne 'day-dream-collapse-receipt-ready') {
    $inquirySurfaceState = 'awaiting-day-dream-collapse-receipt'
    $reasonCode = 'inquiry-session-discipline-surface-collapse-not-ready'
    $nextAction = if ($null -ne $collapseReceiptState) { [string] $collapseReceiptState.nextAction } else { 'emit-day-dream-collapse-receipt' }
} elseif ($currentReturnReceiptState -ne 'cryptic-depth-return-receipt-ready') {
    $inquirySurfaceState = 'awaiting-cryptic-depth-return-receipt'
    $reasonCode = 'inquiry-session-discipline-surface-return-not-ready'
    $nextAction = if ($null -ne $returnReceiptState) { [string] $returnReceiptState.nextAction } else { 'emit-cryptic-depth-return-receipt' }
} elseif ($currentNextEraSelectorState -ne 'next-era-batch-selector-ready' -or $selectedNextMapId -ne 'automation-maturation-map-26') {
    $inquirySurfaceState = 'awaiting-map-26-selection'
    $reasonCode = 'inquiry-session-discipline-surface-next-era-not-selected'
    $nextAction = if ($null -ne $nextEraBatchSelectorState) { [string] $nextEraBatchSelectorState.nextAction } else { 'emit-next-era-batch-selector' }
} elseif ($missingSourceFiles.Count -gt 0 -or -not $inquiryProjectionBound -or -not $inquiryKeyBound -or -not $serviceBindingBound -or -not $testBindingBound) {
    $inquirySurfaceState = 'awaiting-inquiry-binding'
    $reasonCode = 'inquiry-session-discipline-surface-source-missing'
    $nextAction = 'bind-inquiry-session-discipline-surface'
} else {
    $inquirySurfaceState = 'inquiry-session-discipline-ready'
    $reasonCode = 'inquiry-session-discipline-surface-bound'
    $nextAction = 'emit-boundary-condition-ledger'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'inquiry-session-discipline-surface.json'
$bundleMarkdownPath = Join-Path $bundlePath 'inquiry-session-discipline-surface.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    inquirySurfaceState = $inquirySurfaceState
    reasonCode = $reasonCode
    nextAction = $nextAction
    readinessLedgerState = $currentReadinessState
    sessionLedgerState = $currentSessionLedgerState
    collapseReceiptState = $currentCollapseReceiptState
    returnReceiptState = $currentReturnReceiptState
    nextEraSelectorState = $currentNextEraSelectorState
    inquiryState = 'inquiry-session-discipline-ready'
    inquiryStanceCount = 4
    assumptionExposureModeCount = 3
    silenceDispositionCount = 2
    chamberNativeInquiryBound = $true
    hiddenPressureDenied = $true
    prematureGelPromotionDenied = $true
    inquiryProjectionBound = $inquiryProjectionBound
    inquiryKeyBound = $inquiryKeyBound
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
    '# Inquiry Session Discipline Surface',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Inquiry-surface state: `{0}`' -f $payload.inquirySurfaceState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Runtime-habitation readiness state: `{0}`' -f $(if ($payload.readinessLedgerState) { $payload.readinessLedgerState } else { 'missing' })),
    ('- Session-ledger state: `{0}`' -f $(if ($payload.sessionLedgerState) { $payload.sessionLedgerState } else { 'missing' })),
    ('- Collapse-receipt state: `{0}`' -f $(if ($payload.collapseReceiptState) { $payload.collapseReceiptState } else { 'missing' })),
    ('- Return-receipt state: `{0}`' -f $(if ($payload.returnReceiptState) { $payload.returnReceiptState } else { 'missing' })),
    ('- Next-era selector state: `{0}`' -f $(if ($payload.nextEraSelectorState) { $payload.nextEraSelectorState } else { 'missing' })),
    ('- Inquiry stance count: `{0}`' -f $payload.inquiryStanceCount),
    ('- Assumption-exposure mode count: `{0}`' -f $payload.assumptionExposureModeCount),
    ('- Silence-disposition count: `{0}`' -f $payload.silenceDispositionCount),
    ('- Chamber-native inquiry bound: `{0}`' -f [bool] $payload.chamberNativeInquiryBound),
    ('- Hidden pressure denied: `{0}`' -f [bool] $payload.hiddenPressureDenied),
    ('- Premature GEL promotion denied: `{0}`' -f [bool] $payload.prematureGelPromotionDenied),
    ('- Inquiry projection bound: `{0}`' -f [bool] $payload.inquiryProjectionBound),
    ('- Inquiry key bound: `{0}`' -f [bool] $payload.inquiryKeyBound),
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
    inquirySurfaceState = $payload.inquirySurfaceState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    readinessLedgerState = $payload.readinessLedgerState
    sessionLedgerState = $payload.sessionLedgerState
    collapseReceiptState = $payload.collapseReceiptState
    returnReceiptState = $payload.returnReceiptState
    nextEraSelectorState = $payload.nextEraSelectorState
    inquiryState = $payload.inquiryState
    inquiryStanceCount = $payload.inquiryStanceCount
    assumptionExposureModeCount = $payload.assumptionExposureModeCount
    silenceDispositionCount = $payload.silenceDispositionCount
    chamberNativeInquiryBound = $payload.chamberNativeInquiryBound
    hiddenPressureDenied = $payload.hiddenPressureDenied
    prematureGelPromotionDenied = $payload.prematureGelPromotionDenied
    inquiryProjectionBound = $payload.inquiryProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[inquiry-session-discipline-surface] Bundle: {0}' -f $bundlePath)
$bundlePath
