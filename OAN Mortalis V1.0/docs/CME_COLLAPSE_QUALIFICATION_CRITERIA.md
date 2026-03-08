# CME_COLLAPSE_QUALIFICATION_CRITERIA

## Purpose

This document defines the bounded qualification model for first-route CME collapse holding in the active runtime.

It answers one question:

- why did a collapse candidate route to `cMoS`, `cGoA`, or remain under `DeferredReview`

It does not define:

- final `MoS` promotion
- Dream eligibility
- discard semantics
- later `cGoA` enrichment promotion

Those remain later-lifecycle work.

## Governing Rule

Qualification evidence explains:

1. what kind of residue was observed
2. which first protected holding destination it used
3. whether review-trigger evidence was present

Qualification evidence does **not** change the live first-route rule in this phase.

The current live route rule remains:

- autobiographical or `SelfGEL`-identified -> `cMoS`
- otherwise -> `cGoA`

`DeferredReview` remains governance-driven in this phase. Qualification triggers are recorded as evidence, not as an independent deferral engine.

## RouteToCMoS Criteria

Use `RouteToCMoS` for residue that is:

- autobiographical
- explicitly `SelfGEL`-identified
- continuity-sensitive
- witness-bearing in protected self-state terms
- plausibly identity-bearing or personification-adjacent

Typical evidence flags:

- `AutobiographicalSignal`
- `SelfGelIdentitySignal`
- `WitnessBearingSignal`

Positive examples:

- candidate engram structure tied to bonded self-continuity
- autobiographical delta that may later enrich continuity
- protected symbolic residue still attached to CME selfhood

Negative examples:

- generic procedural solving trace with no self-binding
- contextual method discovery that can be reused without autobiographical carry
- low-confidence environmental note with no `SelfGEL` linkage

## RouteToCGoA Criteria

Use `RouteToCGoA` for residue that is:

- contextual
- procedural
- skill-bearing
- method-bearing
- generalizable without self-binding

Typical evidence flags:

- `ContextualSignal`
- `ProceduralSignal`
- `SkillMethodSignal`

Positive examples:

- contextual WH5 trace useful for later problem solving
- method refinement that improves future operator or CME performance
- skill-bearing residue that does not define autobiographical identity

Negative examples:

- autobiographical residue requiring protected self-curation
- witness-bearing symbolic structure still tied to `SelfGEL`
- personification-delta material that may later enrich continuity

## DeferredReview Triggers

`DeferredReview` is a **review state**, not a first-route destination.

Deferred review evidence may be present when:

- classification confidence is below threshold
- identity-bearing and contextual signals are mixed
- policy requires adjudication
- evidence is insufficient or conflicted

Current review triggers:

- `LowConfidence`
- `MixedIdentityContext`
- `PolicyReviewRequired`
- `InsufficientEvidence`

Current default threshold:

- confidence `< 0.80` -> record `LowConfidence`

Important rule:

- material still routes to `cMoS` or `cGoA` first, even when review is deferred

## Route-vs-Review Distinction

The model has two axes.

### First-route destination

- `RouteToCMoS`
- `RouteToCGoA`

### Review state

- `None`
- `DeferredReview`

This means the runtime can truthfully distinguish:

- where the residue is held
- whether later review is still required

without conflating holding, retention, and uncertainty.

## Evidence Flags

The current evidence vocabulary is:

- `AutobiographicalSignal`
- `SelfGelIdentitySignal`
- `ContextualSignal`
- `ProceduralSignal`
- `SkillMethodSignal`
- `WitnessBearingSignal`
- `MixedSignal`

Interpretation guidance:

- `MixedSignal` means both identity-bearing and contextual evidence are present
- `MixedSignal` does not independently change route behavior in this phase
- `WitnessBearingSignal` is stronger than generic autobiographical language and indicates protected self-state significance

## Qualification Output

The runtime should now be able to say:

- destination: `cMoS` or `cGoA`
- residue class: autobiographical-protected or contextual-protected
- classification confidence
- evidence flags
- review-trigger evidence
- source subsystem provenance

That is enough for:

- audit
- conformance
- later promotion modeling

without inventing future lifecycle behavior.

## Later Lifecycle Boundary

Qualification evidence in this phase is **not**:

- final continuity retention
- final `MoS` promotion authorization
- Dream eligibility
- discard authority

It is first-route holding evidence only.

## Engram-Forward Attach Points

Later phases may attach richer cognition science here, including:

- salience evaluators
- anomaly detectors
- engram lenses
- contextual dopants
- typed knowledge localization

These are reserved attach points only. They are not part of the current bounded runtime implementation.
