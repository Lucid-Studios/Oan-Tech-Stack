namespace SLI.Ingestion;

public sealed record GovernedQueryLexicalCue(
    IReadOnlyList<string> HintTokens,
    bool SelfSensitive,
    bool ContradictionRequested);

public interface IGovernedQueryLexicalCueService
{
    GovernedQueryLexicalCue Analyze(string primaryText, IEnumerable<string>? supplementalText = null);
}

public sealed class GovernedQueryLexicalCueService : IGovernedQueryLexicalCueService
{
    private static readonly string[] SelfMarkers =
    [
        "self",
        "identity",
        "continuity",
        "autobiographical",
        "subject"
    ];

    private static readonly string[] ContradictionMarkers =
    [
        "other",
        "mismatch",
        "contradict",
        "foreign",
        "notself"
    ];

    public GovernedQueryLexicalCue Analyze(string primaryText, IEnumerable<string>? supplementalText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryText);

        var tokens = Tokenize(primaryText)
            .Concat((supplementalText ?? Array.Empty<string>()).SelectMany(Tokenize))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var selfSensitive = tokens.Any(IsSelfMarker);
        var contradictionRequested = selfSensitive && tokens.Any(IsContradictionMarker);

        return new GovernedQueryLexicalCue(
            HintTokens: tokens,
            SelfSensitive: selfSensitive,
            ContradictionRequested: contradictionRequested);
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            yield break;
        }

        var separators = new[]
        {
            ' ', '\t', '\r', '\n', ',', ';', '.', ':', '-', '_', '/', '\\', '(', ')', '[', ']', '{', '}', '"', '\''
        };
        foreach (var token in input.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (token.Length >= 3)
            {
                yield return token.ToLowerInvariant();
            }
        }
    }

    private static bool IsSelfMarker(string token) =>
        SelfMarkers.Any(marker => string.Equals(token, marker, StringComparison.OrdinalIgnoreCase));

    private static bool IsContradictionMarker(string token)
    {
        var normalized = token.Replace("-", string.Empty, StringComparison.Ordinal);
        return ContradictionMarkers.Any(marker => string.Equals(normalized, marker, StringComparison.OrdinalIgnoreCase));
    }
}
