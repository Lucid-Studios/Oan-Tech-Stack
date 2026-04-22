using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SLI.Lisp;

public enum RtmeLineParticipationKind
{
    Clustered = 0,
    Swarmed = 1
}

public enum RtmeBraidStateKind
{
    Dispersed = 0,
    Clustered = 1,
    Swarmed = 2,
    CoherentBraid = 3,
    UnstableBraid = 4
}

public enum RtmeNonClosureStatusKind
{
    PrimeClosureWithheld = 0,
    AdvisoryReturnOnly = 1
}

public sealed record RtmeProjectedLineInput(
    string LineHandle,
    string SourceSurfaceHandle,
    CrypticProjectionPostureKind LinePosture,
    RtmeLineParticipationKind ParticipationKind,
    IReadOnlyList<RtmeProjectionContribution> Contributions,
    IReadOnlyList<string> Notes);

public sealed record RtmeProjectedLineResidue(
    string LineHandle,
    string SourceSurfaceHandle,
    CrypticProjectionPostureKind ResidualPosture,
    RtmeLineParticipationKind ParticipationKind,
    IReadOnlyList<string> AcceptedContributionHandles,
    bool DistinctionPreserved,
    IReadOnlyList<string> ResidueNotes);

public sealed record RtmeDuplexBraidSessionSnapshot(
    string SnapshotHandle,
    string SourcePacketHandle,
    string EmittedPacketHandle,
    string ProjectionHandle,
    RtmeBraidStateKind BraidState,
    RtmeNonClosureStatusKind NonClosureStatus,
    IReadOnlyList<RtmeProjectedLineResidue> LineResidues,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PrimeClosureIssued,
    string OutcomeCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public sealed record RtmeDuplexBraidResult(
    DuplexFieldPacket EmittedPacket,
    RtmeDuplexProjectionReceipt ProjectionReceipt,
    RtmeDuplexBraidSessionSnapshot BraidSnapshot);

public interface IRtmeDuplexBraidEngine
{
    RtmeDuplexBraidResult AdvanceBraid(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string receiptHandle);
}

public sealed class RtmeDuplexBraidEngine : IRtmeDuplexBraidEngine
{
    private const string RequiredBraidModule = "duplex-braid.lisp";
    private readonly IRtmeDuplexPostureEngine _postureEngine;
    private readonly ICrypticLispBundleService _bundleService;

    public RtmeDuplexBraidEngine()
        : this(new RtmeDuplexPostureEngine(), new GovernedCrypticLispBundleService())
    {
    }

    public RtmeDuplexBraidEngine(
        IRtmeDuplexPostureEngine postureEngine,
        ICrypticLispBundleService bundleService)
    {
        _postureEngine = postureEngine ?? throw new ArgumentNullException(nameof(postureEngine));
        _bundleService = bundleService ?? throw new ArgumentNullException(nameof(bundleService));
    }

    public RtmeDuplexBraidResult AdvanceBraid(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var normalizedLines = NormalizeLines(lines);
        var timestampUtc = DateTimeOffset.UtcNow;

        if (!_bundleService.LoadModules().ContainsKey(RequiredBraidModule))
        {
            var projectionResult = _postureEngine.Advance(packet, [], $"{receiptHandle}/projection");
            var lineResidues = CreateLineResidues(normalizedLines);
            var braidSnapshot = CreateSnapshot(
                packet,
                projectionResult.EmittedPacket,
                normalizedLines,
                lineResidues,
                RtmeBraidStateKind.Dispersed,
                RtmeNonClosureStatusKind.PrimeClosureWithheld,
                projectionResult.Receipt.AdvisoryClosureEligibility,
                "rtme-duplex-braid-module-required",
                "rtme clustered/swarmed braid discipline requires the duplex braid Lisp module before projected plurality may form a lawful braid snapshot.",
                timestampUtc);

            return new RtmeDuplexBraidResult(
                EmittedPacket: projectionResult.EmittedPacket,
                ProjectionReceipt: projectionResult.Receipt,
                BraidSnapshot: braidSnapshot);
        }

        var flattenedContributions = normalizedLines
            .SelectMany(static line => line.Contributions)
            .ToArray();
        var projectionResultLive = _postureEngine.Advance(packet, flattenedContributions, $"{receiptHandle}/projection");
        var lineResiduesLive = CreateLineResidues(normalizedLines);
        var braidState = DetermineBraidState(projectionResultLive.EmittedPacket, normalizedLines);
        var nonClosureStatus = projectionResultLive.Receipt.AdvisoryClosureEligibility == PrimeClosureEligibilityKind.EligibleForMembraneReceipt
            ? RtmeNonClosureStatusKind.AdvisoryReturnOnly
            : RtmeNonClosureStatusKind.PrimeClosureWithheld;
        var braidSnapshotLive = CreateSnapshot(
            packet,
            projectionResultLive.EmittedPacket,
            normalizedLines,
            lineResiduesLive,
            braidState,
            nonClosureStatus,
            projectionResultLive.Receipt.AdvisoryClosureEligibility,
            DetermineOutcomeCode(braidState, nonClosureStatus),
            "rtme may compose clustered and swarmed projected lines into a bounded braid snapshot, but it must preserve per-line distinction and may not self-authorize closure, retention, or membrane receipt.",
            timestampUtc);

        return new RtmeDuplexBraidResult(
            EmittedPacket: projectionResultLive.EmittedPacket,
            ProjectionReceipt: projectionResultLive.Receipt,
            BraidSnapshot: braidSnapshotLive);
    }

    private static IReadOnlyList<RtmeProjectedLineInput> NormalizeLines(
        IReadOnlyList<RtmeProjectedLineInput>? lines)
    {
        if (lines is null || lines.Count == 0)
        {
            return [];
        }

        return lines
            .Where(static line =>
                line is not null &&
                !string.IsNullOrWhiteSpace(line.LineHandle) &&
                !string.IsNullOrWhiteSpace(line.SourceSurfaceHandle))
            .GroupBy(static line => line.LineHandle, StringComparer.OrdinalIgnoreCase)
            .Select(static group =>
            {
                var first = group.First();
                var normalizedContributions = first.Contributions
                    .Where(static contribution =>
                        contribution is not null &&
                        !string.IsNullOrWhiteSpace(contribution.ContributionHandle) &&
                        !string.IsNullOrWhiteSpace(contribution.SourceSurfaceHandle))
                    .GroupBy(static contribution => contribution.ContributionHandle, StringComparer.OrdinalIgnoreCase)
                    .Select(static contributionGroup => contributionGroup.First())
                    .OrderBy(static contribution => contribution.ContributionHandle, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var normalizedNotes = first.Notes
                    .Where(static note => !string.IsNullOrWhiteSpace(note))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(static note => note, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return first with
                {
                    Contributions = normalizedContributions,
                    Notes = normalizedNotes
                };
            })
            .OrderBy(static line => line.LineHandle, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<RtmeProjectedLineResidue> CreateLineResidues(
        IReadOnlyList<RtmeProjectedLineInput> lines)
    {
        return lines
            .Select(static line => new RtmeProjectedLineResidue(
                LineHandle: line.LineHandle,
                SourceSurfaceHandle: line.SourceSurfaceHandle,
                ResidualPosture: line.LinePosture,
                ParticipationKind: line.ParticipationKind,
                AcceptedContributionHandles: line.Contributions
                    .Select(static contribution => contribution.ContributionHandle)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(static handle => handle, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                DistinctionPreserved: true,
                ResidueNotes: line.Notes
                    .Concat(["per-line-distinction-preserved"])
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(static note => note, StringComparer.OrdinalIgnoreCase)
                    .ToArray()))
            .ToArray();
    }

    private static RtmeBraidStateKind DetermineBraidState(
        DuplexFieldPacket emittedPacket,
        IReadOnlyList<RtmeProjectedLineInput> lines)
    {
        if (lines.Count < 2)
        {
            return RtmeBraidStateKind.Dispersed;
        }

        if (lines.Any(static line => line.LinePosture == CrypticProjectionPostureKind.Unresolved))
        {
            return RtmeBraidStateKind.UnstableBraid;
        }

        if (emittedPacket.ProjectionState.ProjectionPosture is CrypticProjectionPostureKind.Braided or CrypticProjectionPostureKind.Ripening)
        {
            return RtmeBraidStateKind.CoherentBraid;
        }

        return lines.All(static line => line.ParticipationKind == RtmeLineParticipationKind.Clustered)
            ? RtmeBraidStateKind.Clustered
            : RtmeBraidStateKind.Swarmed;
    }

    private static string DetermineOutcomeCode(
        RtmeBraidStateKind braidState,
        RtmeNonClosureStatusKind nonClosureStatus)
    {
        if (nonClosureStatus == RtmeNonClosureStatusKind.AdvisoryReturnOnly)
        {
            return "rtme-braid-advisory-return-only";
        }

        return braidState switch
        {
            RtmeBraidStateKind.Clustered => "rtme-braid-clustered",
            RtmeBraidStateKind.Swarmed => "rtme-braid-swarmed",
            RtmeBraidStateKind.CoherentBraid => "rtme-braid-coherent",
            RtmeBraidStateKind.UnstableBraid => "rtme-braid-unstable",
            _ => "rtme-braid-dispersed"
        };
    }

    private static RtmeDuplexBraidSessionSnapshot CreateSnapshot(
        DuplexFieldPacket sourcePacket,
        DuplexFieldPacket emittedPacket,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        IReadOnlyList<RtmeProjectedLineResidue> residues,
        RtmeBraidStateKind braidState,
        RtmeNonClosureStatusKind nonClosureStatus,
        PrimeClosureEligibilityKind advisoryClosureEligibility,
        string outcomeCode,
        string lawfulBasis,
        DateTimeOffset timestampUtc)
    {
        var snapshotHandle = CreateDerivedHandle(
            "snapshot://rtme-braid/",
            sourcePacket.PacketHandle,
            emittedPacket.PacketHandle,
            braidState.ToString(),
            lines.Count.ToString());

        return new RtmeDuplexBraidSessionSnapshot(
            SnapshotHandle: snapshotHandle,
            SourcePacketHandle: sourcePacket.PacketHandle,
            EmittedPacketHandle: emittedPacket.PacketHandle,
            ProjectionHandle: emittedPacket.ProjectionState.ProjectionHandle,
            BraidState: braidState,
            NonClosureStatus: nonClosureStatus,
            LineResidues: residues,
            AdvisoryClosureEligibility: advisoryClosureEligibility,
            PrimeClosureIssued: false,
            OutcomeCode: outcomeCode,
            LawfulBasis: lawfulBasis,
            TimestampUtc: timestampUtc);
    }

    private static string CreateDerivedHandle(
        string prefix,
        params string[] parts)
    {
        var material = string.Join("|", parts);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
