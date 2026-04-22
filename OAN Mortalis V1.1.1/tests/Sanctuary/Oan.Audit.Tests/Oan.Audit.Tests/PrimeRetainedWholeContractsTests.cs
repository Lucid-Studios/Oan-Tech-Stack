namespace San.Audit.Tests;

using System.Text.Json;
using San.Common;

public sealed class PrimeRetainedWholeContractsTests
{
    [Fact]
    public void RetainedWhole_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrimeRetainedWholeKind.NotRetained,
                PrimeRetainedWholeKind.RetainedPartial,
                PrimeRetainedWholeKind.RetainedWholeUnclosed,
                PrimeRetainedWholeKind.ClosureCandidate,
                PrimeRetainedWholeKind.StillDeferred
            ],
            Enum.GetValues<PrimeRetainedWholeKind>());
    }

    [Fact]
    public void SeenOnly_History_Remains_NotRetained()
    {
        var receipt = CreateHistoryReceipt(
            receiptKind: PrimeMembraneReceiptKind.SeenOnly,
            visibleResidues: CreateResidues(2),
            deferredResidues: CreateResidues(2));

        var record = PrimeRetainedWholeEvaluator.Evaluate(
            receipt,
            "record://prime-retained-whole/not-retained");

        Assert.Equal(PrimeRetainedWholeKind.NotRetained, record.RetainedWholeKind);
        Assert.Empty(record.RetainedResidues);
        Assert.Equal(2, record.UnresolvedResidues.Count);
        Assert.Equal("prime-retained-whole-not-retained", record.ReasonCode);
    }

    [Fact]
    public void ReceiptedHistory_With_One_Residue_Becomes_RetainedPartial()
    {
        var receipt = CreateHistoryReceipt(
            receiptKind: PrimeMembraneReceiptKind.ReceiptedHistory,
            visibleResidues: CreateResidues(1),
            deferredResidues: []);

        var record = PrimeRetainedWholeEvaluator.Evaluate(
            receipt,
            "record://prime-retained-whole/partial");

        Assert.Equal(PrimeRetainedWholeKind.RetainedPartial, record.RetainedWholeKind);
        Assert.Single(record.RetainedResidues);
        Assert.Empty(record.UnresolvedResidues);
        Assert.Equal("prime-retained-whole-partial", record.ReasonCode);
    }

    [Fact]
    public void ReceiptedHistory_With_Multiple_Residues_Becomes_RetainedWholeUnclosed()
    {
        var receipt = CreateHistoryReceipt(
            receiptKind: PrimeMembraneReceiptKind.ReceiptedHistory,
            visibleResidues: CreateResidues(3),
            deferredResidues: []);

        var record = PrimeRetainedWholeEvaluator.Evaluate(
            receipt,
            "record://prime-retained-whole/unclosed");

        Assert.Equal(PrimeRetainedWholeKind.RetainedWholeUnclosed, record.RetainedWholeKind);
        Assert.Equal(3, record.RetainedResidues.Count);
        Assert.Empty(record.UnresolvedResidues);
        Assert.Contains("prime-retained-whole-unclosed", record.ConstraintCodes);
    }

    [Fact]
    public void ReturnBearing_History_Becomes_ClosureCandidate_But_Remains_Unclosed()
    {
        var receipt = CreateHistoryReceipt(
            receiptKind: PrimeMembraneReceiptKind.ReturnBearingUnclosed,
            visibleResidues: CreateResidues(2),
            deferredResidues: []);

        var record = PrimeRetainedWholeEvaluator.Evaluate(
            receipt,
            "record://prime-retained-whole/closure-candidate");

        Assert.Equal(PrimeRetainedWholeKind.ClosureCandidate, record.RetainedWholeKind);
        Assert.Equal(2, record.RetainedResidues.Count);
        Assert.True(record.ExplicitClosureActStillRequired);
        Assert.True(record.PrimeClosureStillWithheld);
        Assert.Contains("prime-retained-whole-closure-candidate-still-unclosed", record.ConstraintCodes);
    }

    [Fact]
    public void Deferred_Or_Flattened_History_Remains_StillDeferred()
    {
        var deferredReceipt = CreateHistoryReceipt(
            receiptKind: PrimeMembraneReceiptKind.Deferred,
            visibleResidues: CreateResidues(2),
            deferredResidues: CreateResidues(2));

        var deferredRecord = PrimeRetainedWholeEvaluator.Evaluate(
            deferredReceipt,
            "record://prime-retained-whole/still-deferred");

        Assert.Equal(PrimeRetainedWholeKind.StillDeferred, deferredRecord.RetainedWholeKind);
        Assert.Empty(deferredRecord.RetainedResidues);
        Assert.Equal(2, deferredRecord.UnresolvedResidues.Count);

        var flattenedReceipt = CreateHistoryReceipt(
            receiptKind: PrimeMembraneReceiptKind.ReceiptedHistory,
            visibleResidues: CreateResidues(2),
            deferredResidues: [],
            preservedDistinctionVisible: false);

        var flattenedRecord = PrimeRetainedWholeEvaluator.Evaluate(
            flattenedReceipt,
            "record://prime-retained-whole/flattened");

        Assert.Equal(PrimeRetainedWholeKind.StillDeferred, flattenedRecord.RetainedWholeKind);
        Assert.Equal("prime-retained-whole-distinction-not-visible", flattenedRecord.ReasonCode);
    }

    [Fact]
    public void RetainedWhole_Record_RoundTrips()
    {
        var record = PrimeRetainedWholeEvaluator.Evaluate(
            CreateHistoryReceipt(
                receiptKind: PrimeMembraneReceiptKind.ReceiptedHistory,
                visibleResidues: CreateResidues(3),
                deferredResidues: []),
            "record://prime-retained-whole/roundtrip");

        var json = JsonSerializer.Serialize(record);
        var roundTrip = JsonSerializer.Deserialize<PrimeRetainedHistoryRecord>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(PrimeRetainedWholeKind.RetainedWholeUnclosed, roundTrip!.RetainedWholeKind);
        Assert.Equal(3, roundTrip.RetainedResidues.Count);
    }

    [Fact]
    public void Docs_Record_Prime_Retained_Whole_Evaluation_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIME_RETAINED_WHOLE_EVALUATION_LAW.md");
        var receiptLawPath = Path.Combine(lineRoot, "docs", "PRIME_MEMBRANE_PROJECTED_HISTORY_RECEIPT_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var receiptLawText = File.ReadAllText(receiptLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("retained-whole shall not imply closure", lawText, StringComparison.Ordinal);
        Assert.Contains("may not silently erase unresolved residue", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_RETAINED_WHOLE_EVALUATION_LAW.md", receiptLawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_RETAINED_WHOLE_EVALUATION_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("prime-retained-whole-evaluation: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Prime retained-whole evaluation", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("re-entrant path inheritance and deep-store navigation beyond post-Prime closure continuity", refinementText, StringComparison.Ordinal);
    }

    private static PrimeMembraneHistoryReceipt CreateHistoryReceipt(
        PrimeMembraneReceiptKind receiptKind,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> visibleResidues,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> deferredResidues,
        bool preservedDistinctionVisible = true)
    {
        return new PrimeMembraneHistoryReceipt(
            ReceiptHandle: "receipt://prime-membrane/history/session-a",
            HistoryHandle: "history://prime-membrane/session-a",
            MembraneHandle: "membrane://prime/session-a",
            ProjectionHandle: "projection://cryptic/session-a",
            Interpretation: receiptKind switch
            {
                PrimeMembraneReceiptKind.ReturnBearingUnclosed => PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate,
                PrimeMembraneReceiptKind.ReceiptedHistory => PrimeMembraneProjectedHistoryInterpretationKind.StableBraid,
                PrimeMembraneReceiptKind.Deferred => PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence,
                _ => PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection
            },
            ReceiptKind: receiptKind,
            AdvisoryClosureEligibility: receiptKind == PrimeMembraneReceiptKind.ReturnBearingUnclosed
                ? PrimeClosureEligibilityKind.EligibleForMembraneReceipt
                : PrimeClosureEligibilityKind.ReviewRequired,
            PreservedDistinctionVisible: preservedDistinctionVisible,
            RetainedWholenessStillWithheld: true,
            PrimeClosureStillWithheld: true,
            VisibleLineResidues: visibleResidues,
            DeferredLineResidues: deferredResidues,
            ConstraintCodes: ["prime-membrane-history-receipt-not-closure"],
            ReasonCode: "history-receipt-test",
            LawfulBasis: "history receipt test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 06, 00, 00, TimeSpan.Zero));
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
