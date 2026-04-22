using San.Common;

namespace Oan.Audit.Tests;

public sealed class PredicateMintContractsTests
{
    private static readonly DefaultPredicateMintProjector Projector = new();

    [Fact]
    public void Mint_MintsStructuredPartialTruth_WhenCategoriesAreLawful()
    {
        var result = Projector.Mint(
            new PredicateMintRequest(
                PathHandle: "path://seed-floor/001",
                AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                Standing: ["aggregate_correlation_a_b", "aggregate_correlation_a_b"],
                Deferred: ["causation_a_implies_b"],
                Conflicted: ["subset_inconsistency"],
                Protected: ["raw_shards", "identity_linked_records"],
                PermittedDerivation: ["aggregate_metrics"],
                Refused:
                [
                    new PredicateRefusalRecord("scope_escalation", "authority_ceiling")
                ],
                ReceiptHandles: ["receipt://derivation/001", "receipt://derivation/001"]));

        Assert.Equal(PredicateMintDecision.Minted, result.Decision);
        Assert.Equal(
            ["aggregate_correlation_a_b"],
            result.Standing);
        Assert.Equal(
            ["causation_a_implies_b"],
            result.Deferred);
        Assert.Equal(
            ["subset_inconsistency"],
            result.Conflicted);
        Assert.Equal(
            ["identity_linked_records", "raw_shards"],
            result.Protected);
        Assert.Equal(
            ["aggregate_metrics"],
            result.PermittedDerivation);
        Assert.Single(result.Refused);
        Assert.Equal(
            ["receipt://derivation/001"],
            result.ReceiptHandles);
    }

    [Fact]
    public void Mint_Defers_WhenNoStandingExistsButDeferredItemsRemain()
    {
        var result = Projector.Mint(
            new PredicateMintRequest(
                PathHandle: "path://seed-floor/002",
                AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                Standing: [],
                Deferred: ["causal_direction_unknown"],
                Conflicted: [],
                Protected: ["raw_shards"],
                PermittedDerivation: [],
                Refused: [],
                ReceiptHandles: ["receipt://derivation/002"]));

        Assert.Equal(PredicateMintDecision.Deferred, result.Decision);
    }

    [Fact]
    public void Mint_Refuses_WhenOnlyConflictExistsWithoutStandingOrDeferral()
    {
        var result = Projector.Mint(
            new PredicateMintRequest(
                PathHandle: "path://seed-floor/003",
                AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                Standing: [],
                Deferred: [],
                Conflicted: ["bounded_subset_reversal"],
                Protected: ["raw_shards"],
                PermittedDerivation: [],
                Refused:
                [
                    new PredicateRefusalRecord("raw_data_exposure", "protected_substrate")
                ],
                ReceiptHandles: ["receipt://derivation/003"]));

        Assert.Equal(PredicateMintDecision.Refused, result.Decision);
    }

    [Fact]
    public void Mint_Throws_WhenStandingOverlapsDeferred()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Projector.Mint(
                new PredicateMintRequest(
                    PathHandle: "path://seed-floor/004",
                    AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                    DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                    Standing: ["aggregate_correlation_a_b"],
                    Deferred: ["aggregate_correlation_a_b"],
                    Conflicted: [],
                    Protected: [],
                    PermittedDerivation: [],
                    Refused: [],
                    ReceiptHandles: ["receipt://derivation/004"])));

        Assert.Contains("may not overlap", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Mint_Throws_WhenRefusedItemAlsoAppearsInProtectedSurface()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Projector.Mint(
                new PredicateMintRequest(
                    PathHandle: "path://seed-floor/005",
                    AuthorityClass: ProtectedExecutionAuthorityClass.FatherBound,
                    DisclosureCeiling: ProtectedExecutionDisclosureCeiling.StructuralOnly,
                    Standing: [],
                    Deferred: [],
                    Conflicted: [],
                    Protected: ["raw_shards"],
                    PermittedDerivation: [],
                    Refused:
                    [
                        new PredicateRefusalRecord("raw_shards", "protected_substrate")
                    ],
                    ReceiptHandles: ["receipt://derivation/005"])));

        Assert.Contains("may not also appear", ex.Message, StringComparison.Ordinal);
    }
}
