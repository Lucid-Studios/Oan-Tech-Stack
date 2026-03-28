namespace CradleTek.CognitionHost.Models;

public sealed class CognitionSelfStateHint
{
    public required int ClaimCount { get; init; }
    public required bool HasDeferredOrContradictedClaim { get; init; }
    public required bool HasHotClaim { get; init; }
    public required int ValidationConceptCount { get; init; }
}
