namespace San.Common;

public enum GroupoidFibrinoidPackageClassKind
{
    StateSurface = 0,
    RunBundle = 1,
    DailyBundle = 2,
    ReportReadout = 3
}

public enum GroupoidFibrinoidLatticeStateKind
{
    CandidateCollected = 0,
    CandidateBundled = 1,
    CandidateCompacted = 2,
    CandidateReadout = 3
}

public sealed record GroupoidFibrinoidCollectionPacket(
    string FibrinoidHandle,
    string SourceSurfaceHandle,
    string DomainValue,
    string SplineValue,
    string SemanticClassValue,
    string AuthorityClassValue,
    string ContinuityClassValue,
    string RetentionClassValue,
    GroupoidFibrinoidPackageClassKind RequestedPackageClass,
    bool DistinctionPreserved,
    bool AdmissionRequested,
    bool InheritanceRequested,
    IReadOnlyList<string> SymbolicCarrierHandles,
    IReadOnlyList<string> PointerHandles,
    IReadOnlyList<string> LatticeNotes,
    DateTimeOffset TimestampUtc);

public sealed record GroupoidFibrinoidBundleMappingReceipt(
    string ReceiptHandle,
    string FibrinoidHandle,
    string SourceSurfaceHandle,
    string DomainValue,
    string SplineValue,
    string SemanticClassValue,
    string AuthorityClassValue,
    string ContinuityClassValue,
    string RetentionClassValue,
    GroupoidFibrinoidPackageClassKind RequestedPackageClass,
    GroupoidFibrinoidPackageClassKind MappedPackageClass,
    GroupoidFibrinoidLatticeStateKind LatticeState,
    bool CandidateOnly,
    bool DistinctionPreserved,
    bool SemanticClassPreserved,
    bool PackagingDoesNotRedefineMeaning,
    bool AdmissionPromoted,
    bool InheritancePromoted,
    bool ReadyForLaterLatticeCondensation,
    IReadOnlyList<string> SymbolicCarrierHandles,
    IReadOnlyList<string> PointerHandles,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class GroupoidFibrinoidCollectionEvaluator
{
    public static GroupoidFibrinoidBundleMappingReceipt Evaluate(
        GroupoidFibrinoidCollectionPacket packet,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var mappedPackageClass = DeterminePackageClass(packet);
        var latticeState = DetermineLatticeState(mappedPackageClass);
        var normalizedCarriers = NormalizeTokens(packet.SymbolicCarrierHandles);
        var normalizedPointers = NormalizeTokens(packet.PointerHandles);
        var readyForLaterCondensation = packet.DistinctionPreserved &&
            HasCompleteTuple(packet) &&
            normalizedPointers.Count > 0;

        return new GroupoidFibrinoidBundleMappingReceipt(
            ReceiptHandle: receiptHandle,
            FibrinoidHandle: packet.FibrinoidHandle,
            SourceSurfaceHandle: packet.SourceSurfaceHandle,
            DomainValue: NormalizeToken(packet.DomainValue),
            SplineValue: NormalizeToken(packet.SplineValue),
            SemanticClassValue: NormalizeToken(packet.SemanticClassValue),
            AuthorityClassValue: NormalizeToken(packet.AuthorityClassValue),
            ContinuityClassValue: NormalizeToken(packet.ContinuityClassValue),
            RetentionClassValue: NormalizeToken(packet.RetentionClassValue),
            RequestedPackageClass: packet.RequestedPackageClass,
            MappedPackageClass: mappedPackageClass,
            LatticeState: latticeState,
            CandidateOnly: true,
            DistinctionPreserved: packet.DistinctionPreserved,
            SemanticClassPreserved: packet.DistinctionPreserved,
            PackagingDoesNotRedefineMeaning: true,
            AdmissionPromoted: false,
            InheritancePromoted: false,
            ReadyForLaterLatticeCondensation: readyForLaterCondensation,
            SymbolicCarrierHandles: normalizedCarriers,
            PointerHandles: normalizedPointers,
            ConstraintCodes: DetermineConstraintCodes(packet, mappedPackageClass, readyForLaterCondensation),
            ReasonCode: DetermineReasonCode(packet, mappedPackageClass),
            LawfulBasis: DetermineLawfulBasis(mappedPackageClass),
            TimestampUtc: packet.TimestampUtc);
    }

    public static GroupoidFibrinoidPackageClassKind DeterminePackageClass(
        GroupoidFibrinoidCollectionPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (Matches(packet.SemanticClassValue, "standing_surface") &&
            packet.RequestedPackageClass == GroupoidFibrinoidPackageClassKind.StateSurface)
        {
            return GroupoidFibrinoidPackageClassKind.StateSurface;
        }

        if (Matches(packet.RetentionClassValue, "daily_compacted"))
        {
            return GroupoidFibrinoidPackageClassKind.DailyBundle;
        }

        if (packet.RequestedPackageClass == GroupoidFibrinoidPackageClassKind.ReportReadout)
        {
            return GroupoidFibrinoidPackageClassKind.ReportReadout;
        }

        return GroupoidFibrinoidPackageClassKind.RunBundle;
    }

    public static GroupoidFibrinoidLatticeStateKind DetermineLatticeState(
        GroupoidFibrinoidPackageClassKind packageClass)
    {
        return packageClass switch
        {
            GroupoidFibrinoidPackageClassKind.StateSurface => GroupoidFibrinoidLatticeStateKind.CandidateCollected,
            GroupoidFibrinoidPackageClassKind.DailyBundle => GroupoidFibrinoidLatticeStateKind.CandidateCompacted,
            GroupoidFibrinoidPackageClassKind.ReportReadout => GroupoidFibrinoidLatticeStateKind.CandidateReadout,
            _ => GroupoidFibrinoidLatticeStateKind.CandidateBundled
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        GroupoidFibrinoidCollectionPacket packet,
        GroupoidFibrinoidPackageClassKind mappedPackageClass,
        bool readyForLaterCondensation)
    {
        var constraints = new List<string>
        {
            "groupoid-fibrinoid-candidate-only",
            "groupoid-fibrinoid-package-class-does-not-redefine-semantic-class"
        };

        constraints.Add(mappedPackageClass switch
        {
            GroupoidFibrinoidPackageClassKind.StateSurface => "groupoid-fibrinoid-mapped-to-state-surface",
            GroupoidFibrinoidPackageClassKind.DailyBundle => "groupoid-fibrinoid-mapped-to-daily-bundle",
            GroupoidFibrinoidPackageClassKind.ReportReadout => "groupoid-fibrinoid-mapped-to-report-readout",
            _ => "groupoid-fibrinoid-mapped-to-run-bundle"
        });

        if (packet.AdmissionRequested)
        {
            constraints.Add("groupoid-fibrinoid-admission-not-promoted");
        }

        if (packet.InheritanceRequested)
        {
            constraints.Add("groupoid-fibrinoid-inheritance-not-promoted");
        }

        if (!packet.DistinctionPreserved)
        {
            constraints.Add("groupoid-fibrinoid-distinction-not-preserved");
        }

        if (!readyForLaterCondensation)
        {
            constraints.Add("groupoid-fibrinoid-not-ready-for-lattice-condensation");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        GroupoidFibrinoidCollectionPacket packet,
        GroupoidFibrinoidPackageClassKind mappedPackageClass)
    {
        if (packet.AdmissionRequested || packet.InheritanceRequested)
        {
            return "groupoid-fibrinoid-candidate-only-refused-promotion";
        }

        if (!packet.DistinctionPreserved)
        {
            return "groupoid-fibrinoid-distinction-not-preserved";
        }

        return mappedPackageClass switch
        {
            GroupoidFibrinoidPackageClassKind.StateSurface => "groupoid-fibrinoid-collected-to-state-surface",
            GroupoidFibrinoidPackageClassKind.DailyBundle => "groupoid-fibrinoid-mapped-to-daily-bundle",
            GroupoidFibrinoidPackageClassKind.ReportReadout => "groupoid-fibrinoid-mapped-to-report-readout",
            _ => "groupoid-fibrinoid-mapped-to-run-bundle"
        };
    }

    private static string DetermineLawfulBasis(
        GroupoidFibrinoidPackageClassKind mappedPackageClass)
    {
        return mappedPackageClass switch
        {
            GroupoidFibrinoidPackageClassKind.StateSurface =>
                "standing-surface fibrinoid remained in state-surface packaging without semantic promotion",
            GroupoidFibrinoidPackageClassKind.DailyBundle =>
                "daily-compacted fibrinoid routed into daily-bundle packaging without semantic promotion",
            GroupoidFibrinoidPackageClassKind.ReportReadout =>
                "report-facing fibrinoid routed into readout packaging without semantic promotion",
            _ =>
                "default fibrinoid transport remained candidate-only and mapped into run-bundle packaging"
        };
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        if (tokens is null || tokens.Count == 0)
        {
            return [];
        }

        return tokens
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(NormalizeToken)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool HasCompleteTuple(
        GroupoidFibrinoidCollectionPacket packet)
    {
        return
            !string.IsNullOrWhiteSpace(packet.DomainValue) &&
            !string.IsNullOrWhiteSpace(packet.SplineValue) &&
            !string.IsNullOrWhiteSpace(packet.SemanticClassValue) &&
            !string.IsNullOrWhiteSpace(packet.AuthorityClassValue) &&
            !string.IsNullOrWhiteSpace(packet.ContinuityClassValue) &&
            !string.IsNullOrWhiteSpace(packet.RetentionClassValue);
    }

    private static bool Matches(string value, string expected)
    {
        return string.Equals(
            NormalizeToken(value),
            expected,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeToken(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }
}
