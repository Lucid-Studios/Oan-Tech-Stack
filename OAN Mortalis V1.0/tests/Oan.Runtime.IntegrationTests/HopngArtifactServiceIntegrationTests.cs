using Oan.Common;
using Oan.Cradle;

namespace Oan.Runtime.IntegrationTests;

public sealed class HopngArtifactServiceIntegrationTests
{
    [Fact]
    public async Task UnavailableService_EmitsExplicitUnavailableReceipt()
    {
        var service = new UnavailableHopngArtifactService();
        var request = CreateEmissionRequest();

        var receipt = await service.EmitAsync(request);

        Assert.Equal(GovernedHopngArtifactOutcome.Unavailable, receipt.Outcome);
        Assert.Equal(request.LoopKey, receipt.LoopKey);
        Assert.Equal(request.Profile, receipt.Profile);
        Assert.Equal("hopng-bridge-unavailable", receipt.FailureCode);
        Assert.Null(receipt.ManifestPath);
        Assert.Null(receipt.ProjectionPath);
    }

    [Fact]
    public void EvidenceReferences_IncludeTargetWitnessHandlesFromSnapshot()
    {
        var request = CreateEmissionRequest(includeTargetWitnessReceipts: true);

        var refs = GovernedHopngEvidenceReferences.Build(request, request.Snapshot);

        Assert.Contains(refs, reference => reference.PointerUri == "target-witness://admission-accepted/aaaaaaaaaaaaaaaa");
        Assert.Contains(refs, reference => reference.PointerUri == "target-lineage://bbbbbbbbbbbbbbbb");
        Assert.Contains(refs, reference => reference.PointerUri == "target-trace://cccccccccccccccc");
        Assert.Contains(refs, reference => reference.PointerUri == "target-residue://dddddddddddddddd");
    }

