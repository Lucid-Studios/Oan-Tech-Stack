namespace San.Common;

public enum BootClass
{
    PersonalSolitary = 0,
    CorporateGoverned = 1
}

public enum BootActivationState
{
    Classified = 0,
    GovernanceForming = 1,
    TriadicActive = 2,
    BondedConfirmed = 3,
    ExpansionEligible = 4
}

public enum ExpansionRights
{
    None = 0,
    InternalGovernedOnly = 1
}

public enum SwarmEligibility
{
    Denied = 0,
    AllowedAfterBondedConfirmation = 1
}

public enum ProtectedIntakeKind
{
    HumanProtectedIntake = 0,
    CorporateProtectedIntake = 1
}

public enum PrimeRevealMode
{
    None = 0,
    MaskedSummary = 1,
    StructuralValidation = 2,
    AuthorizedFieldReveal = 3
}

public enum InternalGoverningCmeOffice
{
    Steward = 0,
    Father = 1,
    Mother = 2
}

public enum FirstBootGovernanceDecision
{
    Allow = 0,
    Quarantine = 1,
    Reject = 2
}

public enum GoverningOfficeVisibilityScope
{
    CustodyClass = 0,
    ProvenanceClass = 1,
    RevealEligibilityState = 2,
    StructuralIdentityRelations = 3,
    GovernanceObligations = 4,
    ContinuityCareObligations = 5,
    PrimeRevealPosture = 6
}

public enum FirstBootGovernanceLayerState
{
    Preformalized = 0,
    RoleBoundEceReady = 1
}

public enum RoleBoundEceState
{
    NotProvisioned = 0,
    Preformalized = 1,
    RoleBoundTestingReady = 2
}

public sealed record MaskedCrypticView(
    string ProtectedHandle,
    ProtectedIntakeKind IntakeKind,
    PrimeRevealMode DefaultRevealMode,
    IReadOnlyDictionary<string, string> StructuralLabels);

public sealed record BondedAuthorityContext(
    string AuthorityId,
    string AuthorityClass,
    bool BondedConfirmed,
    IReadOnlyList<string> ApprovedRevealPurposes);

public sealed record InternalGovernanceBootProfile(
    FirstBootGovernanceDecision Decision,
    string ReasonCode,
    BootClass BootClass,
    BootActivationState ActivationState,
    ExpansionRights ExpansionRights,
    SwarmEligibility SwarmEligibility,
    int RequestedExpansionCount,
    bool TriadicCrossWitnessComplete,
    bool BondedConfirmationComplete);

public sealed record ProtectedIntakeClassificationResult(
    FirstBootGovernanceDecision Decision,
    ProtectedIntakeKind IntakeKind,
    MaskedCrypticView MaskedView,
    PrimeRevealMode EffectiveRevealMode,
    bool RawFieldExposureAllowed,
    bool RequiresBondedAuthority,
    string ReasonCode);

public sealed record InternalGoverningCmeDefinition(
    InternalGoverningCmeOffice Office,
    int FormationOrdinal,
    string DomainSummary,
    IReadOnlyList<GoverningOfficeVisibilityScope> VisibilityScopes,
    bool CanWidenPrimeReveal,
    bool CanAuthorizeExpansion);

public sealed record InternalGoverningCmeFormationRequest(
    BootClass BootClass,
    BootActivationState ActivationState,
    InternalGoverningCmeOffice Office,
    IReadOnlyList<InternalGoverningCmeOffice> AlreadyFormedOffices,
    bool TriadicCrossWitnessComplete,
    bool BondedConfirmationComplete);

public sealed record InternalGoverningCmeFormationRecord(
    InternalGoverningCmeOffice Office,
    FirstBootGovernanceDecision Decision,
    string ReasonCode,
    BootActivationState ActivationState,
    bool FormationEligible);

