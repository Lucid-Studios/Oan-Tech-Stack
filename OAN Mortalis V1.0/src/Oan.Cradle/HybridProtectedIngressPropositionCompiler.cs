using GEL.Contracts;
using GEL.Models;
using Oan.Common;
using SLI.Engine;
using SLI.Engine.Morphology;

namespace Oan.Cradle;

internal sealed class HybridProtectedIngressPropositionCompileResult
{
    public required RootAtlas PropositionAtlas { get; init; }
    public required PropositionalCompileAssessment OracleAssessment { get; init; }
    public required PropositionalCompileAssessment LispAssessment { get; init; }
    public required bool ParityMatched { get; init; }
}

internal sealed class HybridProtectedIngressPropositionCompiler
{
    private const string HumanRootKey = "human-principal";
    private const string CorporateRootKey = "corporate-principal";
    private const string AuthorityRootKey = "authority-relationship";
    private const string HumanHandle = "HumanPrincipal_A";
    private const string CorporateHandle = "CorporatePrincipal_A";

    public async Task<HybridProtectedIngressPropositionCompileResult> CompileAsync(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> grantedRevealModes,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(bootProfile);

        var atlas = CreatePropositionAtlas();
        var oracleAssessment = CreateOracleAssessment(profile, bootProfile, grantedRevealModes, blockedRevealModes);
        var lispAssessment = await CreateLispAssessmentAsync(
            profile,
            bootProfile,
            grantedRevealModes,
            blockedRevealModes,
            oracleAssessment,
            cancellationToken).ConfigureAwait(false);

        return new HybridProtectedIngressPropositionCompileResult
        {
            PropositionAtlas = atlas,
            OracleAssessment = oracleAssessment,
            LispAssessment = lispAssessment,
            ParityMatched = PropositionParityComparer.Matches(oracleAssessment, lispAssessment)
        };
    }

    private static PropositionalCompileAssessment CreateOracleAssessment(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> grantedRevealModes,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes)
    {
        var grade = DetermineGrade(bootProfile, blockedRevealModes);
        var reasonCodes = DetermineReasonCodes(bootProfile, blockedRevealModes);
        var candidate = CreateCandidate(profile, bootProfile, grantedRevealModes, blockedRevealModes, reasonCodes, grade);

        return new PropositionalCompileAssessment
        {
            Candidate = candidate,
            Grade = grade,
            ReasonCodes = reasonCodes,
            Warnings = Array.Empty<string>(),
            ProjectedEngramDraft = grade == PropositionalCompileGrade.Stable
                ? CreateProjectedDraft(candidate, profile, bootProfile, grantedRevealModes)
                : null
        };
    }

    private static async Task<PropositionalCompileAssessment> CreateLispAssessmentAsync(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> grantedRevealModes,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes,
        PropositionalCompileAssessment oracleAssessment,
        CancellationToken cancellationToken)
    {
        var program = BuildLispProgram(profile, bootProfile, grantedRevealModes, blockedRevealModes, oracleAssessment);
        var bridge = LispBridge.CreateForDetachedRuntime();
        await bridge.InitializeAsync(cancellationToken).ConfigureAwait(false);

        var result = await bridge.ExecutePropositionProgramAsync(
            program,
            objective: "hybrid-protected-ingress-proposition",
            cancellationToken).ConfigureAwait(false);

        var grade = result.Grade switch
        {
            SliPropositionalCompileGrade.Stable => PropositionalCompileGrade.Stable,
            SliPropositionalCompileGrade.NeedsSpecification => PropositionalCompileGrade.NeedsSpecification,
            _ => PropositionalCompileGrade.Rejected
        };

        var candidate = new PropositionalCompileCandidate
        {
            Subject = new PropositionTerm
            {
                Role = PropositionRole.Subject,
                RootKey = result.Subject.RootKey,
                SymbolicHandle = result.Subject.SymbolicHandle
            },
            PredicateRoot = result.PredicateRoot,
            Object = new PropositionTerm
            {
                Role = PropositionRole.Object,
                RootKey = result.Object.RootKey,
                SymbolicHandle = result.Object.SymbolicHandle
            },
            Qualifiers = result.Qualifiers
                .Select(qualifier => new PropositionQualifier
                {
                    Name = qualifier.Name,
                    Value = qualifier.Value
                })
                .ToArray(),
            ContextTags = result.ContextTags
                .Select(tag => new PropositionContextTag
                {
                    Name = tag.Name,
                    Value = tag.Value
                })
                .ToArray(),
            DiagnosticPropositionRender = result.DiagnosticRender,
            UnresolvedTensions = result.UnresolvedTensions.ToArray()
        };

        return new PropositionalCompileAssessment
        {
            Candidate = candidate,
            Grade = grade,
            ReasonCodes = oracleAssessment.ReasonCodes,
            Warnings = Array.Empty<string>(),
            ProjectedEngramDraft = grade == PropositionalCompileGrade.Stable
                ? oracleAssessment.ProjectedEngramDraft
                : null
        };
    }

