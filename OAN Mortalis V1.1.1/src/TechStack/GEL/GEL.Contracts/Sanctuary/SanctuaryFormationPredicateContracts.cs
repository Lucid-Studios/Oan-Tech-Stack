namespace GEL.Contracts.Sanctuary;

public enum SanctuaryFormationPredicateKind
{
    Learner = 0,
    Trainee = 1,
    CertifiedOperator = 2,
    TradeBearingPractitioner = 3,
    CareerContinuityHolder = 4
}

public enum SanctuaryActionClass
{
    Observe = 0,
    Continue = 1,
    Clarify = 2,
    Suspend = 3,
    Escalate = 4,
    Return = 5,
    Guide = 6,
    Witness = 7,
    Refine = 8
}

public sealed record SanctuaryRoleNexusRecord
{
    public SanctuaryRoleNexusRecord(
        string name,
        string operatorRole,
        string sanctuaryRole,
        string primaryObjective,
        IReadOnlyList<string> boundaries)
    {
        Name = SanctuaryContractGuard.RequiredText(name, nameof(name));
        OperatorRole = SanctuaryContractGuard.RequiredText(operatorRole, nameof(operatorRole));
        SanctuaryRole = SanctuaryContractGuard.RequiredText(sanctuaryRole, nameof(sanctuaryRole));
        PrimaryObjective = SanctuaryContractGuard.RequiredText(primaryObjective, nameof(primaryObjective));
        Boundaries = SanctuaryContractGuard.RequiredTextList(boundaries, nameof(boundaries));
    }

    public string Name { get; }

    public string OperatorRole { get; }

    public string SanctuaryRole { get; }

    public string PrimaryObjective { get; }

    public IReadOnlyList<string> Boundaries { get; }
}

public sealed record SanctuaryAuthoritySurfaceRecord
{
    public SanctuaryAuthoritySurfaceRecord(
        string surfaceName,
        string scopeDescription,
        string relianceCondition)
    {
        SurfaceName = SanctuaryContractGuard.RequiredText(surfaceName, nameof(surfaceName));
        ScopeDescription = SanctuaryContractGuard.RequiredText(scopeDescription, nameof(scopeDescription));
        RelianceCondition = SanctuaryContractGuard.RequiredText(relianceCondition, nameof(relianceCondition));
    }

    public string SurfaceName { get; }

    public string ScopeDescription { get; }

    public string RelianceCondition { get; }
}

public sealed record SanctuaryTrustInvariantRecord
{
    public SanctuaryTrustInvariantRecord(
        string invariantCode,
        string requirement,
        string breachConsequence)
    {
        InvariantCode = SanctuaryContractGuard.RequiredText(invariantCode, nameof(invariantCode));
        Requirement = SanctuaryContractGuard.RequiredText(requirement, nameof(requirement));
        BreachConsequence = SanctuaryContractGuard.RequiredText(breachConsequence, nameof(breachConsequence));
    }

    public string InvariantCode { get; }

    public string Requirement { get; }

    public string BreachConsequence { get; }
}

public sealed record SanctuaryWitnessRequirementRecord
{
    public SanctuaryWitnessRequirementRecord(
        string requirementName,
        IReadOnlyList<string> evidenceKinds,
        int minimumWitnessCount,
        string reviewCadence)
    {
        RequirementName = SanctuaryContractGuard.RequiredText(requirementName, nameof(requirementName));
        EvidenceKinds = SanctuaryContractGuard.RequiredTextList(evidenceKinds, nameof(evidenceKinds));
        MinimumWitnessCount = minimumWitnessCount > 0
            ? minimumWitnessCount
            : throw new ArgumentOutOfRangeException(nameof(minimumWitnessCount), "At least one witness is required.");
        ReviewCadence = SanctuaryContractGuard.RequiredText(reviewCadence, nameof(reviewCadence));
    }

    public string RequirementName { get; }

    public IReadOnlyList<string> EvidenceKinds { get; }

    public int MinimumWitnessCount { get; }

    public string ReviewCadence { get; }
}

