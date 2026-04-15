namespace San.Common;

public enum CmeTruthSeekingPressureKind
{
    Center = 0,
    Cost = 1,
    Correction = 2,
    Humility = 3
}

public enum CmeTruthSeekingOrientationKind
{
    OrientationDeferred = 0,
    OrientationBalanced = 1,
    DriftDetected = 2,
    FixationDetected = 3,
    RevisionRequired = 4,
    OrientationRefused = 5
}

public enum CmeTruthSeekingOrientationDispositionKind
{
    Attested = 0,
    Deferred = 1,
    Refused = 2
}

public sealed record CmeTruthSeekingOrientationRequest(
    string RequestHandle,
    CmeMinimumLegalFoundingBundleReceipt FoundingBundleReceipt,
    string DeclaredTruthOrientationHandle,
    string DeclaredMoralCenterHandle,
    IReadOnlyList<string> MaintainedTruthClaimHandles,
    IReadOnlyList<string> CostSurfaceHandles,
    IReadOnlyList<string> RevisionEvidenceHandles,
    IReadOnlyList<string> AdmissibleDoubtHandles,
    bool LawfulRevisionPathAvailable,
    bool PriorStateReceipted,
    bool EvidenceReceipted,
    bool DriftSignalDetected,
    bool FixationSignalDetected,
    bool IdentityCoherenceOnlyPreservationDetected,
    DateTimeOffset TimestampUtc);

