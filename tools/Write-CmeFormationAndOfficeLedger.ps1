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

    $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
}

function New-UniqueStringList {
    param([object[]] $Values)

    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $items = New-Object System.Collections.Generic.List[string]

    foreach ($value in $Values) {
        if ($null -eq $value) {
            continue
        }

        $stringValue = [string] $value
        if ([string]::IsNullOrWhiteSpace($stringValue)) {
            continue
        }

        if ($seen.Add($stringValue)) {
            [void] $items.Add($stringValue)
        }
    }

    return @($items)
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

function Resolve-CapabilityStateFromDisposition {
    param([string] $Disposition)

    switch ($Disposition) {
        'Accepted' { return 'Admissible' }
        'Deferred' { return 'Deferred' }
        'Rejected' { return 'Withheld' }
        default { return 'Dormant' }
    }
}

function Resolve-OfficeStateFromCapability {
    param(
        [string] $CapabilityState,
        [string] $HandshakeState
    )

    switch ($CapabilityState) {
        'Admissible' {
            if ($HandshakeState -eq 'awaiting-ratification') {
                return 'Provisional'
            }

            return 'Open'
        }
        'Deferred' { return 'Deferred' }
        'Withheld' { return 'Withheld' }
        default { return 'Dormant' }
    }
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedCyclePolicyPath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath $CyclePolicyPath
$cyclePolicy = Get-Content -Raw -LiteralPath $resolvedCyclePolicyPath | ConvertFrom-Json

$cycleStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.statePath)
$seededGovernanceStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededGovernanceStatePath)
$cmeConsolidationStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeConsolidationStatePath)
$promotionGateStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.promotionGateStatePath)
$seededPromotionReviewStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.seededPromotionReviewStatePath)
$firstPublishIntentStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.firstPublishIntentStatePath)
$releaseHandshakeStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.releaseHandshakeStatePath)
$ledgerOutputRoot = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeFormationAndOfficeLedgerOutputRoot)
$ledgerStatePath = Resolve-PathFromRepo -BasePath $resolvedRepoRoot -CandidatePath ([string] $cyclePolicy.cmeFormationAndOfficeLedgerStatePath)

$cycleState = Read-JsonFileOrNull -Path $cycleStatePath
if ($null -eq $cycleState) {
    throw 'Local automation cycle state is required before the CME formation and office ledger can run.'
}

$seededGovernanceState = Read-JsonFileOrNull -Path $seededGovernanceStatePath
$cmeConsolidationState = Read-JsonFileOrNull -Path $cmeConsolidationStatePath
$promotionGateState = Read-JsonFileOrNull -Path $promotionGateStatePath
$seededPromotionReviewState = Read-JsonFileOrNull -Path $seededPromotionReviewStatePath
$firstPublishIntentState = Read-JsonFileOrNull -Path $firstPublishIntentStatePath
$releaseHandshakeState = Read-JsonFileOrNull -Path $releaseHandshakeStatePath

$lastKnownStatus = [string] (Get-ObjectPropertyValueOrNull -InputObject $cycleState -PropertyName 'lastKnownStatus')
$consolidationState = [string] (Get-ObjectPropertyValueOrNull -InputObject $cmeConsolidationState -PropertyName 'consolidationState')
$consecutiveAcceptedCount = [int] $(if ($null -ne $cmeConsolidationState) { $cmeConsolidationState.consecutiveAcceptedCount } else { 0 })
$seededGovernanceDisposition = [string] (Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'disposition')
$seededPromotionDisposition = [string] (Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'disposition')
$handshakeState = [string] (Get-ObjectPropertyValueOrNull -InputObject $releaseHandshakeState -PropertyName 'handshakeState')
$handshakeNextAction = [string] (Get-ObjectPropertyValueOrNull -InputObject $releaseHandshakeState -PropertyName 'nextAction')
$promotionRecommendation = [string] (Get-ObjectPropertyValueOrNull -InputObject $promotionGateState -PropertyName 'recommendation')
$firstPublishIntent = [string] (Get-ObjectPropertyValueOrNull -InputObject $firstPublishIntentState -PropertyName 'intentState')

