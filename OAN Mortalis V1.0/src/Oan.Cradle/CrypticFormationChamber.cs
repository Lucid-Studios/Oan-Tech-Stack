using System.Security.Cryptography;
using System.Text;
using GEL.Contracts;
using GEL.Graphs;
using GEL.Models;
using Oan.Common;
using SLI.Engine.Morphology;
using SLI.Ingestion;

namespace Oan.Cradle
{
    internal sealed class CrypticSentenceFormationResult
    {
        public required NarrativeTranslationLaneResult SentenceResult { get; init; }
        public required CrypticAdmissionResult AdmissionResult { get; init; }
        public EngramClosureDecision? ClosureDecision { get; init; }
    }

    internal sealed class CrypticParagraphFormationResult
    {
        public required NarrativeParagraphLaneResult ParagraphResult { get; init; }
        public required IReadOnlyList<CrypticSentenceFormationResult> SentenceAdmissions { get; init; }
    }

    internal sealed class CrypticParagraphBodyFormationResult
    {
        public required NarrativeParagraphBody ParagraphBody { get; init; }
        public required IReadOnlyList<CrypticSentenceFormationResult> SentenceAdmissions { get; init; }
    }

    internal sealed class CrypticFormationChamber
    {
        private readonly LispNarrativeMirrorAdapter _mirrorAdapter;
        private readonly ICrypticAdmissionMembrane _admissionMembrane;
        private readonly IEngramClosureValidator _closureValidator;

        public CrypticFormationChamber(
            IEngramClosureValidator closureValidator,
            ICrypticAdmissionMembrane admissionMembrane,
            LispNarrativeMirrorAdapter mirrorAdapter)
        {
            _closureValidator = closureValidator ?? throw new ArgumentNullException(nameof(closureValidator));
            _admissionMembrane = admissionMembrane ?? throw new ArgumentNullException(nameof(admissionMembrane));
            _mirrorAdapter = mirrorAdapter ?? throw new ArgumentNullException(nameof(mirrorAdapter));
        }

        public CrypticFormationChamber(
            IEngramClosureValidator closureValidator,
            ICrypticAdmissionMembrane admissionMembrane)
            : this(
                closureValidator,
                admissionMembrane,
                new LispNarrativeMirrorAdapter(closureValidator))
        {
        }

