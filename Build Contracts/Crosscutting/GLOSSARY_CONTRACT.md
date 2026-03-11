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

The terms below are contract entries, not just prose definitions.

Recommended governance fields for important terms:

- layer
- governance domain
- authority relation
- identity relation
- persistence type
- mutation rule
- audit status
- commit eligibility
- cross-boundary restrictions

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

Contract fields:
- Layer: stack-wide
- Governance domain: constitutional whole
- Authority relation: umbrella constitutional identity
- Identity relation: system identity
- Persistence type: documentary and architectural
- Mutation rule: changes only by explicit constitutional revision

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
- Meaning: `Symbolic Language Interconnect`, the protocol and native morphological Engrammitization surface for deterministic symbolic governance and Engineered Cognition
- Current canonical assembly: `Oan.Sli`
- Usage:
  - use `SLI` for the protocol name in prose and design language
  - use `Oan.Sli` for the active managed implementation assembly
- SLI defines:
  - canonicalization discipline
  - transform trace discipline
  - evaluation membrane behavior
  - symbolic routing and governance semantics
  - native candidate Engrammitization inside bounded runtime cognition

Contract fields:
- Layer: symbolic
- Governance domain: cross-stack transport, gate law, and bounded native Engrammitization
- Authority relation: transport-authoritative and morphologically active, non-identity-authoritative
- Identity relation: identity-neutral
- Persistence type: packet, evidence, gate, tensor, routing, symbolic trace, and candidate Engrammitization artifacts
- Mutation rule: may transform, route, and native-Engrammitize inside bounded runtime cognition; may not directly commit identity by itself

### Lisp

- Status: `compatibility`
- Meaning: the current native representation and computational morphology used to express SLI and Engineered Cognition
- Usage:
  - use when referring to the concrete runtime representation, parser, transform form, or morphological execution space
  - do not use `Lisp` as a synonym for `SLI`

### SoulFrame

- Status: `canonical`
- Meaning: the CME-specific mediated self-state constructor and governance membrane before critical halt
- Current active families:
  - `Oan.SoulFrame`
  - `SoulFrame.*`
- Usage:
  - use `SoulFrame` for authority, governance, and safe-fail boundary language
  - use `Oan.SoulFrame` for stack-level or umbrella-governance surfaces
  - use `SoulFrame.*` for family-owned operator and identity-facing workflow surfaces

Contract fields:
- Layer: governance and contextual mediation
- Governance domain: legality, admissibility, session posture
- Authority relation: legality-authoritative
- Identity relation: identity-coupled, not identity-bearing
- Persistence type: decision, receipt, and enforcement state
- Mutation rule: may gate and refuse; must not directly author identity-bearing state

### AgentiCore

- Status: `canonical`
- Meaning: bounded personification, actualization, and operational cognition surface
- Current active families:
  - `Oan.AgentiCore`
  - `AgentiCore.*`
- Usage:
  - keep the term `AgentiCore`
  - use `Oan.AgentiCore` for stack-level or umbrella integration surfaces
  - use `AgentiCore.*` for family-owned agent runtime behavior

Contract fields:
- Layer: operational cognition
- Governance domain: bounded mission execution, personified actualization, and working self-state formation
- Authority relation: proposal-authoritative, not direct shared-state-authoritative
- Identity relation: identity-bearing and identity-coupled
- Persistence type: runtime state, proposal artifacts, self-state formation
- Mutation rule: may propose and assemble self-state; may not directly commit GEL

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

Contract fields:
- Layer: runtime substrate
- Governance domain: hosting, isolation, resource law, protected stores
- Authority relation: runtime-authoritative
- Identity relation: identity-external
- Persistence type: infrastructure, mounts, cryptic custody, runtime services
- Mutation rule: may host and constrain; may not define identity

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
- Meaning: the live, unmasked, data-practice-governed side of the stack
- Usage:
  - use when referring to live protected data that remains subject to governance, masking law, retention law, and protected data practice
  - do not use merely as a synonym for hidden, archived, or encrypted
  - do not treat as identity, authority, or the base deterministic substrate

### Public

- Status: `canonical`
- Meaning: the processed, checked, and Prime-safe side of operational output
- Usage:
  - use when referring to data that has been processed and checked under data practice
  - Prime-safe data may be unmasked for public use or retained as encrypted or pointerized release form
  - do not treat as required for survival of the deterministic substrate

### Mantle

