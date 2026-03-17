namespace Oan.Common;

public static class IssuedOfficePackageProjector
{
    private static readonly DateTimeOffset DoctrineReviewTimestampUtc = new(2026, 3, 17, 0, 0, 0, TimeSpan.Zero);

    private static readonly IReadOnlyDictionary<InternalGoverningCmeOffice, IssuedOfficeProfile> Profiles =
        new Dictionary<InternalGoverningCmeOffice, IssuedOfficeProfile>
        {
            [InternalGoverningCmeOffice.Steward] = new(
                ChassisClass: "governed-zed-base",
                TargetRuntimeSurface: "active-governance-loop",
                IssuerSurface: "host_truth_runtime",
                AuthorityScope: "routed_operational_vigilance",
                BondRequirement: "bond-required-for-handoff",
                GuardedReviewRequirement: "host-guarded-review",
                RevocationConditions:
                [
                    "authority-revoked",
                    "broken-window",
                    "continuity-ambiguous"
                ],
                ToolAllowlist:
                [
                    "task-routing",
                    "witness-request"
                ],
                ToolDenials:
                [
                    "ambient-worker-swarm",
                    "silent-disclosure-widening"
                ],
                ExternalCallPolicy: "bounded-governed-host-calls-only",
                MutationPermissions: "none",
                WorkerActivationPermissions: "not-authorized-in-sprint1",
                RequiredPacketContracts:
                [
                    "office-return-summary-v1"
                ],
                MountedMemoryLanes:
                [
                    "mission-local"
                ],
                ForbiddenMemoryLanes:
                [
                    "cryptic-sealed"
                ],
                ContinuityLinkageSurface: "governance-loop-review-request",
                ResiduePolicy: "no-persistent-residue-without-receipt",
                ParkReturnPolicy: "return-summary-before-park",
                SameOfficeLineageRule: "same-lineage-per-office-kind-and-chassis-class",
                RequiredWitnessSurfaces:
                [
                    "host-truth-witness"
                ],
                RequiredReceipts:
                [
                    "governed-office-authority-receipt-v1",
                    "governed-weather-disclosure-receipt-v1"
                ],
                ReducedTelemetrySurface: "governance-status",
                GuardedTelemetrySurface: "office-authority-receipt",
                CrypticOrSealedSurfaces:
                [
                    "cryptic-sealed"
                ],
                IncidentEscalationPath: "steward-escalation-review",
                ReturnObligations:
                [
                    "emit-return-summary",
                    "release-mounted-memory"
                ],
                RequiredReturnPacket: "office-return-summary-v1",
                ParkEligibility: "deferred-until-park-return-sprint",
                QuarantineTriggers:
                [
                    "revocation-condition-met",
                    "witness-gap"
                ],
                HandoffTerminationRule: "worker-handoff-not-enabled",
                ExpiryOrReissueWindow: "loop-scoped",
                ImplementationStatus: "package-declared-lifecycle-enforcement-pending",
                WithheldMechanicsMarker: "worker-and-park-mechanics-deferred",
                ReviewOwner: "CradleTek")
        };

    public static (IssuedOfficePackage Package, GovernedOfficeIssuanceReceipt Receipt)? ProjectForLoop(
        string loopKey,
        GovernanceJournalReplayBatch batch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(batch);

        var reviewRequest = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.ReviewRequest)
            .LastOrDefault(request => request is not null);
        if (reviewRequest is null)
        {
            return null;
        }

