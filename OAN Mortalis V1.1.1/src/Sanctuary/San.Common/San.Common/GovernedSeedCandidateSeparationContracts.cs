using System.Collections.Generic;

namespace San.Common;

public enum GovernedSeedPrimeMaterialKind
{
    Unknown = 0,
    InvariantConcern = 1,
    AdmissionRelevantStructure = 2,
    ResponsibilityBearingMarker = 3,
    RoleDomainGatingSignal = 4
}

public enum GovernedSeedCrypticMaterialKind
{
    Unknown = 0,
    UnfinishedThought = 1,
    ResonanceGrouping = 2,
    PartialForm = 3,
    BloomResidue = 4,
    TraceResidue = 5,
    HoldWorthyConstruct = 6
}

public sealed record GovernedSeedPrimeMaterial(
    GovernedSeedPrimeMaterialKind Kind,
    string Summary);

public sealed record GovernedSeedCrypticMaterial(
    GovernedSeedCrypticMaterialKind Kind,
    string Summary);

public sealed record GovernedSeedPrimeCandidateView(
    string CandidateId,
    IReadOnlyList<GovernedSeedPrimeMaterial> PrimeMaterials);

public sealed record GovernedSeedCrypticCandidateView(
    string CandidateId,
    IReadOnlyList<GovernedSeedCrypticMaterial> CrypticMaterials);

public sealed record GovernedSeedCandidateSeparationAssessment(
    string CandidateId,
    bool SeparationSucceeded,
    bool CrypticAuthorityBleedDetected,
    bool PrimeMaterialPresent,
    bool CrypticMaterialPresent,
    string Summary);

public sealed record GovernedSeedCandidateSeparationReceipt(
    string CandidateId,
    bool SeparationSucceeded,
    int PrimeMaterialCount,
    int CrypticMaterialCount,
    bool CrypticAuthorityBleedDetected,
    string Summary);

public sealed record PrimeCrypticDuplexGovernanceReceipt(
    string CandidateId,
    bool PrimeSurfaceEstablished,
    bool CrypticSurfaceEstablished,
    string Summary);
