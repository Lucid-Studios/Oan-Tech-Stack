namespace San.Audit.Tests;

using System.Text.Json;
using San.Common;

public sealed class CognitiveFormationContractsTests
{
    [Fact]
    public void Pressure_And_Failure_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                ZedLocusState.Preserved,
                ZedLocusState.Strained,
                ZedLocusState.Lost
            ],
            Enum.GetValues<ZedLocusState>());

        Assert.Equal(
            [
                DeltaPressureKind.None,
                DeltaPressureKind.LoadBearing,
                DeltaPressureKind.BasinSteepening,
                DeltaPressureKind.NadirDisclosure,
                DeltaPressureKind.CollapsePressure
            ],
            Enum.GetValues<DeltaPressureKind>());

        Assert.Equal(
            [
                FailureSignatureKind.None,
                FailureSignatureKind.BrittleOverclaim,
                FailureSignatureKind.ConjunctionImbalance,
                FailureSignatureKind.BoundarySlip,
                FailureSignatureKind.SourceDissolution,
                FailureSignatureKind.OrientationCollapse
            ],
            Enum.GetValues<FailureSignatureKind>());

        Assert.Equal(
            [
                OrientationIntegrityKind.Stable,
                OrientationIntegrityKind.Minimum,
                OrientationIntegrityKind.Compromised,
                OrientationIntegrityKind.Lost
            ],
            Enum.GetValues<OrientationIntegrityKind>());

        Assert.Equal(
            [
                FailureEvidenceDispositionKind.None,
                FailureEvidenceDispositionKind.Refuse,
                FailureEvidenceDispositionKind.RetainAsFailureEvidence,
                FailureEvidenceDispositionKind.PromoteToOrientationEvidence
            ],
            Enum.GetValues<FailureEvidenceDispositionKind>());
    }

    [Fact]
    public void CognitiveFormationSnapshot_And_Receipt_RoundTrip_With_ExplicitFormationState()
    {
        var snapshot = CreateSnapshot(
            requirementStates:
            [
                CreateRequirementState("req://present", "orientation-ready", RequirementStateKind.Present, WhyNotClassificationKind.None),
                CreateRequirementState("req://optional", "far-field", RequirementStateKind.NotRequired, WhyNotClassificationKind.IrrelevantToCurrentAct)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.ClassifyNotRequired);

        var json = JsonSerializer.Serialize(snapshot);
        var roundTrip = JsonSerializer.Deserialize<CognitiveFormationSnapshot>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(NextLawfulMoveKind.ClassifyNotRequired, roundTrip!.SelectedNextLawfulMove);
        Assert.Equal(AwarenessConeBoundaryKind.Inside, roundTrip.Orientation.ConeBoundary);
        Assert.Equal(CompassOrientationFacetKind.Center, roundTrip.Orientation.OrientationFacet);
        Assert.Equal(SourceRelationKind.Duplex, roundTrip.Orientation.SourceRelation);
        Assert.Equal(ZedLocusState.Preserved, roundTrip.Orientation.ZedLocusState);
        Assert.Equal(OrientationIntegrityKind.Stable, roundTrip.Orientation.OrientationIntegrity);
        Assert.Equal(DeltaPressureKind.None, roundTrip.Orientation.DeltaPressure);

        var receipt = CognitiveFormationEvaluator.Evaluate(
            snapshot,
            receiptHandle: "receipt://formation/session-a");

        var receiptJson = JsonSerializer.Serialize(receipt);
        var receiptRoundTrip = JsonSerializer.Deserialize<FormationReceipt>(receiptJson);

        Assert.NotNull(receiptRoundTrip);
        Assert.Equal("receipt://formation/session-a", receiptRoundTrip!.ReceiptHandle);
        Assert.Equal(NextLawfulMoveKind.ClassifyNotRequired, receiptRoundTrip.SelectedNextLawfulMove);
        Assert.Equal(FormationKnowledgePostureKind.KnownEnoughForNextAct, receiptRoundTrip.KnowledgePosture);
        Assert.True(receiptRoundTrip.PromotionToKnownLawful);
        Assert.Equal(FailureEvidenceDispositionKind.None, receiptRoundTrip.FailureEvidenceDisposition);
        Assert.Empty(receiptRoundTrip.RetainedFailureSignatures);
        Assert.Empty(receiptRoundTrip.OrientationEvidenceSignatures);
        Assert.Equal(["signal://alpha"], receiptRoundTrip.LawfulKnownItems);
        Assert.Contains("signal://beta", receiptRoundTrip.RetainedUnknownItems);
        Assert.Equal("formation-not-required-classified", receiptRoundTrip.ReasonCode);
        Assert.False(receiptRoundTrip.Deferred);
        Assert.False(receiptRoundTrip.Refused);
    }

    [Fact]
    public void OutsideCone_Cannot_Be_Promoted_To_Known()
    {
        var orientation = CreateOrientation(AwarenessConeBoundaryKind.Outside);

        Assert.False(CognitiveFormationEvaluator.IsPromotionToKnownLawful(orientation));
    }

    [Fact]
    public void Evaluate_OutsideCone_WithholdsKnownItems_And_BindsBoundary()
    {
        var snapshot = CreateSnapshot(
            orientation: CreateOrientation(AwarenessConeBoundaryKind.Outside),
            requirementStates:
            [
                CreateRequirementState("req://present", "core-footing", RequirementStateKind.Present, WhyNotClassificationKind.None)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.RetainCurrentFooting);

        var receipt = CognitiveFormationEvaluator.Evaluate(snapshot, "receipt://formation/outside-cone");

        Assert.Equal(FormationKnowledgePostureKind.WithheldOutsideCone, receipt.KnowledgePosture);
        Assert.False(receipt.PromotionToKnownLawful);
        Assert.Empty(receipt.LawfulKnownItems);
        Assert.Equal(NextLawfulMoveKind.BindBoundary, receipt.SelectedNextLawfulMove);
        Assert.Contains(WhyNotClassificationKind.OutOfCone, receipt.WhyNotClasses);
        Assert.Equal("formation-outside-cone", receipt.ReasonCode);
    }

    [Fact]
    public void RequirementState_Distinctions_Remain_Explicit()
    {
        var states = new[]
        {
            RequirementStateKind.Missing,
            RequirementStateKind.Blocked,
            RequirementStateKind.Unknown,
            RequirementStateKind.NotRequired
        };

        Assert.Equal(4, states.Distinct().Count());
        Assert.DoesNotContain(RequirementStateKind.Present, states);
    }

    [Fact]
    public void NotRequired_Item_Does_Not_Block_The_Next_Lawful_Move()
    {
        var next = CognitiveFormationEvaluator.SelectNextLawfulMove(
            CreateOrientation(AwarenessConeBoundaryKind.Inside),
            [
                CreateRequirementState("req://present", "core-footing", RequirementStateKind.Present, WhyNotClassificationKind.None),
                CreateRequirementState("req://not-required", "outer-expansion", RequirementStateKind.NotRequired, WhyNotClassificationKind.IrrelevantToCurrentAct)
            ]);

        Assert.Equal(NextLawfulMoveKind.ClassifyNotRequired, next);
    }

    [Fact]
    public void Missing_Prerequisite_Selects_RequestEvidence()
    {
        var next = CognitiveFormationEvaluator.SelectNextLawfulMove(
            CreateOrientation(AwarenessConeBoundaryKind.Inside),
            [
                CreateRequirementState("req://missing", "needed-evidence", RequirementStateKind.Missing, WhyNotClassificationKind.PrerequisiteAbsent)
            ]);

        Assert.Equal(NextLawfulMoveKind.RequestEvidence, next);
    }

    [Fact]
    public void Blocked_Requirement_Selects_BindBoundary()
    {
        var next = CognitiveFormationEvaluator.SelectNextLawfulMove(
            CreateOrientation(AwarenessConeBoundaryKind.Inside),
            [
                CreateRequirementState("req://blocked", "foreign-authority", RequirementStateKind.Blocked, WhyNotClassificationKind.BlockedByBoundary)
            ]);

        Assert.Equal(NextLawfulMoveKind.BindBoundary, next);
    }

    [Fact]
    public void Evaluate_Preserves_Missing_Blocked_Unknown_And_NotRequired_Distinctions()
    {
        var snapshot = CreateSnapshot(
            requirementStates:
            [
                CreateRequirementState("req://missing", "needed-evidence", RequirementStateKind.Missing, WhyNotClassificationKind.PrerequisiteAbsent),
                CreateRequirementState("req://blocked", "foreign-authority", RequirementStateKind.Blocked, WhyNotClassificationKind.BlockedByBoundary),
                CreateRequirementState("req://unknown", "unread-trace", RequirementStateKind.Unknown, WhyNotClassificationKind.InsufficientEvidence),
                CreateRequirementState("req://not-required", "future-expansion", RequirementStateKind.NotRequired, WhyNotClassificationKind.IrrelevantToCurrentAct)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.RetainCurrentFooting);

        var receipt = CognitiveFormationEvaluator.Evaluate(snapshot, "receipt://formation/distinctions");

        Assert.Contains(receipt.RequirementStates, static item => item.State == RequirementStateKind.Missing);
        Assert.Contains(receipt.RequirementStates, static item => item.State == RequirementStateKind.Blocked);
        Assert.Contains(receipt.RequirementStates, static item => item.State == RequirementStateKind.Unknown);
        Assert.Contains(receipt.RequirementStates, static item => item.State == RequirementStateKind.NotRequired);
        Assert.Contains(WhyNotClassificationKind.PrerequisiteAbsent, receipt.WhyNotClasses);
        Assert.Contains(WhyNotClassificationKind.BlockedByBoundary, receipt.WhyNotClasses);
        Assert.Contains(WhyNotClassificationKind.InsufficientEvidence, receipt.WhyNotClasses);
        Assert.Contains(WhyNotClassificationKind.IrrelevantToCurrentAct, receipt.WhyNotClasses);
        Assert.Equal(NextLawfulMoveKind.BindBoundary, receipt.SelectedNextLawfulMove);
    }

    [Fact]
    public void Unknown_SourceRelation_Forces_Inspect_Without_Promotion()
    {
        var orientation = new SensoryOrientationSnapshot(
            OrientationHandle: "orientation://unknown-source",
            ListeningFrameHandle: "listening://frame/session-a",
            CompassEmbodimentHandle: "compass://embodiment/session-a",
            ConeBoundary: AwarenessConeBoundaryKind.Inside,
            OrientationFacet: CompassOrientationFacetKind.Center,
            PerceptualPolarity: PerceptualPolarityKind.Direct,
            SourceRelation: SourceRelationKind.Unknown,
            ModalityMarkers: ["listening"],
            OrientationNotes: ["source-unresolved"],
            TimestampUtc: new DateTimeOffset(2026, 04, 13, 18, 15, 00, TimeSpan.Zero));

        var snapshot = CreateSnapshot(
            orientation: orientation,
            requirementStates:
            [
                CreateRequirementState("req://present", "orientation-ready", RequirementStateKind.Present, WhyNotClassificationKind.None)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.RetainCurrentFooting);

        var receipt = CognitiveFormationEvaluator.Evaluate(snapshot, "receipt://formation/unknown-source");

        Assert.Equal(FormationKnowledgePostureKind.NeedsInspection, receipt.KnowledgePosture);
        Assert.False(receipt.PromotionToKnownLawful);
        Assert.Equal(NextLawfulMoveKind.Inspect, receipt.SelectedNextLawfulMove);
        Assert.Equal("formation-source-relation-unresolved", receipt.ReasonCode);
        Assert.Contains(WhyNotClassificationKind.InsufficientEvidence, receipt.WhyNotClasses);
    }

    [Fact]
    public void FailureSignature_Promotes_To_OrientationEvidence_Only_When_Zed_And_Integrity_Hold()
    {
        var snapshot = CreateSnapshot(
            orientation: CreateOrientation(
                AwarenessConeBoundaryKind.Inside,
                zedLocusState: ZedLocusState.Preserved,
                orientationIntegrity: OrientationIntegrityKind.Minimum,
                deltaPressure: DeltaPressureKind.NadirDisclosure),
            requirementStates:
            [
                CreateRequirementState("req://present", "orientation-ready", RequirementStateKind.Present, WhyNotClassificationKind.None)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.RetainCurrentFooting,
            failureSignatures:
            [
                CreateFailureSignature("failure://signature/a", FailureSignatureKind.ConjunctionImbalance, DeltaPressureKind.NadirDisclosure)
            ]);

        var receipt = CognitiveFormationEvaluator.Evaluate(snapshot, "receipt://formation/orientation-evidence");

        Assert.Equal(FailureEvidenceDispositionKind.PromoteToOrientationEvidence, receipt.FailureEvidenceDisposition);
        Assert.Equal(FormationKnowledgePostureKind.OrientationEvidencePromoted, receipt.KnowledgePosture);
        Assert.False(receipt.PromotionToKnownLawful);
        Assert.Empty(receipt.LawfulKnownItems);
        Assert.Equal(NextLawfulMoveKind.Inspect, receipt.SelectedNextLawfulMove);
        Assert.Equal("formation-failure-promoted-to-orientation-evidence", receipt.ReasonCode);
        Assert.Single(receipt.RetainedFailureSignatures);
        Assert.Single(receipt.OrientationEvidenceSignatures);
        Assert.Contains(WhyNotClassificationKind.CollapseSignaturePresent, receipt.WhyNotClasses);
        Assert.DoesNotContain(WhyNotClassificationKind.ZedLocusNotPreserved, receipt.WhyNotClasses);
        Assert.DoesNotContain(WhyNotClassificationKind.OrientationIntegrityInsufficient, receipt.WhyNotClasses);
    }

    [Fact]
    public void FailureSignature_With_Strained_ZedLocus_Is_Retained_As_FailureEvidence()
    {
        var snapshot = CreateSnapshot(
            orientation: CreateOrientation(
                AwarenessConeBoundaryKind.Inside,
                zedLocusState: ZedLocusState.Strained,
                orientationIntegrity: OrientationIntegrityKind.Stable,
                deltaPressure: DeltaPressureKind.BasinSteepening),
            requirementStates:
            [
                CreateRequirementState("req://present", "orientation-ready", RequirementStateKind.Present, WhyNotClassificationKind.None)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.RetainCurrentFooting,
            failureSignatures:
            [
                CreateFailureSignature("failure://signature/b", FailureSignatureKind.BrittleOverclaim, DeltaPressureKind.BasinSteepening)
            ]);

        var receipt = CognitiveFormationEvaluator.Evaluate(snapshot, "receipt://formation/failure-retained");

        Assert.Equal(FailureEvidenceDispositionKind.RetainAsFailureEvidence, receipt.FailureEvidenceDisposition);
        Assert.Equal(FormationKnowledgePostureKind.FailureEvidenceRetained, receipt.KnowledgePosture);
        Assert.Equal(NextLawfulMoveKind.Defer, receipt.SelectedNextLawfulMove);
        Assert.Equal("formation-failure-retained-zed-locus-strained", receipt.ReasonCode);
        Assert.Single(receipt.RetainedFailureSignatures);
        Assert.Empty(receipt.OrientationEvidenceSignatures);
        Assert.Contains(WhyNotClassificationKind.CollapseSignaturePresent, receipt.WhyNotClasses);
        Assert.Contains(WhyNotClassificationKind.ZedLocusNotPreserved, receipt.WhyNotClasses);
    }

    [Fact]
    public void FailureSignature_With_Lost_ZedLocus_Is_Refused()
    {
        var snapshot = CreateSnapshot(
            orientation: CreateOrientation(
                AwarenessConeBoundaryKind.Inside,
                zedLocusState: ZedLocusState.Lost,
                orientationIntegrity: OrientationIntegrityKind.Lost,
                deltaPressure: DeltaPressureKind.CollapsePressure),
            requirementStates:
            [
                CreateRequirementState("req://present", "orientation-ready", RequirementStateKind.Present, WhyNotClassificationKind.None)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.RetainCurrentFooting,
            failureSignatures:
            [
                CreateFailureSignature("failure://signature/c", FailureSignatureKind.OrientationCollapse, DeltaPressureKind.CollapsePressure)
            ]);

        var receipt = CognitiveFormationEvaluator.Evaluate(snapshot, "receipt://formation/failure-refused");

        Assert.Equal(FailureEvidenceDispositionKind.Refuse, receipt.FailureEvidenceDisposition);
        Assert.Equal(FormationKnowledgePostureKind.FailureRefused, receipt.KnowledgePosture);
        Assert.Equal(NextLawfulMoveKind.Refuse, receipt.SelectedNextLawfulMove);
        Assert.Equal("formation-failure-refused-zed-locus-lost", receipt.ReasonCode);
        Assert.True(receipt.Refused);
        Assert.Contains(WhyNotClassificationKind.CollapseSignaturePresent, receipt.WhyNotClasses);
        Assert.Contains(WhyNotClassificationKind.ZedLocusNotPreserved, receipt.WhyNotClasses);
        Assert.Contains(WhyNotClassificationKind.OrientationIntegrityInsufficient, receipt.WhyNotClasses);
    }

    [Fact]
    public void CreateReceipt_Can_Still_Emit_ManualReceipt_Without_HiddenInference()
    {
        var snapshot = CreateSnapshot(
            requirementStates:
            [
                CreateRequirementState("req://unknown", "unread-trace", RequirementStateKind.Unknown, WhyNotClassificationKind.InsufficientEvidence)
            ],
            selectedNextLawfulMove: NextLawfulMoveKind.Defer,
            failureSignatures:
            [
                CreateFailureSignature("failure://signature/manual", FailureSignatureKind.BoundarySlip, DeltaPressureKind.LoadBearing)
            ]);

        var receipt = CognitiveFormationEvaluator.CreateReceipt(
            snapshot,
            receiptHandle: "receipt://formation/manual",
            lawfulBasis: "manual-bounded-review",
            deferred: true);

        Assert.Equal(FormationKnowledgePostureKind.NotKnown, receipt.KnowledgePosture);
        Assert.False(receipt.PromotionToKnownLawful);
        Assert.Equal(FailureEvidenceDispositionKind.RetainAsFailureEvidence, receipt.FailureEvidenceDisposition);
        Assert.Equal(NextLawfulMoveKind.Defer, receipt.SelectedNextLawfulMove);
        Assert.Equal("manual-formation-receipt", receipt.ReasonCode);
        Assert.Single(receipt.RetainedFailureSignatures);
        Assert.Empty(receipt.OrientationEvidenceSignatures);
        Assert.True(receipt.Deferred);
    }

    [Fact]
    public void Docs_Record_LightCone_Lineage_EcBuildspace_And_Diamond_Field_Law()
    {
        var lineRoot = GetLineRoot();
        var lineagePath = Path.Combine(lineRoot, "docs", "LIGHT_CONE_AWARENESS_LINEAGE_AND_LISTENING_FRAME_SOURCE_LAW.md");
        var buildspacePath = Path.Combine(lineRoot, "docs", "EC_FORMATION_BUILDSPACE_PREPARATION_NOTE.md");
        var diamondPath = Path.Combine(lineRoot, "docs", "EC_OAN_DIAMOND_LINEAGE_AND_ZED_DELTA_SOURCE_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lineagePath));
        Assert.True(File.Exists(buildspacePath));
        Assert.True(File.Exists(diamondPath));

        var lineageText = File.ReadAllText(lineagePath);
        var buildspaceText = File.ReadAllText(buildspacePath);
        var diamondText = File.ReadAllText(diamondPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("`light cone of awareness`", lineageText, StringComparison.Ordinal);
        Assert.Contains("`ListeningFrame`", lineageText, StringComparison.Ordinal);
        Assert.Contains("`Compass`", lineageText, StringComparison.Ordinal);
        Assert.Contains("`Engineered Cognition`", lineageText, StringComparison.Ordinal);

        Assert.Contains("EC_OAN_DIAMOND_LINEAGE_AND_ZED_DELTA_SOURCE_LAW.md", buildspaceText, StringComparison.Ordinal);
        Assert.Contains("`ZedLocusState`", buildspaceText, StringComparison.Ordinal);
        Assert.Contains("`FailureSignatureKind`", buildspaceText, StringComparison.Ordinal);

        Assert.Contains("`Opal Engram Core`", diamondText, StringComparison.Ordinal);
        Assert.Contains("Mind / Body / Spirit", diamondText, StringComparison.Ordinal);
        Assert.Contains("self-in-self", diamondText, StringComparison.Ordinal);
        Assert.Contains("self-in-other", diamondText, StringComparison.Ordinal);
        Assert.Contains("other-in-other", diamondText, StringComparison.Ordinal);
        Assert.Contains("peerless forms", diamondText, StringComparison.Ordinal);
        Assert.Contains("`Zed`", diamondText, StringComparison.Ordinal);
        Assert.Contains("`Delta`", diamondText, StringComparison.Ordinal);
        Assert.Contains("No failure signature may be treated as orientation evidence", diamondText, StringComparison.Ordinal);

        Assert.Contains("EC_OAN_DIAMOND_LINEAGE_AND_ZED_DELTA_SOURCE_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("ec-oan-diamond-lineage-zed-delta-source-law: frame-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("OAN Diamond lineage and bounded zed/delta field law", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("direct runtime geometry over the full OAN Diamond numbered field", refinementText, StringComparison.Ordinal);
    }

    private static CognitiveFormationSnapshot CreateSnapshot(
        IReadOnlyList<FormationRequirementState> requirementStates,
        NextLawfulMoveKind selectedNextLawfulMove,
        SensoryOrientationSnapshot? orientation = null,
        IReadOnlyList<FormationFailureSignature>? failureSignatures = null)
    {
        return new CognitiveFormationSnapshot(
            SnapshotHandle: "snapshot://formation/session-a",
            EncounterHandle: "encounter://formation/session-a",
            EngineeredCognitionHandle: "ec://formation/session-a",
            Orientation: orientation ?? CreateOrientation(AwarenessConeBoundaryKind.Inside),
            DiscernedItems: ["signal://alpha", "signal://beta"],
            RequirementStates: requirementStates,
            KnownItems: ["signal://alpha"],
            UnknownItems: ["signal://beta"],
            DeferredItems: [],
            SelectedNextLawfulMove: selectedNextLawfulMove,
            RetainedDecisionResult: "candidate-only",
            TimestampUtc: new DateTimeOffset(2026, 04, 13, 18, 00, 00, TimeSpan.Zero),
            FailureSignatures: failureSignatures);
    }

    private static SensoryOrientationSnapshot CreateOrientation(
        AwarenessConeBoundaryKind coneBoundary,
        ZedLocusState zedLocusState = ZedLocusState.Preserved,
        OrientationIntegrityKind orientationIntegrity = OrientationIntegrityKind.Stable,
        DeltaPressureKind deltaPressure = DeltaPressureKind.None)
    {
        return new SensoryOrientationSnapshot(
            OrientationHandle: "orientation://session-a",
            ListeningFrameHandle: "listening://frame/session-a",
            CompassEmbodimentHandle: "compass://embodiment/session-a",
            ConeBoundary: coneBoundary,
            OrientationFacet: CompassOrientationFacetKind.Center,
            PerceptualPolarity: PerceptualPolarityKind.Inverted,
            SourceRelation: SourceRelationKind.Duplex,
            ModalityMarkers: ["listening", "cryptic-trace"],
            OrientationNotes: ["center-held", "candidate-only"],
            TimestampUtc: new DateTimeOffset(2026, 04, 13, 18, 00, 00, TimeSpan.Zero),
            ZedOfDeltaHandle: "zed://delta/session-a",
            ZedLocusState: zedLocusState,
            OrientationIntegrity: orientationIntegrity,
            DeltaPressure: deltaPressure);
    }

    private static FormationRequirementState CreateRequirementState(
        string handle,
        string kind,
        RequirementStateKind state,
        WhyNotClassificationKind whyNot)
    {
        return new FormationRequirementState(
            RequirementHandle: handle,
            RequirementKind: kind,
            State: state,
            WhyNot: whyNot,
            EvidenceHandles: [$"evidence://{kind}"],
            Notes: [$"state:{state}", $"why:{whyNot}"]);
    }

    private static FormationFailureSignature CreateFailureSignature(
        string handle,
        FailureSignatureKind signatureKind,
        DeltaPressureKind deltaPressure)
    {
        return new FormationFailureSignature(
            SignatureHandle: handle,
            SignatureKind: signatureKind,
            DeltaPressure: deltaPressure,
            EvidenceHandles: [$"evidence://{signatureKind}"],
            Notes: [$"pressure:{deltaPressure}", $"signature:{signatureKind}"]);
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
