using System.Collections.ObjectModel;

namespace AgentiCore.Models;

public sealed record AgentiSelfGelWorkingPool(
    string SessionHandle,
    string WorkingStateHandle,
    string ProvenanceMarker,
    string CSelfGelHandle,
    string Classification,
    IReadOnlyList<string> ActiveConcepts,
    IReadOnlyDictionary<string, string> WorkingMemory);

public sealed record AgentiSymbolicTrace(
    string TraceId,
    string DecisionBranch,
    string SheafDomain,
    string Classification,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Tokens);

public sealed record AgentiEngramCandidate(
    string Decision,
    bool CommitRequired,
    string ReturnCandidatePointer,
    string Classification,
    IReadOnlyList<string> EngramReferences,
    IReadOnlyList<string> ConstructorDomains);

public sealed record AgentiTransientResidue(
    string CleaveResidue,
    string HostedSemanticDecision,
    string Classification);

public static class AgentiSelfGelWorkingPoolFactory
{
    public static AgentiSelfGelWorkingPool Create(
        string sessionHandle,
        string workingStateHandle,
        string provenanceMarker,
        string cSelfGelHandle,
        IReadOnlyList<string> activeConcepts,
        IDictionary<string, string> workingMemory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingStateHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenanceMarker);
        ArgumentException.ThrowIfNullOrWhiteSpace(cSelfGelHandle);
        ArgumentNullException.ThrowIfNull(activeConcepts);
        ArgumentNullException.ThrowIfNull(workingMemory);

        return new AgentiSelfGelWorkingPool(
            SessionHandle: sessionHandle,
            WorkingStateHandle: workingStateHandle,
            ProvenanceMarker: provenanceMarker,
            CSelfGelHandle: cSelfGelHandle,
            Classification: "bounded-selfgel-working-pool",
            ActiveConcepts: activeConcepts.ToArray(),
            WorkingMemory: new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(workingMemory, StringComparer.Ordinal)));
    }
}
