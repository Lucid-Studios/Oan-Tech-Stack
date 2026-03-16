using SLI.Engine.Cognition;

namespace SLI.Engine.Runtime;

internal sealed record SliTargetHigherOrderLocalityExecutionRequest(
    string Objective,
    SliCoreProgram Program,
    SliTargetLaneEligibility Eligibility);

internal interface ISliTargetHigherOrderLocalityExecutor
{
    SliRuntimeCapabilityManifest CapabilityManifest { get; }

    Task<SliHigherOrderLocalityResult> ExecuteAsync(
        SliTargetHigherOrderLocalityExecutionRequest request,
        CancellationToken cancellationToken = default);
}
