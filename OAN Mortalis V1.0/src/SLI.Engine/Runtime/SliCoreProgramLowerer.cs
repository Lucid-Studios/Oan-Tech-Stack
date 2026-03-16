using System.Security.Cryptography;
using System.Text;
using SLI.Engine.Models;

namespace SLI.Engine.Runtime;

public sealed class SliCoreProgramLowerer
{
    public SliCoreProgram LowerProgram(
        IReadOnlyList<SExpression> program,
        SliRuntimeCapabilityManifest capabilityManifest)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(capabilityManifest);

        var instructions = new List<SliCoreInstruction>(program.Count);
        foreach (var expression in program)
        {
            if (expression.IsAtom || expression.Children.Count == 0)
            {
                throw new InvalidOperationException("SLI core lowering requires list-form instructions.");
            }

            var opcode = expression.Children[0].Atom;
            if (string.IsNullOrWhiteSpace(opcode))
            {
                throw new InvalidOperationException("SLI core lowering requires an atom opcode.");
            }

            var capability = capabilityManifest.TryGetCapability(opcode, out var resolvedCapability)
                ? resolvedCapability
                : new SliRuntimeOperationCapability(
                    Opcode: opcode,
                    MeaningAuthority: "unresolved",
                    Availability: SliRuntimeCapabilityAvailability.Unavailable,
                    OperationClass: SliRuntimeOperationClass.HostOnly);

            var operands = expression.Children
                .Skip(1)
                .Select(LowerOperand)
                .ToArray();
            instructions.Add(new SliCoreInstruction(
                Opcode: opcode,
                Operands: operands,
                CanonicalForm: expression.ToCanonicalString(),
                MeaningAuthority: capability.MeaningAuthority,
                Availability: capability.Availability,
                OperationClass: capability.OperationClass));
        }

        var canonicalProgram = string.Join("\n", instructions.Select(instruction => instruction.CanonicalForm));
        var programId = $"sli-core://{HashHex(canonicalProgram)}";
        return new SliCoreProgram(
            ProgramId: programId,
            MeaningAuthority: capabilityManifest.MeaningAuthority,
            Instructions: instructions);
    }

    private static SliCoreOperand LowerOperand(SExpression expression)
    {
        if (!expression.IsAtom)
        {
            return new SliCoreOperand(SliCoreOperandKind.ListExpression, expression.ToCanonicalString());
        }

        var atom = expression.Atom!;
        var kind = atom.Length >= 2 && atom[0] == '"' && atom[^1] == '"'
            ? SliCoreOperandKind.StringLiteral
            : SliCoreOperandKind.Symbol;
        return new SliCoreOperand(kind, atom);
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