- Status: `compatibility`
- Meaning: governance or sovereignty-oriented domain surface in the legacy family
- Usage:
  - keep only with explicit context
  - define the exact responsibility where used; the bare term is too broad on its own

### GEL

- Status: `canonical`
- Meaning: `Governance Event Ledger`, also described in stack documents as the `Golden Engram Library`
- Usage:
  - use for the Prime-facing shared invariant, identity-bearing, append-only domain
  - do not use as a synonym for runtime state, OE, or cryptic staging

Contract fields:
- Layer: identity
- Governance domain: shared invariant memory
- Authority relation: authoritative shared domain
- Identity relation: identity-bearing
- Persistence type: append-only canonical store
- Mutation rule: governed append-only commit
- Audit status: mandatory
- Commit eligibility: yes, only through lawful governed commit
- Cross-boundary restrictions: must not receive silent writes from OE, telemetry, or ungated runtime paths

### cGEL

- Status: `canonical`
- Meaning: cryptic GEL-adjacent or protected GEL-resident domain within the Cryptic SLI side
- Usage:
  - use for protected, live, unmasked, cryptic-resident GEL-related material still subject to data practice
  - do not treat as automatically Prime-safe, release-safe, or pointer-safe

Contract fields:
- Layer: cryptic identity-adjacent
- Governance domain: protected cryptic custody
- Authority relation: protected, not automatically public-authoritative
- Identity relation: identity-adjacent
- Persistence type: protected cryptic storage
- Mutation rule: may evolve under cryptic governance; must not silently promote to GEL

### GoA

- Status: `canonical`
- Meaning: `Garden of Almost`, the Prime hosted service plane for public or shared operational context
- Notes:
  - older documentation may still expand `GoA` as `Global of Action`
  - for current constitutional work, `Garden of Almost` is the preferred active reading

Contract fields:
- Layer: standard operational context
- Governance domain: Prime hosted service context
- Authority relation: standard-plane operational domain
- Identity relation: identity-coupled, not primary identity anchor
- Persistence type: Prime service-plane operational context
- Mutation rule: may update from governed identity-bearing admissions

### cGoA

- Status: `canonical`
- Meaning: `Cryptic Garden of Almost`, the Cryptic pre-Engrammitization and protected processing plane
- Notes:
  - `cGoA` processes pre-Engrammitized data into engrams or compostable protected stores
  - it serves `GoA` and `cGEL` directly
  - it does not directly serve personalized CME identity state, which is handled through `cSelfGEL` and sovereign custody

Contract fields:
- Layer: cryptic operational context
- Governance domain: protected pre-Engrammitization, audit, and cryptic processing context
- Authority relation: cryptic operational domain
- Identity relation: identity-external unless explicitly promoted
- Persistence type: append-only cryptic store
- Mutation rule: may accumulate cryptic emissions; must not silently become Prime

### OE

- Status: `canonical`
- Meaning: private or local relative overlay domain for provisional, local, or mission-bound working state inside a prior continuity-binding regime

Contract fields:
- Layer: local or provisional working domain
- Governance domain: operator-local and mission-local build-out
- Authority relation: non-authoritative by default
- Identity relation: identity-coupled, not automatically identity-bearing
- Persistence type: local overlay and working structures
- Mutation rule: may annotate and stage; must not silently rewrite GEL

### SelfGEL

- Status: `canonical`
- Meaning: self-oriented GEL-like surface for local or bonded self-state formation and the first stabilized self-readable Prime-facing surface within a prior coherence regime

Contract fields:
- Layer: self-state
- Governance domain: agent-local self formation
- Authority relation: locally operative
- Identity relation: identity-coupled and potentially identity-bearing when admitted
- Persistence type: self-scoped state surface
- Mutation rule: governed by CME, AgentiCore, and admission law

### cSelfGEL

- Status: `canonical`
- Meaning: cryptic counterpart to self-oriented GEL-like surfaces and the first stabilized protected self-readable surface within a prior coherence regime

Contract fields:
- Layer: cryptic self-state
- Governance domain: protected self-state formation
- Authority relation: locally operative, cryptic-resident
- Identity relation: identity-coupled
- Persistence type: protected self-scoped storage
- Mutation rule: must not silently promote to Prime or GEL

### Pre-Instantiated Coherence

- Status: `canonical`
- Meaning: the latent formative weave from which localizable proposition-bearing and identity-bearing surfaces may later emerge
- Usage:
  - use for doctrine describing the binding condition that precedes stabilized surface formation
  - do not flatten into an ordinary stack module or already-instantiated object

