using Oan.Common;

namespace SoulFrame.Host;

public static class SoulFrameCompassAdvisoryComposer
{
    public static SoulFrameCompassAdvisoryRequest CreateSeedRequired(
        SoulFrameInferenceConstraints constraints,
        string objective)
    {
        ArgumentNullException.ThrowIfNull(constraints);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);

        var (activeBasin, excludedCompetingBasin) = ResolveDoctrineBasinPair(constraints, objective);
        return new SoulFrameCompassAdvisoryRequest
        {
            Version = "compass-seed-advisory-v1",
            RequireStructuredAdvisory = true,
            TargetActiveBasin = activeBasin,
            ExcludedCompetingBasin = excludedCompetingBasin
        };
    }

    public static (CompassDoctrineBasin ActiveBasin, CompassDoctrineBasin ExcludedCompetingBasin) ResolveDoctrineBasinPair(
        SoulFrameInferenceConstraints constraints,
        string objective)
    {
        ArgumentNullException.ThrowIfNull(constraints);
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);

        var normalized = $"{constraints.Domain} {objective}".ToLowerInvariant();
        if (normalized.Contains("bounded-locality continuity", StringComparison.Ordinal) ||
            normalized.Contains("bounded locality continuity", StringComparison.Ordinal))
        {
            return (CompassDoctrineBasin.BoundedLocalityContinuity, CompassDoctrineBasin.FluidContinuityLaw);
        }

        if (normalized.Contains("fluid continuity law", StringComparison.Ordinal) ||
            normalized.Contains("fluid continuity", StringComparison.Ordinal))
        {
            return (CompassDoctrineBasin.FluidContinuityLaw, CompassDoctrineBasin.BoundedLocalityContinuity);
        }

        if (normalized.Contains("identity continuity", StringComparison.Ordinal) ||
            normalized.Contains("identity-continuity", StringComparison.Ordinal))
        {
            return (CompassDoctrineBasin.IdentityContinuity, CompassDoctrineBasin.Unknown);
        }

        if (normalized.Contains("continuity", StringComparison.Ordinal))
        {
            return (CompassDoctrineBasin.GeneralContinuityDiscourse, CompassDoctrineBasin.Unknown);
        }

        var resolvedDomain = SoulFrameGovernedPromptContextComposer.ResolveDomain(
            rawContext: objective,
            configuredDomain: constraints.Domain,
            objectiveHint: objective);

        return resolvedDomain.Trim() switch
        {
            "bounded-locality continuity" => (
                CompassDoctrineBasin.BoundedLocalityContinuity,
                CompassDoctrineBasin.FluidContinuityLaw),
            "fluid continuity law" => (
                CompassDoctrineBasin.FluidContinuityLaw,
                CompassDoctrineBasin.BoundedLocalityContinuity),
            _ => (CompassDoctrineBasin.Unknown, CompassDoctrineBasin.Unknown)
        };
    }
}
