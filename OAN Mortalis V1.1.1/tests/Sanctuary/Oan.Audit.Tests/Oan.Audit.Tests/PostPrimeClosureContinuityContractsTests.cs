namespace San.Audit.Tests;

using San.Common;

public sealed class PostPrimeClosureContinuityContractsTests
{
    [Fact]
    public void PostPrimeClosureContinuity_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PostPrimeClosureContinuityKind.FieldContinuing,
                PostPrimeClosureContinuityKind.CarrierActive,
                PostPrimeClosureContinuityKind.ResidueLawful,
                PostPrimeClosureContinuityKind.DeferredEdgeOpen,
                PostPrimeClosureContinuityKind.ReentryPermitted,
                PostPrimeClosureContinuityKind.ContinuityWithheld
            ],
            Enum.GetValues<PostPrimeClosureContinuityKind>());
    }

    [Fact]
    public void Executed_Closure_With_Preserved_Reentry_Attests_Bounded_Continuity()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            unresolvedResidues: []);
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Redoped,
            deferredResidues: [],
            activeResidues: CreateResidues(2));
        var closureRecord = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/executed",
            "product://prime-closure/executed");

        var record = PostPrimeClosureContinuityEvaluator.Evaluate(
            closureRecord,
            reopeningRecord,
            "record://post-prime-continuity/executed");

        Assert.Equal(PostPrimeClosureContinuityKind.ReentryPermitted, record.ContinuityKind);
        Assert.Equal("post-prime-continuity-reentry-permitted", record.ReasonCode);
        Assert.True(record.BearingFieldNonVoidAttested);
        Assert.True(record.ActiveCarriersStillPresent);
        Assert.True(record.LawfulResiduesStillPresent);
        Assert.True(record.ReentryPermitted);
    }

    [Fact]
    public void Declined_Closure_With_Deferred_Residues_Keeps_Deferred_Edge_Open()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            unresolvedResidues: []);
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Deferred,
            deferredResidues: CreateResidues(1));
        var closureRecord = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/declined",
            "product://prime-closure/declined");

        var record = PostPrimeClosureContinuityEvaluator.Evaluate(
            closureRecord,
            reopeningRecord,
            "record://post-prime-continuity/declined");

        Assert.Equal(PostPrimeClosureContinuityKind.DeferredEdgeOpen, record.ContinuityKind);
        Assert.Equal("post-prime-continuity-deferred-edge-open", record.ReasonCode);
        Assert.True(record.DeferredEdgesStillOpen);
        Assert.True(record.BearingFieldNonVoidAttested);
    }

    [Fact]
    public void Withheld_Closure_Keeps_Continuity_Withheld()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.RetainedWholeUnclosed,
            unresolvedResidues: []);
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Reopened,
            deferredResidues: []);
        var closureRecord = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/withheld",
            "product://prime-closure/withheld");

        var record = PostPrimeClosureContinuityEvaluator.Evaluate(
            closureRecord,
            reopeningRecord,
            "record://post-prime-continuity/withheld");

        Assert.Equal(PostPrimeClosureContinuityKind.ContinuityWithheld, record.ContinuityKind);
        Assert.Equal("post-prime-continuity-withheld-closure-not-executed", record.ReasonCode);
        Assert.False(record.ReentryPermitted);
    }

    [Fact]
    public void Docs_Record_Post_Prime_Closure_Continuity_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "POST_PRIME_CLOSURE_CONTINUITY_LAW.md");
        var closureLawPath = Path.Combine(lineRoot, "docs", "PRIME_CLOSURE_ACT_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var closureLawText = File.ReadAllText(closureLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("shall attest what remains lawfully active after explicit Prime closure", lawText, StringComparison.Ordinal);
        Assert.Contains("closure of a product shall not imply voiding of the bearing field", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not imply that all field matter remains equally open or unresolved", lawText, StringComparison.Ordinal);
        Assert.Contains("POST_PRIME_CLOSURE_CONTINUITY_LAW.md", closureLawText, StringComparison.Ordinal);
        Assert.Contains("POST_PRIME_CLOSURE_CONTINUITY_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("post-prime-closure-continuity-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Post-Prime closure continuity", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("re-entrant path inheritance and deep-store navigation beyond post-Prime closure continuity", refinementText, StringComparison.Ordinal);
    }

    private static PrimeRetainedHistoryRecord CreateRetainedHistory(
        PrimeRetainedWholeKind retainedWholeKind,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> unresolvedResidues)
    {
        return new PrimeRetainedHistoryRecord(
            RecordHandle: "record://prime-retained-whole/session-b",
            MembraneHistoryReceiptHandle: "receipt://prime-membrane/history/session-b",
            HistoryHandle: "history://prime-membrane/session-b",
            MembraneHandle: "membrane://prime/session-b",
            ProjectionHandle: "projection://cryptic/session-b",
            ReceiptKind: retainedWholeKind == PrimeRetainedWholeKind.ClosureCandidate
                ? PrimeMembraneReceiptKind.ReturnBearingUnclosed
                : PrimeMembraneReceiptKind.ReceiptedHistory,
            RetainedWholeKind: retainedWholeKind,
            AdvisoryClosureEligibility: retainedWholeKind == PrimeRetainedWholeKind.ClosureCandidate
                ? PrimeClosureEligibilityKind.EligibleForMembraneReceipt
                : PrimeClosureEligibilityKind.ReviewRequired,
            PreservedDistinctionVisible: true,
            RetainedWholenessStillBounded: true,
            ExplicitClosureActStillRequired: true,
            PrimeClosureStillWithheld: true,
            RetainedResidues: CreateResidues(2),
            UnresolvedResidues: unresolvedResidues,
            ConstraintCodes: ["prime-retained-whole-test"],
            ReasonCode: "prime-retained-whole-test",
            LawfulBasis: "prime retained whole test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 00, 00, TimeSpan.Zero));
    }

    private static LawfulReopeningParticipationRecord CreateReopeningRecord(
        PrimeRetainedHistoryRecord retainedHistory,
        ContinuedParticipationStateKind participationState,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> deferredResidues,
        IReadOnlyList<PrimeMembraneProjectedLineResidue>? activeResidues = null)
    {
        return new LawfulReopeningParticipationRecord(
            RecordHandle: "record://lawful-reopening/session-b",
            CommunicativeReceiptHandle: "receipt://communicative-filament/session-b",
            CarrierHandle: "carrier://communicative-filament/session-b",
            SourceRetainedWholeRecordHandle: retainedHistory.RecordHandle,
            SourceCarrierClass: CommunicativeCarrierClassKind.ChosenPathBearing,
            SourceResolutionTarget: FilamentResolutionTargetKind.SelfGEL,
            SourceAntiEchoDisposition: AntiEchoDispositionKind.Preserve,
            ReopeningMode: participationState == ContinuedParticipationStateKind.Redoped
                ? ReopeningModeKind.RedopedReopen
                : ReopeningModeKind.SimpleReopen,
            ParticipationState: participationState,
            PriorReceiptHistoryPreserved: true,
            PriorPassageStillVisible: true,
            FalseFreshStartRefused: true,
            PreservedDistinctionVisible: true,
            AdjacentPossibilityPreserved: participationState != ContinuedParticipationStateKind.Deferred,
            GelSubstanceStillUnconsumed: true,
            PointerHandles: ["pointer://history"],
            SymbolicCarrierHandles: ["carrier://projection"],
            ReopeningMarkers: ["marker://reopen"],
            ActiveResidues: activeResidues ?? CreateResidues(2),
            DeferredResidues: deferredResidues,
            ConstraintCodes: ["lawful-reopening-test"],
            ReasonCode: "lawful-reopening-test",
            LawfulBasis: "lawful reopening test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 09, 05, 00, TimeSpan.Zero));
    }

    private static IReadOnlyList<PrimeMembraneProjectedLineResidue> CreateResidues(
        int count)
    {
        return Enumerable.Range(1, count)
            .Select(index => new PrimeMembraneProjectedLineResidue(
                LineHandle: $"line://{index}",
                SourceSurfaceHandle: $"surface://{index}",
                ResidualPosture: CrypticProjectionPostureKind.Braided,
                ParticipationKind: index % 2 == 0
                    ? PrimeMembraneProjectedParticipationKind.Swarmed
                    : PrimeMembraneProjectedParticipationKind.Clustered,
                AcceptedContributionHandles: [$"contribution://{index}"],
                DistinctionPreserved: true,
                ResidueNotes: ["per-line-distinction-preserved"]))
            .ToArray();
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
