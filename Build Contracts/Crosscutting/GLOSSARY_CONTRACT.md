# Glossary Contract: OAN Mortalis v1.0

## Purpose

This document is the canonical terminology contract for the active `OAN Mortalis V1.0` build.

It exists to reduce build drift caused by:

- duplicate names for the same layer
- acronym casing drift
- protocol versus implementation confusion
- documentation that uses related terms as if they were interchangeable

This glossary is normative for:

- active architecture documents
- project classification and readiness documents
- new project names, namespaces, and host descriptions
- contributor-facing design and build language

## Status Model

Each term in this contract is classified as one of:

- `canonical`: preferred active term
- `compatibility`: allowed for legacy or migration surfaces only
- `deprecated`: should not be used for new active surfaces
- `prohibited conflation`: terms that must not be treated as equivalent

## Canonical Terms

### OAN

- Status: `canonical`
- Meaning: the product and governance stack name, `OAN Mortalis`
- Usage:
  - use `OAN Mortalis` in product-facing and repository-facing prose
  - use `Oan.*` for umbrella stack composition and stack-level contract ownership
  - do not treat `Oan.*` as the required replacement prefix for every family in the stack
- Notes:
  - `OAN` is an acronym-form product identifier
  - `Oan` is the preferred CLR namespace casing for the active codebase

### Spinal

- Status: `canonical`
- Meaning: the deterministic substrate and base contract layer
- Current canonical assembly: `Oan.Spinal`
- Usage:
  - use `Spinal` when referring to the active deterministic kernel or substrate
  - use `Oan.Spinal` for the base managed assembly
- Do not collapse with:
  - `Core`
  - `Cryptic`
  - persistence infrastructure

### Core

- Status: `compatibility`
- Meaning: legacy base-contract naming used by `OAN.Core`
- Usage:
  - keep only where required for legacy or compatibility assemblies
  - do not introduce new active root projects named `*.Core` when they mean the Spinal layer

### SLI

- Status: `canonical`
- Meaning: `Symbolic Language Interconnect`, the protocol specification for deterministic symbolic governance
- Current canonical assembly: `Oan.Sli`
- Usage:
  - use `SLI` for the protocol name in prose and design language
  - use `Oan.Sli` for the active managed implementation assembly
- SLI defines:
  - canonicalization discipline
  - transform trace discipline
  - evaluation membrane behavior
  - symbolic routing and governance semantics

### Lisp

- Status: `compatibility`
- Meaning: the current AST or symbolic representation form used to express SLI
- Usage:
  - use only when referring to the concrete representation, parser, or transform form
  - do not use `Lisp` as a synonym for `SLI`

### SoulFrame

- Status: `canonical`
- Meaning: the final enforcement and governance authority before critical halt
- Current active families:
  - `Oan.SoulFrame`
  - `SoulFrame.*`
- Usage:
  - use `SoulFrame` for authority, governance, and safe-fail boundary language
  - use `Oan.SoulFrame` for stack-level or umbrella-governance surfaces
  - use `SoulFrame.*` for family-owned operator and identity-facing workflow surfaces

### AgentiCore

- Status: `canonical`
- Meaning: cognition-facing identity and engram management surface
- Current active families:
  - `Oan.AgentiCore`
  - `AgentiCore.*`
- Usage:
  - keep the term `AgentiCore`
  - use `Oan.AgentiCore` for stack-level or umbrella integration surfaces
  - use `AgentiCore.*` for family-owned agent runtime behavior

### Cradle

- Status: `canonical`
- Meaning: active host orchestration and runtime mediation
- Current active families:
  - `Oan.Cradle`
  - `CradleTek.*`
- Usage:
  - use `Cradle` for stack-level orchestration concepts
  - keep one canonical composition root under `Oan.Runtime.Headless`

### CradleTek

- Status: `canonical`
- Meaning: infrastructure and substrate family for host-oriented runtime components
- Usage:
  - use for infrastructure, substrate, hosting, storage, and low-level runtime services
  - do not treat `CradleTek.*` as a legacy family by default

### Place

- Status: `canonical`
- Meaning: module boundary and external logic placement layer
- Current canonical assembly: `Oan.Place`
- Usage:
  - use when referring to placement or external module boundary concerns

### Storage

- Status: `canonical`
- Meaning: persistence adapter layer
- Current canonical assembly: `Oan.Storage`
- Usage:
  - use for persistence implementations and storage adapters
  - do not use as a synonym for `Cryptic`

