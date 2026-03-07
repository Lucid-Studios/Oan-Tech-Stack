# Dependency Contract: OAN Mortalis v1.0

## Purpose

This document defines the allowed dependency directions for the active `OAN Mortalis V1.0` stack.

It exists to enforce the family constitution in build-maintainable terms.

This contract governs:

- project reference direction
- family ownership boundaries
- stack composition boundaries
- temporary shim usage during migration and realignment

## Governing Documents

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `Build Contracts/Crosscutting/ARCHITECTURE_FRAME.md`
- `OAN Mortalis V1.0/docs/PROJECT_CLASSIFICATION_MATRIX.md`
- `OAN Mortalis V1.0/docs/NAMESPACE_CONVERGENCE_PLAN.md`

## Constitutional Summary

The stack is governed as:

- `Oan.*` for umbrella composition and stack-level contracts
- `CradleTek.*` for infrastructure and substrate ownership
- `SoulFrame.*` for operator and identity-facing workflow ownership
- `AgentiCore.*` for agent runtime ownership
- `SLI.*` for symbolic protocol and runtime ownership across families

This means dependency rules must preserve:

- foundational direction
- sibling-family independence
- stack composition clarity
- cross-cutting symbolic reuse without upward capture

## Family Dependency Rules

### Rule 1. `Oan.*` may compose, but should not absorb family-local implementation casually

Allowed:

- `Oan.*` may reference:
  - `CradleTek.*`
  - `SoulFrame.*`
  - `AgentiCore.*`
  - `SLI.*`
  - other `Oan.*` stack-level contracts

Constraint:

- `Oan.*` should primarily own:
  - composition roots
  - stack-wide contracts
  - stack integration facades

It should not become a dumping ground for family-owned implementation code.

### Rule 2. `CradleTek.*` is foundational

Allowed:

- `CradleTek.*` may reference:
  - `CradleTek.*`
  - `SLI.*`
  - stack-level contract surfaces from `Oan.*` when the contract is truly umbrella-owned

Restricted:

- `CradleTek.*` should not reference `SoulFrame.*` unless explicitly justified
- `CradleTek.*` should not reference `AgentiCore.*` unless explicitly justified
- `CradleTek.*` must not depend on `Oan.Runtime.Headless`

Interpretation:

- infrastructure may expose services upward
- it should not depend upward on sibling domain families for normal operation

### Rule 3. `SoulFrame.*` is a sibling domain family over infrastructure

Allowed:

- `SoulFrame.*` may reference:
  - `CradleTek.*`
  - `SLI.*`
  - stack-level contract surfaces from `Oan.*`
  - other `SoulFrame.*`

Restricted:

- `SoulFrame.*` should avoid direct dependency on `AgentiCore.*` unless the interaction is unavoidable and documented
- `SoulFrame.*` must not depend on `Oan.Runtime.Headless`

### Rule 4. `AgentiCore.*` is a sibling domain family over infrastructure

Allowed:

- `AgentiCore.*` may reference:
  - `CradleTek.*`
  - `SLI.*`
  - stack-level contract surfaces from `Oan.*`
  - other `AgentiCore.*`

Restricted:

- `AgentiCore.*` should avoid direct dependency on `SoulFrame.*` unless the interaction is unavoidable and documented
- `AgentiCore.*` must not depend on `Oan.Runtime.Headless`

### Rule 5. `SLI.*` is transversal and should not depend upward into sibling families

Allowed:

- `SLI.*` may reference:
  - `SLI.*`
  - `CradleTek.*`
  - stack-level contract surfaces from `Oan.*` when the contract is symbolic or truly umbrella-owned

Restricted:

- `SLI.*` should not reference `SoulFrame.*` except through explicit adapter or contract seams
- `SLI.*` should not reference `AgentiCore.*` except through explicit adapter or contract seams
- `SLI.*` must not depend on `Oan.Runtime.Headless`

Interpretation:

- `SLI.*` serves multiple families
- it should not become owned by one sibling family through upward reference pressure

## Project-Level Contract Baseline

### Stack Composition

| Project | Allowed Direction |
| --- | --- |
| `Oan.Runtime.Headless` | may compose all active families |
| `Oan.Cradle` | may depend on stack contracts and family services needed for orchestration |
| `Oan.Common` | should remain dependency-light and safe for broad reuse |

### Stack Contracts And Facades

| Project | Allowed Direction |
| --- | --- |
| `Oan.Spinal` | should remain low-dependency and contract-like |
| `Oan.SoulFrame` | should depend on stack contracts, not family-local runtime internals by default |
| `Oan.AgentiCore` | should depend on stack contracts, not family-local runtime internals by default |
| `Oan.Sli` | should depend on stack contracts and symbolic-facing contracts |
| `Oan.Storage` | should clarify whether it is a facade or implementation boundary before dependency spread increases |
| `Oan.Place` | should remain a boundary layer rather than a second composition root |

### Transitional Or Unresolved Surfaces

The following require explicit ownership decisions before stronger automation is added:

- `OAN.Core`
- `GEL`
- `Telemetry.GEL`
- `EngramGovernance`
- `Data.Cryptic`
- `Oan.Fgs`

## Forbidden Dependency Patterns

The following patterns are constitutionally invalid unless explicitly documented as temporary migration exceptions:

- foundational infrastructure depending upward on stack composition roots
- `CradleTek.*` taking routine dependencies on `SoulFrame.*`
- `CradleTek.*` taking routine dependencies on `AgentiCore.*`
- `SLI.*` taking routine dependencies on `SoulFrame.*`
- `SLI.*` taking routine dependencies on `AgentiCore.*`
- sibling-family cycles between `SoulFrame.*` and `AgentiCore.*`
- multiple stack composition roots
- family-local runtimes presenting themselves as stack-level hosts

## Shim Policy

### What a shim means

A shim is a temporary adapter or bridge that allows:

- legacy naming to coexist with new ownership rules
- callers to keep working while implementation moves
- contract evolution without immediate full rewrite

Examples:

- adapter namespaces
- wrapper services
- compatibility interfaces
- forwarding implementations

### Does a shim imply rewrite

Usually, yes.

A shim is not the end state. It is a controlled temporary measure used when:

- the correct owning family is known
- the code cannot be moved safely in one step
- build continuity matters more than immediate purity

The expected lifecycle is:

1. identify the true owner
2. add shim or adapter if needed
3. move or rewrite the implementation behind the seam
4. migrate callers
5. remove the shim

### When a shim is acceptable

- when preserving a green build during namespace or ownership realignment
- when avoiding a breaking rename across many projects at once
- when introducing a family contract before moving implementation

### When a shim is not acceptable

- when it becomes the permanent architecture
- when it hides unresolved ownership indefinitely
- when it introduces circular references or new ambiguous dependency paths

## Exception Process

If a dependency must violate the baseline rules temporarily, document:

- the reason
- the owning team or decision-maker
- the exit condition
- the target family or contract after migration

Temporary exceptions should be tracked as migration work, not normalized as permanent design.

## Enforcement Path

### Immediate

- use this contract in architecture review and project placement decisions
- reject new projects that violate family ownership by default
- reject new ambiguous stack roots

### Near-Term

- add audit coverage that inspects project references against family rules
- classify current violations as:
  - valid by constitution
  - temporary exception
  - misplacement requiring follow-up

### Mature-State

- enforce family dependency rules in CI
- require exception annotation for temporary violations
- block new cross-family drift automatically

## Success Criteria

This contract is working when:

- `Oan.Runtime.Headless` remains the only stack composition root
- family-local runtime services remain family-local
- sibling-family cycles are rare and justified
- `SLI.*` remains cross-cutting without being captured by one sibling family
- shims are shrinking, not accumulating
