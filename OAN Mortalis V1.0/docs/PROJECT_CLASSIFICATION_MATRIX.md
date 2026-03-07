# PROJECT_CLASSIFICATION_MATRIX

## Purpose

This document classifies the active `OAN Mortalis V1.0` solution as an operational placement matrix.

It exists to bridge:

- system ontology
- authority and mutation law
- Theater law
- project boundaries
- dependency enforcement
- eventual CI policy

This matrix is intentionally finite-vocabulary and enforcement-oriented.

It should answer:

1. Which family and layer owns each project?
2. What authority does that project hold?
3. Which Theaters may it operate in?
4. Which references are lawful or unlawful?
5. What is the remediation path for each out-of-law project?

Assessment date:

- March 6, 2026

## Governing Documents

- `docs/SYSTEM_ONTOLOGY.md`
- `docs/STACK_AUTHORITY_AND_MUTATION_LAW.md`
- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `docs/DEPENDENCY_AUDIT.md`

## Controlled Vocabulary

### Family

- `Oan`
- `CradleTek`
- `SoulFrame`
- `AgentiCore`
- `SLI`
- `Governance`
- `Compatibility`
- `Tooling`
- `Test`

### Layer

- `Composition`
- `StackContract`
- `RuntimeSubstrate`
- `GovernanceBoundary`
- `IdentityKernel`
- `SymbolicTransport`
- `SharedInvariant`
- `CrypticCustody`
- `Observability`
- `Tooling`
- `Test`

### Theater Scope

- `StackWide`
- `CrossTheater`
- `Prime`
- `Cryptic`
- `Dream`
- `TheaterNeutral`

### Authority Class

- `ComposeOnly`
- `RuntimeAuthoritative`
- `LegalityAuthoritative`
- `ProposalAuthoritative`
- `TransportAuthoritative`
- `SharedInvariantAuthoritative`
- `ObserveOnly`
- `Transitional`
- `TestOnly`

### Memory / Identity Impact

- `None`
- `ContextOnly`
- `CrypticCustody`
- `IdentityAdjacent`
- `IdentityBearing`
- `SharedInvariant`
- `Mixed`

### Mutation Rights

- `None`
- `RuntimeOnly`
- `GateOnly`
- `ProposalOnly`
- `TransportOnly`
- `AppendOnly`
- `CrypticCustodyOnly`
- `LocalOverlayOnly`
- `TestOnly`
- `Transitional`

### Remediation Action

- `Keep`
- `ClarifyOwner`
- `RestrictDeps`
- `ShimInterface`
- `SplitSurface`
- `RenameOrExpand`
- `MoveFamily`
- `RetireLegacy`
- `RescopeTests`

### Target State

- `Canonical`
- `Transitional`
- `ExceptionDocumented`

## Violation Codes

| Code | Meaning | Priority Band |
| --- | --- | --- |
| `IV1` | Identity-bearing or shared-invariant surface depends on an out-of-law authority path. | identity violation |
| `GB1` | Governance boundary or foundational layer is bypassed or inverted. | governance bypass |
| `TB1` | Theater-local boundary is blurred or cross-Theater discipline is missing. | theater bleed |
| `SR1` | Symbolic and runtime concerns are coupled in the wrong direction. | symbolic/runtime mixing |
| `NC1` | Naming, acronym, or placement is ambiguous but not immediately unsafe. | naming cleanup |

## Source Project Matrix

