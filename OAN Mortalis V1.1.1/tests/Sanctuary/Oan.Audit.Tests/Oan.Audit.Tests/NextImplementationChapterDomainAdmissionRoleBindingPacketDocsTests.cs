using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterDomainAdmissionRoleBindingPacketDocsTests
{
    [Fact]
    public void Domain_Admission_Role_Binding_Packet_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ADMISSION_ROLE_BINDING_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Domain admission/role binding packet chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Proposed - Structural Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("After `0fc08e9` and `d9a871f`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainRoleGatingPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainAdmissionRoleBindingPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream governance or participation reasoning may occur over anything less than a complete admission/binding packet",
            noteText,
            StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainAdmissionRoleBindingPacketContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainAdmissionRoleBindingPacketMaterializationService.cs`", noteText, StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ADMISSION_ROLE_BINDING_PACKET.md", readmeText, StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains("NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ADMISSION_ROLE_BINDING_PACKET.md", readinessText, StringComparison.Ordinal);
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
