using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using GEL.Contracts;
using GEL.Graphs;
using GEL.Models;
using SLI.Engine.Runtime;
using SoulFrame.Host;

namespace SLI.Engine.Morphology;

internal sealed class SliLispMorphologyRuntime : ISliMorphologyRuntime
{
    private readonly LispBridge _bridge;
    private readonly SLI.Ingestion.OntologicalCleaver _ontologicalCleaver;
    private readonly RootAtlasOntologicalCleaver _rootAtlasOntologicalCleaver;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private bool _initialized;

    public SliLispMorphologyRuntime(
        LispBridge? bridge = null,
        SLI.Ingestion.OntologicalCleaver? ontologicalCleaver = null,
        RootAtlasOntologicalCleaver? rootAtlasOntologicalCleaver = null)
    {
        _bridge = bridge ?? new LispBridge(NullEngramResolver.Instance);
        _ontologicalCleaver = ontologicalCleaver ?? new SLI.Ingestion.OntologicalCleaver();
        _rootAtlasOntologicalCleaver = rootAtlasOntologicalCleaver ?? new RootAtlasOntologicalCleaver();
    }

    public async Task<SliMorphologySentenceResult> TranslateSentenceAsync(
        string sentence,
        RootAtlas atlas,
        IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);
        cancellationToken.ThrowIfCancellationRequested();

        if (!SentenceSpecifications.TryGetValue(sentence.Trim(), out var specification))
        {
            return CreateOutOfScopeSentence(sentence);
        }

        ValidateOverlayRoots(overlayRoots);
        var workingAtlas = BuildOverlayAtlas(atlas, overlayRoots);
        _ = _ontologicalCleaver.Cleave(sentence);
        _ = await _rootAtlasOntologicalCleaver.CleaveAsync(sentence, cancellationToken).ConfigureAwait(false);

        var resolvedRoots = ResolveRequiredRoots(specification, atlas, workingAtlas);
        if (resolvedRoots is null)
        {
            return new SliMorphologySentenceResult
            {
                Sentence = sentence,
                ResolvedLemmaRoots = Array.Empty<string>(),
                OperatorAnnotations = Array.Empty<SliMorphologyOperatorAnnotation>(),
                ConstructorBodies = Array.Empty<SliMorphologyConstructorBody>(),
                DiagnosticPredicateRender = specification.DiagnosticPredicateRender,
                LaneOutcome = SliMorphologyLaneOutcome.Rejected,
                PredicateRoot = specification.PredicateRoot,
                Summary = specification.Summary
            };
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        var program = BuildSentenceProgram(specification);
        var result = await _bridge.ExecuteMorphologySentenceProgramAsync(program, sentence, cancellationToken).ConfigureAwait(false);

        return new SliMorphologySentenceResult
        {
            Sentence = sentence,
            ResolvedLemmaRoots = resolvedRoots,
            OperatorAnnotations = result.OperatorAnnotations,
            ConstructorBodies = result.ConstructorBodies,
            DiagnosticPredicateRender = result.DiagnosticPredicateRender,
            LaneOutcome = result.LaneOutcome,
            PredicateRoot = result.PredicateRoot,
            Summary = result.Summary,
            ScalarPayload = result.ScalarPayload
        };
    }

