namespace San.Audit.Tests;

using System.Text.Json;
using San.Common;

public sealed class PrimeMembraneDuplexContractsTests
{
    [Fact]
    public void DuplexPacket_Enums_Are_Explicit_And_Ordered()
    {
        Assert.Equal(
            [
                PrimeMembraneSourceKind.Direct,
                PrimeMembraneSourceKind.Witnessed,
                PrimeMembraneSourceKind.IssuedTemplate
            ],
            Enum.GetValues<PrimeMembraneSourceKind>());

        Assert.Equal(
            [
                CrypticProjectionPostureKind.Hovering,
                CrypticProjectionPostureKind.Rehearsing,
                CrypticProjectionPostureKind.Braided,
                CrypticProjectionPostureKind.Latent,
                CrypticProjectionPostureKind.Ripening,
                CrypticProjectionPostureKind.Unresolved
            ],
            Enum.GetValues<CrypticProjectionPostureKind>());

        Assert.Equal(
            [
                CmeBoundedStateKind.None,
                CmeBoundedStateKind.Candidate,
                CmeBoundedStateKind.Bounded,
                CmeBoundedStateKind.Anchored
            ],
            Enum.GetValues<CmeBoundedStateKind>());

        Assert.Equal(
            [
                PrimeClosureEligibilityKind.Withheld,
                PrimeClosureEligibilityKind.CandidateOnly,
                PrimeClosureEligibilityKind.ReviewRequired,
                PrimeClosureEligibilityKind.EligibleForMembraneReceipt
            ],
            Enum.GetValues<PrimeClosureEligibilityKind>());
    }

    [Fact]
    public void DuplexFieldPacket_RoundTrips_With_Source_Projection_And_BoundedStanding()
    {
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Braided,
            boundedState: CmeBoundedStateKind.Bounded,
            standingFormed: true);

