using System.Security.Cryptography;
using System.Text;

namespace CradleTek.Memory.Models;

public enum EngramSelfValidationPosture
{
    CooledValidated = 0,
    HotClaim = 1,
    Deferred = 2,
    Contradicted = 3
}

public enum EngramSelfResolutionOrigin
{
    HotWorkingResolution = 0,
    CooledValidationSurface = 1
}

public sealed class EngramSelfResolutionClaim
{
    public required string ClaimHandle { get; init; }
    public required string EngramId { get; init; }
    public required string ConceptTag { get; init; }
    public required string SummaryText { get; init; }
    public required string DecisionSpline { get; init; }
    public required string ProvenanceSource { get; init; }
    public required double ConfidenceWeight { get; init; }
    public required EngramSelfValidationPosture ValidationPosture { get; init; }
    public required EngramSelfResolutionOrigin Origin { get; init; }
    public string? ValidationReferenceHandle { get; init; }
    public string? ObstructionCode { get; init; }
}

public sealed class EngramSelfResolutionResult
{
    public required string Source { get; init; }
    public required IReadOnlyList<EngramSelfResolutionClaim> Claims { get; init; }
}

public static class EngramSelfResolutionFactory
{
    public static EngramSelfResolutionResult CreateFallback(
        EngramQueryResult result,
        string cSelfGelHandle)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(cSelfGelHandle);

        var validationReferenceHandle = CreateCooledValidationHandle(cSelfGelHandle);
        var claims = result.Summaries
            .Select(summary => CreateClaim(
                summary,
                result.Source,
                EngramSelfValidationPosture.HotClaim,
                EngramSelfResolutionOrigin.HotWorkingResolution,
                validationReferenceHandle,
                obstructionCode: null))
            .ToArray();

        return new EngramSelfResolutionResult
        {
            Source = result.Source,
            Claims = claims
        };
    }

    public static EngramSelfResolutionClaim CreateClaim(
        EngramSummary summary,
        string provenanceSource,
        EngramSelfValidationPosture validationPosture,
        EngramSelfResolutionOrigin origin,
        string? validationReferenceHandle,
        string? obstructionCode)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(provenanceSource);

        if (validationPosture == EngramSelfValidationPosture.CooledValidated &&
            origin != EngramSelfResolutionOrigin.CooledValidationSurface)
        {
            throw new InvalidOperationException(
                "Hot working resolution may not promote a self claim into cooled validation.");
        }

        return new EngramSelfResolutionClaim
        {
            ClaimHandle = CreateClaimHandle(summary.EngramId, provenanceSource, validationPosture, origin),
            EngramId = summary.EngramId,
            ConceptTag = summary.ConceptTag,
            SummaryText = summary.SummaryText,
            DecisionSpline = summary.DecisionSpline,
            ProvenanceSource = provenanceSource.Trim(),
            ConfidenceWeight = summary.ConfidenceWeight,
            ValidationPosture = validationPosture,
            Origin = origin,
            ValidationReferenceHandle = validationReferenceHandle,
            ObstructionCode = obstructionCode
        };
    }

    public static string CreateCooledValidationHandle(string cSelfGelHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cSelfGelHandle);

        if (cSelfGelHandle.StartsWith("soulframe-cselfgel://", StringComparison.Ordinal))
        {
            return "soulframe-selfgel://" + cSelfGelHandle["soulframe-cselfgel://".Length..];
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(cSelfGelHandle.Trim()));
        return $"soulframe-selfgel://derived/{Convert.ToHexString(bytes).ToLowerInvariant()[..16]}";
    }

    private static string CreateClaimHandle(
        string engramId,
        string provenanceSource,
        EngramSelfValidationPosture validationPosture,
        EngramSelfResolutionOrigin origin)
    {
        var material = string.Join(
            "|",
            engramId.Trim(),
            provenanceSource.Trim(),
            validationPosture.ToString(),
            origin.ToString());
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"selfgel-claim://{Convert.ToHexString(bytes).ToLowerInvariant()[..16]}";
    }
}
