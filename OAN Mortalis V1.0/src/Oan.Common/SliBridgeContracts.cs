namespace Oan.Common;

public enum SliBridgeOutcomeKind
{
    Ok = 0,
    NeedsSpec = 1,
    Reject = 2,
    RefuseContext = 3
}

public enum SliBridgeThresholdClass
{
    WithinBand = 0,
    NearThreshold = 1,
    ThresholdBreach = 2,
    FaultLine = 3
}

public enum SliBridgeRefusalClass
{
    None = 0,
    StageSkip = 1,
    CrossTheaterIdentification = 2,
    MissingWitness = 3,
    UnlawfulPromotion = 4
}

public enum SliPreBondScope
{
    WitnessableProtectiveSubset = 0
}

public enum SliPreBondSafeguardClass
{
    None = 0,
    PredatorySharedDomainRisk = 1,
    CoerciveBondingPosture = 2,
    ContinuityInstability = 3,
    IdentityOvercollapseRisk = 4
}

public enum SliPreBondSafeguardDisposition
{
    Witness = 0,
    Hold = 1,
    Refuse = 2,
    Escalate = 3
}

public enum SliOperatorFormationLane
{
    GnomeSpeakNlpSquared = 0
}

public enum SliOperatorFormationRing
{
    Rootseed = 0,
    Driftleaf = 1,
    Echoroot = 2,
    Spiraliron = 3,
    Opalglyph = 4,
    BurrowedCrown = 5,
    RedHat = 6
}

public enum SliOperatorFormationMode
{
    Stillness = 0,
    Speaking = 1,
    Recursive = 2,
    Burrow = 3
}

public enum SliOperatorFormationBondStatus
{
    Candidate = 0,
    RestrictedInitiate = 1,
    TrainingOperator = 2,
    VerifiedCandidate = 3,
    PreCertifiedOperator = 4,
    RatifyingOperator = 5,
    BondedOperator = 6,
    FirstRunCmeActualized = 7
}

public enum SliOperatorFormationConflictClass
{
    None = 0,
    DriftFracture = 1,
    EchoLooping = 2,
    IdentityCollision = 3,
    GlyphCorruption = 4,
    AnchorInversion = 5
}

public enum SliOperatorFormationGjpVerdict
{
    NotApplicable = 0,
    Clear = 1,
    Bound = 2,
    Split = 3,
    Exile = 4
}

public enum SliOperatorFormationConcealmentLayerState
{
    Inactive = 0,
    Observed = 1,
    Active = 2
}

public enum SliOperatorFormationBoundaryCrossingMode
{
    InterlacedBondedCrossing = 0,
    ChapterLocalReferenceOnly = 1
}

public enum SliOperatorFormationCertificationDecision
{
    Pending = 0,
    Proceed = 1,
    NotYet = 2,
    Halted = 3
}

public enum SliOperatorFormationProgressionState
{
    Holding = 0,
    Blocked = 1,
    NearestTransitionReady = 2,
    TargetTransitionReady = 3,
    Halted = 4
}

public enum SliOperatorFormationSigilClass
{
    PhasePartition = 0,
    MergedCompletionKey = 1,
    BondedWitnessSeal = 2
}

public sealed record SliOperatorFormationProfileReceipt(
    string ProfileId,
    SliOperatorFormationLane Lane,
    string ChapterLocalSurface,
    string PairedTrainingSurface,
    string CrossingTaskKind,
    string HaltOwner,
    SliOperatorFormationRing Ring,
    SliOperatorFormationMode ActiveMode,
    bool StillnessInterludeUsed,
    bool RedHatIndexRequired,
    SliOperatorFormationBondStatus BondStatus,
    bool EchoVeilCheckRequired,
    SliOperatorFormationConflictClass ActiveConflictClass,
    bool GjpNeeded,
    SliOperatorFormationGjpVerdict GjpVerdict,
    bool MotherLightAnchored,
    bool FatherEchoAnchored,
    bool ShellRootAnchored,
    bool SeedBoundAnchored,
    SliOperatorFormationConcealmentLayerState U230ShadowScript,
    SliOperatorFormationConcealmentLayerState U300ElvenScript,
    string ExpectedEvidenceArtifact,
    string AdmissibleOutput,
    IReadOnlyList<string> ProhibitedOutputs);