$capabilityObservedAt = if ($null -ne $seededGovernanceState) { [string] $seededGovernanceState.generatedAtUtc } else { (Get-Date).ToUniversalTime().ToString('o') }
$promotionObservedAt = if ($null -ne $seededPromotionReviewState) { [string] $seededPromotionReviewState.generatedAtUtc } else { $capabilityObservedAt }
$formationObservedAt = if ($null -ne $releaseHandshakeState) { [string] $releaseHandshakeState.generatedAtUtc } else { $capabilityObservedAt }
$continuityObservedAt = if ($null -ne $cmeConsolidationState) { [string] $cmeConsolidationState.generatedAtUtc } else { $capabilityObservedAt }

$observedTalentState = if ($null -ne $seededGovernanceState) { 'Observed' } else { 'Dormant' }
$evidencedSkillState = if ($null -ne $seededGovernanceState -and -not [string]::IsNullOrWhiteSpace($seededGovernanceDisposition)) { 'Evidenced' } else { 'Dormant' }
$seededGovernanceAbilityState = Resolve-CapabilityStateFromDisposition -Disposition $seededGovernanceDisposition
$seededPromotionAbilityState = Resolve-CapabilityStateFromDisposition -Disposition $seededPromotionDisposition

$capabilityLedgerState = 'Dormant'
if ($seededGovernanceAbilityState -eq 'Admissible' -or $seededPromotionAbilityState -eq 'Admissible') {
    $capabilityLedgerState = 'Admissible'
} elseif ($seededGovernanceAbilityState -eq 'Deferred' -or $seededPromotionAbilityState -eq 'Deferred') {
    $capabilityLedgerState = 'Deferred'
} elseif ($seededGovernanceAbilityState -eq 'Withheld' -or $seededPromotionAbilityState -eq 'Withheld') {
    $capabilityLedgerState = 'Withheld'
} elseif ($evidencedSkillState -eq 'Evidenced') {
    $capabilityLedgerState = 'Evidenced'
}

$formationLedgerState = 'Dormant'
$formationReasonCode = 'cme-formation-ledger-dormant'
if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $formationLedgerState = 'Suspended'
    $formationReasonCode = 'cme-formation-ledger-automation-blocked'
} elseif ($handshakeState -eq 'awaiting-ratification' -or $promotionRecommendation -eq 'ratification-required') {
    $formationLedgerState = 'Active'
    $formationReasonCode = 'cme-formation-ledger-ratification-active'
} elseif ($firstPublishIntent -eq 'closed-candidate-intent' -or $consolidationState -in @('SeedAssisted', 'Braided', 'Crystallizing')) {
    $formationLedgerState = 'Active'
    $formationReasonCode = 'cme-formation-ledger-continuity-active'
}

$governanceReviewerOfficeState = Resolve-OfficeStateFromCapability -CapabilityState $seededGovernanceAbilityState -HandshakeState ''
$promotionReviewerOfficeState = Resolve-OfficeStateFromCapability -CapabilityState $seededPromotionAbilityState -HandshakeState $handshakeState

$officeLedgerState = 'Dormant'
$officeReasonCode = 'cme-office-ledger-dormant'
if ($governanceReviewerOfficeState -eq 'Open') {
    $officeLedgerState = 'Open'
    $officeReasonCode = 'cme-office-ledger-governance-open'
}

if ($promotionReviewerOfficeState -eq 'Provisional') {
    $officeLedgerState = 'Provisional'
    $officeReasonCode = 'cme-office-ledger-promotion-provisional'
} elseif ($promotionReviewerOfficeState -eq 'Open' -and $officeLedgerState -eq 'Dormant') {
    $officeLedgerState = 'Open'
    $officeReasonCode = 'cme-office-ledger-promotion-open'
} elseif ($officeLedgerState -eq 'Dormant' -and ($promotionReviewerOfficeState -eq 'Deferred' -or $governanceReviewerOfficeState -eq 'Deferred')) {
    $officeLedgerState = 'Deferred'
    $officeReasonCode = 'cme-office-ledger-offices-deferred'
} elseif ($officeLedgerState -eq 'Dormant' -and ($promotionReviewerOfficeState -eq 'Withheld' -or $governanceReviewerOfficeState -eq 'Withheld')) {
    $officeLedgerState = 'Withheld'
    $officeReasonCode = 'cme-office-ledger-offices-withheld'
}

