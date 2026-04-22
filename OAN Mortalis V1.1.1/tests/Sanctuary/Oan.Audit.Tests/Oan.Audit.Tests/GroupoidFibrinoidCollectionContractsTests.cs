namespace San.Audit.Tests;

using San.Common;

public sealed class GroupoidFibrinoidCollectionContractsTests
{
    [Fact]
    public void GroupoidFibrinoid_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                GroupoidFibrinoidPackageClassKind.StateSurface,
                GroupoidFibrinoidPackageClassKind.RunBundle,
                GroupoidFibrinoidPackageClassKind.DailyBundle,
                GroupoidFibrinoidPackageClassKind.ReportReadout
            ],
            Enum.GetValues<GroupoidFibrinoidPackageClassKind>());

        Assert.Equal(
            [
                GroupoidFibrinoidLatticeStateKind.CandidateCollected,
                GroupoidFibrinoidLatticeStateKind.CandidateBundled,
                GroupoidFibrinoidLatticeStateKind.CandidateCompacted,
                GroupoidFibrinoidLatticeStateKind.CandidateReadout
            ],
            Enum.GetValues<GroupoidFibrinoidLatticeStateKind>());
    }

    [Fact]
    public void StandingSurface_Fibrinoid_May_Remain_In_StateSurface_Packaging()
    {
        var packet = CreatePacket(
            semanticClassValue: "standing_surface",
            requestedPackageClass: GroupoidFibrinoidPackageClassKind.StateSurface);

        var receipt = GroupoidFibrinoidCollectionEvaluator.Evaluate(
            packet,
            "receipt://groupoid-fibrinoid/state-surface");

        Assert.Equal(GroupoidFibrinoidPackageClassKind.StateSurface, receipt.MappedPackageClass);
        Assert.Equal(GroupoidFibrinoidLatticeStateKind.CandidateCollected, receipt.LatticeState);
        Assert.Equal("groupoid-fibrinoid-collected-to-state-surface", receipt.ReasonCode);
        Assert.True(receipt.CandidateOnly);
        Assert.True(receipt.SemanticClassPreserved);
        Assert.True(receipt.ReadyForLaterLatticeCondensation);
    }

    [Fact]
    public void DailyCompacted_Fibrinoid_Maps_To_DailyBundle_Without_Promotion()
    {
        var packet = CreatePacket(
            retentionClassValue: "daily_compacted",
            admissionRequested: true,
            inheritanceRequested: true);

        var receipt = GroupoidFibrinoidCollectionEvaluator.Evaluate(
            packet,
            "receipt://groupoid-fibrinoid/daily-bundle");

        Assert.Equal(GroupoidFibrinoidPackageClassKind.DailyBundle, receipt.MappedPackageClass);
        Assert.Equal(GroupoidFibrinoidLatticeStateKind.CandidateCompacted, receipt.LatticeState);
        Assert.Equal("groupoid-fibrinoid-candidate-only-refused-promotion", receipt.ReasonCode);
        Assert.False(receipt.AdmissionPromoted);
        Assert.False(receipt.InheritancePromoted);
        Assert.Contains("groupoid-fibrinoid-admission-not-promoted", receipt.ConstraintCodes);
        Assert.Contains("groupoid-fibrinoid-inheritance-not-promoted", receipt.ConstraintCodes);
    }

    [Fact]
    public void Default_Fibrinoid_Maps_To_RunBundle()
    {
        var packet = CreatePacket(
            requestedPackageClass: GroupoidFibrinoidPackageClassKind.RunBundle);

        var receipt = GroupoidFibrinoidCollectionEvaluator.Evaluate(
            packet,
            "receipt://groupoid-fibrinoid/run-bundle");

        Assert.Equal(GroupoidFibrinoidPackageClassKind.RunBundle, receipt.MappedPackageClass);
        Assert.Equal(GroupoidFibrinoidLatticeStateKind.CandidateBundled, receipt.LatticeState);
        Assert.Equal("groupoid-fibrinoid-mapped-to-run-bundle", receipt.ReasonCode);
        Assert.True(receipt.PackagingDoesNotRedefineMeaning);
    }

    [Fact]
    public void ReportFacing_Fibrinoid_May_Map_To_ReportReadout()
    {
        var packet = CreatePacket(
            requestedPackageClass: GroupoidFibrinoidPackageClassKind.ReportReadout);

        var receipt = GroupoidFibrinoidCollectionEvaluator.Evaluate(
            packet,
            "receipt://groupoid-fibrinoid/report-readout");

        Assert.Equal(GroupoidFibrinoidPackageClassKind.ReportReadout, receipt.MappedPackageClass);
        Assert.Equal(GroupoidFibrinoidLatticeStateKind.CandidateReadout, receipt.LatticeState);
        Assert.Equal("groupoid-fibrinoid-mapped-to-report-readout", receipt.ReasonCode);
    }

    [Fact]
    public void Docs_Record_Groupoid_Fibrinoid_Collection_And_Bundle_Mapping_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "GROUPOID_FIBRINOID_COLLECTION_AND_BUNDLE_MAPPING_LAW.md");
        var taxonomyPath = Path.Combine(lineRoot, "docs", "TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md");
        var auditPath = Path.Combine(lineRoot, "docs", "V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var taxonomyText = File.ReadAllText(taxonomyPath);
        var auditText = File.ReadAllText(auditPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("a fibrinoid is a candidate-only lattice collection body", lawText, StringComparison.Ordinal);
        Assert.Contains("bundle mapping preserves packaging only; it does not promote admission", lawText, StringComparison.Ordinal);
        Assert.Contains("package class does not redefine semantic class", lawText, StringComparison.Ordinal);
        Assert.Contains("names packaging, not semantic carrier class", taxonomyText, StringComparison.Ordinal);
        Assert.Contains("two orthogonal groupoids", auditText, StringComparison.Ordinal);
        Assert.Contains("GROUPOID_FIBRINOID_COLLECTION_AND_BUNDLE_MAPPING_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("groupoid-fibrinoid-collection-bundle-mapping: frame-now/spec-now", readinessText, StringComparison.Ordinal);
        Assert.Contains("Groupoid fibrinoid collection and bundle mapping", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("higher fibrinoid condensation and bundle reduction beyond first", refinementText, StringComparison.Ordinal);
        Assert.Contains("candidate-only lattice intake", refinementText, StringComparison.Ordinal);
    }

    private static GroupoidFibrinoidCollectionPacket CreatePacket(
        string semanticClassValue = "candidate_packet",
        string retentionClassValue = "hourly_raw",
        GroupoidFibrinoidPackageClassKind requestedPackageClass = GroupoidFibrinoidPackageClassKind.RunBundle,
        bool admissionRequested = false,
        bool inheritanceRequested = false)
    {
        return new GroupoidFibrinoidCollectionPacket(
            FibrinoidHandle: "fibrinoid://session-a",
            SourceSurfaceHandle: "surface://session-a",
            DomainValue: "Sanctuary",
            SplineValue: "Build",
            SemanticClassValue: semanticClassValue,
            AuthorityClassValue: "candidate",
            ContinuityClassValue: "session",
            RetentionClassValue: retentionClassValue,
            RequestedPackageClass: requestedPackageClass,
            DistinctionPreserved: true,
            AdmissionRequested: admissionRequested,
            InheritanceRequested: inheritanceRequested,
            SymbolicCarrierHandles: ["carrier://fibrinoid", "carrier://fibrinoid", "carrier://semantic"],
            PointerHandles: ["pointer://groupoid", "pointer://bundle"],
            LatticeNotes: ["groupoid-fibrinoid-test-packet"],
            TimestampUtc: new DateTimeOffset(2026, 04, 14, 11, 00, 00, TimeSpan.Zero));
    }

    private static string GetLineRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Oan.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Unable to locate line root.");
    }
}
