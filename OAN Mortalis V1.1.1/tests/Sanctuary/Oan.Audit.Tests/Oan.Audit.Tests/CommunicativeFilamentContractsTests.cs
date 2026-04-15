namespace San.Audit.Tests;

using San.Common;

public sealed class CommunicativeFilamentContractsTests
{
    [Fact]
    public void CommunicativeFilament_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                CommunicativeCarrierClassKind.DecisionBearing,
                CommunicativeCarrierClassKind.ChosenPathBearing,
                CommunicativeCarrierClassKind.DistributedIntegrationBearing,
                CommunicativeCarrierClassKind.DeferredEdgeBearing,
                CommunicativeCarrierClassKind.RedundantEchoCandidate
            ],
            Enum.GetValues<CommunicativeCarrierClassKind>());

        Assert.Equal(
            [
                FilamentResolutionTargetKind.OE,
                FilamentResolutionTargetKind.SelfGEL,
                FilamentResolutionTargetKind.CGoA,
                FilamentResolutionTargetKind.WorkingSurfaceOnly
            ],
            Enum.GetValues<FilamentResolutionTargetKind>());

        Assert.Equal(
            [
                AntiEchoDispositionKind.Preserve,
                AntiEchoDispositionKind.DeduplicateCarrier,
                AntiEchoDispositionKind.ThinAsEcho,
                AntiEchoDispositionKind.Defer
            ],
            Enum.GetValues<AntiEchoDispositionKind>());
    }

    [Fact]
    public void DecisionBearing_Requires_Whole_Or_Candidate_Basis_Before_Oe_Routing()
    {
        var closureCandidateCarrier = CreateCarrier(
            sourceRetainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            carrierClass: CommunicativeCarrierClassKind.DecisionBearing);

        var routedReceipt = CommunicativeFilamentEvaluator.Evaluate(
            closureCandidateCarrier,
            "receipt://communicative-filament/decision-bearing");

        Assert.Equal(FilamentResolutionTargetKind.OE, routedReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Preserve, routedReceipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-decision-bearing-to-oe", routedReceipt.ReasonCode);

        var partialCarrier = CreateCarrier(
            sourceRetainedWholeKind: PrimeRetainedWholeKind.RetainedPartial,
            carrierClass: CommunicativeCarrierClassKind.DecisionBearing);

        var deferredReceipt = CommunicativeFilamentEvaluator.Evaluate(
            partialCarrier,
            "receipt://communicative-filament/decision-bearing-insufficient");

        Assert.Equal(FilamentResolutionTargetKind.WorkingSurfaceOnly, deferredReceipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Defer, deferredReceipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-insufficient-retained-basis", deferredReceipt.ReasonCode);
        Assert.Contains("communicative-filament-insufficient-retained-basis", deferredReceipt.ConstraintCodes);
    }

    [Fact]
    public void ChosenPathBearing_Routes_To_SelfGel_When_Path_Body_Is_Retained()
    {
        var carrier = CreateCarrier(
            sourceRetainedWholeKind: PrimeRetainedWholeKind.RetainedWholeUnclosed,
            carrierClass: CommunicativeCarrierClassKind.ChosenPathBearing,
            selfEchoDetected: true);

        var receipt = CommunicativeFilamentEvaluator.Evaluate(
            carrier,
            "receipt://communicative-filament/chosen-path");

        Assert.Equal(FilamentResolutionTargetKind.SelfGEL, receipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.DeduplicateCarrier, receipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-self-echo-deduplicated", receipt.ReasonCode);
        Assert.Contains("communicative-filament-self-echo-detected", receipt.ConstraintCodes);
    }

    [Fact]
    public void DeferredEdgeBearing_Remains_Distributed_And_Deferred()
    {
        var carrier = CreateCarrier(
            sourceRetainedWholeKind: PrimeRetainedWholeKind.StillDeferred,
            carrierClass: CommunicativeCarrierClassKind.DeferredEdgeBearing,
            deferredResidues: CreateResidues(2));

        var receipt = CommunicativeFilamentEvaluator.Evaluate(
            carrier,
            "receipt://communicative-filament/deferred-edge");

        Assert.Equal(FilamentResolutionTargetKind.CGoA, receipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Defer, receipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-deferred", receipt.ReasonCode);
        Assert.Contains("communicative-filament-deferred-residues-visible", receipt.ConstraintCodes);
    }

    [Fact]
    public void RedundantEchoCandidate_Remains_WorkingSurface_Only_And_Is_Thinned()
    {
        var carrier = CreateCarrier(
            sourceRetainedWholeKind: PrimeRetainedWholeKind.RetainedWholeUnclosed,
            carrierClass: CommunicativeCarrierClassKind.RedundantEchoCandidate,
            selfEchoDetected: true);

        var receipt = CommunicativeFilamentEvaluator.Evaluate(
            carrier,
            "receipt://communicative-filament/redundant-echo");

        Assert.Equal(FilamentResolutionTargetKind.WorkingSurfaceOnly, receipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.ThinAsEcho, receipt.AntiEchoDisposition);
        Assert.Equal("communicative-filament-redundant-echo-thinned", receipt.ReasonCode);
        Assert.True(receipt.CarrierTransportOnly);
    }

    [Fact]
    public void GelSubstance_Request_Is_Refused_And_Carrier_Remains_Transport_Only()
    {
        var carrier = CreateCarrier(
            sourceRetainedWholeKind: PrimeRetainedWholeKind.ClosureCandidate,
            carrierClass: CommunicativeCarrierClassKind.DecisionBearing,
            gelSubstanceConsumptionRequested: true);

        var receipt = CommunicativeFilamentEvaluator.Evaluate(
            carrier,
            "receipt://communicative-filament/gel-refusal");

        Assert.Equal(FilamentResolutionTargetKind.WorkingSurfaceOnly, receipt.ResolutionTarget);
        Assert.Equal(AntiEchoDispositionKind.Defer, receipt.AntiEchoDisposition);
        Assert.True(receipt.GelSubstanceConsumptionRequested);
        Assert.True(receipt.GelSubstanceConsumptionRefused);
        Assert.True(receipt.CarrierTransportOnly);
        Assert.Equal("communicative-filament-gel-substance-consumption-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Communicative_Filament_And_AntiEcho_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md");
        var reopeningLawPath = Path.Combine(lineRoot, "docs", "LAWFUL_REOPENING_REDOPING_AND_CONTINUED_PARTICIPATION_LAW.md");
        var retainedWholeLawPath = Path.Combine(lineRoot, "docs", "PRIME_RETAINED_WHOLE_EVALUATION_LAW.md");
        var mosLawPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var reopeningLawText = File.ReadAllText(reopeningLawPath);
        var retainedWholeLawText = File.ReadAllText(retainedWholeLawPath);
        var mosLawText = File.ReadAllText(mosLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("Resolution across the filament shall distribute burden lawfully", lawText, StringComparison.Ordinal);
        Assert.Contains("important decisions to OE", lawText, StringComparison.Ordinal);
        Assert.Contains("chosen-path body to SelfGEL", lawText, StringComparison.Ordinal);
        Assert.Contains("remaining integrable matter to cGoA", lawText, StringComparison.Ordinal);
        Assert.Contains("without treating GEL substance itself as consumable runtime material", lawText, StringComparison.Ordinal);
        Assert.Contains("COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md", reopeningLawText, StringComparison.Ordinal);
        Assert.Contains("COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md", retainedWholeLawText, StringComparison.Ordinal);
        Assert.Contains("COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md", mosLawText, StringComparison.Ordinal);
        Assert.Contains("COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("communicative-filament-anti-echo-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Communicative filament preservation and anti-echo law", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("re-entrant path inheritance and deep-store navigation beyond post-Prime closure continuity", refinementText, StringComparison.Ordinal);
    }

    private static CommunicativeCarrierPacket CreateCarrier(
        PrimeRetainedWholeKind sourceRetainedWholeKind,
        CommunicativeCarrierClassKind carrierClass,
        bool selfEchoDetected = false,
        bool gelSubstanceConsumptionRequested = false,
        bool preservedDistinctionVisible = true,
        IReadOnlyList<PrimeMembraneProjectedLineResidue>? preservedResidues = null,
        IReadOnlyList<PrimeMembraneProjectedLineResidue>? deferredResidues = null)
    {
        return new CommunicativeCarrierPacket(
            CarrierHandle: "carrier://communicative-filament/session-a",
            SourceRetainedWholeRecordHandle: "record://prime-retained-whole/session-a",
            SourceRetainedWholeKind: sourceRetainedWholeKind,
            CarrierClass: carrierClass,
            PreservedDistinctionVisible: preservedDistinctionVisible,
            SelfEchoDetected: selfEchoDetected,
            GelSubstanceConsumptionRequested: gelSubstanceConsumptionRequested,
            PointerHandles: ["pointer://decision", "pointer://history", "pointer://history"],
            SymbolicCarrierHandles: ["carrier://cryptic", "carrier://cryptic", "carrier://projection"],
            PreservedResidues: preservedResidues ?? CreateResidues(2),
            DeferredResidues: deferredResidues ?? [],
            Notes: ["communicative-filament-test-packet"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 07, 00, 00, TimeSpan.Zero));
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
