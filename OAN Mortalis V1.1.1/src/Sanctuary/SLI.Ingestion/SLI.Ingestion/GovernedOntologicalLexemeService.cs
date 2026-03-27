using System.Text.RegularExpressions;

namespace SLI.Ingestion;

public interface IGovernedOntologicalLexemeService
{
    IReadOnlyList<string> Tokenize(string inputText);

    string NormalizeToken(string token);

    bool TryNormalizeMorphology(string token, out string normalized);
}

public sealed partial class GovernedOntologicalLexemeService : IGovernedOntologicalLexemeService
{
    public IReadOnlyList<string> Tokenize(string inputText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputText);

        return LexemeRegex()
            .Matches(inputText)
            .Select(static match => match.Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    public string NormalizeToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var normalized = token.Trim().ToLowerInvariant();
        return normalized.Trim('\'', '"', '`', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}');
    }

    public bool TryNormalizeMorphology(string token, out string normalized)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        normalized = string.Empty;
        if (token.Length < 4)
        {
            return false;
        }

        if (token.EndsWith("ies", StringComparison.Ordinal) && token.Length > 4)
        {
            normalized = token[..^3] + "y";
            return true;
        }

        if (token.EndsWith("ing", StringComparison.Ordinal) && token.Length > 5)
        {
            normalized = token[..^3];
            return true;
        }

        if (token.EndsWith("ed", StringComparison.Ordinal) && token.Length > 4)
        {
            normalized = token[..^2];
            return true;
        }

        if (token.EndsWith("es", StringComparison.Ordinal) && token.Length > 4)
        {
            normalized = token[..^2];
            return true;
        }

        if (token.EndsWith('s') && token.Length > 3)
        {
            normalized = token[..^1];
            return true;
        }

        return false;
    }

    [GeneratedRegex("[\\p{L}\\p{Mn}][\\p{L}\\p{Mn}'\\-]*", RegexOptions.Compiled)]
    private static partial Regex LexemeRegex();
}
