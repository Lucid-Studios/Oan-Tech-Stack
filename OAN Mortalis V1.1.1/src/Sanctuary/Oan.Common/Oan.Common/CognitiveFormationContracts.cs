namespace San.Common;

public enum AwarenessConeBoundaryKind
{
    Inside = 0,
    Edge = 1,
    Outside = 2
}

public enum CompassOrientationFacetKind
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    Center = 4
}

public enum PerceptualPolarityKind
{
    Direct = 0,
    Inverted = 1,
    Reflected = 2
}

public enum SourceRelationKind
{
    Self = 0,
    External = 1,
    Duplex = 2,
    Unknown = 3
}

public enum ZedLocusState
{
    Preserved = 0,
    Strained = 1,
    Lost = 2
}

public enum DeltaPressureKind
{
    None = 0,
    LoadBearing = 1,
    BasinSteepening = 2,
    NadirDisclosure = 3,
    CollapsePressure = 4
}

public enum FailureSignatureKind
{
    None = 0,
    BrittleOverclaim = 1,
    ConjunctionImbalance = 2,
    BoundarySlip = 3,
    SourceDissolution = 4,
    OrientationCollapse = 5
}

public enum OrientationIntegrityKind
{
    Stable = 0,
    Minimum = 1,
    Compromised = 2,
    Lost = 3
}

public enum RequirementStateKind
{
    Present = 0,
    Missing = 1,
    Blocked = 2,
    Unknown = 3,
    NotRequired = 4
}

public enum WhyNotClassificationKind
{
    None = 0,
    InsufficientEvidence = 1,
    OutOfCone = 2,
    BlockedByBoundary = 3,
    PrerequisiteAbsent = 4,
    DeferredByPriority = 5,
    IrrelevantToCurrentAct = 6,
    CollapseSignaturePresent = 7,
    ZedLocusNotPreserved = 8,
    OrientationIntegrityInsufficient = 9
}

public enum NextLawfulMoveKind
{
    RetainCurrentFooting = 0,
    Inspect = 1,
    RequestEvidence = 2,
    BindBoundary = 3,
    ClassifyNotRequired = 4,
    Defer = 5,
    Refuse = 6
}

public enum FormationKnowledgePostureKind
{
    NotKnown = 0,
    KnownEnoughForNextAct = 1,
    WithheldOutsideCone = 2,
    WithheldByBoundary = 3,
    NeedsEvidence = 4,
    NeedsInspection = 5,
    FailureEvidenceRetained = 6,
    OrientationEvidencePromoted = 7,
    FailureRefused = 8
}

public enum FailureEvidenceDispositionKind
{
    None = 0,
    Refuse = 1,
    RetainAsFailureEvidence = 2,
    PromoteToOrientationEvidence = 3
}

public sealed record SensoryOrientationSnapshot(
    string OrientationHandle,
    string? ListeningFrameHandle,
    string? CompassEmbodimentHandle,
    AwarenessConeBoundaryKind ConeBoundary,
    CompassOrientationFacetKind OrientationFacet,
    PerceptualPolarityKind PerceptualPolarity,
    SourceRelationKind SourceRelation,
    IReadOnlyList<string> ModalityMarkers,
    IReadOnlyList<string> OrientationNotes,
    DateTimeOffset TimestampUtc,
    string? ZedOfDeltaHandle = null,
    ZedLocusState ZedLocusState = ZedLocusState.Preserved,
    OrientationIntegrityKind OrientationIntegrity = OrientationIntegrityKind.Stable,
    DeltaPressureKind DeltaPressure = DeltaPressureKind.None);

public sealed record FormationRequirementState(
    string RequirementHandle,
    string RequirementKind,
    RequirementStateKind State,
    WhyNotClassificationKind WhyNot,
    IReadOnlyList<string> EvidenceHandles,
    IReadOnlyList<string> Notes);

public sealed record FormationFailureSignature(
    string SignatureHandle,
    FailureSignatureKind SignatureKind,
    DeltaPressureKind DeltaPressure,
    IReadOnlyList<string> EvidenceHandles,
    IReadOnlyList<string> Notes);

