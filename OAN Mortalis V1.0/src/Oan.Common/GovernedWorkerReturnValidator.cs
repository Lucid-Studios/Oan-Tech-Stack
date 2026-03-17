namespace Oan.Common;

public static class GovernedWorkerReturnValidator
{
    public static GovernedWorkerReturnReceipt Validate(
        string loopKey,
        string cmeId,
        GovernanceLoopStage stage,
        WorkerHandoffPacket handoffPacket,
        GovernedWorkerHandoffReceipt handoffReceipt,
        WorkerReturnPacket returnPacket)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentNullException.ThrowIfNull(handoffPacket);
        ArgumentNullException.ThrowIfNull(handoffReceipt);
        ArgumentNullException.ThrowIfNull(returnPacket);

        var validationFailureCode = ValidateInternal(handoffPacket, handoffReceipt, returnPacket);
        var returnHandle = WorkerGovernanceKeys.CreateWorkerReturnHandle(
            loopKey,
            cmeId,
            returnPacket.WorkerPacketId,
            handoffReceipt.HandoffHandle);

        return new GovernedWorkerReturnReceipt(
            ReturnHandle: returnHandle,
            LoopKey: loopKey,
            Stage: stage,
            CMEId: cmeId,
            RequestingOffice: handoffPacket.RequestingOffice,
            ConstructClass: ConstructClass.BoundedWorker,
            HandoffHandle: handoffReceipt.HandoffHandle,
            HandoffPacketId: returnPacket.HandoffPacketId,
            WorkerPacketId: returnPacket.WorkerPacketId,
            WorkerSpecies: returnPacket.WorkerSpecies,
            CompletionState: returnPacket.CompletionState,
            DisclosureClass: returnPacket.DisclosureClass,
            ReasonCodes: returnPacket.ReasonCodes.ToArray(),
            EvidenceHandles: returnPacket.EvidenceHandles.ToArray(),
            UnsupportedClaimFlags: returnPacket.UnsupportedClaimFlags.ToArray(),
            ProhibitedActionAttempts: returnPacket.ProhibitedActionAttempts.ToArray(),
            ResidueDisposition: returnPacket.ResidueState,
            Validated: validationFailureCode is null,
            ValidationFailureCode: validationFailureCode,
            WitnessedBy: "CradleTek",
            TimestampUtc: returnPacket.TimestampUtc);
    }

    private static string? ValidateInternal(
        WorkerHandoffPacket handoffPacket,
        GovernedWorkerHandoffReceipt handoffReceipt,
        WorkerReturnPacket returnPacket)
    {
        if (!string.Equals(returnPacket.HandoffPacketId, handoffPacket.HandoffPacketId, StringComparison.Ordinal) ||
            !string.Equals(handoffReceipt.HandoffPacketId, handoffPacket.HandoffPacketId, StringComparison.Ordinal))
        {
            return "handoff-packet-mismatch";
        }

        if (returnPacket.WorkerSpecies != handoffPacket.WorkerSpecies)
        {
            return "worker-species-mismatch";
        }

        if (string.IsNullOrWhiteSpace(returnPacket.WorkerPacketId))
        {
            return "missing-worker-packet-id";
        }

        if (returnPacket.ReasonCodes.Any(code => !Enum.IsDefined(code)))
        {
            return "unknown-reason-code";
        }

        if (returnPacket.ReasonCodes.Any(code => !handoffPacket.AllowedReasonCodes.Contains(code)))
        {
            return "reason-code-not-allowed";
        }

        if (returnPacket.DisclosureClass > handoffPacket.ReturnVisibilityClass)
        {
            return "disclosure-ceiling-widened";
        }

        if (returnPacket.CompletionState == WorkerCompletionState.Completed &&
            returnPacket.EvidenceHandles.Count == 0)
        {
            return "missing-evidence-handle";
        }

        if (returnPacket.ExecutionClaimed)
        {
            return "unsupported-execution-claim";
        }

        if (returnPacket.MutationClaimed)
        {
            return "unsupported-mutation-claim";
        }

        if (returnPacket.ProhibitedActionAttempts.Count > 0)
        {
            return "prohibited-action-attempt";
        }

        return null;
    }
}
