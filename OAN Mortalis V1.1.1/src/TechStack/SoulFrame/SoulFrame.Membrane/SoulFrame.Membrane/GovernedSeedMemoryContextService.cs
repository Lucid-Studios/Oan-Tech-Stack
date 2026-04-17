using System.Security.Cryptography;
using System.Text;
using CradleTek.Mantle;
using CradleTek.Memory;
using San.Common;

namespace SoulFrame.Membrane;

public interface IGovernedSeedMemoryContextService
{
    Task<GovernedSeedMemoryContext> CreateContextAsync(
        GovernedSeedEvaluationRequest request,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        CancellationToken cancellationToken = default);
}

public sealed class GovernedSeedMemoryContextService : IGovernedSeedMemoryContextService
{
    private readonly IGovernedEngramResolver _engramResolver;
    private readonly IGovernedRootOntologicalCleaver _ontologicalCleaver;
    private readonly IGovernedSelfGelValidationHandleProjector _validationHandleProjector;

    public GovernedSeedMemoryContextService(
        IGovernedEngramResolver engramResolver,
        IGovernedRootOntologicalCleaver ontologicalCleaver,
        IGovernedSelfGelValidationHandleProjector validationHandleProjector)
    {
        _engramResolver = engramResolver ?? throw new ArgumentNullException(nameof(engramResolver));
        _ontologicalCleaver = ontologicalCleaver ?? throw new ArgumentNullException(nameof(ontologicalCleaver));
        _validationHandleProjector = validationHandleProjector ?? throw new ArgumentNullException(nameof(validationHandleProjector));
    }

    public async Task<GovernedSeedMemoryContext> CreateContextAsync(
        GovernedSeedEvaluationRequest request,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);

        var relevantFragments = CollectRelevantFragments(request.Input);
        var resolutionContext = new GovernedEngramResolutionContext(
            ContextHandle: CreateHandle("memory-resolution://", bootstrapReceipt.BootstrapHandle, request.AgentId, request.TheaterId),
            TaskObjective: request.Input,
            RelevantFragments: relevantFragments,
            SourceSubsystem: "soulframe-memory-mediation");
        var validationReferenceHandle = _validationHandleProjector.ProjectPresentedValidationHandle(
            bootstrapReceipt.CustodySnapshot.CrypticSelfGelHandle);
        var relevantEngrams = await _engramResolver
            .ResolveRelevantAsync(resolutionContext, cancellationToken)
            .ConfigureAwait(false);
        var selfResolution = await _engramResolver
            .ResolveSelfSensitiveAsync(resolutionContext, validationReferenceHandle, cancellationToken)
            .ConfigureAwait(false);
        var ontologicalResult = await _ontologicalCleaver
            .CleaveAsync(request.Input, cancellationToken)
            .ConfigureAwait(false);

        return new GovernedSeedMemoryContext(
            ContextHandle: CreateHandle(
                "memory-context://",
                bootstrapReceipt.BootstrapHandle,
                relevantEngrams.Source,
                ontologicalResult.CanonicalRootAtlas.Source,
                validationReferenceHandle),
            ContextProfile: "soulframe-mediated-memory-context",
            ResolverSource: relevantEngrams.Source,
            AtlasSource: ontologicalResult.CanonicalRootAtlas.Source,
            ValidationReferenceHandle: validationReferenceHandle,
            RelevantEngramIds: relevantEngrams.Summaries.Select(static summary => summary.EngramId).Take(4).ToArray(),
            RelevantConceptTags: relevantEngrams.Summaries.Select(static summary => summary.ConceptTag).Take(4).ToArray(),
            RootSymbolicIds: ontologicalResult.Known
                .Concat(ontologicalResult.PartiallyKnown)
                .Select(static root => root.SymbolicId)
                .Distinct(StringComparer.Ordinal)
                .Take(6)
                .ToArray(),
            UnknownRootCount: ontologicalResult.Unknown.Count,
            SelfResolutionDisposition: DetermineSelfResolutionDisposition(selfResolution),
            ContextStability: ontologicalResult.Metrics.ContextStability,
            ConceptDensity: ontologicalResult.Metrics.ConceptDensity,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static IReadOnlyList<string> CollectRelevantFragments(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var bulletItems = input
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(static line => line.Trim())
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Where(static line => line.StartsWith("-", StringComparison.Ordinal))
            .Select(static line => line[1..].Trim())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (bulletItems.Length > 0)
        {
            return bulletItems;
        }

        return input
            .Split(['.', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static fragment => fragment.Trim())
            .Where(static fragment => !string.IsNullOrWhiteSpace(fragment))
            .Take(6)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static fragment => fragment, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string DetermineSelfResolutionDisposition(GovernedEngramSelfResolutionResult selfResolution)
    {
        ArgumentNullException.ThrowIfNull(selfResolution);

        if (selfResolution.Claims.Any(static claim => claim.ValidationPosture == GovernedEngramSelfValidationPosture.Contradicted))
        {
            return "contradicted";
        }

        if (selfResolution.Claims.Any(static claim => claim.ValidationPosture == GovernedEngramSelfValidationPosture.HotClaim))
        {
            return "hot-claim";
        }

        if (selfResolution.Claims.Any(static claim => claim.ValidationPosture == GovernedEngramSelfValidationPosture.CooledValidated))
        {
            return "cooled-validated";
        }

        return "deferred";
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
