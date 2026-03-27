using System.Security.Cryptography;
using System.Text;

namespace CradleTek.Memory;

public enum GovernedEngramSelfValidationPosture
{
    CooledValidated = 0,
    HotClaim = 1,
    Deferred = 2,
    Contradicted = 3
}

public enum GovernedEngramSelfResolutionOrigin
{
    HotWorkingResolution = 0,
    CooledValidationSurface = 1
}

public sealed record GovernedEngramSelfResolutionClaim(
    string ClaimHandle,
    string EngramId,
    string ConceptTag,
    string SummaryText,
    string DecisionSpline,
    string ProvenanceSource,
    double ConfidenceWeight,
    GovernedEngramSelfValidationPosture ValidationPosture,
    GovernedEngramSelfResolutionOrigin Origin,
    string? ValidationReferenceHandle,
    string? ObstructionCode);

public sealed record GovernedEngramSelfResolutionResult(
    string Source,
    IReadOnlyList<GovernedEngramSelfResolutionClaim> Claims);

public static class GovernedEngramSelfResolutionFactory
{
    public static GovernedEngramSelfResolutionResult CreateFallback(
        GovernedEngramQueryResult result,
        string validationReferenceHandle)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(validationReferenceHandle);
        var claims = result.Summaries
            .Select(summary => CreateClaim(
                summary,
                result.Source,
                GovernedEngramSelfValidationPosture.HotClaim,
                GovernedEngramSelfResolutionOrigin.HotWorkingResolution,
                validationReferenceHandle,
                obstructionCode: null))
            .ToArray();

        return new GovernedEngramSelfResolutionResult(
            Source: result.Source,
            Claims: claims);
    }

    public static GovernedEngramSelfResolutionClaim CreateClaim(
        GovernedEngramSummary summary,
        string provenanceSource,
        GovernedEngramSelfValidationPosture validationPosture,
        GovernedEngramSelfResolutionOrigin origin,
        string? validationReferenceHandle,
        string? obstructionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provenanceSource);

        if (validationPosture == GovernedEngramSelfValidationPosture.CooledValidated &&
            origin != GovernedEngramSelfResolutionOrigin.CooledValidationSurface)
        {
            throw new InvalidOperationException(
                "Hot working resolution may not promote a self claim into cooled validation.");
        }

        return new GovernedEngramSelfResolutionClaim(
            ClaimHandle: CreateClaimHandle(summary.EngramId, provenanceSource, validationPosture, origin),
            EngramId: summary.EngramId,
            ConceptTag: summary.ConceptTag,
            SummaryText: summary.SummaryText,
            DecisionSpline: summary.DecisionSpline,
            ProvenanceSource: provenanceSource.Trim(),
            ConfidenceWeight: summary.ConfidenceWeight,
            ValidationPosture: validationPosture,
            Origin: origin,
            ValidationReferenceHandle: validationReferenceHandle,
            ObstructionCode: obstructionCode);
    }

    private static string CreateClaimHandle(
        string engramId,
        string provenanceSource,
        GovernedEngramSelfValidationPosture validationPosture,
        GovernedEngramSelfResolutionOrigin origin)
    {
        var material = string.Join(
            "|",
            engramId.Trim(),
            provenanceSource.Trim(),
            validationPosture.ToString(),
            origin.ToString());
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"engram-self-claim://{Convert.ToHexString(bytes).ToLowerInvariant()[..16]}";
    }
}
