using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SLI.Engine;

public interface IGovernedSeedCrypticHoldingService
{
    GovernedSeedCrypticHoldingInspectionReceipt Inspect(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection);
}

public sealed class GovernedSeedCrypticHoldingService : IGovernedSeedCrypticHoldingService
{
    public GovernedSeedCrypticHoldingInspectionReceipt Inspect(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection)
    {
        ArgumentNullException.ThrowIfNull(formationReceipt);
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(compassProjection);

        var timestampUtc = DateTimeOffset.UtcNow;
        var entries = new List<GovernedSeedCrypticHoldingEntry>();

        foreach (var item in formationReceipt.RetainedUnknownItems)
        {
            entries.Add(CreateEntry(
                formationReceipt,
                "retained-unknown",
                item,
                [],
                compassProjection.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld
                    ? GovernedSeedPsyPolarityKind.Neutral
                    : GovernedSeedPsyPolarityKind.Positive,
                "retained-unknown-candidate-awaits-checkpoint",
                timestampUtc));
        }

        foreach (var requirementState in formationReceipt.RequirementStates
                     .Where(static state => state.State is RequirementStateKind.Unknown or RequirementStateKind.Missing or RequirementStateKind.Blocked))
        {
            entries.Add(CreateEntry(
                formationReceipt,
                requirementState.RequirementKind,
                requirementState.RequirementKind,
                requirementState.EvidenceHandles,
                requirementState.State switch
                {
                    RequirementStateKind.Blocked => GovernedSeedPsyPolarityKind.Negative,
                    RequirementStateKind.Missing => GovernedSeedPsyPolarityKind.Negative,
                    _ => GovernedSeedPsyPolarityKind.Neutral
                },
                $"requirement-{requirementState.State.ToString().ToLowerInvariant()}-awaits-discernment",
                timestampUtc));
        }

        foreach (var failureSignature in formationReceipt.RetainedFailureSignatures)
        {
            entries.Add(CreateEntry(
                formationReceipt,
                "failure-signature",
                failureSignature.SignatureKind.ToString(),
                failureSignature.EvidenceHandles,
                GovernedSeedPsyPolarityKind.Negative,
                "failure-evidence-retained-for-inspection-only",
                timestampUtc));
        }

        var normalizedEntries = entries
            .DistinctBy(static entry => entry.EntryHandle, StringComparer.Ordinal)
            .OrderBy(static entry => entry.EntryHandle, StringComparer.Ordinal)
            .ToArray();

        return new GovernedSeedCrypticHoldingInspectionReceipt(
            ReceiptHandle: CreateHandle(
                "governed-seed-cryptic-holding://",
                formationReceipt.ReceiptHandle,
                listeningFrame.PacketHandle,
                compassProjection.PacketHandle),
            FormationReceiptHandle: formationReceipt.ReceiptHandle,
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            CompassPacketHandle: compassProjection.PacketHandle,
            HoldingEntries: normalizedEntries,
            CandidateOnly: true,
            InspectionInfluenceOnly: true,
            PromotionAuthorityWithheld: true,
            ReasonCode: normalizedEntries.Length == 0
                ? "governed-seed-cryptic-holding-empty"
                : "governed-seed-cryptic-holding-retained",
            LawfulBasis: "pre-admissible constructs may be retained for cryptic inspection influence only and may not promote themselves into permission, standing, or action.",
            TimestampUtc: timestampUtc);
    }

    private static GovernedSeedCrypticHoldingEntry CreateEntry(
        FormationReceipt formationReceipt,
        string constructKind,
        string summary,
        IReadOnlyList<string> evidenceHandles,
        GovernedSeedPsyPolarityKind polarity,
        string holdingReason,
        DateTimeOffset timestampUtc)
    {
        var constructHandle = CreateHandle(
            "pre-admissible-construct://",
            formationReceipt.ReceiptHandle,
            constructKind,
            summary);

        return new GovernedSeedCrypticHoldingEntry(
            EntryHandle: CreateHandle(
                "cryptic-holding-entry://",
                formationReceipt.ReceiptHandle,
                constructKind,
                summary),
            PsyPolarity: polarity,
            Construct: new GovernedSeedPreAdmissibleConstruct(
                ConstructHandle: constructHandle,
                ConstructKind: constructKind,
                Summary: summary,
                EvidenceHandles: NormalizeEvidence(evidenceHandles),
                CandidateOnly: true),
            HoldingReason: holdingReason,
            InspectionInfluenceOnly: true,
            TimestampUtc: timestampUtc);
    }

    private static IReadOnlyList<string> NormalizeEvidence(IReadOnlyList<string>? evidenceHandles)
    {
        return (evidenceHandles ?? Array.Empty<string>())
            .Where(static handle => !string.IsNullOrWhiteSpace(handle))
            .Select(static handle => handle.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static handle => handle, StringComparer.Ordinal)
            .ToArray();
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
