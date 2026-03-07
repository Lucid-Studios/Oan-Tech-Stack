# CORE_SYSTEMS_MATURATION_PLAN

## Purpose

This document defines the next maturation track for the active stack after the initial build recovery and constitutional pass.

The immediate objective is to mature the three core active families:

- `CradleTek.*`
- `SoulFrame.*`
- `AgentiCore.*`

before substantial new work is pushed into:

- `SLI.*`
- Engineered Cognition implementation
- Lisp-heavy symbolic authoring and refactoring

This is a sequencing document. It exists to prevent the symbolic layer from outrunning the runtime, membrane, and cognition foundations it depends on.

## Strategic Position

The current stack has crossed a useful threshold:

- build hygiene is in place
- constitutional doctrine is in place
- lawful interfaces exist in code
- the Prime derivative path has been narrowed
- the SoulFrame membrane has been narrowed
- the first bounded AgentiCore membrane caller now exists and passes build, test, and hygiene

That means the next maturity work should not primarily be new symbolic language invention.

The next maturity work should be:

- operational stabilization
- boundary hardening
- service ownership clarification
- runtime and cognition discipline

## Why Core Systems Come First

`SLI.*` and Engineered Cognition are the most expressive layers in the stack.

They are also the easiest place to hide unresolved responsibility if:

- CradleTek orchestration is still broad or unclear
- SoulFrame membrane law is still easy to bypass
- AgentiCore working cognition still carries mixed authority

If symbolic growth happens before those foundations are mature, the likely result is not real progress. The likely result is more category drift with better language wrapped around it.

So the sequencing rule is:

**mature runtime, membrane, and bounded cognition first**

then:

**scale symbolic and Lisp-centric work on top of a lawful core**

## Maturation Goal

The target state before major SLI and Engineered Cognition expansion is:

- CradleTek is a stable application and orchestration fabric
- SoulFrame is a narrow and enforced self-state membrane
- AgentiCore is a bounded cognition workspace operating on mediated state
- return-candidate handling remains governed
- working-state handles do not widen into custody or orchestration access

## Family Tracks

### CradleTek Track

Primary role:

- application composition
- swarm coordination
- runtime routing
- lifecycle control
- infrastructure hosting

Near-term goals:

- keep orchestration in CradleTek and out of SoulFrame
- reduce broad convenience flows across Prime and Cryptic surfaces
- clarify which CradleTek services are Prime derivative, Cryptic custody, or orchestration-only
- harden runtime startup, shutdown, and service composition rules

Acceptance markers:

- no new orchestration logic migrates into SoulFrame
- Prime and Cryptic service responsibilities stay separated
- stack composition remains explicit at host and runtime entrypoints

### SoulFrame Track

Primary role:

- self-state membrane
- bounded cognition mediation
- projection and return-candidate intake shaping

Near-term goals:

- hold the current narrowed membrane shape steady
- reject convenience widening around `SessionHandle` and `WorkingStateHandle`
- keep return intake candidate-shaped only
- ensure downstream consumers remain membrane consumers rather than custody reconstructors

Acceptance markers:

- no new custody-shaped fields enter projection models
- no new write-back semantics enter return intake
- no helper path turns membrane handles into broad authority tokens

### AgentiCore Track

Primary role:

- bounded worker cognition
- local reflective operational state
- policy-bound task execution

Near-term goals:

- keep the bounded worker seam narrow
- let broader cognition flows compose around the worker instead of widening it
- watch for authority creep in return handling and state lookup helpers
- keep self-state use explicit, mediated, and bounded

Acceptance markers:

- AgentiCore continues to consume membrane-bounded state without custody widening
- new cognition flow stages do not bypass the membrane
- return-candidate handling remains candidate-only until explicit governance hardening occurs

## Ordered Work Queue

### Phase A. Hold The New Seams

Focus:

- keep the current membrane contracts stable
- keep the bounded worker stable
- keep the thin AgentiCore integration stable

Do:

- treat widening pressure as a signal
- add tests before adding convenience helpers
- reject any helper that turns handles into implicit authority

Do not:

- expand membrane payloads preemptively
- widen the bounded worker for convenience
- add orchestration duties to SoulFrame

Exit:

- the current seam survives routine changes without widening

### Phase B. Downstream Return Governance

Focus:

- harden return-candidate handling after the first real downstream pressure appears

Questions:

- is return still treated as candidate material rather than near-authorized state?
- is provenance still descriptive rather than authority-bearing?
- is any helper becoming a shortcut around governed re-engrammitization?

Likely outputs:

- explicit return-governance contracts
- negative tests around candidate misuse
- clearer handoff from SoulFrame or AgentiCore into governance evaluation

Exit:

- candidate return cannot be mistaken for write authority

### Phase C. Handle-Resolution Restraint

Focus:

- ensure `SessionHandle` and `WorkingStateHandle` remain bounded worker handles

Questions:

- does any lookup recover more than bounded worker-state access?
- does any caller infer source-domain fetch rights from the handle?
- does any integration rehydrate membrane-removed state by convenience?

Likely outputs:

- bounded resolver rules
- negative tests around forged or widened handle use
- explicit prohibition of handle-to-custody shortcuts

Exit:

- handle use remains bounded and reconstructively weak

### Phase D. Runtime Composition Hardening

Focus:

- tighten CradleTek runtime composition after membrane and bounded cognition paths are stable

Questions:

- are service responsibilities explicit at composition roots?
- are Prime derivative, Cryptic custody, and orchestration services still cleanly separated?
- are lifecycle and swarm coordination boundaries staying in CradleTek?

Likely outputs:

- composition cleanup
- startup validation refinement
- clearer service registration and runtime role boundaries

Exit:

- runtime composition is explicit and defensible

### Phase E. Core-System Review Gate

Before major SLI and Lisp-heavy Engineered Cognition work resumes, review whether the following are true:

- CradleTek orchestration is stable
- SoulFrame membrane misuse is difficult and tested
- AgentiCore bounded cognition can grow without immediate authority bleed
- return-candidate handling is governed clearly enough to avoid soft write-back semantics
- handle-resolution pressure is contained

Only after those are true should major effort shift into:

- new `SLI.*` expansion
- larger symbolic protocol work
- major Lisp-centric Engineered Cognition factoring

## What “Ready For SLI Expansion” Means

The core system is ready for the next symbolic phase when:

- the runtime no longer relies on broad cross-layer convenience access
- SoulFrame remains narrow under real use
- AgentiCore can scale bounded cognition stages without widening state authority
- governance around return and custody boundaries is explicit
- symbolic work can target stable interfaces instead of moving system law

## Immediate Next Actions

The next practical actions from this plan are:

1. Hold the current bounded membrane worker and thin AgentiCore integration unchanged unless real pressure appears.
2. Watch downstream return-candidate handling for soft write-authority drift.
3. Watch handle-resolution helpers for membrane-widening behavior.
4. Harden the first real downstream governance or handle-resolution seam only when the pressure is concrete.
5. Defer major SLI and Lisp-heavy Engineered Cognition factoring until the core-system review gate is met.

## Decision Summary

The next maturity track is:

- **CradleTek first**
- **SoulFrame second**
- **AgentiCore third**
- **SLI and Lisp-heavy Engineered Cognition after the core-system review gate**

This is not a delay of symbolic work.

It is the condition required for symbolic work to land on a stable and lawful system core.
