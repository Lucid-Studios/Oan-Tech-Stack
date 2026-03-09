using System.Text.Json;
using CradleTek.Memory.Models;
using GEL.Models;

namespace CradleTek.Memory.Services;

public sealed class AtlasSourceNormalizer
{
    public AtlasSourceNormalizationResult Normalize(
        Stream rootAtlasStream,
        string version,
        Stream? rootIndexStream = null,
        Stream? rootsStream = null,
        Stream? suffixIndexStream = null,
        Stream? symbolicIndexStream = null,
        Stream? reservedExpandedStream = null)
    {
        ArgumentNullException.ThrowIfNull(rootAtlasStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        using var rootAtlasDocument = JsonDocument.Parse(rootAtlasStream);
        using var rootIndexDocument = ParseOptional(rootIndexStream);
        using var rootsDocument = ParseOptional(rootsStream);
        using var suffixIndexDocument = ParseOptional(suffixIndexStream);
        using var symbolicIndexDocument = ParseOptional(symbolicIndexStream);
        using var reservedExpandedDocument = ParseOptional(reservedExpandedStream);

        var diagnostics = new List<AtlasSourceNormalizationDiagnostic>();
        var rootSymbols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var symbolOwners = new Dictionary<string, string>(StringComparer.Ordinal);
        var prefixSymbols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var suffixSymbols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var reservedSymbols = new HashSet<string>(StringComparer.Ordinal);

        if (reservedExpandedDocument is not null)
        {
            LoadReservedSymbols(reservedExpandedDocument.RootElement, reservedSymbols);
        }

        if (rootIndexDocument is not null)
        {
            LoadRootSymbols(rootIndexDocument.RootElement, "RootIndex", rootSymbols, symbolOwners, reservedSymbols, diagnostics);
        }

        if (rootsDocument is not null)
        {
            LoadWrappedRootSymbols(rootsDocument.RootElement, "Roots", rootSymbols, symbolOwners, reservedSymbols, diagnostics);
        }

        if (symbolicIndexDocument is not null)
        {
            LoadPrefixSymbols(symbolicIndexDocument.RootElement, reservedSymbols, prefixSymbols, diagnostics);
        }

        if (suffixIndexDocument is not null)
        {
            LoadSuffixSymbols(suffixIndexDocument.RootElement, reservedSymbols, suffixSymbols, diagnostics);
        }

        var entries = new List<RootAtlasEntry>();
        foreach (var property in rootAtlasDocument.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                {
                    Severity = AtlasSourceDiagnosticSeverity.Error,
                    Code = "atlas.invalid_entry_shape",
                    SourceLayer = "RootAtlas",
                    RootKey = property.Name,
                    Message = $"Atlas entry '{property.Name}' is not an object."
                });
                continue;
            }

            var rootKey = NormalizeRoot(property.Name);
            if (!property.Value.TryGetProperty("root", out var rootProperty) || rootProperty.ValueKind != JsonValueKind.String)
            {
                diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                {
                    Severity = AtlasSourceDiagnosticSeverity.Error,
                    Code = "atlas.missing_root",
                    SourceLayer = "RootAtlas",
                    RootKey = rootKey,
                    Message = $"Atlas entry '{property.Name}' is missing a canonical root string."
                });
                continue;
            }

            var declaredRoot = NormalizeRoot(rootProperty.GetString());
            if (!string.Equals(rootKey, declaredRoot, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                {
                    Severity = AtlasSourceDiagnosticSeverity.Error,
                    Code = "atlas.root_key_mismatch",
                    SourceLayer = "RootAtlas",
                    RootKey = rootKey,
                    Message = $"Atlas entry key '{property.Name}' does not match declared root '{declaredRoot}'."
                });
                continue;
            }

            var variants = new List<string>();
            if (property.Value.TryGetProperty("variants", out var variantsProperty) && variantsProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var variantNode in variantsProperty.EnumerateArray())
                {
                    if (variantNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var variant = NormalizeVariant(variantNode.GetString());
                    if (string.IsNullOrWhiteSpace(variant))
                    {
                        continue;
                    }

                    ValidateVariantConstructor(rootKey, variant, prefixSymbols, suffixSymbols, diagnostics);
                    variants.Add(variant);
                }
            }