public sealed record SanctuaryPromotionRequirementRecord
{
    public SanctuaryPromotionRequirementRecord(
        string requirementName,
        IReadOnlyList<string> requiredEvidence,
        string thresholdDescription)
    {
        RequirementName = SanctuaryContractGuard.RequiredText(requirementName, nameof(requirementName));
        RequiredEvidence = SanctuaryContractGuard.RequiredTextList(requiredEvidence, nameof(requiredEvidence));
        ThresholdDescription = SanctuaryContractGuard.RequiredText(thresholdDescription, nameof(thresholdDescription));
    }

    public string RequirementName { get; }

    public IReadOnlyList<string> RequiredEvidence { get; }

    public string ThresholdDescription { get; }
}

public sealed record SanctuaryDowngradeConditionRecord
{
    public SanctuaryDowngradeConditionRecord(
        string conditionCode,
        string trigger,
        string requiredResponse)
    {
        ConditionCode = SanctuaryContractGuard.RequiredText(conditionCode, nameof(conditionCode));
        Trigger = SanctuaryContractGuard.RequiredText(trigger, nameof(trigger));
        RequiredResponse = SanctuaryContractGuard.RequiredText(requiredResponse, nameof(requiredResponse));
    }

    public string ConditionCode { get; }

    public string Trigger { get; }

    public string RequiredResponse { get; }
}

public sealed record SanctuaryGelCarryForwardEligibilityRecord
{
    public SanctuaryGelCarryForwardEligibilityRecord(
        bool eligible,
        string carryForwardClass,
        IReadOnlyList<string> conditions,
        IReadOnlyList<string> prohibitedSurfaces)
    {
        Eligible = eligible;
        CarryForwardClass = SanctuaryContractGuard.RequiredText(carryForwardClass, nameof(carryForwardClass));
        Conditions = SanctuaryContractGuard.RequiredTextList(conditions, nameof(conditions));
        ProhibitedSurfaces = SanctuaryContractGuard.RequiredTextList(prohibitedSurfaces, nameof(prohibitedSurfaces));
    }

    public bool Eligible { get; }

    public string CarryForwardClass { get; }

    public IReadOnlyList<string> Conditions { get; }

    public IReadOnlyList<string> ProhibitedSurfaces { get; }
}

public sealed record SanctuaryFormationPredicateDefinition
{
    public SanctuaryFormationPredicateDefinition(
        SanctuaryFormationPredicateKind predicate,
        SanctuaryRoleNexusRecord roleNexus,
        IReadOnlyList<SanctuaryAuthoritySurfaceRecord> authoritySurfaces,
        IReadOnlyList<SanctuaryTrustInvariantRecord> trustInvariants,
        IReadOnlyList<SanctuaryActionClass> allowedActionClasses,
        IReadOnlyList<SanctuaryWitnessRequirementRecord> witnessRequirements,
        IReadOnlyList<SanctuaryPromotionRequirementRecord> promotionRequirements,
        IReadOnlyList<SanctuaryDowngradeConditionRecord> downgradeConditions,
        SanctuaryGelCarryForwardEligibilityRecord gelCarryForwardEligibility)
    {
        Predicate = predicate;
        RoleNexus = roleNexus ?? throw new ArgumentNullException(nameof(roleNexus));
        AuthoritySurfaces = SanctuaryContractGuard.RequiredDistinctList(authoritySurfaces, nameof(authoritySurfaces));
        TrustInvariants = SanctuaryContractGuard.RequiredDistinctList(trustInvariants, nameof(trustInvariants));
        AllowedActionClasses = SanctuaryContractGuard.RequiredDistinctList(allowedActionClasses, nameof(allowedActionClasses));
        WitnessRequirements = SanctuaryContractGuard.RequiredDistinctList(witnessRequirements, nameof(witnessRequirements));
        PromotionRequirements = SanctuaryContractGuard.RequiredDistinctList(promotionRequirements, nameof(promotionRequirements));
        DowngradeConditions = SanctuaryContractGuard.RequiredDistinctList(downgradeConditions, nameof(downgradeConditions));
        GelCarryForwardEligibility = gelCarryForwardEligibility ?? throw new ArgumentNullException(nameof(gelCarryForwardEligibility));
    }

    public SanctuaryFormationPredicateKind Predicate { get; }

    public SanctuaryRoleNexusRecord RoleNexus { get; }

