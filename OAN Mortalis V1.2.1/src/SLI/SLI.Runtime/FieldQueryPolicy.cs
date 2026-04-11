using San.Common;

namespace SLI.Runtime;

public interface IFieldQueryEngine
{
    FieldQueryResult Evaluate(FieldQuery query, IReadOnlyList<FieldProductSnapshot> fieldProducts);

    RecompositionCandidate ComposeCandidate(
        FieldQueryResult queryResult,
        string candidateId,
        RecompositionCandidateClass candidateClass,
        DateTimeOffset createdAtUtc);
}

public static class QueryTensionNoteCodes
{
    public const string PassportTruthPreserved = "passport-truth-preserved";
    public const string AuthorityCeilingPreserved = "authority-ceiling-preserved";
    public const string QueryDoesNotRewrite = "query-does-not-rewrite";
    public const string HardContradictionVisible = "hard-contradiction-visible";
    public const string CandidateOnlyRecomposition = "candidate-only-recomposition";
    public const string NoFieldProductsMatched = "no-field-products-matched";
}

public sealed class DefaultFieldQueryEngine : IFieldQueryEngine
{
    public FieldQueryResult Evaluate(FieldQuery query, IReadOnlyList<FieldProductSnapshot> fieldProducts)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(fieldProducts);

        if (string.IsNullOrWhiteSpace(query.QueryId))
        {
            throw new InvalidOperationException("field query requires an explicit query id.");
        }

        if (string.IsNullOrWhiteSpace(query.RequestedByTraceId))
        {
            throw new InvalidOperationException("field query requires an explicit request trace id.");
        }

        if (query.Axes is null || query.Axes.Count == 0)
        {
            throw new InvalidOperationException("field query requires at least one active query axis.");
        }

        if (fieldProducts.Any(static product => product is null))
        {
            throw new InvalidOperationException("field query may not consume null field products.");
        }

        var activeAxes = query.Axes
            .Distinct()
            .OrderBy(static axis => axis)
            .ToArray();

        foreach (var axis in activeAxes)
        {
            EnsureAxisBinding(query, axis);
        }

        var matches = new List<FieldQueryMatch>();

        foreach (var product in fieldProducts)
        {
            if (Matches(query, activeAxes, product, out var matchedAxes))
            {
                matches.Add(new FieldQueryMatch(
                    Product: product,
                    MatchedAxes: matchedAxes,
                    PassportTruthPreserved: true,
                    RetrievalTrace: $"query://field/{query.QueryId}/product/{product.ProductId}"));
            }
        }

        var tensionSummary = BuildSummary(activeAxes, fieldProducts.Count, matches);

