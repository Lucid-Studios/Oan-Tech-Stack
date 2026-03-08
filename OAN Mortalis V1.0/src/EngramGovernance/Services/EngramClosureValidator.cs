using GEL.Contracts;
using GEL.Models;
using Oan.Spinal;

namespace EngramGovernance.Services;

public sealed class EngramClosureValidator : IEngramClosureValidator
{
    public Task<EngramClosureDecision> ValidateAsync(
        EngramDraft draft,
        RootAtlas atlas,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(atlas);
        cancellationToken.ThrowIfCancellationRequested();

        var reasonCodes = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(draft.RootKey))
        {
            reasonCodes.Add("engram.root.missing");
            return Task.FromResult(Reject(reasonCodes, warnings));
        }

        if (!atlas.TryResolveRoot(draft.RootKey, out var root))
        {
            reasonCodes.Add("engram.root.unresolved");
            return Task.FromResult(Reject(reasonCodes, warnings));
        }

        if (draft.EpistemicClass is null)
        {
            reasonCodes.Add("engram.epistemic_class.missing");
            return Task.FromResult(Reject(reasonCodes, warnings));
        }

        if (draft.Trunk is null || draft.Trunk.Segments.Count == 0 || draft.Trunk.Segments.All(string.IsNullOrWhiteSpace))
        {
            reasonCodes.Add("engram.trunk.empty");
            return Task.FromResult(Reject(reasonCodes, warnings));
        }

        if (draft.Invariants.Count == 0 || draft.Invariants.Any(invariant => string.IsNullOrWhiteSpace(invariant.Key) || string.IsNullOrWhiteSpace(invariant.Statement)))
        {
            reasonCodes.Add("engram.invariant.missing");
            return Task.FromResult(Reject(reasonCodes, warnings));
        }

        var requestedGrade = NormalizeRequestedGrade(draft, warnings);
        var normalizedBranches = new List<EngramBranch>(draft.Branches.Count);
        var unresolvedBranchReference = false;

        foreach (var branch in draft.Branches)
        {
            if (string.IsNullOrWhiteSpace(branch.Name))
            {
                reasonCodes.Add("engram.branch.name.missing");
                return Task.FromResult(Reject(reasonCodes, warnings));
            }

            if (string.IsNullOrWhiteSpace(branch.RootKey) && branch.ReferencedEngramId is null)
            {
                reasonCodes.Add("engram.branch.reference.missing");
                return Task.FromResult(Reject(reasonCodes, warnings));
            }

            if (!string.IsNullOrWhiteSpace(branch.RootKey) &&
                !atlas.TryResolveRoot(branch.RootKey, out _))
            {
                unresolvedBranchReference = true;
                reasonCodes.Add($"engram.branch.root.unresolved:{branch.RootKey}");
            }

            normalizedBranches.Add(new EngramBranch
            {
                Name = branch.Name,
                RootKey = branch.RootKey,
                ReferencedEngramId = branch.ReferencedEngramId,
                SymbolicHandle = branch.SymbolicHandle
            });
        }

        if (unresolvedBranchReference)
        {
            return Task.FromResult(new EngramClosureDecision
            {
                Grade = EngramClosureGrade.NeedsSpecification,
                NormalizedId = null,
                CanonicalEngram = null,
                ReasonCodes = reasonCodes,
                Warnings = warnings
            });
        }

        if (normalizedBranches.Count == 0)
        {
            warnings.Add("engram.branch.bootstrap-empty");
            if (requestedGrade == EngramClosureGrade.Closed)
            {
                reasonCodes.Add("engram.branch.required_for_closed");
                return Task.FromResult(new EngramClosureDecision
                {
                    Grade = EngramClosureGrade.NeedsSpecification,
                    NormalizedId = null,
                    CanonicalEngram = null,
                    ReasonCodes = reasonCodes,
                    Warnings = warnings
                });
            }

            var bootstrapEngram = BuildCanonicalEngram(
                draft,
                atlas,
                root,
                Array.Empty<EngramBranch>());

            return Task.FromResult(new EngramClosureDecision
            {
                Grade = EngramClosureGrade.BootstrapClosed,
                NormalizedId = bootstrapEngram.Id,
                CanonicalEngram = bootstrapEngram,
                ReasonCodes = reasonCodes,
                Warnings = warnings
            });
        }

        var canonicalEngram = BuildCanonicalEngram(draft, atlas, root, normalizedBranches);
        return Task.FromResult(new EngramClosureDecision
        {
            Grade = EngramClosureGrade.Closed,
            NormalizedId = canonicalEngram.Id,
            CanonicalEngram = canonicalEngram,
            ReasonCodes = reasonCodes,
            Warnings = warnings
        });
    }

    private static EngramClosureDecision Reject(
        IReadOnlyList<string> reasonCodes,
        IReadOnlyList<string> warnings)
    {
        return new EngramClosureDecision
        {
            Grade = EngramClosureGrade.Rejected,
            NormalizedId = null,
            CanonicalEngram = null,
            ReasonCodes = reasonCodes,
            Warnings = warnings
        };
    }

    private static EngramClosureGrade NormalizeRequestedGrade(EngramDraft draft, ICollection<string> warnings)
    {
        if (draft.RequestedClosureGrade is EngramClosureGrade.BootstrapClosed or EngramClosureGrade.Closed)
        {
            return draft.RequestedClosureGrade;
        }

        warnings.Add("engram.requested_grade.normalized_to_closed");
        return EngramClosureGrade.Closed;
    }

    private static Engram BuildCanonicalEngram(
        EngramDraft draft,
        RootAtlas atlas,
        PredicateRoot root,
        IReadOnlyList<EngramBranch> branches)
    {
        var normalizedId = draft.ProposedId ?? new EngramId(ComputeCanonicalId(draft, atlas, root, branches));

        return new Engram
        {
            Id = normalizedId,
            AtlasVersion = atlas.Version,
            Root = root,
            EpistemicClass = draft.EpistemicClass!.Value,
            Trunk = new EngramTrunk
            {
                Segments = draft.Trunk.Segments
                    .Where(segment => !string.IsNullOrWhiteSpace(segment))
                    .ToArray(),
                Summary = draft.Trunk.Summary
            },
            Branches = branches,
            Invariants = draft.Invariants
                .Select(invariant => new EngramInvariant
                {
                    Key = invariant.Key,
                    Statement = invariant.Statement
                })
                .ToArray()
        };
    }

    private static string ComputeCanonicalId(
        EngramDraft draft,
        RootAtlas atlas,
        PredicateRoot root,
        IReadOnlyList<EngramBranch> branches)
    {
        var projection = new
        {
            AtlasDigest = atlas.Digest,
            Root = root.Key,
            EpistemicClass = draft.EpistemicClass,
            Trunk = draft.Trunk.Segments
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .ToArray(),
            Branches = branches.Select(branch => new
            {
                branch.Name,
                branch.RootKey,
                ReferencedEngramId = branch.ReferencedEngramId?.Value,
                branch.SymbolicHandle
            }),
            Invariants = draft.Invariants.Select(invariant => new
            {
                invariant.Key,
                invariant.Statement
            })
        };

        return Primitives.ComputeHash(Primitives.ToCanonicalJson(projection));
    }
}
