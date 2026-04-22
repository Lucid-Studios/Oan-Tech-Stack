using System.Text;

namespace San.Audit.Tests;

public sealed class NextImplementationChapterPostAdmissionParticipationFromAdmissionBindingPacketDocsTests
{
    [Fact]
    public void Post_Admission_Participation_Chapter_Is_Seated_In_Repo_Docs()
    {
        var repoRoot = ResolveRepoRoot();
        var notePath = Path.Combine(
            repoRoot,
            "OAN Mortalis V1.1.1",
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_POST_ADMISSION_PARTICIPATION_FROM_ADMISSION_BINDING_PACKET.md");
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readinessPath = Path.Combine(repoRoot, "OAN Mortalis V1.1.1", "docs", "BUILD_READINESS.md");

        Assert.True(File.Exists(notePath), "Post-admission participation chapter note should exist.");

        var noteText = File.ReadAllText(notePath, Encoding.UTF8);
        Assert.Contains("Proposed - Structural Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("After `0fc08e9`, `d9a871f`, and `03acdde`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainAdmissionRoleBindingPacket`", noteText, StringComparison.Ordinal);
        Assert.Contains(
            "no downstream participation, occupancy, or post-admission reasoning may occur over anything less than a complete domain admission / role-binding packet",
            noteText,
            StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostAdmissionParticipationContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPostAdmissionParticipationService.cs`", noteText, StringComparison.Ordinal);

        var readmeText = File.ReadAllText(readmePath, Encoding.UTF8);
        Assert.Contains(
            "NEXT_IMPLEMENTATION_CHAPTER_POST_ADMISSION_PARTICIPATION_FROM_ADMISSION_BINDING_PACKET.md",
            readmeText,
            StringComparison.Ordinal);

        var readinessText = File.ReadAllText(readinessPath, Encoding.UTF8);
        Assert.Contains(
            "NEXT_IMPLEMENTATION_CHAPTER_POST_ADMISSION_PARTICIPATION_FROM_ADMISSION_BINDING_PACKET.md",
            readinessText,
            StringComparison.Ordinal);
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