        return new FieldQueryResult(
            Query: query,
            Matches: matches,
            TensionSummary: tensionSummary,
            MembraneReentryRequired: true,
            EvaluatedAtUtc: query.RequestedAtUtc);
    }

    public RecompositionCandidate ComposeCandidate(
        FieldQueryResult queryResult,
        string candidateId,
        RecompositionCandidateClass candidateClass,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(queryResult);

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new InvalidOperationException("recomposition candidate requires an explicit candidate id.");
        }

        var sources = queryResult.Matches
            .Select(match => new RecompositionCandidateProvenance(
                ProductId: match.Product.ProductId,
                ReceiptId: match.Product.ReceiptId,
                WitnessSnapshotId: match.Product.WitnessSnapshotId,
                SourceTraceId: match.Product.Envelope.TraceId,
                Lane: match.Product.Lane,
                Family: match.Product.Envelope.Family,
                ProductClass: match.Product.Envelope.ProductClass,
                Admissibility: match.Product.Envelope.Admissibility,
                ContradictionState: match.Product.Envelope.ContradictionState,
                ReceivedAtUtc: match.Product.ReceivedAtUtc))
            .ToArray();

        var contradictionState = DetermineContradictionState(sources);
        var disposition = sources.Length == 0
            ? RecompositionCandidateDisposition.Withheld
            : RecompositionCandidateDisposition.CandidateOnly;

        return new RecompositionCandidate(
            CandidateId: candidateId,
            QueryId: queryResult.Query.QueryId,
            CandidateClass: candidateClass,
            Disposition: disposition,
            Sources: sources,
            TensionSummary: queryResult.TensionSummary,
            Admissibility: AdmissibilityStatus.Pending,
            ContradictionState: contradictionState,
            MaterializationEligibility: MaterializationEligibility.No,
            PersistenceEligibility: PersistenceEligibility.Never,
            RequiresMembraneReentry: true,
            CreatedAtUtc: createdAtUtc);
    }

    private static QueryTensionSummary BuildSummary(
        IReadOnlyList<FieldQueryAxis> activeAxes,
        int sourceCount,
        IReadOnlyList<FieldQueryMatch> matches)
    {
        var withheldCount = Math.Max(0, sourceCount - matches.Count);
        var contradictionVisible = matches.Any(match => match.Product.Envelope.ContradictionState == ContradictionState.Hard);

        var tensionState = matches.Count == 0
            ? QueryTensionState.Withheld
            : contradictionVisible
                ? QueryTensionState.Contradicted
                : activeAxes.Count > 1 || withheldCount > 0
                    ? QueryTensionState.Narrowed
                    : QueryTensionState.Stable;

        var notes = new List<QueryTensionNote>
        {
            new(
                Code: QueryTensionNoteCodes.PassportTruthPreserved,
                Message: "retrieved field products remain under their original constitutional passport and may not silently widen that passport through recall alone."),
            new(
                Code: QueryTensionNoteCodes.AuthorityCeilingPreserved,
                Message: "query preserves existing authority ceilings and does not make recalled products promotable, continuity-bearing, or operative by convenience."),
            new(
                Code: QueryTensionNoteCodes.QueryDoesNotRewrite,
                Message: "query must not imply rewrite; lawful retrieval inspects preserved field products without mutating the field itself."),
            new(
                Code: QueryTensionNoteCodes.CandidateOnlyRecomposition,
                Message: "recomposition remains candidate-only and any later action still requires lawful membrane re-entry.")
        };

        if (contradictionVisible)
        {
            notes.Add(new QueryTensionNote(
                Code: QueryTensionNoteCodes.HardContradictionVisible,
                Message: "hard contradiction remains visible in retrieved products and may not be normalized away during query or recomposition preparation."));
        }

        if (matches.Count == 0)
        {
            notes.Add(new QueryTensionNote(
                Code: QueryTensionNoteCodes.NoFieldProductsMatched,
                Message: "no field products satisfied the active query axes, so no recomposition candidate may claim standing from this retrieval."));
        }

        return new QueryTensionSummary(
            ActiveAxes: activeAxes.ToArray(),
            TensionState: tensionState,
            SourceCount: sourceCount,
            MatchCount: matches.Count,
            WithheldCount: withheldCount,
            PassportTruthPreserved: true,
            AuthorityCeilingPreserved: true,
            MembraneReentryRequired: true,
            Notes: notes);
    }

    private static void EnsureAxisBinding(FieldQuery query, FieldQueryAxis axis)
    {
        var missingBinding = axis switch
        {
            FieldQueryAxis.Family => query.Family is null,
            FieldQueryAxis.ProductClass => query.ProductClass is null,
            FieldQueryAxis.Intent => query.Intent is null,
            FieldQueryAxis.Admissibility => query.Admissibility is null,
            FieldQueryAxis.Contradiction => query.ContradictionState is null,
            FieldQueryAxis.TemporalWindow => query.TemporalWindow is null,
            FieldQueryAxis.TraceLineage => string.IsNullOrWhiteSpace(query.TraceLineagePrefix),
            FieldQueryAxis.LaneScope => query.LaneScope is null,
            FieldQueryAxis.Origin => string.IsNullOrWhiteSpace(query.Origin),
            _ => true
        };

        if (missingBinding)
        {
            throw new InvalidOperationException(
                $"field query axis '{axis}' requires an explicit binding before retrieval may proceed.");
        }

        if (axis == FieldQueryAxis.TemporalWindow &&
            query.TemporalWindow is { FromUtc: not null, ToUtc: not null } window &&
            window.FromUtc > window.ToUtc)
        {
            throw new InvalidOperationException("field query temporal window requires from <= to.");
        }
    }

    private static bool Matches(
        FieldQuery query,
        IReadOnlyList<FieldQueryAxis> activeAxes,
        FieldProductSnapshot product,
        out IReadOnlyList<FieldQueryAxis> matchedAxes)
    {
        var matches = new List<FieldQueryAxis>(activeAxes.Count);

        foreach (var axis in activeAxes)
        {
            var axisMatched = axis switch
            {
                FieldQueryAxis.Family => query.Family is { } family && product.Envelope.Family == family,
                FieldQueryAxis.ProductClass => query.ProductClass is { } productClass && product.Envelope.ProductClass == productClass,
                FieldQueryAxis.Intent => query.Intent is { } intent && product.Envelope.Intent == intent,
                FieldQueryAxis.Admissibility => query.Admissibility is { } admissibility && product.Envelope.Admissibility == admissibility,
                FieldQueryAxis.Contradiction => query.ContradictionState is { } contradiction && product.Envelope.ContradictionState == contradiction,
                FieldQueryAxis.TemporalWindow => query.TemporalWindow is { } window && IsWithinWindow(product.ReceivedAtUtc, window),
                FieldQueryAxis.TraceLineage => !string.IsNullOrWhiteSpace(query.TraceLineagePrefix) &&
                                               product.Envelope.TraceId.StartsWith(query.TraceLineagePrefix, StringComparison.Ordinal),
                FieldQueryAxis.LaneScope => query.LaneScope is { } lane && product.Lane == lane,
                FieldQueryAxis.Origin => !string.IsNullOrWhiteSpace(query.Origin) &&
                                         string.Equals(product.Envelope.Origin, query.Origin, StringComparison.Ordinal),
                _ => false
            };

            if (!axisMatched)
            {
                matchedAxes = Array.Empty<FieldQueryAxis>();
                return false;
            }

            matches.Add(axis);
        }

        matchedAxes = matches.ToArray();
        return true;
    }

    private static bool IsWithinWindow(DateTimeOffset observedAtUtc, FieldTemporalWindow temporalWindow)
    {
        if (temporalWindow.FromUtc is { } fromUtc && observedAtUtc < fromUtc)
        {
            return false;
        }

        if (temporalWindow.ToUtc is { } toUtc && observedAtUtc > toUtc)
        {
            return false;
        }

        return true;
    }

    private static ContradictionState DetermineContradictionState(
        IReadOnlyList<RecompositionCandidateProvenance> sources)
    {
        if (sources.Any(static source => source.ContradictionState == ContradictionState.Hard))
        {
            return ContradictionState.Hard;
        }

        if (sources.Any(static source => source.ContradictionState == ContradictionState.Soft))
        {
            return ContradictionState.Soft;
        }

        return ContradictionState.None;
    }
}
