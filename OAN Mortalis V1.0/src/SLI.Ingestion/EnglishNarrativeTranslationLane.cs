using CradleTek.Memory.Services;
using GEL.Contracts;
using GEL.Models;

namespace SLI.Ingestion;

internal enum NarrativeTranslationLaneOutcome
{
    Closed,
    NeedsSpecification,
    Rejected,
    OutOfScope
}

internal sealed class NarrativeTranslationLaneResult
{
    public required string Sentence { get; init; }
    public required IReadOnlyList<string> ResolvedLemmaRoots { get; init; }
    public required IReadOnlyList<NarrativeOperatorAnnotation> OperatorAnnotations { get; init; }
    public required IReadOnlyList<NarrativeConstructorBody> ConstructorBodies { get; init; }
    public required string DiagnosticPredicateRender { get; init; }
    public required NarrativeTranslationLaneOutcome LaneOutcome { get; init; }
    public EngramDraft? EngramDraft { get; init; }
    public EngramClosureDecision? ClosureDecision { get; init; }
}

internal sealed class NarrativeOperatorAnnotation
{
    public required string Token { get; init; }
    public required string Kind { get; init; }
}

internal sealed class NarrativeConstructorBody
{
    public required string Role { get; init; }
    public required SymbolicConstructorTriplet Constructor { get; init; }
}

internal sealed class NarrativeOverlayRoot
{
    public required string Lemma { get; init; }
    public required string SymbolicCore { get; init; }
    public required string OperatorCompatibility { get; init; }
    public required string ReservedDomainStatus { get; init; }
    public required IReadOnlyList<string> DisciplinaryReservations { get; init; }
    public required IReadOnlyList<string> VariantExamples { get; init; }
}

internal sealed class EnglishNarrativeTranslationLane
{
    private readonly OntologicalCleaver _ontologicalCleaver;
    private readonly RootAtlasOntologicalCleaver _rootAtlasOntologicalCleaver;
    private readonly IEngramClosureValidator _closureValidator;

    public EnglishNarrativeTranslationLane(
        IEngramClosureValidator closureValidator,
        OntologicalCleaver? ontologicalCleaver = null,
        RootAtlasOntologicalCleaver? rootAtlasOntologicalCleaver = null)
    {
        _closureValidator = closureValidator ?? throw new ArgumentNullException(nameof(closureValidator));
        _ontologicalCleaver = ontologicalCleaver ?? new OntologicalCleaver();
        _rootAtlasOntologicalCleaver = rootAtlasOntologicalCleaver ?? new RootAtlasOntologicalCleaver();
    }

    public async Task<NarrativeTranslationLaneResult> TranslateAsync(
        string sentence,
        RootAtlas atlas,
        IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(overlayRoots);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedSentence = NormalizeSentence(sentence);
        if (!SentenceSpecifications.TryGetValue(normalizedSentence, out var specification))
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

        ValidateOverlayRoots(overlayRoots);

        var cleavedOntology = _ontologicalCleaver.Cleave(sentence);
        _ = await _rootAtlasOntologicalCleaver.CleaveAsync(sentence, cancellationToken).ConfigureAwait(false);
        var workingAtlas = BuildOverlayAtlas(atlas, overlayRoots);

        var resolvedRoots = ResolveRequiredRoots(specification, atlas, workingAtlas);
        if (resolvedRoots is null)
        {
            return new NarrativeTranslationLaneResult
            {
                Sentence = sentence,
                ResolvedLemmaRoots = Array.Empty<string>(),
                OperatorAnnotations = Array.Empty<NarrativeOperatorAnnotation>(),
                ConstructorBodies = Array.Empty<NarrativeConstructorBody>(),
                DiagnosticPredicateRender = specification.DiagnosticPredicateRender,
                LaneOutcome = NarrativeTranslationLaneOutcome.Rejected
            };
        }

        var operatorAnnotations = ResolveOperatorAnnotations(cleavedOntology.Tokens, specification);
        var constructorBodies = BuildConstructorBodies(specification, workingAtlas);

        if (specification.LaneOutcome == NarrativeTranslationLaneOutcome.NeedsSpecification)
        {
            return new NarrativeTranslationLaneResult
            {
                Sentence = sentence,
                ResolvedLemmaRoots = resolvedRoots,
                OperatorAnnotations = operatorAnnotations,
                ConstructorBodies = constructorBodies,
                DiagnosticPredicateRender = specification.DiagnosticPredicateRender,
                LaneOutcome = NarrativeTranslationLaneOutcome.NeedsSpecification
            };
        }

        var draft = BuildDraft(specification, resolvedRoots, operatorAnnotations);
        var closureDecision = await _closureValidator.ValidateAsync(draft, workingAtlas, cancellationToken).ConfigureAwait(false);

        return new NarrativeTranslationLaneResult
        {
            Sentence = sentence,
            ResolvedLemmaRoots = resolvedRoots,
            OperatorAnnotations = operatorAnnotations,
            ConstructorBodies = constructorBodies,
            DiagnosticPredicateRender = specification.DiagnosticPredicateRender,
            LaneOutcome = closureDecision.Grade == EngramClosureGrade.Closed
                ? NarrativeTranslationLaneOutcome.Closed
                : NarrativeTranslationLaneOutcome.Rejected,
            EngramDraft = draft,
            ClosureDecision = closureDecision
        };
    }

    private static string NormalizeSentence(string sentence)
    {
        return sentence.Trim();
    }

