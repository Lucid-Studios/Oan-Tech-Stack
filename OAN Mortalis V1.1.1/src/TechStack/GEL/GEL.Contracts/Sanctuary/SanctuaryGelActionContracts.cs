namespace GEL.Contracts.Sanctuary;

public enum SanctuaryGelActionKind
{
    Attend = 0,
    Reduce = 1,
    Decompose = 2,
    Discriminate = 3,
    Anchor = 4,
    Relate = 5,
    Retain = 6,
    Refuse = 7,
    Recover = 8
}

public enum SanctuaryGelCompositionOperatorKind
{
    Sequential = 0,
    Guarded = 1,
    Choice = 2,
    Iteration = 3,
    RecoveryWrapper = 4,
    RefusalTerminal = 5
}

public sealed record SanctuaryGelActionDefinition
{
    public SanctuaryGelActionDefinition(
        SanctuaryGelActionKind action,
        string handle,
        string requirement,
        IReadOnlyList<string> governingHandleReferences,
        IReadOnlyList<string> successConditions,
        IReadOnlyList<string> failureDispositions,
        string operationalStatus)
    {
        Action = action;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Requirement = SanctuaryContractGuard.RequiredText(requirement, nameof(requirement));
        GoverningHandleReferences = SanctuaryContractGuard.RequiredTextList(governingHandleReferences, nameof(governingHandleReferences));
        SuccessConditions = SanctuaryContractGuard.RequiredTextList(successConditions, nameof(successConditions));
        FailureDispositions = SanctuaryContractGuard.RequiredTextList(failureDispositions, nameof(failureDispositions));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryGelActionKind Action { get; }

    public string Handle { get; }

    public string Requirement { get; }

    public IReadOnlyList<string> GoverningHandleReferences { get; }

    public IReadOnlyList<string> SuccessConditions { get; }

    public IReadOnlyList<string> FailureDispositions { get; }

    public string OperationalStatus { get; }
}

public sealed record SanctuaryGelCompositionOperatorDefinition
{
    public SanctuaryGelCompositionOperatorDefinition(
        SanctuaryGelCompositionOperatorKind @operator,
        string handle,
        string requirement,
        IReadOnlyList<string> guardClasses,
        string terminationCondition,
        string operationalStatus)
    {
        Operator = @operator;
        Handle = SanctuaryContractGuard.RequiredText(handle, nameof(handle));
        Requirement = SanctuaryContractGuard.RequiredText(requirement, nameof(requirement));
        GuardClasses = SanctuaryContractGuard.RequiredTextList(guardClasses, nameof(guardClasses));
        TerminationCondition = SanctuaryContractGuard.RequiredText(terminationCondition, nameof(terminationCondition));
        OperationalStatus = SanctuaryContractGuard.RequiredText(operationalStatus, nameof(operationalStatus));
    }

    public SanctuaryGelCompositionOperatorKind Operator { get; }

    public string Handle { get; }

    public string Requirement { get; }

    public IReadOnlyList<string> GuardClasses { get; }

    public string TerminationCondition { get; }

