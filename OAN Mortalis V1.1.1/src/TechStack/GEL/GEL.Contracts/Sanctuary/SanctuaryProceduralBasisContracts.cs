namespace GEL.Contracts.Sanctuary;

public enum SanctuaryProceduralBasisPrimitiveKind
{
    Retain = 0,
    Relate = 1,
    Verify = 2,
    Mint = 3,
    Refuse = 4,
    Preserve = 5,
    Condense = 6
}

public enum SanctuaryProceduralCompositionPatternKind
{
    Admission = 0,
    Receipt = 1,
    Relation = 2,
    Prune = 3,
    Condense = 4
}

public sealed record SanctuaryProceduralBasisPrimitiveDefinition
{
    public SanctuaryProceduralBasisPrimitiveDefinition(
        SanctuaryProceduralBasisPrimitiveKind primitive,
        string handle,
        string meaning)
    {
        Primitive = primitive;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Meaning = SanctuaryContractGuard.RequiredText(meaning, nameof(meaning));
    }

    public SanctuaryProceduralBasisPrimitiveKind Primitive { get; }

    public string Handle { get; }

    public string Meaning { get; }
}

public sealed record SanctuaryProceduralCompositionPatternDefinition
{
    public SanctuaryProceduralCompositionPatternDefinition(
        SanctuaryProceduralCompositionPatternKind pattern,
        string handle,
        string composition)
    {
        Pattern = pattern;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Composition = SanctuaryContractGuard.RequiredText(composition, nameof(composition));
    }

    public SanctuaryProceduralCompositionPatternKind Pattern { get; }

    public string Handle { get; }

    public string Composition { get; }
}