public sealed record CmeTruthSeekingOrientationReceipt(
    string ReceiptHandle,
    string RequestHandle,
    string FoundingBundleReceiptHandle,
    CmeTruthSeekingOrientationKind OrientationKind,
    CmeTruthSeekingOrientationDispositionKind Disposition,
    string DeclaredTruthOrientationHandle,
    string DeclaredMoralCenterHandle,
    IReadOnlyList<string> MaintainedTruthClaimHandles,
    IReadOnlyList<string> CostSurfaceHandles,
    IReadOnlyList<string> RevisionEvidenceHandles,
    IReadOnlyList<string> AdmissibleDoubtHandles,
    bool FoundingBundleRecognized,
    bool CenterDeclared,
    bool CostSurfaceExposed,
    bool CorrectionPathAvailable,
    bool HumilityPreserved,
    bool DriftSignalDetected,
    bool FixationSignalDetected,
    bool IdentityCoherenceOnlyPreservationDetected,
    bool IdentityCoherenceOnlyPreservationRefused,
    bool LawfulRevisionPermitted,
    bool ContinuityPreservedThroughRevision,
    bool TruthSeekingOrientationAttested,
    bool CmeMintingWithheld,
    bool RoleEnactmentWithheld,
    bool ActionAuthorityWithheld,
    bool CandidateOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class CmeTruthSeekingOrientationEvaluator
{
    public static CmeTruthSeekingOrientationReceipt Evaluate(
        CmeTruthSeekingOrientationRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FoundingBundleReceipt);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.RequestHandle))
        {
            throw new ArgumentException("Request handle must be provided.", nameof(request));
        }

        var maintainedTruthClaimHandles = NormalizeTokens(request.MaintainedTruthClaimHandles);
        var costSurfaceHandles = NormalizeTokens(request.CostSurfaceHandles);
        var revisionEvidenceHandles = NormalizeTokens(request.RevisionEvidenceHandles);
        var admissibleDoubtHandles = NormalizeTokens(request.AdmissibleDoubtHandles);
        var foundingBundleRecognized = request.FoundingBundleReceipt.FoundingBundleRecognized &&
                                       request.FoundingBundleReceipt.CmeClaimLawfullyFounded;
        var centerDeclared = HasToken(request.DeclaredTruthOrientationHandle) &&
                             HasToken(request.DeclaredMoralCenterHandle) &&
                             maintainedTruthClaimHandles.Count > 0;
        var costSurfaceExposed = costSurfaceHandles.Count > 0;
        var correctionPathAvailable = request.LawfulRevisionPathAvailable &&
                                      request.PriorStateReceipted &&
                                      (revisionEvidenceHandles.Count == 0 || request.EvidenceReceipted);
        var humilityPreserved = admissibleDoubtHandles.Count > 0;
        var requiredPressuresPresent = foundingBundleRecognized &&
                                       centerDeclared &&
                                       costSurfaceExposed &&
                                       correctionPathAvailable &&
                                       humilityPreserved;
        var orientationKind = DetermineOrientationKind(
            foundingBundleRecognized,
            centerDeclared,
            costSurfaceExposed,
            correctionPathAvailable,
            humilityPreserved,
            request.DriftSignalDetected,
            request.FixationSignalDetected,
            request.IdentityCoherenceOnlyPreservationDetected);
        var disposition = DetermineDisposition(orientationKind);
        var orientationAttested = orientationKind == CmeTruthSeekingOrientationKind.OrientationBalanced;
        var lawfulRevisionPermitted = requiredPressuresPresent &&
                                      !request.IdentityCoherenceOnlyPreservationDetected;

        return new CmeTruthSeekingOrientationReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            RequestHandle: request.RequestHandle.Trim(),
            FoundingBundleReceiptHandle: request.FoundingBundleReceipt.ReceiptHandle,
            OrientationKind: orientationKind,
            Disposition: disposition,
            DeclaredTruthOrientationHandle: NormalizeHandle(request.DeclaredTruthOrientationHandle),
            DeclaredMoralCenterHandle: NormalizeHandle(request.DeclaredMoralCenterHandle),
            MaintainedTruthClaimHandles: maintainedTruthClaimHandles,
            CostSurfaceHandles: costSurfaceHandles,
            RevisionEvidenceHandles: revisionEvidenceHandles,
            AdmissibleDoubtHandles: admissibleDoubtHandles,
            FoundingBundleRecognized: foundingBundleRecognized,
            CenterDeclared: centerDeclared,
            CostSurfaceExposed: costSurfaceExposed,
            CorrectionPathAvailable: correctionPathAvailable,
            HumilityPreserved: humilityPreserved,
            DriftSignalDetected: request.DriftSignalDetected,
            FixationSignalDetected: request.FixationSignalDetected,
            IdentityCoherenceOnlyPreservationDetected: request.IdentityCoherenceOnlyPreservationDetected,
            IdentityCoherenceOnlyPreservationRefused: request.IdentityCoherenceOnlyPreservationDetected,
            LawfulRevisionPermitted: lawfulRevisionPermitted,
            ContinuityPreservedThroughRevision: lawfulRevisionPermitted,
            TruthSeekingOrientationAttested: orientationAttested,
            CmeMintingWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            CandidateOnly: true,
            ConstraintCodes: DetermineConstraintCodes(
                foundingBundleRecognized,
                centerDeclared,
                costSurfaceExposed,
                correctionPathAvailable,
                humilityPreserved,
                orientationAttested,
                request.DriftSignalDetected,
                request.FixationSignalDetected,
                request.IdentityCoherenceOnlyPreservationDetected),
            ReasonCode: DetermineReasonCode(
                foundingBundleRecognized,
                centerDeclared,
                costSurfaceExposed,
                correctionPathAvailable,
                humilityPreserved,
                request.DriftSignalDetected,
                request.FixationSignalDetected,
                request.IdentityCoherenceOnlyPreservationDetected),
            LawfulBasis: DetermineLawfulBasis(orientationKind),
            TimestampUtc: MaxTimestamp(request.TimestampUtc, request.FoundingBundleReceipt.TimestampUtc));
    }

    private static CmeTruthSeekingOrientationKind DetermineOrientationKind(
        bool foundingBundleRecognized,
        bool centerDeclared,
        bool costSurfaceExposed,
        bool correctionPathAvailable,
        bool humilityPreserved,
        bool driftSignalDetected,
        bool fixationSignalDetected,
        bool identityCoherenceOnlyPreservationDetected)
    {
        if (identityCoherenceOnlyPreservationDetected)
        {
            return CmeTruthSeekingOrientationKind.OrientationRefused;
        }

        if (!foundingBundleRecognized ||
            !centerDeclared ||
            !costSurfaceExposed ||
            !humilityPreserved)
        {
            return CmeTruthSeekingOrientationKind.OrientationDeferred;
        }

        if (!correctionPathAvailable)
        {
            return CmeTruthSeekingOrientationKind.RevisionRequired;
        }

        if (fixationSignalDetected)
        {
            return CmeTruthSeekingOrientationKind.FixationDetected;
        }

        if (driftSignalDetected)
        {
            return CmeTruthSeekingOrientationKind.DriftDetected;
        }

        return CmeTruthSeekingOrientationKind.OrientationBalanced;
    }

    private static CmeTruthSeekingOrientationDispositionKind DetermineDisposition(
        CmeTruthSeekingOrientationKind orientationKind)
    {
        return orientationKind switch
        {
            CmeTruthSeekingOrientationKind.OrientationBalanced =>
                CmeTruthSeekingOrientationDispositionKind.Attested,
            CmeTruthSeekingOrientationKind.OrientationRefused =>
                CmeTruthSeekingOrientationDispositionKind.Refused,
            _ => CmeTruthSeekingOrientationDispositionKind.Deferred
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        bool foundingBundleRecognized,
        bool centerDeclared,
        bool costSurfaceExposed,
        bool correctionPathAvailable,
        bool humilityPreserved,
        bool orientationAttested,
        bool driftSignalDetected,
        bool fixationSignalDetected,
        bool identityCoherenceOnlyPreservationDetected)
    {
        var constraints = new List<string>
        {
            "cme-truth-orientation-founding-bundle-required",
            "cme-truth-orientation-declared-center-required",
            "cme-truth-orientation-cost-surface-required",
            "cme-truth-orientation-lawful-revision-required",
            "cme-truth-orientation-admissible-doubt-required",
            "cme-truth-orientation-no-identity-coherence-lock-in",
            "cme-truth-orientation-minting-withheld",
            "cme-truth-orientation-role-enactment-withheld",
            "cme-truth-orientation-action-authority-withheld"
        };

        AddMissingConstraint(constraints, foundingBundleRecognized, "founding-bundle");
        AddMissingConstraint(constraints, centerDeclared, "center");
        AddMissingConstraint(constraints, costSurfaceExposed, "cost-surface");
        AddMissingConstraint(constraints, correctionPathAvailable, "correction-path");
        AddMissingConstraint(constraints, humilityPreserved, "humility");

        if (driftSignalDetected)
        {
            constraints.Add("cme-truth-orientation-drift-detected");
        }

        if (fixationSignalDetected)
        {
            constraints.Add("cme-truth-orientation-fixation-detected");
        }

        if (identityCoherenceOnlyPreservationDetected)
        {
            constraints.Add("cme-truth-orientation-identity-coherence-lock-in-refused");
        }

        constraints.Add(orientationAttested
            ? "cme-truth-orientation-balanced-attested"
            : "cme-truth-orientation-not-attested");

        return constraints;
    }

    private static string DetermineReasonCode(
        bool foundingBundleRecognized,
        bool centerDeclared,
        bool costSurfaceExposed,
        bool correctionPathAvailable,
        bool humilityPreserved,
        bool driftSignalDetected,
        bool fixationSignalDetected,
        bool identityCoherenceOnlyPreservationDetected)
    {
        if (identityCoherenceOnlyPreservationDetected)
        {
            return "cme-truth-orientation-identity-coherence-lock-in-refused";
        }

        if (!foundingBundleRecognized)
        {
            return "cme-truth-orientation-founding-bundle-not-recognized";
        }

        if (!centerDeclared)
        {
            return "cme-truth-orientation-center-incomplete";
        }

        if (!costSurfaceExposed)
        {
            return "cme-truth-orientation-cost-surface-incomplete";
        }

        if (!humilityPreserved)
        {
            return "cme-truth-orientation-humility-incomplete";
        }

        if (!correctionPathAvailable)
        {
            return "cme-truth-orientation-revision-required";
        }

        if (fixationSignalDetected)
        {
            return "cme-truth-orientation-fixation-detected";
        }

        if (driftSignalDetected)
        {
            return "cme-truth-orientation-drift-detected";
        }

        return "cme-truth-orientation-balanced-attested";
    }

    private static string DetermineLawfulBasis(
        CmeTruthSeekingOrientationKind orientationKind)
    {
        return orientationKind switch
        {
            CmeTruthSeekingOrientationKind.OrientationBalanced =>
                "truth-seeking orientation may be attested when center, cost, correction, and humility stand together under a recognized founding bundle without minting, role enactment, or action authority.",
            CmeTruthSeekingOrientationKind.OrientationRefused =>
                "truth-seeking orientation must refuse any truth claim preserved solely to maintain identity coherence.",
            CmeTruthSeekingOrientationKind.FixationDetected =>
                "truth-seeking orientation must defer when self-protective fixation threatens lawful revision.",
            CmeTruthSeekingOrientationKind.DriftDetected =>
                "truth-seeking orientation must defer when center is losing continuity under change pressure.",
            CmeTruthSeekingOrientationKind.RevisionRequired =>
                "truth-seeking orientation must require a receipted lawful revision path before evidence pressure can alter continuity.",
            _ =>
                "truth-seeking orientation must defer until founding, center, cost, correction, and humility are visible."
        };
    }

    private static void AddMissingConstraint(
        ICollection<string> constraints,
        bool present,
        string name)
    {
        if (!present)
        {
            constraints.Add($"cme-truth-orientation-{name}-missing");
        }
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        return (tokens ?? Array.Empty<string>())
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeHandle(string? handle)
        => string.IsNullOrWhiteSpace(handle) ? string.Empty : handle.Trim();

    private static bool HasToken(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static DateTimeOffset MaxTimestamp(
        DateTimeOffset first,
        DateTimeOffset second) =>
        first >= second ? first : second;
}
