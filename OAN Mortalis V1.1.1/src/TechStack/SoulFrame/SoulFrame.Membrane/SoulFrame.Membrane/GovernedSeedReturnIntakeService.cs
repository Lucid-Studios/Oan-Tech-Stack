using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SoulFrame.Membrane;

public interface IGovernedSeedReturnIntakeService
{
    GovernedSeedSoulFrameReturnIntakeReceipt CreateReturnIntake(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameProjectionReceipt projectionReceipt,
        GovernedSeedEvaluationResult evaluationResult);
}

public sealed class GovernedSeedReturnIntakeService : IGovernedSeedReturnIntakeService
{
    public GovernedSeedSoulFrameReturnIntakeReceipt CreateReturnIntake(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSoulFrameProjectionReceipt projectionReceipt,
        GovernedSeedEvaluationResult evaluationResult)
    {
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentNullException.ThrowIfNull(projectionReceipt);
        ArgumentNullException.ThrowIfNull(evaluationResult);

        var predicate = evaluationResult.VerticalSlice.Predicate;
        var classification = new GovernedSeedReturnClassificationLedger(
            AdmissibleCount: predicate?.Standing.Count ?? 0,
            // The richer transformed/denied split is not in this bootstrap line yet.
            TransformedCount: 0,
            RedactedCount: evaluationResult.ProtectedResidueEvidence.Count,
            DeniedCount: (predicate?.Conflicted.Count ?? 0) + (predicate?.Refused.Count ?? 0),
            DeferredCount: predicate?.Deferred.Count ?? 0);

        return new GovernedSeedSoulFrameReturnIntakeReceipt(
            IntakeHandle: CreateHandle("return-intake://", projectionReceipt.SessionHandle, evaluationResult.Decision),
            SessionHandle: projectionReceipt.SessionHandle,
            IntakeIntent: GovernedSeedReturnIntakeIntent.ReturnCandidateEvaluation,
            CandidateHandle: CreateHandle("return-candidate://", evaluationResult.Decision, evaluationResult.GovernanceTrace),
            ProvenanceMarker: projectionReceipt.ProvenanceMarker,
            ParityConsistent: true,
            Classification: classification,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
