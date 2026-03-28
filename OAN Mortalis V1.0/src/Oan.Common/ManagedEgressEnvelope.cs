namespace Oan.Common;

public enum SliEgressEffectKind
{
    Telemetry,
    JournalAppend,
    ArtifactWrite,
    StructuralCreation
}

public enum SliEgressRetentionPosture
{
    Ephemeral,
    DebugOnly,
    CIArtifact,
    GovernanceArtifact,
    ImmutableLedger
}

public enum SliEgressJurisdictionClass
{
    Operator,
    Cradle,
    CoreRuntime,
    AgentiCore
}

public enum SliEgressTargetSinkClass
{
    Null,
    Console,
    FileSystemLocal,
    MemoryJournal,
    HdtLedger
}

/// <summary>
/// Authoritative envelope defining strict governance bounds for any payload or side-effect 
/// egressing from the SLI framework.
/// </summary>
public sealed record ManagedEgressEnvelope(
    SliEgressEffectKind EffectKind,
    SliEgressRetentionPosture RetentionPosture,
    SliEgressJurisdictionClass JurisdictionClass,
    bool IdentityFormingAllowed,
    SliEgressTargetSinkClass TargetSinkClass,
    string AuthorityReason
);
