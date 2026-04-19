using San.Common;
using San.Runtime.Materialization;

namespace San.Audit.Tests;

public sealed class GovernedSeedPostExecutionOperationalActionPacketTests
{
    [Fact]
    public void Materialized_Packet_Preserves_Execution_Identity_And_Commit_Receipt()
    {
        var service = new GovernedSeedPostExecutionOperationalActionPacketMaterializationService();
        var executionPacket = CreateExecutionPacket();
        var serviceEffectAssessment = CreateServiceEffectAssessment(
            executionPacket.PacketHandle,
            executionPacket.CandidateId,
            true);
        var commitIntent = CreateCommitIntent(
            executionPacket.PacketHandle,
            executionPacket.CandidateId,
            true);
        var operationalActionCommitAssessment = CreateOperationalActionCommitAssessment(
            executionPacket.PacketHandle,
            executionPacket.CandidateId,
            true,
            true);
        var commitReceipt = CreateCommitReceipt(
            executionPacket.PacketHandle,
            executionPacket.CandidateId,
            true,
            true);
        var unifiedAssessment = CreateOperationalActionAssessment(
            executionPacket.PacketHandle,
            executionPacket.CandidateId,
            GovernedSeedOperationalActionDisposition.OperationalActionCommitted,
            true,
            true);
        var receipt = CreateOperationalActionReceipt(
            executionPacket.PacketHandle,
            executionPacket.CandidateId,
            GovernedSeedOperationalActionDisposition.OperationalActionCommitted,
            true,
            true);

        var packet = service.Materialize(
            executionPacket,
            serviceEffectAssessment,
            commitIntent,
            operationalActionCommitAssessment,
            commitReceipt,
            unifiedAssessment,
            receipt);

        Assert.Equal(executionPacket.CandidateId, packet.CandidateId);
        Assert.Equal(executionPacket.PacketHandle, packet.PostParticipationExecutionPacket.PacketHandle);
        Assert.Equal(commitReceipt.ReceiptHandle, packet.CommitReceipt.ReceiptHandle);
        Assert.Equal(GovernedSeedOperationalActionDisposition.OperationalActionCommitted, packet.PostExecutionOperationalActionAssessment.Disposition);
        Assert.Equal(packet.PostExecutionOperationalActionAssessment.Disposition, packet.PostExecutionOperationalActionReceipt.Disposition);
        Assert.True(packet.ServiceEffectAssessment.ServiceEffectAuthorized);
        Assert.True(packet.OperationalActionCommitAssessment.OperationalActionCommitted);
    }

    [Fact]
    public void Materialize_Rejects_Mismatched_Execution_Packet_Identity()
    {
        var service = new GovernedSeedPostExecutionOperationalActionPacketMaterializationService();
        var executionPacket = CreateExecutionPacket();

        var error = Assert.Throws<InvalidOperationException>(() => service.Materialize(
            executionPacket,
            CreateServiceEffectAssessment("post-participation-execution-packet://other", executionPacket.CandidateId, true),
            CreateCommitIntent(executionPacket.PacketHandle, executionPacket.CandidateId, true),
            CreateOperationalActionCommitAssessment(executionPacket.PacketHandle, executionPacket.CandidateId, true, true),
            CreateCommitReceipt(executionPacket.PacketHandle, executionPacket.CandidateId, true, true),
            CreateOperationalActionAssessment(executionPacket.PacketHandle, executionPacket.CandidateId, GovernedSeedOperationalActionDisposition.OperationalActionCommitted, true, true),
            CreateOperationalActionReceipt(executionPacket.PacketHandle, executionPacket.CandidateId, GovernedSeedOperationalActionDisposition.OperationalActionCommitted, true, true)));

        Assert.Contains("consistent execution packet identity", error.Message);
    }

    private static GovernedSeedPostParticipationExecutionPacket CreateExecutionPacket()
    {
        var service = new GovernedSeedPostParticipationExecutionPacketMaterializationService();
        var participationPacket = CreateParticipationPacket();

        return service.Materialize(
            participationPacket,
            CreateServiceBehaviorAssessment(participationPacket.PacketHandle, participationPacket.CandidateId, true, true),
            CreateExecutionAuthorizationAssessment(participationPacket.PacketHandle, participationPacket.CandidateId, true, true),
            CreateExecutionAssessment(
                participationPacket.PacketHandle,
                participationPacket.CandidateId,
                GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized,
                true,
                true),
            CreateExecutionReceipt(
                participationPacket.PacketHandle,
                participationPacket.CandidateId,
                GovernedSeedExecutionAuthorizationDisposition.ExecutionAuthorized,
                true,
                true));
    }

