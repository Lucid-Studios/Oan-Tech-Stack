# SLI LISP CSHARP TRANSLATIONAL MEMBRANE LAW

## Purpose

This note defines the lawful membrane between `SLI.Lisp` symbolic emission and
the bounded C# runtime surfaces that may later materialize, route, witness,
audit, or persist its outputs.

Its job is not to replace `IUTT`, `SLI`, `SoulFrame`, `AgentiCore`, or
runtime policy with one bridge metaphor.
Its job is narrower:

- define what a symbolic product is at the line-local boundary
- define what it must carry to cross that boundary lawfully
- define what the membrane may decide
- define what downstream runtime may and may not do afterward

This note is the line-local doctrine surface for the next membrane-facing
implementation batch.

## Governing Compression

`C#` is the instrument body that bounds and projects action.
`Lisp` is the threaded symbolic tensioning governed by `IUTT` lawful
intervals.
`EC` is the acting functor that plays across those prepared relations,
producing live cognition within the admissible resonant field.
The membrane is the bridge geometry that governs how symbolic tension lawfully
becomes embodied runtime obligation.

Two prohibitions govern the whole note:

> No symbolic product may bypass the membrane.

> No runtime surface may self-authorize symbolic promotion.

## Governing Read

Use this note with:

- `RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md`
- `AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md`
- `DISCERNMENT_AND_ADMISSIBILITY_LAW.md`
- `FIRST_RUN_CONSTITUTION.md`
- `CHAPTER_NINE_LIVING_AGENTICORE_HOLD.md`
- `PRIME_CRYPTIC_DUPLEX_LAW.md`

## Boundary Statement

The active line should now read this seam in four offices.

### `IUTT`

`IUTT` defines the lawful interval space in which admissible symbolic
transformation may occur.

### `SLI.Lisp`

`SLI.Lisp` is authorized to emit symbolic products under that lawful interval
space.
It is the living symbolic metabolism layer.
It is not by itself authorized to:

- materialize runtime obligation
- mutate canonical persistence
- widen disclosure
- promote itself into `Prime`

### Membrane

The membrane validates, classifies, passports, and decides the disposition of
every symbolic product that seeks to cross from symbolic metabolism into
runtime obligation.

The membrane is therefore:

- a constitutional translator
- an admissibility gate
- a bounded decision surface

It is not merely a serializer, adapter, or transport helper.

### C# Runtime

The C# runtime may only act on symbolic products that are:

- typed
- passported
- membrane-decided
- still within the authority class granted by that decision

The C# runtime may route, receipt, hold, audit, materialize, or later persist
only within the membrane's lawful ceiling.

## Symbolic Product Classes

`product_class` is distinct from `family`.

- `family` says what kind of symbolic thing the product is
- `product_class` says how the runtime is allowed to treat that thing at the
  membrane
- `intent` is the narrower family-local requested use of the product inside its
  class

The current class lattice is:

| Product class | Office | Immediate runtime posture | Persistence ceiling |
| --- | --- | --- | --- |
| `ReadProduct` | observation, posture, witness, comparison | readable only | never direct |
| `CandidateProduct` | proposal, ranking, modulation, admissibility-bearing offer | hold, route, or queue review | audit-only |
| `DirectiveProduct` | bounded materialization or routing request under explicit law | may trigger bounded runtime action if admissible | audit-only unless separately promoted |
| `CollapseProduct` | checkpoint, rupture, or lawful session-level state change | may alter session posture or trigger collapse handling | never direct; requires explicit downstream law |

The class table prevents a common collapse:

- not all symbolic products are actionable
- not all actionable products are materialization candidates
- not all materialization candidates are persistence candidates

## Passport Schema

Every symbolic product that reaches the membrane must carry a constitutional
passport.

The minimum passport surface is:

