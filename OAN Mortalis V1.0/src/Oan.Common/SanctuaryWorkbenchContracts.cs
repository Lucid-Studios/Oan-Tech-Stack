namespace Oan.Common;

public sealed record SanctuaryRuntimeWorkbenchSurfaceReceipt(
    string WorkbenchHandle,
    string CMEId,
    string UtilitySurfaceHandle,
    string BondedLocalityLedgerHandle,
    string SanctuaryActualLocality,
    string OperatorActualLocality,
    string RuntimeDeployabilityState,
    string SanctuaryRuntimeReadinessState,
    string RuntimeWorkAdmissibilityState,
    string SessionPosture,
    string BoundedWorkClass,
    bool BondedOperatorLaneWithheld,
    bool MosBearingReleaseDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record AmenableDayDreamTierAdmissibilityReceipt(
    string DayDreamTierHandle,
    string CMEId,
    string WorkbenchHandle,
    IReadOnlyList<string> ExploratoryPredicates,
    IReadOnlyList<string> NonFinalOutputs,
    string AdmissibilityState,
    bool ExploratoryOnly,
    bool IdentityBearingDescentDenied,
    bool ContinuityInflationDenied,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public sealed record SelfRootedCrypticDepthGateReceipt(
    string DepthGateHandle,
    string CMEId,
    string WorkbenchHandle,
    string DayDreamTierHandle,
    string ThreadBirthHandle,
    string IdentityInvariantHandle,
    string CrypticBiadRootHandle,
    string GateState,
    bool SelfRooted,
    bool SharedAmenableOriginDenied,
    bool DeepAccessGranted,
    string ReasonCode,
    DateTimeOffset TimestampUtc);

public static class SanctuaryWorkbenchProjector
{
    private const string AgentiActualSurfacePrefix = "agenticore-actual-surface://";
    private const string BondedLocalityLedgerPrefix = "bonded-locality-ledger://";
    private const string SanctuaryWorkbenchPrefix = "sanctuary-runtime-workbench://";
    private const string AmenableDayDreamPrefix = "amenable-day-dream-tier://";
    private const string GovernedThreadBirthPrefix = "governed-thread-birth://";
    private const string IdentityInvariantPrefix = "identity-invariant://";
    private const string CrypticBiadRootPrefix = "cryptic-biad-root://";

    public static SanctuaryRuntimeWorkbenchSurfaceReceipt CreateRuntimeWorkbenchSurface(
        AgentiActualUtilitySurfaceReceipt utilitySurface,
        BondedParticipationLocalityLedgerReceipt localityLedger,
        string runtimeDeployabilityState,
        string sanctuaryRuntimeReadinessState,
        string runtimeWorkAdmissibilityState,
        string sessionPosture = "bounded-workbench-ready",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(utilitySurface);
        ArgumentNullException.ThrowIfNull(localityLedger);
        EnsurePrefix(utilitySurface.UtilitySurfaceHandle, AgentiActualSurfacePrefix, nameof(utilitySurface));
        EnsurePrefix(localityLedger.LedgerHandle, BondedLocalityLedgerPrefix, nameof(localityLedger));
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeDeployabilityState);
        ArgumentException.ThrowIfNullOrWhiteSpace(sanctuaryRuntimeReadinessState);
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeWorkAdmissibilityState);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionPosture);
        RequireActualLocality(utilitySurface.SanctuaryActualLocality, nameof(utilitySurface.SanctuaryActualLocality));
        RequireActualLocality(utilitySurface.OperatorActualLocality, nameof(utilitySurface.OperatorActualLocality));

        if (!string.Equals(utilitySurface.CMEId, localityLedger.CMEId, StringComparison.Ordinal) ||
            !string.Equals(utilitySurface.SanctuaryActualLocality, localityLedger.SanctuaryActualLocality, StringComparison.Ordinal) ||
            !string.Equals(utilitySurface.OperatorActualLocality, localityLedger.OperatorActualLocality, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Runtime workbench requires utility and bonded locality receipts to remain inside the same CME and locality pair.");
        }

        return new SanctuaryRuntimeWorkbenchSurfaceReceipt(
            WorkbenchHandle: SanctuaryWorkbenchKeys.CreateSanctuaryRuntimeWorkbenchHandle(
                utilitySurface.CMEId,
                utilitySurface.UtilitySurfaceHandle,
                localityLedger.LedgerHandle),
            CMEId: utilitySurface.CMEId,
            UtilitySurfaceHandle: utilitySurface.UtilitySurfaceHandle,
            BondedLocalityLedgerHandle: localityLedger.LedgerHandle,
            SanctuaryActualLocality: utilitySurface.SanctuaryActualLocality,
            OperatorActualLocality: utilitySurface.OperatorActualLocality,
            RuntimeDeployabilityState: runtimeDeployabilityState.Trim(),
            SanctuaryRuntimeReadinessState: sanctuaryRuntimeReadinessState.Trim(),
            RuntimeWorkAdmissibilityState: runtimeWorkAdmissibilityState.Trim(),
            SessionPosture: sessionPosture.Trim(),
            BoundedWorkClass: "bounded-local-candidate-sanctuary-workbench",
            BondedOperatorLaneWithheld: true,
            MosBearingReleaseDenied: true,
            ReasonCode: "sanctuary-runtime-workbench-surface-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static AmenableDayDreamTierAdmissibilityReceipt CreateAmenableDayDreamTier(
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        IReadOnlyList<string> exploratoryPredicates,
        IReadOnlyList<string> nonFinalOutputs,
        string admissibilityState = "amenable-exploratory-only",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(workbench);
        EnsurePrefix(workbench.WorkbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbench));
        ArgumentNullException.ThrowIfNull(exploratoryPredicates);
        ArgumentNullException.ThrowIfNull(nonFinalOutputs);
        ArgumentException.ThrowIfNullOrWhiteSpace(admissibilityState);

        return new AmenableDayDreamTierAdmissibilityReceipt(
            DayDreamTierHandle: SanctuaryWorkbenchKeys.CreateAmenableDayDreamTierHandle(
                workbench.CMEId,
                workbench.WorkbenchHandle,
                admissibilityState),
            CMEId: workbench.CMEId,
            WorkbenchHandle: workbench.WorkbenchHandle,
            ExploratoryPredicates: exploratoryPredicates
                .Where(static predicate => !string.IsNullOrWhiteSpace(predicate))
                .Select(static predicate => predicate.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            NonFinalOutputs: nonFinalOutputs
                .Where(static output => !string.IsNullOrWhiteSpace(output))
                .Select(static output => output.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            AdmissibilityState: admissibilityState.Trim(),
            ExploratoryOnly: true,
            IdentityBearingDescentDenied: true,
            ContinuityInflationDenied: true,
            ReasonCode: "amenable-day-dream-tier-admissibility-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    public static SelfRootedCrypticDepthGateReceipt CreateSelfRootedCrypticDepthGate(
        GovernedThreadBirthReceipt threadBirth,
        SanctuaryRuntimeWorkbenchSurfaceReceipt workbench,
        AmenableDayDreamTierAdmissibilityReceipt dayDreamTier,
        string gateState = "provisionally-rooted-withheld",
        DateTimeOffset? timestampUtc = null)
    {
        ArgumentNullException.ThrowIfNull(threadBirth);
        ArgumentNullException.ThrowIfNull(workbench);
        ArgumentNullException.ThrowIfNull(dayDreamTier);
        EnsurePrefix(threadBirth.ThreadBirthHandle, GovernedThreadBirthPrefix, nameof(threadBirth));
        EnsurePrefix(threadBirth.IdentityInvariantHandle, IdentityInvariantPrefix, nameof(threadBirth.IdentityInvariantHandle));
        EnsurePrefix(workbench.WorkbenchHandle, SanctuaryWorkbenchPrefix, nameof(workbench));
        EnsurePrefix(dayDreamTier.DayDreamTierHandle, AmenableDayDreamPrefix, nameof(dayDreamTier));
        ArgumentException.ThrowIfNullOrWhiteSpace(gateState);

        if (!string.Equals(threadBirth.CMEId, workbench.CMEId, StringComparison.Ordinal) ||
            !string.Equals(threadBirth.CMEId, dayDreamTier.CMEId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Self-rooted cryptic depth gate requires thread birth, workbench, and day-dream tier to remain inside one CME continuity surface.");
        }

        var crypticBiadRootHandle = SanctuaryWorkbenchKeys.CreateCrypticBiadRootHandle(
            threadBirth.CMEId,
            threadBirth.IdentityInvariantHandle,
            threadBirth.ThreadBirthHandle);
        EnsurePrefix(crypticBiadRootHandle, CrypticBiadRootPrefix, nameof(crypticBiadRootHandle));

        return new SelfRootedCrypticDepthGateReceipt(
            DepthGateHandle: SanctuaryWorkbenchKeys.CreateSelfRootedCrypticDepthGateHandle(
                threadBirth.CMEId,
                workbench.WorkbenchHandle,
                crypticBiadRootHandle),
            CMEId: threadBirth.CMEId,
            WorkbenchHandle: workbench.WorkbenchHandle,
            DayDreamTierHandle: dayDreamTier.DayDreamTierHandle,
            ThreadBirthHandle: threadBirth.ThreadBirthHandle,
            IdentityInvariantHandle: threadBirth.IdentityInvariantHandle,
            CrypticBiadRootHandle: crypticBiadRootHandle,
            GateState: gateState.Trim(),
            SelfRooted: true,
            SharedAmenableOriginDenied: true,
            DeepAccessGranted: false,
            ReasonCode: "self-rooted-cryptic-depth-gate-bound",
            TimestampUtc: timestampUtc ?? DateTimeOffset.UtcNow);
    }

    private static void RequireActualLocality(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.IndexOf(".actual", StringComparison.OrdinalIgnoreCase) < 0)
        {
            throw new ArgumentException($"{parameterName} must identify an `.actual` locality.", parameterName);
        }
    }

    private static void EnsurePrefix(string value, string prefix, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (!value.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"{parameterName} must use the `{prefix}` handle class.", parameterName);
        }
    }
}
