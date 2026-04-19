using San.Common;
using San.Runtime.Materialization;
using San.Nexus.Control;

namespace San.Audit.Tests;

public sealed class GovernedSeedPostActionServiceEnactmentTests
{
    [Fact]
    public void Incomplete_Operational_Action_Packet_Is_Refused()
    {
        var service = new GovernedSeedPostActionServiceEnactmentService();
        var packet = CreateOperationalActionPacket();
        packet = packet with
        {
            PostExecutionOperationalActionReceipt = packet.PostExecutionOperationalActionReceipt with
            {
                PacketHandle = string.Empty
            }
        };

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedServiceEnactmentDisposition.Refuse, result.UnifiedAssessment.Disposition);
        Assert.False(result.EffectEmissionAssessment.PacketComplete);
    }

    [Fact]
    public void Effect_Authorized_But_Not_Committed_Yields_Effect_Emission_Authorized()
    {
        var service = new GovernedSeedPostActionServiceEnactmentService();
        var packet = CreateOperationalActionPacket(
            GovernedSeedOperationalActionDisposition.ServiceEffectAuthorized,
            serviceEffectAuthorized: true,
            operationalActionCommitted: false,
            irreversibleEffectRequested: false,
            propagationRequested: false);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedServiceEnactmentDisposition.EffectEmissionAuthorized, result.UnifiedAssessment.Disposition);
        Assert.True(result.EffectEmissionAssessment.EffectEmissionAuthorized);
        Assert.False(result.ServiceEnactmentCommitAssessment.ServiceEnactmentCommitted);
    }

    [Fact]
    public void Committed_Action_With_Propagation_Pending_Yields_Service_Enactment_Pending()
    {
        var service = new GovernedSeedPostActionServiceEnactmentService();
        var packet = CreateOperationalActionPacket(
            GovernedSeedOperationalActionDisposition.OperationalActionCommitted,
            serviceEffectAuthorized: true,
            operationalActionCommitted: true,
            irreversibleEffectRequested: false,
            propagationRequested: true);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedServiceEnactmentDisposition.ServiceEnactmentPending, result.UnifiedAssessment.Disposition);
        Assert.True(result.EffectEmissionAssessment.EffectEmissionAuthorized);
        Assert.False(result.ServiceEnactmentCommitAssessment.ServiceEnactmentCommitted);
    }

    [Fact]
    public void Fully_Clean_Operational_Action_Packet_Yields_Service_Enactment_Committed()
    {
        var service = new GovernedSeedPostActionServiceEnactmentService();
        var packet = CreateOperationalActionPacket(
            GovernedSeedOperationalActionDisposition.OperationalActionCommitted,
            serviceEffectAuthorized: true,
            operationalActionCommitted: true,
            irreversibleEffectRequested: false,
            propagationRequested: false);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedServiceEnactmentDisposition.ServiceEnactmentCommitted, result.UnifiedAssessment.Disposition);
        Assert.True(result.ServiceEnactmentCommitAssessment.EnactmentCommitReady);
        Assert.True(result.Receipt.ServiceEnactmentCommitted);
    }

    [Fact]
    public void Scope_Loss_Returns_To_Operational_Action_Pending()
    {
        var service = new GovernedSeedPostActionServiceEnactmentService();
        var packet = CreateOperationalActionPacket(
            GovernedSeedOperationalActionDisposition.OperationalActionCommitted,
            serviceEffectAuthorized: true,
            operationalActionCommitted: true,
            irreversibleEffectRequested: false,
            propagationRequested: false,
            explicitScopePreserved: false);

        var result = service.Evaluate(packet);

        Assert.Equal(GovernedSeedServiceEnactmentDisposition.ReturnToOperationalActionPending, result.UnifiedAssessment.Disposition);
        Assert.False(result.EffectEmissionAssessment.ExplicitScopePreserved);
        Assert.False(result.ServiceEnactmentCommitAssessment.ServiceEnactmentCommitted);
    }

    private static GovernedSeedPostExecutionOperationalActionPacket CreateOperationalActionPacket(
        GovernedSeedOperationalActionDisposition disposition = GovernedSeedOperationalActionDisposition.OperationalActionCommitted,
        bool serviceEffectAuthorized = true,
        bool operationalActionCommitted = true,
        bool irreversibleEffectRequested = false,
        bool propagationRequested = false,
        bool explicitScopePreserved = true)
    {
        var service = new GovernedSeedPostExecutionOperationalActionPacketMaterializationService();
        var executionPacket = CreateExecutionPacket();

        return service.Materialize(
            executionPacket,
            CreateServiceEffectAssessment(
                executionPacket.PacketHandle,
                executionPacket.CandidateId,
                serviceEffectAuthorized,
                explicitScopePreserved),
            CreateCommitIntent(
                executionPacket.PacketHandle,
                executionPacket.CandidateId,
                operationalActionCommitted,
                irreversibleEffectRequested,
                propagationRequested),
            CreateOperationalActionCommitAssessment(
                executionPacket.PacketHandle,
                executionPacket.CandidateId,
                operationalActionCommitted,
                operationalActionCommitted,
                explicitScopePreserved),
            CreateCommitReceipt(
                executionPacket.PacketHandle,
                executionPacket.CandidateId,
                operationalActionCommitted,
                operationalActionCommitted),
            CreateOperationalActionAssessment(
                executionPacket.PacketHandle,
                executionPacket.CandidateId,
                disposition,
                serviceEffectAuthorized,
                operationalActionCommitted),
            CreateOperationalActionReceipt(
                executionPacket.PacketHandle,
                executionPacket.CandidateId,
                disposition,
                serviceEffectAuthorized,
                operationalActionCommitted));
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
        bool serviceEffectAuthorized,
        bool explicitScopePreserved) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            ExecutionAuthorized: true,
            ServiceBehaviorAuthorized: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            ExplicitScopePreserved: explicitScopePreserved,
            ServiceEffectAuthorized: serviceEffectAuthorized,
            Summary: "service-effect");

    private static GovernedSeedCommitIntent CreateCommitIntent(
        string packetHandle,
        string candidateId,
        bool commitIntentPresent,
        bool irreversibleEffectRequested,
        bool propagationRequested) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            ServiceEffectAuthorized: true,
            ExecutionAuthorized: true,
            ExplicitCommitRequested: true,
            IrreversibleEffectRequested: irreversibleEffectRequested,
            PropagationRequested: propagationRequested,
            CommitIntentPresent: commitIntentPresent,
            Summary: "commit-intent");

    private static GovernedSeedOperationalActionCommitAssessment CreateOperationalActionCommitAssessment(
        string packetHandle,
        string candidateId,
        bool commitReady,
        bool operationalActionCommitted,
        bool explicitScopePreserved) =>
        new(
            PacketHandle: packetHandle,
            CandidateId: candidateId,
            PacketComplete: true,
            ExecutionAuthorized: true,
            ServiceEffectAuthorized: true,
            StandingConsistent: true,
            RevalidationConsistent: true,
            AttributionPreserved: true,
            ExplicitScopePreserved: explicitScopePreserved,
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
            Summary: "operational-action");

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
            RoleBearingExecutionRequested: false,
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
            Summary: "post-participation-execution");

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
            Summary: "post-participation-execution-receipt");

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
            OccupancyStructurePresent: true,
            OccupancyAuthorized: occupancyAuthorized,
            Summary: "occupancy");

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
            RoleLawfulWithinDomain: occupancyAuthorized,
            ResponsibilityBindableAtRoleScope: true,
            ScopeExpansionDetected: false,
            RoleParticipationAuthorized: roleParticipationAuthorized,
            Summary: "role");

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
            LawfulBasis: "carry",
            TimestampUtc: DateTimeOffset.UtcNow);

}
