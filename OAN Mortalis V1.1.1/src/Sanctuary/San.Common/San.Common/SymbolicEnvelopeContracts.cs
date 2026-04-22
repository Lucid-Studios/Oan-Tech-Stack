namespace San.Common;

public readonly record struct SymbolicProductFamily(string Value);

public readonly record struct SymbolicIntent(string Value);

public enum SymbolicProductClass
{
    ReadProduct = 0,
    CandidateProduct = 1,
    DirectiveProduct = 2,
    CollapseProduct = 3
}

public enum AdmissibilityStatus
{
    Pending = 0,
    Admissible = 1,
    Refused = 2
}

public enum ContradictionState
{
    None = 0,
    Soft = 1,
    Hard = 2
}

public enum MaterializationEligibility
{
    No = 0,
    Restricted = 1,
    Yes = 2
}

public enum PersistenceEligibility
{
    Never = 0,
    AuditOnly = 1,
    Promotable = 2
}

public enum MembraneDecision
{
    Accept = 0,
    Transform = 1,
    Defer = 2,
    Refuse = 3,
    Collapse = 4
}

public sealed record SymbolicEnvelope(
    string Origin,
    SymbolicProductFamily Family,
    SymbolicProductClass ProductClass,
    SymbolicIntent Intent,
    AdmissibilityStatus Admissibility,
    ContradictionState ContradictionState,
    MaterializationEligibility MaterializationEligibility,
    PersistenceEligibility PersistenceEligibility,
    string TraceId);
