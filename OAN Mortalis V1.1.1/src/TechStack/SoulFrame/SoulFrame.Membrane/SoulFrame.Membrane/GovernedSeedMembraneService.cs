using AgentiCore;
using San.Common;

namespace SoulFrame.Membrane;

public interface IGovernedSeedMembraneService
{
    Task<GovernedSeedEvaluationResult> EvaluateAsync(
        GovernedSeedEvaluationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class GovernedSeedMembraneService : IGovernedSeedMembraneService
{
    private readonly IGovernedSeedCognitionService _cognitionService;
    private readonly IGovernedSeedProjectionService _projectionService;
    private readonly IGovernedSeedReturnIntakeService _returnIntakeService;
    private readonly IGovernedSeedProtectedHoldRoutingService _holdRoutingService;
    private readonly IGovernedSeedStewardshipService _stewardshipService;
    private readonly IGovernedSeedMemoryContextService _memoryContextService;
    private readonly IGovernedSeedLowMindSfRoutingService _lowMindSfRoutingService;
    private readonly IGovernedSeedSituationalContextService _situationalContextService;

    public GovernedSeedMembraneService(
        IGovernedSeedCognitionService cognitionService,
        IGovernedSeedProjectionService projectionService,
        IGovernedSeedReturnIntakeService returnIntakeService,
        IGovernedSeedProtectedHoldRoutingService holdRoutingService,
        IGovernedSeedStewardshipService stewardshipService,
        IGovernedSeedMemoryContextService memoryContextService,
        IGovernedSeedLowMindSfRoutingService lowMindSfRoutingService,
        IGovernedSeedSituationalContextService situationalContextService)
    {
        _cognitionService = cognitionService ?? throw new ArgumentNullException(nameof(cognitionService));
        _projectionService = projectionService ?? throw new ArgumentNullException(nameof(projectionService));
        _returnIntakeService = returnIntakeService ?? throw new ArgumentNullException(nameof(returnIntakeService));
        _holdRoutingService = holdRoutingService ?? throw new ArgumentNullException(nameof(holdRoutingService));
        _stewardshipService = stewardshipService ?? throw new ArgumentNullException(nameof(stewardshipService));
        _memoryContextService = memoryContextService ?? throw new ArgumentNullException(nameof(memoryContextService));
        _lowMindSfRoutingService = lowMindSfRoutingService ?? throw new ArgumentNullException(nameof(lowMindSfRoutingService));
        _situationalContextService = situationalContextService ?? throw new ArgumentNullException(nameof(situationalContextService));
    }

    public async Task<GovernedSeedEvaluationResult> EvaluateAsync(
        GovernedSeedEvaluationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.BootstrapReceipt);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRequest = request with
        {
            AgentId = request.AgentId.Trim(),
            TheaterId = request.TheaterId.Trim(),
            Input = request.Input.Trim()
        };

        var projectionReceipt = _projectionService.CreateProjection(normalizedRequest, normalizedRequest.BootstrapReceipt);
        var memoryContext = await _memoryContextService
            .CreateContextAsync(normalizedRequest, normalizedRequest.BootstrapReceipt, cancellationToken)
            .ConfigureAwait(false);
        var lowMindSfRoute = _lowMindSfRoutingService.CreateRoute(
            normalizedRequest,
            normalizedRequest.BootstrapReceipt,
            memoryContext);
        var result = _cognitionService.Evaluate(normalizedRequest, memoryContext, lowMindSfRoute);
        var returnIntakeReceipt = _returnIntakeService.CreateReturnIntake(
            normalizedRequest.BootstrapReceipt,
            projectionReceipt,
            result);
        var holdRoutingReceipt = _holdRoutingService.CreateRouting(
            normalizedRequest.BootstrapReceipt,
            returnIntakeReceipt,
            result);
        var stewardshipReceipt = _stewardshipService.CreateStewardship(
            normalizedRequest.BootstrapReceipt,
            projectionReceipt,
            returnIntakeReceipt,
            holdRoutingReceipt,
            result);
        var situationalContext = _situationalContextService.CreateContext(
            normalizedRequest.BootstrapReceipt,
            projectionReceipt,
            returnIntakeReceipt,
            holdRoutingReceipt,
            stewardshipReceipt,
            lowMindSfRoute,
            memoryContext,
            result);

        return result with
        {
            VerticalSlice = result.VerticalSlice with
            {
                BootstrapReceipt = normalizedRequest.BootstrapReceipt,
                ProjectionReceipt = projectionReceipt,
                ReturnIntakeReceipt = returnIntakeReceipt,
                StewardshipReceipt = stewardshipReceipt,
                HoldRoutingReceipt = holdRoutingReceipt,
                SanctuaryIngressReceipt = normalizedRequest.SanctuaryIngressReceipt,
                SituationalContext = situationalContext
            }
        };
    }
}