public sealed record CognitiveFormationSnapshot(
    string SnapshotHandle,
    string EncounterHandle,
    string? EngineeredCognitionHandle,
    SensoryOrientationSnapshot Orientation,
    IReadOnlyList<string> DiscernedItems,
    IReadOnlyList<FormationRequirementState> RequirementStates,
    IReadOnlyList<string> KnownItems,
    IReadOnlyList<string> UnknownItems,
    IReadOnlyList<string> DeferredItems,
    NextLawfulMoveKind SelectedNextLawfulMove,
    string RetainedDecisionResult,
    DateTimeOffset TimestampUtc,
    IReadOnlyList<FormationFailureSignature>? FailureSignatures = null);

public sealed record FormationReceipt(
    string ReceiptHandle,
    string EncounterHandle,
    string? EngineeredCognitionHandle,
    SensoryOrientationSnapshot Orientation,
    IReadOnlyList<string> DiscernedItems,
    IReadOnlyList<FormationRequirementState> RequirementStates,
    FormationKnowledgePostureKind KnowledgePosture,
    bool PromotionToKnownLawful,
    IReadOnlyList<string> LawfulKnownItems,
    IReadOnlyList<string> RetainedUnknownItems,
    IReadOnlyList<WhyNotClassificationKind> WhyNotClasses,
    FailureEvidenceDispositionKind FailureEvidenceDisposition,
    IReadOnlyList<FormationFailureSignature> RetainedFailureSignatures,
    IReadOnlyList<FormationFailureSignature> OrientationEvidenceSignatures,
    NextLawfulMoveKind SelectedNextLawfulMove,
    string ReasonCode,
    string LawfulBasis,
    bool Deferred,
    bool Refused,
    DateTimeOffset TimestampUtc);

public static class CognitiveFormationEvaluator
{
    public static FormationReceipt Evaluate(
        CognitiveFormationSnapshot snapshot,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var failureSignatures = NormalizeFailureSignatures(snapshot.FailureSignatures);
        var failureEvidenceDisposition = DetermineFailureEvidenceDisposition(snapshot.Orientation, failureSignatures);
        var promotionToKnownLawful = failureSignatures.Count == 0 && IsPromotionToKnownLawful(snapshot.Orientation);
        var whyNotClasses = CollectWhyNotClasses(snapshot.Orientation, snapshot.RequirementStates, failureSignatures);
        var selectedNextLawfulMove = SelectNextLawfulMove(
            snapshot.Orientation,
            snapshot.RequirementStates,
            failureSignatures,
            failureEvidenceDisposition);
        var knowledgePosture = DetermineKnowledgePosture(
            snapshot.Orientation,
            snapshot.RequirementStates,
            promotionToKnownLawful,
            failureEvidenceDisposition,
            selectedNextLawfulMove);
        var lawfulKnownItems = DetermineLawfulKnownItems(snapshot, promotionToKnownLawful, failureEvidenceDisposition);
        var retainedUnknownItems = DetermineRetainedUnknownItems(snapshot, lawfulKnownItems);
        var reasonCode = DetermineReasonCode(
            snapshot.Orientation,
            snapshot.RequirementStates,
            failureSignatures,
            failureEvidenceDisposition,
            selectedNextLawfulMove);
        var lawfulBasis = DetermineLawfulBasis(
            knowledgePosture,
            selectedNextLawfulMove,
            failureEvidenceDisposition);
        var orientationEvidenceSignatures = DetermineOrientationEvidenceSignatures(
            failureSignatures,
            failureEvidenceDisposition);
        var deferred = selectedNextLawfulMove == NextLawfulMoveKind.Defer;
        var refused = selectedNextLawfulMove == NextLawfulMoveKind.Refuse ||
                      failureEvidenceDisposition == FailureEvidenceDispositionKind.Refuse;

        return new FormationReceipt(
            ReceiptHandle: receiptHandle,
            EncounterHandle: snapshot.EncounterHandle,
            EngineeredCognitionHandle: snapshot.EngineeredCognitionHandle,
            Orientation: snapshot.Orientation,
            DiscernedItems: snapshot.DiscernedItems,
            RequirementStates: snapshot.RequirementStates,
            KnowledgePosture: knowledgePosture,
            PromotionToKnownLawful: promotionToKnownLawful,
            LawfulKnownItems: lawfulKnownItems,
            RetainedUnknownItems: retainedUnknownItems,
            WhyNotClasses: whyNotClasses,
            FailureEvidenceDisposition: failureEvidenceDisposition,
            RetainedFailureSignatures: failureSignatures,
            OrientationEvidenceSignatures: orientationEvidenceSignatures,
            SelectedNextLawfulMove: selectedNextLawfulMove,
            ReasonCode: reasonCode,
            LawfulBasis: lawfulBasis,
            Deferred: deferred,
            Refused: refused,
            TimestampUtc: snapshot.TimestampUtc);
    }

