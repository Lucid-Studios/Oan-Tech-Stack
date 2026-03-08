using System.Text;
using CradleTek.Memory.Services;

namespace Oan.Sli.Tests;

public sealed class AtlasSourceNormalizerTests
{
    [Fact]
    public void Normalize_ValidAtlasSources_ProducesCanonicalRootAtlas()
    {
        using var rootAtlas = Utf8Stream("""
            {
              "accept": { "root": "accept", "variants": ["", "-ed", "un-"] },
              "method": { "root": "method", "variants": ["", "-ology", "pre-"] }
            }
            """);
        using var rootIndex = Utf8Stream("""
            {
              "a": { "accept": { "symbol": "A1" } },
              "m": { "method": { "symbol": "M1" } }
            }
            """);
        using var roots = Utf8Stream("""
            {
              "roots": {
                "a": { "accept": { "symbol": "A1" } },
                "m": { "method": { "symbol": "M1" } }
              }
            }
            """);
        using var suffixIndex = Utf8Stream("""
            {
              "e": { "-ed": { "symbol": "SE1" } },
              "o": { "-ology": { "symbol": "SO1" } }
            }
            """);
        using var symbolicIndex = Utf8Stream("""
            {
              "prefixes": {
                "p": {
                  "prefixes": {
                    "u": { "un": { "symbol": "PU1" } },
                    "p": { "pre": { "symbol": "PP1" } }
                  }
                }
              },
              "roots": {},
              "suffixes": {}
            }
            """);
        using var reservedExpanded = Utf8Stream("""
            {
              "assignment_rules": {
                "reserved": {
                  "meta": ["$"],
                  "control": ["@"]
                }
              }
            }
            """);

        var normalizer = new AtlasSourceNormalizer();
        var result = normalizer.Normalize(
            rootAtlas,
            "external.root-atlas.v1",
            rootIndex,
            roots,
            suffixIndex,
            symbolicIndex,
            reservedExpanded);

        Assert.False(result.HasErrors);
        Assert.NotNull(result.CanonicalRootAtlas);
        Assert.Equal(2, result.CanonicalRootAtlas!.Entries.Count);
        Assert.True(result.CanonicalRootAtlas.TryResolveRoot("accept", out var root));
        Assert.Equal("ATLAS.SYM.A1", root.SymbolicHandle);
        Assert.Contains(result.CanonicalRootAtlas.Entries, entry => entry.Root.Key == "method" && entry.VariantForms.Contains("-ology"));
    }

    [Fact]
    public void Normalize_RootIndexAndRootsConflict_ProducesError()
    {
        using var rootAtlas = Utf8Stream("""
            {
              "accept": { "root": "accept", "variants": [""] }
            }
            """);
        using var rootIndex = Utf8Stream("""
            {
              "a": { "accept": { "symbol": "A1" } }
            }
            """);
        using var roots = Utf8Stream("""
            {
              "roots": {
                "a": { "accept": { "symbol": "A2" } }
              }
            }
            """);

        var normalizer = new AtlasSourceNormalizer();
        var result = normalizer.Normalize(rootAtlas, "external.root-atlas.v1", rootIndex, roots);

        Assert.True(result.HasErrors);
        Assert.Null(result.CanonicalRootAtlas);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "atlas.root_symbol_conflict");
    }

    [Fact]
    public void Normalize_ReservedOrUnknownConstructors_ProduceErrors()
    {
        using var rootAtlas = Utf8Stream("""
            {
              "accept": { "root": "accept", "variants": ["neo-", "-ed"] }
            }
            """);
        using var rootIndex = Utf8Stream("""
            {
              "a": { "accept": { "symbol": "@" } }
            }
            """);
        using var suffixIndex = Utf8Stream("""
            {
              "e": { "-ed": { "symbol": "SE1" } }
            }
            """);
        using var symbolicIndex = Utf8Stream("""
            {
              "prefixes": {
                "p": {
                  "prefixes": {
                    "u": { "un": { "symbol": "PU1" } }
                  }
                }
              },
              "roots": {},
              "suffixes": {}
            }
            """);
        using var reservedExpanded = Utf8Stream("""
            {
              "assignment_rules": {
                "reserved": {
                  "meta": ["$"],
                  "control": ["@"]
                }
              }
            }
            """);

        var normalizer = new AtlasSourceNormalizer();
        var result = normalizer.Normalize(
            rootAtlas,
            "external.root-atlas.v1",
            rootIndexStream: rootIndex,
            suffixIndexStream: suffixIndex,
            symbolicIndexStream: symbolicIndex,
            reservedExpandedStream: reservedExpanded);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "atlas.reserved_root_symbol");
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "atlas.unknown_prefix_operator");
    }

    private static MemoryStream Utf8Stream(string json) => new(Encoding.UTF8.GetBytes(json));
}
