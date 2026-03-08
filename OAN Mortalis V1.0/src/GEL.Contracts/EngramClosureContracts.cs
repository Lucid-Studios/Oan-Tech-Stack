using GEL.Models;
using Oan.Spinal;

namespace GEL.Contracts;

public interface IEngramClosureValidator
{
    Task<EngramClosureDecision> ValidateAsync(
        EngramDraft draft,
        RootAtlas atlas,
        CancellationToken cancellationToken = default);
}

public interface IGelCommitGateway
{
    Task<EngramId> CommitAsync(Engram engram, CancellationToken cancellationToken = default);
}
