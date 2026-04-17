using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SLI.Engine;

public interface IGovernedSeedFormOrCleaveService
{
    GovernedSeedFormOrCleaveAssessment Evaluate(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspectionReceipt);
}

public sealed class GovernedSeedFormOrCleaveService : IGovernedSeedFormOrCleaveService
{
    public GovernedSeedFormOrCleaveAssessment Evaluate(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspectionReceipt)
    {
        ArgumentNullException.ThrowIfNull(formationReceipt);
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(compassProjection);
        ArgumentNullException.ThrowIfNull(holdingInspectionReceipt);

        var timestampUtc = DateTimeOffset.UtcNow;
        var descendantCandidates = CreateDescendantCandidates(formationReceipt, compassProjection);
        var disposition = DetermineDisposition(formationReceipt, listeningFrame, compassProjection, holdingInspectionReceipt, descendantCandidates);
        var carryDisposition = disposition switch
        {
            GovernedSeedFormOrCleaveDispositionKind.Form => GovernedSeedCarryDispositionKind.Carry,
            GovernedSeedFormOrCleaveDispositionKind.Cleave => GovernedSeedCarryDispositionKind.Cleave,
            GovernedSeedFormOrCleaveDispositionKind.Hold => GovernedSeedCarryDispositionKind.Hold,
            _ => GovernedSeedCarryDispositionKind.Refuse
        };
        var collapseDisposition = disposition switch
        {
            GovernedSeedFormOrCleaveDispositionKind.Cleave => GovernedSeedCollapseDispositionKind.Cleave,
            GovernedSeedFormOrCleaveDispositionKind.Hold => GovernedSeedCollapseDispositionKind.Hold,
            GovernedSeedFormOrCleaveDispositionKind.Reject => GovernedSeedCollapseDispositionKind.Refuse,
            _ => GovernedSeedCollapseDispositionKind.None
        };

        return new GovernedSeedFormOrCleaveAssessment(
            AssessmentHandle: CreateHandle(
                "governed-seed-form-or-cleave://",
                formationReceipt.ReceiptHandle,
                listeningFrame.PacketHandle,
                compassProjection.PacketHandle,
                holdingInspectionReceipt.ReceiptHandle),
            FormationReceiptHandle: formationReceipt.ReceiptHandle,
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            CompassPacketHandle: compassProjection.PacketHandle,
            HoldingReceiptHandle: holdingInspectionReceipt.ReceiptHandle,
            Disposition: disposition,
            CarryDisposition: carryDisposition,
            CollapseDisposition: collapseDisposition,
            DescendantCandidates: descendantCandidates,
            CandidateOnly: true,
            ReasonCode: DetermineReasonCode(disposition),
            LawfulBasis: DetermineLawfulBasis(disposition),
            TimestampUtc: timestampUtc);
    }

    private static GovernedSeedFormOrCleaveDispositionKind DetermineDisposition(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspectionReceipt,
        IReadOnlyList<GovernedSeedDescendantCandidate> descendantCandidates)
    {
        if (formationReceipt.Refused ||
            compassProjection.TransitionRecommendation == CompassTransitionRecommendation.Refuse ||
            compassProjection.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld ||
            listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken)
        {
            return GovernedSeedFormOrCleaveDispositionKind.Reject;
        }

        if (descendantCandidates.Count > 1)
        {
            return GovernedSeedFormOrCleaveDispositionKind.Cleave;
        }

        if (formationReceipt.PromotionToKnownLawful &&
            formationReceipt.LawfulKnownItems.Count == 1 &&
            compassProjection.TransitionRecommendation == CompassTransitionRecommendation.ProceedBounded &&
            compassProjection.AdmissibilityEstimate is CompassAdmissibilityEstimate.Reviewable or CompassAdmissibilityEstimate.ProvisionallyAdmissible)
        {
            return GovernedSeedFormOrCleaveDispositionKind.Form;
        }

        if (holdingInspectionReceipt.HoldingEntries.Count > 0 ||
            formationReceipt.Deferred ||
            listeningFrame.ReviewPosture != ListeningFrameReviewPosture.CandidateOnly ||
            compassProjection.TransitionRecommendation is CompassTransitionRecommendation.Hold or CompassTransitionRecommendation.ReviewRequired or CompassTransitionRecommendation.RepairRecommended)
        {
            return GovernedSeedFormOrCleaveDispositionKind.Hold;
        }

        return formationReceipt.LawfulKnownItems.Count == 1
            ? GovernedSeedFormOrCleaveDispositionKind.Form
            : GovernedSeedFormOrCleaveDispositionKind.Hold;
    }

    private static IReadOnlyList<GovernedSeedDescendantCandidate> CreateDescendantCandidates(
        FormationReceipt formationReceipt,
        CompassProjectionPacket compassProjection)
    {
        var sourceCandidates = formationReceipt.LawfulKnownItems.Count > 0
            ? formationReceipt.LawfulKnownItems
            : compassProjection.CandidateInputs.Select(static input => input.InputHandle).ToArray();

        return sourceCandidates
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static item => item, StringComparer.Ordinal)
            .Select(item => new GovernedSeedDescendantCandidate(
                CandidateHandle: CreateHandle(
                    "governed-seed-descendant-candidate://",
                    formationReceipt.ReceiptHandle,
                    item),
                CandidateKind: "pre-domain-candidate",
                CandidateSurface: item,
                EvidenceHandles: [item],
                CandidateOnly: true))
            .ToArray();
    }

    private static string DetermineReasonCode(GovernedSeedFormOrCleaveDispositionKind disposition) =>
        disposition switch
        {
            GovernedSeedFormOrCleaveDispositionKind.Form => "governed-seed-form-or-cleave-formed",
            GovernedSeedFormOrCleaveDispositionKind.Cleave => "governed-seed-form-or-cleave-cleaved",
            GovernedSeedFormOrCleaveDispositionKind.Reject => "governed-seed-form-or-cleave-rejected",
            _ => "governed-seed-form-or-cleave-held"
        };

    private static string DetermineLawfulBasis(GovernedSeedFormOrCleaveDispositionKind disposition) =>
        disposition switch
        {
            GovernedSeedFormOrCleaveDispositionKind.Form =>
                "a single coherent candidate may continue only as candidate-only carry after listening-frame posture, compass orientation, and holding inspection survive the checkpoint.",
            GovernedSeedFormOrCleaveDispositionKind.Cleave =>
                "when apparent unity cannot remain one lawful candidate, the checkpoint may emit explicit descendant candidates without promoting any descendant into authority.",
            GovernedSeedFormOrCleaveDispositionKind.Reject =>
                "when orientation, admissibility, or integrity fail, the checkpoint must refuse continuation instead of silently promoting unresolved matter into live authority.",
            _ =>
                "when unfinished thought remains active, the checkpoint may hold the candidate inside bounded cryptic continuity without granting permission or action."
        };

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
