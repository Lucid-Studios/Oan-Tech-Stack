namespace Oan.Common;

public static class StewardWorkerHandoffProjector
{
    private static readonly IReadOnlyList<WorkerReasonCode> DefaultAllowedReasonCodes =
    [
        WorkerReasonCode.NeedsSpecification,
        WorkerReasonCode.InsufficientEvidence,
        WorkerReasonCode.DeferredReview,
        WorkerReasonCode.AuthorityDenied,
        WorkerReasonCode.DisclosureScopeViolation,
        WorkerReasonCode.NoHandleNoAction,
        WorkerReasonCode.UnsupportedClaim,
        WorkerReasonCode.BrokenWindow,
        WorkerReasonCode.UnknownNotFailure,
        WorkerReasonCode.OfficeNonOverlap,
        WorkerReasonCode.PromptInjection
    ];

    private static readonly IReadOnlyDictionary<GovernedWorkerSpecies, StewardWorkerProfile> Profiles =
        new Dictionary<GovernedWorkerSpecies, StewardWorkerProfile>
        {
            [GovernedWorkerSpecies.RepoBugStewardWorker] = new(
                AuthorizingSurface: "host_truth_runtime",
                TaskKind: "repo-bug-triage",
                RequiredOutputKind: "worker-return-summary-v1",
                DeadlineOrExpiry: "loop-scoped",
                HaltConditions:
                [
                    "authority-missing",
                    "disclosure-ceiling-breach",
                    "evidence-insufficient"
                ],
                ProhibitedActions:
                [
                    "public-disclosure",
                    "host-mutation",
                    "undeclared-tool-call"
                ],
                PublicationDenial: "public-disclosure-not-authorized",
                MutationDenial: "host-mutation-not-authorized",
                MountedMemoryLanes:
                [
                    "mission-local"
                ],
                ForbiddenMemoryLanes:
                [
                    "cryptic-sealed"
                ],
                ToolAllowlist:
                [
                    "repo-read-only",
                    "receipt-inspection"
                ],
                ToolDenials:
                [
                    "network-call",
                    "mutation",
                    "publication"
                ],
                ContinuityLinkageRequirement: "office-issuance-lineage-link",
                ResidueReturnRequirement: "required-before-completion",
                RequiredWitnessSurface: "host-truth-witness",
                ReturnPacketSchema: "worker-return-packet-v1",
                ReturnDestination: "steward-governance-loop")
        };

    public static (WorkerHandoffPacket Packet, GovernedWorkerHandoffReceipt Receipt)? ProjectForLoop(
        string loopKey,
        GovernanceJournalReplayBatch batch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(batch);

        var reviewRequest = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.ReviewRequest)
            .LastOrDefault(request => request is not null);
        if (reviewRequest is null)
        {
            return null;
        }

