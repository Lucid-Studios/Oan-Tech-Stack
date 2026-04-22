namespace San.Common;

public enum ListeningFrameVisibilityPosture
{
    CommunityLegible = 0,
    OperatorGuarded = 1,
    CrypticOnly = 2
}

public enum ListeningFrameIntegrityState
{
    Usable = 0,
    Sparse = 1,
    Broken = 2
}

public enum ListeningFrameReviewPosture
{
    CandidateOnly = 0,
    ReviewRecommended = 1,
    ReviewRequired = 2
}

public sealed record ListeningFrameProjectionPacket(
    string PacketHandle,
    string? ListeningFrameHandle,
    string? ChamberHandle,
    string? SourceSurfaceHandle,
    ListeningFrameVisibilityPosture VisibilityPosture,
    ListeningFrameIntegrityState IntegrityState,
    ListeningFrameReviewPosture ReviewPosture,
    bool UsableForCompassProjection,
    IReadOnlyList<string> PostureMarkers,
    IReadOnlyList<string> ReviewNotes,
    DateTimeOffset TimestampUtc);
