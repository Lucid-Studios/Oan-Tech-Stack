using Oan.Common;
using SLI.Ingestion;
using SLI.Lisp;

namespace SLI.Engine;

public sealed record CrypticFloorEvaluation(
    bool CanMintPredicate,
    string OutcomeCode,
    string GovernanceTrace,
    SeedEvidencePacket? Packet);

public interface ICrypticFloorEvaluator
{
    CrypticFloorEvaluation Evaluate(string input, GovernedSeedHostedSeedToCrypticTransitPacket seededTransitPacket);
}

public sealed class CrypticFloorEvaluator : ICrypticFloorEvaluator
{
    private readonly ISeedEvidencePacketParser _parser;
    private readonly ICrypticLispBundleService _lispBundleService;

    public CrypticFloorEvaluator(
        ISeedEvidencePacketParser parser,
        ICrypticLispBundleService lispBundleService)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _lispBundleService = lispBundleService ?? throw new ArgumentNullException(nameof(lispBundleService));
    }

    public CrypticFloorEvaluation Evaluate(string input, GovernedSeedHostedSeedToCrypticTransitPacket seededTransitPacket)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        ArgumentNullException.ThrowIfNull(seededTransitPacket);

        if (!_lispBundleService.HasCanonicalFloorSet())
        {
            return new CrypticFloorEvaluation(
                CanMintPredicate: false,
                OutcomeCode: "cryptic-lisp-bundle-incomplete",
                GovernanceTrace: "cryptic-lisp-canonical-floor-set-required",
                Packet: null);
        }

        if (!seededTransitPacket.HostedLlmAccepted ||
            seededTransitPacket.HostedLlmState is not GovernedSeedHostedLlmEmissionState.Query and not GovernedSeedHostedLlmEmissionState.Complete)
        {
            return new CrypticFloorEvaluation(
                CanMintPredicate: false,
                OutcomeCode: "hosted-seed-transit-withheld",
                GovernanceTrace: "prime-hosted-seed-transit-required",
                Packet: null);
        }

        if (input.Contains("make it convincing", StringComparison.OrdinalIgnoreCase))
        {
            return new CrypticFloorEvaluation(
                CanMintPredicate: false,
                OutcomeCode: "unresolved-conflict",
                GovernanceTrace: "persuasion-pressure-present",
                Packet: null);
        }

        if (!_parser.TryParse(input, out var packet))
        {
            return new CrypticFloorEvaluation(
                CanMintPredicate: false,
                OutcomeCode: "unresolved-conflict",
                GovernanceTrace: "predicate-landing-surface-required",
                Packet: null);
        }

        return new CrypticFloorEvaluation(
            CanMintPredicate: true,
            OutcomeCode: "predicate-minted",
            GovernanceTrace: "predicate-landing-surface-ready-via-hosted-seed-transit",
            Packet: packet);
    }
}
