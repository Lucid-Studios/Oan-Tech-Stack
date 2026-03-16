using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using CradleTek.Memory.Models;

namespace AgentiCore.Models;

public enum AgentiSelfGelClaimPosture
{
    CooledValidated = 0,
    HotClaim = 1,
    Deferred = 2,
    Contradicted = 3
}

public enum AgentiSelfGelClaimOrigin
{
    HotWorkingResolution = 0,
    CooledValidationSurface = 1
}

public sealed record AgentiSelfGelValidationSurface(
    string SelfGelHandle,
    string Classification,
    string ValidationPosture,
    IReadOnlyList<string> ValidatedConcepts);

public sealed record AgentiSelfGelClaim(
    string ClaimHandle,
    string EngramId,
    string ConceptTag,
    string SummaryText,
    string ProvenanceSource,
    double ConfidenceWeight,
    AgentiSelfGelClaimPosture Posture,
    AgentiSelfGelClaimOrigin Origin,
    string? ValidationReferenceHandle,
    string? ObstructionCode,
    string Classification);

public sealed record AgentiSelfGelWorkingPool(
    string SessionHandle,
    string WorkingStateHandle,
    string ProvenanceMarker,
    string CSelfGelHandle,
    string Classification,
    IReadOnlyList<string> ActiveConcepts,
    IReadOnlyDictionary<string, string> WorkingMemory,
    AgentiSelfGelValidationSurface ValidationSurface,
    IReadOnlyList<AgentiSelfGelClaim> Claims);

public sealed record AgentiSymbolicTrace(
    string TraceId,
    string DecisionBranch,
    string SheafDomain,
    string Classification,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> Tokens);

public sealed record AgentiEngramCandidate(
    string Decision,
    bool CommitRequired,
    string ReturnCandidatePointer,
    string Classification,
    IReadOnlyList<string> EngramReferences,
    IReadOnlyList<string> ConstructorDomains);

public sealed record AgentiTransientResidue(
    string CleaveResidue,
    string HostedSemanticDecision,
    string Classification);

public static class AgentiSelfGelWorkingPoolFactory
{
    public static AgentiSelfGelWorkingPool Create(
        string sessionHandle,
        string workingStateHandle,
        string provenanceMarker,
        string cSelfGelHandle,
        IReadOnlyList<string> activeConcepts,
        IDictionary<string, string> workingMemory,
        IEnumerable<EngramSelfResolutionClaim>? selfClaims = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingStateHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenanceMarker);
        ArgumentException.ThrowIfNullOrWhiteSpace(cSelfGelHandle);
        ArgumentNullException.ThrowIfNull(activeConcepts);
        ArgumentNullException.ThrowIfNull(workingMemory);

        var validationSurface = CreateValidationSurface(cSelfGelHandle, activeConcepts);
        var mappedClaims = MapClaims(selfClaims, validationSurface);

