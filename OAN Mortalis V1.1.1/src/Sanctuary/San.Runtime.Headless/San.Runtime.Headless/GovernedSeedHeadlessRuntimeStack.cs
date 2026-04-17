using CradleTek.Host;
using San.Trace.Persistence;

namespace San.Runtime.Headless;

public sealed record GovernedSeedHeadlessRuntimeStack(
    IGovernedSeedHost Host,
    InMemoryGovernedCrypticPointerStore PointerStore,
    InMemoryGovernedGelTelemetrySink TelemetrySink,
    IGovernedSeedEnvelopeTraceService EnvelopeTraceService);
