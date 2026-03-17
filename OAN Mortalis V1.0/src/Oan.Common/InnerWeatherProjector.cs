namespace Oan.Common;

public static class InnerWeatherProjector
{
    public static InnerWeatherEvidence? ProjectForLoop(
        string loopKey,
        GovernanceJournalReplayBatch batch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopKey);
        ArgumentNullException.ThrowIfNull(batch);

        var entries = batch.Entries;
        var currentReviewRequest = entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.ReviewRequest)
            .LastOrDefault(request => request is not null);
        var currentObservation = entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.CompassObservationReceipt)
            .LastOrDefault(receipt => receipt is not null);
        var currentDrift = entries
            .Where(entry => string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal))
            .Select(entry => entry.CompassDriftReceipt)
            .LastOrDefault(receipt => receipt is not null);

        if (currentReviewRequest is null || currentObservation is null || currentDrift is null)
        {
            return null;
        }

        var reviewRequestsByLoop = entries
            .Where(entry => entry.ReviewRequest is not null)
            .GroupBy(entry => entry.LoopKey, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Last(entry => entry.ReviewRequest is not null).ReviewRequest!,
                StringComparer.Ordinal);
        var observationRecords = entries
            .Where(entry => entry.CompassObservationReceipt is not null)
            .Select(entry => new ObservationWindowRecord(entry.LoopKey, entry.CompassObservationReceipt!))
            .ToArray();
        var selectedObservations = SelectObservationWindow(
            currentReviewRequest.CMEId,
            currentDrift.ObservationHandles,
            reviewRequestsByLoop,
            observationRecords);

        if (selectedObservations.Count == 0)
        {
            return null;
        }

        var windowIntegrityState = ClassifyWindowIntegrity(
            loopKey,
            batch,
            currentReviewRequest,
            currentDrift,
            selectedObservations,
            reviewRequestsByLoop);
        var shellCompetitionState = ClassifyShellCompetitionState(currentDrift, selectedObservations.Count, windowIntegrityState);
        var hotCoolContactState = ClassifyHotCoolContactState(currentObservation, currentDrift, selectedObservations.Count, windowIntegrityState);
        var residueContributors = BuildResidueContributors(currentDrift, shellCompetitionState, hotCoolContactState, windowIntegrityState);
        var residueState = ClassifyResidueState(currentDrift, shellCompetitionState, hotCoolContactState, windowIntegrityState, selectedObservations.Count);
        var residueVisibilityClass = MapResidueVisibilityClass(residueState);
        var shellCompetitionVisibilityClass = MapShellCompetitionVisibilityClass(shellCompetitionState);
        var hotCoolContactVisibilityClass = MapHotCoolVisibilityClass(hotCoolContactState);
        var stewardAttentionCauses = BuildStewardAttentionCauses(currentDrift, residueState, shellCompetitionState, hotCoolContactState, windowIntegrityState);

        return new InnerWeatherEvidence(
            CMEId: currentReviewRequest.CMEId,
            ActiveBasin: currentObservation.ActiveBasin,
            CompetingBasin: currentObservation.CompetingBasin,
            DriftState: currentDrift.DriftState,
            WindowSize: currentDrift.WindowSize,
            ObservationCount: selectedObservations.Count,
            WindowIntegrityState: windowIntegrityState,
            Residue: new CompassResidueAssessment(
                ResidueState: residueState,
                VisibilityClass: residueVisibilityClass,
                Contributors: residueContributors),
            ShellCompetition: new ShellCompetitionAssessment(
                CompetitionState: shellCompetitionState,
                VisibilityClass: shellCompetitionVisibilityClass),
            HotCoolContactState: hotCoolContactState,
            HotCoolContactVisibilityClass: hotCoolContactVisibilityClass,
            StewardAttentionCauses: stewardAttentionCauses,
            DriftHandle: currentDrift.DriftHandle,
            ObservationHandles: selectedObservations
                .Select(record => record.Receipt.ObservationHandle)
                .ToArray(),
            TimestampUtc: MaxTimestamp(currentObservation.TimestampUtc, currentDrift.TimestampUtc));
    }

    private static IReadOnlyList<ObservationWindowRecord> SelectObservationWindow(
        string cmeId,
        IReadOnlyList<string> observationHandles,
        IReadOnlyDictionary<string, ReturnCandidateReviewRequest> reviewRequestsByLoop,
        IReadOnlyList<ObservationWindowRecord> observationRecords)
    {
        var handleSet = observationHandles
            .Where(handle => !string.IsNullOrWhiteSpace(handle))
            .ToHashSet(StringComparer.Ordinal);

        return observationRecords
            .Where(record =>
                handleSet.Contains(record.Receipt.ObservationHandle) &&
                reviewRequestsByLoop.TryGetValue(record.LoopKey, out var reviewRequest) &&
                string.Equals(reviewRequest.CMEId, cmeId, StringComparison.Ordinal))
            .OrderBy(record => record.Receipt.TimestampUtc)
            .ThenBy(record => record.Receipt.WitnessHandle, StringComparer.Ordinal)
            .ToArray();
    }

    private static WindowIntegrityState ClassifyWindowIntegrity(
        string loopKey,
        GovernanceJournalReplayBatch batch,
        ReturnCandidateReviewRequest currentReviewRequest,
        GovernedCompassDriftReceipt currentDrift,
        IReadOnlyList<ObservationWindowRecord> selectedObservations,
        IReadOnlyDictionary<string, ReturnCandidateReviewRequest> reviewRequestsByLoop)
    {
        var currentLoopEnvelopeIds = batch.Entries
            .Where(entry =>
                string.Equals(entry.LoopKey, loopKey, StringComparison.Ordinal) &&
                entry.ReviewRequest is not null)
            .Select(entry => entry.ReviewRequest!.RequestEnvelope.EnvelopeId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (currentLoopEnvelopeIds.Length > 1)
        {
            return WindowIntegrityState.GovernanceReset;
        }

        if (reviewRequestsByLoop.Values.Any(reviewRequest =>
                !string.Equals(reviewRequest.CMEId, currentReviewRequest.CMEId, StringComparison.Ordinal) &&
                string.Equals(reviewRequest.SessionHandle, currentReviewRequest.SessionHandle, StringComparison.Ordinal)))
        {
            return WindowIntegrityState.CmeReselected;
        }

        if (selectedObservations
            .GroupBy(record => record.Receipt.WorkingStateHandle, StringComparer.Ordinal)
            .Any(group => group.Select(record => record.LoopKey).Distinct(StringComparer.Ordinal).Count() > 1))
        {
            return WindowIntegrityState.RuntimeRestart;
        }

        var currentVisibilityClass = MapProtectionClass(currentReviewRequest.RequestEnvelope.ProtectionClass);
        var selectedVisibilityClasses = selectedObservations
            .Select(record => reviewRequestsByLoop.TryGetValue(record.LoopKey, out var request)
                ? MapProtectionClass(request.RequestEnvelope.ProtectionClass)
                : CompassVisibilityClass.CrypticOnly)
            .ToArray();
        if (selectedVisibilityClasses.Any(visibilityClass => visibilityClass < currentVisibilityClass))
        {
            return WindowIntegrityState.VisibilityDowngraded;
        }

        var selectedLoopKeys = selectedObservations
            .Select(record => record.LoopKey)
            .ToHashSet(StringComparer.Ordinal);
        var hasJournalGap = batch.Issues.Any(issue =>
            string.IsNullOrWhiteSpace(issue.LoopKey) ||
            selectedLoopKeys.Contains(issue.LoopKey));
        if (hasJournalGap || selectedObservations.Count < currentDrift.ObservationHandles.Count)
        {
            return WindowIntegrityState.JournalGap;
        }

        if (selectedObservations.Count < currentDrift.WindowSize)
        {
            return WindowIntegrityState.Sparse;
        }

        return WindowIntegrityState.Intact;
    }

    private static ShellCompetitionState ClassifyShellCompetitionState(
        GovernedCompassDriftReceipt currentDrift,
        int observationCount,
        WindowIntegrityState windowIntegrityState)
    {
        if (observationCount == 0)
        {
            return ShellCompetitionState.Unknown;
        }

        if (currentDrift.CompetingMigrationCount >= 2)
        {
            return ShellCompetitionState.Rising;
        }

        if (currentDrift.CompetingMigrationCount == 1)
        {
            return ShellCompetitionState.Present;
        }

        if (windowIntegrityState == WindowIntegrityState.Sparse && observationCount < currentDrift.WindowSize)
        {
            return ShellCompetitionState.Unknown;
        }

        return ShellCompetitionState.Absent;
    }

    private static HotCoolContactState ClassifyHotCoolContactState(
        GovernedCompassObservationReceipt currentObservation,
        GovernedCompassDriftReceipt currentDrift,
        int observationCount,
        WindowIntegrityState windowIntegrityState)
    {
        if (windowIntegrityState is WindowIntegrityState.JournalGap
            or WindowIntegrityState.RuntimeRestart
            or WindowIntegrityState.CmeReselected
            or WindowIntegrityState.VisibilityDowngraded
            or WindowIntegrityState.GovernanceReset)
        {
            return HotCoolContactState.MissedCheckIn;
        }

        if (string.IsNullOrWhiteSpace(currentObservation.ValidationReferenceHandle))
        {
            return observationCount < currentDrift.WindowSize
                ? HotCoolContactState.Unknown
                : HotCoolContactState.MissedCheckIn;
        }

        if (currentObservation.SelfTouchClass == CompassSelfTouchClass.NoTouch &&
            currentDrift.DriftState == CompassDriftState.Held)
        {
            return HotCoolContactState.Cool;
        }

        return HotCoolContactState.InContact;
    }

    private static IReadOnlyList<AttentionResidueContributor> BuildResidueContributors(
        GovernedCompassDriftReceipt currentDrift,
        ShellCompetitionState shellCompetitionState,
        HotCoolContactState hotCoolContactState,
        WindowIntegrityState windowIntegrityState)
    {
        var contributors = new List<AttentionResidueContributor>();

        if (currentDrift.AdvisoryDivergenceCount > 0)
        {
            contributors.Add(AttentionResidueContributor.AdvisoryDivergence);
        }

        if (shellCompetitionState is ShellCompetitionState.Present or ShellCompetitionState.Rising)
        {
            contributors.Add(AttentionResidueContributor.CompetingPressure);
        }

        if (currentDrift.DriftState is CompassDriftState.Weakened or CompassDriftState.Lost)
        {
            contributors.Add(AttentionResidueContributor.DriftInstability);
        }

        if (windowIntegrityState != WindowIntegrityState.Intact)
        {
            contributors.Add(AttentionResidueContributor.WindowIntegrityBreak);
        }

        if (hotCoolContactState == HotCoolContactState.MissedCheckIn)
        {
            contributors.Add(AttentionResidueContributor.ContactCadenceMissed);
        }

        return contributors.Distinct().ToArray();
    }

    private static AttentionResidueState ClassifyResidueState(
        GovernedCompassDriftReceipt currentDrift,
        ShellCompetitionState shellCompetitionState,
        HotCoolContactState hotCoolContactState,
        WindowIntegrityState windowIntegrityState,
        int observationCount)
    {
        if (currentDrift.DriftState == CompassDriftState.Lost ||
            shellCompetitionState == ShellCompetitionState.Rising ||
            windowIntegrityState is WindowIntegrityState.RuntimeRestart
                or WindowIntegrityState.CmeReselected
                or WindowIntegrityState.GovernanceReset)
        {
            return AttentionResidueState.Escalating;
        }

        if (currentDrift.ObservationCount >= currentDrift.WindowSize &&
            (currentDrift.AdvisoryDivergenceCount >= 2 ||
             currentDrift.DriftState == CompassDriftState.Weakened ||
             windowIntegrityState == WindowIntegrityState.JournalGap))
        {
            return AttentionResidueState.Persistent;
        }

        if (currentDrift.AdvisoryDivergenceCount >= 2 ||
            shellCompetitionState == ShellCompetitionState.Present ||
            currentDrift.DriftState == CompassDriftState.Weakened ||
            windowIntegrityState == WindowIntegrityState.VisibilityDowngraded ||
            hotCoolContactState == HotCoolContactState.MissedCheckIn)
        {
            return AttentionResidueState.Present;
        }

        if (currentDrift.AdvisoryDivergenceCount == 1 ||
            observationCount < currentDrift.WindowSize ||
            windowIntegrityState == WindowIntegrityState.Sparse)
        {
            return AttentionResidueState.Low;
        }

        return AttentionResidueState.None;
    }

    private static CompassVisibilityClass MapResidueVisibilityClass(AttentionResidueState residueState) =>
        residueState switch
        {
            AttentionResidueState.Escalating => CompassVisibilityClass.CrypticOnly,
            AttentionResidueState.Present or AttentionResidueState.Persistent => CompassVisibilityClass.OperatorGuarded,
            _ => CompassVisibilityClass.CommunityLegible
        };

    private static CompassVisibilityClass MapShellCompetitionVisibilityClass(ShellCompetitionState shellCompetitionState) =>
        shellCompetitionState switch
        {
            ShellCompetitionState.Rising => CompassVisibilityClass.CrypticOnly,
            ShellCompetitionState.Present => CompassVisibilityClass.OperatorGuarded,
            _ => CompassVisibilityClass.CommunityLegible
        };

    private static CompassVisibilityClass MapHotCoolVisibilityClass(HotCoolContactState hotCoolContactState) =>
        hotCoolContactState switch
        {
            HotCoolContactState.Cool => CompassVisibilityClass.OperatorGuarded,
            _ => CompassVisibilityClass.CommunityLegible
        };

    private static IReadOnlyList<StewardAttentionCause> BuildStewardAttentionCauses(
        GovernedCompassDriftReceipt currentDrift,
        AttentionResidueState residueState,
        ShellCompetitionState shellCompetitionState,
        HotCoolContactState hotCoolContactState,
        WindowIntegrityState windowIntegrityState)
    {
        var causes = new List<StewardAttentionCause>();

        if (currentDrift.DriftState == CompassDriftState.Weakened)
        {
            causes.Add(StewardAttentionCause.DriftWeakening);
        }

        if (currentDrift.DriftState == CompassDriftState.Lost)
        {
            causes.Add(StewardAttentionCause.DriftLoss);
        }

        if (residueState is AttentionResidueState.Persistent or AttentionResidueState.Escalating)
        {
            causes.Add(StewardAttentionCause.ResiduePersistence);
        }

        if (shellCompetitionState is ShellCompetitionState.Present or ShellCompetitionState.Rising)
        {
            causes.Add(StewardAttentionCause.ShellCompetition);
        }

        if (hotCoolContactState == HotCoolContactState.MissedCheckIn)
        {
            causes.Add(StewardAttentionCause.MissedCheckIn);
        }

        if (windowIntegrityState != WindowIntegrityState.Intact)
        {
            causes.Add(StewardAttentionCause.WindowIntegrityBreak);
        }

        return causes.Distinct().ToArray();
    }

    private static CompassVisibilityClass MapProtectionClass(string? protectionClass)
    {
        if (string.IsNullOrWhiteSpace(protectionClass))
        {
            return CompassVisibilityClass.CrypticOnly;
        }

        var normalized = protectionClass.Trim().ToLowerInvariant();
        if (normalized.Contains("community", StringComparison.Ordinal))
        {
            return CompassVisibilityClass.CommunityLegible;
        }

        if (normalized.Contains("guarded", StringComparison.Ordinal) ||
            normalized.Contains("operator", StringComparison.Ordinal))
        {
            return CompassVisibilityClass.OperatorGuarded;
        }

        return CompassVisibilityClass.CrypticOnly;
    }

    private static DateTimeOffset MaxTimestamp(DateTimeOffset left, DateTimeOffset right) =>
        left >= right ? left : right;

    private sealed record ObservationWindowRecord(
        string LoopKey,
        GovernedCompassObservationReceipt Receipt);
}
