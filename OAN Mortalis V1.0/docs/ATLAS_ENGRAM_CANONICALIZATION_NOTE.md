# ATLAS_ENGRAM_CANONICALIZATION_NOTE

## Purpose

This note records the first canonical Atlas and Engram contract pass for the active `OAN Mortalis V1.0` build.

It exists to stop the repo from carrying multiple near-equal meaning models for Atlas, Engram, constructor output, and governance DTOs.

The canonical owner is now:

- `src/GEL.Contracts`

This phase does not introduce a new cognition subsystem, append engine, or OE/cOE mutation path.

## Canonical Owner

`GEL.Contracts` is now the only canonical owner of:

- `PredicateRoot`
- `PredicateRefinementEdge`
- `DomainDescriptor`
- `RootAtlasEntry`
- `RootAtlas`
- `EngramDraft`
- `Engram`
- `EngramTrunk`
- `EngramBranch`
- `EngramInvariant`
- `EngramClosureDecision`

`Oan.Spinal.EngramId` remains the canonical Engram identifier type.

`Oan.Spinal.EngramEnvelope` remains a runtime-event envelope and must not be treated as the canonical meaning object.

## Draft Vs Canonical Engram

The distinction is now explicit and non-optional:

- `EngramDraft`
  - pre-closure candidate meaning object
- `Engram`
  - normalized, closure-valid canonical meaning object
- `EngramClosureDecision`
  - validator result, warnings, and reason codes

The closure grades are:

- `BootstrapClosed`
- `Closed`
- `NeedsSpecification`
- `Rejected`

Meaning:

- branchless first-pass drafts may be `BootstrapClosed`
- fully specified closure is `Closed`
- unresolved but recoverable drafts are `NeedsSpecification`
- invalid drafts are `Rejected`

## Atlas Identity Discipline

`RootAtlas` is now treated as a pinned object, not an ambient collection.

Minimum identity requirements:

- stable version field
- deterministic canonical ordering for serialization
- deterministic digest/hash field
- validator always receives a `RootAtlas` object directly

No validator path may rely on heuristic best-match atlas resolution.

## Epistemic Mapping

The canonical epistemic mapping is:

- `Propositional` = Master
- `Procedural` = Legendary
- `Perspectival` = Mythic
- `Participatory` = Peerless

This mapping is now frozen in the active architecture docs.

## Adapter Rule

Existing runtime-facing types are still allowed, but only as adapters or downstream views:

- `CradleTek.Memory.RootEngram`
- `RootAtlasOntologicalCleaver`
- `SLI.Ingestion.ConstructorEngramBuilder`
- `EngramGovernance.EngramCandidate`
- `EngramGovernance.EngramRecord`

They must not grow into rival canonical Engram models.

## Validator Seam

The first bounded validator seam is:

- `GEL.Contracts.IEngramClosureValidator`

Implemented in:

- `src/EngramGovernance/Services/EngramClosureValidator.cs`

The validator may:

- resolve roots against canonical `RootAtlas`
- validate trunk, invariants, and branch references
- normalize closure grades
- emit warnings and reason codes

The validator may not:

- append to GEL
- mutate OE/cOE
- mutate custody
- rewrite Golden Path collapse or routing behavior

## Compatibility Note

`LocalSymbolAtlas` is now a compatibility shim over canonical `RootAtlas`.

It may still exist for legacy sheaf/runtime surfaces, but it must no longer be read as a competing atlas model.

## Immediate Consequence

The repo now says plainly:

- Atlas first
- then Engram draft
- then closure validation
- then only later governed memory movement

That is the canonical order for future Atlas -> Engram -> GEL / OE work.