    private static IReadOnlyList<string> BuildLispProgram(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> grantedRevealModes,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes,
        PropositionalCompileAssessment oracleAssessment)
    {
        var lines = new List<string>
        {
            "(prop-subject-root \"human-principal\")",
            "(prop-subject-handle \"HumanPrincipal_A\")",
            "(prop-predicate-root \"authority-relationship\")",
            "(prop-object-root \"corporate-principal\")",
            "(prop-object-handle \"CorporatePrincipal_A\")",
            $"(prop-qualifier \"authority-role\" \"{Escape(profile.AuthorityRelationship)}\")",
            $"(prop-qualifier \"boot-class\" \"{profile.RequestedBootClass}\")",
            $"(prop-qualifier \"expansion-rights\" \"{bootProfile.ExpansionRights}\")"
        };

        foreach (var mode in grantedRevealModes)
        {
            lines.Add($"(prop-context \"granted-reveal\" \"{mode}\")");
        }

        foreach (var mode in blockedRevealModes)
        {
            lines.Add($"(prop-context \"blocked-reveal\" \"{mode}\")");
        }

        foreach (var reasonCode in oracleAssessment.ReasonCodes)
        {
            lines.Add($"(prop-context \"reason-code\" \"{Escape(reasonCode)}\")");
        }

        foreach (var tension in oracleAssessment.Candidate.UnresolvedTensions)
        {
            lines.Add($"(prop-tension \"{Escape(tension)}\")");
        }

        lines.Add($"(prop-render \"{Escape(oracleAssessment.Candidate.DiagnosticPropositionRender)}\")");
        lines.Add($"(prop-grade \"{oracleAssessment.Grade}\")");
        return lines;
    }

    private static RootAtlas CreatePropositionAtlas()
    {
        return RootAtlas.Create(
            version: "hybrid-protected-ingress.proposition-v1",
            entries:
            [
                CreateEntry(HumanRootKey, "Human principal", "atlas.governance.principal", "hp"),
                CreateEntry(CorporateRootKey, "Corporate principal", "atlas.governance.principal", "cp"),
                CreateEntry(AuthorityRootKey, "Authority relationship", "atlas.governance.relationship", "ar")
            ],
            domainDescriptors:
            [
                new DomainDescriptor
                {
                    DomainName = "atlas.governance.principal",
                    Description = "Governance principal proposition roots.",
                    Tags = ["governance", "principal", "test-only"]
                },
                new DomainDescriptor
                {
                    DomainName = "atlas.governance.relationship",
                    Description = "Governance relationship proposition roots.",
                    Tags = ["governance", "relationship", "test-only"]
                }
            ]);
    }

