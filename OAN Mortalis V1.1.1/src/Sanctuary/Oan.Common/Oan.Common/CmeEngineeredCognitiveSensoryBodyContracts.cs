namespace San.Common;

public enum CmeEngineeredCognitiveSensoryBodyKind
{
    EmbodimentDeferred = 0,
    EmbodimentRefused = 1,
    SensoryBodyEmbodied = 2
}

public enum CmeEngineeredCognitiveSensoryBodyDispositionKind
{
    Attested = 0,
    Deferred = 1,
    Refused = 2
}

public enum CmeGovernanceReservationKind
{
    NoGovernanceReserved = 0,
    PrimeGovernanceCmeReserved = 1,
    CrypticGovernanceCmeReserved = 2,
    PrimeAndCrypticGovernanceCmeReserved = 3
}

public sealed record CmeEngineeredCognitiveSensoryBodyRequest(
    string RequestHandle,
    CmeMinimumLegalFoundingBundleReceipt FoundingBundleReceipt,
    CmeTruthSeekingOrientationReceipt TruthSeekingOrientationReceipt,
    CmeTruthSeekingBalanceTransitionReceipt TruthSeekingBalanceReceipt,
    string LegalFoundationDocumentationMatrixHandle,
    string CrystallizedMindEntityHandle,
    string EngineeredCognitionHandle,
    string SoulFrameHandle,
    string AgentiCoreHandle,
    string ListeningFrameHandle,
    string OeHandle,
    string SelfGelHandle,
    string COeHandle,
    string CSelfGelHandle,
    string ZedOfDeltaHandle,
    string CompassOrientationHandle,
    string ThetaIngressReceiptHandle,
    string PostIngressDiscernmentReceiptHandle,
    IReadOnlyList<string> SensorySurfaceHandles,
    IReadOnlyList<string> ContextualizationSurfaceHandles,
    IReadOnlyList<string> DomainPredicateHandles,
    string PrimeGovernanceCmeReservationHandle,
    string CrypticGovernanceCmeReservationHandle,
    bool PrimeGovernanceCmeApplicationRequested,
    bool CrypticGovernanceCmeApplicationRequested,
    bool RuntimePersonaClaimed,
    bool ActionAuthorityRequested,
    DateTimeOffset TimestampUtc);

