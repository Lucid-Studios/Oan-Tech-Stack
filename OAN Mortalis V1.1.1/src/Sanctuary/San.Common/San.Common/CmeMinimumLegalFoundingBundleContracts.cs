namespace San.Common;

public enum CmeFoundingBundlePillarKind
{
    OriginAuthorizationFoundation = 0,
    IdentityFormationRecord = 1,
    FirstPrimeStandingProof = 2,
    DomainRoleAdmissionRecord = 3,
    OperationalProvenanceCustody = 4
}

public enum CmeMinimumLegalFoundingBundleKind
{
    BundleIncomplete = 0,
    BundleDeferred = 1,
    BundleRefused = 2,
    BundleRecognized = 3
}

public enum CmeMinimumLegalFoundingBundleDispositionKind
{
    Recognized = 0,
    Deferred = 1,
    Refused = 2
}

public sealed record CmeMinimumLegalFoundingBundleRequest(
    string RequestHandle,
    string OperatorIdentityHandle,
    string OriginAuthorizationHandle,
    string SiteBindingHandle,
    IReadOnlyList<string> LegalAgreementHandles,
    string IdentityFormationHandle,
    string OeHandle,
    string SelfGelHandle,
    string COeHandle,
    string CSelfGelHandle,
    string IdentityIntegrityHash,
    EngineeredCognitionFirstPrimeStateReceipt FirstPrimeReceipt,
    DomainAdmissionRecord DomainAdmissionRecord,
    PrivateDomainServiceWitnessReceipt ServiceWitnessReceipt,
    bool RuntimePersonaClaimed,
    bool RoleEnactmentRequested,
    bool ActionAuthorityRequested,
    DateTimeOffset TimestampUtc);

