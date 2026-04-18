using AgentiCore;
using CradleTek.Custody;
using CradleTek.Host;
using CradleTek.Mantle;
using CradleTek.Memory;
using CradleTek.Runtime;
using San.FirstRun;
using San.PrimeCryptic.Services;
using San.Nexus.Control;
using San.State.Modulation;
using San.Runtime.Materialization;
using San.HostedLlm;
using San.Trace.Persistence;
using SLI.Engine;
using SLI.Ingestion;
using SLI.Lisp;
using SoulFrame.Bootstrap;
using SoulFrame.Membrane;

namespace San.Runtime.Headless;

public static class HeadlessRuntimeBootstrap
{
    public static GovernedSeedHeadlessRuntimeStack CreateStack()
    {
        var parser = new SeedEvidencePacketParser();
        var sanctuaryIngressService = new GovernedSeedSanctuaryIngressEngrammitizationService();
        var lispBundleService = new GovernedCrypticLispBundleService();
        var hostedLlmSeedService = new GovernedHostedLlmSeedService(new GovernedHostedLlmLocalRuntimeProvider());
        var highMindUptakeService = new GovernedSeedHighMindUptakeService();
        var pointerStore = new InMemoryGovernedCrypticPointerStore();
        var telemetrySink = new InMemoryGovernedGelTelemetrySink();
        var traceService = new GovernedSeedEnvelopeTraceService(pointerStore, telemetrySink);
        var floorEvaluator = new CrypticFloorEvaluator(parser, lispBundleService);
        var cognition = new GovernedSeedCognitionService(
            hostedLlmSeedService,
            highMindUptakeService,
            floorEvaluator,
            new San.Common.DefaultPredicateMintProjector(),
            new San.Common.DefaultCrypticDerivationPolicy());
        var engramCorpusSource = GovernedSeedResidentMemoryBootstrap.CreateEngramCorpusSource();
        var rootAtlasSource = GovernedSeedResidentMemoryBootstrap.CreateRootAtlasSource();
        var memoryContextService = new GovernedSeedMemoryContextService(
            new GovernedEngramResolverService(engramCorpusSource),
            new GovernedRootOntologicalCleaver(rootAtlasSource),
            new GovernedSelfGelValidationHandleProjector());
        var primeCrypticServiceBroker = new PrimeCrypticServiceBroker(lispBundleService);
        var nexusControlService = new GovernedNexusControlService();
        var crypticHoldingService = new GovernedSeedCrypticHoldingService();
        var formOrCleaveService = new GovernedSeedFormOrCleaveService();
        var candidateSeparationService = new GovernedSeedCandidateSeparationService();
        var admissionGateService = new PrimeSeedPreDomainAdmissionGateService();
        var domainRoleGatingService = new GovernedSeedDomainRoleGatingService();
        var domainAdmissionRoleBindingService = new GovernedSeedDomainAdmissionRoleBindingService();
        var preDomainHostLoopService = new GovernedSeedPreDomainHostLoopService(
            crypticHoldingService,
            formOrCleaveService,
            candidateSeparationService,
            admissionGateService);
        var runtimeMaterializationService = new GovernedSeedRuntimeMaterializationService(
            new GovernedFirstRunConstitutionService(),
            new GovernedSeedPreGovernanceService(),
            new GovernedSeedPreDomainGovernancePacketMaterializationService(),
            new GovernedSeedDomainRoleGatingPacketMaterializationService());
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
            sanctuaryIngressService,
            membrane,
            soulFrameBootstrap,
            primeCrypticServiceBroker,
            nexusControlService,
            runtimeMaterializationService,
            preDomainHostLoopService,
            domainRoleGatingService,
            domainAdmissionRoleBindingService,
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
