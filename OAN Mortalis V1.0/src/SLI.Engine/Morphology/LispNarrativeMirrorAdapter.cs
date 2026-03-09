using GEL.Contracts;
using GEL.Graphs;
using GEL.Models;
using SLI.Ingestion;

namespace SLI.Engine.Morphology;

internal sealed class LispNarrativeMirrorAdapter
{
    private readonly ISliMorphologyRuntime _morphologyRuntime;
    private readonly IEngramClosureValidator _closureValidator;

    public LispNarrativeMirrorAdapter(
        IEngramClosureValidator closureValidator,
        ISliMorphologyRuntime? morphologyRuntime = null)
    {
        _closureValidator = closureValidator ?? throw new ArgumentNullException(nameof(closureValidator));
        _morphologyRuntime = morphologyRuntime ?? new SliLispMorphologyRuntime();
    }

    public async Task<NarrativeTranslationLaneResult> TranslateSentenceAsync(
        string sentence,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        var candidateResult = await TranslateSentenceCandidateAsync(
            sentence,
            atlas,
            overlayRoots,
            cancellationToken).ConfigureAwait(false);

        return await FinalizeSentenceResultAsync(candidateResult, atlas, overlayRoots, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<NarrativeTranslationLaneResult> TranslateSentenceCandidateAsync(
        string sentence,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);

        var workingAtlas = BuildOverlayAtlas(atlas, overlayRoots);
        var bridgeResult = await _morphologyRuntime.TranslateSentenceAsync(
            sentence,
            atlas,
            ToMorphologyOverlayRoots(overlayRoots),
            cancellationToken).ConfigureAwait(false);

        var constructorBodies = MaterializeConstructorBodies(bridgeResult.ConstructorBodies, workingAtlas);
        var operatorAnnotations = bridgeResult.OperatorAnnotations
            .Select(annotation => new NarrativeOperatorAnnotation
            {
                Token = annotation.Token,
                Kind = annotation.Kind
            })
            .ToArray();

        if (bridgeResult.LaneOutcome == SliMorphologyLaneOutcome.OutOfScope)
        {
            return new NarrativeTranslationLaneResult
            {
                Sentence = sentence,
                ResolvedLemmaRoots = Array.Empty<string>(),
                OperatorAnnotations = Array.Empty<NarrativeOperatorAnnotation>(),
                ConstructorBodies = Array.Empty<NarrativeConstructorBody>(),
                DiagnosticPredicateRender = string.Empty,
                LaneOutcome = NarrativeTranslationLaneOutcome.OutOfScope
            };
        }

        if (bridgeResult.LaneOutcome == SliMorphologyLaneOutcome.NeedsSpecification)
        {
            return new NarrativeTranslationLaneResult
            {
                Sentence = sentence,
                ResolvedLemmaRoots = bridgeResult.ResolvedLemmaRoots,
                OperatorAnnotations = operatorAnnotations,
                ConstructorBodies = constructorBodies,
                DiagnosticPredicateRender = bridgeResult.DiagnosticPredicateRender,
                LaneOutcome = NarrativeTranslationLaneOutcome.NeedsSpecification,
                PredicateRoot = bridgeResult.PredicateRoot
            };
        }

        if (bridgeResult.LaneOutcome == SliMorphologyLaneOutcome.Rejected)
        {
            return new NarrativeTranslationLaneResult
            {
                Sentence = sentence,
                ResolvedLemmaRoots = bridgeResult.ResolvedLemmaRoots,
                OperatorAnnotations = operatorAnnotations,
                ConstructorBodies = constructorBodies,
                DiagnosticPredicateRender = bridgeResult.DiagnosticPredicateRender,
                LaneOutcome = NarrativeTranslationLaneOutcome.Rejected,
                PredicateRoot = bridgeResult.PredicateRoot
            };
        }

        var draft = MaterializeDraft(bridgeResult, constructorBodies);
        return new NarrativeTranslationLaneResult
        {
            Sentence = sentence,
            ResolvedLemmaRoots = bridgeResult.ResolvedLemmaRoots,
            OperatorAnnotations = operatorAnnotations,
            ConstructorBodies = constructorBodies,
            DiagnosticPredicateRender = bridgeResult.DiagnosticPredicateRender,
            LaneOutcome = NarrativeTranslationLaneOutcome.Closed,
            PredicateRoot = bridgeResult.PredicateRoot,
            EngramDraft = draft
        };
    }

    public async Task<NarrativeParagraphLaneResult> TranslateParagraphAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        var candidateResult = await TranslateParagraphCandidateAsync(
            paragraph,
            atlas,
            overlayRoots,
            cancellationToken).ConfigureAwait(false);

        var finalizedSentenceResults = new List<NarrativeTranslationLaneResult>(candidateResult.SentenceResults.Count);
        foreach (var sentenceResult in candidateResult.SentenceResults)
        {
            finalizedSentenceResults.Add(
                await FinalizeSentenceResultAsync(sentenceResult, atlas, overlayRoots, cancellationToken).ConfigureAwait(false));
        }

        return BuildParagraphResult(candidateResult.Paragraph, candidateResult.ParagraphGraph.Edges, finalizedSentenceResults);
    }

