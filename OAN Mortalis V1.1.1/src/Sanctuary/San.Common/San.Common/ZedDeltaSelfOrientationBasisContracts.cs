namespace San.Common;

using San.Common;

public enum ZedBasisDirectionKind
{
    Center = 0,
    Ahead = 1,
    Behind = 2,
    Above = 3,
    Below = 4,
    Left = 5,
    Right = 6
}

public enum ZedDeltaSelfBasisBand
{
    Stable = 0,
    Reviewable = 1,
    Withheld = 2
}

public enum ZedDeltaSelfBasisDisposition
{
    Orient = 0,
    Review = 1,
    Withhold = 2
}

public sealed record ZedDeltaSelfBasisReceipt(
    string ReceiptHandle,
    string? ListeningFrameHandle,
    string? SoulFrameHandle,
    string? OeHandle,
    string? SelfGelHandle,
    string? COeHandle,
    string? CSelfGelHandle,
    string? ZedOfDeltaHandle,
    string? EngineeredCognitionHandle,
    string? EcIuttLispMatrixHandle,
    ZedDeltaSelfBasisBand BasisBand,
    ZedDeltaSelfBasisDisposition Disposition,
    bool AnchoredByOe,
    bool StoredInSoulFrame,
    bool CastIntoListeningFrame,
    bool WiredThroughSelfGel,
    bool CandidateOnly,
    bool PersistenceAuthorityWithheld,
    bool ContinuityAdmissionWithheld,
    IReadOnlyList<ZedBasisDirectionKind> CardinalDirections,
    IReadOnlyList<string> OrientationMarkers,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class ZedDeltaSelfBasisEvaluator
{
    public static ZedDeltaSelfBasisReceipt Evaluate(
        ListeningFrameProjectionPacket listeningFrame,
        string? soulFrameHandle,
        string? oeHandle,
        string? selfGelHandle,
        string? cOeHandle,
        string? cSelfGelHandle,
        string? zedOfDeltaHandle,
        string? engineeredCognitionHandle,
        string? ecIuttLispMatrixHandle,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(listeningFrame);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var anchoredByOe = HasToken(oeHandle);
        var storedInSoulFrame = HasToken(soulFrameHandle) && anchoredByOe && HasToken(selfGelHandle);
        var castIntoListeningFrame = HasToken(listeningFrame.ListeningFrameHandle) &&
                                     HasToken(cOeHandle) &&
                                     HasToken(cSelfGelHandle);
        var wiredThroughSelfGel = HasToken(selfGelHandle) && HasToken(cSelfGelHandle);
        var basisBand = DetermineBasisBand(
            listeningFrame,
            anchoredByOe,
            storedInSoulFrame,
            castIntoListeningFrame,
            zedOfDeltaHandle,
            ecIuttLispMatrixHandle);
        var disposition = DetermineDisposition(basisBand);

        return new ZedDeltaSelfBasisReceipt(
            ReceiptHandle: receiptHandle,
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            SoulFrameHandle: soulFrameHandle,
            OeHandle: oeHandle,
            SelfGelHandle: selfGelHandle,
            COeHandle: cOeHandle,
            CSelfGelHandle: cSelfGelHandle,
            ZedOfDeltaHandle: zedOfDeltaHandle,
            EngineeredCognitionHandle: engineeredCognitionHandle,
            EcIuttLispMatrixHandle: ecIuttLispMatrixHandle,
            BasisBand: basisBand,
            Disposition: disposition,
            AnchoredByOe: anchoredByOe,
            StoredInSoulFrame: storedInSoulFrame,
            CastIntoListeningFrame: castIntoListeningFrame,
            WiredThroughSelfGel: wiredThroughSelfGel,
            CandidateOnly: true,
            PersistenceAuthorityWithheld: true,
            ContinuityAdmissionWithheld: true,
            CardinalDirections: GetCardinalDirections(),
            OrientationMarkers: DetermineOrientationMarkers(listeningFrame, ecIuttLispMatrixHandle),
            ConstraintCodes: DetermineConstraintCodes(
                listeningFrame,
                basisBand,
                anchoredByOe,
                storedInSoulFrame,
                castIntoListeningFrame,
                wiredThroughSelfGel,
                zedOfDeltaHandle,
                ecIuttLispMatrixHandle),
            ReasonCode: DetermineReasonCode(
                listeningFrame,
                basisBand,
                anchoredByOe,
                storedInSoulFrame,
                castIntoListeningFrame,
                zedOfDeltaHandle,
                ecIuttLispMatrixHandle),
            LawfulBasis: DetermineLawfulBasis(basisBand, disposition),
            TimestampUtc: listeningFrame.TimestampUtc);
    }

    public static ZedDeltaSelfBasisBand DetermineBasisBand(
        ListeningFrameProjectionPacket listeningFrame,
        bool anchoredByOe,
        bool storedInSoulFrame,
        bool castIntoListeningFrame,
        string? zedOfDeltaHandle,
        string? ecIuttLispMatrixHandle)
    {
        ArgumentNullException.ThrowIfNull(listeningFrame);

        if (!listeningFrame.UsableForCompassProjection ||
            listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken ||
            !anchoredByOe ||
            !storedInSoulFrame ||
            !castIntoListeningFrame ||
            !HasToken(zedOfDeltaHandle))
        {
            return ZedDeltaSelfBasisBand.Withheld;
        }

        if (listeningFrame.ReviewPosture != ListeningFrameReviewPosture.CandidateOnly ||
            !HasToken(ecIuttLispMatrixHandle))
        {
            return ZedDeltaSelfBasisBand.Reviewable;
        }

        return ZedDeltaSelfBasisBand.Stable;
    }

    public static ZedDeltaSelfBasisDisposition DetermineDisposition(
        ZedDeltaSelfBasisBand basisBand)
    {
        return basisBand switch
        {
            ZedDeltaSelfBasisBand.Reviewable => ZedDeltaSelfBasisDisposition.Review,
            ZedDeltaSelfBasisBand.Withheld => ZedDeltaSelfBasisDisposition.Withhold,
            _ => ZedDeltaSelfBasisDisposition.Orient
        };
    }

    private static IReadOnlyList<ZedBasisDirectionKind> GetCardinalDirections()
    {
        return
        [
            ZedBasisDirectionKind.Center,
            ZedBasisDirectionKind.Ahead,
            ZedBasisDirectionKind.Behind,
            ZedBasisDirectionKind.Above,
            ZedBasisDirectionKind.Below,
            ZedBasisDirectionKind.Left,
            ZedBasisDirectionKind.Right
        ];
    }

    private static IReadOnlyList<string> DetermineOrientationMarkers(
        ListeningFrameProjectionPacket listeningFrame,
        string? ecIuttLispMatrixHandle)
    {
        var markers = listeningFrame.PostureMarkers
            .Concat(listeningFrame.ReviewNotes)
            .Concat(
            [
                "zed-delta-self-basis-center-ahead-behind-above-below-left-right",
                "zed-delta-self-basis-oe-selfgel-stored-in-soulframe",
                "zed-delta-self-basis-coe-cselfgel-cast-into-listening-frame"
            ]);

        if (HasToken(ecIuttLispMatrixHandle))
        {
            markers = markers.Append("zed-delta-self-basis-ec-iutt-lisp-matrix-visible");
        }

        return NormalizeTokens(markers);
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        ListeningFrameProjectionPacket listeningFrame,
        ZedDeltaSelfBasisBand basisBand,
        bool anchoredByOe,
        bool storedInSoulFrame,
        bool castIntoListeningFrame,
        bool wiredThroughSelfGel,
        string? zedOfDeltaHandle,
        string? ecIuttLispMatrixHandle)
    {
        var constraints = new List<string>
        {
            "zed-delta-self-basis-candidate-only",
            "zed-delta-self-basis-persistence-authority-withheld",
            "zed-delta-self-basis-continuity-admission-withheld",
            "zed-delta-self-basis-center-ahead-behind-above-below-left-right"
        };

        constraints.Add(basisBand switch
        {
            ZedDeltaSelfBasisBand.Reviewable => "zed-delta-self-basis-reviewable",
            ZedDeltaSelfBasisBand.Withheld => "zed-delta-self-basis-withheld",
            _ => "zed-delta-self-basis-stable"
        });

        constraints.Add(anchoredByOe
            ? "zed-delta-self-basis-oe-anchor-preserved"
            : "zed-delta-self-basis-oe-anchor-missing");

        constraints.Add(storedInSoulFrame
            ? "zed-delta-self-basis-soulframe-storage-preserved"
            : "zed-delta-self-basis-soulframe-storage-incomplete");

        constraints.Add(castIntoListeningFrame
            ? "zed-delta-self-basis-listening-frame-cast-preserved"
            : "zed-delta-self-basis-listening-frame-cast-incomplete");

        constraints.Add(wiredThroughSelfGel
            ? "zed-delta-self-basis-selfgel-wire-preserved"
            : "zed-delta-self-basis-selfgel-wire-incomplete");

        if (!HasToken(zedOfDeltaHandle))
        {
            constraints.Add("zed-delta-self-basis-zed-of-delta-handle-missing");
        }

        if (!HasToken(ecIuttLispMatrixHandle))
        {
            constraints.Add("zed-delta-self-basis-ec-iutt-lisp-matrix-reviewable");
        }

        if (!listeningFrame.UsableForCompassProjection)
        {
            constraints.Add("zed-delta-self-basis-listening-frame-not-usable");
        }

        if (listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken)
        {
            constraints.Add("zed-delta-self-basis-listening-frame-broken");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        ListeningFrameProjectionPacket listeningFrame,
        ZedDeltaSelfBasisBand basisBand,
        bool anchoredByOe,
        bool storedInSoulFrame,
        bool castIntoListeningFrame,
        string? zedOfDeltaHandle,
        string? ecIuttLispMatrixHandle)
    {
        if (!listeningFrame.UsableForCompassProjection)
        {
            return "zed-delta-self-basis-listening-frame-not-usable";
        }

        if (listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken)
        {
            return "zed-delta-self-basis-listening-frame-broken";
        }

        if (!anchoredByOe)
        {
            return "zed-delta-self-basis-oe-anchor-missing";
        }

        if (!storedInSoulFrame)
        {
            return "zed-delta-self-basis-soulframe-storage-incomplete";
        }

        if (!castIntoListeningFrame)
        {
            return "zed-delta-self-basis-listening-frame-cast-incomplete";
        }

        if (!HasToken(zedOfDeltaHandle))
        {
            return "zed-delta-self-basis-zed-of-delta-handle-missing";
        }

        if (basisBand == ZedDeltaSelfBasisBand.Reviewable &&
            !HasToken(ecIuttLispMatrixHandle))
        {
            return "zed-delta-self-basis-ec-iutt-lisp-matrix-reviewable";
        }

        return basisBand switch
        {
            ZedDeltaSelfBasisBand.Reviewable => "zed-delta-self-basis-reviewable",
            ZedDeltaSelfBasisBand.Withheld => "zed-delta-self-basis-withheld",
            _ => "zed-delta-self-basis-stable"
        };
    }

    private static string DetermineLawfulBasis(
        ZedDeltaSelfBasisBand basisBand,
        ZedDeltaSelfBasisDisposition disposition)
    {
        var posture = basisBand.ToString().ToLowerInvariant();
        var action = disposition.ToString().ToLowerInvariant();
        return $"{posture} zed-of-delta self basis remains {action}-only, with OE/SelfGEL stored in SoulFrame and cOE/cSelfGEL cast into the ListeningFrame for bounded orientation.";
    }

    private static bool HasToken(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static IReadOnlyList<string> NormalizeTokens(IEnumerable<string?> tokens)
    {
        return tokens
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
