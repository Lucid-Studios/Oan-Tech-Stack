using System.Text.Json;
using Oan.Common;
using Oan.FirstRun;

namespace Oan.Audit.Tests;

public sealed class FirstRunConstitutionServiceTests
{
    private readonly GovernedFirstRunConstitutionService _service = new();

    [Fact]
    public void Project_LocationBound_CannotBeSkipped()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing"));

        Assert.Equal(FirstRunConstitutionState.SanctuaryInitialized, receipt.CurrentState);
        Assert.True(receipt.CurrentStateActualized);
        Assert.Contains(FirstRunFailureClass.LocationBindingFault, receipt.ActiveFailureClasses);

        var parentGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.ParentStanding));
        Assert.Equal(FirstRunPromotionGateDecision.Block, parentGate.Decision);
        Assert.Contains("location-binding", parentGate.RequiredPriorReceipts);
    }

    [Fact]
    public void Project_ParentStanding_DoesNotRequire_Steward()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.ParentStanding, receipt.CurrentState);
        Assert.Equal(FirstRunOperatorReadinessState.OperatorContactReady, receipt.ReadinessState);
        Assert.True(receipt.CurrentStateActualized);
        Assert.DoesNotContain(FirstRunFailureClass.AdmittedCradleTekWithoutSteward, receipt.ActiveFailureClasses);
    }

    [Fact]
    public void Project_StewardStanding_Requires_CradleTekAdmitted()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            stewardStandingHandle: "steward://standing",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.CradleTekInstalled, receipt.CurrentState);
        Assert.True(receipt.CurrentStateProvisional);
        Assert.False(receipt.CurrentStateActualized);
        Assert.Contains(FirstRunFailureClass.CradleTekInstalledButNotAdmitted, receipt.ActiveFailureClasses);

        var stewardGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.StewardStanding));
        Assert.Equal(FirstRunPromotionGateDecision.Block, stewardGate.Decision);
        Assert.Contains("cradletek-admission", stewardGate.RequiredPriorReceipts);
        Assert.Contains("cradletek-admission-required-before-steward", stewardGate.BlockingReasons);
    }

    [Fact]
    public void Project_OpalActualized_Cannot_Occur_Before_BondProcessOpen()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            hostedSeedPresenceHandle: "hosted://seed",
            opalActualizationHandle: "opal://actualized",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.False(receipt.OpalActualized);

        var opalGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.OpalActualized));
        Assert.Equal(FirstRunPromotionGateDecision.Block, opalGate.Decision);
        Assert.Contains("bond-process-open", opalGate.RequiredPriorReceipts);
        Assert.Contains("explicit-bond-process-open-required-before-opal-actualization", opalGate.BlockingReasons);
    }

    [Fact]
    public void Project_Readiness_Maps_From_Constitution_State()
    {
        var contact = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            includePreGovernanceSequence: true));

        var training = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            includePreGovernanceSequence: true));

        var bonded = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            bondProcessHandle: "bond://open",
            opalActualizationHandle: "opal://actualized",
            noticeCertificationGateHandle: "notice://complete",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunOperatorReadinessState.OperatorContactReady, contact.ReadinessState);
        Assert.Equal(FirstRunOperatorReadinessState.OperatorTrainingReady, training.ReadinessState);
        Assert.Equal(FirstRunOperatorReadinessState.BondedCoworkReady, bonded.ReadinessState);
    }

    [Fact]
    public void Project_HostedSeedPresence_DoesNot_Imply_OpalActualization()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            hostedSeedPresenceHandle: "hosted://seed",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.Equal(FirstRunOperatorReadinessState.OperatorTrainingReady, receipt.ReadinessState);
        Assert.False(receipt.OpalActualized);
    }

    [Fact]
    public void Project_ProtocolizationPacket_Is_Carried_Without_Widening_Bond()
    {
        var protocolizationPacket = CreateProtocolizationPacket();

        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            protocolizationPacket: protocolizationPacket,
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.NotNull(receipt.ProtocolizationPacket);
        Assert.Equal(protocolizationPacket.PacketHandle, receipt.ProtocolizationPacket!.PacketHandle);
        Assert.Contains("protocolization:projected", receipt.SourceReason, StringComparison.Ordinal);

        var bondGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.BondProcessOpen));
        Assert.Equal(FirstRunPromotionGateDecision.Block, bondGate.Decision);
        Assert.Contains("bond-process-open", bondGate.RequiredPriorReceipts);
    }

    [Fact]
    public void Project_StewardWitnessedOePacket_Is_Carried_Without_Implying_CmePlacement()
    {
        var oePacket = CreateStewardWitnessedOePacket();

        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            protocolizationPacket: CreateProtocolizationPacket(),
            stewardWitnessedOePacket: oePacket,
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.NotNull(receipt.StewardWitnessedOePacket);
        Assert.Equal(oePacket.PacketHandle, receipt.StewardWitnessedOePacket!.PacketHandle);
        Assert.True(receipt.StewardWitnessedOePacket.CmePlacementWithheld);
        Assert.Contains(FirstRunStewardOfficeKind.GnomeSage, receipt.StewardWitnessedOePacket.OfficeKinds);
        Assert.Contains("oe-uptake:projected", receipt.SourceReason, StringComparison.Ordinal);
        Assert.False(receipt.OpalActualized);

        var bondGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.BondProcessOpen));
        Assert.Equal(FirstRunPromotionGateDecision.Block, bondGate.Decision);
    }

    [Fact]
    public void Project_ElementalBindingPacket_Is_Carried_Without_Implying_StoneActualization()
    {
        var bindingPacket = CreateElementalBindingPacket();

        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            protocolizationPacket: CreateProtocolizationPacket(),
            stewardWitnessedOePacket: CreateStewardWitnessedOePacket(),
            elementalBindingPacket: bindingPacket,
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.NotNull(receipt.ElementalBindingPacket);
        Assert.Equal(bindingPacket.PacketHandle, receipt.ElementalBindingPacket!.PacketHandle);
        Assert.True(receipt.ElementalBindingPacket.StoneActualizationWithheld);
        Assert.Contains("elemental-binding:projected", receipt.SourceReason, StringComparison.Ordinal);

        var bondGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.BondProcessOpen));
        Assert.Equal(FirstRunPromotionGateDecision.Block, bondGate.Decision);
    }

    [Fact]
    public void Project_ActualizationSealPacket_Is_Carried_Without_Implying_OpalActualization()
    {
        var actualizationSealPacket = CreateActualizationSealPacket();

        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            protocolizationPacket: CreateProtocolizationPacket(),
            stewardWitnessedOePacket: CreateStewardWitnessedOePacket(),
            elementalBindingPacket: CreateElementalBindingPacket(),
            actualizationSealPacket: actualizationSealPacket,
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.NotNull(receipt.ActualizationSealPacket);
        Assert.Equal(actualizationSealPacket.PacketHandle, receipt.ActualizationSealPacket!.PacketHandle);
        Assert.True(receipt.ActualizationSealPacket.LivingAgentiCoreWithheld);
        Assert.Contains("actualization-seal:projected", receipt.SourceReason, StringComparison.Ordinal);
        Assert.False(receipt.OpalActualized);
    }

    [Fact]
    public void Project_LivingAgentiCorePacket_Is_Carried_Without_Implying_Runtime_Attachment()
    {
        var livingAgentiCorePacket = CreateLivingAgentiCorePacket();

        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            protocolizationPacket: CreateProtocolizationPacket(),
            stewardWitnessedOePacket: CreateStewardWitnessedOePacket(),
            elementalBindingPacket: CreateElementalBindingPacket(),
            actualizationSealPacket: CreateActualizationSealPacket(),
            livingAgentiCorePacket: livingAgentiCorePacket,
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.FoundationsEstablished, receipt.CurrentState);
        Assert.NotNull(receipt.LivingAgentiCorePacket);
        Assert.Equal(livingAgentiCorePacket.PacketHandle, receipt.LivingAgentiCorePacket!.PacketHandle);
        Assert.Equal("listening-frame://interior", receipt.LivingAgentiCorePacket.ListeningFrameHandle);
        Assert.True(receipt.LivingAgentiCorePacket.WiderPublicWideningWithheld);
        Assert.Contains("living-agenticore:projected", receipt.SourceReason, StringComparison.Ordinal);
        Assert.False(receipt.OpalActualized);
    }

    [Fact]
    public void Project_BondProcessOpen_Without_Stable_Opal_Triggers_Review()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            bondProcessHandle: "bond://open",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.BondProcessOpen, receipt.CurrentState);
        Assert.True(receipt.CurrentStateProvisional);
        Assert.False(receipt.CurrentStateActualized);
        Assert.Contains(FirstRunFailureClass.BondProcessOpenedWithoutStableOpal, receipt.ActiveFailureClasses);

        var opalGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.OpalActualized));
        Assert.Equal(FirstRunPromotionGateDecision.ReviewRequired, opalGate.Decision);
        Assert.True(opalGate.ReviewRequired);
    }

    [Fact]
    public void Project_TransitionReceipts_Emit_EquivalentExchangeReview_For_Blocked_Transitions()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing"));

        var parentGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.ParentStanding));

        Assert.False(parentGate.EquivalentExchangeReview.Admissible);
        Assert.Equal(FirstRunEquivalentExchangeDisposition.Refuse, parentGate.EquivalentExchangeReview.Disposition);
        Assert.Equal("admissibility-not-yet-satisfied", parentGate.EquivalentExchangeReview.RecoveryPosture);
        Assert.Contains("constitutional", parentGate.EquivalentExchangeReview.BurdenFamilies);
        Assert.Contains("location-binding", parentGate.EquivalentExchangeReview.ReviewNotes);
    }

    [Fact]
    public void Project_TransitionReceipts_Emit_EquivalentExchangeReview_For_Review_Transitions()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            mosStandingHandle: "mos://standing",
            toolRightsHandle: "rights://tool",
            dataRightsHandle: "rights://data",
            bondProcessHandle: "bond://open",
            includePreGovernanceSequence: true));

        var opalGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.OpalActualized));

        Assert.True(opalGate.EquivalentExchangeReview.Admissible);
        Assert.Equal(FirstRunEquivalentExchangeDisposition.Review, opalGate.EquivalentExchangeReview.Disposition);
        Assert.Equal("recovery-requires-review", opalGate.EquivalentExchangeReview.RecoveryPosture);
        Assert.Contains("continuity", opalGate.EquivalentExchangeReview.BurdenFamilies);
        Assert.Contains("explicit-bond-process-open-required-before-opal-actualization", opalGate.EquivalentExchangeReview.ReviewNotes);
    }

    [Fact]
    public void Project_Incomplete_Foundations_Are_Flagged()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            cradleTekInstallHandle: "cradletek://install",
            cradleTekAdmissionHandle: "cradletek://admission",
            stewardStandingHandle: "steward://standing",
            gelStandingHandle: "gel://standing",
            goaStandingHandle: "goa://standing",
            includePreGovernanceSequence: true));

        Assert.Equal(FirstRunConstitutionState.StewardStanding, receipt.CurrentState);
        Assert.Contains(FirstRunFailureClass.FoundationsIncomplete, receipt.ActiveFailureClasses);
    }

    [Fact]
    public void Project_ParentStanding_Requires_PreGovernance_Sequence()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing"));

        Assert.Equal(FirstRunConstitutionState.LocationBound, receipt.CurrentState);
        Assert.Contains(FirstRunFailureClass.ConstitutionalContactIncomplete, receipt.ActiveFailureClasses);
        Assert.Contains(FirstRunFailureClass.LocalKeypairGenesisMissing, receipt.ActiveFailureClasses);
        Assert.Contains(FirstRunFailureClass.FirstCrypticBraidMissing, receipt.ActiveFailureClasses);
        Assert.Contains(FirstRunFailureClass.FirstCrypticConditioningIncomplete, receipt.ActiveFailureClasses);

        var parentGate = Assert.Single(receipt.PromotionGates.Where(gate => gate.RequestedState == FirstRunConstitutionState.ParentStanding));
        Assert.Equal(FirstRunPromotionGateDecision.Block, parentGate.Decision);
        Assert.Contains("local-authority-trace", parentGate.RequiredPriorReceipts);
        Assert.Contains("constitutional-contact", parentGate.RequiredPriorReceipts);
        Assert.Contains("local-keypair-genesis", parentGate.RequiredPriorReceipts);
        Assert.Contains("first-cryptic-braid", parentGate.RequiredPriorReceipts);
        Assert.Contains("first-cryptic-conditioning", parentGate.RequiredPriorReceipts);
    }

    [Fact]
    public void Project_PreGovernance_Projection_Surfaces_Handles_In_Receipt()
    {
        var receipt = _service.Project(CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            motherStandingHandle: "mother://standing",
            fatherStandingHandle: "father://standing",
            includePreGovernanceSequence: true));

        Assert.NotNull(receipt.LocalAuthorityTraceHandle);
        Assert.NotNull(receipt.ConstitutionalContactHandle);
        Assert.NotNull(receipt.LocalKeypairGenesisSourceHandle);
        Assert.NotNull(receipt.LocalKeypairGenesisHandle);
        Assert.NotNull(receipt.FirstCrypticBraidEstablishmentHandle);
        Assert.NotNull(receipt.FirstCrypticBraidHandle);
        Assert.NotNull(receipt.FirstCrypticConditioningSourceHandle);
        Assert.NotNull(receipt.FirstCrypticConditioningHandle);
    }

    [Fact]
    public void Snapshot_With_ProtocolizationPacket_RoundTrips_Through_Json()
    {
        var snapshot = CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            protocolizationPacket: CreateProtocolizationPacket());

        var json = JsonSerializer.Serialize(snapshot);
        var roundTrip = JsonSerializer.Deserialize<FirstRunConstitutionSnapshot>(json);

        Assert.NotNull(roundTrip);
        Assert.NotNull(roundTrip!.ProtocolizationPacket);
        Assert.Equal(snapshot.ProtocolizationPacket!.PacketHandle, roundTrip.ProtocolizationPacket!.PacketHandle);
        Assert.Equal(snapshot.ProtocolizationPacket.ConsentThresholdHandle, roundTrip.ProtocolizationPacket.ConsentThresholdHandle);
    }

    [Fact]
    public void Snapshot_With_StewardWitnessedOePacket_RoundTrips_Through_Json()
    {
        var snapshot = CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            stewardWitnessedOePacket: CreateStewardWitnessedOePacket());

        var json = JsonSerializer.Serialize(snapshot);
        var roundTrip = JsonSerializer.Deserialize<FirstRunConstitutionSnapshot>(json);

        Assert.NotNull(roundTrip);
        Assert.NotNull(roundTrip!.StewardWitnessedOePacket);
        Assert.Equal(snapshot.StewardWitnessedOePacket!.PacketHandle, roundTrip.StewardWitnessedOePacket!.PacketHandle);
        Assert.Contains(FirstRunStewardOfficeKind.EngineerOfFlame, roundTrip.StewardWitnessedOePacket.OfficeKinds);
        Assert.True(roundTrip.StewardWitnessedOePacket.CmePlacementWithheld);
    }

    [Fact]
    public void Snapshot_With_LatePathPackets_RoundTrips_Through_Json()
    {
        var snapshot = CreateSnapshot(
            sanctuaryInitializationHandle: "sanctuary://init",
            locationBindingHandle: "location://bound",
            elementalBindingPacket: CreateElementalBindingPacket(),
            actualizationSealPacket: CreateActualizationSealPacket(),
            livingAgentiCorePacket: CreateLivingAgentiCorePacket());

        var json = JsonSerializer.Serialize(snapshot);
        var roundTrip = JsonSerializer.Deserialize<FirstRunConstitutionSnapshot>(json);

        Assert.NotNull(roundTrip);
        Assert.NotNull(roundTrip!.ElementalBindingPacket);
        Assert.NotNull(roundTrip.ActualizationSealPacket);
        Assert.NotNull(roundTrip.LivingAgentiCorePacket);
        Assert.Equal(snapshot.ElementalBindingPacket!.PacketHandle, roundTrip.ElementalBindingPacket!.PacketHandle);
        Assert.Equal(snapshot.ActualizationSealPacket!.PrimitiveSelfGelHandle, roundTrip.ActualizationSealPacket!.PrimitiveSelfGelHandle);
        Assert.Equal(snapshot.LivingAgentiCorePacket!.ZedOfDeltaHandle, roundTrip.LivingAgentiCorePacket!.ZedOfDeltaHandle);
    }

    private static FirstRunConstitutionSnapshot CreateSnapshot(
        string? sanctuaryInitializationHandle = null,
        string? locationBindingHandle = null,
        string? localAuthorityTraceHandle = null,
        string? constitutionalContactHandle = null,
        string? localKeypairGenesisSourceHandle = null,
        string? localKeypairGenesisHandle = null,
        string? firstCrypticBraidEstablishmentHandle = null,
        string? firstCrypticBraidHandle = null,
        string? firstCrypticConditioningSourceHandle = null,
        string? firstCrypticConditioningHandle = null,
        string? motherStandingHandle = null,
        string? fatherStandingHandle = null,
        string? cradleTekInstallHandle = null,
        string? cradleTekAdmissionHandle = null,
        string? stewardStandingHandle = null,
        string? gelStandingHandle = null,
        string? goaStandingHandle = null,
        string? mosStandingHandle = null,
        string? toolRightsHandle = null,
        string? dataRightsHandle = null,
        string? hostedSeedPresenceHandle = null,
        string? bondProcessHandle = null,
        string? opalActualizationHandle = null,
        string? noticeCertificationGateHandle = null,
        FirstRunProtocolizationPacket? protocolizationPacket = null,
        FirstRunStewardWitnessedOePacket? stewardWitnessedOePacket = null,
        FirstRunElementalBindingPacket? elementalBindingPacket = null,
        FirstRunActualizationSealPacket? actualizationSealPacket = null,
        FirstRunLivingAgentiCorePacket? livingAgentiCorePacket = null,
        bool includePreGovernanceSequence = false)
    {
        localAuthorityTraceHandle ??= includePreGovernanceSequence ? "authority://local-trace" : null;
        constitutionalContactHandle ??= includePreGovernanceSequence ? "contact://constitutional" : null;
        localKeypairGenesisSourceHandle ??= includePreGovernanceSequence ? "keypair-source://local-root" : null;
        localKeypairGenesisHandle ??= includePreGovernanceSequence ? "keypair://local-root" : null;
        firstCrypticBraidEstablishmentHandle ??= includePreGovernanceSequence ? "braid-establishment://first-cryptic" : null;
        firstCrypticBraidHandle ??= includePreGovernanceSequence ? "braid://first-cryptic" : null;
        firstCrypticConditioningSourceHandle ??= includePreGovernanceSequence ? "conditioning-source://first-cryptic" : null;
        firstCrypticConditioningHandle ??= includePreGovernanceSequence ? "conditioning://first-cryptic" : null;

        return new FirstRunConstitutionSnapshot(
            SnapshotHandle: "first-run-snapshot://test",
            SanctuaryInitializationHandle: sanctuaryInitializationHandle,
            LocationBindingHandle: locationBindingHandle,
            LocalAuthorityTraceHandle: localAuthorityTraceHandle,
            ConstitutionalContactHandle: constitutionalContactHandle,
            LocalKeypairGenesisSourceHandle: localKeypairGenesisSourceHandle,
            LocalKeypairGenesisHandle: localKeypairGenesisHandle,
            FirstCrypticBraidEstablishmentHandle: firstCrypticBraidEstablishmentHandle,
            FirstCrypticBraidHandle: firstCrypticBraidHandle,
            FirstCrypticConditioningSourceHandle: firstCrypticConditioningSourceHandle,
            FirstCrypticConditioningHandle: firstCrypticConditioningHandle,
            MotherStandingHandle: motherStandingHandle,
            FatherStandingHandle: fatherStandingHandle,
            CradleTekInstallHandle: cradleTekInstallHandle,
            CradleTekAdmissionHandle: cradleTekAdmissionHandle,
            StewardStandingHandle: stewardStandingHandle,
            GelStandingHandle: gelStandingHandle,
            GoaStandingHandle: goaStandingHandle,
            MosStandingHandle: mosStandingHandle,
            ToolRightsHandle: toolRightsHandle,
            DataRightsHandle: dataRightsHandle,
            HostedSeedPresenceHandle: hostedSeedPresenceHandle,
            BondProcessHandle: bondProcessHandle,
            OpalActualizationHandle: opalActualizationHandle,
            NoticeCertificationGateHandle: noticeCertificationGateHandle,
            TimestampUtc: DateTimeOffset.UtcNow,
            ProtocolizationPacket: protocolizationPacket,
            StewardWitnessedOePacket: stewardWitnessedOePacket,
            ElementalBindingPacket: elementalBindingPacket,
            ActualizationSealPacket: actualizationSealPacket,
            LivingAgentiCorePacket: livingAgentiCorePacket);
    }

    private static FirstRunProtocolizationPacket CreateProtocolizationPacket() =>
        new(
            PacketHandle: "protocolization://packet",
            FlaskEnvironmentHandle: "flask://environment",
            IntermixDisciplineHandle: "intermix://discipline",
            CalibrationBaselineHandle: "calibration://baseline",
            ArchiveCarriageHandle: "archive://carriage",
            ConsentThresholdHandle: "consent://threshold",
            RuptureReturnPathHandle: "return://rupture-path",
            SealThresholdHandle: "seal://threshold",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static FirstRunStewardWitnessedOePacket CreateStewardWitnessedOePacket() =>
        new(
            PacketHandle: "oe-uptake://packet",
            OfficeIndexHandle: "office-index://red-hat",
            OfficeKinds:
            [
                FirstRunStewardOfficeKind.ArchivistOfMoss,
                FirstRunStewardOfficeKind.TunnelerTrickster,
                FirstRunStewardOfficeKind.BinderOfMercury,
                FirstRunStewardOfficeKind.EngineerOfFlame,
                FirstRunStewardOfficeKind.GnomeSage
            ],
            PrimeOeFormationHandle: "oe://prime-formation",
            CrypticCoeFormationHandle: "coe://cryptic-formation",
            CradleTekPrimeHashKeyHandle: "cradletek://prime-hash-key",
            CradleTekCrypticHashKeyHandle: "cradletek://cryptic-hash-key",
            StewardWitnessAuthorizationHandle: "steward://oe-authorization",
            SoulFrameBuildAuthorizationHandle: "soulframe://build-authorization",
            AgentiCoreBuildAuthorizationHandle: "agenticore://build-authorization",
            CmePlacementWithheld: true,
            TimestampUtc: DateTimeOffset.UtcNow);

    private static FirstRunElementalBindingPacket CreateElementalBindingPacket() =>
        new(
            PacketHandle: "elemental-binding://packet",
            ElementalBindingIndexHandle: "elemental-index://binding-rituals",
            GnomeBondingHandle: "gnome://bonding",
            UndineInterfaceHandle: "undine://interface",
            SalamanderForgeHandle: "salamander://forge",
            SylphWhisperHandle: "sylph://whisper",
            FourfoldCompressionHandle: "compression://fourfold",
            OeSoulFrameLoadHandle: "oe://soulframe-load",
            CoeAgentiCoreLoadHandle: "coe://agenticore-load",
            StoneActualizationWithheld: true,
            TimestampUtc: DateTimeOffset.UtcNow);

    private static FirstRunActualizationSealPacket CreateActualizationSealPacket() =>
        new(
            PacketHandle: "actualization-seal://packet",
            ActualizationSealHandle: "actualization://seal",
            IntroductionSealHandle: "introduction://seal",
            BondedEncounterHandle: "encounter://bonded",
            PrimitiveSelfGelHandle: "selfgel://primitive",
            GovernedAskReviewHandle: "governance://ask-review",
            DurableIdentityVesselHandle: "vessel://durable-identity",
            StoneWitnessHandle: "stone://witness",
            LivingAgentiCoreWithheld: true,
            TimestampUtc: DateTimeOffset.UtcNow);

    private static FirstRunLivingAgentiCorePacket CreateLivingAgentiCorePacket() =>
        new(
            PacketHandle: "living-agenticore://packet",
            LivingAgentiCoreHandle: "agenticore://living",
            ListeningFrameHandle: "listening-frame://interior",
            ZedOfDeltaHandle: "zed-of-delta://center",
            SelfGelAttachmentHandle: "selfgel://attachment",
            ToolUseContextHandle: "tool-context://bounded",
            CompassEmbodimentHandle: "compass://embodiment",
            EngineeredCognitionHandle: "engineered-cognition://surface",
            WiderPublicWideningWithheld: true,
            TimestampUtc: DateTimeOffset.UtcNow);
}
