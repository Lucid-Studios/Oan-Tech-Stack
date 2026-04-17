namespace San.Audit.Tests;

public sealed class NextImplementationChapterCandidateBoundaryAndDuplexGovernanceDocsTests
{
    [Fact]
    public void Candidate_Boundary_And_Duplex_Governance_Chapter_Preserves_The_Next_Seam()
    {
        var lineRoot = GetLineRoot();
        var repoRoot = Directory.GetParent(lineRoot)?.FullName
            ?? throw new InvalidOperationException("Unable to locate repository root.");

        var notePath = Path.Combine(
            lineRoot,
            "docs",
            "NEXT_IMPLEMENTATION_CHAPTER_CANDIDATE_BOUNDARY_AND_DUPLEX_GOVERNANCE.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var readmePath = Path.Combine(repoRoot, "README.md");

        Assert.True(File.Exists(notePath));

        var noteText = File.ReadAllText(notePath);
        var readinessText = File.ReadAllText(readinessPath);
        var readmeText = File.ReadAllText(readmePath);

        Assert.Contains("Proposed - Structural Chapter Candidate", noteText, StringComparison.Ordinal);
        Assert.Contains("Advance the governed pre-domain host loop beyond checkpointing", noteText, StringComparison.Ordinal);
        Assert.Contains("the host must inspect holistically, then separate what belongs to Prime", noteText, StringComparison.Ordinal);
        Assert.Contains("Current Achieved State", noteText, StringComparison.Ordinal);
        Assert.Contains("After `6859b58`, the runtime possesses a first governed pre-domain host loop.", noteText, StringComparison.Ordinal);
        Assert.Contains("## 1. Candidate-Only Proposal Boundary", noteText, StringComparison.Ordinal);
        Assert.Contains("`IGovernedSeedCandidateProposal`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedCandidateEnvelope`", noteText, StringComparison.Ordinal);
        Assert.Contains("Never as:", noteText, StringComparison.Ordinal);
        Assert.Contains("## 2. Whole-to-Duplex Separation", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedPrimeCandidateView`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedCrypticCandidateView`", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedCandidateSeparationService`", noteText, StringComparison.Ordinal);
        Assert.Contains("Cryptic-bearing material must never inherit Prime authority by proximity.", noteText, StringComparison.Ordinal);
        Assert.Contains("## 3. Pre-Domain Admission Gate", noteText, StringComparison.Ordinal);
        Assert.Contains("`PrimeSeedPreDomainAdmissionAssessment`", noteText, StringComparison.Ordinal);
        Assert.Contains("`PreDomainAdmissionGateReceipt`", noteText, StringComparison.Ordinal);
        Assert.Contains("Admission may only be reasoned over duplex-separated material.", noteText, StringComparison.Ordinal);
        Assert.Contains("## New Receipts", noteText, StringComparison.Ordinal);
        Assert.Contains("`GovernedSeedCandidateBoundaryReceipt`", noteText, StringComparison.Ordinal);
        Assert.Contains("## Host Loop Extension", noteText, StringComparison.Ordinal);
        Assert.Contains("## Witness Plan", noteText, StringComparison.Ordinal);
        Assert.Contains("The next build step is to make proposal boundaries explicit", noteText, StringComparison.Ordinal);

        Assert.Contains(
            "NEXT_IMPLEMENTATION_CHAPTER_CANDIDATE_BOUNDARY_AND_DUPLEX_GOVERNANCE.md",
            readinessText,
            StringComparison.Ordinal);
        Assert.Contains(
            "NEXT_IMPLEMENTATION_CHAPTER_CANDIDATE_BOUNDARY_AND_DUPLEX_GOVERNANCE.md",
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