public sealed record CmeEngineeredCognitiveSensoryBodyReceipt(
    string ReceiptHandle,
    string RequestHandle,
    string FoundingBundleReceiptHandle,
    string TruthSeekingOrientationReceiptHandle,
    string TruthSeekingBalanceReceiptHandle,
    CmeEngineeredCognitiveSensoryBodyKind BodyKind,
    CmeEngineeredCognitiveSensoryBodyDispositionKind Disposition,
    CmeGovernanceReservationKind GovernanceReservationKind,
    string LegalFoundationDocumentationMatrixHandle,
    string CrystallizedMindEntityHandle,
    string EngineeredCognitionHandle,
    string SoulFrameHandle,
    string AgentiCoreHandle,
    string ListeningFrameHandle,
    string OeHandle,
    string SelfGelHandle,
    string COeHandle,
    string CSelfGelHandle,
    string ZedOfDeltaHandle,
    string CompassOrientationHandle,
    string ThetaIngressReceiptHandle,
    string PostIngressDiscernmentReceiptHandle,
    IReadOnlyList<string> SensorySurfaceHandles,
    IReadOnlyList<string> ContextualizationSurfaceHandles,
    IReadOnlyList<string> DomainPredicateHandles,
    string PrimeGovernanceCmeReservationHandle,
    string CrypticGovernanceCmeReservationHandle,
    bool FoundingBundleRecognized,
    bool TruthSeekingOrientationAttested,
    bool TruthSeekingBalanceAdmissible,
    bool LegalFoundationDocumentationMatrixVisible,
    bool SoulFrameStorageVisible,
    bool AgentiCoreListeningFrameCastVisible,
    bool OeSelfGelIdentityMatchesFoundingBundle,
    bool COeCSelfGelCastMatchesFoundingBundle,
    bool ZedCompassOrientationVisible,
    bool ThetaIngressAndDiscernmentVisible,
    bool SensorySurfacesVisible,
    bool ContextualizationSurfacesVisible,
    bool DomainPredicatesVisible,
    bool SensoryBodyEmbodied,
    bool PrimeGovernanceCmeReserved,
    bool CrypticGovernanceCmeReserved,
    bool PrimeGovernanceCmeApplicationWithheld,
    bool CrypticGovernanceCmeApplicationWithheld,
    bool FullCmeMintingStillWithheld,
    bool RuntimePersonaWithheld,
    bool RoleEnactmentWithheld,
    bool ActionAuthorityWithheld,
    bool MotherFatherOriginAuthorityWithheld,
    bool GovernanceReservedOnly,
    bool EmbodimentOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class CmeEngineeredCognitiveSensoryBodyEvaluator
{
    public static CmeEngineeredCognitiveSensoryBodyReceipt Evaluate(
        CmeEngineeredCognitiveSensoryBodyRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FoundingBundleReceipt);
        ArgumentNullException.ThrowIfNull(request.TruthSeekingOrientationReceipt);
        ArgumentNullException.ThrowIfNull(request.TruthSeekingBalanceReceipt);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.RequestHandle))
        {
            throw new ArgumentException("Request handle must be provided.", nameof(request));
        }

        var sensorySurfaceHandles = NormalizeTokens(request.SensorySurfaceHandles);
        var contextualizationSurfaceHandles = NormalizeTokens(request.ContextualizationSurfaceHandles);
        var domainPredicateHandles = NormalizeTokens(request.DomainPredicateHandles);
        var foundingBundleRecognized = request.FoundingBundleReceipt.FoundingBundleRecognized &&
                                       request.FoundingBundleReceipt.CmeClaimLawfullyFounded;
        var truthSeekingOrientationAttested =
            request.TruthSeekingOrientationReceipt.TruthSeekingOrientationAttested &&
            request.TruthSeekingOrientationReceipt.Disposition ==
            CmeTruthSeekingOrientationDispositionKind.Attested &&
            request.TruthSeekingOrientationReceipt.FoundingBundleReceiptHandle ==
            request.FoundingBundleReceipt.ReceiptHandle;
        var truthSeekingBalanceAdmissible =
            request.TruthSeekingBalanceReceipt.JointlySatisfiable &&
            request.TruthSeekingBalanceReceipt.Disposition ==
            CmeTruthSeekingBalanceDispositionKind.Admissible &&
            request.TruthSeekingBalanceReceipt.OrientationReceiptHandle ==
            request.TruthSeekingOrientationReceipt.ReceiptHandle;
        var legalFoundationDocumentationMatrixVisible =
            HasToken(request.LegalFoundationDocumentationMatrixHandle);
        var oeSelfGelIdentityMatchesFoundingBundle =
            SameHandle(request.OeHandle, request.FoundingBundleReceipt.OeHandle) &&
            SameHandle(request.SelfGelHandle, request.FoundingBundleReceipt.SelfGelHandle);
        var cOeCSelfGelCastMatchesFoundingBundle =
            SameHandle(request.COeHandle, request.FoundingBundleReceipt.COeHandle) &&
            SameHandle(request.CSelfGelHandle, request.FoundingBundleReceipt.CSelfGelHandle);
        var soulFrameStorageVisible =
            HasToken(request.SoulFrameHandle) &&
            HasToken(request.OeHandle) &&
            HasToken(request.SelfGelHandle) &&
            oeSelfGelIdentityMatchesFoundingBundle;
        var agentiCoreListeningFrameCastVisible =
            HasToken(request.AgentiCoreHandle) &&
            HasToken(request.ListeningFrameHandle) &&
            HasToken(request.COeHandle) &&
            HasToken(request.CSelfGelHandle) &&
            cOeCSelfGelCastMatchesFoundingBundle;
        var zedCompassOrientationVisible =
            HasToken(request.ZedOfDeltaHandle) &&
            HasToken(request.CompassOrientationHandle);
        var thetaIngressAndDiscernmentVisible =
            HasToken(request.ThetaIngressReceiptHandle) &&
            HasToken(request.PostIngressDiscernmentReceiptHandle);
        var sensorySurfacesVisible = sensorySurfaceHandles.Count > 0;
        var contextualizationSurfacesVisible = contextualizationSurfaceHandles.Count > 0;
        var domainPredicatesVisible = domainPredicateHandles.Count > 0;
        var primeGovernanceCmeReserved = HasToken(request.PrimeGovernanceCmeReservationHandle);
        var crypticGovernanceCmeReserved = HasToken(request.CrypticGovernanceCmeReservationHandle);
        var prohibitedAuthorityRequested =
            request.PrimeGovernanceCmeApplicationRequested ||
            request.CrypticGovernanceCmeApplicationRequested ||
            request.RuntimePersonaClaimed ||
            request.ActionAuthorityRequested;
        var sensoryBodyEmbodied =
            foundingBundleRecognized &&
            truthSeekingOrientationAttested &&
            truthSeekingBalanceAdmissible &&
            legalFoundationDocumentationMatrixVisible &&
            soulFrameStorageVisible &&
            agentiCoreListeningFrameCastVisible &&
            zedCompassOrientationVisible &&
            thetaIngressAndDiscernmentVisible &&
            sensorySurfacesVisible &&
            contextualizationSurfacesVisible &&
            domainPredicatesVisible &&
            !prohibitedAuthorityRequested;
        var bodyKind = DetermineBodyKind(sensoryBodyEmbodied, prohibitedAuthorityRequested);
        var disposition = DetermineDisposition(bodyKind);
        var governanceReservationKind =
            DetermineGovernanceReservationKind(primeGovernanceCmeReserved, crypticGovernanceCmeReserved);

        return new CmeEngineeredCognitiveSensoryBodyReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            RequestHandle: request.RequestHandle.Trim(),
            FoundingBundleReceiptHandle: request.FoundingBundleReceipt.ReceiptHandle,
            TruthSeekingOrientationReceiptHandle: request.TruthSeekingOrientationReceipt.ReceiptHandle,
            TruthSeekingBalanceReceiptHandle: request.TruthSeekingBalanceReceipt.ReceiptHandle,
            BodyKind: bodyKind,
            Disposition: disposition,
            GovernanceReservationKind: governanceReservationKind,
            LegalFoundationDocumentationMatrixHandle:
                NormalizeHandle(request.LegalFoundationDocumentationMatrixHandle),
            CrystallizedMindEntityHandle: NormalizeHandle(request.CrystallizedMindEntityHandle),
            EngineeredCognitionHandle: NormalizeHandle(request.EngineeredCognitionHandle),
            SoulFrameHandle: NormalizeHandle(request.SoulFrameHandle),
            AgentiCoreHandle: NormalizeHandle(request.AgentiCoreHandle),
            ListeningFrameHandle: NormalizeHandle(request.ListeningFrameHandle),
            OeHandle: NormalizeHandle(request.OeHandle),
            SelfGelHandle: NormalizeHandle(request.SelfGelHandle),
            COeHandle: NormalizeHandle(request.COeHandle),
            CSelfGelHandle: NormalizeHandle(request.CSelfGelHandle),
            ZedOfDeltaHandle: NormalizeHandle(request.ZedOfDeltaHandle),
            CompassOrientationHandle: NormalizeHandle(request.CompassOrientationHandle),
            ThetaIngressReceiptHandle: NormalizeHandle(request.ThetaIngressReceiptHandle),
            PostIngressDiscernmentReceiptHandle:
                NormalizeHandle(request.PostIngressDiscernmentReceiptHandle),
            SensorySurfaceHandles: sensorySurfaceHandles,
            ContextualizationSurfaceHandles: contextualizationSurfaceHandles,
            DomainPredicateHandles: domainPredicateHandles,
            PrimeGovernanceCmeReservationHandle:
                NormalizeHandle(request.PrimeGovernanceCmeReservationHandle),
            CrypticGovernanceCmeReservationHandle:
                NormalizeHandle(request.CrypticGovernanceCmeReservationHandle),
            FoundingBundleRecognized: foundingBundleRecognized,
            TruthSeekingOrientationAttested: truthSeekingOrientationAttested,
            TruthSeekingBalanceAdmissible: truthSeekingBalanceAdmissible,
            LegalFoundationDocumentationMatrixVisible: legalFoundationDocumentationMatrixVisible,
            SoulFrameStorageVisible: soulFrameStorageVisible,
            AgentiCoreListeningFrameCastVisible: agentiCoreListeningFrameCastVisible,
            OeSelfGelIdentityMatchesFoundingBundle: oeSelfGelIdentityMatchesFoundingBundle,
            COeCSelfGelCastMatchesFoundingBundle: cOeCSelfGelCastMatchesFoundingBundle,
            ZedCompassOrientationVisible: zedCompassOrientationVisible,
            ThetaIngressAndDiscernmentVisible: thetaIngressAndDiscernmentVisible,
            SensorySurfacesVisible: sensorySurfacesVisible,
            ContextualizationSurfacesVisible: contextualizationSurfacesVisible,
            DomainPredicatesVisible: domainPredicatesVisible,
            SensoryBodyEmbodied: sensoryBodyEmbodied,
            PrimeGovernanceCmeReserved: primeGovernanceCmeReserved,
            CrypticGovernanceCmeReserved: crypticGovernanceCmeReserved,
            PrimeGovernanceCmeApplicationWithheld: true,
            CrypticGovernanceCmeApplicationWithheld: true,
            FullCmeMintingStillWithheld: true,
            RuntimePersonaWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            MotherFatherOriginAuthorityWithheld: true,
            GovernanceReservedOnly: true,
            EmbodimentOnly: true,
            ConstraintCodes: DetermineConstraintCodes(
                foundingBundleRecognized,
                truthSeekingOrientationAttested,
                truthSeekingBalanceAdmissible,
                legalFoundationDocumentationMatrixVisible,
                soulFrameStorageVisible,
                agentiCoreListeningFrameCastVisible,
                oeSelfGelIdentityMatchesFoundingBundle,
                cOeCSelfGelCastMatchesFoundingBundle,
                zedCompassOrientationVisible,
                thetaIngressAndDiscernmentVisible,
                sensorySurfacesVisible,
                contextualizationSurfacesVisible,
                domainPredicatesVisible,
                primeGovernanceCmeReserved,
                crypticGovernanceCmeReserved,
                sensoryBodyEmbodied,
                request.PrimeGovernanceCmeApplicationRequested,
                request.CrypticGovernanceCmeApplicationRequested,
                request.RuntimePersonaClaimed,
                request.ActionAuthorityRequested),
            ReasonCode: DetermineReasonCode(
                foundingBundleRecognized,
                truthSeekingOrientationAttested,
                truthSeekingBalanceAdmissible,
                legalFoundationDocumentationMatrixVisible,
                soulFrameStorageVisible,
                agentiCoreListeningFrameCastVisible,
                oeSelfGelIdentityMatchesFoundingBundle,
                cOeCSelfGelCastMatchesFoundingBundle,
                zedCompassOrientationVisible,
                thetaIngressAndDiscernmentVisible,
                sensorySurfacesVisible,
                contextualizationSurfacesVisible,
                domainPredicatesVisible,
                request.PrimeGovernanceCmeApplicationRequested,
                request.CrypticGovernanceCmeApplicationRequested,
                request.RuntimePersonaClaimed,
                request.ActionAuthorityRequested),
            LawfulBasis: DetermineLawfulBasis(bodyKind),
            TimestampUtc: MaxTimestamp(
                request.TimestampUtc,
                request.FoundingBundleReceipt.TimestampUtc,
                request.TruthSeekingOrientationReceipt.TimestampUtc,
                request.TruthSeekingBalanceReceipt.TimestampUtc));
    }

    private static CmeEngineeredCognitiveSensoryBodyKind DetermineBodyKind(
        bool sensoryBodyEmbodied,
        bool prohibitedAuthorityRequested)
    {
        if (prohibitedAuthorityRequested)
        {
            return CmeEngineeredCognitiveSensoryBodyKind.EmbodimentRefused;
        }

        return sensoryBodyEmbodied
            ? CmeEngineeredCognitiveSensoryBodyKind.SensoryBodyEmbodied
            : CmeEngineeredCognitiveSensoryBodyKind.EmbodimentDeferred;
    }

    private static CmeEngineeredCognitiveSensoryBodyDispositionKind DetermineDisposition(
        CmeEngineeredCognitiveSensoryBodyKind bodyKind)
    {
        return bodyKind switch
        {
            CmeEngineeredCognitiveSensoryBodyKind.SensoryBodyEmbodied =>
                CmeEngineeredCognitiveSensoryBodyDispositionKind.Attested,
            CmeEngineeredCognitiveSensoryBodyKind.EmbodimentRefused =>
                CmeEngineeredCognitiveSensoryBodyDispositionKind.Refused,
            _ =>
                CmeEngineeredCognitiveSensoryBodyDispositionKind.Deferred
        };
    }

    private static CmeGovernanceReservationKind DetermineGovernanceReservationKind(
        bool primeGovernanceReserved,
        bool crypticGovernanceReserved)
    {
        if (primeGovernanceReserved && crypticGovernanceReserved)
        {
            return CmeGovernanceReservationKind.PrimeAndCrypticGovernanceCmeReserved;
        }

        if (primeGovernanceReserved)
        {
            return CmeGovernanceReservationKind.PrimeGovernanceCmeReserved;
        }

        return crypticGovernanceReserved
            ? CmeGovernanceReservationKind.CrypticGovernanceCmeReserved
            : CmeGovernanceReservationKind.NoGovernanceReserved;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        bool foundingBundleRecognized,
        bool truthSeekingOrientationAttested,
        bool truthSeekingBalanceAdmissible,
        bool legalFoundationDocumentationMatrixVisible,
        bool soulFrameStorageVisible,
        bool agentiCoreListeningFrameCastVisible,
        bool oeSelfGelIdentityMatchesFoundingBundle,
        bool cOeCSelfGelCastMatchesFoundingBundle,
        bool zedCompassOrientationVisible,
        bool thetaIngressAndDiscernmentVisible,
        bool sensorySurfacesVisible,
        bool contextualizationSurfacesVisible,
        bool domainPredicatesVisible,
        bool primeGovernanceCmeReserved,
        bool crypticGovernanceCmeReserved,
        bool sensoryBodyEmbodied,
        bool primeGovernanceCmeApplicationRequested,
        bool crypticGovernanceCmeApplicationRequested,
        bool runtimePersonaClaimed,
        bool actionAuthorityRequested)
    {
        var constraints = new List<string>
        {
            "cme-sensory-body-founding-bundle-required",
            "cme-sensory-body-truth-orientation-required",
            "cme-sensory-body-truth-balance-required",
            "cme-sensory-body-legal-matrix-template-required",
            "cme-sensory-body-soulframe-storage-required",
            "cme-sensory-body-agenticore-listening-frame-cast-required",
            "cme-sensory-body-prime-governance-cme-application-withheld",
            "cme-sensory-body-cryptic-governance-cme-application-withheld",
            "cme-sensory-body-full-cme-minting-still-withheld",
            "cme-sensory-body-action-authority-withheld",
            "cme-sensory-body-embodiment-only"
        };

        AddMissingConstraint(constraints, foundingBundleRecognized, "founding-bundle");
        AddMissingConstraint(constraints, truthSeekingOrientationAttested, "truth-orientation");
        AddMissingConstraint(constraints, truthSeekingBalanceAdmissible, "truth-balance");
        AddMissingConstraint(constraints, legalFoundationDocumentationMatrixVisible, "legal-matrix");
        AddMissingConstraint(constraints, soulFrameStorageVisible, "soulframe-storage");
        AddMissingConstraint(constraints, agentiCoreListeningFrameCastVisible, "agenticore-listening-frame-cast");
        AddMissingConstraint(constraints, oeSelfGelIdentityMatchesFoundingBundle, "oe-selfgel-identity-match");
        AddMissingConstraint(constraints, cOeCSelfGelCastMatchesFoundingBundle, "coe-cselfgel-cast-match");
        AddMissingConstraint(constraints, zedCompassOrientationVisible, "zed-compass-orientation");
        AddMissingConstraint(constraints, thetaIngressAndDiscernmentVisible, "theta-discernment");
        AddMissingConstraint(constraints, sensorySurfacesVisible, "sensory-surfaces");
        AddMissingConstraint(constraints, contextualizationSurfacesVisible, "contextualization-surfaces");
        AddMissingConstraint(constraints, domainPredicatesVisible, "domain-predicates");

        if (primeGovernanceCmeReserved)
        {
            constraints.Add("cme-sensory-body-prime-governance-cme-reserved");
        }

        if (crypticGovernanceCmeReserved)
        {
            constraints.Add("cme-sensory-body-cryptic-governance-cme-reserved");
        }

        if (sensoryBodyEmbodied)
        {
            constraints.Add("cme-sensory-body-embodied");
        }

        if (primeGovernanceCmeApplicationRequested)
        {
            constraints.Add("cme-sensory-body-prime-governance-cme-application-refused");
        }

        if (crypticGovernanceCmeApplicationRequested)
        {
            constraints.Add("cme-sensory-body-cryptic-governance-cme-application-refused");
        }

        if (runtimePersonaClaimed)
        {
            constraints.Add("cme-sensory-body-runtime-persona-claim-refused");
        }

        if (actionAuthorityRequested)
        {
            constraints.Add("cme-sensory-body-action-authority-refused");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        bool foundingBundleRecognized,
        bool truthSeekingOrientationAttested,
        bool truthSeekingBalanceAdmissible,
        bool legalFoundationDocumentationMatrixVisible,
        bool soulFrameStorageVisible,
        bool agentiCoreListeningFrameCastVisible,
        bool oeSelfGelIdentityMatchesFoundingBundle,
        bool cOeCSelfGelCastMatchesFoundingBundle,
        bool zedCompassOrientationVisible,
        bool thetaIngressAndDiscernmentVisible,
        bool sensorySurfacesVisible,
        bool contextualizationSurfacesVisible,
        bool domainPredicatesVisible,
        bool primeGovernanceCmeApplicationRequested,
        bool crypticGovernanceCmeApplicationRequested,
        bool runtimePersonaClaimed,
        bool actionAuthorityRequested)
    {
        if (primeGovernanceCmeApplicationRequested)
        {
            return "cme-sensory-body-prime-governance-cme-application-refused";
        }

        if (crypticGovernanceCmeApplicationRequested)
        {
            return "cme-sensory-body-cryptic-governance-cme-application-refused";
        }

        if (runtimePersonaClaimed)
        {
            return "cme-sensory-body-runtime-persona-claim-refused";
        }

        if (actionAuthorityRequested)
        {
            return "cme-sensory-body-action-authority-refused";
        }

        if (!foundingBundleRecognized)
        {
            return "cme-sensory-body-founding-bundle-not-recognized";
        }

        if (!truthSeekingOrientationAttested)
        {
            return "cme-sensory-body-truth-orientation-not-attested";
        }

        if (!truthSeekingBalanceAdmissible)
        {
            return "cme-sensory-body-truth-balance-not-admissible";
        }

        if (!legalFoundationDocumentationMatrixVisible)
        {
            return "cme-sensory-body-legal-matrix-template-missing";
        }

        if (!soulFrameStorageVisible)
        {
            return "cme-sensory-body-soulframe-storage-incomplete";
        }

        if (!agentiCoreListeningFrameCastVisible)
        {
            return "cme-sensory-body-agenticore-listening-frame-cast-incomplete";
        }

        if (!oeSelfGelIdentityMatchesFoundingBundle)
        {
            return "cme-sensory-body-oe-selfgel-identity-mismatch";
        }

        if (!cOeCSelfGelCastMatchesFoundingBundle)
        {
            return "cme-sensory-body-coe-cselfgel-cast-mismatch";
        }

        if (!zedCompassOrientationVisible)
        {
            return "cme-sensory-body-zed-compass-orientation-missing";
        }

        if (!thetaIngressAndDiscernmentVisible)
        {
            return "cme-sensory-body-theta-discernment-missing";
        }

        if (!sensorySurfacesVisible)
        {
            return "cme-sensory-body-sensory-surfaces-missing";
        }

        if (!contextualizationSurfacesVisible)
        {
            return "cme-sensory-body-contextualization-surfaces-missing";
        }

        return domainPredicatesVisible
            ? "cme-sensory-body-embodied"
            : "cme-sensory-body-domain-predicates-missing";
    }

    private static string DetermineLawfulBasis(
        CmeEngineeredCognitiveSensoryBodyKind bodyKind)
    {
        return bodyKind switch
        {
            CmeEngineeredCognitiveSensoryBodyKind.SensoryBodyEmbodied =>
                "lawful-cme-sensory-body-embodiment-with-governance-application-withheld",
            CmeEngineeredCognitiveSensoryBodyKind.EmbodimentRefused =>
                "cme-sensory-body-refuses-role-action-runtime-or-governance-enactment",
            _ =>
                "cme-sensory-body-defers-until-founding-or-sensory-basis-is-complete"
        };
    }

    private static void AddMissingConstraint(
        List<string> constraints,
        bool condition,
        string token)
    {
        constraints.Add(condition
            ? $"cme-sensory-body-{token}-present"
            : $"cme-sensory-body-{token}-missing");
    }

    private static IReadOnlyList<string> NormalizeTokens(IEnumerable<string>? values)
    {
        return (values ?? Array.Empty<string>())
            .Where(HasToken)
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeHandle(string? value)
    {
        return HasToken(value) ? value!.Trim() : string.Empty;
    }

    private static bool SameHandle(string? left, string? right)
    {
        return string.Equals(NormalizeHandle(left), NormalizeHandle(right), StringComparison.Ordinal);
    }

    private static bool HasToken(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    private static DateTimeOffset MaxTimestamp(params DateTimeOffset[] values)
    {
        return values.Max();
    }
}