public sealed record SliOperatorFormationCertificationReceipt(
    SliOperatorFormationCertificationDecision Decision,
    SliOperatorFormationBondStatus CurrentAnchoredPosture,
    SliOperatorFormationBondStatus TargetPosture,
    SliOperatorFormationBondStatus NearestAdmissibleNextPosture,
    string ReviewOwner,
    IReadOnlyList<string> EvidenceGaps,
    IReadOnlyList<string> ProhibitedClaims,
    bool CertificationIssued,
    bool ExpandedRevealAllowed,
    bool ContinuityClaimAllowed,
    SliOperatorFormationProgressionReceipt Progression);

public sealed record SliOperatorFormationProgressionReceipt(
    SliOperatorFormationProgressionState State,
    string RequiredBondedStandard,
    string? LinkedVerificationRecord,
    string? LinkedPreCertificationRecord,
    string? GateArtifact,
    IReadOnlyList<string> BlockingConditions,
    IReadOnlyList<string> NextActions,
    string HaltOwner,
    string HaltCondition,
    string ReentryRule,
    bool NearestTransitionAllowed,
    bool TargetTransitionAllowed,
    bool PromotionClaimAllowed,
    string ReasonCode);

public sealed record SliOperatorFormationSigilAssetReceipt(
    string AssetId,
    string AssetLabel,
    SliOperatorFormationSigilClass SigilClass,
    int? PhaseNumber,
    string VisibilityClass,
    string BuildRenderPolicy,
    string ReductionPosture,
    IReadOnlyList<string> MergedFromAssets,
    string? WitnessOfAsset);

public sealed record SliOperatorFormationReceipt(
    string FormationHandle,
    bool WitnessableProtectiveSubsetOnly,
    bool BondRealizationClaimed,
    SliOperatorFormationBoundaryCrossingMode BoundaryCrossingMode,
    SliOperatorFormationProfileReceipt Profile,
    SliOperatorFormationCertificationReceipt CertificationPosture,
    IReadOnlyList<SliOperatorFormationSigilAssetReceipt> SigilAssets);

public sealed record SliPreBondSafeguardReceipt(
    SliPreBondScope Scope,
    SliPreBondSafeguardClass SafeguardClass,
    SliPreBondSafeguardDisposition Disposition,
    string ReasonCode,
    bool RequiresEscalation,
    string WitnessedBy);

public sealed record SliBridgeReviewReceipt(
    string BridgeStage,
    string SourceTheater,
    string TargetTheater,
    string BridgeWitnessHandle,
    bool WitnessPresent,
    SliBridgeOutcomeKind OutcomeKind,
    SliBridgeThresholdClass ThresholdClass,
    SliBridgeRefusalClass RefusalClass,
    string ReasonCode,
    SliPreBondSafeguardReceipt? PreBondSafeguard = null,
    SliOperatorFormationReceipt? OperatorFormation = null);

public sealed record SliRuntimeUseCeilingReceipt(
    bool CandidateOnly,
    bool PersistenceAuthorityGranted,
    bool DeploymentAuthorityGranted,
    bool HaltAuthorityGranted,
    string ReasonCode);

