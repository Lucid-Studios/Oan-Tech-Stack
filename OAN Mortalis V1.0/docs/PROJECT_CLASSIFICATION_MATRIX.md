# PROJECT_CLASSIFICATION_MATRIX

## Purpose

This document classifies the active `OAN Mortalis V1.0` solution by family ownership, architectural position, dependency posture, and terminology quality.

It is intended to reduce build drift by answering:

- which family owns each project
- whether that project is composition-level, foundational, sibling-domain, or cross-cutting
- whether the project appears to be placed in the correct family
- where dependency and naming pressure suggest future movement

Assessment date:

- March 5, 2026

## Governing Model

This matrix follows the canonical family constitution:

- `Oan.*` is the umbrella composition and stack-level contract family
- `CradleTek.*` is the infrastructure and substrate family
- `SoulFrame.*` is the operator and identity-facing workflow family
- `AgentiCore.*` is the agent runtime family
- `SLI.*` is the cross-cutting symbolic interoperability family

Primary governing documents:

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `docs/NAMESPACE_CONVERGENCE_PLAN.md`

## Naming And Notation Baseline

| Surface | Canonical Target | Policy |
| --- | --- | --- |
| Umbrella composition family | `Oan.*` | Use for stack composition roots, stack-level contracts, and umbrella integration surfaces. |
| Infrastructure family | `CradleTek.*` | Use for hosting, storage, substrate, and low-level runtime services. |
| Operator family | `SoulFrame.*` | Use for operator, review, approval, and identity-facing workflow surfaces. |
| Agent runtime family | `AgentiCore.*` | Use for orchestration, policy-bound task execution, and agent runtime behavior. |
| Symbolic family | `SLI.*` | Use for symbolic protocol, parsing, transform, and symbolic interoperability. |
| Product name in prose | `OAN Mortalis` | Keep uppercase acronym form in product-facing text. |
| Stack composition root | `Oan.Runtime.Headless` | Keep as the single documented stack composition root. |
| JSON field notation | `snake_case` | Keep canonical interchange JSON in `snake_case`. |
| CLR naming | PascalCase | Keep types, members, and namespace segments in PascalCase. |

## Family Ownership Matrix