public sealed record InternalGoverningCmeWitnessReceipt(
    FirstBootGovernanceDecision Decision,
    IReadOnlyList<InternalGoverningCmeOffice> WitnessedOffices,
    bool TriadicActive,
    bool BondedConfirmationRequired,
    string ReasonCode);

public sealed record FirstBootRoleBoundEceReceipt(
    string EceHandle,
    InternalGoverningCmeOffice Office,
    int FormationOrdinal,
    RoleBoundEceState State,
    string RoleBoundaryHandle,
    IReadOnlyList<GoverningOfficeVisibilityScope> VisibilityScopes,
    IReadOnlyList<InternalGoverningCmeOffice> RequiredPriorOffices,
    bool WitnessOnly,
    bool PrimeRevealWideningAllowed,
    bool ExpansionAuthorizationAllowed,
    string ReasonCode);

public sealed record FirstBootGovernanceLayerReceipt(
    string LayerHandle,
    BootClass BootClass,
    BootActivationState ActivationState,
    FirstBootGovernanceLayerState State,
    ExpansionRights ExpansionRights,
    SwarmEligibility SwarmEligibility,
    bool WitnessOnly,
    bool SubordinateCmeAuthorizationAllowed,
    bool RoleBoundEcesReady,
    IReadOnlyList<InternalGoverningCmeOffice> FormedOffices,
    IReadOnlyList<FirstBootRoleBoundEceReceipt> RoleBoundEces,
    string ReasonCode);

public interface IFirstBootGovernancePolicy
{
    InternalGovernanceBootProfile EvaluateBootProfile(
        BootClass bootClass,
        BootActivationState activationState,
        int requestedExpansionCount,
        bool triadicCrossWitnessComplete = false,
        bool bondedConfirmationComplete = false);

    ProtectedIntakeClassificationResult ClassifyProtectedIntake(
        ProtectedIntakeKind intakeKind,
        string protectedHandle,
        PrimeRevealMode requestedRevealMode,
        BondedAuthorityContext? bondedAuthorityContext = null);

    InternalGoverningCmeDefinition GetOfficeDefinition(InternalGoverningCmeOffice office);

    InternalGoverningCmeFormationRecord EvaluateFormationEligibility(
        InternalGoverningCmeFormationRequest request);

    FirstBootGovernanceLayerReceipt ProjectGovernanceLayer(
        BootClass bootClass,
        BootActivationState activationState,
        int requestedExpansionCount,
        IReadOnlyList<InternalGoverningCmeOffice> formedOffices,
        bool triadicCrossWitnessComplete = false,
        bool bondedConfirmationComplete = false);
}

public sealed class DefaultFirstBootGovernancePolicy : IFirstBootGovernancePolicy
{
    public InternalGovernanceBootProfile EvaluateBootProfile(
        BootClass bootClass,
        BootActivationState activationState,
        int requestedExpansionCount,
        bool triadicCrossWitnessComplete = false,
        bool bondedConfirmationComplete = false)
    {
        if (requestedExpansionCount > 1 && bootClass == BootClass.PersonalSolitary)
        {
            return new InternalGovernanceBootProfile(
                FirstBootGovernanceDecision.Quarantine,
                "personal-solitary-swarm-denied",
                bootClass,
                activationState,
                ExpansionRights.None,
                SwarmEligibility.Denied,
                requestedExpansionCount,
                triadicCrossWitnessComplete,
                bondedConfirmationComplete);
        }

        if (activationState == BootActivationState.ExpansionEligible &&
            (!triadicCrossWitnessComplete || !bondedConfirmationComplete))
        {
            return new InternalGovernanceBootProfile(
                FirstBootGovernanceDecision.Reject,
                "expansion-activation-prerequisites-missing",
                bootClass,
                activationState,
                ExpansionRights.None,
                SwarmEligibility.Denied,
                requestedExpansionCount,
                triadicCrossWitnessComplete,
                bondedConfirmationComplete);
        }

        var expansionRights = ExpansionRights.None;
        var swarmEligibility = SwarmEligibility.Denied;

        if (bootClass == BootClass.CorporateGoverned &&
            activationState == BootActivationState.ExpansionEligible &&
            triadicCrossWitnessComplete &&
            bondedConfirmationComplete)
        {
            expansionRights = ExpansionRights.InternalGovernedOnly;
            swarmEligibility = SwarmEligibility.AllowedAfterBondedConfirmation;
        }

        return new InternalGovernanceBootProfile(
            FirstBootGovernanceDecision.Allow,
            bootClass == BootClass.PersonalSolitary
                ? "personal-solitary-single-operator"
                : "corporate-governed-classified",
            bootClass,
            activationState,
            expansionRights,
            swarmEligibility,
            requestedExpansionCount,
            triadicCrossWitnessComplete,
            bondedConfirmationComplete);
    }

