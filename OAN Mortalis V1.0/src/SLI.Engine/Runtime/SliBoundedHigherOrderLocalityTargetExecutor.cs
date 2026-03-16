using System.Security.Cryptography;
using System.Text;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using SLI.Engine.Cognition;
using SLI.Engine.Models;
using SoulFrame.Host;

namespace SLI.Engine.Runtime;

internal sealed class SliBoundedHigherOrderLocalityTargetExecutor : ISliTargetHigherOrderLocalityExecutor
{
    private readonly SliInterpreter _interpreter = new(new SliSymbolTable());

    public SliBoundedHigherOrderLocalityTargetExecutor(SliRuntimeCapabilityManifest capabilityManifest)
    {
        ArgumentNullException.ThrowIfNull(capabilityManifest);
        CapabilityManifest = capabilityManifest;
    }

    public SliRuntimeCapabilityManifest CapabilityManifest { get; }

    public async Task<SliHigherOrderLocalityResult> ExecuteAsync(
        SliTargetHigherOrderLocalityExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Eligibility.EnsureEligible();

        var context = CreateTargetContext(request.Objective, request.Program.ProgramId);
        context.AddTrace($"target-runtime({CapabilityManifest.RuntimeId})");
        context.AddTrace($"target-profile({CapabilityManifest.RealizationProfile.ProfileId})");

        await _interpreter.ExecuteTargetProgramAsync(
                request.Program,
                context,
                CapabilityManifest,
                cancellationToken)
            .ConfigureAwait(false);

        var lineage = SliTargetExecutionContracts.CreateLineage(
            request.Admission,
            request.Objective,
            request.Program.ProgramId,
            emittedTraceCount: context.TraceLines.Count,
            emittedResidueCount: context.HigherOrderLocalityState.Residues.Count);

        return SliHigherOrderLocalityResultFactory.Create(context, lineage);
    }

    private SliExecutionContext CreateTargetContext(string objective, string programId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);
        ArgumentException.ThrowIfNullOrWhiteSpace(programId);

        return new SliExecutionContext(
            new ContextFrame
            {
                CMEId = CapabilityManifest.RuntimeId,
                SoulFrameId = Guid.Empty,
                ContextId = CreateDeterministicContextId(CapabilityManifest.RuntimeId, programId, objective),
                TaskObjective = objective,
                Engrams = []
            },
            NullEngramResolver.Instance,
            NullSoulFrameSemanticDevice.Instance);
    }

    private static Guid CreateDeterministicContextId(string runtimeId, string programId, string objective)
    {
        var seed = $"{runtimeId}|{programId}|{objective}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        var guidBytes = new byte[16];
        Buffer.BlockCopy(bytes, 0, guidBytes, 0, 16);
        return new Guid(guidBytes);
    }

    private sealed class NullEngramResolver : IEngramResolver
    {
        public static readonly NullEngramResolver Instance = new();

        public Task<EngramQueryResult> ResolveRelevantAsync(CradleTek.CognitionHost.Models.CognitionContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("relevant"));
        }

        public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("concept"));
        }

        public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Empty("cluster"));
        }

        private static EngramQueryResult Empty(string source)
        {
            return new EngramQueryResult
            {
                Source = source,
                Summaries = Array.Empty<EngramSummary>()
            };
        }
    }
}
