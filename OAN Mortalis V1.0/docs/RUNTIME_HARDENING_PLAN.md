# RUNTIME_HARDENING_PLAN

## Purpose

This document defines the v1.1 runtime hardening path for the governance-first Golden Path.

The hardening target is not broader feature growth. It is repeatable runtime evidence, lawful transitions, replay safety, and fault-safe downstream behavior.

## Locked Decisions

- first durability target: internal append-only NDJSON journal
- journal model: single append-only stream grouped logically by loop key during replay
- idempotency key: candidate identity plus provenance
- runtime representation: typed loop state model
- recovery model: explicit `PendingRecovery`
- publication tracking: lane-aware pointer and checked-view status
- deferred semantics: persisted for later steward or operator review, not requeued automatically

## Hardening Law

- receipts are append-only internal evidence
- receipts are not sovereign custody substance
- receipts are not Prime artifacts by default
- later events must never imply missing earlier events
- re-engrammitization and Prime publication remain downstream of governance approval only

## Journal Rules

The governance receipt journal must persist:

- governance decision receipts
- deferred-review backlog records
- re-engrammitization act receipts
- Prime publication act receipts
- recovery and failure receipts

Append ordering:

1. governance decision receipt
2. deferred-review record, if decision is deferred
3. downstream act receipts
4. loop completion or pending-recovery state receipt

Replay grouping:

- group entries by loop key derived from candidate identity and provenance
- rebuild decision state, deferred backlog, act completion, and pending recovery from the journal only

## Typed State Model

The hardened loop uses these runtime states:

- `SourceCustodyAvailable`
- `ProjectionIssued`
- `BoundedCognitionCompleted`
- `ReturnCandidateSubmitted`
- `GovernanceDecisionApproved`
- `GovernanceDecisionRejected`
- `GovernanceDecisionDeferred`
- `CrypticReengrammitizationCompleted`
- `PrimeDerivativePublished`
- `LoopCompleted`
- `PendingRecovery`
- `LoopFailed`

The runtime must reject illegal transitions.

## Failure Semantics

If approval is recorded but re-engrammitization fails:

- record the failure
- enter `PendingRecovery`
- do not publish Prime outputs

If re-engrammitization succeeds but publication fails for one or more lanes:

- record the lane-specific publication failure
- enter `PendingRecovery`
- retry only the missing publication lanes on replay
- never repeat re-engrammitization

If a duplicate candidate arrives with the same loop key:

- replay the journal
- return the already-recorded outcome
- do not repeat downstream acts

## Deferred Backlog

Deferred review is durable backlog, not a soft pause.

Required behavior:

- deferred candidates are journaled
- deferred candidates are replay-restorable
- deferred candidates are not auto-requeued
- later steward or operator review resolves them explicitly
- deferred candidates never mutate Cryptic or publish Prime until later explicit resolution

## Operational Completion Condition

The hardening phase is complete when:

- repeated execution of the same loop key is idempotent
- duplicate approval does not repeat re-engrammitization
- duplicate approval does not repeat already-published Prime lanes
- replay reconstructs state from one append-only journal stream
- `PendingRecovery` captures resumable failures distinctly from terminal failure
- pointer and checked-view lanes are independently tracked and retried
- rejected and deferred outcomes remain non-mutating and non-publishing
- build, tests, and hygiene remain green