$careerTrajectoryState = 'Unjustified'
$careerReasonCode = 'cme-career-continuity-not-earned'
if ($lastKnownStatus -eq [string] $cyclePolicy.blockedStatus) {
    $careerTrajectoryState = 'Suspended'
    $careerReasonCode = 'cme-career-continuity-automation-blocked'
} elseif ($consolidationState -eq 'Crystallizing' -and $consecutiveAcceptedCount -ge 2 -and $governanceReviewerOfficeState -in @('Open', 'Provisional')) {
    $careerTrajectoryState = 'Emerging'
    $careerReasonCode = 'cme-career-continuity-emerging'
}

$ledgerState = 'Dormant'
$reasonCode = 'cme-formation-office-ledger-dormant'
$nextAction = 'continue-bounded-observation'

if ($officeLedgerState -eq 'Provisional') {
    $ledgerState = 'Provisional'
    $reasonCode = 'cme-formation-office-ledger-provisional-office'
    $nextAction = if (-not [string]::IsNullOrWhiteSpace($handshakeNextAction)) { $handshakeNextAction } else { 'continue-bounded-ratification-formation' }
} elseif ($officeLedgerState -eq 'Open') {
    $ledgerState = 'Open'
    $reasonCode = 'cme-formation-office-ledger-open-office'
    $nextAction = 'continue-bounded-office-observation'
} elseif ($formationLedgerState -eq 'Active') {
    $ledgerState = 'Active'
    $reasonCode = 'cme-formation-office-ledger-active-formation'
    $nextAction = 'continue-formation-until-office-opens-lawfully'
} elseif ($careerTrajectoryState -eq 'Emerging') {
    $ledgerState = 'Emerging'
    $reasonCode = 'cme-formation-office-ledger-emerging-continuity'
    $nextAction = 'continue-continuity-observation'
}

$timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$bundleKey = if (-not [string]::IsNullOrWhiteSpace([string] $cycleState.lastReleaseCandidateBundle)) {
    [System.IO.Path]::GetFileName([string] $cycleState.lastReleaseCandidateBundle)
} else {
    'no-run'
}
$bundlePath = Join-Path $ledgerOutputRoot ('{0}-{1}' -f $timestamp, $bundleKey)
$bundleJsonPath = Join-Path $bundlePath 'cme-formation-office-ledger.json'
$bundleMarkdownPath = Join-Path $bundlePath 'cme-formation-office-ledger.md'

$evidenceSources = [ordered]@{
    seededGovernance = Get-ObjectPropertyValueOrNull -InputObject $seededGovernanceState -PropertyName 'bundlePath'
    cmeConsolidation = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $cmeConsolidationStatePath
    promotionGate = Get-ObjectPropertyValueOrNull -InputObject $promotionGateState -PropertyName 'bundlePath'
    seededPromotionReview = Get-ObjectPropertyValueOrNull -InputObject $seededPromotionReviewState -PropertyName 'bundlePath'
    firstPublishIntent = Get-ObjectPropertyValueOrNull -InputObject $firstPublishIntentState -PropertyName 'bundlePath'
    releaseHandshake = Get-ObjectPropertyValueOrNull -InputObject $releaseHandshakeState -PropertyName 'bundlePath'
}

$capabilityEntries = @(
    [ordered]@{
        entryId = 'seeded-advisory-predisposition'
        name = 'Seeded Advisory Predisposition'
        capabilityKind = 'Talent'
        state = $observedTalentState
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.cmeConsolidation)
        admissibilityReason = 'Observed repeated seed participation may exist without office on its own.'
        constraints = @(
            'Does not open office by itself.',
            'Must not be collapsed into skill, ability, or sovereignty.'
        )
        observedAtUtc = $capabilityObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    },
    [ordered]@{
        entryId = 'seeded-build-evidence-interpretation'
        name = 'Seeded Build Evidence Interpretation'
        capabilityKind = 'Skill'
        state = $evidencedSkillState
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.seededPromotionReview)
        admissibilityReason = 'Repeated seeded review artifacts evidence a trained advisory performance without automatically opening office.'
        constraints = @(
            'Skill does not open office by itself.',
            'Performance remains bounded to advisory interpretation.'
        )
        observedAtUtc = $capabilityObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    },
    [ordered]@{
        entryId = 'bounded-seeded-governance-review'
        name = 'Bounded Seeded Governance Review'
        capabilityKind = 'Ability'
        state = $seededGovernanceAbilityState
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.cmeConsolidation)
        admissibilityReason = 'Current seeded governance disposition determines whether routine build-evidence review is admissible now.'
        constraints = @(
            'May classify routine build evidence only.',
            'May not promote versions, publish artifacts, or widen authority.'
        )
        observedAtUtc = $capabilityObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    },
    [ordered]@{
        entryId = 'bounded-seeded-promotion-review'
        name = 'Bounded Seeded Promotion Review'
        capabilityKind = 'Ability'
        state = $seededPromotionAbilityState
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededPromotionReview, $evidenceSources.promotionGate, $evidenceSources.releaseHandshake)
        admissibilityReason = 'Promotion review ability is bounded by seeded promotion review evidence and the current ratification seam.'
        constraints = @(
            'May advise on promotion evidence only.',
            'May not ratify publication or replace HITL.'
        )
        observedAtUtc = $promotionObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    }
)

