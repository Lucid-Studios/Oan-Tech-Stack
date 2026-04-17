namespace San.Common;

public static class AgentiActualizationStandingProjector
{
    public static AgentiActualizationStandingProjection BuildStandingProjection(
        DurabilityWitnessReceipt? durability,
        ColdAdmissionEligibilityGateReceipt? coldAdmission,
        InterlockDensityLedgerReceipt? interlockDensity,
        CoreInvariantLatticeWitnessReceipt? coreInvariant)
    {
        var missingReceipts = new List<string>();
        var blockingReasons = new List<string>();
        var nonBlockingReasons = new List<string>();
        var flags = new List<string>();

        var durabilityPresent = durability is not null;
        var coldAdmissionPresent = coldAdmission is not null;
        var interlockDensityPresent = interlockDensity is not null;
        var coreInvariantPresent = coreInvariant is not null;
        var anyReceiptsPresent = durabilityPresent || coldAdmissionPresent || interlockDensityPresent || coreInvariantPresent;

        if (!durabilityPresent)
        {
            missingReceipts.Add("durability-witness-receipt");
        }

        if (!coldAdmissionPresent)
        {
            missingReceipts.Add("cold-admission-gate-receipt");
        }

        if (!interlockDensityPresent)
        {
            missingReceipts.Add("interlock-density-ledger-receipt");
        }

        if (!coreInvariantPresent)
        {
            missingReceipts.Add("core-invariant-lattice-receipt");
        }

        var durableUnderVariation = durability?.DurableUnderVariation == true;
        var coldApproachLawful = coldAdmission?.ColdApproachLawful == true;
        var finalInheritanceStillWithheld = coldAdmission?.FinalInheritanceStillWithheld == true;
        var denseInterweaveEmergent = interlockDensity?.DenseInterweaveEmergent == true;
        var identityAdjacentSignificanceEmergent = coreInvariant?.IdentityAdjacentSignificanceEmergent == true;
        var latticeGradeInvarianceWitnessed = coreInvariant?.LatticeGradeInvarianceWitnessed == true;
        var coreLawSanctificationDenied = coreInvariant?.CoreLawSanctificationDenied == true;

        if (durabilityPresent && !durableUnderVariation)
        {
            blockingReasons.Add("durability-under-variation-absent");
        }

        if (coldAdmissionPresent && !coldApproachLawful)
        {
            blockingReasons.Add("cold-approach-not-lawful");
        }

        if (interlockDensityPresent && !denseInterweaveEmergent)
        {
            blockingReasons.Add("dense-interweave-not-emergent");
        }

        if (coreInvariantPresent && !identityAdjacentSignificanceEmergent)
        {
            blockingReasons.Add("identity-adjacent-significance-not-emergent");
        }

        if (coreInvariantPresent && !latticeGradeInvarianceWitnessed)
        {
            blockingReasons.Add("lattice-grade-invariance-not-witnessed");
        }

        if (finalInheritanceStillWithheld)
        {
            nonBlockingReasons.Add("final-inheritance-still-withheld");
        }

        if (coreLawSanctificationDenied)
        {
            nonBlockingReasons.Add("core-law-sanctification-denied");
        }

        var status = anyReceiptsPresent switch
        {
            false => ActualizationStandingStatus.Missing,
            true when blockingReasons.Count > 0 => ActualizationStandingStatus.Blocked,
            true when missingReceipts.Count > 0 => ActualizationStandingStatus.Partial,
            true when durableUnderVariation &&
                      coldApproachLawful &&
                      denseInterweaveEmergent &&
                      identityAdjacentSignificanceEmergent &&
                      latticeGradeInvarianceWitnessed => ActualizationStandingStatus.Standing,
            _ => ActualizationStandingStatus.Partial
        };

        if (missingReceipts.Count > 0)
        {
            flags.Add("promotion-receipts-missing");
        }

        return new AgentiActualizationStandingProjection(
            Status: status,
            DurabilityWitnessPresent: durabilityPresent,
            DurableUnderVariation: durableUnderVariation,
            ColdAdmissionGatePresent: coldAdmissionPresent,
            ColdApproachLawful: coldApproachLawful,
            FinalInheritanceStillWithheld: finalInheritanceStillWithheld,
            InterlockDensityReceiptPresent: interlockDensityPresent,
            DenseInterweaveEmergent: denseInterweaveEmergent,
            CoreInvariantLatticeReceiptPresent: coreInvariantPresent,
            IdentityAdjacentSignificanceEmergent: identityAdjacentSignificanceEmergent,
            LatticeGradeInvarianceWitnessed: latticeGradeInvarianceWitnessed,
            CoreLawSanctificationDenied: coreLawSanctificationDenied,
            MissingReceipts: NormalizeReasons(missingReceipts),
            BlockingReasons: NormalizeReasons(blockingReasons),
            NonBlockingReasons: NormalizeReasons(nonBlockingReasons),
            Flags: NormalizeReasons(flags));
    }

    public static AgentiActualizationStandingProjection CreateMissingProjection() =>
        new(
            Status: ActualizationStandingStatus.Missing,
            DurabilityWitnessPresent: false,
            DurableUnderVariation: false,
            ColdAdmissionGatePresent: false,
            ColdApproachLawful: false,
            FinalInheritanceStillWithheld: false,
            InterlockDensityReceiptPresent: false,
            DenseInterweaveEmergent: false,
            CoreInvariantLatticeReceiptPresent: false,
            IdentityAdjacentSignificanceEmergent: false,
            LatticeGradeInvarianceWitnessed: false,
            CoreLawSanctificationDenied: false,
            MissingReceipts:
            [
                "durability-witness-receipt",
                "cold-admission-gate-receipt",
                "interlock-density-ledger-receipt",
                "core-invariant-lattice-receipt"
            ],
            BlockingReasons: [],
            NonBlockingReasons: [],
            Flags: ["promotion-receipts-missing"]);

    private static IReadOnlyList<string> NormalizeReasons(IEnumerable<string> reasons) =>
        reasons
            .Where(static reason => !string.IsNullOrWhiteSpace(reason))
            .Select(static reason => reason.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static reason => reason, StringComparer.Ordinal)
            .ToArray();
}
