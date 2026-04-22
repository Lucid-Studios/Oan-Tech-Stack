namespace San.Audit.Tests;

public sealed class LegalFoundationDocumentationMatrixDocsTests
{
    [Fact]
    public void Legal_Foundation_Documentation_Matrix_Preserves_Authorization_Not_Origin_Boundary()
    {
        var lineRoot = GetLineRoot();
        var matrixPath = Path.Combine(lineRoot, "docs", "LEGAL_FOUNDATION_DOCUMENTATION_MATRIX.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");

        Assert.True(File.Exists(matrixPath));

        var matrixText = File.ReadAllText(matrixPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);

        Assert.Contains("Legal documentation is a gate, not a generator.", matrixText, StringComparison.Ordinal);
        Assert.Contains("Legal != Ontology.", matrixText, StringComparison.Ordinal);
        Assert.Contains("Authorization != Origin.", matrixText, StringComparison.Ordinal);
        Assert.Contains("Permission != Being.", matrixText, StringComparison.Ordinal);
        Assert.Contains("not legal advice, not counsel-approved language", matrixText, StringComparison.Ordinal);
        Assert.Contains("construct `Mother` from EULA or disclosure language", matrixText, StringComparison.Ordinal);
        Assert.Contains("construct `Father` from operational terms", matrixText, StringComparison.Ordinal);
        Assert.Contains("LEGAL_FOUNDATION_DOCUMENTATION_MATRIX.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("legal-foundation-documentation-matrix: template-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Legal foundation documentation matrix template preserved", carryForwardText, StringComparison.Ordinal);
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
