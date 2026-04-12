using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SLI.Lisp;

public sealed record InstallAgreementActionSurfaceRequest(
    LocalizedInstallChoiceMatrix ChoiceMatrix);

public sealed record InstallAgreementActionSurfaceResult(
    AgreementPredicateBundle AgreementBundle,
    InstallIdentitySetCandidate? IdentityCandidate,
    CoreCmeUsePostureRecord CmeUsePosture,
    bool FullAssent);

public interface IInstallAgreementActionSurface
{
    InstallAgreementActionSurfaceResult Evaluate(InstallAgreementActionSurfaceRequest request);
}

public sealed class GovernedInstallAgreementActionSurface : IInstallAgreementActionSurface
{
    private const string ResearchAttachedDefault = "research-attached-default";

    public InstallAgreementActionSurfaceResult Evaluate(InstallAgreementActionSurfaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ChoiceMatrix);

        var bundle = BuildAgreementBundle(request.ChoiceMatrix);
        var identityCandidate = bundle.FullAssent
            ? BuildIdentitySetCandidate(request.ChoiceMatrix, bundle)
            : null;
        var posture = BuildPostureRecord(request.ChoiceMatrix);

        return new InstallAgreementActionSurfaceResult(
            AgreementBundle: bundle,
            IdentityCandidate: identityCandidate,
            CmeUsePosture: posture,
            FullAssent: bundle.FullAssent);
    }

    private static AgreementPredicateBundle BuildAgreementBundle(LocalizedInstallChoiceMatrix matrix)
    {
        var predicates = Enum.GetValues<AgreementPredicateKind>()
            .Select(kind => CreatePredicateRecord(matrix, kind))
            .ToArray();

        var fullAssent = predicates.All(static predicate => predicate.AssentState == AgreementAssentState.Assented);
        var bundleId = CreateHandle(
            "install-agreement-bundle://",
            matrix.ChoiceMatrixId,
            matrix.LicensingAgentId,
            matrix.UserId,
            matrix.LanguageDataset.DatasetId,
            string.Join("|", predicates.Select(static predicate => $"{predicate.PredicateLane}:{predicate.AssentState}")));

        return new AgreementPredicateBundle(
            BundleId: bundleId,
            ChoiceMatrixId: matrix.ChoiceMatrixId,
            LicensingAgentId: matrix.LicensingAgentId,
            UserId: matrix.UserId,
            LanguageDatasetId: matrix.LanguageDataset.DatasetId,
            Locale: matrix.LanguageDataset.Locale,
            Jurisdiction: matrix.LanguageDataset.Jurisdiction,
            AgreementTemplateLineage: InstallAgreementGuard.RequiredTextList(
                matrix.AgreementTemplateLineage,
                nameof(matrix.AgreementTemplateLineage)),
            Predicates: predicates,
            FullAssent: fullAssent);
    }

    private static AgreementPredicateRecord CreatePredicateRecord(
        LocalizedInstallChoiceMatrix matrix,
        AgreementPredicateKind predicateKind)
    {
        var templateRef = matrix.AgreementTemplateRefs.TryGetValue(predicateKind, out var configuredTemplateRef)
            ? configuredTemplateRef
            : InstallAgreementGuard.DefaultTemplateRef(predicateKind);
        var assentState = matrix.AgreementAssentStates.TryGetValue(predicateKind, out var configuredAssentState)
            ? configuredAssentState
            : AgreementAssentState.Withheld;
        var witnessRefs = matrix.AgreementWitnessRefs.TryGetValue(predicateKind, out var configuredWitnessRefs)
            ? InstallAgreementGuard.RequiredTextList(configuredWitnessRefs, nameof(matrix.AgreementWitnessRefs))
            : Array.Empty<string>();

        return new AgreementPredicateRecord(
            PredicateKind: predicateKind,
            PredicateLane: InstallAgreementGuard.ToLaneName(predicateKind),
            AssentState: assentState,
            TemplateRef: InstallAgreementGuard.RequiredText(templateRef, nameof(templateRef)),
            WitnessRefs: witnessRefs,
            FormationTrace: "localized-choice-matrix-formed");
    }

    private static InstallIdentitySetCandidate BuildIdentitySetCandidate(
        LocalizedInstallChoiceMatrix matrix,
        AgreementPredicateBundle bundle)
    {
        var assentWitnessByLane = bundle.Predicates.ToDictionary(
            static predicate => predicate.PredicateLane,
            static predicate => predicate.WitnessRefs.FirstOrDefault() ?? "assent-witness-missing",
            StringComparer.OrdinalIgnoreCase);
        var cmeLabNoticeLineageRef = bundle.Predicates
            .First(static predicate => predicate.PredicateKind == AgreementPredicateKind.CmeLabNoticePredicate)
            .TemplateRef;
        var identitySetId = CreateHandle(
            "install-identity-set://",
            bundle.BundleId,
            matrix.LanguageDataset.DatasetId,
            matrix.LanguageDataset.Locale,
            matrix.LanguageDataset.Jurisdiction,
            string.Join("|", assentWitnessByLane.OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(static pair => $"{pair.Key}:{pair.Value}")));

        return new InstallIdentitySetCandidate(
            IdentitySetId: identitySetId,
            BundleId: bundle.BundleId,
            LicensingAgentId: matrix.LicensingAgentId,
            UserId: matrix.UserId,
            LanguageDatasetId: matrix.LanguageDataset.DatasetId,
            Locale: matrix.LanguageDataset.Locale,
            Jurisdiction: matrix.LanguageDataset.Jurisdiction,
            AssentWitnessByLane: assentWitnessByLane,
            AgreementTemplateLineage: bundle.AgreementTemplateLineage,
            CmeLabNoticeLineageRef: cmeLabNoticeLineageRef);
    }

    private static CoreCmeUsePostureRecord BuildPostureRecord(LocalizedInstallChoiceMatrix matrix)
    {
        var postureId = CreateHandle(
            "core-cme-use-posture://",
            matrix.ChoiceMatrixId,
            matrix.LanguageDataset.DatasetId,
            matrix.LanguageDataset.Locale,
            matrix.LanguageDataset.Jurisdiction,
            ResearchAttachedDefault);

        return new CoreCmeUsePostureRecord(
            PostureId: postureId,
            AttachmentProfile: ResearchAttachedDefault,
            CmeLabProductStanding: "cme-lab-product",
            CmeLabTestStanding: "engineered-cognition-test-standing",
            DataPracticePosture: "agency-plus-research-context",
            LanguageDatasetId: matrix.LanguageDataset.DatasetId,
            ActiveLanguage: matrix.LanguageDataset.ActiveLanguage,
            Locale: matrix.LanguageDataset.Locale,
            Jurisdiction: matrix.LanguageDataset.Jurisdiction,
            AgreementLineageRefs: InstallAgreementGuard.RequiredTextList(
                matrix.AgreementTemplateLineage,
                nameof(matrix.AgreementTemplateLineage)));
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}

internal static class InstallAgreementGuard
{
    public static string RequiredText(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    public static IReadOnlyList<string> RequiredTextList(
        IReadOnlyList<string> values,
        string parameterName)
    {
        var normalized = (values ?? throw new ArgumentNullException(parameterName))
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return normalized;
    }

    public static string ToLaneName(AgreementPredicateKind predicateKind) => predicateKind switch
    {
        AgreementPredicateKind.ServiceLicensePredicate => "service-license-predicate",
        AgreementPredicateKind.TermsOfServicePredicate => "terms-of-service-predicate",
        AgreementPredicateKind.BondedOperatorPredicate => "bonded-operator-predicate",
        AgreementPredicateKind.CmeLabNoticePredicate => "cme-lab-notice-predicate",
        AgreementPredicateKind.ResearchDataPracticePredicate => "research-data-practice-predicate",
        AgreementPredicateKind.AccessAttachmentProfilePredicate => "access-attachment-profile-predicate",
        _ => throw new ArgumentOutOfRangeException(nameof(predicateKind), predicateKind, "Unknown agreement predicate kind.")
    };

    public static string DefaultTemplateRef(AgreementPredicateKind predicateKind) =>
        $"{ToLaneName(predicateKind)}-template";
}
