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

$automationCascadePromptHelperPath = Join-Path $PSScriptRoot 'Automation-CascadePrompt.ps1'
. $automationCascadePromptHelperPath
$seededGovernanceAdmissionHelperPath = Join-Path $PSScriptRoot 'Seeded-GovernanceAdmission.ps1'
. $seededGovernanceAdmissionHelperPath
$discernmentAdmissionHelperPath = Join-Path $PSScriptRoot 'Discernment-Admission.ps1'
. $discernmentAdmissionHelperPath

function Resolve-PathFromRepo {
    param(
        [string] $BasePath,
        [string] $CandidatePath
    )

    if ([System.IO.Path]::IsPathRooted($CandidatePath)) {
        return [System.IO.Path]::GetFullPath($CandidatePath)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $BasePath $CandidatePath))
}

function Get-RelativePathString {
    param(
        [string] $BasePath,
        [string] $TargetPath
    )

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
    param(
        [string] $Path,
        [object] $Value
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-ObjectPropertyValueOrNull {
    param(
        [object] $InputObject,
        [string] $PropertyName
    )

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
$runtimeDeployabilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeDeployabilityEnvelopeStatePath)
$sanctuaryRuntimeReadinessStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.sanctuaryRuntimeReadinessStatePath)
$runtimeWorkSurfaceAdmissibilityStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkSurfaceAdmissibilityStatePath)
$runtimeWorkbenchSessionLedgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.runtimeWorkbenchSessionLedgerStatePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$masterThreadOrchestrationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.masterThreadOrchestrationStatePath)
$companionToolTelemetryStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.companionToolTelemetryStatePath)
$outputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.v111EnrichmentPathwayOutputRoot)
$statePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.v111EnrichmentPathwayStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the V1.1.1 enrichment pathway can run.'
}

$runtimeDeployabilityState = Read-JsonFileOrNull -Path $runtimeDeployabilityStatePath
$sanctuaryRuntimeReadinessState = Read-JsonFileOrNull -Path $sanctuaryRuntimeReadinessStatePath
$runtimeWorkSurfaceAdmissibilityState = Read-JsonFileOrNull -Path $runtimeWorkSurfaceAdmissibilityStatePath
$runtimeWorkbenchSessionLedgerState = Read-JsonFileOrNull -Path $runtimeWorkbenchSessionLedgerStatePath
$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$masterThreadOrchestrationState = Read-JsonFileOrNull -Path $masterThreadOrchestrationStatePath
$companionToolTelemetryState = Read-JsonFileOrNull -Path $companionToolTelemetryStatePath
$seededGovernanceAdmission = Get-SeededGovernanceBuildAdmission -SeededGovernanceState $seededGovernanceState -CyclePolicy $cyclePolicy

$pathwayState = 'awaiting-runtime-deployability-envelope'
$reasonCode = 'v111-enrichment-pathway-runtime-envelope-missing'
$nextAction = 'emit-runtime-deployability-envelope'
$currentPhase = 'candidate-evidence'
$fullBodyWorkState = 'withheld'
$productionPreReleaseFormState = 'withheld'
$seedLlmPauseState = 'held-until-production-pre-release-form'
$seedLlmPauseRequired = $true
$seedLlmPauseAction = 'pause-and-run-seed-llm-wrinkle-test'
$externalReleaseState = 'held-awaiting-orchestration'
$requestedStanding = 'v111-enrichment-path-open'
$discernmentAction = 'remain-provisional'
$standingSurfaceClass = 'rhetoric-bearing'
$promotionReceiptState = 'insufficient-for-closure'
$receiptsSufficientForPromotion = $false
$discernmentReason = $reasonCode

