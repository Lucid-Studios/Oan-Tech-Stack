using CradleTek.Host.Interfaces;
using CradleTek.Host.Models;
using Oan.Common;
using System.Security.Cryptography;
using System.Text;

namespace CradleTek.Mantle;

public sealed class MantleOfSovereigntyService : IMantleService, ICrypticCustodyStore, ICrypticReengrammitizationGate
{
    private readonly Dictionary<Guid, OpalEngram> _shadows = [];
    private readonly List<SoulFrameSnapshotRequest> _soulFrameSnapshots = [];
    private readonly Dictionary<string, CrypticReengrammitizationReceipt> _reengramReceiptsByKey = [];

    public string ContainerName => "MantleOfSovereignty";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    // MoS stores shadow copies only; it does not mutate live OpalEngrams.
    public Task ShadowSnapshotAsync(OpalEngram engram, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engram);
        _shadows[engram.IdentityId] = Clone(engram);
        return Task.CompletedTask;
    }

    public Task RequestSoulFrameSnapshotAsync(SoulFrameSnapshotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _soulFrameSnapshots.Add(request);
        return Task.CompletedTask;
    }

    public Task<OpalEngram?> RestoreLastKnownGoodAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        if (_shadows.TryGetValue(identityId, out var shadow))
        {
            return Task.FromResult<OpalEngram?>(Clone(shadow));
        }

        return Task.FromResult<OpalEngram?>(null);
    }

    public Task<CrypticCustodyRecord> AppendAsync(
        CrypticCustodyAppendRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CustodyDomain);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PayloadPointer);

        if (!_shadows.TryGetValue(request.IdentityId, out var shadow))
        {
            shadow = new OpalEngram(request.IdentityId);
            _shadows[request.IdentityId] = shadow;
        }

        shadow.AppendOnlyLedgerBlockChain.AppendCryptic(
            new cSelfGEL(
                BlockHash: HashHex($"{request.IdentityId:D}|{request.CustodyDomain}|{request.PayloadPointer}|{request.Classification}"),
                CreatedAtUtc: DateTimeOffset.UtcNow,
                EncryptedPayloadPointer: request.PayloadPointer));

        return Task.FromResult(new CrypticCustodyRecord(
            request.IdentityId,
            request.CustodyDomain,
            request.PayloadPointer,
            IsMasked: true,
            Timestamp: DateTime.UtcNow));
    }

    public Task<IReadOnlyList<CrypticCustodyRecord>> ReadGuardedAsync(
        CrypticGuardedReadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PolicyHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Purpose);

        if (!_shadows.TryGetValue(request.IdentityId, out var shadow))
        {
            return Task.FromResult<IReadOnlyList<CrypticCustodyRecord>>([]);
        }

        var records = shadow.AppendOnlyLedgerBlockChain.cSelfGEL
            .Select(pointer => new CrypticCustodyRecord(
                request.IdentityId,
                CustodyDomain: "cMoS",
                Pointer: pointer.EncryptedPayloadPointer,
                IsMasked: true,
                Timestamp: DateTime.UtcNow))
            .ToList();

        return Task.FromResult<IReadOnlyList<CrypticCustodyRecord>>(records);
    }

    public async Task<CrypticCustodyRecord?> RestoreAsync(
        Guid identityId,
        CancellationToken cancellationToken = default)
    {
        var restored = await RestoreLastKnownGoodAsync(identityId, cancellationToken).ConfigureAwait(false);
        if (restored is null)
        {
            return null;
        }

        var pointer = $"cmos://restore/{identityId:D}";
        return new CrypticCustodyRecord(identityId, "cMoS", pointer, IsMasked: true, Timestamp: DateTime.UtcNow);
    }

    public Task<bool> CanAdmitAsync(
        GovernedReengrammitizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accepted =
            request.CandidateId != Guid.Empty &&
            !string.IsNullOrWhiteSpace(request.CMEId) &&
            !string.IsNullOrWhiteSpace(request.SourceTheater) &&
            !string.IsNullOrWhiteSpace(request.ResiduePointer) &&
            !string.IsNullOrWhiteSpace(request.Reason) &&
            !string.IsNullOrWhiteSpace(request.AuthorizedBy);
        return Task.FromResult(accepted);
    }

    public async Task<CrypticReengrammitizationReceipt> ReengrammitizeAsync(
        GovernedReengrammitizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_reengramReceiptsByKey.TryGetValue(request.IdempotencyKey, out var existingReceipt))
        {
            return existingReceipt;
        }

        var accepted = await CanAdmitAsync(request, cancellationToken).ConfigureAwait(false);
        if (accepted)
        {
            await AppendAsync(
                new CrypticCustodyAppendRequest(
                    request.IdentityId,
                    CustodyDomain: "cMoS",
                    PayloadPointer: request.ResiduePointer,
                    Classification: "reengrammitized-residue"),
                cancellationToken).ConfigureAwait(false);
        }

        var receipt = new CrypticReengrammitizationReceipt(
            request.IdentityId,
            CustodyDomain: "cMoS",
            ReceiptPointer: $"cmos://reengram/{request.IdentityId:D}/{Guid.NewGuid():N}",
            Accepted: accepted,
            Timestamp: DateTime.UtcNow);
        _reengramReceiptsByKey[request.IdempotencyKey] = receipt;
        return receipt;
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static OpalEngram Clone(OpalEngram source)
    {
        var cloned = new OpalEngram(source.IdentityId);
        foreach (var block in source.AppendOnlyLedgerBlockChain.cSelfGEL)
        {
            cloned.AppendOnlyLedgerBlockChain.AppendCryptic(block);
        }

        foreach (var block in source.AppendOnlyLedgerBlockChain.SelfGEL)
        {
            cloned.AppendOnlyLedgerBlockChain.AppendPublic(block);
        }

        return cloned;
    }
}
