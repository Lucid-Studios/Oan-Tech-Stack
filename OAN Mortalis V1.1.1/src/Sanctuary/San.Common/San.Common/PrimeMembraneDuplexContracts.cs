namespace San.Common;

public enum PrimeMembraneSourceKind
{
    Direct = 0,
    Witnessed = 1,
    IssuedTemplate = 2
}

public enum CrypticProjectionPostureKind
{
    Hovering = 0,
    Rehearsing = 1,
    Braided = 2,
    Latent = 3,
    Ripening = 4,
    Unresolved = 5
}

public enum CmeBoundedStateKind
{
    None = 0,
    Candidate = 1,
    Bounded = 2,
    Anchored = 3
}

public enum PrimeClosureEligibilityKind
{
    Withheld = 0,
    CandidateOnly = 1,
    ReviewRequired = 2,
    EligibleForMembraneReceipt = 3
}

public sealed record PrimeMembraneSourcePacket(
    string PacketHandle,
    string MembraneHandle,
    PrimeMembraneSourceKind SourceKind,
    string SourceSurfaceHandle,
    string LawHandle,
    bool OriginAttested,
    IReadOnlyList<string> WitnessHandles,
    IReadOnlyList<string> SourceNotes,
    DateTimeOffset TimestampUtc);

public sealed record CrypticProjectionPacket(
    string PacketHandle,
    string ProjectionHandle,
    string MembraneHandle,
    string? ListeningFrameHandle,
    string? CompassEmbodimentHandle,
    string? EngineeredCognitionHandle,
    CrypticProjectionPostureKind ProjectionPosture,
    IReadOnlyList<string> ParticipatingSurfaceHandles,
    IReadOnlyList<string> ProjectionNotes,
    DateTimeOffset TimestampUtc);

public sealed record CmeBoundedStatePacket(
    string PacketHandle,
    string? CmeHandle,
    string? EngineeredCognitionHandle,
    string ProjectionHandle,
    CmeBoundedStateKind BoundedState,
    bool StandingFormed,
    IReadOnlyList<string> StateNotes,
    DateTimeOffset TimestampUtc);

public sealed record DuplexFieldPacket(
    string PacketHandle,
    PrimeMembraneSourcePacket MembraneSource,
    CrypticProjectionPacket ProjectionState,
    CmeBoundedStatePacket BoundedState,
    IReadOnlyList<string> FieldNotes,
    DateTimeOffset TimestampUtc);