$candidateStatus = [string] $cycleState.lastKnownStatus
$runtimeEnvelopeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityState -PropertyName 'envelopeState')
$readinessState = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'readinessState')
$workSurfaceState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'admissibilityState')
$workbenchState = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'sessionLedgerState')
$workbenchDiscernment = Get-DiscernmentAdmissionEnvelope -State $runtimeWorkbenchSessionLedgerState -DefaultRequestedStanding 'runtime-workbench-session-ledger-ready'
$seedDisposition = [string] $seededGovernanceAdmission.disposition
$seedReadyState = [string] $seededGovernanceAdmission.readyState
$seededGovernanceBuildAdmissionState = [string] $seededGovernanceAdmission.buildAdmissionState
$seededGovernanceBuildAdmissionReason = [string] $seededGovernanceAdmission.buildAdmissionReason
$seededGovernanceBuildAdmitted = [bool] $seededGovernanceAdmission.buildAdmissionIsAdmitted
$orchestrationState = [string] (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'orchestrationState')
$orchestrationNextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $masterThreadOrchestrationState -PropertyName 'nextAction')
$workbenchReturnPosture = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'returnPosture')
$companionTelemetryState = [string] (Get-ObjectPropertyValueOrNull -InputObject $companionToolTelemetryState -PropertyName 'companionToolTelemetryState')
$holographicDataToolTelemetryState = [string] (Get-ObjectPropertyValueOrNull -InputObject $companionToolTelemetryState -PropertyName 'holographicDataToolTelemetryState')
$triviumForumTelemetryState = [string] (Get-ObjectPropertyValueOrNull -InputObject $companionToolTelemetryState -PropertyName 'triviumForumTelemetryState')
$seededGovernanceClarifyRequired = [bool] $seededGovernanceAdmission.buildAdmissionClarifyRequired

