namespace San.Common;

public static class PersonificationStandingEvaluator
{
    private const string DefaultDecisionAuthority = "personification-standing-evaluator";

    public static NormalizedStandingSources NormalizeStandingSources(
        CompassChamberEvidenceRecord chamber,
        IReadOnlyList<GoverningOfficeAuthorityAssessment>? officeAuthority,
        AgentiActualizationStandingProjection? actualization,
        IReadOnlyList<string>? observedJobSets,
        IReadOnlyList<string>? observedCareerSignals)
    {
        ArgumentNullException.ThrowIfNull(chamber);

        var normalizedOfficeAuthority = officeAuthority ?? [];
        var normalizedActualization = actualization ?? AgentiActualizationStandingProjector.CreateMissingProjection();
        var jobsObserved = HasObservedSignals(observedJobSets);
        var careerSignalsObserved = HasObservedSignals(observedCareerSignals);
        var chamberBucket = NormalizeChamber(chamber);
        var office = NormalizeOffice(normalizedOfficeAuthority);
        // Actualization-derived truth must be separated into bonded-continuity and append-only-identity
        // facets before claim materialization; no helper may infer one from the standing of the other.
        var bondedContinuity = NormalizeBondedContinuity(normalizedActualization);
        var appendOnlyIdentity = NormalizeAppendOnlyIdentity(normalizedActualization);
        var anyBondedConfirmed = normalizedOfficeAuthority.Any(static assessment => assessment.BondedConfirmed);
        var missingReceipts = MergeMissingReceipts(
            chamber.MissingReceipts,
            normalizedActualization.MissingReceipts,
            normalizedOfficeAuthority.Count == 0 ? ["governing-office-authority-assessment"] : []);

        return new NormalizedStandingSources(
            Chamber: chamberBucket,
            Office: office,
            BondedContinuity: bondedContinuity,
            AppendOnlyIdentity: appendOnlyIdentity,
            JobsObserved: jobsObserved,
            CareerSignalsObserved: careerSignalsObserved,
            AnyBondedConfirmed: anyBondedConfirmed,
            MissingReceipts: missingReceipts,
            VisibleGuardrails: ExtractVisibleGuardrails(normalizedActualization));
    }

    /// <summary>
    /// Standing is a witnessed evidentiary determination of present class truth.
    /// It is not inheritance, publication, sanctification, or sovereign permission.
    /// </summary>
    public static PersonificationStandingEvaluation EvaluatePersonificationStanding(
        PersonificationClass currentClass,
        NormalizedStandingSources sources,
        string? decisionAuthority = null)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var claims = MaterializeClaimEvaluations(sources);
        var currentGate = PersonificationStandingGrammar.Gates[currentClass];
        var currentOutcome = ApplyCurrentClassGate(currentGate, claims, sources);
        var promotionOutcome = EvaluatePromotionTargets(currentClass, claims, sources);

