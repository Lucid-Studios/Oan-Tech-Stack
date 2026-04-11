using San.Common;

namespace SLI.Engine;

public enum PredicateLandingRouteKind
{
    DirectTransit = 0,
    BoundedEcTransit = 1
}

public enum CrypticFloorDisposition
{
    Withhold = 0,
    Ready = 1,
    Refuse = 2
}

public sealed record PredicateLandingRequest(
    SymbolicEnvelope Envelope,
    MembraneDecision MembraneDecision,
    string? SanctuaryGelHandle,
    string? IssuedRtmeHandle,
    string? RouteHandle,
    PredicateLandingRouteKind RouteKind);

public sealed record CrypticFloorEvaluation(
    bool PredicateLandingReady,
    CrypticFloorDisposition Disposition,
    string OutcomeCode,
    string GovernanceTrace,
    SymbolicEnvelope? Envelope);
