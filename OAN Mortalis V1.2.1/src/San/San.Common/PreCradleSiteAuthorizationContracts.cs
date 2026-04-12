namespace San.Common;

public enum CradleTekSiteClass
{
    PersonalPc = 0,
    CoResidentSanctuaryHost = 1,
    SeparateCradleTekHost = 2,
    EnterpriseDistributedConstruct = 3
}

public enum ToolAuthorizationState
{
    Authorized = 0,
    Withheld = 1,
    Refused = 2
}

public sealed record CradleTekSiteBindingProfile(
    string SiteBindingProfileId,
    CradleTekSiteClass SiteClass,
    string SiteSummary,
    string SanctuaryHostFooting,
    string CradleTekHostFooting,
    string JurisdictionProfile,
    string LocalizationProfile,
    IReadOnlyList<string> WitnessRefs);

public sealed record OperatorTrainingAdmissionRecord(
    string AdmissionId,
    string OperatorRef,
    string TrainingSetRef,
    string AdmissionStanding,
    string SiteBindingProfileRef,
    IReadOnlyList<string> WitnessRefs);

public sealed record ToolStateAuthorizationRecord(
    string AuthorizationId,
    string ToolSurfaceName,
    ToolAuthorizationState AuthorizationState,
    string SiteBindingProfileRef,
    string OperatorRef,
    IReadOnlyList<string> WitnessRefs);

public sealed record FinalCmeDisclosureAgreementBundle(
    string BundleId,
    string SiteBindingProfileRef,
    string OperatorRef,
    IReadOnlyList<string> DisclosureLineageRefs,
    bool FullDisclosureAssent,
    IReadOnlyList<string> WitnessRefs);

public sealed record InheritedIpScopeRecord(
    string ScopeId,
    IReadOnlyList<string> ContentAssetRefs,
    string RightsHolderRef,
    string SubjectRef,
    string AuthorityBasis,
    string InheritanceScope,
    string GuardianRef,
    string BrandRef,
    string CampaignRef,
    string RevocationPosture,
    IReadOnlyList<string> WitnessRefs);

public sealed record CreationUseScopeRecord(
    string ScopeId,
    IReadOnlyList<string> ContentAssetRefs,
    string RightsHolderRef,
    string SubjectRef,
    string AuthorityBasis,
    string CreationScope,
    string GuardianRef,
    string BrandRef,
    string CampaignRef,
    string RevocationPosture,
    IReadOnlyList<string> WitnessRefs);

public sealed record CustodialAuthorityRecord(
    string AuthorityId,
    string GuardianRef,
    string SubjectRef,
    string AuthorityBasis,
    string SiteBindingProfileRef,
    string RevocationPosture,
    IReadOnlyList<string> WitnessRefs);

public sealed record BrandAuthorityRecord(
    string AuthorityId,
    string BrandPrincipalRef,
    string BrandRef,
    string CampaignRef,
    string AuthorityBasis,
    string SiteBindingProfileRef,
    string RevocationPosture,
    IReadOnlyList<string> WitnessRefs);

public sealed record CmeContentAuthorityBundle(
    string BundleId,
    IReadOnlyList<InheritedIpScopeRecord> InheritedIpScopes,
    IReadOnlyList<CreationUseScopeRecord> CreationUseScopes,
    IReadOnlyList<CustodialAuthorityRecord> CustodialAuthorities,
    IReadOnlyList<BrandAuthorityRecord> BrandAuthorities,
    IReadOnlyList<string> WitnessRefs);

public sealed record PreCradleSiteAuthorizationCandidate(
    string CandidateId,
    string InstallIdentitySetRef,
    string CoreCmeUsePostureRef,
    string RtmeServiceLiftPreconditionSnapshotRef,
    string MosStorageSeatRef,
    string CMosSurfaceRef,
    string SiteBindingProfileRef,
    string OperatorRef,
    string OperatorTrainingAdmissionRef,
    IReadOnlyList<string> ToolAuthorizationRefs,
    string FinalCmeDisclosureBundleRef,
    string CmeContentAuthorityBundleRef,
    string NonGrantSummary,
    IReadOnlyList<string> WitnessRefs);

public sealed record PreCradleAuthorizationRefusalReason(
    string ReasonCode,
    string Summary,
    bool RequiresExplicitReceipt);

public sealed record PreCradleAuthorizationReceipt(
    string ReceiptId,
    string CandidateRef,
    string SiteBindingProfileRef,
    string OperatorRef,
    string StandingPosture,
    string NonGrantSummary,
    IReadOnlyList<string> WitnessRefs,
    DateTimeOffset Timestamp);
