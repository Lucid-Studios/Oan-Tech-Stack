using System;
using System.Collections.Generic;

namespace San.Common;

/// <summary>
/// Marker for all Lisp-originated or cryptic-originated candidate surfaces.
/// These surfaces may inform host inspection, but they never carry authority.
/// </summary>
public interface IGovernedSeedCandidateProposal
{
    string ProposalId { get; }
    string ProposalKind { get; }
    string Summary { get; }
}

public interface IGovernedSeedCrypticHoldingMutationProposal
{
    string MutationId { get; }
    string MutationKind { get; }
    string Summary { get; }
}

public interface IGovernedSeedResonanceObservation
{
    string ObservationId { get; }
    string ObservationKind { get; }
    string Summary { get; }
}

public interface IGovernedSeedDescendantProposal
{
    string DescendantId { get; }
    string DescendantKind { get; }
    string Summary { get; }
}

public interface IGovernedSeedCollapseSuggestion
{
    string SuggestionId { get; }
    string SuggestionKind { get; }
    string Summary { get; }
}

public enum GovernedSeedCandidateSourceType
{
    Unknown = 0,
    LispProposal = 1,
    HostGenerated = 2,
    SyntheticTest = 3
}

/// <summary>
/// Candidate-only envelope. This is the only shape by which proposal-bearing
/// material may enter the host loop from cryptic/proposal surfaces.
/// </summary>
public sealed record GovernedSeedCandidateEnvelope(
    string CandidateId,
    GovernedSeedCandidateSourceType SourceType,
    string SourceChannel,
    DateTimeOffset ObservedAtUtc,
    string? PriorContinuityReference,
    IReadOnlyList<IGovernedSeedCandidateProposal> CandidateProposals,
    IReadOnlyList<IGovernedSeedCrypticHoldingMutationProposal> HoldingMutationProposals,
    IReadOnlyList<IGovernedSeedResonanceObservation> ResonanceObservations,
    IReadOnlyList<IGovernedSeedDescendantProposal> DescendantProposals,
    IReadOnlyList<IGovernedSeedCollapseSuggestion> CollapseSuggestions)
{
    public static GovernedSeedCandidateEnvelope Empty(
        string candidateId,
        GovernedSeedCandidateSourceType sourceType,
        string sourceChannel,
        DateTimeOffset observedAtUtc,
        string? priorContinuityReference = null) =>
        new(
            candidateId,
            sourceType,
            sourceChannel,
            observedAtUtc,
            priorContinuityReference,
            Array.Empty<IGovernedSeedCandidateProposal>(),
            Array.Empty<IGovernedSeedCrypticHoldingMutationProposal>(),
            Array.Empty<IGovernedSeedResonanceObservation>(),
            Array.Empty<IGovernedSeedDescendantProposal>(),
            Array.Empty<IGovernedSeedCollapseSuggestion>());
}

/// <summary>
/// Receipt proving that a candidate entered the host as candidate-only
/// material, without any authority-bearing promotion at intake.
/// </summary>
public sealed record GovernedSeedCandidateBoundaryReceipt(
    string ReceiptHandle,
    string CandidateId,
    GovernedSeedCandidateSourceType SourceType,
    string SourceChannel,
    DateTimeOffset ObservedAtUtc,
    bool ContainsAuthorityBearingFields,
    int CandidateProposalCount,
    int HoldingMutationProposalCount,
    int ResonanceObservationCount,
    int DescendantProposalCount,
    int CollapseSuggestionCount,
    string Summary);
