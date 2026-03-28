using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace Oan.Nexus.Control;

public sealed record GovernedSeedNexusControlResult(
    GovernedSeedNexusPostureSnapshot Posture,
    GovernedSeedNexusTransitionRequest Request,
    GovernedSeedNexusTransitionDecision Decision);

public interface IGovernedNexusControlService
{
    GovernedSeedNexusControlResult EvaluateBootstrapAdmission(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt);

    GovernedSeedNexusHoldRoutingDisposition EvaluateHoldRoutingDisposition(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedEvaluationResult evaluationResult,
        GovernedSeedProtectedHoldRoute route);

    GovernedSeedNexusStewardshipDisposition EvaluateStewardshipDisposition(
        GovernedSeedEvaluationResult evaluationResult,
        GovernedSeedProtectedHoldRoutingReceipt holdRoutingReceipt);

    GovernedSeedNexusControlResult Evaluate(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSituationalContext situationalContext);
}

public sealed class GovernedNexusControlService : IGovernedNexusControlService
{
    public GovernedSeedNexusControlResult EvaluateBootstrapAdmission(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt)
    {
        ArgumentNullException.ThrowIfNull(primeCrypticReceipt);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);

        var disposition = string.IsNullOrWhiteSpace(bootstrapReceipt.BootstrapProfile) ||
                          string.IsNullOrWhiteSpace(primeCrypticReceipt.ResidencyProfile)
            ? GovernedSeedNexusTransitionDisposition.Denied
            : GovernedSeedNexusTransitionDisposition.Admitted;

        var activatedModality = disposition == GovernedSeedNexusTransitionDisposition.Admitted
            ? GovernedSeedNexusModality.Instantiate
            : GovernedSeedNexusModality.Observe;

        var admittedModalities = disposition == GovernedSeedNexusTransitionDisposition.Admitted
            ? new[]
            {
                GovernedSeedNexusModality.Observe,
                GovernedSeedNexusModality.Instantiate,
                GovernedSeedNexusModality.Modulate
            }
            : new[]
            {
                GovernedSeedNexusModality.Observe,
                GovernedSeedNexusModality.Modulate
            };

        var posture = new GovernedSeedNexusPostureSnapshot(
            PostureHandle: CreateHandle("nexus-posture://", bootstrapReceipt.BootstrapHandle, "bootstrap-admission"),
            PostureProfile: "prime-cryptic-steward-interface",
            PrimeAuthorityProfile: "foundational-standing-and-admissibility",
            CrypticAuthorityProfile: primeCrypticReceipt.CrypticResidencyClass,
            StewardAuthorityProfile: "bootstrap-steward-readiness",
            BraidingProfile: "prime-cryptic-steward-braid",
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            PrimeCrypticHandle: primeCrypticReceipt.ServiceHandle,
            WorkState: GovernedSeedWorkState.BootstrapReady,
            CollapseReadinessState: GovernedSeedCollapseReadinessState.None,
            ProtectedHoldClass: GovernedSeedProtectedHoldClass.None,
            ProtectedHoldRoute: GovernedSeedProtectedHoldRoute.None,
            ReviewState: GovernedSeedReviewState.NoReviewRequired,
            GovernanceReadable: true,
            TargetBoundedLaneAvailable: primeCrypticReceipt.TargetBoundedLaneAvailable,
            AdmittedModalities: admittedModalities,
            TimestampUtc: DateTimeOffset.UtcNow);

        var request = new GovernedSeedNexusTransitionRequest(
            RequestHandle: CreateHandle("nexus-request://", posture.PostureHandle, GovernedSeedNexusModality.Instantiate.ToString()),
            RequestedModality: GovernedSeedNexusModality.Instantiate,
            RequestedByLayer: "bootstrap-admission",
            SourceReason: "bootstrap-admission-check",
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            PrimeCrypticHandle: primeCrypticReceipt.ServiceHandle,
            TimestampUtc: DateTimeOffset.UtcNow);

        var decision = new GovernedSeedNexusTransitionDecision(
            DecisionHandle: CreateHandle("nexus-decision://", bootstrapReceipt.BootstrapHandle, "bootstrap-admission"),
            DecisionProfile: disposition == GovernedSeedNexusTransitionDisposition.Admitted
                ? "braided-bootstrap-admission"
                : "braided-bootstrap-denial",
            Disposition: disposition,
            RequestedModality: GovernedSeedNexusModality.Instantiate,
            ActivatedModality: activatedModality,
            ReviewRequired: false,
            DecisionReason: disposition == GovernedSeedNexusTransitionDisposition.Admitted
                ? "bootstrap-admitted"
                : "bootstrap-preconditions-unsatisfied",
            ActivatedHandleSet: disposition == GovernedSeedNexusTransitionDisposition.Admitted
                ? new[] { bootstrapReceipt.SoulFrameHandle, bootstrapReceipt.MembraneHandle }
                : Array.Empty<string>(),
            TimestampUtc: DateTimeOffset.UtcNow);

