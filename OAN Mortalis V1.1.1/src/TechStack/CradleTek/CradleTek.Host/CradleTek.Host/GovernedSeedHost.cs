using CradleTek.Runtime;
using Oan.Common;

namespace CradleTek.Host;

public interface IGovernedSeedHost
{
    Task<EvaluateEnvelope> EvaluateAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default);

    Task<EvaluateEnvelope> EvaluateToolAccessAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default);

    Task<EvaluateEnvelope> EvaluateDataAccessAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default);

    Task<GovernedSeedReturnSurfaceContext> EvaluateReturnSurfaceAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default);

    Task<GovernedSeedOutboundObjectContext> EvaluateOutboundObjectAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default);

    Task<GovernedSeedOutboundLaneContext> EvaluateOutboundLaneAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default);
}

public sealed class GovernedSeedHost : IGovernedSeedHost
{
    private readonly GovernedSeedRuntimeService _runtimeService;

    public GovernedSeedHost(GovernedSeedRuntimeService runtimeService)
    {
        _runtimeService = runtimeService ?? throw new ArgumentNullException(nameof(runtimeService));
    }

    public Task<EvaluateEnvelope> EvaluateAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default) =>
        _runtimeService.EvaluateAsync(agentId, theaterId, input, cancellationToken);

    public Task<EvaluateEnvelope> EvaluateToolAccessAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default) =>
        _runtimeService.EvaluateToolAccessAsync(agentId, theaterId, input, cancellationToken);

    public Task<EvaluateEnvelope> EvaluateDataAccessAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default) =>
        _runtimeService.EvaluateDataAccessAsync(agentId, theaterId, input, cancellationToken);

    public async Task<GovernedSeedReturnSurfaceContext> EvaluateReturnSurfaceAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var envelope = await _runtimeService
            .EvaluateAsync(agentId, theaterId, input, cancellationToken)
            .ConfigureAwait(false);

        return envelope.ReturnSurfaceContext
            ?? throw new InvalidOperationException("Return surface context was not materialized for the evaluation envelope.");
    }

    public async Task<GovernedSeedOutboundObjectContext> EvaluateOutboundObjectAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var envelope = await _runtimeService
            .EvaluateAsync(agentId, theaterId, input, cancellationToken)
            .ConfigureAwait(false);

        return envelope.OutboundObjectContext
            ?? throw new InvalidOperationException("Outbound object context was not materialized for the evaluation envelope.");
    }

    public async Task<GovernedSeedOutboundLaneContext> EvaluateOutboundLaneAsync(
        string agentId,
        string theaterId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var envelope = await _runtimeService
            .EvaluateAsync(agentId, theaterId, input, cancellationToken)
            .ConfigureAwait(false);

        return envelope.OutboundLaneContext
            ?? throw new InvalidOperationException("Outbound lane context was not materialized for the evaluation envelope.");
    }
}
