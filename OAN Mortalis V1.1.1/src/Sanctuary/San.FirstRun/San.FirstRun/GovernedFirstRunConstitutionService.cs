using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.FirstRun;

public sealed class GovernedFirstRunConstitutionService : IFirstRunConstitutionService
{
    private static readonly IReadOnlyList<FirstRunConstitutionState> OrderedStates =
    [
        FirstRunConstitutionState.SanctuaryInitialized,
        FirstRunConstitutionState.LocationBound,
        FirstRunConstitutionState.ParentStanding,
        FirstRunConstitutionState.CradleTekInstalled,
        FirstRunConstitutionState.CradleTekAdmitted,
        FirstRunConstitutionState.StewardStanding,
        FirstRunConstitutionState.FoundationsEstablished,
        FirstRunConstitutionState.BondProcessOpen,
        FirstRunConstitutionState.OpalActualized
    ];

    public FirstRunConstitutionReceipt Project(FirstRunConstitutionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.SnapshotHandle);

        var currentState = DetermineCurrentState(snapshot);
        var activeFailures = DetermineActiveFailures(snapshot);
        var transitions = OrderedStates
            .Where(state => state != currentState)
            .Select(state => CreateTransitionReceipt(snapshot, currentState, state, activeFailures))
            .ToArray();
        var readinessState = DetermineReadinessState(snapshot, currentState);
        var currentStateSatisfied = IsStateSatisfied(snapshot, currentState);
        var currentStateProvisional = currentStateSatisfied && IsProvisionalState(currentState);
        var opalActualized =
            currentState == FirstRunConstitutionState.OpalActualized &&
            IsStateSatisfied(snapshot, FirstRunConstitutionState.OpalActualized);

