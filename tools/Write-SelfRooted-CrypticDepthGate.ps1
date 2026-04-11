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
$governedThreadBirthReceiptStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.governedThreadBirthReceiptStatePath)
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$amenableDayDreamTierAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the self-rooted cryptic-depth gate writer can run.'
}

$governedThreadBirthReceiptState = Read-JsonFileOrNull -Path $governedThreadBirthReceiptStatePath
$sanctuaryRuntimeWorkbenchSurfaceState = Read-JsonFileOrNull -Path $sanctuaryRuntimeWorkbenchSurfaceStatePath
$amenableDayDreamTierAdmissibilityState = Read-JsonFileOrNull -Path $amenableDayDreamTierAdmissibilityStatePath

$threadBirthState = [string] (Get-ObjectPropertyValueOrNull -InputObject $governedThreadBirthReceiptState -PropertyName 'receiptState')
$workbenchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'workbenchState')
$dayDreamTierState = [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'tierState')
$workbenchDiscernment = Get-DiscernmentAdmissionEnvelope -State $sanctuaryRuntimeWorkbenchSurfaceState -DefaultRequestedStanding 'sanctuary-runtime-workbench-ready'
$dayDreamDiscernment = Get-DiscernmentAdmissionEnvelope -State $amenableDayDreamTierAdmissibilityState -DefaultRequestedStanding 'amenable-day-dream-tier-ready'