if ($candidateStatus -eq [string] $cyclePolicy.blockedStatus) {
    $pathwayState = 'blocked'
    $reasonCode = 'v111-enrichment-pathway-automation-blocked'
    $nextAction = 'investigate-blocked-state'
    $externalReleaseState = 'withheld'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $reasonCode
} elseif ($null -eq $runtimeDeployabilityState) {
    $pathwayState = 'awaiting-runtime-deployability-envelope'
    $reasonCode = 'v111-enrichment-pathway-runtime-envelope-missing'
    $nextAction = 'emit-runtime-deployability-envelope'
} elseif ($runtimeEnvelopeState -ne 'deployable-candidate-ready') {
    $pathwayState = 'awaiting-runtime-deployability-envelope'
    $reasonCode = 'v111-enrichment-pathway-runtime-envelope-not-ready'
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeDeployabilityState -PropertyName 'nextAction')
} elseif ($null -eq $seededGovernanceState) {
    $pathwayState = 'awaiting-seeded-governance'
    $reasonCode = 'v111-enrichment-pathway-seeded-governance-missing'
    $nextAction = 'emit-seeded-governance-surface'
} elseif ($seededGovernanceClarifyRequired) {
    $pathwayState = 'clarify-seeded-governance-admission'
    $reasonCode = 'v111-enrichment-pathway-build-admission-unresolved'
    $nextAction = 'clarify-seeded-governance-admission-before-continuing'
} elseif (-not $seededGovernanceBuildAdmitted) {
    $pathwayState = 'awaiting-seeded-governance'
    $reasonCode = 'v111-enrichment-pathway-build-admission-withheld'
    $nextAction = 'bring-seeded-governance-to-ready-state'
} elseif ($null -eq $sanctuaryRuntimeReadinessState) {
    $pathwayState = 'awaiting-sanctuary-runtime-readiness'
    $reasonCode = 'v111-enrichment-pathway-sanctuary-readiness-missing'
    $nextAction = 'emit-sanctuary-runtime-readiness'
} elseif ($readinessState -ne 'bounded-working-state-ready') {
    $pathwayState = 'awaiting-sanctuary-runtime-readiness'
    $reasonCode = 'v111-enrichment-pathway-sanctuary-readiness-not-earned'
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $sanctuaryRuntimeReadinessState -PropertyName 'nextAction')
} elseif ($null -eq $runtimeWorkSurfaceAdmissibilityState) {
    $pathwayState = 'awaiting-runtime-work-surface'
    $reasonCode = 'v111-enrichment-pathway-work-surface-missing'
    $nextAction = 'emit-runtime-work-surface-admissibility'
} elseif ($workSurfaceState -notin @('provisional-runtime-work', 'bounded-internal-work-only')) {
    $pathwayState = 'awaiting-runtime-work-surface'
    $reasonCode = 'v111-enrichment-pathway-work-surface-not-admitted'
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkSurfaceAdmissibilityState -PropertyName 'nextAction')
} elseif ($null -eq $runtimeWorkbenchSessionLedgerState) {
    $pathwayState = 'awaiting-runtime-workbench-session-ledger'
    $reasonCode = 'v111-enrichment-pathway-workbench-session-missing'
    $nextAction = 'emit-runtime-workbench-session-ledger'
} elseif ($workbenchDiscernment.isRefused) {
    $pathwayState = 'refused-runtime-workbench-session-ledger'
    $reasonCode = 'v111-enrichment-pathway-workbench-session-refused'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-runtime-workbench-session-ledger' }
    $externalReleaseState = 'withheld'
    $discernmentAction = 'refuse'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $workbenchDiscernment.reason
} elseif ($workbenchDiscernment.isHeld) {
    $pathwayState = 'held-runtime-workbench-session-ledger'
    $reasonCode = 'v111-enrichment-pathway-workbench-session-held'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($workbenchDiscernment.nextAction)) { $workbenchDiscernment.nextAction } else { 'emit-runtime-workbench-session-ledger' }
    $externalReleaseState = 'withheld'
    $discernmentAction = 'hold'
    $standingSurfaceClass = 'refusal-surface'
    $discernmentReason = $workbenchDiscernment.reason
} elseif (-not $workbenchDiscernment.isAdmitted -or $workbenchState -ne 'runtime-workbench-session-ledger-ready') {
    $pathwayState = 'awaiting-runtime-workbench-session-ledger'
    $reasonCode = if ($workbenchDiscernment.awaitsPromotion) { 'v111-enrichment-pathway-workbench-session-promotion-not-earned' } else { 'v111-enrichment-pathway-workbench-session-not-ready' }
    $nextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $runtimeWorkbenchSessionLedgerState -PropertyName 'nextAction')
    $discernmentReason = $workbenchDiscernment.reason
} else {
    $pathwayState = 'v111-enrichment-path-open'
    $currentPhase = 'bounded-full-body-work'
    $fullBodyWorkState = 'open'
    $productionPreReleaseFormState = 'forming'
    $nextAction = 'continue-v111-enrichment-full-body-work'
    $discernmentAction = 'admit'
    $standingSurfaceClass = 'closure-bearing'
    $promotionReceiptState = 'sufficient'
    $receiptsSufficientForPromotion = $true

    if ($orchestrationState -eq 'awaiting-publishable-master-thread') {
        $reasonCode = 'v111-enrichment-pathway-local-work-open-external-release-held'
        $externalReleaseState = 'held-awaiting-publishable-master-thread'
    } else {
        $reasonCode = 'v111-enrichment-pathway-local-work-open'
        $externalReleaseState = if ([string]::IsNullOrWhiteSpace($orchestrationState)) { 'locally-admissible' } else { 'aligned-to-orchestration-state' }
    }
    $discernmentReason = $reasonCode
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $outputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'v111-enrichment-pathway.json'
$bundleMarkdownPath = Join-Path $bundlePath 'v111-enrichment-pathway.md'

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    pathwayState = $pathwayState
    reasonCode = $reasonCode
    nextAction = $nextAction
    currentPhase = $currentPhase
    fullBodyWorkState = $fullBodyWorkState
    productionPreReleaseFormState = $productionPreReleaseFormState
    seedLlmPauseState = $seedLlmPauseState
    seedLlmPauseRequired = $seedLlmPauseRequired
    seedLlmPauseAction = $seedLlmPauseAction
    externalReleaseState = $externalReleaseState
    optionalHopngState = 'optional-bounded'
    candidateStatus = $candidateStatus
    runtimeDeployabilityEnvelopeState = $runtimeEnvelopeState
    sanctuaryRuntimeReadinessState = $readinessState
    runtimeWorkSurfaceAdmissibilityState = $workSurfaceState
    runtimeWorkbenchSessionLedgerState = $workbenchState
    runtimeWorkbenchSessionDiscernmentAction = $workbenchDiscernment.action
    runtimeWorkbenchSessionStandingSurfaceClass = $workbenchDiscernment.standingSurfaceClass
    runtimeWorkbenchSessionPromotionReceiptState = $workbenchDiscernment.promotionReceiptState
    runtimeWorkbenchSessionReceiptsSufficientForPromotion = $workbenchDiscernment.receiptsSufficientForPromotion
    runtimeWorkbenchSessionDiscernmentReason = $workbenchDiscernment.reason
    requestedStanding = $requestedStanding
    discernmentAction = $discernmentAction
    standingSurfaceClass = $standingSurfaceClass
    promotionReceiptState = $promotionReceiptState
    receiptsSufficientForPromotion = $receiptsSufficientForPromotion
    discernmentReason = $discernmentReason
    seededGovernanceDisposition = $seedDisposition
    seededGovernanceReadyState = $seedReadyState
    seededGovernanceBuildAdmissionState = $seededGovernanceBuildAdmissionState
    seededGovernanceBuildAdmissionReason = $seededGovernanceBuildAdmissionReason
    seededGovernanceBuildAdmitted = $seededGovernanceBuildAdmitted
    seededGovernanceClarifyRequired = $seededGovernanceClarifyRequired
    masterThreadOrchestrationState = $orchestrationState
    masterThreadNextAction = $orchestrationNextAction
    workbenchReturnPosture = $workbenchReturnPosture
    companionToolTelemetryState = $companionTelemetryState
    holographicDataToolTelemetryState = $holographicDataToolTelemetryState
    triviumForumTelemetryState = $triviumForumTelemetryState
    phaseVector = [ordered]@{
        contractFirstUnlock = 'admitted'
        boundedFullBodyWork = $fullBodyWorkState
        productionPreReleaseForm = $productionPreReleaseFormState
        seedLlmWrinkleTest = $seedLlmPauseState
    }
    sourceCandidateBundle = [string] $cycleState.lastReleaseCandidateBundle
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $payload | Out-Null

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# V1.1.1 Enrichment Pathway',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Pathway state: `{0}`' -f $payload.pathwayState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Current phase: `{0}`' -f $payload.currentPhase),
    ('- Full-body work state: `{0}`' -f $payload.fullBodyWorkState),
    ('- Production-pre-release form state: `{0}`' -f $payload.productionPreReleaseFormState),
    ('- Seed-LLM pause state: `{0}`' -f $payload.seedLlmPauseState),
    ('- Seed-LLM pause required: `{0}`' -f [bool] $payload.seedLlmPauseRequired),
    ('- Seed-LLM pause action: `{0}`' -f $payload.seedLlmPauseAction),
    ('- External release state: `{0}`' -f $payload.externalReleaseState),
    ('- Optional `.hopng` state: `{0}`' -f $payload.optionalHopngState),
    ('- Candidate status: `{0}`' -f $(if ($payload.candidateStatus) { $payload.candidateStatus } else { 'missing' })),
    ('- Runtime deployability envelope state: `{0}`' -f $(if ($payload.runtimeDeployabilityEnvelopeState) { $payload.runtimeDeployabilityEnvelopeState } else { 'missing' })),
    ('- Sanctuary runtime readiness state: `{0}`' -f $(if ($payload.sanctuaryRuntimeReadinessState) { $payload.sanctuaryRuntimeReadinessState } else { 'missing' })),
    ('- Runtime work-surface admissibility state: `{0}`' -f $(if ($payload.runtimeWorkSurfaceAdmissibilityState) { $payload.runtimeWorkSurfaceAdmissibilityState } else { 'missing' })),
    ('- Runtime workbench session-ledger state: `{0}`' -f $(if ($payload.runtimeWorkbenchSessionLedgerState) { $payload.runtimeWorkbenchSessionLedgerState } else { 'missing' })),
    ('- Runtime workbench session discernment action: `{0}`' -f $payload.runtimeWorkbenchSessionDiscernmentAction),
    ('- Runtime workbench session standing surface class: `{0}`' -f $payload.runtimeWorkbenchSessionStandingSurfaceClass),
    ('- Runtime workbench session promotion receipt state: `{0}`' -f $payload.runtimeWorkbenchSessionPromotionReceiptState),
    ('- Runtime workbench session receipts sufficient for promotion: `{0}`' -f [bool] $payload.runtimeWorkbenchSessionReceiptsSufficientForPromotion),
    ('- Runtime workbench session discernment reason: `{0}`' -f $payload.runtimeWorkbenchSessionDiscernmentReason),
    ('- Requested standing: `{0}`' -f $payload.requestedStanding),
    ('- Discernment action: `{0}`' -f $payload.discernmentAction),
    ('- Standing surface class: `{0}`' -f $payload.standingSurfaceClass),
    ('- Promotion receipt state: `{0}`' -f $payload.promotionReceiptState),
    ('- Receipts sufficient for promotion: `{0}`' -f [bool] $payload.receiptsSufficientForPromotion),
    ('- Discernment reason: `{0}`' -f $payload.discernmentReason),
    ('- Seeded governance disposition: `{0}`' -f $(if ($payload.seededGovernanceDisposition) { $payload.seededGovernanceDisposition } else { 'missing' })),
    ('- Seeded governance ready state: `{0}`' -f $(if ($payload.seededGovernanceReadyState) { $payload.seededGovernanceReadyState } else { 'missing' })),
    ('- Seeded governance build admission state: `{0}`' -f $(if ($payload.seededGovernanceBuildAdmissionState) { $payload.seededGovernanceBuildAdmissionState } else { 'missing' })),
    ('- Seeded governance build admission reason: `{0}`' -f $(if ($payload.seededGovernanceBuildAdmissionReason) { $payload.seededGovernanceBuildAdmissionReason } else { 'missing' })),
    ('- Seeded governance build admitted: `{0}`' -f [bool] $payload.seededGovernanceBuildAdmitted),
    ('- Seeded governance clarify required: `{0}`' -f [bool] $payload.seededGovernanceClarifyRequired),
    ('- Master-thread orchestration state: `{0}`' -f $(if ($payload.masterThreadOrchestrationState) { $payload.masterThreadOrchestrationState } else { 'missing' })),
    ('- Master-thread next action: `{0}`' -f $(if ($payload.masterThreadNextAction) { $payload.masterThreadNextAction } else { 'missing' })),
    ('- Workbench return posture: `{0}`' -f $(if ($payload.workbenchReturnPosture) { $payload.workbenchReturnPosture } else { 'missing' })),
    ('- Companion-tool telemetry state: `{0}`' -f $(if ($payload.companionToolTelemetryState) { $payload.companionToolTelemetryState } else { 'missing' })),
    ('- Holographic Data Tool telemetry state: `{0}`' -f $(if ($payload.holographicDataToolTelemetryState) { $payload.holographicDataToolTelemetryState } else { 'missing' })),
    ('- Trivium Forum telemetry state: `{0}`' -f $(if ($payload.triviumForumTelemetryState) { $payload.triviumForumTelemetryState } else { 'missing' })),
    ('- Source candidate bundle: `{0}`' -f $(if ($payload.sourceCandidateBundle) { $payload.sourceCandidateBundle } else { 'missing' })),
    '',
    '## Phase Vector',
    '',
    ('- Contract-first unlock: `{0}`' -f $payload.phaseVector.contractFirstUnlock),
    ('- Bounded full-body work: `{0}`' -f $payload.phaseVector.boundedFullBodyWork),
    ('- Production-pre-release form: `{0}`' -f $payload.phaseVector.productionPreReleaseForm),
    ('- Seed-LLM wrinkle test: `{0}`' -f $payload.phaseVector.seedLlmWrinkleTest)
)
$markdownLines = Add-AutomationCascadePromptMarkdownLines -MarkdownLines $markdownLines

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    pathwayState = $payload.pathwayState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    currentPhase = $payload.currentPhase
    fullBodyWorkState = $payload.fullBodyWorkState
    productionPreReleaseFormState = $payload.productionPreReleaseFormState
    seedLlmPauseState = $payload.seedLlmPauseState
    seedLlmPauseRequired = $payload.seedLlmPauseRequired
    seedLlmPauseAction = $payload.seedLlmPauseAction
    externalReleaseState = $payload.externalReleaseState
    optionalHopngState = $payload.optionalHopngState
    candidateStatus = $payload.candidateStatus
    runtimeDeployabilityEnvelopeState = $payload.runtimeDeployabilityEnvelopeState
    sanctuaryRuntimeReadinessState = $payload.sanctuaryRuntimeReadinessState
    runtimeWorkSurfaceAdmissibilityState = $payload.runtimeWorkSurfaceAdmissibilityState
    runtimeWorkbenchSessionLedgerState = $payload.runtimeWorkbenchSessionLedgerState
    runtimeWorkbenchSessionDiscernmentAction = $payload.runtimeWorkbenchSessionDiscernmentAction
    runtimeWorkbenchSessionStandingSurfaceClass = $payload.runtimeWorkbenchSessionStandingSurfaceClass
    runtimeWorkbenchSessionPromotionReceiptState = $payload.runtimeWorkbenchSessionPromotionReceiptState
    runtimeWorkbenchSessionReceiptsSufficientForPromotion = $payload.runtimeWorkbenchSessionReceiptsSufficientForPromotion
    runtimeWorkbenchSessionDiscernmentReason = $payload.runtimeWorkbenchSessionDiscernmentReason
    requestedStanding = $payload.requestedStanding
    discernmentAction = $payload.discernmentAction
    standingSurfaceClass = $payload.standingSurfaceClass
    promotionReceiptState = $payload.promotionReceiptState
    receiptsSufficientForPromotion = $payload.receiptsSufficientForPromotion
    discernmentReason = $payload.discernmentReason
    seededGovernanceDisposition = $payload.seededGovernanceDisposition
    seededGovernanceReadyState = $payload.seededGovernanceReadyState
    seededGovernanceBuildAdmissionState = $payload.seededGovernanceBuildAdmissionState
    seededGovernanceBuildAdmissionReason = $payload.seededGovernanceBuildAdmissionReason
    seededGovernanceBuildAdmitted = $payload.seededGovernanceBuildAdmitted
    seededGovernanceClarifyRequired = $payload.seededGovernanceClarifyRequired
    masterThreadOrchestrationState = $payload.masterThreadOrchestrationState
    workbenchReturnPosture = $payload.workbenchReturnPosture
    companionToolTelemetryState = $payload.companionToolTelemetryState
    holographicDataToolTelemetryState = $payload.holographicDataToolTelemetryState
    triviumForumTelemetryState = $payload.triviumForumTelemetryState
}
Add-AutomationCascadeOperatorPromptProperty -InputObject $statePayload | Out-Null

Write-JsonFile -Path $statePath -Value $statePayload
Write-Host ('[v111-enrichment-pathway] Bundle: {0}' -f $bundlePath)
$bundlePath