### Proposition-Bearing Stabilization

- Status: `canonical`
- Meaning: the first threshold at which a stable, inspectable, repeatable proposition-bearing formation can appear
- Usage:
  - use for the layer prior to morphism candidacy and prior to governed identity
  - do not treat as equivalent to accepted truth or automatic custody eligibility

### Admissible Proposition Surface

- Status: `canonical`
- Meaning: proposition-bearing structure that has become fit for observation, review, governance intake, and possible downstream handling
- Usage:
  - use when proposition-bearing structure has crossed into admissibility
  - do not treat as governed identity by default

### Admissible Identity Surface

- Status: `canonical`
- Meaning: a later accountable surface at which governed identity may be recognized under Sanctuary law
- Usage:
  - use for identity-bearing admissibility under governance and accountability
  - do not collapse this back into mere proposition stabilization

### CME

- Status: `canonical`
- Meaning: `Cognitive Mission Envelope`, the bounded mission frame for operator, agent, evidence, and permitted action surfaces

Contract fields:
- Layer: mission governance
- Governance domain: bounded execution frame
- Authority relation: conditionally authoritative mission boundary
- Identity relation: identity-scoping, not identity-defining by itself
- Persistence type: mission object and operating frame
- Mutation rule: constrains action; does not itself bypass stack law

### MoS

- Status: `canonical`
- Meaning: `Mantle of Sovereigns`, the sovereignty-facing store family spanning Prime and Cryptic surfaces

Contract fields:
- Layer: sovereignty and continuity service
- Governance domain: redacted Prime sovereignty access and protected cryptic continuity custody
- Authority relation: protected service family, not public canonical truth by default
- Identity relation: identity-adjacent
- Persistence type: sovereignty-facing custody and pointer surfaces
- Mutation rule: may retain governed sovereign copies and expose masked or pointerized surfaces; may not act as public canonical truth

### cMoS

- Status: `canonical`
- Meaning: `Cryptic Mantle of Sovereigns`, the sovereign continuity store for protected CME self-state and protected telemetry retention

Contract fields:
- Layer: cryptic sovereignty and continuity custody
- Governance domain: protected CME continuity, `cOE`, `cSelfGEL`, and legally retained protected constructs
- Authority relation: protected sovereign custody
- Identity relation: identity-bearing or identity-adjacent depending on carried payload
- Persistence type: cryptic sovereign continuity store
- Mutation rule: may retain protected and continuity-bearing state under governance; must not expose masked constructs without governed mediation

### Mother

- Status: `canonical`
- Meaning: Prime-layer governing witness role inside the active CradleTek runtime

Contract fields:
- Layer: Prime governance
- Governance domain: outward coherence, public-facing lawful presentation, release posture
- Authority relation: witness and Prime-domain governor
- Identity relation: identity-external
- Persistence type: witness signals, attestations, and Prime governance records
- Mutation rule: may constrain Prime behavior; may not weaken cryptic law

### Father

- Status: `canonical`
- Meaning: Cryptic-layer governing witness role inside the active CradleTek runtime

Contract fields:
- Layer: cryptic governance
- Governance domain: seals, custody, cryptic telemetry, boundary integrity
- Authority relation: witness and cryptic-domain governor
- Identity relation: identity-external
- Persistence type: cryptic witness signals, attestations, and integrity records
- Mutation rule: may constrain cryptic and security posture; may not silently promote to Prime

### Steward

- Status: `canonical`
- Meaning: Sanctuary runtime continuity and CME stewardship witness role

Contract fields:
- Layer: runtime continuity
- Governance domain: orchestration, continuity, swarm facilitation, bounded labor structures
- Authority relation: witness and runtime governor
- Identity relation: identity-external
- Persistence type: continuity records, orchestration signals, stewardship state
- Mutation rule: may coordinate and constrain runtime; may not seize discourse or seal authority

### Prime Theater

- Status: `canonical`
- Meaning: the public operational universe of discourse, interaction, publication, and dialectic exchange

Contract fields:
- Layer: theater
- Governance domain: public and shared-use operation
- Authority relation: governance-sensitive operating universe
- Identity relation: commitment-eligible under lawful transition
- Persistence type: public-facing and standard-plane artifact forms
- Mutation rule: outputs may become binding only through explicit lawful paths
- Cross-boundary restrictions: must not silently inherit Dream speculation or Cryptic protected matter

