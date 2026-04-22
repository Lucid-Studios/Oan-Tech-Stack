namespace San.Audit.Tests;

using System.Text.Json;
using San.Common;

public sealed class PrimeMembraneProjectedHistoryReceiptContractsTests
{
    [Fact]
    public void ProjectedHistoryReceipt_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrimeMembraneReceiptKind.SeenOnly,
                PrimeMembraneReceiptKind.ReceiptedHistory,
                PrimeMembraneReceiptKind.ReturnBearingUnclosed,
                PrimeMembraneReceiptKind.Deferred
            ],
            Enum.GetValues<PrimeMembraneReceiptKind>());
    }

    [Fact]
    public void ActiveProjection_Is_Receipted_As_SeenOnly_With_Deferred_Residues()
    {
        var interpretation = CreateInterpretationReceipt(
            interpretation: PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.CandidateOnly,
            preservedDistinctionVisible: true);

        var receipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            interpretation,
            "receipt://prime-membrane/history-receipt/seen");

        Assert.Equal(PrimeMembraneReceiptKind.SeenOnly, receipt.ReceiptKind);
        Assert.True(receipt.RetainedWholenessStillWithheld);
        Assert.True(receipt.PrimeClosureStillWithheld);
        Assert.Equal(receipt.VisibleLineResidues.Count, receipt.DeferredLineResidues.Count);
        Assert.Equal("prime-membrane-history-seen-only", receipt.ReasonCode);
    }

    [Fact]
    public void StableBraid_Is_Receipted_As_History_Without_Deferred_Residues()
    {
        var interpretation = CreateInterpretationReceipt(
            interpretation: PrimeMembraneProjectedHistoryInterpretationKind.StableBraid,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.ReviewRequired,
            preservedDistinctionVisible: true);

        var receipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            interpretation,
            "receipt://prime-membrane/history-receipt/stable");

        Assert.Equal(PrimeMembraneReceiptKind.ReceiptedHistory, receipt.ReceiptKind);
        Assert.Empty(receipt.DeferredLineResidues);
        Assert.Contains("prime-membrane-history-receipted", receipt.ConstraintCodes);
        Assert.Equal("prime-membrane-history-receipted", receipt.ReasonCode);
    }

    [Fact]
    public void ReturnCandidate_Is_Receipted_As_ReturnBearing_Unclosed()
    {
        var interpretation = CreateInterpretationReceipt(
            interpretation: PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.EligibleForMembraneReceipt,
            preservedDistinctionVisible: true);

        var receipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            interpretation,
            "receipt://prime-membrane/history-receipt/return-bearing");

        Assert.Equal(PrimeMembraneReceiptKind.ReturnBearingUnclosed, receipt.ReceiptKind);
        Assert.Empty(receipt.DeferredLineResidues);
        Assert.Contains("prime-membrane-history-return-bearing-unclosed", receipt.ConstraintCodes);
        Assert.Contains("prime-membrane-history-return-still-advisory", receipt.ConstraintCodes);
        Assert.Equal("prime-membrane-history-receipted-return-bearing-unclosed", receipt.ReasonCode);
    }

    [Fact]
    public void DeferOnly_Or_Flattened_History_Remains_Deferred()
    {
        var deferOnlyInterpretation = CreateInterpretationReceipt(
            interpretation: PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.Withheld,
            preservedDistinctionVisible: true);

        var deferOnlyReceipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            deferOnlyInterpretation,
            "receipt://prime-membrane/history-receipt/deferred");

        Assert.Equal(PrimeMembraneReceiptKind.Deferred, deferOnlyReceipt.ReceiptKind);
        Assert.Equal(deferOnlyReceipt.VisibleLineResidues.Count, deferOnlyReceipt.DeferredLineResidues.Count);

        var flattenedInterpretation = CreateInterpretationReceipt(
            interpretation: PrimeMembraneProjectedHistoryInterpretationKind.StableBraid,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.ReviewRequired,
            preservedDistinctionVisible: false);

        var flattenedReceipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            flattenedInterpretation,
            "receipt://prime-membrane/history-receipt/flattened");

        Assert.Equal(PrimeMembraneReceiptKind.Deferred, flattenedReceipt.ReceiptKind);
        Assert.Equal("prime-membrane-history-receipt-distinction-not-visible", flattenedReceipt.ReasonCode);
        Assert.Contains("prime-membrane-history-distinction-not-visible", flattenedReceipt.ConstraintCodes);
    }

    [Fact]
    public void ProjectedHistoryReceipt_RoundTrips_With_Visible_And_Deferred_Residues()
    {
        var receipt = PrimeMembraneHistoryReceiptEvaluator.Evaluate(
            CreateInterpretationReceipt(
                interpretation: PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection,
                advisoryClosureEligibility: PrimeClosureEligibilityKind.CandidateOnly,
                preservedDistinctionVisible: true),
            "receipt://prime-membrane/history-receipt/roundtrip");

        var json = JsonSerializer.Serialize(receipt);
        var roundTrip = JsonSerializer.Deserialize<PrimeMembraneHistoryReceipt>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(PrimeMembraneReceiptKind.SeenOnly, roundTrip!.ReceiptKind);
        Assert.Equal(roundTrip.VisibleLineResidues.Count, roundTrip.DeferredLineResidues.Count);
    }

    [Fact]
    public void Docs_Record_Prime_Membrane_Projected_History_Receipt_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIME_MEMBRANE_PROJECTED_HISTORY_RECEIPT_LAW.md");
        var interpretationLawPath = Path.Combine(lineRoot, "docs", "PRIME_MEMBRANE_PROJECTED_BRAID_HISTORY_INTERPRETATION_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var interpretationLawText = File.ReadAllText(interpretationLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("receipt shall not erase preserved distinction", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not imply retained wholeness", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not constitute Prime closure", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_PROJECTED_HISTORY_RECEIPT_LAW.md", interpretationLawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_PROJECTED_HISTORY_RECEIPT_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("prime-membrane-projected-history-receipt: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Prime Membrane projected history receipt", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("Prime closure from receipted projected braid history", refinementText, StringComparison.Ordinal);
    }

    private static PrimeMembraneProjectedBraidInterpretationReceipt CreateInterpretationReceipt(
        PrimeMembraneProjectedHistoryInterpretationKind interpretation,
        PrimeClosureEligibilityKind advisoryClosureEligibility,
        bool preservedDistinctionVisible)
    {
        var residues = new[]
        {
            new PrimeMembraneProjectedLineResidue(
                LineHandle: "line://a",
                SourceSurfaceHandle: "surface://a",
                ResidualPosture: CrypticProjectionPostureKind.Braided,
                ParticipationKind: PrimeMembraneProjectedParticipationKind.Clustered,
                AcceptedContributionHandles: ["contribution://a"],
                DistinctionPreserved: preservedDistinctionVisible,
                ResidueNotes: ["per-line-distinction-preserved"]),
            new PrimeMembraneProjectedLineResidue(
                LineHandle: "line://b",
                SourceSurfaceHandle: "surface://b",
                ResidualPosture: CrypticProjectionPostureKind.Braided,
                ParticipationKind: PrimeMembraneProjectedParticipationKind.Swarmed,
                AcceptedContributionHandles: ["contribution://b"],
                DistinctionPreserved: preservedDistinctionVisible,
                ResidueNotes: ["per-line-distinction-preserved"])
        };

        return new PrimeMembraneProjectedBraidInterpretationReceipt(
            ReceiptHandle: "receipt://prime-membrane/interpretation/session-a",
            HistoryHandle: "history://prime-membrane/session-a",
            SourceDuplexPacketHandle: "packet://duplex-field/source/session-a",
            EmittedDuplexPacketHandle: "packet://duplex-field/emitted/session-a",
            MembraneHandle: "membrane://prime/session-a",
            ProjectionHandle: "projection://cryptic/session-a",
            Interpretation: interpretation,
            AdvisoryClosureEligibility: advisoryClosureEligibility,
            PreservedDistinctionRequired: true,
            PreservedDistinctionVisible: preservedDistinctionVisible,
            ExplicitMembraneReceiptStillRequired: true,
            PrimeClosureStillWithheld: true,
            VisibleLineResidues: residues,
            ConstraintCodes: ["prime-membrane-interpretation-classification-only"],
            ReasonCode: "projected-history-test",
            LawfulBasis: "projected history interpretation test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 05, 00, 00, TimeSpan.Zero));
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