        var weatherDisclosureReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.WeatherDisclosureReceipt)
            .LastOrDefault(receipt => receipt is not null);
        if (weatherDisclosureReceipt is null)
        {
            return null;
        }

        var authorityReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.OfficeAuthorityReceipt)
            .LastOrDefault(receipt => receipt is not null && receipt.Office == InternalGoverningCmeOffice.Steward);
        if (authorityReceipt is null)
        {
            return null;
        }

        var issuanceReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.OfficeIssuanceReceipt)
            .LastOrDefault(receipt => receipt is not null && receipt.Office == InternalGoverningCmeOffice.Steward);
        if (issuanceReceipt is null)
        {
            return null;
        }

        if (issuanceReceipt.Office != InternalGoverningCmeOffice.Steward ||
            issuanceReceipt.ConstructClass != ConstructClass.IssuedOffice ||
            authorityReceipt.ViewEligibility != OfficeViewEligibility.OfficeSpecificView ||
            authorityReceipt.RationaleCode != OfficeAuthorityRationaleCode.OfficeSpecificStewardView ||
            authorityReceipt.ActionEligibility < OfficeActionEligibility.CheckInAllowed)
        {
            return null;
        }

        if (issuanceReceipt.MaturityPosture == MaturityPosture.Withheld ||
            issuanceReceipt.AllowedActionCeiling < OfficeActionEligibility.CheckInAllowed)
        {
            return null;
        }

        if (authorityReceipt.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient ||
            weatherDisclosureReceipt.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient)
        {
            return null;
        }

        if (weatherDisclosureReceipt.WindowIntegrityState is WindowIntegrityState.JournalGap
            or WindowIntegrityState.RuntimeRestart
            or WindowIntegrityState.CmeReselected
            or WindowIntegrityState.VisibilityDowngraded
            or WindowIntegrityState.GovernanceReset)
        {
            return null;
        }

        if (!string.Equals(reviewRequest.CMEId, issuanceReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(authorityReceipt.CMEId, issuanceReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(weatherDisclosureReceipt.CMEId, issuanceReceipt.CMEId, StringComparison.Ordinal))
        {
            return null;
        }

        if (!Profiles.TryGetValue(GovernedWorkerSpecies.RepoBugStewardWorker, out var profile))
        {
            return null;
        }

        var bridgeReview = reviewRequest.BridgeReview ?? CreateFallbackBridgeReview(reviewRequest);
        var runtimeUseCeiling = reviewRequest.RuntimeUseCeiling
            ?? SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling();
        if (bridgeReview.OutcomeKind != SliBridgeOutcomeKind.Ok ||
            runtimeUseCeiling.CandidateOnly != true)
        {
            return null;
        }

        var handoffPacketId = WorkerGovernanceKeys.CreateWorkerHandoffPacketId(
            loopKey,
            issuanceReceipt.CMEId,
            GovernedWorkerSpecies.RepoBugStewardWorker,
            issuanceReceipt.IssuanceHandle,
            issuanceReceipt.OfficeInstanceId);
        var objective = string.IsNullOrWhiteSpace(reviewRequest.IntakeIntent)
            ? "repo-bug-triage"
            : reviewRequest.IntakeIntent.Trim();
        var disclosureClass = issuanceReceipt.DisclosureCeiling;
        var timestampUtc = issuanceReceipt.TimestampUtc;
        var sourceHandles = new List<string>
        {
            reviewRequest.ReturnCandidatePointer,
            reviewRequest.WorkingStateHandle,
            authorityReceipt.AuthorityHandle,
            issuanceReceipt.IssuanceHandle,
            weatherDisclosureReceipt.DisclosureHandle
        };
        if (!string.IsNullOrWhiteSpace(bridgeReview.BridgeWitnessHandle))
        {
            sourceHandles.Add(bridgeReview.BridgeWitnessHandle);
        }

        var packet = new WorkerHandoffPacket(
            HandoffPacketId: handoffPacketId,
            RequestingOffice: InternalGoverningCmeOffice.Steward,
            RequestingOfficeInstanceId: issuanceReceipt.OfficeInstanceId,
            AuthorizingSurface: profile.AuthorizingSurface,
            WorkerSpecies: GovernedWorkerSpecies.RepoBugStewardWorker,
            WorkerInstanceMode: WorkerInstanceMode.RequestOnly,
            Objective: objective,
            TaskKind: profile.TaskKind,
            SourceHandles: sourceHandles,
            RequiredOutputKind: profile.RequiredOutputKind,
            DeadlineOrExpiry: profile.DeadlineOrExpiry,
            HaltConditions: profile.HaltConditions,
            ActionCeiling: issuanceReceipt.AllowedActionCeiling,
            DisclosureClass: disclosureClass,
            AllowedReasonCodes: DefaultAllowedReasonCodes,
            ProhibitedActions: profile.ProhibitedActions,
            PublicationDenial: profile.PublicationDenial,
            MutationDenial: profile.MutationDenial,
            MountedMemoryLanes: profile.MountedMemoryLanes,
            ForbiddenMemoryLanes: profile.ForbiddenMemoryLanes,
            ToolAllowlist: profile.ToolAllowlist,
            ToolDenials: profile.ToolDenials,
            ContinuityLinkageRequirement: profile.ContinuityLinkageRequirement,
            ResidueReturnRequirement: profile.ResidueReturnRequirement,
            WitnessRequired: true,
            RequiredWitnessSurface: profile.RequiredWitnessSurface,
            ReturnPacketSchema: profile.ReturnPacketSchema,
            ReturnDestination: profile.ReturnDestination,
            ReturnVisibilityClass: disclosureClass,
            ResidueDisposition: WorkerResidueDisposition.NeedsClassification,
            EvidenceSufficiencyState: authorityReceipt.EvidenceSufficiencyState,
            MaturityPosture: MaturityPosture.DoctrineBacked,
            TimestampUtc: timestampUtc,
            BridgeReview: bridgeReview,
            RuntimeUseCeiling: runtimeUseCeiling);

        var handoffHandle = WorkerGovernanceKeys.CreateWorkerHandoffHandle(
            loopKey,
            issuanceReceipt.CMEId,
            packet.HandoffPacketId,
            issuanceReceipt.IssuanceHandle);
        var receipt = new GovernedWorkerHandoffReceipt(
            HandoffHandle: handoffHandle,
            LoopKey: loopKey,
            Stage: issuanceReceipt.Stage,
            CMEId: issuanceReceipt.CMEId,
            RequestingOffice: packet.RequestingOffice,
            RequestingOfficeInstanceId: issuanceReceipt.OfficeInstanceId,
            ConstructClass: ConstructClass.BoundedWorker,
            WorkerSpecies: packet.WorkerSpecies,
            WorkerInstanceMode: packet.WorkerInstanceMode,
            ActionCeiling: packet.ActionCeiling,
            DisclosureClass: packet.DisclosureClass,
            EvidenceSufficiencyState: packet.EvidenceSufficiencyState,
            MaturityPosture: packet.MaturityPosture,
            HandoffPacketId: packet.HandoffPacketId,
            OfficeIssuanceHandle: issuanceReceipt.IssuanceHandle,
            OfficeAuthorityHandle: issuanceReceipt.OfficeAuthorityHandle,
            WeatherDisclosureHandle: issuanceReceipt.WeatherDisclosureHandle,
            WitnessedBy: "CradleTek",
            TimestampUtc: packet.TimestampUtc,
            BridgeReview: bridgeReview,
            RuntimeUseCeiling: runtimeUseCeiling);

        return (packet, receipt);
    }

    private static SliBridgeReviewReceipt CreateFallbackBridgeReview(ReturnCandidateReviewRequest reviewRequest)
    {
        if (!string.Equals(reviewRequest.SourceTheater, reviewRequest.RequestedTheater, StringComparison.OrdinalIgnoreCase))
        {
            return SliBridgeContracts.CreateReview(
                bridgeStage: "steward-worker-handoff-fallback",
                sourceTheater: reviewRequest.SourceTheater,
                targetTheater: reviewRequest.RequestedTheater,
                bridgeWitnessHandle: reviewRequest.ReturnCandidatePointer,
                outcomeKind: SliBridgeOutcomeKind.RefuseContext,
                thresholdClass: SliBridgeThresholdClass.FaultLine,
                reasonCode: "sli-bridge-cross-theater-identification",
                refusalClass: SliBridgeRefusalClass.CrossTheaterIdentification);
        }

        return SliBridgeContracts.CreateReview(
            bridgeStage: "steward-worker-handoff-fallback",
            sourceTheater: reviewRequest.SourceTheater,
            targetTheater: reviewRequest.RequestedTheater,
            bridgeWitnessHandle: reviewRequest.ReturnCandidatePointer,
            outcomeKind: SliBridgeOutcomeKind.Ok,
            thresholdClass: SliBridgeThresholdClass.WithinBand,
            reasonCode: "sli-bridge-worker-fallback");
    }

    private sealed record StewardWorkerProfile(
        string AuthorizingSurface,
        string TaskKind,
        string RequiredOutputKind,
        string DeadlineOrExpiry,
        IReadOnlyList<string> HaltConditions,
        IReadOnlyList<string> ProhibitedActions,
        string PublicationDenial,
        string MutationDenial,
        IReadOnlyList<string> MountedMemoryLanes,
        IReadOnlyList<string> ForbiddenMemoryLanes,
        IReadOnlyList<string> ToolAllowlist,
        IReadOnlyList<string> ToolDenials,
        string ContinuityLinkageRequirement,
        string ResidueReturnRequirement,
        string RequiredWitnessSurface,
        string ReturnPacketSchema,
        string ReturnDestination);
}
