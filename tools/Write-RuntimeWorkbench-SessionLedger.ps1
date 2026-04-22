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

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$identityInvariantThreadRootStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.identityInvariantThreadRootStatePath)
$sanctuaryRuntimeWorkbenchSurfaceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeWorkbenchSurfaceStatePath)
$amenableDayDreamTierAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.amenableDayDreamTierAdmissibilityStatePath)
$selfRootedCrypticDepthGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.selfRootedCrypticDepthGateStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the runtime workbench session-ledger writer can run.'
}

$identityInvariantThreadRootState = Read-JsonFileOrNull -Path $identityInvariantThreadRootStatePath
$sanctuaryRuntimeWorkbenchSurfaceState = Read-JsonFileOrNull -Path $sanctuaryRuntimeWorkbenchSurfaceStatePath
$amenableDayDreamTierAdmissibilityState = Read-JsonFileOrNull -Path $amenableDayDreamTierAdmissibilityStatePath
$selfRootedCrypticDepthGateState = Read-JsonFileOrNull -Path $selfRootedCrypticDepthGateStatePath

$threadRootState = [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'threadRootState')
$threadId = [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'threadId')
$governanceRootId = [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'governanceRootId')
$threadWitnessStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $identityInvariantThreadRootState -PropertyName 'witnessStatus')
$workbenchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeWorkbenchSurfaceState -PropertyName 'workbenchState')
$dayDreamTierState = [string] (Get-ObjectPropertyValueOrNull -InputObject $amenableDayDreamTierAdmissibilityState -PropertyName 'tierState')
$depthGateState = [string] (Get-ObjectPropertyValueOrNull -InputObject $selfRootedCrypticDepthGateState -PropertyName 'gateState')
$workbenchDiscernment = Get-DiscernmentAdmissionEnvelope -State $sanctuaryRuntimeWorkbenchSurfaceState -DefaultRequestedStanding 'sanctuary-runtime-workbench-ready'
$dayDreamDiscernment = Get-DiscernmentAdmissionEnvelope -State $amenableDayDreamTierAdmissibilityState -DefaultRequestedStanding 'amenable-day-dream-tier-ready'
$depthGateDiscernment = Get-DiscernmentAdmissionEnvelope -State $selfRootedCrypticDepthGateState -DefaultRequestedStanding 'self-rooted-cryptic-depth-gate-ready'

if (-not (Get-Command -Name Resolve-OanWorkspaceTouchPointFamily -ErrorAction SilentlyContinue)) {
    $oanWorkspaceResolverPath = Join-Path $PSScriptRoot 'Resolve-OanWorkspacePath.ps1'
    if (Test-Path -LiteralPath $oanWorkspaceResolverPath -PathType Leaf) {
        . $oanWorkspaceResolverPath
    }
}

