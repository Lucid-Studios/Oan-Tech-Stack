using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Oan.Common;
using Oan.Spinal;

namespace Oan.Storage
{
    /// <summary>
    /// Compatibility adapter that lets Prime publication flow through the
    /// derivative contract while legacy plane storage remains underneath.
    /// </summary>
    public sealed class PrimeDerivativePublisherAdapter : IPrimeDerivativePublisher, IPrimeDerivativeView
    {
        private readonly IPublicPlaneStores _publicStores;
        private readonly List<PrimeDerivativeViewRecord> _views = [];

        public PrimeDerivativePublisherAdapter(IPublicPlaneStores publicStores)
        {
            _publicStores = publicStores ?? throw new ArgumentNullException(nameof(publicStores));
        }

        public async Task PublishPointerAsync(
            PrimeDerivativePointerPublication publication,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(publication);

            var payload = new
            {
                classification = publication.Classification,
                pointer = publication.Pointer
            };

            await _publicStores.AppendToGELAsync(publication.Pointer, payload).ConfigureAwait(false);
            await _publicStores.AppendToGoAAsync(publication.Pointer, payload).ConfigureAwait(false);

            _views.Add(new PrimeDerivativeViewRecord(
                publication.IdentityId,
                RepresentationKind: "pointer",
                Value: publication.Pointer,
                publication.Classification,
                Timestamp: DateTime.UtcNow));
        }

        public async Task PublishRedactedAsync(
            PrimeDerivativeRedactedPublication publication,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(publication);

            var pointer = ComputePointer(publication.IdentityId, publication.Classification, publication.RedactedPayload);
            var payload = new
            {
                classification = publication.Classification,
                redacted_payload = publication.RedactedPayload
            };

            await _publicStores.AppendToGELAsync(pointer, payload).ConfigureAwait(false);
            await _publicStores.AppendToGoAAsync(pointer, payload).ConfigureAwait(false);

            _views.Add(new PrimeDerivativeViewRecord(
                publication.IdentityId,
                RepresentationKind: "redacted",
                Value: publication.RedactedPayload,
                publication.Classification,
                Timestamp: DateTime.UtcNow));
        }

        public async Task PublishEncryptedAsync(
            PrimeDerivativeEncryptedPublication publication,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(publication);

            var payload = new
            {
                classification = "encrypted-pointer",
                encrypted_payload = publication.EncryptedPayload,
                pointer = publication.Pointer
            };

            await _publicStores.AppendToGELAsync(publication.Pointer, payload).ConfigureAwait(false);
            await _publicStores.AppendToGoAAsync(publication.Pointer, payload).ConfigureAwait(false);

            _views.Add(new PrimeDerivativeViewRecord(
                publication.IdentityId,
                RepresentationKind: "encrypted",
                Value: publication.Pointer,
                Classification: "encrypted-pointer",
                Timestamp: DateTime.UtcNow));
        }

        public Task<IReadOnlyList<PrimeDerivativeViewRecord>> ListDerivativeViewsAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<PrimeDerivativeViewRecord>>(_views.ToArray());
        }

        private static string ComputePointer(Guid identityId, string classification, string payload)
        {
            var material = JsonSerializer.Serialize(new
            {
                identity_id = identityId,
                classification,
                payload
            });

            return $"prime://{Primitives.ComputeHash(material)}";
        }
    }
}
