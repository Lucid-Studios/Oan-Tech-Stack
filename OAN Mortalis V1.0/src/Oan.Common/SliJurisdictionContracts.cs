using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public enum SliJurisdictionSurfaceClass
{
    Actualized = 0,
    Industrialized = 1,
    Civic = 2,
    Private = 3,
    Government = 4,
    Special = 5
}

public enum SliJurisdictionAuditDepth
{
    Minimal = 0,
    Standard = 1,
    Deep = 2,
    Maximal = 3
}

public enum SliJurisdictionOversightRequirement
{
    None = 0,
    StewardReview = 1,
    BondedReview = 2,
    InstitutionalReview = 3,
    RegulatedHumanReview = 4
}

public enum SliJurisdictionRetentionClass
{
    RuntimeOnly = 0,
    GovernanceEventOnly = 1,
    ProtectedReviewLedger = 2,
    ComplianceRetention = 3
}

public enum SliJurisdictionTransitionDecision
{
    Allow = 0,
    Hold = 1,
    Refuse = 2
}

public sealed record SliJurisdictionEnvelopeReceipt(
    string EnvelopeHandle,
    SliJurisdictionSurfaceClass SurfaceClass,
    BootClass BootClass,
    string? SourceGovernanceLayerHandle,
    string? SourceFormationHandle,
    bool WitnessOnly,
    bool BondRealizationClaimed,
    SliJurisdictionAuditDepth AuditDepth,
    SliJurisdictionOversightRequirement OversightRequirement,
    SliJurisdictionRetentionClass RetentionClass,
    PrimeRevealMode RevealModeCeiling,
    bool SubordinateCmeAuthorizationAllowed,
    bool HumanReviewRequired,
    string ReasonCode);

public sealed record SliJurisdictionTransitionReceipt(
    string TransitionHandle,
    string SourceEnvelopeHandle,
    SliJurisdictionSurfaceClass SourceSurfaceClass,
    SliJurisdictionSurfaceClass TargetSurfaceClass,
    SliJurisdictionTransitionDecision Decision,
    IReadOnlyList<string> PreservedInvariantSet,
    IReadOnlyList<string> RequiredWitnesses,
    SliJurisdictionAuditDepth RequiredAuditDepth,
    SliJurisdictionOversightRequirement RequiredOversightRequirement,
    SliJurisdictionRetentionClass RequiredRetentionClass,
    IReadOnlyList<string> BlockingConditions,
    IReadOnlyList<string> NextActions,
    string ReasonCode,
    bool GovernanceEventRetained,
    bool IdentityFormingRetentionAllowed,
    IReadOnlyList<string> GovernanceEventReasonCodes);

public static class SliJurisdictionContracts
{
    public const string IdentityContinuityUnchangedInvariant = "identity-continuity-unchanged";
    public const string BondRealizationUnchangedInvariant = "bond-realization-unchanged";
    public const string BridgeLegalityRemainsPrimaryInvariant = "bridge-legality-remains-primary";
    public const string WitnessOnlyDoesNotAuthorizeInvariant = "witness-only-does-not-authorize";
    public const string RevealCeilingMayNotWidenWithoutExplicitAuthorityInvariant = "reveal-ceiling-may-not-widen-without-explicit-authority";
    public const string SubordinateCmeAuthorizationUnchangedInvariant = "subordinate-cme-authorization-unchanged-unless-separately-authorized";
    public const string RetentionBurdenMayOnlyHardenInvariant = "retention-burden-may-only-harden-on-stricter-surfaces";
    public const string NoLocalRuntimeInferenceOfPromotionInvariant = "no-local-runtime-inference-of-surface-promotion";

    public const string ReasonActualizedFirstBootFormed = "jurisdiction-actualized-first-boot-formed";
    public const string ReasonIndustrializedIngressCandidate = "jurisdiction-industrialized-ingress-candidate";
    public const string ReasonEnvelopePreformalized = "jurisdiction-envelope-preformalized";
    public const string ReasonEnvelopeUnreachableSurface = "jurisdiction-envelope-unreachable-surface";
    public const string ReasonEnvelopeWitnessOnly = "jurisdiction-envelope-witness-only";

