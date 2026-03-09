using GEL.Contracts;
using GEL.Graphs;
using GEL.Models;

namespace SLI.Engine.Morphology;

internal enum SliMorphologyLaneOutcome
{
    Closed,
    NeedsSpecification,
    Rejected,
    OutOfScope
}

internal sealed class SliMorphologyOperatorAnnotation
{
    public required string Token { get; init; }
    public required string Kind { get; init; }
}

internal sealed class SliMorphologyConstructorBody
{
    public required string Role { get; init; }
    public required string RootKey { get; init; }
}

internal sealed class SliMorphologySentenceResult
{
    public required string Sentence { get; init; }
    public required IReadOnlyList<string> ResolvedLemmaRoots { get; init; }
    public required IReadOnlyList<SliMorphologyOperatorAnnotation> OperatorAnnotations { get; init; }
    public required IReadOnlyList<SliMorphologyConstructorBody> ConstructorBodies { get; init; }
    public required string DiagnosticPredicateRender { get; init; }
    public required SliMorphologyLaneOutcome LaneOutcome { get; init; }
    public string? PredicateRoot { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? ScalarPayload { get; init; }
}

internal sealed class SliMorphologyParagraphResult
{
    public required SliMorphologyLaneOutcome LaneOutcome { get; init; }
    public required string Paragraph { get; init; }
    public required IReadOnlyList<SliMorphologySentenceResult> SentenceResults { get; init; }
    public required IReadOnlyList<ConstructorEdge> GraphEdges { get; init; }
}

internal sealed class SliMorphologyParagraphBodyResult
{
    public required SliMorphologyLaneOutcome LaneOutcome { get; init; }
    public required string Paragraph { get; init; }
    public required SliMorphologyParagraphResult ParagraphResult { get; init; }
    public required IReadOnlyList<string> ContinuityAnchors { get; init; }
    public required IReadOnlyList<string> ParagraphInvariants { get; init; }
    public required string ClusterDiagnosticRender { get; init; }
    public required string BodySummary { get; init; }
}

internal sealed class SliMorphologyOverlayRoot
{
    public required string Lemma { get; init; }
    public required string SymbolicCore { get; init; }
    public required string OperatorCompatibility { get; init; }
    public required string ReservedDomainStatus { get; init; }
    public required IReadOnlyList<string> DisciplinaryReservations { get; init; }
    public required IReadOnlyList<string> VariantExamples { get; init; }
}

internal interface ISliMorphologyRuntime
{
    Task<SliMorphologySentenceResult> TranslateSentenceAsync(
        string sentence,
        RootAtlas atlas,
        IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default);

    Task<SliMorphologyParagraphResult> TranslateParagraphAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default);

    Task<SliMorphologyParagraphBodyResult> TranslateParagraphBodyAsync(
        string paragraph,
        RootAtlas atlas,
        IReadOnlyList<SliMorphologyOverlayRoot> overlayRoots,
        CancellationToken cancellationToken = default);
}
