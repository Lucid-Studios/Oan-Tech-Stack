namespace San.Common;

public enum PrimeSeedStateKind
{
    FirstPrimeNotReady = 0,
    SeedMaterialIncomplete = 1,
    PrimeSeedPreDomainStanding = 2
}

public sealed record PrimeSeedStateRequest(
    string RequestHandle,
    EngineeredCognitionFirstPrimeStateReceipt FirstPrimeReceipt,
    string SeedSourceHandle,
    string SeedCarrierHandle,
    string SeedContinuityHandle,
    string SeedIntegrityHandle,
    IReadOnlyList<string> SeedEvidenceHandles,
    DateTimeOffset TimestampUtc);

public sealed record PrimeSeedStateReceipt(
    string ReceiptHandle,
    string RequestHandle,
    string FirstPrimeReceiptHandle,
    string PrimeRetainedRecordHandle,
    string? StableOneHandle,
    PrimeSeedStateKind SeedState,
    string SeedSourceHandle,
    string SeedCarrierHandle,
    string SeedContinuityHandle,
    string SeedIntegrityHandle,
    IReadOnlyList<string> SeedEvidenceHandles,
    bool FirstPrimePreRoleStandingPresent,
    bool StableOnePresent,
    bool PrimeRetainedStandingPresent,
    bool SeedSourcePresent,
    bool SeedCarrierPresent,
    bool SeedContinuityPresent,
    bool SeedIntegrityPresent,
    bool DomainAdmissionWithheld,
    bool LawfullyBondedDomainIntegrationWithheld,
    bool CmeFoundingWithheld,
    bool CmeMintingWithheld,
    bool RuntimePersonaWithheld,
    bool RoleEnactmentWithheld,
    bool ActionAuthorityWithheld,
    bool MotherFatherDomainRoleApplicationWithheld,
    bool CradleLocalGoverningSurfaceWithheld,
    bool PrimeClosureStillWithheld,
    bool CandidateOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrimeSeedStateEvaluator
{
    public static PrimeSeedStateReceipt Evaluate(
        PrimeSeedStateRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FirstPrimeReceipt);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.RequestHandle))
        {
            throw new ArgumentException("Request handle must be provided.", nameof(request));
        }

        var firstPrime = request.FirstPrimeReceipt;
        var seedEvidenceHandles = NormalizeTokens(request.SeedEvidenceHandles);
        var firstPrimePresent = DetermineFirstPrimePresent(firstPrime);
        var stableOnePresent = HasToken(firstPrime.StableOneHandle) && firstPrime.StableOneSatisfied;
        var primeRetainedPresent = firstPrime.PrimeRetainedStandingReached;
        var seedSourcePresent = HasToken(request.SeedSourceHandle);
        var seedCarrierPresent = HasToken(request.SeedCarrierHandle);
        var seedContinuityPresent = HasToken(request.SeedContinuityHandle);
        var seedIntegrityPresent = HasToken(request.SeedIntegrityHandle) && seedEvidenceHandles.Count > 0;
        var seedState = DetermineSeedState(
            firstPrimePresent,
            stableOnePresent,
            primeRetainedPresent,
            seedSourcePresent,
            seedCarrierPresent,
            seedContinuityPresent,
            seedIntegrityPresent);

        return new PrimeSeedStateReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            RequestHandle: request.RequestHandle.Trim(),
            FirstPrimeReceiptHandle: firstPrime.ReceiptHandle,
            PrimeRetainedRecordHandle: firstPrime.PrimeRetainedRecordHandle,
            StableOneHandle: stableOnePresent ? firstPrime.StableOneHandle : null,
            SeedState: seedState,
            SeedSourceHandle: NormalizeHandle(request.SeedSourceHandle),
            SeedCarrierHandle: NormalizeHandle(request.SeedCarrierHandle),
            SeedContinuityHandle: NormalizeHandle(request.SeedContinuityHandle),
            SeedIntegrityHandle: NormalizeHandle(request.SeedIntegrityHandle),
            SeedEvidenceHandles: seedEvidenceHandles,
            FirstPrimePreRoleStandingPresent: firstPrimePresent,
            StableOnePresent: stableOnePresent,
            PrimeRetainedStandingPresent: primeRetainedPresent,
            SeedSourcePresent: seedSourcePresent,
            SeedCarrierPresent: seedCarrierPresent,
            SeedContinuityPresent: seedContinuityPresent,
            SeedIntegrityPresent: seedIntegrityPresent,
            DomainAdmissionWithheld: true,
            LawfullyBondedDomainIntegrationWithheld: true,
            CmeFoundingWithheld: true,
            CmeMintingWithheld: true,
            RuntimePersonaWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            MotherFatherDomainRoleApplicationWithheld: firstPrime.MotherFatherDomainRoleApplicationWithheld,
            CradleLocalGoverningSurfaceWithheld: firstPrime.CradleLocalGoverningSurfaceWithheld,
            PrimeClosureStillWithheld: firstPrime.PrimeClosureStillWithheld,
            CandidateOnly: true,
            ConstraintCodes: DetermineConstraintCodes(
                seedState,
                firstPrimePresent,
                stableOnePresent,
                primeRetainedPresent,
                seedSourcePresent,
                seedCarrierPresent,
                seedContinuityPresent,
                seedIntegrityPresent),
            ReasonCode: DetermineReasonCode(
                seedState,
                firstPrimePresent,
                stableOnePresent,
                primeRetainedPresent,
                seedSourcePresent,
                seedCarrierPresent,
                seedContinuityPresent,
                seedIntegrityPresent),
            LawfulBasis: DetermineLawfulBasis(seedState),
            TimestampUtc: MaxTimestamp(request.TimestampUtc, firstPrime.TimestampUtc));
    }

    public static PrimeSeedStateKind DetermineSeedState(
        bool firstPrimePreRoleStandingPresent,
        bool stableOnePresent,
        bool primeRetainedStandingPresent,
        bool seedSourcePresent,
        bool seedCarrierPresent,
        bool seedContinuityPresent,
        bool seedIntegrityPresent)
    {
        if (!firstPrimePreRoleStandingPresent ||
            !stableOnePresent ||
            !primeRetainedStandingPresent)
        {
            return PrimeSeedStateKind.FirstPrimeNotReady;
        }

        if (!seedSourcePresent ||
            !seedCarrierPresent ||
            !seedContinuityPresent ||
            !seedIntegrityPresent)
        {
            return PrimeSeedStateKind.SeedMaterialIncomplete;
        }

        return PrimeSeedStateKind.PrimeSeedPreDomainStanding;
    }

    private static bool DetermineFirstPrimePresent(
        EngineeredCognitionFirstPrimeStateReceipt firstPrime)
    {
        return firstPrime.FirstPrimeState == EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding &&
               firstPrime.StableOneSatisfied &&
               firstPrime.PrimeRetainedStandingReached &&
               firstPrime.MotherFatherDomainRoleApplicationWithheld &&
               firstPrime.CradleLocalGoverningSurfaceWithheld &&
               firstPrime.PrimeClosureStillWithheld;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        PrimeSeedStateKind seedState,
        bool firstPrimePresent,
        bool stableOnePresent,
        bool primeRetainedPresent,
        bool seedSourcePresent,
        bool seedCarrierPresent,
        bool seedContinuityPresent,
        bool seedIntegrityPresent)
    {
        var constraints = new List<string>
        {
            "prime-seed-state-domain-admission-withheld",
            "prime-seed-state-lawfully-bonded-domain-integration-withheld",
            "prime-seed-state-cme-founding-withheld",
            "prime-seed-state-cme-minting-withheld",
            "prime-seed-state-role-enactment-withheld",
            "prime-seed-state-action-authority-withheld",
            "prime-seed-state-candidate-only"
        };

        constraints.Add(seedState switch
        {
            PrimeSeedStateKind.PrimeSeedPreDomainStanding => "prime-seed-state-pre-domain-standing",
            PrimeSeedStateKind.SeedMaterialIncomplete => "prime-seed-state-seed-material-incomplete",
            _ => "prime-seed-state-first-prime-not-ready"
        });

        if (!firstPrimePresent)
        {
            constraints.Add("prime-seed-state-first-prime-pre-role-standing-required");
        }

        if (!stableOnePresent)
        {
            constraints.Add("prime-seed-state-stable-one-required");
        }

        if (!primeRetainedPresent)
        {
            constraints.Add("prime-seed-state-prime-retained-standing-required");
        }

        if (!seedSourcePresent)
        {
            constraints.Add("prime-seed-state-seed-source-required");
        }

        if (!seedCarrierPresent)
        {
            constraints.Add("prime-seed-state-seed-carrier-required");
        }

        if (!seedContinuityPresent)
        {
            constraints.Add("prime-seed-state-seed-continuity-required");
        }

        if (!seedIntegrityPresent)
        {
            constraints.Add("prime-seed-state-seed-integrity-required");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        PrimeSeedStateKind seedState,
        bool firstPrimePresent,
        bool stableOnePresent,
        bool primeRetainedPresent,
        bool seedSourcePresent,
        bool seedCarrierPresent,
        bool seedContinuityPresent,
        bool seedIntegrityPresent)
    {
        if (!firstPrimePresent)
        {
            return "prime-seed-state-first-prime-pre-role-standing-required";
        }

        if (!stableOnePresent)
        {
            return "prime-seed-state-stable-one-required";
        }

        if (!primeRetainedPresent)
        {
            return "prime-seed-state-prime-retained-standing-required";
        }

        if (!seedSourcePresent)
        {
            return "prime-seed-state-seed-source-required";
        }

        if (!seedCarrierPresent)
        {
            return "prime-seed-state-seed-carrier-required";
        }

        if (!seedContinuityPresent)
        {
            return "prime-seed-state-seed-continuity-required";
        }

        if (!seedIntegrityPresent)
        {
            return "prime-seed-state-seed-integrity-required";
        }

        return seedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding
            ? "prime-seed-state-pre-domain-standing"
            : "prime-seed-state-not-ready";
    }

    private static string DetermineLawfulBasis(
        PrimeSeedStateKind seedState)
    {
        return seedState switch
        {
            PrimeSeedStateKind.PrimeSeedPreDomainStanding =>
                "Prime seed state may stand only as pre-domain seed-bearing readiness after first Prime pre-role standing, while bonded-domain integration, CME founding, role enactment, and action authority remain withheld.",
            PrimeSeedStateKind.SeedMaterialIncomplete =>
                "Prime seed state must defer until seed source, carrier, continuity, and integrity evidence are explicit.",
            _ =>
                "Prime seed state remains withheld until first Prime pre-role standing, stable-one discernment, and Prime retained standing all remain present."
        };
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