    private static GovernedSeedPostAdmissionParticipationPacket CreateParticipationPacket()
    {
        var service = new GovernedSeedPostAdmissionParticipationPacketMaterializationService();
        var admissionBindingPacket = CreateAdmissionBindingPacket();

        return service.Materialize(
            admissionBindingPacket,
            CreateDomainOccupancyAssessment(admissionBindingPacket.PacketHandle, admissionBindingPacket.CandidateId, true),
            CreateRoleParticipationAssessment(admissionBindingPacket.PacketHandle, admissionBindingPacket.CandidateId, true, false),
            CreateParticipationAssessment(
                admissionBindingPacket.PacketHandle,
                admissionBindingPacket.CandidateId,
                GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized,
                true,
                false),
            CreateParticipationReceipt(
                admissionBindingPacket.PacketHandle,
                admissionBindingPacket.CandidateId,
                GovernedSeedPostAdmissionParticipationDisposition.DomainOccupancyAuthorized,
                true,
                false));
    }

    private static GovernedSeedDomainAdmissionRoleBindingPacket CreateAdmissionBindingPacket()
    {
        var service = new GovernedSeedDomainAdmissionRoleBindingPacketMaterializationService();
        var gatingPacket = CreateGatingPacket();

        return service.Materialize(
            gatingPacket,
            CreateDomainAdmissionAssessment(gatingPacket.PacketHandle, gatingPacket.CandidateId, true, false),
            CreateRoleBindingAssessment(gatingPacket.PacketHandle, gatingPacket.CandidateId, true, false),
            CreateAdmissionBindingAssessment(
                gatingPacket.PacketHandle,
                gatingPacket.CandidateId,
                GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
                true,
                false),
            CreateAdmissionBindingReceipt(
                gatingPacket.PacketHandle,
                gatingPacket.CandidateId,
                GovernedSeedDomainAdmissionRoleBindingDisposition.DomainAdmittedRolePending,
                true,
                false));
    }

    private static GovernedSeedDomainRoleGatingPacket CreateGatingPacket()
    {
        var service = new GovernedSeedDomainRoleGatingPacketMaterializationService();
        var preDomainPacket = CreatePreDomainPacket();

        return service.Materialize(
            preDomainPacket,
            CreateDomainEligibilityAssessment(preDomainPacket.PacketHandle, preDomainPacket.CandidateId, true),
            CreateRoleEligibilityAssessment(preDomainPacket.PacketHandle, preDomainPacket.CandidateId, true, true),
            CreateDomainRoleGatingAssessment(
                preDomainPacket.PacketHandle,
                preDomainPacket.CandidateId,
                GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                true,
                true),
            CreateDomainRoleGatingReceipt(
                preDomainPacket.PacketHandle,
                preDomainPacket.CandidateId,
                GovernedSeedDomainRoleGatingDisposition.DomainAndRoleAdmissible,
                true,
                true));
    }

    private static GovernedSeedPreDomainGovernancePacket CreatePreDomainPacket()
    {
        var service = new GovernedSeedPreDomainGovernancePacketMaterializationService();
        return service.Materialize(
            CreatePrimeSeedReceipt(),
            CreateBoundaryReceipt(),
            CreateHoldingReceipt(),
            CreateFormOrCleaveAssessment(),
            CreateSeparationReceipt(),
            CreateDuplexReceipt(),
            CreateAdmissionGateReceipt(),
            CreateHostLoopReceipt());
    }

    private static GovernedSeedServiceEffectAssessment CreateServiceEffectAssessment(
        string packetHandle,
        string candidateId,
        bool serviceEffectAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            ExecutionAuthorized: true,
            ServiceBehaviorAuthorized: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            ExplicitScopePreserved: true,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            Summary: "service-effect");