        return FreezeStandingEvaluation(
            currentClass,
            claims,
            currentOutcome,
            promotionOutcome,
            sources,
            decisionAuthority);
    }

    // Chamber normalization may emit contradiction, deferral, and satisfaction,
    // but must not infer standing from absence.
    private static StandingSourceBucket NormalizeChamber(CompassChamberEvidenceRecord chamber)
    {
        var satisfied = new List<string>();
        var contradicted = new List<string>();
        var deferred = new List<string>();
        var visible = new List<string>();

        if (chamber.TracePresent)
        {
            satisfied.Add("trace-present");
        }
        else
        {
            contradicted.Add("trace-no-core-window");
        }

        switch (chamber.EvidenceSufficiency)
        {
            case ChamberEvidenceSufficiencyState.Sufficient:
                satisfied.Add("evidence-sufficient");
                break;
            case ChamberEvidenceSufficiencyState.Partial:
                deferred.Add("evidence-partial");
                break;
            default:
                contradicted.Add("evidence-broken-window");
                break;
        }

        switch (chamber.WindowIntegrity)
        {
            case ChamberWindowIntegrityState.Intact:
                satisfied.Add("window-intact");
                break;
            case ChamberWindowIntegrityState.Ambiguous:
                contradicted.Add("continuity-ambiguous");
                break;
            case ChamberWindowIntegrityState.Broken:
                contradicted.Add("evidence-broken-window");
                break;
        }

        visible.AddRange(chamber.Reasons);

        var standing = chamber.TracePresent &&
                       chamber.EvidenceSufficiency == ChamberEvidenceSufficiencyState.Sufficient &&
                       chamber.WindowIntegrity == ChamberWindowIntegrityState.Intact &&
                       !contradicted.Contains("evidence-broken-window", StringComparer.Ordinal) &&
                       !contradicted.Contains("trace-no-core-window", StringComparer.Ordinal) &&
                       !contradicted.Contains("continuity-ambiguous", StringComparer.Ordinal);

        return new StandingSourceBucket(
            Standing: standing,
            SatisfiedReasons: NormalizeReasons(satisfied),
            ContradictedReasons: NormalizeReasons(contradicted),
            DeferredReasons: NormalizeReasons(deferred),
            VisibleReasons: NormalizeReasons(visible));
    }

    private static StandingSourceBucket NormalizeOffice(
        IReadOnlyList<GoverningOfficeAuthorityAssessment> officeAuthority)
    {
        var anyOfficeAttached = officeAuthority.Any(static assessment => assessment.OfficeAttached);
        var anyOfficeViewStanding = officeAuthority.Any(static assessment =>
            assessment.ViewEligibility != OfficeViewEligibility.Withheld);
        var anyOfficeActionAtLeastAcknowledge = officeAuthority.Any(static assessment =>
            assessment.ActionEligibility >= OfficeActionEligibility.AcknowledgeAllowed);
        var anyBondedConfirmed = officeAuthority.Any(static assessment => assessment.BondedConfirmed);

        var satisfiedReasons = new List<string>();
        var contradictedReasons = new List<string>();
        var deferredReasons = new List<string>();
        var visibleReasons = new List<string>();

        if (officeAuthority.Count > 0)
        {
            visibleReasons.Add("office-authority-observed");
        }

        if (anyOfficeAttached)
        {
            satisfiedReasons.Add("office-attached");
        }
        else if (officeAuthority.Count > 0)
        {
            contradictedReasons.Add("office-not-attached");
        }

        if (anyOfficeViewStanding)
        {
            satisfiedReasons.Add("office-view-standing");
        }
        else if (officeAuthority.Any(static assessment => assessment.ViewEligibility == OfficeViewEligibility.Withheld))
        {
            deferredReasons.Add("office-view-withheld");
        }

        if (anyOfficeActionAtLeastAcknowledge)
        {
            satisfiedReasons.Add("predicate-office-standing");
        }
        else if (officeAuthority.Count > 0)
        {
            contradictedReasons.Add("office-action-below-acknowledge");
        }

        if (anyBondedConfirmed)
        {
            visibleReasons.Add("bond-confirmation-present");
        }

        return new StandingSourceBucket(
            Standing: anyOfficeAttached && anyOfficeViewStanding && anyOfficeActionAtLeastAcknowledge,
            SatisfiedReasons: NormalizeReasons(satisfiedReasons),
            ContradictedReasons: NormalizeReasons(contradictedReasons),
            DeferredReasons: NormalizeReasons(deferredReasons),
            VisibleReasons: NormalizeReasons(visibleReasons));
    }

    private static StandingSourceBucket NormalizeBondedContinuity(
        AgentiActualizationStandingProjection projection) =>
        new(
            Standing: projection.DurableUnderVariation &&
                      projection.ColdApproachLawful &&
                      projection.DenseInterweaveEmergent,
            SatisfiedReasons: NormalizeReasons(GetBondedContinuitySatisfiedReasons(projection)),
            ContradictedReasons: NormalizeReasons(GetBondedContinuityContradictedReasons(projection)),
            DeferredReasons: NormalizeReasons(GetBondedContinuityDeferredReasons(projection)),
            VisibleReasons: []);

    private static StandingSourceBucket NormalizeAppendOnlyIdentity(
        AgentiActualizationStandingProjection projection) =>
        new(
            Standing: projection.IdentityAdjacentSignificanceEmergent &&
                      projection.LatticeGradeInvarianceWitnessed,
            SatisfiedReasons: NormalizeReasons(GetAppendOnlyIdentitySatisfiedReasons(projection)),
            ContradictedReasons: NormalizeReasons(GetAppendOnlyIdentityContradictedReasons(projection)),
            DeferredReasons: NormalizeReasons(GetAppendOnlyIdentityDeferredReasons(projection)),
            VisibleReasons: []);

    private static IReadOnlyList<string> MergeMissingReceipts(params IEnumerable<string>[] receiptSets) =>
        NormalizeReasons(receiptSets.SelectMany(static receipts => receipts ?? []));

    private static IReadOnlyList<string> ExtractVisibleGuardrails(
        AgentiActualizationStandingProjection projection) =>
        NormalizeReasons(projection.NonBlockingReasons);

    // Claim materialization is source-derived and class-agnostic.
    // Gate application is class-specific and consumes claim evaluations only.
    private static IReadOnlyList<PersonificationClaimEvaluation> MaterializeClaimEvaluations(
        NormalizedStandingSources sources)
    {
        return
        [
            EvaluateOeCoeParticipation(sources),
            EvaluateChamberContinuityStanding(sources),
            EvaluateHoldJobs(sources),
            EvaluateAccrueBoundedCareerContinuity(sources),
            EvaluatePredicateOfficeStanding(sources),
            EvaluateGovernanceFacingCareerContinuity(sources),
            EvaluateBondedContinuityStanding(sources),
            EvaluateAppendOnlyIdentityStanding(sources),
            EvaluateFullPersonificationStanding(sources),
            EvaluateInheritanceStillWithheld(sources),
            EvaluateCoreLawSanctificationDenied(sources)
        ];
    }

    private static PersonificationClaimEvaluation EvaluateOeCoeParticipation(NormalizedStandingSources sources) =>
        CreateClaim(
            PersonificationClaimKind.OeCoeParticipation,
            standing: sources.Chamber.Standing,
            visible: HasAnyReason(sources.Chamber),
            satisfiedBy: sources.Chamber.SatisfiedReasons,
            contradictedBy: sources.Chamber.ContradictedReasons
                .Where(static reason =>
                    reason is "trace-no-core-window" or "borrowed-markers-denied" or "evidence-broken-window"),
            deferredBy: [],
            visibleBy: sources.Chamber.VisibleReasons);

    private static PersonificationClaimEvaluation EvaluateChamberContinuityStanding(NormalizedStandingSources sources) =>
        CreateClaim(
            PersonificationClaimKind.ChamberContinuityStanding,
            standing: sources.Chamber.Standing,
            visible: HasAnyReason(sources.Chamber),
            satisfiedBy: sources.Chamber.SatisfiedReasons,
            contradictedBy: sources.Chamber.ContradictedReasons
                .Where(static reason => reason is "evidence-broken-window" or "continuity-ambiguous"),
            deferredBy: sources.Chamber.DeferredReasons,
            visibleBy: sources.Chamber.VisibleReasons);

    private static PersonificationClaimEvaluation EvaluateHoldJobs(NormalizedStandingSources sources)
    {
        var satisfiedBy = new List<string>();
        var contradictedBy = new List<string>();
        var deferredBy = new List<string>();

        if (sources.Chamber.Standing)
        {
            satisfiedBy.Add("chamber-continuity-standing");
        }
        else
        {
            contradictedBy.Add("chamber-not-standing");
        }

        if (sources.JobsObserved)
        {
            satisfiedBy.Add("jobs-observed");
        }
        else
        {
            deferredBy.Add("job-set-unobserved");
        }

        return CreateClaim(
            PersonificationClaimKind.HoldJobs,
            standing: sources.Chamber.Standing && sources.JobsObserved,
            visible: sources.JobsObserved || HasAnyReason(sources.Chamber),
            satisfiedBy: satisfiedBy,
            contradictedBy: contradictedBy,
            deferredBy: deferredBy,
            visibleBy: []);
    }

    private static PersonificationClaimEvaluation EvaluateAccrueBoundedCareerContinuity(
        NormalizedStandingSources sources)
    {
        var satisfiedBy = sources.CareerSignalsObserved
            ? ["bounded-career-signals-observed"]
            : Array.Empty<string>();

        var deferredBy = sources.CareerSignalsObserved
            ? Array.Empty<string>()
            : ["career-signals-absent"];

        return CreateClaim(
            PersonificationClaimKind.AccrueBoundedCareerContinuity,
            standing: sources.CareerSignalsObserved,
            visible: sources.CareerSignalsObserved,
            satisfiedBy: satisfiedBy,
            contradictedBy: [],
            deferredBy: deferredBy,
            visibleBy: []);
    }

    private static PersonificationClaimEvaluation EvaluatePredicateOfficeStanding(NormalizedStandingSources sources) =>
        CreateClaim(
            PersonificationClaimKind.PredicateOfficeStanding,
            standing: sources.Office.Standing,
            visible: HasAnyReason(sources.Office),
            satisfiedBy: sources.Office.SatisfiedReasons,
            contradictedBy: sources.Office.ContradictedReasons,
            deferredBy: sources.Office.DeferredReasons,
            visibleBy: sources.Office.VisibleReasons);

    private static PersonificationClaimEvaluation EvaluateGovernanceFacingCareerContinuity(
        NormalizedStandingSources sources)
    {
        var satisfiedBy = new List<string>();
        var contradictedBy = new List<string>();
        var deferredBy = new List<string>();

        if (sources.Office.Standing)
        {
            satisfiedBy.Add("predicate-office-standing");
        }
        else if (sources.Office.ContradictedReasons.Count > 0)
        {
            contradictedBy.AddRange(sources.Office.ContradictedReasons);
        }
        else if (sources.Office.DeferredReasons.Count > 0)
        {
            deferredBy.AddRange(sources.Office.DeferredReasons);
        }

        if (sources.CareerSignalsObserved)
        {
            satisfiedBy.Add("governance-career-signals-observed");
        }
        else
        {
            deferredBy.Add("governance-career-signals-absent");
        }

        return CreateClaim(
            PersonificationClaimKind.GovernanceFacingCareerContinuity,
            standing: sources.Office.Standing && sources.CareerSignalsObserved,
            visible: HasAnyReason(sources.Office) || sources.CareerSignalsObserved,
            satisfiedBy: satisfiedBy,
            contradictedBy: contradictedBy,
            deferredBy: deferredBy,
            visibleBy: []);
    }

    private static PersonificationClaimEvaluation EvaluateBondedContinuityStanding(
        NormalizedStandingSources sources) =>
        CreateClaim(
            PersonificationClaimKind.BondedContinuityStanding,
            standing: sources.BondedContinuity.Standing,
            visible: HasAnyReason(sources.BondedContinuity),
            satisfiedBy: sources.BondedContinuity.SatisfiedReasons,
            contradictedBy: sources.BondedContinuity.ContradictedReasons,
            deferredBy: sources.BondedContinuity.DeferredReasons,
            visibleBy: sources.BondedContinuity.VisibleReasons);

    private static PersonificationClaimEvaluation EvaluateAppendOnlyIdentityStanding(
        NormalizedStandingSources sources) =>
        CreateClaim(
            PersonificationClaimKind.AppendOnlyIdentityStanding,
            standing: sources.AppendOnlyIdentity.Standing,
            visible: HasAnyReason(sources.AppendOnlyIdentity),
            satisfiedBy: sources.AppendOnlyIdentity.SatisfiedReasons,
            contradictedBy: sources.AppendOnlyIdentity.ContradictedReasons,
            deferredBy: sources.AppendOnlyIdentity.DeferredReasons,
            visibleBy: sources.AppendOnlyIdentity.VisibleReasons);

    private static PersonificationClaimEvaluation EvaluateFullPersonificationStanding(
        NormalizedStandingSources sources)
    {
        var satisfiedBy = new List<string>();
        var deferredBy = new List<string>();

        if (sources.BondedContinuity.Standing)
        {
            satisfiedBy.Add("bonded-continuity-standing");
        }
        else
        {
            deferredBy.AddRange(sources.BondedContinuity.DeferredReasons);
        }

        if (sources.AppendOnlyIdentity.Standing)
        {
            satisfiedBy.Add("append-only-identity-standing");
        }
        else
        {
            deferredBy.AddRange(sources.AppendOnlyIdentity.DeferredReasons);
        }

        if (sources.AnyBondedConfirmed)
        {
            satisfiedBy.Add("bond-confirmation-present");
        }
        else
        {
            deferredBy.Add("bond-confirmation-absent");
        }

        if (sources.MissingReceipts.Contains("durability-witness-receipt", StringComparer.Ordinal) ||
            sources.MissingReceipts.Contains("cold-admission-gate-receipt", StringComparer.Ordinal) ||
            sources.MissingReceipts.Contains("interlock-density-ledger-receipt", StringComparer.Ordinal) ||
            sources.MissingReceipts.Contains("core-invariant-lattice-receipt", StringComparer.Ordinal))
        {
            deferredBy.Add("promotion-receipts-missing");
        }

        return CreateClaim(
            PersonificationClaimKind.FullPersonificationStanding,
            standing: sources.BondedContinuity.Standing &&
                      sources.AppendOnlyIdentity.Standing &&
                      sources.AnyBondedConfirmed,
            visible: HasAnyReason(sources.BondedContinuity) ||
                     HasAnyReason(sources.AppendOnlyIdentity) ||
                     sources.AnyBondedConfirmed,
            satisfiedBy: satisfiedBy,
            contradictedBy: [],
            deferredBy: deferredBy,
            visibleBy: []);
    }

    private static PersonificationClaimEvaluation EvaluateInheritanceStillWithheld(
        NormalizedStandingSources sources)
    {
        var visible = sources.VisibleGuardrails.Contains("final-inheritance-still-withheld", StringComparer.Ordinal);
        return CreateClaim(
            PersonificationClaimKind.InheritanceStillWithheld,
            standing: false,
            visible: visible,
            satisfiedBy: [],
            contradictedBy: [],
            deferredBy: [],
            visibleBy: visible ? ["final-inheritance-still-withheld"] : []);
    }

    private static PersonificationClaimEvaluation EvaluateCoreLawSanctificationDenied(
        NormalizedStandingSources sources)
    {
        var visible = sources.VisibleGuardrails.Contains("core-law-sanctification-denied", StringComparer.Ordinal);
        return CreateClaim(
            PersonificationClaimKind.CoreLawSanctificationDenied,
            standing: false,
            visible: visible,
            satisfiedBy: [],
            contradictedBy: [],
            deferredBy: [],
            visibleBy: visible ? ["core-law-sanctification-denied"] : []);
    }

    private static CurrentClassGateOutcome ApplyCurrentClassGate(
        PersonificationGateTableEntry gate,
        IReadOnlyList<PersonificationClaimEvaluation> claims,
        NormalizedStandingSources sources)
    {
        var claimLookup = claims.ToDictionary(static claim => claim.Claim);
        var activeContradictions = claims.SelectMany(static claim => claim.ContradictedBy).ToHashSet(StringComparer.Ordinal);
        var activeDeferrals = claims.SelectMany(static claim => claim.DeferredBy).ToHashSet(StringComparer.Ordinal);

        var blockingReasons = gate.BlockingReasons
            .Where(activeContradictions.Contains)
            .ToArray();

        var deferralReasons = gate.DeferralReasons
            .Where(activeDeferrals.Contains)
            .ToArray();

        var validClaims = gate.AllowedClaims
            .Select(claimLookup.GetValueOrDefault)
            .Where(static claim => claim is not null)
            .Where(static claim => claim!.Standing && claim.ClaimClass != PersonificationClaimClass.NonBlockingGuardrail)
            .Select(static claim => claim!.Claim)
            .Distinct()
            .ToArray();

        var visibleGuardrails = gate.AllowedClaims
            .Select(claimLookup.GetValueOrDefault)
            .Where(static claim => claim is not null)
            .Where(static claim => claim!.ClaimClass == PersonificationClaimClass.NonBlockingGuardrail && claim.Visible)
            .Select(static claim => claim!.Claim)
            .Distinct()
            .ToArray();

        var withheldClaims = gate.WithheldClaims
            .Concat(gate.AllowedClaims
                .Select(claimLookup.GetValueOrDefault)
                .Where(static claim => claim is not null)
                .Where(static claim => claim!.ClaimClass != PersonificationClaimClass.NonBlockingGuardrail && !claim.Standing)
                .Select(static claim => claim!.Claim))
            .Distinct()
            .ToArray();

        var missingReceipts = gate.RequiredNextReceipts
            .Where(receipt => sources.MissingReceipts.Contains(receipt, StringComparer.Ordinal))
            .ToArray();

        var standing = blockingReasons.Length == 0 &&
                       gate.RequiredStandingClaims.All(claim => claimLookup.TryGetValue(claim, out var evaluation) && evaluation.Standing);

        return new CurrentClassGateOutcome(
            Standing: standing,
            ValidClaims: validClaims,
            WithheldClaims: withheldClaims,
            VisibleGuardrails: visibleGuardrails,
            BlockingReasons: blockingReasons,
            DeferralReasons: deferralReasons,
            MissingReceipts: missingReceipts);
    }

    private static PromotionOutcome EvaluatePromotionTargets(
        PersonificationClass currentClass,
        IReadOnlyList<PersonificationClaimEvaluation> claims,
        NormalizedStandingSources sources)
    {
        var claimLookup = claims.ToDictionary(static claim => claim.Claim);
        var activeContradictions = claims.SelectMany(static claim => claim.ContradictedBy).ToHashSet(StringComparer.Ordinal);
        var activeDeferrals = claims.SelectMany(static claim => claim.DeferredBy).ToHashSet(StringComparer.Ordinal);

        if (sources.Chamber.ContradictedReasons.Any(static reason =>
                reason is "evidence-broken-window" or "trace-no-core-window"))
        {
            var blockedTargets = Enum.GetValues<PersonificationClass>()
                .Where(targetClass => targetClass > currentClass)
                .ToArray();

            return new PromotionOutcome(
                EligibleTargets: [],
                DeferredTargets: [],
                BlockedTargets: blockedTargets,
                BlockingReasons: NormalizeReasons(sources.Chamber.ContradictedReasons
                    .Where(static reason => reason is "evidence-broken-window" or "trace-no-core-window")),
                DeferralReasons: [],
                MissingReceipts: []);
        }

        var eligible = new List<PersonificationClass>();
        var deferred = new List<PersonificationClass>();
        var blocked = new List<PersonificationClass>();
        var blockingReasons = new List<string>();
        var deferralReasons = new List<string>();
        var missingReceipts = new List<string>();

        foreach (var targetClass in Enum.GetValues<PersonificationClass>().Where(targetClass => targetClass > currentClass))
        {
            var gate = PersonificationStandingGrammar.Gates[targetClass];
            var targetBlockingReasons = gate.BlockingReasons.Where(activeContradictions.Contains).ToArray();
            var targetDeferralReasons = gate.DeferralReasons.Where(activeDeferrals.Contains).ToArray();
            var targetMissingReceipts = gate.RequiredNextReceipts
                .Where(receipt => sources.MissingReceipts.Contains(receipt, StringComparer.Ordinal))
                .ToArray();
            var requiredClaimsStanding = gate.RequiredStandingClaims
                .All(claim => claimLookup.TryGetValue(claim, out var evaluation) && evaluation.Standing);

            if (targetBlockingReasons.Length > 0)
            {
                blocked.Add(targetClass);
                blockingReasons.AddRange(targetBlockingReasons);
                continue;
            }

            if (!requiredClaimsStanding || targetDeferralReasons.Length > 0 || targetMissingReceipts.Length > 0)
            {
                deferred.Add(targetClass);
                deferralReasons.AddRange(targetDeferralReasons);
                missingReceipts.AddRange(targetMissingReceipts);
                continue;
            }

            eligible.Add(targetClass);
        }

        return new PromotionOutcome(
            EligibleTargets: eligible.ToArray(),
            DeferredTargets: deferred.ToArray(),
            BlockedTargets: blocked.ToArray(),
            BlockingReasons: NormalizeReasons(blockingReasons),
            DeferralReasons: NormalizeReasons(deferralReasons),
            MissingReceipts: NormalizeReasons(missingReceipts));
    }

    private static PersonificationStandingEvaluation FreezeStandingEvaluation(
        PersonificationClass currentClass,
        IReadOnlyList<PersonificationClaimEvaluation> claims,
        CurrentClassGateOutcome currentOutcome,
        PromotionOutcome promotionOutcome,
        NormalizedStandingSources sources,
        string? decisionAuthority)
    {
        var withheldClaims = currentOutcome.WithheldClaims.ToHashSet();
        var frozenClaims = claims
            .Select(claim => claim with { Withheld = withheldClaims.Contains(claim.Claim) })
            .ToArray();

        var blockingReasons = NormalizeReasons(currentOutcome.BlockingReasons.Concat(promotionOutcome.BlockingReasons));
        var deferralReasons = NormalizeReasons(currentOutcome.DeferralReasons.Concat(promotionOutcome.DeferralReasons));
        var missingReceipts = NormalizeReasons(sources.MissingReceipts
            .Concat(currentOutcome.MissingReceipts)
            .Concat(promotionOutcome.MissingReceipts));

        var status = currentOutcome.Standing switch
        {
            false => PersonificationStandingStatus.Withheld,
            true when promotionOutcome.EligibleTargets.Count > 0 => PersonificationStandingStatus.PromotionEligible,
            true when promotionOutcome.DeferredTargets.Count > 0 => PersonificationStandingStatus.PromotionDeferred,
            true when promotionOutcome.BlockedTargets.Count > 0 => PersonificationStandingStatus.PromotionBlocked,
            _ => PersonificationStandingStatus.Standing
        };

        return new PersonificationStandingEvaluation(
            CurrentClass: currentClass,
            Status: status,
            Claims: frozenClaims,
            ValidClaims: currentOutcome.ValidClaims,
            WithheldClaims: currentOutcome.WithheldClaims,
            VisibleGuardrails: currentOutcome.VisibleGuardrails,
            BlockingReasons: blockingReasons,
            DeferralReasons: deferralReasons,
            MissingReceipts: missingReceipts,
            EligiblePromotionTargets: promotionOutcome.EligibleTargets,
            DeferredPromotionTargets: promotionOutcome.DeferredTargets,
            BlockedPromotionTargets: promotionOutcome.BlockedTargets,
            DecisionAuthority: string.IsNullOrWhiteSpace(decisionAuthority)
                ? DefaultDecisionAuthority
                : decisionAuthority.Trim(),
            Frozen: true);
    }

    private static PersonificationClaimEvaluation CreateClaim(
        PersonificationClaimKind claim,
        bool standing,
        bool visible,
        IEnumerable<string> satisfiedBy,
        IEnumerable<string> contradictedBy,
        IEnumerable<string> deferredBy,
        IEnumerable<string> visibleBy)
    {
        var definition = PersonificationStandingGrammar.Claims[claim];
        return new PersonificationClaimEvaluation(
            Claim: claim,
            ClaimClass: definition.ClaimClass,
            Standing: standing,
            Withheld: false,
            Visible: visible,
            SatisfiedBy: NormalizeReasons(satisfiedBy),
            ContradictedBy: NormalizeReasons(contradictedBy),
            DeferredBy: NormalizeReasons(deferredBy),
            VisibleBy: NormalizeReasons(visibleBy));
    }

    private static StandingSourceBucket NormalizeBucket(StandingSourceBucket bucket) =>
        new(
            Standing: bucket.Standing,
            SatisfiedReasons: NormalizeReasons(bucket.SatisfiedReasons),
            ContradictedReasons: NormalizeReasons(bucket.ContradictedReasons),
            DeferredReasons: NormalizeReasons(bucket.DeferredReasons),
            VisibleReasons: NormalizeReasons(bucket.VisibleReasons));

    private static IEnumerable<string> GetBondedContinuitySatisfiedReasons(
        AgentiActualizationStandingProjection projection)
    {
        if (projection.DurableUnderVariation)
        {
            yield return "durable-under-variation";
        }

        if (projection.ColdApproachLawful)
        {
            yield return "cold-approach-lawful";
        }

        if (projection.DenseInterweaveEmergent)
        {
            yield return "dense-interweave-emergent";
        }
    }

    private static IEnumerable<string> GetBondedContinuityContradictedReasons(
        AgentiActualizationStandingProjection projection)
    {
        return projection.BlockingReasons.Where(static reason =>
            reason is "durability-under-variation-absent" or
                      "cold-approach-not-lawful" or
                      "dense-interweave-not-emergent");
    }

    private static IEnumerable<string> GetBondedContinuityDeferredReasons(
        AgentiActualizationStandingProjection projection)
    {
        return projection.Flags.Contains("promotion-receipts-missing", StringComparer.Ordinal)
            ? ["promotion-receipts-missing"]
            : Array.Empty<string>();
    }

    private static IEnumerable<string> GetAppendOnlyIdentitySatisfiedReasons(
        AgentiActualizationStandingProjection projection)
    {
        if (projection.IdentityAdjacentSignificanceEmergent)
        {
            yield return "identity-adjacent-significance-emergent";
        }

        if (projection.LatticeGradeInvarianceWitnessed)
        {
            yield return "lattice-grade-invariance-witnessed";
        }
    }

    private static IEnumerable<string> GetAppendOnlyIdentityContradictedReasons(
        AgentiActualizationStandingProjection projection)
    {
        return projection.BlockingReasons.Where(static reason =>
            reason is "identity-adjacent-significance-not-emergent" or
                      "lattice-grade-invariance-not-witnessed");
    }

    private static IEnumerable<string> GetAppendOnlyIdentityDeferredReasons(
        AgentiActualizationStandingProjection projection)
    {
        return projection.Flags.Contains("promotion-receipts-missing", StringComparer.Ordinal)
            ? ["promotion-receipts-missing"]
            : Array.Empty<string>();
    }

    private static bool HasObservedSignals(IReadOnlyList<string>? signals) =>
        signals is { Count: > 0 } &&
        signals.Any(static signal => !string.IsNullOrWhiteSpace(signal));

    private static bool HasAnyReason(StandingSourceBucket bucket) =>
        bucket.SatisfiedReasons.Count > 0 ||
        bucket.ContradictedReasons.Count > 0 ||
        bucket.DeferredReasons.Count > 0 ||
        bucket.VisibleReasons.Count > 0;

    private static IReadOnlyList<string> NormalizeReasons(IEnumerable<string> reasons) =>
        reasons
            .Where(static reason => !string.IsNullOrWhiteSpace(reason))
            .Select(static reason => reason.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static reason => reason, StringComparer.Ordinal)
            .ToArray();

    private sealed record CurrentClassGateOutcome(
        bool Standing,
        IReadOnlyList<PersonificationClaimKind> ValidClaims,
        IReadOnlyList<PersonificationClaimKind> WithheldClaims,
        IReadOnlyList<PersonificationClaimKind> VisibleGuardrails,
        IReadOnlyList<string> BlockingReasons,
        IReadOnlyList<string> DeferralReasons,
        IReadOnlyList<string> MissingReceipts);

    private sealed record PromotionOutcome(
        IReadOnlyList<PersonificationClass> EligibleTargets,
        IReadOnlyList<PersonificationClass> DeferredTargets,
        IReadOnlyList<PersonificationClass> BlockedTargets,
        IReadOnlyList<string> BlockingReasons,
        IReadOnlyList<string> DeferralReasons,
        IReadOnlyList<string> MissingReceipts);
}
