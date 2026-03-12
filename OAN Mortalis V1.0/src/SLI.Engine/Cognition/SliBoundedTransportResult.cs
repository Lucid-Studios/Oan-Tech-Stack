using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

internal sealed class SliTransportMappingResult
{
    public required string Source { get; init; }
    public required string Target { get; init; }
}

internal sealed class SliTransportResult
{
    public required bool IsConfigured { get; init; }
    public required string TransportHandle { get; init; }
    public required string WitnessHandle { get; init; }
    public required string SourceLocalityHandle { get; init; }
    public required string TargetLocalityHandle { get; init; }
    public required IReadOnlyList<string> PreservedInvariants { get; init; }
    public required IReadOnlyList<SliTransportMappingResult> MappedDifferences { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
    public required string Status { get; init; }
}

internal sealed class SliBoundedTransportResult
{
    public required SliHigherOrderLocalityResult Locality { get; init; }
    public required SliRehearsalResult Rehearsal { get; init; }
    public required SliWitnessResult Witness { get; init; }
    public required SliTransportResult Transport { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
}
