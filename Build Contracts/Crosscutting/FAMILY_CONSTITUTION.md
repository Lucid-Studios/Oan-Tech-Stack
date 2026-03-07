# Family Constitution: OAN Mortalis v1.0

## Purpose

This document defines the canonical family model for the active `OAN Mortalis V1.0` stack.

It exists to stabilize:

- family ownership
- dependency direction
- naming lineage
- composition versus service responsibility

This is a constitutional document, not a temporary migration note.

## Constitutional Model

The active stack is not modeled as several equivalent top-level namespace families competing for ownership.

It is modeled as:

- one umbrella composition family
- three owned service or infrastructure families
- one transversal symbolic protocol and runtime family

Canonical family structure:

```text
Oan.*
  composes
  - CradleTek.*
  - SoulFrame.*
  - AgentiCore.*
  - SLI.*   (cross-cutting symbolic family)
```

## Canonical Family Roles

### `Oan.*`

Role:

- Sanctuary and full-stack application family

Owns:

- executable composition roots
- stack-level contracts
- stack-level integration surfaces
- umbrella runtime composition
- Sanctuary-level application products

Examples:

- `Oan.Runtime.Headless`
- future stack-level hosts, contracts, and integration surfaces

Rule:

- `Oan.*` is the umbrella composition family
- it is not the required replacement prefix for all domain or infrastructure code

### `CradleTek.*`

Role:

- primary application, orchestration, and infrastructure family

Owns:

- application composition for active services
- swarm coordination
- system hosting
- storage substrates
- infrastructure services
- low-level runtime support
- infrastructure-bound operational services

Rule:

- `CradleTek.*` is a first-class active family, not a compatibility-only family by default

### `SoulFrame.*`

Role:

- self-state membrane and identity-facing workflow family

Owns:

- mitigated self-state projection
- collapse and return intake shaping
- identity-safe service mediation
- review surfaces
- approval surfaces
- operator interaction layers
- identity-facing workflow surfaces
- experiential mission surfaces

Rule:

- `SoulFrame.*` is a first-class active family, not merely a compatibility label

### `AgentiCore.*`

Role:

- extended cognitive workspace and bounded agent runtime family

Owns:

- policy-bound task execution
- cognitive runtime operations
- agent labor routing
- local reflective and private operational cognition space

Rule:

- `AgentiCore.*` is a first-class active family and should own agent runtime behavior directly
- it should operate on SoulFrame-mediated state rather than sovereign custody directly

### `SLI.*`

Role:

- symbolic interoperability family

Owns:

- symbolic protocol behavior
- symbolic runtime
- parsing and transformation
- symbolic cognition surfaces
- cross-family symbolic interoperability

Rule:

- `SLI.*` is transversal across the stack
- it is not owned exclusively by `CradleTek.*`, `SoulFrame.*`, or `AgentiCore.*`

## Dependency Constitution

### Allowed High-Level Flow

- `Oan.*` may compose all families
- `CradleTek.*` is the application and orchestration fabric over infrastructure
- `SoulFrame.*` may depend on `CradleTek.*`
- `AgentiCore.*` may depend on `SoulFrame.*`
- `AgentiCore.*` may depend on `CradleTek.*` through lawful service and substrate seams
- `CradleTek.*` may consume `SLI.*`
- `SoulFrame.*` may consume `SLI.*`
- `AgentiCore.*` may consume `SLI.*`

### Restricted Flow

- `SLI.*` should avoid upward dependence on `SoulFrame.*`
- `SLI.*` should avoid upward dependence on `AgentiCore.*`
- `CradleTek.*` should avoid depending on `SoulFrame.*` unless the dependency is explicitly justified
- `CradleTek.*` should avoid depending on `AgentiCore.*` unless the dependency is explicitly justified
- sibling family coupling should be minimized and documented

### Composition Rule

- only `Oan.*` owns stack composition roots
- family-local runtime services may exist inside each family
- family-local runtime services must not present themselves as stack-level composition roots
- `CradleTek.*` may act as the primary application and swarm fabric without becoming the sovereign source of identity law
- `SoulFrame.*` may provision and mediate AgentiCore runtime self-state without becoming a generic everything-service host

## Naming Constitution

### Family Prefixes

- `Oan.*` means umbrella stack composition or stack-level contract ownership
- `CradleTek.*` means infrastructure or substrate ownership
- `SoulFrame.*` means operator and identity-facing workflow ownership
- `AgentiCore.*` means agent runtime ownership
- `SLI.*` means symbolic protocol or symbolic runtime ownership

### Ambiguous Terms

The following names require family qualification:

- `Runtime`
- `Host`
- `Engine`
- `Core`

Examples:

- `Oan.Runtime.Headless` is a stack composition root
- `CradleTek.Runtime` is an infrastructure runtime service
- `AgentiCore.Runtime` is an agent runtime service
- `SLI.Engine` is a symbolic engine service

These names are acceptable only when the family context makes the ownership unambiguous.

## Current Interpretation Of Existing Projects

The presence of:

- `CradleTek.*`
- `SoulFrame.*`
- `AgentiCore.*`
- `SLI.*`

should not be interpreted as evidence of uncontrolled naming drift by itself.

The actual governance problem is narrower:

- family boundaries are not yet documented rigorously enough
- some projects may sit in the wrong family
- some `Oan.*` projects may currently own work that belongs in a family-specific namespace
- stack composition and family-local runtime naming are not yet distinguished sharply enough

## Success Criteria

The family model is healthy when:

- `Oan.*` owns composition and stack-level contracts
- `CradleTek.*`, `SoulFrame.*`, and `AgentiCore.*` own their proper domains
- `SLI.*` remains cross-cutting without depending upward on sibling families
- project names clearly communicate family ownership
- dependency rules reflect family lineage rather than accidental coupling

## Reference Documents

- `Build Contracts/Crosscutting/ARCHITECTURE_FRAME.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `OAN Mortalis V1.0/docs/PROJECT_CLASSIFICATION_MATRIX.md`
- `OAN Mortalis V1.0/docs/NAMESPACE_CONVERGENCE_PLAN.md`
