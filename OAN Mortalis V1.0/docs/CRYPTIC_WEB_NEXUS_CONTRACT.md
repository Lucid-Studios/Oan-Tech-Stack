# CRYPTIC_WEB_NEXUS_CONTRACT

## Purpose

This document defines the code-facing contact surface between office-bearing participation, host law, and the `SLI/Lisp` topological cognition field.

It is not an ontology document.

It is not a shelling law.

It is the implementation-near contract that answers:

- what the host may call
- what the host may witness
- what the field must expose
- what the nexus must explicitly refuse to become

This document is a companion to:

- `docs/SLI_TOPOLOGICAL_COGNITION_FIELD_AND_CRYPTIC_NEXUS.md`
- `docs/STACK_AUTHORITY_AND_MUTATION_LAW.md`

## Root Contract Sentence

The nexus contract exists to expose the state and transitions of the field for lawful observation and engagement.

It does not grant authority.

## Non-Authority Clause

The nexus contract must not silently absorb:

- admissibility
- persistence sovereignty
- office ratification
- legality judgment
- retention governance
- identity authorship

The nexus may expose state.

The nexus may expose transitions.

The nexus may expose witness receipts.

The nexus may expose bounded engagement surfaces.

The nexus may not self-authorize field outcomes into legally admitted stack reality.

Short rule:

> Contact is not authority.

## Contract Family

The initial contract family should be expressed through a bounded set of code-facing types.

Representative names:

- `ICrypticWebNexus`
- `WebTopologySnapshot`
- `MutationEvent`
- `RelaxationReceipt`
- `NexusTelemetryFrame`

These names may evolve in code, but the contract roles they represent should remain stable.

## ICrypticWebNexus

### Purpose

`ICrypticWebNexus` is the lawful contact plane between:

- host law
- office-bearing participants
- the `SLI/Lisp` field

It must present one coherent bounded entry surface rather than a scattered set of incidental hooks.

### Responsibilities

The nexus interface should be able to:

- expose the current topology snapshot
- expose active mutation state or mutation events
- expose relaxation and readiness receipts
- expose orientation telemetry
- accept bounded requests for lawful field engagement where such requests are permitted

### Prohibitions

The nexus interface must not:

- directly decide admissibility
- directly persist identity-bearing authority surfaces by silent convenience
- masquerade as the host law
- masquerade as the field itself
- permit arbitrary office reach into random Lisp fragments

## WebTopologySnapshot

### Purpose

`WebTopologySnapshot` is the present condition surface of the field as field.

It should answer:

- what regions exist now
- what relations are active now
- what braid-capable boundaries are in play now
- what the current equilibrium or non-equilibrium markers are

### Minimum Shape

A topology snapshot should be able to carry at least:

- `snapshotId`
- `capturedAtUtc`
- `fieldState`
- `activeRegions`
- `activeRelations`
- `braidRegions`
- `equilibriumMarkers`
- `unresolvedStrain`

The exact type names may differ in code, but the semantic load should remain.

## MutationEvent

### Purpose

`MutationEvent` is the causal movement surface for deformation under work.

It exists so the stack can witness not only present condition, but also transition.

### Minimum Shape

A mutation event should be able to carry at least:

- `eventId`
- `occurredAtUtc`
- `originRegion`
- `affectedRegions`
- `mutationKind`
- `preservedIdentityConstraints`
- `strainDelta`
- `causalReason`
- `eventState`

Mutation events must preserve bounded identity witness rather than narrating change as free-floating transformation.

## RelaxationReceipt

### Purpose

`RelaxationReceipt` is the formal answer to whether the field has restabilized after mutation.

It exists to prevent the false assumption that intermediate execution success implies a ready field.

### Minimum Shape

A relaxation receipt should be able to carry at least:

- `receiptId`
- `capturedAtUtc`
- `sourceMutationIds`
- `relaxationState`
- `readyForReentry`
- `residualStrain`
- `boundaryIntegrityState`
- `reasonCode`

### Law

The host must be able to distinguish:

- active mutation without relaxation
- incomplete relaxation
- coherent dormancy
- ready equilibrium
- unresolved contradiction

## NexusTelemetryFrame

### Purpose

`NexusTelemetryFrame` is the orientation and witness surface for the field at a point in time.

It is not merely debug output.

It is part of the continuity witness by which office-bearing action may later be traced lawfully.

### Minimum Shape

A telemetry frame should be able to carry at least:

- `frameId`
- `capturedAtUtc`
- `focalRegion`
- `topologyState`
- `mutationIndex`
- `relaxationProgress`
- `readinessState`
- `orientationNotes`
- `reasonCode`

## Snapshot Versus Event

The contract must preserve the distinction between:

- current shaped state
- causal movement

Therefore:

- `WebTopologySnapshot` answers the present shape of the field
- `MutationEvent` answers what changed and how
- `RelaxationReceipt` answers whether change lawfully settled
- `NexusTelemetryFrame` answers how the nexus witnessed and oriented that state

If this distinction collapses, the field becomes harder to audit and easier to mythologize incorrectly.

## Office Engagement Law

An office-bearing participant such as a later lawful `Father` surface should engage the field through the nexus contract.

That means:

- office calls the nexus
- the nexus exposes field state and transitions
- host law witnesses and adjudicates admissibility

It must not mean:

- office directly crawls arbitrary field fragments
- office silently seizes topology authority
- office treats the nexus as a secret sovereign runtime

## Host Witness Law

The host may use nexus surfaces to:

- witness current field state
- witness field transitions
- witness readiness or non-readiness
- record receipts and telemetry
- decide what may be admitted, deferred, suspended, or refused

The host may not use nexus surfaces to:

- rewrite the meaning of lawful target-side field results
- pretend that witness is equivalent to authorship
- quietly mutate the field as if the nexus were a host-owned adapter

## Implementation Consequence

Before more isolated `Lisp` helpers are added, the codebase should introduce the bounded nexus-facing types that let the field appear as a first-class structure.

Until that happens, the `SLI/Lisp` layer will continue to appear smaller and more fragmented than the architecture actually requires.
