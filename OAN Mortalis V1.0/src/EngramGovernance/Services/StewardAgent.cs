using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using CradleTek.Host.Interfaces;
using EngramGovernance.Models;
using OAN.Core.Telemetry;
using Oan.Common;
using Telemetry.GEL;

namespace EngramGovernance.Services;

public sealed class StewardAgent : IReturnGovernanceAdjudicator, IDeferredReviewQueue
{
    private const string StewardIdentity = "Steward Agent";
    private readonly OntologicalCleaver _cleaver;
    private readonly EncryptionService _encryptionService;
    private readonly LedgerWriter _ledgerWriter;
    private readonly EngramBootstrapService _engramBootstrap;
    private readonly SymbolicConstructorGuidanceService _constructorGuidance;
    private readonly IPublicStore _publicStore;
    private readonly ICrypticStore _crypticStore;
    private readonly GelTelemetryAdapter _telemetry;
    private readonly IGovernanceReceiptJournal? _governanceJournal;
    private readonly List<ReturnCandidateReviewRequest> _deferredReviewQueue = [];

    public StewardAgent(
        OntologicalCleaver cleaver,
        EncryptionService encryptionService,
        LedgerWriter ledgerWriter,
        EngramBootstrapService? engramBootstrap,
        SymbolicConstructorGuidanceService? constructorGuidance,
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry,
        IGovernanceReceiptJournal? governanceJournal = null)
    {
        _cleaver = cleaver;
        _encryptionService = encryptionService;
        _ledgerWriter = ledgerWriter;
        _engramBootstrap = engramBootstrap ?? new EngramBootstrapService(ledgerWriter, publicStore, telemetry);
        _constructorGuidance = constructorGuidance ?? new SymbolicConstructorGuidanceService();
        _publicStore = publicStore;
        _crypticStore = crypticStore;
        _telemetry = telemetry;
        _governanceJournal = governanceJournal;
    }

    public async Task<EngramRecord?> ProcessCandidateAsync(
        EngramCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        Validate(candidate);
        await EmitTelemetryAsync("engram-candidate-received", candidate, cancellationToken).ConfigureAwait(false);
        var sliTokens = ExtractSliTokens(candidate.Metadata);
        var bootstrapResult = await _engramBootstrap.BootstrapAsync(candidate, sliTokens, cancellationToken).ConfigureAwait(false);
        await EmitTelemetryAsync("engram-bootstrap-complete", candidate, cancellationToken).ConfigureAwait(false);

        var decision = _cleaver.Classify(candidate);
        await EmitTelemetryAsync($"classification-{decision.Classification.ToString().ToLowerInvariant()}", candidate, cancellationToken)
            .ConfigureAwait(false);

        if (decision.Classification == EngramClassification.Discard)
        {
            await EmitTelemetryAsync("governance-rejection", candidate, cancellationToken).ConfigureAwait(false);
            return null;
        }

        var split = SplitCandidate(candidate);
        var bodyHash = EncryptionService.ComputeBodyHash(split.CognitionBody);
        var selfGelPointer = await StoreCognitionBodyAsync(candidate, split.CognitionBody, decision, cancellationToken)
            .ConfigureAwait(false);

        var decisionEntry = new OEDecisionEntry
        {
            DecisionId = CreateDeterministicDecisionId(candidate, decision.Classification, bodyHash),
            CMEId = candidate.CMEId,
            SoulFrameId = candidate.SoulFrameId,
            ContextId = candidate.ContextId,
            Classification = decision.Classification,
            BodyHash = bodyHash,
            Timestamp = DateTime.UtcNow
        };

        var symbolicTrace = ExtractMetadata(candidate.Metadata, "symbolic_trace", "[]");
        var guidanceEvaluation = _constructorGuidance.Evaluate(symbolicTrace);
        symbolicTrace = guidanceEvaluation.NormalizedTrace;

        var decisionSpline =
            $"decision:{decisionEntry.DecisionId:D}|class:{decisionEntry.Classification}|context:{decisionEntry.ContextId:D}" +
            $"|constructor:{guidanceEvaluation.ConstructorTag}|roots:{bootstrapResult.RootEngramsCreated.Count}|constructors:{bootstrapResult.ConstructorEngramsCreated.Count}";

        if (guidanceEvaluation.UsedFallback || guidanceEvaluation.WasTruncated || guidanceEvaluation.ReservedCollisionCount > 0)
        {
            await EmitTelemetryAsync("constructor-guidance-applied", candidate, cancellationToken).ConfigureAwait(false);
        }

        var compassState = ExtractCompassState(candidate.Metadata);

        var record = await _ledgerWriter
            .AppendDecisionAsync(decisionEntry, selfGelPointer, decisionSpline, symbolicTrace, compassState, cancellationToken)
            .ConfigureAwait(false);

        await RouteResidueAsync(split.CleavedResidue, record.DecisionEntry, decision.ResidueTarget, cancellationToken).ConfigureAwait(false);
        await EmitTelemetryAsync("governance-commit-success", candidate, cancellationToken).ConfigureAwait(false);
        return record;
    }

