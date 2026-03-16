using CradleTek.Host.Interfaces;
using EngramGovernance.Services;
using Oan.Common;
using Telemetry.GEL;

namespace Oan.Audit.Tests;

public sealed class StewardAgentGovernanceTests
{
    [Fact]
    public async Task ApprovedCandidate_AuthorizesReengrammitization_AndPrimePublication()
    {
        var steward = CreateSteward();
        var request = CreateRequest();

        var adjudication = await steward.AdjudicateAsync(request);

        Assert.Equal(GovernanceDecision.Approved, adjudication.Receipt.Decision);
        Assert.Equal("Steward Agent", adjudication.Receipt.AdjudicatorIdentity);
        Assert.True(adjudication.Receipt.ReengrammitizationAuthorized);
        Assert.True(adjudication.Receipt.PrimePublicationAuthorized);
        Assert.Equal(
            GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView,
            adjudication.Receipt.AuthorizedDerivativeLanes);
        Assert.Equal(ControlMutationOutcome.Authorized, adjudication.Receipt.MutationReceipt.Outcome);
        Assert.Equal(ControlSurfaceKind.GovernanceDecision, adjudication.Receipt.MutationReceipt.TargetSurface);
        Assert.NotNull(adjudication.ReengrammitizationRequest);
        Assert.NotNull(adjudication.PrimePublicationRequest);
    }

    [Fact]
    public async Task RejectedCandidate_DoesNotAuthorizeDownstreamActs()
    {
        var steward = CreateSteward();
        var request = CreateRequest() with
        {
            SessionHandle = "invalid-session",
            WorkingStateHandle = "cmos://sovereign"
        };

        var adjudication = await steward.AdjudicateAsync(request);

        Assert.Equal(GovernanceDecision.Rejected, adjudication.Receipt.Decision);
        Assert.False(adjudication.Receipt.ReengrammitizationAuthorized);
        Assert.False(adjudication.Receipt.PrimePublicationAuthorized);
        Assert.Equal(GovernedPrimeDerivativeLane.Neither, adjudication.Receipt.AuthorizedDerivativeLanes);
        Assert.Equal(ControlMutationOutcome.Refused, adjudication.Receipt.MutationReceipt.Outcome);
        Assert.Null(adjudication.ReengrammitizationRequest);
        Assert.Null(adjudication.PrimePublicationRequest);
    }

    [Fact]
    public async Task DeferredCandidate_IsPersistedForReview_WithoutMutationOrPublication()
    {
        var steward = CreateSteward();
        var request = CreateRequest() with
        {
            IntakeIntent = "defer-review"
        };

        var adjudication = await steward.AdjudicateAsync(request);
        var deferred = await steward.ListDeferredCandidatesAsync();

        Assert.Equal(GovernanceDecision.Deferred, adjudication.Receipt.Decision);
        Assert.False(adjudication.Receipt.ReengrammitizationAuthorized);
        Assert.False(adjudication.Receipt.PrimePublicationAuthorized);
        Assert.Null(adjudication.ReengrammitizationRequest);
        Assert.Null(adjudication.PrimePublicationRequest);
        Assert.Equal(ControlMutationOutcome.Deferred, adjudication.Receipt.MutationReceipt.Outcome);
        Assert.Single(deferred);
        Assert.Equal(request.CandidateId, deferred[0].CandidateId);
    }

    private static StewardAgent CreateSteward()
    {
        var telemetry = new GelTelemetryAdapter();
        return new StewardAgent(
            new OntologicalCleaver(),
            new EncryptionService(),
            new LedgerWriter(telemetry),
            engramBootstrap: null,
            constructorGuidance: null,
            new RecordingPublicStore(),
            new RecordingCrypticStore(),
            telemetry);
    }

    private static ReturnCandidateReviewRequest CreateRequest()
    {
        return new ReturnCandidateReviewRequest(
            CandidateId: Guid.NewGuid(),
            IdentityId: Guid.NewGuid(),
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-governance",
            ContextId: Guid.NewGuid(),
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: "soulframe-session://cme-governance/session",
            WorkingStateHandle: "soulframe-working://cme-governance/state",
            ReturnCandidatePointer: "agenticore-return://candidate/approved",
            ProvenanceMarker: "membrane-derived:cme:cme-governance|policy:agenticore.cognition.cycle",
            IntakeIntent: "candidate-return-evaluation",
            SubmittedBy: "AgentiCore",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: new CmeCollapseClassification(
                CollapseConfidence: 0.92,
                SelfGelIdentified: true,
                AutobiographicalRelevant: true,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"),
            RequestEnvelope: ControlSurfaceContractGuards.CreateRequestEnvelope(
                targetSurface: ControlSurfaceKind.StewardReturnReview,
                requestedBy: "CradleTek",
                scopeHandle: "soulframe-session://cme-governance/session",
                protectionClass: "cryptic-review",
                witnessRequirement: "governance-witness",
                actionableContent: new GovernedActionableContent(
                    ContentHandle: "agenticore-return://candidate/approved",
                    Kind: ActionableContentKind.ReturnCandidate,
                    OriginSurface: "prime",
                    ProvenanceMarker: "membrane-derived:cme:cme-governance|policy:agenticore.cognition.cycle",
                    SourceSubsystem: "AgentiCore",
                    PayloadClass: "return-candidate",
                    TraceReference: null,
                    ResidueReference: null)));
    }

    private sealed class RecordingPublicStore : IPublicStore
    {
        public string ContainerName => "public";

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task PublishPointerAsync(string pointer, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<string>> ListPublishedPointersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);
    }

    private sealed class RecordingCrypticStore : ICrypticStore
    {
        public string ContainerName => "cryptic";

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<string> StorePointerAsync(string pointer, CancellationToken cancellationToken = default) =>
            Task.FromResult(pointer);

        public Task<IReadOnlyList<string>> ListPointersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);
    }
}
