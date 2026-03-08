# ATLAS_SOURCE_LAYERING

## Purpose

This note records the first bounded Atlas-source layering rule for the active `OAN Mortalis V1.0` build.

It exists to keep the repo truthful about the difference between:

- the canonical external Atlas ingest target
- the supporting source layers that reconcile root identity, constructor legality, and symbol policy

This is a local-only source normalization concern.

It is not:

- a new runtime dependency
- a Golden Path rewrite
- a custody/governance change
- a public/shared memory mutation path

## Canonical External Ingest Target

The canonical external ingest target is the normalized root-to-variant Atlas surface.

In the current source-layer model, that means:

- `RootAtlas` is the primary ingest object

The repository now has a bounded source normalizer that can turn that atlas surface into the canonical `GEL.Contracts.RootAtlas` object.

## Supporting Source Layers

The supporting source layers remain distinct and subordinate to the canonical Atlas ingest target.

They are used for reconciliation and law checks:

- root symbol identity consistency
- prefix constructor legality
- suffix constructor legality
- reserved symbol collision policy

The intended layering is:

- root symbol seed layer
  - direct root-to-symbol assignment surfaces
- constructor and operator layer
  - prefix grammar
  - suffix grammar
- symbol constitution layer
  - reserved control/meta symbols
  - uniqueness and collision rules
- canonical atlas layer
  - root
  - variants
  - symbolic constructor triplets
  - stable canonical identity
- symbolic governance layer
  - reserved grammar operators
  - atlas-owned root-native cores
  - reserved disciplinary domains
  - governance/meta symbols
  - experimental extension space

## Normalization Rule

The repo should treat the source layers in this order:

1. ingest the canonical Atlas source
2. reconcile root symbol identity against the root symbol seed layers
3. validate prefix/suffix constructor references
4. validate reserved symbol and collision policy
5. preserve canonical prefix-root-suffix constructor bodies
6. validate symbolic domain reservation and bridge policy
7. produce one canonical `RootAtlas`

The supporting source files are not co-equal meaning models.

They are law layers around the canonical Atlas surface.

## Runtime Boundary

This source-layer normalizer does not make external Atlas files part of the mandatory runtime path.

Current runtime behavior remains:

- the active runtime still uses the repo-local root atlas path already wired through `RootAtlasOntologicalCleaver`
- external Atlas sources are a bounded local normalization/validation seam

That boundary is intentional.

It lets the repo truthfully say:

- `RootAtlas` is the canonical external ingest target
- the runtime is not yet hard-wired to local external Atlas files

## Immediate Consequence

The stack now has the first truthful bridge from:

- source lexical-symbol layers
- constructor law layers
- symbol constitution layers

to:

- one canonical `RootAtlas` object in `GEL.Contracts`

That is the required bedrock for later Atlas -> Engram admission without rival Atlas meanings.

## Constructor Body Rule

The atlas now distinguishes between:

- lexical variant strings for compatibility and lookup
- canonical symbolic constructor triplets for meaning-bearing symbolic structure

The triplet is the canonical object.

The merged glyph, if present, is only a rendered artifact.

No symbolic contextualization generation should proceed from this Atlas layer unless the constructor body is structurally represented, not merely validated.

That rule now sits under:

- `docs/SYMBOLIC_DOMAIN_CONSTITUTION.md`
- `docs/KNOWN_UNKNOWN_EXTENSION_SEMANTICS.md`
