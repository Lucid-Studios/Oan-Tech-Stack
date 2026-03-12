namespace SLI.Engine.Runtime;

internal enum HigherOrderLocalityResidueKind
{
    MissingLocalityPrerequisites,
    MissingAnchorPrerequisites,
    InvalidPostureValue,
    InvalidParticipationMode,
    IncompletePerspective,
    IncompleteParticipation,
    MissingRehearsalPrerequisites,
    InvalidRehearsalMode,
    InvalidIdentitySeal,
    IncompleteRehearsal,
    MissingWitnessPrerequisites,
    InvalidWitnessReference,
    InvalidGlueThreshold,
    IncompleteWitness,
    LawfulDifferenceResidue,
    NonCandidateWitness
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

internal sealed class SliRehearsalSubstitutionState
{
    public required string Source { get; init; }
    public required string Target { get; init; }
}

internal sealed class SliRehearsalAnalogyState
{
    public required string Source { get; init; }
    public required string Target { get; init; }
}

internal sealed class SliRehearsalState
{
    public const string DreamGameMode = "dream-game";
    public const string IdentitySealed = "identity-sealed";
    public const string PreAdmissible = "pre-admissible";

    public bool IsConfigured { get; set; }
    public string RehearsalHandle { get; private set; } = string.Empty;
    public string SourceLocalityHandle { get; private set; } = string.Empty;
    public string Mode { get; set; } = DreamGameMode;
    public string IdentitySeal { get; set; } = IdentitySealed;
    public string AdmissionStatus { get; set; } = PreAdmissible;
    public bool IsBindable { get; set; }
    public List<string> BranchSet { get; } = [];
    public List<SliRehearsalSubstitutionState> SubstitutionLedger { get; } = [];
    public List<SliRehearsalAnalogyState> AnalogyLedger { get; } = [];
    public List<string> Warnings { get; } = [];
    public List<HigherOrderLocalityResidue> Residues { get; } = [];

    public void Reset()
    {
        IsConfigured = false;
        RehearsalHandle = string.Empty;
        SourceLocalityHandle = string.Empty;
        Mode = DreamGameMode;
        IdentitySeal = IdentitySealed;
        AdmissionStatus = PreAdmissible;
        IsBindable = false;
        BranchSet.Clear();
        SubstitutionLedger.Clear();
        AnalogyLedger.Clear();
        Warnings.Clear();
        Residues.Clear();
    }

    public void Configure(string sourceLocalityHandle, string mode)
    {
        Reset();
        IsConfigured = true;
        SourceLocalityHandle = sourceLocalityHandle;
        Mode = mode;
        RehearsalHandle = $"{sourceLocalityHandle}:rehearsal:{mode}";
    }
}

internal sealed class SliWitnessState
{
    public const string NonCandidate = "non-candidate";
    public const string Comparable = "comparable";
    public const string MorphismCandidate = "morphism-candidate";
    public const double DefaultGlueThreshold = 0.75;

    public bool IsConfigured { get; set; }
    public string WitnessHandle { get; private set; } = string.Empty;
    public string LeftLocalityHandle { get; private set; } = string.Empty;
    public string RightLocalityHandle { get; private set; } = string.Empty;
    public List<string> PreservedInvariants { get; } = [];
    public List<string> DifferenceSet { get; } = [];
    public double GlueThreshold { get; set; } = DefaultGlueThreshold;
    public string CandidacyStatus { get; set; } = NonCandidate;
    public List<string> Warnings { get; } = [];
    public List<HigherOrderLocalityResidue> Residues { get; } = [];

    public void Reset()
    {
        IsConfigured = false;
        WitnessHandle = string.Empty;
        LeftLocalityHandle = string.Empty;
        RightLocalityHandle = string.Empty;
        PreservedInvariants.Clear();
        DifferenceSet.Clear();
        GlueThreshold = DefaultGlueThreshold;
        CandidacyStatus = NonCandidate;
        Warnings.Clear();
        Residues.Clear();
    }

    public void Configure(string leftLocalityHandle, string rightLocalityHandle)
    {
        Reset();
        IsConfigured = true;
        LeftLocalityHandle = leftLocalityHandle;
        RightLocalityHandle = rightLocalityHandle;
        WitnessHandle = $"{leftLocalityHandle}=>{rightLocalityHandle}:witness";
        CandidacyStatus = Comparable;
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
    public SliRehearsalState Rehearsal { get; } = new();
    public SliWitnessState Witness { get; } = new();

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
        Rehearsal.Reset();
        Witness.Reset();
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