$sourceFiles = Resolve-OanWorkspaceTouchPointFamily -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy
$missingSourceFiles = @($sourceFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
$contractsSource = if (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$gateProjectionBound = $contractsSource.IndexOf('CreateSelfRootedCrypticDepthGate', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('self-rooted-cryptic-depth-gate-bound', [System.StringComparison]::Ordinal) -ge 0
$biadRootKeyBound = $keysSource.IndexOf('CreateCrypticBiadRootHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateSelfRootedCrypticDepthGateHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateSelfRootedCrypticDepthGate', [System.StringComparison]::Ordinal) -ge 0

$gateState = 'awaiting-governed-thread-birth'
$reasonCode = 'self-rooted-cryptic-depth-gate-awaiting-governed-thread-birth'
$nextAction = 'emit-governed-thread-birth-receipt'
$requestedStanding = 'self-rooted-cryptic-depth-gate-ready'
$discernmentAction = 'remain-provisional'
$standingSurfaceClass = 'rhetoric-bearing'
$promotionReceiptState = 'insufficient-for-closure'
$receiptsSufficientForPromotion = $false
$discernmentReason = $reasonCode

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $gateState = 'blocked'
    $reasonCode = 'self-rooted-cryptic-depth-gate-automation-blocked'
    $nextAction = 'investigate-blocked-state'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $reasonCode
} elseif ($threadBirthState -ne 'thread-birth-ready') {
    $gateState = 'awaiting-governed-thread-birth'
    $reasonCode = 'self-rooted-cryptic-depth-gate-thread-birth-not-ready'
    $nextAction = if ($null -ne $governedThreadBirthReceiptState) { [string] $governedThreadBirthReceiptState.nextAction } else { 'emit-governed-thread-birth-receipt' }
} elseif ($workbenchDiscernment.isRefused) {
    $gateState = 'refused-by-runtime-workbench-discernment'
    $reasonCode = 'self-rooted-cryptic-depth-gate-workbench-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
    $discernmentAction = 'refuse'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $workbenchDiscernment.reason
} elseif ($workbenchDiscernment.isHeld) {
    $gateState = 'held-by-runtime-workbench-discernment'
    $reasonCode = 'self-rooted-cryptic-depth-gate-workbench-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $workbenchDiscernment.reason
} elseif (-not $workbenchDiscernment.isAdmitted -or $workbenchState -ne 'sanctuary-runtime-workbench-ready') {
    $gateState = 'awaiting-runtime-workbench'
    $reasonCode = if ($workbenchDiscernment.awaitsPromotion) { 'self-rooted-cryptic-depth-gate-workbench-promotion-not-earned' } else { 'self-rooted-cryptic-depth-gate-workbench-not-ready' }
    $nextAction = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
    $discernmentReason = $workbenchDiscernment.reason
} elseif ($dayDreamDiscernment.isRefused) {
    $gateState = 'refused-by-day-dream-discernment'
    $reasonCode = 'self-rooted-cryptic-depth-gate-day-dream-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($dayDreamDiscernment.nextAction)) { $dayDreamDiscernment.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
    $discernmentAction = 'refuse'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $dayDreamDiscernment.reason
} elseif ($dayDreamDiscernment.isHeld) {
    $gateState = 'held-by-day-dream-discernment'
    $reasonCode = 'self-rooted-cryptic-depth-gate-day-dream-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($dayDreamDiscernment.nextAction)) { $dayDreamDiscernment.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $dayDreamDiscernment.reason
} elseif (-not $dayDreamDiscernment.isAdmitted -or $dayDreamTierState -ne 'amenable-day-dream-tier-ready') {
    $gateState = 'awaiting-day-dream-tier'
    $reasonCode = if ($dayDreamDiscernment.awaitsPromotion) { 'self-rooted-cryptic-depth-gate-day-dream-promotion-not-earned' } else { 'self-rooted-cryptic-depth-gate-day-dream-not-ready' }
    $nextAction = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
    $discernmentReason = $dayDreamDiscernment.reason
} elseif ($missingSourceFiles.Count -gt 0 -or -not $gateProjectionBound -or -not $biadRootKeyBound -or -not $serviceBindingBound) {
    $gateState = 'awaiting-self-rooted-depth-binding'
    $reasonCode = 'self-rooted-cryptic-depth-gate-source-missing'
    $nextAction = 'bind-self-rooted-cryptic-depth-gate'
} else {
    $gateState = 'self-rooted-cryptic-depth-gate-ready'
    $reasonCode = 'self-rooted-cryptic-depth-gate-bound'
    $nextAction = 'pull-forward-to-map-22'
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
$bundleJsonPath = Join-Path $bundlePath 'self-rooted-cryptic-depth-gate.json'
$bundleMarkdownPath = Join-Path $bundlePath 'self-rooted-cryptic-depth-gate.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    gateState = $gateState
    reasonCode = $reasonCode
    nextAction = $nextAction
    threadBirthState = $threadBirthState
    workbenchState = $workbenchState
    workbenchDiscernmentAction = $workbenchDiscernment.action
    workbenchStandingSurfaceClass = $workbenchDiscernment.standingSurfaceClass
    workbenchPromotionReceiptState = $workbenchDiscernment.promotionReceiptState
    workbenchDiscernmentReason = $workbenchDiscernment.reason
    dayDreamTierState = $dayDreamTierState
    dayDreamDiscernmentAction = $dayDreamDiscernment.action
    dayDreamStandingSurfaceClass = $dayDreamDiscernment.standingSurfaceClass
    dayDreamPromotionReceiptState = $dayDreamDiscernment.promotionReceiptState
    dayDreamDiscernmentReason = $dayDreamDiscernment.reason
    requestedStanding = $requestedStanding
    discernmentAction = $discernmentAction
    standingSurfaceClass = $standingSurfaceClass
    promotionReceiptState = $promotionReceiptState
    receiptsSufficientForPromotion = $receiptsSufficientForPromotion
    discernmentReason = $discernmentReason
    gateMode = 'provisionally-rooted-withheld'
    crypticBiadRooted = $true
    sharedAmenableOriginDenied = $true
    deepAccessGranted = $false
    gateProjectionBound = $gateProjectionBound
    biadRootKeyBound = $biadRootKeyBound
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
    '# Self-Rooted Cryptic Depth Gate',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Gate state: `{0}`' -f $payload.gateState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Thread-birth state: `{0}`' -f $(if ($payload.threadBirthState) { $payload.threadBirthState } else { 'missing' })),
    ('- Workbench state: `{0}`' -f $(if ($payload.workbenchState) { $payload.workbenchState } else { 'missing' })),
    ('- Workbench discernment action: `{0}`' -f $payload.workbenchDiscernmentAction),
    ('- Workbench standing surface class: `{0}`' -f $payload.workbenchStandingSurfaceClass),
    ('- Workbench promotion receipt state: `{0}`' -f $payload.workbenchPromotionReceiptState),
    ('- Workbench discernment reason: `{0}`' -f $payload.workbenchDiscernmentReason),
    ('- Day-dream tier state: `{0}`' -f $(if ($payload.dayDreamTierState) { $payload.dayDreamTierState } else { 'missing' })),
    ('- Day-dream discernment action: `{0}`' -f $payload.dayDreamDiscernmentAction),
    ('- Day-dream standing surface class: `{0}`' -f $payload.dayDreamStandingSurfaceClass),
    ('- Day-dream promotion receipt state: `{0}`' -f $payload.dayDreamPromotionReceiptState),
    ('- Day-dream discernment reason: `{0}`' -f $payload.dayDreamDiscernmentReason),
    ('- Requested standing: `{0}`' -f $payload.requestedStanding),
    ('- Discernment action: `{0}`' -f $payload.discernmentAction),
    ('- Standing surface class: `{0}`' -f $payload.standingSurfaceClass),
    ('- Promotion receipt state: `{0}`' -f $payload.promotionReceiptState),
    ('- Receipts sufficient for promotion: `{0}`' -f [bool] $payload.receiptsSufficientForPromotion),
    ('- Discernment reason: `{0}`' -f $payload.discernmentReason),
    ('- Gate mode: `{0}`' -f $payload.gateMode),
    ('- Cryptic biad rooted: `{0}`' -f [bool] $payload.crypticBiadRooted),
    ('- Shared amenable origin denied: `{0}`' -f [bool] $payload.sharedAmenableOriginDenied),
    ('- Deep access granted: `{0}`' -f [bool] $payload.deepAccessGranted),
    ('- Gate projection bound: `{0}`' -f [bool] $payload.gateProjectionBound),
    ('- Biad-root key bound: `{0}`' -f [bool] $payload.biadRootKeyBound),
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
    gateState = $payload.gateState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    threadBirthState = $payload.threadBirthState
    workbenchState = $payload.workbenchState
    workbenchDiscernmentAction = $payload.workbenchDiscernmentAction
    workbenchStandingSurfaceClass = $payload.workbenchStandingSurfaceClass
    workbenchPromotionReceiptState = $payload.workbenchPromotionReceiptState
    workbenchDiscernmentReason = $payload.workbenchDiscernmentReason
    dayDreamTierState = $payload.dayDreamTierState
    dayDreamDiscernmentAction = $payload.dayDreamDiscernmentAction
    dayDreamStandingSurfaceClass = $payload.dayDreamStandingSurfaceClass
    dayDreamPromotionReceiptState = $payload.dayDreamPromotionReceiptState
    dayDreamDiscernmentReason = $payload.dayDreamDiscernmentReason
    requestedStanding = $payload.requestedStanding
    discernmentAction = $payload.discernmentAction
    standingSurfaceClass = $payload.standingSurfaceClass
    promotionReceiptState = $payload.promotionReceiptState
    receiptsSufficientForPromotion = $payload.receiptsSufficientForPromotion
    discernmentReason = $payload.discernmentReason
    gateMode = $payload.gateMode
    crypticBiadRooted = $payload.crypticBiadRooted
    sharedAmenableOriginDenied = $payload.sharedAmenableOriginDenied
    deepAccessGranted = $payload.deepAccessGranted
    gateProjectionBound = $payload.gateProjectionBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[self-rooted-cryptic-depth-gate] Bundle: {0}' -f $bundlePath)
$bundlePath
