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

    internal sealed class CrypticPropositionFormationResult
    {
        public required PropositionalCompileAssessment PropositionAssessment { get; init; }
        public required CrypticAdmissionResult AdmissionResult { get; init; }
        public EngramClosureDecision? ClosureDecision { get; init; }
    }

    internal sealed class CrypticFormationChamber
    {
        private readonly LispNarrativeMirrorAdapter _mirrorAdapter;
        private readonly ICrypticAdmissionMembrane _admissionMembrane;
        private readonly IEngramClosureValidator _closureValidator;
        private readonly IAgentiFormationObserver? _formationObserver;

        public CrypticFormationChamber(
            IEngramClosureValidator closureValidator,
            ICrypticAdmissionMembrane admissionMembrane,
            LispNarrativeMirrorAdapter mirrorAdapter,
            IAgentiFormationObserver? formationObserver = null)
        {
            _closureValidator = closureValidator ?? throw new ArgumentNullException(nameof(closureValidator));
            _admissionMembrane = admissionMembrane ?? throw new ArgumentNullException(nameof(admissionMembrane));
            _mirrorAdapter = mirrorAdapter ?? throw new ArgumentNullException(nameof(mirrorAdapter));
            _formationObserver = formationObserver;
        }

        public CrypticFormationChamber(
            IEngramClosureValidator closureValidator,
            ICrypticAdmissionMembrane admissionMembrane,
            IAgentiFormationObserver? formationObserver = null)
            : this(
                closureValidator,
                admissionMembrane,
                new LispNarrativeMirrorAdapter(closureValidator),
                formationObserver)
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
                AgentiFormationObservationSource.Sentence,
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
                    AgentiFormationObservationSource.ParagraphGraph,
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
                    AgentiFormationObservationSource.ParagraphBody,
                    cancellationToken).ConfigureAwait(false));
            }

            return new CrypticParagraphBodyFormationResult
            {
                ParagraphBody = BuildParagraphBody(candidateBody, sentenceAdmissions),
                SentenceAdmissions = sentenceAdmissions
            };
        }

        public async Task<CrypticPropositionFormationResult> FormPropositionAsync(
            PropositionalCompileAssessment assessment,
            RootAtlas atlas,
            CrypticOriginRuntime originRuntime = CrypticOriginRuntime.OracleCSharp,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(assessment);
            ArgumentNullException.ThrowIfNull(atlas);

            var admissionCandidate = new CrypticAdmissionCandidate(
                CandidateId: CreateDeterministicCandidateId(
                    assessment.Candidate.DiagnosticPropositionRender,
                    originRuntime,
                    CrypticOriginLane.ProtectedIngressProposition),
                OriginRuntime: originRuntime,
                OriginLane: CrypticOriginLane.ProtectedIngressProposition,
                SourceText: assessment.Candidate.DiagnosticPropositionRender,
                MaterializedPayload: assessment,
                CandidateDraft: assessment.ProjectedEngramDraft,
                Outcome: MapOutcome(assessment.Grade),
                DeterministicPrimeMaterializationSucceeded: assessment.Grade != PropositionalCompileGrade.Stable ||
                                                            assessment.ProjectedEngramDraft is not null,
                ReservedDomainViolation: false,
                DiagnosticRender: assessment.Candidate.DiagnosticPropositionRender,
                TelemetryTags:
                [
                    $"runtime:{originRuntime}",
                    "lane:protected-ingress-proposition",
                    $"grade:{assessment.Grade}"
                ]);

            var admissionResult = await _admissionMembrane
                .EvaluateAsync(admissionCandidate, cancellationToken)
                .ConfigureAwait(false);

            await RecordAdmissionObservationAsync(
                admissionResult,
                AgentiFormationObservationSource.ProtectedIngressProposition,
                cancellationToken).ConfigureAwait(false);

            if (admissionResult.Decision != CrypticAdmissionDecision.Admit ||
                admissionResult.NormalizedPrimePayload is null)
            {
                await RecordClosureObservationAsync(
                    admissionResult,
                    closureState: AgentiFormationClosureState.NoClosure,
                    AgentiFormationObservationSource.ProtectedIngressProposition,
                    cancellationToken).ConfigureAwait(false);

                return new CrypticPropositionFormationResult
                {
                    PropositionAssessment = assessment,
                    AdmissionResult = admissionResult
                };
            }

            var closureDecision = await _closureValidator
                .ValidateAsync(admissionResult.NormalizedPrimePayload.EngramDraft, atlas, cancellationToken)
                .ConfigureAwait(false);

            await RecordClosureObservationAsync(
                admissionResult,
                closureDecision.Grade == EngramClosureGrade.Closed
                    ? AgentiFormationClosureState.Closed
                    : AgentiFormationClosureState.Rejected,
                AgentiFormationObservationSource.ProtectedIngressProposition,
                cancellationToken).ConfigureAwait(false);

            return new CrypticPropositionFormationResult
            {
                PropositionAssessment = new PropositionalCompileAssessment
                {
                    Candidate = assessment.Candidate,
                    Grade = assessment.Grade,
                    ReasonCodes = assessment.ReasonCodes,
                    Warnings = assessment.Warnings,
                    ProjectedEngramDraft = admissionResult.NormalizedPrimePayload.EngramDraft
                },
                AdmissionResult = admissionResult,
                ClosureDecision = closureDecision
            };
        }

        private async Task<CrypticSentenceFormationResult> EvaluateSentenceCandidateAsync(
            string sourceText,
            RootAtlas atlas,
            IReadOnlyList<NarrativeOverlayRoot> overlayRoots,
            NarrativeTranslationLaneResult candidateResult,
            CrypticOriginRuntime originRuntime,
            AgentiFormationObservationSource observationSource,
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

            await RecordAdmissionObservationAsync(
                admissionResult,
                observationSource,
                cancellationToken).ConfigureAwait(false);

            if (admissionResult.Decision != CrypticAdmissionDecision.Admit ||
                admissionResult.NormalizedPrimePayload is null)
            {
                await RecordClosureObservationAsync(
                    admissionResult,
                    closureState: AgentiFormationClosureState.NoClosure,
                    observationSource,
                    cancellationToken).ConfigureAwait(false);

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

            await RecordClosureObservationAsync(
                admissionResult,
                closureDecision.Grade == EngramClosureGrade.Closed
                    ? AgentiFormationClosureState.Closed
                    : AgentiFormationClosureState.Rejected,
                observationSource,
                cancellationToken).ConfigureAwait(false);

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

        private async Task RecordAdmissionObservationAsync(
            CrypticAdmissionResult admissionResult,
            AgentiFormationObservationSource source,
            CancellationToken cancellationToken)
        {
            if (_formationObserver is null)
            {
                return;
            }

            var tags = admissionResult.TelemetryTags
                .Append($"reason:{admissionResult.ReasonCode}")
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            await _formationObserver.RecordAsync(
                new AgentiFormationObservation(
                    ObservationId: Guid.NewGuid(),
                    Stage: AgentiFormationObservationStage.CrypticAdmission,
                    CandidateId: admissionResult.CandidateId,
                    BootClass: null,
                    ActivationState: null,
                    ExpansionRights: null,
                    Office: null,
                    AdmissionDecision: admissionResult.Decision,
                    ClosureState: AgentiFormationClosureState.NotSubmitted,
                    RevealMode: null,
                    OriginRuntime: MapOriginRuntime(admissionResult.OriginRuntime),
                    Source: source,
                    SubmissionEligible: admissionResult.SubmissionEligible,
                    ObservationTags: tags,
                    Timestamp: DateTimeOffset.UtcNow),
                cancellationToken).ConfigureAwait(false);
        }

        private async Task RecordClosureObservationAsync(
            CrypticAdmissionResult admissionResult,
            AgentiFormationClosureState closureState,
            AgentiFormationObservationSource source,
            CancellationToken cancellationToken)
        {
            if (_formationObserver is null)
            {
                return;
            }

            var tags = admissionResult.TelemetryTags
                .Append($"reason:{admissionResult.ReasonCode}")
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            await _formationObserver.RecordAsync(
                new AgentiFormationObservation(
                    ObservationId: Guid.NewGuid(),
                    Stage: AgentiFormationObservationStage.PrimeClosure,
                    CandidateId: admissionResult.CandidateId,
                    BootClass: null,
                    ActivationState: null,
                    ExpansionRights: null,
                    Office: null,
                    AdmissionDecision: admissionResult.Decision,
                    ClosureState: closureState,
                    RevealMode: null,
                    OriginRuntime: MapOriginRuntime(admissionResult.OriginRuntime),
                    Source: source,
                    SubmissionEligible: admissionResult.SubmissionEligible,
                    ObservationTags: tags,
                    Timestamp: DateTimeOffset.UtcNow),
                cancellationToken).ConfigureAwait(false);
        }

        private static AgentiFormationOriginRuntime MapOriginRuntime(CrypticOriginRuntime originRuntime)
        {
            return originRuntime switch
            {
                CrypticOriginRuntime.OracleCSharp => AgentiFormationOriginRuntime.OracleCSharp,
                CrypticOriginRuntime.Lisp => AgentiFormationOriginRuntime.Lisp,
                _ => throw new ArgumentOutOfRangeException(nameof(originRuntime), originRuntime, null)
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

        private static CrypticFormationOutcome MapOutcome(PropositionalCompileGrade grade)
        {
            return grade switch
            {
                PropositionalCompileGrade.Stable => CrypticFormationOutcome.Closed,
                PropositionalCompileGrade.NeedsSpecification => CrypticFormationOutcome.NeedsSpecification,
                PropositionalCompileGrade.Rejected => CrypticFormationOutcome.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, "Unsupported proposition compile grade.")
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