            var atlasDomain = $"atlas.root.{rootKey[0]}";
            var hasRootSymbol = rootSymbols.TryGetValue(rootKey, out var rootSymbol);
            var symbolicHandle = hasRootSymbol
                ? $"ATLAS.SYM.{rootSymbol}"
                : null;

            if (symbolicHandle is null)
            {
                diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                {
                    Severity = AtlasSourceDiagnosticSeverity.Warning,
                    Code = "atlas.root_symbol_missing",
                    SourceLayer = "RootAtlas",
                    RootKey = rootKey,
                    Message = $"Atlas root '{rootKey}' does not currently resolve to a root-level symbolic assignment."
                });
            }

            var symbolicConstructors = BuildSymbolicConstructors(
                rootKey,
                hasRootSymbol ? rootSymbol! : rootKey,
                hasRootSymbol ? rootSymbol : null,
                variants,
                prefixSymbols,
                suffixSymbols,
                diagnostics);

            entries.Add(new RootAtlasEntry
            {
                Root = new PredicateRoot
                {
                    Key = rootKey,
                    DisplayLabel = declaredRoot,
                    AtlasDomain = atlasDomain,
                    SymbolicHandle = symbolicHandle,
                    DictionaryPointer = $"atlas://root/{rootKey}"
                },
                VariantForms = variants
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(variant => variant, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                SymbolicConstructors = symbolicConstructors,
                FrequencyWeight = 1d
            });
        }

        var domains = entries
            .Select(entry => entry.Root.AtlasDomain)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(domain => domain, StringComparer.OrdinalIgnoreCase)
            .Select(domain => new DomainDescriptor
            {
                DomainName = domain,
                Description = $"External atlas source domain {domain}.",
                Tags = ["external-root-atlas", "canonical-atlas-source"]
            })
            .ToArray();

        var hasErrors = diagnostics.Any(diagnostic => diagnostic.Severity == AtlasSourceDiagnosticSeverity.Error);
        var canonicalRootAtlas = hasErrors
            ? null
            : RootAtlas.Create(version, entries, domainDescriptors: domains);

        return new AtlasSourceNormalizationResult
        {
            CanonicalRootAtlas = canonicalRootAtlas,
            Diagnostics = diagnostics,
            RootSymbols = rootSymbols
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
            PrefixSymbols = prefixSymbols
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
            SuffixSymbols = suffixSymbols
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static JsonDocument? ParseOptional(Stream? stream)
    {
        if (stream is null)
        {
            return null;
        }

        return JsonDocument.Parse(stream);
    }

    private static void LoadReservedSymbols(
        JsonElement root,
        HashSet<string> reservedSymbols)
    {
        if (!root.TryGetProperty("assignment_rules", out var assignmentRules) ||
            assignmentRules.ValueKind != JsonValueKind.Object ||
            !assignmentRules.TryGetProperty("reserved", out var reserved) ||
            reserved.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var group in reserved.EnumerateObject())
        {
            if (group.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var node in group.Value.EnumerateArray())
            {
                if (node.ValueKind == JsonValueKind.String)
                {
                    var symbol = node.GetString();
                    if (!string.IsNullOrWhiteSpace(symbol))
                    {
                        reservedSymbols.Add(symbol);
                    }
                }
            }
        }
    }

    private static void LoadRootSymbols(
        JsonElement root,
        string sourceLayer,
        Dictionary<string, string> rootSymbols,
        Dictionary<string, string> symbolOwners,
        HashSet<string> reservedSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        foreach (var letterGroup in root.EnumerateObject())
        {
            if (letterGroup.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var rootNode in letterGroup.Value.EnumerateObject())
            {
                if (rootNode.Value.ValueKind != JsonValueKind.Object ||
                    !rootNode.Value.TryGetProperty("symbol", out var symbolNode) ||
                    symbolNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                RegisterRootSymbol(
                    sourceLayer,
                    NormalizeRoot(rootNode.Name),
                    symbolNode.GetString(),
                    rootSymbols,
                    symbolOwners,
                    reservedSymbols,
                    diagnostics);
            }
        }
    }

    private static void LoadWrappedRootSymbols(
        JsonElement root,
        string sourceLayer,
        Dictionary<string, string> rootSymbols,
        Dictionary<string, string> symbolOwners,
        HashSet<string> reservedSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        if (!root.TryGetProperty("roots", out var wrappedRoots) || wrappedRoots.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new AtlasSourceNormalizationDiagnostic
            {
                Severity = AtlasSourceDiagnosticSeverity.Error,
                Code = "atlas.invalid_wrapped_roots",
                SourceLayer = sourceLayer,
                Message = "Wrapped roots source is missing the top-level 'roots' object."
            });
            return;
        }

        LoadRootSymbols(wrappedRoots, sourceLayer, rootSymbols, symbolOwners, reservedSymbols, diagnostics);
    }

    private static void RegisterRootSymbol(
        string sourceLayer,
        string rootKey,
        string? symbol,
        Dictionary<string, string> rootSymbols,
        Dictionary<string, string> symbolOwners,
        HashSet<string> reservedSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(rootKey) || string.IsNullOrWhiteSpace(symbol))
        {
            return;
        }

        if (reservedSymbols.Contains(symbol))
        {
            diagnostics.Add(new AtlasSourceNormalizationDiagnostic
            {
                Severity = AtlasSourceDiagnosticSeverity.Error,
                Code = "atlas.reserved_root_symbol",
                SourceLayer = sourceLayer,
                RootKey = rootKey,
                Message = $"Root '{rootKey}' uses reserved symbol '{symbol}'."
            });
            return;
        }

        if (rootSymbols.TryGetValue(rootKey, out var existingSymbol) &&
            !string.Equals(existingSymbol, symbol, StringComparison.Ordinal))
        {
            diagnostics.Add(new AtlasSourceNormalizationDiagnostic
            {
                Severity = AtlasSourceDiagnosticSeverity.Error,
                Code = "atlas.root_symbol_conflict",
                SourceLayer = sourceLayer,
                RootKey = rootKey,
                Message = $"Root '{rootKey}' resolves to conflicting symbols '{existingSymbol}' and '{symbol}'."
            });
            return;
        }

        if (symbolOwners.TryGetValue(symbol, out var existingRoot) &&
            !string.Equals(existingRoot, rootKey, StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new AtlasSourceNormalizationDiagnostic
            {
                Severity = AtlasSourceDiagnosticSeverity.Error,
                Code = "atlas.symbol_collision",
                SourceLayer = sourceLayer,
                RootKey = rootKey,
                Message = $"Symbol '{symbol}' is already assigned to root '{existingRoot}'."
            });
            return;
        }

        rootSymbols[rootKey] = symbol;
        symbolOwners[symbol] = rootKey;
    }

    private static void LoadPrefixSymbols(
        JsonElement root,
        HashSet<string> reservedSymbols,
        Dictionary<string, string> prefixSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        if (!root.TryGetProperty("prefixes", out var prefixesRoot) ||
            prefixesRoot.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var outerNode in prefixesRoot.EnumerateObject())
        {
            if (outerNode.Value.ValueKind != JsonValueKind.Object ||
                !outerNode.Value.TryGetProperty("prefixes", out var prefixesNode) ||
                prefixesNode.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var letterNode in prefixesNode.EnumerateObject())
            {
                if (letterNode.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (var prefixNode in letterNode.Value.EnumerateObject())
                {
                    if (prefixNode.Value.ValueKind != JsonValueKind.Object ||
                        !prefixNode.Value.TryGetProperty("symbol", out var symbolNode) ||
                        symbolNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    RegisterOperatorSymbol(
                        "SymbolicIndex",
                        $"prefix:{prefixNode.Name}",
                        NormalizePrefix(prefixNode.Name),
                        symbolNode.GetString(),
                        reservedSymbols,
                        prefixSymbols,
                        diagnostics);
                }
            }
        }
    }

    private static void LoadSuffixSymbols(
        JsonElement root,
        HashSet<string> reservedSymbols,
        Dictionary<string, string> suffixSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        foreach (var letterNode in root.EnumerateObject())
        {
            if (letterNode.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var suffixNode in letterNode.Value.EnumerateObject())
            {
                if (suffixNode.Value.ValueKind != JsonValueKind.Object ||
                    !suffixNode.Value.TryGetProperty("symbol", out var symbolNode) ||
                    symbolNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                RegisterOperatorSymbol(
                    "SuffixIndex",
                    $"suffix:{suffixNode.Name}",
                    NormalizeSuffix(suffixNode.Name),
                    symbolNode.GetString(),
                    reservedSymbols,
                    suffixSymbols,
                    diagnostics);
            }
        }
    }

    private static void RegisterOperatorSymbol(
        string sourceLayer,
        string owner,
        string key,
        string? symbol,
        HashSet<string> reservedSymbols,
        Dictionary<string, string> symbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(symbol))
        {
            return;
        }

        if (reservedSymbols.Contains(symbol))
        {
            diagnostics.Add(new AtlasSourceNormalizationDiagnostic
            {
                Severity = AtlasSourceDiagnosticSeverity.Error,
                Code = "atlas.reserved_operator_symbol",
                SourceLayer = sourceLayer,
                RootKey = owner,
                Message = $"Operator '{owner}' uses reserved symbol '{symbol}'."
            });
            return;
        }

        symbols[key] = symbol;
    }

    private static void ValidateVariantConstructor(
        string rootKey,
        string variant,
        Dictionary<string, string> prefixSymbols,
        Dictionary<string, string> suffixSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        foreach (var token in variant.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            if (token.EndsWith("-", StringComparison.Ordinal))
            {
                var prefix = NormalizePrefix(token[..^1]);
                if (!prefixSymbols.ContainsKey(prefix))
                {
                    diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                    {
                        Severity = AtlasSourceDiagnosticSeverity.Error,
                        Code = "atlas.unknown_prefix_operator",
                        SourceLayer = "RootAtlas",
                        RootKey = rootKey,
                        Message = $"Variant '{variant}' references unknown prefix operator '{token}'."
                    });
                }

                continue;
            }

            if (token.StartsWith("-", StringComparison.Ordinal))
            {
                var suffix = NormalizeSuffix(token);
                if (!suffixSymbols.ContainsKey(suffix))
                {
                    diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                    {
                        Severity = AtlasSourceDiagnosticSeverity.Error,
                        Code = "atlas.unknown_suffix_operator",
                        SourceLayer = "RootAtlas",
                        RootKey = rootKey,
                        Message = $"Variant '{variant}' references unknown suffix operator '{token}'."
                    });
                }
            }
        }
    }

    private static IReadOnlyList<SymbolicConstructorTriplet> BuildSymbolicConstructors(
        string rootKey,
        string canonicalRootSymbol,
        string? renderableRootSymbol,
        IReadOnlyList<string> variants,
        Dictionary<string, string> prefixSymbols,
        Dictionary<string, string> suffixSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics)
    {
        var constructors = new List<SymbolicConstructorTriplet>
        {
            CreateTriplet(rootKey, canonicalRootSymbol, renderableRootSymbol)
        };

        foreach (var variant in variants
                     .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!TryBuildVariantTriplet(
                    rootKey,
                    canonicalRootSymbol,
                    renderableRootSymbol,
                    variant,
                    prefixSymbols,
                    suffixSymbols,
                    diagnostics,
                    out var triplet))
            {
                continue;
            }

            constructors.Add(triplet);
        }

        return constructors
            .GroupBy(constructor => constructor.CanonicalText, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(constructor => constructor.RootKey, StringComparer.OrdinalIgnoreCase)
            .ThenBy(constructor => constructor.PrefixKey ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(constructor => constructor.SuffixKey ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(constructor => constructor.CanonicalText, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool TryBuildVariantTriplet(
        string rootKey,
        string canonicalRootSymbol,
        string? renderableRootSymbol,
        string variant,
        Dictionary<string, string> prefixSymbols,
        Dictionary<string, string> suffixSymbols,
        List<AtlasSourceNormalizationDiagnostic> diagnostics,
        out SymbolicConstructorTriplet triplet)
    {
        triplet = default!;

        var prefixKey = default(string);
        var prefixSymbol = default(string);
        var suffixKey = default(string);
        var suffixSymbol = default(string);
        var sawOperator = false;

        foreach (var token in variant.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            if (token.EndsWith("-", StringComparison.Ordinal))
            {
                sawOperator = true;
                if (prefixKey is not null)
                {
                    diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                    {
                        Severity = AtlasSourceDiagnosticSeverity.Error,
                        Code = "atlas.multiple_prefix_operators",
                        SourceLayer = "RootAtlas",
                        RootKey = rootKey,
                        Message = $"Variant '{variant}' declares multiple prefix operators."
                    });
                    return false;
                }

                var normalizedPrefix = NormalizePrefix(token[..^1]);
                if (!prefixSymbols.TryGetValue(normalizedPrefix, out prefixSymbol))
                {
                    return false;
                }

                prefixKey = normalizedPrefix;
                continue;
            }

            if (token.StartsWith("-", StringComparison.Ordinal))
            {
                sawOperator = true;
                if (suffixKey is not null)
                {
                    diagnostics.Add(new AtlasSourceNormalizationDiagnostic
                    {
                        Severity = AtlasSourceDiagnosticSeverity.Error,
                        Code = "atlas.multiple_suffix_operators",
                        SourceLayer = "RootAtlas",
                        RootKey = rootKey,
                        Message = $"Variant '{variant}' declares multiple suffix operators."
                    });
                    return false;
                }

                var normalizedSuffix = NormalizeSuffix(token);
                if (!suffixSymbols.TryGetValue(normalizedSuffix, out suffixSymbol))
                {
                    return false;
                }

                suffixKey = normalizedSuffix;
            }
        }

        if (!sawOperator)
        {
            return false;
        }

        triplet = CreateTriplet(
            rootKey,
            canonicalRootSymbol,
            renderableRootSymbol,
            prefixKey,
            prefixSymbol,
            suffixKey,
            suffixSymbol);
        return true;
    }

    private static SymbolicConstructorTriplet CreateTriplet(
        string rootKey,
        string canonicalRootSymbol,
        string? renderableRootSymbol,
        string? prefixKey = null,
        string? prefixSymbol = null,
        string? suffixKey = null,
        string? suffixSymbol = null)
    {
        var canonicalText = BuildCanonicalText(prefixKey, rootKey, suffixKey);
        var mergedGlyph = string.Concat(
            prefixSymbol ?? string.Empty,
            renderableRootSymbol ?? string.Empty,
            suffixSymbol ?? string.Empty);

        return new SymbolicConstructorTriplet
        {
            PrefixKey = prefixKey,
            RootKey = rootKey,
            SuffixKey = suffixKey,
            PrefixSymbol = prefixSymbol,
            RootSymbol = canonicalRootSymbol,
            SuffixSymbol = suffixSymbol,
            CanonicalText = canonicalText,
            MergedGlyph = string.IsNullOrWhiteSpace(mergedGlyph) ? null : mergedGlyph
        };
    }

    private static string BuildCanonicalText(string? prefixKey, string rootKey, string? suffixKey)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(prefixKey))
        {
            parts.Add($"{prefixKey}-");
        }

        parts.Add(rootKey);

        if (!string.IsNullOrWhiteSpace(suffixKey))
        {
            parts.Add(suffixKey!);
        }

        return string.Join(" | ", parts);
    }

    private static string NormalizeRoot(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private static string NormalizeVariant(string? value) => (value ?? string.Empty).Trim();

    private static string NormalizePrefix(string? value) => (value ?? string.Empty).Trim().TrimEnd('-').ToLowerInvariant();

    private static string NormalizeSuffix(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized.StartsWith("-", StringComparison.Ordinal) ? normalized : $"-{normalized.TrimStart('-')}";
    }
}
