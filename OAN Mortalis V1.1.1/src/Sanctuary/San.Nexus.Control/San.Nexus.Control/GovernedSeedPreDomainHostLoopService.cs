using System.Security.Cryptography;
using System.Text;
using San.Common;
using SLI.Engine;

namespace San.Nexus.Control;

public interface IGovernedSeedPreDomainHostLoopService
{
    GovernedSeedPreDomainHostLoopEvaluation
    Evaluate(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection,
        EngineeredCognitionFirstPrimeStateReceipt firstPrimeReceipt,
        PrimeSeedStateReceipt primeSeedReceipt);
}

public sealed record GovernedSeedPreDomainHostLoopEvaluation(
    GovernedSeedCandidateBoundaryReceipt CandidateBoundaryReceipt,
    GovernedSeedCrypticHoldingInspectionReceipt HoldingInspectionReceipt,
    GovernedSeedFormOrCleaveAssessment FormOrCleaveAssessment,
    GovernedSeedCandidateSeparationReceipt? CandidateSeparationReceipt,
    PrimeCrypticDuplexGovernanceReceipt? DuplexGovernanceReceipt,
    PrimeSeedPreDomainAdmissionGateReceipt? AdmissionGateReceipt,
    GovernedSeedPreDomainHostLoopReceipt HostLoopReceipt);

public sealed class GovernedSeedPreDomainHostLoopService : IGovernedSeedPreDomainHostLoopService
{
    private readonly IGovernedSeedCrypticHoldingService _crypticHoldingService;
    private readonly IGovernedSeedFormOrCleaveService _formOrCleaveService;
    private readonly IGovernedSeedCandidateSeparationService _candidateSeparationService;
    private readonly IPrimeSeedPreDomainAdmissionGateService _admissionGateService;

    public GovernedSeedPreDomainHostLoopService(
        IGovernedSeedCrypticHoldingService crypticHoldingService,
        IGovernedSeedFormOrCleaveService formOrCleaveService,
        IGovernedSeedCandidateSeparationService candidateSeparationService,
        IPrimeSeedPreDomainAdmissionGateService admissionGateService)
    {
        _crypticHoldingService = crypticHoldingService ?? throw new ArgumentNullException(nameof(crypticHoldingService));
        _formOrCleaveService = formOrCleaveService ?? throw new ArgumentNullException(nameof(formOrCleaveService));
        _candidateSeparationService = candidateSeparationService ?? throw new ArgumentNullException(nameof(candidateSeparationService));
        _admissionGateService = admissionGateService ?? throw new ArgumentNullException(nameof(admissionGateService));
    }

    public GovernedSeedPreDomainHostLoopEvaluation
        Evaluate(
            FormationReceipt formationReceipt,
            ListeningFrameProjectionPacket listeningFrame,
            CompassProjectionPacket compassProjection,
            EngineeredCognitionFirstPrimeStateReceipt firstPrimeReceipt,
            PrimeSeedStateReceipt primeSeedReceipt)
    {
        ArgumentNullException.ThrowIfNull(formationReceipt);
        ArgumentNullException.ThrowIfNull(listeningFrame);
        ArgumentNullException.ThrowIfNull(compassProjection);
        ArgumentNullException.ThrowIfNull(firstPrimeReceipt);
        ArgumentNullException.ThrowIfNull(primeSeedReceipt);

        var candidateEnvelope = CreateCandidateEnvelope(formationReceipt, compassProjection);
        var candidateBoundaryReceipt = CreateCandidateBoundaryReceipt(candidateEnvelope);
        var holdingInspection = _crypticHoldingService.Inspect(formationReceipt, listeningFrame, compassProjection);
        var formOrCleave = primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding
            ? _formOrCleaveService.Evaluate(formationReceipt, listeningFrame, compassProjection, holdingInspection)
            : CreateNonReadyAssessment(formationReceipt, listeningFrame, compassProjection, holdingInspection);
        var separation = _candidateSeparationService.Separate(
            candidateEnvelope,
            primeSeedReceipt,
            holdingInspection,
            formOrCleave);
        var separationReceipt = separation.SeparationReceipt;
        var duplexReceipt = separation.DuplexReceipt;

        var admissionGate = _admissionGateService.Evaluate(
            separation.PrimeView,
            separation.CrypticView,
            primeSeedReceipt,
            CreateRevalidationContext(primeSeedReceipt, formOrCleave));
        var admissionGateReceipt = admissionGate.Receipt;

        var hostLoopReceipt = CreateHostLoopReceipt(
            firstPrimeReceipt,
            primeSeedReceipt,
            candidateBoundaryReceipt,
            holdingInspection,
            formOrCleave,
            separationReceipt,
            duplexReceipt,
            admissionGateReceipt);
        return new GovernedSeedPreDomainHostLoopEvaluation(
            candidateBoundaryReceipt,
            holdingInspection,
            formOrCleave,
            separationReceipt,
            duplexReceipt,
            admissionGateReceipt,
            hostLoopReceipt);
    }