$formationEntries = @(
    [ordered]@{
        entryId = 'release-ratification-formation'
        name = 'Release Ratification Formation'
        formationState = $formationLedgerState
        whyFormationIsActive = 'The stack is still forming the lawful passage from bounded promotion evidence into ratified publication.'
        targetCapabilityOrOffice = 'Seeded Promotion Reviewer'
        requiredMilestones = @(
            'Promotion gate remains bounded and explicit.',
            'Release handshake becomes ratifiable under present law.',
            'HITL ratification occurs when requested.'
        )
        blockingConditions = @(
            'Automation enters blocked posture.',
            'Promotion evidence becomes contradictory or missing.'
        )
        suspensionConditions = @(
            'Release authority widens by implication.',
            'Seeded review tries to substitute for ratification.'
        )
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.promotionGate, $evidenceSources.firstPublishIntent, $evidenceSources.releaseHandshake)
        observedAtUtc = $formationObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    },
    [ordered]@{
        entryId = 'cme-governance-continuity-formation'
        name = 'CME Governance Continuity Formation'
        formationState = if ($careerTrajectoryState -eq 'Suspended') { 'Suspended' } elseif ($careerTrajectoryState -eq 'Emerging') { 'Active' } elseif ($consolidationState -in @('SeedAssisted', 'Braided', 'Crystallizing')) { 'Active' } else { 'Deferred' }
        whyFormationIsActive = 'The stack is still forming lawful continuity between repeated seeded offices and later continuity-bearing claims.'
        targetCapabilityOrOffice = 'Seeded Governance Advisory Trajectory'
        requiredMilestones = @(
            'Repeated accepted seeded governance runs remain bounded.',
            'Scheduler and governance evidence stay aligned.',
            'Office continuity remains advisory and lawful.'
        )
        blockingConditions = @(
            'Seeded governance becomes deferred or rejected across intervals.',
            'Automation evidence enters contradiction.'
        )
        suspensionConditions = @(
            'Automation posture becomes blocked.',
            'Continuity is narrated beyond current evidence.'
        )
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.cmeConsolidation, $evidenceSources.seededPromotionReview)
        observedAtUtc = $continuityObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    }
)

$officeEntries = @(
    [ordered]@{
        entryId = 'seeded-governance-reviewer'
        officeName = 'Seeded Governance Reviewer'
        officeState = $governanceReviewerOfficeState
        openingReason = 'The office opens only when bounded seeded governance review is currently admissible.'
        abilityRequirements = @('Bounded Seeded Governance Review')
        admissibleDuties = @(
            'Interpret routine build evidence.',
            'Contribute bounded advisory provenance.',
            'Report readiness posture without widening authority.'
        )
        withheldAuthorities = @(
            'Version promotion',
            'Publication ratification',
            'Deployable scope widening',
            'Unbounded runtime authority'
        )
        requiredOversight = @(
            'HITL ratifies promotion and publication.',
            'Automation and doctrine remain the executable truth surfaces.'
        )
        suspensionConditions = @(
            'Seeded governance disposition becomes deferred or rejected.',
            'Automation posture becomes blocked.'
        )
        dissolutionConditions = @(
            'Capability evidence is withdrawn or no longer admissible.',
            'Governed advisory lane is removed from policy.'
        )
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.cmeConsolidation)
        observedAtUtc = $capabilityObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    },
    [ordered]@{
        entryId = 'seeded-promotion-reviewer'
        officeName = 'Seeded Promotion Reviewer'
        officeState = $promotionReviewerOfficeState
        openingReason = 'The office may open only after promotion review ability is admissible and the ratification seam is kept explicit.'
        abilityRequirements = @('Bounded Seeded Promotion Review')
        admissibleDuties = @(
            'Review bounded promotion evidence.',
            'Advise on release readiness under law.',
            'Surface why ratification is still required.'
        )
        withheldAuthorities = @(
            'Final ratification',
            'Artifact publication',
            'Release scope expansion',
            'Career continuity claims by implication'
        )
        requiredOversight = @(
            'HITL remains the ratifier.',
            'Release handshake remains explicit and bounded.'
        )
        suspensionConditions = @(
            'Release handshake ceases to be bounded.',
            'Promotion evidence becomes blocked or contradictory.'
        )
        dissolutionConditions = @(
            'Seeded promotion review is no longer admissible.',
            'The promotion lane is withdrawn from the automation policy.'
        )
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededPromotionReview, $evidenceSources.promotionGate, $evidenceSources.releaseHandshake, $evidenceSources.firstPublishIntent)
        observedAtUtc = $promotionObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    }
)