    public const string ReasonTransitionActualizedToIndustrializedAllowed = "jurisdiction-transition-actualized-to-industrialized-allowed";
    public const string ReasonTransitionMissingOperatorFormation = "jurisdiction-transition-missing-operator-formation";
    public const string ReasonTransitionBridgeNotOk = "jurisdiction-transition-bridge-not-ok";
    public const string ReasonTransitionRuntimeNotCandidateOnly = "jurisdiction-transition-runtime-not-candidate-only";
    public const string ReasonTransitionIndustrializedToCivicAllowed = "jurisdiction-transition-industrialized-to-civic-allowed";
    public const string ReasonTransitionCivicRevealWideningRefused = "jurisdiction-transition-civic-reveal-widening-refused";
    public const string ReasonTransitionPrivateEvidenceMissing = "jurisdiction-transition-private-evidence-missing";
    public const string ReasonTransitionGovernmentOversightMissing = "jurisdiction-transition-government-oversight-missing";
    public const string ReasonTransitionSpecialPolicyMissing = "jurisdiction-transition-special-policy-missing";
    public const string ReasonTransitionUnlistedRefused = "jurisdiction-transition-unlisted-refused";
    public const string ReasonTransitionPrivateAllowed = "jurisdiction-transition-private-allowed";
    public const string ReasonTransitionGovernmentAllowed = "jurisdiction-transition-government-allowed";

    public const string ReasonGovernanceEventRecorded = "jurisdiction-governance-event-recorded";
    public const string ReasonWithdrawalGovernanceRetained = "jurisdiction-withdrawal-governance-retained";
    public const string ReasonHostilePayloadNotIdentityForming = "jurisdiction-hostile-payload-not-identity-forming";

    private static readonly string[] InitialInvariantSpineValue =
    [
        IdentityContinuityUnchangedInvariant,
        BondRealizationUnchangedInvariant,
        BridgeLegalityRemainsPrimaryInvariant,
        WitnessOnlyDoesNotAuthorizeInvariant,
        RevealCeilingMayNotWidenWithoutExplicitAuthorityInvariant,
        SubordinateCmeAuthorizationUnchangedInvariant,
        RetentionBurdenMayOnlyHardenInvariant,
        NoLocalRuntimeInferenceOfPromotionInvariant
    ];

    public static IReadOnlyList<string> InitialInvariantSpine => InitialInvariantSpineValue;

    public static SliJurisdictionEnvelopeReceipt ProjectFirstBootEnvelope(
        FirstBootGovernanceLayerReceipt governanceLayer)
    {
        ArgumentNullException.ThrowIfNull(governanceLayer);

        var reasonCode = governanceLayer.State == FirstBootGovernanceLayerState.RoleBoundEceReady &&
                         governanceLayer.RoleBoundEcesReady
            ? ReasonActualizedFirstBootFormed
            : ReasonEnvelopePreformalized;

        return CreateEnvelope(
            surfaceClass: SliJurisdictionSurfaceClass.Actualized,
            bootClass: governanceLayer.BootClass,
            sourceGovernanceLayerHandle: governanceLayer.LayerHandle,
            sourceFormationHandle: null,
            witnessOnly: true,
            bondRealizationClaimed: false,
            auditDepth: SliJurisdictionAuditDepth.Standard,
            oversightRequirement: SliJurisdictionOversightRequirement.StewardReview,
            retentionClass: SliJurisdictionRetentionClass.GovernanceEventOnly,
            revealModeCeiling: PrimeRevealMode.StructuralValidation,
            subordinateCmeAuthorizationAllowed: false,
            humanReviewRequired: true,
            reasonCode: reasonCode);
    }

