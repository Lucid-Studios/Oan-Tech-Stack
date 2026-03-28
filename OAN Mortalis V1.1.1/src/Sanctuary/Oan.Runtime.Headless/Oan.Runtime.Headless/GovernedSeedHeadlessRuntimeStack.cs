using CradleTek.Host;
using Oan.Trace.Persistence;

namespace Oan.Runtime.Headless;

public sealed record GovernedSeedHeadlessRuntimeStack(
    IGovernedSeedHost Host,
    InMemoryGovernedCrypticPointerStore PointerStore,
    InMemoryGovernedGelTelemetrySink TelemetrySink,
    IGovernedSeedEnvelopeTraceService EnvelopeTraceService);
