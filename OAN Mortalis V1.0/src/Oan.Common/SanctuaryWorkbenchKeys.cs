using System.Security.Cryptography;
using System.Text;

namespace Oan.Common;

public static class SanctuaryWorkbenchKeys
{
    public static string CreateSanctuaryRuntimeWorkbenchHandle(
        string cmeId,
        string utilitySurfaceHandle,
        string bondedLocalityLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(utilitySurfaceHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(bondedLocalityLedgerHandle);

        return $"sanctuary-runtime-workbench://{ComputeDigest(cmeId, utilitySurfaceHandle, bondedLocalityLedgerHandle)}";
    }

    public static string CreateAmenableDayDreamTierHandle(
        string cmeId,
        string workbenchHandle,
        string admissibilityState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(admissibilityState);

        return $"amenable-day-dream-tier://{ComputeDigest(cmeId, workbenchHandle, admissibilityState)}";
    }

    public static string CreateCrypticBiadRootHandle(
        string cmeId,
        string identityInvariantHandle,
        string threadBirthHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityInvariantHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(threadBirthHandle);

        return $"cryptic-biad-root://{ComputeDigest(cmeId, identityInvariantHandle, threadBirthHandle)}";
    }

    public static string CreateSelfRootedCrypticDepthGateHandle(
        string cmeId,
        string workbenchHandle,
        string crypticBiadRootHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(crypticBiadRootHandle);

        return $"self-rooted-cryptic-depth-gate://{ComputeDigest(cmeId, workbenchHandle, crypticBiadRootHandle)}";
    }

    public static string CreateRuntimeWorkbenchSessionLedgerHandle(
        string cmeId,
        string workbenchHandle,
        string dayDreamTierHandle,
        string depthGateHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(dayDreamTierHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(depthGateHandle);

        return $"runtime-workbench-session-ledger://{ComputeDigest(cmeId, workbenchHandle, dayDreamTierHandle, depthGateHandle)}";
    }

    public static string CreateWorkbenchSessionEventHandle(
        string cmeId,
        string workbenchHandle,
        string eventKind,
        string inquiryStance,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(inquiryStance);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return $"workbench-session-event://{ComputeDigest(cmeId, workbenchHandle, eventKind, inquiryStance, description)}";
    }

    public static string CreateBoundaryConditionHandle(
        string cmeId,
        string workbenchHandle,
        string boundaryCode,
        string triggerPredicate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(workbenchHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(triggerPredicate);

        return $"boundary-condition://{ComputeDigest(cmeId, workbenchHandle, boundaryCode, triggerPredicate)}";
    }

    public static string CreateResidueMarkerHandle(
        string cmeId,
        string anchorHandle,
        string markerCode,
        string residueClass)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(markerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(residueClass);

        return $"residue-marker://{ComputeDigest(cmeId, anchorHandle, markerCode, residueClass)}";
    }

    public static string CreateContinuityMarkerHandle(
        string cmeId,
        string anchorHandle,
        string markerCode,
        string sourceHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(markerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceHandle);

        return $"continuity-marker://{ComputeDigest(cmeId, anchorHandle, markerCode, sourceHandle)}";
    }

    public static string CreateDayDreamCollapseReceiptHandle(
        string cmeId,
        string sessionLedgerHandle,
        string dayDreamTierHandle,
        string collapseState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(dayDreamTierHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(collapseState);

        return $"day-dream-collapse-receipt://{ComputeDigest(cmeId, sessionLedgerHandle, dayDreamTierHandle, collapseState)}";
    }

    public static string CreateCrypticDepthReturnReceiptHandle(
        string cmeId,
        string sessionLedgerHandle,
        string depthGateHandle,
        string returnState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(depthGateHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnState);

        return $"cryptic-depth-return-receipt://{ComputeDigest(cmeId, sessionLedgerHandle, depthGateHandle, returnState)}";
    }

    public static string CreateLocalHostSanctuaryResidencyEnvelopeHandle(
        string cmeId,
        string sessionLedgerHandle,
        string returnReceiptHandle,
        string localityWitnessLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnReceiptHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(localityWitnessLedgerHandle);

        return $"local-host-sanctuary-residency-envelope://{ComputeDigest(cmeId, sessionLedgerHandle, returnReceiptHandle, localityWitnessLedgerHandle)}";
    }

    public static string CreateRuntimeHabitationReadinessLedgerHandle(
        string cmeId,
        string residencyEnvelopeHandle,
        string habitationState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(residencyEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(habitationState);

        return $"runtime-habitation-readiness-ledger://{ComputeDigest(cmeId, residencyEnvelopeHandle, habitationState)}";
    }

    public static string CreateBoundedInhabitationLaunchRehearsalHandle(
        string cmeId,
        string residencyEnvelopeHandle,
        string readinessLedgerHandle,
        string sessionLedgerHandle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(residencyEnvelopeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(readinessLedgerHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionLedgerHandle);

        return $"bounded-inhabitation-launch-rehearsal://{ComputeDigest(cmeId, residencyEnvelopeHandle, readinessLedgerHandle, sessionLedgerHandle)}";
    }

    private static string ComputeDigest(params string[] parts)
    {
        var material = string.Join("|", parts.Select(part => part.Trim()));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
