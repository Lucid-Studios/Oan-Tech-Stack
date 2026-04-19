using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterPostExecutionOperationalActionPacketDocsTests
{
    [Fact]
    public void Post_Execution_Operational_Action_Packet_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_POST_EXECUTION_OPERATIONAL_ACTION_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Post-execution operational-action packet chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Status: Seated - Packet Chapter Realized In Runtime", noteText, StringComparison.Ordinal);
        Assert.Contains("After `68d8a87`, `309cee0`, and `1e4525d`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostParticipationExecutionPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedCommitIntent`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedCommitReceipt`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream service enactment, effect emission, or post-action reasoning may occur over anything less than a complete post-execution operational-action packet",
            noteText,
            StringComparison.Ordinal);
        Assert.DoesNotContain("What is still missing is one stable carried body", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionPacketContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionPacketMaterializationService.cs`", noteText, StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_EXECUTION_OPERATIONAL_ACTION_PACKET.md", readmeText, StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_EXECUTION_OPERATIONAL_ACTION_PACKET.md", readinessText, StringComparison.Ordinal);
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