| Project | Owning Family | Architectural Position | Current Role | Key References | Placement Status | Drift Risk | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Oan.Runtime.Headless` | `Oan.*` | Composition | stack composition root | `Oan.Spinal`, `Oan.Sli`, `Oan.Cradle`, `Oan.SoulFrame`, `Oan.AgentiCore`, `Oan.Place`, `Oan.Storage`, `Oan.Common` | correct | low | Canonical stack entrypoint. |
| `Oan.Common` | `Oan.*` | Stack Contract | cross-stack utility and contract support | none | probably correct | medium | Keep small; avoid turning it into a second base layer. |
| `Oan.Spinal` | `Oan.*` | Stack Contract | deterministic stack substrate contract | none | ambiguous | medium | Could remain umbrella-level substrate contract, but overlaps conceptually with `OAN.Core` and infrastructure concerns. |
| `Oan.Cradle` | `Oan.*` | Composition | stack orchestration surface | `Oan.Sli`, `Oan.Spinal`, `Oan.Common`, `Oan.SoulFrame`, `Oan.AgentiCore` | probably correct | medium | Good fit if treated as umbrella orchestration rather than infrastructure implementation. |
| `Oan.SoulFrame` | `Oan.*` | Stack Contract | umbrella SoulFrame contract or integration surface | `Oan.Spinal`, `Oan.Common` | probably correct | medium | Best used for stack-level SoulFrame integration, not to replace the whole `SoulFrame.*` family. |
| `Oan.AgentiCore` | `Oan.*` | Stack Contract | umbrella AgentiCore integration surface | `Oan.Sli`, `Oan.Spinal`, `Oan.SoulFrame` | probably correct | medium | Best used for stack-level integration, not family-local agent implementation. |
| `Oan.Sli` | `Oan.*` | Stack Contract | umbrella SLI integration surface | `Oan.Spinal`, `Oan.Common`, `Oan.SoulFrame` | ambiguous | medium | May be acceptable as a stack-facing SLI contract surface, but must not erase `SLI.*` family ownership. |
| `Oan.Place` | `Oan.*` | Stack Contract | stack-level external module placement boundary | `Oan.Spinal`, `Oan.Cradle` | probably correct | medium | Keep if it remains stack-facing and not infrastructure-local. |
| `Oan.Storage` | `Oan.*` | Stack Contract | stack-level storage contract or adapter façade | `Oan.Spinal`, `Oan.SoulFrame`, `Oan.Cradle` | ambiguous | medium | Storage ownership may belong partly in `CradleTek.*`; clarify whether this is façade or implementation. |
| `Oan.Fgs` | `Oan.*` | Stack Domain | focused stack domain module | `Oan.Spinal` | unclear | medium | Term needs explicit expansion and owning-domain definition. |
| `CradleTek.Host` | `CradleTek.*` | Foundational Infrastructure | host contract and orchestration core | `OAN.Core`, `AgentiCore.Runtime`, `SoulFrame.Identity` | correct | medium | Valid as infrastructure family surface if dependency direction is tightened. |
| `CradleTek.Runtime` | `CradleTek.*` | Foundational Infrastructure | infrastructure runtime activation service | `AgentiCore`, `CradleTek.CognitionHost`, `CradleTek.Host`, `SLI.Engine`, `Telemetry.GEL` | ambiguous | high | Acceptable as family-local runtime, but must not compete with `Oan.Runtime.Headless` as stack root. |
| `CradleTek.CognitionHost` | `CradleTek.*` | Foundational Infrastructure | cognition host interface or service hub | none | correct | low | Good low-level infrastructure position. |
| `CradleTek.Memory` | `CradleTek.*` | Foundational Infrastructure | memory substrate or registry service | `CradleTek.CognitionHost` | correct | medium | Reasonable infrastructure ownership. |
| `CradleTek.Public` | `CradleTek.*` | Foundational Infrastructure | public operational layer service | `CradleTek.Host` | probably correct | medium | Valid if the public operational surface is infrastructure-hosted. |
| `CradleTek.Cryptic` | `CradleTek.*` | Foundational Infrastructure | cryptic operational layer service | `CradleTek.Host` | probably correct | medium | Valid if cryptic persistence is infrastructure-owned. |
| `CradleTek.Mantle` | `CradleTek.*` | Foundational Infrastructure | governance or sovereignty service | `CradleTek.Host` | unclear | medium | Needs sharper definition to justify infrastructure ownership. |
| `SoulFrame.Identity` | `SoulFrame.*` | Sibling Domain | identity models and services | `OAN.Core` | correct | medium | Good family ownership. |
| `SoulFrame.Host` | `SoulFrame.*` | Sibling Domain | session or workflow host support | `OAN.Core`, `Telemetry.GEL` | probably correct | medium | Valid if it is family-local workflow hosting rather than stack composition. |
| `AgentiCore` | `AgentiCore.*` | Sibling Domain | agent cognition and orchestration service | `CradleTek.CognitionHost`, `CradleTek.Host`, `CradleTek.Memory`, `EngramGovernance`, `GEL`, `OAN.Core`, `SoulFrame.Identity`, `Telemetry.GEL`, `SLI.Ingestion`, `SoulFrame.Host` | correct but over-coupled | high | Real family ownership, but dependency fan-in is too broad. |
| `AgentiCore.Runtime` | `AgentiCore.*` | Sibling Domain | family-local runtime service | `OAN.Core` | correct | medium | Acceptable as family-local runtime if kept distinct from stack composition. |
| `SLI.Engine` | `SLI.*` | Cross-Cutting | symbolic engine | `AgentiCore`, `CradleTek.CognitionHost`, `CradleTek.Host`, `CradleTek.Memory`, `GEL`, `OAN.Core`, `SLI.Lisp`, `SoulFrame.Host` | correct but over-coupled | high | Legitimate cross-family engine; needs upward dependency discipline. |
| `SLI.Ingestion` | `SLI.*` | Cross-Cutting | symbolic ingestion services | `CradleTek.Memory`, `GEL`, `SoulFrame.Host` | correct | medium | Good cross-cutting ownership if kept independent from sibling-family internals. |
| `SLI.Lisp` | `SLI.*` | Cross-Cutting | symbolic representation library | none | correct | low | Good representation-specific package. |
| `Telemetry.GEL` | unresolved | Cross-Cutting | telemetry support | `OAN.Core` | unclear | high | Needs owning family and full acronym expansion. |
| `GEL` | unresolved | Cross-Cutting | domain or telemetry-linked surface | `CradleTek.Host`, `OAN.Core`, `Telemetry.GEL` | unclear | high | Unexplained acronym and unclear family owner. |
| `EngramGovernance` | unresolved | Cross-Cutting | governance around engrams | `CradleTek.Memory`, `CradleTek.Host`, `OAN.Core`, `Telemetry.GEL` | unclear | medium | Domain is clear; family ownership is not. |
| `Data.Cryptic` | unresolved | Foundational Infrastructure | cryptic data surface | none | unclear | medium | Likely infrastructure-adjacent, but current family naming is incomplete. |
| `OAN.Core` | compatibility root | Foundational Infrastructure | legacy or transitional base contracts | none | transitional | high | Casing and role overlap with `Oan.Spinal` need an explicit resolution. |

## Test Matrix

| Project | Intended Coverage | Current References | Status | Notes |
| --- | --- | --- | --- | --- |
| `Oan.Runtime.IntegrationTests` | stack composition | `Oan.Runtime.Headless` | correct | Best canonical end-to-end suite. |
| `Oan.Audit.Tests` | stack governance and contract audits | `Oan.Common`, `Oan.SoulFrame`, `Oan.Sli`, `Oan.Spinal`, `Oan.Storage`, `Oan.Cradle` | correct | Good cross-stack governance surface. |
| `Oan.Spinal.Tests` | stack substrate tests | `Oan.Spinal` | correct | Clean ownership. |
| `Oan.Fgs.Tests` | stack domain tests | `Oan.Fgs`, `Oan.Spinal` | probably correct | Needs `FGS` glossary clarity. |
| `Oan.Sli.Tests` | symbolic stack tests | `Oan.Sli`, `SLI.Engine`, `SLI.Ingestion` | mixed | Tests both umbrella and family-local symbolic surfaces. |
| `Oan.SoulFrame.Tests` | SoulFrame stack and family behavior | `SoulFrame.Host`, `SLI.Engine`, `CradleTek.Memory` | mixed | Rename or re-scope to state whether this is family-local or stack integration coverage. |

## Main Structural Findings

### 1. The multi-family model is viable

The solution does not need to collapse everything into `Oan.*`.

A stronger reading is:

- `Oan.*` as umbrella stack composition
- `CradleTek.*` as infrastructure family
- `SoulFrame.*` as operator family
- `AgentiCore.*` as agent runtime family
- `SLI.*` as transversal symbolic family

### 2. The current risk is boundary blur, not family plurality

The actual drift problems are:

- unresolved ownership for `GEL`, `Telemetry.GEL`, `EngramGovernance`, and `Data.Cryptic`
- ambiguous placement of some `Oan.*` projects that may be façade surfaces rather than true owners
- host and runtime names that are not always qualified by family purpose

### 3. `Oan.*` should own composition, not everything

The safest interpretation is:

- `Oan.Runtime.Headless` remains the only stack composition root
- `Oan.*` projects can own stack contracts and umbrella integration façades
- family-local implementation can remain under `CradleTek.*`, `SoulFrame.*`, `AgentiCore.*`, and `SLI.*`

### 4. `SLI.*` is a cross-cutting family, not a child family

`SLI.*` should serve:

- `CradleTek.*`
- `SoulFrame.*`
- `AgentiCore.*`
- `Oan.*` composition

It should avoid upward dependence on sibling-family internals wherever possible.

## Realistic Maintenance Path

### Immediate

- keep `Oan.Runtime.Headless` as the only documented stack composition root
- adopt the family constitution as the governing model
- stop describing `CradleTek.*`, `SoulFrame.*`, `AgentiCore.*`, and `SLI.*` as legacy by default
- define the owner for unresolved families and acronyms

### Near-Term

- write a dependency contract aligned to the family constitution
- clarify which `Oan.*` projects are stack façades versus family-owned implementations
- rename or document ambiguous runtime, host, core, and engine surfaces
- split mixed-family tests where the current naming obscures coverage intent

### Mature-State

- one umbrella composition family
- clear sibling-family ownership
- one transversal symbolic family
- one documented stack composition root
- family-qualified runtime names with low ambiguity

## Verdict

The solution is healthier when treated as a composed family architecture rather than a failed single-family architecture.

The next governance step is not forced collapse into `Oan.*`. It is to enforce family ownership, dependency direction, and stack-composition boundaries under the constitutional family model.
