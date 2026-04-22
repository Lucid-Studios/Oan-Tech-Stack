namespace San.Audit.Tests;

public sealed class NextImplementationChapterDomainRoleGatingFromPredomainPacketDocsTests
{
    [Fact]
    public void Domain_Role_Gating_Chapter_Preserves_Packet_First_Gating_Invariant()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(
            lineRoot,
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_FROM_PREDOMAIN_PACKET.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var readinessText = File.ReadAllText(readinessPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPreDomainGovernancePacket`", noteText, StringComparison.Ordinal);
        Assert.Contains("no domain/role gating may occur over anything less than a complete", noteText, StringComparison.Ordinal);
        Assert.Contains("## Current Achieved State", noteText, StringComparison.Ordinal);
        Assert.Contains("After `ecb224c`", noteText, StringComparison.Ordinal);
        Assert.Contains("## Input Body", noteText, StringComparison.Ordinal);
        Assert.Contains("## First Gating Intent", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainRoleGatingContracts.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedDomainRoleGatingService.cs`", noteText, StringComparison.Ordinal);
        Assert.Contains("## First Disposition Family", noteText, StringComparison.Ordinal);
        Assert.Contains("`DomainAdmissibleRoleIncomplete`", noteText, StringComparison.Ordinal);
        Assert.Contains("`DomainAndRoleAdmissible`", noteText, StringComparison.Ordinal);
        Assert.Contains("## Suggested First Service", noteText, StringComparison.Ordinal);
        Assert.Contains("## Witness Plan", noteText, StringComparison.Ordinal);
        Assert.Contains("the pre-domain chain now has a body", noteText, StringComparison.Ordinal);

        Assert.Contains(
            "NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_FROM_PREDOMAIN_PACKET.md",
            readinessText,
            StringComparison.Ordinal);
        Assert.Contains(
            "NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_FROM_PREDOMAIN_PACKET.md",
            readmeText,
            StringComparison.Ordinal);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