        return new FirstRunConstitutionReceipt(
            ReceiptHandle: CreateHandle("first-run-receipt://", snapshot.SnapshotHandle, currentState.ToString(), readinessState.ToString()),
            SnapshotHandle: snapshot.SnapshotHandle,
            LocalAuthorityTraceHandle: snapshot.LocalAuthorityTraceHandle,
            ConstitutionalContactHandle: snapshot.ConstitutionalContactHandle,
            LocalKeypairGenesisSourceHandle: snapshot.LocalKeypairGenesisSourceHandle,
            LocalKeypairGenesisHandle: snapshot.LocalKeypairGenesisHandle,
            FirstCrypticBraidEstablishmentHandle: snapshot.FirstCrypticBraidEstablishmentHandle,
            FirstCrypticBraidHandle: snapshot.FirstCrypticBraidHandle,
            FirstCrypticConditioningSourceHandle: snapshot.FirstCrypticConditioningSourceHandle,
            FirstCrypticConditioningHandle: snapshot.FirstCrypticConditioningHandle,
            CurrentState: currentState,
            ReadinessState: readinessState,
            CurrentStateProvisional: currentStateProvisional,
            CurrentStateActualized: currentStateSatisfied && !currentStateProvisional,
            OpalActualized: opalActualized,
            ActiveFailureClasses: activeFailures,
            PromotionGates: transitions,
            SourceReason: $"first-run-current-state:{currentState}/pre-governance:{(HasPreGovernanceSequence(snapshot) ? "projected" : "incomplete")}/protocolization:{(snapshot.ProtocolizationPacket is null ? "withheld" : "projected")}/oe-uptake:{(snapshot.StewardWitnessedOePacket is null ? "withheld" : "projected")}/elemental-binding:{(snapshot.ElementalBindingPacket is null ? "withheld" : "projected")}/actualization-seal:{(snapshot.ActualizationSealPacket is null ? "withheld" : "projected")}/living-agenticore:{(snapshot.LivingAgentiCorePacket is null ? "withheld" : "projected")}",
            TimestampUtc: DateTimeOffset.UtcNow,
            ProtocolizationPacket: snapshot.ProtocolizationPacket,
            StewardWitnessedOePacket: snapshot.StewardWitnessedOePacket,
            ElementalBindingPacket: snapshot.ElementalBindingPacket,
            ActualizationSealPacket: snapshot.ActualizationSealPacket,
            LivingAgentiCorePacket: snapshot.LivingAgentiCorePacket);
    }

    private static FirstRunConstitutionState DetermineCurrentState(FirstRunConstitutionSnapshot snapshot)
    {
        var currentState = FirstRunConstitutionState.SanctuaryInitialized;
        foreach (var state in OrderedStates)
        {
            if (IsStateSatisfied(snapshot, state))
            {
                currentState = state;
            }
            else
            {
                break;
            }
        }

        return currentState;
    }

    private static FirstRunOperatorReadinessState DetermineReadinessState(
        FirstRunConstitutionSnapshot snapshot,
        FirstRunConstitutionState currentState)
    {
        if (currentState >= FirstRunConstitutionState.OpalActualized &&
            !string.IsNullOrWhiteSpace(snapshot.NoticeCertificationGateHandle))
        {
            return FirstRunOperatorReadinessState.BondedCoworkReady;
        }

        if (currentState >= FirstRunConstitutionState.FoundationsEstablished)
        {
            return FirstRunOperatorReadinessState.OperatorTrainingReady;
        }

        if (currentState >= FirstRunConstitutionState.ParentStanding)
        {
            return FirstRunOperatorReadinessState.OperatorContactReady;
        }

        return FirstRunOperatorReadinessState.NotReady;
    }

    private static IReadOnlyList<FirstRunFailureClass> DetermineActiveFailures(FirstRunConstitutionSnapshot snapshot)
    {
        var failures = new List<FirstRunFailureClass>();

        var hasHigherOrderStatePressure =
            !string.IsNullOrWhiteSpace(snapshot.MotherStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.FatherStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.CradleTekInstallHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.CradleTekAdmissionHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.StewardStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.GelStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.GoaStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.MosStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.ToolRightsHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.DataRightsHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.BondProcessHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.OpalActualizationHandle);

        var hasPreGovernancePressure =
            !string.IsNullOrWhiteSpace(snapshot.MotherStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.FatherStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.CradleTekInstallHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.CradleTekAdmissionHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.StewardStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.GelStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.GoaStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.MosStandingHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.ToolRightsHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.DataRightsHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.BondProcessHandle) ||
            !string.IsNullOrWhiteSpace(snapshot.OpalActualizationHandle);

        if (hasHigherOrderStatePressure && string.IsNullOrWhiteSpace(snapshot.LocationBindingHandle))
        {
            failures.Add(FirstRunFailureClass.LocationBindingFault);
        }

        if (hasPreGovernancePressure && !HasConstitutionalContact(snapshot))
        {
            failures.Add(FirstRunFailureClass.ConstitutionalContactIncomplete);
        }

        if (hasPreGovernancePressure && string.IsNullOrWhiteSpace(snapshot.LocalKeypairGenesisHandle))
        {
            failures.Add(FirstRunFailureClass.LocalKeypairGenesisMissing);
        }

        if (hasPreGovernancePressure && string.IsNullOrWhiteSpace(snapshot.FirstCrypticBraidHandle))
        {
            failures.Add(FirstRunFailureClass.FirstCrypticBraidMissing);
        }

        if (hasPreGovernancePressure && string.IsNullOrWhiteSpace(snapshot.FirstCrypticConditioningHandle))
        {
            failures.Add(FirstRunFailureClass.FirstCrypticConditioningIncomplete);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.CradleTekInstallHandle) &&
            string.IsNullOrWhiteSpace(snapshot.CradleTekAdmissionHandle))
        {
            failures.Add(FirstRunFailureClass.CradleTekInstalledButNotAdmitted);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.CradleTekAdmissionHandle) &&
            string.IsNullOrWhiteSpace(snapshot.StewardStandingHandle))
        {
            failures.Add(FirstRunFailureClass.AdmittedCradleTekWithoutSteward);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.StewardStandingHandle) &&
            (!HasFoundations(snapshot)))
        {
            failures.Add(FirstRunFailureClass.FoundationsIncomplete);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.BondProcessHandle) &&
            string.IsNullOrWhiteSpace(snapshot.OpalActualizationHandle))
        {
            failures.Add(FirstRunFailureClass.BondProcessOpenedWithoutStableOpal);
        }

        return failures.Count == 0
            ? []
            : failures;
    }

    private static FirstRunTransitionReceipt CreateTransitionReceipt(
        FirstRunConstitutionSnapshot snapshot,
        FirstRunConstitutionState currentState,
        FirstRunConstitutionState requestedState,
        IReadOnlyList<FirstRunFailureClass> activeFailures)
    {
        var requiredReceipts = GetRequiredReceiptLabels(requestedState);
        var blockingReasons = GetBlockingReasons(snapshot, requestedState);
        var decision = ResolveDecision(snapshot, requestedState, blockingReasons);
        var transitionFailures = ResolveTransitionFailures(requestedState, activeFailures);

        return new FirstRunTransitionReceipt(
            TransitionHandle: CreateHandle("first-run-transition://", snapshot.SnapshotHandle, currentState.ToString(), requestedState.ToString()),
            CurrentState: currentState,
            RequestedState: requestedState,
            Decision: decision,
            RequiredPriorReceipts: requiredReceipts,
            BlockingReasons: blockingReasons,
            FailureClasses: transitionFailures,
            RequestedStateProvisional: IsProvisionalState(requestedState),
            RequestedStateActualized: !IsProvisionalState(requestedState),
            Retryable: decision == FirstRunPromotionGateDecision.Block,
            ReviewRequired: decision == FirstRunPromotionGateDecision.ReviewRequired,
            TimestampUtc: DateTimeOffset.UtcNow,
            EquivalentExchangeReview: CreateEquivalentExchangeReview(
                snapshot,
                requestedState,
                decision,
                blockingReasons));
    }

    private static FirstRunEquivalentExchangeReview CreateEquivalentExchangeReview(
        FirstRunConstitutionSnapshot snapshot,
        FirstRunConstitutionState requestedState,
        FirstRunPromotionGateDecision decision,
        IReadOnlyList<string> blockingReasons)
    {
        var disposition = decision switch
        {
            FirstRunPromotionGateDecision.Allow => FirstRunEquivalentExchangeDisposition.Preserve,
            FirstRunPromotionGateDecision.ReviewRequired => FirstRunEquivalentExchangeDisposition.Review,
            _ => FirstRunEquivalentExchangeDisposition.Refuse
        };

        var admissible = decision is not FirstRunPromotionGateDecision.Block;
        var reviewNotes = blockingReasons.Count == 0
            ? ["equivalent-exchange-review-cleared"]
            : blockingReasons;

        return new FirstRunEquivalentExchangeReview(
            ReviewHandle: CreateHandle("first-run-equivalent-exchange://", snapshot.SnapshotHandle, requestedState.ToString(), disposition.ToString()),
            Admissible: admissible,
            BurdenFamilies: GetBurdenFamilies(requestedState),
            RecoveryPosture: disposition switch
            {
                FirstRunEquivalentExchangeDisposition.Preserve => "invariant-recovery-holds",
                FirstRunEquivalentExchangeDisposition.Review => "recovery-requires-review",
                FirstRunEquivalentExchangeDisposition.Cleave => "lawful-cleaving-required",
                _ => "admissibility-not-yet-satisfied"
            },
            Disposition: disposition,
            ReviewNotes: reviewNotes,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static FirstRunPromotionGateDecision ResolveDecision(
        FirstRunConstitutionSnapshot snapshot,
        FirstRunConstitutionState requestedState,
        IReadOnlyList<string> blockingReasons)
    {
        if (blockingReasons.Count == 0)
        {
            return FirstRunPromotionGateDecision.Allow;
        }

        if (requestedState == FirstRunConstitutionState.StewardStanding &&
            !string.IsNullOrWhiteSpace(snapshot.CradleTekAdmissionHandle))
        {
            return FirstRunPromotionGateDecision.ReviewRequired;
        }

        if (requestedState == FirstRunConstitutionState.OpalActualized &&
            !string.IsNullOrWhiteSpace(snapshot.BondProcessHandle))
        {
            return FirstRunPromotionGateDecision.ReviewRequired;
        }

        return FirstRunPromotionGateDecision.Block;
    }

    private static IReadOnlyList<FirstRunFailureClass> ResolveTransitionFailures(
        FirstRunConstitutionState requestedState,
        IReadOnlyList<FirstRunFailureClass> activeFailures)
    {
        var failures = new List<FirstRunFailureClass>();

        if (requestedState >= FirstRunConstitutionState.ParentStanding)
        {
            failures.AddRange(activeFailures.Where(failure => failure is
                FirstRunFailureClass.ConstitutionalContactIncomplete or
                FirstRunFailureClass.LocalKeypairGenesisMissing or
                FirstRunFailureClass.FirstCrypticBraidMissing or
                FirstRunFailureClass.FirstCrypticConditioningIncomplete));
        }

        if (requestedState == FirstRunConstitutionState.LocationBound &&
            activeFailures.Contains(FirstRunFailureClass.LocationBindingFault))
        {
            failures.Add(FirstRunFailureClass.LocationBindingFault);
        }

        if (requestedState == FirstRunConstitutionState.CradleTekAdmitted &&
            activeFailures.Contains(FirstRunFailureClass.CradleTekInstalledButNotAdmitted))
        {
            failures.Add(FirstRunFailureClass.CradleTekInstalledButNotAdmitted);
        }

        if (requestedState == FirstRunConstitutionState.StewardStanding &&
            activeFailures.Contains(FirstRunFailureClass.AdmittedCradleTekWithoutSteward))
        {
            failures.Add(FirstRunFailureClass.AdmittedCradleTekWithoutSteward);
        }

        if (requestedState == FirstRunConstitutionState.FoundationsEstablished &&
            activeFailures.Contains(FirstRunFailureClass.FoundationsIncomplete))
        {
            failures.Add(FirstRunFailureClass.FoundationsIncomplete);
        }

        if (requestedState == FirstRunConstitutionState.OpalActualized &&
            activeFailures.Contains(FirstRunFailureClass.BondProcessOpenedWithoutStableOpal))
        {
            failures.Add(FirstRunFailureClass.BondProcessOpenedWithoutStableOpal);
        }

        return failures.Count == 0
            ? []
            : failures.Distinct().ToArray();
    }

    private static IReadOnlyList<string> GetBlockingReasons(
        FirstRunConstitutionSnapshot snapshot,
        FirstRunConstitutionState requestedState)
    {
        var missing = GetRequiredReceiptLabels(requestedState)
            .Where(label => !HasReceipt(snapshot, label))
            .ToList();

        return requestedState switch
        {
            FirstRunConstitutionState.ParentStanding when missing.Count > 0 => missing.Append("mother-and-father-standing-required").ToArray(),
            FirstRunConstitutionState.StewardStanding when missing.Count > 0 => missing.Append("cradletek-admission-required-before-steward").ToArray(),
            FirstRunConstitutionState.OpalActualized when missing.Count > 0 => missing.Append("explicit-bond-process-open-required-before-opal-actualization").ToArray(),
            _ => missing
        };
    }

    private static bool IsStateSatisfied(FirstRunConstitutionSnapshot snapshot, FirstRunConstitutionState state)
    {
        return state switch
        {
            FirstRunConstitutionState.SanctuaryInitialized => !string.IsNullOrWhiteSpace(snapshot.SanctuaryInitializationHandle),
            FirstRunConstitutionState.LocationBound => HasReceipt(snapshot, "location-binding"),
            FirstRunConstitutionState.ParentStanding =>
                HasPreGovernanceSequence(snapshot) &&
                HasReceipt(snapshot, "mother-standing") &&
                HasReceipt(snapshot, "father-standing"),
            FirstRunConstitutionState.CradleTekInstalled => HasReceipt(snapshot, "cradletek-install"),
            FirstRunConstitutionState.CradleTekAdmitted => HasReceipt(snapshot, "cradletek-admission"),
            FirstRunConstitutionState.StewardStanding => HasReceipt(snapshot, "steward-standing"),
            FirstRunConstitutionState.FoundationsEstablished => HasFoundations(snapshot),
            FirstRunConstitutionState.BondProcessOpen => HasReceipt(snapshot, "bond-process-open"),
            FirstRunConstitutionState.OpalActualized => HasReceipt(snapshot, "opal-actualization"),
            _ => false
        };
    }

    private static bool HasFoundations(FirstRunConstitutionSnapshot snapshot) =>
        HasReceipt(snapshot, "gel-standing") &&
        HasReceipt(snapshot, "goa-standing") &&
        HasReceipt(snapshot, "mos-standing") &&
        HasReceipt(snapshot, "tool-rights") &&
        HasReceipt(snapshot, "data-rights");

    private static bool HasConstitutionalContact(FirstRunConstitutionSnapshot snapshot) =>
        HasReceipt(snapshot, "local-authority-trace") &&
        HasReceipt(snapshot, "constitutional-contact");

    private static bool HasPreGovernanceSequence(FirstRunConstitutionSnapshot snapshot) =>
        HasConstitutionalContact(snapshot) &&
        HasReceipt(snapshot, "local-keypair-genesis") &&
        HasReceipt(snapshot, "first-cryptic-braid") &&
        HasReceipt(snapshot, "first-cryptic-conditioning");

    private static IReadOnlyList<string> GetRequiredReceiptLabels(FirstRunConstitutionState requestedState) => requestedState switch
    {
        FirstRunConstitutionState.SanctuaryInitialized =>
        [
            "sanctuary-initialization"
        ],
        FirstRunConstitutionState.LocationBound =>
        [
            "sanctuary-initialization",
            "location-binding"
        ],
        FirstRunConstitutionState.ParentStanding =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing"
        ],
        FirstRunConstitutionState.CradleTekInstalled =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing",
            "cradletek-install"
        ],
        FirstRunConstitutionState.CradleTekAdmitted =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing",
            "cradletek-install",
            "cradletek-admission"
        ],
        FirstRunConstitutionState.StewardStanding =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing",
            "cradletek-install",
            "cradletek-admission",
            "steward-standing"
        ],
        FirstRunConstitutionState.FoundationsEstablished =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing",
            "cradletek-install",
            "cradletek-admission",
            "steward-standing",
            "gel-standing",
            "goa-standing",
            "mos-standing",
            "tool-rights",
            "data-rights"
        ],
        FirstRunConstitutionState.BondProcessOpen =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing",
            "cradletek-install",
            "cradletek-admission",
            "steward-standing",
            "gel-standing",
            "goa-standing",
            "mos-standing",
            "tool-rights",
            "data-rights",
            "bond-process-open"
        ],
        FirstRunConstitutionState.OpalActualized =>
        [
            "sanctuary-initialization",
            "location-binding",
            "local-authority-trace",
            "constitutional-contact",
            "local-keypair-genesis",
            "first-cryptic-braid",
            "first-cryptic-conditioning",
            "mother-standing",
            "father-standing",
            "cradletek-install",
            "cradletek-admission",
            "steward-standing",
            "gel-standing",
            "goa-standing",
            "mos-standing",
            "tool-rights",
            "data-rights",
            "bond-process-open",
            "opal-actualization"
        ],
        _ => []
    };

    private static IReadOnlyList<string> GetBurdenFamilies(FirstRunConstitutionState requestedState) => requestedState switch
    {
        FirstRunConstitutionState.SanctuaryInitialized =>
        [
            "constitutional"
        ],
        FirstRunConstitutionState.LocationBound =>
        [
            "constitutional",
            "body",
            "continuity"
        ],
        FirstRunConstitutionState.ParentStanding =>
        [
            "constitutional",
            "relational",
            "cognitive",
            "continuity"
        ],
        FirstRunConstitutionState.CradleTekInstalled =>
        [
            "constitutional",
            "body",
            "continuity"
        ],
        FirstRunConstitutionState.CradleTekAdmitted =>
        [
            "constitutional",
            "body",
            "relational",
            "continuity"
        ],
        FirstRunConstitutionState.StewardStanding =>
        [
            "constitutional",
            "body",
            "relational",
            "cognitive",
            "continuity"
        ],
        FirstRunConstitutionState.FoundationsEstablished =>
        [
            "constitutional",
            "body",
            "relational",
            "cognitive",
            "continuity"
        ],
        FirstRunConstitutionState.BondProcessOpen =>
        [
            "constitutional",
            "relational",
            "continuity"
        ],
        FirstRunConstitutionState.OpalActualized =>
        [
            "constitutional",
            "body",
            "relational",
            "cognitive",
            "continuity"
        ],
        _ => []
    };

    private static bool HasReceipt(FirstRunConstitutionSnapshot snapshot, string label) => label switch
    {
        "sanctuary-initialization" => !string.IsNullOrWhiteSpace(snapshot.SanctuaryInitializationHandle),
        "location-binding" => !string.IsNullOrWhiteSpace(snapshot.LocationBindingHandle),
        "local-authority-trace" => !string.IsNullOrWhiteSpace(snapshot.LocalAuthorityTraceHandle),
        "constitutional-contact" => !string.IsNullOrWhiteSpace(snapshot.ConstitutionalContactHandle),
        "local-keypair-genesis" => !string.IsNullOrWhiteSpace(snapshot.LocalKeypairGenesisHandle),
        "first-cryptic-braid" => !string.IsNullOrWhiteSpace(snapshot.FirstCrypticBraidHandle),
        "first-cryptic-conditioning" => !string.IsNullOrWhiteSpace(snapshot.FirstCrypticConditioningHandle),
        "mother-standing" => !string.IsNullOrWhiteSpace(snapshot.MotherStandingHandle),
        "father-standing" => !string.IsNullOrWhiteSpace(snapshot.FatherStandingHandle),
        "cradletek-install" => !string.IsNullOrWhiteSpace(snapshot.CradleTekInstallHandle),
        "cradletek-admission" => !string.IsNullOrWhiteSpace(snapshot.CradleTekAdmissionHandle),
        "steward-standing" => !string.IsNullOrWhiteSpace(snapshot.StewardStandingHandle),
        "gel-standing" => !string.IsNullOrWhiteSpace(snapshot.GelStandingHandle),
        "goa-standing" => !string.IsNullOrWhiteSpace(snapshot.GoaStandingHandle),
        "mos-standing" => !string.IsNullOrWhiteSpace(snapshot.MosStandingHandle),
        "tool-rights" => !string.IsNullOrWhiteSpace(snapshot.ToolRightsHandle),
        "data-rights" => !string.IsNullOrWhiteSpace(snapshot.DataRightsHandle),
        "hosted-seed-presence" => !string.IsNullOrWhiteSpace(snapshot.HostedSeedPresenceHandle),
        "bond-process-open" => !string.IsNullOrWhiteSpace(snapshot.BondProcessHandle),
        "opal-actualization" => !string.IsNullOrWhiteSpace(snapshot.OpalActualizationHandle),
        "notice-certification-gate" => !string.IsNullOrWhiteSpace(snapshot.NoticeCertificationGateHandle),
        _ => false
    };

    private static bool IsProvisionalState(FirstRunConstitutionState state) =>
        state is FirstRunConstitutionState.CradleTekInstalled or FirstRunConstitutionState.BondProcessOpen;

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
