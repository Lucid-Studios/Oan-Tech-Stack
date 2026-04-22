namespace San.Common;

using San.Common;

public enum ThetaIngressStatusKind
{
    Lawful = 0,
    Deferred = 1,
    Blocked = 2,
    Malformed = 3
}

public sealed record ThetaIngressSensoryClusterReceipt(
    string ReceiptHandle,
    string? ThetaHandle,
    string? ListeningFrameHandle,
    string? SoulFrameHandle,
    string? ZedOfDeltaHandle,
    string? OeHandle,
    string? SelfGelHandle,
    string? COeHandle,
    string? CSelfGelHandle,
    string? EngineeredCognitionHandle,
    ThetaIngressStatusKind IngressStatus,
    bool PresentedInListeningFrame,
    bool CrossedRelativeToZed,
    bool TakenUpAtCOe,
    bool EnteredCSelfGel,
    bool ContextualizationBegun,
    bool CandidateOnly,
    bool PersistenceAuthorityWithheld,
    bool SelfMutationWithheld,
    bool InheritanceWithheld,
    bool CondensationWithheld,
    bool PulseAuthorityWithheld,
    IReadOnlyList<string> ThetaMarkers,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class ThetaIngressSensoryClusterEvaluator
{
    public static ThetaIngressSensoryClusterReceipt Evaluate(
        ListeningFrameProjectionPacket listeningFrame,
        ZedDeltaSelfBasisReceipt selfBasis,
        string? thetaHandle,
        IReadOnlyList<string>? thetaMarkers,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(selfBasis);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var presentedInListeningFrame = HasToken(thetaHandle) &&
                                        HasToken(listeningFrame.ListeningFrameHandle);
        var crossedRelativeToZed = presentedInListeningFrame &&
                                   HasToken(selfBasis.ZedOfDeltaHandle);
        var takenUpAtCOe = crossedRelativeToZed &&
                           HasToken(selfBasis.COeHandle);
        var enteredCSelfGel = takenUpAtCOe &&
                              HasToken(selfBasis.CSelfGelHandle);
        var contextualizationBegun = enteredCSelfGel &&
                                     selfBasis.WiredThroughSelfGel;
        var ingressStatus = DetermineIngressStatus(
            listeningFrame,
            selfBasis,
            thetaHandle,
            contextualizationBegun);

        return new ThetaIngressSensoryClusterReceipt(
            ReceiptHandle: receiptHandle,
            ThetaHandle: thetaHandle,
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            SoulFrameHandle: selfBasis.SoulFrameHandle,
            ZedOfDeltaHandle: selfBasis.ZedOfDeltaHandle,
            OeHandle: selfBasis.OeHandle,
            SelfGelHandle: selfBasis.SelfGelHandle,
            COeHandle: selfBasis.COeHandle,
            CSelfGelHandle: selfBasis.CSelfGelHandle,
            EngineeredCognitionHandle: selfBasis.EngineeredCognitionHandle,
            IngressStatus: ingressStatus,
            PresentedInListeningFrame: presentedInListeningFrame,
            CrossedRelativeToZed: crossedRelativeToZed,
            TakenUpAtCOe: takenUpAtCOe,
            EnteredCSelfGel: enteredCSelfGel,
            ContextualizationBegun: contextualizationBegun,
            CandidateOnly: true,
            PersistenceAuthorityWithheld: true,
            SelfMutationWithheld: true,
            InheritanceWithheld: true,
            CondensationWithheld: true,
            PulseAuthorityWithheld: true,
            ThetaMarkers: DetermineThetaMarkers(thetaMarkers, ingressStatus),
            ConstraintCodes: DetermineConstraintCodes(
                listeningFrame,
                selfBasis,
                ingressStatus,
                presentedInListeningFrame,
                crossedRelativeToZed,
                takenUpAtCOe,
                enteredCSelfGel,
                contextualizationBegun),
            ReasonCode: DetermineReasonCode(
                listeningFrame,
                selfBasis,
                thetaHandle,
                ingressStatus),
            LawfulBasis: DetermineLawfulBasis(ingressStatus),
            TimestampUtc: MaxTimestamp(listeningFrame.TimestampUtc, selfBasis.TimestampUtc));
    }

    public static ThetaIngressStatusKind DetermineIngressStatus(
        ListeningFrameProjectionPacket listeningFrame,
        ZedDeltaSelfBasisReceipt selfBasis,
        string? thetaHandle,
        bool contextualizationBegun)
    {
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(selfBasis);

        if (!HasToken(thetaHandle))
        {
            return ThetaIngressStatusKind.Malformed;
        }

        if (!listeningFrame.UsableForCompassProjection ||
            listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken ||
            selfBasis.BasisBand == ZedDeltaSelfBasisBand.Withheld ||
            !selfBasis.AnchoredByOe ||
            !selfBasis.StoredInSoulFrame ||
            !selfBasis.CastIntoListeningFrame ||
            !selfBasis.WiredThroughSelfGel ||
            !contextualizationBegun)
        {
            return ThetaIngressStatusKind.Blocked;
        }

        if (listeningFrame.ReviewPosture != ListeningFrameReviewPosture.CandidateOnly ||
            selfBasis.BasisBand == ZedDeltaSelfBasisBand.Reviewable)
        {
            return ThetaIngressStatusKind.Deferred;
        }

        return ThetaIngressStatusKind.Lawful;
    }

    private static IReadOnlyList<string> DetermineThetaMarkers(
        IReadOnlyList<string>? thetaMarkers,
        ThetaIngressStatusKind ingressStatus)
    {
        return NormalizeTokens((thetaMarkers ?? Array.Empty<string>())
            .Concat(
            [
                "theta-ingress-listening-frame-presented",
                "theta-ingress-coe-needle-uptake",
                "theta-ingress-cselfgel-contextualization"
            ])
            .Concat(
                ingressStatus == ThetaIngressStatusKind.Lawful
                    ? ["theta-ingress-lawful"]
                    : Array.Empty<string>()));
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        ListeningFrameProjectionPacket listeningFrame,
        ZedDeltaSelfBasisReceipt selfBasis,
        ThetaIngressStatusKind ingressStatus,
        bool presentedInListeningFrame,
        bool crossedRelativeToZed,
        bool takenUpAtCOe,
        bool enteredCSelfGel,
        bool contextualizationBegun)
    {
        var constraints = new List<string>
        {
            "theta-ingress-candidate-only",
            "theta-ingress-persistence-authority-withheld",
            "theta-ingress-self-mutation-withheld",
            "theta-ingress-inheritance-withheld",
            "theta-ingress-condensation-withheld",
            "theta-ingress-pulse-authority-withheld"
        };

        constraints.Add(ingressStatus switch
        {
            ThetaIngressStatusKind.Deferred => "theta-ingress-deferred",
            ThetaIngressStatusKind.Blocked => "theta-ingress-blocked",
            ThetaIngressStatusKind.Malformed => "theta-ingress-malformed",
            _ => "theta-ingress-lawful"
        });

        constraints.Add(presentedInListeningFrame
            ? "theta-ingress-listening-frame-presented"
            : "theta-ingress-listening-frame-presentation-missing");

        constraints.Add(crossedRelativeToZed
            ? "theta-ingress-crossed-relative-to-zed"
            : "theta-ingress-zed-crossing-missing");

        constraints.Add(takenUpAtCOe
            ? "theta-ingress-coe-needle-uptake-preserved"
            : "theta-ingress-coe-needle-uptake-missing");

        constraints.Add(enteredCSelfGel
            ? "theta-ingress-cselfgel-entry-preserved"
            : "theta-ingress-cselfgel-entry-missing");

        constraints.Add(contextualizationBegun
            ? "theta-ingress-contextualization-begun"
            : "theta-ingress-contextualization-withheld");

        if (!listeningFrame.UsableForCompassProjection)
        {
            constraints.Add("theta-ingress-listening-frame-not-usable");
        }

        if (listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken)
        {
            constraints.Add("theta-ingress-listening-frame-broken");
        }

        if (selfBasis.BasisBand == ZedDeltaSelfBasisBand.Reviewable)
        {
            constraints.Add("theta-ingress-self-basis-reviewable");
        }

        if (selfBasis.BasisBand == ZedDeltaSelfBasisBand.Withheld)
        {
            constraints.Add("theta-ingress-self-basis-withheld");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        ListeningFrameProjectionPacket listeningFrame,
        ZedDeltaSelfBasisReceipt selfBasis,
        string? thetaHandle,
        ThetaIngressStatusKind ingressStatus)
    {
        if (!HasToken(thetaHandle))
        {
            return "theta-ingress-theta-handle-missing";
        }

        if (!listeningFrame.UsableForCompassProjection)
        {
            return "theta-ingress-listening-frame-not-usable";
        }

        if (listeningFrame.IntegrityState == ListeningFrameIntegrityState.Broken)
        {
            return "theta-ingress-listening-frame-broken";
        }

        if (selfBasis.BasisBand == ZedDeltaSelfBasisBand.Withheld)
        {
            return "theta-ingress-self-basis-withheld";
        }

        if (ingressStatus == ThetaIngressStatusKind.Deferred)
        {
            return "theta-ingress-deferred";
        }

        if (ingressStatus == ThetaIngressStatusKind.Blocked)
        {
            return "theta-ingress-contextualization-withheld";
        }

        return "theta-ingress-lawful";
    }

    private static string DetermineLawfulBasis(
        ThetaIngressStatusKind ingressStatus)
    {
        var posture = ingressStatus.ToString().ToLowerInvariant();
        return $"{posture} theta ingress witnesses traversal from ListeningFrame, through cOE, into cSelfGEL for contextualization without implying persistence, self-mutation, inheritance, condensation, or pulse.";
    }

    private static bool HasToken(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static DateTimeOffset MaxTimestamp(
        DateTimeOffset first,
        DateTimeOffset second) =>
        first >= second ? first : second;

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