    public async Task<IReadOnlyList<ReturnCandidateReviewRequest>> ListDeferredCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        if (_governanceJournal is null)
        {
            return _deferredReviewQueue.ToArray();
        }

        var deferred = await ListDeferredAsync(cancellationToken).ConfigureAwait(false);
        var reviewRequests = new List<ReturnCandidateReviewRequest>(deferred.Count);
        foreach (var item in deferred)
        {
            var reviewRequest = await LoadReviewRequestAsync(item.LoopKey, cancellationToken).ConfigureAwait(false);
            if (reviewRequest is not null)
            {
                reviewRequests.Add(reviewRequest);
            }
        }

        return reviewRequests;
    }

    public async Task<IReadOnlyList<DeferredBacklogItemView>> ListDeferredAsync(
        CancellationToken cancellationToken = default)
    {
        if (_governanceJournal is null)
        {
            return _deferredReviewQueue
                .Select(request => new DeferredBacklogItemView(
                    GovernanceLoopKeys.Create(request.CandidateId, request.ProvenanceMarker),
                    request.CandidateId,
                    request.ProvenanceMarker,
                    DateTime.UtcNow,
                    StewardIdentity,
                    "steward.deferred.review-required",
                    LatestAnnotation: null,
                    CanReview: true))
                .ToArray();
        }

        var batch = await _governanceJournal.ReplayBatchAsync(cancellationToken).ConfigureAwait(false);
        return batch.Entries
            .GroupBy(entry => entry.LoopKey, StringComparer.Ordinal)
            .Select(CreateDeferredBacklogItem)
            .Where(item => item is not null)
            .Cast<DeferredBacklogItemView>()
            .OrderBy(item => item.DeferredAtUtc)
            .ToArray();
    }

    public async Task<DeferredBacklogItemView?> GetDeferredAsync(
        string loopKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        var items = await ListDeferredAsync(cancellationToken).ConfigureAwait(false);
        return items.FirstOrDefault(item => string.Equals(item.LoopKey, loopKey, StringComparison.Ordinal));
    }

    public async Task<GovernanceAdjudicationResult> AdjudicateAsync(
        ReturnCandidateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlSurfaceContractGuards.ValidateReturnCandidateReviewRequest(request);

        var decision = ClassifyDecision(request);
        var timestamp = DateTime.UtcNow;
        var loopKey = GovernanceLoopKeys.Create(request.CandidateId, request.ProvenanceMarker);
        var lanes = decision == GovernanceDecision.Approved
            ? GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView
            : GovernedPrimeDerivativeLane.Neither;
        var reengrammitizationAuthorized = decision == GovernanceDecision.Approved &&
                                           RequiresCrypticReengrammitization(request);

        if (decision == GovernanceDecision.Deferred)
        {
            _deferredReviewQueue.Add(request);
        }

        var mutationOutcome = decision switch
        {
            GovernanceDecision.Approved => ControlMutationOutcome.Authorized,
            GovernanceDecision.Deferred => ControlMutationOutcome.Deferred,
            _ => ControlMutationOutcome.Refused
        };
        var rationaleCode = BuildRationaleCode(request, decision);
        var mutationReceipt = ControlSurfaceContractGuards.CreateMutationReceipt(
            envelopeId: request.RequestEnvelope.EnvelopeId,
            contentHandle: request.RequestEnvelope.ActionableContent.ContentHandle,
            targetSurface: ControlSurfaceKind.GovernanceDecision,
            outcome: mutationOutcome,
            governedBy: StewardIdentity,
            decisionCode: rationaleCode,
            timestampUtc: new DateTimeOffset(timestamp, TimeSpan.Zero));

        var receipt = new GovernanceDecisionReceipt(
            CandidateId: request.CandidateId,
            IdempotencyKey: loopKey,
            CandidateProvenance: request.ProvenanceMarker,
            Decision: decision,
            AdjudicatorIdentity: StewardIdentity,
            RationaleCode: rationaleCode,
            Timestamp: timestamp,
            ReengrammitizationAuthorized: reengrammitizationAuthorized,
            PrimePublicationAuthorized: decision == GovernanceDecision.Approved,
            AuthorizedDerivativeLanes: lanes,
            MutationReceipt: mutationReceipt);

        GovernedReengrammitizationRequest? reengrammitizationRequest = null;
        GovernedPrimePublicationRequest? publicationRequest = null;

        if (decision == GovernanceDecision.Approved)
        {
            reengrammitizationRequest = CreateReengrammitizationRequest(request, receipt);
            publicationRequest = CreatePrimePublicationRequest(request, receipt);
        }

        if (_governanceJournal is not null)
        {
            var stage = decision switch
            {
                GovernanceDecision.Approved => GovernanceLoopStage.GovernanceDecisionApproved,
                GovernanceDecision.Rejected => GovernanceLoopStage.GovernanceDecisionRejected,
                GovernanceDecision.Deferred => GovernanceLoopStage.GovernanceDecisionDeferred,
                _ => GovernanceLoopStage.GovernanceDecisionRejected
            };

            await _governanceJournal.AppendAsync(
                new GovernanceJournalEntry(
                    loopKey,
                    GovernanceJournalEntryKind.Decision,
                    stage,
                    receipt.Timestamp,
                    receipt,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: request,
                    Annotation: null),
                cancellationToken).ConfigureAwait(false);

            if (decision == GovernanceDecision.Deferred)
            {
                await _governanceJournal.AppendAsync(
                    new GovernanceJournalEntry(
                        loopKey,
                        GovernanceJournalEntryKind.DeferredReview,
                        GovernanceLoopStage.GovernanceDecisionDeferred,
                        receipt.Timestamp,
                        receipt,
                        new DeferredReviewRecord(
                            loopKey,
                            request.CandidateId,
                            request.ProvenanceMarker,
                            StewardIdentity,
                            receipt.RationaleCode,
                            receipt.Timestamp),
                        ActReceipt: null,
                        ReviewRequest: request,
                        Annotation: null),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        return new GovernanceAdjudicationResult(receipt, reengrammitizationRequest, publicationRequest);
    }

    public async Task<GovernanceAdjudicationResult> ApproveDeferredAsync(
        ReviewDeferredCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ReviewDeferredAsync(request, GovernanceDecision.Approved, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GovernanceAdjudicationResult> RejectDeferredAsync(
        ReviewDeferredCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ReviewDeferredAsync(request, GovernanceDecision.Rejected, cancellationToken).ConfigureAwait(false);
    }

    public async Task AnnotateDeferredAsync(
        ReviewDeferredCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Annotation))
        {
            throw new ArgumentException("Deferred annotation requires non-empty annotation text.", nameof(request));
        }

        var deferred = await GetDeferredAsync(request.LoopKey, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Deferred loop was not found or is no longer reviewable.");

        if (_governanceJournal is not null)
        {
            await _governanceJournal.AppendAsync(
                new GovernanceJournalEntry(
                    deferred.LoopKey,
                    GovernanceJournalEntryKind.Annotation,
                    GovernanceLoopStage.GovernanceDecisionDeferred,
                    DateTime.UtcNow,
                    DecisionReceipt: null,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: null,
                    Annotation: new GovernanceDeferredAnnotation(
                        deferred.LoopKey,
                        deferred.CandidateId,
                        deferred.CandidateProvenance,
                        string.IsNullOrWhiteSpace(request.ReviewedBy) ? StewardIdentity : request.ReviewedBy.Trim(),
                        request.Annotation!,
                        DateTime.UtcNow)),
                cancellationToken).ConfigureAwait(false);
        }
    }

    public GovernedReengrammitizationRequest? CreateReengrammitizationRequest(
        ReturnCandidateReviewRequest request,
        GovernanceDecisionReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(receipt);

        if (!receipt.ReengrammitizationAuthorized)
        {
            return null;
        }

        return new GovernedReengrammitizationRequest(
            CandidateId: request.CandidateId,
            IdempotencyKey: receipt.IdempotencyKey,
            IdentityId: request.IdentityId,
            CMEId: request.CMEId,
            SourceTheater: request.SourceTheater,
            ResiduePointer: request.ReturnCandidatePointer,
            Reason: "approved-governance-cycle",
            AuthorizedBy: receipt.AdjudicatorIdentity,
            AuthorizedAtUtc: receipt.Timestamp);
    }

    public GovernedPrimePublicationRequest? CreatePrimePublicationRequest(
        ReturnCandidateReviewRequest request,
        GovernanceDecisionReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(receipt);

        if (!receipt.PrimePublicationAuthorized)
        {
            return null;
        }

        return new GovernedPrimePublicationRequest(
            CandidateId: request.CandidateId,
            IdempotencyKey: receipt.IdempotencyKey,
            IdentityId: request.IdentityId,
            PointerValue: $"prime://approved/{request.CandidateId:D}",
            CheckedViewValue: BuildCheckedView(request),
            Classification: "governed-approved",
            AuthorizedBy: receipt.AdjudicatorIdentity,
            AuthorizedAtUtc: receipt.Timestamp,
            AuthorizedLanes: receipt.AuthorizedDerivativeLanes);
    }

    private async Task<string> StoreCognitionBodyAsync(
        EngramCandidate candidate,
        string cognitionBody,
        CleavingDecision decision,
        CancellationToken cancellationToken)
    {
        var payload = _encryptionService.PrepareSelfGelPayload(
            candidate.CMEId,
            candidate.SoulFrameId,
            candidate.ContextId,
            cognitionBody,
            decision.RequiresEncryption);

        if (payload.EncryptForCrypticLayer)
        {
            var crypticPointer = await _crypticStore
                .StorePointerAsync($"cselfgel:body:{payload.StoragePointer}:{payload.BodyHash}", cancellationToken)
                .ConfigureAwait(false);
            await EmitTelemetryAsync("encryption-performed", candidate, cancellationToken).ConfigureAwait(false);
            return crypticPointer;
        }

        await _publicStore.PublishPointerAsync($"selfgel:body:{payload.StoragePointer}:{payload.BodyHash}", cancellationToken)
            .ConfigureAwait(false);
        return payload.StoragePointer;
    }

    private async Task RouteResidueAsync(
        string residue,
        OEDecisionEntry decisionEntry,
        string residueTarget,
        CancellationToken cancellationToken)
    {
        var residueHash = HashHex(residue);
        var residuePointer = $"residue:{decisionEntry.DecisionId:D}:{residueHash[..16]}";

        if (residueTarget == ResidueTargets.GoA)
        {
            await _publicStore.PublishPointerAsync($"goa:{residuePointer}", cancellationToken).ConfigureAwait(false);
            return;
        }

        if (residueTarget == ResidueTargets.cGoA)
        {
            var crypticPointer = await _crypticStore.StorePointerAsync($"cgoa:{residuePointer}", cancellationToken).ConfigureAwait(false);
            await _publicStore.PublishPointerAsync($"goa:pointer:{decisionEntry.DecisionId:D}:{crypticPointer}", cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (residueTarget == ResidueTargets.Discard)
        {
            return;
        }

        throw new InvalidOperationException($"Unsupported residue target '{residueTarget}'.");
    }

    private async Task EmitTelemetryAsync(string stage, EngramCandidate candidate, CancellationToken cancellationToken)
    {
        ITelemetryEvent telemetryEvent = new GovernanceTelemetryEvent
        {
            EventHash = HashHex($"{stage}|{candidate.CandidateId:D}|{candidate.CMEId}|{candidate.ContextId:D}"),
            Timestamp = DateTime.UtcNow
        };

        await _telemetry.AppendAsync(telemetryEvent, stage, cancellationToken).ConfigureAwait(false);
    }

    private static CandidateSplit SplitCandidate(EngramCandidate candidate)
    {
        var body = candidate.CognitionBody.Trim();
        var metadataProjection = candidate.Metadata
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => $"{kvp.Key}={kvp.Value}")
            .ToArray();

        var residue = metadataProjection.Length == 0
            ? $"context={candidate.ContextId:D};source=governance"
            : string.Join(";", metadataProjection);

        return new CandidateSplit(body, residue);
    }

    private static Guid CreateDeterministicDecisionId(
        EngramCandidate candidate,
        EngramClassification classification,
        string bodyHash)
    {
        var source = $"{candidate.CMEId}|{candidate.SoulFrameId:D}|{candidate.ContextId:D}|{classification}|{bodyHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        var guidBytes = new byte[16];
        Buffer.BlockCopy(bytes, 0, guidBytes, 0, 16);
        return new Guid(guidBytes);
    }

    private static void Validate(EngramCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate.CMEId);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate.CognitionBody);
        ArgumentNullException.ThrowIfNull(candidate.Metadata);
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static EngramCompassState ExtractCompassState(IReadOnlyDictionary<string, string> metadata)
    {
        var valueElevationRaw = ExtractMetadata(metadata, "compass_value_elevation", "Neutral");
        var valueElevation = Enum.TryParse<EngramValueElevation>(valueElevationRaw, ignoreCase: true, out var parsedElevation)
            ? parsedElevation
            : EngramValueElevation.Neutral;

        return new EngramCompassState
        {
            IdForce = ExtractDouble(metadata, "compass_id_force"),
            SuperegoConstraint = ExtractDouble(metadata, "compass_superego_constraint"),
            EgoStability = ExtractDouble(metadata, "compass_ego_stability"),
            ValueElevation = valueElevation,
            SymbolicDepth = (int)Math.Round(ExtractDouble(metadata, "compass_symbolic_depth")),
            BranchingFactor = (int)Math.Round(ExtractDouble(metadata, "compass_branching_factor")),
            DecisionEntropy = ExtractDouble(metadata, "compass_decision_entropy"),
            Timestamp = ExtractDate(metadata, "compass_timestamp")
        };
    }

    private static string ExtractMetadata(IReadOnlyDictionary<string, string> metadata, string key, string fallback)
    {
        if (metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    private static IReadOnlyList<string> ExtractSliTokens(IReadOnlyDictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue("sli_tokens", out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            if (!metadata.TryGetValue("symbolic_trace", out raw) || string.IsNullOrWhiteSpace(raw))
            {
                return [];
            }
        }

        var separators = new[] { '|', ',', ';', ' ' };
        return raw
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static double ExtractDouble(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var value) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return 0.0;
    }

    private static DateTime ExtractDate(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var value) &&
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }

    private static GovernanceDecision ClassifyDecision(ReturnCandidateReviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReturnCandidatePointer) ||
            string.IsNullOrWhiteSpace(request.ProvenanceMarker) ||
            string.IsNullOrWhiteSpace(request.SessionHandle) ||
            string.IsNullOrWhiteSpace(request.WorkingStateHandle))
        {
            return GovernanceDecision.Rejected;
        }

        if (!request.SessionHandle.StartsWith("soulframe-session://", StringComparison.Ordinal) ||
            !request.WorkingStateHandle.StartsWith("soulframe-working://", StringComparison.Ordinal) ||
            !request.ReturnCandidatePointer.StartsWith("agenticore-return://", StringComparison.Ordinal) ||
            !request.ProvenanceMarker.StartsWith("membrane-derived:", StringComparison.Ordinal))
        {
            return GovernanceDecision.Rejected;
        }

        if (request.IntakeIntent.Contains("defer", StringComparison.OrdinalIgnoreCase) ||
            request.CandidatePayload.Contains("\"decision\":\"defer\"", StringComparison.OrdinalIgnoreCase) ||
            request.CandidatePayload.Contains("\"decision\":\"review\"", StringComparison.OrdinalIgnoreCase))
        {
            return GovernanceDecision.Deferred;
        }

        return GovernanceDecision.Approved;
    }

    private static string BuildRationaleCode(ReturnCandidateReviewRequest request, GovernanceDecision decision) =>
        decision switch
        {
            GovernanceDecision.Approved => "steward.approved.governed-loop",
            GovernanceDecision.Deferred when request.IntakeIntent.Contains("defer", StringComparison.OrdinalIgnoreCase)
                => "steward.deferred.intent-review",
            GovernanceDecision.Deferred => "steward.deferred.review-required",
            _ => "steward.rejected.invalid-boundary"
        };

    private static string BuildCheckedView(ReturnCandidateReviewRequest request)
    {
        var preview = request.CandidatePayload.Length <= 96
            ? request.CandidatePayload
            : request.CandidatePayload[..96];
        return $"checked-view:{request.CandidateId:D}:{preview}";
    }

    private static bool RequiresCrypticReengrammitization(ReturnCandidateReviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.CollapseClassification.AutobiographicalRelevant ||
               request.CollapseClassification.SelfGelIdentified;
    }

    private async Task<GovernanceAdjudicationResult> ReviewDeferredAsync(
        ReviewDeferredCandidateRequest request,
        GovernanceDecision reviewedDecision,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (reviewedDecision == GovernanceDecision.Deferred)
        {
            throw new InvalidOperationException("Deferred review requires an approving or rejecting decision.");
        }

        var reviewRequest = await LoadReviewRequestAsync(request.LoopKey, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Deferred review request could not be reconstructed from audit history.");
        ControlSurfaceContractGuards.ValidateReturnCandidateReviewRequest(reviewRequest);

        var latestDeferred = await GetDeferredAsync(request.LoopKey, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Deferred loop is no longer reviewable.");

        if (!string.Equals(latestDeferred.CandidateProvenance, request.CandidateProvenance, StringComparison.Ordinal) ||
            latestDeferred.CandidateId != request.CandidateId)
        {
            throw new InvalidOperationException("Deferred review identity does not match the recorded deferred candidate.");
        }

        var timestamp = DateTime.UtcNow;
        var reengrammitizationAuthorized = reviewedDecision == GovernanceDecision.Approved &&
                                           RequiresCrypticReengrammitization(reviewRequest);
        var outcome = reviewedDecision == GovernanceDecision.Approved
            ? ControlMutationOutcome.Authorized
            : ControlMutationOutcome.Refused;
        var reviewedRationaleCode = string.IsNullOrWhiteSpace(request.RationaleCode)
            ? reviewedDecision == GovernanceDecision.Approved
                ? "steward.approved.deferred-review"
                : "steward.rejected.deferred-review"
            : request.RationaleCode.Trim();
        var mutationReceipt = ControlSurfaceContractGuards.CreateMutationReceipt(
            envelopeId: reviewRequest.RequestEnvelope.EnvelopeId,
            contentHandle: reviewRequest.RequestEnvelope.ActionableContent.ContentHandle,
            targetSurface: ControlSurfaceKind.GovernanceDecision,
            outcome: outcome,
            governedBy: StewardIdentity,
            decisionCode: reviewedRationaleCode,
            timestampUtc: new DateTimeOffset(timestamp, TimeSpan.Zero));
        var receipt = new GovernanceDecisionReceipt(
            CandidateId: reviewRequest.CandidateId,
            IdempotencyKey: request.LoopKey,
            CandidateProvenance: reviewRequest.ProvenanceMarker,
            Decision: reviewedDecision,
            AdjudicatorIdentity: StewardIdentity,
            RationaleCode: reviewedRationaleCode,
            Timestamp: timestamp,
            ReengrammitizationAuthorized: reengrammitizationAuthorized,
            PrimePublicationAuthorized: reviewedDecision == GovernanceDecision.Approved,
            AuthorizedDerivativeLanes: reviewedDecision == GovernanceDecision.Approved
                ? GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView
                : GovernedPrimeDerivativeLane.Neither,
            MutationReceipt: mutationReceipt);

        GovernedReengrammitizationRequest? reengrammitizationRequest = null;
        GovernedPrimePublicationRequest? publicationRequest = null;
        if (reviewedDecision == GovernanceDecision.Approved)
        {
            reengrammitizationRequest = CreateReengrammitizationRequest(reviewRequest, receipt);
            publicationRequest = CreatePrimePublicationRequest(reviewRequest, receipt);
        }

        if (_governanceJournal is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.Annotation))
            {
                await _governanceJournal.AppendAsync(
                    new GovernanceJournalEntry(
                        request.LoopKey,
                        GovernanceJournalEntryKind.Annotation,
                        GovernanceLoopStage.GovernanceDecisionDeferred,
                        DateTime.UtcNow,
                        DecisionReceipt: null,
                        DeferredReview: null,
                        ActReceipt: null,
                        ReviewRequest: null,
                        Annotation: new GovernanceDeferredAnnotation(
                            request.LoopKey,
                            request.CandidateId,
                            request.CandidateProvenance,
                            string.IsNullOrWhiteSpace(request.ReviewedBy) ? StewardIdentity : request.ReviewedBy.Trim(),
                            request.Annotation!,
                            DateTime.UtcNow)),
                    cancellationToken).ConfigureAwait(false);
            }

            var stage = reviewedDecision == GovernanceDecision.Approved
                ? GovernanceLoopStage.GovernanceDecisionApproved
                : GovernanceLoopStage.GovernanceDecisionRejected;

            await _governanceJournal.AppendAsync(
                new GovernanceJournalEntry(
                    request.LoopKey,
                    GovernanceJournalEntryKind.Decision,
                    stage,
                    receipt.Timestamp,
                    receipt,
                    DeferredReview: null,
                    ActReceipt: null,
                    ReviewRequest: reviewRequest,
                    Annotation: null),
                cancellationToken).ConfigureAwait(false);
        }

        _deferredReviewQueue.RemoveAll(existing => GovernanceLoopKeys.Create(existing.CandidateId, existing.ProvenanceMarker) == request.LoopKey);
        return new GovernanceAdjudicationResult(receipt, reengrammitizationRequest, publicationRequest);
    }

    private async Task<ReturnCandidateReviewRequest?> LoadReviewRequestAsync(
        string loopKey,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);

        if (_governanceJournal is null)
        {
            var inMemory = _deferredReviewQueue.FirstOrDefault(request =>
                string.Equals(GovernanceLoopKeys.Create(request.CandidateId, request.ProvenanceMarker), loopKey, StringComparison.Ordinal));
            return inMemory;
        }

        var batch = await _governanceJournal.ReplayLoopBatchAsync(loopKey, cancellationToken).ConfigureAwait(false);
        return batch.Entries
            .Where(entry => entry.ReviewRequest is not null)
            .OrderByDescending(entry => entry.Timestamp)
            .Select(entry => entry.ReviewRequest)
            .FirstOrDefault();
    }

    private static DeferredBacklogItemView? CreateDeferredBacklogItem(
        IGrouping<string, GovernanceJournalEntry> group)
    {
        var ordered = group.OrderBy(entry => entry.Timestamp).ToArray();
        var latestDecision = ordered.LastOrDefault(entry => entry.DecisionReceipt is not null)?.DecisionReceipt;
        if (latestDecision?.Decision != GovernanceDecision.Deferred)
        {
            return null;
        }

        var deferredRecord = ordered.LastOrDefault(entry => entry.DeferredReview is not null)?.DeferredReview;
        var reviewRequest = ordered.LastOrDefault(entry => entry.ReviewRequest is not null)?.ReviewRequest;
        if (deferredRecord is null || reviewRequest is null)
        {
            return null;
        }

        var latestAnnotation = ordered.LastOrDefault(entry => entry.Annotation is not null)?.Annotation?.Annotation;
        return new DeferredBacklogItemView(
            group.Key,
            deferredRecord.CandidateId,
            deferredRecord.CandidateProvenance,
            deferredRecord.DeferredAtUtc,
            deferredRecord.AdjudicatorIdentity,
            deferredRecord.RationaleCode,
            latestAnnotation,
            CanReview: true);
    }
}

internal sealed record CandidateSplit(string CognitionBody, string CleavedResidue);
