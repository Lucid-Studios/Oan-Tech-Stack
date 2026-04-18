using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterPostExecutionOperationalActionFromExecutionPacketDocsTests
{
    [Fact]
    public void Post_Execution_Operational_Action_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_POST_EXECUTION_OPERATIONAL_ACTION_FROM_EXECUTION_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Post-execution operational-action chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Status: Proposed - Structural Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("After `808505a` and `88eff12`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostParticipationExecutionPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream operational action or post-execution reasoning may occur over anything less than a complete post-participation execution packet",
            noteText,
            StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionService.cs`", noteText, StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_EXECUTION_OPERATIONAL_ACTION_FROM_EXECUTION_PACKET.md", readmeText, StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_EXECUTION_OPERATIONAL_ACTION_FROM_EXECUTION_PACKET.md", readinessText, StringComparison.Ordinal);
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
