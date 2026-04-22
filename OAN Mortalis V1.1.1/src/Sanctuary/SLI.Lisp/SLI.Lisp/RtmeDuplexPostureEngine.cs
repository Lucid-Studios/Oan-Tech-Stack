using System.Security.Cryptography;
using System.Text;
using San.Common;

namespace SLI.Lisp;

public enum RtmeProjectionTransitionKind
{
    Hold = 0,
    Deepen = 1,
    Braid = 2,
    Stabilize = 3
}

public sealed record RtmeProjectionContribution(
    string ContributionHandle,
    string SourceSurfaceHandle,
    string ContributionKind,
    IReadOnlyList<string> Notes);

public sealed record RtmeDuplexProjectionReceipt(
    string ReceiptHandle,
    string SourcePacketHandle,
    string EmittedPacketHandle,
    string ProjectionHandle,
    string? CmeHandle,
    CrypticProjectionPostureKind PreviousPosture,
    CrypticProjectionPostureKind CurrentPosture,
    RtmeProjectionTransitionKind TransitionKind,
    IReadOnlyList<string> AcceptedContributionHandles,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PrimeClosureIssued,
    string OutcomeCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public sealed record RtmeDuplexProjectionResult(
    DuplexFieldPacket EmittedPacket,
    RtmeDuplexProjectionReceipt Receipt);

public interface IRtmeDuplexPostureEngine
{
    RtmeDuplexProjectionResult Advance(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectionContribution> contributions,
        string receiptHandle);
}

public sealed class RtmeDuplexPostureEngine : IRtmeDuplexPostureEngine
{
    private const string RequiredProjectionModule = "duplex-posture.lisp";
    private readonly ICrypticLispBundleService _bundleService;

    public RtmeDuplexPostureEngine()
        : this(new GovernedCrypticLispBundleService())
    {
    }

    public RtmeDuplexPostureEngine(ICrypticLispBundleService bundleService)
    {
        _bundleService = bundleService ?? throw new ArgumentNullException(nameof(bundleService));
    }

    public RtmeDuplexProjectionResult Advance(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectionContribution> contributions,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        var normalizedContributions = NormalizeContributions(contributions);
        var timestampUtc = DateTimeOffset.UtcNow;

        if (!_bundleService.LoadModules().ContainsKey(RequiredProjectionModule))
        {
            return CreateResult(
                packet,
                packet,
                receiptHandle,
                packet.ProjectionState.ProjectionPosture,
                packet.ProjectionState.ProjectionPosture,
                RtmeProjectionTransitionKind.Hold,
                [],
                PrimeMembraneClosureEvaluator.DetermineEligibility(packet),
                "rtme-duplex-posture-module-required",
                "rtme projected posture movement requires the duplex posture Lisp module before live projected transitions may proceed.",
                timestampUtc);
        }

        var nextPosture = DetermineNextPosture(packet, normalizedContributions);
        var transitionKind = DetermineTransitionKind(packet.ProjectionState.ProjectionPosture, nextPosture);
        var updatedProjectionPacket = packet.ProjectionState with
        {
            PacketHandle = CreateDerivedHandle(
                "packet://projection/rtme/",
                receiptHandle,
                packet.ProjectionState.PacketHandle,
                nextPosture.ToString()),
            ProjectionPosture = nextPosture,
            ParticipatingSurfaceHandles = packet.ProjectionState.ParticipatingSurfaceHandles
                .Concat(normalizedContributions.Select(static item => item.SourceSurfaceHandle))
                .Where(static handle => !string.IsNullOrWhiteSpace(handle))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static handle => handle, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            ProjectionNotes = packet.ProjectionState.ProjectionNotes
                .Concat(normalizedContributions.SelectMany(static item => item.Notes))
                .Concat(
                [
                    $"rtme-transition:{transitionKind}",
                    $"rtme-posture:{nextPosture}"
                ])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static note => note, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            TimestampUtc = timestampUtc
        };

        var emittedPacket = packet with
        {
            PacketHandle = CreateDerivedHandle(
                "packet://duplex-field/rtme/",
                receiptHandle,
                packet.PacketHandle,
                nextPosture.ToString()),
            ProjectionState = updatedProjectionPacket,
            FieldNotes = packet.FieldNotes
                .Concat(
                [
                    "rtme-projected-field-snapshot",
                    "prime-closure-still-membrane-bound"
                ])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static note => note, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            TimestampUtc = timestampUtc
        };

        var advisoryClosureEligibility = PrimeMembraneClosureEvaluator.DetermineEligibility(emittedPacket);

        return CreateResult(
            packet,
            emittedPacket,
            receiptHandle,
            packet.ProjectionState.ProjectionPosture,
            nextPosture,
            transitionKind,
            normalizedContributions.Select(static item => item.ContributionHandle).ToArray(),
            advisoryClosureEligibility,
            DetermineOutcomeCode(transitionKind, advisoryClosureEligibility),
            "rtme may deepen, braid, or stabilize projected posture but may not issue Prime closure, erase origin, or mutate bounded standing.",
            timestampUtc);
    }

    private static RtmeDuplexProjectionResult CreateResult(
        DuplexFieldPacket sourcePacket,
        DuplexFieldPacket emittedPacket,
        string receiptHandle,
        CrypticProjectionPostureKind previousPosture,
        CrypticProjectionPostureKind currentPosture,
        RtmeProjectionTransitionKind transitionKind,
        IReadOnlyList<string> acceptedContributionHandles,
        PrimeClosureEligibilityKind advisoryClosureEligibility,
        string outcomeCode,
        string lawfulBasis,
        DateTimeOffset timestampUtc)
    {
        return new RtmeDuplexProjectionResult(
            EmittedPacket: emittedPacket,
            Receipt: new RtmeDuplexProjectionReceipt(
                ReceiptHandle: receiptHandle,
                SourcePacketHandle: sourcePacket.PacketHandle,
                EmittedPacketHandle: emittedPacket.PacketHandle,
                ProjectionHandle: emittedPacket.ProjectionState.ProjectionHandle,
                CmeHandle: emittedPacket.BoundedState.CmeHandle,
                PreviousPosture: previousPosture,
                CurrentPosture: currentPosture,
                TransitionKind: transitionKind,
                AcceptedContributionHandles: acceptedContributionHandles,
                AdvisoryClosureEligibility: advisoryClosureEligibility,
                PrimeClosureIssued: false,
                OutcomeCode: outcomeCode,
                LawfulBasis: lawfulBasis,
                TimestampUtc: timestampUtc));
    }

    private static CrypticProjectionPostureKind DetermineNextPosture(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectionContribution> contributions)
    {
        var contributionCount = contributions.Count;
        var distinctSurfaceCount = contributions
            .Select(static item => item.SourceSurfaceHandle)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var hasStanding = packet.BoundedState.StandingFormed &&
                          packet.BoundedState.BoundedState != CmeBoundedStateKind.None;

        return packet.ProjectionState.ProjectionPosture switch
        {
            CrypticProjectionPostureKind.Unresolved when packet.MembraneSource.OriginAttested && contributionCount > 0 =>
                CrypticProjectionPostureKind.Rehearsing,
            CrypticProjectionPostureKind.Hovering when contributionCount > 0 =>
                CrypticProjectionPostureKind.Rehearsing,
            CrypticProjectionPostureKind.Latent when contributionCount > 0 =>
                CrypticProjectionPostureKind.Hovering,
            CrypticProjectionPostureKind.Rehearsing when distinctSurfaceCount >= 2 =>
                CrypticProjectionPostureKind.Braided,
            CrypticProjectionPostureKind.Braided when hasStanding && contributionCount > 0 =>
                CrypticProjectionPostureKind.Ripening,
            CrypticProjectionPostureKind.Ripening =>
                CrypticProjectionPostureKind.Ripening,
            _ => packet.ProjectionState.ProjectionPosture
        };
    }

    private static RtmeProjectionTransitionKind DetermineTransitionKind(
        CrypticProjectionPostureKind previousPosture,
        CrypticProjectionPostureKind currentPosture)
    {
        if (previousPosture == currentPosture)
        {
            return currentPosture == CrypticProjectionPostureKind.Ripening
                ? RtmeProjectionTransitionKind.Stabilize
                : RtmeProjectionTransitionKind.Hold;
        }

        if (currentPosture == CrypticProjectionPostureKind.Braided)
        {
            return RtmeProjectionTransitionKind.Braid;
        }

        if (currentPosture == CrypticProjectionPostureKind.Ripening)
        {
            return RtmeProjectionTransitionKind.Stabilize;
        }

        return RtmeProjectionTransitionKind.Deepen;
    }

    private static string DetermineOutcomeCode(
        RtmeProjectionTransitionKind transitionKind,
        PrimeClosureEligibilityKind advisoryClosureEligibility)
    {
        if (advisoryClosureEligibility == PrimeClosureEligibilityKind.EligibleForMembraneReceipt)
        {
            return "rtme-projected-field-ripening-advisory-eligible";
        }

        return transitionKind switch
        {
            RtmeProjectionTransitionKind.Braid => "rtme-projected-field-braided",
            RtmeProjectionTransitionKind.Stabilize => "rtme-projected-field-stabilized",
            RtmeProjectionTransitionKind.Deepen => "rtme-projected-field-deepened",
            _ => "rtme-projected-field-held"
        };
    }

    private static IReadOnlyList<RtmeProjectionContribution> NormalizeContributions(
        IReadOnlyList<RtmeProjectionContribution>? contributions)
    {
        if (contributions is null || contributions.Count == 0)
        {
            return [];
        }

        return contributions
            .Where(static item =>
                item is not null &&
                !string.IsNullOrWhiteSpace(item.ContributionHandle) &&
                !string.IsNullOrWhiteSpace(item.SourceSurfaceHandle) &&
                !string.IsNullOrWhiteSpace(item.ContributionKind))
            .GroupBy(static item => item.ContributionHandle, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static item => item.ContributionHandle, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string CreateDerivedHandle(
        string prefix,
        string receiptHandle,
        string packetHandle,
        string postureToken)
    {
        var material = $"{receiptHandle}|{packetHandle}|{postureToken}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
