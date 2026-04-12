namespace San.Common;

public enum AgreementPredicateKind
{
    ServiceLicensePredicate = 0,
    TermsOfServicePredicate = 1,
    BondedOperatorPredicate = 2,
    CmeLabNoticePredicate = 3,
    ResearchDataPracticePredicate = 4,
    AccessAttachmentProfilePredicate = 5
}

public enum AgreementAssentState
{
    Presented = 0,
    Acknowledged = 1,
    Assented = 2,
    Refused = 3,
    Withheld = 4
}

public sealed record LanguageSelectionDataset(
    string DatasetId,
    string ActiveLanguage,
    string Locale,
    string Jurisdiction,
    string LegalFormFamily,
    IReadOnlyList<string> NamingConventions,
    IReadOnlyList<string> ScriptExpectations,
    IReadOnlyList<string> ReservedPredicateBindings,
    IReadOnlyList<string> LocalLexemeAllowances,
    IReadOnlyList<string> UnresolvedLexemeLanes,
    string DatasetPosture);

public sealed record LocalizedFootingSection(
    string FootingSectionId,
    string WhoRef,
    string WhatRef,
    string WhenRef,
    string WhereRef,
    string WhyRef,
    string HowRef,
    string BiographicalScope,
    string LocalScope,
    string GovernmentalScope,
    string LegalPosture,
    string LocalizationAuthorizationState);

public sealed record LocalizedInstallChoiceMatrix(
    string ChoiceMatrixId,
    LanguageSelectionDataset LanguageDataset,
    LocalizedFootingSection FootingSection,
    string LicensingAgentId,
    string UserId,
    IReadOnlyList<string> AgreementTemplateLineage,
    IReadOnlyDictionary<AgreementPredicateKind, string> AgreementTemplateRefs,
    IReadOnlyDictionary<AgreementPredicateKind, AgreementAssentState> AgreementAssentStates,
    IReadOnlyDictionary<AgreementPredicateKind, IReadOnlyList<string>> AgreementWitnessRefs);

public sealed record AgreementPredicateRecord(
    AgreementPredicateKind PredicateKind,
    string PredicateLane,
    AgreementAssentState AssentState,
    string TemplateRef,
    IReadOnlyList<string> WitnessRefs,
    string FormationTrace);

public sealed record AgreementPredicateBundle(
    string BundleId,
    string ChoiceMatrixId,
    string LicensingAgentId,
    string UserId,
    string LanguageDatasetId,
    string Locale,
    string Jurisdiction,
    IReadOnlyList<string> AgreementTemplateLineage,
    IReadOnlyList<AgreementPredicateRecord> Predicates,
    bool FullAssent);

public sealed record InstallIdentitySetCandidate(
    string IdentitySetId,
    string BundleId,
    string LicensingAgentId,
    string UserId,
    string LanguageDatasetId,
    string Locale,
    string Jurisdiction,
    IReadOnlyDictionary<string, string> AssentWitnessByLane,
    IReadOnlyList<string> AgreementTemplateLineage,
    string CmeLabNoticeLineageRef);

public sealed record CoreCmeUsePostureRecord(
    string PostureId,
    string AttachmentProfile,
    string CmeLabProductStanding,
    string CmeLabTestStanding,
    string DataPracticePosture,
    string LanguageDatasetId,
    string ActiveLanguage,
    string Locale,
    string Jurisdiction,
    IReadOnlyList<string> AgreementLineageRefs);