    public static bool IsPromotionToKnownLawful(SensoryOrientationSnapshot orientation)
    {
        ArgumentNullException.ThrowIfNull(orientation);
        return orientation.ConeBoundary == AwarenessConeBoundaryKind.Inside &&
               orientation.SourceRelation != SourceRelationKind.Unknown;
    }

    public static NextLawfulMoveKind SelectNextLawfulMove(
        SensoryOrientationSnapshot orientation,
        IReadOnlyList<FormationRequirementState> requirementStates)
    {
        return SelectNextLawfulMove(
            orientation,
            requirementStates,
            [],
            FailureEvidenceDispositionKind.None);
    }

    public static FormationReceipt CreateReceipt(
        CognitiveFormationSnapshot snapshot,
        string receiptHandle,
        string lawfulBasis,
        bool deferred = false,
        bool refused = false)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(lawfulBasis))
        {
            throw new ArgumentException("Lawful basis must be provided.", nameof(lawfulBasis));
        }

        var failureSignatures = NormalizeFailureSignatures(snapshot.FailureSignatures);
        var failureEvidenceDisposition = failureSignatures.Count > 0
            ? FailureEvidenceDispositionKind.RetainAsFailureEvidence
            : FailureEvidenceDispositionKind.None;

        return new FormationReceipt(
            ReceiptHandle: receiptHandle,
            EncounterHandle: snapshot.EncounterHandle,
            EngineeredCognitionHandle: snapshot.EngineeredCognitionHandle,
            Orientation: snapshot.Orientation,
            DiscernedItems: snapshot.DiscernedItems,
            RequirementStates: snapshot.RequirementStates,
            KnowledgePosture: FormationKnowledgePostureKind.NotKnown,
            PromotionToKnownLawful: false,
            LawfulKnownItems: [],
            RetainedUnknownItems: snapshot.UnknownItems,
            WhyNotClasses: NormalizeWhyNotClasses(snapshot.RequirementStates.Select(static item => item.WhyNot)),
            FailureEvidenceDisposition: failureEvidenceDisposition,
            RetainedFailureSignatures: failureSignatures,
            OrientationEvidenceSignatures: [],
            SelectedNextLawfulMove: snapshot.SelectedNextLawfulMove,
            ReasonCode: "manual-formation-receipt",
            LawfulBasis: lawfulBasis,
            Deferred: deferred,
            Refused: refused,
            TimestampUtc: snapshot.TimestampUtc);
    }

    private static NextLawfulMoveKind SelectNextLawfulMove(
        SensoryOrientationSnapshot orientation,
        IReadOnlyList<FormationRequirementState> requirementStates,
        IReadOnlyList<FormationFailureSignature> failureSignatures,
        FailureEvidenceDispositionKind failureEvidenceDisposition)
    {
        ArgumentNullException.ThrowIfNull(orientation);
        ArgumentNullException.ThrowIfNull(requirementStates);
        ArgumentNullException.ThrowIfNull(failureSignatures);

        static bool HasState(IReadOnlyList<FormationRequirementState> states, RequirementStateKind state)
            => states.Any(item => item.State == state);

        static bool HasWhy(IReadOnlyList<FormationRequirementState> states, WhyNotClassificationKind whyNot)
            => states.Any(item => item.WhyNot == whyNot);

        if (failureSignatures.Count > 0 &&
            failureEvidenceDisposition == FailureEvidenceDispositionKind.Refuse)
        {
            return NextLawfulMoveKind.Refuse;
        }

        if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Outside)
        {
            return NextLawfulMoveKind.BindBoundary;
        }

        if (HasState(requirementStates, RequirementStateKind.Blocked) ||
            HasWhy(requirementStates, WhyNotClassificationKind.BlockedByBoundary) ||
            HasWhy(requirementStates, WhyNotClassificationKind.OutOfCone))
        {
            return NextLawfulMoveKind.BindBoundary;
        }

        if (failureSignatures.Count > 0)
        {
            return failureEvidenceDisposition == FailureEvidenceDispositionKind.PromoteToOrientationEvidence
                ? NextLawfulMoveKind.Inspect
                : NextLawfulMoveKind.Defer;
        }

        if (orientation.SourceRelation == SourceRelationKind.Unknown)
        {
            return NextLawfulMoveKind.Inspect;
        }

        if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Edge)
        {
            return NextLawfulMoveKind.Inspect;
        }

        if (HasState(requirementStates, RequirementStateKind.Missing) ||
            HasWhy(requirementStates, WhyNotClassificationKind.PrerequisiteAbsent) ||
            HasWhy(requirementStates, WhyNotClassificationKind.InsufficientEvidence))
        {
            return NextLawfulMoveKind.RequestEvidence;
        }

        if (HasState(requirementStates, RequirementStateKind.Unknown))
        {
            return NextLawfulMoveKind.Inspect;
        }

        if (requirementStates.Count > 0 &&
            requirementStates.All(item => item.State is RequirementStateKind.Present or RequirementStateKind.NotRequired) &&
            requirementStates.Any(item => item.State == RequirementStateKind.NotRequired))
        {
            return NextLawfulMoveKind.ClassifyNotRequired;
        }

        return NextLawfulMoveKind.RetainCurrentFooting;
    }

    private static FormationKnowledgePostureKind DetermineKnowledgePosture(
        SensoryOrientationSnapshot orientation,
        IReadOnlyList<FormationRequirementState> requirementStates,
        bool promotionToKnownLawful,
        FailureEvidenceDispositionKind failureEvidenceDisposition,
        NextLawfulMoveKind nextLawfulMove)
    {
        if (failureEvidenceDisposition == FailureEvidenceDispositionKind.Refuse)
        {
            return FormationKnowledgePostureKind.FailureRefused;
        }

        if (failureEvidenceDisposition == FailureEvidenceDispositionKind.PromoteToOrientationEvidence)
        {
            return FormationKnowledgePostureKind.OrientationEvidencePromoted;
        }

        if (failureEvidenceDisposition == FailureEvidenceDispositionKind.RetainAsFailureEvidence)
        {
            return FormationKnowledgePostureKind.FailureEvidenceRetained;
        }

        if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Outside)
        {
            return FormationKnowledgePostureKind.WithheldOutsideCone;
        }

        if (requirementStates.Any(static item => item.State == RequirementStateKind.Blocked) ||
            requirementStates.Any(static item => item.WhyNot == WhyNotClassificationKind.BlockedByBoundary))
        {
            return FormationKnowledgePostureKind.WithheldByBoundary;
        }

        if (requirementStates.Any(static item => item.State == RequirementStateKind.Missing) ||
            requirementStates.Any(static item => item.WhyNot is WhyNotClassificationKind.PrerequisiteAbsent or WhyNotClassificationKind.InsufficientEvidence))
        {
            return FormationKnowledgePostureKind.NeedsEvidence;
        }

        if (!promotionToKnownLawful ||
            orientation.ConeBoundary == AwarenessConeBoundaryKind.Edge ||
            orientation.SourceRelation == SourceRelationKind.Unknown ||
            requirementStates.Any(static item => item.State == RequirementStateKind.Unknown))
        {
            return FormationKnowledgePostureKind.NeedsInspection;
        }

        return nextLawfulMove == NextLawfulMoveKind.RetainCurrentFooting ||
               nextLawfulMove == NextLawfulMoveKind.ClassifyNotRequired
            ? FormationKnowledgePostureKind.KnownEnoughForNextAct
            : FormationKnowledgePostureKind.NotKnown;
    }

    private static FailureEvidenceDispositionKind DetermineFailureEvidenceDisposition(
        SensoryOrientationSnapshot orientation,
        IReadOnlyList<FormationFailureSignature> failureSignatures)
    {
        if (failureSignatures.Count == 0)
        {
            return FailureEvidenceDispositionKind.None;
        }

        if (orientation.ZedLocusState == ZedLocusState.Lost)
        {
            return FailureEvidenceDispositionKind.Refuse;
        }

        if (orientation.ZedLocusState != ZedLocusState.Preserved)
        {
            return FailureEvidenceDispositionKind.RetainAsFailureEvidence;
        }

        return HasMinimumOrientationIntegrity(orientation)
            ? FailureEvidenceDispositionKind.PromoteToOrientationEvidence
            : FailureEvidenceDispositionKind.RetainAsFailureEvidence;
    }

    private static bool HasMinimumOrientationIntegrity(SensoryOrientationSnapshot orientation)
    {
        return orientation.OrientationIntegrity is OrientationIntegrityKind.Stable or OrientationIntegrityKind.Minimum;
    }

    private static IReadOnlyList<string> DetermineLawfulKnownItems(
        CognitiveFormationSnapshot snapshot,
        bool promotionToKnownLawful,
        FailureEvidenceDispositionKind failureEvidenceDisposition)
    {
        if (!promotionToKnownLawful ||
            failureEvidenceDisposition != FailureEvidenceDispositionKind.None)
        {
            return [];
        }

        return NormalizeStrings(snapshot.KnownItems);
    }

    private static IReadOnlyList<string> DetermineRetainedUnknownItems(
        CognitiveFormationSnapshot snapshot,
        IReadOnlyList<string> lawfulKnownItems)
    {
        var knownLookup = lawfulKnownItems.ToHashSet(StringComparer.Ordinal);
        return NormalizeStrings(
            snapshot.DiscernedItems
                .Concat(snapshot.UnknownItems)
                .Where(item => !knownLookup.Contains(item)));
    }

    private static IReadOnlyList<FormationFailureSignature> DetermineOrientationEvidenceSignatures(
        IReadOnlyList<FormationFailureSignature> failureSignatures,
        FailureEvidenceDispositionKind failureEvidenceDisposition)
    {
        return failureEvidenceDisposition == FailureEvidenceDispositionKind.PromoteToOrientationEvidence
            ? failureSignatures
            : [];
    }

    private static string DetermineReasonCode(
        SensoryOrientationSnapshot orientation,
        IReadOnlyList<FormationRequirementState> requirementStates,
        IReadOnlyList<FormationFailureSignature> failureSignatures,
        FailureEvidenceDispositionKind failureEvidenceDisposition,
        NextLawfulMoveKind selectedNextLawfulMove)
    {
        if (failureSignatures.Count > 0)
        {
            return failureEvidenceDisposition switch
            {
                FailureEvidenceDispositionKind.Refuse => "formation-failure-refused-zed-locus-lost",
                FailureEvidenceDispositionKind.PromoteToOrientationEvidence => "formation-failure-promoted-to-orientation-evidence",
                FailureEvidenceDispositionKind.RetainAsFailureEvidence when orientation.ZedLocusState == ZedLocusState.Strained
                    => "formation-failure-retained-zed-locus-strained",
                FailureEvidenceDispositionKind.RetainAsFailureEvidence
                    => "formation-failure-retained-orientation-integrity-insufficient",
                _ => "formation-failure-retained"
            };
        }

        if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Outside)
        {
            return "formation-outside-cone";
        }

        if (requirementStates.Any(static item => item.State == RequirementStateKind.Blocked) ||
            requirementStates.Any(static item => item.WhyNot == WhyNotClassificationKind.BlockedByBoundary))
        {
            return "formation-blocked-by-boundary";
        }

        if (orientation.SourceRelation == SourceRelationKind.Unknown)
        {
            return "formation-source-relation-unresolved";
        }

        if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Edge)
        {
            return "formation-edge-of-cone";
        }

        if (requirementStates.Any(static item => item.State == RequirementStateKind.Missing) ||
            requirementStates.Any(static item => item.WhyNot is WhyNotClassificationKind.PrerequisiteAbsent or WhyNotClassificationKind.InsufficientEvidence))
        {
            return "formation-insufficient-evidence";
        }

        if (requirementStates.Any(static item => item.State == RequirementStateKind.Unknown))
        {
            return "formation-unknown-requirement";
        }

        if (selectedNextLawfulMove == NextLawfulMoveKind.ClassifyNotRequired)
        {
            return "formation-not-required-classified";
        }

        return "formation-retained-current-footing";
    }

    private static string DetermineLawfulBasis(
        FormationKnowledgePostureKind knowledgePosture,
        NextLawfulMoveKind selectedNextLawfulMove,
        FailureEvidenceDispositionKind failureEvidenceDisposition)
    {
        return $"{knowledgePosture.ToString().ToLowerInvariant()}::{selectedNextLawfulMove.ToString().ToLowerInvariant()}::{failureEvidenceDisposition.ToString().ToLowerInvariant()}";
    }

    private static IReadOnlyList<WhyNotClassificationKind> CollectWhyNotClasses(
        SensoryOrientationSnapshot orientation,
        IReadOnlyList<FormationRequirementState> requirementStates,
        IReadOnlyList<FormationFailureSignature> failureSignatures)
    {
        var whyNot = new List<WhyNotClassificationKind>();

        if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Outside)
        {
            whyNot.Add(WhyNotClassificationKind.OutOfCone);
        }
        else if (orientation.ConeBoundary == AwarenessConeBoundaryKind.Edge)
        {
            whyNot.Add(WhyNotClassificationKind.DeferredByPriority);
        }

        if (orientation.SourceRelation == SourceRelationKind.Unknown)
        {
            whyNot.Add(WhyNotClassificationKind.InsufficientEvidence);
        }

        if (failureSignatures.Count > 0)
        {
            whyNot.Add(WhyNotClassificationKind.CollapseSignaturePresent);

            if (orientation.ZedLocusState != ZedLocusState.Preserved)
            {
                whyNot.Add(WhyNotClassificationKind.ZedLocusNotPreserved);
            }

            if (!HasMinimumOrientationIntegrity(orientation))
            {
                whyNot.Add(WhyNotClassificationKind.OrientationIntegrityInsufficient);
            }
        }

        whyNot.AddRange(requirementStates.Select(static item => item.WhyNot));

        return NormalizeWhyNotClasses(whyNot);
    }

    private static IReadOnlyList<WhyNotClassificationKind> NormalizeWhyNotClasses(
        IEnumerable<WhyNotClassificationKind> whyNotClasses)
    {
        return whyNotClasses
            .Distinct()
            .OrderBy(static item => (int)item)
            .ToArray();
    }

    private static IReadOnlyList<FormationFailureSignature> NormalizeFailureSignatures(
        IReadOnlyList<FormationFailureSignature>? failureSignatures)
    {
        return (failureSignatures ?? Array.Empty<FormationFailureSignature>())
            .Where(static item => !string.IsNullOrWhiteSpace(item.SignatureHandle))
            .DistinctBy(static item => item.SignatureHandle, StringComparer.Ordinal)
            .OrderBy(static item => item.SignatureHandle, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> NormalizeStrings(IEnumerable<string> items)
    {
        return items
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static item => item, StringComparer.Ordinal)
            .ToArray();
    }
}