    public ProtectedIntakeClassificationResult ClassifyProtectedIntake(
        ProtectedIntakeKind intakeKind,
        string protectedHandle,
        PrimeRevealMode requestedRevealMode,
        BondedAuthorityContext? bondedAuthorityContext = null)
    {
        var maskedView = new MaskedCrypticView(
            protectedHandle,
            intakeKind,
            PrimeRevealMode.None,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["handle_class"] = intakeKind.ToString(),
                ["masking_state"] = "cryptic-default"
            });

        if (requestedRevealMode == PrimeRevealMode.AuthorizedFieldReveal)
        {
            if (bondedAuthorityContext is null ||
                !bondedAuthorityContext.BondedConfirmed ||
                bondedAuthorityContext.ApprovedRevealPurposes.Count == 0)
            {
                return new ProtectedIntakeClassificationResult(
                    FirstBootGovernanceDecision.Quarantine,
                    intakeKind,
                    maskedView,
                    PrimeRevealMode.None,
                    RawFieldExposureAllowed: false,
                    RequiresBondedAuthority: true,
                    "authorized-field-reveal-requires-bonded-authority");
            }

            return new ProtectedIntakeClassificationResult(
                FirstBootGovernanceDecision.Allow,
                intakeKind,
                maskedView,
                PrimeRevealMode.AuthorizedFieldReveal,
                RawFieldExposureAllowed: true,
                RequiresBondedAuthority: true,
                "authorized-field-reveal-approved");
        }