$familyResolution = @(Get-OanWorkspaceTouchPointFamilyResolution -BasePath $resolvedRepoRoot -FamilyName 'sanctuary-workbench-base' -CyclePolicy $cyclePolicy)
$sourceFiles = @($familyResolution | ForEach-Object { [string] $_.SelectedPath })
$missingSourceFiles = @($familyResolution | Where-Object { -not [bool] $_.SelectedPathExists })
$missingBuildTouchPoints = @($missingSourceFiles | Where-Object { [string] $_.TouchPointStatus -ne 'research-handoff' })
$researchHandOffTouchPoints = @($missingSourceFiles | Where-Object { [string] $_.TouchPointStatus -eq 'research-handoff' })
$contractsSource = if ($sourceFiles.Count -gt 0 -and (Test-Path -LiteralPath $sourceFiles[0] -PathType Leaf)) { Get-Content -Raw -LiteralPath $sourceFiles[0] } else { '' }
$keysSource = if ($sourceFiles.Count -gt 1 -and (Test-Path -LiteralPath $sourceFiles[1] -PathType Leaf)) { Get-Content -Raw -LiteralPath $sourceFiles[1] } else { '' }
$serviceSource = if ($sourceFiles.Count -gt 2 -and (Test-Path -LiteralPath $sourceFiles[2] -PathType Leaf)) { Get-Content -Raw -LiteralPath $sourceFiles[2] } else { '' }
$sessionProjectionBound = $contractsSource.IndexOf('CreateRuntimeWorkbenchSessionLedger', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateSessionEvent', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('CreateBoundaryCondition', [System.StringComparison]::Ordinal) -ge 0 -and
    $contractsSource.IndexOf('runtime-workbench-session-ledger-bound', [System.StringComparison]::Ordinal) -ge 0
$sessionKeyBound = $keysSource.IndexOf('CreateRuntimeWorkbenchSessionLedgerHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateWorkbenchSessionEventHandle', [System.StringComparison]::Ordinal) -ge 0 -and
    $keysSource.IndexOf('CreateBoundaryConditionHandle', [System.StringComparison]::Ordinal) -ge 0
$serviceBindingBound = $serviceSource.IndexOf('CreateRuntimeWorkbenchSessionLedger', [System.StringComparison]::Ordinal) -ge 0

$sessionLedgerState = 'awaiting-runtime-workbench-surface'
$reasonCode = 'runtime-workbench-session-ledger-awaiting-runtime-workbench-surface'
$nextAction = 'emit-sanctuary-runtime-workbench-surface'

if ([string] $cycleState.lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $sessionLedgerState = 'blocked'
    $reasonCode = 'runtime-workbench-session-ledger-automation-blocked'
    $nextAction = 'investigate-blocked-state'
} elseif ($workbenchDiscernment.isRefused) {
    $sessionLedgerState = 'refused-by-runtime-workbench-discernment'
    $reasonCode = 'runtime-workbench-session-ledger-workbench-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
} elseif ($workbenchDiscernment.isHeld) {
    $sessionLedgerState = 'held-by-runtime-workbench-discernment'
    $reasonCode = 'runtime-workbench-session-ledger-workbench-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
} elseif (-not $workbenchDiscernment.isAdmitted -or $workbenchState -ne 'sanctuary-runtime-workbench-ready') {
    $sessionLedgerState = 'awaiting-runtime-workbench-surface'
    $reasonCode = if ($workbenchDiscernment.awaitsPromotion) { 'runtime-workbench-session-ledger-workbench-promotion-not-earned' } else { 'runtime-workbench-session-ledger-workbench-not-ready' }
    $nextAction = if ($null -ne $sanctuaryRuntimeWorkbenchSurfaceState) { [string] $sanctuaryRuntimeWorkbenchSurfaceState.nextAction } else { 'emit-sanctuary-runtime-workbench-surface' }
} elseif ($dayDreamDiscernment.isRefused) {
    $sessionLedgerState = 'refused-by-day-dream-discernment'
    $reasonCode = 'runtime-workbench-session-ledger-day-dream-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($dayDreamDiscernment.nextAction)) { $dayDreamDiscernment.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
} elseif ($dayDreamDiscernment.isHeld) {
    $sessionLedgerState = 'held-by-day-dream-discernment'
    $reasonCode = 'runtime-workbench-session-ledger-day-dream-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($dayDreamDiscernment.nextAction)) { $dayDreamDiscernment.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
} elseif (-not $dayDreamDiscernment.isAdmitted -or $dayDreamTierState -ne 'amenable-day-dream-tier-ready') {
    $sessionLedgerState = 'awaiting-day-dream-tier'
    $reasonCode = if ($dayDreamDiscernment.awaitsPromotion) { 'runtime-workbench-session-ledger-day-dream-promotion-not-earned' } else { 'runtime-workbench-session-ledger-day-dream-not-ready' }
    $nextAction = if ($null -ne $amenableDayDreamTierAdmissibilityState) { [string] $amenableDayDreamTierAdmissibilityState.nextAction } else { 'emit-amenable-day-dream-tier-admissibility' }
} elseif ($depthGateDiscernment.isRefused) {
    $sessionLedgerState = 'refused-by-depth-gate-discernment'
    $reasonCode = 'runtime-workbench-session-ledger-depth-gate-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($depthGateDiscernment.nextAction)) { $depthGateDiscernment.nextAction } else { 'emit-self-rooted-cryptic-depth-gate' }
} elseif ($depthGateDiscernment.isHeld) {
    $sessionLedgerState = 'held-by-depth-gate-discernment'
    $reasonCode = 'runtime-workbench-session-ledger-depth-gate-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($depthGateDiscernment.nextAction)) { $depthGateDiscernment.nextAction } else { 'emit-self-rooted-cryptic-depth-gate' }
} elseif (-not $depthGateDiscernment.isAdmitted -or $depthGateState -ne 'self-rooted-cryptic-depth-gate-ready') {
    $sessionLedgerState = 'awaiting-depth-gate'
    $reasonCode = if ($depthGateDiscernment.awaitsPromotion) { 'runtime-workbench-session-ledger-depth-gate-promotion-not-earned' } else { 'runtime-workbench-session-ledger-depth-gate-not-ready' }
    $nextAction = if ($null -ne $selfRootedCrypticDepthGateState) { [string] $selfRootedCrypticDepthGateState.nextAction } else { 'emit-self-rooted-cryptic-depth-gate' }
} elseif ($missingBuildTouchPoints.Count -gt 0 -or -not $sessionProjectionBound -or -not $sessionKeyBound -or -not $serviceBindingBound) {
    $sessionLedgerState = 'awaiting-session-ledger-binding'
    $reasonCode = 'runtime-workbench-session-ledger-source-missing'
    $nextAction = 'bind-runtime-workbench-session-ledger'
} elseif ($researchHandOffTouchPoints.Count -gt 0) {
    $sessionLedgerState = 'awaiting-session-ledger-research-return'
    $reasonCode = 'runtime-workbench-session-ledger-research-handoff-pending'
    $nextAction = 'review-source-bucket-return-for-runtime-workbench-session-ledger'
} else {
    $sessionLedgerState = 'runtime-workbench-session-ledger-ready'
    $reasonCode = 'runtime-workbench-session-ledger-bound'
    $nextAction = 'emit-day-dream-collapse-receipt'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'runtime-workbench-session-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'runtime-workbench-session-ledger.md'

$projectSpaceId = 'oan-mortalis-v1.1.1'
$sessionId = 'runtime-workbench-session://v111-bounded-session'
if ([string]::IsNullOrWhiteSpace($threadId)) {
    $threadId = 'worker-thread-root://local-main-worker'
}
if ([string]::IsNullOrWhiteSpace($governanceRootId)) {
    $governanceRootId = 'identity-invariant://bounded-local-governance-root'
}

$currentStateClass = 'provisional'
$admissibilityStatus = 'provisional'
$predicateLandingClass = 'candidate-governed-structure'
$witnessStatus = 'awaiting-session-ledger-witness'
$requestedStanding = 'runtime-workbench-session-ledger-ready'
$discernmentAction = 'remain-provisional'
$standingSurfaceClass = 'rhetoric-bearing'
$promotionReceiptState = 'insufficient-for-closure'
$receiptsSufficientForPromotion = $false
$categoryErrorDetected = $false
$promotionWithoutReceiptsDetected = $false
$discernmentDefinedTerms = 'pass'
$discernmentContextualScope = 'pass'
$discernmentEvidenceSufficiency = 'fail'
$discernmentNonContradiction = 'pass'
$discernmentSurfaceAppropriateness = 'pass'
$discernmentPromotionWarrant = 'fail'

if ($sessionLedgerState -eq 'blocked') {
    $currentStateClass = 'hold'
    $admissibilityStatus = 'hold'
    $predicateLandingClass = 'hold'
    $witnessStatus = 'session-ledger-hold-witnessed'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
} elseif ($sessionLedgerState -like 'held-by-*') {
    $currentStateClass = 'hold'
    $admissibilityStatus = 'hold'
    $predicateLandingClass = 'hold'
    $witnessStatus = 'session-ledger-hold-witnessed'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
} elseif ($sessionLedgerState -like 'refused-by-*') {
    $currentStateClass = 'hold'
    $admissibilityStatus = 'refuse'
    $predicateLandingClass = 'refused'
    $witnessStatus = 'session-ledger-hold-witnessed'
    $discernmentAction = 'refuse'
    $standingSurfaceClass = 'refusal-surface'
} elseif ($sessionLedgerState -eq 'runtime-workbench-session-ledger-ready') {
    $currentStateClass = 'ready'
    $admissibilityStatus = 'admitted'
    $predicateLandingClass = 'admitted'
    $witnessStatus = 'session-ledger-witnessed'
    $discernmentAction = 'admit'
    $standingSurfaceClass = 'closure-bearing'
    $promotionReceiptState = 'sufficient'
    $receiptsSufficientForPromotion = $true
    $discernmentEvidenceSufficiency = 'pass'
    $discernmentPromotionWarrant = 'pass'
}

$lastLawfulTransition = switch ($sessionLedgerState) {
    'runtime-workbench-session-ledger-ready' { 'runtime-workbench-session-ledger-bound' }
    'blocked' { 'automation-blocked' }
    default { 'bounded-session-open' }
}

$continuityAnchor = if ($threadRootState -eq 'identity-thread-root-ready') {
    $governanceRootId
} else {
    'identity-invariant-thread-root-pending'
}

$discernmentReason = $reasonCode

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    sessionLedgerState = $sessionLedgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    researchHandOffPending = $researchHandOffTouchPoints.Count -gt 0
    researchHandOffTouchPointIds = @($researchHandOffTouchPoints | ForEach-Object { [string] $_.TouchPointId })
    sessionId = $sessionId
    threadId = $threadId
    projectSpaceId = $projectSpaceId
    governanceRootId = $governanceRootId
    sessionScope = 'bounded-governance-analysis-workbench'
    sessionOpenTimestampUtc = (Get-Date).ToUniversalTime().ToString('o')
    currentStateClass = $currentStateClass
    witnessStatus = $witnessStatus
    threadWitnessStatus = $threadWitnessStatus
    workbenchDiscernmentAction = $workbenchDiscernment.action
    workbenchStandingSurfaceClass = $workbenchDiscernment.standingSurfaceClass
    workbenchPromotionReceiptState = $workbenchDiscernment.promotionReceiptState
    workbenchDiscernmentReason = $workbenchDiscernment.reason
    dayDreamDiscernmentAction = $dayDreamDiscernment.action
    dayDreamStandingSurfaceClass = $dayDreamDiscernment.standingSurfaceClass
    dayDreamPromotionReceiptState = $dayDreamDiscernment.promotionReceiptState
    dayDreamDiscernmentReason = $dayDreamDiscernment.reason
    depthGateDiscernmentAction = $depthGateDiscernment.action
    depthGateStandingSurfaceClass = $depthGateDiscernment.standingSurfaceClass
    depthGatePromotionReceiptState = $depthGateDiscernment.promotionReceiptState
    depthGateDiscernmentReason = $depthGateDiscernment.reason
    requestedStanding = $requestedStanding
    discernmentAction = $discernmentAction
    standingSurfaceClass = $standingSurfaceClass
    promotionReceiptState = $promotionReceiptState
    receiptsSufficientForPromotion = $receiptsSufficientForPromotion
    categoryErrorDetected = $categoryErrorDetected
    promotionWithoutReceiptsDetected = $promotionWithoutReceiptsDetected
    discernmentReason = $discernmentReason
    discernmentEvaluation = [ordered]@{
        definedTerms = $discernmentDefinedTerms
        contextualScope = $discernmentContextualScope
        evidenceSufficiency = $discernmentEvidenceSufficiency
        nonContradiction = $discernmentNonContradiction
        surfaceAppropriateness = $discernmentSurfaceAppropriateness
        promotionWarrant = $discernmentPromotionWarrant
    }
    authorizationBasis = 'admitted-local-bounded seeded governance and bounded runtime readiness'
    continuityAnchor = $continuityAnchor
    admissibilityStatus = $admissibilityStatus
    stateReason = $reasonCode
    lastLawfulTransition = $lastLawfulTransition
    nextAllowedTransition = $nextAction
    predicateLandingClass = $predicateLandingClass
    autobiographicalPromotionState = 'operational-only'
    engramPredicateEligibilityState = 'not-eligible'
    carryForwardPolicy = 'receipted-bounded-workbench-only'
    workbenchState = $workbenchState
    dayDreamTierState = $dayDreamTierState
    depthGateState = $depthGateState
    sessionState = 'bounded-session-open'
    sessionPosture = 'bounded-session-open'
    returnPosture = 'return-through-bounded-workbench'
    admittedLaneCount = 3
    withheldLaneCount = 3
    sessionEventCount = 3
    boundaryConditionCount = 1
    sessionProjectionBound = $sessionProjectionBound
    sessionKeyBound = $sessionKeyBound
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
    '# Runtime Workbench Session Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Session-ledger state: `{0}`' -f $payload.sessionLedgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Research handoff pending: `{0}`' -f [bool] $payload.researchHandOffPending),
    ('- Research handoff touchpoints: `{0}`' -f $(if (@($payload.researchHandOffTouchPointIds).Count -gt 0) { (@($payload.researchHandOffTouchPointIds) -join '`, `') } else { 'none' })),
    ('- Session id: `{0}`' -f $payload.sessionId),
    ('- Thread id: `{0}`' -f $payload.threadId),
    ('- Project-space id: `{0}`' -f $payload.projectSpaceId),
    ('- Governance-root id: `{0}`' -f $payload.governanceRootId),
    ('- Session scope: `{0}`' -f $payload.sessionScope),
    ('- Session open timestamp (UTC): `{0}`' -f $payload.sessionOpenTimestampUtc),
    ('- Current state class: `{0}`' -f $payload.currentStateClass),
    ('- Witness status: `{0}`' -f $payload.witnessStatus),
    ('- Thread witness status: `{0}`' -f $(if ($payload.threadWitnessStatus) { $payload.threadWitnessStatus } else { 'missing' })),
    ('- Workbench discernment action: `{0}`' -f $payload.workbenchDiscernmentAction),
    ('- Workbench standing surface class: `{0}`' -f $payload.workbenchStandingSurfaceClass),
    ('- Workbench promotion receipt state: `{0}`' -f $payload.workbenchPromotionReceiptState),
    ('- Workbench discernment reason: `{0}`' -f $payload.workbenchDiscernmentReason),
    ('- Day-dream discernment action: `{0}`' -f $payload.dayDreamDiscernmentAction),
    ('- Day-dream standing surface class: `{0}`' -f $payload.dayDreamStandingSurfaceClass),
    ('- Day-dream promotion receipt state: `{0}`' -f $payload.dayDreamPromotionReceiptState),
    ('- Day-dream discernment reason: `{0}`' -f $payload.dayDreamDiscernmentReason),
    ('- Depth-gate discernment action: `{0}`' -f $payload.depthGateDiscernmentAction),
    ('- Depth-gate standing surface class: `{0}`' -f $payload.depthGateStandingSurfaceClass),
    ('- Depth-gate promotion receipt state: `{0}`' -f $payload.depthGatePromotionReceiptState),
    ('- Depth-gate discernment reason: `{0}`' -f $payload.depthGateDiscernmentReason),
    ('- Requested standing: `{0}`' -f $payload.requestedStanding),
    ('- Discernment action: `{0}`' -f $payload.discernmentAction),
    ('- Standing surface class: `{0}`' -f $payload.standingSurfaceClass),
    ('- Promotion receipt state: `{0}`' -f $payload.promotionReceiptState),
    ('- Receipts sufficient for promotion: `{0}`' -f [bool] $payload.receiptsSufficientForPromotion),
    ('- Category error detected: `{0}`' -f [bool] $payload.categoryErrorDetected),
    ('- Promotion without receipts detected: `{0}`' -f [bool] $payload.promotionWithoutReceiptsDetected),
    ('- Discernment reason: `{0}`' -f $payload.discernmentReason),
    ('- Discernment evaluation: `definedTerms={0}; contextualScope={1}; evidenceSufficiency={2}; nonContradiction={3}; surfaceAppropriateness={4}; promotionWarrant={5}`' -f
        $payload.discernmentEvaluation.definedTerms,
        $payload.discernmentEvaluation.contextualScope,
        $payload.discernmentEvaluation.evidenceSufficiency,
        $payload.discernmentEvaluation.nonContradiction,
        $payload.discernmentEvaluation.surfaceAppropriateness,
        $payload.discernmentEvaluation.promotionWarrant),
    ('- Authorization basis: `{0}`' -f $payload.authorizationBasis),
    ('- Continuity anchor: `{0}`' -f $payload.continuityAnchor),
    ('- Admissibility status: `{0}`' -f $payload.admissibilityStatus),
    ('- State reason: `{0}`' -f $payload.stateReason),
    ('- Last lawful transition: `{0}`' -f $payload.lastLawfulTransition),
    ('- Next allowed transition: `{0}`' -f $payload.nextAllowedTransition),
    ('- Predicate landing class: `{0}`' -f $payload.predicateLandingClass),
    ('- Autobiographical promotion state: `{0}`' -f $payload.autobiographicalPromotionState),
    ('- Engram predicate eligibility state: `{0}`' -f $payload.engramPredicateEligibilityState),
    ('- Carry-forward policy: `{0}`' -f $payload.carryForwardPolicy),
    ('- Workbench state: `{0}`' -f $(if ($payload.workbenchState) { $payload.workbenchState } else { 'missing' })),
    ('- Day-dream tier state: `{0}`' -f $(if ($payload.dayDreamTierState) { $payload.dayDreamTierState } else { 'missing' })),
    ('- Depth-gate state: `{0}`' -f $(if ($payload.depthGateState) { $payload.depthGateState } else { 'missing' })),
    ('- Session state: `{0}`' -f $payload.sessionState),
    ('- Session posture: `{0}`' -f $payload.sessionPosture),
    ('- Return posture: `{0}`' -f $payload.returnPosture),
    ('- Admitted lane count: `{0}`' -f $payload.admittedLaneCount),
    ('- Withheld lane count: `{0}`' -f $payload.withheldLaneCount),
    ('- Session event count: `{0}`' -f $payload.sessionEventCount),
    ('- Boundary-condition count: `{0}`' -f $payload.boundaryConditionCount),
    ('- Session projection bound: `{0}`' -f [bool] $payload.sessionProjectionBound),
    ('- Session key bound: `{0}`' -f [bool] $payload.sessionKeyBound),
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
    sessionLedgerState = $payload.sessionLedgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    researchHandOffPending = $payload.researchHandOffPending
    researchHandOffTouchPointIds = $payload.researchHandOffTouchPointIds
    sessionId = $payload.sessionId
    threadId = $payload.threadId
    projectSpaceId = $payload.projectSpaceId
    governanceRootId = $payload.governanceRootId
    sessionScope = $payload.sessionScope
    sessionOpenTimestampUtc = $payload.sessionOpenTimestampUtc
    currentStateClass = $payload.currentStateClass
    witnessStatus = $payload.witnessStatus
    threadWitnessStatus = $payload.threadWitnessStatus
    workbenchDiscernmentAction = $payload.workbenchDiscernmentAction
    workbenchStandingSurfaceClass = $payload.workbenchStandingSurfaceClass
    workbenchPromotionReceiptState = $payload.workbenchPromotionReceiptState
    workbenchDiscernmentReason = $payload.workbenchDiscernmentReason
    dayDreamDiscernmentAction = $payload.dayDreamDiscernmentAction
    dayDreamStandingSurfaceClass = $payload.dayDreamStandingSurfaceClass
    dayDreamPromotionReceiptState = $payload.dayDreamPromotionReceiptState
    dayDreamDiscernmentReason = $payload.dayDreamDiscernmentReason
    depthGateDiscernmentAction = $payload.depthGateDiscernmentAction
    depthGateStandingSurfaceClass = $payload.depthGateStandingSurfaceClass
    depthGatePromotionReceiptState = $payload.depthGatePromotionReceiptState
    depthGateDiscernmentReason = $payload.depthGateDiscernmentReason
    requestedStanding = $payload.requestedStanding
    discernmentAction = $payload.discernmentAction
    standingSurfaceClass = $payload.standingSurfaceClass
    promotionReceiptState = $payload.promotionReceiptState
    receiptsSufficientForPromotion = $payload.receiptsSufficientForPromotion
    categoryErrorDetected = $payload.categoryErrorDetected
    promotionWithoutReceiptsDetected = $payload.promotionWithoutReceiptsDetected
    discernmentReason = $payload.discernmentReason
    discernmentEvaluation = $payload.discernmentEvaluation
    authorizationBasis = $payload.authorizationBasis
    continuityAnchor = $payload.continuityAnchor
    admissibilityStatus = $payload.admissibilityStatus
    stateReason = $payload.stateReason
    lastLawfulTransition = $payload.lastLawfulTransition
    nextAllowedTransition = $payload.nextAllowedTransition
    predicateLandingClass = $payload.predicateLandingClass
    autobiographicalPromotionState = $payload.autobiographicalPromotionState
    engramPredicateEligibilityState = $payload.engramPredicateEligibilityState
    carryForwardPolicy = $payload.carryForwardPolicy
    workbenchState = $payload.workbenchState
    dayDreamTierState = $payload.dayDreamTierState
    depthGateState = $payload.depthGateState
    sessionState = $payload.sessionState
    sessionPosture = $payload.sessionPosture
    returnPosture = $payload.returnPosture
    admittedLaneCount = $payload.admittedLaneCount
    withheldLaneCount = $payload.withheldLaneCount
    sessionEventCount = $payload.sessionEventCount
    boundaryConditionCount = $payload.boundaryConditionCount
    sessionProjectionBound = $payload.sessionProjectionBound
    sessionKeyBound = $payload.sessionKeyBound
    serviceBindingBound = $payload.serviceBindingBound
    sourceFileCount = $payload.sourceFileCount
    missingSourceFileCount = $payload.missingSourceFileCount
}

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[runtime-workbench-session-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
