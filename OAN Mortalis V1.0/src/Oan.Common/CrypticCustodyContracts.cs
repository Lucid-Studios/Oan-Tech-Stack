namespace Oan.Common;

public sealed record CrypticCustodyAppendRequest(
    Guid IdentityId,
    string CustodyDomain,
    string PayloadPointer,
    string Classification);

public sealed record CrypticGuardedReadRequest(
    Guid IdentityId,
    string PolicyHandle,
    string Purpose);

public sealed record CrypticCustodyRecord(
    Guid IdentityId,
    string CustodyDomain,
    string Pointer,
    bool IsMasked,
    DateTime Timestamp);

public sealed record CrypticReengrammitizationReceipt(
    Guid IdentityId,
    string CustodyDomain,
    string ReceiptPointer,
    bool Accepted,
    DateTime Timestamp);

public interface ICrypticCustodyStore
{
    Task<CrypticCustodyRecord> AppendAsync(
        CrypticCustodyAppendRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CrypticCustodyRecord>> ReadGuardedAsync(
        CrypticGuardedReadRequest request,
        CancellationToken cancellationToken = default);

    Task<CrypticCustodyRecord?> RestoreAsync(
        Guid identityId,
        CancellationToken cancellationToken = default);
}

public interface ICrypticReengrammitizationGate
{
    Task<bool> CanAdmitAsync(
        GovernedReengrammitizationRequest request,
        CancellationToken cancellationToken = default);

    Task<CrypticReengrammitizationReceipt> ReengrammitizeAsync(
        GovernedReengrammitizationRequest request,
        CancellationToken cancellationToken = default);
}