    public string OperationalStatus { get; }
}

public static class SanctuaryGelActionAtlas
{
    private static readonly IReadOnlyDictionary<SanctuaryGelActionKind, SanctuaryGelActionDefinition> ActionDefinitions =
        new Dictionary<SanctuaryGelActionKind, SanctuaryGelActionDefinition>
        {
            [SanctuaryGelActionKind.Attend] = new(
                action: SanctuaryGelActionKind.Attend,
                handle: "act.attend.v0",
                requirement: "isolate-candidate-without-altering-it",
                governingHandleReferences:
                [
                    "a0.layer-integrity",
                    "a0.posture-stability"
                ],
                successConditions:
                [
                    "candidate-isolated",
                    "source-unaltered"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "recover"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Reduce] = new(
                action: SanctuaryGelActionKind.Reduce,
                handle: "act.reduce.v0",
                requirement: "strip-definitional-contextual-and-procedural-inflation",
                governingHandleReferences:
                [
                    "a0.non-inflation",
                    "a0.discernment-admissibility"
                ],
                successConditions:
                [
                    "inflation-load-reduced",
                    "candidate-ready-for-discrimination"
                ],
                failureDispositions:
                [
                    "decompose",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Decompose] = new(
                action: SanctuaryGelActionKind.Decompose,
                handle: "act.decompose.v0",
                requirement: "split-mixed-candidate-into-more-discernible-parts",
                governingHandleReferences:
                [
                    "a0.discernment-admissibility",
                    "l3.pruning"
                ],
                successConditions:
                [
                    "children-more-discernible-than-parent",
                    "mixed-load-separated"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Discriminate] = new(
                action: SanctuaryGelActionKind.Discriminate,
                handle: "act.discriminate.v0",
                requirement: "determine-what-kind-of-knowing-this-is",
                governingHandleReferences:
                [
                    "a0.layer-integrity",
                    "a0.discernment-admissibility"
                ],
                successConditions:
                [
                    "unique-layer-determined",
                    "routing-made-lawful"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "decompose",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Anchor] = new(
                action: SanctuaryGelActionKind.Anchor,
                handle: "act.anchor.v0",
                requirement: "bind-root-candidate-to-existing-or-new-anchor",
                governingHandleReferences:
                [
                    "a0.root-identity-invariance",
                    "prime.root-carrier.utf8",
                    "anchor-emergence.v0"
                ],
                successConditions:
                [
                    "anchor-bound-or-created",
                    "root-realized"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Relate] = new(
                action: SanctuaryGelActionKind.Relate,
                handle: "act.relate.v0",
                requirement: "form-lawful-relation-without-identity-collapse",
                governingHandleReferences:
                [
                    "l4.grove-relation",
                    "l5.anchor-merge-refusal"
                ],
                successConditions:
                [
                    "distinct-anchors-preserved",
                    "relation-admitted"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Retain] = new(
                action: SanctuaryGelActionKind.Retain,
                handle: "act.retain.v0",
                requirement: "admit-lawful-object-into-gel",
                governingHandleReferences:
                [
                    "a0.discernment-admissibility",
                    "a0.downward-grounding"
                ],
                successConditions:
                [
                    "object-admitted",
                    "gel-lineage-preserved"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Refuse] = new(
                action: SanctuaryGelActionKind.Refuse,
                handle: "act.refuse.v0",
                requirement: "deny-admission-or-continuation-when-law-fails",
                governingHandleReferences:
                [
                    "a0.discernment-admissibility",
                    "l3.pruning"
                ],
                successConditions:
                [
                    "unlawful-object-not-retained",
                    "trust-surface-protected"
                ],
                failureDispositions:
                [
                    "recover"
                ],
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelActionKind.Recover] = new(
                action: SanctuaryGelActionKind.Recover,
                handle: "act.recover.v0",
                requirement: "return-acting-system-to-prime-posture-after-drift",
                governingHandleReferences:
                [
                    "a0.posture-stability"
                ],
                successConditions:
                [
                    "prime-posture-restored",
                    "forced-action-cleared"
                ],
                failureDispositions:
                [
                    "hold-provisional",
                    "refuse"
                ],
                operationalStatus: "placeholder-contract-only")
        };

    private static readonly IReadOnlyDictionary<SanctuaryGelCompositionOperatorKind, SanctuaryGelCompositionOperatorDefinition> OperatorDefinitions =
        new Dictionary<SanctuaryGelCompositionOperatorKind, SanctuaryGelCompositionOperatorDefinition>
        {
            [SanctuaryGelCompositionOperatorKind.Sequential] = new(
                @operator: SanctuaryGelCompositionOperatorKind.Sequential,
                handle: "comp.sequential.v0",
                requirement: "perform-second-action-only-after-first-action-succeeds",
                guardClasses:
                [
                    "success-required"
                ],
                terminationCondition: "halts-on-first-failure",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelCompositionOperatorKind.Guarded] = new(
                @operator: SanctuaryGelCompositionOperatorKind.Guarded,
                handle: "comp.guarded.v0",
                requirement: "perform-next-action-only-when-guard-holds",
                guardClasses:
                [
                    "layer-clean",
                    "prime-valid",
                    "posture-stable"
                ],
                terminationCondition: "halts-when-guard-fails",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelCompositionOperatorKind.Choice] = new(
                @operator: SanctuaryGelCompositionOperatorKind.Choice,
                handle: "comp.choice.v0",
                requirement: "select-exactly-one-lawful-branch",
                guardClasses:
                [
                    "discrimination-result"
                ],
                terminationCondition: "terminates-when-one-branch-is-selected",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelCompositionOperatorKind.Iteration] = new(
                @operator: SanctuaryGelCompositionOperatorKind.Iteration,
                handle: "comp.iteration.v0",
                requirement: "repeat-until-fixed-point-or-routable-state",
                guardClasses:
                [
                    "change-detected",
                    "fixed-point-not-yet-reached"
                ],
                terminationCondition: "stops-at-fixed-point-or-terminal-routing",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelCompositionOperatorKind.RecoveryWrapper] = new(
                @operator: SanctuaryGelCompositionOperatorKind.RecoveryWrapper,
                handle: "comp.recovery-wrapper.v0",
                requirement: "wrap-flow-so-posture-returns-to-prime",
                guardClasses:
                [
                    "drift-detected"
                ],
                terminationCondition: "returns-when-prime-posture-restored",
                operationalStatus: "placeholder-contract-only"),
            [SanctuaryGelCompositionOperatorKind.RefusalTerminal] = new(
                @operator: SanctuaryGelCompositionOperatorKind.RefusalTerminal,
                handle: "comp.refusal-terminal.v0",
                requirement: "terminate-flow-with-no-further-composition",
                guardClasses:
                [
                    "lawful-refusal"
                ],
                terminationCondition: "no-further-actions-allowed",
                operationalStatus: "placeholder-contract-only")
        };

    public static IReadOnlyList<SanctuaryGelActionDefinition> Actions { get; } =
        ActionDefinitions.Values
            .OrderBy(static item => item.Action)
            .ToArray();

    public static IReadOnlyList<SanctuaryGelCompositionOperatorDefinition> CompositionOperators { get; } =
        OperatorDefinitions.Values
            .OrderBy(static item => item.Operator)
            .ToArray();

    public static bool TryGetAction(
        SanctuaryGelActionKind action,
        out SanctuaryGelActionDefinition definition)
    {
        return ActionDefinitions.TryGetValue(action, out definition!);
    }

    public static SanctuaryGelActionDefinition GetAction(SanctuaryGelActionKind action)
    {
        if (!TryGetAction(action, out var definition))
        {
            throw new KeyNotFoundException($"No GEL action definition exists for '{action}'.");
        }

        return definition;
    }

    public static bool TryGetCompositionOperator(
        SanctuaryGelCompositionOperatorKind @operator,
        out SanctuaryGelCompositionOperatorDefinition definition)
    {
        return OperatorDefinitions.TryGetValue(@operator, out definition!);
    }

    public static SanctuaryGelCompositionOperatorDefinition GetCompositionOperator(SanctuaryGelCompositionOperatorKind @operator)
    {
        if (!TryGetCompositionOperator(@operator, out var definition))
        {
            throw new KeyNotFoundException($"No GEL composition operator definition exists for '{@operator}'.");
        }

        return definition;
    }
}
