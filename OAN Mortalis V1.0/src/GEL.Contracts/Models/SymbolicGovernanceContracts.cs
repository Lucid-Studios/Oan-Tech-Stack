using Oan.Spinal;

namespace GEL.Models;

public enum SymbolicDomainClass
{
    GrammarOperator,
    RootNativeCore,
    DisciplinaryReserved,
    GovernanceMeta,
    ExperimentalExtension
}

public enum SymbolicConstructorRole
{
    PrefixOperator,
    RootCore,
    SuffixOperator,
    DisciplinaryOverlay
}

public enum KnownUnknownExtensionState
{
    Closed,
    BootstrapClosed,
    NeedsSpecification,
    ExtensionCandidate,
    ProhibitedCollision
}

public sealed class ReservedSymbolicDomain
{
    public required string Key { get; init; }
    public required string DisplayLabel { get; init; }
    public required SymbolicDomainClass DomainClass { get; init; }
    public string? Description { get; init; }
    public required IReadOnlyList<string> ReservedSymbols { get; init; }
    public required IReadOnlyList<string> AllowedBridgeDomainKeys { get; init; }
    public required IReadOnlyList<SymbolicConstructorRole> AllowedConstructorRoles { get; init; }
    public required bool AllowsMergedRenderForms { get; init; }
}

public sealed class SymbolicDomainConstitution
{
    public required string Version { get; init; }
    public required IReadOnlyList<ReservedSymbolicDomain> Domains { get; init; }
    public required IReadOnlyList<string> GlobalReservedSymbols { get; init; }
    public required string Digest { get; init; }

    public static SymbolicDomainConstitution Create(
        string version,
        IEnumerable<ReservedSymbolicDomain> domains,
        IEnumerable<string>? globalReservedSymbols = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(domains);

        var normalizedDomains = domains
            .OrderBy(domain => domain.Key, StringComparer.OrdinalIgnoreCase)
            .Select(domain => new ReservedSymbolicDomain
            {
                Key = domain.Key,
                DisplayLabel = domain.DisplayLabel,
                DomainClass = domain.DomainClass,
                Description = domain.Description,
                ReservedSymbols = domain.ReservedSymbols
                    .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(symbol => symbol, StringComparer.Ordinal)
                    .ToArray(),
                AllowedBridgeDomainKeys = domain.AllowedBridgeDomainKeys
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                AllowedConstructorRoles = domain.AllowedConstructorRoles
                    .Distinct()
                    .OrderBy(role => role)
                    .ToArray(),
                AllowsMergedRenderForms = domain.AllowsMergedRenderForms
            })
            .ToArray();

        var normalizedGlobalReservedSymbols = (globalReservedSymbols ?? Array.Empty<string>())
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(symbol => symbol, StringComparer.Ordinal)
            .ToArray();

        var digestProjection = new
        {
            Version = version,
            Domains = normalizedDomains.Select(domain => new
            {
                domain.Key,
                domain.DisplayLabel,
                DomainClass = domain.DomainClass.ToString(),
                domain.Description,
                domain.ReservedSymbols,
                domain.AllowedBridgeDomainKeys,
                AllowedConstructorRoles = domain.AllowedConstructorRoles.Select(role => role.ToString()),
                domain.AllowsMergedRenderForms
            }),
            GlobalReservedSymbols = normalizedGlobalReservedSymbols
        };

        var digest = Primitives.ComputeHash(Primitives.ToCanonicalJson(digestProjection));

        return new SymbolicDomainConstitution
        {
            Version = version,
            Domains = normalizedDomains,
            GlobalReservedSymbols = normalizedGlobalReservedSymbols,
            Digest = digest
        };
    }
}
