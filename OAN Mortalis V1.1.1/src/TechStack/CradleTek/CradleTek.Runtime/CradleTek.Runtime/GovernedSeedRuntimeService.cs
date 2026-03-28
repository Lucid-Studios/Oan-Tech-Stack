using Oan.Common;
using Oan.Nexus.Control;
using Oan.PrimeCryptic.Services;
using Oan.Runtime.Materialization;
using Oan.State.Modulation;
using Oan.Trace.Persistence;
using SLI.Ingestion;
using SoulFrame.Bootstrap;
using SoulFrame.Membrane;

namespace CradleTek.Runtime;

public sealed class GovernedSeedRuntimeService
{
    private readonly IGovernedSeedSanctuaryIngressEngrammitizationService _sanctuaryIngressService;
    private readonly IGovernedSeedMembraneService _membraneService;
    private readonly IGovernedSeedSoulFrameBootstrapService _bootstrapService;
    private readonly IPrimeCrypticServiceBroker _primeCrypticServiceBroker;
    private readonly IGovernedNexusControlService _nexusControlService;
    private readonly IGovernedSeedRuntimeMaterializationService _materializationService;
    private readonly IGovernedStateModulationService _stateModulationService;
    private readonly IGovernedSeedEnvelopeTraceService _traceService;

    public GovernedSeedRuntimeService(
        IGovernedSeedSanctuaryIngressEngrammitizationService sanctuaryIngressService,
        IGovernedSeedMembraneService membraneService,
        IGovernedSeedSoulFrameBootstrapService bootstrapService,
        IPrimeCrypticServiceBroker primeCrypticServiceBroker,
        IGovernedNexusControlService nexusControlService,
        IGovernedSeedRuntimeMaterializationService materializationService,
        IGovernedStateModulationService stateModulationService,
        IGovernedSeedEnvelopeTraceService traceService)
    {
        _sanctuaryIngressService = sanctuaryIngressService ?? throw new ArgumentNullException(nameof(sanctuaryIngressService));
        _membraneService = membraneService ?? throw new ArgumentNullException(nameof(membraneService));
        _bootstrapService = bootstrapService ?? throw new ArgumentNullException(nameof(bootstrapService));
        _primeCrypticServiceBroker = primeCrypticServiceBroker ?? throw new ArgumentNullException(nameof(primeCrypticServiceBroker));
        _nexusControlService = nexusControlService ?? throw new ArgumentNullException(nameof(nexusControlService));
        _materializationService = materializationService ?? throw new ArgumentNullException(nameof(materializationService));
        _stateModulationService = stateModulationService ?? throw new ArgumentNullException(nameof(stateModulationService));
        _traceService = traceService ?? throw new ArgumentNullException(nameof(traceService));
    }

    public async Task<EvaluateEnvelope> EvaluateAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sanctuaryIngress = _sanctuaryIngressService.Prepare(agentId, theaterId, input);
        var primeCrypticReceipt = _primeCrypticServiceBroker.DescribeResidentField(agentId, theaterId);
        var bootstrapReceipt = _bootstrapService.Bootstrap(agentId, theaterId);
        var bootstrapNexusResult = _nexusControlService.EvaluateBootstrapAdmission(primeCrypticReceipt, bootstrapReceipt);
        var bootstrapAdmissionReceipt = _materializationService.CreateBootstrapAdmissionReceipt(
            bootstrapNexusResult.Posture,
            bootstrapNexusResult.Request,
            bootstrapNexusResult.Decision);

        if (!bootstrapAdmissionReceipt.MembraneWakePermitted)
        {
            var bootstrapDeniedResult = _materializationService.MaterializeBootstrapDeniedResult(
                agentId,
                theaterId,
                input,
                sanctuaryIngress.Receipt,
                primeCrypticReceipt,
                bootstrapReceipt,
                bootstrapNexusResult.Posture,
                bootstrapNexusResult.Request,
                bootstrapNexusResult.Decision,
                bootstrapAdmissionReceipt);
            var deniedStateModulationReceipt = _stateModulationService.CreateReceipt(
                primeCrypticReceipt,
                bootstrapReceipt,
                bootstrapDeniedResult);
            var hydratedDeniedResult = _materializationService.AttachStateModulation(
                bootstrapDeniedResult,
                deniedStateModulationReceipt);

            var deniedEnvelope = _materializationService.CreateEnvelope(agentId, theaterId, hydratedDeniedResult);
            return await _traceService.TraceAsync(deniedEnvelope, hydratedDeniedResult, cancellationToken).ConfigureAwait(false);
        }

        var result = await _membraneService.EvaluateAsync(
            new GovernedSeedEvaluationRequest(
                AgentId: agentId,
                TheaterId: theaterId,
                Input: sanctuaryIngress.PreparedInput,
                AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                BootstrapReceipt: bootstrapReceipt,
                SanctuaryIngressReceipt: sanctuaryIngress.Receipt),
            cancellationToken).ConfigureAwait(false);
        var situationalContext = result.VerticalSlice.SituationalContext
            ?? throw new InvalidOperationException("SoulFrame must expose situational context before nexus evaluation.");
        var nexusResult = _nexusControlService.Evaluate(primeCrypticReceipt, bootstrapReceipt, situationalContext);
        var nexusHydratedResult = _materializationService.HydrateNexusAndPrime(
            result,
            agentId,
            theaterId,
            input,
            bootstrapReceipt,
            bootstrapAdmissionReceipt,
            primeCrypticReceipt,
            nexusResult.Posture,
            nexusResult.Request,
            nexusResult.Decision);
        var stateModulationReceipt = _stateModulationService.CreateReceipt(primeCrypticReceipt, bootstrapReceipt, nexusHydratedResult);
        var hydratedResult = _materializationService.AttachStateModulation(nexusHydratedResult, stateModulationReceipt);

        var envelope = _materializationService.CreateEnvelope(agentId, theaterId, hydratedResult);
        return await _traceService.TraceAsync(envelope, hydratedResult, cancellationToken).ConfigureAwait(false);
    }
}
