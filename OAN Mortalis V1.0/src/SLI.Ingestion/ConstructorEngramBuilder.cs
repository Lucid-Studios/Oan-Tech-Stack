using System.Text.RegularExpressions;
using GEL.Graphs;
using GEL.Models;
using Oan.Spinal;

namespace SLI.Ingestion;

public sealed partial class ConstructorEngramBuilder
{
    public IReadOnlyList<ConstructorEngramRecord> Build(
        CleavedOntology cleavedOntology,
        EngramMatchResult matchResult)
    {
        ArgumentNullException.ThrowIfNull(cleavedOntology);
        ArgumentNullException.ThrowIfNull(matchResult);

        var records = new List<ConstructorEngramRecord>();

        foreach (var expression in cleavedOntology.Expressions)
        {
            if (!TryParseLinearEquation(expression, out var equation) || equation is null)
            {
                continue;
            }

            var structure = equation.Operator switch
            {
                '+' => $"(≡ (= (+ (⊗ {equation.Coefficient} {equation.Variable}) {equation.Constant}) {equation.RightHandSide}))",
                '-' => $"(≡ (= (⊖ (⊗ {equation.Coefficient} {equation.Variable}) {equation.Constant}) {equation.RightHandSide}))",
                _ => $"(≡ (= (⊗ {equation.Coefficient} {equation.Variable}) {equation.RightHandSide}))"
            };

            records.Add(new ConstructorEngramRecord
            {
                Domain = "mathematics",
                SymbolicStructure = structure,
                RootReferences = ["equation", "variable", "multiplication", equation.Operator == '-' ? "subtraction" : "addition"],
                Level = ConstructorEngramLevel.Intermediate,
                EquationDescriptor = equation
            });
        }

        if (records.Count == 0)
        {
            var roots = matchResult.KnownEngrams
                .Select(entry => entry.RootTerm)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .ToList();

            records.Add(new ConstructorEngramRecord
            {
                Domain = roots.Contains("equation", StringComparer.OrdinalIgnoreCase) ? "mathematics" : "general",
                SymbolicStructure = "(≡ (context-map roots))",
                RootReferences = roots,
                Level = ConstructorEngramLevel.Basic
            });
        }

        return records;
    }

    public ConstructorGraph BuildGraph(IReadOnlyList<ConstructorEngramRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var edges = new List<ConstructorEdge>();
        foreach (var record in records)
        {
            if (record.RootReferences.Count < 2)
            {
                continue;
            }

            var source = record.RootReferences[0];
            foreach (var target in record.RootReferences.Skip(1))
            {
                edges.Add(new ConstructorEdge
                {
                    Source = source,
                    Target = target,
                    Relation = "ingestion-derived"
                });
            }
        }

        return new ConstructorGraph
        {
            Edges = edges
        };
    }

    public IReadOnlyList<EngramDraft> BuildCanonicalDrafts(IReadOnlyList<ConstructorEngramRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        return records.Select(record =>
        {
            var rootKey = record.RootReferences.FirstOrDefault() ?? record.Domain;
            var requestedGrade = record.RootReferences.Count > 1
                ? EngramClosureGrade.Closed
                : EngramClosureGrade.BootstrapClosed;

            return new EngramDraft
            {
                ProposedId = new EngramId(BuildDraftId(record)),
                RootKey = rootKey,
                EpistemicClass = MapLevel(record.Level),
                Trunk = new EngramTrunk
                {
                    Segments = [record.SymbolicStructure],
                    Summary = $"{record.Domain}:{record.Level}"
                },
                Branches = record.RootReferences
                    .Skip(1)
                    .Select((reference, index) => new EngramBranch
                    {
                        Name = $"branch-{index + 1}",
                        RootKey = reference,
                        ReferencedEngramId = null,
                        SymbolicHandle = reference
                    })
                    .ToArray(),
                Invariants =
                [
                    new EngramInvariant
                    {
                        Key = "constructor.symbolic_structure",
                        Statement = $"Constructor structure retained for {record.Domain}."
                    }
                ],
                RequestedClosureGrade = requestedGrade
            };
        }).ToArray();
    }

    private static bool TryParseLinearEquation(string expression, out LinearEquationDescriptor? equation)
    {
        var match = LinearEquationRegex().Match(expression);
        if (!match.Success)
        {
            equation = null;
            return false;
        }

        equation = new LinearEquationDescriptor(
            int.Parse(match.Groups["coef"].Value),
            match.Groups["var"].Value.ToLowerInvariant(),
            match.Groups["op"].Value[0],
            int.Parse(match.Groups["constant"].Value),
            int.Parse(match.Groups["rhs"].Value));
        return true;
    }

    [GeneratedRegex(@"^\s*(?<coef>[-+]?\d+)\s*(?<var>[a-zA-Z])\s*(?<op>[+\-])\s*(?<constant>[-+]?\d+)\s*=\s*(?<rhs>[-+]?\d+)\s*$", RegexOptions.Compiled)]
    private static partial Regex LinearEquationRegex();

    private static string BuildDraftId(ConstructorEngramRecord record)
    {
        var projection = $"{record.Domain}|{record.Level}|{record.SymbolicStructure}|{string.Join("|", record.RootReferences)}";
        return Primitives.ComputeHash(projection);
    }

    private static EngramEpistemicClass MapLevel(ConstructorEngramLevel level) => level switch
    {
        ConstructorEngramLevel.Basic => EngramEpistemicClass.Propositional,
        ConstructorEngramLevel.Intermediate => EngramEpistemicClass.Procedural,
        ConstructorEngramLevel.Advanced => EngramEpistemicClass.Perspectival,
        _ => EngramEpistemicClass.Participatory
    };
}

public enum ConstructorEngramLevel
{
    Basic,
    Intermediate,
    Advanced
}

public sealed class ConstructorEngramRecord
{
    public required string Domain { get; init; }
    public required string SymbolicStructure { get; init; }
    public required IReadOnlyList<string> RootReferences { get; init; }
    public required ConstructorEngramLevel Level { get; init; }
    public LinearEquationDescriptor? EquationDescriptor { get; init; }
}

public sealed record LinearEquationDescriptor(
    int Coefficient,
    string Variable,
    char Operator,
    int Constant,
    int RightHandSide);
