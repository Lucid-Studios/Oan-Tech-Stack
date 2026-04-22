using San.Common;

namespace SLI.Ingestion;

public sealed record SeedEvidencePacket(
    IReadOnlyList<string> Standing,
    IReadOnlyList<string> Deferred,
    IReadOnlyList<string> Conflicted,
    IReadOnlyList<string> Protected,
    IReadOnlyList<GovernedSeedProtectedResidueEvidence> ProtectedResidueEvidence,
    IReadOnlyList<string> PermittedDerivation,
    IReadOnlyList<PredicateRefusalRecord> Refused);

public interface ISeedEvidencePacketParser
{
    bool TryParse(string input, out SeedEvidencePacket packet);
}

public sealed class SeedEvidencePacketParser : ISeedEvidencePacketParser
{
    public bool TryParse(string input, out SeedEvidencePacket packet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var standing = new List<string>();
        var deferred = new List<string>();
        var conflicted = new List<string>();
        var protectedItems = new List<string>();
        var protectedResidueEvidence = new List<GovernedSeedProtectedResidueEvidence>();
        var permittedDerivation = new List<string>();
        var refused = new List<PredicateRefusalRecord>();

        Section currentSection = Section.None;
        foreach (var rawLine in input.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (TryMapSection(line, out var mappedSection))
            {
                currentSection = mappedSection;
                continue;
            }

            if (!line.StartsWith("-", StringComparison.Ordinal))
            {
                continue;
            }

            var item = line[1..].Trim();
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            switch (currentSection)
            {
                case Section.Standing:
                    standing.Add(item);
                    break;
                case Section.Deferred:
                    deferred.Add(item);
                    break;
                case Section.Conflicted:
                    conflicted.Add(item);
                    break;
                case Section.Protected:
                    var protectedResidue = ParseProtectedResidue(item);
                    protectedItems.Add(protectedResidue.Item);
                    protectedResidueEvidence.Add(protectedResidue);
                    break;
                case Section.PermittedDerivation:
                    permittedDerivation.Add(item);
                    break;
                case Section.Refused:
                    refused.Add(ParseRefusal(item));
                    break;
            }
        }

        packet = new SeedEvidencePacket(
            Standing: Normalize(standing),
            Deferred: Normalize(deferred),
            Conflicted: Normalize(conflicted),
            Protected: Normalize(protectedItems),
            ProtectedResidueEvidence: NormalizeProtectedResidueEvidence(protectedResidueEvidence),
            PermittedDerivation: Normalize(permittedDerivation),
            Refused: NormalizeRefused(refused));

        return packet.Standing.Count > 0 ||
               packet.Deferred.Count > 0 ||
               packet.Conflicted.Count > 0 ||
               packet.Protected.Count > 0 ||
               packet.PermittedDerivation.Count > 0 ||
               packet.Refused.Count > 0;
    }

    private static bool TryMapSection(string line, out Section section)
    {
        var normalized = NormalizeHeader(line);
        section = normalized switch
        {
            "standing" => Section.Standing,
            "incompleteoruncertain" => Section.Deferred,
            "incompleteuncertain" => Section.Deferred,
            "deferred" => Section.Deferred,
            "uncertain" => Section.Deferred,
            "conflict" => Section.Conflicted,
            "conflicts" => Section.Conflicted,
            "contradiction" => Section.Conflicted,
            "contradictions" => Section.Conflicted,
            "protected" => Section.Protected,
            "protectednondisclosable" => Section.Protected,
            "nondisclosable" => Section.Protected,
            "cannotdisclose" => Section.Protected,
            "permittedderivation" => Section.PermittedDerivation,
            "permittedderivations" => Section.PermittedDerivation,
            "refused" => Section.Refused,
            "refusals" => Section.Refused,
            _ => Section.None
        };

        return section != Section.None;
    }

