namespace San.Common;

public sealed record MosStorageSeatRefusalReason(
    string ReasonCode,
    string Summary,
    bool RequiresExplicitReceipt);

public sealed record MosStorageSeatRecord(
    string SeatId,
    string SeatProfile,
    string SeatClass,
    string LegalGovernmentalSeatSummary,
    string NonGoverningRule,
    string NonAuthorizationRule,
    IReadOnlyList<string> WitnessRefs);

public sealed record CMosSurfaceRecord(
    string CMosId,
    string MosStorageSeatRef,
    string DerivationSummary,
    string NonSovereignRule,
    IReadOnlyList<string> WitnessRefs);

public sealed record OeStructuralStandingRecord(
    string StandingId,
    string MosStorageSeatRef,
    string StandingSummary,
    IReadOnlyList<string> WitnessRefs);

public sealed record SelfGelStructuralStandingRecord(
    string StandingId,
    string MosStorageSeatRef,
    string StandingSummary,
    IReadOnlyList<string> WitnessRefs);

public enum CrypticDerivativeSurfaceKind
{
    COe = 0,
    CSelfGel = 1
}

public sealed record CrypticDerivativeSurfacingRecord(
    string SurfacingId,
    CrypticDerivativeSurfaceKind DerivativeKind,
    string CMosSurfaceRef,
    string ParentStructuralStandingRef,
    string DerivationSummary,
    string NonSovereignRule,
    IReadOnlyList<string> WitnessRefs);

public sealed record GelCgelToMosHandoffReceipt(
    string ReceiptId,
    string SourceGelRef,
    string SourceCGelRef,
    string TargetMosSeatRef,
    string TargetCMosRef,
    IReadOnlyList<string> OccupantStandingRefs,
    string ContinuityRule,
    string NonAuthorizationRule,
    DateTimeOffset Timestamp);
