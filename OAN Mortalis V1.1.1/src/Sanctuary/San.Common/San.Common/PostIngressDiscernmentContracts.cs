namespace San.Common;

public enum PostIngressDiscernmentStateKind
{
    Stabilized = 0,
    Investigatory = 1,
    CarriedIncomplete = 2,
    Blocked = 3
}

public enum PostIngressDiscernmentSignalKind
{
    None = 0,
    Ambiguity = 1,
    StructuralConflict = 2,
    GroundingInsufficient = 3,
    UnresolvedRelation = 4
}

public sealed record PostIngressDiscernmentReceipt(
    string ReceiptHandle,
    string ThetaIngressReceiptHandle,
    string? ThetaHandle,
    string? ListeningFrameHandle,
    string? ZedOfDeltaHandle,
    string? COeHandle,
    string? CSelfGelHandle,
    string? EngineeredCognitionHandle,
    string? StableOneHandle,
    PostIngressDiscernmentStateKind DiscernmentState,
    bool StableOneAchieved,
    bool CandidateOnly,
    bool SemanticRiseWithheld,
    bool PersistenceAuthorityWithheld,
    bool InheritanceWithheld,
    bool SelfMutationWithheld,
    bool PulseAuthorityWithheld,
    IReadOnlyList<PostIngressDiscernmentSignalKind> DiscernmentSignals,
    IReadOnlyList<string> QuestionHandles,
    IReadOnlyList<string> EnrichmentHandles,
    IReadOnlyList<string> CarriedIncompleteHandles,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PostIngressDiscernmentEvaluator
{
    public static PostIngressDiscernmentReceipt Evaluate(
        ThetaIngressSensoryClusterReceipt thetaIngress,
        bool stableOneAchieved,
        string? stableOneHandle,
        IReadOnlyList<PostIngressDiscernmentSignalKind>? discernmentSignals,
        IReadOnlyList<string>? questionHandles,
        IReadOnlyList<string>? enrichmentHandles,
        IReadOnlyList<string>? carriedIncompleteHandles,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(thetaIngress);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var normalizedSignals = NormalizeSignals(discernmentSignals);
        var normalizedQuestionHandles = NormalizeTokens(questionHandles);
        var normalizedEnrichmentHandles = NormalizeTokens(enrichmentHandles);
        var normalizedCarriedIncompleteHandles = NormalizeTokens(carriedIncompleteHandles);
        var discernmentState = DetermineDiscernmentState(
            thetaIngress,
            stableOneAchieved,
            stableOneHandle,
            normalizedSignals,
            normalizedQuestionHandles,
            normalizedEnrichmentHandles,
            normalizedCarriedIncompleteHandles);

        return new PostIngressDiscernmentReceipt(
            ReceiptHandle: receiptHandle,
            ThetaIngressReceiptHandle: thetaIngress.ReceiptHandle,
            ThetaHandle: thetaIngress.ThetaHandle,
            ListeningFrameHandle: thetaIngress.ListeningFrameHandle,
            ZedOfDeltaHandle: thetaIngress.ZedOfDeltaHandle,
            COeHandle: thetaIngress.COeHandle,
            CSelfGelHandle: thetaIngress.CSelfGelHandle,
            EngineeredCognitionHandle: thetaIngress.EngineeredCognitionHandle,
            StableOneHandle: discernmentState == PostIngressDiscernmentStateKind.Stabilized
                ? stableOneHandle
                : null,
            DiscernmentState: discernmentState,
            StableOneAchieved: discernmentState == PostIngressDiscernmentStateKind.Stabilized,
            CandidateOnly: true,
            SemanticRiseWithheld: true,
            PersistenceAuthorityWithheld: true,
            InheritanceWithheld: true,
            SelfMutationWithheld: true,
            PulseAuthorityWithheld: true,
            DiscernmentSignals: normalizedSignals,
            QuestionHandles: discernmentState == PostIngressDiscernmentStateKind.Investigatory
                ? normalizedQuestionHandles
                : [],
            EnrichmentHandles: discernmentState is PostIngressDiscernmentStateKind.Investigatory or PostIngressDiscernmentStateKind.CarriedIncomplete
                ? normalizedEnrichmentHandles
                : [],
            CarriedIncompleteHandles: discernmentState == PostIngressDiscernmentStateKind.CarriedIncomplete
                ? normalizedCarriedIncompleteHandles
                : [],
            ConstraintCodes: DetermineConstraintCodes(
                thetaIngress,
                discernmentState,
                stableOneAchieved,
                stableOneHandle,
                normalizedSignals,
                normalizedQuestionHandles,
                normalizedEnrichmentHandles,
                normalizedCarriedIncompleteHandles),
            ReasonCode: DetermineReasonCode(
                thetaIngress,
                discernmentState,
                stableOneAchieved,
                stableOneHandle,
                normalizedQuestionHandles,
                normalizedCarriedIncompleteHandles),
            LawfulBasis: DetermineLawfulBasis(discernmentState),
            TimestampUtc: thetaIngress.TimestampUtc);
    }

    public static PostIngressDiscernmentStateKind DetermineDiscernmentState(
        ThetaIngressSensoryClusterReceipt thetaIngress,
        bool stableOneAchieved,
        string? stableOneHandle,
        IReadOnlyList<PostIngressDiscernmentSignalKind> discernmentSignals,
        IReadOnlyList<string> questionHandles,
        IReadOnlyList<string> enrichmentHandles,
        IReadOnlyList<string> carriedIncompleteHandles)
    {
        ArgumentNullException.ThrowIfNull(thetaIngress);
        ArgumentNullException.ThrowIfNull(discernmentSignals);
        ArgumentNullException.ThrowIfNull(questionHandles);
        ArgumentNullException.ThrowIfNull(enrichmentHandles);
        ArgumentNullException.ThrowIfNull(carriedIncompleteHandles);

        if (thetaIngress.IngressStatus != ThetaIngressStatusKind.Lawful ||
            !thetaIngress.ContextualizationBegun)
        {
            return PostIngressDiscernmentStateKind.Blocked;
        }

        if (stableOneAchieved)
        {
            return !string.IsNullOrWhiteSpace(stableOneHandle) &&
                   questionHandles.Count == 0 &&
                   enrichmentHandles.Count == 0 &&
                   carriedIncompleteHandles.Count == 0 &&
                   !HasInvestigatorySignal(discernmentSignals)
                ? PostIngressDiscernmentStateKind.Stabilized
                : PostIngressDiscernmentStateKind.Blocked;
        }

        if (questionHandles.Count > 0)
        {
            return PostIngressDiscernmentStateKind.Investigatory;
        }

        if (carriedIncompleteHandles.Count > 0 || enrichmentHandles.Count > 0)
        {
            return PostIngressDiscernmentStateKind.CarriedIncomplete;
        }

        return PostIngressDiscernmentStateKind.Blocked;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        ThetaIngressSensoryClusterReceipt thetaIngress,
        PostIngressDiscernmentStateKind discernmentState,
        bool stableOneAchieved,
        string? stableOneHandle,
        IReadOnlyList<PostIngressDiscernmentSignalKind> discernmentSignals,
        IReadOnlyList<string> questionHandles,
        IReadOnlyList<string> enrichmentHandles,
        IReadOnlyList<string> carriedIncompleteHandles)
    {
        var constraints = new List<string>
        {
            "post-ingress-discernment-candidate-only",
            "post-ingress-discernment-semantic-rise-withheld",
            "post-ingress-discernment-persistence-authority-withheld",
            "post-ingress-discernment-inheritance-withheld",
            "post-ingress-discernment-self-mutation-withheld",
            "post-ingress-discernment-pulse-authority-withheld"
        };

        constraints.Add(discernmentState switch
        {
            PostIngressDiscernmentStateKind.Investigatory => "post-ingress-discernment-investigatory",
            PostIngressDiscernmentStateKind.CarriedIncomplete => "post-ingress-discernment-carried-incomplete",
            PostIngressDiscernmentStateKind.Blocked => "post-ingress-discernment-blocked",
            _ => "post-ingress-discernment-stabilized"
        });

        constraints.Add(stableOneAchieved && !string.IsNullOrWhiteSpace(stableOneHandle)
            ? "post-ingress-discernment-stable-one-achieved"
            : "post-ingress-discernment-stable-one-not-achieved");

        if (thetaIngress.IngressStatus != ThetaIngressStatusKind.Lawful)
        {
            constraints.Add("post-ingress-discernment-theta-ingress-not-lawful");
        }

        if (!thetaIngress.ContextualizationBegun)
        {
            constraints.Add("post-ingress-discernment-contextualization-not-begun");
        }

        if (questionHandles.Count > 0)
        {
            constraints.Add("post-ingress-discernment-question-handles-visible");
        }

        if (enrichmentHandles.Count > 0)
        {
            constraints.Add("post-ingress-discernment-enrichment-handles-visible");
        }

        if (carriedIncompleteHandles.Count > 0)
        {
            constraints.Add("post-ingress-discernment-carried-incomplete-visible");
        }

        if (HasInvestigatorySignal(discernmentSignals))
        {
            constraints.Add("post-ingress-discernment-investigatory-signal-present");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        ThetaIngressSensoryClusterReceipt thetaIngress,
        PostIngressDiscernmentStateKind discernmentState,
        bool stableOneAchieved,
        string? stableOneHandle,
        IReadOnlyList<string> questionHandles,
        IReadOnlyList<string> carriedIncompleteHandles)
    {
        if (thetaIngress.IngressStatus != ThetaIngressStatusKind.Lawful)
        {
            return "post-ingress-discernment-theta-ingress-not-lawful";
        }

        if (!thetaIngress.ContextualizationBegun)
        {
            return "post-ingress-discernment-contextualization-not-begun";
        }

        if (stableOneAchieved && string.IsNullOrWhiteSpace(stableOneHandle))
        {
            return "post-ingress-discernment-stable-one-handle-missing";
        }

        if (stableOneAchieved && questionHandles.Count > 0)
        {
            return "post-ingress-discernment-question-handles-inconsistent-with-stable-one";
        }

        if (stableOneAchieved && carriedIncompleteHandles.Count > 0)
        {
            return "post-ingress-discernment-carried-incomplete-inconsistent-with-stable-one";
        }

        return discernmentState switch
        {
            PostIngressDiscernmentStateKind.Investigatory => "post-ingress-discernment-investigatory",
            PostIngressDiscernmentStateKind.CarriedIncomplete => "post-ingress-discernment-carried-incomplete",
            PostIngressDiscernmentStateKind.Blocked => "post-ingress-discernment-blocked",
            _ => "post-ingress-discernment-stabilized"
        };
    }

    private static string DetermineLawfulBasis(
        PostIngressDiscernmentStateKind discernmentState)
    {
        var posture = discernmentState.ToString().ToLowerInvariant();
        return $"{posture} post-ingress discernment witnesses whether contextualized uptake reached a stable one without implying semantic rise, persistence, inheritance, self-mutation, or pulse.";
    }

    private static IReadOnlyList<PostIngressDiscernmentSignalKind> NormalizeSignals(
        IReadOnlyList<PostIngressDiscernmentSignalKind>? discernmentSignals)
    {
        var normalized = (discernmentSignals ?? Array.Empty<PostIngressDiscernmentSignalKind>())
            .Distinct()
            .OrderBy(static item => (int)item)
            .ToArray();

        if (normalized.Any(static signal => signal != PostIngressDiscernmentSignalKind.None))
        {
            return normalized
                .Where(static signal => signal != PostIngressDiscernmentSignalKind.None)
                .ToArray();
        }

        return normalized;
    }

    private static bool HasInvestigatorySignal(
        IReadOnlyList<PostIngressDiscernmentSignalKind> discernmentSignals)
    {
        return discernmentSignals.Any(static signal => signal != PostIngressDiscernmentSignalKind.None);
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        return (tokens ?? Array.Empty<string>())
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
