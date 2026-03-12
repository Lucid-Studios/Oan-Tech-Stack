using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

internal sealed class SliWitnessResult
{
    public required bool IsConfigured { get; init; }
    public required string WitnessHandle { get; init; }
    public required string LeftLocalityHandle { get; init; }
    public required string RightLocalityHandle { get; init; }
    public required IReadOnlyList<string> PreservedInvariants { get; init; }
    public required IReadOnlyList<string> DifferenceSet { get; init; }
    public required double GlueThreshold { get; init; }
    public required string CandidacyStatus { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
}

internal sealed class SliBoundedWitnessResult
{
    public required SliHigherOrderLocalityResult Locality { get; init; }
    public required SliRehearsalResult Rehearsal { get; init; }
    public required SliWitnessResult Witness { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
}
