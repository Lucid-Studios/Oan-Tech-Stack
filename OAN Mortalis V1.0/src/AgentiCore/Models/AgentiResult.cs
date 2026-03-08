namespace AgentiCore.Models;

public sealed class AgentiResult
{
    public required Guid ContextId { get; init; }
    public required string ResultType { get; init; }
    public required string ResultPayload { get; init; }
    public required bool EngramCommitRequired { get; init; }
    public required AgentiSelfGelWorkingPool SelfGelWorkingPool { get; init; }
    public required AgentiSymbolicTrace SymbolicTrace { get; init; }
    public required AgentiEngramCandidate EngramCandidate { get; init; }
    public required AgentiTransientResidue TransientResidue { get; init; }
}
