namespace San.Common;

public enum ConstructorEngramTierKind
{
    Root = 0,
    Primitive = 1,
    Basic = 2,
    LowerIntermediate = 3,
    UpperIntermediate = 4,
    Advanced = 5,
    ResearchFormation = 6,
    Cathedral = 7
}

public enum ConstructorEngramBurdenMovementKind
{
    WithinTier = 0,
    PromotionCandidate = 1,
    SplitRequired = 2,
    ClusterRequired = 3,
    DeferRequired = 4,
    PruneRequired = 5,
    NonPromotable = 6,
    Refused = 7
}

public sealed record ConstructorEngramBurdenAssessmentRequest(
    string AssessmentHandle,
    string EngramHandle,
    ConstructorEngramTierKind RequestedTier,
    bool PromotionRequested,
    int BranchCount,
    int ClusterCount,
    int DependencyDepth,
    decimal CrossLinkDensity,
    decimal ReceiptCoverage,
    decimal ReceiptFreshnessCoverage,
    int OperatorLoad,
    int CrossClusterLinkCount,
    int CompressedClaimCount,
    bool ReceiptIntegrityHolds,
    IReadOnlyList<string> ClusterBoundaryHandles,
    IReadOnlyList<string> RefusalPathHandles,
    IReadOnlyList<string> CrossClusterTransportReceiptHandles,
    IReadOnlyList<string> DecompressionPathHandles,
    DateTimeOffset TimestampUtc);

public sealed record ConstructorEngramTierBudget(
    ConstructorEngramTierKind Tier,
    int MinBranches,
    int? MaxBranches,
    int MinClusters,
    int? MaxClusters,
    int MinDepth,
    int? MaxDepth,
    decimal MinCrossLinkDensity,
    decimal? MaxCrossLinkDensity,
    decimal MinReceiptCoverage,
    decimal MinReceiptFreshnessCoverage,
    int? MaxOperatorLoad);

