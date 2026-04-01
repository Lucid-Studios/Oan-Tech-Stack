using System.Security.Cryptography;
using System.Text;
using Oan.Common;

namespace Oan.FirstRun;

public interface IGovernedSeedPreGovernanceService
{
    GovernedSeedPreGovernancePacket Project(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSanctuaryIngressReceipt? sanctuaryIngressReceipt,
        GovernedSeedLowMindSfRoutePacket? lowMindSfRoute,
        string theaterId);
}

public sealed class GovernedSeedPreGovernanceService : IGovernedSeedPreGovernanceService
{
    public GovernedSeedPreGovernancePacket Project(
        GovernedSeedSoulFrameBootstrapReceipt bootstrapReceipt,
        GovernedSeedSanctuaryIngressReceipt? sanctuaryIngressReceipt,
        GovernedSeedLowMindSfRoutePacket? lowMindSfRoute,
        string theaterId)
    {
        ArgumentNullException.ThrowIfNull(bootstrapReceipt);
        ArgumentException.ThrowIfNullOrWhiteSpace(theaterId);

        var timestampUtc = DateTimeOffset.UtcNow;

        GovernedSeedLocalAuthorityTraceReceipt? localAuthorityTrace = null;
        if (sanctuaryIngressReceipt is not null)
        {
            localAuthorityTrace = new GovernedSeedLocalAuthorityTraceReceipt(
                ReceiptHandle: CreateHandle(
                    "local-authority-trace://",
                    sanctuaryIngressReceipt.ReceiptHandle,
                    bootstrapReceipt.BootstrapHandle,
                    theaterId),
                SanctuaryIngressReceiptHandle: sanctuaryIngressReceipt.ReceiptHandle,
                AuthorityProfile: "sanctuary-ingress-local-authority-trace",
                AuthoritySurface: "notice-custody-authority-responsibility",
                ResponsibilityTraceHandle: CreateHandle(
                    "responsibility-trace://",
                    sanctuaryIngressReceipt.ReceiptHandle,
                    bootstrapReceipt.BootstrapHandle,
                    theaterId),
                ObsidianWallApplied: sanctuaryIngressReceipt.ObsidianWallApplied,
                SourceReason: "local-authority-trace-projected-from-sanctuary-ingress",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedConstitutionalContactReceipt? constitutionalContact = null;
        if (sanctuaryIngressReceipt is not null && localAuthorityTrace is not null)
        {
            constitutionalContact = new GovernedSeedConstitutionalContactReceipt(
                ReceiptHandle: CreateHandle(
                    "constitutional-contact://",
                    localAuthorityTrace.ReceiptHandle,
                    bootstrapReceipt.BootstrapHandle,
                    theaterId),
                SanctuaryIngressReceiptHandle: sanctuaryIngressReceipt.ReceiptHandle,
                LocalAuthorityTraceReceiptHandle: localAuthorityTrace.ReceiptHandle,
                ContactProfile: "sanctuary-ingress-constitutional-contact",
                ContactSurface: "notice-custody-authority-trace",
                ObsidianWallApplied: sanctuaryIngressReceipt.ObsidianWallApplied,
                SourceReason: "constitutional-contact-projected-from-local-authority-trace",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedLocalKeypairGenesisSourceReceipt? localKeypairGenesisSource = null;
        if (constitutionalContact is not null)
        {
            localKeypairGenesisSource = new GovernedSeedLocalKeypairGenesisSourceReceipt(
                ReceiptHandle: CreateHandle(
                    "local-keypair-genesis-source://",
                    constitutionalContact.ReceiptHandle,
                    bootstrapReceipt.IdentitySeat.IdentityHandle,
                    bootstrapReceipt.IdentitySeat.IntegrityHash,
                    theaterId),
                ConstitutionalContactReceiptHandle: constitutionalContact.ReceiptHandle,
                IdentityHandle: bootstrapReceipt.IdentitySeat.IdentityHandle,
                OperatorBondHandle: bootstrapReceipt.IdentitySeat.OperatorBondHandle,
                IntegrityHash: bootstrapReceipt.IdentitySeat.IntegrityHash,
                SourceProfile: "soulframe-identity-seat-key-genesis-source",
                SourceReason: "local-keypair-genesis-source-projected-from-soulframe-identity-seat",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedLocalKeypairGenesisReceipt? localKeypairGenesis = null;
        if (constitutionalContact is not null && localKeypairGenesisSource is not null)
        {
            localKeypairGenesis = new GovernedSeedLocalKeypairGenesisReceipt(
                ReceiptHandle: CreateHandle(
                    "local-keypair-genesis://",
                    localKeypairGenesisSource.ReceiptHandle,
                    theaterId),
                ConstitutionalContactReceiptHandle: constitutionalContact.ReceiptHandle,
                LocalKeypairGenesisSourceReceiptHandle: localKeypairGenesisSource.ReceiptHandle,
                IdentityHandle: localKeypairGenesisSource.IdentityHandle,
                OperatorBondHandle: localKeypairGenesisSource.OperatorBondHandle,
                IntegrityHash: localKeypairGenesisSource.IntegrityHash,
                KeyProfile: "identity-seat-integrity-keypair-genesis",
                SourceReason: "local-keypair-genesis-projected-from-key-genesis-source",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedFirstCrypticBraidEstablishmentReceipt? firstCrypticBraidEstablishment = null;
        if (constitutionalContact is not null &&
            localKeypairGenesis is not null &&
            bootstrapReceipt.MantleReceipt.ProtectedPresentedBraided)
        {
            firstCrypticBraidEstablishment = new GovernedSeedFirstCrypticBraidEstablishmentReceipt(
                ReceiptHandle: CreateHandle(
                    "first-cryptic-braid-establishment://",
                    constitutionalContact.ReceiptHandle,
                    localKeypairGenesis.ReceiptHandle,
                    bootstrapReceipt.MantleReceipt.MantleHandle,
                    theaterId),
                ConstitutionalContactReceiptHandle: constitutionalContact.ReceiptHandle,
                LocalKeypairGenesisReceiptHandle: localKeypairGenesis.ReceiptHandle,
                MantleHandle: bootstrapReceipt.MantleReceipt.MantleHandle,
                BraidProfile: bootstrapReceipt.MantleReceipt.BraidProfile,
                ProtectedPresentedBraided: true,
                SourceReason: "first-cryptic-braid-establishment-projected-from-mantle-braid-seat",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedFirstCrypticBraidReceipt? firstCrypticBraid = null;
        if (firstCrypticBraidEstablishment is not null)
        {
            firstCrypticBraid = new GovernedSeedFirstCrypticBraidReceipt(
                ReceiptHandle: CreateHandle(
                    "first-cryptic-braid://",
                    firstCrypticBraidEstablishment.ReceiptHandle,
                    theaterId),
                ConstitutionalContactReceiptHandle: firstCrypticBraidEstablishment.ConstitutionalContactReceiptHandle,
                LocalKeypairGenesisReceiptHandle: firstCrypticBraidEstablishment.LocalKeypairGenesisReceiptHandle,
                FirstCrypticBraidEstablishmentReceiptHandle: firstCrypticBraidEstablishment.ReceiptHandle,
                MantleHandle: firstCrypticBraidEstablishment.MantleHandle,
                BraidProfile: firstCrypticBraidEstablishment.BraidProfile,
                ProtectedPresentedBraided: firstCrypticBraidEstablishment.ProtectedPresentedBraided,
                SourceReason: "first-cryptic-braid-projected-from-braid-establishment",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedFirstCrypticConditioningSourceReceipt? firstCrypticConditioningSource = null;
        if (firstCrypticBraid is not null && lowMindSfRoute is not null)
        {
            firstCrypticConditioningSource = new GovernedSeedFirstCrypticConditioningSourceReceipt(
                ReceiptHandle: CreateHandle(
                    "first-cryptic-conditioning-source://",
                    firstCrypticBraid.ReceiptHandle,
                    lowMindSfRoute.PacketHandle,
                    theaterId),
                FirstCrypticBraidReceiptHandle: firstCrypticBraid.ReceiptHandle,
                LowMindSfRouteHandle: lowMindSfRoute.PacketHandle,
                IngressAccessClass: lowMindSfRoute.IngressAccessClass,
                RouteKind: lowMindSfRoute.RouteKind,
                SourceProfile: "lowmind-sf-cryptic-conditioning-source",
                SourceReason: "first-cryptic-conditioning-source-projected-from-lowmind-sf-route",
                TimestampUtc: timestampUtc);
        }

        GovernedSeedFirstCrypticConditioningReceipt? firstCrypticConditioning = null;
        if (firstCrypticConditioningSource is not null)
        {
            firstCrypticConditioning = new GovernedSeedFirstCrypticConditioningReceipt(
                ReceiptHandle: CreateHandle(
                    "first-cryptic-conditioning://",
                    firstCrypticConditioningSource.ReceiptHandle,
                    theaterId),
                FirstCrypticBraidReceiptHandle: firstCrypticConditioningSource.FirstCrypticBraidReceiptHandle,
                FirstCrypticConditioningSourceReceiptHandle: firstCrypticConditioningSource.ReceiptHandle,
                LowMindSfRouteHandle: firstCrypticConditioningSource.LowMindSfRouteHandle,
                IngressAccessClass: firstCrypticConditioningSource.IngressAccessClass,
                RouteKind: firstCrypticConditioningSource.RouteKind,
                ConditioningProfile: "lowmind-sf-conditioned-cryptic-entry",
                SourceReason: "first-cryptic-conditioning-projected-from-conditioning-source",
                TimestampUtc: timestampUtc);
        }

        return new GovernedSeedPreGovernancePacket(
            PacketHandle: CreateHandle(
                "pre-governance-packet://",
                bootstrapReceipt.BootstrapHandle,
                localAuthorityTrace?.ReceiptHandle ?? "no-local-authority-trace",
                constitutionalContact?.ReceiptHandle ?? "no-contact",
                localKeypairGenesisSource?.ReceiptHandle ?? "no-keypair-source",
                localKeypairGenesis?.ReceiptHandle ?? "no-keypair",
                firstCrypticBraidEstablishment?.ReceiptHandle ?? "no-braid-establishment",
                firstCrypticBraid?.ReceiptHandle ?? "no-braid",
                firstCrypticConditioningSource?.ReceiptHandle ?? "no-conditioning-source",
                firstCrypticConditioning?.ReceiptHandle ?? "no-conditioning"),
            LocalAuthorityTrace: localAuthorityTrace,
            ConstitutionalContact: constitutionalContact,
            LocalKeypairGenesisSource: localKeypairGenesisSource,
            LocalKeypairGenesis: localKeypairGenesis,
            FirstCrypticBraidEstablishment: firstCrypticBraidEstablishment,
            FirstCrypticBraid: firstCrypticBraid,
            FirstCrypticConditioningSource: firstCrypticConditioningSource,
            FirstCrypticConditioning: firstCrypticConditioning,
            SourceReason: firstCrypticConditioning is not null
                ? "pre-governance-sequence-projected-through-first-cryptic-conditioning"
                : "pre-governance-sequence-partially-projected-before-first-cryptic-conditioning",
            TimestampUtc: timestampUtc);
    }

    private static string CreateHandle(string prefix, params string[] parts)
    {
        var material = string.Join("|", parts.Select(static part => part.Trim()));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return $"{prefix}{Convert.ToHexString(hash).ToLowerInvariant()[..16]}";
    }
}
