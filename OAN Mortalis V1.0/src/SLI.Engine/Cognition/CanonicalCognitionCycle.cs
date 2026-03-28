namespace SLI.Engine.Cognition;

public static class CanonicalCognitionCycle
{
    public static readonly IReadOnlyList<string> Steps =
    [
        "signal-intake",
        "pre-sli-translation",
        "sli-packetization",
        "engram-context-expansion",
        "symbolic-reasoning",
        "higher-order-locality-bootstrap",
        "perspectival-configuration",
        "participatory-configuration",
        "golden-code-bloom",
        "compass-orientation-update",
        "decision-branch-resolution",
        "ontological-cleave",
        "engram-commit-proposal",
        "steward-governance-pipeline"
    ];

    public static void ValidateProgramOrder(IReadOnlyList<string> symbolicProgram)
    {
        ArgumentNullException.ThrowIfNull(symbolicProgram);

        var reasoningIndex = IndexOf(symbolicProgram, "(decision-evaluate");
        var localityIndex = IndexOf(symbolicProgram, "(locality-bootstrap");
        var perspectiveIndex = IndexOf(symbolicProgram, "(perspective-bounded-observer");
        var participationIndex = IndexOf(symbolicProgram, "(participation-bounded-cme");
        var goldenCodeIndex = IndexOf(symbolicProgram, "(golden-code-bloom");
        var compassIndex = IndexOf(symbolicProgram, "(compass-update");
        var decisionIndex = IndexOf(symbolicProgram, "(decision-branch");
        var cleaveIndex = IndexOf(symbolicProgram, "(cleave");
        var commitIndex = IndexOf(symbolicProgram, "(commit");

        if (reasoningIndex < 0 ||
            localityIndex < 0 ||
            perspectiveIndex < 0 ||
            participationIndex < 0 ||
            goldenCodeIndex < 0 ||
            compassIndex < 0 ||
            decisionIndex < 0 ||
            cleaveIndex < 0 ||
            commitIndex < 0)
        {
            throw new InvalidOperationException(
                "Canonical cognition cycle requires reasoning, locality, perspective, participation, golden-code bloom, compass, decision, cleave, and commit steps.");
        }

        var valid =
            reasoningIndex < localityIndex &&
            localityIndex < perspectiveIndex &&
            perspectiveIndex < participationIndex &&
            participationIndex < goldenCodeIndex &&
            goldenCodeIndex < compassIndex &&
            compassIndex < decisionIndex &&
            decisionIndex < cleaveIndex &&
            cleaveIndex < commitIndex;

        if (!valid)
        {
            throw new InvalidOperationException("Canonical cognition cycle ordering violation.");
        }
    }

    private static int IndexOf(IReadOnlyList<string> symbolicProgram, string fragment)
    {
        for (var index = 0; index < symbolicProgram.Count; index++)
        {
            if (symbolicProgram[index].Contains(fragment, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