### Cryptic Theater

- Status: `canonical`
- Meaning: the protected interior universe of identity continuity, security boundaries, sealed memory, certification records, and governance verification

Contract fields:
- Layer: theater
- Governance domain: protected interior and cryptic custody
- Authority relation: governance-sensitive protected universe
- Identity relation: identity-continuity-sensitive
- Persistence type: cryptic and sealed artifact forms
- Mutation rule: may not silently surface into Prime
- Cross-boundary restrictions: may reach Prime only through lawful SLI-mediated transition and governed admission

### Dream Theater

- Status: `canonical`
- Meaning: the generative exploratory universe of speculation, hypothesis generation, creative exploration, and plural branching

Contract fields:
- Layer: theater
- Governance domain: exploratory and generative operation
- Authority relation: non-binding by default
- Identity relation: non-binding unless lawfully transitioned
- Persistence type: speculative and exploratory forms
- Mutation rule: may not silently become Prime fact or Cryptic guarantee
- Cross-boundary restrictions: must pass lawful transformation before discourse, custody, or commitment use

### Theater

- Status: `canonical`
- Meaning: a locally coherent universe of operation with its own geometry, ruleset, symbolic grammar, admissible actors, lawful operations, and locality of universal selection
- Notes:
  - Theater is an Inter-universal Techmuller Theory-aligned locality concept in this stack
  - Prime, Cryptic, and Dream are distinct Theater-local realities, not merely UI modes
  - each Theater acts as a distinct coordinate system for cognition

Contract fields:
- Layer: theater meta-structure
- Governance domain: local universe definition
- Authority relation: geometry- and rule-bounded
- Identity relation: depends on the Theater
- Persistence type: Theater-native artifact and reasoning forms
- Mutation rule: cross-Theater movement requires lawful transformation

### Braiding

- Status: `canonical`
- Meaning: a lawful cross-Theater association that preserves required invariants across distinct local universes
- Notes:
  - later Engineered Cognition surfaces may operationalize this as a role transition or context switch, but the locality-preserving association remains primary

### Comingling

- Status: `canonical`
- Meaning: a governed cross-boundary association in which multiple domains interact without losing their lawful distinctions

### Fiber Bundles

- Status: `canonical`
- Meaning: structured continuity relations that connect local universes while preserving the distinction between local geometry and global association

### Gluing

- Status: `canonical`
- Meaning: the lawful joining relation by which multiple Theater projections or local structures are made to coexist without collapsing their distinctions

### Theater Projection

- Status: `canonical`
- Meaning: an artifact-local manifestation of a specific Theater's geometry, ruleset, and lawful form
- Notes:
  - a single artifact may hold more than one Theater projection when the gluing relation is explicit and governed
  - layered or holographic artifacts such as `.hopng` may carry Prime-visible, Cryptic-structural, and Dream-speculative projections at once

### Holographic Data Tool

- Status: `canonical`
- Meaning: the governed artifact system for creating `.hopng` objects
- Notes:
  - it creates visible PNG-based carriers backed by structured sidecars
  - it is intended for lawful trust, validation, relational structure, and later semantic growth
  - it is building toward higher-order relation, temporality, identity, commitment, and runtime participation

Contract fields:
- Layer: cross-Theater artifact infrastructure
- Governance domain: trusted packaging, inspection, relation, and projection binding
- Authority relation: non-authoritative carrier system by default
- Identity relation: identity-neutral unless explicit governed payload law says otherwise
- Persistence type: governed artifact package with carrier and sidecars
- Mutation rule: may package and relate lawfully; must not silently flatten projections or release privileged structure

### .hopng

- Status: `canonical`
- Meaning: the emerging holographic artifact and tool format under development in this workspace
- Notes:
  - `.hopng` is the primary artifact form of the Holographic Data Tool, not just a static image extension
  - a `.hopng` artifact may carry multiple lawful Theater projections at once
  - expected projection families include Prime-visible, Cryptic-structural, and Dream-speculative layers
  - the artifact must define or preserve the gluing relation between those projections

Contract fields:
- Layer: cross-Theater artifact surface
- Governance domain: governed holographic representation and projection binding
- Authority relation: non-authoritative container or carrier surface
- Identity relation: depends on the governed payload it carries
- Persistence type: layered holographic artifact
- Mutation rule: may host multiple Theater-local projections but must not collapse or silently promote them across Theater boundaries

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