    private static RootAtlasEntry CreateEntry(
        string key,
        string displayLabel,
        string domain,
        string symbolicHandle)
    {
        return new RootAtlasEntry
        {
            Root = new PredicateRoot
            {
                Key = key,
                DisplayLabel = displayLabel,
                AtlasDomain = domain,
                SymbolicHandle = symbolicHandle,
                DictionaryPointer = $"atlas://hybrid-ingress/{key}"
            },
            VariantForms = [key],
            SymbolicConstructors =
            [
                new SymbolicConstructorTriplet
                {
                    RootKey = key,
                    RootSymbol = symbolicHandle,
                    CanonicalText = key,
                    MergedGlyph = symbolicHandle
                }
            ],
            FrequencyWeight = 1d
        };
    }

    private static PropositionalCompileCandidate CreateCandidate(
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> grantedRevealModes,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes,
        IReadOnlyList<string> reasonCodes,
        PropositionalCompileGrade grade)
    {
        var qualifiers = new List<PropositionQualifier>
        {
            new()
            {
                Name = "authority-role",
                Value = profile.AuthorityRelationship
            },
            new()
            {
                Name = "boot-class",
                Value = profile.RequestedBootClass.ToString()
            },
            new()
            {
                Name = "expansion-rights",
                Value = bootProfile.ExpansionRights.ToString()
            }
        };

        var contextTags = new List<PropositionContextTag>();
        contextTags.AddRange(grantedRevealModes.Select(mode => new PropositionContextTag
        {
            Name = "granted-reveal",
            Value = mode.ToString()
        }));
        contextTags.AddRange(blockedRevealModes.Select(mode => new PropositionContextTag
        {
            Name = "blocked-reveal",
            Value = mode.ToString()
        }));
        contextTags.AddRange(reasonCodes.Select(code => new PropositionContextTag
        {
            Name = "reason-code",
            Value = code
        }));

        return new PropositionalCompileCandidate
        {
            Subject = new PropositionTerm
            {
                Role = PropositionRole.Subject,
                RootKey = HumanRootKey,
                SymbolicHandle = HumanHandle
            },
            PredicateRoot = AuthorityRootKey,
            Object = new PropositionTerm
            {
                Role = PropositionRole.Object,
                RootKey = CorporateRootKey,
                SymbolicHandle = CorporateHandle
            },
            Qualifiers = qualifiers,
            ContextTags = contextTags,
            DiagnosticPropositionRender = $"{AuthorityRootKey}({HumanHandle},{CorporateHandle})",
            UnresolvedTensions = grade == PropositionalCompileGrade.Stable ? Array.Empty<string>() : reasonCodes.ToArray()
        };
    }

    private static EngramDraft CreateProjectedDraft(
        PropositionalCompileCandidate candidate,
        HybridProtectedIngressProfile profile,
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> grantedRevealModes)
    {
        return new EngramDraft
        {
            RootKey = candidate.PredicateRoot,
            EpistemicClass = EngramEpistemicClass.Propositional,
            Trunk = new EngramTrunk
            {
                Segments =
                [
                    candidate.Subject.SymbolicHandle,
                    candidate.PredicateRoot,
                    candidate.Object.SymbolicHandle
                ],
                Summary = candidate.DiagnosticPropositionRender
            },
            Branches =
            [
                new EngramBranch
                {
                    Name = "subject",
                    RootKey = candidate.Subject.RootKey,
                    SymbolicHandle = candidate.Subject.SymbolicHandle
                },
                new EngramBranch
                {
                    Name = "object",
                    RootKey = candidate.Object.RootKey,
                    SymbolicHandle = candidate.Object.SymbolicHandle
                }
            ],
            Invariants =
            [
                new EngramInvariant
                {
                    Key = "proposition.render",
                    Statement = candidate.DiagnosticPropositionRender
                },
                new EngramInvariant
                {
                    Key = "authority.role",
                    Statement = profile.AuthorityRelationship
                },
                new EngramInvariant
                {
                    Key = "boot.class",
                    Statement = profile.RequestedBootClass.ToString()
                },
                new EngramInvariant
                {
                    Key = "expansion.rights",
                    Statement = bootProfile.ExpansionRights.ToString()
                },
                new EngramInvariant
                {
                    Key = "prime.reveal.granted",
                    Statement = grantedRevealModes.Count == 0 ? PrimeRevealMode.None.ToString() : string.Join(",", grantedRevealModes)
                }
            ],
            RequestedClosureGrade = EngramClosureGrade.Closed
        };
    }