    public static SliJurisdictionEnvelopeReceipt ProjectProtectedIngressEnvelope(
        FirstBootGovernanceLayerReceipt governanceLayer,
        SliBridgeReviewReceipt bridgeReview,
        SliRuntimeUseCeilingReceipt runtimeUseCeiling,
        SliOperatorFormationReceipt? operatorFormation)
    {
        ArgumentNullException.ThrowIfNull(governanceLayer);
        ArgumentNullException.ThrowIfNull(bridgeReview);
        ArgumentNullException.ThrowIfNull(runtimeUseCeiling);

        var sourceEnvelope = ProjectFirstBootEnvelope(governanceLayer);
        if (CanProjectIndustrialized(sourceEnvelope, bridgeReview, runtimeUseCeiling, operatorFormation))
        {
            return CreateEnvelope(
                surfaceClass: SliJurisdictionSurfaceClass.Industrialized,
                bootClass: governanceLayer.BootClass,
                sourceGovernanceLayerHandle: governanceLayer.LayerHandle,
                sourceFormationHandle: operatorFormation?.FormationHandle,
                witnessOnly: true,
                bondRealizationClaimed: false,
                auditDepth: SliJurisdictionAuditDepth.Deep,
                oversightRequirement: SliJurisdictionOversightRequirement.StewardReview,
                retentionClass: SliJurisdictionRetentionClass.ProtectedReviewLedger,
                revealModeCeiling: PrimeRevealMode.StructuralValidation,
                subordinateCmeAuthorizationAllowed: false,
                humanReviewRequired: true,
                reasonCode: ReasonIndustrializedIngressCandidate);
        }

        var fallbackReason = sourceEnvelope.ReasonCode == ReasonEnvelopePreformalized
            ? ReasonEnvelopePreformalized
            : ReasonEnvelopeWitnessOnly;

        return CreateEnvelope(
            surfaceClass: sourceEnvelope.SurfaceClass,
            bootClass: sourceEnvelope.BootClass,
            sourceGovernanceLayerHandle: sourceEnvelope.SourceGovernanceLayerHandle,
            sourceFormationHandle: operatorFormation?.FormationHandle ?? sourceEnvelope.SourceFormationHandle,
            witnessOnly: true,
            bondRealizationClaimed: false,
            auditDepth: sourceEnvelope.AuditDepth,
            oversightRequirement: sourceEnvelope.OversightRequirement,
            retentionClass: sourceEnvelope.RetentionClass,
            revealModeCeiling: sourceEnvelope.RevealModeCeiling,
            subordinateCmeAuthorizationAllowed: false,
            humanReviewRequired: sourceEnvelope.HumanReviewRequired,
            reasonCode: fallbackReason);
    }

    public static SliJurisdictionEnvelopeReceipt ProjectCivicEnvelope(
        SliJurisdictionEnvelopeReceipt sourceEnvelope,
        CommunityWeatherPacket communityWeatherPacket)
    {
        ArgumentNullException.ThrowIfNull(sourceEnvelope);
        ArgumentNullException.ThrowIfNull(communityWeatherPacket);

        if (sourceEnvelope.SurfaceClass != SliJurisdictionSurfaceClass.Industrialized ||
            communityWeatherPacket.VisibilityClass != CompassVisibilityClass.CommunityLegible)
        {
            return CreateUnreachableEnvelope(
                SliJurisdictionSurfaceClass.Civic,
                sourceEnvelope.BootClass,
                sourceEnvelope.SourceGovernanceLayerHandle,
                sourceEnvelope.SourceFormationHandle);
        }

        return CreateEnvelope(
            surfaceClass: SliJurisdictionSurfaceClass.Civic,
            bootClass: sourceEnvelope.BootClass,
            sourceGovernanceLayerHandle: sourceEnvelope.SourceGovernanceLayerHandle,
            sourceFormationHandle: sourceEnvelope.SourceFormationHandle,
            witnessOnly: true,
            bondRealizationClaimed: false,
            auditDepth: SliJurisdictionAuditDepth.Standard,
            oversightRequirement: SliJurisdictionOversightRequirement.StewardReview,
            retentionClass: SliJurisdictionRetentionClass.GovernanceEventOnly,
            revealModeCeiling: PrimeRevealMode.MaskedSummary,
            subordinateCmeAuthorizationAllowed: false,
            humanReviewRequired: true,
            reasonCode: ReasonEnvelopeWitnessOnly);
    }

