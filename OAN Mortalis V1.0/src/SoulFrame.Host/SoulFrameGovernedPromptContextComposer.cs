namespace SoulFrame.Host;

public static class SoulFrameGovernedPromptContextComposer
{
    private const string BoundedLocalityContinuityDomain = "bounded-locality continuity";
    private const string FluidContinuityLawDomain = "fluid continuity law";

    public static string Compose(
        string task,
        string rawContext,
        SoulFrameInferenceConstraints constraints,
        string? objectiveHint = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawContext);
        ArgumentNullException.ThrowIfNull(constraints);

        var trimmedContext = rawContext.Trim();
        var trimmedObjective = string.IsNullOrWhiteSpace(objectiveHint)
            ? null
            : objectiveHint.Trim();

        if (!TryResolveContinuityBasin(trimmedContext, constraints.Domain, trimmedObjective, out var activeDomain, out var excludedDomain))
        {
            return trimmedContext;
        }

        var segments = new List<string>
        {
            $"ACTIVE_DOCTRINE_DOMAIN: {activeDomain}",
            $"EXCLUDED_NEARBY_DOMAIN: {excludedDomain}",
            "COLLAPSE_RULE: if domain confidence is split, emit the unresolved governed state or the needs-more-information governed state; do not choose the excluded nearby domain.",
            "JUSTIFICATION_RULE: tie the answer to ACTIVE_DOCTRINE_DOMAIN in one short clause.",
            $"TASK: {task}"
        };

        if (!string.IsNullOrWhiteSpace(trimmedObjective) &&
            !string.Equals(trimmedObjective, trimmedContext, StringComparison.Ordinal))
        {
            segments.Add($"OBJECTIVE_HINT: {trimmedObjective}");
        }

        segments.Add($"INPUT: {trimmedContext}");
        return string.Join(Environment.NewLine, segments);
    }

    public static string ResolveDomain(
        string rawContext,
        string configuredDomain,
        string? objectiveHint = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configuredDomain);
        ArgumentNullException.ThrowIfNull(rawContext);

        return TryResolveContinuityBasin(rawContext, configuredDomain, objectiveHint, out var activeDomain, out _)
            ? activeDomain
            : configuredDomain.Trim();
    }

    private static bool TryResolveContinuityBasin(
        string rawContext,
        string configuredDomain,
        string? objectiveHint,
        out string activeDomain,
        out string excludedDomain)
    {
        var combined = string.Join(
            " ",
            new[]
            {
                rawContext,
                configuredDomain,
                objectiveHint ?? string.Empty
            }.Where(value => !string.IsNullOrWhiteSpace(value)))
            .ToLowerInvariant();

        if (ContainsAny(
                combined,
                "bounded-locality",
                "bounded locality",
                "identity-continuity",
                "identity continuity",
                "locality-bootstrap",
                "locality-state") ||
            (combined.Contains("locality", StringComparison.Ordinal) &&
             combined.Contains("continuity", StringComparison.Ordinal)))
        {
            activeDomain = BoundedLocalityContinuityDomain;
            excludedDomain = FluidContinuityLawDomain;
            return true;
        }

        if (ContainsAny(combined, "fluid continuity", "fluid-continuity", "fluid law", "fluid-law"))
        {
            activeDomain = FluidContinuityLawDomain;
            excludedDomain = BoundedLocalityContinuityDomain;
            return true;
        }

        activeDomain = string.Empty;
        excludedDomain = string.Empty;
        return false;
    }

    private static bool ContainsAny(string value, params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (value.Contains(candidate, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
