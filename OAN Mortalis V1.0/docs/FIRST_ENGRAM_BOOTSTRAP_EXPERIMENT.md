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

No ambient atlas collections are allowed.

That pinned object may come from:

- a repo-local atlas surface
- or a bounded local Atlas-source normalization pass where the canonical external ingest target is the normalized root-to-variant Atlas surface and the supporting symbol/constructor files act only as reconciliation and validation layers

The bootstrap experiment still operates on the pinned canonical `RootAtlas`, not on the raw source-layer files directly.

## Minimum Draft Path

The first lawful path is:

1. start from a pinned `RootAtlas`
2. construct a bounded `EngramDraft`
3. validate it through `IEngramClosureValidator`
4. record whether it is:
   - `BootstrapClosed`
   - `Closed`
   - `NeedsSpecification`
   - `Rejected`

No GEL append or OE/cOE promotion occurs in this phase.

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
- candidate Engram structure
- routed residue
- OE/cOE admission
- `SelfGEL` / `cSelfGEL` effects
- any GEL/cGEL consequence

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
- `EngramDraft` and `Engram` are no longer conflated
- closure validation is explicit, repeatable, and non-heuristic
- the repo can distinguish trace, draft, closed Engram, and downstream governance DTOs

That is enough to support later Atlas -> Engram -> GEL / OE work without rival meaning models.
