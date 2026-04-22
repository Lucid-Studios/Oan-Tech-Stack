namespace San.Common;

public enum DomainRoleAdmissionEligibilityKind
{
    Admissible = 0,
    Inadmissible = 1,
    InsufficientInformation = 2
}

public enum DomainRoleAdmissionDecisionKind
{
    Accept = 0,
    Defer = 1,
    Refuse = 2
}

public sealed record DomainOffer(
    string OfferHandle,
    string SourceAuthorityHandle,
    string LegalFoundationHandle,
    IReadOnlyList<string> LegalPredicateHandles,
    IReadOnlyList<string> CandidateDomainHandles,
    IReadOnlyList<string> CandidateRoleHandles,
    string DeclaredIntent,
    IReadOnlyList<string> AuthorityScopeHandles,
    IReadOnlyList<string> ExplicitExclusionHandles,
    IReadOnlyList<string> ContinuityBurdenHandles,
    IReadOnlyList<string> RevocationConditionHandles,
    bool OriginAuthorityClaimed,
    DateTimeOffset TimestampUtc);

public sealed record DomainEligibilityAssessment(
    string AssessmentHandle,
    string OfferHandle,
    string FirstPrimeReceiptHandle,
    DomainRoleAdmissionEligibilityKind Eligibility,
    bool LegalFoundationPresent,
    bool AuthorityScopeDeclared,
    bool RefusalPathPreserved,
    bool ContinuityBurdenDeclared,
    bool RequiresUnlawfulIdentityClaim,
    bool ViolatesPrimeInvariants,
    bool CollapsesIncompleteness,
    bool RequiresUnseatedCapabilities,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public sealed record DomainAdmissionRecord(
    string RecordHandle,
    string OfferHandle,
    string AssessmentHandle,
    DomainRoleAdmissionDecisionKind Decision,
    string LegalFoundationHandle,
    IReadOnlyList<string> AcceptedDomainHandles,
    IReadOnlyList<string> AcceptedRoleHandles,
    IReadOnlyList<string> AuthorityScopeHandles,
    IReadOnlyList<string> ExplicitExclusionHandles,
    IReadOnlyList<string> ContinuityBurdenHandles,
    IReadOnlyList<string> RevocationConditionHandles,
    bool StandingOverwritten,
    bool MotherFatherOriginAuthorityWithheld,
    bool CradleLocalGoverningSurfaceStillWithheld,
    bool ImplicitDomainPromotionRefused,
    bool ReceiptRequiredForAllOutcomes,
    bool CandidateOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class DomainRoleAdmissionEvaluator
{
    public static DomainEligibilityAssessment Assess(
        EngineeredCognitionFirstPrimeStateReceipt firstPrime,
        DomainOffer offer,
        string assessmentHandle)
    {
        ArgumentNullException.ThrowIfNull(firstPrime);
        ArgumentNullException.ThrowIfNull(offer);

        if (string.IsNullOrWhiteSpace(assessmentHandle))
        {
            throw new ArgumentException("Assessment handle must be provided.", nameof(assessmentHandle));
        }

        var legalFoundationPresent = HasToken(offer.LegalFoundationHandle) &&
                                     NormalizeTokens(offer.LegalPredicateHandles).Count > 0;
        var authorityScopeDeclared = NormalizeTokens(offer.AuthorityScopeHandles).Count > 0 &&
                                     NormalizeTokens(offer.ExplicitExclusionHandles).Count > 0;
        var refusalPathPreserved = NormalizeTokens(offer.RevocationConditionHandles).Count > 0;
        var continuityBurdenDeclared = NormalizeTokens(offer.ContinuityBurdenHandles).Count > 0;
        var requiresUnlawfulIdentityClaim = offer.OriginAuthorityClaimed;
        var violatesPrimeInvariants = !firstPrime.PrimeClosureStillWithheld ||
                                      !firstPrime.MotherFatherDomainRoleApplicationWithheld ||
                                      !firstPrime.CradleLocalGoverningSurfaceWithheld;
        var collapsesIncompleteness = !firstPrime.StableOneSatisfied ||
                                      !firstPrime.PrimeRetainedStandingReached;
        var requiresUnseatedCapabilities = NormalizeTokens(offer.CandidateDomainHandles).Count == 0 ||
                                           NormalizeTokens(offer.CandidateRoleHandles).Count == 0;
        var eligibility = DetermineEligibility(
            firstPrime,
            legalFoundationPresent,
            authorityScopeDeclared,
            refusalPathPreserved,
            continuityBurdenDeclared,
            requiresUnlawfulIdentityClaim,
            violatesPrimeInvariants,
            collapsesIncompleteness,
            requiresUnseatedCapabilities);

        return new DomainEligibilityAssessment(
            AssessmentHandle: assessmentHandle,
            OfferHandle: offer.OfferHandle,
            FirstPrimeReceiptHandle: firstPrime.ReceiptHandle,
            Eligibility: eligibility,
            LegalFoundationPresent: legalFoundationPresent,
            AuthorityScopeDeclared: authorityScopeDeclared,
            RefusalPathPreserved: refusalPathPreserved,
            ContinuityBurdenDeclared: continuityBurdenDeclared,
            RequiresUnlawfulIdentityClaim: requiresUnlawfulIdentityClaim,
            ViolatesPrimeInvariants: violatesPrimeInvariants,
            CollapsesIncompleteness: collapsesIncompleteness,
            RequiresUnseatedCapabilities: requiresUnseatedCapabilities,
            ConstraintCodes: DetermineAssessmentConstraints(
                firstPrime,
                eligibility,
                legalFoundationPresent,
                authorityScopeDeclared,
                refusalPathPreserved,
                continuityBurdenDeclared,
                requiresUnlawfulIdentityClaim,
                violatesPrimeInvariants,
                collapsesIncompleteness,
                requiresUnseatedCapabilities),
            ReasonCode: DetermineAssessmentReason(
                firstPrime,
                legalFoundationPresent,
                authorityScopeDeclared,
                refusalPathPreserved,
                continuityBurdenDeclared,
                requiresUnlawfulIdentityClaim,
                violatesPrimeInvariants,
                collapsesIncompleteness,
                requiresUnseatedCapabilities,
                eligibility),
            LawfulBasis: DetermineAssessmentLawfulBasis(eligibility),
            TimestampUtc: MaxTimestamp(firstPrime.TimestampUtc, offer.TimestampUtc));
    }

    public static DomainAdmissionRecord Decide(
        DomainOffer offer,
        DomainEligibilityAssessment assessment,
        DomainRoleAdmissionDecisionKind requestedDecision,
        string recordHandle)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(assessment);

        if (string.IsNullOrWhiteSpace(recordHandle))
        {
            throw new ArgumentException("Record handle must be provided.", nameof(recordHandle));
        }

        var decision = DetermineDecision(assessment, requestedDecision);
        var accepted = decision == DomainRoleAdmissionDecisionKind.Accept;

        return new DomainAdmissionRecord(
            RecordHandle: recordHandle,
            OfferHandle: offer.OfferHandle,
            AssessmentHandle: assessment.AssessmentHandle,
            Decision: decision,
            LegalFoundationHandle: accepted ? offer.LegalFoundationHandle : string.Empty,
            AcceptedDomainHandles: accepted ? NormalizeTokens(offer.CandidateDomainHandles) : [],
            AcceptedRoleHandles: accepted ? NormalizeTokens(offer.CandidateRoleHandles) : [],
            AuthorityScopeHandles: accepted ? NormalizeTokens(offer.AuthorityScopeHandles) : [],
            ExplicitExclusionHandles: accepted ? NormalizeTokens(offer.ExplicitExclusionHandles) : [],
            ContinuityBurdenHandles: accepted ? NormalizeTokens(offer.ContinuityBurdenHandles) : [],
            RevocationConditionHandles: NormalizeTokens(offer.RevocationConditionHandles),
            StandingOverwritten: false,
            MotherFatherOriginAuthorityWithheld: true,
            CradleLocalGoverningSurfaceStillWithheld: true,
            ImplicitDomainPromotionRefused: true,
            ReceiptRequiredForAllOutcomes: true,
            CandidateOnly: true,
            ConstraintCodes: DetermineRecordConstraints(assessment, decision, requestedDecision),
            ReasonCode: DetermineRecordReason(assessment, decision, requestedDecision),
            LawfulBasis: DetermineRecordLawfulBasis(decision),
            TimestampUtc: MaxTimestamp(offer.TimestampUtc, assessment.TimestampUtc));
    }

    public static DomainRoleAdmissionEligibilityKind DetermineEligibility(
        EngineeredCognitionFirstPrimeStateReceipt firstPrime,
        bool legalFoundationPresent,
        bool authorityScopeDeclared,
        bool refusalPathPreserved,
        bool continuityBurdenDeclared,
        bool requiresUnlawfulIdentityClaim,
        bool violatesPrimeInvariants,
        bool collapsesIncompleteness,
        bool requiresUnseatedCapabilities)
    {
        ArgumentNullException.ThrowIfNull(firstPrime);

        if (firstPrime.FirstPrimeState != EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding ||
            violatesPrimeInvariants ||
            collapsesIncompleteness ||
            requiresUnlawfulIdentityClaim)
        {
            return DomainRoleAdmissionEligibilityKind.Inadmissible;
        }

        if (!legalFoundationPresent ||
            !authorityScopeDeclared ||
            !refusalPathPreserved ||
            !continuityBurdenDeclared ||
            requiresUnseatedCapabilities)
        {
            return DomainRoleAdmissionEligibilityKind.InsufficientInformation;
        }

        return DomainRoleAdmissionEligibilityKind.Admissible;
    }

    private static DomainRoleAdmissionDecisionKind DetermineDecision(
        DomainEligibilityAssessment assessment,
        DomainRoleAdmissionDecisionKind requestedDecision)
    {
        return assessment.Eligibility switch
        {
            DomainRoleAdmissionEligibilityKind.Admissible => requestedDecision,
            DomainRoleAdmissionEligibilityKind.InsufficientInformation => DomainRoleAdmissionDecisionKind.Defer,
            _ => DomainRoleAdmissionDecisionKind.Refuse
        };
    }

    private static IReadOnlyList<string> DetermineAssessmentConstraints(
        EngineeredCognitionFirstPrimeStateReceipt firstPrime,
        DomainRoleAdmissionEligibilityKind eligibility,
        bool legalFoundationPresent,
        bool authorityScopeDeclared,
        bool refusalPathPreserved,
        bool continuityBurdenDeclared,
        bool requiresUnlawfulIdentityClaim,
        bool violatesPrimeInvariants,
        bool collapsesIncompleteness,
        bool requiresUnseatedCapabilities)
    {
        var constraints = new List<string>
        {
            "domain-role-admission-legal-foundation-required",
            "domain-role-admission-no-silent-domain-promotion",
            "domain-role-admission-domain-not-origin",
            "domain-role-admission-domain-not-governance-root",
            "domain-role-admission-refusal-path-required",
            "domain-role-admission-continuity-burden-required"
        };

        constraints.Add(eligibility switch
        {
            DomainRoleAdmissionEligibilityKind.Admissible => "domain-role-admission-eligible",
            DomainRoleAdmissionEligibilityKind.InsufficientInformation => "domain-role-admission-insufficient-information",
            _ => "domain-role-admission-inadmissible"
        });

        if (firstPrime.FirstPrimeState != EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding)
        {
            constraints.Add("domain-role-admission-first-prime-pre-role-standing-required");
        }

        if (!legalFoundationPresent)
        {
            constraints.Add("domain-role-admission-legal-foundation-missing");
        }

        if (!authorityScopeDeclared)
        {
            constraints.Add("domain-role-admission-authority-scope-incomplete");
        }

        if (!refusalPathPreserved)
        {
            constraints.Add("domain-role-admission-refusal-path-missing");
        }

        if (!continuityBurdenDeclared)
        {
            constraints.Add("domain-role-admission-continuity-burden-missing");
        }

        if (requiresUnlawfulIdentityClaim)
        {
            constraints.Add("domain-role-admission-origin-authority-claim-refused");
        }

        if (violatesPrimeInvariants)
        {
            constraints.Add("domain-role-admission-prime-invariant-violation");
        }

        if (collapsesIncompleteness)
        {
            constraints.Add("domain-role-admission-incompleteness-collapse-refused");
        }

        if (requiresUnseatedCapabilities)
        {
            constraints.Add("domain-role-admission-unseated-capability-requirement");
        }

        return constraints;
    }

    private static string DetermineAssessmentReason(
        EngineeredCognitionFirstPrimeStateReceipt firstPrime,
        bool legalFoundationPresent,
        bool authorityScopeDeclared,
        bool refusalPathPreserved,
        bool continuityBurdenDeclared,
        bool requiresUnlawfulIdentityClaim,
        bool violatesPrimeInvariants,
        bool collapsesIncompleteness,
        bool requiresUnseatedCapabilities,
        DomainRoleAdmissionEligibilityKind eligibility)
    {
        if (firstPrime.FirstPrimeState != EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding)
        {
            return "domain-role-admission-first-prime-pre-role-standing-required";
        }

        if (requiresUnlawfulIdentityClaim)
        {
            return "domain-role-admission-origin-authority-claim-refused";
        }

        if (violatesPrimeInvariants)
        {
            return "domain-role-admission-prime-invariant-violation";
        }

        if (collapsesIncompleteness)
        {
            return "domain-role-admission-incompleteness-collapse-refused";
        }

        if (!legalFoundationPresent)
        {
            return "domain-role-admission-legal-foundation-missing";
        }

        if (!authorityScopeDeclared)
        {
            return "domain-role-admission-authority-scope-incomplete";
        }

        if (!refusalPathPreserved)
        {
            return "domain-role-admission-refusal-path-missing";
        }

        if (!continuityBurdenDeclared)
        {
            return "domain-role-admission-continuity-burden-missing";
        }

        if (requiresUnseatedCapabilities)
        {
            return "domain-role-admission-unseated-capability-requirement";
        }

        return eligibility == DomainRoleAdmissionEligibilityKind.Admissible
            ? "domain-role-admission-eligible"
            : "domain-role-admission-insufficient-information";
    }

    private static IReadOnlyList<string> DetermineRecordConstraints(
        DomainEligibilityAssessment assessment,
        DomainRoleAdmissionDecisionKind decision,
        DomainRoleAdmissionDecisionKind requestedDecision)
    {
        var constraints = new List<string>
        {
            "domain-role-admission-record-required",
            "domain-role-admission-standing-not-overwritten",
            "domain-role-admission-mother-father-origin-authority-withheld",
            "domain-role-admission-cradle-local-governing-surface-withheld",
            "domain-role-admission-implicit-domain-promotion-refused"
        };

        constraints.Add(decision switch
        {
            DomainRoleAdmissionDecisionKind.Accept => "domain-role-admission-accepted",
            DomainRoleAdmissionDecisionKind.Refuse => "domain-role-admission-refused",
            _ => "domain-role-admission-deferred"
        });

        if (decision != requestedDecision)
        {
            constraints.Add("domain-role-admission-requested-decision-overridden-by-law");
        }

        if (assessment.Eligibility != DomainRoleAdmissionEligibilityKind.Admissible)
        {
            constraints.Add("domain-role-admission-non-admissible-assessment");
        }

        return constraints;
    }

    private static string DetermineRecordReason(
        DomainEligibilityAssessment assessment,
        DomainRoleAdmissionDecisionKind decision,
        DomainRoleAdmissionDecisionKind requestedDecision)
    {
        if (decision != requestedDecision)
        {
            return "domain-role-admission-requested-decision-overridden-by-law";
        }

        return decision switch
        {
            DomainRoleAdmissionDecisionKind.Accept => "domain-role-admission-accepted",
            DomainRoleAdmissionDecisionKind.Refuse => "domain-role-admission-refused",
            _ => assessment.ReasonCode
        };
    }

    private static string DetermineAssessmentLawfulBasis(
        DomainRoleAdmissionEligibilityKind eligibility)
    {
        return eligibility switch
        {
            DomainRoleAdmissionEligibilityKind.Admissible =>
                "domain and role admission may proceed only because legal foundation, authority scope, refusal path, and continuity burden are declared against first Prime pre-role standing.",
            DomainRoleAdmissionEligibilityKind.Inadmissible =>
                "domain and role admission must be refused when it violates first Prime standing, claims origin authority, collapses incompleteness, or overwrites Prime invariants.",
            _ =>
                "domain and role admission must be deferred until legal foundation, authority scope, refusal path, continuity burden, and capability posture are explicit."
        };
    }

    private static string DetermineRecordLawfulBasis(
        DomainRoleAdmissionDecisionKind decision)
    {
        return decision switch
        {
            DomainRoleAdmissionDecisionKind.Accept =>
                "domain and role may be accepted as bounded operational jurisdiction without overwriting standing, claiming origin authority, or selecting the cradle-local governing surface.",
            DomainRoleAdmissionDecisionKind.Refuse =>
                "domain and role must be refused when admission would lack lawful foundation or violate first Prime pre-role standing.",
            _ =>
                "domain and role must be deferred when admission remains under-specified while preserving refusal and continuity."
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

    private static bool HasToken(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static DateTimeOffset MaxTimestamp(
        DateTimeOffset first,
        DateTimeOffset second) =>
        first >= second ? first : second;
}