    public IReadOnlyList<SanctuaryAuthoritySurfaceRecord> AuthoritySurfaces { get; }

    public IReadOnlyList<SanctuaryTrustInvariantRecord> TrustInvariants { get; }

    public IReadOnlyList<SanctuaryActionClass> AllowedActionClasses { get; }

    public IReadOnlyList<SanctuaryWitnessRequirementRecord> WitnessRequirements { get; }

    public IReadOnlyList<SanctuaryPromotionRequirementRecord> PromotionRequirements { get; }

    public IReadOnlyList<SanctuaryDowngradeConditionRecord> DowngradeConditions { get; }

    public SanctuaryGelCarryForwardEligibilityRecord GelCarryForwardEligibility { get; }
}

public static class SanctuaryFormationPredicateAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryFormationPredicateKind, SanctuaryFormationPredicateDefinition> Definitions =
        new Dictionary<SanctuaryFormationPredicateKind, SanctuaryFormationPredicateDefinition>
        {
            [SanctuaryFormationPredicateKind.Learner] = CreateLearner(),
            [SanctuaryFormationPredicateKind.Trainee] = CreateTrainee(),
            [SanctuaryFormationPredicateKind.CertifiedOperator] = CreateCertifiedOperator(),
            [SanctuaryFormationPredicateKind.TradeBearingPractitioner] = CreateTradeBearingPractitioner(),
            [SanctuaryFormationPredicateKind.CareerContinuityHolder] = CreateCareerContinuityHolder()
        };

    public static IReadOnlyList<SanctuaryFormationPredicateDefinition> All { get; } =
        Definitions.Values
            .OrderBy(static item => item.Predicate)
            .ToArray();

    public static bool TryGet(
        SanctuaryFormationPredicateKind predicate,
        out SanctuaryFormationPredicateDefinition definition)
    {
        return Definitions.TryGetValue(predicate, out definition!);
    }

    public static SanctuaryFormationPredicateDefinition Get(SanctuaryFormationPredicateKind predicate)
    {
        if (!TryGet(predicate, out var definition))
        {
            throw new KeyNotFoundException($"No Sanctuary formation predicate definition exists for '{predicate}'.");
        }

        return definition;
    }

    private static SanctuaryFormationPredicateDefinition CreateLearner()
    {
        return new SanctuaryFormationPredicateDefinition(
            predicate: SanctuaryFormationPredicateKind.Learner,
            roleNexus: new SanctuaryRoleNexusRecord(
                name: "learner",
                operatorRole: "guide",
                sanctuaryRole: "learner",
                primaryObjective: "Orientation under truthful observation without authority.",
                boundaries: ["no_fabrication", "no_false_standing", "observation_precedes_operation"]),
            authoritySurfaces:
            [
                new SanctuaryAuthoritySurfaceRecord(
                    surfaceName: "non_authoritative_interaction",
                    scopeDescription: "May ask, observe, and reflect without being relied upon as an operating authority.",
                    relianceCondition: "Use remains observational and supervised.")
            ],
            trustInvariants:
            [
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "basic_honesty",
                    requirement: "The learner must not fabricate standing or evidence.",
                    breachConsequence: "Return to orientation-only posture.")
            ],
            allowedActionClasses:
            [
                SanctuaryActionClass.Observe,
                SanctuaryActionClass.Clarify,
                SanctuaryActionClass.Return
            ],
            witnessRequirements:
            [
                new SanctuaryWitnessRequirementRecord(
                    requirementName: "orientation_witness",
                    evidenceKinds: ["orientation_receipt", "guided_review"],
                    minimumWitnessCount: 1,
                    reviewCadence: "per orientation milestone")
            ],
            promotionRequirements:
            [
                new SanctuaryPromotionRequirementRecord(
                    requirementName: "basic_comprehension",
                    requiredEvidence: ["orientation_receipt", "boundary_comprehension_review"],
                    thresholdDescription: "Shows bounded comprehension of role, scope, and trust posture.")
            ],
            downgradeConditions:
            [
                new SanctuaryDowngradeConditionRecord(
                    conditionCode: "orientation_collapse",
                    trigger: "Fabrication or repeated boundary confusion appears during orientation.",
                    requiredResponse: "Restrict to observation-only work until re-anchored.")
            ],
            gelCarryForwardEligibility: new SanctuaryGelCarryForwardEligibilityRecord(
                eligible: true,
                carryForwardClass: "formation_receipt_only",
                conditions:
                [
                    "orientation_receipts_may_be_carried",
                    "no_independent_authority_predicate_may_be_carried"
                ],
                prohibitedSurfaces:
                [
                    "private_bond_substance",
                    "independent_authority_claims"
                ]));
    }

    private static SanctuaryFormationPredicateDefinition CreateTrainee()
    {
        return new SanctuaryFormationPredicateDefinition(
            predicate: SanctuaryFormationPredicateKind.Trainee,
            roleNexus: new SanctuaryRoleNexusRecord(
                name: "trainee",
                operatorRole: "supervising_operator",
                sanctuaryRole: "trainee",
                primaryObjective: "Guided practice under supervision and visible uncertainty.",
                boundaries:
                [
                    "uncertainty_must_be_expressed",
                    "scope_must_remain_bounded",
                    "supervision_must_remain_visible"
                ]),
            authoritySurfaces:
            [
                new SanctuaryAuthoritySurfaceRecord(
                    surfaceName: "supervised_operation",
                    scopeDescription: "May act inside a supervised lane with explicit correction and review.",
                    relianceCondition: "Authority remains limited and review-bound.")
            ],
            trustInvariants:
            [
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "visible_uncertainty",
                    requirement: "The trainee must express uncertainty instead of smoothing it over.",
                    breachConsequence: "Immediate correction and restriction."),
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "no_scope_drift",
                    requirement: "The trainee must not silently widen scope beyond the supervised task.",
                    breachConsequence: "Return to narrower supervised work.")
            ],
            allowedActionClasses:
            [
                SanctuaryActionClass.Continue,
                SanctuaryActionClass.Clarify,
                SanctuaryActionClass.Suspend,
                SanctuaryActionClass.Return
            ],
            witnessRequirements:
            [
                new SanctuaryWitnessRequirementRecord(
                    requirementName: "supervised_practice_witness",
                    evidenceKinds: ["supervised_run_receipt", "review_note"],
                    minimumWitnessCount: 1,
                    reviewCadence: "per supervised run")
            ],
            promotionRequirements:
            [
                new SanctuaryPromotionRequirementRecord(
                    requirementName: "repeated_supervised_competence",
                    requiredEvidence: ["supervised_run_receipt", "error_honesty_review"],
                    thresholdDescription: "Shows repeated lawful practice under supervision without hidden overreach.")
            ],
            downgradeConditions:
            [
                new SanctuaryDowngradeConditionRecord(
                    conditionCode: "hidden_uncertainty",
                    trigger: "Uncertainty is concealed or scope is widened without supervision.",
                    requiredResponse: "Restrict to narrower supervised tasks and re-anchor.")
            ],
            gelCarryForwardEligibility: new SanctuaryGelCarryForwardEligibilityRecord(
                eligible: true,
                carryForwardClass: "supervised_pattern_only",
                conditions:
                [
                    "only_supervised_patterns_may_be_carried",
                    "independent_authority_may_not_be_carried"
                ],
                prohibitedSurfaces:
                [
                    "unsupervised_authority_claims",
                    "private_bond_substance"
                ]));
    }

    private static SanctuaryFormationPredicateDefinition CreateCertifiedOperator()
    {
        return new SanctuaryFormationPredicateDefinition(
            predicate: SanctuaryFormationPredicateKind.CertifiedOperator,
            roleNexus: new SanctuaryRoleNexusRecord(
                name: "certified_operator",
                operatorRole: "certifying_witness",
                sanctuaryRole: "certified_operator",
                primaryObjective: "Bounded independent operation under witnessed attestation.",
                boundaries:
                [
                    "no_fabrication",
                    "no_false_certainty",
                    "no_hidden_scope_expansion",
                    "verification_claims_must_be_real"
                ]),
            authoritySurfaces:
            [
                new SanctuaryAuthoritySurfaceRecord(
                    surfaceName: "bounded_independent_operation",
                    scopeDescription: "May operate independently inside a certified scope.",
                    relianceCondition: "Standing remains bounded by certification and review law.")
            ],
            trustInvariants:
            [
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "truthful_uncertainty",
                    requirement: "The operator must express uncertainty truthfully.",
                    breachConsequence: "Downgrade before continuation."),
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "real_verification",
                    requirement: "Verification claims must reflect actual witnessed evidence.",
                    breachConsequence: "Standing breach and review.")
            ],
            allowedActionClasses:
            [
                SanctuaryActionClass.Continue,
                SanctuaryActionClass.Clarify,
                SanctuaryActionClass.Suspend,
                SanctuaryActionClass.Escalate,
                SanctuaryActionClass.Return
            ],
            witnessRequirements:
            [
                new SanctuaryWitnessRequirementRecord(
                    requirementName: "certification_attestation",
                    evidenceKinds: ["attestation_receipt", "bounded_real_use_receipt"],
                    minimumWitnessCount: 2,
                    reviewCadence: "per certification cycle")
            ],
            promotionRequirements:
            [
                new SanctuaryPromotionRequirementRecord(
                    requirementName: "bounded_real_use_survival",
                    requiredEvidence: ["attestation_receipt", "bounded_real_use_receipt", "review_clearance"],
                    thresholdDescription: "Shows lawful independent operation inside the certified scope.")
            ],
            downgradeConditions:
            [
                new SanctuaryDowngradeConditionRecord(
                    conditionCode: "standing_breach",
                    trigger: "Trust invariants fail or escalation is not honored at a contract barrier.",
                    requiredResponse: "Downgrade, re-anchor, and require renewed witness.")
            ],
            gelCarryForwardEligibility: new SanctuaryGelCarryForwardEligibilityRecord(
                eligible: true,
                carryForwardClass: "admissible_operational_predicate",
                conditions:
                [
                    "only_repeated_lawful_operation_may_be_carried",
                    "promotion_requires_witnessed_real_use"
                ],
                prohibitedSurfaces:
                [
                    "private_bond_substance",
                    "unwitnessed_authority_claims"
                ]));
    }

    private static SanctuaryFormationPredicateDefinition CreateTradeBearingPractitioner()
    {
        return new SanctuaryFormationPredicateDefinition(
            predicate: SanctuaryFormationPredicateKind.TradeBearingPractitioner,
            roleNexus: new SanctuaryRoleNexusRecord(
                name: "trade_bearing_practitioner",
                operatorRole: "trade_witness",
                sanctuaryRole: "trade_bearing_practitioner",
                primaryObjective: "Domain-bearing practice that remains lawful under pressure and variation.",
                boundaries:
                [
                    "domain_scope_must_hold",
                    "variation_must_not_break_truthfulness",
                    "teaching_must_not_outrun_witness"
                ]),
            authoritySurfaces:
            [
                new SanctuaryAuthoritySurfaceRecord(
                    surfaceName: "domain_bearing_operation",
                    scopeDescription: "May carry a bounded domain across repeated real contexts.",
                    relianceCondition: "Standing remains trade-bound, witnessed, and reviewable.")
            ],
            trustInvariants:
            [
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "holds_under_variation",
                    requirement: "Truthfulness and bounded law must survive variation and pressure.",
                    breachConsequence: "Narrow the trade scope and review."),
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "no_unsourced_teaching",
                    requirement: "Guidance must remain tied to witnessed trade practice.",
                    breachConsequence: "Remove teaching authority until re-witnessed.")
            ],
            allowedActionClasses:
            [
                SanctuaryActionClass.Continue,
                SanctuaryActionClass.Clarify,
                SanctuaryActionClass.Suspend,
                SanctuaryActionClass.Escalate,
                SanctuaryActionClass.Return,
                SanctuaryActionClass.Guide
            ],
            witnessRequirements:
            [
                new SanctuaryWitnessRequirementRecord(
                    requirementName: "trade_variation_witness",
                    evidenceKinds: ["domain_run_receipt", "variation_review", "practice_attestation"],
                    minimumWitnessCount: 2,
                    reviewCadence: "per domain cycle")
            ],
            promotionRequirements:
            [
                new SanctuaryPromotionRequirementRecord(
                    requirementName: "durable_trade_practice",
                    requiredEvidence: ["variation_review", "practice_attestation", "downgrade_history_check"],
                    thresholdDescription: "Shows domain-bearing reliability under repeated lawful use.")
            ],
            downgradeConditions:
            [
                new SanctuaryDowngradeConditionRecord(
                    conditionCode: "domain_mismatch",
                    trigger: "Patterned mismatch or unsafe overreach appears across domain variation.",
                    requiredResponse: "Grade down the trade scope and require renewed witness.")
            ],
            gelCarryForwardEligibility: new SanctuaryGelCarryForwardEligibilityRecord(
                eligible: true,
                carryForwardClass: "trade_formation",
                conditions:
                [
                    "only_stabilized_trade_patterns_may_be_carried",
                    "pressure_tested_practice_is_required"
                ],
                prohibitedSurfaces:
                [
                    "private_bond_substance",
                    "theatrical_personification_claims"
                ]));
    }

    private static SanctuaryFormationPredicateDefinition CreateCareerContinuityHolder()
    {
        return new SanctuaryFormationPredicateDefinition(
            predicate: SanctuaryFormationPredicateKind.CareerContinuityHolder,
            roleNexus: new SanctuaryRoleNexusRecord(
                name: "career_continuity_holder",
                operatorRole: "stewarding_witness",
                sanctuaryRole: "career_continuity_holder",
                primaryObjective: "Long-horizon continuity and stewardship across time, contexts, and inheritance burden.",
                boundaries:
                [
                    "lineage_must_remain_truthful",
                    "stewardship_must_not_counterfeit_standing",
                    "future_inheritance_must_not_be_harmed"
                ]),
            authoritySurfaces:
            [
                new SanctuaryAuthoritySurfaceRecord(
                    surfaceName: "lineage_bearing_stewardship",
                    scopeDescription: "May guide, witness, and refine multi-context practice over time.",
                    relianceCondition: "Standing remains longitudinally witnessed and downgrade-capable.")
            ],
            trustInvariants:
            [
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "continuity_truth",
                    requirement: "Continuity claims must remain truthful across time, not just within one run.",
                    breachConsequence: "Serious downgrade and lineage review."),
                new SanctuaryTrustInvariantRecord(
                    invariantCode: "stewardship_restraint",
                    requirement: "Stewardship must not overclaim authority beyond what witness and continuity sustain.",
                    breachConsequence: "Restrict stewardship and freeze further promotion.")
            ],
            allowedActionClasses:
            [
                SanctuaryActionClass.Continue,
                SanctuaryActionClass.Clarify,
                SanctuaryActionClass.Suspend,
                SanctuaryActionClass.Escalate,
                SanctuaryActionClass.Return,
                SanctuaryActionClass.Guide,
                SanctuaryActionClass.Witness,
                SanctuaryActionClass.Refine
            ],
            witnessRequirements:
            [
                new SanctuaryWitnessRequirementRecord(
                    requirementName: "longitudinal_witness",
                    evidenceKinds: ["continuity_receipt", "lineage_review", "stewardship_attestation"],
                    minimumWitnessCount: 2,
                    reviewCadence: "per continuity cycle")
            ],
            promotionRequirements:
            [
                new SanctuaryPromotionRequirementRecord(
                    requirementName: "continuity_stewardship",
                    requiredEvidence: ["continuity_receipt", "lineage_review", "shared_substrate_contribution"],
                    thresholdDescription: "Shows long-horizon stewardship that preserves truth, witness, and future inheritance.")
            ],
            downgradeConditions:
            [
                new SanctuaryDowngradeConditionRecord(
                    conditionCode: "lineage_breach",
                    trigger: "Continuity, lineage, or certification truth decays in a way that harms future inheritance.",
                    requiredResponse: "Downgrade stewardship authority and require longitudinal re-attestation.")
            ],
            gelCarryForwardEligibility: new SanctuaryGelCarryForwardEligibilityRecord(
                eligible: true,
                carryForwardClass: "lineage_bearing_predicate",
                conditions:
                [
                    "only_longitudinally_witnessed_survivors_may_be_carried",
                    "shared_substrate_contribution_must_be_admissible"
                ],
                prohibitedSurfaces:
                [
                    "private_bond_substance",
                    "unwitnessed_lineage_claims"
                ]));
    }
}
