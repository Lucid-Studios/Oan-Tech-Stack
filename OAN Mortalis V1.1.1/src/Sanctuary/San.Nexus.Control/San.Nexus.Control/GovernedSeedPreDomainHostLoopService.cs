using System.Security.Cryptography;
using System.Text;
using San.Common;
using SLI.Engine;

namespace San.Nexus.Control;

public interface IGovernedSeedPreDomainHostLoopService
{
    (
        GovernedSeedCrypticHoldingInspectionReceipt HoldingInspectionReceipt,
        GovernedSeedFormOrCleaveAssessment FormOrCleaveAssessment,
        GovernedSeedPreDomainHostLoopReceipt HostLoopReceipt)
    Evaluate(
        FormationReceipt formationReceipt,
        ListeningFrameProjectionPacket listeningFrame,
        CompassProjectionPacket compassProjection,
        EngineeredCognitionFirstPrimeStateReceipt firstPrimeReceipt,
        PrimeSeedStateReceipt primeSeedReceipt);
}

public sealed class GovernedSeedPreDomainHostLoopService : IGovernedSeedPreDomainHostLoopService
{
    private readonly IGovernedSeedCrypticHoldingService _crypticHoldingService;
    private readonly IGovernedSeedFormOrCleaveService _formOrCleaveService;

    public GovernedSeedPreDomainHostLoopService(
        IGovernedSeedCrypticHoldingService crypticHoldingService,
        IGovernedSeedFormOrCleaveService formOrCleaveService)
    {
        _crypticHoldingService = crypticHoldingService ?? throw new ArgumentNullException(nameof(crypticHoldingService));
        _formOrCleaveService = formOrCleaveService ?? throw new ArgumentNullException(nameof(formOrCleaveService));
    }

    public (
        GovernedSeedCrypticHoldingInspectionReceipt HoldingInspectionReceipt,
        GovernedSeedFormOrCleaveAssessment FormOrCleaveAssessment,
        GovernedSeedPreDomainHostLoopReceipt HostLoopReceipt)
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

        var holdingInspection = _crypticHoldingService.Inspect(formationReceipt, listeningFrame, compassProjection);
        var formOrCleave = primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding
            ? _formOrCleaveService.Evaluate(formationReceipt, listeningFrame, compassProjection, holdingInspection)
            : CreateNonReadyAssessment(formationReceipt, listeningFrame, compassProjection, holdingInspection);

        var hostLoopReceipt = CreateHostLoopReceipt(firstPrimeReceipt, primeSeedReceipt, holdingInspection, formOrCleave);
        return (holdingInspection, formOrCleave, hostLoopReceipt);
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

    private static GovernedSeedPreDomainHostLoopReceipt CreateHostLoopReceipt(
        EngineeredCognitionFirstPrimeStateReceipt firstPrimeReceipt,
        PrimeSeedStateReceipt primeSeedReceipt,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspection,
        GovernedSeedFormOrCleaveAssessment formOrCleave)
    {
        var seedReady = primeSeedReceipt.SeedState == PrimeSeedStateKind.PrimeSeedPreDomainStanding;
        var carryDisposition = seedReady
            ? formOrCleave.CarryDisposition
            : GovernedSeedCarryDispositionKind.None;
        var collapseDisposition = seedReady
            ? formOrCleave.CollapseDisposition
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
            CrypticHoldingInspectionHandle: holdingInspection.ReceiptHandle,
            FormOrCleaveAssessmentHandle: formOrCleave.AssessmentHandle,
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
}