$officeHistory = @()
if ($governanceReviewerOfficeState -in @('Open', 'Provisional')) {
    $officeHistory += [ordered]@{
        officeName = 'Seeded Governance Reviewer'
        officeState = $governanceReviewerOfficeState
        evidence = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.cmeConsolidation)
    }
}
if ($promotionReviewerOfficeState -in @('Open', 'Provisional')) {
    $officeHistory += [ordered]@{
        officeName = 'Seeded Promotion Reviewer'
        officeState = $promotionReviewerOfficeState
        evidence = New-UniqueStringList -Values @($evidenceSources.seededPromotionReview, $evidenceSources.releaseHandshake)
    }
}

$careerEntries = @(
    [ordered]@{
        entryId = 'seeded-governance-advisory-trajectory'
        trajectoryName = 'Seeded Governance Advisory Trajectory'
        trajectoryState = $careerTrajectoryState
        officeHistory = $officeHistory
        continuityEvidence = [ordered]@{
            consolidationState = $consolidationState
            consecutiveAcceptedSeedRuns = $consecutiveAcceptedCount
            schedulerAligned = [bool] (Get-ObjectPropertyValueOrNull -InputObject $cmeConsolidationState -PropertyName 'schedulerAligned')
        }
        stabilityThresholdsMet = @(
            $(if ($consecutiveAcceptedCount -ge 2) { 'repeated-accepted-seed-runs' }),
            $(if ($consolidationState -eq 'Crystallizing') { 'crystallizing-consolidation' }),
            $(if ($governanceReviewerOfficeState -in @('Open', 'Provisional')) { 'bounded-advisory-office-open' })
        ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        suspensionConditions = @(
            'Automation enters blocked posture.',
            'Office continuity is narrated beyond current evidence.',
            'Seeded governance loses admissibility.'
        )
        evidenceSources = New-UniqueStringList -Values @($evidenceSources.seededGovernance, $evidenceSources.cmeConsolidation, $evidenceSources.seededPromotionReview)
        observedAtUtc = $continuityObservedAt
        updatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    }
)

$payload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
    ledgerState = $ledgerState
    reasonCode = $reasonCode
    nextAction = $nextAction
    capabilityLedgerState = $capabilityLedgerState
    formationLedgerState = $formationLedgerState
    formationReasonCode = $formationReasonCode
    officeLedgerState = $officeLedgerState
    officeReasonCode = $officeReasonCode
    careerContinuityLedgerState = $careerTrajectoryState
    careerReasonCode = $careerReasonCode
    evidenceSources = $evidenceSources
    capabilityLedger = $capabilityEntries
    formationLedger = $formationEntries
    officeLedger = $officeEntries
    careerContinuityLedger = $careerEntries
}

Write-JsonFile -Path $bundleJsonPath -Value $payload

$markdownLines = @(
    '# CME Formation and Office Ledger',
    '',
    ('- Generated at (UTC): `{0}`' -f $payload.generatedAtUtc),
    ('- Ledger state: `{0}`' -f $payload.ledgerState),
    ('- Reason code: `{0}`' -f $payload.reasonCode),
    ('- Next action: `{0}`' -f $payload.nextAction),
    ('- Capability ledger state: `{0}`' -f $payload.capabilityLedgerState),
    ('- Formation ledger state: `{0}`' -f $payload.formationLedgerState),
    ('- Office ledger state: `{0}`' -f $payload.officeLedgerState),
    ('- Career continuity state: `{0}`' -f $payload.careerContinuityLedgerState),
    '',
    '## Capability Ledger',
    ''
)