        return new GovernedSeedNexusControlResult(posture, request, decision);
    }

    public GovernedSeedNexusHoldRoutingDisposition EvaluateHoldRoutingDisposition(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameReturnIntakeReceipt returnIntakeReceipt,
        GovernedSeedEvaluationResult evaluationResult,
        GovernedSeedProtectedHoldRoute route)
    {
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(returnIntakeReceipt);
        ArgumentNullException.ThrowIfNull(evaluationResult);

        var residueEvidence = evaluationResult.ProtectedResidueEvidence ?? [];
        var hasMixedResidue = residueEvidence.Any(static evidence =>
            evidence.ResidueKind == GovernedSeedProtectedResidueKind.Mixed);

        var reviewRequired = route switch
        {
            GovernedSeedProtectedHoldRoute.None => false,
            GovernedSeedProtectedHoldRoute.SplitRouteAcrossCGoaAndCMos => true,
            GovernedSeedProtectedHoldRoute.RouteToCMos => hasMixedResidue ||
                returnIntakeReceipt.Classification.DeniedCount > 0 ||
                returnIntakeReceipt.Classification.DeferredCount > 0 ||
                bootstrapReceipt.CustodySnapshot.CMosHoldSurface.DeferredReviewByDefault,
            GovernedSeedProtectedHoldRoute.RouteToCGoa => hasMixedResidue ||
                returnIntakeReceipt.Classification.DeniedCount > 0 ||
                returnIntakeReceipt.Classification.DeferredCount > 0 ||
                bootstrapReceipt.CustodySnapshot.CGoaHoldSurface.DeferredReviewByDefault,
            _ => true
        };

        var evidenceClass = route switch
        {
            GovernedSeedProtectedHoldRoute.RouteToCGoa => "typed-contextual-protected-residue",
            GovernedSeedProtectedHoldRoute.RouteToCMos => "typed-self-state-protected-residue",
            GovernedSeedProtectedHoldRoute.SplitRouteAcrossCGoaAndCMos => "typed-mixed-protected-residue",
            _ => "no-protected-residue"
        };

        return new GovernedSeedNexusHoldRoutingDisposition(
            DispositionHandle: CreateHandle("nexus-hold-routing://", bootstrapReceipt.BootstrapHandle, returnIntakeReceipt.IntakeHandle, route.ToString()),
            DispositionProfile: "braided-hold-routing-disposition",
            EvidenceClass: evidenceClass,
            ReviewRequired: reviewRequired,
            SourceReason: evaluationResult.GovernanceTrace,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    public GovernedSeedNexusStewardshipDisposition EvaluateStewardshipDisposition(
        GovernedSeedEvaluationResult evaluationResult,
        GovernedSeedProtectedHoldRoutingReceipt holdRoutingReceipt)
    {
        ArgumentNullException.ThrowIfNull(evaluationResult);
        ArgumentNullException.ThrowIfNull(holdRoutingReceipt);

        var protectedHoldClass = holdRoutingReceipt.ProtectedHoldRoute switch
        {
            GovernedSeedProtectedHoldRoute.RouteToCGoa => GovernedSeedProtectedHoldClass.CGoaCandidate,
            GovernedSeedProtectedHoldRoute.RouteToCMos => GovernedSeedProtectedHoldClass.CMosCandidate,
            GovernedSeedProtectedHoldRoute.SplitRouteAcrossCGoaAndCMos => GovernedSeedProtectedHoldClass.MixedProtectedCandidate,
            _ => GovernedSeedProtectedHoldClass.None
        };

        var collapseReadinessState = evaluationResult.GovernanceState switch
        {
            GovernedSeedEvaluationState.UnresolvedConflict => GovernedSeedCollapseReadinessState.DeferredReview,
            GovernedSeedEvaluationState.Query when protectedHoldClass != GovernedSeedProtectedHoldClass.None =>
                GovernedSeedCollapseReadinessState.ProtectedHoldRequired,
            GovernedSeedEvaluationState.Query when evaluationResult.Accepted =>
                GovernedSeedCollapseReadinessState.ReturnCandidatePrepared,
            _ => GovernedSeedCollapseReadinessState.DeferredReview
        };

        var reviewState = collapseReadinessState switch
        {
            GovernedSeedCollapseReadinessState.ReturnCandidatePrepared when
                protectedHoldClass == GovernedSeedProtectedHoldClass.None =>
                GovernedSeedReviewState.NoReviewRequired,
            GovernedSeedCollapseReadinessState.ProtectedHoldRequired when !holdRoutingReceipt.ReviewRequired =>
                GovernedSeedReviewState.NoReviewRequired,
            _ => GovernedSeedReviewState.DeferredReview
        };

        return new GovernedSeedNexusStewardshipDisposition(
            DispositionHandle: CreateHandle("nexus-stewardship://", evaluationResult.Decision, holdRoutingReceipt.RoutingHandle),
            DispositionProfile: "braided-stewardship-disposition",
            CollapseReadinessState: collapseReadinessState,
            ProtectedHoldClass: protectedHoldClass,
            ReviewState: reviewState,
            SourceReason: evaluationResult.GovernanceTrace,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    public GovernedSeedNexusControlResult Evaluate(
        GovernedSeedPrimeCrypticServiceReceipt primeCrypticReceipt,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSituationalContext situationalContext)
    {
        ArgumentNullException.ThrowIfNull(primeCrypticReceipt);
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(situationalContext);

        var workState = situationalContext.GovernanceState switch
        {
            GovernedSeedEvaluationState.Query when situationalContext.Accepted => GovernedSeedWorkState.ActiveCognition,
            GovernedSeedEvaluationState.UnresolvedConflict => GovernedSeedWorkState.BootstrapReady,
            _ => GovernedSeedWorkState.DormantResident
        };

        var requestedModality = DetermineRequestedModality(
            situationalContext);
        var decision = CreateDecision(requestedModality, situationalContext, bootstrapReceipt);
        var admittedModalities = DetermineAdmittedModalities(situationalContext, requestedModality);

        var posture = new GovernedSeedNexusPostureSnapshot(
            PostureHandle: CreateHandle("nexus-posture://", bootstrapReceipt.BootstrapHandle, situationalContext.DecisionCode),
            PostureProfile: "prime-cryptic-steward-interface",
            PrimeAuthorityProfile: "foundational-standing-and-admissibility",
            CrypticAuthorityProfile: primeCrypticReceipt.CrypticResidencyClass,
            StewardAuthorityProfile: situationalContext.StewardAuthorityProfile,
            BraidingProfile: "prime-cryptic-steward-braid",
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            PrimeCrypticHandle: primeCrypticReceipt.ServiceHandle,
            WorkState: workState,
            CollapseReadinessState: situationalContext.CollapseReadinessState,
            ProtectedHoldClass: situationalContext.ProtectedHoldClass,
            ProtectedHoldRoute: situationalContext.ProtectedHoldRoute,
            ReviewState: situationalContext.ReviewState,
            GovernanceReadable: true,
            TargetBoundedLaneAvailable: primeCrypticReceipt.TargetBoundedLaneAvailable,
            AdmittedModalities: admittedModalities,
            TimestampUtc: DateTimeOffset.UtcNow);

        var request = new GovernedSeedNexusTransitionRequest(
            RequestHandle: CreateHandle("nexus-request://", posture.PostureHandle, requestedModality.ToString()),
            RequestedModality: requestedModality,
            RequestedByLayer: "runtime-vertical-slice",
            SourceReason: situationalContext.GovernanceTrace,
            BootstrapHandle: bootstrapReceipt.BootstrapHandle,
            PrimeCrypticHandle: primeCrypticReceipt.ServiceHandle,
            TimestampUtc: DateTimeOffset.UtcNow);

        return new GovernedSeedNexusControlResult(posture, request, decision);
    }

    private static IReadOnlyList<GovernedSeedNexusModality> DetermineAdmittedModalities(
        GovernedSeedSituationalContext situationalContext,
        GovernedSeedNexusModality requestedModality)
    {
        var modalities = new HashSet<GovernedSeedNexusModality>
        {
            GovernedSeedNexusModality.Observe,
            GovernedSeedNexusModality.Modulate
        };

        if (situationalContext.Accepted)
        {
            modalities.Add(GovernedSeedNexusModality.Project);
            modalities.Add(requestedModality);
        }

        if (situationalContext.GovernanceState == GovernedSeedEvaluationState.UnresolvedConflict ||
            requestedModality == GovernedSeedNexusModality.Hold)
        {
            modalities.Add(GovernedSeedNexusModality.Govern);
        }

        return modalities.OrderBy(static modality => modality).ToArray();
    }

    private static GovernedSeedNexusModality DetermineRequestedModality(
        GovernedSeedSituationalContext situationalContext)
    {
        if (situationalContext.Accepted && situationalContext.ProtectedHoldRoute != GovernedSeedProtectedHoldRoute.None)
        {
            return GovernedSeedNexusModality.Hold;
        }

        if (situationalContext.Accepted &&
            situationalContext.ProtectedHoldRoute == GovernedSeedProtectedHoldRoute.None &&
            situationalContext.CollapseReadinessState == GovernedSeedCollapseReadinessState.ReturnCandidatePrepared &&
            situationalContext.ReviewState == GovernedSeedReviewState.NoReviewRequired &&
            situationalContext.ReturnDeniedCount == 0 &&
            situationalContext.ReturnDeferredCount == 0)
        {
            return GovernedSeedNexusModality.Archive;
        }

        if (situationalContext.Accepted)
        {
            return GovernedSeedNexusModality.Return;
        }

        if (situationalContext.GovernanceState == GovernedSeedEvaluationState.UnresolvedConflict)
        {
            return GovernedSeedNexusModality.Govern;
        }

        return GovernedSeedNexusModality.Observe;
    }

    private static GovernedSeedNexusTransitionDecision CreateDecision(
        GovernedSeedNexusModality requestedModality,
        GovernedSeedSituationalContext situationalContext,
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt)
    {
        var decisionHandle = CreateHandle("nexus-decision://", bootstrapReceipt.BootstrapHandle, requestedModality.ToString(), situationalContext.DecisionCode);
        var activatedHandleSet = requestedModality switch
        {
            GovernedSeedNexusModality.Hold => situationalContext.HoldDestinationHandles,
            GovernedSeedNexusModality.Archive => Array.Empty<string>(),
            _ => new[] { bootstrapReceipt.MembraneHandle }
        };

        return requestedModality switch
        {
            GovernedSeedNexusModality.Hold when situationalContext.HoldReviewRequired => new GovernedSeedNexusTransitionDecision(
                DecisionHandle: decisionHandle,
                DecisionProfile: "braided-hold-with-review",
                Disposition: GovernedSeedNexusTransitionDisposition.AdmittedWithReview,
                RequestedModality: requestedModality,
                ActivatedModality: GovernedSeedNexusModality.Hold,
                ReviewRequired: true,
                DecisionReason: situationalContext.GovernanceTrace,
                ActivatedHandleSet: activatedHandleSet,
                TimestampUtc: DateTimeOffset.UtcNow),
            GovernedSeedNexusModality.Hold => new GovernedSeedNexusTransitionDecision(
                DecisionHandle: decisionHandle,
                DecisionProfile: "braided-hold-no-review",
                Disposition: GovernedSeedNexusTransitionDisposition.AdmittedToHold,
                RequestedModality: requestedModality,
                ActivatedModality: GovernedSeedNexusModality.Hold,
                ReviewRequired: false,
                DecisionReason: situationalContext.GovernanceTrace,
                ActivatedHandleSet: activatedHandleSet,
                TimestampUtc: DateTimeOffset.UtcNow),
            GovernedSeedNexusModality.Archive => new GovernedSeedNexusTransitionDecision(
                DecisionHandle: decisionHandle,
                DecisionProfile: "braided-archive-admissible",
                Disposition: GovernedSeedNexusTransitionDisposition.AdmittedToArchive,
                RequestedModality: requestedModality,
                ActivatedModality: GovernedSeedNexusModality.Archive,
                ReviewRequired: false,
                DecisionReason: "archive-admissible-posture",
                ActivatedHandleSet: activatedHandleSet,
                TimestampUtc: DateTimeOffset.UtcNow),
            GovernedSeedNexusModality.Return => new GovernedSeedNexusTransitionDecision(
                DecisionHandle: decisionHandle,
                DecisionProfile: "braided-return-path-only",
                Disposition: GovernedSeedNexusTransitionDisposition.AdmittedToReturnPathOnly,
                RequestedModality: requestedModality,
                ActivatedModality: GovernedSeedNexusModality.Return,
                ReviewRequired: false,
                DecisionReason: situationalContext.GovernanceTrace,
                ActivatedHandleSet: activatedHandleSet,
                TimestampUtc: DateTimeOffset.UtcNow),
            GovernedSeedNexusModality.Govern => new GovernedSeedNexusTransitionDecision(
                DecisionHandle: decisionHandle,
                DecisionProfile: "braided-governance-review",
                Disposition: GovernedSeedNexusTransitionDisposition.AdmittedWithReview,
                RequestedModality: requestedModality,
                ActivatedModality: GovernedSeedNexusModality.Govern,
                ReviewRequired: true,
                DecisionReason: situationalContext.GovernanceTrace,
                ActivatedHandleSet: activatedHandleSet,
                TimestampUtc: DateTimeOffset.UtcNow),
            _ => new GovernedSeedNexusTransitionDecision(
                DecisionHandle: decisionHandle,
                DecisionProfile: "braided-denied",
                Disposition: GovernedSeedNexusTransitionDisposition.Denied,
                RequestedModality: requestedModality,
                ActivatedModality: GovernedSeedNexusModality.Observe,
                ReviewRequired: false,
                DecisionReason: situationalContext.GovernanceTrace,
                ActivatedHandleSet: activatedHandleSet,
                TimestampUtc: DateTimeOffset.UtcNow)
        };
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