public sealed record ConstructorEngramBurdenAssessmentReceipt(
    string ReceiptHandle,
    string AssessmentHandle,
    string EngramHandle,
    ConstructorEngramTierKind RequestedTier,
    ConstructorEngramTierKind ComputedTier,
    ConstructorEngramBurdenMovementKind MovementKind,
    int BranchCount,
    int ClusterCount,
    int DependencyDepth,
    decimal CrossLinkDensity,
    decimal ReceiptCoverage,
    decimal ReceiptFreshnessCoverage,
    int OperatorLoad,
    int CrossClusterLinkCount,
    int CompressedClaimCount,
    decimal NearSwampIndex,
    bool ScoresWithinRange,
    bool RequestedTierEarned,
    bool BurdenVectorWithinTierBounds,
    bool ReceiptIntegrityHolds,
    bool ReceiptCoverageWithinBound,
    bool ReceiptFreshnessWithinBound,
    bool ClusterBoundariesExplicit,
    bool RefusalPathsIntact,
    bool CrossClusterTransportReceipted,
    bool CompressedClaimsDecompressible,
    bool PromotionAdmissible,
    bool DemotionOrThinningRequired,
    bool NearSwampDetected,
    bool CandidateOnly,
    IReadOnlyList<string> ClusterBoundaryHandles,
    IReadOnlyList<string> RefusalPathHandles,
    IReadOnlyList<string> CrossClusterTransportReceiptHandles,
    IReadOnlyList<string> DecompressionPathHandles,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class ConstructorEngramBurdenEvaluator
{
    private static readonly ConstructorEngramTierBudget[] Budgets =
    [
        new(
            Tier: ConstructorEngramTierKind.Root,
            MinBranches: 1,
            MaxBranches: 1,
            MinClusters: 0,
            MaxClusters: 1,
            MinDepth: 0,
            MaxDepth: 1,
            MinCrossLinkDensity: 0.0m,
            MaxCrossLinkDensity: 2.0m,
            MinReceiptCoverage: 0.99m,
            MinReceiptFreshnessCoverage: 0.99m,
            MaxOperatorLoad: 1),
        new(
            Tier: ConstructorEngramTierKind.Primitive,
            MinBranches: 2,
            MaxBranches: 7,
            MinClusters: 1,
            MaxClusters: 1,
            MinDepth: 1,
            MaxDepth: 2,
            MinCrossLinkDensity: 1.0m,
            MaxCrossLinkDensity: 4.0m,
            MinReceiptCoverage: 0.95m,
            MinReceiptFreshnessCoverage: 0.95m,
            MaxOperatorLoad: 7),
        new(
            Tier: ConstructorEngramTierKind.Basic,
            MinBranches: 8,
            MaxBranches: 32,
            MinClusters: 1,
            MaxClusters: 4,
            MinDepth: 2,
            MaxDepth: 4,
            MinCrossLinkDensity: 2.0m,
            MaxCrossLinkDensity: 8.0m,
            MinReceiptCoverage: 0.90m,
            MinReceiptFreshnessCoverage: 0.90m,
            MaxOperatorLoad: 32),
        new(
            Tier: ConstructorEngramTierKind.LowerIntermediate,
            MinBranches: 33,
            MaxBranches: 128,
            MinClusters: 4,
            MaxClusters: 12,
            MinDepth: 3,
            MaxDepth: 6,
            MinCrossLinkDensity: 4.0m,
            MaxCrossLinkDensity: 16.0m,
            MinReceiptCoverage: 0.85m,
            MinReceiptFreshnessCoverage: 0.85m,
            MaxOperatorLoad: 128),
        new(
            Tier: ConstructorEngramTierKind.UpperIntermediate,
            MinBranches: 129,
            MaxBranches: 512,
            MinClusters: 8,
            MaxClusters: 32,
            MinDepth: 5,
            MaxDepth: 9,
            MinCrossLinkDensity: 8.0m,
            MaxCrossLinkDensity: 32.0m,
            MinReceiptCoverage: 0.80m,
            MinReceiptFreshnessCoverage: 0.80m,
            MaxOperatorLoad: 512),
        new(
            Tier: ConstructorEngramTierKind.Advanced,
            MinBranches: 513,
            MaxBranches: 2048,
            MinClusters: 16,
            MaxClusters: 64,
            MinDepth: 8,
            MaxDepth: 14,
            MinCrossLinkDensity: 16.0m,
            MaxCrossLinkDensity: 64.0m,
            MinReceiptCoverage: 0.75m,
            MinReceiptFreshnessCoverage: 0.75m,
            MaxOperatorLoad: 2048),
        new(
            Tier: ConstructorEngramTierKind.ResearchFormation,
            MinBranches: 2049,
            MaxBranches: 8192,
            MinClusters: 32,
            MaxClusters: 128,
            MinDepth: 12,
            MaxDepth: 21,
            MinCrossLinkDensity: 32.0m,
            MaxCrossLinkDensity: 128.0m,
            MinReceiptCoverage: 0.70m,
            MinReceiptFreshnessCoverage: 0.70m,
            MaxOperatorLoad: 8192),
        new(
            Tier: ConstructorEngramTierKind.Cathedral,
            MinBranches: 8193,
            MaxBranches: null,
            MinClusters: 128,
            MaxClusters: null,
            MinDepth: 21,
            MaxDepth: null,
            MinCrossLinkDensity: 64.0m,
            MaxCrossLinkDensity: null,
            MinReceiptCoverage: 0.65m,
            MinReceiptFreshnessCoverage: 0.65m,
            MaxOperatorLoad: null)
    ];

    public static ConstructorEngramBurdenAssessmentReceipt Evaluate(
        ConstructorEngramBurdenAssessmentRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.AssessmentHandle))
        {
            throw new ArgumentException("Assessment handle must be provided.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.EngramHandle))
        {
            throw new ArgumentException("Engram handle must be provided.", nameof(request));
        }

        var computedTier = DetermineTier(request.BranchCount);
        var budget = GetBudget(computedTier);
        var clusterBoundaryHandles = NormalizeTokens(request.ClusterBoundaryHandles);
        var refusalPathHandles = NormalizeTokens(request.RefusalPathHandles);
        var crossClusterTransportReceiptHandles = NormalizeTokens(request.CrossClusterTransportReceiptHandles);
        var decompressionPathHandles = NormalizeTokens(request.DecompressionPathHandles);
        var scoresWithinRange = DetermineScoresWithinRange(request);
        var requestedTierEarned = request.RequestedTier == computedTier;
        var receiptCoverageWithinBound = request.ReceiptCoverage >= budget.MinReceiptCoverage;
        var receiptFreshnessWithinBound = request.ReceiptFreshnessCoverage >= budget.MinReceiptFreshnessCoverage;
        var clusterBoundariesExplicit = clusterBoundaryHandles.Count > 0;
        var refusalPathsIntact = refusalPathHandles.Count > 0;
        var crossClusterTransportReceipted = request.CrossClusterLinkCount == 0 ||
                                            crossClusterTransportReceiptHandles.Count > 0;
        var compressedClaimsDecompressible = request.CompressedClaimCount == 0 ||
                                             decompressionPathHandles.Count > 0;
        var burdenVectorWithinTierBounds = scoresWithinRange &&
                                           requestedTierEarned &&
                                           BranchCountWithinBudget(request.BranchCount, budget) &&
                                           ClusterCountWithinBudget(request.ClusterCount, budget) &&
                                           DepthWithinBudget(request.DependencyDepth, budget) &&
                                           CrossLinkDensityWithinBudget(request.CrossLinkDensity, budget) &&
                                           receiptCoverageWithinBound &&
                                           receiptFreshnessWithinBound &&
                                           OperatorLoadWithinBudget(request.OperatorLoad, budget);
        var promotionAdmissible = request.PromotionRequested &&
                                  burdenVectorWithinTierBounds &&
                                  request.ReceiptIntegrityHolds &&
                                  clusterBoundariesExplicit &&
                                  refusalPathsIntact &&
                                  crossClusterTransportReceipted &&
                                  compressedClaimsDecompressible;
        var nearSwampIndex = DetermineNearSwampIndex(request, budget);
        var nearSwampDetected = nearSwampIndex >= 0.75m;
        var movementKind = DetermineMovementKind(
            request,
            budget,
            scoresWithinRange,
            requestedTierEarned,
            burdenVectorWithinTierBounds,
            receiptCoverageWithinBound,
            receiptFreshnessWithinBound,
            clusterBoundariesExplicit,
            refusalPathsIntact,
            crossClusterTransportReceipted,
            compressedClaimsDecompressible,
            promotionAdmissible);
        var demotionOrThinningRequired = movementKind is
            ConstructorEngramBurdenMovementKind.SplitRequired or
            ConstructorEngramBurdenMovementKind.ClusterRequired or
            ConstructorEngramBurdenMovementKind.DeferRequired or
            ConstructorEngramBurdenMovementKind.PruneRequired or
            ConstructorEngramBurdenMovementKind.NonPromotable;

        return new ConstructorEngramBurdenAssessmentReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            AssessmentHandle: request.AssessmentHandle.Trim(),
            EngramHandle: request.EngramHandle.Trim(),
            RequestedTier: request.RequestedTier,
            ComputedTier: computedTier,
            MovementKind: movementKind,
            BranchCount: request.BranchCount,
            ClusterCount: request.ClusterCount,
            DependencyDepth: request.DependencyDepth,
            CrossLinkDensity: request.CrossLinkDensity,
            ReceiptCoverage: request.ReceiptCoverage,
            ReceiptFreshnessCoverage: request.ReceiptFreshnessCoverage,
            OperatorLoad: request.OperatorLoad,
            CrossClusterLinkCount: request.CrossClusterLinkCount,
            CompressedClaimCount: request.CompressedClaimCount,
            NearSwampIndex: nearSwampIndex,
            ScoresWithinRange: scoresWithinRange,
            RequestedTierEarned: requestedTierEarned,
            BurdenVectorWithinTierBounds: burdenVectorWithinTierBounds,
            ReceiptIntegrityHolds: request.ReceiptIntegrityHolds,
            ReceiptCoverageWithinBound: receiptCoverageWithinBound,
            ReceiptFreshnessWithinBound: receiptFreshnessWithinBound,
            ClusterBoundariesExplicit: clusterBoundariesExplicit,
            RefusalPathsIntact: refusalPathsIntact,
            CrossClusterTransportReceipted: crossClusterTransportReceipted,
            CompressedClaimsDecompressible: compressedClaimsDecompressible,
            PromotionAdmissible: promotionAdmissible,
            DemotionOrThinningRequired: demotionOrThinningRequired,
            NearSwampDetected: nearSwampDetected,
            CandidateOnly: true,
            ClusterBoundaryHandles: clusterBoundaryHandles,
            RefusalPathHandles: refusalPathHandles,
            CrossClusterTransportReceiptHandles: crossClusterTransportReceiptHandles,
            DecompressionPathHandles: decompressionPathHandles,
            ConstraintCodes: DetermineConstraintCodes(
                request,
                movementKind,
                scoresWithinRange,
                requestedTierEarned,
                burdenVectorWithinTierBounds,
                request.ReceiptIntegrityHolds,
                receiptCoverageWithinBound,
                receiptFreshnessWithinBound,
                clusterBoundariesExplicit,
                refusalPathsIntact,
                crossClusterTransportReceipted,
                compressedClaimsDecompressible,
                nearSwampDetected),
            ReasonCode: DetermineReasonCode(
                request,
                budget,
                movementKind,
                scoresWithinRange,
                requestedTierEarned,
                request.ReceiptIntegrityHolds,
                receiptCoverageWithinBound,
                receiptFreshnessWithinBound,
                clusterBoundariesExplicit,
                refusalPathsIntact,
                crossClusterTransportReceipted,
                compressedClaimsDecompressible),
            LawfulBasis: DetermineLawfulBasis(movementKind),
            TimestampUtc: request.TimestampUtc);
    }

    public static ConstructorEngramTierKind DetermineTier(int branchCount)
    {
        if (branchCount <= 0)
        {
            return ConstructorEngramTierKind.Root;
        }

        foreach (var budget in Budgets)
        {
            if (BranchCountWithinBudget(branchCount, budget))
            {
                return budget.Tier;
            }
        }

        return ConstructorEngramTierKind.Cathedral;
    }

    public static ConstructorEngramTierBudget GetBudget(ConstructorEngramTierKind tier)
        => Budgets.Single(budget => budget.Tier == tier);

    private static ConstructorEngramBurdenMovementKind DetermineMovementKind(
        ConstructorEngramBurdenAssessmentRequest request,
        ConstructorEngramTierBudget budget,
        bool scoresWithinRange,
        bool requestedTierEarned,
        bool burdenVectorWithinTierBounds,
        bool receiptCoverageWithinBound,
        bool receiptFreshnessWithinBound,
        bool clusterBoundariesExplicit,
        bool refusalPathsIntact,
        bool crossClusterTransportReceipted,
        bool compressedClaimsDecompressible,
        bool promotionAdmissible)
    {
        if (!scoresWithinRange)
        {
            return ConstructorEngramBurdenMovementKind.Refused;
        }

        if (!requestedTierEarned && request.PromotionRequested)
        {
            return ConstructorEngramBurdenMovementKind.DeferRequired;
        }

        if (!compressedClaimsDecompressible)
        {
            return ConstructorEngramBurdenMovementKind.NonPromotable;
        }

        if (!request.ReceiptIntegrityHolds ||
            !receiptCoverageWithinBound ||
            !receiptFreshnessWithinBound)
        {
            return ConstructorEngramBurdenMovementKind.PruneRequired;
        }

        if (!clusterBoundariesExplicit ||
            request.ClusterCount < budget.MinClusters)
        {
            return ConstructorEngramBurdenMovementKind.SplitRequired;
        }

        if (!ClusterCountWithinBudget(request.ClusterCount, budget) ||
            !CrossLinkDensityWithinBudget(request.CrossLinkDensity, budget) ||
            !OperatorLoadWithinBudget(request.OperatorLoad, budget))
        {
            return ConstructorEngramBurdenMovementKind.ClusterRequired;
        }

        if (!DepthWithinBudget(request.DependencyDepth, budget) ||
            !refusalPathsIntact ||
            !crossClusterTransportReceipted)
        {
            return ConstructorEngramBurdenMovementKind.DeferRequired;
        }

        if (promotionAdmissible)
        {
            return ConstructorEngramBurdenMovementKind.PromotionCandidate;
        }

        return burdenVectorWithinTierBounds
            ? ConstructorEngramBurdenMovementKind.WithinTier
            : ConstructorEngramBurdenMovementKind.DeferRequired;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        ConstructorEngramBurdenAssessmentRequest request,
        ConstructorEngramBurdenMovementKind movementKind,
        bool scoresWithinRange,
        bool requestedTierEarned,
        bool burdenVectorWithinTierBounds,
        bool receiptIntegrityHolds,
        bool receiptCoverageWithinBound,
        bool receiptFreshnessWithinBound,
        bool clusterBoundariesExplicit,
        bool refusalPathsIntact,
        bool crossClusterTransportReceipted,
        bool compressedClaimsDecompressible,
        bool nearSwampDetected)
    {
        var constraints = new List<string>
        {
            "constructor-engram-burden-vector-required",
            "constructor-engram-tier-computed-not-asserted",
            "constructor-engram-cluster-boundaries-required",
            "constructor-engram-refusal-paths-required",
            "constructor-engram-no-cross-cluster-link-without-transport-receipt",
            "constructor-engram-compressed-claims-require-decompression-path",
            "constructor-engram-candidate-only"
        };

        AddMissingConstraint(constraints, scoresWithinRange, "bounded-scores");
        AddMissingConstraint(constraints, requestedTierEarned, "requested-tier-earned");
        AddMissingConstraint(constraints, burdenVectorWithinTierBounds, "burden-vector-within-tier");
        AddMissingConstraint(constraints, receiptIntegrityHolds, "receipt-integrity");
        AddMissingConstraint(constraints, receiptCoverageWithinBound, "receipt-coverage");
        AddMissingConstraint(constraints, receiptFreshnessWithinBound, "receipt-freshness");
        AddMissingConstraint(constraints, clusterBoundariesExplicit, "cluster-boundaries");
        AddMissingConstraint(constraints, refusalPathsIntact, "refusal-paths");
        AddMissingConstraint(constraints, crossClusterTransportReceipted, "cross-cluster-transport");
        AddMissingConstraint(constraints, compressedClaimsDecompressible, "decompression-path");

        if (request.PromotionRequested)
        {
            constraints.Add("constructor-engram-promotion-requested");
        }

        if (nearSwampDetected)
        {
            constraints.Add("constructor-engram-near-swamp-detected");
        }

        constraints.Add($"constructor-engram-movement-{ToReasonToken(movementKind)}");

        return constraints;
    }

    private static string DetermineReasonCode(
        ConstructorEngramBurdenAssessmentRequest request,
        ConstructorEngramTierBudget budget,
        ConstructorEngramBurdenMovementKind movementKind,
        bool scoresWithinRange,
        bool requestedTierEarned,
        bool receiptIntegrityHolds,
        bool receiptCoverageWithinBound,
        bool receiptFreshnessWithinBound,
        bool clusterBoundariesExplicit,
        bool refusalPathsIntact,
        bool crossClusterTransportReceipted,
        bool compressedClaimsDecompressible)
    {
        if (!scoresWithinRange)
        {
            return "constructor-engram-burden-scores-out-of-range";
        }

        if (!requestedTierEarned && request.PromotionRequested)
        {
            return "constructor-engram-requested-tier-not-earned";
        }

        if (!compressedClaimsDecompressible)
        {
            return "constructor-engram-compressed-claims-non-promotable";
        }

        if (!receiptIntegrityHolds)
        {
            return "constructor-engram-receipt-integrity-broken";
        }

        if (!receiptCoverageWithinBound)
        {
            return "constructor-engram-receipt-coverage-below-tier-bound";
        }

        if (!receiptFreshnessWithinBound)
        {
            return "constructor-engram-receipt-freshness-below-tier-bound";
        }

        if (!clusterBoundariesExplicit || request.ClusterCount < budget.MinClusters)
        {
            return "constructor-engram-split-required";
        }

        if (!ClusterCountWithinBudget(request.ClusterCount, budget))
        {
            return "constructor-engram-cluster-required";
        }

        if (!CrossLinkDensityWithinBudget(request.CrossLinkDensity, budget))
        {
            return "constructor-engram-cross-link-density-exceeds-tier-bound";
        }

        if (!OperatorLoadWithinBudget(request.OperatorLoad, budget))
        {
            return "constructor-engram-operator-load-exceeds-tier-bound";
        }

        if (!DepthWithinBudget(request.DependencyDepth, budget))
        {
            return "constructor-engram-depth-exceeds-tier-bound";
        }

        if (!refusalPathsIntact)
        {
            return "constructor-engram-refusal-paths-missing";
        }

        if (!crossClusterTransportReceipted)
        {
            return "constructor-engram-cross-cluster-transport-receipt-missing";
        }

        return movementKind == ConstructorEngramBurdenMovementKind.PromotionCandidate
            ? "constructor-engram-promotion-candidate"
            : "constructor-engram-within-tier";
    }

    private static string DetermineLawfulBasis(
        ConstructorEngramBurdenMovementKind movementKind)
    {
        return movementKind switch
        {
            ConstructorEngramBurdenMovementKind.PromotionCandidate =>
                "constructor engram may be considered for promotion because the burden vector fits its computed tier with receipt integrity, explicit clusters, refusal paths, transport receipts, and decompression paths intact.",
            ConstructorEngramBurdenMovementKind.SplitRequired =>
                "constructor engram must split when branch burden lacks enough explicit cluster boundaries for its computed tier.",
            ConstructorEngramBurdenMovementKind.ClusterRequired =>
                "constructor engram must cluster or stage when cluster count, cross-link density, or operator load exceeds its tier budget.",
            ConstructorEngramBurdenMovementKind.DeferRequired =>
                "constructor engram must defer when requested tier, dependency depth, refusal path, or cross-cluster transport remains under-specified.",
            ConstructorEngramBurdenMovementKind.PruneRequired =>
                "constructor engram must prune or refresh when receipt integrity, receipt coverage, or receipt freshness falls below tier burden.",
            ConstructorEngramBurdenMovementKind.NonPromotable =>
                "constructor engram remains non-promotable when compressed claims lack a decompression path.",
            ConstructorEngramBurdenMovementKind.Refused =>
                "constructor engram burden assessment must refuse invalid numerical burden vectors.",
            _ =>
                "constructor engram may remain within its computed tier while staying candidate-only and without authority escalation."
        };
    }

    private static decimal DetermineNearSwampIndex(
        ConstructorEngramBurdenAssessmentRequest request,
        ConstructorEngramTierBudget budget)
    {
        var maxCrossLinkDensity = budget.MaxCrossLinkDensity ?? Math.Max(request.CrossLinkDensity, budget.MinCrossLinkDensity);
        var maxOperatorLoad = budget.MaxOperatorLoad ?? Math.Max(request.OperatorLoad, budget.MinBranches);
        var linkPressure = Clamp01(maxCrossLinkDensity == 0 ? 0 : request.CrossLinkDensity / maxCrossLinkDensity);
        var loadPressure = Clamp01(maxOperatorLoad == 0 ? 0 : (decimal)request.OperatorLoad / maxOperatorLoad);
        var proofDeficit = Clamp01(1.0m - request.ReceiptCoverage);

        return Math.Round((linkPressure + loadPressure + proofDeficit) / 3.0m, 4);
    }

    private static bool DetermineScoresWithinRange(
        ConstructorEngramBurdenAssessmentRequest request)
    {
        return request.BranchCount > 0 &&
               request.ClusterCount >= 0 &&
               request.DependencyDepth >= 0 &&
               request.CrossLinkDensity >= 0.0m &&
               IsCoverage(request.ReceiptCoverage) &&
               IsCoverage(request.ReceiptFreshnessCoverage) &&
               request.OperatorLoad >= 0 &&
               request.CrossClusterLinkCount >= 0 &&
               request.CompressedClaimCount >= 0;
    }

    private static bool BranchCountWithinBudget(
        int branchCount,
        ConstructorEngramTierBudget budget)
    {
        return branchCount >= budget.MinBranches &&
               (budget.MaxBranches is null || branchCount <= budget.MaxBranches.Value);
    }

    private static bool ClusterCountWithinBudget(
        int clusterCount,
        ConstructorEngramTierBudget budget)
    {
        return clusterCount >= budget.MinClusters &&
               (budget.MaxClusters is null || clusterCount <= budget.MaxClusters.Value);
    }

    private static bool DepthWithinBudget(
        int dependencyDepth,
        ConstructorEngramTierBudget budget)
    {
        return dependencyDepth >= budget.MinDepth &&
               (budget.MaxDepth is null || dependencyDepth <= budget.MaxDepth.Value);
    }

    private static bool CrossLinkDensityWithinBudget(
        decimal crossLinkDensity,
        ConstructorEngramTierBudget budget)
    {
        return crossLinkDensity >= budget.MinCrossLinkDensity &&
               (budget.MaxCrossLinkDensity is null || crossLinkDensity <= budget.MaxCrossLinkDensity.Value);
    }

    private static bool OperatorLoadWithinBudget(
        int operatorLoad,
        ConstructorEngramTierBudget budget)
    {
        return budget.MaxOperatorLoad is null || operatorLoad <= budget.MaxOperatorLoad.Value;
    }

    private static bool IsCoverage(decimal coverage)
        => coverage is >= 0.0m and <= 1.0m;

    private static void AddMissingConstraint(
        ICollection<string> constraints,
        bool present,
        string name)
    {
        if (!present)
        {
            constraints.Add($"constructor-engram-{name}-missing");
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

    private static decimal Clamp01(decimal value)
        => Math.Min(1.0m, Math.Max(0.0m, value));

    private static string ToReasonToken(
        ConstructorEngramBurdenMovementKind movementKind)
    {
        return movementKind switch
        {
            ConstructorEngramBurdenMovementKind.PromotionCandidate => "promotion-candidate",
            ConstructorEngramBurdenMovementKind.SplitRequired => "split-required",
            ConstructorEngramBurdenMovementKind.ClusterRequired => "cluster-required",
            ConstructorEngramBurdenMovementKind.DeferRequired => "defer-required",
            ConstructorEngramBurdenMovementKind.PruneRequired => "prune-required",
            ConstructorEngramBurdenMovementKind.NonPromotable => "non-promotable",
            ConstructorEngramBurdenMovementKind.Refused => "refused",
            _ => "within-tier"
        };
    }
}