| Field | Meaning | Minimum rule |
| --- | --- | --- |
| `origin` | emitting Lisp surface, function, or module lineage | must be explicit |
| `family` | symbolic product family | must resolve to a known family |
| `product_class` | membrane treatment class | must resolve to a known class |
| `intent` | family-local requested use | may not outrun class |
| `admissibility` | current admissibility posture | may not be inferred by runtime |
| `contradiction_state` | contradiction or refusal standing | must be explicit |
| `materialization_eligibility` | whether runtime may act now | must be explicit |
| `persistence_eligibility` | whether later persistence is lawful | must be explicit |
| `trace_id` | replay, audit, and witness handle | must be explicit |

Recommended first bounded values are:

- `admissibility`
  - `pending`
  - `admissible`
  - `refused`
- `contradiction_state`
  - `none`
  - `soft`
  - `hard`
- `materialization_eligibility`
  - `no`
  - `restricted`
  - `yes`
- `persistence_eligibility`
  - `never`
  - `audit_only`
  - `promotable`

The passport is not optional metadata.
It is the minimum lawful identity of a symbolic product at the seam.

## Membrane Decision Lattice

The membrane may make only a finite set of decisions.

| Decision | Meaning | Immediate effect | Ceiling |
| --- | --- | --- | --- |
| `accept` | product is lawful in its current bounded form | route or receipt according to passport | no automatic persistence |
| `transform` | product may cross only after bounded normalization or constraint | normalize, redact, or constrain without widening authority | may not self-upgrade admissibility |
| `defer` | product remains candidate-bearing but not presently actionable | hold in cryptic or queue review | no materialization or persistence |
| `refuse` | product may not cross | emit contradiction or refusal record | no downstream action |
| `collapse` | product triggers session-level checkpoint, rupture, or legal close handling | enter collapse lane | no direct canon or persistence write |

This lattice is closed on purpose.
If a proposed membrane action cannot be truthfully named by one of these
decisions, it should remain outside implementation.

## Allowed Downstream Effects

### Immediate lawful effect

If and only if the membrane has issued a lawful decision, the C# runtime may:

- route the product
- receipt the product
- hold the product
- transform the product within the membrane's own bounded rules
- materialize a bounded action only when `materialization_eligibility` allows
  it

### Audit-gated effect

The following are never immediate.
They require later audit-bearing law:

- persistence
- promotion
- cross-session reuse
- exposure to `Prime`-visible canonical surfaces
- durable continuity-bearing carry-forward

### Forbidden effect

The runtime may never:

- self-promote a symbolic product
- silently persist a symbolic product
- infer missing passport fields
- treat `ReadProduct` as a `DirectiveProduct` without membrane re-issue
- mutate canonical `Prime` surfaces directly from membrane ingress

## Hard Prohibitions

The membrane batch should inherit these red lines explicitly.

1. No direct `SLI.Lisp -> persistence`.
2. No direct `SLI.Lisp -> Prime mutation`.
3. No C# runtime surface may infer absent passport fields.
4. No symbolic product may upgrade its own admissibility.
5. No membrane decision may widen disclosure beyond the passport ceiling.
6. No downstream runtime surface may treat advisory symbolic output as
   executable authority merely because the output is coherent.

## Failure Modes

The instrument-body line gives the clearest diagnostic language for failure at
this seam.

### Distorted transfer

The membrane passes a product across in a form that no longer preserves the
lawful symbolic relation it carried.

This is a membrane failure.

### False resonance

The runtime treats a malformed, contradictory, or merely candidate-bearing
product as if it were stable actionable signal.

This is usually a class, passport, or admissibility failure.

### Silent promotion

A symbolic product reaches persistence, canon, or wider runtime authority
without explicit membrane and audit law.

This is a critical constitutional violation.

### Hollow fixation

The runtime materializes a typed surface whose symbolic passport was too weak
to support the action claimed.

This is a bounded-action fraud condition even if the code path compiles.

## Implementation Consequences

This note should force, rather than merely inspire, the first membrane-facing
type family.

The minimum expected implementation family now reads:

- `SymbolicEnvelope`
- `SymbolicProductFamily`
- `SymbolicProductClass`
- `SymbolicIntent`
- `AdmissibilityStatus`
- `ContradictionState`
- `MaterializationEligibility`
- `PersistenceEligibility`
- `MembraneDecision`

