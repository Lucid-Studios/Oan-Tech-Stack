using AgentiCore;
using CradleTek.Custody;
using CradleTek.Host;
using CradleTek.Mantle;
using CradleTek.Memory;
using CradleTek.Runtime;
using Oan.PrimeCryptic.Services;
using Oan.Nexus.Control;
using Oan.State.Modulation;
using Oan.Runtime.Materialization;
using Oan.HostedLlm;
using Oan.Trace.Persistence;
using SLI.Engine;
using SLI.Ingestion;
using SLI.Lisp;
using SoulFrame.Bootstrap;
using SoulFrame.Membrane;

namespace Oan.Runtime.Headless;

public static class HeadlessRuntimeBootstrap
{
    public static GovernedSeedHeadlessRuntimeStack CreateStack()
    {
        var parser = new SeedEvidencePacketParser();
        var lispBundleService = new GovernedCrypticLispBundleService();
        var hostedLlmSeedService = new GovernedHostedLlmSeedService(new GovernedHostedLlmLocalRuntimeProvider());
        var pointerStore = new InMemoryGovernedCrypticPointerStore();
        var telemetrySink = new InMemoryGovernedGelTelemetrySink();
        var traceService = new GovernedSeedEnvelopeTraceService(pointerStore, telemetrySink);
        var floorEvaluator = new CrypticFloorEvaluator(parser, lispBundleService);
        var cognition = new GovernedSeedCognitionService(
            hostedLlmSeedService,
            floorEvaluator,
            new Oan.Common.DefaultPredicateMintProjector(),
            new Oan.Common.DefaultCrypticDerivationPolicy());
        var engramCorpusSource = GovernedSeedResidentMemoryBootstrap.CreateEngramCorpusSource();
        var rootAtlasSource = GovernedSeedResidentMemoryBootstrap.CreateRootAtlasSource();
        var memoryContextService = new GovernedSeedMemoryContextService(
            new GovernedEngramResolverService(engramCorpusSource),
            new GovernedRootOntologicalCleaver(rootAtlasSource),
            new GovernedSelfGelValidationHandleProjector());
        var primeCrypticServiceBroker = new PrimeCrypticServiceBroker(lispBundleService);
        var nexusControlService = new GovernedNexusControlService();
        var runtimeMaterializationService = new GovernedSeedRuntimeMaterializationService();
        var stateModulationService = new GovernedStateModulationService();
        var custodySource = new BootstrapCustodySource();
        var soulFrameBootstrap = new GovernedSeedSoulFrameBootstrapService(custodySource);
        var membrane = new GovernedSeedMembraneService(
            cognition,
            new GovernedSeedProjectionService(),
            new GovernedSeedReturnIntakeService(),
            new GovernedSeedProtectedHoldRoutingService(nexusControlService),
            new GovernedSeedStewardshipService(nexusControlService),
            memoryContextService,
            new GovernedSeedLowMindSfRoutingService(),
            new GovernedSeedSituationalContextService());
        var runtime = new GovernedSeedRuntimeService(
            membrane,
            soulFrameBootstrap,
            primeCrypticServiceBroker,
            nexusControlService,
            runtimeMaterializationService,
            stateModulationService,
            traceService);

        return new GovernedSeedHeadlessRuntimeStack(
            Host: new GovernedSeedHost(runtime),
            PointerStore: pointerStore,
            TelemetrySink: telemetrySink,
            EnvelopeTraceService: traceService);
    }

    public static IGovernedSeedHost CreateHost() => CreateStack().Host;
}