public sealed record PrimeClosureEligibilityReceipt(
    string ReceiptHandle,
    string DuplexPacketHandle,
    string MembraneHandle,
    string ProjectionHandle,
    string? CmeHandle,
    PrimeClosureEligibilityKind Eligibility,
    bool ReturnToPrimeLawful,
    bool ExplicitMembraneReceiptStillRequired,
    bool PrimeClosureStillWithheld,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrimeMembraneClosureEvaluator
{
    public static PrimeClosureEligibilityReceipt Evaluate(
        DuplexFieldPacket packet,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(packet.MembraneSource);
        ArgumentNullException.ThrowIfNull(packet.ProjectionState);
        ArgumentNullException.ThrowIfNull(packet.BoundedState);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var eligibility = DetermineEligibility(packet);
        var constraintCodes = DetermineConstraintCodes(packet, eligibility);
        var reasonCode = DetermineReasonCode(packet, eligibility);
        var lawfulBasis = DetermineLawfulBasis(eligibility);

        return new PrimeClosureEligibilityReceipt(
            ReceiptHandle: receiptHandle,
            DuplexPacketHandle: packet.PacketHandle,
            MembraneHandle: packet.MembraneSource.MembraneHandle,
            ProjectionHandle: packet.ProjectionState.ProjectionHandle,
            CmeHandle: packet.BoundedState.CmeHandle,
            Eligibility: eligibility,
            ReturnToPrimeLawful: eligibility == PrimeClosureEligibilityKind.EligibleForMembraneReceipt,
            ExplicitMembraneReceiptStillRequired: true,
            PrimeClosureStillWithheld: true,
            ConstraintCodes: constraintCodes,
            ReasonCode: reasonCode,
            LawfulBasis: lawfulBasis,
            TimestampUtc: packet.TimestampUtc);
    }

    public static PrimeClosureEligibilityKind DetermineEligibility(DuplexFieldPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (!packet.MembraneSource.OriginAttested)
        {
            return PrimeClosureEligibilityKind.Withheld;
        }

        if (packet.ProjectionState.ProjectionPosture == CrypticProjectionPostureKind.Unresolved)
        {
            return PrimeClosureEligibilityKind.Withheld;
        }

        if (!packet.BoundedState.StandingFormed || packet.BoundedState.BoundedState == CmeBoundedStateKind.None)
        {
            return PrimeClosureEligibilityKind.CandidateOnly;
        }

        return packet.ProjectionState.ProjectionPosture switch
        {
            CrypticProjectionPostureKind.Hovering => PrimeClosureEligibilityKind.CandidateOnly,
            CrypticProjectionPostureKind.Rehearsing => PrimeClosureEligibilityKind.CandidateOnly,
            CrypticProjectionPostureKind.Latent => PrimeClosureEligibilityKind.CandidateOnly,
            CrypticProjectionPostureKind.Braided => PrimeClosureEligibilityKind.ReviewRequired,
            CrypticProjectionPostureKind.Ripening when packet.BoundedState.BoundedState == CmeBoundedStateKind.Anchored =>
                PrimeClosureEligibilityKind.EligibleForMembraneReceipt,
            CrypticProjectionPostureKind.Ripening => PrimeClosureEligibilityKind.ReviewRequired,
            _ => PrimeClosureEligibilityKind.Withheld
        };
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        DuplexFieldPacket packet,
        PrimeClosureEligibilityKind eligibility)
    {
        var constraints = new List<string>();

        if (!packet.MembraneSource.OriginAttested)
        {
            constraints.Add("membrane-origin-not-attested");
        }

        switch (packet.ProjectionState.ProjectionPosture)
        {
            case CrypticProjectionPostureKind.Hovering:
                constraints.Add("projection-hovering");
                break;
            case CrypticProjectionPostureKind.Rehearsing:
                constraints.Add("projection-rehearsing");
                break;
            case CrypticProjectionPostureKind.Latent:
                constraints.Add("projection-latent");
                break;
            case CrypticProjectionPostureKind.Unresolved:
                constraints.Add("projection-unresolved");
                break;
            case CrypticProjectionPostureKind.Braided:
                constraints.Add("projection-braided-review");
                break;
            case CrypticProjectionPostureKind.Ripening:
                constraints.Add("projection-ripening");
                break;
        }

        if (!packet.BoundedState.StandingFormed || packet.BoundedState.BoundedState == CmeBoundedStateKind.None)
        {
            constraints.Add("bounded-state-not-formed");
        }

        if (eligibility == PrimeClosureEligibilityKind.EligibleForMembraneReceipt)
        {
            constraints.Add("prime-closure-still-withheld-until-explicit-membrane-receipt");
        }

        return constraints;
    }

    private static string DetermineReasonCode(
        DuplexFieldPacket packet,
        PrimeClosureEligibilityKind eligibility)
    {
        if (!packet.MembraneSource.OriginAttested)
        {
            return "prime-membrane-origin-not-attested";
        }

        if (packet.ProjectionState.ProjectionPosture == CrypticProjectionPostureKind.Unresolved)
        {
            return "cryptic-projection-unresolved";
        }

        if (!packet.BoundedState.StandingFormed || packet.BoundedState.BoundedState == CmeBoundedStateKind.None)
        {
            return "cme-bounded-state-not-formed";
        }

        return eligibility switch
        {
            PrimeClosureEligibilityKind.CandidateOnly => "cryptic-projection-remains-non-binding",
            PrimeClosureEligibilityKind.ReviewRequired => "prime-membrane-review-required-before-closure",
            PrimeClosureEligibilityKind.EligibleForMembraneReceipt => "eligible-for-prime-membrane-receipt",
            _ => "prime-closure-withheld"
        };
    }

    private static string DetermineLawfulBasis(PrimeClosureEligibilityKind eligibility)
    {
        return eligibility switch
        {
            PrimeClosureEligibilityKind.Withheld =>
                "membrane origin, projection posture, or bounded standing are insufficient for return-to-Prime review.",
            PrimeClosureEligibilityKind.CandidateOnly =>
                "projected field remains non-binding and may stand only as candidate-bearing duplex participation.",
            PrimeClosureEligibilityKind.ReviewRequired =>
                "bounded CME standing exists, but the packet must remain review-bound before membrane-side receipt.",
            PrimeClosureEligibilityKind.EligibleForMembraneReceipt =>
                "bounded standing is anchored and may return for explicit membrane-side receipt without being treated as closed Prime form.",
            _ => "prime membrane handoff remains bounded."
        };
    }
}
