namespace GEL.Contracts.Sanctuary;

public enum SanctuarySelfGelLegalOrientationPredicateKind
{
    GoverningBodySeated = 0,
    JurisdictionSeated = 1,
    EntityLineageValid = 2,
    GovernorBound = 3,
    LawfulOperatingSurface = 4
}

public enum SanctuaryLegalEvidenceKind
{
    LegalName = 0,
    StateRegistrationSurface = 1,
    Jurisdiction = 2,
    Ubi = 3,
    Ein = 4,
    FilingReceipt = 5,
    IrsNotice = 6,
    GovernorSurface = 7,
    EntityForm = 8,
    ReportingPosture = 9
}

public sealed record SanctuarySelfGelLegalOrientationPredicateDefinition
{
    public SanctuarySelfGelLegalOrientationPredicateDefinition(
        SanctuarySelfGelLegalOrientationPredicateKind predicate,
        string handle,
        string meaning,
        IReadOnlyList<SanctuaryLegalEvidenceKind> requiredEvidenceKinds,
        IReadOnlyList<string> preservedInvariants,
        IReadOnlyList<string> failureStates,
        string operationalStatus)
    {
        Predicate = predicate;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
        RequiredEvidenceKinds = SanctuaryContractGuard.RequiredDistinctList(requiredEvidenceKinds, nameof(requiredEvidenceKinds));
        PreservedInvariants = SanctuaryContractGuard.RequiredTextList(preservedInvariants, nameof(preservedInvariants));
        FailureStates = SanctuaryContractGuard.RequiredTextList(failureStates, nameof(failureStates));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuarySelfGelLegalOrientationPredicateKind Predicate { get; }

    public string Handle { get; }

    public string Meaning { get; }

    public IReadOnlyList<SanctuaryLegalEvidenceKind> RequiredEvidenceKinds { get; }

    public IReadOnlyList<string> PreservedInvariants { get; }

    public IReadOnlyList<string> FailureStates { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryLegalEvidencePoolDefinition
{
    public SanctuaryLegalEvidencePoolDefinition(
        string handle,
        IReadOnlyList<SanctuaryLegalEvidenceKind> evidenceFields,
        string evidenceMixtureClass,
        string storagePolicy,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        EvidenceFields = SanctuaryContractGuard.RequiredDistinctList(evidenceFields, nameof(evidenceFields));
        EvidenceMixtureClass = SanctuaryContractGuard.RequiredText(evidenceMixtureClass, nameof(evidenceMixtureClass));
        StoragePolicy = SanctuaryContractGuard.RequiredText(storagePolicy, nameof(storagePolicy));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryLegalEvidenceKind> EvidenceFields { get; }

    public string EvidenceMixtureClass { get; }

    public string StoragePolicy { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryLegalOrientationInstallPacketDefinition
{
    public SanctuaryLegalOrientationInstallPacketDefinition(
        string handle,
        IReadOnlyList<string> evidencePoolFieldNames,
        IReadOnlyList<SanctuarySelfGelLegalOrientationPredicateKind> rootPredicateFamily,
        IReadOnlyList<string> deferredBranchFamilies,
        string projectionBoundary,
        string storagePolicy,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        EvidencePoolFieldNames = SanctuaryContractGuard.RequiredTextList(evidencePoolFieldNames, nameof(evidencePoolFieldNames));
        RootPredicateFamily = SanctuaryContractGuard.RequiredDistinctList(rootPredicateFamily, nameof(rootPredicateFamily));
        DeferredBranchFamilies = SanctuaryContractGuard.RequiredTextList(deferredBranchFamilies, nameof(deferredBranchFamilies));
        ProjectionBoundary = SanctuaryContractGuard.RequiredText(projectionBoundary, nameof(projectionBoundary));
        StoragePolicy = SanctuaryContractGuard.RequiredText(storagePolicy, nameof(storagePolicy));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> EvidencePoolFieldNames { get; }

    public IReadOnlyList<SanctuarySelfGelLegalOrientationPredicateKind> RootPredicateFamily { get; }

    public IReadOnlyList<string> DeferredBranchFamilies { get; }

    public string ProjectionBoundary { get; }

    public string StoragePolicy { get; }

    public string OperationalStatus { get; }
}

public static class SanctuarySelfGelLegalOrientationAtlas
{
    private static readonly IReadOnlyList<string> SharedPreservedInvariants =
    [
        "legal-body-continuity",
        "jurisdictional-seat",
        "governor-lineage",
        "lawful-authority-boundary"
    ];

    private static readonly IReadOnlyDictionary<SanctuarySelfGelLegalOrientationPredicateKind, SanctuarySelfGelLegalOrientationPredicateDefinition> Definitions =
        new Dictionary<SanctuarySelfGelLegalOrientationPredicateKind, SanctuarySelfGelLegalOrientationPredicateDefinition>
        {
            [SanctuarySelfGelLegalOrientationPredicateKind.GoverningBodySeated] = new(
                predicate: SanctuarySelfGelLegalOrientationPredicateKind.GoverningBodySeated,
                handle: "selfgel.governing-body-seated",
                meaning: "cme-presides-under-a-specific-legal-body",
                requiredEvidenceKinds:
                [
                    SanctuaryLegalEvidenceKind.LegalName,
                    SanctuaryLegalEvidenceKind.StateRegistrationSurface,
                    SanctuaryLegalEvidenceKind.FilingReceipt
                ],
                preservedInvariants: SharedPreservedInvariants,
                failureStates:
                [
                    "legal-body-unbound",
                    "registration-surface-missing",
                    "entity-seat-ambiguous"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuarySelfGelLegalOrientationPredicateKind.JurisdictionSeated] = new(
                predicate: SanctuarySelfGelLegalOrientationPredicateKind.JurisdictionSeated,
                handle: "selfgel.jurisdiction-seated",
                meaning: "cme-is-civilly-grounded-in-a-specific-jurisdiction",
                requiredEvidenceKinds:
                [
                    SanctuaryLegalEvidenceKind.Jurisdiction,
                    SanctuaryLegalEvidenceKind.StateRegistrationSurface,
                    SanctuaryLegalEvidenceKind.Ubi,
                    SanctuaryLegalEvidenceKind.FilingReceipt
                ],
                preservedInvariants: SharedPreservedInvariants,
                failureStates:
                [
                    "jurisdiction-unbound",
                    "regional-seat-missing",
                    "jurisdiction-conflict"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid] = new(
                predicate: SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid,
                handle: "selfgel.entity-lineage-valid",
                meaning: "legal-continuity-holds-across-filings-and-records",
                requiredEvidenceKinds:
                [
                    SanctuaryLegalEvidenceKind.LegalName,
                    SanctuaryLegalEvidenceKind.Ubi,
                    SanctuaryLegalEvidenceKind.Ein,
                    SanctuaryLegalEvidenceKind.FilingReceipt,
                    SanctuaryLegalEvidenceKind.IrsNotice
                ],
                preservedInvariants: SharedPreservedInvariants,
                failureStates:
                [
                    "lineage-break",
                    "identifier-mismatch",
                    "receipt-chain-incomplete"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuarySelfGelLegalOrientationPredicateKind.GovernorBound] = new(
                predicate: SanctuarySelfGelLegalOrientationPredicateKind.GovernorBound,
                handle: "selfgel.governor-bound",
                meaning: "governing-cme-is-tied-to-an-explicit-governing-surface",
                requiredEvidenceKinds:
                [
                    SanctuaryLegalEvidenceKind.GovernorSurface,
                    SanctuaryLegalEvidenceKind.FilingReceipt
                ],
                preservedInvariants: SharedPreservedInvariants,
                failureStates:
                [
                    "governor-surface-missing",
                    "governance-role-ambiguous",
                    "unbound-governance-claim"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuarySelfGelLegalOrientationPredicateKind.LawfulOperatingSurface] = new(
                predicate: SanctuarySelfGelLegalOrientationPredicateKind.LawfulOperatingSurface,
                handle: "selfgel.lawful-operating-surface",
                meaning: "operating-authority-is-constrained-by-actual-entity-form-and-reporting-posture",
                requiredEvidenceKinds:
                [
                    SanctuaryLegalEvidenceKind.EntityForm,
                    SanctuaryLegalEvidenceKind.ReportingPosture,
                    SanctuaryLegalEvidenceKind.Jurisdiction
                ],
                preservedInvariants: SharedPreservedInvariants,
                failureStates:
                [
                    "entity-form-unbound",
                    "reporting-posture-unclear",
                    "authority-surface-overclaimed"
                ],
                operationalStatus: "placeholder-contract-only")
        };

    public static IReadOnlyList<SanctuarySelfGelLegalOrientationPredicateDefinition> All { get; } =
        Definitions.Values
            .OrderBy(static item => item.Predicate)
            .ToArray();

    public static SanctuaryLegalEvidencePoolDefinition LegalEvidencePool { get; } =
        new(
            handle: "selfgel.legal-evidence-pool.v0",
            evidenceFields:
            [
                SanctuaryLegalEvidenceKind.LegalName,
                SanctuaryLegalEvidenceKind.StateRegistrationSurface,
                SanctuaryLegalEvidenceKind.Jurisdiction,
                SanctuaryLegalEvidenceKind.Ubi,
                SanctuaryLegalEvidenceKind.Ein,
                SanctuaryLegalEvidenceKind.FilingReceipt,
                SanctuaryLegalEvidenceKind.IrsNotice,
                SanctuaryLegalEvidenceKind.GovernorSurface,
                SanctuaryLegalEvidenceKind.EntityForm,
                SanctuaryLegalEvidenceKind.ReportingPosture
            ],
            evidenceMixtureClass: "mixed-legal-surfaces-local-only",
            storagePolicy: "organization-controlled-local-only-packet",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryLegalOrientationInstallPacketDefinition InstallPacket { get; } =
        new(
            handle: "selfgel.legal-orientation-install.packet.v0",
            evidencePoolFieldNames:
            [
                "legal_name",
                "state_registration_surface",
                "jurisdiction",
                "ubi",
                "ein",
                "filing_receipt_handles",
                "irs_notice_handle",
                "governor_surface",
                "entity_form",
                "reporting_posture",
                "evidence_mixture_class"
            ],
            rootPredicateFamily:
            [
                SanctuarySelfGelLegalOrientationPredicateKind.GoverningBodySeated,
                SanctuarySelfGelLegalOrientationPredicateKind.JurisdictionSeated,
                SanctuarySelfGelLegalOrientationPredicateKind.EntityLineageValid,
                SanctuarySelfGelLegalOrientationPredicateKind.GovernorBound,
                SanctuarySelfGelLegalOrientationPredicateKind.LawfulOperatingSurface
            ],
            deferredBranchFamilies:
            [
                "charitable-trust-seat",
                "bonded-operator-seat",
                "b-corp-transition-seat",
                "authority-delegation",
                "cryptographic-custody"
            ],
            projectionBoundary: "gel-owned-root-family-first-run-bridge-doctrine-only",
            storagePolicy: "tracked-template-with-ignored-local-packet",
            operationalStatus: "placeholder-contract-only");

    public static bool TryGet(
        SanctuarySelfGelLegalOrientationPredicateKind predicate,
        out SanctuarySelfGelLegalOrientationPredicateDefinition definition)
    {
        return Definitions.TryGetValue(predicate, out definition!);
    }

    public static SanctuarySelfGelLegalOrientationPredicateDefinition Get(
        SanctuarySelfGelLegalOrientationPredicateKind predicate)
    {
        if (!TryGet(predicate, out var definition))
        {
            throw new KeyNotFoundException($"No SelfGEL legal orientation predicate exists for '{predicate}'.");
        }

        return definition;
    }
}
