namespace SLI.Engine.Nexus;

public enum WebFieldState
{
    DormantCoherent = 0,
    Relaxing = 1,
    ReadyForReentry = 2,
    Contradictory = 3
}

public enum MutationEventState
{
    Completed = 0,
    Deferred = 1,
    Obstructed = 2
}

public enum RelaxationState
{
    DormantCoherent = 0,
    IncompleteRelaxation = 1,
    ReadyForReentry = 2,
    Contradictory = 3
}

public enum NexusReadinessState
{
    DormantCoherent = 0,
    NotReady = 1,
    ReadyForReentry = 2,
    Contradictory = 3
}

public sealed record WebTopologySnapshot(
    string SnapshotId,
    DateTime CapturedAtUtc,
    WebFieldState FieldState,
    IReadOnlyList<string> ActiveRegions,
    IReadOnlyList<string> ActiveRelations,
    IReadOnlyList<string> BraidRegions,
    IReadOnlyList<string> EquilibriumMarkers,
    IReadOnlyList<string> UnresolvedStrain);

public sealed record MutationEvent(
    string EventId,
    DateTime OccurredAtUtc,
    string OriginRegion,
    IReadOnlyList<string> AffectedRegions,
    string MutationKind,
    IReadOnlyList<string> PreservedIdentityConstraints,
    int StrainDelta,
    string CausalReason,
    MutationEventState EventState);

public sealed record RelaxationReceipt(
    string ReceiptId,
    DateTime CapturedAtUtc,
    IReadOnlyList<string> SourceMutationIds,
    RelaxationState RelaxationState,
    bool ReadyForReentry,
    int ResidualStrain,
    string BoundaryIntegrityState,
    string ReasonCode);

public sealed record NexusTelemetryFrame(
    string FrameId,
    DateTime CapturedAtUtc,
    string FocalRegion,
    WebFieldState TopologyState,
    int MutationIndex,
    string RelaxationProgress,
    NexusReadinessState ReadinessState,
    string OrientationNotes,
    string ReasonCode);
