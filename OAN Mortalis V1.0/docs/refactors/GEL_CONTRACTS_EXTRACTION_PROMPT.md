# GEL_CONTRACTS_EXTRACTION_PROMPT

## Objective

Extract `GEL.Contracts` as a policy-neutral shared assembly.

## Scope

- create a new `GEL.Contracts` project
- move only policy-neutral shared GEL types and interfaces
- update existing dependent projects to reference `GEL.Contracts` for shared shapes
- preserve current behavior

## Out Of Scope

Do not move:

- storage implementations
- governance logic
- telemetry adapter logic
- runtime publication or orchestration logic
- graph traversal or analysis services
- symbolic interpretation or projection logic

## Constraints

`GEL.Contracts` must remain dependency-light.

It must not take dependencies on:

- `CradleTek.Host`
- `Telemetry.GEL`
- `EngramGovernance`
- `SLI.*`
- runtime publication services
- storage adapters

It may reference only the minimum shared framework surface needed to compile.

## Extraction Rule

Move a type into `GEL.Contracts` only if it satisfies all of the following:

1. it expresses shared meaning, shape, or identity vocabulary
2. it does not decide who may mutate it
3. it does not know where it is stored
4. it does not require runtime orchestration or publication services
5. governance, telemetry, and future symbolic adapters can all reference it without constitutional confusion

If a type performs decisions, persistence, orchestration, mutation, graph computation, or projection logic, leave it out.

## Candidate Sources

Primary source:

- `src/GEL/Models`

Candidate first-pass types:

- `EngramEpistemicClass`
- `PropositionalEngramLevel`
- `PropositionalEngram`
- `ProceduralEngram`
- `PerspectivalEngram`
- `ParticipatoryEngram`
- `DomainMorphism`
- `LocalSymbolAtlas`

Conditional candidates:

- `ConsistencyRule`
- `ConsistencyRules`

Condition:

- move only if `ConsistencyRules.HasRule(...)` is treated as harmless vocabulary-side helper logic
- if that helper is judged to be domain logic rather than shape logic, move only `ConsistencyRule` and leave `ConsistencyRules` in the core

Conditional telemetry-state candidate:

- `SheafCohomologyState`

Condition:

- move only if it is used as a shared immutable state shape
- do not move `SheafCohomologyEvent` in pass 1 unless the `ITelemetryEvent` dependency can be removed from the contract boundary

## Explicit Exclusions

Do not move in pass 1:

- `SheafMasterEngram`
  - currently depends on `GEL.Graphs` and `GEL.Telemetry`
- `SheafCohomologyAnalyzer`
- `ConstructorGraph`
- `ProceduralFunctorGraph`
- `SheafMasterEngramService`
- any class under `src/GEL/Runtime`
- any class under `src/GEL/Analysis`
- any telemetry adapter
- any host-facing publication logic

## Current Evidence

Evidence for mixed concerns in the current core:

- [`src/GEL/Runtime/SheafMasterEngramService.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\GEL\Runtime\SheafMasterEngramService.cs)
  - mixes execution planning with `IPublicStore` publication and telemetry usage
- [`src/Telemetry.GEL/GelTelemetryAdapter.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\Telemetry.GEL\GelTelemetryAdapter.cs)
  - observe-only witness surface, should remain outside contracts
- [`src/EngramGovernance/Services/LedgerWriter.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\EngramGovernance\Services\LedgerWriter.cs)
  - governed mutation path, should remain outside contracts

## Implementation Sequence

### Phase 1. Create Assembly

1. create `src/GEL.Contracts/GEL.Contracts.csproj`
2. target `.NET 8`
3. keep the project free of concrete runtime-family dependencies

### Phase 2. Extract Safe Types

1. move the first-pass candidate types from `src/GEL/Models`
2. preserve namespaces intentionally
3. prefer `GEL.Contracts` or `GEL.Contracts.Models` namespace shape
4. keep types immutable or DTO-style

### Phase 3. Rewire References

Update consumers so they reference `GEL.Contracts` for shared shapes:

- `GEL`
- `EngramGovernance`
- `Telemetry.GEL`
- future `SLI.GEL.Adapter` surfaces

Do not change behavior at this step.

### Phase 4. Freeze Boundary

After extraction:

- new shared GEL-facing types may enter `GEL.Contracts` only if they remain policy-neutral
- no helper logic, mappers, storage helpers, or governance checks should be added there

## Acceptance Criteria

The extraction is complete when:

- `GEL.Contracts` exists as a separate assembly
- extracted types compile there cleanly
- dependent projects can reference shared GEL types without taking a full `GEL` runtime dependency
- no authority logic is moved into contracts
- no storage or telemetry implementation is moved into contracts
- the solution still builds and tests
- `PROJECT_CLASSIFICATION_MATRIX.md` can classify `GEL.Contracts` as a lawful low-dependency contract surface

## Failure Modes To Avoid

Do not let `GEL.Contracts` become a second monolith.

Reject additions such as:

- helper services
- validation services
- storage repositories
- telemetry adapters
- convenience mappers
- graph utilities
- runtime publication logic
- policy checks hidden as “shared helpers”

## Expected Follow-up

After `GEL.Contracts` exists and callers are rewired, the next split should be one of:

- storage extraction into a CradleTek-owned GEL storage surface
- governance narrowing inside `EngramGovernance`

Do not attempt both before the contract boundary is stable.