    public static SliJurisdictionEnvelopeReceipt ProjectPrivateEnvelope(
        SliJurisdictionEnvelopeReceipt sourceEnvelope,
        string privateCustodyPartitionEvidence)
    {
        ArgumentNullException.ThrowIfNull(sourceEnvelope);

        if (sourceEnvelope.SurfaceClass != SliJurisdictionSurfaceClass.Industrialized ||
            string.IsNullOrWhiteSpace(privateCustodyPartitionEvidence))
        {
            return CreateUnreachableEnvelope(
                SliJurisdictionSurfaceClass.Private,
                sourceEnvelope.BootClass,
                sourceEnvelope.SourceGovernanceLayerHandle,
                sourceEnvelope.SourceFormationHandle);
        }

        return CreateEnvelope(
            surfaceClass: SliJurisdictionSurfaceClass.Private,
            bootClass: sourceEnvelope.BootClass,
            sourceGovernanceLayerHandle: sourceEnvelope.SourceGovernanceLayerHandle,
            sourceFormationHandle: privateCustodyPartitionEvidence.Trim(),
            witnessOnly: true,
            bondRealizationClaimed: false,
            auditDepth: SliJurisdictionAuditDepth.Deep,
            oversightRequirement: SliJurisdictionOversightRequirement.InstitutionalReview,
            retentionClass: SliJurisdictionRetentionClass.ProtectedReviewLedger,
            revealModeCeiling: PrimeRevealMode.StructuralValidation,
            subordinateCmeAuthorizationAllowed: false,
            humanReviewRequired: true,
            reasonCode: ReasonEnvelopeWitnessOnly);
    }

    public static SliJurisdictionEnvelopeReceipt ProjectGovernmentEnvelope(
        SliJurisdictionEnvelopeReceipt sourceEnvelope,
        string jurisdictionMappingHandle,
        string regulatedOversightHandle)
    {
        ArgumentNullException.ThrowIfNull(sourceEnvelope);

        if (sourceEnvelope.SurfaceClass != SliJurisdictionSurfaceClass.Industrialized ||
            string.IsNullOrWhiteSpace(jurisdictionMappingHandle) ||
            string.IsNullOrWhiteSpace(regulatedOversightHandle))
        {
            return CreateUnreachableEnvelope(
                SliJurisdictionSurfaceClass.Government,
                sourceEnvelope.BootClass,
                sourceEnvelope.SourceGovernanceLayerHandle,
                sourceEnvelope.SourceFormationHandle);
        }

        return CreateEnvelope(
            surfaceClass: SliJurisdictionSurfaceClass.Government,
            bootClass: sourceEnvelope.BootClass,
            sourceGovernanceLayerHandle: jurisdictionMappingHandle.Trim(),
            sourceFormationHandle: regulatedOversightHandle.Trim(),
            witnessOnly: true,
            bondRealizationClaimed: false,
            auditDepth: SliJurisdictionAuditDepth.Maximal,
            oversightRequirement: SliJurisdictionOversightRequirement.RegulatedHumanReview,
            retentionClass: SliJurisdictionRetentionClass.ComplianceRetention,
            revealModeCeiling: PrimeRevealMode.StructuralValidation,
            subordinateCmeAuthorizationAllowed: false,
            humanReviewRequired: true,
            reasonCode: ReasonEnvelopeWitnessOnly);
    }

    public static SliJurisdictionEnvelopeReceipt CreateUnreachableEnvelope(
        SliJurisdictionSurfaceClass surfaceClass,
        BootClass bootClass,
        string? sourceGovernanceLayerHandle = null,
        string? sourceFormationHandle = null)
    {
        var defaults = GetSurfaceDefaults(surfaceClass);
        return CreateEnvelope(
            surfaceClass: surfaceClass,
            bootClass: bootClass,
            sourceGovernanceLayerHandle: sourceGovernanceLayerHandle,
            sourceFormationHandle: sourceFormationHandle,
            witnessOnly: true,
            bondRealizationClaimed: false,
            auditDepth: defaults.AuditDepth,
            oversightRequirement: defaults.OversightRequirement,
            retentionClass: defaults.RetentionClass,
            revealModeCeiling: defaults.RevealModeCeiling,
            subordinateCmeAuthorizationAllowed: false,
            humanReviewRequired: defaults.HumanReviewRequired,
            reasonCode: ReasonEnvelopeUnreachableSurface);
    }

