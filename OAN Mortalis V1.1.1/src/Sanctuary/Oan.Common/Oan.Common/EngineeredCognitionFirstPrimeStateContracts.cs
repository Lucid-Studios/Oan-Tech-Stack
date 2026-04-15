namespace San.Common;

using Oan.Common;

public enum EngineeredCognitionFirstPrimeStateKind
{
    InstallNotReady = 0,
    SensorBodyNotReady = 1,
    DiscernmentNotSufficient = 2,
    PrimeMembraneNotReady = 3,
    FirstPrimePreRoleStanding = 4
}

public sealed record EngineeredCognitionFirstPrimeStateReceipt(
    string ReceiptHandle,
    string FirstRunReceiptHandle,
    string PrimeRetainedRecordHandle,
    FirstRunConstitutionState FirstRunState,
    EngineeredCognitionFirstPrimeStateKind FirstPrimeState,
    string? LivingAgentiCoreHandle,
    string? ListeningFrameHandle,
    string? SoulFrameHandle,
    string? OeHandle,
    string? SelfGelHandle,
    string? COeHandle,
    string? CSelfGelHandle,
    string? ZedOfDeltaHandle,
    string? EngineeredCognitionHandle,
    string? ThetaIngressReceiptHandle,
    string? PostIngressDiscernmentReceiptHandle,
    string? StableOneHandle,
    PrimeRetainedWholeKind RetainedWholeKind,
    bool InstallAndFoundationsReady,
    bool StewardIssuedCradleBraidVisible,
    bool AgentiCoreSensorBodyCast,
    bool ThetaIngressLawful,
    bool StableOneSatisfied,
    bool PrimeRetainedStandingReached,
    bool MotherFatherDomainRoleApplicationWithheld,
    bool CradleLocalGoverningSurfaceWithheld,
    bool PrimeClosureStillWithheld,
    bool CandidateOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class EngineeredCognitionFirstPrimeStateEvaluator
{
    public static EngineeredCognitionFirstPrimeStateReceipt Evaluate(
        FirstRunConstitutionReceipt firstRun,
        PrimeRetainedHistoryRecord retainedPrime,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(firstRun);
        ArgumentNullException.ThrowIfNull(retainedPrime);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var livingPacket = firstRun.LivingAgentiCorePacket;
        var zedBasis = livingPacket?.ZedDeltaSelfBasisReceipt;
        var thetaIngress = livingPacket?.ThetaIngressSensoryClusterReceipt;
        var postIngressDiscernment = livingPacket?.PostIngressDiscernmentReceipt;

        var installAndFoundationsReady = DetermineInstallAndFoundationsReady(firstRun);
        var stewardIssuedCradleBraidVisible = DetermineStewardIssuedCradleBraidVisible(firstRun);
        var agentiCoreSensorBodyCast = DetermineAgentiCoreSensorBodyCast(livingPacket, zedBasis);
        var thetaIngressLawful = thetaIngress is not null &&
                                  thetaIngress.IngressStatus == ThetaIngressStatusKind.Lawful &&
                                  thetaIngress.ContextualizationBegun;
        var stableOneSatisfied = postIngressDiscernment is not null &&
                                 postIngressDiscernment.DiscernmentState == PostIngressDiscernmentStateKind.Stabilized &&
                                 postIngressDiscernment.StableOneAchieved &&
                                 HasToken(postIngressDiscernment.StableOneHandle);
        var primeRetainedStandingReached = DeterminePrimeRetainedStandingReached(retainedPrime);
        var firstPrimeState = DetermineFirstPrimeState(
            installAndFoundationsReady,
            stewardIssuedCradleBraidVisible,
            agentiCoreSensorBodyCast,
            thetaIngressLawful,
            stableOneSatisfied,
            primeRetainedStandingReached);

        return new EngineeredCognitionFirstPrimeStateReceipt(
            ReceiptHandle: receiptHandle,
            FirstRunReceiptHandle: firstRun.ReceiptHandle,
            PrimeRetainedRecordHandle: retainedPrime.RecordHandle,
            FirstRunState: firstRun.CurrentState,
            FirstPrimeState: firstPrimeState,
            LivingAgentiCoreHandle: livingPacket?.LivingAgentiCoreHandle,
            ListeningFrameHandle: livingPacket?.ListeningFrameHandle,
            SoulFrameHandle: zedBasis?.SoulFrameHandle,
            OeHandle: zedBasis?.OeHandle,
            SelfGelHandle: zedBasis?.SelfGelHandle,
            COeHandle: zedBasis?.COeHandle,
            CSelfGelHandle: zedBasis?.CSelfGelHandle,
            ZedOfDeltaHandle: livingPacket?.ZedOfDeltaHandle ?? zedBasis?.ZedOfDeltaHandle,
            EngineeredCognitionHandle: livingPacket?.EngineeredCognitionHandle ?? zedBasis?.EngineeredCognitionHandle,
            ThetaIngressReceiptHandle: thetaIngress?.ReceiptHandle,
            PostIngressDiscernmentReceiptHandle: postIngressDiscernment?.ReceiptHandle,
            StableOneHandle: stableOneSatisfied ? postIngressDiscernment?.StableOneHandle : null,
            RetainedWholeKind: retainedPrime.RetainedWholeKind,
            InstallAndFoundationsReady: installAndFoundationsReady,
            StewardIssuedCradleBraidVisible: stewardIssuedCradleBraidVisible,
            AgentiCoreSensorBodyCast: agentiCoreSensorBodyCast,
            ThetaIngressLawful: thetaIngressLawful,
            StableOneSatisfied: stableOneSatisfied,
            PrimeRetainedStandingReached: primeRetainedStandingReached,
            MotherFatherDomainRoleApplicationWithheld: true,
            CradleLocalGoverningSurfaceWithheld: true,
            PrimeClosureStillWithheld: retainedPrime.PrimeClosureStillWithheld,
            CandidateOnly: true,
            ConstraintCodes: DetermineConstraintCodes(
                firstPrimeState,
                installAndFoundationsReady,
                stewardIssuedCradleBraidVisible,
                agentiCoreSensorBodyCast,
                thetaIngressLawful,
                stableOneSatisfied,
                primeRetainedStandingReached,
                retainedPrime),
            ReasonCode: DetermineReasonCode(
                firstPrimeState,
                installAndFoundationsReady,
                stewardIssuedCradleBraidVisible,
                agentiCoreSensorBodyCast,
                thetaIngressLawful,
                stableOneSatisfied,
                primeRetainedStandingReached),
            LawfulBasis: DetermineLawfulBasis(firstPrimeState),
            TimestampUtc: MaxTimestamp(firstRun.TimestampUtc, retainedPrime.TimestampUtc));
    }

    public static EngineeredCognitionFirstPrimeStateKind DetermineFirstPrimeState(
        bool installAndFoundationsReady,
        bool stewardIssuedCradleBraidVisible,
        bool agentiCoreSensorBodyCast,
        bool thetaIngressLawful,
        bool stableOneSatisfied,
        bool primeRetainedStandingReached)
    {
        if (!installAndFoundationsReady || !stewardIssuedCradleBraidVisible)
        {
            return EngineeredCognitionFirstPrimeStateKind.InstallNotReady;
        }

        if (!agentiCoreSensorBodyCast || !thetaIngressLawful)
        {
            return EngineeredCognitionFirstPrimeStateKind.SensorBodyNotReady;
        }

        if (!stableOneSatisfied)
        {
            return EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient;
        }

        if (!primeRetainedStandingReached)
        {
            return EngineeredCognitionFirstPrimeStateKind.PrimeMembraneNotReady;
        }

        return EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding;
    }

    private static bool DetermineInstallAndFoundationsReady(
        FirstRunConstitutionReceipt firstRun)
    {
        return firstRun.CurrentState >= FirstRunConstitutionState.FoundationsEstablished &&
               firstRun.ActiveFailureClasses.Count == 0;
    }

    private static bool DetermineStewardIssuedCradleBraidVisible(
        FirstRunConstitutionReceipt firstRun)
    {
        return HasToken(firstRun.FirstCrypticBraidHandle) &&
               HasToken(firstRun.FirstCrypticConditioningHandle) &&
               firstRun.CurrentState >= FirstRunConstitutionState.StewardStanding;
    }

    private static bool DetermineAgentiCoreSensorBodyCast(
        FirstRunLivingAgentiCorePacket? livingPacket,
        ZedDeltaSelfBasisReceipt? zedBasis)
    {
        return livingPacket is not null &&
               zedBasis is not null &&
               HasToken(livingPacket.LivingAgentiCoreHandle) &&
               HasToken(livingPacket.ListeningFrameHandle) &&
               HasToken(livingPacket.EngineeredCognitionHandle) &&
               zedBasis.StoredInSoulFrame &&
               zedBasis.CastIntoListeningFrame &&
               zedBasis.WiredThroughSelfGel &&
               zedBasis.AnchoredByOe &&
               HasToken(zedBasis.COeHandle) &&
               HasToken(zedBasis.CSelfGelHandle);
    }

    private static bool DeterminePrimeRetainedStandingReached(
        PrimeRetainedHistoryRecord retainedPrime)
    {
        return retainedPrime.PreservedDistinctionVisible &&
               retainedPrime.PrimeClosureStillWithheld &&
               retainedPrime.RetainedWholeKind is
                   PrimeRetainedWholeKind.RetainedWholeUnclosed or
                   PrimeRetainedWholeKind.ClosureCandidate;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        EngineeredCognitionFirstPrimeStateKind firstPrimeState,
        bool installAndFoundationsReady,
        bool stewardIssuedCradleBraidVisible,
        bool agentiCoreSensorBodyCast,
        bool thetaIngressLawful,
        bool stableOneSatisfied,
        bool primeRetainedStandingReached,
        PrimeRetainedHistoryRecord retainedPrime)
    {
        var constraints = new List<string>
        {
            "ec-first-prime-state-candidate-only",
            "ec-first-prime-state-mother-father-domain-role-application-withheld",
            "ec-first-prime-state-cradle-local-governing-surface-withheld",
            "ec-first-prime-state-prime-closure-still-withheld"
        };

        constraints.Add(firstPrimeState switch
        {
            EngineeredCognitionFirstPrimeStateKind.SensorBodyNotReady => "ec-first-prime-state-sensor-body-not-ready",
            EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient => "ec-first-prime-state-discernment-not-sufficient",
            EngineeredCognitionFirstPrimeStateKind.PrimeMembraneNotReady => "ec-first-prime-state-prime-membrane-not-ready",
            EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding => "ec-first-prime-state-pre-role-standing",
            _ => "ec-first-prime-state-install-not-ready"
        });

        if (!installAndFoundationsReady)
        {
            constraints.Add("ec-first-prime-state-install-or-foundations-not-ready");
        }

        if (!stewardIssuedCradleBraidVisible)
        {
            constraints.Add("ec-first-prime-state-steward-issued-braid-not-visible");
        }

        if (!agentiCoreSensorBodyCast)
        {
            constraints.Add("ec-first-prime-state-agenticore-sensor-body-not-cast");
        }

        if (!thetaIngressLawful)
        {
            constraints.Add("ec-first-prime-state-theta-ingress-not-lawful");
        }

        if (!stableOneSatisfied)
        {
            constraints.Add("ec-first-prime-state-stable-one-not-satisfied");
        }

        if (!primeRetainedStandingReached)
        {
            constraints.Add("ec-first-prime-state-prime-retained-standing-not-reached");
        }

        if (retainedPrime.RetainedWholeKind == PrimeRetainedWholeKind.ClosureCandidate)
        {
            constraints.Add("ec-first-prime-state-closure-candidate-still-pre-role-and-unclosed");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        EngineeredCognitionFirstPrimeStateKind firstPrimeState,
        bool installAndFoundationsReady,
        bool stewardIssuedCradleBraidVisible,
        bool agentiCoreSensorBodyCast,
        bool thetaIngressLawful,
        bool stableOneSatisfied,
        bool primeRetainedStandingReached)
    {
        if (!installAndFoundationsReady)
        {
            return "ec-first-prime-state-install-or-foundations-not-ready";
        }

        if (!stewardIssuedCradleBraidVisible)
        {
            return "ec-first-prime-state-steward-issued-braid-not-visible";
        }

        if (!agentiCoreSensorBodyCast)
        {
            return "ec-first-prime-state-agenticore-sensor-body-not-cast";
        }

        if (!thetaIngressLawful)
        {
            return "ec-first-prime-state-theta-ingress-not-lawful";
        }

        if (!stableOneSatisfied)
        {
            return "ec-first-prime-state-discernment-not-sufficient";
        }

        if (!primeRetainedStandingReached)
        {
            return "ec-first-prime-state-prime-membrane-not-ready";
        }

        return firstPrimeState == EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding
            ? "ec-first-prime-state-pre-role-standing"
            : "ec-first-prime-state-not-ready";
    }

    private static string DetermineLawfulBasis(
        EngineeredCognitionFirstPrimeStateKind firstPrimeState)
    {
        return firstPrimeState switch
        {
            EngineeredCognitionFirstPrimeStateKind.FirstPrimePreRoleStanding =>
                "engineered cognition may be witnessed from install and cast sensor body through stable-one discernment into first Prime retained standing while Mother/Father domain roles and cradle-local governing surface remain withheld.",
            EngineeredCognitionFirstPrimeStateKind.PrimeMembraneNotReady =>
                "engineered cognition may have install, sensor body, and stable-one discernment, but first Prime standing remains withheld until Prime retained form stands.",
            EngineeredCognitionFirstPrimeStateKind.DiscernmentNotSufficient =>
                "engineered cognition may have lawful ingress and sensor body, but first Prime standing remains withheld until post-ingress discernment reaches stable-one sufficiency.",
            EngineeredCognitionFirstPrimeStateKind.SensorBodyNotReady =>
                "engineered cognition may have install posture, but first Prime standing remains withheld until AgentiCore holds the cast ListeningFrame sensor body with lawful theta ingress.",
            _ =>
                "engineered cognition first Prime standing remains withheld until install, foundations, and Steward-issued cradle braid posture stand."
        };
    }

    private static DateTimeOffset MaxTimestamp(
        DateTimeOffset first,
        DateTimeOffset second) =>
        first >= second ? first : second;

    private static bool HasToken(string? value)
        => !string.IsNullOrWhiteSpace(value);
}