    public async Task<SliMorphologyParagraphResult> TranslateParagraphAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);
        cancellationToken.ThrowIfCancellationRequested();

        if (!SupportedParagraphs.Contains(paragraph.Trim()))
        {
            return new SliMorphologyParagraphResult
            {
                LaneOutcome = SliMorphologyLaneOutcome.OutOfScope,
                Paragraph = paragraph,
                SentenceResults = Array.Empty<SliMorphologySentenceResult>(),
                GraphEdges = Array.Empty<ConstructorEdge>()
            };
        }

        var sentenceResults = new List<SliMorphologySentenceResult>();
        foreach (var sentence in SplitParagraph(paragraph))
        {
            sentenceResults.Add(await TranslateSentenceAsync(sentence, atlas, overlayRoots, cancellationToken).ConfigureAwait(false));
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        var graphEdges = BuildGraphEdges(sentenceResults);
        var program = BuildParagraphProgram(graphEdges);
        return await _bridge.ExecuteMorphologyParagraphProgramAsync(sentenceResults, program, paragraph, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SliMorphologyParagraphBodyResult> TranslateParagraphBodyAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paragraph);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);
        cancellationToken.ThrowIfCancellationRequested();

        if (!ParagraphBodySpecifications.TryGetValue(paragraph.Trim(), out var specification))
        {
            return new SliMorphologyParagraphBodyResult
            {
                LaneOutcome = SliMorphologyLaneOutcome.OutOfScope,
                Paragraph = paragraph,
                ParagraphResult = new SliMorphologyParagraphResult
                {
                    LaneOutcome = SliMorphologyLaneOutcome.OutOfScope,
                    Paragraph = paragraph,
                    SentenceResults = Array.Empty<SliMorphologySentenceResult>(),
                    GraphEdges = Array.Empty<ConstructorEdge>()
                },
                ContinuityAnchors = Array.Empty<string>(),
                ParagraphInvariants = Array.Empty<string>(),
                ClusterDiagnosticRender = string.Empty,
                BodySummary = "body[anchors:none; closed:0; ambiguous:0]"
            };
        }

        var paragraphResult = await TranslateParagraphAsync(paragraph, atlas, overlayRoots, cancellationToken).ConfigureAwait(false);
        var continuityAnchors = paragraphResult.GraphEdges
            .Where(edge => edge.Relation.StartsWith("continuity:", StringComparison.Ordinal))
            .Select(edge => edge.Relation["continuity:".Length..])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var paragraphInvariants = BuildParagraphInvariants(paragraphResult, continuityAnchors, specification);
        var clusterEntries = paragraphResult.SentenceResults
            .Select(sentenceResult => sentenceResult.LaneOutcome == SliMorphologyLaneOutcome.NeedsSpecification
                ? $"ambiguous:{sentenceResult.DiagnosticPredicateRender}"
                : sentenceResult.DiagnosticPredicateRender)
            .ToArray();

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        var program = BuildParagraphBodyProgram(continuityAnchors, paragraphInvariants, clusterEntries, BuildBodySummary(continuityAnchors, paragraphResult));
        return await _bridge.ExecuteMorphologyParagraphBodyProgramAsync(paragraphResult, program, paragraph, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            await _bridge.InitializeAsync(cancellationToken).ConfigureAwait(false);
            _initialized = true;
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    private static SliMorphologySentenceResult CreateOutOfScopeSentence(string sentence)
    {
        return new SliMorphologySentenceResult
        {
            Sentence = sentence,
            ResolvedLemmaRoots = Array.Empty<string>(),
            OperatorAnnotations = Array.Empty<SliMorphologyOperatorAnnotation>(),
            ConstructorBodies = Array.Empty<SliMorphologyConstructorBody>(),
            DiagnosticPredicateRender = string.Empty,
            LaneOutcome = SliMorphologyLaneOutcome.OutOfScope,
            Summary = string.Empty
        };
    }

    private static void ValidateOverlayRoots(IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots)
    {
        foreach (var overlayRoot in overlayRoots)
        {
            if (string.IsNullOrWhiteSpace(overlayRoot.SymbolicCore))
            {
                throw new InvalidOperationException($"Overlay root {overlayRoot.Lemma} is missing a symbolic core.");
            }

            if (overlayRoot.ReservedDomainStatus is not ("none" or "bridge-only"))
            {
                throw new InvalidOperationException($"Overlay root {overlayRoot.Lemma} has invalid reserved-domain status.");
            }

            if (overlayRoot.ReservedDomainStatus == "none" && overlayRoot.DisciplinaryReservations.Count > 0)
            {
                throw new InvalidOperationException($"Overlay root {overlayRoot.Lemma} cannot carry disciplinary reservations when status is none.");
            }
        }
    }

    private static RootAtlas BuildOverlayAtlas(RootAtlas baseAtlas, IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots)
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
            $"{baseAtlas.Version}.lisp-bridge-fixture",
            baseAtlas.Entries.Concat(overlayEntries),
            baseAtlas.RefinementEdges,
            domains);
    }

    private static IReadOnlyList<string>? ResolveRequiredRoots(
        SentenceSpecification specification,
        RootAtlas baseAtlas,
        RootAtlas workingAtlas)
    {
        var resolved = new List<string>(specification.RequiredRoots.Count);
        foreach (var rootKey in specification.RequiredRoots)
        {
            if (baseAtlas.TryResolveRoot(rootKey, out _) || workingAtlas.TryResolveRoot(rootKey, out _))
            {
                resolved.Add(rootKey);
                continue;
            }

            return null;
        }

        return resolved;
    }

    private static IReadOnlyList<string> BuildSentenceProgram(SentenceSpecification specification)
    {
        var program = new List<string>();
        foreach (var root in specification.RequiredRoots)
        {
            program.Add($"(morph-root \"{root}\")");
        }

        foreach (var annotation in specification.OperatorAnnotations)
        {
            program.Add($"(morph-operator \"{annotation.Token}\" \"{annotation.Kind}\")");
        }

        foreach (var constructor in specification.ConstructorRoles)
        {
            program.Add($"(morph-constructor \"{constructor.Role}\" \"{constructor.RootKey}\")");
        }

        program.Add($"(morph-predicate-root \"{specification.PredicateRoot}\")");
        program.Add($"(morph-render \"{specification.DiagnosticPredicateRender}\")");
        program.Add($"(morph-summary \"{specification.Summary}\")");
        if (specification.ScalarPayload is not null)
        {
            program.Add($"(morph-scalar \"{specification.ScalarPayload}\")");
        }

        program.Add($"(morph-outcome \"{specification.LaneOutcome}\")");
        return program;
    }

    private static IReadOnlyList<string> SplitParagraph(string paragraph)
    {
        return paragraph
            .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(sentence => $"{sentence}.")
            .ToArray();
    }

    private static IReadOnlyList<ConstructorEdge> BuildGraphEdges(IReadOnlyList<SliMorphologySentenceResult> sentenceResults)
    {
        var edges = new List<ConstructorEdge>();

        foreach (var sentenceResult in sentenceResults)
        {
            var predicateRoot = ResolvePredicateRoot(sentenceResult);
            if (string.IsNullOrWhiteSpace(predicateRoot))
            {
                continue;
            }

            foreach (var branchBody in sentenceResult.ConstructorBodies.Where(body =>
                         !string.Equals(body.Role, "predicate", StringComparison.OrdinalIgnoreCase)))
            {
                edges.Add(new ConstructorEdge
                {
                    Source = predicateRoot,
                    Target = branchBody.RootKey,
                    Relation = branchBody.Role
                });
            }
        }

        for (var index = 0; index < sentenceResults.Count - 1; index++)
        {
            var currentPredicate = ResolvePredicateRoot(sentenceResults[index]);
            var nextPredicate = ResolvePredicateRoot(sentenceResults[index + 1]);
            if (string.IsNullOrWhiteSpace(currentPredicate) || string.IsNullOrWhiteSpace(nextPredicate))
            {
                continue;
            }

            var sharedRoots = sentenceResults[index].ResolvedLemmaRoots
                .Intersect(sentenceResults[index + 1].ResolvedLemmaRoots, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (sharedRoots.Length != 1)
            {
                continue;
            }

            edges.Add(new ConstructorEdge
            {
                Source = currentPredicate,
                Target = nextPredicate,
                Relation = $"continuity:{sharedRoots[0]}"
            });
        }

        return edges;
    }

    private static IReadOnlyList<string> BuildParagraphProgram(IReadOnlyList<ConstructorEdge> graphEdges)
    {
        return graphEdges
            .Select(edge => $"(morph-graph-edge \"{edge.Source}\" \"{edge.Target}\" \"{edge.Relation}\")")
            .ToArray();
    }

    private static string? ResolvePredicateRoot(SliMorphologySentenceResult sentenceResult)
    {
        if (!string.IsNullOrWhiteSpace(sentenceResult.PredicateRoot))
        {
            return sentenceResult.PredicateRoot;
        }

        return sentenceResult.ConstructorBodies
            .FirstOrDefault(body => string.Equals(body.Role, "predicate", StringComparison.OrdinalIgnoreCase))
            ?.RootKey;
    }

    private static IReadOnlyList<string> BuildParagraphInvariants(
        SliMorphologyParagraphResult paragraphResult,
        IReadOnlyList<string> continuityAnchors,
        ParagraphBodySpecification specification)
    {
        var invariants = new List<string>();
        invariants.AddRange(continuityAnchors.Select(anchor => $"paragraph.continuity.root:{anchor}"));
        invariants.AddRange(specification.FixedProofLabels);

        if (paragraphResult.SentenceResults.Any(result => !string.IsNullOrWhiteSpace(result.ScalarPayload)))
        {
            invariants.Add("paragraph.measurement.present");
        }

        var ambiguousCount = paragraphResult.SentenceResults.Count(result => result.LaneOutcome == SliMorphologyLaneOutcome.NeedsSpecification);
        if (ambiguousCount > 0)
        {
            invariants.Add("paragraph.ambiguity.present");
        }

        var closedCount = paragraphResult.SentenceResults.Count(result => result.LaneOutcome == SliMorphologyLaneOutcome.Closed);
        invariants.Add($"paragraph.closed.draft.count:{closedCount}");

        if (ambiguousCount > 0)
        {
            invariants.Add($"paragraph.ambiguous.sentence.count:{ambiguousCount}");
        }

        return invariants;
    }

    private static string BuildBodySummary(IReadOnlyList<string> continuityAnchors, SliMorphologyParagraphResult paragraphResult)
    {
        var anchorSummary = continuityAnchors.Count == 0 ? "none" : string.Join(",", continuityAnchors);
        var closedCount = paragraphResult.SentenceResults.Count(result => result.LaneOutcome == SliMorphologyLaneOutcome.Closed);
        var ambiguousCount = paragraphResult.SentenceResults.Count(result => result.LaneOutcome == SliMorphologyLaneOutcome.NeedsSpecification);
        return $"body[anchors:{anchorSummary}; closed:{closedCount}; ambiguous:{ambiguousCount}]";
    }

    private static IReadOnlyList<string> BuildParagraphBodyProgram(
        IReadOnlyList<string> continuityAnchors,
        IReadOnlyList<string> invariants,
        IReadOnlyList<string> clusterEntries,
        string bodySummary)
    {
        var program = new List<string>();
        foreach (var anchor in continuityAnchors)
        {
            program.Add($"(morph-anchor \"{anchor}\")");
        }

        foreach (var invariant in invariants)
        {
            program.Add($"(morph-invariant \"{invariant}\")");
        }

        foreach (var clusterEntry in clusterEntries)
        {
            program.Add($"(morph-cluster-entry \"{clusterEntry}\")");
        }

        program.Add($"(morph-body-summary \"{bodySummary}\")");
        return program;
    }

    private static readonly IReadOnlyDictionary<string, SentenceSpecification> SentenceSpecifications =
        new Dictionary<string, SentenceSpecification>(StringComparer.Ordinal)
        {
            ["The Gate remembers its makers."] = new()
            {
                RequiredRoots = ["gate", "remember", "make"],
                PredicateRoot = "remember",
                ConstructorRoles = [new("subject", "gate"), new("predicate", "remember"), new("object", "make")],
                OperatorAnnotations = [new("its", "possessive-determiner")],
                DiagnosticPredicateRender = "remember(gate,make)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.transitive"
            },
            ["The hum has increased twelve percent."] = new()
            {
                RequiredRoots = ["hum", "increase", "percent"],
                PredicateRoot = "increase",
                ConstructorRoles = [new("subject", "hum"), new("predicate", "increase"), new("unit", "percent")],
                OperatorAnnotations = [new("has", "auxiliary-aspect")],
                DiagnosticPredicateRender = "increase(hum,12,percent)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.measurement",
                ScalarPayload = "12"
            },
            ["The light was the first lie."] = new()
            {
                RequiredRoots = ["light", "lie"],
                PredicateRoot = "lie",
                ConstructorRoles = [new("subject", "light"), new("predicate", "lie")],
                OperatorAnnotations = [new("was", "copula"), new("first", "qualifier")],
                DiagnosticPredicateRender = "lie(light,first)",
                LaneOutcome = SliMorphologyLaneOutcome.NeedsSpecification,
                Summary = "narrative.ambiguous"
            },
            ["The Gate observes the hum."] = new()
            {
                RequiredRoots = ["gate", "observe", "hum"],
                PredicateRoot = "observe",
                ConstructorRoles = [new("subject", "gate"), new("predicate", "observe"), new("object", "hum")],
                OperatorAnnotations = Array.Empty<OperatorSpec>(),
                DiagnosticPredicateRender = "observe(gate,hum)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.transitive"
            },
            ["The light changes the Gate."] = new()
            {
                RequiredRoots = ["light", "change", "gate"],
                PredicateRoot = "change",
                ConstructorRoles = [new("subject", "light"), new("predicate", "change"), new("object", "gate")],
                OperatorAnnotations = Array.Empty<OperatorSpec>(),
                DiagnosticPredicateRender = "change(light,gate)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.transitive"
            },
            ["The command dome hummed with a low and constant note."] = new()
            {
                RequiredRoots = ["dome", "hum"],
                PredicateRoot = "hum",
                ConstructorRoles = [new("subject", "dome"), new("predicate", "hum")],
                OperatorAnnotations = Array.Empty<OperatorSpec>(),
                DiagnosticPredicateRender = "hum(dome)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.environment"
            },
            ["The command dome vibrated with the hum."] = new()
            {
                RequiredRoots = ["dome", "vibrate", "hum"],
                PredicateRoot = "vibrate",
                ConstructorRoles = [new("subject", "dome"), new("predicate", "vibrate"), new("object", "hum")],
                OperatorAnnotations = Array.Empty<OperatorSpec>(),
                DiagnosticPredicateRender = "vibrate(dome,hum)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.environment"
            },
            ["The hologram pulsed in slow rhythm."] = new()
            {
                RequiredRoots = ["hologram", "pulse", "rhythm"],
                PredicateRoot = "pulse",
                ConstructorRoles = [new("subject", "hologram"), new("predicate", "pulse"), new("context", "rhythm")],
                OperatorAnnotations = Array.Empty<OperatorSpec>(),
                DiagnosticPredicateRender = "pulse(hologram,rhythm)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.environment"
            },
            ["Subsurface activity is rising."] = new()
            {
                RequiredRoots = ["activity", "subsurface", "rise"],
                PredicateRoot = "rise",
                ConstructorRoles = [new("subject", "activity"), new("predicate", "rise"), new("context", "subsurface")],
                OperatorAnnotations = [new("is", "copula")],
                DiagnosticPredicateRender = "rise(activity,subsurface)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.measurement"
            },
            ["Subsurface activity resonates with the ridge."] = new()
            {
                RequiredRoots = ["activity", "resonate", "ridge"],
                PredicateRoot = "resonate",
                ConstructorRoles = [new("subject", "activity"), new("predicate", "resonate"), new("object", "ridge")],
                OperatorAnnotations = Array.Empty<OperatorSpec>(),
                DiagnosticPredicateRender = "resonate(activity,ridge)",
                LaneOutcome = SliMorphologyLaneOutcome.Closed,
                Summary = "narrative.measurement"
            }
        };

    private static readonly HashSet<string> SupportedParagraphs =
    [
        "The Gate remembers its makers. The Gate observes the hum. The hum has increased twelve percent. The light changes the Gate. The light was the first lie.",
        "The command dome hummed with a low and constant note. The command dome vibrated with the hum. The hologram pulsed in slow rhythm.",
        "The hum has increased twelve percent. Subsurface activity is rising. Subsurface activity resonates with the ridge.",
        "The Gate remembers its makers. The light was the first lie."
    ];

    private static readonly IReadOnlyDictionary<string, ParagraphBodySpecification> ParagraphBodySpecifications =
        new Dictionary<string, ParagraphBodySpecification>(StringComparer.Ordinal)
        {
            ["The command dome hummed with a low and constant note. The command dome vibrated with the hum. The hologram pulsed in slow rhythm."] = new()
            {
                FixedProofLabels = ["paragraph.environment.present", "paragraph.state.field"]
            },
            ["The hum has increased twelve percent. Subsurface activity is rising. Subsurface activity resonates with the ridge."] = new()
            {
                FixedProofLabels = Array.Empty<string>()
            },
            ["The Gate remembers its makers. The light was the first lie."] = new()
            {
                FixedProofLabels = Array.Empty<string>()
            }
        };

    private sealed class SentenceSpecification
    {
        public required IReadOnlyList<string> RequiredRoots { get; init; }
        public required string PredicateRoot { get; init; }
        public required IReadOnlyList<ConstructorRole> ConstructorRoles { get; init; }
        public required IReadOnlyList<OperatorSpec> OperatorAnnotations { get; init; }
        public required string DiagnosticPredicateRender { get; init; }
        public required SliMorphologyLaneOutcome LaneOutcome { get; init; }
        public required string Summary { get; init; }
        public string? ScalarPayload { get; init; }
    }

    private sealed class ParagraphBodySpecification
    {
        public required IReadOnlyList<string> FixedProofLabels { get; init; }
    }

    private sealed record ConstructorRole(string Role, string RootKey);

    private sealed record OperatorSpec(string Token, string Kind);

    private sealed class NullEngramResolver : IEngramResolver
    {
        public static readonly NullEngramResolver Instance = new();

        public Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("relevant"));
        }

        public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("concept"));
        }

        public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("cluster"));
        }

        private static EngramQueryResult Empty(string source)
        {
            return new EngramQueryResult
            {
                Source = source,
                Summaries = Array.Empty<EngramSummary>()
            };
        }
    }
}
