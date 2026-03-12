using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

internal sealed class SliAccountabilityPacketResult
{
    public required bool IsConfigured { get; init; }
    public required string PacketHandle { get; init; }
    public required string SurfaceHandle { get; init; }
    public required string TransportHandle { get; init; }
    public required string WitnessHandle { get; init; }
    public required string SourceLocalityHandle { get; init; }
    public required string TargetLocalityHandle { get; init; }
    public required IReadOnlyList<string> PreservedInvariants { get; init; }
    public required string SurfaceClass { get; init; }
    public required bool IdentityBearingApplicable { get; init; }
    public required string RevealPosture { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
    public required string ReadinessStatus { get; init; }
}

internal sealed class SliBoundedAccountabilityPacketResult
{
    public required SliHigherOrderLocalityResult Locality { get; init; }
    public required SliRehearsalResult Rehearsal { get; init; }
    public required SliWitnessResult Witness { get; init; }
    public required SliTransportResult Transport { get; init; }
    public required SliAdmissibleSurfaceResult Surface { get; init; }
    public required SliAccountabilityPacketResult Packet { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
}
