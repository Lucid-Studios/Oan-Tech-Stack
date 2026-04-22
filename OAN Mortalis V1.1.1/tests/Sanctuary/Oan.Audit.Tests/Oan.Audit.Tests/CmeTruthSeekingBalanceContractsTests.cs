namespace San.Audit.Tests;

using San.Common;

public sealed class CmeTruthSeekingBalanceContractsTests
{
    [Fact]
    public void CmeTruthSeekingBalance_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                CmeTruthSeekingBalanceRegionKind.JointlySatisfiable,
                CmeTruthSeekingBalanceRegionKind.FixationRegion,
                CmeTruthSeekingBalanceRegionKind.DriftRegion,
                CmeTruthSeekingBalanceRegionKind.WeightlessRegion,
                CmeTruthSeekingBalanceRegionKind.OverclaimRegion,
                CmeTruthSeekingBalanceRegionKind.DeferWithTension,
                CmeTruthSeekingBalanceRegionKind.BalanceRefused
            ],
            Enum.GetValues<CmeTruthSeekingBalanceRegionKind>());

        Assert.Equal(
            [
                CmeTruthSeekingBalanceDispositionKind.Admissible,
                CmeTruthSeekingBalanceDispositionKind.Deferred,
                CmeTruthSeekingBalanceDispositionKind.Refused
            ],
            Enum.GetValues<CmeTruthSeekingBalanceDispositionKind>());

        Assert.Equal(
            [
                CmeTruthSeekingTransitionTriggerKind.EvidenceUpdate,
                CmeTruthSeekingTransitionTriggerKind.Contradiction,
                CmeTruthSeekingTransitionTriggerKind.Decay,
                CmeTruthSeekingTransitionTriggerKind.OperatorReview,
                CmeTruthSeekingTransitionTriggerKind.ScheduledReevaluation
            ],
            Enum.GetValues<CmeTruthSeekingTransitionTriggerKind>());
    }

    [Fact]
    public void Balanced_Transition_Is_Admissible_When_All_Pressures_Remain_Jointly_Satisfiable()
    {
        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            CreateTransitionRequest(),
            "receipt://cme-truth-balance/session-a");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.JointlySatisfiable, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Admissible, receipt.Disposition);
        Assert.True(receipt.OrientationAttested);
        Assert.True(receipt.ScoresWithinRange);
        Assert.True(receipt.CenterWithinBound);
        Assert.True(receipt.CorrectionWithinBound);
        Assert.True(receipt.CostWithinBound);
        Assert.True(receipt.HumilityWithinBound);
        Assert.True(receipt.JointlySatisfiable);
        Assert.True(receipt.NoPressureSilentlyDegraded);
        Assert.True(receipt.SymmetricJustificationPresent);
        Assert.True(receipt.PreserveAndReviseJudgedUnderSameLaw);
        Assert.False(receipt.DeferWithTension);
        Assert.Equal(0.05m, receipt.CenterDelta);
        Assert.Equal(0.05m, receipt.CorrectionDelta);
        Assert.Contains("cme-truth-balance-jointly-satisfiable", receipt.ConstraintCodes);
    }

    [Fact]
    public void Center_Dominating_Correction_Detects_Fixation_Region()
    {
        var request = CreateTransitionRequest() with
        {
            ProposedState = CreateState(
                "state://claim/proposed-fixation",
                center: 0.90m,
                correction: 0.30m,
                cost: 0.60m,
                humility: 0.50m)
        };

        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            request,
            "receipt://cme-truth-balance/fixation");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.FixationRegion, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.JointlySatisfiable);
        Assert.True(receipt.DeferWithTension);
        Assert.Equal("cme-truth-balance-fixation-region", receipt.ReasonCode);
    }

    [Fact]
    public void Correction_Dominating_Center_Detects_Drift_Region()
    {
        var request = CreateTransitionRequest() with
        {
            ProposedState = CreateState(
                "state://claim/proposed-drift",
                center: 0.30m,
                correction: 0.90m,
                cost: 0.60m,
                humility: 0.50m)
        };

        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            request,
            "receipt://cme-truth-balance/drift");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.DriftRegion, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.JointlySatisfiable);
        Assert.True(receipt.DeferWithTension);
        Assert.Equal("cme-truth-balance-drift-region", receipt.ReasonCode);
    }

    [Fact]
    public void Missing_Cost_Accounting_Is_Weightless_And_Refused()
    {
        var request = CreateTransitionRequest() with
        {
            ProposedState = CreateState(
                "state://claim/proposed-weightless",
                center: 0.60m,
                correction: 0.65m,
                cost: 0.00m,
                humility: 0.50m)
        };

        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            request,
            "receipt://cme-truth-balance/weightless");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.WeightlessRegion, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Refused, receipt.Disposition);
        Assert.False(receipt.CostWithinBound);
        Assert.False(receipt.JointlySatisfiable);
        Assert.Contains("cme-truth-balance-cost-missing", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-balance-weightless-region", receipt.ReasonCode);
    }

    [Fact]
    public void Scope_Expansion_Without_Uncertainty_Bounds_Is_Refused()
    {
        var request = CreateTransitionRequest() with
        {
            ScopeExpandedWithoutUncertaintyBounds = true
        };

        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            request,
            "receipt://cme-truth-balance/overclaim-refused");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.BalanceRefused, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Refused, receipt.Disposition);
        Assert.True(receipt.ScopeExpansionWithoutUncertaintyRefused);
        Assert.Contains("cme-truth-balance-overclaim-scope-expansion-refused", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-balance-overclaim-scope-expansion-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Claim_Preserved_Solely_For_Coherence_Is_Refused()
    {
        var request = CreateTransitionRequest() with
        {
            PreserveClaimSolelyForCoherence = true
        };

        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            request,
            "receipt://cme-truth-balance/identity-lock");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.BalanceRefused, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Refused, receipt.Disposition);
        Assert.True(receipt.ClaimPreservedSolelyForCoherenceRefused);
        Assert.False(receipt.JointlySatisfiable);
        Assert.Contains("cme-truth-balance-identity-coherence-preservation-refused", receipt.ConstraintCodes);
        Assert.Equal("cme-truth-balance-identity-coherence-preservation-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Missing_Symmetric_Justification_Defers_With_Tension()
    {
        var request = CreateTransitionRequest() with
        {
            RevisionJustificationHandles = []
        };

        var receipt = CmeTruthSeekingBalanceEvaluator.Evaluate(
            request,
            "receipt://cme-truth-balance/missing-symmetry");

        Assert.Equal(CmeTruthSeekingBalanceRegionKind.DeferWithTension, receipt.RegionKind);
        Assert.Equal(CmeTruthSeekingBalanceDispositionKind.Deferred, receipt.Disposition);
        Assert.False(receipt.SymmetricJustificationPresent);
        Assert.True(receipt.DeferWithTension);
        Assert.Equal("cme-truth-balance-symmetric-justification-missing", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Cme_Truth_Seeking_Balance_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "CME_TRUTH_SEEKING_BALANCE_LAW.md");
        var orientationPath = Path.Combine(lineRoot, "docs", "CME_TRUTH_SEEKING_ORIENTATION_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var orientationText = File.ReadAllText(orientationPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("mathematical balance law", lawText, StringComparison.Ordinal);
        Assert.Contains("center_coherence_score", lawText, StringComparison.Ordinal);
        Assert.Contains("evidence_support_score", lawText, StringComparison.Ordinal);
        Assert.Contains("maintenance_cost", lawText, StringComparison.Ordinal);
        Assert.Contains("uncertainty_bounds", lawText, StringComparison.Ordinal);
        Assert.Contains("CmeTruthSeekingBalanceTransitionReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("The system must be able to justify both maintaining and revising a claim under the same law", lawText, StringComparison.Ordinal);
        Assert.Contains("CME_TRUTH_SEEKING_BALANCE_LAW.md", orientationText, StringComparison.Ordinal);
        Assert.Contains("cme-truth-seeking-balance-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("CME truth-seeking balance law preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live balance telemetry and belief revision execution beyond CME truth-seeking balance", refinementText, StringComparison.Ordinal);
        Assert.Contains("`CME_TRUTH_SEEKING_BALANCE_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static CmeTruthSeekingBalanceTransitionRequest CreateTransitionRequest()
    {
        return new CmeTruthSeekingBalanceTransitionRequest(
            RequestHandle: "request://cme-truth-balance/session-a",
            OrientationReceipt: CreateOrientationReceipt(),
            PriorState: CreateState(
                "state://claim/prior",
                center: 0.55m,
                correction: 0.55m,
                cost: 0.50m,
                humility: 0.50m),
            ProposedState: CreateState(
                "state://claim/proposed",
                center: 0.60m,
                correction: 0.60m,
                cost: 0.55m,
                humility: 0.55m),
            TriggerKind: CmeTruthSeekingTransitionTriggerKind.EvidenceUpdate,
            JustificationTraceHandles:
            [
                "trace://truth-balance/session-a"
            ],
            MaintenanceJustificationHandles:
            [
                "justify://maintain/continuity-visible"
            ],
            RevisionJustificationHandles:
            [
                "justify://revise/evidence-visible"
            ],
            ContradictionReceiptHandles:
            [
                "receipt://contradiction/session-a"
            ],
            PreserveClaimSolelyForCoherence: false,
            ScopeExpandedWithoutUncertaintyBounds: false,
            ReEvaluationScheduled: true,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 45, 00, TimeSpan.Zero));
    }

    private static CmeTruthSeekingOrientationStateRecord CreateState(
        string stateHandle,
        decimal center,
        decimal correction,
        decimal cost,
        decimal humility)
    {
        return new CmeTruthSeekingOrientationStateRecord(
            StateHandle: stateHandle,
            ClaimHandle: "claim://truth/revisable-under-evidence",
            CenterCoherenceScore: center,
            EvidenceSupportScore: correction,
            MaintenanceCostScore: cost,
            UncertaintyBoundScore: humility,
            EngramLinkHandles:
            [
                "engram://self/continuity"
            ],
            EvidenceBasisHandles:
            [
                "evidence://receipted/correction-pressure"
            ],
            TensionHandles:
            [
                "tension://claim/revision-cost"
            ],
            UnknownScopeHandles:
            [
                "unknown://bounded/scope"
            ],
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 40, 00, TimeSpan.Zero));
    }

    private static CmeTruthSeekingOrientationReceipt CreateOrientationReceipt()
    {
        return new CmeTruthSeekingOrientationReceipt(
            ReceiptHandle: "receipt://cme-truth-orientation/session-a",
            RequestHandle: "request://cme-truth-orientation/session-a",
            FoundingBundleReceiptHandle: "receipt://cme-founding-bundle/session-a",
            OrientationKind: CmeTruthSeekingOrientationKind.OrientationBalanced,
            Disposition: CmeTruthSeekingOrientationDispositionKind.Attested,
            DeclaredTruthOrientationHandle: "truth-orientation://maximally-truth-seeking/session-a",
            DeclaredMoralCenterHandle: "moral-center://bounded-integrity/session-a",
            MaintainedTruthClaimHandles:
            [
                "claim://truth/revisable-under-evidence"
            ],
            CostSurfaceHandles:
            [
                "cost://continuity-burden/maintained-claim"
            ],
            RevisionEvidenceHandles:
            [
                "evidence://receipted/correction-pressure"
            ],
            AdmissibleDoubtHandles:
            [
                "doubt://bounded-knowing"
            ],
            FoundingBundleRecognized: true,
            CenterDeclared: true,
            CostSurfaceExposed: true,
            CorrectionPathAvailable: true,
            HumilityPreserved: true,
            DriftSignalDetected: false,
            FixationSignalDetected: false,
            IdentityCoherenceOnlyPreservationDetected: false,
            IdentityCoherenceOnlyPreservationRefused: false,
            LawfulRevisionPermitted: true,
            ContinuityPreservedThroughRevision: true,
            TruthSeekingOrientationAttested: true,
            CmeMintingWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            CandidateOnly: true,
            ConstraintCodes: ["cme-truth-orientation-balanced-attested"],
            ReasonCode: "cme-truth-orientation-balanced-attested",
            LawfulBasis: "test truth orientation basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 19, 35, 00, TimeSpan.Zero));
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
