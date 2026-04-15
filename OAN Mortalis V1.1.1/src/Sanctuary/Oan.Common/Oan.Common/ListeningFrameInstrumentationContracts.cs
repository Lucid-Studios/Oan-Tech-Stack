namespace Oan.Common;

public enum ListeningFrameInstrumentationBand
{
    Stable = 0,
    Reviewable = 1,
    RepairBearing = 2,
    Withheld = 3
}

public enum ListeningFrameInstrumentationDisposition
{
    Observe = 0,
    Review = 1,
    Repair = 2,
    Withhold = 3
}

public sealed record ListeningFrameInstrumentationReceipt(
    string ReceiptHandle,
    string? ListeningFrameHandle,
    string? ChamberHandle,
    string? SourceSurfaceHandle,
    string? CompassEmbodimentHandle,
    ListeningFrameVisibilityPosture VisibilityPosture,
    ListeningFrameIntegrityState IntegrityState,
    ListeningFrameReviewPosture ReviewPosture,
    CompassDriftState DriftState,
    CompassOrientationPosture OrientationPosture,
    CompassAdmissibilityEstimate AdmissibilityEstimate,
    CompassTransitionRecommendation TransitionRecommendation,
    CompassAuthorityPosture AuthorityPosture,
    ListeningFrameInstrumentationBand InstrumentationBand,
    ListeningFrameInstrumentationDisposition Disposition,
    bool UsableForCompassProjection,
    bool CandidateOnly,
    bool PersistenceAuthorityWithheld,
    bool ContinuityAdmissionWithheld,
    IReadOnlyList<string> ObservedMarkers,
    IReadOnlyList<string> CandidateInputHandles,
    IReadOnlyList<string> CandidateInputKinds,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class ListeningFrameInstrumentationEvaluator
{
    public static ListeningFrameInstrumentationReceipt Evaluate(
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compass,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(compass);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var handlesConsistent = AreListeningFrameHandlesConsistent(listeningFrame, compass);
        var instrumentationBand = DetermineInstrumentationBand(listeningFrame, compass, handlesConsistent);
        var disposition = DetermineDisposition(instrumentationBand);
        var observedMarkers = NormalizeTokens(
            listeningFrame.PostureMarkers
                .Concat(listeningFrame.ReviewNotes)
                .Concat(compass.ReviewNotes)
                .Concat(compass.CandidateInputs.Select(static input => input.SourceReason)));
        var candidateInputHandles = NormalizeTokens(compass.CandidateInputs.Select(static input => input.InputHandle));
        var candidateInputKinds = NormalizeTokens(compass.CandidateInputs.Select(static input => input.InputKind));

        return new ListeningFrameInstrumentationReceipt(
            ReceiptHandle: receiptHandle,
            ListeningFrameHandle: FirstNonBlank(listeningFrame.ListeningFrameHandle, compass.ListeningFrameHandle),
            ChamberHandle: listeningFrame.ChamberHandle,
            SourceSurfaceHandle: listeningFrame.SourceSurfaceHandle,
            CompassEmbodimentHandle: compass.CompassEmbodimentHandle,
            VisibilityPosture: listeningFrame.VisibilityPosture,
            IntegrityState: listeningFrame.IntegrityState,
            ReviewPosture: listeningFrame.ReviewPosture,
            DriftState: compass.DriftState,
            OrientationPosture: compass.OrientationPosture,
            AdmissibilityEstimate: compass.AdmissibilityEstimate,
            TransitionRecommendation: compass.TransitionRecommendation,
            AuthorityPosture: compass.AuthorityPosture,
            InstrumentationBand: instrumentationBand,
            Disposition: disposition,
            UsableForCompassProjection: listeningFrame.UsableForCompassProjection,
            CandidateOnly: compass.AuthorityPosture == CompassAuthorityPosture.CandidateOnly,
            PersistenceAuthorityWithheld: true,
            ContinuityAdmissionWithheld: true,
            ObservedMarkers: observedMarkers,
            CandidateInputHandles: candidateInputHandles,
            CandidateInputKinds: candidateInputKinds,
            ConstraintCodes: DetermineConstraintCodes(
                listeningFrame,
                compass,
                instrumentationBand,
                handlesConsistent,
                candidateInputHandles.Count),
            ReasonCode: DetermineReasonCode(listeningFrame, compass, instrumentationBand, handlesConsistent),
            LawfulBasis: DetermineLawfulBasis(instrumentationBand, disposition),
            TimestampUtc: compass.TimestampUtc >= listeningFrame.TimestampUtc
                ? compass.TimestampUtc
                : listeningFrame.TimestampUtc);
    }

    public static ListeningFrameInstrumentationBand DetermineInstrumentationBand(
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compass,
        bool handlesConsistent)
    {
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(compass);

        if (!handlesConsistent ||
            !listeningFrame.UsableForCompassProjection ||
            listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken ||
            compass.DriftState == CompassDriftState.Lost ||
            compass.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld ||
            compass.TransitionRecommendation == CompassTransitionRecommendation.Refuse)
        {
            return ListeningFrameInstrumentationBand.Withheld;
        }

        if (compass.DriftState == CompassDriftState.Weakened ||
            compass.TransitionRecommendation == CompassTransitionRecommendation.RepairRecommended)
        {
            return ListeningFrameInstrumentationBand.RepairBearing;
        }

        if (listeningFrame.ReviewPosture != ListeningFrameReviewPosture.CandidateOnly ||
            compass.OrientationPosture != CompassOrientationPosture.Centered ||
            compass.AdmissibilityEstimate == CompassAdmissibilityEstimate.Reviewable ||
            compass.TransitionRecommendation == CompassTransitionRecommendation.ReviewRequired)
        {
            return ListeningFrameInstrumentationBand.Reviewable;
        }

        return ListeningFrameInstrumentationBand.Stable;
    }

    public static ListeningFrameInstrumentationDisposition DetermineDisposition(
        ListeningFrameInstrumentationBand instrumentationBand)
    {
        return instrumentationBand switch
        {
            ListeningFrameInstrumentationBand.Reviewable => ListeningFrameInstrumentationDisposition.Review,
            ListeningFrameInstrumentationBand.RepairBearing => ListeningFrameInstrumentationDisposition.Repair,
            ListeningFrameInstrumentationBand.Withheld => ListeningFrameInstrumentationDisposition.Withhold,
            _ => ListeningFrameInstrumentationDisposition.Observe
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compass,
        ListeningFrameInstrumentationBand instrumentationBand,
        bool handlesConsistent,
        int candidateInputCount)
    {
        var constraints = new List<string>
        {
            "listening-frame-instrumentation-candidate-only",
            "listening-frame-instrumentation-persistence-authority-withheld",
            "listening-frame-instrumentation-continuity-admission-withheld"
        };

        constraints.Add(instrumentationBand switch
        {
            ListeningFrameInstrumentationBand.Reviewable => "listening-frame-instrumentation-reviewable",
            ListeningFrameInstrumentationBand.RepairBearing => "listening-frame-instrumentation-repair-bearing",
            ListeningFrameInstrumentationBand.Withheld => "listening-frame-instrumentation-withheld",
            _ => "listening-frame-instrumentation-stable"
        });

        if (handlesConsistent)
        {
            constraints.Add("listening-frame-instrumentation-listening-frame-handles-consistent");
        }
        else
        {
            constraints.Add("listening-frame-instrumentation-listening-frame-handle-mismatch");
        }

        if (!listeningFrame.UsableForCompassProjection)
        {
            constraints.Add("listening-frame-instrumentation-compass-projection-not-usable");
        }

        if (candidateInputCount > 0)
        {
            constraints.Add("listening-frame-instrumentation-candidate-inputs-visible");
        }

        if (compass.DriftState == CompassDriftState.Weakened)
        {
            constraints.Add("listening-frame-instrumentation-drift-weakened");
        }

        if (compass.DriftState == CompassDriftState.Lost)
        {
            constraints.Add("listening-frame-instrumentation-drift-lost");
        }

        if (compass.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld)
        {
            constraints.Add("listening-frame-instrumentation-admissibility-withheld");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compass,
        ListeningFrameInstrumentationBand instrumentationBand,
        bool handlesConsistent)
    {
        if (!handlesConsistent)
        {
            return "listening-frame-instrumentation-listening-frame-handle-mismatch";
        }

        if (!listeningFrame.UsableForCompassProjection)
        {
            return "listening-frame-instrumentation-compass-projection-withheld";
        }

        if (instrumentationBand == ListeningFrameInstrumentationBand.Withheld)
        {
            return compass.AdmissibilityEstimate == CompassAdmissibilityEstimate.Withheld
                ? "listening-frame-instrumentation-admissibility-withheld"
                : "listening-frame-instrumentation-withheld";
        }

        return instrumentationBand switch
        {
            ListeningFrameInstrumentationBand.RepairBearing => "listening-frame-instrumentation-repair-bearing",
            ListeningFrameInstrumentationBand.Reviewable => "listening-frame-instrumentation-reviewable",
            _ => "listening-frame-instrumentation-stable"
        };
    }

    private static string DetermineLawfulBasis(
        ListeningFrameInstrumentationBand instrumentationBand,
        ListeningFrameInstrumentationDisposition disposition)
    {
        return $"{instrumentationBand.ToString().ToLowerInvariant()} listening-frame instrumentation remains {disposition.ToString().ToLowerInvariant()}-only and does not grant persistence or continuity authority.";
    }

    private static bool AreListeningFrameHandlesConsistent(
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compass)
    {
        if (string.IsNullOrWhiteSpace(listeningFrame.ListeningFrameHandle) ||
            string.IsNullOrWhiteSpace(compass.ListeningFrameHandle))
        {
            return true;
        }

        return string.Equals(
            listeningFrame.ListeningFrameHandle,
            compass.ListeningFrameHandle,
            StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> NormalizeTokens(IEnumerable<string?> tokens)
    {
        return tokens
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? FirstNonBlank(string? first, string? second)
    {
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        return string.IsNullOrWhiteSpace(second) ? null : second;
    }
}