        var json = JsonSerializer.Serialize(packet);
        var roundTrip = JsonSerializer.Deserialize<DuplexFieldPacket>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(PrimeMembraneSourceKind.Witnessed, roundTrip!.MembraneSource.SourceKind);
        Assert.Equal(CrypticProjectionPostureKind.Braided, roundTrip.ProjectionState.ProjectionPosture);
        Assert.Equal(CmeBoundedStateKind.Bounded, roundTrip.BoundedState.BoundedState);
        Assert.True(roundTrip.BoundedState.StandingFormed);
        Assert.Equal("membrane://prime/session-a", roundTrip.MembraneSource.MembraneHandle);
        Assert.Equal("projection://cryptic/session-a", roundTrip.ProjectionState.ProjectionHandle);
    }

    [Fact]
    public void Ripening_Anchored_Packet_Is_Eligible_For_MembraneReceipt_But_Not_Closed()
    {
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Ripening,
            boundedState: CmeBoundedStateKind.Anchored,
            standingFormed: true);

        var receipt = PrimeMembraneClosureEvaluator.Evaluate(
            packet,
            "receipt://prime-membrane/eligible");

        Assert.Equal(PrimeClosureEligibilityKind.EligibleForMembraneReceipt, receipt.Eligibility);
        Assert.True(receipt.ReturnToPrimeLawful);
        Assert.True(receipt.ExplicitMembraneReceiptStillRequired);
        Assert.True(receipt.PrimeClosureStillWithheld);
        Assert.Equal("eligible-for-prime-membrane-receipt", receipt.ReasonCode);
        Assert.Contains(
            "prime-closure-still-withheld-until-explicit-membrane-receipt",
            receipt.ConstraintCodes);
    }

    [Fact]
    public void Hovering_Packet_Remains_CandidateOnly()
    {
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Hovering,
            boundedState: CmeBoundedStateKind.Candidate,
            standingFormed: true);

        var receipt = PrimeMembraneClosureEvaluator.Evaluate(
            packet,
            "receipt://prime-membrane/candidate");

        Assert.Equal(PrimeClosureEligibilityKind.CandidateOnly, receipt.Eligibility);
        Assert.False(receipt.ReturnToPrimeLawful);
        Assert.True(receipt.ExplicitMembraneReceiptStillRequired);
        Assert.True(receipt.PrimeClosureStillWithheld);
        Assert.Equal("cryptic-projection-remains-non-binding", receipt.ReasonCode);
        Assert.Contains("projection-hovering", receipt.ConstraintCodes);
    }

    [Fact]
    public void Unattested_Origin_Withholds_ClosureEligibility()
    {
        var packet = CreatePacket(
            posture: CrypticProjectionPostureKind.Ripening,
            boundedState: CmeBoundedStateKind.Anchored,
            standingFormed: true,
            originAttested: false);

        var receipt = PrimeMembraneClosureEvaluator.Evaluate(
            packet,
            "receipt://prime-membrane/withheld");

        Assert.Equal(PrimeClosureEligibilityKind.Withheld, receipt.Eligibility);
        Assert.False(receipt.ReturnToPrimeLawful);
        Assert.Equal("prime-membrane-origin-not-attested", receipt.ReasonCode);
        Assert.Contains("membrane-origin-not-attested", receipt.ConstraintCodes);
    }

    [Fact]
    public void Docs_Record_Prime_Membrane_Duplex_Packet_Law()
    {
        var lineRoot = GetLineRoot();
        var lawPath = Path.Combine(lineRoot, "docs", "PRIME_MEMBRANE_DUPLEX_PACKET_LAW.md");
        var duplexLawPath = Path.Combine(lineRoot, "docs", "PRIME_CRYPTIC_DUPLEX_LAW.md");
        var mosPath = Path.Combine(lineRoot, "docs", "MOS_CMOS_CGOA_INSTANTIATION_LAW.md");
        var readinessPath = Path.Combine(lineRoot, "docs", "BUILD_READINESS.md");
        var carryForwardPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_LEDGER.md");
        var refinementPath = Path.Combine(lineRoot, "docs", "V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md");

        Assert.True(File.Exists(lawPath));

        var lawText = File.ReadAllText(lawPath);
        var duplexLawText = File.ReadAllText(duplexLawPath);
        var mosText = File.ReadAllText(mosPath);
        var readinessText = File.ReadAllText(readinessPath);
        var carryForwardText = File.ReadAllText(carryForwardPath);
        var refinementText = File.ReadAllText(refinementPath);

        Assert.Contains("PrimeMembraneSourcePacket", lawText, StringComparison.Ordinal);
        Assert.Contains("CrypticProjectionPacket", lawText, StringComparison.Ordinal);
        Assert.Contains("CmeBoundedStatePacket", lawText, StringComparison.Ordinal);
        Assert.Contains("PrimeClosureEligibilityReceipt", lawText, StringComparison.Ordinal);
        Assert.Contains("no projected or bounded state shall be treated as Prime closure", lawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_DUPLEX_PACKET_LAW.md", duplexLawText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_DUPLEX_PACKET_LAW.md", mosText, StringComparison.Ordinal);
        Assert.Contains("PRIME_MEMBRANE_DUPLEX_PACKET_LAW.md", readinessText, StringComparison.Ordinal);
        Assert.Contains("Prime Membrane duplex packet law", carryForwardText, StringComparison.Ordinal);
        Assert.Contains("live Prime Membrane closure issuance", refinementText, StringComparison.Ordinal);
        Assert.Contains("prime-membrane-duplex-packet-law: frame-now/spec-now", readinessText, StringComparison.Ordinal);
    }

    private static DuplexFieldPacket CreatePacket(
        CrypticProjectionPostureKind posture,
        CmeBoundedStateKind boundedState,
        bool standingFormed,
        bool originAttested = true)
    {
        return new DuplexFieldPacket(
            PacketHandle: "packet://duplex-field/session-a",
            MembraneSource: new PrimeMembraneSourcePacket(
                PacketHandle: "packet://membrane-source/session-a",
                MembraneHandle: "membrane://prime/session-a",
                SourceKind: PrimeMembraneSourceKind.Witnessed,
                SourceSurfaceHandle: "prime://surface/session-a",
                LawHandle: "law://prime-membrane-duplex",
                OriginAttested: originAttested,
                WitnessHandles: ["witness://mother/session-a"],
                SourceNotes: ["prime-membrane-source-attested"],
                TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 10, 00, TimeSpan.Zero)),
            ProjectionState: new CrypticProjectionPacket(
                PacketHandle: "packet://projection/session-a",
                ProjectionHandle: "projection://cryptic/session-a",
                MembraneHandle: "membrane://prime/session-a",
                ListeningFrameHandle: "listening://frame/session-a",
                CompassEmbodimentHandle: "compass://embodiment/session-a",
                EngineeredCognitionHandle: "ec://session-a",
                ProjectionPosture: posture,
                ParticipatingSurfaceHandles:
                [
                    "sli://rtme/session-a",
                    "cryptic://field/session-a"
                ],
                ProjectionNotes: ["non-binding-projected-field"],
                TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 11, 00, TimeSpan.Zero)),
            BoundedState: new CmeBoundedStatePacket(
                PacketHandle: "packet://bounded-state/session-a",
                CmeHandle: "cme://bounded/session-a",
                EngineeredCognitionHandle: "ec://session-a",
                ProjectionHandle: "projection://cryptic/session-a",
                BoundedState: boundedState,
                StandingFormed: standingFormed,
                StateNotes: ["bounded-standing-track"],
                TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 12, 00, TimeSpan.Zero)),
            FieldNotes: ["prime-membrane-duplex-field"],
            TimestampUtc: new DateTimeOffset(2026, 04, 13, 23, 13, 00, TimeSpan.Zero));
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
