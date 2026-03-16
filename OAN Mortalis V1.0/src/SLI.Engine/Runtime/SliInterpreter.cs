using SLI.Engine.Models;
using SLI.Engine.Parser;

namespace SLI.Engine.Runtime;

public sealed class SliInterpreter
{
    private readonly SliSymbolTable _symbolTable;
    private readonly SliParser _parser = new();

    public SliInterpreter(SliSymbolTable symbolTable)
    {
        _symbolTable = symbolTable;
        CapabilityManifest = symbolTable.CreateCapabilityManifest();
    }

    public SliRuntimeCapabilityManifest CapabilityManifest { get; }

    public SliRuntimeCapabilityManifest CreateTargetCapabilityManifest(
        IEnumerable<string> supportedOpcodes,
        string runtimeId = "target-sli-runtime",
        SliRuntimeRealizationProfile? realizationProfile = null)
    {
        return _symbolTable.CreateTargetCapabilityManifest(supportedOpcodes, runtimeId, realizationProfile);
    }

    public async Task ExecuteProgramAsync(
        IReadOnlyList<SExpression> program,
        SliExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(context);

        string? previousNode = null;
        foreach (var expression in program)
        {
            var result = await ExecuteAsync(expression, context, cancellationToken).ConfigureAwait(false);
            var currentNode = expression.ToCanonicalString();
            context.ExecutionGraph.AddNode(currentNode);
            if (previousNode is not null)
            {
                context.ExecutionGraph.AddEdge(previousNode, currentNode);
            }

            previousNode = currentNode;
            context.ExecutionGraph.AddNode(result.ToCanonicalString());
        }
    }

    public async Task ExecuteProgramAsync(
        SliCoreProgram program,
        SliExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        await ExecuteProgramInternalAsync(program, context, targetManifest: null, cancellationToken).ConfigureAwait(false);
    }

    internal async Task ExecuteTargetProgramAsync(
        SliCoreProgram program,
        SliExecutionContext context,
        SliRuntimeCapabilityManifest targetManifest,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(targetManifest);

        await ExecuteProgramInternalAsync(program, context, targetManifest, cancellationToken).ConfigureAwait(false);
    }

    private async Task ExecuteProgramInternalAsync(
        SliCoreProgram program,
        SliExecutionContext context,
        SliRuntimeCapabilityManifest? targetManifest,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(context);

        string? previousNode = null;
        foreach (var instruction in program.Instructions)
        {
            if (targetManifest is not null)
            {
                EnsureTargetInstructionSupported(instruction, targetManifest);
            }

            var result = await ExecuteInstructionAsync(instruction, context, cancellationToken).ConfigureAwait(false);
            var currentNode = instruction.CanonicalForm;
            context.ExecutionGraph.AddNode(currentNode);
            if (previousNode is not null)
            {
                context.ExecutionGraph.AddEdge(previousNode, currentNode);
            }

            previousNode = currentNode;
            context.ExecutionGraph.AddNode(result.ToCanonicalString());
        }
    }

    private static void EnsureTargetInstructionSupported(
        SliCoreInstruction instruction,
        SliRuntimeCapabilityManifest targetManifest)
    {
        if (instruction.OperationClass != SliRuntimeOperationClass.TargetCandidate)
        {
            throw new InvalidOperationException(
                $"Target runtime '{targetManifest.RuntimeId}' may not execute non-target opcode '{instruction.Opcode}' classified as '{instruction.OperationClass}'.");
        }

        if (!targetManifest.TryGetCapability(instruction.Opcode, out var capability) ||
            capability.OperationClass != SliRuntimeOperationClass.TargetCandidate ||
            capability.Availability != SliRuntimeCapabilityAvailability.Available)
        {
            throw new InvalidOperationException(
                $"Target runtime '{targetManifest.RuntimeId}' does not advertise available target capability for opcode '{instruction.Opcode}'.");
        }
    }

    public async Task<SExpression> ExecuteAsync(
        SExpression expression,
        SliExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(context);

        if (expression.IsAtom)
        {
            return expression;
        }

        if (expression.Children.Count == 0)
        {
            return SExpression.AtomNode("()");
        }

        var op = expression.Children[0].Atom;
        if (string.IsNullOrWhiteSpace(op))
        {
            return SExpression.AtomNode("invalid-op");
        }

        if (_symbolTable.TryResolve(op, out var handler))
        {
            return await handler(expression, context, cancellationToken).ConfigureAwait(false);
        }

        context.AddTrace($"unknown-op({op})");
        return SExpression.AtomNode("unknown-op");
    }

    public async Task<SExpression> ExecuteInstructionAsync(
        SliCoreInstruction instruction,
        SliExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        ArgumentNullException.ThrowIfNull(context);

        var expression = _parser.ParseSingle(instruction.CanonicalForm);
        return await ExecuteAsync(expression, context, cancellationToken).ConfigureAwait(false);
    }
}
