namespace San.Common;

public enum CompassOrientationPosture
{
    Centered = 0,
    Seeking = 1,
    Disoriented = 2
}

public enum CompassAdmissibilityEstimate
{
    CandidateOnly = 0,
    Reviewable = 1,
    ProvisionallyAdmissible = 2,
    Withheld = 3
}

public enum CompassTransitionRecommendation
{
    Hold = 0,
    ProceedBounded = 1,
    ReviewRequired = 2,
    RepairRecommended = 3,
    Refuse = 4
}

public enum CompassAuthorityPosture
{
    CandidateOnly = 0
}

public sealed record CompassCandidateModulationInput(
    string InputHandle,
    string InputKind,
    string SourceReason);

public sealed record CompassProjectionPacket(
    string PacketHandle,
    string? CompassEmbodimentHandle,
    string? ListeningFrameHandle,
    CompassDriftState DriftState,
    CompassOrientationPosture OrientationPosture,
    CompassAdmissibilityEstimate AdmissibilityEstimate,
    CompassTransitionRecommendation TransitionRecommendation,
    CompassAuthorityPosture AuthorityPosture,
    IReadOnlyList<CompassCandidateModulationInput> CandidateInputs,
    IReadOnlyList<string> ReviewNotes,
    DateTimeOffset TimestampUtc);