The first runtime batch after this note should be limited to:

- typed envelope contracts
- constitutional passport validation
- bounded membrane decision logic
- fail-closed adapters that transcribe raw symbolic ingress into bounded C#
  envelopes
- tests proving no symbolic product can self-promote into persistence or canon

The first passive contract transcription of this family should live in
`src/Sanctuary/Oan.Common/Oan.Common/SymbolicEnvelopeContracts.cs`.
The first bounded validator transcription should live beside it in
`src/Sanctuary/Oan.Common/Oan.Common/SymbolicEnvelopeValidation.cs`.
The first passive post-validation decision surface should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneDecisionPolicy.cs`.
The first raw-ingress transcription surface should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/RawSymbolicEnvelopeAdapter.cs`.
The first bounded downstream dispatch surface should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneDecisionDispatcher.cs`.
The first bounded membrane receiving chamber family should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneLaneSinks.cs`.
The first bounded collective witness surface should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneLaneWitness.cs`.
The first bounded runtime-facing lane read model should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneLaneReadModel.cs`.
The first bounded runtime-facing membrane inspection API should live beside
them in `src/Sanctuary/Oan.Common/Oan.Common/MembraneInspectionApi.cs`.
The first bounded operator-facing membrane projection surface should live
beside them in `src/Sanctuary/Oan.Common/Oan.Common/MembraneViewProjection.cs`.
The first bounded dashboard-facing membrane adapter should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneDashboardAdapter.cs`.
The first bounded UI-facing membrane panel-state binder should live beside
them in `src/Sanctuary/Oan.Common/Oan.Common/MembranePanelStateBinder.cs`.
The first bounded operator-surface membrane adapter should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneOperatorSurfaceAdapter.cs`.
The first bounded UI component membrane binder should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneUiComponentBinder.cs`.
The first bounded membrane view-template adapter should live beside them in
`src/Sanctuary/Oan.Common/Oan.Common/MembraneViewTemplateAdapter.cs`.

The first runtime batch should not claim:

- full continuity law
- direct engram minting from Lisp output
- direct `Prime` mutation
- bypass of audit-bearing promotion surfaces

## Working Summary

The membrane is the first lawful transfer surface between living symbolic
motion and bounded runtime obligation.

It exists so that:

- `SLI.Lisp` may remain plastic without being sovereign
- C# runtime may become executable without inventing its own ontology
- symbolic products arrive with both class and passport
- no symbolic output can counterfeit persistence, authority, or canon merely
  by crossing into typed form
- bounded membrane decisions may enter finite downstream lanes without
  becoming persistence, audit promotion, or `Prime` mutation
- bounded downstream lanes may receive, hold, and witness dispatches in memory
  without becoming persistence, promotion, or canonical exposure
- bounded lane chambers may be collectively witnessed without inferring new
  authority, re-dispatching held state, or widening disclosure
- bounded witness snapshots may be presented to runtime callers through a read
  model that summarizes chamber state without becoming a store, workflow
  engine, or `Prime`-facing disclosure surface
- bounded read-model state may be exposed through an inspection API that
  presents observational membrane state without mutating sinks, reloading
  witness state, or inferring authority from observation
- bounded inspection state may be projected into operator-facing view models
  that improve readability without mutating membrane state, inferring new
  authority, or widening disclosure
- bounded operator-facing view models may be adapted into dashboard-safe cards
  and empty-state surfaces without becoming a control plane, persistence
  surface, or promotion channel
- bounded dashboard state may be bound into UI-facing panel state without
  introducing action, mutation, authority, or widened disclosure
- bounded UI-facing panel state may be adapted into operator-surface sections
  without introducing commands, hidden handles, mutation, persistence, or
  widened disclosure
- bounded operator-surface sections may be bound into renderable UI component
  contracts without introducing action, authority, mutation, persistence, or
  widened disclosure
- bounded UI component contracts may be adapted into renderer-neutral view
  templates without introducing actions, hooks, authority, mutation,
  persistence, or widened disclosure