public sealed record SanctuaryProceduralBasisDefinition
{
    public SanctuaryProceduralBasisDefinition(
        string handle,
        IReadOnlyList<SanctuaryProceduralBasisPrimitiveKind> primitives,
        IReadOnlyList<SanctuaryProceduralCompositionPatternKind> compositionPatterns,
        string excludedFreePrimitive,
        string gatingRule,
        string executionBoundary,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Primitives = SanctuaryContractGuard.RequiredDistinctList(primitives, nameof(primitives));
        CompositionPatterns = SanctuaryContractGuard.RequiredDistinctList(compositionPatterns, nameof(compositionPatterns));
        ExcludedFreePrimitive = SanctuaryContractGuard.RequiredText(excludedFreePrimitive, nameof(excludedFreePrimitive));
        GatingRule = SanctuaryContractGuard.RequiredText(gatingRule, nameof(gatingRule));
        ExecutionBoundary = SanctuaryContractGuard.RequiredText(executionBoundary, nameof(executionBoundary));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<SanctuaryProceduralBasisPrimitiveKind> Primitives { get; }

    public IReadOnlyList<SanctuaryProceduralCompositionPatternKind> CompositionPatterns { get; }

    public string ExcludedFreePrimitive { get; }

    public string GatingRule { get; }

    public string ExecutionBoundary { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryProceduralBasisPrimeFaceDefinition
{
    public SanctuaryProceduralBasisPrimeFaceDefinition(
        string handle,
        string id,
        string layer,
        string role,
        string postureGate,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Id = SanctuaryContractGuard.RequiredText(id, nameof(id));
        Layer = SanctuaryContractGuard.RequiredText(layer, nameof(layer));
        Role = SanctuaryContractGuard.RequiredText(role, nameof(role));
        PostureGate = SanctuaryContractGuard.RequiredText(postureGate, nameof(postureGate));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public string Id { get; }

    public string Layer { get; }

    public string Role { get; }

    public string PostureGate { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryProceduralBasisCrypticFaceDefinition
{
    public SanctuaryProceduralBasisCrypticFaceDefinition(
        string handle,
        IReadOnlyList<string> slots,
        string integrityRule,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Slots = SanctuaryContractGuard.RequiredTextList(slots, nameof(slots));
        IntegrityRule = SanctuaryContractGuard.RequiredText(integrityRule, nameof(integrityRule));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> Slots { get; }

    public string IntegrityRule { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryProceduralBasisReceiptDefinition
{
    public SanctuaryProceduralBasisReceiptDefinition(
        string handle,
        IReadOnlyList<string> receiptFields,
        string result,
        string operationalStatus)
    {
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        ReceiptFields = SanctuaryContractGuard.RequiredTextList(receiptFields, nameof(receiptFields));
        Result = SanctuaryContractGuard.RequiredText(result, nameof(result));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public string Handle { get; }

    public IReadOnlyList<string> ReceiptFields { get; }

    public string Result { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryProceduralBasisAtlas
{
    public static IReadOnlyList<SanctuaryProceduralBasisPrimitiveDefinition> Primitives { get; } =
    [
        new(SanctuaryProceduralBasisPrimitiveKind.Retain, "basis.retain", "admit-lawful-object-into-retained-structure"),
        new(SanctuaryProceduralBasisPrimitiveKind.Relate, "basis.relate", "form-lawful-relation-between-grounded-members"),
        new(SanctuaryProceduralBasisPrimitiveKind.Verify, "basis.verify", "check-grounding-posture-and-trace-before-action"),
        new(SanctuaryProceduralBasisPrimitiveKind.Mint, "basis.mint", "issue-lawful-receipt-or-seal-only-after-verification"),
        new(SanctuaryProceduralBasisPrimitiveKind.Refuse, "basis.refuse", "deny-unlawful-or-unstable-action"),
        new(SanctuaryProceduralBasisPrimitiveKind.Preserve, "basis.preserve", "maintain-lineage-boundary-and-posture"),
        new(SanctuaryProceduralBasisPrimitiveKind.Condense, "basis.condense", "canonicalize-stable-grounded-structure")
    ];

    public static IReadOnlyList<SanctuaryProceduralCompositionPatternDefinition> CompositionPatterns { get; } =
    [
        new(SanctuaryProceduralCompositionPatternKind.Admission, "basis.pattern.admission", "verify->retain"),
        new(SanctuaryProceduralCompositionPatternKind.Receipt, "basis.pattern.receipt", "verify->mint"),
        new(SanctuaryProceduralCompositionPatternKind.Relation, "basis.pattern.relation", "verify->relate->retain"),
        new(SanctuaryProceduralCompositionPatternKind.Prune, "basis.pattern.prune", "verify->refuse"),
        new(SanctuaryProceduralCompositionPatternKind.Condense, "basis.pattern.condense", "verify->condense->mint")
    ];

    public static SanctuaryProceduralBasisDefinition ProceduralBasis { get; } =
        new(
            handle: "procedural-basis.condensate.v0",
            primitives:
            [
                SanctuaryProceduralBasisPrimitiveKind.Retain,
                SanctuaryProceduralBasisPrimitiveKind.Relate,
                SanctuaryProceduralBasisPrimitiveKind.Verify,
                SanctuaryProceduralBasisPrimitiveKind.Mint,
                SanctuaryProceduralBasisPrimitiveKind.Refuse,
                SanctuaryProceduralBasisPrimitiveKind.Preserve,
                SanctuaryProceduralBasisPrimitiveKind.Condense
            ],
            compositionPatterns:
            [
                SanctuaryProceduralCompositionPatternKind.Admission,
                SanctuaryProceduralCompositionPatternKind.Receipt,
                SanctuaryProceduralCompositionPatternKind.Relation,
                SanctuaryProceduralCompositionPatternKind.Prune,
                SanctuaryProceduralCompositionPatternKind.Condense
            ],
            excludedFreePrimitive: "execute",
            gatingRule: "all-primitives-are-posture-gated-and-grounded",
            executionBoundary: "no-action-outside-this-set-may-be-retained-or-executed",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryProceduralBasisPrimeFaceDefinition PrimeFace { get; } =
        new(
            handle: "procedural-basis.prime-face.v0",
            id: "p_c",
            layer: "P",
            role: "proc-basis",
            postureGate: "Sigma_prime",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryProceduralBasisCrypticFaceDefinition CrypticFace { get; } =
        new(
            handle: "procedural-basis.cryptic-face.v0",
            slots:
            [
                "anchor-keys",
                "dependency-keys",
                "trace-keys",
                "basis-seal"
            ],
            integrityRule: "cryptic-face-preserves-basis-lineage-and-trace",
            operationalStatus: "placeholder-contract-only");

    public static SanctuaryProceduralBasisReceiptDefinition Receipt { get; } =
        new(
            handle: "procedural-basis.receipt.v0",
            receiptFields:
            [
                "event-id",
                "source-set",
                "stability",
                "phi",
                "resolution",
                "result=condensed-basis"
            ],
            result: "condensed-basis",
            operationalStatus: "placeholder-contract-only");
}
