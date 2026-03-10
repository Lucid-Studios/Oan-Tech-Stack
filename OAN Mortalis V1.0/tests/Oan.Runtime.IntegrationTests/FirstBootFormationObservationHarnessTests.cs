using AgentiCore.Observation;
using Oan.Common;
using Oan.Cradle;

namespace Oan.Runtime.IntegrationTests;

public sealed class FirstBootFormationObservationHarnessTests
{
    [Fact]
    public async Task PersonalSolitary_BootClassification_IsObservedWithNoExpansionRights()
    {
        var observer = new InMemoryAgentiFormationObserver();
        var harness = CreateHarness(observer);

        var batch = await harness.ObserveAsync(
            BootClass.PersonalSolitary,
            ProtectedIntakeKind.HumanProtectedIntake,
            PrimeRevealMode.None);

        Assert.Equal(6, batch.Observations.Count);

        var boot = batch.Observations[0];
        Assert.Equal(AgentiFormationObservationStage.BootClassification, boot.Stage);
        Assert.Equal(BootClass.PersonalSolitary, boot.BootClass);
        Assert.Equal(BootActivationState.Classified, boot.ActivationState);
        Assert.Equal(ExpansionRights.None, boot.ExpansionRights);
        Assert.Equal(AgentiFormationOriginRuntime.OracleCSharp, boot.OriginRuntime);
        Assert.Equal(AgentiFormationObservationSource.FirstBootPolicy, boot.Source);
        Assert.False(boot.SubmissionEligible);

        Assert.Equal(batch.Observations, observer.Snapshot());
    }

    [Fact]
    public async Task CorporateGoverned_BootClassification_IsObservedWithNoExpansionRights()
    {
        var harness = CreateHarness();

        var batch = await harness.ObserveAsync(
            BootClass.CorporateGoverned,
            ProtectedIntakeKind.CorporateProtectedIntake,
            PrimeRevealMode.StructuralValidation);

        var boot = batch.Observations[0];
        Assert.Equal(AgentiFormationObservationStage.BootClassification, boot.Stage);
        Assert.Equal(BootClass.CorporateGoverned, boot.BootClass);
        Assert.Equal(BootActivationState.Classified, boot.ActivationState);
        Assert.Equal(ExpansionRights.None, boot.ExpansionRights);

        var intake = batch.Observations[1];
        Assert.Equal(AgentiFormationObservationStage.ProtectedIntakePosture, intake.Stage);
        Assert.Equal(PrimeRevealMode.StructuralValidation, intake.RevealMode);
        Assert.Contains("intake:CorporateProtectedIntake", intake.ObservationTags);
    }

    [Fact]
    public async Task GoverningOfficeSequence_IsStewardThenFatherThenMotherThenTriadicCrossWitness()
    {
        var harness = CreateHarness();

        var batch = await harness.ObserveAsync(
            BootClass.CorporateGoverned,
            ProtectedIntakeKind.CorporateProtectedIntake,
            PrimeRevealMode.None);

        Assert.Collection(
            batch.Observations.Select(observation => observation.Stage),
            stage => Assert.Equal(AgentiFormationObservationStage.BootClassification, stage),
            stage => Assert.Equal(AgentiFormationObservationStage.ProtectedIntakePosture, stage),
            stage => Assert.Equal(AgentiFormationObservationStage.GoverningOfficeFormation, stage),
            stage => Assert.Equal(AgentiFormationObservationStage.GoverningOfficeFormation, stage),
            stage => Assert.Equal(AgentiFormationObservationStage.GoverningOfficeFormation, stage),
            stage => Assert.Equal(AgentiFormationObservationStage.TriadicCrossWitness, stage));

        var offices = batch.Observations
            .Where(observation => observation.Stage == AgentiFormationObservationStage.GoverningOfficeFormation)
            .Select(observation => observation.Office)
            .ToArray();

        Assert.Equal(
            new InternalGoverningCmeOffice?[]
            {
                InternalGoverningCmeOffice.Steward,
                InternalGoverningCmeOffice.Father,
                InternalGoverningCmeOffice.Mother
            },
            offices);

        var triadic = batch.Observations[^1];
        Assert.Equal(AgentiFormationObservationStage.TriadicCrossWitness, triadic.Stage);
        Assert.Equal(BootActivationState.TriadicActive, triadic.ActivationState);
        Assert.Equal(ExpansionRights.None, triadic.ExpansionRights);
    }

    [Fact]
    public async Task MilestoneTwo_HarnessNeverYieldsExpansionEligible()
    {
        var harness = CreateHarness();

        var batch = await harness.ObserveAsync(
            BootClass.CorporateGoverned,
            ProtectedIntakeKind.CorporateProtectedIntake,
            PrimeRevealMode.MaskedSummary);

        Assert.DoesNotContain(batch.Observations, observation => observation.ActivationState == BootActivationState.ExpansionEligible);
        Assert.All(batch.Observations, observation =>
        {
            Assert.False(observation.SubmissionEligible);
            Assert.Equal(AgentiFormationClosureState.NotSubmitted, observation.ClosureState);
            Assert.Equal(AgentiFormationOriginRuntime.OracleCSharp, observation.OriginRuntime);
            Assert.Equal(AgentiFormationObservationSource.FirstBootPolicy, observation.Source);
            Assert.Equal(ExpansionRights.None, observation.ExpansionRights);
        });
    }

    private static FirstBootFormationObservationHarness CreateHarness(
        IAgentiFormationObserver? observer = null)
    {
        return new FirstBootFormationObservationHarness(
            new DefaultFirstBootGovernancePolicy(),
            observer);
    }
}
