# FIRST_ENGRAM_BOOTSTRAP_EXPERIMENT

## Purpose

This document defines the first bootstrap experiment protocol for canonical Atlas and Engram machinery.

It is a protocol artifact only.

It is not:

- a runnable harness
- a GEL append engine
- an OE/cOE mutation workflow
- a replacement for the current Golden Path

## Goal

Prove that the stack can move from:

- pinned Atlas
- to canonical `EngramDraft`
- to bounded closure validation

before any public/shared memory mutation path is attempted.

## Prerequisites

The experiment assumes:

- canonical Atlas and Engram contracts in `GEL.Contracts`
- deterministic `RootAtlas` identity
- explicit symbolic domain constitution
- explicit lemma-root constitution
- bounded `IEngramClosureValidator`
- current governance-first Golden Path remains unchanged
- current `SelfGEL` / `cSelfGEL` law remains unchanged

## Minimum Viable RootAtlas

The bootstrap run must use one pinned `RootAtlas` object with:

- version
- digest
- root entries
- optional refinement edges
- optional domain descriptors

That pinned atlas must interpret root keys as lemma anchors rather than surface-word forms.

No ambient atlas collections are allowed.

The bootstrap run must also use one pinned symbolic constitution object that reserves:

- grammar operator space
- root-native symbolic core space
- governance/meta space
- disciplinary reserved domains
- experimental extension space

That pinned object may come from:

- a repo-local atlas surface
- or a bounded local Atlas-source normalization pass where the canonical external ingest target is the normalized root-to-variant Atlas surface and the supporting symbol/constructor files act only as reconciliation and validation layers

The bootstrap experiment still operates on the pinned canonical `RootAtlas`, not on the raw source-layer files directly.

The first English-facing bootstrap pass must also operate over one curated seed lemma subset:

- `docs/SEED_LEMMA_ROOT_SET.md`
- `public_root/seed/SeedLemmaRoots.json`

That subset is used to prove English -> lemma -> root landing before broader `EngramDraft` generation expands.

## Minimum Draft Path

The first lawful path is:

1. start from a pinned `RootAtlas`
2. prove English intake lands on one admitted seed lemma root
3. allow a bounded fixture-backed overlay only for test-scoped narrative roots that are absent from the canonical seed set
4. construct a bounded `EngramDraft`
5. validate it through `IEngramClosureValidator` only for structurally admissible sentences
6. record whether it is:
   - `BootstrapClosed`
   - `Closed`
   - `NeedsSpecification`
   - `Rejected`

No GEL append or OE/cOE promotion occurs in this phase.

The first translation proof lane is intentionally fixture-backed and limited to three sentences:

- `The Gate remembers its makers.`
- `The hum has increased twelve percent.`
- `The light was the first lie.`

The third sentence stops at lane-level `NeedsSpecification` before structural closure. Ambiguity is surfaced before validator invocation rather than being simulated through a structurally broken draft.

The next bounded proof lane is an internal five-sentence paragraph fixture that reuses the sentence lane unchanged, emits a deterministic `ConstructorGraph`, and records continuity edges only between adjacent sentence predicate roots:

- `remember -> observe` / `continuity:gate`
- `observe -> increase` / `continuity:hum`
- `change -> lie` / `continuity:light`

That paragraph lane still stops the ambiguous sentence at `NeedsSpecification` before structural closure. Paragraph continuity is proved without laundering ambiguity into closure.

## Ambiguity-Resolution Protocol

The bootstrap ambiguity experiment is:

1. ask one intentionally vague question
2. run three context-building turns
3. collapse and return through the current runtime
4. re-ask the same vague question
5. compare whether resolution improved from continuity-bearing structure rather than prompt adjacency

The experiment is valid only if the comparison distinguishes structure-bearing continuity from prompt-neighbor fluency.

## Logging Separation

The protocol must keep the following classes separate:

- symbolic trace
- canonical symbolic constructor triplets
- candidate Engram structure
- routed residue
- OE/cOE admission
- `SelfGEL` / `cSelfGEL` effects
- any GEL/cGEL consequence
- symbolic domain collision or bridge diagnostics

Symbolic trace must not be silently treated as a candidate Engram.

Candidate Engram structure must not be silently treated as GEL admission.

## OE/cOE And Self-State Attach Points

Future attach points exist for:

- `SelfGEL`
- `cSelfGEL`
- OE/cOE admission
- later GEL/cGEL commit gateways

These remain future surfaces in this phase.

The bootstrap experiment only reserves them.

## Success Condition

The bootstrap phase is successful when:

- the `RootAtlas` used for evaluation is deterministic and pinned
- the symbolic domain constitution used for evaluation is deterministic and pinned
- `EngramDraft` and `Engram` are no longer conflated
- the symbolic world is reserved before contextualization generation
- closure validation is explicit, repeatable, and non-heuristic
- the repo can distinguish trace, draft, closed Engram, and downstream governance DTOs

That is enough to support later Atlas -> Engram -> GEL / OE work without rival meaning models.
