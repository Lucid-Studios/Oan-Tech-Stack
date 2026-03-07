# GEL_SPLIT_PLAN

## Constitutional Trigger

Source:

- `docs/PROJECT_CLASSIFICATION_MATRIX.md`

Current row:

- `GEL`

Violation codes:

- `IV1`
- `GB1`

Reason for elevation:

- the current `GEL` project mixes shared-invariant identity concerns with host-facing publication and telemetry-adjacent behavior
- that creates a constitutional fracture between identity-bearing memory, governance, storage, and observation

## Current Problem

The current `GEL` surface is not just a shared-invariant domain model.

It currently mixes at least four responsibilities:

- identity-bearing and symbolic domain models
- runtime service behavior
- publication or pointer emission through a host-facing store
- telemetry-adjacent event and state surfaces

Evidence in current code:

- `src/GEL/Models/*`
  - engram and atlas models
- `src/GEL/Graphs/*`
  - graph and functor structures
- `src/GEL/Analysis/SheafCohomologyAnalyzer.cs`
  - internal analytic logic
- `src/GEL/Runtime/SheafMasterEngramService.cs`
  - execution planning plus `IPublicStore` publication and telemetry interaction
- `src/GEL/Telemetry/*`
  - telemetry event and state types inside the `GEL` project itself
- `src/GEL/GEL.csproj`
  - direct references to `CradleTek.Host`, `OAN.Core`, and `Telemetry.GEL`

That means the current assembly is not merely archive, contract, or shared-invariant core.

It is simultaneously:

- model owner
- runtime actor
- publisher
- telemetry participant

That is the fracture.

## Desired Constitutional End State

After the split, the following statement should be true:

An identity-bearing GEL object can be:

- described by contracts
- validated by a GEL core
- stored by infrastructure
- observed by telemetry
- interpreted by symbolic systems
- mutated only through governance-approved paths

No single assembly should own all of those concerns concretely.

## Current Responsibility Inventory

| Responsibility | Current Surface | Present Evidence | Constitutional Outcome |
| --- | --- | --- | --- |
| identity-bearing models | `GEL` | `Models/SheafMasterEngram.cs`, `Models/EngramEpistemicModels.cs` | keep, but isolate from runtime and telemetry |
| graph and functor domain logic | `GEL` | `Graphs/*` | keep in GEL core or symbolic adapter, depending on write authority |
| analysis and validation | `GEL` | `Analysis/SheafCohomologyAnalyzer.cs` | keep in GEL core if read-only and invariant-oriented |
| runtime planning and publication | `GEL` | `Runtime/SheafMasterEngramService.cs` | split; publication leaves GEL core |
| telemetry event and state shapes | `GEL` | `Telemetry/SheafCohomologyEvent.cs`, `Telemetry/SheafCohomologyState.cs` | move to telemetry-facing contract or narrow to read-only event shapes |
| append-only observation | `Telemetry.GEL` | `GelTelemetryAdapter.cs` | keep as observe-only witness surface |
| mutation and ledger append | `EngramGovernance` | `Services/LedgerWriter.cs` | keep, narrow, and make it the explicit governed mutation path |
| encryption and bootstrap | `EngramGovernance` | `Services/EncryptionService.cs`, `Services/EngramBootstrapService.cs` | keep only if it serves governance; otherwise split later |
| stewardship and symbolic guidance | `EngramGovernance` | `Services/StewardAgent.cs`, `Services/SymbolicConstructorGuidanceService.cs` | likely too broad; may require later split after GEL disentanglement |
| storage infrastructure | currently blurred | `GEL` publishes through `IPublicStore`; storage implementation not isolated | move behind storage adapter family |

## Proposed Assembly Partition

The preferred partition is:

### 1. `GEL.Contracts`

Owns:

- identifiers
- envelopes
- append and read interfaces
- policy-neutral DTOs
- event shapes that must be shared without carrying implementation

Constraints:

- boring by design
- no runtime convenience logic
- no storage implementation
- no mutation authority

### 2. `GEL.Core`

Owns:

- identity-bearing and shared-invariant GEL domain logic
- append-only invariants
- retrieval semantics
- internal model validation
- read-safe analysis that does not seize runtime or governance authority

Constraints:

- no host publication logic
- no storage adapters
- no telemetry append implementation

### 3. `CradleTek.GEL.Storage`

Owns:

- persistence adapters
- serialization
- indexing backends
- repository implementations

Constraints:

- infrastructure only
- must not decide mutation authority

Note:

- if a stack-facing facade is required, it may later surface through `Oan.Storage`
- storage ownership should still remain infrastructure-local

### 4. `EngramGovernance`

Owns:

- mutation authorization
- ledger append approval
- constitutional guardrails
- privileged transition approval

Constraints:

- must not become the de facto storage implementation
- must not absorb telemetry witness logic

### 5. `Telemetry.GEL`

Owns:

- append-only observation
- diagnostics
- audit projection
- witness records

Constraints:

- observe-only
- no silent normalization
- no direct shared-invariant mutation

### 6. `SLI.GEL.Adapter`

Owns:

- symbolic interpretation surfaces
- query and projection for SLI
- translation into symbolic models
- read-oriented transformation

Constraints:

- read-oriented by default
- must not become a hidden mutation path

## Allowed Reference Graph

Preferred lawful edges:

- `GEL.Core -> GEL.Contracts`
- `CradleTek.GEL.Storage -> GEL.Contracts`
- `CradleTek.GEL.Storage -> GEL.Core`
- `EngramGovernance -> GEL.Contracts`
- `EngramGovernance -> GEL.Core`
- `Telemetry.GEL -> GEL.Contracts`
- `SLI.GEL.Adapter -> GEL.Contracts`
- `SLI.GEL.Adapter -> GEL.Core`

Allowed outer-family consumers:

- `SoulFrame.*` may reference `GEL.Contracts` and governed read seams
- `AgentiCore.*` may reference `GEL.Contracts` and governed proposal-facing seams
- `CradleTek.*` may host storage and read infrastructure seams, but not seize mutation authority

## Forbidden Reference Graph

The split should eliminate or prevent these patterns:

- `GEL.Core -> CradleTek.Host`
- `GEL.Core -> Telemetry.GEL`
- `GEL.Core -> runtime publication helpers`
- `Telemetry.GEL -> GEL mutation services`
- `SLI.GEL.Adapter -> privileged mutation services`
- `CradleTek.GEL.Storage -> governance decision law`
- `EngramGovernance -> concrete storage implementation details when policy-neutral abstractions will do`

## Migration Sequence

### Pass 1. Extract contracts

1. create `GEL.Contracts`
2. move policy-neutral identifiers, envelopes, and shared event shapes
3. rewire callers to contracts first

### Pass 2. Evict infrastructure

1. move storage-facing publication and persistence concerns out of `GEL`
2. create infrastructure-local storage adapters under `CradleTek`
3. keep storage policy-neutral

### Pass 3. Isolate governance

1. narrow `EngramGovernance` to explicit mutation approval and ledger law
2. remove accidental storage or telemetry ownership
3. document steward-facing mutation gates

### Pass 4. Separate observers and interpreters

1. narrow `Telemetry.GEL` to witness-only behavior
2. move symbolic read and projection concerns behind an SLI adapter
3. prevent the old monolith from reforming under new names

## Acceptance Criteria

The `GEL` Priority 1 issue is resolved when all of the following are true:

- engram identity structures are not directly mutated by outer runtime convenience code
- storage backends do not decide authority
- telemetry cannot govern
- symbolic systems cannot silently mutate kernel-adjacent state
- governance is explicit, narrow, and named
- the `GEL` matrix row either disappears into several lawful rows or shrinks to a compliant `GEL.Core`

## Immediate Next Implementation Target

The lowest-risk first implementation move is:

1. extract `GEL.Contracts`
2. move policy-neutral shared types and event shapes there
3. remove direct `Telemetry.GEL` and host-facing pressure from the core domain assembly

That creates a stable language for the rest of the split without forcing the whole refactor at once.

Execution prompt:

- `docs/refactors/GEL_CONTRACTS_EXTRACTION_PROMPT.md`
