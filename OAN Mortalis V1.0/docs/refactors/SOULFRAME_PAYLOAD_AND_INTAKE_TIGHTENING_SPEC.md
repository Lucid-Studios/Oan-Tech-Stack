# SOULFRAME_PAYLOAD_AND_INTAKE_TIGHTENING_SPEC

**Status:** In progress

**Anchoring Implementation:** `src/SoulFrame.Host/SoulFrameHostClient.cs`

**Related Authority Sources:**

- `docs/SYSTEM_ONTOLOGY.md`
- `docs/STACK_AUTHORITY_AND_MUTATION_LAW.md`
- `docs/PRIME_CRYPTIC_DATA_TOPOLOGY.md`
- `docs/refactors/CRYPTIC_CUSTODY_SOULFRAME_MEMBRANE_SPEC.md`
- `docs/refactors/FIRST_MEMBRANE_CALLER_RULES.md`

## Purpose

This specification narrows SoulFrame payloads, intake contracts, and call directions so SoulFrame remains a lawful membrane rather than drifting into a general-purpose execution or storage layer.

The governing test for every change is:

**Does this make SoulFrame a better membrane, or does it make SoulFrame a blob?**

SoulFrame exists to mediate self-state between sovereign Cryptic custody and AgentiCore working cognition.

It does **not** exist to become:

- a sovereign custody store
- a general orchestration fabric
- a Prime derivative publication service
- a broad runtime convenience layer

## Constitutional Position

SoulFrame is the **self-state membrane and bounded cognition mediation layer**.

Therefore SoulFrame may:

- project mitigated self-state outward for active cognition use
- accept narrowed return material for governed re-engrammitization preparation
- shape, filter, and constrain operational self-state exchange
- enforce lawful call boundaries between sovereign custody and worker cognition

SoulFrame may not:

- originate sovereign identity-bearing inscription
- act as canonical identity root
- expose protected Cryptic custody directly
- perform broad service orchestration
- publish Prime derivative outputs as a public or release layer

## Scope

This pass is limited to three concerns only:

1. `ISelfStateProjection` narrowing
2. `SoulFrameReturnIntakeRequest` narrowing
3. allowed call-direction tightening around the current membrane path

This pass does **not** redesign:

- CradleTek orchestration
- Cryptic custody internals
- Prime publication surfaces
- full AgentiCore runtime behavior
- general project restructuring

## Design Law

### Law 1. Projection Is For Use, Not Sovereignty

Anything carried by `ISelfStateProjection` must be sufficient for bounded worker cognition, but insufficient to act as sovereign Cryptic custody.

### Law 2. Intake Is For Return Evaluation, Not Write-Back

Anything carried by `SoulFrameReturnIntakeRequest` must represent candidate return material for governed Cryptic-side re-engrammitization, not a raw or implied direct write-back into custody.

### Law 3. SoulFrame Mediates; It Does Not Orchestrate

Call paths through SoulFrame must be limited to membrane acts:

- projection
- intake
- mitigation
- shaping
- handoff

Broad lifecycle control, swarm routing, and execution composition belong to CradleTek.

### Law 4. SoulFrame Does Not Publish Prime

SoulFrame may shape or hand off state for downstream processing, but it must not itself become the derivative publication authority.

### Law 5. Cryptic Remains Upstream

SoulFrame may consume governed source-domain inputs from Cryptic custody and may hand governed return candidates back toward Cryptic re-engrammitization gates, but it may not impersonate custody or store canonical sovereign state.

## `ISelfStateProjection` Contract Narrowing

### Allowed Contents

`ISelfStateProjection` may contain only mitigated working-state material required for active cognition.

Allowed categories:

- session-scoped self-state identifiers that are non-sovereign and non-canonical
- bounded role or state descriptors needed for current cognition execution
- mitigated symbolic or operational context required by AgentiCore
- ephemeral continuity hints suitable for worker cognition
- narrowed task-local memory references or handles
- safety, scope, or membrane metadata indicating limits of use
- provenance markers indicating the projection is derived from SoulFrame mediation rather than sovereign source ownership

### Disallowed Contents

`ISelfStateProjection` must not contain:

- raw sovereign custody records
- canonical Cryptic identity-bearing structures
- full `OE`, `SelfGEL`, `cSelfGEL`, or `cOE` custody objects
- unrestricted ledger state
- direct mutation tokens or authority grants over Cryptic custody
- broad plane-store handles
- Prime publication payloads or release-facing view models
- orchestration directives that belong to CradleTek
- telemetry aggregates that imply custody reconstruction
- enough raw material to reconstruct protected Cryptic state in full

### Practical Test

If the projection could be used to:

- replace `MoS` or `cMoS`
- reconstruct protected custody state
- govern Cryptic mutation directly

then it is too broad and must be narrowed.

## `SoulFrameReturnIntakeRequest` Contract Narrowing

### Allowed Contents

`SoulFrameReturnIntakeRequest` may contain only candidate return material required to evaluate whether a new governed Cryptic-side Engrammitization act should occur.

Allowed categories:

- session identifier and bounded origin metadata
- return provenance showing which worker cognition path produced the material
- narrowed operational residue suitable for evaluation
- summary deltas or interpreted outcome fragments
- candidate memory or self-state observations
- drift, conflict, or stability markers relevant to re-engrammitization review
- explicit intake intent metadata stating that the payload is a return candidate, not direct custody mutation
- references to governed review or gate pathways