        var weatherDisclosureReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.WeatherDisclosureReceipt)
            .LastOrDefault(receipt => receipt is not null);
        if (weatherDisclosureReceipt is null)
        {
            return null;
        }

        var authorityReceipt = batch.Entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.OfficeAuthorityReceipt)
            .LastOrDefault(receipt => receipt is not null && receipt.Office == InternalGoverningCmeOffice.Steward);
        if (authorityReceipt is null)
        {
            return null;
        }

        if (authorityReceipt.Office != InternalGoverningCmeOffice.Steward ||
            authorityReceipt.ViewEligibility != OfficeViewEligibility.OfficeSpecificView ||
            authorityReceipt.RationaleCode != OfficeAuthorityRationaleCode.OfficeSpecificStewardView)
        {
            return null;
        }

        if (authorityReceipt.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient ||
            weatherDisclosureReceipt.EvidenceSufficiencyState != EvidenceSufficiencyState.Sufficient)
        {
            return null;
        }

        if (!string.Equals(reviewRequest.CMEId, authorityReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(weatherDisclosureReceipt.CMEId, authorityReceipt.CMEId, StringComparison.Ordinal) ||
            !string.Equals(authorityReceipt.WeatherDisclosureHandle, weatherDisclosureReceipt.DisclosureHandle, StringComparison.Ordinal))
        {
            return null;
        }

        if (!Profiles.TryGetValue(InternalGoverningCmeOffice.Steward, out var profile))
        {
            return null;
        }

        var packageId = LifecycleGovernanceKeys.CreateIssuedOfficePackageId(
            loopKey,
            authorityReceipt.CMEId,
            authorityReceipt.Office,
            authorityReceipt.AuthorityHandle,
            weatherDisclosureReceipt.DisclosureHandle);
        var issuanceLineageId = LifecycleGovernanceKeys.CreateIssuanceLineageId(
            authorityReceipt.CMEId,
            authorityReceipt.Office,
            profile.ChassisClass);
        var officeInstanceId = LifecycleGovernanceKeys.CreateOfficeInstanceId(
            loopKey,
            authorityReceipt.CMEId,
            authorityReceipt.Office,
            authorityReceipt.AuthorityHandle);
        var disclosureCeiling = MapDisclosureCeiling(authorityReceipt.DisclosureScope);
        var authorizingOperatorOrKernel = string.IsNullOrWhiteSpace(reviewRequest.SubmittedBy)
            ? "CradleTek"
            : reviewRequest.SubmittedBy.Trim();

        var package = new IssuedOfficePackage(
            PackageId: packageId,
            IssuanceLineageId: issuanceLineageId,
            OfficeKind: authorityReceipt.Office,
            OfficeInstanceId: officeInstanceId,
            ChassisClass: profile.ChassisClass,
            TargetRuntimeSurface: profile.TargetRuntimeSurface,
            IssuerSurface: profile.IssuerSurface,
            AuthorizingOperatorOrKernel: authorizingOperatorOrKernel,
            AuthorityScope: profile.AuthorityScope,
            AllowedActionCeiling: authorityReceipt.ActionEligibility,
            DisclosureCeiling: disclosureCeiling,
            BondRequirement: profile.BondRequirement,
            GuardedReviewRequirement: profile.GuardedReviewRequirement,
            RevocationConditions: profile.RevocationConditions,
            ToolAllowlist: profile.ToolAllowlist,
            ToolDenials: profile.ToolDenials,
            ExternalCallPolicy: profile.ExternalCallPolicy,
            MutationPermissions: profile.MutationPermissions,
            WorkerActivationPermissions: profile.WorkerActivationPermissions,
            RequiredPacketContracts: profile.RequiredPacketContracts,
            MountedMemoryLanes: profile.MountedMemoryLanes,
            ForbiddenMemoryLanes: profile.ForbiddenMemoryLanes,
            ContinuityLinkageSurface: profile.ContinuityLinkageSurface,
            ResiduePolicy: profile.ResiduePolicy,
            ParkReturnPolicy: profile.ParkReturnPolicy,
            SameOfficeLineageRule: profile.SameOfficeLineageRule,
            RequiredWitnessSurfaces: profile.RequiredWitnessSurfaces,
            RequiredReceipts: profile.RequiredReceipts,
            ReducedTelemetrySurface: profile.ReducedTelemetrySurface,
            GuardedTelemetrySurface: profile.GuardedTelemetrySurface,
            CrypticOrSealedSurfaces: profile.CrypticOrSealedSurfaces,
            IncidentEscalationPath: profile.IncidentEscalationPath,
            ReturnObligations: profile.ReturnObligations,
            RequiredReturnPacket: profile.RequiredReturnPacket,
            ParkEligibility: profile.ParkEligibility,
            QuarantineTriggers: profile.QuarantineTriggers,
            HandoffTerminationRule: profile.HandoffTerminationRule,
            ExpiryOrReissueWindow: profile.ExpiryOrReissueWindow,
            MaturityPosture: MaturityPosture.DoctrineBacked,
            ImplementationStatus: profile.ImplementationStatus,
            WithheldMechanicsMarker: profile.WithheldMechanicsMarker,
            ReviewOwner: profile.ReviewOwner,
            LastReviewedUtc: DoctrineReviewTimestampUtc);

        var issuanceHandle = LifecycleGovernanceKeys.CreateOfficeIssuanceHandle(
            loopKey,
            authorityReceipt.CMEId,
            authorityReceipt.Office,
            packageId,
            authorityReceipt.AuthorityHandle);
        var receipt = new GovernedOfficeIssuanceReceipt(
            IssuanceHandle: issuanceHandle,
            LoopKey: loopKey,
            Stage: authorityReceipt.Stage,
            CMEId: authorityReceipt.CMEId,
            Office: authorityReceipt.Office,
            ConstructClass: ConstructClass.IssuedOffice,
            PackageId: package.PackageId,
            IssuanceLineageId: package.IssuanceLineageId,
            OfficeInstanceId: package.OfficeInstanceId,
            AllowedActionCeiling: package.AllowedActionCeiling,
            DisclosureCeiling: package.DisclosureCeiling,
            MaturityPosture: package.MaturityPosture,
            OfficeAuthorityHandle: authorityReceipt.AuthorityHandle,
            WeatherDisclosureHandle: weatherDisclosureReceipt.DisclosureHandle,
            WitnessedBy: "CradleTek",
            TimestampUtc: authorityReceipt.TimestampUtc);

        return (package, receipt);
    }

    private static CompassVisibilityClass MapDisclosureCeiling(WeatherDisclosureScope scope)
    {
        return scope switch
        {
            WeatherDisclosureScope.Community => CompassVisibilityClass.CommunityLegible,
            WeatherDisclosureScope.Steward => CompassVisibilityClass.OperatorGuarded,
            WeatherDisclosureScope.OperatorGuarded => CompassVisibilityClass.OperatorGuarded,
            _ => CompassVisibilityClass.OperatorGuarded
        };
    }

    private sealed record IssuedOfficeProfile(
        string ChassisClass,
        string TargetRuntimeSurface,
        string IssuerSurface,
        string AuthorityScope,
        string BondRequirement,
        string GuardedReviewRequirement,
        IReadOnlyList<string> RevocationConditions,
        IReadOnlyList<string> ToolAllowlist,
        IReadOnlyList<string> ToolDenials,
        string ExternalCallPolicy,
        string MutationPermissions,
        string WorkerActivationPermissions,
        IReadOnlyList<string> RequiredPacketContracts,
        IReadOnlyList<string> MountedMemoryLanes,
        IReadOnlyList<string> ForbiddenMemoryLanes,
        string ContinuityLinkageSurface,
        string ResiduePolicy,
        string ParkReturnPolicy,
        string SameOfficeLineageRule,
        IReadOnlyList<string> RequiredWitnessSurfaces,
        IReadOnlyList<string> RequiredReceipts,
        string ReducedTelemetrySurface,
        string GuardedTelemetrySurface,
        IReadOnlyList<string> CrypticOrSealedSurfaces,
        string IncidentEscalationPath,
        IReadOnlyList<string> ReturnObligations,
        string RequiredReturnPacket,
        string ParkEligibility,
        IReadOnlyList<string> QuarantineTriggers,
        string HandoffTerminationRule,
        string ExpiryOrReissueWindow,
        string ImplementationStatus,
        string WithheldMechanicsMarker,
        string ReviewOwner);
}
