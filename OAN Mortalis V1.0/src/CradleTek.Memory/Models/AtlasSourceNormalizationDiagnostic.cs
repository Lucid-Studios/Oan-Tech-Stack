namespace CradleTek.Memory.Models;

public enum AtlasSourceDiagnosticSeverity
{
    Warning,
    Error
}

public sealed class AtlasSourceNormalizationDiagnostic
{
    public required AtlasSourceDiagnosticSeverity Severity { get; init; }
    public required string Code { get; init; }
    public required string SourceLayer { get; init; }
    public required string Message { get; init; }
    public string? RootKey { get; init; }
}