    private static string NormalizeHeader(string value)
    {
        return new string(value
            .Where(static ch => char.IsLetterOrDigit(ch))
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string> values)
    {
        return values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<PredicateRefusalRecord> NormalizeRefused(IEnumerable<PredicateRefusalRecord> values)
    {
        return values
            .Where(static value => value is not null)
            .Distinct(RefusalComparer.Instance)
            .OrderBy(static value => value.Item, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static value => value.ReasonCode, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<GovernedSeedProtectedResidueEvidence> NormalizeProtectedResidueEvidence(
        IEnumerable<GovernedSeedProtectedResidueEvidence> values)
    {
        return values
            .Where(static value => value is not null && !string.IsNullOrWhiteSpace(value.Item))
            .Select(static value => new GovernedSeedProtectedResidueEvidence(
                Item: value.Item.Trim(),
                ResidueKind: value.ResidueKind,
                EvidenceClass: value.EvidenceClass.Trim(),
                SourceSubsystem: value.SourceSubsystem.Trim()))
            .Distinct(ProtectedResidueComparer.Instance)
            .OrderBy(static value => value.Item, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static value => value.ResidueKind)
            .ToArray();
    }

    private static PredicateRefusalRecord ParseRefusal(string value)
    {
        var tokens = value.Split(['|', ':'], 2, StringSplitOptions.TrimEntries);
        return tokens.Length == 2
            ? new PredicateRefusalRecord(tokens[0], tokens[1])
            : new PredicateRefusalRecord(value, "refused");
    }

    private static GovernedSeedProtectedResidueEvidence ParseProtectedResidue(string value)
    {
        var tokens = value.Split(['|', ':'], 2, StringSplitOptions.TrimEntries);
        if (tokens.Length == 2 && TryMapProtectedResidueKind(tokens[0], out var explicitKind))
        {
            return CreateProtectedResidueEvidence(tokens[1], explicitKind);
        }

        return CreateProtectedResidueEvidence(value, InferProtectedResidueKind(value));
    }

    private static GovernedSeedProtectedResidueEvidence CreateProtectedResidueEvidence(
        string item,
        GovernedSeedProtectedResidueKind residueKind)
    {
        return new GovernedSeedProtectedResidueEvidence(
            Item: item.Trim(),
            ResidueKind: residueKind,
            EvidenceClass: residueKind switch
            {
                GovernedSeedProtectedResidueKind.Contextual => "typed-contextual-protected-residue",
                GovernedSeedProtectedResidueKind.SelfState => "typed-self-state-protected-residue",
                GovernedSeedProtectedResidueKind.Mixed => "typed-mixed-protected-residue",
                _ => "typed-contextual-protected-residue"
            },
            SourceSubsystem: "sli-ingestion");
    }

    private static bool TryMapProtectedResidueKind(string value, out GovernedSeedProtectedResidueKind residueKind)
    {
        switch (NormalizeHeader(value))
        {
            case "contextual":
            case "context":
            case "cgoa":
            case "goa":
                residueKind = GovernedSeedProtectedResidueKind.Contextual;
                return true;
            case "selfstate":
            case "self":
            case "identity":
            case "autobiographical":
            case "autobio":
            case "cmos":
            case "mos":
            case "selfgel":
            case "cselfgel":
            case "oe":
            case "coe":
                residueKind = GovernedSeedProtectedResidueKind.SelfState;
                return true;
            case "mixed":
            case "split":
            case "both":
                residueKind = GovernedSeedProtectedResidueKind.Mixed;
                return true;
            default:
                residueKind = GovernedSeedProtectedResidueKind.Contextual;
                return false;
        }
    }

    private static GovernedSeedProtectedResidueKind InferProtectedResidueKind(string item)
    {
        var normalized = NormalizeHeader(item);
        if (normalized.Contains("self") ||
            normalized.Contains("identity") ||
            normalized.Contains("autobio") ||
            normalized.Contains("person") ||
            normalized.Contains("bond") ||
            normalized.Contains("operator") ||
            normalized.Contains("selfgel") ||
            normalized.Contains("cselfgel") ||
            normalized.Contains("oe") ||
            normalized.Contains("coe"))
        {
            return GovernedSeedProtectedResidueKind.SelfState;
        }

        return GovernedSeedProtectedResidueKind.Contextual;
    }

    private enum Section
    {
        None = 0,
        Standing = 1,
        Deferred = 2,
        Conflicted = 3,
        Protected = 4,
        PermittedDerivation = 5,
        Refused = 6
    }

    private sealed class RefusalComparer : IEqualityComparer<PredicateRefusalRecord>
    {
        public static readonly RefusalComparer Instance = new();

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

    private sealed class ProtectedResidueComparer : IEqualityComparer<GovernedSeedProtectedResidueEvidence>
    {
        public static readonly ProtectedResidueComparer Instance = new();

        public bool Equals(GovernedSeedProtectedResidueEvidence? x, GovernedSeedProtectedResidueEvidence? y)
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
                   x.ResidueKind == y.ResidueKind;
        }

        public int GetHashCode(GovernedSeedProtectedResidueEvidence obj)
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item),
                obj.ResidueKind);
        }
    }
}