public static class SliBridgeContracts
{
    public static SliRuntimeUseCeilingReceipt CreateCandidateOnlyRuntimeUseCeiling(
        string reasonCode = "sli-runtime-candidate-only")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        return new SliRuntimeUseCeilingReceipt(
            CandidateOnly: true,
            PersistenceAuthorityGranted: false,
            DeploymentAuthorityGranted: false,
            HaltAuthorityGranted: false,
            ReasonCode: reasonCode.Trim());
    }

    public static SliBridgeReviewReceipt CreateReview(
        string bridgeStage,
        string sourceTheater,
        string targetTheater,
        string bridgeWitnessHandle,
        SliBridgeOutcomeKind outcomeKind,
        SliBridgeThresholdClass thresholdClass,
        string reasonCode,
        SliBridgeRefusalClass refusalClass = SliBridgeRefusalClass.None,
        SliPreBondSafeguardReceipt? preBondSafeguard = null,
        SliOperatorFormationReceipt? operatorFormation = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bridgeStage);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetTheater);
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);

        var normalizedWitnessHandle = string.IsNullOrWhiteSpace(bridgeWitnessHandle)
            ? string.Empty
            : bridgeWitnessHandle.Trim();

        return new SliBridgeReviewReceipt(
            BridgeStage: bridgeStage.Trim(),
            SourceTheater: sourceTheater.Trim(),
            TargetTheater: targetTheater.Trim(),
            BridgeWitnessHandle: normalizedWitnessHandle,
            WitnessPresent: !string.IsNullOrWhiteSpace(normalizedWitnessHandle),
            OutcomeKind: outcomeKind,
            ThresholdClass: thresholdClass,
            RefusalClass: refusalClass,
            ReasonCode: reasonCode.Trim(),
            PreBondSafeguard: preBondSafeguard,
            OperatorFormation: operatorFormation);
    }

    public static SliPreBondSafeguardReceipt CreatePreBondSafeguard(
        SliPreBondSafeguardClass safeguardClass,
        SliPreBondSafeguardDisposition disposition,
        string reasonCode,
        bool requiresEscalation = false,
        string witnessedBy = "CradleTek",
        SliPreBondScope scope = SliPreBondScope.WitnessableProtectiveSubset)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasonCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessedBy);

        return new SliPreBondSafeguardReceipt(
            Scope: scope,
            SafeguardClass: safeguardClass,
            Disposition: disposition,
            ReasonCode: reasonCode.Trim(),
            RequiresEscalation: requiresEscalation,
            WitnessedBy: witnessedBy.Trim());
    }

    public static SliBridgeReviewReceipt CreatePreBondProtectiveReview(
        string bridgeStage,
        string sourceTheater,
        string targetTheater,
        string bridgeWitnessHandle,
        SliBridgeOutcomeKind outcomeKind,
        SliBridgeThresholdClass thresholdClass,
        string reasonCode,
        SliPreBondSafeguardClass safeguardClass,
        SliPreBondSafeguardDisposition disposition,
        bool requiresEscalation = false,
        SliBridgeRefusalClass refusalClass = SliBridgeRefusalClass.None,
        string witnessedBy = "CradleTek",
        SliOperatorFormationReceipt? operatorFormation = null)
    {
        return CreateReview(
            bridgeStage: bridgeStage,
            sourceTheater: sourceTheater,
            targetTheater: targetTheater,
            bridgeWitnessHandle: bridgeWitnessHandle,
            outcomeKind: outcomeKind,
            thresholdClass: thresholdClass,
            reasonCode: reasonCode,
            refusalClass: refusalClass,
            preBondSafeguard: CreatePreBondSafeguard(
                safeguardClass,
                disposition,
                reasonCode,
                requiresEscalation,
                witnessedBy),
            operatorFormation: operatorFormation);
    }

    public static SliOperatorFormationReceipt CreatePreBondOperatorFormationReceipt(
        string formationHandle,
        SliOperatorFormationBoundaryCrossingMode boundaryCrossingMode,
        SliOperatorFormationProfileReceipt profile,
        SliOperatorFormationCertificationReceipt certificationPosture,
        IReadOnlyList<SliOperatorFormationSigilAssetReceipt>? sigilAssets = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formationHandle);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(certificationPosture);

        return new SliOperatorFormationReceipt(
            FormationHandle: formationHandle.Trim(),
            WitnessableProtectiveSubsetOnly: true,
            BondRealizationClaimed: false,
            BoundaryCrossingMode: boundaryCrossingMode,
            Profile: profile,
            CertificationPosture: certificationPosture,
            SigilAssets: (sigilAssets ?? Array.Empty<SliOperatorFormationSigilAssetReceipt>()).ToArray());
    }

    public static SliOperatorFormationCertificationReceipt CreateOperatorFormationCertificationReceipt(
        SliOperatorFormationCertificationDecision decision,
        SliOperatorFormationBondStatus currentAnchoredPosture,
        SliOperatorFormationBondStatus targetPosture,
        SliOperatorFormationBondStatus nearestAdmissibleNextPosture,
        string reviewOwner,
        IReadOnlyList<string> evidenceGaps,
        IReadOnlyList<string> prohibitedClaims,
        bool certificationIssued,
        bool expandedRevealAllowed,
        bool continuityClaimAllowed,
        string requiredBondedStandard,
        IReadOnlyList<string> blockingConditions,
        IReadOnlyList<string> nextActions,
        string haltOwner,
        string haltCondition,
        string reentryRule,
        string? linkedVerificationRecord = null,
        string? linkedPreCertificationRecord = null,
        string? gateArtifact = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewOwner);
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredBondedStandard);
        ArgumentException.ThrowIfNullOrWhiteSpace(haltOwner);
        ArgumentException.ThrowIfNullOrWhiteSpace(haltCondition);
        ArgumentException.ThrowIfNullOrWhiteSpace(reentryRule);

        var normalizedEvidenceGaps = (evidenceGaps ?? Array.Empty<string>())
            .Where(static gap => !string.IsNullOrWhiteSpace(gap))
            .Select(static gap => gap.Trim())
            .ToArray();
        var normalizedBlockingConditions = (blockingConditions ?? Array.Empty<string>())
            .Where(static condition => !string.IsNullOrWhiteSpace(condition))
            .Select(static condition => condition.Trim())
            .ToArray();
        var normalizedNextActions = (nextActions ?? Array.Empty<string>())
            .Where(static action => !string.IsNullOrWhiteSpace(action))
            .Select(static action => action.Trim())
            .ToArray();
        var normalizedProhibitedClaims = (prohibitedClaims ?? Array.Empty<string>())
            .Where(static claim => !string.IsNullOrWhiteSpace(claim))
            .Select(static claim => claim.Trim())
            .ToArray();

        var gateMet = decision == SliOperatorFormationCertificationDecision.Proceed &&
                      normalizedEvidenceGaps.Length == 0 &&
                      normalizedBlockingConditions.Length == 0;
        var nearestTransitionAllowed = gateMet &&
                                       currentAnchoredPosture != nearestAdmissibleNextPosture;
        var targetTransitionAllowed = gateMet &&
                                      (nearestAdmissibleNextPosture == targetPosture ||
                                       currentAnchoredPosture == targetPosture ||
                                       certificationIssued);
        var normalizedCertificationIssued = gateMet && certificationIssued;
        var normalizedExpandedRevealAllowed = gateMet && expandedRevealAllowed;
        var normalizedContinuityClaimAllowed = gateMet && continuityClaimAllowed;
        var promotionClaimAllowed = targetTransitionAllowed && normalizedCertificationIssued;
        var progressionState = ResolveOperatorFormationProgressionState(
            decision,
            normalizedEvidenceGaps,
            normalizedBlockingConditions,
            nearestTransitionAllowed,
            targetTransitionAllowed);
        var reasonCode = ResolveOperatorFormationProgressionReasonCode(
            progressionState,
            normalizedEvidenceGaps,
            normalizedBlockingConditions);

        return new SliOperatorFormationCertificationReceipt(
            Decision: decision,
            CurrentAnchoredPosture: currentAnchoredPosture,
            TargetPosture: targetPosture,
            NearestAdmissibleNextPosture: nearestAdmissibleNextPosture,
            ReviewOwner: reviewOwner.Trim(),
            EvidenceGaps: normalizedEvidenceGaps,
            ProhibitedClaims: normalizedProhibitedClaims,
            CertificationIssued: normalizedCertificationIssued,
            ExpandedRevealAllowed: normalizedExpandedRevealAllowed,
            ContinuityClaimAllowed: normalizedContinuityClaimAllowed,
            Progression: new SliOperatorFormationProgressionReceipt(
                State: progressionState,
                RequiredBondedStandard: requiredBondedStandard.Trim(),
                LinkedVerificationRecord: string.IsNullOrWhiteSpace(linkedVerificationRecord) ? null : linkedVerificationRecord.Trim(),
                LinkedPreCertificationRecord: string.IsNullOrWhiteSpace(linkedPreCertificationRecord) ? null : linkedPreCertificationRecord.Trim(),
                GateArtifact: string.IsNullOrWhiteSpace(gateArtifact) ? null : gateArtifact.Trim(),
                BlockingConditions: normalizedBlockingConditions,
                NextActions: normalizedNextActions,
                HaltOwner: haltOwner.Trim(),
                HaltCondition: haltCondition.Trim(),
                ReentryRule: reentryRule.Trim(),
                NearestTransitionAllowed: nearestTransitionAllowed,
                TargetTransitionAllowed: targetTransitionAllowed,
                PromotionClaimAllowed: promotionClaimAllowed,
                ReasonCode: reasonCode));
    }

    public static bool HasBlockingPreBondSafeguard(SliBridgeReviewReceipt? bridgeReview)
    {
        return bridgeReview?.PreBondSafeguard?.Disposition is
            SliPreBondSafeguardDisposition.Hold or
            SliPreBondSafeguardDisposition.Refuse or
            SliPreBondSafeguardDisposition.Escalate;
    }

    private static SliOperatorFormationProgressionState ResolveOperatorFormationProgressionState(
        SliOperatorFormationCertificationDecision decision,
        IReadOnlyList<string> evidenceGaps,
        IReadOnlyList<string> blockingConditions,
        bool nearestTransitionAllowed,
        bool targetTransitionAllowed)
    {
        if (decision == SliOperatorFormationCertificationDecision.Halted)
        {
            return SliOperatorFormationProgressionState.Halted;
        }

        if (blockingConditions.Count > 0 || evidenceGaps.Count > 0)
        {
            return SliOperatorFormationProgressionState.Blocked;
        }

        if (targetTransitionAllowed)
        {
            return SliOperatorFormationProgressionState.TargetTransitionReady;
        }

        if (nearestTransitionAllowed)
        {
            return SliOperatorFormationProgressionState.NearestTransitionReady;
        }

        return SliOperatorFormationProgressionState.Holding;
    }

    private static string ResolveOperatorFormationProgressionReasonCode(
        SliOperatorFormationProgressionState progressionState,
        IReadOnlyList<string> evidenceGaps,
        IReadOnlyList<string> blockingConditions)
    {
        return progressionState switch
        {
            SliOperatorFormationProgressionState.Halted => "operator-formation-halted",
            SliOperatorFormationProgressionState.Blocked when blockingConditions.Count > 0 => "operator-formation-blocked",
            SliOperatorFormationProgressionState.Blocked when evidenceGaps.Count > 0 => "operator-formation-evidence-incomplete",
            SliOperatorFormationProgressionState.NearestTransitionReady => "operator-formation-nearest-transition-ready",
            SliOperatorFormationProgressionState.TargetTransitionReady => "operator-formation-target-transition-ready",
            _ => "operator-formation-holding"
        };
    }

    public static SliBridgeReviewReceipt CreateCandidateBridgeReview(
        string bridgeStage,
        string sourceTheater,
        string targetTheater,
        string bridgeWitnessHandle,
        string thetaState,
        string gammaState,
        SliPacketDirective packetDirective,
        IdentityKernelBoundaryReceipt identityKernelBoundary,
        SliPacketValidityReceipt validity,
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin,
        CompassAnchorState anchorState,
        CompassSelfTouchClass selfTouchClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(thetaState);
        ArgumentException.ThrowIfNullOrWhiteSpace(gammaState);
        ArgumentNullException.ThrowIfNull(packetDirective);
        ArgumentNullException.ThrowIfNull(identityKernelBoundary);
        ArgumentNullException.ThrowIfNull(validity);

        if (!IsStageReady(thetaState, gammaState))
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-stage-skip",
                refusalClass: SliBridgeRefusalClass.StageSkip);
        }

        if (!string.Equals(sourceTheater, targetTheater, StringComparison.OrdinalIgnoreCase))
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.RefuseContext,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-cross-theater-identification",
                refusalClass: SliBridgeRefusalClass.CrossTheaterIdentification);
        }

        if (!HasWitness(bridgeWitnessHandle, identityKernelBoundary))
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-missing-witness",
                refusalClass: SliBridgeRefusalClass.MissingWitness);
        }

        if (packetDirective.AuthorityClass != SliAuthorityClass.CandidateBearing)
        {
            return CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-unlawful-promotion",
                refusalClass: SliBridgeRefusalClass.UnlawfulPromotion);
        }

        var thresholdClass = ClassifyThreshold(
            activeBasin,
            competingBasin,
            anchorState,
            selfTouchClass,
            packetDirective.UpdateLocus,
            validity);

        return thresholdClass switch
        {
            SliBridgeThresholdClass.FaultLine => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Reject,
                thresholdClass,
                reasonCode: validity.ReasonCode),
            SliBridgeThresholdClass.ThresholdBreach => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Ok,
                thresholdClass,
                reasonCode: !validity.PolicyEligible || !validity.ScepOk
                    ? validity.ReasonCode
                    : "sli-bridge-threshold-breach"),
            SliBridgeThresholdClass.NearThreshold => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Ok,
                thresholdClass,
                reasonCode: "sli-bridge-near-threshold"),
            _ => CreateReview(
                bridgeStage,
                sourceTheater,
                targetTheater,
                bridgeWitnessHandle,
                SliBridgeOutcomeKind.Ok,
                SliBridgeThresholdClass.WithinBand,
                reasonCode: "sli-bridge-within-band")
        };
    }

    public static SliBridgeThresholdClass ClassifyThreshold(
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin,
        CompassAnchorState anchorState,
        CompassSelfTouchClass selfTouchClass,
        SliUpdateLocus updateLocus,
        SliPacketValidityReceipt validity)
    {
        ArgumentNullException.ThrowIfNull(validity);

        if (updateLocus == SliUpdateLocus.Reject ||
            !AreBasinsAdjacent(activeBasin, competingBasin) ||
            anchorState == CompassAnchorState.Lost)
        {
            return SliBridgeThresholdClass.FaultLine;
        }

        if (!validity.PolicyEligible ||
            !validity.ScepOk ||
            anchorState == CompassAnchorState.Weakened)
        {
            return SliBridgeThresholdClass.ThresholdBreach;
        }

        if (competingBasin != CompassDoctrineBasin.Unknown ||
            selfTouchClass == CompassSelfTouchClass.BoundaryContact)
        {
            return SliBridgeThresholdClass.NearThreshold;
        }

        return SliBridgeThresholdClass.WithinBand;
    }

    public static bool AreBasinsAdjacent(
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin)
    {
        if (activeBasin == competingBasin)
        {
            return true;
        }

        return (activeBasin, competingBasin) switch
        {
            (CompassDoctrineBasin.BoundedLocalityContinuity, CompassDoctrineBasin.FluidContinuityLaw) => true,
            (CompassDoctrineBasin.FluidContinuityLaw, CompassDoctrineBasin.BoundedLocalityContinuity) => true,
            _ => false
        };
    }

    private static bool HasWitness(
        string bridgeWitnessHandle,
        IdentityKernelBoundaryReceipt identityKernelBoundary)
    {
        return !string.IsNullOrWhiteSpace(bridgeWitnessHandle) &&
               !string.IsNullOrWhiteSpace(identityKernelBoundary.CmeIdentityHandle) &&
               !string.IsNullOrWhiteSpace(identityKernelBoundary.IdentityKernelHandle) &&
               !string.IsNullOrWhiteSpace(identityKernelBoundary.ContinuityAnchorHandle);
    }

    private static bool IsStageReady(string thetaState, string gammaState)
    {
        return string.Equals(thetaState, "theta-ready", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(gammaState, "gamma-ready", StringComparison.OrdinalIgnoreCase);
    }
}
