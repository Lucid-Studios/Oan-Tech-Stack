using System.Text.Json;
using System.Text.Json.Serialization;
using AgentiCore.Observation;
using CradleTek.Cryptic;
using CradleTek.Mantle;
using CradleTek.Public;
using EngramGovernance.Models;
using EngramGovernance.Services;
using OAN.Core.Telemetry;
using Oan.Common;
using Oan.Cradle;
using Oan.Storage;
using Telemetry.GEL;

namespace Oan.Runtime.Headless;

public sealed record RuntimeOperatorContext(
    IGovernanceLoopStatusReader StatusReader,
    IDeferredReviewQueue DeferredQueue,
    IPendingRecoveryCoordinator RecoveryCoordinator);

public enum RuntimeOperatorExitCode
{
    Success = 0,
    InvalidArguments = 2,
    NotFound = 3,
    InvalidState = 4,
    RuntimeFailure = 5,
    FailedSafeEvidence = 6
}

public static class RuntimeOperatorCli
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        RuntimeOperatorContext? context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(stdout);
        ArgumentNullException.ThrowIfNull(stderr);

        if (args.Length == 0 || IsHelpCommand(args))
        {
            await stdout.WriteLineAsync(BuildHelpText()).ConfigureAwait(false);
            return (int)RuntimeOperatorExitCode.Success;
        }

        try
        {
            return await DispatchAsync(args, stdout, stderr, context, cancellationToken).ConfigureAwait(false);
        }
        catch (RuntimeOperatorException ex)
        {
            await WriteFailureAsync(stderr, ex.Command, ex.ExitCode, ex.ErrorCode, ex.Message, ex.LoopKey, cancellationToken)
                .ConfigureAwait(false);
            return (int)ex.ExitCode;
        }
        catch (Exception ex)
        {
            await WriteFailureAsync(
                    stderr,
                    command: string.Join(' ', args),
                    RuntimeOperatorExitCode.RuntimeFailure,
                    "runtime_failure",
                    ex.Message,
                    loopKey: null,
                    cancellationToken)
                .ConfigureAwait(false);
            return (int)RuntimeOperatorExitCode.RuntimeFailure;
        }
    }

    private static async Task<int> DispatchAsync(
        string[] args,
        TextWriter stdout,
        TextWriter stderr,
        RuntimeOperatorContext? context,
        CancellationToken cancellationToken)
    {
        var root = args[0].Trim().ToLowerInvariant();
        switch (root)
        {
            case "status":
                return await HandleStatusAsync(args, stdout, RequireContext(context), cancellationToken).ConfigureAwait(false);
            case "deferred":
                return await HandleDeferredAsync(args, stdout, RequireContext(context), cancellationToken).ConfigureAwait(false);
            case "recovery":
                return await HandleRecoveryAsync(args, stdout, RequireContext(context), cancellationToken).ConfigureAwait(false);
            default:
                throw new RuntimeOperatorException(
                    RuntimeOperatorExitCode.InvalidArguments,
                    string.Join(' ', args),
                    "invalid_arguments",
                    $"Unknown command '{args[0]}'.");
        }
    }

    private static RuntimeOperatorContext RequireContext(RuntimeOperatorContext? context) =>
        context ?? throw new InvalidOperationException("Operator context is required for this command.");

    private static async Task<int> HandleStatusAsync(
        string[] args,
        TextWriter stdout,
        RuntimeOperatorContext context,
        CancellationToken cancellationToken)
    {
        var options = ParseOptions(args, startIndex: 1);
        GovernanceLoopStatusView status;
        string? loopKey = null;

        if (options.TryGetValue("loop-key", out var explicitLoopKey))
        {
            loopKey = explicitLoopKey;
            status = await context.StatusReader.GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
        }
        else if (options.TryGetValue("candidate-id", out var candidateIdText) &&
                 options.TryGetValue("provenance", out var provenance))
        {
            if (!Guid.TryParse(candidateIdText, out var candidateId))
            {
                throw new RuntimeOperatorException(
                    RuntimeOperatorExitCode.InvalidArguments,
                    "status",
                    "invalid_arguments",
                    "The value for --candidate-id must be a valid GUID.");
            }

            loopKey = GovernanceLoopKeys.Create(candidateId, provenance);
            status = await context.StatusReader.GetStatusByCandidateAsync(candidateId, provenance, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.InvalidArguments,
                "status",
                "invalid_arguments",
                "Use either --loop-key <key> or --candidate-id <guid> --provenance <value>.");
        }

        ThrowForStatusFailure(status, "status", loopKey);
        await WriteSuccessAsync(stdout, "status", loopKey, status, cancellationToken).ConfigureAwait(false);
        return (int)RuntimeOperatorExitCode.Success;
    }

    private static async Task<int> HandleDeferredAsync(
        string[] args,
        TextWriter stdout,
        RuntimeOperatorContext context,
        CancellationToken cancellationToken)
    {
        if (args.Length < 2)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.InvalidArguments,
                "deferred",
                "invalid_arguments",
                "Deferred commands require a subcommand.");
        }

        var subcommand = args[1].Trim().ToLowerInvariant();
        var command = $"deferred {subcommand}";
        var options = ParseOptions(args, startIndex: 2);

        switch (subcommand)
        {
            case "list":
            {
                var items = await context.DeferredQueue.ListDeferredAsync(cancellationToken).ConfigureAwait(false);
                await WriteSuccessAsync(stdout, command, loopKey: null, items, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            case "get":
            {
                var loopKey = RequireOption(options, "loop-key", command);
                var item = await context.DeferredQueue.GetDeferredAsync(loopKey, cancellationToken).ConfigureAwait(false);
                if (item is null)
                {
                    throw new RuntimeOperatorException(
                        RuntimeOperatorExitCode.NotFound,
                        command,
                        "not_found",
                        $"Deferred loop '{loopKey}' was not found.",
                        loopKey);
                }

                await WriteSuccessAsync(stdout, command, loopKey, item, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            case "annotate":
            {
                var loopKey = RequireOption(options, "loop-key", command);
                var deferred = await RequireDeferredItemAsync(context, command, loopKey, cancellationToken).ConfigureAwait(false);
                var request = new ReviewDeferredCandidateRequest(
                    loopKey,
                    deferred.CandidateId,
                    deferred.CandidateProvenance,
                    RequireOption(options, "reviewed-by", command),
                    "steward.annotation.note",
                    RequireOption(options, "annotation", command));

                await context.DeferredQueue.AnnotateDeferredAsync(request, cancellationToken).ConfigureAwait(false);
                var status = await context.StatusReader.GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
                ThrowForStatusFailure(status, command, loopKey);
                await WriteSuccessAsync(stdout, command, loopKey, status, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            case "approve":
            case "reject":
            {
                var loopKey = RequireOption(options, "loop-key", command);
                var deferred = await RequireDeferredItemAsync(context, command, loopKey, cancellationToken).ConfigureAwait(false);
                var request = new ReviewDeferredCandidateRequest(
                    loopKey,
                    deferred.CandidateId,
                    deferred.CandidateProvenance,
                    RequireOption(options, "reviewed-by", command),
                    RequireOption(options, "rationale", command),
                    options.GetValueOrDefault("annotation"));

                if (subcommand == "approve")
                {
                    await context.DeferredQueue.ApproveDeferredAsync(request, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await context.DeferredQueue.RejectDeferredAsync(request, cancellationToken).ConfigureAwait(false);
                }

                var status = await context.StatusReader.GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
                ThrowForStatusFailure(status, command, loopKey, allowFailedState: subcommand == "reject");
                await WriteSuccessAsync(stdout, command, loopKey, status, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            default:
                throw new RuntimeOperatorException(
                    RuntimeOperatorExitCode.InvalidArguments,
                    command,
                    "invalid_arguments",
                    $"Unknown deferred subcommand '{subcommand}'.");
        }
    }

    private static async Task<int> HandleRecoveryAsync(
        string[] args,
        TextWriter stdout,
        RuntimeOperatorContext context,
        CancellationToken cancellationToken)
    {
        if (args.Length < 2)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.InvalidArguments,
                "recovery",
                "invalid_arguments",
                "Recovery commands require a subcommand.");
        }

        var subcommand = args[1].Trim().ToLowerInvariant();
        var command = $"recovery {subcommand}";
        var options = ParseOptions(args, startIndex: 2);

        switch (subcommand)
        {
            case "list":
            {
                var items = await context.RecoveryCoordinator.ListPendingRecoveryAsync(cancellationToken).ConfigureAwait(false);
                await WriteSuccessAsync(stdout, command, loopKey: null, items, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            case "resume":
            {
                var loopKey = RequireOption(options, "loop-key", command);
                await EnsureActionableStatusAsync(context, command, loopKey, cancellationToken).ConfigureAwait(false);
                await context.RecoveryCoordinator.ResumeGovernanceLoopAsync(
                        new ResumeGovernanceLoopRequest(
                            loopKey,
                            RequireOption(options, "requested-by", command),
                            RequireOption(options, "reason", command)),
                        cancellationToken)
                    .ConfigureAwait(false);

                var status = await context.StatusReader.GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
                ThrowForStatusFailure(status, command, loopKey, allowFailedState: false);
                await WriteSuccessAsync(stdout, command, loopKey, status, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            case "retry-lane":
            {
                var loopKey = RequireOption(options, "loop-key", command);
                await EnsureActionableStatusAsync(context, command, loopKey, cancellationToken).ConfigureAwait(false);
                var laneText = RequireOption(options, "lane", command);
                var lane = laneText.ToLowerInvariant() switch
                {
                    "pointer" => GovernedPrimeDerivativeLane.Pointer,
                    "checked-view" => GovernedPrimeDerivativeLane.CheckedView,
                    _ => throw new RuntimeOperatorException(
                        RuntimeOperatorExitCode.InvalidArguments,
                        command,
                        "invalid_arguments",
                        "The value for --lane must be 'pointer' or 'checked-view'.",
                        loopKey)
                };

                await context.RecoveryCoordinator.RetryPublicationLaneAsync(
                        new ResumePublicationLaneRequest(
                            loopKey,
                            lane,
                            RequireOption(options, "requested-by", command),
                            RequireOption(options, "reason", command)),
                        cancellationToken)
                    .ConfigureAwait(false);

                var status = await context.StatusReader.GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
                ThrowForStatusFailure(status, command, loopKey, allowFailedState: false);
                await WriteSuccessAsync(stdout, command, loopKey, status, cancellationToken).ConfigureAwait(false);
                return (int)RuntimeOperatorExitCode.Success;
            }

            default:
                throw new RuntimeOperatorException(
                    RuntimeOperatorExitCode.InvalidArguments,
                    command,
                    "invalid_arguments",
                    $"Unknown recovery subcommand '{subcommand}'.");
        }
    }

    private static async Task<DeferredBacklogItemView> RequireDeferredItemAsync(
        RuntimeOperatorContext context,
        string command,
        string loopKey,
        CancellationToken cancellationToken)
    {
        var item = await context.DeferredQueue.GetDeferredAsync(loopKey, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.NotFound,
                command,
                "not_found",
                $"Deferred loop '{loopKey}' was not found.",
                loopKey);
        }

        return item;
    }

    private static async Task EnsureActionableStatusAsync(
        RuntimeOperatorContext context,
        string command,
        string loopKey,
        CancellationToken cancellationToken)
    {
        var status = await context.StatusReader.GetStatusByLoopKeyAsync(loopKey, cancellationToken).ConfigureAwait(false);
        if (status.HasJournalIntegrityErrors)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.FailedSafeEvidence,
                command,
                "failed_safe_evidence",
                $"Loop '{loopKey}' entered failed-safe state due to malformed or partial journal evidence.",
                loopKey);
        }

        if (status.ControlState == GovernanceLoopControlState.NotFound)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.NotFound,
                command,
                "not_found",
                $"Loop '{loopKey}' was not found.",
                loopKey);
        }
    }

    private static void ThrowForStatusFailure(
        GovernanceLoopStatusView status,
        string command,
        string? loopKey,
        bool allowFailedState = false)
    {
        if (status.HasJournalIntegrityErrors)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.FailedSafeEvidence,
                command,
                "failed_safe_evidence",
                $"Loop '{loopKey ?? status.LoopKey}' entered failed-safe state due to malformed or partial journal evidence.",
                loopKey ?? status.LoopKey);
        }

        if (status.ControlState == GovernanceLoopControlState.NotFound)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.NotFound,
                command,
                "not_found",
                $"Loop '{loopKey ?? status.LoopKey}' was not found.",
                loopKey ?? status.LoopKey);
        }

        if (!allowFailedState && status.ControlState == GovernanceLoopControlState.Failed)
        {
            throw new RuntimeOperatorException(
                RuntimeOperatorExitCode.InvalidState,
                command,
                "invalid_state",
                $"Loop '{loopKey ?? status.LoopKey}' is in failed state and cannot perform this action.",
                loopKey ?? status.LoopKey);
        }
    }

    private static Dictionary<string, string> ParseOptions(string[] args, int startIndex)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = startIndex; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                throw new RuntimeOperatorException(
                    RuntimeOperatorExitCode.InvalidArguments,
                    string.Join(' ', args.Take(startIndex)),
                    "invalid_arguments",
                    $"Unexpected token '{token}'. Options must use --name value syntax.");
            }

            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new RuntimeOperatorException(
                    RuntimeOperatorExitCode.InvalidArguments,
                    string.Join(' ', args.Take(startIndex)),
                    "invalid_arguments",
                    $"Missing value for option '{token}'.");
            }

            options[token[2..]] = args[i + 1];
            i++;
        }

        return options;
    }

    private static string RequireOption(
        IReadOnlyDictionary<string, string> options,
        string key,
        string command)
    {
        if (options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new RuntimeOperatorException(
            RuntimeOperatorExitCode.InvalidArguments,
            command,
            "invalid_arguments",
            $"Missing required option '--{key}'.");
    }

    private static bool IsHelpCommand(string[] args) =>
        args.Length == 1 &&
        (string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(args[0], "--help", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(args[0], "-h", StringComparison.OrdinalIgnoreCase));

    private static async Task WriteSuccessAsync<T>(
        TextWriter stdout,
        string command,
        string? loopKey,
        T result,
        CancellationToken cancellationToken)
    {
        var envelope = new OperatorSuccessEnvelope<T>(
            Ok: true,
            Command: command,
            Timestamp: DateTime.UtcNow,
            LoopKey: loopKey,
            Result: result);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        await stdout.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteFailureAsync(
        TextWriter stderr,
        string command,
        RuntimeOperatorExitCode exitCode,
        string errorCode,
        string message,
        string? loopKey,
        CancellationToken cancellationToken)
    {
        var envelope = new OperatorFailureEnvelope(
            Ok: false,
            Command: command,
            Timestamp: DateTime.UtcNow,
            LoopKey: loopKey,
            ErrorCode: errorCode,
            Message: message,
            ExitCode: (int)exitCode);

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        await stderr.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
    }

    private static string BuildHelpText() =>
        """
        OAN Runtime Operator CLI

        Read-only commands:
          status --loop-key <key>
          status --candidate-id <guid> --provenance <value>
          deferred list
          deferred get --loop-key <key>
          recovery list

        Actuation commands:
          deferred annotate --loop-key <key> --reviewed-by <id> --annotation <text>
          deferred approve --loop-key <key> --reviewed-by <id> --rationale <code> [--annotation <text>]
          deferred reject --loop-key <key> --reviewed-by <id> --rationale <code> [--annotation <text>]
          recovery resume --loop-key <key> --requested-by <id> --reason <text>
          recovery retry-lane --loop-key <key> --lane pointer|checked-view --requested-by <id> --reason <text>

        Compatibility:
          evaluate

        Notes:
          - This CLI is local/process-scoped only.
          - Success writes JSON to stdout.
          - Failures write JSON to stderr and return a non-zero exit code.
        """;

    private sealed record OperatorSuccessEnvelope<T>(
        bool Ok,
        string Command,
        DateTime Timestamp,
        string? LoopKey,
        T Result);

    private sealed record OperatorFailureEnvelope(
        bool Ok,
        string Command,
        DateTime Timestamp,
        string? LoopKey,
        string ErrorCode,
        string Message,
        int ExitCode);
}

public static class HeadlessRuntimeBootstrap
{
    public static async Task<CradleTekHost> CreateEvaluateHostAsync(
        string runtimeRoot,
        IManagedEgressRouter? egressRouter = null,
        CancellationToken cancellationToken = default)
    {
        var publicRoot = Path.Combine(runtimeRoot, "public_root");
        var crypticRoot = Path.Combine(runtimeRoot, "cryptic_root");
        
        var envelope = new ManagedEgressEnvelope(
            EffectKind: SliEgressEffectKind.StructuralCreation,
            RetentionPosture: SliEgressRetentionPosture.GovernanceArtifact,
            JurisdictionClass: SliEgressJurisdictionClass.Cradle,
            IdentityFormingAllowed: true,
            TargetSinkClass: SliEgressTargetSinkClass.FileSystemLocal,
            AuthorityReason: "Establishing local structural bounds for Evaluation runtime"
        );

        var router = egressRouter ?? NullEgressRouter.Instance;
        var authorized = await router.TryRouteEgressAsync(envelope, () =>
        {
            Directory.CreateDirectory(publicRoot);
            Directory.CreateDirectory(crypticRoot);
            return Task.CompletedTask;
        }, cancellationToken).ConfigureAwait(false);
        if (!authorized)
        {
            throw new InvalidOperationException("Managed egress router denied evaluation runtime structural creation.");
        }

        var govTelemetry = new GovernanceTelemetrySink(
            Path.Combine(runtimeRoot, "governance.ndjson"),
            ex => Console.WriteLine($"[FATAL] Governance Telemetry Failure: {ex.Message}"));
        var storageTelemetry = new StorageTelemetrySink(Path.Combine(runtimeRoot, "storage.ndjson"));
        var publicStore = new PublicPlaneStore(publicRoot, storageTelemetry, router);
        var primeDerivativePublisher = new PrimeDerivativePublisherAdapter(publicStore);
        var crypticStore = new CrypticPlaneStore(crypticRoot, storageTelemetry, router);
        var formationObserver = new InMemoryAgentiFormationObserver();
        var admissionMembrane = new CrypticAdmissionMembrane();
        var closureValidator = new EngramClosureValidator();
        var firstBootHarness = new FirstBootFormationObservationHarness(
            new DefaultFirstBootGovernancePolicy(),
            formationObserver);
        var stores = new StoreRegistry(
            govTelemetry,
            storageTelemetry,
            publicStore,
            primeDerivativePublisher,
            primeDerivativePublisher,
            true,
            crypticStore,
            true,
            hopngArtifactService: HopngArtifactServiceFactory.Create(Path.Combine(runtimeRoot, "telemetry", "hopng"), router),
            crypticAdmissionMembrane: admissionMembrane,
            formationObserver: formationObserver,
            firstBootFormationObservationHarness: firstBootHarness);

        var host = new CradleTekHost(stores);
        await host.InitializeAsync().ConfigureAwait(false);
        return host;
    }

    public static RuntimeOperatorContext CreateOperatorContext(
        string runtimeRoot, 
        IManagedEgressRouter? egressRouter = null)
    {
        var publicRoot = Path.Combine(runtimeRoot, "public_root");
        var crypticRoot = Path.Combine(runtimeRoot, "cryptic_root");

        var envelope = new ManagedEgressEnvelope(
            EffectKind: SliEgressEffectKind.StructuralCreation,
            RetentionPosture: SliEgressRetentionPosture.GovernanceArtifact,
            JurisdictionClass: SliEgressJurisdictionClass.Cradle,
            IdentityFormingAllowed: true,
            TargetSinkClass: SliEgressTargetSinkClass.FileSystemLocal,
            AuthorityReason: "Establishing local structural bounds for CLI Operations"
        );

        var router = egressRouter ?? NullEgressRouter.Instance;
        var authorized = router.TryRouteEgressAsync(envelope, () =>
        {
            Directory.CreateDirectory(publicRoot);
            Directory.CreateDirectory(crypticRoot);
            return Task.CompletedTask;
        }).GetAwaiter().GetResult();
        if (!authorized)
        {
            throw new InvalidOperationException("Managed egress router denied CLI operator structural creation.");
        }

        var govTelemetry = new GovernanceTelemetrySink(
            Path.Combine(runtimeRoot, "governance.ndjson"),
            _ => { });
        var storageTelemetry = new StorageTelemetrySink(Path.Combine(runtimeRoot, "storage.ndjson"));
        var publicPlane = new PublicPlaneStore(publicRoot, storageTelemetry, router);
        var primeDerivativePublisher = new PrimeDerivativePublisherAdapter(publicPlane);
        var crypticPlane = new CrypticPlaneStore(crypticRoot, storageTelemetry, router);

        var publicLayer = new PublicLayerService();
        var crypticLayer = new CrypticLayerService();
        var mantle = new MantleOfSovereigntyService();
        var telemetry = new GelTelemetryAdapter();
        var journal = new NdjsonGovernanceReceiptJournal(Path.Combine(runtimeRoot, "governance-control.ndjson"), router);
        var formationObserver = new InMemoryAgentiFormationObserver();
        var admissionMembrane = new CrypticAdmissionMembrane();
        var closureValidator = new EngramClosureValidator();
        var firstBootHarness = new FirstBootFormationObservationHarness(
            new DefaultFirstBootGovernancePolicy(),
            formationObserver);
        var steward = new StewardAgent(
            new OntologicalCleaver(),
            new EncryptionService(),
            new LedgerWriter(telemetry),
            engramBootstrap: null,
            constructorGuidance: null,
            publicLayer,
            crypticLayer,
            telemetry,
            governanceJournal: journal);
        var collapseQualifier = new CmeCollapseQualifier();

        var stores = new StoreRegistry(
            governanceTelemetry: govTelemetry,
            storageTelemetry: storageTelemetry,
            publicStores: publicPlane,
            primeDerivativePublisher: primeDerivativePublisher,
            primeDerivativeView: primeDerivativePublisher,
            publicAvailable: true,
            crypticStores: crypticPlane,
            crypticAvailable: true,
            soulFrameMembrane: null,
            governanceCognitionService: null,
            returnGovernanceAdjudicator: steward,
            crypticCustodyStore: mantle,
            crypticReengrammitizationGate: mantle,
            governedPrimePublicationSink: publicLayer,
            governanceReceiptJournal: journal,
            cmeCollapseQualifier: collapseQualifier,
            crypticAdmissionMembrane: admissionMembrane,
            formationObserver: formationObserver,
            firstBootFormationObservationHarness: firstBootHarness);

        var manager = new StackManager(stores);
        return new RuntimeOperatorContext(manager, steward, manager);
    }
}

public sealed class RuntimeOperatorException : Exception
{
    public RuntimeOperatorException(
        RuntimeOperatorExitCode exitCode,
        string command,
        string errorCode,
        string message,
        string? loopKey = null)
        : base(message)
    {
        ExitCode = exitCode;
        Command = command;
        ErrorCode = errorCode;
        LoopKey = loopKey;
    }

    public RuntimeOperatorExitCode ExitCode { get; }

    public string Command { get; }

    public string ErrorCode { get; }

    public string? LoopKey { get; }
}
