using CradleTek.Host.Interfaces;
using Oan.Common;

namespace CradleTek.Public;

public sealed class PublicLayerService : IPublicStore, IPrimeDerivativePublisher, IPrimeDerivativeView, IGovernedPrimePublicationSink
{
    private readonly List<string> _publishedPointers = [];
    private readonly List<PrimeDerivativeViewRecord> _views = [];
    private readonly HashSet<string> _governedPublicationKeys = [];

    public string ContainerName => "PublicLayer";

    public string GelService => "GEL";
    public string GoAService => "GoA";
    public string PrimeSliService => "PrimeSLI";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishPointerAsync(string pointer, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pointer);
        _publishedPointers.Add(pointer);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ListPublishedPointersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(_publishedPointers.ToList());
    }

    public Task PublishPointerAsync(
        PrimeDerivativePointerPublication publication,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publication);
        _publishedPointers.Add(publication.Pointer);
        _views.Add(new PrimeDerivativeViewRecord(
            publication.IdentityId,
            RepresentationKind: "pointer",
            Value: publication.Pointer,
            publication.Classification,
            Timestamp: DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task PublishRedactedAsync(
        PrimeDerivativeRedactedPublication publication,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publication);
        _views.Add(new PrimeDerivativeViewRecord(
            publication.IdentityId,
            RepresentationKind: "redacted",
            Value: publication.RedactedPayload,
            publication.Classification,
            Timestamp: DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task PublishEncryptedAsync(
        PrimeDerivativeEncryptedPublication publication,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publication);
        _publishedPointers.Add(publication.Pointer);
        _views.Add(new PrimeDerivativeViewRecord(
            publication.IdentityId,
            RepresentationKind: "encrypted",
            Value: publication.Pointer,
            Classification: "encrypted-pointer",
            Timestamp: DateTime.UtcNow));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PrimeDerivativeViewRecord>> ListDerivativeViewsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<PrimeDerivativeViewRecord>>(_views.ToList());
    }

    public async Task<GovernedPrimeDerivativeLane> PublishApprovedOutcomeAsync(
        GovernedPrimePublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var published = GovernedPrimeDerivativeLane.Neither;

        if (request.AuthorizedLanes.HasFlag(GovernedPrimeDerivativeLane.Pointer))
        {
            var pointerKey = $"{request.IdempotencyKey}:pointer";
            if (_governedPublicationKeys.Add(pointerKey))
            {
                await PublishPointerAsync(
                        new PrimeDerivativePointerPublication(
                            request.IdentityId,
                            request.PointerValue,
                            request.Classification),
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            published |= GovernedPrimeDerivativeLane.Pointer;
        }

        if (request.AuthorizedLanes.HasFlag(GovernedPrimeDerivativeLane.CheckedView))
        {
            var checkedViewKey = $"{request.IdempotencyKey}:checked-view";
            if (_governedPublicationKeys.Add(checkedViewKey))
            {
                _views.Add(new PrimeDerivativeViewRecord(
                    request.IdentityId,
                    RepresentationKind: "checked-view",
                    Value: request.CheckedViewValue,
                    request.Classification,
                    Timestamp: DateTime.UtcNow));
            }

            published |= GovernedPrimeDerivativeLane.CheckedView;
        }

        return published;
    }
}
