namespace San.Audit.Tests;

using System.Text.Json;
using San.Common;

public sealed class PrimeMembraneProjectedBraidInterpretationContractsTests
{
    [Fact]
    public void ProjectedBraidInterpretation_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrimeMembraneProjectedParticipationKind.Clustered,
                PrimeMembraneProjectedParticipationKind.Swarmed
            ],
            Enum.GetValues<PrimeMembraneProjectedParticipationKind>());

        Assert.Equal(
            [
                PrimeMembraneProjectedBraidStateKind.Dispersed,
                PrimeMembraneProjectedBraidStateKind.Clustered,
                PrimeMembraneProjectedBraidStateKind.Swarmed,
                PrimeMembraneProjectedBraidStateKind.CoherentBraid,
                PrimeMembraneProjectedBraidStateKind.UnstableBraid
            ],
            Enum.GetValues<PrimeMembraneProjectedBraidStateKind>());

        Assert.Equal(
            [
                PrimeMembraneProjectedHistoryInterpretationKind.ActiveProjection,
                PrimeMembraneProjectedHistoryInterpretationKind.StableBraid,
                PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate,
                PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence
            ],
            Enum.GetValues<PrimeMembraneProjectedHistoryInterpretationKind>());
    }

    [Fact]
    public void ProjectedBraidHistory_RoundTrips_With_Visible_Line_Residues()
    {
        var history = CreateHistoryPacket(
            braidState: PrimeMembraneProjectedBraidStateKind.Clustered,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.CandidateOnly,
            lineResidues:
            [
                CreateResidue("line://a", PrimeMembraneProjectedParticipationKind.Clustered, true),
                CreateResidue("line://b", PrimeMembraneProjectedParticipationKind.Clustered, true)
            ]);

        var json = JsonSerializer.Serialize(history);
        var roundTrip = JsonSerializer.Deserialize<PrimeMembraneProjectedBraidHistoryPacket>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(PrimeMembraneProjectedBraidStateKind.Clustered, roundTrip!.BraidState);
        Assert.Equal(2, roundTrip.LineResidues.Count);
        Assert.All(roundTrip.LineResidues, static residue => Assert.True(residue.DistinctionPreserved));
    }

    [Fact]
    public void Coherent_Braid_With_ReviewRequired_Is_Classified_As_StableBraid()
    {
        var history = CreateHistoryPacket(
            braidState: PrimeMembraneProjectedBraidStateKind.CoherentBraid,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.ReviewRequired,
            lineResidues:
            [
                CreateResidue("line://a", PrimeMembraneProjectedParticipationKind.Clustered, true),
                CreateResidue("line://b", PrimeMembraneProjectedParticipationKind.Swarmed, true)
            ]);

        var receipt = PrimeMembraneProjectedBraidHistoryEvaluator.Evaluate(
            history,
            "receipt://prime-membrane/history/stable");

        Assert.Equal(PrimeMembraneProjectedHistoryInterpretationKind.StableBraid, receipt.Interpretation);
        Assert.True(receipt.PreservedDistinctionVisible);
        Assert.True(receipt.ExplicitMembraneReceiptStillRequired);
        Assert.True(receipt.PrimeClosureStillWithheld);
        Assert.Equal("projected-history-stable-braid-visible", receipt.ReasonCode);
        Assert.Equal(2, receipt.VisibleLineResidues.Count);
    }

    [Fact]
    public void Coherent_Braid_With_Eligible_Return_And_Visible_Distinction_Becomes_ReturnCandidate()
    {
        var history = CreateHistoryPacket(
            braidState: PrimeMembraneProjectedBraidStateKind.CoherentBraid,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.EligibleForMembraneReceipt,
            lineResidues:
            [
                CreateResidue("line://a", PrimeMembraneProjectedParticipationKind.Swarmed, true),
                CreateResidue("line://b", PrimeMembraneProjectedParticipationKind.Swarmed, true)
            ]);

        var receipt = PrimeMembraneProjectedBraidHistoryEvaluator.Evaluate(
            history,
            "receipt://prime-membrane/history/return-candidate");

        Assert.Equal(PrimeMembraneProjectedHistoryInterpretationKind.ReturnCandidate, receipt.Interpretation);
        Assert.True(receipt.PreservedDistinctionVisible);
        Assert.Contains("projected-history-advisory-return-signal-only", receipt.ConstraintCodes);
        Assert.Equal("projected-history-return-candidate-visible", receipt.ReasonCode);
    }

    [Fact]
    public void Flattened_Or_Unstable_History_Remains_DeferOnlyEvidence()
    {
        var flattenedHistory = CreateHistoryPacket(
            braidState: PrimeMembraneProjectedBraidStateKind.CoherentBraid,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.EligibleForMembraneReceipt,
            lineResidues:
            [
                CreateResidue("line://a", PrimeMembraneProjectedParticipationKind.Clustered, false),
                CreateResidue("line://b", PrimeMembraneProjectedParticipationKind.Clustered, true)
            ]);

        var flattenedReceipt = PrimeMembraneProjectedBraidHistoryEvaluator.Evaluate(
            flattenedHistory,
            "receipt://prime-membrane/history/flattened");

        Assert.Equal(PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence, flattenedReceipt.Interpretation);
        Assert.False(flattenedReceipt.PreservedDistinctionVisible);
        Assert.Equal("projected-history-distinction-not-visible", flattenedReceipt.ReasonCode);

        var unstableHistory = CreateHistoryPacket(
            braidState: PrimeMembraneProjectedBraidStateKind.UnstableBraid,
            advisoryClosureEligibility: PrimeClosureEligibilityKind.Withheld,
            lineResidues:
            [
                CreateResidue("line://a", PrimeMembraneProjectedParticipationKind.Swarmed, true),
                CreateResidue("line://b", PrimeMembraneProjectedParticipationKind.Clustered, true)
            ]);

        var unstableReceipt = PrimeMembraneProjectedBraidHistoryEvaluator.Evaluate(
            unstableHistory,
            "receipt://prime-membrane/history/unstable");

        Assert.Equal(PrimeMembraneProjectedHistoryInterpretationKind.DeferOnlyEvidence, unstableReceipt.Interpretation);
        Assert.Equal("projected-history-unstable", unstableReceipt.ReasonCode);
        Assert.Contains("projected-history-braid-unstable", unstableReceipt.ConstraintCodes);
    }

    [Fact]
    public void Docs_Record_Prime_Membrane_Projected_Braid_History_Interpretation_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIME_MEMBRANE_PROJECTED_BRAID_HISTORY_INTERPRETATION_LAW.md");
        var duplexLawPath = Path.Combine(lineRoot, "docs", "PRIME_MEMBRANE_DUPLEX_PACKET_LAW.md");
        var braidNotePath = Path.Combine(lineRoot, "docs", "SLI_RTME_CLUSTERED_SWARMED_BRAID_DISCIPLINE_NOTE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var duplexLawText = File.ReadAllText(duplexLawPath);
        var braidNoteText = File.ReadAllText(braidNotePath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("membrane interpretation may classify projected braid history", lawText, StringComparison.Ordinal);
        Assert.Contains("may not erase preserved distinction", lawText, StringComparison.Ordinal);
        Assert.Contains("may not treat classification as receipt or closure", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_PROJECTED_BRAID_HISTORY_INTERPRETATION_LAW.md", duplexLawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_PROJECTED_BRAID_HISTORY_INTERPRETATION_LAW.md", braidNoteText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_PROJECTED_BRAID_HISTORY_INTERPRETATION_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("prime-membrane-projected-braid-history-interpretation: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Prime Membrane projected braid-history interpretation", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("receipted projected braid history", refinementText, StringComparison.Ordinal);
    }

    private static PrimeMembraneProjectedBraidHistoryPacket CreateHistoryPacket(
        PrimeMembraneProjectedBraidStateKind braidState,
        PrimeClosureEligibilityKind advisoryClosureEligibility,
        IReadOnlyList<PrimeMembraneProjectedLineResidue> lineResidues)
    {
        return new PrimeMembraneProjectedBraidHistoryPacket(
            HistoryHandle: "history://prime-membrane/projected-braid/session-a",
            SourceDuplexPacketHandle: "packet://duplex-field/source/session-a",
            EmittedDuplexPacketHandle: "packet://duplex-field/emitted/session-a",
            MembraneHandle: "membrane://prime/session-a",
            ProjectionHandle: "projection://cryptic/session-a",
            BraidState: braidState,
            AdvisoryClosureEligibility: advisoryClosureEligibility,
            PrimeClosureIssued: false,
            LineResidues: lineResidues,
            OutcomeCode: "projected-history-test",
            LawfulBasis: "projected history test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 03, 30, 00, TimeSpan.Zero));
    }

    private static PrimeMembraneProjectedLineResidue CreateResidue(
        string lineHandle,
        PrimeMembraneProjectedParticipationKind participationKind,
        bool distinctionPreserved)
    {
        var suffix = lineHandle.Split('/').Last();

        return new PrimeMembraneProjectedLineResidue(
            LineHandle: lineHandle,
            SourceSurfaceHandle: $"surface://{suffix}",
            ResidualPosture: CrypticProjectionPostureKind.Braided,
            ParticipationKind: participationKind,
            AcceptedContributionHandles:
            [
                $"contribution://{suffix}"
            ],
            DistinctionPreserved: distinctionPreserved,
            ResidueNotes:
            [
                "per-line-distinction-preserved"
            ]);
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