    private static void ValidateOverlayRoots(IReadOnlyList<NarrativeOverlayRoot> overlayRoots)
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
            $"{baseAtlas.Version}.narrative-fixture",
            baseAtlas.Entries.Concat(overlayEntries),
            baseAtlas.RefinementEdges,
            domains);
    }

    private static IReadOnlyList<string>? ResolveRequiredRoots(
        NarrativeSentenceSpecification specification,
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

    private static IReadOnlyList<NarrativeOperatorAnnotation> ResolveOperatorAnnotations(
        IReadOnlyList<string> tokens,
        NarrativeSentenceSpecification specification)
    {
        return specification.OperatorAnnotations
            .Where(annotation => tokens.Contains(annotation.Token, StringComparer.OrdinalIgnoreCase))
            .Select(annotation => new NarrativeOperatorAnnotation
            {
                Token = annotation.Token,
                Kind = annotation.Kind
            })
            .ToArray();
    }

    private static IReadOnlyList<NarrativeConstructorBody> BuildConstructorBodies(
        NarrativeSentenceSpecification specification,
        RootAtlas atlas)
    {
        var bodies = new List<NarrativeConstructorBody>(specification.ConstructorRoles.Count);
        foreach (var role in specification.ConstructorRoles)
        {
            var entry = atlas.Entries.First(candidate =>
                string.Equals(candidate.Root.Key, role.RootKey, StringComparison.OrdinalIgnoreCase));
            var constructor = entry.SymbolicConstructors.First(candidate =>
                string.Equals(candidate.RootKey, role.RootKey, StringComparison.OrdinalIgnoreCase));

            bodies.Add(new NarrativeConstructorBody
            {
                Role = role.Role,
                Constructor = constructor
            });
        }

        return bodies;
    }

    private static EngramDraft BuildDraft(
        NarrativeSentenceSpecification specification,
        IReadOnlyList<string> resolvedRoots,
        IReadOnlyList<NarrativeOperatorAnnotation> operatorAnnotations)
    {
        return new EngramDraft
        {
            RootKey = specification.PredicateRoot,
            EpistemicClass = EngramEpistemicClass.Propositional,
            Trunk = new EngramTrunk
            {
                Segments =
                [
                    specification.DiagnosticPredicateRender,
                    .. operatorAnnotations.Select(annotation => $"{annotation.Kind}:{annotation.Token}")
                ],
                Summary = specification.Summary
            },
            Branches = specification.BranchRoles
                .Select(role => new EngramBranch
                {
                    Name = role.Role,
                    RootKey = role.RootKey,
                    SymbolicHandle = role.RootKey
                })
                .ToArray(),
            Invariants = BuildInvariants(specification),
            RequestedClosureGrade = EngramClosureGrade.Closed
        };
    }

    private static IReadOnlyList<EngramInvariant> BuildInvariants(NarrativeSentenceSpecification specification)
    {
        var invariants = new List<EngramInvariant>
        {
            new()
            {
                Key = "narrative.predicate.render",
                Statement = specification.DiagnosticPredicateRender
            }
        };

        if (specification.ScalarPayload is not null)
        {
            invariants.Add(new EngramInvariant
            {
                Key = "narrative.scalar.payload",
                Statement = specification.ScalarPayload
            });
        }

        return invariants;
    }

    private static readonly IReadOnlyDictionary<string, NarrativeSentenceSpecification> SentenceSpecifications =
        new Dictionary<string, NarrativeSentenceSpecification>(StringComparer.Ordinal)
        {
            ["The Gate remembers its makers."] = new()
            {
                RequiredRoots = ["gate", "remember", "make"],
                PredicateRoot = "remember",
                BranchRoles = [new("subject", "gate"), new("object", "make")],
                ConstructorRoles = [new("subject", "gate"), new("predicate", "remember"), new("object", "make")],
                OperatorAnnotations = [new("its", "possessive-determiner")],
                DiagnosticPredicateRender = "remember(gate,make)",
                LaneOutcome = NarrativeTranslationLaneOutcome.Closed,
                Summary = "narrative.transitive"
            },
            ["The hum has increased twelve percent."] = new()
            {
                RequiredRoots = ["hum", "increase", "percent"],
                PredicateRoot = "increase",
                BranchRoles = [new("subject", "hum"), new("unit", "percent")],
                ConstructorRoles = [new("subject", "hum"), new("predicate", "increase"), new("unit", "percent")],
                OperatorAnnotations = [new("has", "auxiliary-aspect")],
                DiagnosticPredicateRender = "increase(hum,12,percent)",
                LaneOutcome = NarrativeTranslationLaneOutcome.Closed,
                Summary = "narrative.measurement",
                ScalarPayload = "12"
            },
            ["The light was the first lie."] = new()
            {
                RequiredRoots = ["light", "lie"],
                PredicateRoot = "lie",
                BranchRoles = [new("subject", "light")],
                ConstructorRoles = [new("subject", "light"), new("predicate", "lie")],
                OperatorAnnotations = [new("was", "copula"), new("first", "qualifier")],
                DiagnosticPredicateRender = "lie(light,first)",
                LaneOutcome = NarrativeTranslationLaneOutcome.NeedsSpecification,
                Summary = "narrative.ambiguous"
            }
        };

    private sealed class NarrativeSentenceSpecification
    {
        public required IReadOnlyList<string> RequiredRoots { get; init; }
        public required string PredicateRoot { get; init; }
        public required IReadOnlyList<NarrativeBranchRole> BranchRoles { get; init; }
        public required IReadOnlyList<NarrativeBranchRole> ConstructorRoles { get; init; }
        public required IReadOnlyList<NarrativeOperatorRole> OperatorAnnotations { get; init; }
        public required string DiagnosticPredicateRender { get; init; }
        public required NarrativeTranslationLaneOutcome LaneOutcome { get; init; }
        public required string Summary { get; init; }
        public string? ScalarPayload { get; init; }
    }

    private sealed record NarrativeBranchRole(string Role, string RootKey);

    private sealed record NarrativeOperatorRole(string Token, string Kind);
}
