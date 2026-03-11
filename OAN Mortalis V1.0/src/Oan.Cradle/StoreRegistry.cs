using Oan.Common;

namespace Oan.Cradle
{
    /// <summary>
    /// Structured, immutable container for all system stores.
    /// </summary>
    public sealed class StoreRegistry
    {
        public ITelemetrySink GovernanceTelemetry { get; }
        public ITelemetrySink StorageTelemetry { get; }

        public IPublicPlaneStores Public { get; }
        public IPrimeDerivativePublisher PrimeDerivativePublisher { get; }
        public IPrimeDerivativeView PrimeDerivativeView { get; }
        public ICrypticPlaneStores Cryptic { get; }
        public ISoulFrameMembrane? SoulFrameMembrane { get; }
        public IGovernanceCycleCognitionService? GovernanceCognitionService { get; }
        public IReturnGovernanceAdjudicator? ReturnGovernanceAdjudicator { get; }
        public ICrypticCustodyStore? CrypticCustodyStore { get; }
        public ICrypticReengrammitizationGate? CrypticReengrammitizationGate { get; }
        public IGovernedPrimePublicationSink? GovernedPrimePublicationSink { get; }
        public IGovernanceReceiptJournal? GovernanceReceiptJournal { get; }
        public ICmeCollapseQualifier? CmeCollapseQualifier { get; }
        public ICrypticAdmissionMembrane? CrypticAdmissionMembrane { get; }
        public IAgentiFormationObserver? FormationObserver { get; }
        public FirstBootFormationObservationHarness? FirstBootFormationObservationHarness { get; }

        public bool PublicAvailable { get; }
        public bool CrypticAvailable { get; }

        public StoreRegistry(
            ITelemetrySink governanceTelemetry,
            ITelemetrySink storageTelemetry,
            IPublicPlaneStores publicStores,
            IPrimeDerivativePublisher primeDerivativePublisher,
            IPrimeDerivativeView primeDerivativeView,
            bool publicAvailable,
            ICrypticPlaneStores crypticStores,
            bool crypticAvailable,
            ISoulFrameMembrane? soulFrameMembrane = null,
            IGovernanceCycleCognitionService? governanceCognitionService = null,
            IReturnGovernanceAdjudicator? returnGovernanceAdjudicator = null,
            ICrypticCustodyStore? crypticCustodyStore = null,
            ICrypticReengrammitizationGate? crypticReengrammitizationGate = null,
            IGovernedPrimePublicationSink? governedPrimePublicationSink = null,
            IGovernanceReceiptJournal? governanceReceiptJournal = null,
            ICmeCollapseQualifier? cmeCollapseQualifier = null,
            ICrypticAdmissionMembrane? crypticAdmissionMembrane = null,
            IAgentiFormationObserver? formationObserver = null,
            FirstBootFormationObservationHarness? firstBootFormationObservationHarness = null)
        {
            GovernanceTelemetry = governanceTelemetry;
            StorageTelemetry = storageTelemetry;
            Public = publicStores;
            PrimeDerivativePublisher = primeDerivativePublisher;
            PrimeDerivativeView = primeDerivativeView;
            PublicAvailable = publicAvailable;
            Cryptic = crypticStores;
            CrypticAvailable = crypticAvailable;
            SoulFrameMembrane = soulFrameMembrane;
            GovernanceCognitionService = governanceCognitionService;
            ReturnGovernanceAdjudicator = returnGovernanceAdjudicator;
            CrypticCustodyStore = crypticCustodyStore;
            CrypticReengrammitizationGate = crypticReengrammitizationGate;
            GovernedPrimePublicationSink = governedPrimePublicationSink;
            GovernanceReceiptJournal = governanceReceiptJournal;
            CmeCollapseQualifier = cmeCollapseQualifier;
            CrypticAdmissionMembrane = crypticAdmissionMembrane;
            FormationObserver = formationObserver;
            FirstBootFormationObservationHarness = firstBootFormationObservationHarness;
        }

        public StoreRegistry(
            ITelemetrySink governanceTelemetry,
            ITelemetrySink storageTelemetry,
            IPublicPlaneStores publicStores,
            bool publicAvailable,
            ICrypticPlaneStores crypticStores,
            bool crypticAvailable)
            : this(
                governanceTelemetry,
                storageTelemetry,
                publicStores,
                new LegacyPrimeDerivativePublisher(publicStores),
                new LegacyPrimeDerivativePublisher(publicStores),
                publicAvailable,
                crypticStores,
                crypticAvailable)
        {
        }

        private sealed class LegacyPrimeDerivativePublisher : IPrimeDerivativePublisher, IPrimeDerivativeView
        {
            private readonly IPublicPlaneStores _publicStores;
            private readonly List<PrimeDerivativeViewRecord> _views = [];

            public LegacyPrimeDerivativePublisher(IPublicPlaneStores publicStores)
            {
                _publicStores = publicStores;
            }

            public async Task PublishPointerAsync(PrimeDerivativePointerPublication publication, CancellationToken cancellationToken = default)
            {
                await _publicStores.AppendToGELAsync(publication.Pointer, new { classification = publication.Classification, pointer = publication.Pointer });
                await _publicStores.AppendToGoAAsync(publication.Pointer, new { classification = publication.Classification, pointer = publication.Pointer });
                _views.Add(new PrimeDerivativeViewRecord(publication.IdentityId, "pointer", publication.Pointer, publication.Classification, DateTime.UtcNow));
            }

            public async Task PublishRedactedAsync(PrimeDerivativeRedactedPublication publication, CancellationToken cancellationToken = default)
            {
                var pointer = $"prime://{Guid.NewGuid():N}";
                await _publicStores.AppendToGELAsync(pointer, new { classification = publication.Classification, redacted_payload = publication.RedactedPayload });
                await _publicStores.AppendToGoAAsync(pointer, new { classification = publication.Classification, redacted_payload = publication.RedactedPayload });
                _views.Add(new PrimeDerivativeViewRecord(publication.IdentityId, "redacted", publication.RedactedPayload, publication.Classification, DateTime.UtcNow));
            }

            public async Task PublishEncryptedAsync(PrimeDerivativeEncryptedPublication publication, CancellationToken cancellationToken = default)
            {
                await _publicStores.AppendToGELAsync(publication.Pointer, new { classification = "encrypted-pointer", encrypted_payload = publication.EncryptedPayload, pointer = publication.Pointer });
                await _publicStores.AppendToGoAAsync(publication.Pointer, new { classification = "encrypted-pointer", encrypted_payload = publication.EncryptedPayload, pointer = publication.Pointer });
                _views.Add(new PrimeDerivativeViewRecord(publication.IdentityId, "encrypted", publication.Pointer, "encrypted-pointer", DateTime.UtcNow));
            }

            public Task<IReadOnlyList<PrimeDerivativeViewRecord>> ListDerivativeViewsAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<PrimeDerivativeViewRecord>>(_views.ToArray());
            }
        }
    }
}
