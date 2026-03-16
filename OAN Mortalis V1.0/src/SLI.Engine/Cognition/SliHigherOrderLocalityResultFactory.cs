using SLI.Engine.Runtime;

namespace SLI.Engine.Cognition;

internal static class SliHigherOrderLocalityResultFactory
{
    public static SliHigherOrderLocalityResult Create(
        SliExecutionContext context,
        SliTargetExecutionLineage? targetLineage = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var state = context.HigherOrderLocalityState;
        return new SliHigherOrderLocalityResult
        {
            LocalityHandle = state.LocalityHandle,
            SelfAnchor = state.SelfAnchor,
            OtherAnchor = state.OtherAnchor,
            RelationAnchor = state.RelationAnchor,
            SealPosture = state.SealPosture,
            RevealPosture = state.RevealPosture,
            Warnings = state.Warnings.ToArray(),
            Residues = CloneResidues(state.Residues),
            Perspective = new SliPerspectiveResult
            {
                IsConfigured = state.Perspective.IsConfigured,
                OrientationVector = new Dictionary<string, double>(state.Perspective.OrientationVector, StringComparer.OrdinalIgnoreCase),
                EthicalConstraints = state.Perspective.EthicalConstraints.ToArray(),
                WeightFunctions = new Dictionary<string, double>(state.Perspective.WeightFunctions, StringComparer.OrdinalIgnoreCase),
                Residues = CloneResidues(state.Perspective.Residues)
            },
            Participation = new SliParticipationResult
            {
                IsConfigured = state.Participation.IsConfigured,
                Mode = state.Participation.Mode,
                Role = state.Participation.Role,
                InteractionRules = state.Participation.InteractionRules.ToArray(),
                CapabilitySet = state.Participation.CapabilitySet.ToArray(),
                Residues = CloneResidues(state.Participation.Residues)
            },
            SymbolicTrace = context.TraceLines.ToArray(),
            TargetLineage = targetLineage
        };
    }

    private static IReadOnlyList<HigherOrderLocalityResidue> CloneResidues(IEnumerable<HigherOrderLocalityResidue> residues)
    {
        return residues
            .Select(residue => new HigherOrderLocalityResidue
            {
                Kind = residue.Kind,
                Source = residue.Source,
                Detail = residue.Detail
            })
            .ToArray();
    }
}
