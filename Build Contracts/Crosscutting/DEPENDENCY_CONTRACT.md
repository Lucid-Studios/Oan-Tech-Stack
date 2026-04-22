# Dependency Contract

## Purpose

This document defines the allowed dependency directions for the active
Sanctuary-root stack.

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
- `OAN Mortalis V1.1.1/docs/PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md`
- `OAN Mortalis V1.1.1/docs/STACK_ROOT_RENAMING_MIGRATION_PLAN.md`

## Constitutional Summary

The active stack is governed as:

- `San.*` for Sanctuary constitutional habitat and stack-root composition
- `Ctk.*` for CradleTek habitation, custody, extension, and runtime-distribution
  ownership
- `Sfr.*` for SoulFrame relational, membrane, projection, and interface
  ownership
- `Acr.*` for AgentiCore identity and governance-capable core machinery
  ownership
- `SLI.*` for symbolic protocol and runtime ownership across families
- `Oan.*` as downstream application identity or legacy migration hold only

This means dependency rules must preserve:

- foundational direction
- sibling-family independence
- stack composition clarity
- cross-cutting symbolic reuse without upward capture
- explicit handling of legacy `Oan.*` migration surfaces

## Family Dependency Rules

### Rule 1. `San.*` owns foundational composition

Allowed:

- `San.*` may reference:
  - `Ctk.*`
  - `Sfr.*`
  - `Acr.*`
  - `SLI.*`
  - other `San.*` stack-level contracts and host services
  - legacy `Oan.*` surfaces only while governed by the line-local migration
    allowlist

Constraint:

- `San.*` should primarily own:
  - composition roots
  - stack-wide contracts
  - constitutional host services
  - governed outward service-origin surfaces

It should not become a dumping ground for family-owned implementation code.

### Rule 2. `Ctk.*` is habitation, custody, and extension ownership

Allowed:

- `Ctk.*` may reference:
  - `Ctk.*`
  - `San.*` through lawful host or service seams
  - `SLI.*`

Restricted:

- `Ctk.*` should not re-own `Sfr.*` responsibilities
- `Ctk.*` should not re-own `Acr.*` responsibilities
- `Ctk.*` must not depend on `San.Runtime.Headless` or legacy
  `Oan.Runtime.Headless`

Interpretation:

- habitation and custody may expose services upward
- distribution and extension surfaces must not become a second foundational
  composition root

### Rule 3. `Sfr.*` is relational, membrane, and projection ownership

Allowed:

- `Sfr.*` may reference:
  - `San.*`
  - `SLI.*`
  - other `Sfr.*`
  - `Ctk.*` only through explicit custody, mantle, or substrate seams

Restricted:

- `Sfr.*` should avoid direct dependency on `Acr.*` unless the interaction is
  unavoidable and documented
- `Sfr.*` must not depend on `San.Runtime.Headless` or legacy
  `Oan.Runtime.Headless`

### Rule 4. `Acr.*` is cognition-core machinery ownership

Allowed:

- `Acr.*` may reference:
  - `San.*`
  - `Sfr.*`
  - `SLI.*`
  - other `Acr.*`
  - `Ctk.*` only through explicit custody, memory, or runtime service seams

Restricted:

- `Acr.*` should not own `Sfr.*` membrane responsibilities
- `Acr.*` should not own `Ctk.*` custody or hosting responsibilities
- `Acr.*` must not depend on `San.Runtime.Headless` or legacy
  `Oan.Runtime.Headless`

### Rule 5. `SLI.*` is transversal and should not depend upward into sibling families

Allowed:

- `SLI.*` may reference:
  - `SLI.*`
  - low-level `San.*` contract surfaces when the contract is symbolic or truly
    stack-root owned
  - `Ctk.*` only through explicit symbolic transport or substrate seams

Restricted:

- `SLI.*` should not reference `Sfr.*` except through explicit adapter or
  contract seams
- `SLI.*` should not reference `Acr.*` except through explicit adapter or
  contract seams
- `SLI.*` must not depend on `San.Runtime.Headless` or legacy
  `Oan.Runtime.Headless`

Interpretation:

- `SLI.*` serves multiple families
- it should not become owned by one sibling family through upward reference
  pressure

### Rule 6. `Oan.*` is a legacy migration hold for foundational code

Allowed:

- existing `Oan.*` projects may remain while they are listed in the line-local
  legacy allowlist and continue to build
- `Oan.*` may later own downstream application, game, or domain composition

Restricted:

- new foundational stack code should not use `Oan.*`
- new foundational project identities should not use `Oan.*`
- legacy `Oan.*` surfaces must not expand their foundational authority beyond
  the allowlisted migration posture

## Project-Level Contract Baseline

### Current Transitional Stack Composition

| Current Project | Constitutional Standing |
| --- | --- |
| `Oan.Runtime.Headless` | legacy migration composition root for the current line; target owner is `San.Runtime.Headless` |
| `Oan.Common` | legacy migration contract surface; target owner is `San.Common` |
| `Oan.FirstRun` | legacy migration first-run service; target owner is `San.FirstRun` |
| `Oan.HostedLlm` | legacy migration hosted-LLM surface; target owner is `San.HostedLlm` |
| `Oan.Nexus.Control` | legacy migration nexus/control surface; target owner is `San.Nexus.Control` |
| `Oan.Runtime.Materialization` | legacy migration materialization surface; target owner is `San.Runtime.Materialization` |
| `Oan.State.Modulation` | legacy migration modulation surface; target owner is `San.State.Modulation` |
| `Oan.Trace.Persistence` | legacy migration trace surface; target owner is `San.Trace.Persistence` |

### Family Runtime Surfaces

| Current Project | Constitutional Standing |
| --- | --- |
| `CradleTek.*` | legacy-named current CradleTek surfaces; target prefix is `Ctk.*` |
| `SoulFrame.*` | legacy-named current SoulFrame surfaces; target prefix is `Sfr.*` |
| `AgentiCore` / `AgentiCore.*` | legacy-named current AgentiCore surfaces; target prefix is `Acr.*` |
| `SLI.*` | active transversal symbolic family; prefix remains valid |
| `GEL.Contracts` | bounded supporting domain surface pending stronger final placement |

## Forbidden Dependency Patterns

The following patterns are constitutionally invalid unless explicitly
documented as temporary migration exceptions:

- foundational infrastructure depending upward on stack composition roots
- `Ctk.*` taking routine dependencies on `Sfr.*`
- `Ctk.*` taking routine dependencies on `Acr.*`
- `SLI.*` taking routine dependencies on `Sfr.*`
- `SLI.*` taking routine dependencies on `Acr.*`
- sibling-family cycles between `Sfr.*` and `Acr.*`
- multiple foundational stack composition roots
- family-local runtimes presenting themselves as stack-level hosts
- new foundational `Oan.*` projects

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

Temporary exceptions should be tracked as migration work, not normalized as
permanent design.

## Enforcement Path

### Immediate

- use this contract in architecture review and project placement decisions
- reject new foundational projects that use legacy `Oan.*` by default
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

- `San.Runtime.Headless` is the only foundational stack composition root
- current legacy `Oan.*` foundational surfaces shrink under a governed allowlist
- family-local runtime services remain family-local
- sibling-family cycles are rare and justified
- `SLI.*` remains cross-cutting without being captured by one sibling family
- shims are shrinking, not accumulating
