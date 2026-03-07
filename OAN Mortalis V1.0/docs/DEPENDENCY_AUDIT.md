# DEPENDENCY_AUDIT

## Purpose

This document audits the current project reference graph of the active `OAN Mortalis V1.0` solution against the constitutional family model and dependency contract.

Governing references:

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
- `OAN Mortalis V1.0/docs/PROJECT_CLASSIFICATION_MATRIX.md`

Assessment date:

- March 6, 2026

## Scope

Included:

- source project references under `OAN Mortalis V1.0/src`
- test project references under `OAN Mortalis V1.0/tests`

Excluded:

- package references
- runtime-only reflection wiring
- dynamic loading paths not represented in `.csproj`

## Method

Each project reference edge was classified as one of:

- `allowed`
- `restricted`
- `unresolved`
- `test`

Severity model:

- `P1`: constitution-level dependency violation with high architectural risk
- `P2`: sibling-family coupling that should be reduced or mediated
- `P3`: ownership ambiguity or transitional dependency that blocks stronger enforcement
- `Info`: test-only edge or non-production note

## Executive Summary

Current graph totals:

- `42` allowed production edges
- `8` restricted production edges
- `22` unresolved production edges
- `16` test edges

Overall reading:

- the constitutional model is viable against the current solution
- most `Oan.*` composition edges are structurally consistent
- the highest-risk dependency problems are concentrated in:
  - `CradleTek.Host`
  - `CradleTek.Runtime`
  - `SLI.Engine`
  - `SLI.Ingestion`
  - `AgentiCore`

The largest blocker to stronger enforcement is unresolved ownership for:

- `OAN.Core`
- `GEL`
- `Telemetry.GEL`
- `EngramGovernance`
- `Oan.Fgs`
- `Data.Cryptic`

## Restricted Findings

### P1 Findings

| From | To | Constitutional Concern | Likely Fix |
| --- | --- | --- | --- |
| `CradleTek.Host` | `AgentiCore.Runtime` | foundational infrastructure reaches upward into sibling agent runtime family | move the dependency behind an `Oan.*` or `CradleTek.*` contract seam, or shim then relocate ownership |
| `CradleTek.Host` | `SoulFrame.Identity` | foundational infrastructure reaches upward into sibling operator family | invert control through contract or adapter; avoid direct infrastructure-to-operator dependency |
| `CradleTek.Runtime` | `AgentiCore` | infrastructure runtime reaches upward into sibling agent family | treat as temporary shim or move orchestration upward into `Oan.*` composition |
| `SLI.Engine` | `AgentiCore` | transversal symbolic family depends upward into sibling agent family | introduce adapter or contract seam; symbolic core should not bind directly to agent runtime ownership |
| `SLI.Engine` | `SoulFrame.Host` | transversal symbolic family depends upward into sibling operator family | introduce adapter or stack-owned contract seam |
| `SLI.Ingestion` | `SoulFrame.Host` | transversal symbolic family depends upward into sibling operator family | isolate ingestion contract or move operator-specific logic out of `SLI.*` |

### P2 Findings

| From | To | Constitutional Concern | Likely Fix |
| --- | --- | --- | --- |
| `AgentiCore` | `SoulFrame.Host` | sibling-family coupling between agent runtime and operator family | mediate through shared contract, stack facade, or event seam |
| `AgentiCore` | `SoulFrame.Identity` | sibling-family coupling between agent runtime and operator family | move identity contract into a neutral or stack-owned seam if both families need it |

## Unresolved Findings

These edges cannot be scored strongly yet because ownership is still unresolved.

### Compatibility Root Pressure

`OAN.Core` is still a dependency source for:

- `AgentiCore`
- `AgentiCore.Runtime`
- `CradleTek.Host`
- `SoulFrame.Host`
- `SoulFrame.Identity`
- `Telemetry.GEL`
- `SLI.Engine`
- `EngramGovernance`
- `GEL`

