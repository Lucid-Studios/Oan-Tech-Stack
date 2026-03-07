namespace Oan.Common;

public sealed record PrimeDerivativePointerPublication(
    Guid IdentityId,
    string Pointer,
    string Classification);

public sealed record PrimeDerivativeRedactedPublication(
    Guid IdentityId,
    string RedactedPayload,
    string Classification);

public sealed record PrimeDerivativeEncryptedPublication(
    Guid IdentityId,
    string EncryptedPayload,
    string Pointer);

public sealed record PrimeDerivativeCheckedViewPublication(
    Guid IdentityId,
    string CheckedView,
    string Classification);

public sealed record PrimeDerivativeViewRecord(
    Guid IdentityId,
    string RepresentationKind,
    string Value,
    string Classification,
    DateTime Timestamp);

public interface IPrimeDerivativePublisher
{
    Task PublishPointerAsync(
        PrimeDerivativePointerPublication publication,
        CancellationToken cancellationToken = default);

    Task PublishRedactedAsync(
        PrimeDerivativeRedactedPublication publication,
        CancellationToken cancellationToken = default);

    Task PublishEncryptedAsync(
        PrimeDerivativeEncryptedPublication publication,
        CancellationToken cancellationToken = default);
}

public interface IPrimeDerivativeView
{
    Task<IReadOnlyList<PrimeDerivativeViewRecord>> ListDerivativeViewsAsync(
        CancellationToken cancellationToken = default);
}

public interface IGovernedPrimePublicationSink
{
    Task<GovernedPrimeDerivativeLane> PublishApprovedOutcomeAsync(
        GovernedPrimePublicationRequest request,
        CancellationToken cancellationToken = default);
}
