using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public enum ActionableContentKind
{
    ReturnCandidate = 0,
    CollapseCandidate = 1,
    PrimeDerivativeCandidate = 2
}

public enum ControlSurfaceKind
{
    SoulFrameReturnIntake = 0,
    StewardReturnReview = 1,
    GovernanceDecision = 2,
    GovernanceAct = 3
}

public enum ControlMutationOutcome
{
    Authorized = 0,
    Refused = 1,
    Deferred = 2,
    Quarantined = 3,
    NoOp = 4
}

public sealed record GovernedActionableContent(
    string ContentHandle,
    ActionableContentKind Kind,
    string OriginSurface,
    string ProvenanceMarker,
    string SourceSubsystem,
    string PayloadClass,
    string? TraceReference,
    string? ResidueReference);

public sealed record GovernedControlSurfaceRequestEnvelope(
    string EnvelopeId,
    ControlSurfaceKind TargetSurface,
    string RequestedBy,
    string ScopeHandle,
    string ProtectionClass,
    string WitnessRequirement,
    string? ParentEnvelopeId,
    GovernedActionableContent ActionableContent);

public sealed record GovernedControlSurfaceMutationReceipt(
    string ReceiptHandle,
    string EnvelopeId,
    string ContentHandle,
    ControlSurfaceKind TargetSurface,
    ControlMutationOutcome Outcome,
    string GovernedBy,
    string DecisionCode,
    DateTimeOffset TimestampUtc);

public static class ControlSurfaceContractGuards
{
    public static GovernedActionableContent CreateReturnCandidateActionableContent(
        string contentHandle,
        string originSurface,
        string provenanceMarker,
        string sourceSubsystem,
        string payloadClass = "return-candidate",
        string? traceReference = null,
        string? residueReference = null)
    {
        RequireNonEmpty(contentHandle, nameof(contentHandle));
        RequireNonEmpty(originSurface, nameof(originSurface));
        RequireNonEmpty(provenanceMarker, nameof(provenanceMarker));
        RequireNonEmpty(sourceSubsystem, nameof(sourceSubsystem));
        RequireNonEmpty(payloadClass, nameof(payloadClass));

        return new GovernedActionableContent(
            ContentHandle: contentHandle.Trim(),
            Kind: ActionableContentKind.ReturnCandidate,
            OriginSurface: originSurface.Trim(),
            ProvenanceMarker: provenanceMarker.Trim(),
            SourceSubsystem: sourceSubsystem.Trim(),
            PayloadClass: payloadClass.Trim(),
            TraceReference: NormalizeOptional(traceReference),
            ResidueReference: NormalizeOptional(residueReference));
    }

    public static GovernedControlSurfaceRequestEnvelope CreateRequestEnvelope(
        ControlSurfaceKind targetSurface,
        string requestedBy,
        string scopeHandle,
        string protectionClass,
        string witnessRequirement,
        GovernedActionableContent actionableContent,
        string? parentEnvelopeId = null)
    {
        ArgumentNullException.ThrowIfNull(actionableContent);
        RequireNonEmpty(requestedBy, nameof(requestedBy));
        RequireNonEmpty(scopeHandle, nameof(scopeHandle));
        RequireNonEmpty(protectionClass, nameof(protectionClass));
        RequireNonEmpty(witnessRequirement, nameof(witnessRequirement));

        EnsureTargetSurfaceAllowed(actionableContent.Kind, targetSurface);

        var normalizedParentEnvelopeId = NormalizeOptional(parentEnvelopeId);
        var envelopeId = CreateDeterministicHandle(
            "control-envelope://",
            targetSurface.ToString(),
            requestedBy,
            scopeHandle,
            protectionClass,
            witnessRequirement,
            normalizedParentEnvelopeId ?? string.Empty,
            actionableContent.ContentHandle,
            actionableContent.Kind.ToString(),
            actionableContent.OriginSurface,
            actionableContent.ProvenanceMarker,
            actionableContent.SourceSubsystem,
            actionableContent.PayloadClass,
            actionableContent.TraceReference ?? string.Empty,
            actionableContent.ResidueReference ?? string.Empty);

        return new GovernedControlSurfaceRequestEnvelope(
            EnvelopeId: envelopeId,
            TargetSurface: targetSurface,
            RequestedBy: requestedBy.Trim(),
            ScopeHandle: scopeHandle.Trim(),
            ProtectionClass: protectionClass.Trim(),
            WitnessRequirement: witnessRequirement.Trim(),
            ParentEnvelopeId: normalizedParentEnvelopeId,
            ActionableContent: actionableContent);
    }

    public static GovernedControlSurfaceMutationReceipt CreateMutationReceipt(
        string envelopeId,
        string contentHandle,
        ControlSurfaceKind targetSurface,
        ControlMutationOutcome outcome,
        string governedBy,
        string decisionCode,
        DateTimeOffset timestampUtc)
    {
        RequireNonEmpty(envelopeId, nameof(envelopeId));
        RequireNonEmpty(contentHandle, nameof(contentHandle));
        RequireNonEmpty(governedBy, nameof(governedBy));
        RequireNonEmpty(decisionCode, nameof(decisionCode));

        var receiptHandle = CreateDeterministicHandle(
            "control-receipt://",
            envelopeId,
            contentHandle,
            targetSurface.ToString(),
            outcome.ToString(),
            governedBy,
            decisionCode,
            timestampUtc.ToUniversalTime().ToString("O"));

        return new GovernedControlSurfaceMutationReceipt(
            ReceiptHandle: receiptHandle,
            EnvelopeId: envelopeId.Trim(),
            ContentHandle: contentHandle.Trim(),
            TargetSurface: targetSurface,
            Outcome: outcome,
            GovernedBy: governedBy.Trim(),
            DecisionCode: decisionCode.Trim(),
            TimestampUtc: timestampUtc.ToUniversalTime());
    }

