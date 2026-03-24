namespace Oan.Common;

public sealed record AgentiActualUtilitySurfaceReceipt(
    string UtilitySurfaceHandle,
    string CMEId,
    string ThreadBirthHandle,
    string IdentityInvariantHandle,
    string DuplexEnvelopeId,
    string WorkPredicate,
    string GovernancePredicate,
    string NexusPortalHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    string WitnessRequirement,
    string ReturnCondition,
    string AuthorityClass,
    string UtilityPosture,
    bool SovereigntyDenied,
    bool RemoteControlDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ReachDuplexRealizationReceipt(
    string RealizationHandle,
    string CMEId,
    string UtilitySurfaceHandle,
    string DuplexEnvelopeId,
    string ReachEnvelopeId,
    string SourceLocality,
    string TargetLocality,
    string BondedSpaceHandle,
    string AccessTopologyState,
    string LegibilityState,
    string DispatchState,
    bool AccessGrantImplied,
    bool LocalityCollapseDenied,
    bool IdentityCollapseDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BondedParticipationLocalityLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string UtilitySurfaceHandle,
    string RealizationHandle,
    string ThreadBirthHandle,
    string BondedSpaceHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    IReadOnlyList<string> CoRealizedSurfaces,
    IReadOnlyList<string> WithheldSurfaces,
    bool BondedParticipationProvisional,
    bool RemoteControlDenied,
    string ReturnCondition,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BondedCoWorkSessionRehearsalReceipt(
    string RehearsalHandle,
    string CMEId,
    string SessionLedgerHandle,
    string UtilitySurfaceHandle,
    string RealizationHandle,
    string LocalityLedgerHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    IReadOnlyList<string> SharedWorkLoop,
    IReadOnlyList<string> DuplexPredicateLanes,
    IReadOnlyList<string> WithheldLanes,
    string RehearsalState,
    bool LocalityCollapseDenied,
    bool RemoteControlDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ReachReturnDissolutionReceipt(
    string ReturnReceiptHandle,
    string CMEId,
    string RehearsalHandle,
    string RealizationHandle,
    string SourceLocality,
    string TargetLocality,
    string ReturnState,
    string DissolutionState,
    bool BondedEventReturned,
    bool BondedEventDissolved,
    bool AmbientGrantDenied,
    bool LocalityDistinctionPreserved,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record LocalityDistinctionWitnessLedgerReceipt(
    string WitnessLedgerHandle,
    string CMEId,
    string RehearsalHandle,
    string ReturnReceiptHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    IReadOnlyList<string> SharedSurfaces,
    IReadOnlyList<string> SanctuaryLocalSurfaces,
    IReadOnlyList<string> OperatorLocalSurfaces,
    IReadOnlyList<string> WithheldSurfaces,
    bool LocalityCollapseDetected,
    bool ProjectionTheaterDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record OperatorInquirySelectionEnvelopeReceipt(
    string EnvelopeHandle,
    string CMEId,
    string RehearsalHandle,
    string LocalityWitnessHandle,
    string InquirySurfaceHandle,
    string BoundaryLedgerHandle,
    string CoherenceWitnessHandle,
    string OperatorActualLocality,
    string EnvelopeState,
    IReadOnlyList<string> AvailableInquiryStances,
    IReadOnlyList<string> KnownBoundaryWarnings,
    IReadOnlyList<string> LawfulUseConditions,
    bool ProtectedInteriorityDenied,
    bool LocalityBypassDenied,
    bool RawGrantDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record BondedCrucibleSessionRehearsalReceipt(
    string RehearsalHandle,
    string CMEId,
    string CoWorkRehearsalHandle,
    string OperatorInquiryEnvelopeHandle,
    string BoundaryLedgerHandle,
    string CoherenceWitnessHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    string RehearsalState,
    string SharedUnknownClass,
    IReadOnlyList<string> SelectedInquiryStances,
    IReadOnlyList<string> SharedUnknownFacets,
    int CoordinationHoldCount,
    int ExposedBoundaryCount,
    bool PreScriptedAnswerDenied,
    bool RemoteDominanceDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record SharedBoundaryMemoryLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string BoundaryLedgerHandle,
    string ReturnReceiptHandle,
    string LocalityWitnessHandle,
    string LedgerState,
    IReadOnlyList<string> SharedBoundaryCodes,
    IReadOnlyList<string> SharedContinuityRequirements,
    IReadOnlyList<string> WithheldCommonPropertyClaims,
    bool LocalityProvenancePreserved,
    bool IdentityBleedDetected,
    bool AmbientCommonPropertyDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ContinuityUnderPressureLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string SharedBoundaryMemoryLedgerHandle,
    string CoherenceWitnessHandle,
    string LedgerState,
    IReadOnlyList<string> HeldContinuities,
    IReadOnlyList<string> PartialContinuities,
    IReadOnlyList<string> RequiredPreservations,
    int BoundaryPressureCount,
    bool FluentSuccessDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ExpressiveDeformationReceipt(
    string ReceiptHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string OperatorInquiryEnvelopeHandle,
    string ContinuityLedgerHandle,
    string SharedBoundaryMemoryLedgerHandle,
    string ReceiptState,
    string DeformationClass,
    IReadOnlyList<string> ChangedExpressions,
    IReadOnlyList<string> RecognizableContinuities,
    IReadOnlyList<string> FractureBoundaries,
    bool AdaptiveRefinementPreserved,
    bool IdentityCollapseDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record MutualIntelligibilityWitnessReceipt(
    string WitnessHandle,
    string CMEId,
    string CrucibleRehearsalHandle,
    string ContinuityLedgerHandle,
    string DeformationReceiptHandle,
    string LocalityWitnessHandle,
    string WitnessState,
    string SharedUnderstandingState,
    int HeldIntelligibilityCount,
    int NarrowedIntelligibilityCount,
    int BrokenIntelligibilityCount,
    bool SamenessCollapseDenied,
    bool OpaqueDivergenceDetected,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record InquiryPatternContinuityLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string OperatorInquiryEnvelopeHandle,
    string ContinuityLedgerHandle,
    string MutualIntelligibilityWitnessHandle,
    string SharedBoundaryMemoryLedgerHandle,
    string LedgerState,
    IReadOnlyList<string> ReusableInquiryPatterns,
    IReadOnlyList<string> TriggerConditions,
    IReadOnlyList<string> PreservedConstraints,
    int BoundaryPairCount,
    bool IdentityBleedDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record QuestioningBoundaryPairLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string OperatorInquiryEnvelopeHandle,
    string ContinuityLedgerHandle,
    string DeformationReceiptHandle,
    string SharedBoundaryMemoryLedgerHandle,
    string LedgerState,
    IReadOnlyList<string> InquiryPatterns,
    IReadOnlyList<string> SupportingBoundaries,
    IReadOnlyList<string> BoundaryConstraints,
    IReadOnlyList<string> OverreachWarnings,
    bool ConstraintMemoryPreserved,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record CarryForwardInquirySelectionSurfaceReceipt(
    string SurfaceHandle,
    string CMEId,
    string InquiryPatternLedgerHandle,
    string BoundaryPairLedgerHandle,
    string OperatorInquiryEnvelopeHandle,
    string LocalityWitnessHandle,
    string SurfaceState,
    IReadOnlyList<string> AvailableCarryForwardPatterns,
    IReadOnlyList<string> AdmittedReuseConditions,
    IReadOnlyList<string> WithheldReuseWarnings,
    bool LocalitySafeReview,
    bool AmbientHabitDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public enum EngramDistanceClass
{
    CoRoot,
    AdjacentRoot,
    FirstOrderOther,
    FarOther
}

public enum EngramPromotionCeiling
{
    FastTrackCandidateReview,
    GuardedCandidateReview,
    CandidateOnlyMemory,
    NarrativeArchiveOnly
}

public sealed record EngramDistanceClassificationEntry(
    string PatternCode,
    EngramDistanceClass DistanceClass,
    string SourceMode,
    string PromotionDisposition);

public sealed record EngramDistanceRequirementEntry(
    EngramDistanceClass DistanceClass,
    int RequiredEvidenceCount,
    int MaximumUnknownLoad,
    int RequiredReentryDepth,
    EngramPromotionCeiling PromotionCeiling,
    bool FreshConstraintContactRequired,
    IReadOnlyList<string> RefusalConditions);

public sealed record EngramDistanceClassificationLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string CarryForwardInquirySelectionSurfaceHandle,
    string InquiryPatternLedgerHandle,
    string BoundaryPairLedgerHandle,
    string ContinuityLedgerHandle,
    string MutualIntelligibilityWitnessHandle,
    string LedgerState,
    EngramDistanceClass DominantDistanceClass,
    IReadOnlyList<EngramDistanceClassificationEntry> ClassifiedPatterns,
    int CoRootPatternCount,
    int AdjacentRootPatternCount,
    int FirstOrderOtherPatternCount,
    int FarOtherArtifactCount,
    bool PromotionFromFarOtherDenied,
    bool ReRootingRequiredForFarOther,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record EngramPromotionRequirementsMatrixReceipt(
    string MatrixHandle,
    string CMEId,
    string ClassificationLedgerHandle,
    string MatrixState,
    IReadOnlyList<EngramDistanceRequirementEntry> RequirementEntries,
    bool BurdenScalingPreserved,
    bool PortableInheritanceRequiresVariation,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record DistanceWeightedQuestioningAdmissionSurfaceReceipt(
    string SurfaceHandle,
    string CMEId,
    string ClassificationLedgerHandle,
    string PromotionRequirementsMatrixHandle,
    string CarryForwardInquirySelectionSurfaceHandle,
    string SurfaceState,
    EngramDistanceClass DominantDistanceClass,
    EngramPromotionCeiling PromotionCeiling,
    IReadOnlyList<string> AdmittedCandidatePatterns,
    IReadOnlyList<string> WithheldCandidatePatterns,
    IReadOnlyList<string> RequiredReentryBurdens,
    int UnknownTolerance,
    bool DistanceScalingPreserved,
    bool FarOtherPromotionDenied,
    bool ReRootingRequired,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record QuestioningOperatorCandidateLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string CarryForwardInquirySelectionSurfaceHandle,
    string InquiryPatternLedgerHandle,
    string BoundaryPairLedgerHandle,
    string ContinuityLedgerHandle,
    string MutualIntelligibilityWitnessHandle,
    string DistanceWeightedAdmissionSurfaceHandle,
    string LedgerState,
    EngramDistanceClass DominantDistanceClass,
    EngramPromotionCeiling PromotionCeiling,
    IReadOnlyList<string> EventBoundInquiryForms,
    IReadOnlyList<string> CandidateInquiryPatterns,
    IReadOnlyList<string> PromotionEvidence,
    IReadOnlyList<string> RequiredReentryConditions,
    IReadOnlyList<string> FailureSignatureExpectations,
    bool HiddenAuthorityPatternsDenied,
    bool IdentityBoundPatternsWithheld,
    bool DistanceScalingPreserved,
    bool FarOtherPromotionDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record QuestioningGelPromotionGateReceipt(
    string GateHandle,
    string CMEId,
    string CandidateLedgerHandle,
    string CarryForwardInquirySelectionSurfaceHandle,
    string OperatorInquiryEnvelopeHandle,
    string LocalityWitnessHandle,
    string DistanceWeightedAdmissionSurfaceHandle,
    string GateState,
    EngramDistanceClass DominantDistanceClass,
    EngramPromotionCeiling PromotionCeiling,
    IReadOnlyList<string> CandidateInquiryPatterns,
    IReadOnlyList<string> SatisfiedPromotionConditions,
    IReadOnlyList<string> UnmetPromotionConditions,
    IReadOnlyList<string> PromotionWarnings,
    bool LocalitySeparationPreserved,
    bool AuthoritySeparationPreserved,
    bool TruthSeekingInvariantPreserved,
    bool OutcomeSeekingDenied,
    bool DistanceScalingPreserved,
    bool ReRootingRequired,
    bool PromotionReviewAdmitted,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record ProtectedQuestioningPatternSurfaceReceipt(
    string SurfaceHandle,
    string CMEId,
    string CandidateLedgerHandle,
    string PromotionGateHandle,
    string CarryForwardInquirySelectionSurfaceHandle,
    string LocalityWitnessHandle,
    string SurfaceState,
    IReadOnlyList<string> ReviewableCandidatePatterns,
    IReadOnlyList<string> LawfulReviewEnvelopes,
    IReadOnlyList<string> WithheldInteriorityWarnings,
    bool LocalitySafeLegibility,
    bool RawInteriorityDenied,
    bool AutomaticGrantDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record VariationTestedReentryLedgerReceipt(
    string LedgerHandle,
    string CMEId,
    string DistanceWeightedAdmissionSurfaceHandle,
    string CandidateLedgerHandle,
    string PromotionGateHandle,
    string ProtectedPatternSurfaceHandle,
    string LedgerState,
    IReadOnlyList<string> VariationContexts,
    IReadOnlyList<string> SurvivingPatterns,
    IReadOnlyList<string> FailedPatterns,
    IReadOnlyList<string> RequiredRetestPatterns,
    int RequiredReentryPassCount,
    bool VariationBurdenSatisfied,
    bool PortablePatternsWithstoodVariation,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record QuestioningAdmissionRefusalReceipt(
    string ReceiptHandle,
    string CMEId,
    string VariationTestedReentryLedgerHandle,
    string CandidateLedgerHandle,
    string PromotionGateHandle,
    string ProtectedPatternSurfaceHandle,
    string ReceiptState,
    IReadOnlyList<string> RefusedPatterns,
    IReadOnlyList<string> DeferredPatterns,
    IReadOnlyList<string> RefusalReasons,
    bool AttractiveButUnderEvidencedDenied,
    bool ArchiveProtectionPreserved,
    bool DelayWithoutDisposalAllowed,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record PromotionSeductionWatchReceipt(
    string WatchHandle,
    string CMEId,
    string CandidateLedgerHandle,
    string PromotionGateHandle,
    string VariationTestedReentryLedgerHandle,
    string AdmissionRefusalReceiptHandle,
    string WatchState,
    IReadOnlyList<string> SeductionSignals,
    IReadOnlyList<string> BlockedPromotionVectors,
    IReadOnlyList<string> DriftWarnings,
    bool PrestigeInflationDenied,
    bool EleganceBiasDenied,
    bool EmotionalCompulsionDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class AgentiActualizationProjector
{
    private const string GovernedThreadBirthPrefix = "governed-thread-birth://";
    private const string IdentityInvariantPrefix = "identity-invariant://";
    private const string DuplexEnvelopePrefix = "agenticore-duplex-envelope://";
    private const string ReachEnvelopePrefix = "reach-duplex-envelope://";
    private const string AgentiActualSurfacePrefix = "agenticore-actual-surface://";
    private const string BondedSpacePrefix = "bonded-space://";
    private const string RuntimeWorkbenchSessionPrefix = "runtime-workbench-session-ledger://";
    private const string BondedLocalityLedgerPrefix = "bonded-locality-ledger://";
    private const string ReachRealizationPrefix = "reach-duplex-realization://";
    private const string InquirySessionDisciplinePrefix = "inquiry-session-discipline-surface://";
    private const string BoundaryConditionLedgerPrefix = "boundary-condition-ledger://";
    private const string CoherenceGainWitnessPrefix = "coherence-gain-witness-receipt://";
    private const string OperatorInquirySelectionEnvelopePrefix = "operator-inquiry-selection-envelope://";
    private const string BondedCrucibleSessionRehearsalPrefix = "bonded-crucible-session-rehearsal://";
    private const string SharedBoundaryMemoryLedgerPrefix = "shared-boundary-memory-ledger://";
    private const string ContinuityUnderPressureLedgerPrefix = "continuity-under-pressure-ledger://";
    private const string ExpressiveDeformationReceiptPrefix = "expressive-deformation-receipt://";
    private const string MutualIntelligibilityWitnessPrefix = "mutual-intelligibility-witness://";
    private const string InquiryPatternContinuityLedgerPrefix = "inquiry-pattern-continuity-ledger://";
    private const string QuestioningBoundaryPairLedgerPrefix = "questioning-boundary-pair-ledger://";
    private const string CarryForwardInquirySelectionSurfacePrefix = "carry-forward-inquiry-selection-surface://";
    private const string EngramDistanceClassificationLedgerPrefix = "engram-distance-classification-ledger://";
    private const string EngramPromotionRequirementsMatrixPrefix = "engram-promotion-requirements-matrix://";
    private const string DistanceWeightedQuestioningAdmissionSurfacePrefix = "distance-weighted-questioning-admission-surface://";
    private const string QuestioningOperatorCandidateLedgerPrefix = "questioning-operator-candidate-ledger://";
    private const string QuestioningGelPromotionGatePrefix = "questioning-gel-promotion-gate://";
    private const string ProtectedQuestioningPatternSurfacePrefix = "protected-questioning-pattern-surface://";
    private const string VariationTestedReentryLedgerPrefix = "variation-tested-reentry-ledger://";
    private const string QuestioningAdmissionRefusalReceiptPrefix = "questioning-admission-refusal-receipt://";
    private const string PromotionSeductionWatchPrefix = "promotion-seduction-watch://";

    public static AgentiActualUtilitySurfaceReceipt CreateAgentiActualUtilitySurface(
        string cmeId,
        string threadBirthHandle,
        string identityInvariantHandle,
        string duplexEnvelopeId,
        string workPredicate,
        string governancePredicate,
        string nexusPortalHandle,
        string sanctuaryActualLocality,
        string operatorActualLocality,
        string witnessRequirement,
        string returnCondition,
        string authorityClass,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        EnsurePrefix(threadBirthHandle, GovernedThreadBirthPrefix, nameof(threadBirthHandle));
        EnsurePrefix(identityInvariantHandle, IdentityInvariantPrefix, nameof(identityInvariantHandle));
        EnsurePrefix(duplexEnvelopeId, DuplexEnvelopePrefix, nameof(duplexEnvelopeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(workPredicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(governancePredicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(nexusPortalHandle);
        RequireActualLocality(sanctuaryActualLocality, nameof(sanctuaryActualLocality));
        RequireActualLocality(operatorActualLocality, nameof(operatorActualLocality));
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessRequirement);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnCondition);
        ArgumentException.ThrowIfNullOrWhiteSpace(authorityClass);

        return new AgentiActualUtilitySurfaceReceipt(
            UtilitySurfaceHandle: AgentiActualizationKeys.CreateAgentiActualUtilitySurfaceHandle(
                cmeId,
                threadBirthHandle,
                duplexEnvelopeId,
                operatorActualLocality),
            CMEId: cmeId.Trim(),
            ThreadBirthHandle: threadBirthHandle.Trim(),
            IdentityInvariantHandle: identityInvariantHandle.Trim(),
            DuplexEnvelopeId: duplexEnvelopeId.Trim(),
            WorkPredicate: workPredicate.Trim(),
            GovernancePredicate: governancePredicate.Trim(),
            NexusPortalHandle: nexusPortalHandle.Trim(),
            SanctuaryActualLocality: sanctuaryActualLocality.Trim(),
            OperatorActualLocality: operatorActualLocality.Trim(),
            WitnessRequirement: witnessRequirement.Trim(),
            ReturnCondition: returnCondition.Trim(),
            AuthorityClass: authorityClass.Trim(),
            UtilityPosture: "governed-utility-virtualized",
            SovereigntyDenied: true,
            RemoteControlDenied: true,
            ReasonCode: "agenticore-actual-utility-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ReachDuplexRealizationReceipt CreateReachDuplexRealizationReceipt(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        string reachEnvelopeId,
        string sourceLocality,
        string targetLocality,
        string bondedSpaceHandle,
        string accessTopologyState,
        string legibilityState,
        string dispatchState,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        EnsurePrefix(utilitySurface.UtilitySurfaceHandle, AgentiActualSurfacePrefix, nameof(utilitySurface));
        EnsurePrefix(reachEnvelopeId, ReachEnvelopePrefix, nameof(reachEnvelopeId));
        RequireActualLocality(sourceLocality, nameof(sourceLocality));
        RequireActualLocality(targetLocality, nameof(targetLocality));
        EnsurePrefix(bondedSpaceHandle, BondedSpacePrefix, nameof(bondedSpaceHandle));
        ArgumentException.ThrowIfNullOrWhiteSpace(accessTopologyState);
        ArgumentException.ThrowIfNullOrWhiteSpace(legibilityState);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispatchState);

        var normalizedDispatchState = dispatchState.Trim();
        var reasonCode = string.Equals(normalizedDispatchState, "accepted", StringComparison.OrdinalIgnoreCase)
            ? "reach-duplex-realization-dispatched"
            : "reach-duplex-realization-withheld";

        return new ReachDuplexRealizationReceipt(
            RealizationHandle: AgentiActualizationKeys.CreateReachDuplexRealizationHandle(
                utilitySurface.CMEId,
                utilitySurface.UtilitySurfaceHandle,
                reachEnvelopeId,
                targetLocality),
            CMEId: utilitySurface.CMEId,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            DuplexEnvelopeId: utilitySurface.DuplexEnvelopeId,
            ReachEnvelopeId: reachEnvelopeId.Trim(),
            SourceLocality: sourceLocality.Trim(),
            TargetLocality: targetLocality.Trim(),
            BondedSpaceHandle: bondedSpaceHandle.Trim(),
            AccessTopologyState: accessTopologyState.Trim(),
            LegibilityState: legibilityState.Trim(),
            DispatchState: normalizedDispatchState,
            AccessGrantImplied: false,
            LocalityCollapseDenied: true,
            IdentityCollapseDenied: true,
            ReasonCode: reasonCode,
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BondedParticipationLocalityLedgerReceipt CreateBondedParticipationLocalityLedger(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        ReachDuplexRealizationReceipt realization,
        string threadBirthHandle,
        IReadOnlyList<string> coRealizedSurfaces,
        IReadOnlyList<string> withheldSurfaces,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(realization);
        EnsurePrefix(threadBirthHandle, GovernedThreadBirthPrefix, nameof(threadBirthHandle));
        ArgumentNullException.ThrowIfNull(coRealizedSurfaces);
        ArgumentNullException.ThrowIfNull(withheldSurfaces);

        if (!string.Equals(utilitySurface.CMEId, realization.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bonded locality ledger requires utility and reach realization to remain inside the same CME continuity surface.");
        }

        return new BondedParticipationLocalityLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateBondedParticipationLocalityLedgerHandle(
                utilitySurface.CMEId,
                realization.RealizationHandle,
                utilitySurface.SanctuaryActualLocality,
                utilitySurface.OperatorActualLocality),
            CMEId: utilitySurface.CMEId,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            RealizationHandle: realization.RealizationHandle,
            ThreadBirthHandle: threadBirthHandle.Trim(),
            BondedSpaceHandle: realization.BondedSpaceHandle,
            SanctuaryActualLocality: utilitySurface.SanctuaryActualLocality,
            OperatorActualLocality: utilitySurface.OperatorActualLocality,
            CoRealizedSurfaces: (coRealizedSurfaces ?? Array.Empty<string>()).ToArray(),
            WithheldSurfaces: (withheldSurfaces ?? Array.Empty<string>()).ToArray(),
            BondedParticipationProvisional: true,
            RemoteControlDenied: true,
            ReturnCondition: utilitySurface.ReturnCondition,
            ReasonCode: "bonded-participation-locality-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BondedCoWorkSessionRehearsalReceipt CreateBondedCoWorkSessionRehearsal(
        RuntimeWorkbenchSessionLedger sessionLedger,
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        ReachDuplexRealizationReceipt realization,
        BondedParticipationLocalityLedgerReceipt localityLedger,
        IReadOnlyList<string> sharedWorkLoop,
        IReadOnlyList<string> duplexPredicateLanes,
        IReadOnlyList<string> withheldLanes,
        string rehearsalState = "bounded-cowork-rehearsal-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sessionLedger);
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(realization);
        ArgumentNullException.ThrowIfNull(localityLedger);
        EnsurePrefix(sessionLedger.SessionLedgerHandle, RuntimeWorkbenchSessionPrefix, nameof(sessionLedger));
        EnsurePrefix(realization.RealizationHandle, ReachRealizationPrefix, nameof(realization));
        EnsurePrefix(localityLedger.LedgerHandle, BondedLocalityLedgerPrefix, nameof(localityLedger));
        ArgumentNullException.ThrowIfNull(sharedWorkLoop);
        ArgumentNullException.ThrowIfNull(duplexPredicateLanes);
        ArgumentNullException.ThrowIfNull(withheldLanes);
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalState);

        if (!string.Equals(sessionLedger.CMEId, utilitySurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(utilitySurface.CMEId, realization.CMEId, StringComparison.Ordinal) ||
            !string.Equals(realization.CMEId, localityLedger.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bonded co-work rehearsal requires session, utility, reach, and locality receipts to remain inside one CME continuity surface.");
        }

        return new BondedCoWorkSessionRehearsalReceipt(
            RehearsalHandle: AgentiActualizationKeys.CreateBondedCoWorkSessionRehearsalHandle(
                sessionLedger.CMEId,
                sessionLedger.SessionLedgerHandle,
                realization.RealizationHandle,
                localityLedger.LedgerHandle),
            CMEId: sessionLedger.CMEId,
            SessionLedgerHandle: sessionLedger.SessionLedgerHandle,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            RealizationHandle: realization.RealizationHandle,
            LocalityLedgerHandle: localityLedger.LedgerHandle,
            SanctuaryActualLocality: utilitySurface.SanctuaryActualLocality,
            OperatorActualLocality: utilitySurface.OperatorActualLocality,
            SharedWorkLoop: (sharedWorkLoop ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray(),
            DuplexPredicateLanes: (duplexPredicateLanes ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray(),
            WithheldLanes: (withheldLanes ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray(),
            RehearsalState: rehearsalState.Trim(),
            LocalityCollapseDenied: true,
            RemoteControlDenied: true,
            ReasonCode: "bonded-cowork-session-rehearsal-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ReachReturnDissolutionReceipt CreateReachReturnDissolutionReceipt(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        ReachDuplexRealizationReceipt realization,
        string returnState = "returned-through-reach",
        string dissolutionState = "dissolution-witnessed",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(realization);
        EnsurePrefix(rehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(rehearsal));
        EnsurePrefix(realization.RealizationHandle, ReachRealizationPrefix, nameof(realization));
        ArgumentException.ThrowIfNullOrWhiteSpace(returnState);
        ArgumentException.ThrowIfNullOrWhiteSpace(dissolutionState);

        if (!string.Equals(rehearsal.CMEId, realization.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.RealizationHandle, realization.RealizationHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Reach return dissolution requires bonded co-work and realization receipts to remain inside one realized bonded event.");
        }

        var normalizedReturnState = returnState.Trim();
        var normalizedDissolutionState = dissolutionState.Trim();
        var bondedEventReturned = normalizedReturnState.Contains("returned", StringComparison.OrdinalIgnoreCase);
        var bondedEventDissolved = normalizedDissolutionState.Contains("dissolution", StringComparison.OrdinalIgnoreCase);

        return new ReachReturnDissolutionReceipt(
            ReturnReceiptHandle: AgentiActualizationKeys.CreateReachReturnDissolutionReceiptHandle(
                rehearsal.CMEId,
                rehearsal.RehearsalHandle,
                normalizedReturnState,
                normalizedDissolutionState),
            CMEId: rehearsal.CMEId,
            RehearsalHandle: rehearsal.RehearsalHandle,
            RealizationHandle: realization.RealizationHandle,
            SourceLocality: realization.SourceLocality,
            TargetLocality: realization.TargetLocality,
            ReturnState: normalizedReturnState,
            DissolutionState: normalizedDissolutionState,
            BondedEventReturned: bondedEventReturned,
            BondedEventDissolved: bondedEventDissolved,
            AmbientGrantDenied: true,
            LocalityDistinctionPreserved: rehearsal.LocalityCollapseDenied && realization.LocalityCollapseDenied,
            ReasonCode: "reach-return-dissolution-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static LocalityDistinctionWitnessLedgerReceipt CreateLocalityDistinctionWitnessLedger(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        ReachReturnDissolutionReceipt returnReceipt,
        IReadOnlyList<string> sharedSurfaces,
        IReadOnlyList<string> sanctuaryLocalSurfaces,
        IReadOnlyList<string> operatorLocalSurfaces,
        IReadOnlyList<string> withheldSurfaces,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(returnReceipt);
        EnsurePrefix(rehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(rehearsal));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, "reach-return-dissolution://", nameof(returnReceipt));
        ArgumentNullException.ThrowIfNull(sharedSurfaces);
        ArgumentNullException.ThrowIfNull(sanctuaryLocalSurfaces);
        ArgumentNullException.ThrowIfNull(operatorLocalSurfaces);
        ArgumentNullException.ThrowIfNull(withheldSurfaces);

        if (!string.Equals(rehearsal.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.RehearsalHandle, returnReceipt.RehearsalHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Locality distinction witness requires rehearsal and return receipts to remain inside one bonded co-work event.");
        }

        var normalizedSharedSurfaces = (sharedSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var normalizedSanctuaryLocalSurfaces = (sanctuaryLocalSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var normalizedOperatorLocalSurfaces = (operatorLocalSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var normalizedWithheldSurfaces = (withheldSurfaces ?? Array.Empty<string>()).Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.Ordinal).ToArray();
        var localityCollapseDetected = normalizedSanctuaryLocalSurfaces.Intersect(normalizedOperatorLocalSurfaces, StringComparer.Ordinal).Any();

        return new LocalityDistinctionWitnessLedgerReceipt(
            WitnessLedgerHandle: AgentiActualizationKeys.CreateLocalityDistinctionWitnessLedgerHandle(
                rehearsal.CMEId,
                rehearsal.RehearsalHandle,
                returnReceipt.ReturnReceiptHandle),
            CMEId: rehearsal.CMEId,
            RehearsalHandle: rehearsal.RehearsalHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            SanctuaryActualLocality: rehearsal.SanctuaryActualLocality,
            OperatorActualLocality: rehearsal.OperatorActualLocality,
            SharedSurfaces: normalizedSharedSurfaces,
            SanctuaryLocalSurfaces: normalizedSanctuaryLocalSurfaces,
            OperatorLocalSurfaces: normalizedOperatorLocalSurfaces,
            WithheldSurfaces: normalizedWithheldSurfaces,
            LocalityCollapseDetected: localityCollapseDetected,
            ProjectionTheaterDenied: !localityCollapseDetected,
            ReasonCode: "locality-distinction-witness-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static OperatorInquirySelectionEnvelopeReceipt CreateOperatorInquirySelectionEnvelope(
        BondedCoWorkSessionRehearsalReceipt rehearsal,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        InquirySessionDisciplineSurfaceReceipt inquirySurface,
        BoundaryConditionLedgerReceipt boundaryLedger,
        CoherenceGainWitnessReceipt coherenceWitness,
        string envelopeState = "operator-inquiry-selection-envelope-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(rehearsal);
        ArgumentNullException.ThrowIfNull(localityWitness);
        ArgumentNullException.ThrowIfNull(inquirySurface);
        ArgumentNullException.ThrowIfNull(boundaryLedger);
        ArgumentNullException.ThrowIfNull(coherenceWitness);
        EnsurePrefix(rehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(rehearsal));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        EnsurePrefix(inquirySurface.InquirySurfaceHandle, InquirySessionDisciplinePrefix, nameof(inquirySurface));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        EnsurePrefix(coherenceWitness.CoherenceWitnessHandle, CoherenceGainWitnessPrefix, nameof(coherenceWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(envelopeState);

        if (!string.Equals(rehearsal.CMEId, localityWitness.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.CMEId, inquirySurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(rehearsal.CMEId, coherenceWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Operator inquiry selection requires rehearsal, locality witness, inquiry, boundary, and coherence receipts to remain inside one bonded CME surface.");
        }

        var availableInquiryStances = inquirySurface.InquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var knownBoundaryWarnings = boundaryLedger.RetainedBoundaryConditions
            .Select(static boundary => boundary.BoundaryCode)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var lawfulUseConditions = boundaryLedger.ContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new OperatorInquirySelectionEnvelopeReceipt(
            EnvelopeHandle: AgentiActualizationKeys.CreateOperatorInquirySelectionEnvelopeHandle(
                rehearsal.CMEId,
                rehearsal.RehearsalHandle,
                inquirySurface.InquirySurfaceHandle,
                rehearsal.OperatorActualLocality),
            CMEId: rehearsal.CMEId,
            RehearsalHandle: rehearsal.RehearsalHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            InquirySurfaceHandle: inquirySurface.InquirySurfaceHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            CoherenceWitnessHandle: coherenceWitness.CoherenceWitnessHandle,
            OperatorActualLocality: rehearsal.OperatorActualLocality,
            EnvelopeState: envelopeState.Trim(),
            AvailableInquiryStances: availableInquiryStances,
            KnownBoundaryWarnings: knownBoundaryWarnings,
            LawfulUseConditions: lawfulUseConditions,
            ProtectedInteriorityDenied: true,
            LocalityBypassDenied: !localityWitness.LocalityCollapseDetected,
            RawGrantDenied: true,
            ReasonCode: "operator-inquiry-selection-envelope-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static BondedCrucibleSessionRehearsalReceipt CreateBondedCrucibleSessionRehearsal(
        BondedCoWorkSessionRehearsalReceipt coWorkRehearsal,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        BoundaryConditionLedgerReceipt boundaryLedger,
        CoherenceGainWitnessReceipt coherenceWitness,
        string rehearsalState = "bonded-crucible-session-rehearsal-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(coWorkRehearsal);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(boundaryLedger);
        ArgumentNullException.ThrowIfNull(coherenceWitness);
        EnsurePrefix(coWorkRehearsal.RehearsalHandle, "bonded-cowork-session-rehearsal://", nameof(coWorkRehearsal));
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        EnsurePrefix(coherenceWitness.CoherenceWitnessHandle, CoherenceGainWitnessPrefix, nameof(coherenceWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(rehearsalState);

        if (!string.Equals(coWorkRehearsal.CMEId, operatorInquiryEnvelope.CMEId, StringComparison.Ordinal) ||
            !string.Equals(coWorkRehearsal.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(coWorkRehearsal.CMEId, coherenceWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Bonded crucible rehearsal requires co-work, operator inquiry, boundary, and coherence receipts to remain inside one bonded CME surface.");
        }

        var selectedInquiryStances = operatorInquiryEnvelope.AvailableInquiryStances
            .Take(3)
            .ToArray();
        var sharedUnknownFacets = new[]
        {
            "partial-information",
            "assumption-reversal",
            "boundary-pressure"
        };

        return new BondedCrucibleSessionRehearsalReceipt(
            RehearsalHandle: AgentiActualizationKeys.CreateBondedCrucibleSessionRehearsalHandle(
                coWorkRehearsal.CMEId,
                coWorkRehearsal.RehearsalHandle,
                operatorInquiryEnvelope.EnvelopeHandle,
                boundaryLedger.BoundaryLedgerHandle),
            CMEId: coWorkRehearsal.CMEId,
            CoWorkRehearsalHandle: coWorkRehearsal.RehearsalHandle,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            CoherenceWitnessHandle: coherenceWitness.CoherenceWitnessHandle,
            SanctuaryActualLocality: coWorkRehearsal.SanctuaryActualLocality,
            OperatorActualLocality: coWorkRehearsal.OperatorActualLocality,
            RehearsalState: rehearsalState.Trim(),
            SharedUnknownClass: "shared-uncertainty-bounded-crucible",
            SelectedInquiryStances: selectedInquiryStances,
            SharedUnknownFacets: sharedUnknownFacets,
            CoordinationHoldCount: coherenceWitness.CoherencePreservingEventCount,
            ExposedBoundaryCount: boundaryLedger.RetainedBoundaryConditions.Count,
            PreScriptedAnswerDenied: true,
            RemoteDominanceDenied: coWorkRehearsal.RemoteControlDenied && operatorInquiryEnvelope.RawGrantDenied,
            ReasonCode: "bonded-crucible-session-rehearsal-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static SharedBoundaryMemoryLedgerReceipt CreateSharedBoundaryMemoryLedger(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        BoundaryConditionLedgerReceipt boundaryLedger,
        ReachReturnDissolutionReceipt returnReceipt,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string ledgerState = "shared-boundary-memory-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(boundaryLedger);
        ArgumentNullException.ThrowIfNull(returnReceipt);
        ArgumentNullException.ThrowIfNull(localityWitness);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(boundaryLedger.BoundaryLedgerHandle, BoundaryConditionLedgerPrefix, nameof(boundaryLedger));
        EnsurePrefix(returnReceipt.ReturnReceiptHandle, "reach-return-dissolution://", nameof(returnReceipt));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(crucibleRehearsal.CMEId, boundaryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, returnReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, localityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Shared boundary memory requires crucible, boundary, return, and locality receipts to remain inside one bonded CME surface.");
        }

        var sharedBoundaryCodes = boundaryLedger.RetainedBoundaryConditions
            .Select(static boundary => boundary.BoundaryCode)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sharedContinuityRequirements = boundaryLedger.ContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var withheldCommonPropertyClaims = new[]
        {
            "ambient-shared-interiority",
            "identity-collapse",
            "sovereign-cross-grant"
        };
        var localityProvenancePreserved = !localityWitness.LocalityCollapseDetected && returnReceipt.LocalityDistinctionPreserved;

        return new SharedBoundaryMemoryLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateSharedBoundaryMemoryLedgerHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                boundaryLedger.BoundaryLedgerHandle,
                returnReceipt.ReturnReceiptHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            BoundaryLedgerHandle: boundaryLedger.BoundaryLedgerHandle,
            ReturnReceiptHandle: returnReceipt.ReturnReceiptHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            LedgerState: ledgerState.Trim(),
            SharedBoundaryCodes: sharedBoundaryCodes,
            SharedContinuityRequirements: sharedContinuityRequirements,
            WithheldCommonPropertyClaims: withheldCommonPropertyClaims,
            LocalityProvenancePreserved: localityProvenancePreserved,
            IdentityBleedDetected: boundaryLedger.IdentityBleedDetected,
            AmbientCommonPropertyDenied: true,
            ReasonCode: "shared-boundary-memory-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ContinuityUnderPressureLedgerReceipt CreateContinuityUnderPressureLedger(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        CoherenceGainWitnessReceipt coherenceWitness,
        string ledgerState = "continuity-under-pressure-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        ArgumentNullException.ThrowIfNull(coherenceWitness);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(sharedBoundaryMemory.LedgerHandle, SharedBoundaryMemoryLedgerPrefix, nameof(sharedBoundaryMemory));
        EnsurePrefix(coherenceWitness.CoherenceWitnessHandle, CoherenceGainWitnessPrefix, nameof(coherenceWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(crucibleRehearsal.CMEId, sharedBoundaryMemory.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, coherenceWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Continuity under pressure requires crucible, shared-boundary-memory, and coherence receipts to remain inside one bonded CME surface.");
        }

        var heldContinuities = sharedBoundaryMemory.SharedContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var partialContinuities = crucibleRehearsal.SelectedInquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Select(static item => $"{item}-under-pressure")
            .Take(3)
            .ToArray();
        var requiredPreservations = sharedBoundaryMemory.SharedContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new ContinuityUnderPressureLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateContinuityUnderPressureLedgerHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                sharedBoundaryMemory.LedgerHandle,
                coherenceWitness.CoherenceWitnessHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            SharedBoundaryMemoryLedgerHandle: sharedBoundaryMemory.LedgerHandle,
            CoherenceWitnessHandle: coherenceWitness.CoherenceWitnessHandle,
            LedgerState: ledgerState.Trim(),
            HeldContinuities: heldContinuities,
            PartialContinuities: partialContinuities,
            RequiredPreservations: requiredPreservations,
            BoundaryPressureCount: crucibleRehearsal.ExposedBoundaryCount,
            FluentSuccessDenied: true,
            ReasonCode: "continuity-under-pressure-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ExpressiveDeformationReceipt CreateExpressiveDeformationReceipt(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        string receiptState = "expressive-deformation-receipt-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(sharedBoundaryMemory.LedgerHandle, SharedBoundaryMemoryLedgerPrefix, nameof(sharedBoundaryMemory));
        ArgumentException.ThrowIfNullOrWhiteSpace(receiptState);

        if (!string.Equals(crucibleRehearsal.CMEId, operatorInquiryEnvelope.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, sharedBoundaryMemory.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Expressive deformation requires crucible, operator inquiry, continuity, and shared-boundary-memory receipts to remain inside one bonded CME surface.");
        }

        var changedExpressions = operatorInquiryEnvelope.AvailableInquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Select(static item => $"{item}-under-pressure")
            .Take(3)
            .ToArray();
        var recognizableContinuities = continuityLedger.HeldContinuities
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var fractureBoundaries = sharedBoundaryMemory.SharedBoundaryCodes
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new ExpressiveDeformationReceipt(
            ReceiptHandle: AgentiActualizationKeys.CreateExpressiveDeformationReceiptHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                operatorInquiryEnvelope.EnvelopeHandle,
                continuityLedger.LedgerHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            SharedBoundaryMemoryLedgerHandle: sharedBoundaryMemory.LedgerHandle,
            ReceiptState: receiptState.Trim(),
            DeformationClass: "adaptive-refinement-with-bounded-strain",
            ChangedExpressions: changedExpressions,
            RecognizableContinuities: recognizableContinuities,
            FractureBoundaries: fractureBoundaries,
            AdaptiveRefinementPreserved: true,
            IdentityCollapseDetected: sharedBoundaryMemory.IdentityBleedDetected,
            ReasonCode: "expressive-deformation-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static MutualIntelligibilityWitnessReceipt CreateMutualIntelligibilityWitness(
        BondedCrucibleSessionRehearsalReceipt crucibleRehearsal,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        ExpressiveDeformationReceipt deformationReceipt,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string witnessState = "mutual-intelligibility-witness-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(crucibleRehearsal);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(deformationReceipt);
        ArgumentNullException.ThrowIfNull(localityWitness);
        EnsurePrefix(crucibleRehearsal.RehearsalHandle, BondedCrucibleSessionRehearsalPrefix, nameof(crucibleRehearsal));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(deformationReceipt.ReceiptHandle, ExpressiveDeformationReceiptPrefix, nameof(deformationReceipt));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(witnessState);

        if (!string.Equals(crucibleRehearsal.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, deformationReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(crucibleRehearsal.CMEId, localityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Mutual intelligibility witness requires crucible, continuity, deformation, and locality receipts to remain inside one bonded CME surface.");
        }

        return new MutualIntelligibilityWitnessReceipt(
            WitnessHandle: AgentiActualizationKeys.CreateMutualIntelligibilityWitnessHandle(
                crucibleRehearsal.CMEId,
                crucibleRehearsal.RehearsalHandle,
                continuityLedger.LedgerHandle,
                deformationReceipt.ReceiptHandle),
            CMEId: crucibleRehearsal.CMEId,
            CrucibleRehearsalHandle: crucibleRehearsal.RehearsalHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            DeformationReceiptHandle: deformationReceipt.ReceiptHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            WitnessState: witnessState.Trim(),
            SharedUnderstandingState: "mutual-intelligibility-preserved",
            HeldIntelligibilityCount: continuityLedger.HeldContinuities.Count,
            NarrowedIntelligibilityCount: deformationReceipt.RecognizableContinuities.Count,
            BrokenIntelligibilityCount: deformationReceipt.FractureBoundaries.Count,
            SamenessCollapseDenied: !localityWitness.LocalityCollapseDetected,
            OpaqueDivergenceDetected: false,
            ReasonCode: "mutual-intelligibility-witness-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static InquiryPatternContinuityLedgerReceipt CreateInquiryPatternContinuityLedger(
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        MutualIntelligibilityWitnessReceipt mutualIntelligibilityWitness,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        string ledgerState = "inquiry-pattern-continuity-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(mutualIntelligibilityWitness);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(mutualIntelligibilityWitness.WitnessHandle, MutualIntelligibilityWitnessPrefix, nameof(mutualIntelligibilityWitness));
        EnsurePrefix(sharedBoundaryMemory.LedgerHandle, SharedBoundaryMemoryLedgerPrefix, nameof(sharedBoundaryMemory));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(operatorInquiryEnvelope.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(operatorInquiryEnvelope.CMEId, mutualIntelligibilityWitness.CMEId, StringComparison.Ordinal) ||
            !string.Equals(operatorInquiryEnvelope.CMEId, sharedBoundaryMemory.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Inquiry pattern continuity requires inquiry envelope, pressure continuity, mutual intelligibility, and shared boundary memory to remain inside one bonded CME surface.");
        }

        var reusableInquiryPatterns = operatorInquiryEnvelope.AvailableInquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var triggerConditions = sharedBoundaryMemory.SharedContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var preservedConstraints = operatorInquiryEnvelope.LawfulUseConditions
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new InquiryPatternContinuityLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateInquiryPatternContinuityLedgerHandle(
                operatorInquiryEnvelope.CMEId,
                operatorInquiryEnvelope.EnvelopeHandle,
                continuityLedger.LedgerHandle,
                mutualIntelligibilityWitness.WitnessHandle),
            CMEId: operatorInquiryEnvelope.CMEId,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            MutualIntelligibilityWitnessHandle: mutualIntelligibilityWitness.WitnessHandle,
            SharedBoundaryMemoryLedgerHandle: sharedBoundaryMemory.LedgerHandle,
            LedgerState: ledgerState.Trim(),
            ReusableInquiryPatterns: reusableInquiryPatterns,
            TriggerConditions: triggerConditions,
            PreservedConstraints: preservedConstraints,
            BoundaryPairCount: sharedBoundaryMemory.SharedBoundaryCodes.Count,
            IdentityBleedDenied: !sharedBoundaryMemory.IdentityBleedDetected,
            ReasonCode: "inquiry-pattern-continuity-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static QuestioningBoundaryPairLedgerReceipt CreateQuestioningBoundaryPairLedger(
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        ExpressiveDeformationReceipt deformationReceipt,
        SharedBoundaryMemoryLedgerReceipt sharedBoundaryMemory,
        string ledgerState = "questioning-boundary-pair-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(deformationReceipt);
        ArgumentNullException.ThrowIfNull(sharedBoundaryMemory);
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(deformationReceipt.ReceiptHandle, ExpressiveDeformationReceiptPrefix, nameof(deformationReceipt));
        EnsurePrefix(sharedBoundaryMemory.LedgerHandle, SharedBoundaryMemoryLedgerPrefix, nameof(sharedBoundaryMemory));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(operatorInquiryEnvelope.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(operatorInquiryEnvelope.CMEId, deformationReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(operatorInquiryEnvelope.CMEId, sharedBoundaryMemory.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Questioning boundary pairing requires inquiry envelope, pressure continuity, expressive deformation, and shared boundary memory to remain inside one bonded CME surface.");
        }

        var inquiryPatterns = operatorInquiryEnvelope.AvailableInquiryStances
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var supportingBoundaries = sharedBoundaryMemory.SharedBoundaryCodes
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var boundaryConstraints = sharedBoundaryMemory.SharedContinuityRequirements
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var overreachWarnings = operatorInquiryEnvelope.KnownBoundaryWarnings
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new QuestioningBoundaryPairLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateQuestioningBoundaryPairLedgerHandle(
                operatorInquiryEnvelope.CMEId,
                operatorInquiryEnvelope.EnvelopeHandle,
                continuityLedger.LedgerHandle,
                deformationReceipt.ReceiptHandle),
            CMEId: operatorInquiryEnvelope.CMEId,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            DeformationReceiptHandle: deformationReceipt.ReceiptHandle,
            SharedBoundaryMemoryLedgerHandle: sharedBoundaryMemory.LedgerHandle,
            LedgerState: ledgerState.Trim(),
            InquiryPatterns: inquiryPatterns,
            SupportingBoundaries: supportingBoundaries,
            BoundaryConstraints: boundaryConstraints,
            OverreachWarnings: overreachWarnings,
            ConstraintMemoryPreserved: true,
            ReasonCode: "questioning-boundary-pair-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static CarryForwardInquirySelectionSurfaceReceipt CreateCarryForwardInquirySelectionSurface(
        InquiryPatternContinuityLedgerReceipt inquiryPatternLedger,
        QuestioningBoundaryPairLedgerReceipt boundaryPairLedger,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string surfaceState = "carry-forward-inquiry-selection-surface-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(inquiryPatternLedger);
        ArgumentNullException.ThrowIfNull(boundaryPairLedger);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(localityWitness);
        EnsurePrefix(inquiryPatternLedger.LedgerHandle, InquiryPatternContinuityLedgerPrefix, nameof(inquiryPatternLedger));
        EnsurePrefix(boundaryPairLedger.LedgerHandle, QuestioningBoundaryPairLedgerPrefix, nameof(boundaryPairLedger));
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceState);

        if (!string.Equals(inquiryPatternLedger.CMEId, boundaryPairLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(inquiryPatternLedger.CMEId, operatorInquiryEnvelope.CMEId, StringComparison.Ordinal) ||
            !string.Equals(inquiryPatternLedger.CMEId, localityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Carry-forward inquiry selection requires inquiry-pattern, boundary-pair, operator inquiry, and locality witness receipts to remain inside one bonded CME surface.");
        }

        var availableCarryForwardPatterns = inquiryPatternLedger.ReusableInquiryPatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var admittedReuseConditions = inquiryPatternLedger.PreservedConstraints
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var withheldReuseWarnings = boundaryPairLedger.OverreachWarnings
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new CarryForwardInquirySelectionSurfaceReceipt(
            SurfaceHandle: AgentiActualizationKeys.CreateCarryForwardInquirySelectionSurfaceHandle(
                inquiryPatternLedger.CMEId,
                inquiryPatternLedger.LedgerHandle,
                boundaryPairLedger.LedgerHandle,
                localityWitness.WitnessLedgerHandle),
            CMEId: inquiryPatternLedger.CMEId,
            InquiryPatternLedgerHandle: inquiryPatternLedger.LedgerHandle,
            BoundaryPairLedgerHandle: boundaryPairLedger.LedgerHandle,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            SurfaceState: surfaceState.Trim(),
            AvailableCarryForwardPatterns: availableCarryForwardPatterns,
            AdmittedReuseConditions: admittedReuseConditions,
            WithheldReuseWarnings: withheldReuseWarnings,
            LocalitySafeReview: !localityWitness.LocalityCollapseDetected,
            AmbientHabitDenied: true,
            ReasonCode: "carry-forward-inquiry-selection-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static EngramDistanceClassificationLedgerReceipt CreateEngramDistanceClassificationLedger(
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        InquiryPatternContinuityLedgerReceipt inquiryPatternLedger,
        QuestioningBoundaryPairLedgerReceipt boundaryPairLedger,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        MutualIntelligibilityWitnessReceipt mutualIntelligibilityWitness,
        string ledgerState = "engram-distance-classification-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(inquiryPatternLedger);
        ArgumentNullException.ThrowIfNull(boundaryPairLedger);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(mutualIntelligibilityWitness);
        EnsurePrefix(carryForwardInquirySelectionSurface.SurfaceHandle, CarryForwardInquirySelectionSurfacePrefix, nameof(carryForwardInquirySelectionSurface));
        EnsurePrefix(inquiryPatternLedger.LedgerHandle, InquiryPatternContinuityLedgerPrefix, nameof(inquiryPatternLedger));
        EnsurePrefix(boundaryPairLedger.LedgerHandle, QuestioningBoundaryPairLedgerPrefix, nameof(boundaryPairLedger));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(mutualIntelligibilityWitness.WitnessHandle, MutualIntelligibilityWitnessPrefix, nameof(mutualIntelligibilityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(carryForwardInquirySelectionSurface.CMEId, inquiryPatternLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, boundaryPairLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, mutualIntelligibilityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Engram distance classification requires carry-forward inquiry, inquiry continuity, boundary pairing, pressure continuity, and mutual intelligibility to remain inside one bonded CME surface.");
        }

        var adjacentRootPatterns = carryForwardInquirySelectionSurface.AvailableCarryForwardPatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var firstOrderOtherPatterns = boundaryPairLedger.InquiryPatterns
            .Except(adjacentRootPatterns, StringComparer.Ordinal)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var farOtherArtifacts = Array.Empty<string>();
        var classifiedPatterns = adjacentRootPatterns
            .Select(static pattern => new EngramDistanceClassificationEntry(
                PatternCode: pattern,
                DistanceClass: EngramDistanceClass.AdjacentRoot,
                SourceMode: "adjacent-constraint-observation",
                PromotionDisposition: "guarded-candidate-review"))
            .Concat(firstOrderOtherPatterns.Select(static pattern => new EngramDistanceClassificationEntry(
                PatternCode: pattern,
                DistanceClass: EngramDistanceClass.FirstOrderOther,
                SourceMode: "reported-or-reconstructed-constraint",
                PromotionDisposition: "candidate-only-retention")))
            .Concat(farOtherArtifacts.Select(static artifact => new EngramDistanceClassificationEntry(
                PatternCode: artifact,
                DistanceClass: EngramDistanceClass.FarOther,
                SourceMode: "far-other-artifact",
                PromotionDisposition: "narrative-archive-only")))
            .ToArray();
        var coRootPatternCount = 0;
        var adjacentRootPatternCount = adjacentRootPatterns.Length;
        var firstOrderOtherPatternCount = firstOrderOtherPatterns.Length;
        var farOtherArtifactCount = farOtherArtifacts.Length;
        var dominantDistanceClass = DetermineDominantDistanceClass(
            coRootPatternCount,
            adjacentRootPatternCount,
            firstOrderOtherPatternCount,
            farOtherArtifactCount);

        return new EngramDistanceClassificationLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateEngramDistanceClassificationLedgerHandle(
                carryForwardInquirySelectionSurface.CMEId,
                carryForwardInquirySelectionSurface.SurfaceHandle,
                inquiryPatternLedger.LedgerHandle,
                continuityLedger.LedgerHandle),
            CMEId: carryForwardInquirySelectionSurface.CMEId,
            CarryForwardInquirySelectionSurfaceHandle: carryForwardInquirySelectionSurface.SurfaceHandle,
            InquiryPatternLedgerHandle: inquiryPatternLedger.LedgerHandle,
            BoundaryPairLedgerHandle: boundaryPairLedger.LedgerHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            MutualIntelligibilityWitnessHandle: mutualIntelligibilityWitness.WitnessHandle,
            LedgerState: ledgerState.Trim(),
            DominantDistanceClass: dominantDistanceClass,
            ClassifiedPatterns: classifiedPatterns,
            CoRootPatternCount: coRootPatternCount,
            AdjacentRootPatternCount: adjacentRootPatternCount,
            FirstOrderOtherPatternCount: firstOrderOtherPatternCount,
            FarOtherArtifactCount: farOtherArtifactCount,
            PromotionFromFarOtherDenied: true,
            ReRootingRequiredForFarOther: farOtherArtifactCount > 0,
            ReasonCode: "engram-distance-classification-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static EngramPromotionRequirementsMatrixReceipt CreateEngramPromotionRequirementsMatrix(
        EngramDistanceClassificationLedgerReceipt classificationLedger,
        string matrixState = "engram-promotion-requirements-matrix-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(classificationLedger);
        EnsurePrefix(classificationLedger.LedgerHandle, EngramDistanceClassificationLedgerPrefix, nameof(classificationLedger));
        ArgumentException.ThrowIfNullOrWhiteSpace(matrixState);

        var requirementEntries = CreateDefaultEngramDistanceRequirementEntries();

        return new EngramPromotionRequirementsMatrixReceipt(
            MatrixHandle: AgentiActualizationKeys.CreateEngramPromotionRequirementsMatrixHandle(
                classificationLedger.CMEId,
                classificationLedger.LedgerHandle),
            CMEId: classificationLedger.CMEId,
            ClassificationLedgerHandle: classificationLedger.LedgerHandle,
            MatrixState: matrixState.Trim(),
            RequirementEntries: requirementEntries,
            BurdenScalingPreserved: true,
            PortableInheritanceRequiresVariation: true,
            ReasonCode: "engram-promotion-requirements-matrix-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static DistanceWeightedQuestioningAdmissionSurfaceReceipt CreateDistanceWeightedQuestioningAdmissionSurface(
        EngramDistanceClassificationLedgerReceipt classificationLedger,
        EngramPromotionRequirementsMatrixReceipt promotionRequirementsMatrix,
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        string surfaceState = "distance-weighted-questioning-admission-surface-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(classificationLedger);
        ArgumentNullException.ThrowIfNull(promotionRequirementsMatrix);
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        EnsurePrefix(classificationLedger.LedgerHandle, EngramDistanceClassificationLedgerPrefix, nameof(classificationLedger));
        EnsurePrefix(promotionRequirementsMatrix.MatrixHandle, EngramPromotionRequirementsMatrixPrefix, nameof(promotionRequirementsMatrix));
        EnsurePrefix(carryForwardInquirySelectionSurface.SurfaceHandle, CarryForwardInquirySelectionSurfacePrefix, nameof(carryForwardInquirySelectionSurface));
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceState);

        if (!string.Equals(classificationLedger.CMEId, promotionRequirementsMatrix.CMEId, StringComparison.Ordinal) ||
            !string.Equals(classificationLedger.CMEId, carryForwardInquirySelectionSurface.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Distance-weighted questioning admission requires distance classification, promotion requirements, and carry-forward inquiry to remain inside one bonded CME surface.");
        }

        var dominantRequirement = GetRequirementEntry(
            promotionRequirementsMatrix.RequirementEntries,
            classificationLedger.DominantDistanceClass);
        var admittedCandidatePatterns = classificationLedger.ClassifiedPatterns
            .Where(static entry => entry.DistanceClass is EngramDistanceClass.CoRoot or EngramDistanceClass.AdjacentRoot)
            .Select(static entry => entry.PatternCode)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var withheldCandidatePatterns = classificationLedger.ClassifiedPatterns
            .Where(static entry => entry.DistanceClass is EngramDistanceClass.FirstOrderOther or EngramDistanceClass.FarOther)
            .Select(static entry => entry.PatternCode)
            .Concat(carryForwardInquirySelectionSurface.WithheldReuseWarnings)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var requiredReentryBurdens = new[]
            {
                $"required-evidence-count:{dominantRequirement.RequiredEvidenceCount}",
                $"maximum-unknown-load:{dominantRequirement.MaximumUnknownLoad}",
                $"required-reentry-depth:{dominantRequirement.RequiredReentryDepth}",
                dominantRequirement.FreshConstraintContactRequired ? "fresh-constraint-contact-required" : "fresh-constraint-contact-not-required"
            }
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var promotionCeiling = dominantRequirement.PromotionCeiling;
        var distanceScalingPreserved = promotionRequirementsMatrix.BurdenScalingPreserved &&
            classificationLedger.PromotionFromFarOtherDenied;
        var farOtherPromotionDenied = classificationLedger.PromotionFromFarOtherDenied;
        var reRootingRequired = classificationLedger.ReRootingRequiredForFarOther ||
            dominantRequirement.FreshConstraintContactRequired;

        return new DistanceWeightedQuestioningAdmissionSurfaceReceipt(
            SurfaceHandle: AgentiActualizationKeys.CreateDistanceWeightedQuestioningAdmissionSurfaceHandle(
                classificationLedger.CMEId,
                classificationLedger.LedgerHandle,
                promotionRequirementsMatrix.MatrixHandle,
                carryForwardInquirySelectionSurface.SurfaceHandle),
            CMEId: classificationLedger.CMEId,
            ClassificationLedgerHandle: classificationLedger.LedgerHandle,
            PromotionRequirementsMatrixHandle: promotionRequirementsMatrix.MatrixHandle,
            CarryForwardInquirySelectionSurfaceHandle: carryForwardInquirySelectionSurface.SurfaceHandle,
            SurfaceState: surfaceState.Trim(),
            DominantDistanceClass: classificationLedger.DominantDistanceClass,
            PromotionCeiling: promotionCeiling,
            AdmittedCandidatePatterns: AllowsCandidateReview(promotionCeiling) ? admittedCandidatePatterns : Array.Empty<string>(),
            WithheldCandidatePatterns: withheldCandidatePatterns,
            RequiredReentryBurdens: requiredReentryBurdens,
            UnknownTolerance: dominantRequirement.MaximumUnknownLoad,
            DistanceScalingPreserved: distanceScalingPreserved,
            FarOtherPromotionDenied: farOtherPromotionDenied,
            ReRootingRequired: reRootingRequired,
            ReasonCode: "distance-weighted-questioning-admission-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static QuestioningOperatorCandidateLedgerReceipt CreateQuestioningOperatorCandidateLedger(
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        InquiryPatternContinuityLedgerReceipt inquiryPatternLedger,
        QuestioningBoundaryPairLedgerReceipt boundaryPairLedger,
        ContinuityUnderPressureLedgerReceipt continuityLedger,
        MutualIntelligibilityWitnessReceipt mutualIntelligibilityWitness,
        DistanceWeightedQuestioningAdmissionSurfaceReceipt distanceWeightedAdmissionSurface,
        string ledgerState = "questioning-operator-candidate-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(inquiryPatternLedger);
        ArgumentNullException.ThrowIfNull(boundaryPairLedger);
        ArgumentNullException.ThrowIfNull(continuityLedger);
        ArgumentNullException.ThrowIfNull(mutualIntelligibilityWitness);
        ArgumentNullException.ThrowIfNull(distanceWeightedAdmissionSurface);
        EnsurePrefix(carryForwardInquirySelectionSurface.SurfaceHandle, CarryForwardInquirySelectionSurfacePrefix, nameof(carryForwardInquirySelectionSurface));
        EnsurePrefix(inquiryPatternLedger.LedgerHandle, InquiryPatternContinuityLedgerPrefix, nameof(inquiryPatternLedger));
        EnsurePrefix(boundaryPairLedger.LedgerHandle, QuestioningBoundaryPairLedgerPrefix, nameof(boundaryPairLedger));
        EnsurePrefix(continuityLedger.LedgerHandle, ContinuityUnderPressureLedgerPrefix, nameof(continuityLedger));
        EnsurePrefix(mutualIntelligibilityWitness.WitnessHandle, MutualIntelligibilityWitnessPrefix, nameof(mutualIntelligibilityWitness));
        EnsurePrefix(distanceWeightedAdmissionSurface.SurfaceHandle, DistanceWeightedQuestioningAdmissionSurfacePrefix, nameof(distanceWeightedAdmissionSurface));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(carryForwardInquirySelectionSurface.CMEId, inquiryPatternLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, boundaryPairLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, continuityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, mutualIntelligibilityWitness.CMEId, StringComparison.Ordinal) ||
            !string.Equals(carryForwardInquirySelectionSurface.CMEId, distanceWeightedAdmissionSurface.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Questioning operator candidacy requires carry-forward selection, inquiry continuity, boundary pairing, pressure continuity, mutual intelligibility, and distance-weighted admission to remain inside one bonded CME surface.");
        }

        var eventBoundInquiryForms = distanceWeightedAdmissionSurface.WithheldCandidatePatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var candidateInquiryPatterns = distanceWeightedAdmissionSurface.AdmittedCandidatePatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var promotionEvidence = continuityLedger.HeldContinuities
            .Concat(new[]
            {
                mutualIntelligibilityWitness.SharedUnderstandingState,
                $"engram-distance:{distanceWeightedAdmissionSurface.DominantDistanceClass}"
            })
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var requiredReentryConditions = distanceWeightedAdmissionSurface.RequiredReentryBurdens
            .Concat(inquiryPatternLedger.PreservedConstraints)
            .Concat(inquiryPatternLedger.TriggerConditions)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var failureSignatureExpectations = boundaryPairLedger.OverreachWarnings
            .Concat(boundaryPairLedger.BoundaryConstraints)
            .Concat(distanceWeightedAdmissionSurface.WithheldCandidatePatterns)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new QuestioningOperatorCandidateLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateQuestioningOperatorCandidateLedgerHandle(
                carryForwardInquirySelectionSurface.CMEId,
                carryForwardInquirySelectionSurface.SurfaceHandle,
                continuityLedger.LedgerHandle,
                mutualIntelligibilityWitness.WitnessHandle,
                distanceWeightedAdmissionSurface.SurfaceHandle),
            CMEId: carryForwardInquirySelectionSurface.CMEId,
            CarryForwardInquirySelectionSurfaceHandle: carryForwardInquirySelectionSurface.SurfaceHandle,
            InquiryPatternLedgerHandle: inquiryPatternLedger.LedgerHandle,
            BoundaryPairLedgerHandle: boundaryPairLedger.LedgerHandle,
            ContinuityLedgerHandle: continuityLedger.LedgerHandle,
            MutualIntelligibilityWitnessHandle: mutualIntelligibilityWitness.WitnessHandle,
            DistanceWeightedAdmissionSurfaceHandle: distanceWeightedAdmissionSurface.SurfaceHandle,
            LedgerState: ledgerState.Trim(),
            DominantDistanceClass: distanceWeightedAdmissionSurface.DominantDistanceClass,
            PromotionCeiling: distanceWeightedAdmissionSurface.PromotionCeiling,
            EventBoundInquiryForms: eventBoundInquiryForms,
            CandidateInquiryPatterns: candidateInquiryPatterns,
            PromotionEvidence: promotionEvidence,
            RequiredReentryConditions: requiredReentryConditions,
            FailureSignatureExpectations: failureSignatureExpectations,
            HiddenAuthorityPatternsDenied: carryForwardInquirySelectionSurface.AmbientHabitDenied,
            IdentityBoundPatternsWithheld: inquiryPatternLedger.IdentityBleedDenied,
            DistanceScalingPreserved: distanceWeightedAdmissionSurface.DistanceScalingPreserved,
            FarOtherPromotionDenied: distanceWeightedAdmissionSurface.FarOtherPromotionDenied,
            ReasonCode: "questioning-operator-candidate-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static QuestioningGelPromotionGateReceipt CreateQuestioningGelPromotionGate(
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        OperatorInquirySelectionEnvelopeReceipt operatorInquiryEnvelope,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        DistanceWeightedQuestioningAdmissionSurfaceReceipt distanceWeightedAdmissionSurface,
        string gateState = "questioning-gel-promotion-gate-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(operatorInquiryEnvelope);
        ArgumentNullException.ThrowIfNull(localityWitness);
        ArgumentNullException.ThrowIfNull(distanceWeightedAdmissionSurface);
        EnsurePrefix(candidateLedger.LedgerHandle, QuestioningOperatorCandidateLedgerPrefix, nameof(candidateLedger));
        EnsurePrefix(carryForwardInquirySelectionSurface.SurfaceHandle, CarryForwardInquirySelectionSurfacePrefix, nameof(carryForwardInquirySelectionSurface));
        EnsurePrefix(operatorInquiryEnvelope.EnvelopeHandle, OperatorInquirySelectionEnvelopePrefix, nameof(operatorInquiryEnvelope));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        EnsurePrefix(distanceWeightedAdmissionSurface.SurfaceHandle, DistanceWeightedQuestioningAdmissionSurfacePrefix, nameof(distanceWeightedAdmissionSurface));
        ArgumentException.ThrowIfNullOrWhiteSpace(gateState);

        if (!string.Equals(candidateLedger.CMEId, carryForwardInquirySelectionSurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, operatorInquiryEnvelope.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, localityWitness.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, distanceWeightedAdmissionSurface.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Questioning GEL promotion requires candidate, carry-forward, operator inquiry, locality witness, and distance-weighted admission receipts to remain inside one bonded CME surface.");
        }

        var candidateInquiryPatterns = candidateLedger.CandidateInquiryPatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var satisfiedPromotionConditions = carryForwardInquirySelectionSurface.AdmittedReuseConditions
            .Concat(operatorInquiryEnvelope.LawfulUseConditions)
            .Concat(new[] { $"promotion-ceiling:{distanceWeightedAdmissionSurface.PromotionCeiling}" })
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var unmetPromotionConditions = candidateLedger.RequiredReentryConditions
            .Concat(distanceWeightedAdmissionSurface.ReRootingRequired ? new[] { "fresh-root-reentry-required" } : Array.Empty<string>())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var promotionWarnings = candidateLedger.FailureSignatureExpectations
            .Concat(distanceWeightedAdmissionSurface.WithheldCandidatePatterns)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        var localitySeparationPreserved = !localityWitness.LocalityCollapseDetected;
        var authoritySeparationPreserved = operatorInquiryEnvelope.RawGrantDenied && candidateLedger.HiddenAuthorityPatternsDenied;
        var truthSeekingInvariantPreserved = authoritySeparationPreserved &&
            candidateLedger.IdentityBoundPatternsWithheld &&
            distanceWeightedAdmissionSurface.DistanceScalingPreserved;
        var outcomeSeekingDenied = true;
        var promotionReviewAdmitted = localitySeparationPreserved &&
            authoritySeparationPreserved &&
            truthSeekingInvariantPreserved &&
            distanceWeightedAdmissionSurface.FarOtherPromotionDenied &&
            AllowsCandidateReview(distanceWeightedAdmissionSurface.PromotionCeiling) &&
            candidateInquiryPatterns.Length > 0;

        return new QuestioningGelPromotionGateReceipt(
            GateHandle: AgentiActualizationKeys.CreateQuestioningGelPromotionGateHandle(
                candidateLedger.CMEId,
                candidateLedger.LedgerHandle,
                operatorInquiryEnvelope.EnvelopeHandle,
                localityWitness.WitnessLedgerHandle,
                distanceWeightedAdmissionSurface.SurfaceHandle),
            CMEId: candidateLedger.CMEId,
            CandidateLedgerHandle: candidateLedger.LedgerHandle,
            CarryForwardInquirySelectionSurfaceHandle: carryForwardInquirySelectionSurface.SurfaceHandle,
            OperatorInquiryEnvelopeHandle: operatorInquiryEnvelope.EnvelopeHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            DistanceWeightedAdmissionSurfaceHandle: distanceWeightedAdmissionSurface.SurfaceHandle,
            GateState: gateState.Trim(),
            DominantDistanceClass: distanceWeightedAdmissionSurface.DominantDistanceClass,
            PromotionCeiling: distanceWeightedAdmissionSurface.PromotionCeiling,
            CandidateInquiryPatterns: candidateInquiryPatterns,
            SatisfiedPromotionConditions: satisfiedPromotionConditions,
            UnmetPromotionConditions: unmetPromotionConditions,
            PromotionWarnings: promotionWarnings,
            LocalitySeparationPreserved: localitySeparationPreserved,
            AuthoritySeparationPreserved: authoritySeparationPreserved,
            TruthSeekingInvariantPreserved: truthSeekingInvariantPreserved,
            OutcomeSeekingDenied: outcomeSeekingDenied,
            DistanceScalingPreserved: distanceWeightedAdmissionSurface.DistanceScalingPreserved,
            ReRootingRequired: distanceWeightedAdmissionSurface.ReRootingRequired,
            PromotionReviewAdmitted: promotionReviewAdmitted,
            ReasonCode: "questioning-gel-promotion-gate-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static ProtectedQuestioningPatternSurfaceReceipt CreateProtectedQuestioningPatternSurface(
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        CarryForwardInquirySelectionSurfaceReceipt carryForwardInquirySelectionSurface,
        LocalityDistinctionWitnessLedgerReceipt localityWitness,
        string surfaceState = "protected-questioning-pattern-surface-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(carryForwardInquirySelectionSurface);
        ArgumentNullException.ThrowIfNull(localityWitness);
        EnsurePrefix(candidateLedger.LedgerHandle, QuestioningOperatorCandidateLedgerPrefix, nameof(candidateLedger));
        EnsurePrefix(promotionGate.GateHandle, QuestioningGelPromotionGatePrefix, nameof(promotionGate));
        EnsurePrefix(carryForwardInquirySelectionSurface.SurfaceHandle, CarryForwardInquirySelectionSurfacePrefix, nameof(carryForwardInquirySelectionSurface));
        EnsurePrefix(localityWitness.WitnessLedgerHandle, "locality-distinction-witness-ledger://", nameof(localityWitness));
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceState);

        if (!string.Equals(candidateLedger.CMEId, promotionGate.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, carryForwardInquirySelectionSurface.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, localityWitness.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Protected questioning patterns require candidate, promotion gate, carry-forward inquiry, and locality witness receipts to remain inside one bonded CME surface.");
        }

        var reviewableCandidatePatterns = promotionGate.CandidateInquiryPatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var lawfulReviewEnvelopes = promotionGate.SatisfiedPromotionConditions
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();
        var withheldInteriorityWarnings = candidateLedger.FailureSignatureExpectations
            .Concat(carryForwardInquirySelectionSurface.WithheldReuseWarnings)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return new ProtectedQuestioningPatternSurfaceReceipt(
            SurfaceHandle: AgentiActualizationKeys.CreateProtectedQuestioningPatternSurfaceHandle(
                candidateLedger.CMEId,
                candidateLedger.LedgerHandle,
                promotionGate.GateHandle,
                localityWitness.WitnessLedgerHandle),
            CMEId: candidateLedger.CMEId,
            CandidateLedgerHandle: candidateLedger.LedgerHandle,
            PromotionGateHandle: promotionGate.GateHandle,
            CarryForwardInquirySelectionSurfaceHandle: carryForwardInquirySelectionSurface.SurfaceHandle,
            LocalityWitnessHandle: localityWitness.WitnessLedgerHandle,
            SurfaceState: surfaceState.Trim(),
            ReviewableCandidatePatterns: reviewableCandidatePatterns,
            LawfulReviewEnvelopes: lawfulReviewEnvelopes,
            WithheldInteriorityWarnings: withheldInteriorityWarnings,
            LocalitySafeLegibility: promotionGate.LocalitySeparationPreserved && !localityWitness.LocalityCollapseDetected,
            RawInteriorityDenied: true,
            AutomaticGrantDenied: true,
            ReasonCode: "protected-questioning-pattern-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static VariationTestedReentryLedgerReceipt CreateVariationTestedReentryLedger(
        DistanceWeightedQuestioningAdmissionSurfaceReceipt distanceWeightedAdmissionSurface,
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        ProtectedQuestioningPatternSurfaceReceipt protectedPatternSurface,
        string ledgerState = "variation-tested-reentry-ledger-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(distanceWeightedAdmissionSurface);
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(protectedPatternSurface);
        EnsurePrefix(distanceWeightedAdmissionSurface.SurfaceHandle, DistanceWeightedQuestioningAdmissionSurfacePrefix, nameof(distanceWeightedAdmissionSurface));
        EnsurePrefix(candidateLedger.LedgerHandle, QuestioningOperatorCandidateLedgerPrefix, nameof(candidateLedger));
        EnsurePrefix(promotionGate.GateHandle, QuestioningGelPromotionGatePrefix, nameof(promotionGate));
        EnsurePrefix(protectedPatternSurface.SurfaceHandle, ProtectedQuestioningPatternSurfacePrefix, nameof(protectedPatternSurface));
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerState);

        if (!string.Equals(distanceWeightedAdmissionSurface.CMEId, candidateLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, promotionGate.CMEId, StringComparison.Ordinal) ||
            !string.Equals(promotionGate.CMEId, protectedPatternSurface.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Variation-tested reentry requires weighted admission, candidate, promotion, and protected surfaces to remain inside one CME continuity lane.");
        }

        var survivingPatterns = candidateLedger.CandidateInquiryPatterns
            .Take(2)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var failedPatterns = candidateLedger.CandidateInquiryPatterns
            .Skip(survivingPatterns.Length)
            .Take(1)
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var requiredRetestPatterns = failedPatterns;
        var variationContexts = new[]
        {
            "operator-shift",
            "pressure-shift",
            "stance-shift"
        };
        var requiredReentryPassCount = survivingPatterns.Length;
        var variationBurdenSatisfied = distanceWeightedAdmissionSurface.RequiredReentryBurdens.Count >= requiredReentryPassCount &&
            candidateLedger.RequiredReentryConditions.Count >= requiredReentryPassCount;

        return new VariationTestedReentryLedgerReceipt(
            LedgerHandle: AgentiActualizationKeys.CreateVariationTestedReentryLedgerHandle(
                candidateLedger.CMEId,
                distanceWeightedAdmissionSurface.SurfaceHandle,
                promotionGate.GateHandle,
                protectedPatternSurface.SurfaceHandle),
            CMEId: candidateLedger.CMEId,
            DistanceWeightedAdmissionSurfaceHandle: distanceWeightedAdmissionSurface.SurfaceHandle,
            CandidateLedgerHandle: candidateLedger.LedgerHandle,
            PromotionGateHandle: promotionGate.GateHandle,
            ProtectedPatternSurfaceHandle: protectedPatternSurface.SurfaceHandle,
            LedgerState: ledgerState.Trim(),
            VariationContexts: variationContexts,
            SurvivingPatterns: survivingPatterns,
            FailedPatterns: failedPatterns,
            RequiredRetestPatterns: requiredRetestPatterns,
            RequiredReentryPassCount: requiredReentryPassCount,
            VariationBurdenSatisfied: variationBurdenSatisfied,
            PortablePatternsWithstoodVariation: survivingPatterns.Length > 0 && !promotionGate.ReRootingRequired,
            ReasonCode: "variation-tested-reentry-ledger-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static QuestioningAdmissionRefusalReceipt CreateQuestioningAdmissionRefusalReceipt(
        VariationTestedReentryLedgerReceipt variationTestedReentryLedger,
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        ProtectedQuestioningPatternSurfaceReceipt protectedPatternSurface,
        string receiptState = "questioning-admission-refusal-receipt-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(variationTestedReentryLedger);
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(protectedPatternSurface);
        EnsurePrefix(variationTestedReentryLedger.LedgerHandle, VariationTestedReentryLedgerPrefix, nameof(variationTestedReentryLedger));
        EnsurePrefix(candidateLedger.LedgerHandle, QuestioningOperatorCandidateLedgerPrefix, nameof(candidateLedger));
        EnsurePrefix(promotionGate.GateHandle, QuestioningGelPromotionGatePrefix, nameof(promotionGate));
        EnsurePrefix(protectedPatternSurface.SurfaceHandle, ProtectedQuestioningPatternSurfacePrefix, nameof(protectedPatternSurface));
        ArgumentException.ThrowIfNullOrWhiteSpace(receiptState);

        if (!string.Equals(variationTestedReentryLedger.CMEId, candidateLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, promotionGate.CMEId, StringComparison.Ordinal) ||
            !string.Equals(promotionGate.CMEId, protectedPatternSurface.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Questioning admission refusal requires reentry, candidate, promotion, and protected surfaces to remain inside one CME continuity lane.");
        }

        var refusedPatterns = variationTestedReentryLedger.RequiredRetestPatterns
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var refusalReasons = new[]
        {
            "weak-failure-signature-clarity",
            "single-context-variation-insufficient",
            "attractive-but-under-evidenced"
        };

        return new QuestioningAdmissionRefusalReceipt(
            ReceiptHandle: AgentiActualizationKeys.CreateQuestioningAdmissionRefusalReceiptHandle(
                candidateLedger.CMEId,
                variationTestedReentryLedger.LedgerHandle,
                promotionGate.GateHandle),
            CMEId: candidateLedger.CMEId,
            VariationTestedReentryLedgerHandle: variationTestedReentryLedger.LedgerHandle,
            CandidateLedgerHandle: candidateLedger.LedgerHandle,
            PromotionGateHandle: promotionGate.GateHandle,
            ProtectedPatternSurfaceHandle: protectedPatternSurface.SurfaceHandle,
            ReceiptState: receiptState.Trim(),
            RefusedPatterns: refusedPatterns,
            DeferredPatterns: Array.Empty<string>(),
            RefusalReasons: refusalReasons,
            AttractiveButUnderEvidencedDenied: refusedPatterns.Length > 0,
            ArchiveProtectionPreserved: true,
            DelayWithoutDisposalAllowed: true,
            ReasonCode: "questioning-admission-refusal-receipt-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static PromotionSeductionWatchReceipt CreatePromotionSeductionWatch(
        QuestioningOperatorCandidateLedgerReceipt candidateLedger,
        QuestioningGelPromotionGateReceipt promotionGate,
        VariationTestedReentryLedgerReceipt variationTestedReentryLedger,
        QuestioningAdmissionRefusalReceipt admissionRefusalReceipt,
        string watchState = "promotion-seduction-watch-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(candidateLedger);
        ArgumentNullException.ThrowIfNull(promotionGate);
        ArgumentNullException.ThrowIfNull(variationTestedReentryLedger);
        ArgumentNullException.ThrowIfNull(admissionRefusalReceipt);
        EnsurePrefix(candidateLedger.LedgerHandle, QuestioningOperatorCandidateLedgerPrefix, nameof(candidateLedger));
        EnsurePrefix(promotionGate.GateHandle, QuestioningGelPromotionGatePrefix, nameof(promotionGate));
        EnsurePrefix(variationTestedReentryLedger.LedgerHandle, VariationTestedReentryLedgerPrefix, nameof(variationTestedReentryLedger));
        EnsurePrefix(admissionRefusalReceipt.ReceiptHandle, QuestioningAdmissionRefusalReceiptPrefix, nameof(admissionRefusalReceipt));
        ArgumentException.ThrowIfNullOrWhiteSpace(watchState);

        if (!string.Equals(candidateLedger.CMEId, promotionGate.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, variationTestedReentryLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(candidateLedger.CMEId, admissionRefusalReceipt.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Promotion seduction watch requires candidate, promotion, reentry, and refusal receipts to remain inside one CME continuity lane.");
        }

        var seductionSignals = new[]
        {
            "elegance-without-variation",
            "repeat-success-single-context",
            "social-prestige-pressure"
        };
        var blockedPromotionVectors = new[]
        {
            "prestige-promotion",
            "emotional-certification",
            "almost-good-enough-canonization"
        };
        var driftWarnings = admissionRefusalReceipt.RefusalReasons
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new PromotionSeductionWatchReceipt(
            WatchHandle: AgentiActualizationKeys.CreatePromotionSeductionWatchHandle(
                candidateLedger.CMEId,
                candidateLedger.LedgerHandle,
                variationTestedReentryLedger.LedgerHandle,
                admissionRefusalReceipt.ReceiptHandle),
            CMEId: candidateLedger.CMEId,
            CandidateLedgerHandle: candidateLedger.LedgerHandle,
            PromotionGateHandle: promotionGate.GateHandle,
            VariationTestedReentryLedgerHandle: variationTestedReentryLedger.LedgerHandle,
            AdmissionRefusalReceiptHandle: admissionRefusalReceipt.ReceiptHandle,
            WatchState: watchState.Trim(),
            SeductionSignals: seductionSignals,
            BlockedPromotionVectors: blockedPromotionVectors,
            DriftWarnings: driftWarnings,
            PrestigeInflationDenied: true,
            EleganceBiasDenied: true,
            EmotionalCompulsionDenied: true,
            ReasonCode: "promotion-seduction-watch-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    private static EngramDistanceRequirementEntry[] CreateDefaultEngramDistanceRequirementEntries()
    {
        return
        [
            new EngramDistanceRequirementEntry(
                DistanceClass: EngramDistanceClass.CoRoot,
                RequiredEvidenceCount: 1,
                MaximumUnknownLoad: 0,
                RequiredReentryDepth: 1,
                PromotionCeiling: EngramPromotionCeiling.FastTrackCandidateReview,
                FreshConstraintContactRequired: false,
                RefusalConditions:
                [
                    "direct-constraint-witness-missing",
                    "identity-claim-detached-from-act"
                ]),
            new EngramDistanceRequirementEntry(
                DistanceClass: EngramDistanceClass.AdjacentRoot,
                RequiredEvidenceCount: 2,
                MaximumUnknownLoad: 1,
                RequiredReentryDepth: 1,
                PromotionCeiling: EngramPromotionCeiling.GuardedCandidateReview,
                FreshConstraintContactRequired: false,
                RefusalConditions:
                [
                    "candidate-invariant-unresolved",
                    "failure-signature-not-articulated"
                ]),
            new EngramDistanceRequirementEntry(
                DistanceClass: EngramDistanceClass.FirstOrderOther,
                RequiredEvidenceCount: 3,
                MaximumUnknownLoad: 0,
                RequiredReentryDepth: 2,
                PromotionCeiling: EngramPromotionCeiling.CandidateOnlyMemory,
                FreshConstraintContactRequired: true,
                RefusalConditions:
                [
                    "reported-pattern-lacks-reentry",
                    "portability-claimed-without-variation"
                ]),
            new EngramDistanceRequirementEntry(
                DistanceClass: EngramDistanceClass.FarOther,
                RequiredEvidenceCount: 3,
                MaximumUnknownLoad: 0,
                RequiredReentryDepth: 3,
                PromotionCeiling: EngramPromotionCeiling.NarrativeArchiveOnly,
                FreshConstraintContactRequired: true,
                RefusalConditions:
                [
                    "far-other-material-cannot-inherit",
                    "re-rooting-required-before-promotion"
                ])
        ];
    }

    private static EngramDistanceRequirementEntry GetRequirementEntry(
        IReadOnlyList<EngramDistanceRequirementEntry> requirementEntries,
        EngramDistanceClass distanceClass)
    {
        ArgumentNullException.ThrowIfNull(requirementEntries);

        var entry = requirementEntries.FirstOrDefault(candidate => candidate.DistanceClass == distanceClass);
        return entry ?? throw new InvalidOperationException($"Missing promotion requirement entry for distance class `{distanceClass}`.");
    }

    private static EngramDistanceClass DetermineDominantDistanceClass(
        int coRootPatternCount,
        int adjacentRootPatternCount,
        int firstOrderOtherPatternCount,
        int farOtherArtifactCount)
    {
        if (coRootPatternCount > 0)
        {
            return EngramDistanceClass.CoRoot;
        }

        if (adjacentRootPatternCount > 0)
        {
            return EngramDistanceClass.AdjacentRoot;
        }

        if (firstOrderOtherPatternCount > 0)
        {
            return EngramDistanceClass.FirstOrderOther;
        }

        return farOtherArtifactCount > 0
            ? EngramDistanceClass.FarOther
            : EngramDistanceClass.FirstOrderOther;
    }

    private static bool AllowsCandidateReview(EngramPromotionCeiling promotionCeiling)
    {
        return promotionCeiling is EngramPromotionCeiling.FastTrackCandidateReview or EngramPromotionCeiling.GuardedCandidateReview;
    }

    private static void RequireActualLocality(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.IndexOf(".actual", StringComparison.OrdinalIgnoreCase) < 0)
        {
            throw new ArgumentException($"{parameterName} must identify an `.actual` locality.", parameterName);
        }
    }

    private static void EnsurePrefix(string value, string prefix, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"{parameterName} must use the `{prefix}` handle class.", parameterName);
        }
    }
}