### Cryptic

- Status: `canonical`
- Meaning: append-only fingerprint persistence and cryptic operational surface
- Usage:
  - use when referring to the cryptic operational layer
  - do not treat as identity, authority, or the base deterministic substrate

### Public

- Status: `canonical`
- Meaning: public operational output layer
- Usage:
  - use when referring to public-facing or externalized operational outputs
  - do not treat as required for survival of the deterministic substrate

### Mantle

- Status: `compatibility`
- Meaning: governance or sovereignty-oriented domain surface in the legacy family
- Usage:
  - keep only with explicit context
  - define the exact responsibility where used; the bare term is too broad on its own

### GEL

- Status: `compatibility`
- Meaning: project-specific acronym currently used in telemetry and domain surfaces
- Usage:
  - glossary-own the full expansion before using the acronym in new active docs
  - do not assume readers know the expansion from context
- Policy:
  - until expanded in an owning contract, treat `GEL` as compatibility terminology

### FGS

- Status: `compatibility`
- Meaning: project-specific acronym currently represented by `Oan.Fgs`
- Usage:
  - provide a full definition before using in new architectural prose
  - prefer the expanded phrase when it becomes stable
- Policy:
  - until expanded in an owning contract, treat `FGS` as transitional terminology

## Prohibited Conflations

The following equivalences are forbidden in active documentation, code review language, and design notes:

- `SLI == Lisp`
- `Spinal == Core`
- `Spinal == Cryptic`
- `Cryptic == identity`
- `Public == required for survival`
- `SoulFrame == optional governance`
- `Cradle == composition root`

Clarification for the last rule:

- `Cradle` is an orchestration layer
- `Oan.Runtime.Headless` is the current canonical composition root

## Namespace And Naming Policy

### Namespace Families

- Umbrella composition family:
  - `Oan.*`
- Active owned families:
  - `CradleTek.*`
  - `SoulFrame.*`
  - `AgentiCore.*`
  - `SLI.*`
- Compatibility families or roots:
  - `OAN.*`
  - unexplained bare roots such as `GEL`

### Project Naming

- New stack-level composition projects should prefer `Oan.*`
- New family-owned projects should use their owning family prefix
- New active host entrypoints should not create alternate stack roots alongside `Oan.Runtime.Headless`
- compatibility projects should be described as compatibility or migration surfaces in documentation when they do not fit the constitutional family model

### Acronym Casing

- Product acronym in prose: `OAN`
- Managed namespace root: `Oan`
- Protocol acronym in prose: `SLI`
- Assembly name for active managed protocol layer: `Oan.Sli`

## Data And Syntax Notation Policy

### CLR Naming

- Types: PascalCase
- Members: PascalCase
- Namespaces: family-qualified PascalCase segments

### Runtime And Telemetry Identifiers

- use lower dotted identifiers where service ids are needed
- examples:
  - `cradletek.host`
  - `agenticore.runtime`

### Canonical JSON

- interchange JSON keys should use `snake_case`
- optional fields should be omitted rather than emitted as `null` unless a contract explicitly requires nullability

### NDJSON

- use minified JSON
- use UTF-8
- use `\n` newlines only
- treat emission as append-only where the contract requires NDJSON persistence

## Build Maintenance Rules

- if a new term appears in architecture or runtime docs and it is not defined here or in an owning contract, add or reference a definition before treating it as canonical
- if a compatibility term is retained, document what canonical active surface it maps to
- if a project name and its test suite imply different architecture families, document that mismatch explicitly

## Reference Documents

- `Build Contracts/Crosscutting/ARCHITECTURE_FRAME.md`
- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/NAMING_CONVENTION.md`
- `OAN Mortalis V1.0/docs/PROJECT_CLASSIFICATION_MATRIX.md`
- `OAN Mortalis V1.0/docs/BUILD_READINESS.md`

## Current Canonical Family Model

The current preferred family model is:

- `Oan.*` for umbrella composition and stack-level contracts
- `CradleTek.*` for infrastructure and substrate ownership
- `SoulFrame.*` for operator and identity-facing workflow ownership
- `AgentiCore.*` for agent runtime ownership
- `SLI.*` for symbolic protocol and runtime ownership across the stack

Project-specific placement should follow the owning family rather than forcing all active code into `Oan.*`.