        return new ProtectedIntakeClassificationResult(
            FirstBootGovernanceDecision.Allow,
            intakeKind,
            maskedView,
            requestedRevealMode,
            RawFieldExposureAllowed: false,
            RequiresBondedAuthority: false,
            requestedRevealMode switch
            {
                PrimeRevealMode.None => "cryptic-masked-default",
                PrimeRevealMode.MaskedSummary => "prime-masked-summary-allowed",
                PrimeRevealMode.StructuralValidation => "prime-structural-validation-allowed",
                _ => "cryptic-masked-default"
            });
    }

    public InternalGoverningCmeDefinition GetOfficeDefinition(InternalGoverningCmeOffice office)
    {
        return office switch
        {
            InternalGoverningCmeOffice.Steward => new InternalGoverningCmeDefinition(
                office,
                FormationOrdinal: 1,
                DomainSummary: "runtime continuity, review, and supervision",
                VisibilityScopes:
                [
                    GoverningOfficeVisibilityScope.CustodyClass,
                    GoverningOfficeVisibilityScope.ProvenanceClass,
                    GoverningOfficeVisibilityScope.RevealEligibilityState
                ],
                CanWidenPrimeReveal: false,
                CanAuthorizeExpansion: false),

            InternalGoverningCmeOffice.Father => new InternalGoverningCmeDefinition(
                office,
                FormationOrdinal: 2,
                DomainSummary: "cryptic-side protected formation and governance obligations",
                VisibilityScopes:
                [
                    GoverningOfficeVisibilityScope.StructuralIdentityRelations,
                    GoverningOfficeVisibilityScope.GovernanceObligations
                ],
                CanWidenPrimeReveal: false,
                CanAuthorizeExpansion: false),

            InternalGoverningCmeOffice.Mother => new InternalGoverningCmeDefinition(
                office,
                FormationOrdinal: 3,
                DomainSummary: "prime-side reveal posture and continuity/care obligations",
                VisibilityScopes:
                [
                    GoverningOfficeVisibilityScope.ContinuityCareObligations,
                    GoverningOfficeVisibilityScope.PrimeRevealPosture
                ],
                CanWidenPrimeReveal: false,
                CanAuthorizeExpansion: false),

            _ => throw new ArgumentOutOfRangeException(nameof(office), office, null)
        };
    }

    public InternalGoverningCmeFormationRecord EvaluateFormationEligibility(
        InternalGoverningCmeFormationRequest request)
    {
        if (request.ActivationState < BootActivationState.GovernanceForming)
        {
            return new InternalGoverningCmeFormationRecord(
                request.Office,
                FirstBootGovernanceDecision.Reject,
                "governance-forming-required",
                request.ActivationState,
                FormationEligible: false);
        }

        if (request.AlreadyFormedOffices.Contains(request.Office))
        {
            return new InternalGoverningCmeFormationRecord(
                request.Office,
                FirstBootGovernanceDecision.Reject,
                "office-already-formed",
                request.ActivationState,
                FormationEligible: false);
        }

        var requiredOffice = request.Office switch
        {
            InternalGoverningCmeOffice.Steward => Array.Empty<InternalGoverningCmeOffice>(),
            InternalGoverningCmeOffice.Father => [InternalGoverningCmeOffice.Steward],
            InternalGoverningCmeOffice.Mother => [InternalGoverningCmeOffice.Steward, InternalGoverningCmeOffice.Father],
            _ => throw new ArgumentOutOfRangeException()
        };

        if (requiredOffice.Except(request.AlreadyFormedOffices).Any())
        {
            return new InternalGoverningCmeFormationRecord(
                request.Office,
                FirstBootGovernanceDecision.Reject,
                "governing-office-order-violation",
                request.ActivationState,
                FormationEligible: false);
        }

        return new InternalGoverningCmeFormationRecord(
            request.Office,
            FirstBootGovernanceDecision.Allow,
            "governing-office-formation-allowed",
            request.ActivationState,
            FormationEligible: true);
    }

    public FirstBootGovernanceLayerReceipt ProjectGovernanceLayer(
        BootClass bootClass,
        BootActivationState activationState,
        int requestedExpansionCount,
        IReadOnlyList<InternalGoverningCmeOffice> formedOffices,
        bool triadicCrossWitnessComplete = false,
        bool bondedConfirmationComplete = false)
    {
        ArgumentNullException.ThrowIfNull(formedOffices);

        var bootProfile = EvaluateBootProfile(
            bootClass,
            activationState,
            requestedExpansionCount,
            triadicCrossWitnessComplete,
            bondedConfirmationComplete);
        var formedOfficeSet = formedOffices
            .Distinct()
            .ToHashSet();
        var orderedFormedOffices = OrderedOffices
            .Where(formedOfficeSet.Contains)
            .ToArray();
        var roleBoundEces = OrderedOffices
            .Select(office =>
            {
                var definition = GetOfficeDefinition(office);
                var requiredPriorOffices = GetRequiredPriorOffices(office);
                var isFormed = formedOfficeSet.Contains(office);
                var state = !isFormed
                    ? RoleBoundEceState.NotProvisioned
                    : triadicCrossWitnessComplete
                        ? RoleBoundEceState.RoleBoundTestingReady
                        : RoleBoundEceState.Preformalized;
                var reasonCode = state switch
                {
                    RoleBoundEceState.NotProvisioned => "role-bound-ece-not-provisioned",
                    RoleBoundEceState.RoleBoundTestingReady => "role-bound-ece-testing-ready",
                    _ => "role-bound-ece-preformalized"
                };

                return new FirstBootRoleBoundEceReceipt(
                    EceHandle: CreateRoleBoundEceHandle(bootClass, office),
                    Office: office,
                    FormationOrdinal: definition.FormationOrdinal,
                    State: state,
                    RoleBoundaryHandle: CreateRoleBoundaryHandle(office),
                    VisibilityScopes: definition.VisibilityScopes,
                    RequiredPriorOffices: requiredPriorOffices,
                    WitnessOnly: true,
                    PrimeRevealWideningAllowed: false,
                    ExpansionAuthorizationAllowed: false,
                    ReasonCode: reasonCode);
            })
            .ToArray();

        var roleBoundEcesReady = triadicCrossWitnessComplete &&
                                     OrderedOffices.All(formedOfficeSet.Contains) &&
                                     roleBoundEces.All(ece => ece.State == RoleBoundEceState.RoleBoundTestingReady);
        var layerState = roleBoundEcesReady
            ? FirstBootGovernanceLayerState.RoleBoundEceReady
            : FirstBootGovernanceLayerState.Preformalized;
        var subordinateCmeAuthorizationAllowed =
            activationState == BootActivationState.ExpansionEligible &&
            bootProfile.ExpansionRights == ExpansionRights.InternalGovernedOnly &&
            triadicCrossWitnessComplete &&
            bondedConfirmationComplete;
        var reasonCode = layerState == FirstBootGovernanceLayerState.RoleBoundEceReady
            ? "first-boot-governance-layer-role-bound-ece-ready"
            : "first-boot-governance-layer-preformalized";

        return new FirstBootGovernanceLayerReceipt(
            LayerHandle: CreateGovernanceLayerHandle(bootClass, activationState),
            BootClass: bootClass,
            ActivationState: activationState,
            State: layerState,
            ExpansionRights: bootProfile.ExpansionRights,
            SwarmEligibility: bootProfile.SwarmEligibility,
            WitnessOnly: true,
            SubordinateCmeAuthorizationAllowed: subordinateCmeAuthorizationAllowed,
            RoleBoundEcesReady: roleBoundEcesReady,
            FormedOffices: orderedFormedOffices,
            RoleBoundEces: roleBoundEces,
            ReasonCode: reasonCode);
    }

    private static IReadOnlyList<InternalGoverningCmeOffice> GetRequiredPriorOffices(InternalGoverningCmeOffice office)
    {
        return office switch
        {
            InternalGoverningCmeOffice.Steward => Array.Empty<InternalGoverningCmeOffice>(),
            InternalGoverningCmeOffice.Father => [InternalGoverningCmeOffice.Steward],
            InternalGoverningCmeOffice.Mother => [InternalGoverningCmeOffice.Steward, InternalGoverningCmeOffice.Father],
            _ => throw new ArgumentOutOfRangeException(nameof(office), office, null)
        };
    }

    private static string CreateGovernanceLayerHandle(BootClass bootClass, BootActivationState activationState)
    {
        return $"first-boot-governance://{bootClass.ToString().ToLowerInvariant()}/{activationState.ToString().ToLowerInvariant()}";
    }

    private static string CreateRoleBoundEceHandle(BootClass bootClass, InternalGoverningCmeOffice office)
    {
        return $"first-boot-governance://{bootClass.ToString().ToLowerInvariant()}/role-bound-ece/{office.ToString().ToLowerInvariant()}";
    }

    private static string CreateRoleBoundaryHandle(InternalGoverningCmeOffice office)
    {
        return $"first-boot-governance://boundary/{office.ToString().ToLowerInvariant()}";
    }

    private static readonly IReadOnlyList<InternalGoverningCmeOffice> OrderedOffices =
    [
        InternalGoverningCmeOffice.Steward,
        InternalGoverningCmeOffice.Father,
        InternalGoverningCmeOffice.Mother
    ];
}
