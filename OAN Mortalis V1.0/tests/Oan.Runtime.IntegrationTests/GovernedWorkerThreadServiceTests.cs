using AgentiCore.Services;
using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedWorkerThreadServiceTests
{
    [Fact]
    public void CreateIdentityThreadRoot_BoundedWorkersRemainLocallyRootedByIdentityInvariant()
    {
        var service = new GovernedWorkerThreadService();
        var firstWorker = CreateBoundedWorkerState(
            identityId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            sessionOrdinal: 1);
        var secondWorker = CreateBoundedWorkerState(
            identityId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            sessionOrdinal: 2);

        var firstThread = service.CreateIdentityThreadRoot(firstWorker, FixedTimestamp);
        var secondThread = service.CreateIdentityThreadRoot(secondWorker, FixedTimestamp);

        Assert.StartsWith("worker-thread-root://", firstThread.ThreadRootHandle, StringComparison.Ordinal);
        Assert.StartsWith("identity-invariant://", firstThread.IdentityInvariantHandle, StringComparison.Ordinal);
        Assert.True(firstThread.AmbientSharedIdentityDenied);
        Assert.True(firstThread.InterWorkerBraidRequired);
        Assert.Equal("identity-invariant-thread-root-bound", firstThread.ReasonCode);
        Assert.NotEqual(firstThread.ThreadRootHandle, secondThread.ThreadRootHandle);
        Assert.NotEqual(firstThread.IdentityInvariantHandle, secondThread.IdentityInvariantHandle);
    }

    [Fact]
    public void CreateGovernedThreadBirth_TriadicGovernanceReady_BindsBirthBeforeMovement()
    {
        var service = new GovernedWorkerThreadService();
        var worker = CreateBoundedWorkerState(
            identityId: Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            sessionOrdinal: 3);
        var governanceLayer = CreateGovernanceLayer(triadicReady: true);

        var birth = service.CreateGovernedThreadBirth(
            worker,
            governanceLayer,
            nexusBindingHandle: "nexus-binding://cme-threads/worker/3",
            nexusPortalHandle: "nexus-portal://cme-threads/primary",
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("governed-thread-birth://", birth.ThreadBirthHandle, StringComparison.Ordinal);
        Assert.Equal(
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            birth.WitnessedOffices);
        Assert.True(birth.TriadicWitnessBound);
        Assert.True(birth.MovementReleaseEligible);
        Assert.Equal("governed-thread-birth-triadic-bound", birth.ReasonCode);
        Assert.Equal("nexus-binding://cme-threads/worker/3", birth.NexusBindingHandle);
        Assert.Equal("nexus-portal://cme-threads/primary", birth.NexusPortalHandle);
    }

    [Fact]
    public void CreateGovernedThreadBirth_PreformalizedLayer_Throws()
    {
        var service = new GovernedWorkerThreadService();
        var worker = CreateBoundedWorkerState(
            identityId: Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            sessionOrdinal: 4);
        var preformalizedLayer = CreateGovernanceLayer(triadicReady: false);

        var exception = Assert.Throws<InvalidOperationException>(() => service.CreateGovernedThreadBirth(
            worker,
            preformalizedLayer,
            nexusBindingHandle: "nexus-binding://cme-threads/worker/4",
            nexusPortalHandle: "nexus-portal://cme-threads/primary",
            timestampUtc: FixedTimestamp));

        Assert.Contains("role-bound governance layer", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateInterWorkerBraidHandoff_RequiresDistinctThreadsAndWithholdsIdentityHandles()
    {
        var service = new GovernedWorkerThreadService();
        var governanceLayer = CreateGovernanceLayer(triadicReady: true);
        var sourceBirth = service.CreateGovernedThreadBirth(
            CreateBoundedWorkerState(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), 5),
            governanceLayer,
            nexusBindingHandle: "nexus-binding://cme-threads/worker/5",
            nexusPortalHandle: "nexus-portal://cme-threads/primary",
            timestampUtc: FixedTimestamp);
        var targetBirth = service.CreateGovernedThreadBirth(
            CreateBoundedWorkerState(Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), 6),
            governanceLayer,
            nexusBindingHandle: "nexus-binding://cme-threads/worker/6",
            nexusPortalHandle: "nexus-portal://cme-threads/primary",
            timestampUtc: FixedTimestamp);

        var packet = service.CreateInterWorkerBraidHandoff(
            sourceBirth,
            targetBirth,
            predicateContextHandle: "duplex-predicate://mission/packet-1",
            objective: "handoff-bounded-runtime-context",
            bridgedHandles:
            [
                "duplex-envelope://mission/1",
                "operator-session://bounded/1"
            ],
            timestampUtc: FixedTimestamp);

        Assert.StartsWith("worker-braid-handoff://", packet.BraidPacketId, StringComparison.Ordinal);
        Assert.Equal("inter-worker-braid-handoff-explicit", packet.ReasonCode);
        Assert.True(packet.AmbientSharedIdentityDenied);
        Assert.True(packet.IdentityInheritanceDenied);
        Assert.Contains(sourceBirth.IdentityInvariantHandle, packet.WithheldIdentityHandles);
        Assert.Contains(targetBirth.IdentityInvariantHandle, packet.WithheldIdentityHandles);
        Assert.DoesNotContain(sourceBirth.ThreadRootHandle, packet.BridgedHandles);
        Assert.DoesNotContain(targetBirth.ThreadRootHandle, packet.BridgedHandles);
    }

    [Fact]
    public void CreateInterWorkerBraidHandoff_SameThreadRoot_Throws()
    {
        var service = new GovernedWorkerThreadService();
        var governanceLayer = CreateGovernanceLayer(triadicReady: true);
        var birth = service.CreateGovernedThreadBirth(
            CreateBoundedWorkerState(Guid.Parse("11111111-1111-1111-1111-111111111111"), 7),
            governanceLayer,
            nexusBindingHandle: "nexus-binding://cme-threads/worker/7",
            nexusPortalHandle: "nexus-portal://cme-threads/primary",
            timestampUtc: FixedTimestamp);

        var exception = Assert.Throws<InvalidOperationException>(() => service.CreateInterWorkerBraidHandoff(
            birth,
            birth,
            predicateContextHandle: "duplex-predicate://mission/packet-2",
            objective: "handoff-bounded-runtime-context",
            bridgedHandles: ["duplex-envelope://mission/2"],
            timestampUtc: FixedTimestamp));

        Assert.Contains("distinct local thread roots", exception.Message, StringComparison.Ordinal);
    }

    private static readonly DateTimeOffset FixedTimestamp = new(2026, 3, 22, 6, 0, 0, TimeSpan.Zero);

    private static BoundedWorkerState CreateBoundedWorkerState(Guid identityId, int sessionOrdinal)
    {
        return new BoundedWorkerState(
            IdentityId: identityId,
            CMEId: "cme-threads",
            SessionHandle: $"soulframe-session://cme-threads/{sessionOrdinal}",
            WorkingStateHandle: $"soulframe-working://cme-threads/{sessionOrdinal}",
            ProvenanceMarker: $"membrane-derived:cme:cme-threads|policy:agenticore.cognition.cycle|loop:{sessionOrdinal}",
            TargetTheater: "sanctuary.actual",
            MediatedSelfState: new MediatedSelfStateContour(
                CSelfGelHandle: $"soulframe-cselfgel://cme-threads/{sessionOrdinal}",
                Classification: "bounded-worker",
                PolicyHandle: "policy://bounded-worker"));
    }

    private static FirstBootGovernanceLayerReceipt CreateGovernanceLayer(bool triadicReady)
    {
        var policy = new DefaultFirstBootGovernancePolicy();
        return policy.ProjectGovernanceLayer(
            bootClass: BootClass.CorporateGoverned,
            activationState: BootActivationState.TriadicActive,
            requestedExpansionCount: 1,
            formedOffices:
            [
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            ],
            triadicCrossWitnessComplete: triadicReady,
            bondedConfirmationComplete: triadicReady);
    }
}
