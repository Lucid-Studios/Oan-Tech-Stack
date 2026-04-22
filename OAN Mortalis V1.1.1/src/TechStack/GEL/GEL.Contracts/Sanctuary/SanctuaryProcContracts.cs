namespace GEL.Contracts.Sanctuary;

public enum SanctuaryGelProceduralInvariantKind
{
    Grounding = 0,
    DownwardResolution = 1,
    PostureStability = 2,
    TraceValidity = 3,
    CrypticIntegrity = 4
}

public enum SanctuaryGelProcedureDisposition
{
    Admissible = 0,
    Gated = 1,
    Refused = 2
}

public sealed record SanctuaryGelProcedureDefinition
{
    public SanctuaryGelProcedureDefinition(
        string handle,
        string meaning,
        IReadOnlyList<string> canonicalBodyFields,
        IReadOnlyList<string> requiredLowerChain,
        IReadOnlyList<SanctuaryGelProceduralInvariantKind> governingInvariants,
        IReadOnlyList<string> requiredWitnesses,
        IReadOnlyList<string> refusalTriggers,
        string admissibilityRule,
        string executionRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
        CanonicalBodyFields = SanctuaryContractGuard.RequiredTextList(canonicalBodyFields, nameof(canonicalBodyFields));
        RequiredLowerChain = SanctuaryContractGuard.RequiredTextList(requiredLowerChain, nameof(requiredLowerChain));
        GoverningInvariants = SanctuaryContractGuard.RequiredDistinctList(governingInvariants, nameof(governingInvariants));
        RequiredWitnesses = SanctuaryContractGuard.RequiredTextList(requiredWitnesses, nameof(requiredWitnesses));
        RefusalTriggers = SanctuaryContractGuard.RequiredTextList(refusalTriggers, nameof(refusalTriggers));
        AdmissibilityRule = SanctuaryContractGuard.RequiredText(admissibilityRule, nameof(admissibilityRule));
        ExecutionRule = SanctuaryContractGuard.RequiredText(executionRule, nameof(executionRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string Meaning { get; }

    public IReadOnlyList<string> CanonicalBodyFields { get; }

    public IReadOnlyList<string> RequiredLowerChain { get; }

    public IReadOnlyList<SanctuaryGelProceduralInvariantKind> GoverningInvariants { get; }

    public IReadOnlyList<string> RequiredWitnesses { get; }

    public IReadOnlyList<string> RefusalTriggers { get; }

    public string AdmissibilityRule { get; }

    public string ExecutionRule { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryGelProcedureAtlas
{
    public static SanctuaryGelProcedureDefinition Procedure { get; } =
        new(
            handle: "gel.proc.grounded-action.v0",
            meaning: "grounded-posture-gated-action-pattern-derived-from-root-definition-and-relation",
            canonicalBodyFields:
            [
                "pi",
                "delta",
                "alpha*",
                "nu",
                "phi",
                "chi",
                "sigma_proc"
            ],
            requiredLowerChain:
            [
                "root",
                "def",
                "rel"
            ],
            governingInvariants:
            [
                SanctuaryGelProceduralInvariantKind.Grounding,
                SanctuaryGelProceduralInvariantKind.DownwardResolution,
                SanctuaryGelProceduralInvariantKind.PostureStability,
                SanctuaryGelProceduralInvariantKind.TraceValidity,
                SanctuaryGelProceduralInvariantKind.CrypticIntegrity
            ],
            requiredWitnesses:
            [
                "simulation-required",
                "trace-resolves-to-supporting-anchors",
                "prime-posture-required",
                "cryptic-integrity-sealed"
            ],
            refusalTriggers:
            [
                "ungrounded-step",
                "hidden-assumption",
                "action-first-reasoning",
                "lower-layer-bypass"
            ],
            admissibilityRule: "delta=P-anchor-support-and-R-D-E-grounding-prime-posture-and-phi-required",
            executionRule: "execute-only-when-prime-posture-holds",
            operationalStatus: "placeholder-contract-only");
}
