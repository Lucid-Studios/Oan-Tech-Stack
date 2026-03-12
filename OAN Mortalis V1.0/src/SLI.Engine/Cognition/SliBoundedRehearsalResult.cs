using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

internal sealed class SliRehearsalSubstitutionResult
{
    public required string Source { get; init; }
    public required string Target { get; init; }
}

internal sealed class SliRehearsalAnalogyResult
{
    public required string Source { get; init; }
    public required string Target { get; init; }
}

internal sealed class SliRehearsalResult
{
    public required bool IsConfigured { get; init; }
    public required string RehearsalHandle { get; init; }
    public required string SourceLocalityHandle { get; init; }
    public required string Mode { get; init; }
    public required string IdentitySeal { get; init; }
    public required string AdmissionStatus { get; init; }
    public required bool IsBindable { get; init; }
    public required IReadOnlyList<string> BranchSet { get; init; }
    public required IReadOnlyList<SliRehearsalSubstitutionResult> SubstitutionLedger { get; init; }
    public required IReadOnlyList<SliRehearsalAnalogyResult> AnalogyLedger { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public required IReadOnlyList<HigherOrderLocalityResidue> Residues { get; init; }
}

internal sealed class SliBoundedRehearsalResult
{
    public required SliHigherOrderLocalityResult Locality { get; init; }
    public required SliRehearsalResult Rehearsal { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
}