    private static GovernedHopngEmissionRequest CreateEmissionRequest(bool includeTargetWitnessReceipts = false)
    {
        var actionableContent = ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
            contentHandle: "agenticore-return://candidate/test",
            originSurface: "prime",
            provenanceMarker: "membrane-derived:cme:cme-runtime|policy:agenticore.cognition.cycle",
            sourceSubsystem: "AgentiCore");
        var reviewEnvelope = ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.StewardReturnReview,
            requestedBy: "CradleTek",
            scopeHandle: "soulframe-session://cme-runtime/test",
            protectionClass: "cryptic-review",
            witnessRequirement: "governance-witness",
            actionableContent: actionableContent,
            parentEnvelopeId: "control-envelope://seed");
        var decisionReceipt = new GovernanceDecisionReceipt(
            CandidateId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            IdempotencyKey: "loop:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:test",
            CandidateProvenance: actionableContent.ProvenanceMarker,
            Decision: GovernanceDecision.Approved,
            AdjudicatorIdentity: "Steward Agent",
            RationaleCode: "steward.approved.governed-loop",
            Timestamp: DateTime.UtcNow,
            ReengrammitizationAuthorized: true,
            PrimePublicationAuthorized: true,
            AuthorizedDerivativeLanes: GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            MutationReceipt: ControlSurfaceContractGuards.CreateMutationReceipt(
                envelopeId: reviewEnvelope.EnvelopeId,
                contentHandle: actionableContent.ContentHandle,
                targetSurface: ControlSurfaceKind.GovernanceDecision,
                outcome: ControlMutationOutcome.Authorized,
                governedBy: "Steward Agent",
                decisionCode: "steward.approved.governed-loop",
                timestampUtc: DateTimeOffset.UtcNow));
        var reviewRequest = new ReturnCandidateReviewRequest(
            CandidateId: decisionReceipt.CandidateId,
            IdentityId: Guid.NewGuid(),
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-runtime",
            ContextId: Guid.NewGuid(),
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-runtime/test",
            WorkingStateHandle: "soulframe-working://cme-runtime/test",
            ReturnCandidatePointer: actionableContent.ContentHandle,
            ProvenanceMarker: actionableContent.ProvenanceMarker,
            IntakeIntent: "candidate-return-evaluation",
            SubmittedBy: "CradleTek",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: new CmeCollapseClassification(
                CollapseConfidence: 0.92,
                SelfGelIdentified: true,
                AutobiographicalRelevant: true,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"),
            RequestEnvelope: reviewEnvelope);
        var snapshot = new GovernanceLoopStateSnapshot(
            LoopKey: decisionReceipt.IdempotencyKey,
            Stage: GovernanceLoopStage.LoopCompleted,
            DecisionReceipt: decisionReceipt,
            ReviewRequest: reviewRequest,
            ReengrammitizationReceipt: null,
            PublishedLanes: GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            FirstRouteCompleted: true,
            FirstRouteDisposition: CmeCollapseDisposition.RouteToCMoS,
            LatestCollapseQualification: new CmeCollapseQualificationView(
                Destination: "cMoS",
                ResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                ClassificationConfidence: 0.92,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                ReviewState: CmeCollapseReviewState.None,
                SourceSubsystem: "AgentiCore"),
            ReengrammitizationCompleted: true,
            IsTerminal: true,
            FailureCode: null,
            FailureStage: null,
            JournalIntegrityErrorCount: 0,
            HopngArtifacts: [],
            TargetWitnessReceipts: includeTargetWitnessReceipts ? [CreateTargetWitnessReceipt()] : []);

        return new GovernedHopngEmissionRequest(
            LoopKey: snapshot.LoopKey,
            CandidateId: decisionReceipt.CandidateId,
            CandidateProvenance: decisionReceipt.CandidateProvenance,
            Profile: GovernedHopngArtifactProfile.GoverningTrafficEvidence,
            Stage: snapshot.Stage,
            RequestedBy: "CradleTek",
            DecisionReceipt: decisionReceipt,
            Snapshot: snapshot,
            JournalEntries: [],
            CollapseRoutingDecision: new CmeCollapseRoutingDecision(
                Disposition: CmeCollapseDisposition.RouteToCMoS,
                ResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                ReviewState: CmeCollapseReviewState.None,
                ReasonCode: decisionReceipt.RationaleCode,
                IssuedBy: decisionReceipt.AdjudicatorIdentity,
                IssuedAt: decisionReceipt.Timestamp,
                TargetClass: "cMoS",
                ClassificationConfidence: 0.92,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"));
    }

    private static GovernedTargetWitnessReceipt CreateTargetWitnessReceipt()
    {
        return new GovernedTargetWitnessReceipt(
            WitnessHandle: "target-witness://admission-accepted/aaaaaaaaaaaaaaaa",
            Stage: GovernanceLoopStage.BoundedCognitionCompleted,
            Kind: GovernedTargetWitnessKind.AdmissionAccepted,
            Accepted: true,
            WitnessedBy: "CradleTek",
            LaneId: "higher-order-locality",
            RuntimeId: "gc-locality-runtime",
            ProfileId: "gc-locality-profile",
            BudgetClass: "target-bounded-lane",
            CommitAuthorityClass: "refusal-only",
            Objective: "identity-continuity",
            ProgramId: "program-001",
            AdmissionHandle: "target-admission://eeeeeeeeeeeeeeee",
            LineageHandle: "target-lineage://bbbbbbbbbbbbbbbb",
            TraceHandle: "target-trace://cccccccccccccccc",
            ResidueHandle: "target-residue://dddddddddddddddd",
            Reasons: [],
            ReasonFamilies: [],
            BudgetUsage: new GovernedTargetExecutionBudgetUsage(
                InstructionCount: 3,
                SymbolicDepth: 3,
                ProjectedTraceEntryCount: 5,
                ProjectedResidueCount: 1,
                WitnessOperationCount: 0,
                TransportOperationCount: 0),
            EmittedTraceCount: 5,
            EmittedResidueCount: 1,
            TimestampUtc: DateTimeOffset.UtcNow);
    }
}
