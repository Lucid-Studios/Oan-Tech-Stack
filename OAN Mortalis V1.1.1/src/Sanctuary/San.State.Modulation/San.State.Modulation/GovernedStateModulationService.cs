using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.State.Modulation;

public interface IGovernedStateModulationService
{
    GovernedSeedStateModulationReceipt CreateReceipt(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedEvaluationResult evaluationResult);
}

public sealed class GovernedStateModulationService : IGovernedStateModulationService
{
    public GovernedSeedStateModulationReceipt CreateReceipt(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedEvaluationResult evaluationResult)
    {
        ArgumentNullException.ThrowIfNull(primeCrypticReceipt);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(evaluationResult);
        ArgumentNullException.ThrowIfNull(evaluationResult.VerticalSlice);

        var stewardshipReceipt = evaluationResult.VerticalSlice.StewardshipReceipt;
        var holdRoutingReceipt = evaluationResult.VerticalSlice.HoldRoutingReceipt;
        var bootstrapAdmissionReceipt = evaluationResult.VerticalSlice.BootstrapAdmissionReceipt;
        var situationalContext = evaluationResult.VerticalSlice.SituationalContext;
        var operationalContext = evaluationResult.VerticalSlice.OperationalContext;
        var formationContext = operationalContext?.FormationContext;
        var nexusPosture = evaluationResult.VerticalSlice.NexusPosture;
        var nexusDecision = evaluationResult.VerticalSlice.NexusTransitionDecision;

        var workState = operationalContext?.WorkState ??
                        nexusPosture?.WorkState ??
                        (bootstrapAdmissionReceipt is { MembraneWakePermitted: false }
            ? GovernedSeedWorkState.BootstrapReady
            : evaluationResult.GovernanceState switch
        {
            GovernedSeedEvaluationState.Query when evaluationResult.Accepted => GovernedSeedWorkState.ActiveCognition,
            GovernedSeedEvaluationState.UnresolvedConflict => GovernedSeedWorkState.BootstrapReady,
            _ => GovernedSeedWorkState.DormantResident
        });

        return new GovernedSeedStateModulationReceipt(
            ModulationHandle: CreateHandle("modulation://", bootstrapReceipt.BootstrapHandle, evaluationResult.Decision),
            ModulationProfile: "resident-governance-readable",
            AvailableBands:
            [
                GovernedSeedModulationBand.Industrial,
                GovernedSeedModulationBand.Government
            ],
            WorkState: workState,
            GovernanceReadable: operationalContext?.GovernanceReadable ??
                nexusPosture?.GovernanceReadable ??
                true,
            CollapseReadinessState: situationalContext?.CollapseReadinessState ??
                operationalContext?.CollapseReadinessState ??
                nexusPosture?.CollapseReadinessState ??
                stewardshipReceipt?.CollapseReadinessState ??
                GovernedSeedCollapseReadinessState.None,
            ProtectedHoldClass: situationalContext?.ProtectedHoldClass ??
                operationalContext?.ProtectedHoldClass ??
                nexusPosture?.ProtectedHoldClass ??
                stewardshipReceipt?.ProtectedHoldClass ??
                GovernedSeedProtectedHoldClass.None,
            HoldRoutingHandle: situationalContext?.HoldRoutingHandle ?? holdRoutingReceipt?.RoutingHandle,
            ProtectedHoldRoute: situationalContext?.ProtectedHoldRoute ??
                operationalContext?.ProtectedHoldRoute ??
                nexusPosture?.ProtectedHoldRoute ??
                holdRoutingReceipt?.ProtectedHoldRoute ??
                GovernedSeedProtectedHoldRoute.None,
            ProtectedHoldDestinationHandles: situationalContext?.HoldDestinationHandles ??
                holdRoutingReceipt?.DestinationHandles ??
                [],
            BootstrapAdmissionHandle: bootstrapAdmissionReceipt?.AdmissionHandle,
            BootstrapAdmissionDisposition: operationalContext?.BootstrapDisposition ??
                bootstrapAdmissionReceipt?.Disposition ??
                GovernedSeedNexusTransitionDisposition.Denied,
            BootstrapMembraneWakePermitted: operationalContext?.MembraneWakePermitted ??
                bootstrapAdmissionReceipt?.MembraneWakePermitted ??
                false,
            SituationalContextHandle: situationalContext?.ContextHandle,
            OperationalContextHandle: operationalContext?.ContextHandle,
            FormationContextHandle: formationContext?.ContextHandle,
            LispBundleHandle: operationalContext?.LispBundleHandle ?? primeCrypticReceipt.LispBundleReceipt.BundleHandle,
            SanctuaryIngressReceiptHandle: operationalContext?.SanctuaryIngressReceiptHandle ?? evaluationResult.VerticalSlice.SanctuaryIngressReceipt?.ReceiptHandle,
            HostedLlmServiceHandle: operationalContext?.HostedLlmServiceHandle,
            HostedLlmReceiptHandle: operationalContext?.HostedLlmReceiptHandle,
            HostedLlmRequestPacketHandle: operationalContext?.HostedLlmRequestPacketHandle,
            HostedLlmResponsePacketHandle: operationalContext?.HostedLlmResponsePacketHandle,
            HostedLlmState: operationalContext?.HostedLlmState,
            HighMindContextHandle: operationalContext?.HighMindContextHandle,
            HighMindUptakeKind: operationalContext?.HighMindUptakeKind,
            FirstPrimeReceiptHandle: operationalContext?.FirstPrimeReceiptHandle,
            FirstPrimeState: operationalContext?.FirstPrimeState,
            PrimeSeedReceiptHandle: operationalContext?.PrimeSeedReceiptHandle,
            PrimeSeedState: operationalContext?.PrimeSeedState,
            PreDomainGovernancePacketHandle: operationalContext?.PreDomainGovernancePacketHandle,
            CandidateBoundaryReceiptHandle: operationalContext?.CandidateBoundaryReceiptHandle,
            CrypticHoldingInspectionHandle: operationalContext?.CrypticHoldingInspectionHandle,
            FormOrCleaveAssessmentHandle: operationalContext?.FormOrCleaveAssessmentHandle,
            CandidateSeparationReceiptHandle: operationalContext?.CandidateSeparationReceiptHandle,
            DuplexGovernanceReceiptHandle: operationalContext?.DuplexGovernanceReceiptHandle,
            PreDomainAdmissionGateReceiptHandle: operationalContext?.PreDomainAdmissionGateReceiptHandle,
            PreDomainHostLoopReceiptHandle: operationalContext?.PreDomainHostLoopReceiptHandle,
            PreDomainAdmissionDisposition: operationalContext?.PreDomainAdmissionDisposition,
            PreDomainCarryDisposition: operationalContext?.PreDomainCarryDisposition,
            PreDomainCollapseDisposition: operationalContext?.PreDomainCollapseDisposition,
            DomainRoleGatingReceiptHandle: operationalContext?.DomainRoleGatingReceiptHandle,
            DomainRoleGatingDisposition: operationalContext?.DomainRoleGatingDisposition,
            DomainEligible: operationalContext?.DomainEligible,
            RoleEligible: operationalContext?.RoleEligible,
            FirstRunReceiptHandle: operationalContext?.FirstRunReceiptHandle,
            PreGovernancePacketHandle: operationalContext?.PreGovernancePacketHandle,
            LocalAuthorityTraceHandle: operationalContext?.LocalAuthorityTraceHandle,
            ConstitutionalContactHandle: operationalContext?.ConstitutionalContactHandle,
            LocalKeypairGenesisSourceHandle: operationalContext?.LocalKeypairGenesisSourceHandle,
            LocalKeypairGenesisHandle: operationalContext?.LocalKeypairGenesisHandle,
            FirstCrypticBraidEstablishmentHandle: operationalContext?.FirstCrypticBraidEstablishmentHandle,
            FirstCrypticBraidHandle: operationalContext?.FirstCrypticBraidHandle,
            FirstCrypticConditioningSourceHandle: operationalContext?.FirstCrypticConditioningSourceHandle,
            FirstCrypticConditioningHandle: operationalContext?.FirstCrypticConditioningHandle,
            FirstRunState: operationalContext?.FirstRunState,
            FirstRunReadinessState: operationalContext?.FirstRunReadinessState,
            FirstRunStateProvisional: operationalContext?.FirstRunStateProvisional,
            FirstRunStateActualized: operationalContext?.FirstRunStateActualized,
            FirstRunOpalActualized: operationalContext?.FirstRunOpalActualized ?? false,
            LowMindSfRouteHandle: operationalContext?.LowMindSfRouteHandle,
            IngressAccessClass: operationalContext?.IngressAccessClass ?? GovernedSeedIngressAccessClass.PromptInput,
            LowMindSfRouteKind: operationalContext?.LowMindSfRouteKind ?? GovernedSeedLowMindSfRouteKind.DirectPrompt,
            PrimeToCrypticTransitHandle: operationalContext?.PrimeToCrypticTransit.TransitHandle,
            PrimeToCrypticPacketHandle: operationalContext?.PrimeToCrypticTransit.TransitPacket.PacketHandle,
            CrypticToPrimeTransitHandle: operationalContext?.CrypticToPrimeTransit.TransitHandle,
            CrypticToPrimePacketHandle: operationalContext?.CrypticToPrimeTransit.ReturnPacket.PacketHandle,
            NexusPostureHandle: operationalContext?.NexusPostureHandle ?? nexusPosture?.PostureHandle,
            NexusDecisionHandle: operationalContext?.NexusDecisionHandle ?? nexusDecision?.DecisionHandle,
            NexusActivatedModality: operationalContext?.ActivatedModality ??
                nexusDecision?.ActivatedModality ??
                GovernedSeedNexusModality.Observe,
            NexusDisposition: operationalContext?.NexusDisposition ??
                nexusDecision?.Disposition ??
                GovernedSeedNexusTransitionDisposition.Denied,
            NexusBraidingProfile: nexusPosture?.BraidingProfile,
            ScopeLane: formationContext?.ScopeLane,
            GovernedFormKind: formationContext?.GovernedFormKind,
            ReviewState: situationalContext?.ReviewState ??
                operationalContext?.ReviewState ??
                nexusPosture?.ReviewState ??
                stewardshipReceipt?.ReviewState ??
                (bootstrapAdmissionReceipt is { MembraneWakePermitted: false }
                    ? GovernedSeedReviewState.NoReviewRequired
                    : GovernedSeedReviewState.DeferredReview),
            StewardshipHandle: situationalContext?.StewardshipHandle ?? stewardshipReceipt?.StewardshipHandle,
            SourceReason: operationalContext?.SourceReason ??
                situationalContext?.GovernanceTrace ??
                nexusDecision?.DecisionReason ??
                evaluationResult.GovernanceTrace,
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            PrimeCrypticHandle: primeCrypticReceipt.ServiceHandle,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