    public static SliJurisdictionTransitionReceipt EvaluateTransition(
        SliJurisdictionEnvelopeReceipt sourceEnvelope,
        SliJurisdictionSurfaceClass targetSurfaceClass,
        SliBridgeReviewReceipt? bridgeReview = null,
        SliRuntimeUseCeilingReceipt? runtimeUseCeiling = null,
        SliOperatorFormationReceipt? operatorFormation = null,
        CommunityWeatherPacket? communityWeatherPacket = null,
        string? privateCustodyPartitionEvidence = null,
        string? jurisdictionMappingHandle = null,
        string? regulatedOversightHandle = null,
        string? specialSurfacePolicyHandle = null)
    {
        ArgumentNullException.ThrowIfNull(sourceEnvelope);

        var targetDefaults = GetSurfaceDefaults(targetSurfaceClass);
        var requiredWitnesses = GetRequiredWitnesses(targetSurfaceClass);
        var blockingConditions = new List<string>();
        var nextActions = new List<string>();
        var decision = SliJurisdictionTransitionDecision.Refuse;
        var reasonCode = ReasonTransitionUnlistedRefused;

        if (sourceEnvelope.SurfaceClass == SliJurisdictionSurfaceClass.Actualized &&
            targetSurfaceClass == SliJurisdictionSurfaceClass.Industrialized)
        {
            if (bridgeReview?.OutcomeKind != SliBridgeOutcomeKind.Ok)
            {
                decision = SliJurisdictionTransitionDecision.Hold;
                reasonCode = ReasonTransitionBridgeNotOk;
                blockingConditions.Add("bridge-outcome-not-ok");
                nextActions.Add("Reestablish a lawful bridge outcome before deployment widening.");
            }
            else if (runtimeUseCeiling?.CandidateOnly != true)
            {
                decision = SliJurisdictionTransitionDecision.Hold;
                reasonCode = ReasonTransitionRuntimeNotCandidateOnly;
                blockingConditions.Add("runtime-use-ceiling-not-candidate-only");
                nextActions.Add("Reduce runtime authority to the candidate-only ceiling.");
            }
            else if (operatorFormation is null)
            {
                decision = SliJurisdictionTransitionDecision.Hold;
                reasonCode = ReasonTransitionMissingOperatorFormation;
                blockingConditions.Add("operator-formation-missing");
                nextActions.Add("Attach a pre-bond operator-formation receipt before projection.");
            }
            else
            {
                decision = SliJurisdictionTransitionDecision.Allow;
                reasonCode = ReasonTransitionActualizedToIndustrializedAllowed;
            }

            return CreateTransition(
                sourceEnvelope,
                targetSurfaceClass,
                decision,
                requiredWitnesses,
                targetDefaults.AuditDepth,
                targetDefaults.OversightRequirement,
                targetDefaults.RetentionClass,
                blockingConditions,
                nextActions,
                reasonCode);
        }

        if (sourceEnvelope.SurfaceClass == SliJurisdictionSurfaceClass.Industrialized &&
            targetSurfaceClass == SliJurisdictionSurfaceClass.Civic)
        {
            if (communityWeatherPacket is not null &&
                communityWeatherPacket.VisibilityClass == CompassVisibilityClass.CommunityLegible)
            {
                decision = SliJurisdictionTransitionDecision.Allow;
                reasonCode = ReasonTransitionIndustrializedToCivicAllowed;
            }
            else
            {
                decision = SliJurisdictionTransitionDecision.Refuse;
                reasonCode = ReasonTransitionCivicRevealWideningRefused;
                blockingConditions.Add("civic-safe-reduction-missing");
                nextActions.Add("Route through the community-safe weather reduction path before civic projection.");
            }

            return CreateTransition(
                sourceEnvelope,
                targetSurfaceClass,
                decision,
                requiredWitnesses,
                targetDefaults.AuditDepth,
                targetDefaults.OversightRequirement,
                targetDefaults.RetentionClass,
                blockingConditions,
                nextActions,
                reasonCode);
        }

        if (sourceEnvelope.SurfaceClass == SliJurisdictionSurfaceClass.Industrialized &&
            targetSurfaceClass == SliJurisdictionSurfaceClass.Private)
        {
            if (string.IsNullOrWhiteSpace(privateCustodyPartitionEvidence))
            {
                decision = SliJurisdictionTransitionDecision.Hold;
                reasonCode = ReasonTransitionPrivateEvidenceMissing;
                blockingConditions.Add("private-custody-disclosure-partition-missing");
                nextActions.Add("Attach explicit private custody and disclosure partition evidence.");
            }
            else
            {
                decision = SliJurisdictionTransitionDecision.Allow;
                reasonCode = ReasonTransitionPrivateAllowed;
            }

            return CreateTransition(
                sourceEnvelope,
                targetSurfaceClass,
                decision,
                requiredWitnesses,
                targetDefaults.AuditDepth,
                targetDefaults.OversightRequirement,
                targetDefaults.RetentionClass,
                blockingConditions,
                nextActions,
                reasonCode);
        }

        if (sourceEnvelope.SurfaceClass == SliJurisdictionSurfaceClass.Industrialized &&
            targetSurfaceClass == SliJurisdictionSurfaceClass.Government)
        {
            if (string.IsNullOrWhiteSpace(jurisdictionMappingHandle) ||
                string.IsNullOrWhiteSpace(regulatedOversightHandle))
            {
                decision = SliJurisdictionTransitionDecision.Hold;
                reasonCode = ReasonTransitionGovernmentOversightMissing;
                blockingConditions.Add("regulated-oversight-or-jurisdiction-mapping-missing");
                nextActions.Add("Attach explicit jurisdiction mapping and regulated oversight evidence.");
            }
            else
            {
                decision = SliJurisdictionTransitionDecision.Allow;
                reasonCode = ReasonTransitionGovernmentAllowed;
            }

            return CreateTransition(
                sourceEnvelope,
                targetSurfaceClass,
                decision,
                requiredWitnesses,
                targetDefaults.AuditDepth,
                targetDefaults.OversightRequirement,
                targetDefaults.RetentionClass,
                blockingConditions,
                nextActions,
                reasonCode);
        }

        if (targetSurfaceClass == SliJurisdictionSurfaceClass.Special)
        {
            decision = SliJurisdictionTransitionDecision.Refuse;
            reasonCode = string.IsNullOrWhiteSpace(specialSurfacePolicyHandle)
                ? ReasonTransitionSpecialPolicyMissing
                : ReasonTransitionUnlistedRefused;
            blockingConditions.Add("special-surface-policy-unavailable");
            nextActions.Add("Provide a dedicated special-surface policy object before retrying.");

            return CreateTransition(
                sourceEnvelope,
                targetSurfaceClass,
                decision,
                requiredWitnesses,
                targetDefaults.AuditDepth,
                targetDefaults.OversightRequirement,
                targetDefaults.RetentionClass,
                blockingConditions,
                nextActions,
                reasonCode);
        }

        return CreateTransition(
            sourceEnvelope,
            targetSurfaceClass,
            SliJurisdictionTransitionDecision.Refuse,
            requiredWitnesses,
            targetDefaults.AuditDepth,
            targetDefaults.OversightRequirement,
            targetDefaults.RetentionClass,
            ["transition-not-listed"],
            ["Use an explicitly supported jurisdiction transition path."],
            ReasonTransitionUnlistedRefused);
    }