| Project | Path | Family | Layer | Theater Scope | Authority Class | Runtime Role | Memory / Identity Impact | Mutation Rights | May Reference | May Not Reference | Observed Violations | Remediation Action | Target State |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Oan.Runtime.Headless` | `src/Oan.Runtime.Headless` | `Oan` | `Composition` | `StackWide` | `ComposeOnly` | canonical Sanctuary stack composition root | `None` | `None` | `Oan` stack contracts and documented family entry surfaces | sibling-family internals acting as alternate stack roots | none | `Keep` | `Canonical` |
| `Oan.Common` | `src/Oan.Common` | `Oan` | `StackContract` | `CrossTheater` | `ComposeOnly` | cross-stack contract and vocabulary support, including custody, membrane, and Prime-derivative law interfaces | `None` | `None` | low-level stack contracts and policy-neutral law interfaces only | identity-bearing stores, runtime authority, Theater-specific implementations | none | `Keep` | `Canonical` |
| `Oan.Spinal` | `src/Oan.Spinal` | `Oan` | `StackContract` | `StackWide` | `ComposeOnly` | deterministic stack substrate contract | `None` | `None` | stack contracts and deterministic abstractions | runtime authorities, shared invariant implementations | `NC1` | `ClarifyOwner` | `Canonical` |
| `Oan.Cradle` | `src/Oan.Cradle` | `Oan` | `Composition` | `StackWide` | `ComposeOnly` | umbrella CradleTek integration and orchestration facade | `ContextOnly` | `None` | `Oan` stack contracts and lawful family entry surfaces | direct cryptic custody or sibling-family private internals | none | `Keep` | `Canonical` |
| `Oan.Place` | `src/Oan.Place` | `Oan` | `StackContract` | `StackWide` | `ComposeOnly` | external module placement boundary | `None` | `None` | stack contracts and placement abstractions | identity-bearing or cryptic custody implementations | none | `Keep` | `Canonical` |
| `Oan.Sli` | `src/Oan.Sli` | `Oan` | `StackContract` | `CrossTheater` | `TransportAuthoritative` | umbrella SLI integration surface | `ContextOnly` | `TransportOnly` | stack contracts and SLI-facing abstractions | direct identity-bearing commit surfaces | none | `Keep` | `Canonical` |
| `Oan.SoulFrame` | `src/Oan.SoulFrame` | `Oan` | `StackContract` | `CrossTheater` | `LegalityAuthoritative` | umbrella SoulFrame integration surface | `ContextOnly` | `GateOnly` | stack contracts and governance abstractions | direct identity-kernel implementation | none | `Keep` | `Canonical` |
| `Oan.AgentiCore` | `src/Oan.AgentiCore` | `Oan` | `StackContract` | `CrossTheater` | `ProposalAuthoritative` | umbrella AgentiCore integration surface | `IdentityAdjacent` | `ProposalOnly` | stack contracts and admitted symbolic surfaces | direct shared-invariant commit surfaces | none | `Keep` | `Canonical` |
| `Oan.Storage` | `src/Oan.Storage` | `Oan` | `StackContract` | `CrossTheater` | `ComposeOnly` | stack storage facade or adapter boundary | `Mixed` | `None` | stack contracts and documented storage abstractions | direct cryptic custody ownership without family clarification | `NC1` | `ClarifyOwner` | `Canonical` |
| `Oan.Fgs` | `src/Oan.Fgs` | `Oan` | `StackContract` | `TheaterNeutral` | `ComposeOnly` | unresolved stack domain surface | `None` | `None` | `Oan.Spinal` and owning-domain contracts | new dependency growth until term is expanded | `NC1` | `RenameOrExpand` | `Transitional` |
| `OAN.Core` | `src/OAN.Core` | `Compatibility` | `StackContract` | `TheaterNeutral` | `Transitional` | legacy base-contract root | `None` | `Transitional` | compatibility dependents only | new active architecture surfaces | `NC1` | `RetireLegacy` | `Transitional` |
| `GEL.Contracts` | `src/GEL.Contracts` | `Governance` | `StackContract` | `CrossTheater` | `ComposeOnly` | policy-neutral shared GEL language surface | `IdentityAdjacent` | `None` | shared DTOs, identifiers, envelopes, and neutral state shapes | storage, governance, telemetry adapters, graph logic, runtime publication | none | `Keep` | `Canonical` |
| `CradleTek.CognitionHost` | `src/CradleTek.CognitionHost` | `CradleTek` | `RuntimeSubstrate` | `CrossTheater` | `RuntimeAuthoritative` | low-level cognition host boundary | `None` | `RuntimeOnly` | substrate and hosting abstractions | direct identity-bearing stores or Prime publication surfaces | none | `Keep` | `Canonical` |
| `CradleTek.Memory` | `src/CradleTek.Memory` | `CradleTek` | `RuntimeSubstrate` | `Cryptic` | `RuntimeAuthoritative` | memory substrate and registry support | `CrypticCustody` | `CrypticCustodyOnly` | CradleTek substrate contracts | Prime publication or legality authority surfaces | none | `Keep` | `Canonical` |
| `CradleTek.Host` | `src/CradleTek.Host` | `CradleTek` | `RuntimeSubstrate` | `CrossTheater` | `RuntimeAuthoritative` | foundational host contract and orchestration core | `ContextOnly` | `RuntimeOnly` | CradleTek substrate, compatibility contracts | direct sibling-family runtime implementations and identity-facing services | `GB1`, `SR1` | `ShimInterface` | `Canonical` |
| `CradleTek.Runtime` | `src/CradleTek.Runtime` | `CradleTek` | `RuntimeSubstrate` | `CrossTheater` | `RuntimeAuthoritative` | family-local runtime activation and hosting | `Mixed` | `RuntimeOnly` | CradleTek substrate and SLI-facing infrastructure seams | direct AgentiCore domain ownership and shared-state observability bypass | `GB1`, `SR1` | `RestrictDeps` | `Canonical` |
| `CradleTek.Public` | `src/CradleTek.Public` | `CradleTek` | `RuntimeSubstrate` | `Prime` | `RuntimeAuthoritative` | Prime-side hosting surface | `ContextOnly` | `RuntimeOnly` | CradleTek host surfaces | direct Cryptic custody or Dream semantics without law | none | `Keep` | `Canonical` |
| `CradleTek.Cryptic` | `src/CradleTek.Cryptic` | `CradleTek` | `CrypticCustody` | `Cryptic` | `RuntimeAuthoritative` | cryptic-side hosting and custody surface | `CrypticCustody` | `CrypticCustodyOnly` | CradleTek host and cryptic storage surfaces | Prime release surfaces and shared invariant authorship | none | `Keep` | `Canonical` |
| `CradleTek.Mantle` | `src/CradleTek.Mantle` | `CradleTek` | `CrypticCustody` | `Cryptic` | `RuntimeAuthoritative` | Mantle / sovereignty-aligned custody service | `IdentityAdjacent` | `CrypticCustodyOnly` | CradleTek host and cryptic custody surfaces | public canonical truth surfaces | `NC1` | `ClarifyOwner` | `Canonical` |
| `SoulFrame.Identity` | `src/SoulFrame.Identity` | `SoulFrame` | `GovernanceBoundary` | `CrossTheater` | `LegalityAuthoritative` | identity-facing legality and selection surface | `IdentityAdjacent` | `GateOnly` | SoulFrame family, compatibility contracts | direct runtime substrate authority or identity-kernel rewrite | `NC1` | `RetireLegacy` | `Canonical` |
| `SoulFrame.Host` | `src/SoulFrame.Host` | `SoulFrame` | `GovernanceBoundary` | `CrossTheater` | `LegalityAuthoritative` | session and workflow host support | `ContextOnly` | `GateOnly` | SoulFrame family, observability abstractions | direct shared-invariant mutation or runtime seizure | `NC1` | `RestrictDeps` | `Canonical` |
| `AgentiCore.Runtime` | `src/AgentiCore.Runtime` | `AgentiCore` | `IdentityKernel` | `CrossTheater` | `ProposalAuthoritative` | family-local runtime shell | `IdentityAdjacent` | `ProposalOnly` | AgentiCore contracts and compatibility surfaces | direct stack composition or shared invariant commit | `NC1` | `RetireLegacy` | `Canonical` |
| `AgentiCore` | `src/AgentiCore` | `AgentiCore` | `IdentityKernel` | `CrossTheater` | `ProposalAuthoritative` | agent cognition, bonded runtime behavior, self-state formation | `IdentityBearing` | `ProposalOnly` | AgentiCore family, admitted SoulFrame gates, SLI interfaces, approved substrate seams | direct shared-invariant commit, broad sibling-family host coupling | `GB1`, `SR1` | `RestrictDeps` | `Canonical` |
| `SLI.Lisp` | `src/SLI.Lisp` | `SLI` | `SymbolicTransport` | `CrossTheater` | `TransportAuthoritative` | symbolic representation library | `None` | `TransportOnly` | symbolic parsing and representation surfaces | runtime authority and identity-kernel internals | none | `Keep` | `Canonical` |
| `SLI.Ingestion` | `src/SLI.Ingestion` | `SLI` | `SymbolicTransport` | `CrossTheater` | `TransportAuthoritative` | symbolic ingestion surface | `ContextOnly` | `TransportOnly` | SLI family, storage-facing and memory-facing interfaces | sibling-family host internals | `GB1`, `TB1` | `ShimInterface` | `Canonical` |
| `SLI.Engine` | `src/SLI.Engine` | `SLI` | `SymbolicTransport` | `CrossTheater` | `TransportAuthoritative` | symbolic engine and routing core | `Mixed` | `TransportOnly` | SLI family, substrate interfaces, admitted shared-state abstractions | direct sibling-family host and identity-kernel internals | `GB1`, `TB1`, `SR1` | `RestrictDeps` | `Canonical` |
| `Telemetry.GEL` | `src/Telemetry.GEL` | `Governance` | `Observability` | `CrossTheater` | `ObserveOnly` | GEL-oriented observability surface | `IdentityAdjacent` | `None` | observability abstractions and read-only governance contracts | direct commit or authority-bearing surfaces | `NC1` | `ClarifyOwner` | `Transitional` |
| `GEL` | `src/GEL` | `Governance` | `SharedInvariant` | `Prime` | `SharedInvariantAuthoritative` | shared invariant identity-bearing domain | `SharedInvariant` | `AppendOnly` | `GEL.Contracts`, governance contracts, admitted observability, compatibility until migrated | direct host ownership or telemetry-defined authority | `IV1`, `GB1` | `SplitSurface` | `Canonical` |
| `EngramGovernance` | `src/EngramGovernance` | `Governance` | `SharedInvariant` | `CrossTheater` | `LegalityAuthoritative` | governed engram policy and admission surface | `IdentityAdjacent` | `GateOnly` | governance contracts, observability, approved storage abstractions | direct runtime host authority and unexplained compatibility sprawl | `GB1`, `NC1` | `ClarifyOwner` | `Canonical` |
| `Data.Cryptic` | `src/Data.Cryptic` | `Governance` | `CrypticCustody` | `Cryptic` | `ObserveOnly` | cryptic data boundary or custody model surface | `CrypticCustody` | `CrypticCustodyOnly` | cryptic custody contracts and storage abstractions | Prime publication and direct shared invariant authorship | `NC1` | `MoveFamily` | `Transitional` |

## Test Project Matrix

| Project | Path | Family | Layer | Theater Scope | Authority Class | Runtime Role | Memory / Identity Impact | Mutation Rights | May Reference | May Not Reference | Observed Violations | Remediation Action | Target State |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Oan.Runtime.IntegrationTests` | `tests/Oan.Runtime.IntegrationTests` | `Test` | `Test` | `StackWide` | `TestOnly` | end-to-end composition verification | `None` | `TestOnly` | canonical stack root and public testing seams | private undocumented runtime shortcuts | none | `Keep` | `Canonical` |
| `Oan.Audit.Tests` | `tests/Oan.Audit.Tests` | `Test` | `Test` | `CrossTheater` | `TestOnly` | governance and contract audit coverage | `None` | `TestOnly` | stack contracts and audit seams | production-only private internals | none | `Keep` | `Canonical` |
| `Oan.Spinal.Tests` | `tests/Oan.Spinal.Tests` | `Test` | `Test` | `TheaterNeutral` | `TestOnly` | deterministic substrate verification | `None` | `TestOnly` | `Oan.Spinal` only | unrelated runtime or identity internals | none | `Keep` | `Canonical` |
| `Oan.Fgs.Tests` | `tests/Oan.Fgs.Tests` | `Test` | `Test` | `TheaterNeutral` | `TestOnly` | unresolved domain coverage | `None` | `TestOnly` | `Oan.Fgs`, `Oan.Spinal` | broad sibling-family reach | `NC1` | `RenameOrExpand` | `Transitional` |
| `Oan.Sli.Tests` | `tests/Oan.Sli.Tests` | `Test` | `Test` | `CrossTheater` | `TestOnly` | symbolic stack and family-local integration coverage | `None` | `TestOnly` | `Oan.Sli`, `SLI.Engine`, `SLI.Ingestion` | identity-kernel or runtime mutation shortcuts | none | `Keep` | `Canonical` |
| `Oan.SoulFrame.Tests` | `tests/Oan.SoulFrame.Tests` | `Test` | `Test` | `CrossTheater` | `TestOnly` | SoulFrame-oriented mixed integration coverage | `None` | `TestOnly` | lawful SoulFrame seams and supporting test doubles | permanent dependence on CradleTek or SLI internals by convenience | `NC1` | `RescopeTests` | `Canonical` |

