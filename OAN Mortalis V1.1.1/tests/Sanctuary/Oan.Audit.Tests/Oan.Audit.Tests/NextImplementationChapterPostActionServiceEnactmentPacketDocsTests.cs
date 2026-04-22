using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterPostActionServiceEnactmentPacketDocsTests
{
    [Fact]
    public void Post_Action_Service_Enactment_Packet_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_POST_ACTION_SERVICE_ENACTMENT_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Post-action service enactment packet chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Status: Seated - Packet Chapter Realized In Runtime", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostExecutionOperationalActionPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostActionServiceEnactmentPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream service consequence, effect follow-through, or post-action reasoning may occur over anything less than a complete post-action service enactment packet",
            noteText,
            StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostActionServiceEnactmentPacketContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostActionServiceEnactmentPacketMaterializationService.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("fate, closure, continuity", noteText, StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_ACTION_SERVICE_ENACTMENT_PACKET.md", readmeText, StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_POST_ACTION_SERVICE_ENACTMENT_PACKET.md", readinessText, StringComparison.Ordinal);
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
