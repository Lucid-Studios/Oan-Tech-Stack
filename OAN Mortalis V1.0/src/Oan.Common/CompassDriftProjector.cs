namespace Oan.Common;

public static class CompassDriftProjector
{
    public const int MaxObservationWindow = 3;

    public static CompassDriftAssessment? ProjectForLoop(
        string loopKey,
        IReadOnlyList<GovernanceJournalEntry> entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(entries);

        var latestObservation = entries
            .Where(entry =>
                string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal) &&
                entry.CompassObservationReceipt is not null)
            .OrderBy(entry => entry.CompassObservationReceipt!.TimestampUtc)
            .ThenBy(entry => entry.CompassObservationReceipt!.WitnessHandle, StringComparer.Ordinal)
            .LastOrDefault();
        if (latestObservation?.CompassObservationReceipt is null)
        {
            return null;
        }

        var cmeIdByLoop = BuildCmeIdMap(entries);
        if (!cmeIdByLoop.TryGetValue(loopKey, out var cmeId) || string.IsNullOrWhiteSpace(cmeId))
        {
            return null;
        }

        var currentReceipt = latestObservation.CompassObservationReceipt;
        var familyWindow = entries
            .Where(entry =>
                entry.CompassObservationReceipt is not null &&
                cmeIdByLoop.TryGetValue(entry.LoopKey, out var entryCmeId) &&
                string.Equals(entryCmeId, cmeId, StringComparison.Ordinal) &&
                IsSameFamily(entry.CompassObservationReceipt, currentReceipt))
            .OrderBy(entry => entry.CompassObservationReceipt!.TimestampUtc)
            .ThenBy(entry => entry.CompassObservationReceipt!.WitnessHandle, StringComparer.Ordinal)
            .ToArray();
        if (familyWindow.Length == 0)
        {
            return null;
        }

        var window = familyWindow
            .Skip(Math.Max(0, familyWindow.Length - MaxObservationWindow))
            .Select(entry => entry.CompassObservationReceipt!)
            .ToArray();
        var baseline = window[0];
        var latest = window[^1];
        var observationHandles = window
            .Select(receipt => receipt.ObservationHandle)
            .ToArray();
        var advisoryDivergenceCount = window.Count(IsAdvisoryDivergent);
        var competingMigrationCount = window
            .Skip(1)
            .Count(receipt =>
                baseline.CompetingBasin != CompassDoctrineBasin.Unknown &&
                receipt.ActiveBasin == baseline.CompetingBasin);

        return new CompassDriftAssessment(
            CMEId: cmeId,
            WindowSize: MaxObservationWindow,
            ObservationCount: window.Length,
            BaselineActiveBasin: baseline.ActiveBasin,
            BaselineCompetingBasin: baseline.CompetingBasin,
            LatestActiveBasin: latest.ActiveBasin,
            LatestAnchorState: latest.AnchorState,
            DriftState: DetermineDriftState(
                baseline,
                latest,
                window.Length,
                advisoryDivergenceCount,
                competingMigrationCount),
            AdvisoryDivergenceCount: advisoryDivergenceCount,
            CompetingMigrationCount: competingMigrationCount,
            ObservationHandles: observationHandles,
            TimestampUtc: latest.TimestampUtc);
    }

    private static Dictionary<string, string> BuildCmeIdMap(IReadOnlyList<GovernanceJournalEntry> entries)
    {
        var cmeIdByLoop = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (entry.ReviewRequest is null || string.IsNullOrWhiteSpace(entry.ReviewRequest.CMEId))
            {
                continue;
            }

            cmeIdByLoop[entry.LoopKey] = entry.ReviewRequest.CMEId;
        }

        return cmeIdByLoop;
    }

    private static bool IsSameFamily(
        GovernedCompassObservationReceipt prior,
        GovernedCompassObservationReceipt current)
    {
        if (current.ActiveBasin == CompassDoctrineBasin.Unknown)
        {
            return prior.ActiveBasin == CompassDoctrineBasin.Unknown;
        }

        return prior.ActiveBasin == current.ActiveBasin ||
               prior.ActiveBasin == current.CompetingBasin ||
               prior.CompetingBasin == current.ActiveBasin;
    }

    private static bool IsAdvisoryDivergent(GovernedCompassObservationReceipt receipt) =>
        receipt.AdvisoryDisposition is CompassSeedAdvisoryDisposition.Deferred or CompassSeedAdvisoryDisposition.Rejected;

    private static CompassDriftState DetermineDriftState(
        GovernedCompassObservationReceipt baseline,
        GovernedCompassObservationReceipt latest,
        int observationCount,
        int advisoryDivergenceCount,
        int competingMigrationCount)
    {
        var crossedIntoCompetingBasin =
            baseline.CompetingBasin != CompassDoctrineBasin.Unknown &&
            latest.ActiveBasin == baseline.CompetingBasin;

        if (latest.AnchorState == CompassAnchorState.Lost ||
            crossedIntoCompetingBasin ||
            competingMigrationCount >= 2)
        {
            return CompassDriftState.Lost;
        }

        if (observationCount == 1 && latest.AnchorState == CompassAnchorState.Held)
        {
            return CompassDriftState.Held;
        }

        if (latest.ActiveBasin == baseline.ActiveBasin &&
            (latest.AnchorState == CompassAnchorState.Weakened ||
             competingMigrationCount == 1 ||
             advisoryDivergenceCount >= 2))
        {
            return CompassDriftState.Weakened;
        }

        if (latest.ActiveBasin == baseline.ActiveBasin &&
            latest.AnchorState != CompassAnchorState.Lost &&
            competingMigrationCount == 0 &&
            advisoryDivergenceCount <= 1)
        {
            return CompassDriftState.Held;
        }

        return latest.ActiveBasin != baseline.ActiveBasin
            ? CompassDriftState.Lost
            : CompassDriftState.Weakened;
    }
}