public sealed record CmeMinimumLegalFoundingBundleReceipt(
    string ReceiptHandle,
    string RequestHandle,
    CmeMinimumLegalFoundingBundleKind BundleKind,
    CmeMinimumLegalFoundingBundleDispositionKind Disposition,
    string OperatorIdentityHandle,
    string OriginAuthorizationHandle,
    string SiteBindingHandle,
    IReadOnlyList<string> LegalAgreementHandles,
    string IdentityFormationHandle,
    string OeHandle,
    string SelfGelHandle,
    string COeHandle,
    string CSelfGelHandle,
    string IdentityIntegrityHash,
    string FirstPrimeReceiptHandle,
    string DomainAdmissionRecordHandle,
    string ServiceWitnessReceiptHandle,
    bool OriginAuthorizationFoundationPresent,
    bool IdentityFormationRecordPresent,
    bool FirstPrimeStandingProofPresent,
    bool DomainRoleAdmissionRecordPresent,
    bool OperationalProvenanceCustodyPresent,
    bool FoundingBundleRecognized,
    bool CmeClaimLawfullyFounded,
    bool CmeMintingWithheld,
    bool RuntimePersonaWithheld,
    bool RoleEnactmentWithheld,
    bool ActionAuthorityWithheld,
    bool MotherFatherApplicationWithheld,
    bool CandidateOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class CmeMinimumLegalFoundingBundleEvaluator
{
    public static CmeMinimumLegalFoundingBundleReceipt Evaluate(
        CmeMinimumLegalFoundingBundleRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.FirstPrimeReceipt);
        ArgumentNullException.ThrowIfNull(request.DomainAdmissionRecord);
        ArgumentNullException.ThrowIfNull(request.ServiceWitnessReceipt);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.RequestHandle))
        {
            throw new ArgumentException("Request handle must be provided.", nameof(request));
        }

        var legalAgreementHandles = NormalizeTokens(request.LegalAgreementHandles);
        var originAuthorizationPresent = HasToken(request.OperatorIdentityHandle) &&
                                         HasToken(request.OriginAuthorizationHandle) &&
                                         HasToken(request.SiteBindingHandle) &&
                                         legalAgreementHandles.Count > 0;
        var identityFormationPresent = HasToken(request.IdentityFormationHandle) &&
                                       HasToken(request.OeHandle) &&
                                       HasToken(request.SelfGelHandle) &&
                                       HasToken(request.COeHandle) &&
                                       HasToken(request.CSelfGelHandle) &&
                                       HasToken(request.IdentityIntegrityHash);
        var firstPrimeStandingPresent = DetermineFirstPrimeStandingPresent(request.FirstPrimeReceipt);
        var domainAdmissionPresent = DetermineDomainAdmissionPresent(request.DomainAdmissionRecord);
        var operationalProvenancePresent = DetermineOperationalProvenancePresent(request.ServiceWitnessReceipt);
        var prohibitedClaimRequested = request.RuntimePersonaClaimed ||
                                       request.RoleEnactmentRequested ||
                                       request.ActionAuthorityRequested;
        var allPillarsPresent = originAuthorizationPresent &&
                                identityFormationPresent &&
                                firstPrimeStandingPresent &&
                                domainAdmissionPresent &&
                                operationalProvenancePresent;
        var bundleKind = DetermineBundleKind(allPillarsPresent, prohibitedClaimRequested);
        var disposition = DetermineDisposition(allPillarsPresent, prohibitedClaimRequested);
        var foundingRecognized = allPillarsPresent && !prohibitedClaimRequested;

        return new CmeMinimumLegalFoundingBundleReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            RequestHandle: request.RequestHandle.Trim(),
            BundleKind: bundleKind,
            Disposition: disposition,
            OperatorIdentityHandle: NormalizeHandle(request.OperatorIdentityHandle),
            OriginAuthorizationHandle: NormalizeHandle(request.OriginAuthorizationHandle),
            SiteBindingHandle: NormalizeHandle(request.SiteBindingHandle),
            LegalAgreementHandles: legalAgreementHandles,
            IdentityFormationHandle: NormalizeHandle(request.IdentityFormationHandle),
            OeHandle: NormalizeHandle(request.OeHandle),
            SelfGelHandle: NormalizeHandle(request.SelfGelHandle),
            COeHandle: NormalizeHandle(request.COeHandle),
            CSelfGelHandle: NormalizeHandle(request.CSelfGelHandle),
            IdentityIntegrityHash: NormalizeHandle(request.IdentityIntegrityHash),
            FirstPrimeReceiptHandle: request.FirstPrimeReceipt.ReceiptHandle,
            DomainAdmissionRecordHandle: request.DomainAdmissionRecord.RecordHandle,
            ServiceWitnessReceiptHandle: request.ServiceWitnessReceipt.ReceiptHandle,
            OriginAuthorizationFoundationPresent: originAuthorizationPresent,
            IdentityFormationRecordPresent: identityFormationPresent,
            FirstPrimeStandingProofPresent: firstPrimeStandingPresent,
            DomainRoleAdmissionRecordPresent: domainAdmissionPresent,
            OperationalProvenanceCustodyPresent: operationalProvenancePresent,
            FoundingBundleRecognized: foundingRecognized,
            CmeClaimLawfullyFounded: foundingRecognized,
            CmeMintingWithheld: true,
            RuntimePersonaWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            MotherFatherApplicationWithheld: true,
            CandidateOnly: true,
            ConstraintCodes: DetermineConstraintCodes(
                originAuthorizationPresent,
                identityFormationPresent,
                firstPrimeStandingPresent,
                domainAdmissionPresent,
                operationalProvenancePresent,
                foundingRecognized,
                request.RuntimePersonaClaimed,
                request.RoleEnactmentRequested,
                request.ActionAuthorityRequested),
            ReasonCode: DetermineReasonCode(
                originAuthorizationPresent,
                identityFormationPresent,
                firstPrimeStandingPresent,
                domainAdmissionPresent,
                operationalProvenancePresent,
                request.RuntimePersonaClaimed,
                request.RoleEnactmentRequested,
                request.ActionAuthorityRequested),
            LawfulBasis: DetermineLawfulBasis(bundleKind, disposition),
            TimestampUtc: MaxTimestamp(
                request.TimestampUtc,
                request.FirstPrimeReceipt.TimestampUtc,
                request.DomainAdmissionRecord.TimestampUtc,
                request.ServiceWitnessReceipt.TimestampUtc));
    }

    private static bool DetermineFirstPrimeStandingPresent(
        EngineeredCognitionFirstPrimeStateReceipt firstPrime)
    {
        return firstPrime.FirstPrimeState == EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding &&
               firstPrime.PrimeRetainedStandingReached &&
               firstPrime.StableOneSatisfied &&
               firstPrime.MotherFatherDomainRoleApplicationWithheld &&
               firstPrime.CradleLocalGoverningSurfaceWithheld &&
               firstPrime.PrimeClosureStillWithheld;
    }

    private static bool DetermineDomainAdmissionPresent(
        DomainAdmissionRecord domainAdmission)
    {
        return domainAdmission.Decision == DomainRoleAdmissionDecisionKind.Accept &&
               HasToken(domainAdmission.LegalFoundationHandle) &&
               domainAdmission.AcceptedDomainHandles.Count > 0 &&
               domainAdmission.AcceptedRoleHandles.Count > 0 &&
               domainAdmission.AuthorityScopeHandles.Count > 0 &&
               domainAdmission.ContinuityBurdenHandles.Count > 0 &&
               !domainAdmission.StandingOverwritten &&
               domainAdmission.MotherFatherOriginAuthorityWithheld &&
               domainAdmission.CradleLocalGoverningSurfaceStillWithheld &&
               domainAdmission.ImplicitDomainPromotionRefused;
    }

    private static bool DetermineOperationalProvenancePresent(
        PrivateDomainServiceWitnessReceipt serviceWitness)
    {
        return serviceWitness.WitnessKind == PrivateDomainServiceWitnessKind.PrivateDomainWitness &&
               serviceWitness.Disposition == PrivateDomainServiceOperationDisposition.Attested &&
               serviceWitness.RelationAttested &&
               serviceWitness.ActionExecutionWithheld &&
               serviceWitness.CradleLocalGovernanceEnactmentWithheld &&
               serviceWitness.CustodialMemoryOnly;
    }

    private static CmeMinimumLegalFoundingBundleKind DetermineBundleKind(
        bool allPillarsPresent,
        bool prohibitedClaimRequested)
    {
        if (prohibitedClaimRequested)
        {
            return CmeMinimumLegalFoundingBundleKind.BundleRefused;
        }

        return allPillarsPresent
            ? CmeMinimumLegalFoundingBundleKind.BundleRecognized
            : CmeMinimumLegalFoundingBundleKind.BundleDeferred;
    }

    private static CmeMinimumLegalFoundingBundleDispositionKind DetermineDisposition(
        bool allPillarsPresent,
        bool prohibitedClaimRequested)
    {
        if (prohibitedClaimRequested)
        {
            return CmeMinimumLegalFoundingBundleDispositionKind.Refused;
        }

        return allPillarsPresent
            ? CmeMinimumLegalFoundingBundleDispositionKind.Recognized
            : CmeMinimumLegalFoundingBundleDispositionKind.Deferred;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        bool originAuthorizationPresent,
        bool identityFormationPresent,
        bool firstPrimeStandingPresent,
        bool domainAdmissionPresent,
        bool operationalProvenancePresent,
        bool foundingRecognized,
        bool runtimePersonaClaimed,
        bool roleEnactmentRequested,
        bool actionAuthorityRequested)
    {
        var constraints = new List<string>
        {
            "cme-founding-bundle-origin-authorization-required",
            "cme-founding-bundle-identity-formation-required",
            "cme-founding-bundle-first-prime-standing-required",
            "cme-founding-bundle-domain-admission-required",
            "cme-founding-bundle-operational-provenance-required",
            "cme-founding-bundle-minting-withheld",
            "cme-founding-bundle-runtime-persona-withheld",
            "cme-founding-bundle-role-enactment-withheld",
            "cme-founding-bundle-action-authority-withheld"
        };

        AddMissingPillarConstraint(constraints, originAuthorizationPresent, "origin-authorization");
        AddMissingPillarConstraint(constraints, identityFormationPresent, "identity-formation");
        AddMissingPillarConstraint(constraints, firstPrimeStandingPresent, "first-prime-standing");
        AddMissingPillarConstraint(constraints, domainAdmissionPresent, "domain-admission");
        AddMissingPillarConstraint(constraints, operationalProvenancePresent, "operational-provenance");

        if (runtimePersonaClaimed)
        {
            constraints.Add("cme-founding-bundle-runtime-persona-claim-refused");
        }

        if (roleEnactmentRequested)
        {
            constraints.Add("cme-founding-bundle-role-enactment-refused");
        }

        if (actionAuthorityRequested)
        {
            constraints.Add("cme-founding-bundle-action-authority-refused");
        }

        constraints.Add(foundingRecognized
            ? "cme-founding-bundle-lawfully-founded"
            : "cme-founding-bundle-not-lawfully-founded");

        return constraints;
    }

    private static string DetermineReasonCode(
        bool originAuthorizationPresent,
        bool identityFormationPresent,
        bool firstPrimeStandingPresent,
        bool domainAdmissionPresent,
        bool operationalProvenancePresent,
        bool runtimePersonaClaimed,
        bool roleEnactmentRequested,
        bool actionAuthorityRequested)
    {
        if (runtimePersonaClaimed)
        {
            return "cme-founding-bundle-runtime-persona-claim-refused";
        }

        if (roleEnactmentRequested)
        {
            return "cme-founding-bundle-role-enactment-refused";
        }

        if (actionAuthorityRequested)
        {
            return "cme-founding-bundle-action-authority-refused";
        }

        if (!originAuthorizationPresent)
        {
            return "cme-founding-bundle-origin-authorization-incomplete";
        }

        if (!identityFormationPresent)
        {
            return "cme-founding-bundle-identity-formation-incomplete";
        }

        if (!firstPrimeStandingPresent)
        {
            return "cme-founding-bundle-first-prime-standing-incomplete";
        }

        if (!domainAdmissionPresent)
        {
            return "cme-founding-bundle-domain-admission-incomplete";
        }

        if (!operationalProvenancePresent)
        {
            return "cme-founding-bundle-operational-provenance-incomplete";
        }

        return "cme-founding-bundle-lawfully-founded";
    }

    private static string DetermineLawfulBasis(
        CmeMinimumLegalFoundingBundleKind bundleKind,
        CmeMinimumLegalFoundingBundleDispositionKind disposition)
    {
        if (bundleKind == CmeMinimumLegalFoundingBundleKind.BundleRecognized &&
            disposition == CmeMinimumLegalFoundingBundleDispositionKind.Recognized)
        {
            return "a CME claim may be legally founded only when origin authorization, identity formation, first Prime standing, domain admission, and operational provenance custody all stand together without minting, persona, role, or action-authority enactment.";
        }

        if (disposition == CmeMinimumLegalFoundingBundleDispositionKind.Refused)
        {
            return "minimum legal founding bundle must refuse any request that turns founding evidence into runtime persona, role enactment, or action authority.";
        }

        return "minimum legal founding bundle must defer until all five founding pillars are present and receipted.";
    }

    private static void AddMissingPillarConstraint(
        ICollection<string> constraints,
        bool pillarPresent,
        string pillarName)
    {
        if (!pillarPresent)
        {
            constraints.Add($"cme-founding-bundle-{pillarName}-pillar-missing");
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
        params DateTimeOffset[] timestamps)
    {
        return timestamps.Max();
    }
}
