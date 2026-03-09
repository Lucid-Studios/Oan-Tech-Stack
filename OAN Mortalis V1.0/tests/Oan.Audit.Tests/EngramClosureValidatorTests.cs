using EngramGovernance.Services;
using GEL.Models;
using Oan.Spinal;

namespace Oan.Audit.Tests;

public sealed class EngramClosureValidatorTests
{
    private readonly EngramClosureValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ReturnsClosed_ForCompleteDraft()
    {
        var atlas = BuildAtlas();
        var draft = new EngramDraft
        {
            RootKey = "equation",
            EpistemicClass = EngramEpistemicClass.Propositional,
            Trunk = new EngramTrunk
            {
                Segments = ["predicate-root", "equation-closure"],
                Summary = "complete"
            },
            Branches =
            [
                new EngramBranch
                {
                    Name = "supporting-variable",
                    RootKey = "variable",
                    SymbolicHandle = "ATLAS.ROOT.VARIABLE"
                }
            ],
            Invariants =
            [
                new EngramInvariant
                {
                    Key = "engram.identity",
                    Statement = "The equation root remains stable across closure."
                }
            ],
            RequestedClosureGrade = EngramClosureGrade.Closed
        };

        var decision = await _validator.ValidateAsync(draft, atlas);

        Assert.Equal(EngramClosureGrade.Closed, decision.Grade);
        Assert.True(decision.IsSuccess);
        Assert.NotNull(decision.CanonicalEngram);
        Assert.Equal("equation", decision.CanonicalEngram!.Root.Key);
        Assert.Equal(EngramEpistemicClass.Propositional, decision.CanonicalEngram.EpistemicClass);
        Assert.NotNull(decision.NormalizedId);
        Assert.Empty(decision.ReasonCodes);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsBootstrapClosed_ForBranchlessDraft()
    {
        var atlas = BuildAtlas();
        var draft = new EngramDraft
        {
            RootKey = "equation",
            EpistemicClass = EngramEpistemicClass.Procedural,
            Trunk = new EngramTrunk
            {
                Segments = ["normalize-equation"],
                Summary = "bootstrap"
            },
            Branches = [],
            Invariants =
            [
                new EngramInvariant
                {
                    Key = "engram.bootstrap",
                    Statement = "Bootstrap closure keeps one invariant."
                }
            ],
            RequestedClosureGrade = EngramClosureGrade.BootstrapClosed
        };

        var decision = await _validator.ValidateAsync(draft, atlas);

        Assert.Equal(EngramClosureGrade.BootstrapClosed, decision.Grade);
        Assert.Contains("engram.branch.bootstrap-empty", decision.Warnings);
        Assert.NotNull(decision.CanonicalEngram);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsNeedsSpecification_ForUnresolvedBranch()
    {
        var atlas = BuildAtlas();
        var draft = new EngramDraft
        {
            RootKey = "equation",
            EpistemicClass = EngramEpistemicClass.Propositional,
            Trunk = new EngramTrunk
            {
                Segments = ["predicate-root"],
                Summary = "needs-spec"
            },
            Branches =
            [
                new EngramBranch
                {
                    Name = "missing-branch",
                    RootKey = "nonexistent-root",
                    SymbolicHandle = "ATLAS.ROOT.MISSING"
                }
            ],
            Invariants =
            [
                new EngramInvariant
                {
                    Key = "engram.identity",
                    Statement = "Invariant remains required."
                }
            ],
            RequestedClosureGrade = EngramClosureGrade.Closed
        };

        var decision = await _validator.ValidateAsync(draft, atlas);

        Assert.Equal(EngramClosureGrade.NeedsSpecification, decision.Grade);
        Assert.Contains(decision.ReasonCodes, code => code.StartsWith("engram.branch.root.unresolved:", StringComparison.Ordinal));
        Assert.Null(decision.CanonicalEngram);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsRejected_ForMissingInvariant()
    {
        var atlas = BuildAtlas();
        var draft = new EngramDraft
        {
            RootKey = "equation",
            EpistemicClass = EngramEpistemicClass.Propositional,
            Trunk = new EngramTrunk
            {
                Segments = ["predicate-root"],
                Summary = "invalid"
            },
            Branches = [],
            Invariants = [],
            RequestedClosureGrade = EngramClosureGrade.BootstrapClosed
        };

        var decision = await _validator.ValidateAsync(draft, atlas);

        Assert.Equal(EngramClosureGrade.Rejected, decision.Grade);
        Assert.Contains("engram.invariant.missing", decision.ReasonCodes);
    }

    [Fact]
    public void RootAtlas_Create_ProducesStableDigest()
    {
        var atlasA = BuildAtlas();
        var atlasB = BuildAtlas();

        Assert.Equal(atlasA.Version, atlasB.Version);
        Assert.Equal(atlasA.Digest, atlasB.Digest);
    }

    [Fact]
    public void EngramEnvelope_RemainsSeparateFromCanonicalEngramMeaningObject()
    {
        var canonicalProperties = typeof(Engram).GetProperties().Select(property => property.Name).ToArray();
        var envelopeProperties = typeof(EngramEnvelope).GetProperties().Select(property => property.Name).ToArray();

        Assert.Contains("Root", canonicalProperties);
        Assert.Contains("EpistemicClass", canonicalProperties);
        Assert.Contains("payloadHash", envelopeProperties, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("payloadHash", canonicalProperties, StringComparer.OrdinalIgnoreCase);
    }

    private static RootAtlas BuildAtlas()
    {
        return RootAtlas.Create(
            "root-atlas.v1",
            [
                new RootAtlasEntry
                {
                    Root = new PredicateRoot
                    {
                        Key = "equation",
                        DisplayLabel = "equation",
                        AtlasDomain = "atlas.root.e",
                        SymbolicHandle = "ATLAS.ROOT.EQUATION",
                        DictionaryPointer = "atlas://root/equation"
                    },
                    VariantForms = ["equation", "equations"],
                    SymbolicConstructors =
                    [
                        new SymbolicConstructorTriplet
                        {
                            RootKey = "equation",
                            RootSymbol = "ATLAS.ROOT.EQUATION",
                            CanonicalText = "equation",
                            MergedGlyph = "ATLAS.ROOT.EQUATION"
                        }
                    ],
                    FrequencyWeight = 4d
                },
                new RootAtlasEntry
                {
                    Root = new PredicateRoot
                    {
                        Key = "variable",
                        DisplayLabel = "variable",
                        AtlasDomain = "atlas.root.v",
                        SymbolicHandle = "ATLAS.ROOT.VARIABLE",
                        DictionaryPointer = "atlas://root/variable"
                    },
                    VariantForms = ["variable", "variables"],
                    SymbolicConstructors =
                    [
                        new SymbolicConstructorTriplet
                        {
                            RootKey = "variable",
                            RootSymbol = "ATLAS.ROOT.VARIABLE",
                            CanonicalText = "variable",
                            MergedGlyph = "ATLAS.ROOT.VARIABLE"
                        }
                    ],
                    FrequencyWeight = 3d
                }
            ],
            refinementEdges:
            [
                new PredicateRefinementEdge
                {
                    ParentRootKey = "equation",
                    ChildRootKey = "variable",
                    Relation = "requires"
                }
            ],
            domainDescriptors:
            [
                new DomainDescriptor
                {
                    DomainName = "atlas.root.e",
                    Description = "Equation roots.",
                    Tags = ["mathematics", "canonical"]
                },
                new DomainDescriptor
                {
                    DomainName = "atlas.root.v",
                    Description = "Variable roots.",
                    Tags = ["mathematics", "canonical"]
                }
            ]);
    }
}
