using San.Common;

namespace SLI.Runtime;

public enum GovernedReturnReceiptFamily
{
    PermissionReceipt = 0,
    RestrictionReceipt = 1,
    CorrectionReceipt = 2,
    RedirectionReceipt = 3,
    CollapseReceipt = 4,
    DeferReceipt = 5,
    RefusalNoticeReceipt = 6,
    QuarantineNoticeReceipt = 7,
    TransformedInstructionReceipt = 8
}

public enum GovernedReturnAcknowledgementBurden
{
    AcknowledgeOnly = 0,
    AcknowledgeAndComplianceRecord = 1,
    AcknowledgeAndWitnessLog = 2,
    AcknowledgeAndRepresentPacket = 3
}

public enum GovernedReturnTransformabilityRule
{
    VerbatimOnly = 0,
    RenderOnly = 1,
    BoundedLocalPackaging = 2
}

public sealed record GovernedReturnReceipt(
    string ReceiptId,
    GovernedReturnReceiptFamily ReturnFamily,
    string SourceReviewJurisdiction,
    string SourceStateLineage,
    string TraceId,
    AdmissibilityStatus AdmissibilityPosture,
    string AuthorityCeiling,
    string BurdenOfReturn,
    GovernedReturnAcknowledgementBurden RequiredAcknowledgement,
    string? ExpiryOrRepresentWindow,
    GovernedReturnTransformabilityRule LocalTransformabilityRule,
    DateTimeOffset TimestampUtc);
