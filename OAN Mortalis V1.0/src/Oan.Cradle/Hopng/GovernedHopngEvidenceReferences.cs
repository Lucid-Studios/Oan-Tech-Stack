using Oan.Common;

namespace Oan.Cradle;

internal sealed record GovernedHopngEvidenceReference(
    string RefId,
    string PointerUri,
    string Summary);

internal static class GovernedHopngEvidenceReferences
{
    public static IReadOnlyList<GovernedHopngEvidenceReference> Build(
        GovernedHopngEmissionRequest request,
        GovernanceLoopStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(snapshot);

        var refs = new List<GovernedHopngEvidenceReference>();
        var seenPointers = new HashSet<string>(StringComparer.Ordinal);

        AddReference(
            refs,
            seenPointers,
            "decision",
            request.DecisionReceipt.MutationReceipt.ReceiptHandle,
            "governance-decision-receipt");

        if (snapshot.ReviewRequest?.RequestEnvelope is not null)
        {
            AddReference(
                refs,
                seenPointers,
                "review-envelope",
                snapshot.ReviewRequest.RequestEnvelope.EnvelopeId,
                "steward-review-envelope");
            AddReference(
                refs,
                seenPointers,
                "actionable-content",
                snapshot.ReviewRequest.RequestEnvelope.ActionableContent.ContentHandle,
                "actionable-return-candidate");
        }

        if (!string.IsNullOrWhiteSpace(snapshot.ReviewRequest?.ReturnCandidatePointer))
        {
            AddReference(
                refs,
                seenPointers,
                "return-pointer",
                snapshot.ReviewRequest.ReturnCandidatePointer,
                "return-candidate-pointer");
        }

        for (var index = 0; index < snapshot.TargetWitnessReceipts.Count; index++)
        {
            var receipt = snapshot.TargetWitnessReceipts[index];
            var ordinal = index + 1;
            AddReference(
                refs,
                seenPointers,
                $"target-witness-{ordinal}",
                receipt.WitnessHandle,
                $"target-witness:{GovernedTargetWitnessKeys.GetKindSlug(receipt.Kind)}");

            if (!string.IsNullOrWhiteSpace(receipt.LineageHandle))
            {
                AddReference(
                    refs,
                    seenPointers,
                    $"target-lineage-{ordinal}",
                    receipt.LineageHandle,
                    "target-lineage-handle");
            }

            if (!string.IsNullOrWhiteSpace(receipt.TraceHandle))
            {
                AddReference(
                    refs,
                    seenPointers,
                    $"target-trace-{ordinal}",
                    receipt.TraceHandle,
                    "target-trace-handle");
            }

            if (!string.IsNullOrWhiteSpace(receipt.ResidueHandle))
            {
                AddReference(
                    refs,
                    seenPointers,
                    $"target-residue-{ordinal}",
                    receipt.ResidueHandle,
                    "target-residue-handle");
            }
        }

        var compassObservationReceipts = snapshot.CompassObservationReceipts ?? [];
        for (var index = 0; index < compassObservationReceipts.Count; index++)
        {
            var receipt = compassObservationReceipts[index];
            AddReference(
                refs,
                seenPointers,
                $"compass-observation-{index + 1}",
                receipt.WitnessHandle,
                "compass-observation");
        }

        var compassDriftReceipts = snapshot.CompassDriftReceipts ?? [];
        for (var index = 0; index < compassDriftReceipts.Count; index++)
        {
            var receipt = compassDriftReceipts[index];
            AddReference(
                refs,
                seenPointers,
                $"compass-drift-{index + 1}",
                receipt.DriftHandle,
                "compass-drift");
        }

        var innerWeatherReceipts = snapshot.InnerWeatherReceipts ?? [];
        for (var index = 0; index < innerWeatherReceipts.Count; index++)
        {
            var receipt = innerWeatherReceipts[index];
            AddReference(
                refs,
                seenPointers,
                $"inner-weather-{index + 1}",
                receipt.InnerWeatherHandle,
                "inner-weather");
        }

        foreach (var receipt in snapshot.HopngArtifacts)
        {
            AddReference(
                refs,
                seenPointers,
                $"artifact-{receipt.Profile}",
                receipt.ArtifactHandle,
                "prior-hopng-artifact");
        }

        return refs;
    }

    private static void AddReference(
        ICollection<GovernedHopngEvidenceReference> refs,
        ISet<string> seenPointers,
        string refId,
        string pointerUri,
        string summary)
    {
        if (string.IsNullOrWhiteSpace(pointerUri) || !seenPointers.Add(pointerUri))
        {
            return;
        }

        refs.Add(new GovernedHopngEvidenceReference(refId, pointerUri, summary));
    }
}