    private static bool CanProjectIndustrialized(
        SliJurisdictionEnvelopeReceipt sourceEnvelope,
        SliBridgeReviewReceipt bridgeReview,
        SliRuntimeUseCeilingReceipt runtimeUseCeiling,
        SliOperatorFormationReceipt? operatorFormation)
    {
        return sourceEnvelope.SurfaceClass == SliJurisdictionSurfaceClass.Actualized &&
               bridgeReview.OutcomeKind == SliBridgeOutcomeKind.Ok &&
               !SliBridgeContracts.HasBlockingPreBondSafeguard(bridgeReview) &&
               runtimeUseCeiling.CandidateOnly &&
               operatorFormation is not null;
    }

    private static SliJurisdictionEnvelopeReceipt CreateEnvelope(
        SliJurisdictionSurfaceClass surfaceClass,
        BootClass bootClass,
        string? sourceGovernanceLayerHandle,
        string? sourceFormationHandle,
        bool witnessOnly,
        bool bondRealizationClaimed,
        SliJurisdictionAuditDepth auditDepth,
        SliJurisdictionOversightRequirement oversightRequirement,
        SliJurisdictionRetentionClass retentionClass,
        PrimeRevealMode revealModeCeiling,
        bool subordinateCmeAuthorizationAllowed,
        bool humanReviewRequired,
        string reasonCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        var envelopeHandle = CreateEnvelopeHandle(
            surfaceClass,
            bootClass,
            sourceGovernanceLayerHandle,
            sourceFormationHandle,
            reasonCode);

        return new SliJurisdictionEnvelopeReceipt(
            EnvelopeHandle: envelopeHandle,
            SurfaceClass: surfaceClass,
            BootClass: bootClass,
            SourceGovernanceLayerHandle: sourceGovernanceLayerHandle,
            SourceFormationHandle: sourceFormationHandle,
            WitnessOnly: witnessOnly,
            BondRealizationClaimed: bondRealizationClaimed,
            AuditDepth: auditDepth,
            OversightRequirement: oversightRequirement,
            RetentionClass: retentionClass,
            RevealModeCeiling: revealModeCeiling,
            SubordinateCmeAuthorizationAllowed: subordinateCmeAuthorizationAllowed,
            HumanReviewRequired: humanReviewRequired,
            ReasonCode: reasonCode.Trim());
    }

