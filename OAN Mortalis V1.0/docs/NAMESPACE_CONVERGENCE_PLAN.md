# NAMESPACE_CONVERGENCE_PLAN

## Purpose

This document defines the controlled alignment path from the current mixed-family solution state into one maintainable constitutional naming model under version control.

Convergence does not mean collapsing all projects into `Oan.*`.

It means:

- one stable umbrella composition family
- clear family ownership for infrastructure and domain runtime code
- one cross-cutting symbolic family
- explicit rules for how families compose without obscuring their identity

## Decision

The canonical family model for `OAN Mortalis V1.0` is:

- `Oan.*` for umbrella stack composition and stack-level contracts
- `CradleTek.*` for infrastructure and substrate ownership
- `SoulFrame.*` for operator and identity-facing workflow ownership
- `AgentiCore.*` for agent runtime ownership
- `SLI.*` for symbolic protocol and runtime ownership across the stack

`Oan.*` is the umbrella composition family.

It is not the mandatory replacement namespace for all active families.

## Convergence Objective

The objective is to move from accidental mixed naming to intentional family lineage.

That requires:

- naming each project according to its owning family
- distinguishing stack composition from family-local runtime services
- reducing unexplained roots and acronyms
- clarifying which projects are stack façades versus family implementations

## Alignment Principles

### 1. Preserve real family identity

If a project genuinely belongs to infrastructure, operator workflow, agent runtime, or symbolic runtime ownership, it should keep the appropriate family prefix.

### 2. Reserve `Oan.*` for stack-level ownership

Use `Oan.*` for:

- composition roots
- stack-wide contracts
- stack-wide integration façades
- Sanctuary-level application products

Do not force domain-specific or infrastructure-specific code into `Oan.*` merely for cosmetic uniformity.

### 3. Qualify runtime names by family purpose

Terms like:

- `Runtime`
- `Host`
- `Engine`
- `Core`

are acceptable only when family ownership makes the responsibility clear.

### 4. Resolve unclear ownership before renaming

Projects such as:

- `Telemetry.GEL`
- `GEL`
- `EngramGovernance`
- `Data.Cryptic`

need ownership decisions before rename or consolidation decisions.

## Family Alignment Map

| Current Surface | Target Interpretation | Action |
| --- | --- | --- |
| `Oan.Runtime.Headless` | canonical stack composition root | keep |
| `Oan.Cradle` | stack orchestration façade | keep, clarify boundary |
| `Oan.SoulFrame` | stack-level SoulFrame contract or integration façade | keep, clarify boundary |
| `Oan.AgentiCore` | stack-level AgentiCore contract or integration façade | keep, clarify boundary |
| `Oan.Sli` | stack-level SLI contract or integration façade | keep, clarify boundary |
| `Oan.Storage` | stack-level storage façade or adapter boundary | keep, clarify ownership split with `CradleTek.*` |
| `Oan.Spinal` | stack-level deterministic substrate contract | keep, clarify relationship to `OAN.Core` |
| `CradleTek.*` | active infrastructure family | keep as first-class family |
| `SoulFrame.*` | active operator family | keep as first-class family |
| `AgentiCore.*` | active agent runtime family | keep as first-class family |
| `SLI.*` | active cross-cutting symbolic family | keep as first-class family |
| `OAN.Core` | compatibility or transitional base root | resolve explicitly |
| `GEL` and `Telemetry.GEL` | unresolved family ownership | assign owner before long-term retention |

## Work Packages

### Phase 1. Constitutional Lock

Objective:

- stop further family drift

Required outputs:

- family constitution
- glossary contract
- family ownership matrix
- dependency contract aligned to the family model

Status:

- family constitution: complete
- glossary contract: complete
- family ownership matrix: complete
- dependency contract: complete

### Phase 2. Ownership Clarification

Objective:

- determine whether each project is:
  - stack composition
  - family-owned implementation
  - cross-cutting symbolic service
  - unresolved or transitional

Actions:

- annotate ambiguous `Oan.*` projects as façade, contract, or implementation
- assign ownership for unresolved roots
- document whether `OAN.Core` is retained, wrapped, or replaced

### Phase 3. Naming Realignment

Objective:

- move misnamed projects into the correct family without disturbing correct family identity

Examples:

- a stack façade may remain under `Oan.*`
- a family-owned implementation should move under the owning family if currently misfiled
- an unresolved acronym should not be preserved without a defined owner

### Phase 4. Dependency Enforcement

Objective:

- enforce family direction rather than merely cataloging it

Actions:

- define allowed high-level family dependencies
- prevent upward bleed from foundational and cross-cutting families where inappropriate
- keep `Oan.Runtime.Headless` as the single stack composition root

### Phase 5. Test Realignment

Objective:

- make test names reflect actual family and stack coverage

Actions:

- distinguish stack-composition tests from family-local tests
- split mixed-family tests where the current naming hides architectural scope

## Avoid

- global rename campaigns that erase family identity
- treating all non-`Oan.*` projects as legacy without ownership analysis
- introducing new ambiguous top-level families
- creating multiple stack composition roots

## Definition Of Success

The convergence is successful when:

- `Oan.*` clearly owns stack composition and stack-level contracts
- `CradleTek.*`, `SoulFrame.*`, and `AgentiCore.*` clearly own their domains
- `SLI.*` is documented and enforced as a transversal symbolic family
- ambiguous roots are either assigned or retired
- runtime, host, and engine names are understandable from family context
- version control reflects intentional lineage instead of accidental duplication

## Immediate Next Step

The next technical step should be a dependency contract aligned to the family constitution so the build can enforce:

- stack composition boundaries
- family ownership boundaries
- cross-cutting `SLI.*` constraints

This contract now exists at:

- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