Interpretation:

- `OAN.Core` remains a real compatibility root in the current solution
- stronger family enforcement is blocked until it is classified as:
  - retained compatibility substrate
  - shimmed contract source
  - or rewrite target

### Unowned Acronym Pressure

`GEL` and `Telemetry.GEL` are still referenced by:

- `AgentiCore`
- `CradleTek.Runtime`
- `SoulFrame.Host`
- `SLI.Engine`
- `SLI.Ingestion`
- `EngramGovernance`

Interpretation:

- this is not just naming ambiguity
- it is dependency ambiguity
- the build cannot fully govern these edges until `GEL` has an explicit family owner

### Other Unresolved Surfaces

Ownership is also still unclear for:

- `EngramGovernance`
- `Oan.Fgs`
- `Data.Cryptic`

## Allowed Pattern Summary

The following broad patterns are already healthy:

- `Oan.Runtime.Headless` only composes stack-facing `Oan.*` projects
- `Oan.Cradle`, `Oan.SoulFrame`, `Oan.AgentiCore`, and `Oan.Sli` currently behave as stack-side composition or contract surfaces
- `CradleTek.*` internal references mostly remain within the infrastructure family
- `SLI.*` internal references remain mostly within `SLI.*` and `CradleTek.*`

This means the repo is not structurally chaotic. The violations are concentrated, not universal.

## Test Graph Notes

Test-only edges were not treated as production violations.

Important notes:

- `Oan.Sli.Tests` spans both `Oan.Sli` and `SLI.*`
- `Oan.SoulFrame.Tests` spans `SoulFrame.*`, `SLI.*`, and `CradleTek.*`

This is acceptable for transition coverage, but the naming should eventually distinguish:

- stack-level integration tests
- family-local tests
- mixed-family compatibility tests

## Recommended Remediation Queue

### Queue 1. Remove P1 upward dependencies

Start with:

1. `CradleTek.Host -> SoulFrame.Identity`
2. `CradleTek.Host -> AgentiCore.Runtime`
3. `SLI.Engine -> SoulFrame.Host`
4. `SLI.Engine -> AgentiCore`
5. `SLI.Ingestion -> SoulFrame.Host`
6. `CradleTek.Runtime -> AgentiCore`

Reason:

- these are the clearest constitutional violations
- they drive the most confusion about whether infrastructure and symbolic layers are truly foundational

### Queue 2. Resolve `OAN.Core`

Decide whether `OAN.Core` is:

- a compatibility substrate to retain temporarily
- a contract shim source
- or a rewrite target

Until this is decided, many edges remain hard to score.

### Queue 3. Assign `GEL` ownership

Decide whether `GEL` belongs to:

- `CradleTek.*`
- `SLI.*`
- a stack-owned `Oan.*` telemetry or contract surface
- or a renamed dedicated family

Without this, the telemetry and governance side of the graph remains constitutionally fuzzy.

### Queue 4. Reduce P2 sibling coupling

Focus on:

- `AgentiCore -> SoulFrame.Host`
- `AgentiCore -> SoulFrame.Identity`

These may be valid interactions, but they should not remain informal direct coupling.

### Queue 5. Split mixed-family tests

Create clearer distinction between:

- stack integration tests
- family tests
- compatibility tests

## Shim Guidance For This Audit

Where a direct rewrite is too risky, a shim is acceptable if:

- the correct future owner is known
- the shim narrows the dependency instead of widening it
- an exit path is documented

For this graph, shims are most defensible when:

- replacing direct `SLI.*` references to sibling families
- replacing direct `CradleTek.*` references to sibling families
- extracting neutral contracts from `OAN.Core`

They are not a license to preserve current coupling forever.

## Verdict

The dependency graph is governable now.

The main work is not discovering the problem anymore. It is executing a focused cleanup program:

- remove P1 upward dependencies
- resolve compatibility-root ownership
- assign unresolved acronym families
- then automate enforcement in CI