## Tools Outside The Active Solution

The following project-space tools exist but are not currently part of `Oan.sln`:

- `tools/CorpusGraphAnalysis`
- `tools/CorpusGraphVisualizer`
- `tools/CorpusIndexer`
- `tools/EngramResolver`

These should be classified separately later as `Tooling` surfaces, not mixed into the current solution-law matrix.

## Ranking Rule For Remediation

Apply remediation in this order:

1. `IV1`
2. `GB1`
3. `TB1`
4. `SR1`
5. `NC1`

## Immediate Refactor Queue

### Priority 1

- `GEL`
  - reason: `IV1`, `GB1`
  - action: split shared-invariant contract from host-coupled implementation and remove host-owned authority inversion

### Priority 2

- `CradleTek.Host`
- `CradleTek.Runtime`
- `AgentiCore`
- `SLI.Engine`
- `SLI.Ingestion`
  - reason: governance bypass, Theater bleed, or symbolic/runtime mixing
  - action: replace direct sibling-family references with governed interfaces or braids

### Priority 3

- `Telemetry.GEL`
- `EngramGovernance`
- `Data.Cryptic`
- `Oan.Storage`
- `CradleTek.Mantle`
  - reason: ownership is plausible but still underdefined
  - action: clarify owning family and move or split if necessary

### Priority 4

- `OAN.Core`
- `Oan.Fgs`
- `Oan.Fgs.Tests`
  - reason: compatibility or acronym ambiguity
  - action: expand, document, retire, or replace

## Verdict

The active solution is no longer blocked by missing constitutional language.

The first lawful `GEL` seam now exists in `GEL.Contracts`.

The first explicit source-domain, membrane, and derivative-domain law interfaces now exist in `Oan.Common`.

The active solution is still blocked by a finite set of out-of-law project placements and dependency edges in the remaining `GEL` core and its surrounding governance and runtime surfaces.

That is a much healthier engineering problem.
