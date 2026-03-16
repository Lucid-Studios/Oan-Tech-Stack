# Actionable Content Contract

## Purpose

This note defines the first executable control-plane lineage seam for the active return and collapse path.

It exists to make actionable content, governed request envelopes, and mutation receipts explicit without collapsing local domain meaning into one generic contract family.

## Boundary

This note does not:

- replace local domain records with one flat control-plane packet
- give CradleTek, C#, or the transit spine semantic authorship
- allow hot symbolic output to mutate governance directly
- make `.hopng` evidence constitutive authority

This note does:

- define what counts as actionable content in this pass
- define how governed request envelopes are formed
- define how mutation receipts correlate lawful control-surface changes
- state that existing domain records remain the meaning-bearing surface while shared contracts become the lineage-bearing surface

## Root Statement

The shared contracts are authoritative for correlation and audit lineage, while the existing domain records remain authoritative for local domain meaning until a later convergence pass.

## Shared Contract Family

The first shared lineage family lives in `src/Oan.Common` and includes:

- `GovernedActionableContent`
- `GovernedControlSurfaceRequestEnvelope`
- `GovernedControlSurfaceMutationReceipt`
- `ControlSurfaceContractGuards`

The shared family is additive in this pass.

## Actionable Content Kinds

The bounded kinds are:

- `ReturnCandidate`
- `CollapseCandidate`
- `PrimeDerivativeCandidate`

The current executable landing is limited to the existing return and collapse seam.

## Control Surfaces

The bounded target surfaces are:

- `SoulFrameReturnIntake`
- `StewardReturnReview`
- `GovernanceDecision`
- `GovernanceAct`

## Legality Matrix

The current legality matrix is:

- `ReturnCandidate` may target `SoulFrameReturnIntake`
- `ReturnCandidate` may target `StewardReturnReview`
- `CollapseCandidate` may target `StewardReturnReview`
- `CollapseCandidate` may target `GovernanceDecision`
- `CollapseCandidate` may target `GovernanceAct`
- `PrimeDerivativeCandidate` is not admitted on this return and collapse seam in this pass

Structurally valid but doctrinally nonsense envelopes are therefore unlawful.

## Return-Lane Landing

The first executable landing is:

1. AgentiCore forms a `ReturnCandidate`
2. the bounded membrane submits it through a `SoulFrameReturnIntake` envelope
3. SoulFrame validates parity between the legacy request and the shared envelope
4. governance work preserves the same actionable content into the Steward review envelope
5. Steward emits a mutation receipt on governance decision
6. CradleTek emits mutation receipts on downstream governance acts

The line of authority remains unchanged:

- the target runtime or hot lane may propose
- the membrane may validate and witness
- governance may authorize, refuse, or defer

## Parity Law

The shared envelope must remain parity-consistent with the legacy domain record.

For the current return seam, parity includes:

- legacy return pointer equals actionable content handle
- legacy source theater equals actionable origin surface
- legacy provenance marker equals actionable provenance marker
- collapse-classification source subsystem equals actionable source subsystem

Parity failure is lawful refusal.

## Mutation Receipt Law

Meaningful control-surface change must emit an explicit mutation receipt.

The first bounded outcomes are:

- `Authorized`
- `Refused`
- `Deferred`
- `Quarantined`
- `NoOp`

In the active return path:

- approved governance decision maps to `Authorized`
- rejected governance decision maps to `Refused`
- deferred governance decision maps to `Deferred`
- failed downstream acts map to `Refused`

## Constitutional Relation

This note implements the maxims already stated elsewhere:

> Transit is not authority.
>
> Carriage is not mutation.
>
> Passage is not admission.

The shared contracts do not weaken that law.

They make it executable.

## HOPNG Relation

`.hopng` evidence may point to actionable content handles, request envelope identifiers, and mutation receipt identifiers.

It may witness that lineage.

It may not replace the lineage.

## First Code Surface

The first executable surface is the active return and governance lane through:

- `src/Oan.Common`
- `src/AgentiCore/Services/BoundedMembraneWorkerService.cs`
- `src/SoulFrame.Host/SoulFrameHostClient.cs`
- `src/EngramGovernance/Services/StewardAgent.cs`
- `src/Oan.Cradle/StackManager.cs`

## Compression Line

Actionable content may move through the stack only as a governed candidate, never as a self-authorizing mutation.
