# Family Constitution

## Purpose

This document defines the canonical family model for the active Sanctuary-root
stack.

It exists to stabilize:

- family ownership
- dependency direction
- naming lineage
- composition versus service responsibility

This is a constitutional document, not a temporary migration note.

## Constitutional Model

The active stack is not modeled as one umbrella `Oan.*` code family over all
foundational ownership.

It is modeled as:

- one constitutional habitat and host family
- three owned deployable families
- one transversal symbolic protocol and runtime family

Canonical family structure:

```text
San.*
  hosts and composes
  - Ctk.*
  - Sfr.*
  - Acr.*
  - SLI.*   (cross-cutting symbolic family)
```

`OAN` remains a valid technology and product label.

It is no longer the canonical foundational code-family root for the active
stack.

`OAN` is now treated as a downstream application, game, or domain identity
built from the Sanctuary-root stack.

## Canonical Family Roles

### `San.*`

Role:

- Sanctuary constitutional habitat and local host family

Owns:

- executable habitat roots
- stack-level contracts
- stack-level integration surfaces
- first local constitutional services
- governed outward host and service-origin surfaces

Examples:

- `San.Common`
- `San.FirstRun`
- `San.HostedLlm`
- `San.Runtime.Headless`

Rule:

- `San.*` is the canonical foundational stack root
- it may host and compose all stack families
- it should not be replaced by a product-facing `OAN` prefix in new
  foundational code

### `Ctk.*`

Role:

- CradleTek habitation, custody, extension, and runtime-distribution family

Owns:

- custody and mantle surfaces
- runtime distribution and hosting extension
- storage substrates and distributed embodiment seams
- remote, local-network, or outward service embodiment under lawful admission

Rule:

- `Ctk.*` is a first-class active family
- it is not the universal prerequisite for all meaningful local operation
- it appears as a major extension family rather than the sole base habitat

### `Sfr.*`

Role:

- SoulFrame relational, membrane, projection, and interface substrate family

Owns:

- low-mind and situational shaping
- membrane and stewardship surfaces
- relational and projection mediation
- bounded outward interface shaping

Rule:

- `Sfr.*` is a first-class active family
- it may operate natively within `San.*` without requiring `Ctk.*` as an
  intermediate owner

### `Acr.*`

Role:

- AgentiCore identity and governance-capable core machinery family

Owns:

- higher-order cognition machinery
- EC-bearing internal machinery
- governance-capable runtime core behavior
- capability and derivation posture over lawful mediated state

Rule:

- `Acr.*` is a first-class active family
- it may operate natively within `San.*` without requiring `Ctk.*` as an
  intermediate owner

### `SLI.*`

Role:

- symbolic interoperability and symbolic runtime family

Owns:

- symbolic protocol behavior
- symbolic runtime
- parsing and transformation
- symbolic cognition surfaces
- cross-family symbolic interoperability

Rule:

- `SLI.*` is transversal across the stack
- it is not owned exclusively by `San.*`, `Ctk.*`, `Sfr.*`, or `Acr.*`

### `Oan.*`

Role:

- reserved downstream application, game, or domain namespace

Current Standing:

- legacy migration hold inside the active line
- not admissible as the root for new foundational stack code

Rule:

- no new foundational `Oan.*` namespaces or project names should be admitted
- existing `Oan.*` source and project surfaces remain temporary migration
  holds until a governed rename slice retires them

## Dependency Constitution

### Allowed High-Level Flow

- `San.*` may host and compose all families
- `Sfr.*` may depend on `San.*`
- `Acr.*` may depend on `San.*`
- `Acr.*` may depend on `Sfr.*`
- `Ctk.*` may depend on `San.*` through lawful host or service seams
- `Ctk.*` may consume `SLI.*`
- `Sfr.*` may consume `SLI.*`
- `Acr.*` may consume `SLI.*`

### Restricted Flow

- `SLI.*` should avoid upward dependence on `Sfr.*` or `Acr.*`
- `Ctk.*` should avoid re-owning `Sfr.*` or `Acr.*` responsibilities
- sibling family coupling should be minimized and documented

### Composition Rule

- only `San.*` owns foundational stack composition roots
- `Sfr.*` and `Acr.*` may be activated natively inside `San.*`
- `Ctk.*` may be templated or service-exposed without implying local bonded
  activation
- `Oan.*` may later own downstream application composition, but not the
  foundational stack root

## Naming Constitution

### Family Prefixes

- `San.*` means Sanctuary constitutional habitat or stack-root ownership
- `Ctk.*` means CradleTek habitation, custody, or extension ownership
- `Sfr.*` means SoulFrame relational and interface ownership
- `Acr.*` means AgentiCore core machinery ownership
- `SLI.*` means symbolic protocol or symbolic runtime ownership
- `Oan.*` is reserved for downstream application or legacy migration hold only

### Forward Freeze

From this point forward:

- no new foundational code should use `Oan.*`
- no new foundational project should use an `Oan.*` project identity
- new stack-root surfaces should use `San.*`
- new CradleTek surfaces should use `Ctk.*`
- new SoulFrame surfaces should use `Sfr.*`
- new AgentiCore surfaces should use `Acr.*`

The line-local legacy `Oan.*` allowlist is governed by:

- `OAN Mortalis V1.1.1/build/legacy-oan-namespace-allowlist.json`

## Success Criteria

The family model is healthy when:

- `San.*` owns foundational stack composition and constitutional host surfaces
- `Ctk.*`, `Sfr.*`, and `Acr.*` own their proper domains
- `SLI.*` remains cross-cutting without depending upward on sibling families
- `Oan.*` no longer accumulates new foundational ownership
- project names communicate family truth instead of legacy drift

## Reference Documents

- `Build Contracts/Crosscutting/ARCHITECTURE_FRAME.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `OAN Mortalis V1.1.1/docs/PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md`
- `OAN Mortalis V1.1.1/docs/STACK_ROOT_RENAMING_MIGRATION_PLAN.md`
