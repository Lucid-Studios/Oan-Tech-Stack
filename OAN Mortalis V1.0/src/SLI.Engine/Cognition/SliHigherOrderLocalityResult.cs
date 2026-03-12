using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

internal sealed class SliPerspectiveResult
{
    public required bool IsConfigured { get; init; }
    public required IReadOnlyDictionary<string, double> OrientationVector { get; init; }
    public required IReadOnlyList<string> EthicalConstraints { get; init; }
    public required IReadOnlyDictionary<string, double> WeightFunctions { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
}

internal sealed class SliParticipationResult
{
    public required bool IsConfigured { get; init; }
    public required string Mode { get; init; }
    public required string Role { get; init; }
    public required IReadOnlyList<string> InteractionRules { get; init; }
    public required IReadOnlyList<string> CapabilitySet { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
}

internal sealed class SliHigherOrderLocalityResult
{
    public required string LocalityHandle { get; init; }
    public required string SelfAnchor { get; init; }
    public required string OtherAnchor { get; init; }
    public required string RelationAnchor { get; init; }
    public required string SealPosture { get; init; }
    public required string RevealPosture { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
    public required SliPerspectiveResult Perspective { get; init; }
    public required SliParticipationResult Participation { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
}
