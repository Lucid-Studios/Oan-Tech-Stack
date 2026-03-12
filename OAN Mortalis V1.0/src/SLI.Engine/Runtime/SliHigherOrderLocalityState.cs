namespace SLI.Engine.Runtime;

internal enum HigherOrderLocalityResidueKind
{
    MissingLocalityPrerequisites,
    MissingAnchorPrerequisites,
    InvalidPostureValue,
    InvalidParticipationMode,
    IncompletePerspective,
    IncompleteParticipation
}

internal sealed class HigherOrderLocalityResidue
{
    public required HigherOrderLocalityResidueKind Kind { get; init; }
    public required string Source { get; init; }
    public required string Detail { get; init; }
}

internal sealed class SliPerspectiveState
{
    public bool IsConfigured { get; set; }
    public Dictionary<string, double> OrientationVector { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> EthicalConstraints { get; } = [];
    public Dictionary<string, double> WeightFunctions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<HigherOrderLocalityResidue> Residues { get; } = [];

    public void Reset()
    {
        IsConfigured = false;
        OrientationVector.Clear();
        EthicalConstraints.Clear();
        WeightFunctions.Clear();
        Residues.Clear();
    }
}

internal sealed class SliParticipationState
{
    public bool IsConfigured { get; set; }
    public string Mode { get; set; } = SliHigherOrderLocalityState.ObserveMode;
    public string Role { get; set; } = string.Empty;
    public List<string> InteractionRules { get; } = [];
    public List<string> CapabilitySet { get; } = [];
    public List<HigherOrderLocalityResidue> Residues { get; } = [];

    public void Reset()
    {
        IsConfigured = false;
        Mode = SliHigherOrderLocalityState.ObserveMode;
        Role = string.Empty;
        InteractionRules.Clear();
        CapabilitySet.Clear();
        Residues.Clear();
    }
}

internal sealed class SliHigherOrderLocalityState
{
    public const string BoundedSealPosture = "bounded";
    public const string MaskedRevealPosture = "masked";
    public const string ObserveMode = "observe";

    public string LocalityHandle { get; private set; } = string.Empty;
    public string SelfAnchor { get; set; } = string.Empty;
    public string OtherAnchor { get; set; } = string.Empty;
    public string RelationAnchor { get; set; } = string.Empty;
    public string SealPosture { get; set; } = BoundedSealPosture;
    public string RevealPosture { get; set; } = MaskedRevealPosture;
    public List<string> Warnings { get; } = [];
    public List<HigherOrderLocalityResidue> Residues { get; } = [];
    public SliPerspectiveState Perspective { get; } = new();
    public SliParticipationState Participation { get; } = new();

    public void Reset(string localityHandle)
    {
        LocalityHandle = localityHandle;
        SelfAnchor = string.Empty;
        OtherAnchor = string.Empty;
        RelationAnchor = string.Empty;
        SealPosture = BoundedSealPosture;
        RevealPosture = MaskedRevealPosture;
        Warnings.Clear();
        Residues.Clear();
        Perspective.Reset();
        Participation.Reset();
    }

    public void AddWarning(string warning)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(warning);
        Warnings.Add(warning);
    }

    public void AddResidue(HigherOrderLocalityResidue residue)
    {
        ArgumentNullException.ThrowIfNull(residue);
        Residues.Add(residue);
    }
}
