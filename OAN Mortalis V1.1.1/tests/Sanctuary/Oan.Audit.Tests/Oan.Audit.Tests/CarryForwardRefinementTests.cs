using System.Text.Json;

namespace Oan.Audit.Tests;

public sealed class CarryForwardRefinementTests
{
    private static readonly HashSet<string> AllowedResearchBuckets = new(StringComparer.Ordinal)
    {
        "IUTT SLI & Lisp",
        "Latex Styles",
        "Trivium Forum",
        "Holographic Data Tool"
    };

    [Fact]
    public void VersionedTouchPointMatrix_Marks_ResearchHandOffs_Explicitly()
    {
        var lineRoot = GetLineRoot();
        var matrixPath = Path.Combine(lineRoot, "build", "versioned-touchpoint-matrix.json");
        using var document = JsonDocument.Parse(File.ReadAllText(matrixPath));
        var touchPoints = document.RootElement.GetProperty("touchPoints");

        var buildLaneMissing = new List<string>();
        var researchLaneMissingMetadata = new List<string>();

        foreach (var touchPoint in touchPoints.EnumerateObject())
        {
            var laneClass = GetLaneClass(touchPoint);
            var localCandidates = touchPoint.Value.TryGetProperty("localCandidates", out var localCandidatesElement)
                ? localCandidatesElement.EnumerateArray().Select(static item => item.GetString() ?? string.Empty).ToArray()
                : Array.Empty<string>();

            if (string.Equals(laneClass, "research-lane", StringComparison.Ordinal))
            {
                var status = touchPoint.Value.GetProperty("status").GetString();
                var bucket = touchPoint.Value.GetProperty("researchBucketLabel").GetString();
                var reason = touchPoint.Value.GetProperty("researchReason").GetString();

                if (!string.Equals(status, "research-handoff", StringComparison.Ordinal) ||
                    string.IsNullOrWhiteSpace(bucket) ||
                    !AllowedResearchBuckets.Contains(bucket) ||
                    string.IsNullOrWhiteSpace(reason))
                {
                    researchLaneMissingMetadata.Add(touchPoint.Name);
                }

                continue;
            }

            if (!string.Equals(laneClass, "build-lane", StringComparison.Ordinal))
            {
                continue;
            }

            var resolvedCandidates = localCandidates
                .Select(candidate => ResolveMatrixCandidate(candidate, lineRoot))
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToArray();

            if (resolvedCandidates.Length == 0 || resolvedCandidates.All(path => !File.Exists(path)))
            {
                buildLaneMissing.Add(touchPoint.Name);
            }
        }

        Assert.True(researchLaneMissingMetadata.Count == 0, string.Join(Environment.NewLine, researchLaneMissingMetadata));
        Assert.True(buildLaneMissing.Count == 0, string.Join(Environment.NewLine, buildLaneMissing));
    }

    [Fact]
    public void HistoricalArchiveDocs_Use_ExternalArchive_Posture()
    {
        var lineRoot = GetLineRoot();
        var retirementGatePath = Path.Combine(lineRoot, "docs", "V1_0_RETIREMENT_GATE.md");
        var migrationCharterPath = Path.Combine(lineRoot, "docs", "V1_1_1_MIGRATION_CHARTER.md");

        var retirementGateText = File.ReadAllText(retirementGatePath);
        var migrationCharterText = File.ReadAllText(migrationCharterPath);

        Assert.Contains("external historical archive", retirementGateText, StringComparison.Ordinal);
        Assert.Contains("historical line", migrationCharterText, StringComparison.Ordinal);
        Assert.DoesNotContain("remains in the repository", retirementGateText, StringComparison.Ordinal);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Mortalis V1.1.1", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the V1.1.1 line root.");
    }

    private static string GetLaneClass(JsonProperty touchPoint)
    {
        if (touchPoint.Value.TryGetProperty("laneClass", out var laneClassElement))
        {
            var explicitLaneClass = laneClassElement.GetString();
            if (!string.IsNullOrWhiteSpace(explicitLaneClass))
            {
                return explicitLaneClass;
            }
        }

        if (touchPoint.Name.StartsWith("policy.", StringComparison.OrdinalIgnoreCase))
        {
            return "policy-lane";
        }

        if (touchPoint.Name.StartsWith("docs.", StringComparison.OrdinalIgnoreCase))
        {
            return "documentation-lane";
        }

        return "build-lane";
    }

    private static string ResolveMatrixCandidate(string template, string lineRoot)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        var automationPolicyRoot = Path.Combine(lineRoot, "build").Replace('\\', '/');
        var activeBuildRoot = lineRoot.Replace('\\', '/');

        var resolved = template
            .Replace("{automationPolicyRoot}", automationPolicyRoot, StringComparison.Ordinal)
            .Replace("{activeBuildRoot}", activeBuildRoot, StringComparison.Ordinal)
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(resolved);
    }
}
