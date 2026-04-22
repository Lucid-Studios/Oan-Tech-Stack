namespace San.Audit.Tests;

using San.Common;

public sealed class LawfulReopeningParticipationContractsTests
{
    [Fact]
    public void LawfulReopening_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                ReopeningModeKind.SimpleReopen,
                ReopeningModeKind.ModifiedReopen,
                ReopeningModeKind.RedopedReopen,
                ReopeningModeKind.DistributedContinuation
            ],
            Enum.GetValues<ReopeningModeKind>());

        Assert.Equal(
            [
                ContinuedParticipationStateKind.Reopened,
                ContinuedParticipationStateKind.ReopenedModified,
                ContinuedParticipationStateKind.Redoped,
                ContinuedParticipationStateKind.ContinuedDistributed,
                ContinuedParticipationStateKind.Deferred
            ],
            Enum.GetValues<ContinuedParticipationStateKind>());
    }

    [Fact]
    public void SimpleReopen_Preserves_History_Without_False_Fresh_Start()
    {
        var receipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.SelfGEL,
            antiEchoDisposition: AntiEchoDispositionKind.Preserve);

        var record = LawfulReopeningParticipationEvaluator.Evaluate(
            receipt,
            ReopeningModeKind.SimpleReopen,
            "record://lawful-reopening/simple");

        Assert.Equal(ContinuedParticipationStateKind.Reopened, record.ParticipationState);
        Assert.True(record.PriorReceiptHistoryPreserved);
        Assert.True(record.FalseFreshStartRefused);
        Assert.True(record.AdjacentPossibilityPreserved);
        Assert.Equal("lawful-reopening-simple", record.ReasonCode);
    }

    [Fact]
    public void ModifiedReopen_Keeps_Reopening_Markers_Visible()
    {
        var receipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.SelfGEL,
            antiEchoDisposition: AntiEchoDispositionKind.DeduplicateCarrier);

        var record = LawfulReopeningParticipationEvaluator.Evaluate(
            receipt,
            ReopeningModeKind.ModifiedReopen,
            "record://lawful-reopening/modified",
            ["marker://temperature-shift", "marker://temperature-shift", "marker://focus-shift"]);

        Assert.Equal(ContinuedParticipationStateKind.ReopenedModified, record.ParticipationState);
        Assert.Equal(2, record.ReopeningMarkers.Count);
        Assert.Contains("lawful-reopening-markers-visible", record.ConstraintCodes);
        Assert.Equal("lawful-reopening-modified", record.ReasonCode);
    }

    [Fact]
    public void RedopedReopen_Preserves_Gel_NonConsumption_And_Distinction()
    {
        var receipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.SelfGEL,
            antiEchoDisposition: AntiEchoDispositionKind.Preserve);

        var record = LawfulReopeningParticipationEvaluator.Evaluate(
            receipt,
            ReopeningModeKind.RedopedReopen,
            "record://lawful-reopening/redoped",
            ["marker://dopant-band"]);

        Assert.Equal(ContinuedParticipationStateKind.Redoped, record.ParticipationState);
        Assert.True(record.GelSubstanceStillUnconsumed);
        Assert.True(record.PreservedDistinctionVisible);
        Assert.Equal("lawful-reopening-redoped", record.ReasonCode);
    }

    [Fact]
    public void DistributedContinuation_Is_Lawful_Only_For_CGoA_Resolved_Matter()
    {
        var distributedReceipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.CGoA,
            antiEchoDisposition: AntiEchoDispositionKind.Preserve,
            carrierClass: CommunicativeCarrierClassKind.DistributedIntegrationBearing);

        var distributedRecord = LawfulReopeningParticipationEvaluator.Evaluate(
            distributedReceipt,
            ReopeningModeKind.DistributedContinuation,
            "record://lawful-reopening/distributed");

        Assert.Equal(ContinuedParticipationStateKind.ContinuedDistributed, distributedRecord.ParticipationState);
        Assert.True(distributedRecord.AdjacentPossibilityPreserved);

        var selfGelReceipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.SelfGEL,
            antiEchoDisposition: AntiEchoDispositionKind.Preserve);

        var invalidRecord = LawfulReopeningParticipationEvaluator.Evaluate(
            selfGelReceipt,
            ReopeningModeKind.DistributedContinuation,
            "record://lawful-reopening/distributed-invalid");

        Assert.Equal(ContinuedParticipationStateKind.Deferred, invalidRecord.ParticipationState);
        Assert.Equal("lawful-reopening-distributed-continuation-not-lawful", invalidRecord.ReasonCode);
    }

    [Fact]
    public void WorkingSurfaceOnly_Or_GelConsumption_Request_Remains_Deferred()
    {
        var workingSurfaceReceipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.WorkingSurfaceOnly,
            antiEchoDisposition: AntiEchoDispositionKind.ThinAsEcho,
            carrierClass: CommunicativeCarrierClassKind.RedundantEchoCandidate);

        var workingSurfaceRecord = LawfulReopeningParticipationEvaluator.Evaluate(
            workingSurfaceReceipt,
            ReopeningModeKind.SimpleReopen,
            "record://lawful-reopening/working-surface");

        Assert.Equal(ContinuedParticipationStateKind.Deferred, workingSurfaceRecord.ParticipationState);
        Assert.Empty(workingSurfaceRecord.ActiveResidues);
        Assert.Equal("lawful-reopening-working-surface-only", workingSurfaceRecord.ReasonCode);

        var gelReceipt = CreateFilamentReceipt(
            resolutionTarget: FilamentResolutionTargetKind.OE,
            antiEchoDisposition: AntiEchoDispositionKind.Defer,
            gelSubstanceConsumptionRequested: true);

        var gelRecord = LawfulReopeningParticipationEvaluator.Evaluate(
            gelReceipt,
            ReopeningModeKind.RedopedReopen,
            "record://lawful-reopening/gel-refused");

        Assert.Equal(ContinuedParticipationStateKind.Deferred, gelRecord.ParticipationState);
        Assert.True(gelRecord.GelSubstanceStillUnconsumed);
        Assert.Equal("lawful-reopening-gel-substance-still-withheld", gelRecord.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Lawful_Reopening_Redoping_And_Continued_Participation_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "LAWFUL_REOPENING_REDOPING_AND_CONTINUED_PARTICIPATION_LAW.md");
        var communicativeLawPath = Path.Combine(lineRoot, "docs", "COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var communicativeLawText = File.ReadAllText(communicativeLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("reopening is not innocence", lawText, StringComparison.Ordinal);
        Assert.Contains("shall preserve lawful history", lawText, StringComparison.Ordinal);
        Assert.Contains("shall not treat prior passage as void", lawText, StringComparison.Ordinal);
        Assert.Contains("adjacent possibility", lawText, StringComparison.Ordinal);
        Assert.Contains("LAWFUL_REOPENING_REDOPING_AND_CONTINUED_PARTICIPATION_LAW.md", communicativeLawText, StringComparison.Ordinal);
        Assert.Contains("LAWFUL_REOPENING_REDOPING_AND_CONTINUED_PARTICIPATION_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("lawful-reopening-redoping-continued-participation: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Lawful reopening, redoping, and continued participation", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("re-entrant path inheritance and deep-store navigation beyond post-Prime closure continuity", refinementText, StringComparison.Ordinal);
    }

    private static CommunicativeFilamentResolutionReceipt CreateFilamentReceipt(
        FilamentResolutionTargetKind resolutionTarget,
        AntiEchoDispositionKind antiEchoDisposition,
        CommunicativeCarrierClassKind carrierClass = CommunicativeCarrierClassKind.ChosenPathBearing,
        bool preservedDistinctionVisible = true,
        bool gelSubstanceConsumptionRequested = false)
    {
        return new CommunicativeFilamentResolutionReceipt(
            ReceiptHandle: "receipt://communicative-filament/session-a",
            CarrierHandle: "carrier://communicative-filament/session-a",
            SourceRetainedWholeRecordHandle: "record://prime-retained-whole/session-a",
            SourceRetainedWholeKind: PrimeRetainedWholeKind.RetainedWholeUnclosed,
            CarrierClass: carrierClass,
            ResolutionTarget: resolutionTarget,
            AntiEchoDisposition: antiEchoDisposition,
            PreservedDistinctionVisible: preservedDistinctionVisible,
            GelSubstanceConsumptionRequested: gelSubstanceConsumptionRequested,
            GelSubstanceConsumptionRefused: gelSubstanceConsumptionRequested,
            CarrierTransportOnly: true,
            PointerHandles: ["pointer://history", "pointer://decision"],
            SymbolicCarrierHandles: ["carrier://projection", "carrier://cryptic"],
            PreservedResidues: CreateResidues(2),
            DeferredResidues: CreateResidues(1),
            ConstraintCodes: ["communicative-filament-pointer-and-symbolic-carrier-only"],
            ReasonCode: "communicative-filament-test",
            LawfulBasis: "communicative filament test basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 08, 00, 00, TimeSpan.Zero));
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
