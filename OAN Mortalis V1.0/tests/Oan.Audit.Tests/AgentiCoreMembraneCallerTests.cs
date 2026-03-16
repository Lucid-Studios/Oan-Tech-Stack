using AgentiCore.Services;
using Oan.Common;

namespace Oan.Audit.Tests;

public sealed class AgentiCoreMembraneCallerTests
{
    [Fact]
    public async Task FirstMembraneCaller_UsesBoundedHandlesAndSubmitsCandidateReturn()
    {
        var membrane = new RecordingMembrane();
        var worker = new BoundedMembraneWorkerService(membrane);
        var identityId = Guid.NewGuid();

        var state = await worker.BeginBoundedWorkAsync(
            new BoundedWorkerProjectionRequest(
                identityId,
                CMEId: "cme-alpha",
                SourceCustodyDomain: "cmos",
                RequestedTheater: "prime",
                PolicyHandle: "policy-17"));

        var receipt = await worker.SubmitReturnCandidateAsync(
            state,
            sourceTheater: "prime",
            returnCandidatePointer: "agenticore-return://delta/42",
            collapseClassification: CreateCollapseClassification());

        Assert.Equal(identityId, state.IdentityId);
        Assert.Equal("cme-alpha", state.CMEId);
        Assert.StartsWith("soulframe-session://", state.SessionHandle, StringComparison.Ordinal);
        Assert.StartsWith("soulframe-working://", state.WorkingStateHandle, StringComparison.Ordinal);
        Assert.StartsWith("membrane-derived:", state.ProvenanceMarker, StringComparison.Ordinal);

        Assert.NotNull(membrane.LastProjectionRequest);
        Assert.NotNull(membrane.LastReturnRequest);
        Assert.Equal("candidate-return-evaluation", membrane.LastReturnRequest!.IntakeIntent);
        Assert.Equal(state.SessionHandle, membrane.LastReturnRequest.SessionHandle);
        Assert.Equal(state.ProvenanceMarker, membrane.LastReturnRequest.ProvenanceMarker);
        Assert.Equal("agenticore-return://delta/42", membrane.LastReturnRequest.ReturnCandidatePointer);
        Assert.Equal(ControlSurfaceKind.SoulFrameReturnIntake, membrane.LastReturnRequest.RequestEnvelope.TargetSurface);
        Assert.Equal("agenticore-return://delta/42", membrane.LastReturnRequest.RequestEnvelope.ActionableContent.ContentHandle);
        Assert.Equal("prime", membrane.LastReturnRequest.RequestEnvelope.ActionableContent.OriginSurface);
        Assert.Equal(state.ProvenanceMarker, membrane.LastReturnRequest.RequestEnvelope.ActionableContent.ProvenanceMarker);
        Assert.True(receipt.Accepted);
        Assert.Equal(membrane.LastReturnRequest.RequestEnvelope.EnvelopeId, receipt.RequestEnvelopeId);
        Assert.Equal(membrane.LastReturnRequest.RequestEnvelope.ActionableContent.ContentHandle, receipt.ActionableContentHandle);
    }

    [Fact]
    public async Task FirstMembraneCaller_ForgedCustodyHandle_IsRejected()
    {
        var membrane = new RecordingMembrane
        {
            Projection = new SelfStateProjection(
                Guid.NewGuid(),
                ProjectionHandle: "soulframe://projection/forged",
                SessionHandle: "soulframe-session://cme-alpha/forged",
                TargetTheater: "prime",
                IsMitigated: true,
                WorkingStateHandle: "cmos://raw-state/forged",
                ProvenanceMarker: "membrane-derived:cme:cme-alpha|policy:policy-17",
                MediatedSelfState: CreateMediatedSelfState("cme-alpha", "policy-17"))
        };
        var worker = new BoundedMembraneWorkerService(membrane);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            worker.BeginBoundedWorkAsync(
                new BoundedWorkerProjectionRequest(
                    Guid.NewGuid(),
                    CMEId: "cme-alpha",
                    SourceCustodyDomain: "cmos",
                    RequestedTheater: "prime",
                    PolicyHandle: "policy-17")));
    }

    private static MediatedSelfStateContour CreateMediatedSelfState(string cmeId, string policyHandle) =>
        new(
            CSelfGelHandle: $"soulframe-cselfgel://{cmeId}/{Guid.NewGuid():D}",
            Classification: "mediated-cselfgel-issue",
            PolicyHandle: policyHandle);

    private sealed class RecordingMembrane : ISoulFrameMembrane
    {
        public SoulFrameProjectionRequest? LastProjectionRequest { get; private set; }
        public SoulFrameReturnIntakeRequest? LastReturnRequest { get; private set; }

        public SelfStateProjection Projection { get; set; } = new SelfStateProjection(
            Guid.NewGuid(),
            ProjectionHandle: "soulframe://projection/default",
            SessionHandle: "soulframe-session://cme-alpha/default",
            TargetTheater: "prime",
            IsMitigated: true,
            WorkingStateHandle: "soulframe-working://cme-alpha/default",
            ProvenanceMarker: "membrane-derived:cme:cme-alpha|policy:policy-17",
            MediatedSelfState: CreateMediatedSelfState("cme-alpha", "policy-17"));

        public Task<ISelfStateProjection> ProjectMitigatedAsync(
            SoulFrameProjectionRequest request,
            CancellationToken cancellationToken = default)
        {
            LastProjectionRequest = request;
            return Task.FromResult<ISelfStateProjection>(Projection with { IdentityId = request.IdentityId });
        }

        public Task<SoulFrameReturnIntakeReceipt> ReceiveReturnIntakeAsync(
            SoulFrameReturnIntakeRequest request,
            CancellationToken cancellationToken = default)
        {
            LastReturnRequest = request;
            ControlSurfaceContractGuards.ValidateSoulFrameReturnIntakeRequest(request);
            return Task.FromResult(new SoulFrameReturnIntakeReceipt(
                request.IdentityId,
                IntakeHandle: "soulframe://return/test",
                Accepted: true,
                Disposition: "return-candidate-recorded",
                Evaluation: new SoulFrameCollapseEvaluation(
                    Classification: "candidate-collapse-evaluation",
                    CollapseClassification: CreateCollapseClassification(),
                    ResidueClass: CmeCollapseResidueClass.AutobiographicalProtected,
                    ReviewState: CmeCollapseReviewState.DeferredReview,
                    RequiresReview: true,
                    CanRouteToCustody: false,
                    CanPublishPrime: false),
                RequestEnvelopeId: request.RequestEnvelope.EnvelopeId,
                ActionableContentHandle: request.RequestEnvelope.ActionableContent.ContentHandle));
        }

    }

    private static CmeCollapseClassification CreateCollapseClassification(
        double confidence = 0.92,
        bool selfGelIdentified = true,
        bool autobiographicalRelevant = true) =>
        new(
            confidence,
            selfGelIdentified,
            autobiographicalRelevant,
            autobiographicalRelevant || selfGelIdentified
                ? CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal
                : CmeCollapseEvidenceFlag.ContextualSignal | CmeCollapseEvidenceFlag.ProceduralSignal | CmeCollapseEvidenceFlag.SkillMethodSignal,
            CmeCollapseReviewTrigger.None,
            "AgentiCore");
}
