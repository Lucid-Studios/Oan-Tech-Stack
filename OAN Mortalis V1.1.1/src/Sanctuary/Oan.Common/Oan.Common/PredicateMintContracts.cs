namespace Oan.Common;

public enum PredicateMintDecision
{
    Minted = 0,
    Deferred = 1,
    Refused = 2
}

public sealed record PredicateRefusalRecord(
    string Item,
    string ReasonCode);

public sealed record PredicateReturnSurface(
    string SurfaceHandle,
    PredicateMintDecision Decision,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    IReadOnlyList<string> Standing,
    IReadOnlyList<string> Deferred,
    IReadOnlyList<string> Conflicted,
    IReadOnlyList<string> Protected,
    IReadOnlyList<string> PermittedDerivation,
    IReadOnlyList<PredicateRefusalRecord> Refused,
    IReadOnlyList<string> ReceiptHandles,
    DateTimeOffset TimestampUtc);

public sealed record PredicateMintRequest(
    string PathHandle,
    ProtectedExecutionAuthorityClass AuthorityClass,
    ProtectedExecutionDisclosureCeiling DisclosureCeiling,
    IReadOnlyList<string> Standing,
    IReadOnlyList<string> Deferred,
    IReadOnlyList<string> Conflicted,
    IReadOnlyList<string> Protected,
    IReadOnlyList<string> PermittedDerivation,
    IReadOnlyList<PredicateRefusalRecord> Refused,
    IReadOnlyList<string> ReceiptHandles);

public interface IPredicateMintProjector
{
    PredicateReturnSurface Mint(PredicateMintRequest request);
}

public sealed class DefaultPredicateMintProjector : IPredicateMintProjector
{
    public PredicateReturnSurface Mint(PredicateMintRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PathHandle);

        var standing = NormalizeTokens(request.Standing);
        var deferred = NormalizeTokens(request.Deferred);
        var conflicted = NormalizeTokens(request.Conflicted);
        var protectedItems = NormalizeTokens(request.Protected);
        var permittedDerivation = NormalizeTokens(request.PermittedDerivation);
        var receiptHandles = NormalizeTokens(request.ReceiptHandles);
        var refused = NormalizeRefusals(request.Refused);

        EnsureDisjoint("standing", standing, "deferred", deferred);
        EnsureDisjoint("standing", standing, "conflicted", conflicted);
        EnsureDisjoint("standing", standing, "protected", protectedItems);
        EnsureDisjoint("deferred", deferred, "conflicted", conflicted);
        EnsureDisjoint("permitted_derivation", permittedDerivation, "protected", protectedItems);
        EnsureRefusedItemsNotPresent(refused, standing, deferred, conflicted, protectedItems, permittedDerivation);

        var decision = DetermineDecision(standing, deferred, conflicted, refused);
        var surfaceHandle = $"predicate-mint://{request.PathHandle.Trim()}";

        return new PredicateReturnSurface(
            SurfaceHandle: surfaceHandle,
            Decision: decision,
            AuthorityClass: request.AuthorityClass,
            DisclosureCeiling: request.DisclosureCeiling,
            Standing: standing,
            Deferred: deferred,
            Conflicted: conflicted,
            Protected: protectedItems,
            PermittedDerivation: permittedDerivation,
            Refused: refused,
            ReceiptHandles: receiptHandles,
            TimestampUtc: DateTimeOffset.UtcNow);
    }

    private static PredicateMintDecision DetermineDecision(
        IReadOnlyList<string> standing,
        IReadOnlyList<string> deferred,
        IReadOnlyList<string> conflicted,
        IReadOnlyList<PredicateRefusalRecord> refused)
    {
        if (conflicted.Count > 0 && standing.Count == 0 && deferred.Count == 0)
        {
            return PredicateMintDecision.Refused;
        }

        if (standing.Count == 0 && (deferred.Count > 0 || refused.Count > 0))
        {
            return PredicateMintDecision.Deferred;
        }

        return PredicateMintDecision.Minted;
    }

    private static IReadOnlyList<string> NormalizeTokens(IReadOnlyList<string> values)
    {
        return (values ?? Array.Empty<string>())
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<PredicateRefusalRecord> NormalizeRefusals(IReadOnlyList<PredicateRefusalRecord> values)
    {
        return (values ?? Array.Empty<PredicateRefusalRecord>())
            .Where(static item =>
                item is not null &&
                !string.IsNullOrWhiteSpace(item.Item) &&
                !string.IsNullOrWhiteSpace(item.ReasonCode))
            .Select(static item => new PredicateRefusalRecord(item.Item.Trim(), item.ReasonCode.Trim()))
            .Distinct(PredicateRefusalRecordComparer.Instance)
            .OrderBy(static item => item.Item, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static item => item.ReasonCode, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void EnsureDisjoint(
        string leftName,
        IReadOnlyList<string> left,
        string rightName,
        IReadOnlyList<string> right)
    {
        var overlap = left.Intersect(right, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (overlap is not null)
        {
            throw new InvalidOperationException(
                $"Predicate mint category '{leftName}' may not overlap '{rightName}' for item '{overlap}'.");
        }
    }

    private static void EnsureRefusedItemsNotPresent(
        IReadOnlyList<PredicateRefusalRecord> refused,
        IReadOnlyList<string> standing,
        IReadOnlyList<string> deferred,
        IReadOnlyList<string> conflicted,
        IReadOnlyList<string> protectedItems,
        IReadOnlyList<string> permittedDerivation)
    {
        var activeItems = standing
            .Concat(deferred)
            .Concat(conflicted)
            .Concat(protectedItems)
            .Concat(permittedDerivation)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var refusal in refused)
        {
            if (activeItems.Contains(refusal.Item))
            {
                throw new InvalidOperationException(
                    $"Predicate mint refusal item '{refusal.Item}' may not also appear in an active predicate category.");
            }
        }
    }

    private sealed class PredicateRefusalRecordComparer : IEqualityComparer<PredicateRefusalRecord>
    {
        public static readonly PredicateRefusalRecordComparer Instance = new();

        public bool Equals(PredicateRefusalRecord? x, PredicateRefusalRecord? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return string.Equals(x.Item, y.Item, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(x.ReasonCode, y.ReasonCode, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(PredicateRefusalRecord obj)
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ReasonCode));
        }
    }
}
