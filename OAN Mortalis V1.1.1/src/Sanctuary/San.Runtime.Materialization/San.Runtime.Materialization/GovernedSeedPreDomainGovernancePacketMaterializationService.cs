using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace San.Runtime.Materialization;

public interface IGovernedSeedPreDomainGovernancePacketMaterializationService
{
    GovernedSeedPreDomainGovernancePacket Materialize(
        PrimeSeedStateReceipt primeSeedStateReceipt,
        GovernedSeedCandidateBoundaryReceipt boundaryReceipt,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspectionReceipt,
        GovernedSeedFormOrCleaveAssessment formOrCleaveAssessment,
        GovernedSeedCandidateSeparationReceipt separationReceipt,
        PrimeCrypticDuplexGovernanceReceipt duplexGovernanceReceipt,
        PrimeSeedPreDomainAdmissionGateReceipt admissionGateReceipt,
        GovernedSeedPreDomainHostLoopReceipt hostLoopReceipt);
}

public sealed class GovernedSeedPreDomainGovernancePacketMaterializationService
    : IGovernedSeedPreDomainGovernancePacketMaterializationService
{
    public GovernedSeedPreDomainGovernancePacket Materialize(
        PrimeSeedStateReceipt primeSeedStateReceipt,
        GovernedSeedCandidateBoundaryReceipt boundaryReceipt,
        GovernedSeedCrypticHoldingInspectionReceipt holdingInspectionReceipt,
        GovernedSeedFormOrCleaveAssessment formOrCleaveAssessment,
        GovernedSeedCandidateSeparationReceipt separationReceipt,
        PrimeCrypticDuplexGovernanceReceipt duplexGovernanceReceipt,
        PrimeSeedPreDomainAdmissionGateReceipt admissionGateReceipt,
        GovernedSeedPreDomainHostLoopReceipt hostLoopReceipt)
    {
        ArgumentNullException.ThrowIfNull(primeSeedStateReceipt);
        ArgumentNullException.ThrowIfNull(boundaryReceipt);
        ArgumentNullException.ThrowIfNull(holdingInspectionReceipt);
        ArgumentNullException.ThrowIfNull(formOrCleaveAssessment);
        ArgumentNullException.ThrowIfNull(separationReceipt);
        ArgumentNullException.ThrowIfNull(duplexGovernanceReceipt);
        ArgumentNullException.ThrowIfNull(admissionGateReceipt);
        ArgumentNullException.ThrowIfNull(hostLoopReceipt);

        EnsureCandidateIdentity(boundaryReceipt.CandidateId, separationReceipt.CandidateId, "boundary/separation");
        EnsureCandidateIdentity(boundaryReceipt.CandidateId, duplexGovernanceReceipt.CandidateId, "boundary/duplex");
        EnsureCandidateIdentity(boundaryReceipt.CandidateId, admissionGateReceipt.CandidateId, "boundary/admission");

        return new GovernedSeedPreDomainGovernancePacket(
            PacketHandle: CreateHandle(
                "governed-seed-pre-domain-governance-packet://",
                boundaryReceipt.CandidateId,
                hostLoopReceipt.ReceiptHandle),
            CandidateId: boundaryReceipt.CandidateId,
            PrimeSeedStateReceipt: primeSeedStateReceipt,
            BoundaryReceipt: boundaryReceipt,
            HoldingInspectionReceipt: holdingInspectionReceipt,
            FormOrCleaveAssessment: formOrCleaveAssessment,
            SeparationReceipt: separationReceipt,
            DuplexGovernanceReceipt: duplexGovernanceReceipt,
            AdmissionGateReceipt: admissionGateReceipt,
            HostLoopReceipt: hostLoopReceipt,
            MaterializedAtUtc: DateTimeOffset.UtcNow,
            Summary: "Pre-domain governance witness chain materialized as one carried packet.");
    }

    private static void EnsureCandidateIdentity(string expected, string actual, string surface)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Pre-domain governance packet requires consistent candidate identity across {surface} surfaces.");
        }
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
