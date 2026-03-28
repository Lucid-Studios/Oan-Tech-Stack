using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Oan.Common;

namespace Oan.Storage
{
    /// <summary>
    /// File-backed implementation of public plane stores.
    /// </summary>
    public sealed class PublicPlaneStore : IPublicPlaneStores
    {
        private readonly string _basePath;
        private readonly ITelemetrySink _telemetry;
        private readonly IManagedEgressRouter _egressRouter;

        public PublicPlaneStore(string basePath, ITelemetrySink telemetry, IManagedEgressRouter? egressRouter = null)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _egressRouter = egressRouter ?? NullEgressRouter.Instance;
        }

        public async Task AppendToGoAAsync(string engramHash, object payload)
        {
            await AppendInternalAsync("GoA", engramHash, payload);
        }

        public async Task AppendToGELAsync(string engramHash, object payload)
        {
            await AppendInternalAsync("GEL", engramHash, payload);
        }

        private async Task AppendInternalAsync(string storeName, string engramHash, object payload)
        {
            string filePath = Path.Combine(_basePath, $"{storeName}.ndjson");
            string json = JsonSerializer.Serialize(payload);

            var envelope = new ManagedEgressEnvelope(
                EffectKind: SliEgressEffectKind.JournalAppend,
                RetentionPosture: SliEgressRetentionPosture.ImmutableLedger,
                JurisdictionClass: SliEgressJurisdictionClass.Cradle,
                IdentityFormingAllowed: true,
                TargetSinkClass: SliEgressTargetSinkClass.FileSystemLocal,
                AuthorityReason: $"Appending to governed {storeName} plane store"
            );

            var authorized = await _egressRouter.TryRouteEgressAsync(envelope, async () =>
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.AppendAllTextAsync(filePath, json + "\n").ConfigureAwait(false);
            }).ConfigureAwait(false);

            await _telemetry.EmitAsync(new
            {
                store_name = storeName,
                action = "append",
                pointer = engramHash,
                result = authorized ? "OK" : "Denied"
            });

            if (!authorized)
            {
                throw new InvalidOperationException($"Managed egress router denied append to {storeName} plane store.");
            }
        }
    }

    /// <summary>
    /// File-backed implementation of cryptic plane stores.
    /// </summary>
    public sealed class CrypticPlaneStore : ICrypticPlaneStores
    {
        private readonly string _basePath;
        private readonly ITelemetrySink _telemetry;
        private readonly IManagedEgressRouter _egressRouter;

        public CrypticPlaneStore(string basePath, ITelemetrySink telemetry, IManagedEgressRouter? egressRouter = null)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _egressRouter = egressRouter ?? NullEgressRouter.Instance;
        }

        public async Task AppendToCGoAAsync(string engramHash, object payload)
        {
            await AppendInternalAsync("cGoA", engramHash, payload);
        }

        private async Task AppendInternalAsync(string storeName, string engramHash, object payload)
        {
            string filePath = Path.Combine(_basePath, $"{storeName}.ndjson");
            string json = JsonSerializer.Serialize(payload);

            var envelope = new ManagedEgressEnvelope(
                EffectKind: SliEgressEffectKind.JournalAppend,
                RetentionPosture: SliEgressRetentionPosture.ImmutableLedger,
                JurisdictionClass: SliEgressJurisdictionClass.Cradle,
                IdentityFormingAllowed: true,
                TargetSinkClass: SliEgressTargetSinkClass.FileSystemLocal,
                AuthorityReason: $"Appending to governed {storeName} cryptic plane store"
            );

            var authorized = await _egressRouter.TryRouteEgressAsync(envelope, async () =>
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.AppendAllTextAsync(filePath, json + "\n").ConfigureAwait(false);
            }).ConfigureAwait(false);

            await _telemetry.EmitAsync(new
            {
                store_name = storeName,
                action = "append",
                pointer = engramHash,
                result = authorized ? "OK" : "Denied"
            });

            if (!authorized)
            {
                throw new InvalidOperationException($"Managed egress router denied append to {storeName} cryptic plane store.");
            }
        }
    }
}