        return new AgentiSelfGelWorkingPool(
            SessionHandle: sessionHandle,
            WorkingStateHandle: workingStateHandle,
            ProvenanceMarker: provenanceMarker,
            CSelfGelHandle: cSelfGelHandle,
            Classification: "bounded-selfgel-working-pool",
            ActiveConcepts: activeConcepts.ToArray(),
            WorkingMemory: new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(workingMemory, StringComparer.Ordinal)),
            ValidationSurface: validationSurface,
            Claims: mappedClaims);
    }

    private static AgentiSelfGelValidationSurface CreateValidationSurface(
        string cSelfGelHandle,
        IReadOnlyList<string> activeConcepts)
    {
        var validatedConcepts = activeConcepts
            .Where(IsSelfSensitiveConcept)
            .Append("identity-continuity")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new AgentiSelfGelValidationSurface(
            SelfGelHandle: EngramSelfResolutionFactory.CreateCooledValidationHandle(cSelfGelHandle),
            Classification: "cooled-selfgel-validation-surface",
            ValidationPosture: "validation-only",
            ValidatedConcepts: validatedConcepts);
    }

    private static IReadOnlyList<AgentiSelfGelClaim> MapClaims(
        IEnumerable<EngramSelfResolutionClaim>? selfClaims,
        AgentiSelfGelValidationSurface validationSurface)
    {
        if (selfClaims is null)
        {
            return Array.Empty<AgentiSelfGelClaim>();
        }

        return selfClaims
            .Select(claim => MapClaim(claim, validationSurface))
            .ToArray();
    }

    private static AgentiSelfGelClaim MapClaim(
        EngramSelfResolutionClaim claim,
        AgentiSelfGelValidationSurface validationSurface)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(validationSurface);

        if (claim.ValidationPosture == EngramSelfValidationPosture.CooledValidated &&
            claim.Origin != EngramSelfResolutionOrigin.CooledValidationSurface)
        {
            throw new InvalidOperationException(
                "Hot self claims may not be promoted directly into cooled validation truth.");
        }

        if (!string.IsNullOrWhiteSpace(claim.ValidationReferenceHandle) &&
            !string.Equals(claim.ValidationReferenceHandle, validationSurface.SelfGelHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Self claim validation references must point at the cooled SelfGEL validation surface.");
        }

        return new AgentiSelfGelClaim(
            ClaimHandle: string.IsNullOrWhiteSpace(claim.ClaimHandle)
                ? CreateClaimHandle(claim.EngramId, claim.ProvenanceSource, claim.ValidationPosture)
                : claim.ClaimHandle,
            EngramId: claim.EngramId,
            ConceptTag: claim.ConceptTag,
            SummaryText: claim.SummaryText,
            ProvenanceSource: claim.ProvenanceSource,
            ConfidenceWeight: claim.ConfidenceWeight,
            Posture: MapPosture(claim.ValidationPosture),
            Origin: MapOrigin(claim.Origin),
            ValidationReferenceHandle: claim.ValidationReferenceHandle,
            ObstructionCode: claim.ObstructionCode,
            Classification: BuildClassification(claim.ValidationPosture));
    }

    private static AgentiSelfGelClaimPosture MapPosture(EngramSelfValidationPosture posture) =>
        posture switch
        {
            EngramSelfValidationPosture.CooledValidated => AgentiSelfGelClaimPosture.CooledValidated,
            EngramSelfValidationPosture.HotClaim => AgentiSelfGelClaimPosture.HotClaim,
            EngramSelfValidationPosture.Deferred => AgentiSelfGelClaimPosture.Deferred,
            EngramSelfValidationPosture.Contradicted => AgentiSelfGelClaimPosture.Contradicted,
            _ => throw new ArgumentOutOfRangeException(nameof(posture), posture, "Unsupported self validation posture.")
        };

    private static AgentiSelfGelClaimOrigin MapOrigin(EngramSelfResolutionOrigin origin) =>
        origin switch
        {
            EngramSelfResolutionOrigin.HotWorkingResolution => AgentiSelfGelClaimOrigin.HotWorkingResolution,
            EngramSelfResolutionOrigin.CooledValidationSurface => AgentiSelfGelClaimOrigin.CooledValidationSurface,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, "Unsupported self resolution origin.")
        };

    private static bool IsSelfSensitiveConcept(string concept)
    {
        return concept.Contains("self", StringComparison.OrdinalIgnoreCase) ||
               concept.Contains("identity", StringComparison.OrdinalIgnoreCase) ||
               concept.Contains("continuity", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildClassification(EngramSelfValidationPosture posture) =>
        posture switch
        {
            EngramSelfValidationPosture.CooledValidated => "cooled-selfgel-validated-claim",
            EngramSelfValidationPosture.HotClaim => "hot-selfgel-claim",
            EngramSelfValidationPosture.Deferred => "deferred-selfgel-claim",
            EngramSelfValidationPosture.Contradicted => "contradicted-selfgel-claim",
            _ => "selfgel-claim"
        };

    private static string CreateClaimHandle(
        string engramId,
        string provenanceSource,
        EngramSelfValidationPosture posture)
    {
        var material = string.Join("|", engramId.Trim(), provenanceSource.Trim(), posture.ToString());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"selfgel-claim://{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
