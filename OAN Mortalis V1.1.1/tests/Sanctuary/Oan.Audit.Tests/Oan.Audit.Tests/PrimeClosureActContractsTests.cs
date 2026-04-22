namespace San.Audit.Tests;

using San.Common;

public sealed class PrimeClosureActContractsTests
{
    [Fact]
    public void PrimeClosureAct_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrimeClosureActKind.ClosureWithheld,
                PrimeClosureActKind.ClosureDeclined,
                PrimeClosureActKind.ClosureExecuted
            ],
            Enum.GetValues<PrimeClosureActKind>());
    }

    [Fact]
    public void NonCandidate_RetainedHistory_Keeps_Closure_Withheld()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.RetainedWholeUnclosed,
            unresolvedResidues: []);
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Reopened,
            deferredResidues: []);

        var record = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/withheld",
            "product://prime-closure/withheld");

        Assert.Equal(PrimeClosureActKind.ClosureWithheld, record.ClosureActKind);
        Assert.Equal("prime-closure-withheld-not-closure-candidate", record.ReasonCode);
        Assert.Empty(record.AttestedRemainingProductResidues);
    }

    [Fact]
    public void Candidate_With_Deferred_Reopening_Declines_Closure()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            unresolvedResidues: []);
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Deferred,
            deferredResidues: CreateResidues(1));

        var record = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/declined-reopening",
            "product://prime-closure/declined-reopening");

        Assert.Equal(PrimeClosureActKind.ClosureDeclined, record.ClosureActKind);
        Assert.Equal("prime-closure-declined-reopening-deferred", record.ReasonCode);
        Assert.True(record.DeferredResiduesStillVisible);
    }

    [Fact]
    public void Candidate_With_Unresolved_Or_Deferred_Residues_Declines_Closure()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            unresolvedResidues: CreateResidues(1));
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Redoped,
            deferredResidues: []);

        var unresolvedRecord = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/declined-unresolved",
            "product://prime-closure/declined-unresolved");

        Assert.Equal(PrimeClosureActKind.ClosureDeclined, unresolvedRecord.ClosureActKind);
        Assert.Equal("prime-closure-declined-unresolved-residues-visible", unresolvedRecord.ReasonCode);

        var cleanRetainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            unresolvedResidues: []);
        var deferredReopening = CreateReopeningRecord(
            cleanRetainedHistory,
            participationState: ContinuedParticipationStateKind.Redoped,
            deferredResidues: CreateResidues(1));

        var deferredRecord = PrimeClosureActEvaluator.Evaluate(
            cleanRetainedHistory,
            deferredReopening,
            "record://prime-closure/declined-deferred",
            "product://prime-closure/declined-deferred");

        Assert.Equal(PrimeClosureActKind.ClosureDeclined, deferredRecord.ClosureActKind);
        Assert.Equal("prime-closure-declined-deferred-residues-visible", deferredRecord.ReasonCode);
    }

    [Fact]
    public void Candidate_With_Clean_Reopening_Executes_Closure_Without_Voiding_Field()
    {
        var retainedHistory = CreateRetainedHistory(
            retainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            unresolvedResidues: []);
        var reopeningRecord = CreateReopeningRecord(
            retainedHistory,
            participationState: ContinuedParticipationStateKind.Redoped,
            deferredResidues: [],
            activeResidues: CreateResidues(2));

        var record = PrimeClosureActEvaluator.Evaluate(
            retainedHistory,
            reopeningRecord,
            "record://prime-closure/executed",
            "product://prime-closure/executed");

        Assert.Equal(PrimeClosureActKind.ClosureExecuted, record.ClosureActKind);
        Assert.Equal("remaining-product-only", record.ClosureScope);
        Assert.Equal("prime-closure-executed-remaining-product-attested", record.ReasonCode);
        Assert.True(record.BearingFieldStillVisible);
        Assert.True(record.GelSubstanceStillUnconsumed);
        Assert.Equal(2, record.AttestedRemainingProductResidues.Count);
    }

    [Fact]
    public void Docs_Record_Prime_Closure_Act_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIME_CLOSURE_ACT_LAW.md");
        var reopeningLawPath = Path.Combine(lineRoot, "docs", "LAWFUL_REOPENING_REDOPING_AND_CONTINUED_PARTICIPATION_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var reopeningLawText = File.ReadAllText(reopeningLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("shall be an explicit attestation that a lawful product now stands and remains from the process", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not imply reset", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not void prior passage", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not imply that the bearing field became empty", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_CLOSURE_ACT_LAW.md", reopeningLawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_CLOSURE_ACT_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("prime-closure-act-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Prime closure act", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("re-entrant path inheritance and deep-store navigation beyond post-Prime closure continuity", refinementText, StringComparison.Ordinal);
    }

    private static PrimeRetainedHistoryRecord CreateRetainedHistory(
        PrimeRetainedWholeKind retainedWholeKind,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> unresolvedResidues)
    {
        return new PrimeRetainedHistoryRecord(
            RecordHandle: "record://prime-retained-whole/session-a",
            MembraneHistoryReceiptHandle: "receipt://prime-membrane/history/session-a",
            HistoryHandle: "history://prime-membrane/session-a",
            MembraneHandle: "membrane://prime/session-a",
            ProjectionHandle: "projection://cryptic/session-a",
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
            RecordHandle: "record://lawful-reopening/session-a",
            CommunicativeReceiptHandle: "receipt://communicative-filament/session-a",
            CarrierHandle: "carrier://communicative-filament/session-a",
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