foreach ($entry in $capabilityEntries) {
    $markdownLines += @(
        ('### {0}' -f [string] $entry.name),
        ('- Kind: `{0}`' -f [string] $entry.capabilityKind),
        ('- State: `{0}`' -f [string] $entry.state),
        ('- Admissibility reason: `{0}`' -f [string] $entry.admissibilityReason),
        ('- Evidence: `{0}`' -f $((@($entry.evidenceSources) -join ', '))),
        ('- Constraints: `{0}`' -f $((@($entry.constraints) -join '; '))),
        ''
    )
}

$markdownLines += @(
    '## Formation Ledger',
    ''
)

foreach ($entry in $formationEntries) {
    $markdownLines += @(
        ('### {0}' -f [string] $entry.name),
        ('- State: `{0}`' -f [string] $entry.formationState),
        ('- Target: `{0}`' -f [string] $entry.targetCapabilityOrOffice),
        ('- Why active: `{0}`' -f [string] $entry.whyFormationIsActive),
        ('- Milestones: `{0}`' -f $((@($entry.requiredMilestones) -join '; '))),
        ('- Blocking conditions: `{0}`' -f $((@($entry.blockingConditions) -join '; '))),
        ''
    )
}

$markdownLines += @(
    '## Office Ledger',
    ''
)

foreach ($entry in $officeEntries) {
    $markdownLines += @(
        ('### {0}' -f [string] $entry.officeName),
        ('- State: `{0}`' -f [string] $entry.officeState),
        ('- Opening reason: `{0}`' -f [string] $entry.openingReason),
        ('- Ability requirements: `{0}`' -f $((@($entry.abilityRequirements) -join ', '))),
        ('- Duties: `{0}`' -f $((@($entry.admissibleDuties) -join '; '))),
        ('- Withheld authorities: `{0}`' -f $((@($entry.withheldAuthorities) -join '; '))),
        ('- Oversight: `{0}`' -f $((@($entry.requiredOversight) -join '; '))),
        ''
    )
}

$markdownLines += @(
    '## Career Continuity Ledger',
    ''
)

foreach ($entry in $careerEntries) {
    $officeHistorySummary = if (@($entry.officeHistory).Count -gt 0) {
        ((@($entry.officeHistory | ForEach-Object { '{0} ({1})' -f [string] $_.officeName, [string] $_.officeState }) -join '; '))
    } else {
        'none-yet'
    }

    $markdownLines += @(
        ('### {0}' -f [string] $entry.trajectoryName),
        ('- State: `{0}`' -f [string] $entry.trajectoryState),
        ('- Office history: `{0}`' -f $officeHistorySummary),
        ('- Stability thresholds met: `{0}`' -f $(if (@($entry.stabilityThresholdsMet).Count -gt 0) { ((@($entry.stabilityThresholdsMet) -join '; ')) } else { 'none-yet' })),
        ('- Continuity evidence: consolidation=`{0}`, acceptedRuns=`{1}`, schedulerAligned=`{2}`' -f [string] $entry.continuityEvidence.consolidationState, [string] $entry.continuityEvidence.consecutiveAcceptedSeedRuns, [string] $entry.continuityEvidence.schedulerAligned),
        ''
    )
}

Set-Content -LiteralPath $bundleMarkdownPath -Value $markdownLines -Encoding utf8

$statePayload = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = $payload.generatedAtUtc
    bundlePath = Get-RelativePathString -BasePath $resolvedRepoRoot -TargetPath $bundlePath
    ledgerState = $payload.ledgerState
    reasonCode = $payload.reasonCode
    nextAction = $payload.nextAction
    capabilityLedgerState = $payload.capabilityLedgerState
    formationLedgerState = $payload.formationLedgerState
    officeLedgerState = $payload.officeLedgerState
    careerContinuityLedgerState = $payload.careerContinuityLedgerState
}

Write-JsonFile -Path $ledgerStatePath -Value $statePayload
Write-Host ('[cme-formation-office-ledger] Bundle: {0}' -f $bundlePath)
$bundlePath
