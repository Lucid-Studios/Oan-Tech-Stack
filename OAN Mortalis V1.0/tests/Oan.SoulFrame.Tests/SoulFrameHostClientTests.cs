using System.Net;
using System.Text;
using Oan.Common;
using Telemetry.GEL;
using SoulFrame.Host;

namespace Oan.SoulFrame.Tests;

public sealed class SoulFrameHostClientTests
{
    [Fact]
    public async Task RuntimeConnection_HealthEndpoint_ReturnsTrue()
    {
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/health")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var connected = await client.CheckConnectionAsync();

        Assert.True(connected);
    }

    [Fact]
    public async Task VmConnection_SpawnVm_ReturnsTrue()
    {
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/vm/spawn")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var started = await client.SpawnVmAsync();

        Assert.True(started);
    }

    [Fact]
    public async Task SemanticInference_Classify_ReturnsDeviceResponse()
    {
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath != "/classify")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var json = "{\"decision\":\"label:equation\",\"payload\":\"equation-structure\",\"confidence\":0.82}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        var response = await client.ClassifyAsync(new SoulFrameInferenceRequest
        {
            Task = "classify",
            Context = "3x + 7 = 25",
            OpalConstraints = new SoulFrameInferenceConstraints
            {
                Domain = "arithmetic",
                DriftLimit = 0.02,
                MaxTokens = 64
            },
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid()
        });

        Assert.True(response.Accepted);
        Assert.Equal("label:equation", response.Decision);
        Assert.Equal("equation-structure", response.Payload);
        Assert.Equal(0.82, response.Confidence, 3);
    }

    [Fact]
    public async Task ConstraintEnforcement_ExcessTokens_IsRefused()
    {
        var telemetry = new GelTelemetryAdapter();
        var adapter = new SoulFrameTelemetryAdapter(telemetry);
        var client = CreateClient((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)), adapter);

        var response = await client.InferAsync(new SoulFrameInferenceRequest
        {
            Task = "infer",
            Context = string.Join(' ', Enumerable.Repeat("token", 32)),
            OpalConstraints = new SoulFrameInferenceConstraints
            {
                Domain = "arithmetic",
                DriftLimit = 0.02,
                MaxTokens = 8
            },
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid()
        });

        Assert.False(response.Accepted);
        Assert.Contains(telemetry.Records, record => record.RuntimeState == "soulframe-host:constraintviolation");
    }

    [Fact]
    public async Task TelemetryEmission_InferenceLifecycle_IsEmitted()
    {
        var telemetry = new GelTelemetryAdapter();
        var adapter = new SoulFrameTelemetryAdapter(telemetry);
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/infer")
            {
                var json = "{\"decision\":\"infer-ok\",\"payload\":\"{}\",\"confidence\":0.7}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }, adapter);

        var request = new SoulFrameInferenceRequest
        {
            Task = "infer",
            Context = "context sample",
            OpalConstraints = new SoulFrameInferenceConstraints
            {
                Domain = "general",
                DriftLimit = 0.02,
                MaxTokens = 64
            },
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid()
        };

        var response = await client.InferAsync(request);

        Assert.True(response.Accepted);
        Assert.Contains(telemetry.Records, record => record.RuntimeState == "soulframe-host:inferencerequested");
        Assert.Contains(telemetry.Records, record => record.RuntimeState == "soulframe-host:inferencecompleted");
    }

    [Fact]
    public async Task MembraneProjection_ProjectMitigated_ReturnsBoundedWorkingState()
    {
        var client = CreateClient((_, _) => throw new InvalidOperationException("Membrane projection should not call transport."));
        var identityId = Guid.NewGuid();
        var request = new SoulFrameProjectionRequest(
            identityId,
            CMEId: "cme-alpha",
            SourceCustodyDomain: "cmos",
            RequestedTheater: "prime",
            PolicyHandle: "policy-17");

        var projection = await client.ProjectMitigatedAsync(request);

        Assert.Equal(identityId, projection.IdentityId);
        Assert.Equal("prime", projection.TargetTheater);
        Assert.True(projection.IsMitigated);
        Assert.StartsWith("soulframe://projection/", projection.ProjectionHandle, StringComparison.Ordinal);
        Assert.StartsWith("soulframe-session://cme-alpha/", projection.SessionHandle, StringComparison.Ordinal);
        Assert.StartsWith("soulframe-working://cme-alpha/", projection.WorkingStateHandle, StringComparison.Ordinal);
        Assert.StartsWith("membrane-derived:cme:cme-alpha|policy:policy-17", projection.ProvenanceMarker, StringComparison.Ordinal);
        Assert.StartsWith("soulframe-cselfgel://cme-alpha/", projection.MediatedSelfState.CSelfGelHandle, StringComparison.Ordinal);
        Assert.Equal("mediated-cselfgel-issue", projection.MediatedSelfState.Classification);
        Assert.Equal("policy-17", projection.MediatedSelfState.PolicyHandle);
        Assert.DoesNotContain("cmos", projection.SessionHandle, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cmos", projection.WorkingStateHandle, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cmos", projection.MediatedSelfState.CSelfGelHandle, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MembraneIntake_ReceiveReturnIntake_RecordsCandidateWithoutTransport()
    {
        var client = CreateClient((_, _) => throw new InvalidOperationException("Membrane intake should not call transport."));
        var identityId = Guid.NewGuid();
        var request = new SoulFrameReturnIntakeRequest(
            identityId,
            CMEId: "cme-alpha",
            SessionHandle: $"soulframe-session://cme-alpha/{identityId:D}",
            SourceTheater: "prime",
            ReturnCandidatePointer: "return-candidate://delta/42",
            ProvenanceMarker: "worker:agenticore/session:cme-alpha",
            IntakeIntent: "candidate-return-evaluation");

        var receipt = await client.ReceiveReturnIntakeAsync(request);

        Assert.Equal(identityId, receipt.IdentityId);
        Assert.True(receipt.Accepted);
        Assert.Equal("return-candidate-recorded", receipt.Disposition);
        Assert.Equal("candidate-collapse-evaluation", receipt.Evaluation.Classification);
        Assert.True(receipt.Evaluation.RequiresReview);
        Assert.False(receipt.Evaluation.CanRouteToCustody);
        Assert.False(receipt.Evaluation.CanPublishPrime);
        Assert.StartsWith("soulframe://return/", receipt.IntakeHandle, StringComparison.Ordinal);
    }

    private static SoulFrameHostClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder,
        SoulFrameTelemetryAdapter? telemetry = null)
    {
        var handler = new DelegatingHandlerStub(responder);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:8181")
        };
        return new SoulFrameHostClient(httpClient, telemetry, "http://127.0.0.1:8181");
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responder(request, cancellationToken);
        }
    }
}