    private static SliJurisdictionTransitionReceipt CreateTransition(
        SliJurisdictionEnvelopeReceipt sourceEnvelope,
        SliJurisdictionSurfaceClass targetSurfaceClass,
        SliJurisdictionTransitionDecision decision,
        IReadOnlyList<string> requiredWitnesses,
        SliJurisdictionAuditDepth requiredAuditDepth,
        SliJurisdictionOversightRequirement requiredOversightRequirement,
        SliJurisdictionRetentionClass requiredRetentionClass,
        IReadOnlyList<string> blockingConditions,
        IReadOnlyList<string> nextActions,
        string reasonCode)
    {
        ArgumentNullException.ThrowIfNull(sourceEnvelope);
        ArgumentNullException.ThrowIfNull(requiredWitnesses);
        ArgumentNullException.ThrowIfNull(blockingConditions);
        ArgumentNullException.ThrowIfNull(nextActions);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        var governanceEventRetained = decision != SliJurisdictionTransitionDecision.Allow;
        var governanceEventReasonCodes = governanceEventRetained
            ? new[]
            {
                ReasonGovernanceEventRecorded,
                ReasonWithdrawalGovernanceRetained,
                ReasonHostilePayloadNotIdentityForming
            }
            : Array.Empty<string>();

        var transitionHandle = CreateTransitionHandle(
            sourceEnvelope.EnvelopeHandle,
            sourceEnvelope.SurfaceClass,
            targetSurfaceClass,
            reasonCode);

        return new SliJurisdictionTransitionReceipt(
            TransitionHandle: transitionHandle,
            SourceEnvelopeHandle: sourceEnvelope.EnvelopeHandle,
            SourceSurfaceClass: sourceEnvelope.SurfaceClass,
            TargetSurfaceClass: targetSurfaceClass,
            Decision: decision,
            PreservedInvariantSet: InitialInvariantSpineValue,
            RequiredWitnesses: requiredWitnesses.ToArray(),
            RequiredAuditDepth: requiredAuditDepth,
            RequiredOversightRequirement: requiredOversightRequirement,
            RequiredRetentionClass: requiredRetentionClass,
            BlockingConditions: blockingConditions.ToArray(),
            NextActions: nextActions.ToArray(),
            ReasonCode: reasonCode.Trim(),
            GovernanceEventRetained: governanceEventRetained,
            IdentityFormingRetentionAllowed: false,
            GovernanceEventReasonCodes: governanceEventReasonCodes);
    }

