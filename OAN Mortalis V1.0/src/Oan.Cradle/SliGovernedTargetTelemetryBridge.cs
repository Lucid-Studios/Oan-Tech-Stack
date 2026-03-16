using Oan.Common;
using SLI.Engine;
using SLI.Engine.Runtime;
using SLI.Engine.Telemetry;

namespace Oan.Cradle;

internal sealed record SliGovernedTargetWitnessJournalContext(
    string LoopKey,
    GovernanceLoopStage Stage,
    GovernanceDecisionReceipt? DecisionReceipt = null,
    ReturnCandidateReviewRequest? ReviewRequest = null);

internal sealed class SliGovernedTargetTelemetryBridge
{
    private readonly ITelemetrySink _governanceTelemetry;
    private readonly IGovernanceReceiptJournal? _governanceReceiptJournal;

    public SliGovernedTargetTelemetryBridge(
        ITelemetrySink governanceTelemetry,
        IGovernanceReceiptJournal? governanceReceiptJournal = null)
    {
        _governanceTelemetry = governanceTelemetry ?? throw new ArgumentNullException(nameof(governanceTelemetry));
        _governanceReceiptJournal = governanceReceiptJournal;
    }

    public async Task WitnessHigherOrderLocalityTargetExecutionAsync(
        IReadOnlyList<string> symbolicProgram,
        string objective,
        IEnumerable<string>? supportedOpcodes = null,
        string runtimeId = "target-sli-runtime",
        SliRuntimeRealizationProfile? realizationProfile = null,
        SliGovernedTargetWitnessJournalContext? journalContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(symbolicProgram);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);

        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync(cancellationToken).ConfigureAwait(false);

        var lowered = bridge.LowerProgram(symbolicProgram);
        var manifest = bridge.CreateTargetCapabilityManifest(
            supportedOpcodes ?? lowered.Instructions.Select(instruction => instruction.Opcode),
            runtimeId,
            realizationProfile);
        var telemetrySink = CreateTelemetrySink(journalContext);

        _ = await bridge.ExecuteHigherOrderLocalityOnTargetAsync(
                symbolicProgram,
                objective,
                manifest,
                telemetrySink,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private ITelemetrySink CreateTelemetrySink(SliGovernedTargetWitnessJournalContext? journalContext)
    {
        if (journalContext is null)
        {
            return _governanceTelemetry;
        }

        if (_governanceReceiptJournal is null)
        {
            throw new InvalidOperationException("Target witness journaling requires a governance receipt journal.");
        }

        return new GovernanceTargetWitnessJournalSink(
            _governanceTelemetry,
            _governanceReceiptJournal,
            journalContext);
    }

    private sealed class GovernanceTargetWitnessJournalSink : ITelemetrySink
    {
        private readonly ITelemetrySink _inner;
        private readonly IGovernanceReceiptJournal _journal;
        private readonly SliGovernedTargetWitnessJournalContext _context;

        public GovernanceTargetWitnessJournalSink(
            ITelemetrySink inner,
            IGovernanceReceiptJournal journal,
            SliGovernedTargetWitnessJournalContext context)
        {
            _inner = inner;
            _journal = journal;
            _context = context;
        }

        public async Task EmitAsync(object telemetryEvent)
        {
            await _inner.EmitAsync(telemetryEvent).ConfigureAwait(false);

            if (telemetryEvent is not SliTargetExecutionTelemetryEvent targetEvent)
            {
                return;
            }

            var receipt = CreateWitnessReceipt(_context, targetEvent);
            await _journal.AppendAsync(
                new GovernanceJournalEntry(
                    _context.LoopKey,
                    GovernanceJournalEntryKind.TargetWitness,
                    _context.Stage,
                    receipt.TimestampUtc.UtcDateTime,
                    _context.DecisionReceipt,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: _context.ReviewRequest,
                    Annotation: null,
                    HopngArtifactReceipt: null,
                    TargetWitnessReceipt: receipt))
                .ConfigureAwait(false);
        }

        private static GovernedTargetWitnessReceipt CreateWitnessReceipt(
            SliGovernedTargetWitnessJournalContext context,
            SliTargetExecutionTelemetryEvent targetEvent)
        {
            var kind = targetEvent.EventType switch
            {
                "sli-target-admission-accepted" => GovernedTargetWitnessKind.AdmissionAccepted,
                "sli-target-admission-refused" => GovernedTargetWitnessKind.AdmissionRefused,
                "sli-target-lineage-recorded" => GovernedTargetWitnessKind.LineageRecorded,
                _ => throw new InvalidOperationException($"Unsupported SLI target telemetry event '{targetEvent.EventType}'.")
            };

            return new GovernedTargetWitnessReceipt(
                WitnessHandle: GovernedTargetWitnessKeys.CreateWitnessHandle(
                    context.LoopKey,
                    kind,
                    targetEvent.EventHash),
                Stage: context.Stage,
                Kind: kind,
                Accepted: targetEvent.Accepted,
                WitnessedBy: targetEvent.WitnessedBy,
                LaneId: targetEvent.LaneId,
                RuntimeId: targetEvent.RuntimeId,
                ProfileId: targetEvent.ProfileId,
                BudgetClass: targetEvent.BudgetClass,
                CommitAuthorityClass: targetEvent.CommitAuthorityClass,
                Objective: targetEvent.Objective,
                ProgramId: targetEvent.ProgramId,
                AdmissionHandle: targetEvent.AdmissionHandle,
                LineageHandle: targetEvent.LineageHandle,
                TraceHandle: targetEvent.TraceHandle,
                ResidueHandle: targetEvent.ResidueHandle,
                Reasons: targetEvent.Reasons.ToArray(),
                ReasonFamilies: targetEvent.ReasonFamilies.ToArray(),
                BudgetUsage: new GovernedTargetExecutionBudgetUsage(
                    InstructionCount: targetEvent.BudgetUsage.InstructionCount,
                    SymbolicDepth: targetEvent.BudgetUsage.SymbolicDepth,
                    ProjectedTraceEntryCount: targetEvent.BudgetUsage.ProjectedTraceEntryCount,
                    ProjectedResidueCount: targetEvent.BudgetUsage.ProjectedResidueCount,
                    WitnessOperationCount: targetEvent.BudgetUsage.WitnessOperationCount,
                    TransportOperationCount: targetEvent.BudgetUsage.TransportOperationCount),
                EmittedTraceCount: targetEvent.EmittedTraceCount,
                EmittedResidueCount: targetEvent.EmittedResidueCount,
                TimestampUtc: targetEvent.Timestamp.Kind == DateTimeKind.Utc
                    ? new DateTimeOffset(targetEvent.Timestamp)
                    : new DateTimeOffset(DateTime.SpecifyKind(targetEvent.Timestamp, DateTimeKind.Utc)));
        }
    }
}