    private static GovernedSeedFormOrCleaveAssessment CreateNonReadyAssessment(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspection)
    {
        return new GovernedSeedFormOrCleaveAssessment(
            AssessmentHandle: CreateHandle(
                "governed-seed-form-or-cleave://",
                formationReceipt.ReceiptHandle,
                "non-ready"),
            FormationReceiptHandle: formationReceipt.ReceiptHandle,
            ListeningFrameHandle: listeningFrame.ListeningFrameHandle,
            CompassPacketHandle: compassProjection.PacketHandle,
            HoldingReceiptHandle: holdingInspection.ReceiptHandle,
            Disposition: GovernedSeedFormOrCleaveDispositionKind.Hold,
            CarryDisposition: GovernedSeedCarryDispositionKind.None,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            DescendantCandidates: [],
            CandidateOnly: true,
            ReasonCode: "governed-seed-form-or-cleave-seed-not-ready",
            LawfulBasis: "pre-domain host loop may not advance beyond candidate-only hold until PrimeSeedPreDomainStanding is explicit.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedCandidateEnvelope CreateCandidateEnvelope(
        FormationReceipt formationReceipt,
        CompassProjectionPacket compassProjection)
    {
        var candidateProposals = compassProjection.CandidateInputs
            .Select(static input => (IGovernedSeedCandidateProposal)new CandidateProposal(
                input.InputHandle,
                input.InputKind,
                input.SourceReason))
            .ToArray();
        var resonanceObservations = formationReceipt.OrientationEvidenceSignatures
            .Select(static signature => (IGovernedSeedResonanceObservation)new ResonanceObservation(
                signature.SignatureHandle,
                signature.SignatureKind.ToString(),
                string.Join("; ", signature.Notes)))
            .ToArray();
        var descendantProposals = formationReceipt.LawfulKnownItems
            .Select(static item => (IGovernedSeedDescendantProposal)new DescendantProposal(
                item,
                "lawful-known-item",
                item))
            .ToArray();
        var collapseSuggestions = formationReceipt.Refused || formationReceipt.Deferred
            ? new IGovernedSeedCollapseSuggestion[]
            {
                new CollapseSuggestion(
                    formationReceipt.ReceiptHandle,
                    formationReceipt.Refused ? "refusal-signal" : "deferred-signal",
                    formationReceipt.ReasonCode)
            }
            : Array.Empty<IGovernedSeedCollapseSuggestion>();

        return new GovernedSeedCandidateEnvelope(
            CandidateId: CreateHandle(
                "governed-seed-candidate-envelope://",
                formationReceipt.ReceiptHandle,
                compassProjection.PacketHandle),
            SourceType: GovernedSeedCandidateSourceType.HostGenerated,
            SourceChannel: "governed-pre-domain-host-loop",
            ObservedAtUtc: DateTimeOffset.UtcNow,
            PriorContinuityReference: formationReceipt.ReceiptHandle,
            CandidateProposals: candidateProposals,
            HoldingMutationProposals: Array.Empty<IGovernedSeedCrypticHoldingMutationProposal>(),
            ResonanceObservations: resonanceObservations,
            DescendantProposals: descendantProposals,
            CollapseSuggestions: collapseSuggestions);
    }

    private static GovernedSeedCandidateBoundaryReceipt CreateCandidateBoundaryReceipt(
        GovernedSeedCandidateEnvelope candidateEnvelope)
    {
        return new GovernedSeedCandidateBoundaryReceipt(
            ReceiptHandle: CreateHandle(
                "governed-seed-candidate-boundary://",
                candidateEnvelope.CandidateId,
                candidateEnvelope.SourceChannel),
            CandidateId: candidateEnvelope.CandidateId,
            SourceType: candidateEnvelope.SourceType,
            SourceChannel: candidateEnvelope.SourceChannel,
            ObservedAtUtc: candidateEnvelope.ObservedAtUtc,
            ContainsAuthorityBearingFields: false,
            CandidateProposalCount: candidateEnvelope.CandidateProposals.Count,
            HoldingMutationProposalCount: candidateEnvelope.HoldingMutationProposals.Count,
            ResonanceObservationCount: candidateEnvelope.ResonanceObservations.Count,
            DescendantProposalCount: candidateEnvelope.DescendantProposals.Count,
            CollapseSuggestionCount: candidateEnvelope.CollapseSuggestions.Count,
            Summary: "Candidate envelope entered the pre-domain host loop as candidate-only proposal material.");
    }

    private static GovernedSeedRevalidationContext CreateRevalidationContext(
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedFormOrCleaveAssessment formOrCleave)
    {
        return new GovernedSeedRevalidationContext(
            ContextHandle: CreateHandle(
                "governed-seed-revalidation://",
                primeSeedReceipt.ReceiptHandle,
                formOrCleave.AssessmentHandle),
            ContextProfile: "pre-domain-host-loop",
            RevalidationSatisfied: primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding &&
                                   formOrCleave.CandidateOnly,
            Summary: "Pre-domain host loop revalidation remains bounded to candidate-only standing.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static GovernedSeedPreDomainHostLoopReceipt CreateHostLoopReceipt(
        EngineeredCognitionFirstPrimeStateReceipt firstPrimeReceipt,
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedCandidateBoundaryReceipt boundaryReceipt,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspection,
        GovernedSeedFormOrCleaveAssessment formOrCleave,
        GovernedSeedCandidateSeparationReceipt? separationReceipt,
        PrimeCrypticDuplexGovernanceReceipt? duplexReceipt,
        PrimeSeedPreDomainAdmissionGateReceipt? admissionGateReceipt)
    {
        var seedReady = primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var carryDisposition = seedReady
            ? admissionGateReceipt?.Disposition switch
            {
                PrimeSeedPreDomainAdmissionDisposition.CarryCrypticOnly => GovernedSeedCarryDispositionKind.Carry,
                PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate => formOrCleave.CarryDisposition,
                PrimeSeedPreDomainAdmissionDisposition.Refuse => GovernedSeedCarryDispositionKind.Refuse,
                _ => formOrCleave.CarryDisposition
            }
            : GovernedSeedCarryDispositionKind.None;
        var collapseDisposition = seedReady
            ? admissionGateReceipt?.Disposition == PrimeSeedPreDomainAdmissionDisposition.Refuse
                ? GovernedSeedCollapseDispositionKind.Refuse
                : formOrCleave.CollapseDisposition
            : GovernedSeedCollapseDispositionKind.None;
        var candidateHandles = formOrCleave.DescendantCandidates.Select(static candidate => candidate.CandidateHandle).ToArray();

        return new GovernedSeedPreDomainHostLoopReceipt(
            ReceiptHandle: CreateHandle(
                "governed-seed-pre-domain-host-loop://",
                firstPrimeReceipt.ReceiptHandle,
                primeSeedReceipt.ReceiptHandle,
                formOrCleave.AssessmentHandle),
            FirstPrimeReceiptHandle: firstPrimeReceipt.ReceiptHandle,
            PrimeSeedReceiptHandle: primeSeedReceipt.ReceiptHandle,
            PreDomainGovernancePacketHandle: null,
            CandidateBoundaryReceiptHandle: boundaryReceipt.ReceiptHandle,
            CrypticHoldingInspectionHandle: holdingInspection.ReceiptHandle,
            FormOrCleaveAssessmentHandle: formOrCleave.AssessmentHandle,
            CandidateSeparationReceiptHandle: separationReceipt?.ReceiptHandle,
            DuplexGovernanceReceiptHandle: duplexReceipt?.ReceiptHandle,
            AdmissionGateReceiptHandle: admissionGateReceipt?.ReceiptHandle,
            CarryDisposition: carryDisposition,
            CollapseDisposition: collapseDisposition,
            CandidateHandles: candidateHandles,
            SeedReady: seedReady,
            CandidateOnly: true,
            DomainAdmissionWithheld: true,
            ActionAuthorityWithheld: true,
            ReasonCode: !seedReady
                ? "governed-seed-pre-domain-host-loop-seed-not-ready"
                : carryDisposition switch
                {
                    GovernedSeedCarryDispositionKind.Carry => "governed-seed-pre-domain-host-loop-carry",
                    GovernedSeedCarryDispositionKind.Cleave => "governed-seed-pre-domain-host-loop-cleave",
                    GovernedSeedCarryDispositionKind.Refuse => "governed-seed-pre-domain-host-loop-refuse",
                    _ => "governed-seed-pre-domain-host-loop-hold"
                },
            LawfulBasis: !seedReady
                ? "pre-domain host loop must stop as candidate-only non-ready receipt whenever prime seed standing has not been reached."
                : "pre-domain host loop may inspect holding, force form-or-cleave checkpointing, and emit only candidate-only carry or collapse dispositions while domain admission and action authority remain withheld.",
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private sealed record CandidateProposal(
        string ProposalId,
        string ProposalKind,
        string Summary) : IGovernedSeedCandidateProposal;

    private sealed record ResonanceObservation(
        string ObservationId,
        string ObservationKind,
        string Summary) : IGovernedSeedResonanceObservation;

    private sealed record DescendantProposal(
        string DescendantId,
        string DescendantKind,
        string Summary) : IGovernedSeedDescendantProposal;

    private sealed record CollapseSuggestion(
        string SuggestionId,
        string SuggestionKind,
        string Summary) : IGovernedSeedCollapseSuggestion;
}
