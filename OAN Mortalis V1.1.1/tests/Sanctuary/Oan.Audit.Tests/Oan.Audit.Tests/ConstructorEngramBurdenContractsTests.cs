namespace San.Audit.Tests;

using San.Common;

public sealed class ConstructorEngramBurdenContractsTests
{
    [Fact]
    public void ConstructorEngramBurden_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                ConstructorEngramTierKind.Root,
                ConstructorEngramTierKind.Primitive,
                ConstructorEngramTierKind.Basic,
                ConstructorEngramTierKind.LowerIntermediate,
                ConstructorEngramTierKind.UpperIntermediate,
                ConstructorEngramTierKind.Advanced,
                ConstructorEngramTierKind.ResearchFormation,
                ConstructorEngramTierKind.Cathedral
            ],
            Enum.GetValues<ConstructorEngramTierKind>());

        Assert.Equal(
            [
                ConstructorEngramBurdenMovementKind.WithinTier,
                ConstructorEngramBurdenMovementKind.PromotionCandidate,
                ConstructorEngramBurdenMovementKind.SplitRequired,
                ConstructorEngramBurdenMovementKind.ClusterRequired,
                ConstructorEngramBurdenMovementKind.DeferRequired,
                ConstructorEngramBurdenMovementKind.PruneRequired,
                ConstructorEngramBurdenMovementKind.NonPromotable,
                ConstructorEngramBurdenMovementKind.Refused
            ],
            Enum.GetValues<ConstructorEngramBurdenMovementKind>());
    }

    [Fact]
    public void Basic_Vector_Computes_Basic_And_Remains_Within_Tier()
    {
        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            CreateRequest(
                requestedTier: ConstructorEngramTierKind.Basic,
                promotionRequested: false,
                branchCount: 16,
                clusterCount: 2,
                depth: 3,
                linksPerNode: 4.0m,
                receiptCoverage: 0.94m,
                operatorLoad: 16),
            "receipt://constructor-engram/basic");

        Assert.Equal(ConstructorEngramTierKind.Basic, receipt.ComputedTier);
        Assert.Equal(ConstructorEngramBurdenMovementKind.WithinTier, receipt.MovementKind);
        Assert.True(receipt.RequestedTierEarned);
        Assert.True(receipt.BurdenVectorWithinTierBounds);
        Assert.False(receipt.PromotionAdmissible);
        Assert.False(receipt.DemotionOrThinningRequired);
        Assert.Contains("constructor-engram-movement-within-tier", receipt.ConstraintCodes);
    }

    [Fact]
    public void Advanced_Vector_Can_Be_A_Promotion_Candidate_When_Guards_Hold()
    {
        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            CreateRequest(
                requestedTier: ConstructorEngramTierKind.Advanced,
                promotionRequested: true,
                branchCount: 1024,
                clusterCount: 32,
                depth: 10,
                linksPerNode: 32.0m,
                receiptCoverage: 0.80m,
                operatorLoad: 1024),
            "receipt://constructor-engram/advanced");

        Assert.Equal(ConstructorEngramTierKind.Advanced, receipt.ComputedTier);
        Assert.Equal(ConstructorEngramBurdenMovementKind.PromotionCandidate, receipt.MovementKind);
        Assert.True(receipt.PromotionAdmissible);
        Assert.True(receipt.ClusterBoundariesExplicit);
        Assert.True(receipt.RefusalPathsIntact);
        Assert.True(receipt.CrossClusterTransportReceipted);
        Assert.True(receipt.CompressedClaimsDecompressible);
        Assert.Equal("constructor-engram-promotion-candidate", receipt.ReasonCode);
    }

    [Fact]
    public void Requested_Advanced_Label_Is_Not_Earned_By_Basic_Vector()
    {
        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            CreateRequest(
                requestedTier: ConstructorEngramTierKind.Advanced,
                promotionRequested: true,
                branchCount: 16,
                clusterCount: 2,
                depth: 3,
                linksPerNode: 4.0m,
                receiptCoverage: 0.94m,
                operatorLoad: 16),
            "receipt://constructor-engram/false-advanced");

        Assert.Equal(ConstructorEngramTierKind.Basic, receipt.ComputedTier);
        Assert.Equal(ConstructorEngramBurdenMovementKind.DeferRequired, receipt.MovementKind);
        Assert.False(receipt.RequestedTierEarned);
        Assert.False(receipt.PromotionAdmissible);
        Assert.Contains("constructor-engram-requested-tier-earned-missing", receipt.ConstraintCodes);
        Assert.Equal("constructor-engram-requested-tier-not-earned", receipt.ReasonCode);
    }

    [Fact]
    public void Receipt_Erosion_Forces_Prune_Or_Refresh()
    {
        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            CreateRequest(
                requestedTier: ConstructorEngramTierKind.Basic,
                promotionRequested: false,
                branchCount: 16,
                clusterCount: 2,
                depth: 3,
                linksPerNode: 4.0m,
                receiptCoverage: 0.50m,
                operatorLoad: 16),
            "receipt://constructor-engram/receipt-erosion");

        Assert.Equal(ConstructorEngramBurdenMovementKind.PruneRequired, receipt.MovementKind);
        Assert.False(receipt.ReceiptCoverageWithinBound);
        Assert.True(receipt.DemotionOrThinningRequired);
        Assert.Contains("constructor-engram-receipt-coverage-missing", receipt.ConstraintCodes);
        Assert.Equal("constructor-engram-receipt-coverage-below-tier-bound", receipt.ReasonCode);
    }

    [Fact]
    public void High_Link_And_Load_Pressure_Forces_Cluster_And_Near_Swamp()
    {
        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            CreateRequest(
                requestedTier: ConstructorEngramTierKind.Basic,
                promotionRequested: false,
                branchCount: 16,
                clusterCount: 8,
                depth: 3,
                linksPerNode: 12.0m,
                receiptCoverage: 0.20m,
                operatorLoad: 48),
            "receipt://constructor-engram/near-swamp");

        Assert.Equal(ConstructorEngramBurdenMovementKind.PruneRequired, receipt.MovementKind);
        Assert.True(receipt.NearSwampDetected);
        Assert.True(receipt.NearSwampIndex >= 0.75m);
        Assert.True(receipt.DemotionOrThinningRequired);
        Assert.Contains("constructor-engram-near-swamp-detected", receipt.ConstraintCodes);
    }

    [Fact]
    public void Cross_Cluster_Link_Without_Transport_Receipt_Defers()
    {
        var request = CreateRequest(
            requestedTier: ConstructorEngramTierKind.Basic,
            promotionRequested: false,
            branchCount: 16,
            clusterCount: 2,
            depth: 3,
            linksPerNode: 4.0m,
            receiptCoverage: 0.94m,
            operatorLoad: 16) with
        {
            CrossClusterLinkCount = 3,
            CrossClusterTransportReceiptHandles = []
        };

        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            request,
            "receipt://constructor-engram/cross-cluster-missing");

        Assert.Equal(ConstructorEngramBurdenMovementKind.DeferRequired, receipt.MovementKind);
        Assert.False(receipt.CrossClusterTransportReceipted);
        Assert.False(receipt.PromotionAdmissible);
        Assert.Contains("constructor-engram-cross-cluster-transport-missing", receipt.ConstraintCodes);
        Assert.Equal("constructor-engram-cross-cluster-transport-receipt-missing", receipt.ReasonCode);
    }

    [Fact]
    public void Compressed_Claims_Without_Decompression_Path_Are_NonPromotable()
    {
        var request = CreateRequest(
            requestedTier: ConstructorEngramTierKind.Basic,
            promotionRequested: true,
            branchCount: 16,
            clusterCount: 2,
            depth: 3,
            linksPerNode: 4.0m,
            receiptCoverage: 0.94m,
            operatorLoad: 16) with
        {
            CompressedClaimCount = 2,
            DecompressionPathHandles = []
        };

        var receipt = ConstructorEngramBurdenEvaluator.Evaluate(
            request,
            "receipt://constructor-engram/compressed-nonpromotable");

        Assert.Equal(ConstructorEngramBurdenMovementKind.NonPromotable, receipt.MovementKind);
        Assert.False(receipt.CompressedClaimsDecompressible);
        Assert.False(receipt.PromotionAdmissible);
        Assert.True(receipt.DemotionOrThinningRequired);
        Assert.Contains("constructor-engram-decompression-path-missing", receipt.ConstraintCodes);
        Assert.Equal("constructor-engram-compressed-claims-non-promotable", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Constructor_Engram_Burden_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "CONSTRUCTOR_ENGRAM_BURDEN_LAW.md");
        var fibrinoidPath = Path.Combine(lineRoot, "docs", "GROUPOID_FIBRINOID_COLLECTION_AND_BUNDLE_MAPPING_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var fibrinoidText = File.ReadAllText(fibrinoidPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("Tier is computed", lawText, StringComparison.Ordinal);
        Assert.Contains("B/C/D/L/P/O", lawText, StringComparison.Ordinal);
        Assert.Contains("ConstructorEngramBurdenAssessmentReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("No cross-cluster link may stand without a receipted transport justification", lawText, StringComparison.Ordinal);
        Assert.Contains("Compressed claims must carry a decompression path", lawText, StringComparison.Ordinal);
        Assert.Contains("Near-Swamp Index", lawText, StringComparison.Ordinal);
        Assert.Contains("CONSTRUCTOR_ENGRAM_BURDEN_LAW.md", fibrinoidText, StringComparison.Ordinal);
        Assert.Contains("constructor-engram-burden-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Constructor engram burden law preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("temporal near-swamp pressure and constructor-engram branch-pruning execution beyond burden law", refinementText, StringComparison.Ordinal);
        Assert.Contains("`CONSTRUCTOR_ENGRAM_BURDEN_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static ConstructorEngramBurdenAssessmentRequest CreateRequest(
        ConstructorEngramTierKind requestedTier,
        bool promotionRequested,
        int branchCount,
        int clusterCount,
        int depth,
        decimal linksPerNode,
        decimal receiptCoverage,
        int operatorLoad)
    {
        return new ConstructorEngramBurdenAssessmentRequest(
            AssessmentHandle: "assessment://constructor-engram/session-a",
            EngramHandle: "constructor-engram://arithmetic/functions",
            RequestedTier: requestedTier,
            PromotionRequested: promotionRequested,
            BranchCount: branchCount,
            ClusterCount: clusterCount,
            DependencyDepth: depth,
            CrossLinkDensity: linksPerNode,
            ReceiptCoverage: receiptCoverage,
            ReceiptFreshnessCoverage: receiptCoverage,
            OperatorLoad: operatorLoad,
            CrossClusterLinkCount: 1,
            CompressedClaimCount: 1,
            ReceiptIntegrityHolds: true,
            ClusterBoundaryHandles:
            [
                "cluster://constructor-engram/functions"
            ],
            RefusalPathHandles:
            [
                "refusal://constructor-engram/not-enough"
            ],
            CrossClusterTransportReceiptHandles:
            [
                "receipt://constructor-engram/cross-cluster-transport"
            ],
            DecompressionPathHandles:
            [
                "decompress://constructor-engram/compressed-claim"
            ],
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 20, 10, 00, TimeSpan.Zero));
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
