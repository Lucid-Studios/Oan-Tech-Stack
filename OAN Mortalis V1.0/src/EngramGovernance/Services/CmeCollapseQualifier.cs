using Oan.Common;

namespace EngramGovernance.Services;

public sealed class CmeCollapseQualifier : ICmeCollapseQualifier
{
    private const double LowConfidenceThreshold = 0.80d;

    public CmeCollapseQualificationResult Qualify(CmeCollapseClassification classification)
    {
        ArgumentNullException.ThrowIfNull(classification);

        var evidenceFlags = classification.EvidenceFlags;
        var reviewTriggers = classification.ReviewTriggers;

        if (classification.AutobiographicalRelevant)
        {
            evidenceFlags |= CmeCollapseEvidenceFlag.AutobiographicalSignal;
        }

        if (classification.SelfGelIdentified)
        {
            evidenceFlags |= CmeCollapseEvidenceFlag.SelfGelIdentitySignal;
        }

        if (classification.CollapseConfidence < LowConfidenceThreshold)
        {
            reviewTriggers |= CmeCollapseReviewTrigger.LowConfidence;
        }

        var hasIdentitySignals = evidenceFlags.HasFlag(CmeCollapseEvidenceFlag.AutobiographicalSignal) ||
                                 evidenceFlags.HasFlag(CmeCollapseEvidenceFlag.SelfGelIdentitySignal) ||
                                 evidenceFlags.HasFlag(CmeCollapseEvidenceFlag.WitnessBearingSignal);
        var hasContextSignals = evidenceFlags.HasFlag(CmeCollapseEvidenceFlag.ContextualSignal) ||
                                evidenceFlags.HasFlag(CmeCollapseEvidenceFlag.ProceduralSignal) ||
                                evidenceFlags.HasFlag(CmeCollapseEvidenceFlag.SkillMethodSignal);

        if (hasIdentitySignals && hasContextSignals)
        {
            evidenceFlags |= CmeCollapseEvidenceFlag.MixedSignal;
            reviewTriggers |= CmeCollapseReviewTrigger.MixedIdentityContext;
        }

        if (evidenceFlags == CmeCollapseEvidenceFlag.None)
        {
            reviewTriggers |= CmeCollapseReviewTrigger.InsufficientEvidence;
        }

        var residueClass = classification.AutobiographicalRelevant || classification.SelfGelIdentified
            ? CmeCollapseResidueClass.AutobiographicalProtected
            : CmeCollapseResidueClass.ContextualProtected;
        var disposition = residueClass == CmeCollapseResidueClass.AutobiographicalProtected
            ? CmeCollapseDisposition.RouteToCMoS
            : CmeCollapseDisposition.RouteToCGoA;

        return new CmeCollapseQualificationResult(
            Disposition: disposition,
            ResidueClass: residueClass,
            ClassificationConfidence: classification.CollapseConfidence,
            EvidenceFlags: evidenceFlags,
            ReviewTriggers: reviewTriggers,
            SourceSubsystem: string.IsNullOrWhiteSpace(classification.SourceSubsystem)
                ? "unknown"
                : classification.SourceSubsystem.Trim(),
            TargetClass: disposition == CmeCollapseDisposition.RouteToCMoS ? "cMoS" : "cGoA");
    }
}