### Disallowed Contents

`SoulFrameReturnIntakeRequest` must not contain:

- direct custody overwrite payloads
- raw `MoS` or `cMoS` write instructions
- direct mutation authority over sovereign records
- repository or store handles
- orchestration instructions
- Prime derivative publication artifacts
- full unfiltered runtime dumps
- telemetry blobs masquerading as identity input
- any field whose semantics imply “apply this directly to Cryptic state”

### Practical Test

If the intake request looks like:

- a storage command
- a direct patch operation
- an implicit state replacement object

then it violates membrane law.

Return intake must be a **candidate for governed evaluation**, not a sovereign action in disguise.

## Allowed Call Directions

### Lawful Directions

These directions are allowed:

- Cryptic custody or gate -> SoulFrame membrane
- SoulFrame membrane -> AgentiCore worker cognition
- AgentiCore worker cognition -> SoulFrame return intake
- SoulFrame return intake -> Cryptic re-engrammitization gate
- SoulFrame membrane -> downstream bounded consumer through lawful narrowed contracts

### Forbidden Directions

These directions are forbidden:

- AgentiCore -> direct Cryptic custody mutation
- Prime derivative publisher or view -> direct Cryptic custody access
- CradleTek orchestration -> direct use of SoulFrame as a broad runtime state store
- SoulFrame -> direct Prime publication authority
- Prime derivative surfaces -> direct reconstitution of Cryptic source objects
- SoulFrame -> generic swarm or orchestration control that belongs to CradleTek

## First Narrowing Targets

This pass should target the smallest current paths that most directly express membrane law.

### Target 1. `SoulFrameHostClient` projection payload

Review the current membrane projection path and remove any fields that are:

- custody-shaped
- orchestration-shaped
- derivative-publication-shaped
- broader than AgentiCore requires for bounded operation

### Target 2. Return intake request shape

Introduce or tighten `SoulFrameReturnIntakeRequest` so it clearly represents:

- return candidate material
- provenance
- narrowed operational residue
- governed handoff intent

and not:

- direct patch instructions
- broad state replacement
- hidden custody mutation

### Target 3. Call-site narrowing

Identify current callers that treat SoulFrame as:

- a broad state owner
- an execution manager
- a convenience facade for custody or publication access

and narrow them so they only invoke:

- projection
- intake
- handoff
- mitigation

## Suggested Shape Rules

### Projection Shape Rule

Projection models should be:

- bounded
- explicit
- session-scoped
- provenance-aware
- reconstructively weak

### Intake Shape Rule

Return intake models should be:

- candidate-oriented
- reviewable
- append or evaluate friendly
- non-authoritative
- non-overwriting

### Naming Rule

Names should state act and posture clearly.

Prefer names like:

- `ProjectedSelfState`
- `MembraneScopedContext`
- `ReturnIntakeCandidate`
- `ReengrammitizationCandidate`

Avoid names like:

- `FullSelfState`
- `IdentitySnapshot`
- `PlaneState`
- `ApplyStateRequest`

if those names imply sovereignty or broad ownership.

## Anti-Blob Checks

A change fails this pass if it causes SoulFrame to accumulate any of the following:

- storage ownership
- sovereign identity authorship
- broad orchestration logic
- Prime publication responsibilities
- telemetry accumulation beyond membrane needs
- generic execution lifecycle control
- direct plane-wide convenience APIs

If a proposed addition makes SoulFrame more globally useful but less constitutionally specific, it is probably blob growth and should be rejected.

## Acceptance Criteria

This pass is complete when:

- `ISelfStateProjection` is explicitly narrowed to mitigated worker-cognition payloads
- `SoulFrameReturnIntakeRequest` is explicitly narrowed to governed return candidates
- no projection payload contains direct sovereign custody structures
- no intake payload implies direct Cryptic write-back
- current SoulFrame call paths are limited to membrane acts
- SoulFrame does not acquire CradleTek orchestration responsibilities
- SoulFrame does not acquire Prime derivative publication responsibilities
- build, test, and hygiene all pass
- any changed docs are updated to reflect the narrowed membrane posture

## Completion Condition

SoulFrame is successfully tightened when the code makes this statement true:

**SoulFrame mediates self-state between sovereign custody and worker cognition without becoming custody, orchestration, or publication itself.**

## Current Implementation Status

The first narrowing cut has landed in code:

- `ISelfStateProjection` no longer exposes source custody domain or broad operational envelope fields
- `SelfStateProjection` now carries session-scoped and reconstructively weaker membrane fields
- `SoulFrameReturnIntakeRequest` now uses session handle, provenance marker, return-candidate pointer, and intake intent semantics
- `SoulFrameHostClient` now emits narrowed projection and intake payloads aligned to membrane acts
- membrane tests now assert projection and intake remain local membrane acts rather than transport-backed custody or publication paths
- there are no downstream membrane callers yet, so the next expansion risk is future caller widening rather than current caller misuse

Remaining work:

- review downstream callers as SoulFrame membrane usage expands
- keep future payload additions bounded to worker cognition and governed return evaluation only
- enforce the first-caller rule set before any AgentiCore or CradleTek integration widens handle semantics
- keep the first AgentiCore caller narrow until later cognition-pipeline integration is constitutionally justified