    public static void ValidateSoulFrameReturnIntakeRequest(SoulFrameReturnIntakeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateEnvelope(request.RequestEnvelope);

        EnsureParity(
            request.RequestEnvelope.TargetSurface == ControlSurfaceKind.SoulFrameReturnIntake,
            "SoulFrame return intake requests must target the SoulFrameReturnIntake control surface.");
        EnsureParity(
            string.Equals(request.ReturnCandidatePointer, request.RequestEnvelope.ActionableContent.ContentHandle, StringComparison.Ordinal),
            "SoulFrame return intake request actionable content handle must match the legacy return candidate pointer.");
        EnsureParity(
            string.Equals(request.SourceTheater, request.RequestEnvelope.ActionableContent.OriginSurface, StringComparison.Ordinal),
            "SoulFrame return intake request actionable origin surface must match the legacy source theater.");
        EnsureParity(
            string.Equals(request.ProvenanceMarker, request.RequestEnvelope.ActionableContent.ProvenanceMarker, StringComparison.Ordinal),
            "SoulFrame return intake request actionable provenance must match the legacy provenance marker.");
        EnsureParity(
            string.Equals(request.CollapseClassification.SourceSubsystem, request.RequestEnvelope.ActionableContent.SourceSubsystem, StringComparison.Ordinal),
            "SoulFrame return intake request actionable source subsystem must match the collapse classification source subsystem.");
    }

    public static void ValidateReturnCandidateReviewRequest(ReturnCandidateReviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateEnvelope(request.RequestEnvelope);

        EnsureParity(
            request.RequestEnvelope.TargetSurface == ControlSurfaceKind.StewardReturnReview,
            "Return candidate review requests must target the StewardReturnReview control surface.");
        EnsureParity(
            string.Equals(request.ReturnCandidatePointer, request.RequestEnvelope.ActionableContent.ContentHandle, StringComparison.Ordinal),
            "Return candidate review actionable content handle must match the legacy return candidate pointer.");
        EnsureParity(
            string.Equals(request.SourceTheater, request.RequestEnvelope.ActionableContent.OriginSurface, StringComparison.Ordinal),
            "Return candidate review actionable origin surface must match the legacy source theater.");
        EnsureParity(
            string.Equals(request.ProvenanceMarker, request.RequestEnvelope.ActionableContent.ProvenanceMarker, StringComparison.Ordinal),
            "Return candidate review actionable provenance must match the legacy provenance marker.");
        EnsureParity(
            string.Equals(request.CollapseClassification.SourceSubsystem, request.RequestEnvelope.ActionableContent.SourceSubsystem, StringComparison.Ordinal),
            "Return candidate review actionable source subsystem must match the collapse classification source subsystem.");
    }

    public static void ValidateEnvelope(GovernedControlSurfaceRequestEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        RequireNonEmpty(envelope.EnvelopeId, nameof(envelope.EnvelopeId));
        RequireNonEmpty(envelope.RequestedBy, nameof(envelope.RequestedBy));
        RequireNonEmpty(envelope.ScopeHandle, nameof(envelope.ScopeHandle));
        RequireNonEmpty(envelope.ProtectionClass, nameof(envelope.ProtectionClass));
        RequireNonEmpty(envelope.WitnessRequirement, nameof(envelope.WitnessRequirement));
        ArgumentNullException.ThrowIfNull(envelope.ActionableContent);

        ValidateActionableContent(envelope.ActionableContent);
        EnsureTargetSurfaceAllowed(envelope.ActionableContent.Kind, envelope.TargetSurface);
    }

    public static void ValidateActionableContent(GovernedActionableContent actionableContent)
    {
        ArgumentNullException.ThrowIfNull(actionableContent);
        RequireNonEmpty(actionableContent.ContentHandle, nameof(actionableContent.ContentHandle));
        RequireNonEmpty(actionableContent.OriginSurface, nameof(actionableContent.OriginSurface));
        RequireNonEmpty(actionableContent.ProvenanceMarker, nameof(actionableContent.ProvenanceMarker));
        RequireNonEmpty(actionableContent.SourceSubsystem, nameof(actionableContent.SourceSubsystem));
        RequireNonEmpty(actionableContent.PayloadClass, nameof(actionableContent.PayloadClass));
    }

    private static void EnsureTargetSurfaceAllowed(ActionableContentKind kind, ControlSurfaceKind targetSurface)
    {
        var allowed = kind switch
        {
            ActionableContentKind.ReturnCandidate => targetSurface is ControlSurfaceKind.SoulFrameReturnIntake or ControlSurfaceKind.StewardReturnReview,
            ActionableContentKind.CollapseCandidate => targetSurface is ControlSurfaceKind.StewardReturnReview or ControlSurfaceKind.GovernanceDecision or ControlSurfaceKind.GovernanceAct,
            ActionableContentKind.PrimeDerivativeCandidate => false,
            _ => false
        };

        if (!allowed)
        {
            throw new InvalidOperationException(
                $"Actionable content kind '{kind}' is not lawful for target surface '{targetSurface}' in this pass.");
        }
    }

    private static string CreateDeterministicHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }

    private static void RequireNonEmpty(string value, string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, fieldName);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void EnsureParity(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