    private static PropositionalCompileGrade DetermineGrade(
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes)
    {
        if (bootProfile.Decision == FirstBootGovernanceDecision.Quarantine)
        {
            return PropositionalCompileGrade.Rejected;
        }

        if (blockedRevealModes.Count > 0)
        {
            return PropositionalCompileGrade.Rejected;
        }

        return PropositionalCompileGrade.Stable;
    }

    private static IReadOnlyList<string> DetermineReasonCodes(
        InternalGovernanceBootProfile bootProfile,
        IReadOnlyList<PrimeRevealMode> blockedRevealModes)
    {
        if (bootProfile.Decision == FirstBootGovernanceDecision.Quarantine)
        {
            return ["topology.personal-swarm.denied"];
        }

        if (blockedRevealModes.Contains(PrimeRevealMode.AuthorizedFieldReveal))
        {
            return ["reveal.authorized-field.denied"];
        }

        return Array.Empty<string>();
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static class PropositionParityComparer
    {
        public static bool Matches(
            PropositionalCompileAssessment left,
            PropositionalCompileAssessment right)
        {
            return left.Grade == right.Grade &&
                   string.Equals(left.Candidate.Subject.RootKey, right.Candidate.Subject.RootKey, StringComparison.Ordinal) &&
                   string.Equals(left.Candidate.Subject.SymbolicHandle, right.Candidate.Subject.SymbolicHandle, StringComparison.Ordinal) &&
                   string.Equals(left.Candidate.PredicateRoot, right.Candidate.PredicateRoot, StringComparison.Ordinal) &&
                   string.Equals(left.Candidate.Object.RootKey, right.Candidate.Object.RootKey, StringComparison.Ordinal) &&
                   string.Equals(left.Candidate.Object.SymbolicHandle, right.Candidate.Object.SymbolicHandle, StringComparison.Ordinal) &&
                   string.Equals(left.Candidate.DiagnosticPropositionRender, right.Candidate.DiagnosticPropositionRender, StringComparison.Ordinal) &&
                   left.Candidate.Qualifiers.SequenceEqual(right.Candidate.Qualifiers, PropositionQualifierComparer.Instance) &&
                   left.Candidate.ContextTags.SequenceEqual(right.Candidate.ContextTags, PropositionContextTagComparer.Instance);
        }

        private sealed class PropositionQualifierComparer : IEqualityComparer<PropositionQualifier>
        {
            public static readonly PropositionQualifierComparer Instance = new();

            public bool Equals(PropositionQualifier? x, PropositionQualifier? y)
            {
                if (x is null || y is null)
                {
                    return x is null && y is null;
                }

                return string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                       string.Equals(x.Value, y.Value, StringComparison.Ordinal);
            }

            public int GetHashCode(PropositionQualifier obj)
            {
                return HashCode.Combine(obj.Name, obj.Value);
            }
        }

        private sealed class PropositionContextTagComparer : IEqualityComparer<PropositionContextTag>
        {
            public static readonly PropositionContextTagComparer Instance = new();

            public bool Equals(PropositionContextTag? x, PropositionContextTag? y)
            {
                if (x is null || y is null)
                {
                    return x is null && y is null;
                }

                return string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                       string.Equals(x.Value, y.Value, StringComparison.Ordinal);
            }

            public int GetHashCode(PropositionContextTag obj)
            {
                return HashCode.Combine(obj.Name, obj.Value);
            }
        }
    }
}