    private static (SliJurisdictionAuditDepth AuditDepth,
        SliJurisdictionOversightRequirement OversightRequirement,
        SliJurisdictionRetentionClass RetentionClass,
        PrimeRevealMode RevealModeCeiling,
        bool HumanReviewRequired) GetSurfaceDefaults(SliJurisdictionSurfaceClass surfaceClass)
    {
        return surfaceClass switch
        {
            SliJurisdictionSurfaceClass.Actualized => (
                SliJurisdictionAuditDepth.Standard,
                SliJurisdictionOversightRequirement.StewardReview,
                SliJurisdictionRetentionClass.GovernanceEventOnly,
                PrimeRevealMode.StructuralValidation,
                true),
            SliJurisdictionSurfaceClass.Industrialized => (
                SliJurisdictionAuditDepth.Deep,
                SliJurisdictionOversightRequirement.StewardReview,
                SliJurisdictionRetentionClass.ProtectedReviewLedger,
                PrimeRevealMode.StructuralValidation,
                true),
            SliJurisdictionSurfaceClass.Civic => (
                SliJurisdictionAuditDepth.Standard,
                SliJurisdictionOversightRequirement.StewardReview,
                SliJurisdictionRetentionClass.GovernanceEventOnly,
                PrimeRevealMode.MaskedSummary,
                true),
            SliJurisdictionSurfaceClass.Private => (
                SliJurisdictionAuditDepth.Deep,
                SliJurisdictionOversightRequirement.InstitutionalReview,
                SliJurisdictionRetentionClass.ProtectedReviewLedger,
                PrimeRevealMode.StructuralValidation,
                true),
            SliJurisdictionSurfaceClass.Government => (
                SliJurisdictionAuditDepth.Maximal,
                SliJurisdictionOversightRequirement.RegulatedHumanReview,
                SliJurisdictionRetentionClass.ComplianceRetention,
                PrimeRevealMode.StructuralValidation,
                true),
            _ => (
                SliJurisdictionAuditDepth.Maximal,
                SliJurisdictionOversightRequirement.RegulatedHumanReview,
                SliJurisdictionRetentionClass.ComplianceRetention,
                PrimeRevealMode.None,
                true)
        };
    }

    private static IReadOnlyList<string> GetRequiredWitnesses(SliJurisdictionSurfaceClass targetSurfaceClass)
    {
        return targetSurfaceClass switch
        {
            SliJurisdictionSurfaceClass.Industrialized =>
            [
                "bridge-review",
                "runtime-use-ceiling",
                "operator-formation",
                "jurisdiction-envelope"
            ],
            SliJurisdictionSurfaceClass.Civic =>
            [
                "community-weather-packet"
            ],
            SliJurisdictionSurfaceClass.Private =>
            [
                "private-custody-disclosure-partition"
            ],
            SliJurisdictionSurfaceClass.Government =>
            [
                "jurisdiction-mapping",
                "regulated-oversight"
            ],
            SliJurisdictionSurfaceClass.Special =>
            [
                "special-surface-policy"
            ],
            _ => Array.Empty<string>()
        };
    }

    private static string CreateEnvelopeHandle(
        SliJurisdictionSurfaceClass surfaceClass,
        BootClass bootClass,
        string? sourceGovernanceLayerHandle,
        string? sourceFormationHandle,
        string reasonCode)
    {
        var material = string.Join(
            "|",
            surfaceClass,
            bootClass,
            sourceGovernanceLayerHandle ?? "none",
            sourceFormationHandle ?? "none",
            reasonCode.Trim());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"jurisdiction-envelope://{surfaceClass.ToString().ToLowerInvariant()}/{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private static string CreateTransitionHandle(
        string sourceEnvelopeHandle,
        SliJurisdictionSurfaceClass sourceSurfaceClass,
        SliJurisdictionSurfaceClass targetSurfaceClass,
        string reasonCode)
    {
        var material = string.Join(
            "|",
            sourceEnvelopeHandle.Trim(),
            sourceSurfaceClass,
            targetSurfaceClass,
            reasonCode.Trim());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"jurisdiction-transition://{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