    private static GovernedSeedCommitIntent CreateCommitIntent(
        string packetHandle,
        string candidateId,
        bool commitIntentPresent) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            ServiceEffectAuthorized: true,
            ExecutionAuthorized: true,
            ExplicitCommitRequested: true,
            IrreversibleEffectRequested: false,
            PropagationRequested: false,
            CommitIntentPresent: commitIntentPresent,
            Summary: "commit-intent");

    private static GovernedSeedOperationalActionCommitAssessment CreateOperationalActionCommitAssessment(
        string packetHandle,
        string candidateId,
        bool commitReady,
        bool operationalActionCommitted) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            ExecutionAuthorized: true,
            ServiceEffectAuthorized: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            ExplicitScopePreserved: true,
            ExplicitCommitRequested: true,
            CommitReady: commitReady,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: "commit-assessment");

    private static GovernedSeedCommitReceipt CreateCommitReceipt(
        string packetHandle,
        string candidateId,
        bool commitReady,
        bool operationalActionCommitted) =>
        new(
            ReceiptHandle: "post-execution-commit://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            CommitReady: commitReady,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: "commit-receipt");

    private static GovernedSeedPostExecutionOperationalActionAssessment CreateOperationalActionAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedOperationalActionDisposition disposition,
        bool serviceEffectAuthorized,
        bool operationalActionCommitted) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: "operational-action-assessment");

    private static GovernedSeedPostExecutionOperationalActionReceipt CreateOperationalActionReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedOperationalActionDisposition disposition,
        bool serviceEffectAuthorized,
        bool operationalActionCommitted) =>
        new(
            ReceiptHandle: "post-execution-operational-action://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            OperationalActionCommitted: operationalActionCommitted,
            Summary: "operational-action-receipt");

    private static GovernedSeedServiceBehaviorAssessment CreateServiceBehaviorAssessment(
        string packetHandle,
        string candidateId,
        bool serviceBehaviorAuthorized,
        bool executionAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            OccupancyAuthorized: true,
            ParticipationAuthorized: executionAuthorized,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            ServiceScopeLawful: true,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            Summary: "service-behavior");

    private static GovernedSeedExecutionAuthorizationAssessment CreateExecutionAuthorizationAssessment(
        string packetHandle,
        string candidateId,
        bool serviceBehaviorAuthorized,
        bool executionAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            OccupancyAuthorized: serviceBehaviorAuthorized,
            ParticipationAuthorized: executionAuthorized,
            ExecutionStructurePresent: executionAuthorized,
            ExplicitScopePreserved: true,
            RoleBearingExecutionRequested: executionAuthorized,
            ExecutionAuthorized: executionAuthorized,
            Summary: "execution-authorization");

    private static GovernedSeedPostParticipationExecutionAssessment CreateExecutionAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedExecutionAuthorizationDisposition disposition,
        bool serviceBehaviorAuthorized,
        bool executionAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            ExecutionAuthorized: executionAuthorized,
            Summary: "unified");

    private static GovernedSeedPostParticipationExecutionReceipt CreateExecutionReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedExecutionAuthorizationDisposition disposition,
        bool serviceBehaviorAuthorized,
        bool executionAuthorized) =>
        new(
            ReceiptHandle: "post-participation-execution://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            ServiceBehaviorAuthorized: serviceBehaviorAuthorized,
            ExecutionAuthorized: executionAuthorized,
            Summary: "receipt");

    private static GovernedSeedDomainOccupancyAssessment CreateDomainOccupancyAssessment(
        string packetHandle,
        string candidateId,
        bool occupancyAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            DomainAdmissionGranted: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            OccupancyStructurePresent: occupancyAuthorized,
            OccupancyAuthorized: occupancyAuthorized,
            Summary: "domain-occupancy");

    private static GovernedSeedRoleParticipationAssessment CreateRoleParticipationAssessment(
        string packetHandle,
        string candidateId,
        bool occupancyAuthorized,
        bool roleParticipationAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            DomainAdmissionGranted: occupancyAuthorized,
            RoleBound: roleParticipationAuthorized,
            RoleLawfulWithinDomain: true,
            ResponsibilityBindableAtRoleScope: true,
            ScopeExpansionDetected: false,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "role-participation");

    private static GovernedSeedPostAdmissionParticipationAssessment CreateParticipationAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedPostAdmissionParticipationDisposition disposition,
        bool occupancyAuthorized,
        bool roleParticipationAuthorized) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            DomainAdmissionGranted: true,
            OccupancyAuthorized: occupancyAuthorized,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "participation");

    private static GovernedSeedPostAdmissionParticipationReceipt CreateParticipationReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedPostAdmissionParticipationDisposition disposition,
        bool occupancyAuthorized,
        bool roleParticipationAuthorized) =>
        new(
            ReceiptHandle: "post-admission-participation://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            OccupancyAuthorized: occupancyAuthorized,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "receipt");

    private static GovernedSeedDomainAdmissionAssessment CreateDomainAdmissionAssessment(
        string packetHandle,
        string candidateId,
        bool domainAdmissionGranted,
        bool crypticAuthorityBleedDetected) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            CrypticAuthorityBleedDetected: crypticAuthorityBleedDetected,
            StandingConsistent: true,
            DomainEligibilitySatisfied: true,
            BurdenAttributableAtDomainScope: true,
            DomainAdmissionGranted: domainAdmissionGranted,
            Summary: "domain-admission");

    private static GovernedSeedRoleBindingAssessment CreateRoleBindingAssessment(
        string packetHandle,
        string candidateId,
        bool domainAdmissionGranted,
        bool roleBound) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleRelevantStructurePresent: roleBound,
            ResponsibilityBindableAtRoleScope: true,
            RoleLawfulWithinDomain: true,
            RoleBound: roleBound,
            Summary: "role-binding");

    private static GovernedSeedDomainAdmissionRoleBindingAssessment CreateAdmissionBindingAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainAdmissionRoleBindingDisposition disposition,
        bool domainAdmissionGranted,
        bool roleBound) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            CrypticAuthorityBleedDetected: false,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            Summary: "unified");

    private static GovernedSeedDomainAdmissionRoleBindingReceipt CreateAdmissionBindingReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainAdmissionRoleBindingDisposition disposition,
        bool domainAdmissionGranted,
        bool roleBound) =>
        new(
            ReceiptHandle: "domain-admission-role-binding://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            DomainAdmissionGranted: domainAdmissionGranted,
            RoleBound: roleBound,
            Summary: "receipt");

    private static GovernedSeedDomainEligibilityAssessment CreateDomainEligibilityAssessment(
        string packetHandle,
        string candidateId,
        bool packetComplete) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: packetComplete,
            PrimeAdmissionStructurePresent: true,
            CrypticAuthorityBleedDetected: false,
            ForwardMotionSupported: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            DomainEligible: true,
            Summary: "domain");

    private static GovernedSeedRoleEligibilityAssessment CreateRoleEligibilityAssessment(
        string packetHandle,
        string candidateId,
        bool domainEligible,
        bool roleEligible) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            DomainEligible: domainEligible,
            RoleRelevantStructurePresent: roleEligible,
            ResponsibilityAttributable: true,
            RoleEligible: roleEligible,
            Summary: "role");

    private static GovernedSeedDomainRoleGatingAssessment CreateDomainRoleGatingAssessment(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainRoleGatingDisposition disposition,
        bool domainEligible,
        bool roleEligible) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            PacketComplete: true,
            CrypticAuthorityBleedDetected: false,
            StandingConsistent: true,
            DomainEligible: domainEligible,
            RoleEligible: roleEligible,
            Summary: "gating");

    private static GovernedSeedDomainRoleGatingReceipt CreateDomainRoleGatingReceipt(
        string packetHandle,
        string candidateId,
        GovernedSeedDomainRoleGatingDisposition disposition,
        bool domainEligible,
        bool roleEligible) =>
        new(
            ReceiptHandle: "domain-role-gating://packet/a",
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            Disposition: disposition,
            DomainEligible: domainEligible,
            RoleEligible: roleEligible,
            Summary: "receipt");

    private static PrimeSeedStateReceipt CreatePrimeSeedReceipt() =>
        new(
            ReceiptHandle: "prime-seed://packet/a",
            RequestHandle: "prime-seed-request://packet/a",
            FirstPrimeReceiptHandle: "first-prime://packet/a",
            PrimeRetainedRecordHandle: "prime-retained://packet/a",
            StableOneHandle: "stable-one://packet/a",
            SeedState: PrimeSeedStateKind.PrimeSeedPreDomainStanding,
            SeedSourceHandle: "seed-source://packet/a",
            SeedCarrierHandle: "seed-carrier://packet/a",
            SeedContinuityHandle: "seed-continuity://packet/a",
            SeedIntegrityHandle: "seed-integrity://packet/a",
            SeedEvidenceHandles: ["evidence://packet/a"],
            FirstPrimePreRoleStandingPresent: true,
            StableOnePresent: true,
            PrimeRetainedStandingPresent: true,
            SeedSourcePresent: true,
            SeedCarrierPresent: true,
            SeedContinuityPresent: true,
            SeedIntegrityPresent: true,
            DomainAdmissionWithheld: true,
            LawfullyBondedDomainIntegrationWithheld: true,
            CmeFoundingWithheld: true,
            CmeMintingWithheld: true,
            RuntimePersonaWithheld: true,
            RoleEnactmentWithheld: true,
            ActionAuthorityWithheld: true,
            MotherFatherDomainRoleApplicationWithheld: true,
            CradleLocalGoverningSurfaceWithheld: true,
            PrimeClosureStillWithheld: true,
            CandidateOnly: true,
            ConstraintCodes: ["candidate-only"],
            ReasonCode: "prime-seed-state-pre-domain-standing",
            LawfulBasis: "pre-domain standing.",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedCandidateBoundaryReceipt CreateBoundaryReceipt() =>
        new(
            ReceiptHandle: "candidate-boundary://packet/a",
            CandidateId: "candidate://packet/a",
            SourceType: GovernedSeedCandidateSourceType.SyntheticTest,
            SourceChannel: "audit",
            ObservedAtUtc: DateTimeOffset.UtcNow,
            ContainsAuthorityBearingFields: false,
            CandidateProposalCount: 1,
            HoldingMutationProposalCount: 0,
            ResonanceObservationCount: 1,
            DescendantProposalCount: 1,
            CollapseSuggestionCount: 0,
            Summary: "candidate-only");

    private static GovernedSeedCrypticHoldingInspectionReceipt CreateHoldingReceipt() =>
        new(
            ReceiptHandle: "holding://packet/a",
            FormationReceiptHandle: "formation://packet/a",
            ListeningFrameHandle: "listening://packet/a",
            CompassPacketHandle: "compass://packet/a",
            HoldingEntries: [],
            CandidateOnly: true,
            InspectionInfluenceOnly: true,
            PromotionAuthorityWithheld: true,
            ReasonCode: "holding",
            LawfulBasis: "holding",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedFormOrCleaveAssessment CreateFormOrCleaveAssessment() =>
        new(
            AssessmentHandle: "form-or-cleave://packet/a",
            FormationReceiptHandle: "formation://packet/a",
            ListeningFrameHandle: "listening://packet/a",
            CompassPacketHandle: "compass://packet/a",
            HoldingReceiptHandle: "holding://packet/a",
            Disposition: GovernedSeedFormOrCleaveDispositionKind.Form,
            CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            DescendantCandidates: [],
            CandidateOnly: true,
            ReasonCode: "formed",
            LawfulBasis: "formed",
            TimestampUtc: DateTimeOffset.UtcNow);

    private static GovernedSeedCandidateSeparationReceipt CreateSeparationReceipt() =>
        new(
            ReceiptHandle: "candidate-separation://packet/a",
            CandidateId: "candidate://packet/a",
            SeparationSucceeded: true,
            PrimeMaterialCount: 2,
            CrypticMaterialCount: 1,
            CrypticAuthorityBleedDetected: false,
            Summary: "separated");

    private static PrimeCrypticDuplexGovernanceReceipt CreateDuplexReceipt() =>
        new(
            ReceiptHandle: "duplex://packet/a",
            CandidateId: "candidate://packet/a",
            PrimeSurfaceEstablished: true,
            CrypticSurfaceEstablished: true,
            Summary: "duplex");

    private static PrimeSeedPreDomainAdmissionGateReceipt CreateAdmissionGateReceipt() =>
        new(
            ReceiptHandle: "admission://packet/a",
            CandidateId: "candidate://packet/a",
            Disposition: PrimeSeedPreDomainAdmissionDisposition.PrepareForDomainRoleGate,
            DomainEligible: true,
            RoleEligible: true,
            Summary: "admission");

    private static GovernedSeedPreDomainHostLoopReceipt CreateHostLoopReceipt() =>
        new(
            ReceiptHandle: "host-loop://packet/a",
            FirstPrimeReceiptHandle: "first-prime://packet/a",
            PrimeSeedReceiptHandle: "prime-seed://packet/a",
            PreDomainGovernancePacketHandle: null,
            CandidateBoundaryReceiptHandle: "candidate-boundary://packet/a",
            CrypticHoldingInspectionHandle: "holding://packet/a",
            FormOrCleaveAssessmentHandle: "form-or-cleave://packet/a",
            CandidateSeparationReceiptHandle: "candidate-separation://packet/a",
            DuplexGovernanceReceiptHandle: "duplex://packet/a",
            AdmissionGateReceiptHandle: "admission://packet/a",
            CarryDisposition: GovernedSeedCarryDispositionKind.Carry,
            CollapseDisposition: GovernedSeedCollapseDispositionKind.None,
            CandidateHandles: ["candidate://packet/a"],
            SeedReady: true,
            CandidateOnly: true,
            DomainAdmissionWithheld: true,
            ActionAuthorityWithheld: true,
            ReasonCode: "carry",
            LawfulBasis: "bounded carry",
            TimestampUtc: DateTimeOffset.UtcNow);
}