        public async Task<CrypticSentenceFormationResult> FormSentenceAsync(
            string sentence,
            RootAtlas atlas,
            IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
            CancellationToken cancellationToken = default)
        {
            var candidateResult = await _mirrorAdapter
                .TranslateSentenceCandidateAsync(sentence, atlas, overlayRoots, cancellationToken)
                .ConfigureAwait(false);

            return await EvaluateSentenceCandidateAsync(
                sentence,
                atlas,
                overlayRoots,
                candidateResult,
                CrypticOriginRuntime.Lisp,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<CrypticParagraphFormationResult> FormParagraphAsync(
            string paragraph,
            RootAtlas atlas,
            IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
            CancellationToken cancellationToken = default)
        {
            var candidateParagraph = await _mirrorAdapter
                .TranslateParagraphCandidateAsync(paragraph, atlas, overlayRoots, cancellationToken)
                .ConfigureAwait(false);

            var sentenceAdmissions = new List<CrypticSentenceFormationResult>(candidateParagraph.SentenceResults.Count);
            foreach (var sentenceResult in candidateParagraph.SentenceResults)
            {
                sentenceAdmissions.Add(await EvaluateSentenceCandidateAsync(
                    sentenceResult.Sentence,
                    atlas,
                    overlayRoots,
                    sentenceResult,
                    CrypticOriginRuntime.Lisp,
                    cancellationToken).ConfigureAwait(false));
            }

            return new CrypticParagraphFormationResult
            {
                ParagraphResult = BuildParagraphResult(paragraph, candidateParagraph.ParagraphGraph.Edges, sentenceAdmissions),
                SentenceAdmissions = sentenceAdmissions
            };
        }

        public async Task<CrypticParagraphBodyFormationResult> FormParagraphBodyAsync(
            string paragraph,
            RootAtlas atlas,
            IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
            CancellationToken cancellationToken = default)
        {
            var candidateBody = await _mirrorAdapter
                .TranslateParagraphBodyCandidateAsync(paragraph, atlas, overlayRoots, cancellationToken)
                .ConfigureAwait(false);

            var sentenceAdmissions = new List<CrypticSentenceFormationResult>(candidateBody.SentenceResults.Count);
            foreach (var sentenceResult in candidateBody.SentenceResults)
            {
                sentenceAdmissions.Add(await EvaluateSentenceCandidateAsync(
                    sentenceResult.Sentence,
                    atlas,
                    overlayRoots,
                    sentenceResult,
                    CrypticOriginRuntime.Lisp,
                    cancellationToken).ConfigureAwait(false));
            }

            return new CrypticParagraphBodyFormationResult
            {
                ParagraphBody = BuildParagraphBody(candidateBody, sentenceAdmissions),
                SentenceAdmissions = sentenceAdmissions
            };
        }

        private async Task<CrypticSentenceFormationResult> EvaluateSentenceCandidateAsync(
            string sourceText,
            RootAtlas atlas,
            IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
            NarrativeTranslationLaneResult candidateResult,
            CrypticOriginRuntime originRuntime,
            CancellationToken cancellationToken)
        {
            var admissionCandidate = new CrypticAdmissionCandidate(
                CandidateId: CreateDeterministicCandidateId(sourceText, originRuntime, CrypticOriginLane.Sentence),
                OriginRuntime: originRuntime,
                OriginLane: CrypticOriginLane.Sentence,
                SourceText: sourceText,
                MaterializedPayload: candidateResult,
                CandidateDraft: candidateResult.EngramDraft,
                Outcome: MapOutcome(candidateResult.LaneOutcome),
                DeterministicPrimeMaterializationSucceeded: candidateResult.LaneOutcome != NarrativeTranslationLaneOutcome.Closed || candidateResult.EngramDraft is not null,
                ReservedDomainViolation: false,
                DiagnosticRender: candidateResult.DiagnosticPredicateRender,
                TelemetryTags:
                [
                    $"runtime:{originRuntime}",
                    "lane:sentence",
                    $"outcome:{candidateResult.LaneOutcome}"
                ]);

            var admissionResult = await _admissionMembrane
                .EvaluateAsync(admissionCandidate, cancellationToken)
                .ConfigureAwait(false);

            if (admissionResult.Decision != CrypticAdmissionDecision.Admit ||
                admissionResult.NormalizedPrimePayload is null)
            {
                return new CrypticSentenceFormationResult
                {
                    SentenceResult = candidateResult,
                    AdmissionResult = admissionResult
                };
            }

            var workingAtlas = BuildOverlayAtlas(atlas, overlayRoots);
            var closureDecision = await _closureValidator
                .ValidateAsync(admissionResult.NormalizedPrimePayload.EngramDraft, workingAtlas, cancellationToken)
                .ConfigureAwait(false);

            return new CrypticSentenceFormationResult
            {
                SentenceResult = new NarrativeTranslationLaneResult
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
                    EngramDraft = admissionResult.NormalizedPrimePayload.EngramDraft,
                    ClosureDecision = closureDecision
                },
                AdmissionResult = admissionResult,
                ClosureDecision = closureDecision
            };
        }

        private static NarrativeParagraphLaneResult BuildParagraphResult(
            string paragraph,
            IReadOnlyList<ConstructorEdge> graphEdges,
            IReadOnlyList<CrypticSentenceFormationResult> sentenceAdmissions)
        {
            var sentenceResults = sentenceAdmissions.Select(admission => admission.SentenceResult).ToArray();
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
            NarrativeParagraphBody candidateBody,
            IReadOnlyList<CrypticSentenceFormationResult> sentenceAdmissions)
        {
            var finalizedParagraph = BuildParagraphResult(
                candidateBody.Paragraph,
                candidateBody.ParagraphGraph.Edges,
                sentenceAdmissions);

            return new NarrativeParagraphBody
            {
                Paragraph = candidateBody.Paragraph,
                SentenceResults = finalizedParagraph.SentenceResults,
                ParagraphGraph = candidateBody.ParagraphGraph,
                ParagraphInvariants = candidateBody.ParagraphInvariants,
                ContinuityAnchors = candidateBody.ContinuityAnchors,
                BodySummary = candidateBody.BodySummary,
                DraftCluster = new NarrativeDraftCluster
                {
                    MemberDrafts = finalizedParagraph.GeneratedDrafts,
                    MemberClosureDecisions = finalizedParagraph.ClosureDecisions,
                    AmbiguousSentenceKeys = candidateBody.DraftCluster.AmbiguousSentenceKeys,
                    ClusterDiagnosticRender = candidateBody.DraftCluster.ClusterDiagnosticRender
                }
            };
        }

        private static CrypticFormationOutcome MapOutcome(NarrativeTranslationLaneOutcome laneOutcome)
        {
            return laneOutcome switch
            {
                NarrativeTranslationLaneOutcome.Closed => CrypticFormationOutcome.Closed,
                NarrativeTranslationLaneOutcome.NeedsSpecification => CrypticFormationOutcome.NeedsSpecification,
                NarrativeTranslationLaneOutcome.Rejected => CrypticFormationOutcome.Rejected,
                NarrativeTranslationLaneOutcome.OutOfScope => CrypticFormationOutcome.OutOfScope,
                _ => throw new ArgumentOutOfRangeException(nameof(laneOutcome), laneOutcome, "Unsupported lane outcome.")
            };
        }

        private static Guid CreateDeterministicCandidateId(
            string sourceText,
            CrypticOriginRuntime originRuntime,
            CrypticOriginLane originLane)
        {
            var bytes = Encoding.UTF8.GetBytes($"{originRuntime}|{originLane}|{sourceText.Trim()}");
            var hash = MD5.HashData(bytes);
            return new Guid(hash);
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
                $"{baseAtlas.Version}.cryptic-chamber-fixture",
                baseAtlas.Entries.Concat(overlayEntries),
                baseAtlas.RefinementEdges,
                domains);
        }
    }
}
