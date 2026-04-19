using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterPostActionServiceEnactmentFromOperationalActionPacketDocsTests
{
    [Fact]
    public void Post_Action_Service_Enactment_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_POST_ACTION_SERVICE_ENACTMENT_FROM_OPERATIONAL_ACTION_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Post-action service enactment chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Status: Proposed - Service Enactment Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostActionServiceEnactmentContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostActionServiceEnactmentService.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedEffectEmissionAssessment`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedServiceEnactmentCommitAssessment`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostActionServiceEnactmentReceipt`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream enactment, effect emission, or post-action reasoning may occur over anything less than a complete operational-action packet",
            noteText,
            StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_ACTION_SERVICE_ENACTMENT_FROM_OPERATIONAL_ACTION_PACKET.md", readmeText, StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_ACTION_SERVICE_ENACTMENT_FROM_OPERATIONAL_ACTION_PACKET.md", readinessText, StringComparison.Ordinal);
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "README.md")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root could not be located.");
    }
}
