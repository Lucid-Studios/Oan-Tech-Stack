namespace SLI.Engine.Runtime;

public enum SliCoreOperandKind
{
    Symbol,
    StringLiteral,
    ListExpression
}

public enum SliRuntimeCapabilityAvailability
{
    Available,
    Unavailable
}

public enum SliRuntimeOperationClass
{
    HostOnly,
    SharedContract,
    TargetCandidate
}

public sealed record SliCoreOperand(
    SliCoreOperandKind Kind,
    string Value);

public sealed record SliCoreInstruction(
    string Opcode,
    IReadOnlyList<SliCoreOperand> Operands,
    string CanonicalForm,
    string MeaningAuthority,
    SliRuntimeCapabilityAvailability Availability,
    SliRuntimeOperationClass OperationClass);

public sealed record SliCoreProgram(
    string ProgramId,
    string MeaningAuthority,
    IReadOnlyList<SliCoreInstruction> Instructions);

public sealed record SliRuntimeOperationCapability(
    string Opcode,
    string MeaningAuthority,
    SliRuntimeCapabilityAvailability Availability,
    SliRuntimeOperationClass OperationClass);

public sealed record SliRuntimeRealizationProfile(
    string ProfileId,
    string ResidencyClass,
    int MaxInstructionCount,
    int MaxSymbolicDepth,
    bool SupportsHigherOrderLocality,
    bool SupportsBoundedRehearsal,
    bool SupportsBoundedWitness,
    bool SupportsBoundedTransport,
    bool SupportsAdmissibleSurface,
    bool SupportsAccountabilityPacket,
    bool SupportsResidueEmission,
    bool SupportsSymbolicTrace)
{
    public static SliRuntimeRealizationProfile CreateHostManaged(string profileId = "host-managed-runtime")
    {
        return new SliRuntimeRealizationProfile(
            ProfileId: profileId,
            ResidencyClass: "host-managed",
            MaxInstructionCount: int.MaxValue,
            MaxSymbolicDepth: int.MaxValue,
            SupportsHigherOrderLocality: true,
            SupportsBoundedRehearsal: true,
            SupportsBoundedWitness: true,
            SupportsBoundedTransport: true,
            SupportsAdmissibleSurface: true,
            SupportsAccountabilityPacket: true,
            SupportsResidueEmission: true,
            SupportsSymbolicTrace: true);
    }

    public static SliRuntimeRealizationProfile CreateTargetBounded(
        string profileId,
        bool supportsHigherOrderLocality,
        bool supportsBoundedRehearsal,
        bool supportsBoundedWitness,
        bool supportsBoundedTransport,
        bool supportsAdmissibleSurface,
        bool supportsAccountabilityPacket,
        bool supportsResidueEmission = true,
        bool supportsSymbolicTrace = true,
        int maxInstructionCount = 256,
        int maxSymbolicDepth = 128,
        string residencyClass = "target-bounded")
    {
        return new SliRuntimeRealizationProfile(
            ProfileId: profileId,
            ResidencyClass: residencyClass,
            MaxInstructionCount: maxInstructionCount,
            MaxSymbolicDepth: maxSymbolicDepth,
            SupportsHigherOrderLocality: supportsHigherOrderLocality,
            SupportsBoundedRehearsal: supportsBoundedRehearsal,
            SupportsBoundedWitness: supportsBoundedWitness,
            SupportsBoundedTransport: supportsBoundedTransport,
            SupportsAdmissibleSurface: supportsAdmissibleSurface,
            SupportsAccountabilityPacket: supportsAccountabilityPacket,
            SupportsResidueEmission: supportsResidueEmission,
            SupportsSymbolicTrace: supportsSymbolicTrace);
    }
}

public sealed class SliRuntimeCapabilityManifest
{
    private readonly Dictionary<string, SliRuntimeOperationCapability> _capabilities;

    public SliRuntimeCapabilityManifest(
        string runtimeId,
        string meaningAuthority,
        SliRuntimeRealizationProfile realizationProfile,
        IEnumerable<SliRuntimeOperationCapability> capabilities)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(meaningAuthority);
        ArgumentNullException.ThrowIfNull(realizationProfile);
        ArgumentNullException.ThrowIfNull(capabilities);

        RuntimeId = runtimeId;
        MeaningAuthority = meaningAuthority;
        RealizationProfile = realizationProfile;
        _capabilities = capabilities
            .GroupBy(capability => capability.Opcode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last(),
                StringComparer.OrdinalIgnoreCase);
    }

    public string RuntimeId { get; }
    public string MeaningAuthority { get; }
    public SliRuntimeRealizationProfile RealizationProfile { get; }
    public IReadOnlyDictionary<string, SliRuntimeOperationCapability> Capabilities => _capabilities;

    public bool TryGetCapability(string opcode, out SliRuntimeOperationCapability capability)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(opcode);
        return _capabilities.TryGetValue(opcode, out capability!);
    }
}
