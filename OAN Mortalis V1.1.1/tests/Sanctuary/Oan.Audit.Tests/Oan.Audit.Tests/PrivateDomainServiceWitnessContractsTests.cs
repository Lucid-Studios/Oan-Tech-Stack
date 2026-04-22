namespace San.Audit.Tests;

using San.Common;

public sealed class PrivateDomainServiceWitnessContractsTests
{
    [Fact]
    public void PrivateDomainServiceWitness_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrivateDomainServiceWitnessKind.RegionalPosture,
                PrivateDomainServiceWitnessKind.LocalServiceWitness,
                PrivateDomainServiceWitnessKind.PrivateDomainWitness,
                PrivateDomainServiceWitnessKind.WitnessWithheld
            ],
            Enum.GetValues<PrivateDomainServiceWitnessKind>());

        Assert.Equal(
            [
                PrivateDomainServiceOperationDisposition.Attested,
                PrivateDomainServiceOperationDisposition.Deferred,
                PrivateDomainServiceOperationDisposition.Refused
            ],
            Enum.GetValues<PrivateDomainServiceOperationDisposition>());

        Assert.Equal(
            [
                PrivateDomainServiceAxisKind.Actor,
                PrivateDomainServiceAxisKind.Action,
                PrivateDomainServiceAxisKind.Instrument,
                PrivateDomainServiceAxisKind.Method,
                PrivateDomainServiceAxisKind.Locality,
                PrivateDomainServiceAxisKind.StandingContext,
                PrivateDomainServiceAxisKind.ContinuityBurden
            ],
            Enum.GetValues<PrivateDomainServiceAxisKind>());
    }

    [Fact]
    public void Accepted_Domain_With_All_Axes_Attests_Private_Domain_Relation()
    {
        var receipt = PrivateDomainServiceWitnessEvaluator.Evaluate(
            CreateWitnessRequest(),
            "receipt://private-domain-service/session-a");

        Assert.Equal(PrivateDomainServiceWitnessKind.PrivateDomainWitness, receipt.WitnessKind);
        Assert.Equal(PrivateDomainServiceOperationDisposition.Attested, receipt.Disposition);
        Assert.True(receipt.ActorAxisPresent);
        Assert.True(receipt.ActionAxisPresent);
        Assert.True(receipt.InstrumentAxisPresent);
        Assert.True(receipt.MethodAxisPresent);
        Assert.True(receipt.LocalityAxisPresent);
        Assert.True(receipt.StandingContextAxisPresent);
        Assert.True(receipt.ContinuityBurdenAxisPresent);
        Assert.True(receipt.RelationAttested);
        Assert.True(receipt.ActionExecutionWithheld);
        Assert.True(receipt.CradleLocalGovernanceEnactmentWithheld);
        Assert.True(receipt.CustodialMemoryOnly);
        Assert.True(receipt.CandidateOnly);
        Assert.Equal("record://domain-role/session-a", receipt.DomainAdmissionRecordHandle);
        Assert.Contains("private-domain-service-private-relation-attested", receipt.ConstraintCodes);
        Assert.Equal("private-domain-service-private-relation-attested", receipt.ReasonCode);
    }

    [Fact]
    public void Missing_Private_Standing_Or_Burden_Leaves_Local_Service_Witness_Deferred()
    {
        var request = CreateWitnessRequest() with
        {
            StandingContextHandle = "",
            ContinuityBurdenHandles = []
        };

        var receipt = PrivateDomainServiceWitnessEvaluator.Evaluate(
            request,
            "receipt://private-domain-service/local-only");

        Assert.Equal(PrivateDomainServiceWitnessKind.LocalServiceWitness, receipt.WitnessKind);
        Assert.Equal(PrivateDomainServiceOperationDisposition.Deferred, receipt.Disposition);
        Assert.False(receipt.StandingContextAxisPresent);
        Assert.False(receipt.ContinuityBurdenAxisPresent);
        Assert.False(receipt.RelationAttested);
        Assert.Contains("private-domain-service-standing-context-axis-missing", receipt.ConstraintCodes);
        Assert.Contains("private-domain-service-continuity-burden-axis-missing", receipt.ConstraintCodes);
        Assert.Equal("private-domain-service-private-axis-incomplete", receipt.ReasonCode);
    }

    [Fact]
    public void Non_Accepted_Domain_Admission_Withholds_Private_Domain_Witness()
    {
        var request = CreateWitnessRequest() with
        {
            DomainAdmissionRecord = CreateDomainAdmissionRecord(DomainRoleAdmissionDecisionKind.Defer)
        };

        var receipt = PrivateDomainServiceWitnessEvaluator.Evaluate(
            request,
            "receipt://private-domain-service/no-domain");

        Assert.Equal(PrivateDomainServiceWitnessKind.WitnessWithheld, receipt.WitnessKind);
        Assert.Equal(PrivateDomainServiceOperationDisposition.Deferred, receipt.Disposition);
        Assert.False(receipt.RelationAttested);
        Assert.Contains("private-domain-service-domain-admission-not-accepted", receipt.ConstraintCodes);
        Assert.Equal("private-domain-service-domain-admission-not-accepted", receipt.ReasonCode);
    }

    [Fact]
    public void Action_Execution_Request_Is_Refused_By_Witness_Only_Seam()
    {
        var request = CreateWitnessRequest() with
        {
            ActionExecutionRequested = true
        };

        var receipt = PrivateDomainServiceWitnessEvaluator.Evaluate(
            request,
            "receipt://private-domain-service/action-refused");

        Assert.Equal(PrivateDomainServiceWitnessKind.WitnessWithheld, receipt.WitnessKind);
        Assert.Equal(PrivateDomainServiceOperationDisposition.Refused, receipt.Disposition);
        Assert.True(receipt.ActionExecutionWithheld);
        Assert.True(receipt.CradleLocalGovernanceEnactmentWithheld);
        Assert.False(receipt.RelationAttested);
        Assert.Contains("private-domain-service-action-execution-refused", receipt.ConstraintCodes);
        Assert.Equal("private-domain-service-action-execution-refused", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Private_Domain_Service_Witness_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md");
        var domainLawPath = Path.Combine(lineRoot, "docs", "DOMAIN_AND_ROLE_ADMISSION_LAW.md");
        var procLawPath = Path.Combine(lineRoot, "docs", "PROC_GROUNDED_ACTION_AND_TRACE_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");
        var baselinePath = Path.Combine(lineRoot, "docs", "SESSION_BODY_STABILIZATION_BASELINE.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var domainLawText = File.ReadAllText(domainLawPath);
        var procLawText = File.ReadAllText(procLawPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);
        var baselineText = File.ReadAllText(baselinePath);

        Assert.Contains("who did what with what and how within where and who", lawText, StringComparison.Ordinal);
        Assert.Contains("structured accountability grammar", lawText, StringComparison.Ordinal);
        Assert.Contains("custodial memory with lawful operational trace", lawText, StringComparison.Ordinal);
        Assert.Contains("attest relation, not action execution", lawText, StringComparison.Ordinal);
        Assert.Contains("PrivateDomainServiceWitnessReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md", domainLawText, StringComparison.Ordinal);
        Assert.Contains("PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md", procLawText, StringComparison.Ordinal);
        Assert.Contains("private-domain-service-witness-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Private-domain service witness preserved", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live action execution and cradle-local governance enactment beyond private-domain service witness", refinementText, StringComparison.Ordinal);
        Assert.Contains("`PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md`", baselineText, StringComparison.Ordinal);
    }

    private static PrivateDomainServiceWitnessRequest CreateWitnessRequest()
    {
        return new PrivateDomainServiceWitnessRequest(
            RequestHandle: "request://private-domain-service/session-a",
            DomainAdmissionRecord: CreateDomainAdmissionRecord(DomainRoleAdmissionDecisionKind.Accept),
            ActorHandle: "actor://operator/steward",
            ActionHandle: "action://service/provenance-attestation",
            InstrumentHandle: "instrument://service/private-domain-witness",
            MethodHandle: "method://custodial-attestation",
            LocalityHandle: "locality://site/private-domain/session-a",
            StandingContextHandle: "standing://first-prime-pre-role/session-a",
            ContinuityBurdenHandles:
            [
                "burden://preserve-first-prime-standing",
                "burden://preserve-domain-admission-record"
            ],
            ResultReceiptHandles:
            [
                "receipt://domain-role/session-a"
            ],
            ActionExecutionRequested: false,
            CradleLocalGovernanceEnactmentRequested: false,
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 18, 00, 00, TimeSpan.Zero));
    }

    private static DomainAdmissionRecord CreateDomainAdmissionRecord(
        DomainRoleAdmissionDecisionKind decision)
    {
        var accepted = decision == DomainRoleAdmissionDecisionKind.Accept;

        return new DomainAdmissionRecord(
            RecordHandle: "record://domain-role/session-a",
            OfferHandle: "offer://domain-role/session-a",
            AssessmentHandle: "assessment://domain-role/session-a",
            Decision: decision,
            LegalFoundationHandle: accepted ? "legal-foundation://lucid/root-family" : string.Empty,
            AcceptedDomainHandles: accepted ? ["domain://private/service"] : [],
            AcceptedRoleHandles: accepted ? ["role://bounded-service-witness"] : [],
            AuthorityScopeHandles: accepted ? ["scope://attest-provenance"] : [],
            ExplicitExclusionHandles:
            [
                "exclude://action-execution",
                "exclude://cradle-local-governance-enactment",
                "exclude://mother-father-origin-authority"
            ],
            ContinuityBurdenHandles: accepted ? ["burden://preserve-domain-admission-record"] : [],
            RevocationConditionHandles: ["revoke://scope-overclaimed"],
            StandingOverwritten: false,
            MotherFatherOriginAuthorityWithheld: true,
            CradleLocalGoverningSurfaceStillWithheld: true,
            ImplicitDomainPromotionRefused: true,
            ReceiptRequiredForAllOutcomes: true,
            CandidateOnly: true,
            ConstraintCodes: accepted
                ? ["domain-role-admission-accepted"]
                : ["domain-role-admission-deferred"],
            ReasonCode: accepted
                ? "domain-role-admission-accepted"
                : "domain-role-admission-deferred",
            LawfulBasis: "test domain admission basis",
            TimestampUtc: new DateTimeOffset(2026, 04, 15, 17, 55, 00, TimeSpan.Zero));
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
