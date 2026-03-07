# FIRST_MEMBRANE_CALLER_RULES

**Status:** Active

**Scope:** First AgentiCore, CradleTek, or adjacent consumers of SoulFrame membrane contracts

**Anchored Contracts:**

- `src/Oan.Common/SoulFrameMembraneContracts.cs`
- `src/SoulFrame.Host/SoulFrameHostClient.cs`

**Related Authority Sources:**

- `docs/SYSTEM_ONTOLOGY.md`
- `docs/STACK_AUTHORITY_AND_MUTATION_LAW.md`
- `docs/PRIME_CRYPTIC_DATA_TOPOLOGY.md`
- `docs/refactors/SOULFRAME_PAYLOAD_AND_INTAKE_TIGHTENING_SPEC.md`

## Purpose

This document defines the first-caller rule set for SoulFrame membrane consumers.

The membrane has already been narrowed in code. The next drift risk is not SoulFrame widening itself. The next drift risk is a first real caller trying to recover the wider world that the membrane intentionally removed.

The governing test for every new caller is:

**Does this caller consume a membrane projection, or is it trying to recover the wider world that the membrane removed?**

## Core Law

The following statements are constitutional rules for every membrane consumer:

- `SessionHandle` is not custody access.
- `WorkingStateHandle` is not sovereign identity access.
- `SoulFrameReturnIntakeRequest` is not a patch or overwrite command.

Any caller that behaves as though one of those statements is false is out of law.

## Lawful Caller Behavior

A lawful caller may:

- consume bounded projection from `ISelfStateProjection`
- operate locally on mediated working state
- use provenance as audit context only
- submit narrowed return candidates with provenance and intent
- hand off return candidates for governed evaluation

A lawful caller may not:

- dereference handles into broad custody access
- infer source-domain authority from provenance markers
- treat `WorkingStateHandle` as a universal access token
- treat `SoulFrameReturnIntakeRequest` as direct re-engrammitization or write-back
- push orchestration duties into SoulFrame
- use SoulFrame as a publication or release surface

## Review Questions

Every first caller should be reviewed against these questions:

1. Does this caller try to infer or fetch source-domain details that the membrane no longer exposes?
2. Does this caller treat `SessionHandle` or `WorkingStateHandle` as latent broad authority?
3. Does this caller use provenance markers as if they were permission grants?
4. Does this caller assume intake means "apply changes" instead of "submit candidate return material"?
5. Does this caller push orchestration, lifecycle, or routing work back into SoulFrame?

Any "yes" answer is a stop condition until the caller is narrowed.

## Required Integration Sequence

The first real membrane caller should follow this order:

1. Add the caller against the narrowed membrane interface only.
2. Refuse helper methods that widen handle semantics.
3. Write tests proving the caller works with bounded handles alone.
4. Add at least one negative test proving the caller cannot use the membrane as custody or orchestration access.

## Negative-Test Requirements

The first caller is not complete without a negative test covering at least one of these failure modes:

- attempting to treat `SessionHandle` as broad store access
- attempting to treat `WorkingStateHandle` as sovereign identity state
- attempting to use intake as a patch or overwrite path
- attempting to route orchestration behavior through SoulFrame

## Acceptance Criteria

The first membrane caller is compliant when:

- it depends only on narrowed membrane contracts
- it works without reconstructing broad custody state
- it uses provenance as witness context, not authority
- it submits return candidates rather than write-back commands
- it does not push orchestration or publication work into SoulFrame
- positive and negative tests both pass

## Completion Condition

The first caller rule set is satisfied when the first real consumer proves this statement true:

**The caller can use SoulFrame-mediated state productively without treating the membrane as custody, orchestration, or publication authority.**

## Current Implementation Status

The first real membrane caller now exists in `src/AgentiCore/Services/BoundedMembraneWorkerService.cs`.

It currently proves:

- AgentiCore can consume `SessionHandle`, `WorkingStateHandle`, and `ProvenanceMarker` without recovering broad custody semantics
- AgentiCore can submit a `SoulFrameReturnIntakeRequest` only as a candidate return path
- the first caller is pinned behind one positive bounded-handle test and one negative forged-custody-handle test in `tests/Oan.Audit.Tests/AgentiCoreMembraneCallerTests.cs`
- the broader AgentiCore cognition flow can invoke the bounded worker as a stage without changing the worker contract, proven in `tests/Oan.Audit.Tests/AgentiCoreFlowMembraneIntegrationTests.cs`

This caller is intentionally narrow and is not yet a broad cognition-pipeline dependency.
