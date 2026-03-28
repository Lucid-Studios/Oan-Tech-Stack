namespace Oan.Common;

public sealed record IdentityInvariantThreadRootReceipt(
    string ThreadRootHandle,
    string IdentityInvariantHandle,
    string CMEId,
    Guid IdentityId,
    string SessionHandle,
    string WorkingStateHandle,
    string ProvenanceMarker,
    string TargetTheater,
    string ContinuityClass,
    bool AmbientSharedIdentityDenied,
    bool InterWorkerBraidRequired,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record GovernedThreadBirthReceipt(
    string ThreadBirthHandle,
    string ThreadRootHandle,
    string IdentityInvariantHandle,
    string CMEId,
    IReadOnlyList<InternalGoverningCmeOffice> WitnessedOffices,
    string GovernanceLayerHandle,
    string NexusBindingHandle,
    string NexusPortalHandle,
    bool TriadicWitnessBound,
    bool MovementReleaseEligible,
    string DissolutionRule,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record InterWorkerBraidHandoffPacket(
    string BraidPacketId,
    string CMEId,
    string SourceThreadBirthHandle,
    string SourceThreadRootHandle,
    string TargetThreadBirthHandle,
    string TargetThreadRootHandle,
    string PredicateContextHandle,
    string Objective,
    IReadOnlyList<string> BridgedHandles,
    IReadOnlyList<string> WithheldIdentityHandles,
    bool AmbientSharedIdentityDenied,
    bool IdentityInheritanceDenied,
    string ReturnPath,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class WorkerThreadGovernanceProjector
{
    private const string SoulFrameSessionPrefix = "soulframe-session://";
    private const string SoulFrameWorkingPrefix = "soulframe-working://";
    private const string NexusBindingPrefix = "nexus-binding://";
    private const string NexusPortalPrefix = "nexus-portal://";

    private static readonly IReadOnlyList<InternalGoverningCmeOffice> RequiredWitnessedOffices =
    [
        InternalGoverningCmeOffice.Steward,
        InternalGoverningCmeOffice.Father,
        InternalGoverningCmeOffice.Mother
    ];

    public static IdentityInvariantThreadRootReceipt CreateIdentityInvariantThreadRoot(
        Guid identityId,
        string cmeId,
        string sessionHandle,
        string workingStateHandle,
        string provenanceMarker,
        string targetTheater,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(identityId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        EnsureHandlePrefix(sessionHandle, SoulFrameSessionPrefix, nameof(sessionHandle));
        EnsureHandlePrefix(workingStateHandle, SoulFrameWorkingPrefix, nameof(workingStateHandle));
        ArgumentException.ThrowIfNullOrWhiteSpace(provenanceMarker);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetTheater);

        var identityInvariantHandle = WorkerGovernanceKeys.CreateIdentityInvariantHandle(cmeId, identityId);
        var threadRootHandle = WorkerGovernanceKeys.CreateWorkerThreadRootHandle(cmeId, identityId, sessionHandle, workingStateHandle);

        return new IdentityInvariantThreadRootReceipt(
            ThreadRootHandle: threadRootHandle,
            IdentityInvariantHandle: identityInvariantHandle,
            CMEId: cmeId,
            IdentityId: identityId,
            SessionHandle: sessionHandle,
            WorkingStateHandle: workingStateHandle,
            ProvenanceMarker: provenanceMarker,
            TargetTheater: targetTheater,
            ContinuityClass: "identity-invariant-local-thread-root",
            AmbientSharedIdentityDenied: true,
            InterWorkerBraidRequired: true,
            ReasonCode: "identity-invariant-thread-root-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static GovernedThreadBirthReceipt CreateGovernedThreadBirthReceipt(
        IdentityInvariantThreadRootReceipt threadRoot,
        FirstBootGovernanceLayerReceipt governanceLayer,
        string nexusBindingHandle,
        string nexusPortalHandle,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(threadRoot);
        ArgumentNullException.ThrowIfNull(governanceLayer);
        EnsureHandlePrefix(nexusBindingHandle, NexusBindingPrefix, nameof(nexusBindingHandle));
        EnsureHandlePrefix(nexusPortalHandle, NexusPortalPrefix, nameof(nexusPortalHandle));

        if (!governanceLayer.RoleBoundEcesReady ||
            governanceLayer.State != FirstBootGovernanceLayerState.RoleBoundEceReady)
        {
            throw new InvalidOperationException("Governed thread birth requires a role-bound governance layer before movement begins.");
        }

        var formedOffices = governanceLayer.FormedOffices
            .Distinct()
            .ToArray();
        if (RequiredWitnessedOffices.Except(formedOffices).Any())
        {
            throw new InvalidOperationException("Governed thread birth requires Steward, Father, and Mother to be witnessed at thread birth.");
        }

        return new GovernedThreadBirthReceipt(
            ThreadBirthHandle: WorkerGovernanceKeys.CreateGovernedThreadBirthHandle(
                threadRoot.CMEId,
                threadRoot.ThreadRootHandle,
                governanceLayer.LayerHandle,
                nexusBindingHandle),
            ThreadRootHandle: threadRoot.ThreadRootHandle,
            IdentityInvariantHandle: threadRoot.IdentityInvariantHandle,
            CMEId: threadRoot.CMEId,
            WitnessedOffices: RequiredWitnessedOffices,
            GovernanceLayerHandle: governanceLayer.LayerHandle,
            NexusBindingHandle: nexusBindingHandle,
            NexusPortalHandle: nexusPortalHandle,
            TriadicWitnessBound: true,
            MovementReleaseEligible: true,
            DissolutionRule: "dissolve-through-governed-return-and-thread-root-separation",
            ReasonCode: "governed-thread-birth-triadic-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static InterWorkerBraidHandoffPacket CreateInterWorkerBraidHandoffPacket(
        GovernedThreadBirthReceipt sourceThread,
        GovernedThreadBirthReceipt targetThread,
        string predicateContextHandle,
        string objective,
        IReadOnlyList<string> bridgedHandles,
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sourceThread);
        ArgumentNullException.ThrowIfNull(targetThread);
        ArgumentException.ThrowIfNullOrWhiteSpace(predicateContextHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);
        ArgumentNullException.ThrowIfNull(bridgedHandles);

        if (!string.Equals(sourceThread.CMEId, targetThread.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Inter-worker braid handoff is currently bounded to a single CME continuity surface.");
        }

        if (string.Equals(sourceThread.ThreadRootHandle, targetThread.ThreadRootHandle, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Inter-worker braid handoff requires distinct local thread roots.");
        }

        var withheldIdentityHandles = new[]
        {
            sourceThread.ThreadRootHandle,
            sourceThread.IdentityInvariantHandle,
            targetThread.ThreadRootHandle,
            targetThread.IdentityInvariantHandle
        };

        if (bridgedHandles.Any(handle => withheldIdentityHandles.Contains(handle, StringComparer.Ordinal)))
        {
            throw new InvalidOperationException("Inter-worker braid handoff must not widen local identity handles into bridged context.");
        }

        return new InterWorkerBraidHandoffPacket(
            BraidPacketId: WorkerGovernanceKeys.CreateInterWorkerBraidHandoffPacketId(
                sourceThread.CMEId,
                sourceThread.ThreadBirthHandle,
                targetThread.ThreadBirthHandle,
                predicateContextHandle),
            CMEId: sourceThread.CMEId,
            SourceThreadBirthHandle: sourceThread.ThreadBirthHandle,
            SourceThreadRootHandle: sourceThread.ThreadRootHandle,
            TargetThreadBirthHandle: targetThread.ThreadBirthHandle,
            TargetThreadRootHandle: targetThread.ThreadRootHandle,
            PredicateContextHandle: predicateContextHandle,
            Objective: objective,
            BridgedHandles: bridgedHandles.ToArray(),
            WithheldIdentityHandles: withheldIdentityHandles,
            AmbientSharedIdentityDenied: true,
            IdentityInheritanceDenied: true,
            ReturnPath: "return-through-explicit-braid-dissolution",
            ReasonCode: "inter-worker-braid-handoff-explicit",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    private static void EnsureHandlePrefix(
        string handle,
        string prefix,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(handle);
        if (!handle.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"{parameterName} must use the `{prefix}` handle class.", parameterName);
        }
    }
}