    internal async Task<NarrativeParagraphLaneResult> TranslateParagraphCandidateAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);

        var bridgeResult = await _morphologyRuntime.TranslateParagraphAsync(
            paragraph,
            atlas,
            ToMorphologyOverlayRoots(overlayRoots),
            cancellationToken).ConfigureAwait(false);

        if (bridgeResult.LaneOutcome == SliMorphologyLaneOutcome.OutOfScope)
        {
            return new NarrativeParagraphLaneResult
            {
                Paragraph = paragraph,
                SentenceResults = Array.Empty<NarrativeTranslationLaneResult>(),
                ParagraphGraph = new ConstructorGraph { Edges = Array.Empty<ConstructorEdge>() },
                DiagnosticGraphEdges = Array.Empty<ConstructorEdge>(),
                GeneratedDrafts = Array.Empty<EngramDraft>(),
                ClosureDecisions = Array.Empty<EngramClosureDecision>()
            };
        }

        var sentenceResults = new List<NarrativeTranslationLaneResult>(bridgeResult.SentenceResults.Count);
        foreach (var sentenceResult in bridgeResult.SentenceResults)
        {
            sentenceResults.Add(await TranslateSentenceCandidateAsync(sentenceResult.Sentence, atlas, overlayRoots, cancellationToken).ConfigureAwait(false));
        }

        return BuildParagraphResult(paragraph, bridgeResult.GraphEdges, sentenceResults);
    }

    public async Task<NarrativeParagraphBody> TranslateParagraphBodyAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);

        var bridgeResult = await _morphologyRuntime.TranslateParagraphBodyAsync(
            paragraph,
            atlas,
            ToMorphologyOverlayRoots(overlayRoots),
            cancellationToken).ConfigureAwait(false);

        if (bridgeResult.LaneOutcome == SliMorphologyLaneOutcome.OutOfScope)
        {
            return CreateOutOfScopeParagraphBody(paragraph, bridgeResult);
        }

        var paragraphResult = await TranslateParagraphAsync(paragraph, atlas, overlayRoots, cancellationToken).ConfigureAwait(false);
        return BuildParagraphBody(paragraph, bridgeResult, paragraphResult);
    }

    internal async Task<NarrativeParagraphBody> TranslateParagraphBodyCandidateAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);

        var bridgeResult = await _morphologyRuntime.TranslateParagraphBodyAsync(
            paragraph,
            atlas,
            ToMorphologyOverlayRoots(overlayRoots),
            cancellationToken).ConfigureAwait(false);

        if (bridgeResult.LaneOutcome == SliMorphologyLaneOutcome.OutOfScope)
        {
            return CreateOutOfScopeParagraphBody(paragraph, bridgeResult);
        }

        var paragraphResult = await TranslateParagraphCandidateAsync(paragraph, atlas, overlayRoots, cancellationToken).ConfigureAwait(false);
        return BuildParagraphBody(paragraph, bridgeResult, paragraphResult);
    }

    private static IReadOnlyList<SliMorphologyOverlayRoot> ToMorphologyOverlayRoots(IReadOnlyList<NarrativeOverlayRoot> overlayRoots)
    {
        return overlayRoots.Select(root => new SliMorphologyOverlayRoot
        {
            Lemma = root.Lemma,
            SymbolicCore = root.SymbolicCore,
            OperatorCompatibility = root.OperatorCompatibility,
            ReservedDomainStatus = root.ReservedDomainStatus,
            DisciplinaryReservations = root.DisciplinaryReservations,
            VariantExamples = root.VariantExamples
        }).ToArray();
    }

    private static RootAtlas BuildOverlayAtlas(RootAtlas baseAtlas, IReadOnlyList<NarrativeOverlayRoot> overlayRoots)
    {
        if (overlayRoots.Count == 0)
        {
            return baseAtlas;
        }

        var overlayEntries = overlayRoots.Select(root => new RootAtlasEntry
        {
            Root = new PredicateRoot
            {
                Key = root.Lemma,
                DisplayLabel = root.Lemma,
                AtlasDomain = $"atlas.narrative.{root.Lemma[0]}",
                SymbolicHandle = root.SymbolicCore,
                DictionaryPointer = $"atlas://fixture-root/{root.Lemma}"
            },
            VariantForms = root.VariantExamples
                .Append(root.Lemma)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            SymbolicConstructors =
            [
                new SymbolicConstructorTriplet
                {
                    RootKey = root.Lemma,
                    RootSymbol = root.SymbolicCore,
                    CanonicalText = root.Lemma,
                    MergedGlyph = root.SymbolicCore
                }
            ],
            FrequencyWeight = 1d
        });

        var domains = baseAtlas.DomainDescriptors
            .Concat(overlayEntries
                .Select(entry => entry.Root.AtlasDomain)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(domain => new DomainDescriptor
                {
                    DomainName = domain,
                    Description = $"Narrative translation fixture domain {domain}.",
                    Tags = ["narrative-fixture", "test-only"]
                }))
            .ToArray();

        return RootAtlas.Create(
            $"{baseAtlas.Version}.lisp-mirror-fixture",
            baseAtlas.Entries.Concat(overlayEntries),
            baseAtlas.RefinementEdges,
            domains);
    }

    private static IReadOnlyList<NarrativeConstructorBody> MaterializeConstructorBodies(
        IReadOnlyList<SliMorphologyConstructorBody> constructorBodies,
        RootAtlas atlas)
    {
        return constructorBodies.Select(body =>
        {
            var entry = atlas.Entries.First(candidate =>
                string.Equals(candidate.Root.Key, body.RootKey, StringComparison.OrdinalIgnoreCase));
            var constructor = entry.SymbolicConstructors.First(candidate =>
                string.Equals(candidate.RootKey, body.RootKey, StringComparison.OrdinalIgnoreCase));

            return new NarrativeConstructorBody
            {
                Role = body.Role,
                Constructor = constructor
            };
        }).ToArray();
    }

    private static EngramDraft MaterializeDraft(
        SliMorphologySentenceResult sentenceResult,
        IReadOnlyList<NarrativeConstructorBody> constructorBodies)
    {
        return new EngramDraft
        {
            RootKey = sentenceResult.PredicateRoot ?? throw new InvalidOperationException("Closed bridge result is missing a predicate root."),
            EpistemicClass = EngramEpistemicClass.Propositional,
            Trunk = new EngramTrunk
            {
                Segments =
                [
                    sentenceResult.DiagnosticPredicateRender,
                    .. sentenceResult.OperatorAnnotations.Select(annotation => $"{annotation.Kind}:{annotation.Token}")
                ],
                Summary = sentenceResult.Summary
            },
            Branches = constructorBodies
                .Where(body => !string.Equals(body.Role, "predicate", StringComparison.OrdinalIgnoreCase))
                .Select(body => new EngramBranch
                {
                    Name = body.Role,
                    RootKey = body.Constructor.RootKey,
                    SymbolicHandle = body.Constructor.RootKey
                })
                .ToArray(),
            Invariants = BuildInvariants(sentenceResult),
            RequestedClosureGrade = EngramClosureGrade.Closed
        };
    }

    private static IReadOnlyList<EngramInvariant> BuildInvariants(SliMorphologySentenceResult sentenceResult)
    {
        var invariants = new List<EngramInvariant>
        {
            new()
            {
                Key = "narrative.predicate.render",
                Statement = sentenceResult.DiagnosticPredicateRender
            }
        };

        if (!string.IsNullOrWhiteSpace(sentenceResult.ScalarPayload))
        {
            invariants.Add(new EngramInvariant
            {
                Key = "narrative.scalar.payload",
                Statement = sentenceResult.ScalarPayload
            });
        }

        return invariants;
    }

    private async Task<NarrativeTranslationLaneResult> FinalizeSentenceResultAsync(
        NarrativeTranslationLaneResult candidateResult,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken)
    {
        if (candidateResult.LaneOutcome != NarrativeTranslationLaneOutcome.Closed ||
            candidateResult.EngramDraft is null)
        {
            return candidateResult;
        }

        var workingAtlas = BuildOverlayAtlas(atlas, overlayRoots);
        var closureDecision = await _closureValidator
            .ValidateAsync(candidateResult.EngramDraft, workingAtlas, cancellationToken)
            .ConfigureAwait(false);

        return new NarrativeTranslationLaneResult
        {
            Sentence = candidateResult.Sentence,
            ResolvedLemmaRoots = candidateResult.ResolvedLemmaRoots,
            OperatorAnnotations = candidateResult.OperatorAnnotations,
            ConstructorBodies = candidateResult.ConstructorBodies,
            DiagnosticPredicateRender = candidateResult.DiagnosticPredicateRender,
            LaneOutcome = closureDecision.Grade == EngramClosureGrade.Closed
                ? NarrativeTranslationLaneOutcome.Closed
                : NarrativeTranslationLaneOutcome.Rejected,
            PredicateRoot = candidateResult.PredicateRoot,
            EngramDraft = candidateResult.EngramDraft,
            ClosureDecision = closureDecision
        };
    }

    private static NarrativeParagraphLaneResult BuildParagraphResult(
        string paragraph,
        IReadOnlyList<ConstructorEdge> graphEdges,
        IReadOnlyList<NarrativeTranslationLaneResult> sentenceResults)
    {
        return new NarrativeParagraphLaneResult
        {
            Paragraph = paragraph,
            SentenceResults = sentenceResults,
            ParagraphGraph = new ConstructorGraph { Edges = graphEdges },
            DiagnosticGraphEdges = graphEdges,
            GeneratedDrafts = sentenceResults
                .Where(result => result.EngramDraft is not null)
                .Select(result => result.EngramDraft!)
                .ToArray(),
            ClosureDecisions = sentenceResults
                .Where(result => result.ClosureDecision is not null)
                .Select(result => result.ClosureDecision!)
                .ToArray()
        };
    }

    private static NarrativeParagraphBody BuildParagraphBody(
        string paragraph,
        SliMorphologyParagraphBodyResult bridgeResult,
        NarrativeParagraphLaneResult paragraphResult)
    {
        return new NarrativeParagraphBody
        {
            Paragraph = paragraph,
            SentenceResults = paragraphResult.SentenceResults,
            ParagraphGraph = paragraphResult.ParagraphGraph,
            ParagraphInvariants = bridgeResult.ParagraphInvariants,
            ContinuityAnchors = bridgeResult.ContinuityAnchors,
            BodySummary = bridgeResult.BodySummary,
            DraftCluster = new NarrativeDraftCluster
            {
                MemberDrafts = paragraphResult.GeneratedDrafts,
                MemberClosureDecisions = paragraphResult.ClosureDecisions,
                AmbiguousSentenceKeys = paragraphResult.SentenceResults
                    .Where(result => result.LaneOutcome == NarrativeTranslationLaneOutcome.NeedsSpecification)
                    .Select(result => result.DiagnosticPredicateRender)
                    .ToArray(),
                ClusterDiagnosticRender = bridgeResult.ClusterDiagnosticRender
            }
        };
    }

    private static NarrativeParagraphBody CreateOutOfScopeParagraphBody(
        string paragraph,
        SliMorphologyParagraphBodyResult bridgeResult)
    {
        return new NarrativeParagraphBody
        {
            Paragraph = paragraph,
            SentenceResults = Array.Empty<NarrativeTranslationLaneResult>(),
            ParagraphGraph = new ConstructorGraph { Edges = Array.Empty<ConstructorEdge>() },
            ParagraphInvariants = Array.Empty<string>(),
            ContinuityAnchors = Array.Empty<string>(),
            BodySummary = bridgeResult.BodySummary,
            DraftCluster = new NarrativeDraftCluster
            {
                MemberDrafts = Array.Empty<EngramDraft>(),
                MemberClosureDecisions = Array.Empty<EngramClosureDecision>(),
                AmbiguousSentenceKeys = Array.Empty<string>(),
                ClusterDiagnosticRender = bridgeResult.ClusterDiagnosticRender
            }
        };
    }
}
