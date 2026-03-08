# OPERATIONAL_CONTROL_AND_RECOVERY_PLAN

## Summary

This document defines the first internal control plane for the governance-first Golden Path.

The purpose of this phase is to make the hardened runtime operable under repetition, deferment, replay, and partial failure without widening custody, membrane, or publication boundaries.

This phase adds:

- journal-first status views
- explicit deferred-review actions
- explicit pending-recovery resume actions
- process-local same-loop execution guarding
- restart, concurrency, and malformed-journal fail-safe coverage

## Control-Plane Law

- control-plane views are internal only
- control-plane views are descriptive only
- control-plane views must derive from journal evidence plus typed loop state
- query surfaces do not authorize custody mutation, publication, or recovery by themselves
- resume remains explicit; no deferred or failed loop resumes automatically

## Loop Identity

The canonical loop identity remains:

- `candidateId + provenance`

Normalization rules:

- candidate id serializes in canonical `D` format
- provenance is used exactly as supplied by the bounded membrane path after trimming
- loop-key materialization occurs only through `GovernanceLoopKeys.Create(...)`

No secondary loop-key builders are allowed.

## Internal Query Surfaces

The control plane exposes these internal views:

- `GovernanceLoopStatusView`
- `GovernanceDecisionView`
- `DeferredBacklogItemView`
- `PendingRecoveryItemView`
- `PublicationLaneStatusView`

Required queries:

- status by candidate id plus provenance
- status by loop key
- latest governance decision
- latest stage
- re-engrammitization completion
- pointer publication completion
- checked-view publication completion
- failure code and failure stage
- resume eligibility
- deferred backlog listing
- pending-recovery listing

## Deferred Review Workflow

Deferred means:

- a durable deferred-review record exists
- no Cryptic mutation is authorized
- no Prime publication is authorized
- explicit steward review is required later

Required actions:

- `ListDeferredAsync()`
- `GetDeferredAsync(...)`
- `ApproveDeferredAsync(...)`
- `RejectDeferredAsync(...)`
- `AnnotateDeferredAsync(...)`

Rules:

- deferred items do not auto-requeue
- later review appends a new decision receipt
- prior deferred records remain part of audit history
- annotation is descriptive only and does not authorize downstream acts

## Pending-Recovery Workflow

Pending recovery means:

- governance approval exists
- one or more downstream acts are incomplete or failed
- replay and resume may continue lawfully without repeating completed acts

Required actions:

- `ListPendingRecoveryAsync()`
- `ResumeGovernanceLoopAsync(...)`
- `RetryPublicationLaneAsync(...)`

Rules:

- recovery inspects journal evidence first
- completed re-engrammitization never repeats
- pointer and checked-view retry independently
- invalid resume attempts fail explicitly from typed state

## Same-Loop Execution Guard

`StackManager` remains the orchestration root and now owns a process-local same-loop guard.

Behavior:

- same-loop concurrent execution does not double-run downstream acts
- duplicate callers receive explicit control-state/status responses
- the guard is keyed by canonical loop key
- this phase does not introduce distributed locking

## Malformed Journal Policy

- malformed journal lines never imply completion
- malformed journal lines never authorize recovery
- malformed journal issues surface as internal control-plane errors
- replay continues safely where possible
- affected loops fail safe rather than falsely appearing complete

## Operational Done Criteria

This phase is complete when:

- loop status can be queried internally by candidate or loop identity
- deferred backlog is durable and reviewable
- pending-recovery loops are visible and resumable
- same-loop concurrent execution is suppressed locally
- malformed journal evidence fails safe
- build, tests, and hygiene remain green

## Next Bounded Conformance Item

The next control-plane conformance target is alignment with the operator telemetry visibility lattice in `docs/OPERATOR_TELEMETRY_VISIBILITY_LATTICE.md`.

This does not yet require a richer operator experience or privileged display logic.

It only requires that future status/read models be extended carefully enough to carry descriptive metadata for:

- visibility class
- consent state
- governed access state
- privileged access state
- protection or classification posture
- disclosure eligibility summary

The intent is to keep the live control plane consistent with the later lattice without prematurely implementing tiered disclosure behavior.
