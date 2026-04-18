using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterDomainRoleGatingPacketDocsTests
{
    [Fact]
    public void Domain_Role_Gating_Packet_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Domain/role gating packet chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Proposed - Structural Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("After `b30a01f`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPreDomainGovernancePacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainRoleGatingPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream admission or role-binding reasoning may occur over anything less than a complete domain/role gating packet",
            noteText,
            StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainRoleGatingPacketContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainRoleGatingPacketMaterializationService.cs`", noteText, StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_PACKET.md", readmeText, StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_PACKET.md", readinessText, StringComparison.Ordinal);
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
