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

$discernmentAdmissionHelperPath = Join-Path $PSScriptRoot 'Discernment-Admission.ps1'
. $discernmentAdmissionHelperPath

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

if (-not (Get-Command -Name Resolve-OanWorkspaceTouchPointFamily -ErrorAction SilentlyContinue)) {
    $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
    if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
        . $oanWorkspaceResolverPath
    }
}

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the amenable day-dream writer can run.'
}

$sanctuaryRuntimeWorkbenchSurfaceState = Read-JsonFileOrNull -Path $sanctuaryRuntimeWorkbenchSurfaceStatePath
$workbenchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'workbenchState')
$workbenchDiscernment = Get-DiscernmentAdmissionEnvelope -State $sanctuaryRuntimeWorkbenchSurfaceState -DefaultRequestedStanding 'sanctuary-runtime-workbench-ready'

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$tierProjectionBound = $contractsSource.IndexOf('CreateAmenableDayDreamTier', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('amenable-day-dream-tier-admissibility-bound', [System.StringComparison]::Ordinal) -ge 0
$tierKeyBound = $keysSource.IndexOf('CreateAmenableDayDreamTierHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateAmenableDayDreamTier', [System.StringComparison]::Ordinal) -ge 0

$tierState = 'awaiting-runtime-workbench'
$reasonCode = 'amenable-day-dream-tier-awaiting-runtime-workbench'
$nextAction = 'emit-sanctuary-runtime-workbench-surface'
$admissibilityState = 'amenable-exploratory-only'
$requestedStanding = 'amenable-day-dream-tier-ready'
$discernmentAction = 'remain-provisional'
$standingSurfaceClass = 'rhetoric-bearing'
$promotionReceiptState = 'insufficient-for-closure'
$receiptsSufficientForPromotion = $false
$discernmentReason = $reasonCode

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $tierState = 'blocked'
    $reasonCode = 'amenable-day-dream-tier-automation-blocked'
    $nextAction = 'investigate-blocked-state'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $reasonCode
} elseif ($workbenchDiscernment.isRefused) {
    $tierState = 'refused-by-runtime-workbench-discernment'
    $reasonCode = 'amenable-day-dream-tier-workbench-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
    $admissibilityState = 'refused-by-upstream-workbench'
    $discernmentAction = 'refuse'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $workbenchDiscernment.reason
} elseif ($workbenchDiscernment.isHeld) {
    $tierState = 'held-by-runtime-workbench-discernment'
    $reasonCode = 'amenable-day-dream-tier-workbench-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
    $admissibilityState = 'held-by-upstream-workbench'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $workbenchDiscernment.reason
} elseif (-not $workbenchDiscernment.isAdmitted -or $workbenchState -ne 'sanctuary-runtime-workbench-ready') {
    $tierState = 'awaiting-runtime-workbench'
    $reasonCode = if ($workbenchDiscernment.awaitsPromotion) { 'amenable-day-dream-tier-workbench-promotion-not-earned' } else { 'amenable-day-dream-tier-workbench-not-ready' }
    $nextAction = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
    $discernmentReason = $workbenchDiscernment.reason
} elseif ($missingSourceFiles.Count -gt 0 -or -not $tierProjectionBound -or -not $tierKeyBound -or -not $serviceBindingBound) {
    $tierState = 'awaiting-day-dream-binding'
    $reasonCode = 'amenable-day-dream-tier-source-missing'
    $nextAction = 'bind-amenable-day-dream-tier-admissibility'
} else {
    $tierState = 'amenable-day-dream-tier-ready'
    $reasonCode = 'amenable-day-dream-tier-admissibility-bound'
    $nextAction = 'emit-self-rooted-cryptic-depth-gate'
    $discernmentAction = 'admit'
    $standingSurfaceClass = 'closure-bearing'
    $promotionReceiptState = 'sufficient'
    $receiptsSufficientForPromotion = $true
    $discernmentReason = $reasonCode
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'amenable-day-dream-tier-admissibility.json'
$bundleMarkdownPath = Join-Path $bundlePath 'amenable-day-dream-tier-admissibility.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    tierState = $tierState
    reasonCode = $reasonCode
    nextAction = $nextAction
    workbenchState = $workbenchState
    workbenchDiscernmentAction = $workbenchDiscernment.action
    workbenchStandingSurfaceClass = $workbenchDiscernment.standingSurfaceClass
    workbenchPromotionReceiptState = $workbenchDiscernment.promotionReceiptState
    workbenchReceiptsSufficientForPromotion = $workbenchDiscernment.receiptsSufficientForPromotion
    workbenchDiscernmentReason = $workbenchDiscernment.reason
    requestedStanding = $requestedStanding
    discernmentAction = $discernmentAction
    standingSurfaceClass = $standingSurfaceClass
    promotionReceiptState = $promotionReceiptState
    receiptsSufficientForPromotion = $receiptsSufficientForPromotion
    discernmentReason = $discernmentReason
    admissibilityState = $admissibilityState
    exploratoryPredicateCount = 3
    nonFinalOutputCount = 2
    exploratoryOnly = $true
    identityBearingDescentDenied = $true
    continuityInflationDenied = $true
    tierProjectionBound = $tierProjectionBound
    tierKeyBound = $tierKeyBound
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
    '# Amenable Day-Dream Tier Admissibility',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Tier state: `{0}`' -f $payload.tierState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Workbench state: `{0}`' -f $(if ($payload.workbenchState) { $payload.workbenchState } else { 'missing' })),
    ('- Workbench discernment action: `{0}`' -f $payload.workbenchDiscernmentAction),
    ('- Workbench standing surface class: `{0}`' -f $payload.workbenchStandingSurfaceClass),
    ('- Workbench promotion receipt state: `{0}`' -f $payload.workbenchPromotionReceiptState),
    ('- Workbench receipts sufficient for promotion: `{0}`' -f [bool] $payload.workbenchReceiptsSufficientForPromotion),
    ('- Workbench discernment reason: `{0}`' -f $payload.workbenchDiscernmentReason),
    ('- Requested standing: `{0}`' -f $payload.requestedStanding),
    ('- Discernment action: `{0}`' -f $payload.discernmentAction),
    ('- Standing surface class: `{0}`' -f $payload.standingSurfaceClass),
    ('- Promotion receipt state: `{0}`' -f $payload.promotionReceiptState),
    ('- Receipts sufficient for promotion: `{0}`' -f [bool] $payload.receiptsSufficientForPromotion),
    ('- Discernment reason: `{0}`' -f $payload.discernmentReason),
    ('- Admissibility state: `{0}`' -f $payload.admissibilityState),
    ('- Exploratory predicate count: `{0}`' -f $payload.exploratoryPredicateCount),
    ('- Non-final output count: `{0}`' -f $payload.nonFinalOutputCount),
    ('- Exploratory only: `{0}`' -f [bool] $payload.exploratoryOnly),
    ('- Identity-bearing descent denied: `{0}`' -f [bool] $payload.identityBearingDescentDenied),
    ('- Continuity inflation denied: `{0}`' -f [bool] $payload.continuityInflationDenied),
    ('- Tier projection bound: `{0}`' -f [bool] $payload.tierProjectionBound),
    ('- Tier key bound: `{0}`' -f [bool] $payload.tierKeyBound),
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
    tierState = $payload.tierState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    workbenchState = $payload.workbenchState
    workbenchDiscernmentAction = $payload.workbenchDiscernmentAction
    workbenchStandingSurfaceClass = $payload.workbenchStandingSurfaceClass
    workbenchPromotionReceiptState = $payload.workbenchPromotionReceiptState
    workbenchReceiptsSufficientForPromotion = $payload.workbenchReceiptsSufficientForPromotion
    workbenchDiscernmentReason = $payload.workbenchDiscernmentReason
    requestedStanding = $payload.requestedStanding
    discernmentAction = $payload.discernmentAction
    standingSurfaceClass = $payload.standingSurfaceClass
    promotionReceiptState = $payload.promotionReceiptState
    receiptsSufficientForPromotion = $payload.receiptsSufficientForPromotion
    discernmentReason = $payload.discernmentReason
    admissibilityState = $payload.admissibilityState
    exploratoryPredicateCount = $payload.exploratoryPredicateCount
    nonFinalOutputCount = $payload.nonFinalOutputCount
    exploratoryOnly = $payload.exploratoryOnly
    identityBearingDescentDenied = $payload.identityBearingDescentDenied
    continuityInflationDenied = $payload.continuityInflationDenied
    tierProjectionBound = $payload.tierProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[amenable-day-dream-tier-admissibility] Bundle: {0}' -f $bundlePath)
$bundlePath
