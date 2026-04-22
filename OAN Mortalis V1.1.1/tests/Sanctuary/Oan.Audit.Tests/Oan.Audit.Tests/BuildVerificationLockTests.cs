namespace San.Audit.Tests;

public sealed class BuildVerificationLockTests
{
    [Fact]
    public void Build_And_Test_Wrappers_Share_The_Line_Verification_Lock()
    {
        var repoRoot = GetRepoRoot();
        var buildScriptPath = Path.Combine(repoRoot, "build.ps1");
        var testScriptPath = Path.Combine(repoRoot, "test.ps1");
        var serialScriptPath = Path.Combine(repoRoot, "tools", "build-serial.ps1");
        var helperScriptPath = Path.Combine(repoRoot, "tools", "Use-LineVerificationLock.ps1");

        var buildScriptText = File.ReadAllText(buildScriptPath);
        var testScriptText = File.ReadAllText(testScriptPath);
        var serialScriptText = File.ReadAllText(serialScriptPath);
        var helperScriptText = File.ReadAllText(helperScriptPath);

        Assert.Contains("[int] $VerificationLockTimeoutSeconds = 900", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("[int] $VerificationLockTimeoutSeconds = 900", testScriptText, StringComparison.Ordinal);
        Assert.Contains("$verificationLockScriptPath = Join-Path $repoRoot \"tools\\Use-LineVerificationLock.ps1\"", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("$verificationLockScriptPath = Join-Path $repoRoot \"tools\\Use-LineVerificationLock.ps1\"", testScriptText, StringComparison.Ordinal);
        Assert.Contains(". $verificationLockScriptPath", buildScriptText, StringComparison.Ordinal);
        Assert.Contains(". $verificationLockScriptPath", testScriptText, StringComparison.Ordinal);
        Assert.Contains("Use-LineVerificationLock", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("Use-LineVerificationLock", testScriptText, StringComparison.Ordinal);
        Assert.Contains("-RepositoryRoot $repoRoot", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("-RepositoryRoot $repoRoot", testScriptText, StringComparison.Ordinal);
        Assert.Contains("-LineRoot $LineRoot", buildScriptText, StringComparison.Ordinal);
        Assert.Contains("-LineRoot $LineRoot", testScriptText, StringComparison.Ordinal);
        Assert.Contains("-VerificationLockTimeoutSeconds", serialScriptText, StringComparison.Ordinal);

        Assert.Contains("function Get-LineVerificationMutexName", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("function Use-LineVerificationLock", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("System.Threading.Mutex", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("SHA256", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("WaitOne", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("ReleaseMutex", helperScriptText, StringComparison.Ordinal);
    }

    [Fact]
    public void Verification_Lock_Helper_Is_Scoped_Per_Line()
    {
        var repoRoot = GetRepoRoot();
        var helperScriptPath = Path.Combine(repoRoot, "tools", "Use-LineVerificationLock.ps1");
        var helperScriptText = File.ReadAllText(helperScriptPath);

        Assert.Contains("line-verification|$resolvedRepositoryRoot|$resolvedLineRoot", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("Local\\OanTechStack.LineVerification.", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("AbandonedMutexException", helperScriptText, StringComparison.Ordinal);
        Assert.Contains("Timed out waiting for the shared line verification lock", helperScriptText, StringComparison.Ordinal);
    }

    private static string GetRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !string.Equals(current.Name, "OAN Tech Stack", StringComparison.OrdinalIgnoreCase))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to resolve the repo root.");
    }
}
