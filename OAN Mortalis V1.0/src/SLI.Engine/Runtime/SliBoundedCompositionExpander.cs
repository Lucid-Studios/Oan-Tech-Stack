using SLI.Engine.Models;
using SLI.Engine.Parser;

namespace SLI.Engine.Runtime;

internal sealed class SliBoundedCompositionExpander
{
    private static readonly HashSet<string> AllowedComposites = new(StringComparer.OrdinalIgnoreCase)
    {
        "locality-bootstrap",
        "perspective-bounded-observer",
        "participation-bounded-cme",
        "rehearsal-bounded-exploration"
    };

    private readonly Dictionary<string, CompositeTemplate> _templates;

    public SliBoundedCompositionExpander(IReadOnlyDictionary<string, string> loadedModules)
    {
        ArgumentNullException.ThrowIfNull(loadedModules);
        _templates = LoadTemplates(loadedModules);
    }

    public IReadOnlyList<SExpression> ExpandProgram(IReadOnlyList<SExpression> program)
    {
        ArgumentNullException.ThrowIfNull(program);

        var expanded = new List<SExpression>();
        foreach (var expression in program)
        {
            ExpandExpression(expression, expanded);
        }

        return expanded;
    }

    private void ExpandExpression(SExpression expression, List<SExpression> expanded)
    {
        if (TryGetCompositeName(expression, out var compositeName))
        {
            var template = _templates[compositeName];
            var args = expression.Children.Skip(1).ToArray();
            if (args.Length != template.Arity)
            {
                throw new InvalidOperationException(
                    $"Composite '{compositeName}' expects {template.Arity} arguments but received {args.Length}.");
            }

            foreach (var step in template.Steps)
            {
                expanded.Add(Substitute(step, args));
            }

            return;
        }

        expanded.Add(expression);
    }

    private static bool TryGetCompositeName(SExpression expression, out string compositeName)
    {
        compositeName = string.Empty;
        if (expression.IsAtom || expression.Children.Count == 0)
        {
            return false;
        }

        var op = expression.Children[0].Atom;
        if (string.IsNullOrWhiteSpace(op) || !AllowedComposites.Contains(op))
        {
            return false;
        }

        compositeName = op;
        return true;
    }

    private static Dictionary<string, CompositeTemplate> LoadTemplates(IReadOnlyDictionary<string, string> loadedModules)
    {
        var templates = new Dictionary<string, CompositeTemplate>(StringComparer.OrdinalIgnoreCase);
        LoadModuleTemplates(templates, loadedModules, "locality.lisp", "locality-composite");
        LoadModuleTemplates(templates, loadedModules, "rehearsal.lisp", "rehearsal-composite");

        foreach (var required in AllowedComposites)
        {
            if (!templates.ContainsKey(required))
            {
                throw new InvalidOperationException(
                    $"Required bounded composite '{required}' is not declared in the embedded Lisp modules.");
            }
        }

        return templates;
    }

    private static void LoadModuleTemplates(
        Dictionary<string, CompositeTemplate> templates,
        IReadOnlyDictionary<string, string> loadedModules,
        string moduleName,
        string directive)
    {
        if (!loadedModules.TryGetValue(moduleName, out var moduleContent))
        {
            throw new InvalidOperationException($"Required Lisp module '{moduleName}' is not available.");
        }

        var parser = new SliParser();
        var prefix = $"({directive} ";
        var compositeLines = moduleContent
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith(prefix, StringComparison.Ordinal));

        foreach (var line in compositeLines)
        {
            var expression = parser.ParseSingle(line);
            if (expression.IsAtom || expression.Children.Count < 3)
            {
                throw new InvalidOperationException("Malformed bounded composite declaration.");
            }

            var expressionDirective = expression.Children[0].Atom;
            var compositeName = expression.Children[1].Atom;
            if (!string.Equals(expressionDirective, directive, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(compositeName))
            {
                throw new InvalidOperationException("Malformed bounded composite declaration.");
            }

            if (!AllowedComposites.Contains(compositeName))
            {
                throw new InvalidOperationException(
                    $"Composite '{compositeName}' is outside the bounded higher-order composition surface.");
            }

            var steps = expression.Children.Skip(2).ToArray();
            templates[compositeName] = new CompositeTemplate(compositeName, DetermineArity(steps), steps);
        }
    }

    private static int DetermineArity(IEnumerable<SExpression> steps)
    {
        var maxIndex = 0;
        foreach (var step in steps)
        {
            maxIndex = Math.Max(maxIndex, FindHighestPlaceholder(step));
        }

        return maxIndex;
    }

    private static int FindHighestPlaceholder(SExpression expression)
    {
        if (expression.IsAtom)
        {
            return TryParsePlaceholder(expression.Atom!, out var placeholderIndex)
                ? placeholderIndex
                : 0;
        }

        var maxIndex = 0;
        foreach (var child in expression.Children)
        {
            maxIndex = Math.Max(maxIndex, FindHighestPlaceholder(child));
        }

        return maxIndex;
    }

    private static SExpression Substitute(SExpression template, IReadOnlyList<SExpression> args)
    {
        if (template.IsAtom)
        {
            if (TryParsePlaceholder(template.Atom!, out var placeholderIndex))
            {
                var argIndex = placeholderIndex - 1;
                if (argIndex < 0 || argIndex >= args.Count)
                {
                    throw new InvalidOperationException("Composite placeholder exceeds supplied argument count.");
                }

                return args[argIndex];
            }

            return SExpression.AtomNode(template.Atom!);
        }

        return SExpression.ListNode(template.Children.Select(child => Substitute(child, args)));
    }

    private static bool TryParsePlaceholder(string value, out int index)
    {
        index = 0;
        if (value.Length < 2 || value[0] != '$')
        {
            return false;
        }

        return int.TryParse(value[1..], out index);
    }

    private sealed record CompositeTemplate(string Name, int Arity, IReadOnlyList<SExpression> Steps);
}
