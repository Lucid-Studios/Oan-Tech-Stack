# OPERATIONAL_GOLDEN_PATH

## Purpose

This document defines the canonical v1 governance-first runtime loop for the active stack.

It exists to answer one operational question:

- what is the minimum complete loop that proves the core runtime is operational?

## Locked Decisions

- canonical v1 entrypoint: governance cycle
- canonical Prime outcome: pointer publication and checked-view publication are co-equal post-governance outputs
- deferred semantics: persisted for later steward or operator review, not requeued automatically
- governance adjudicator identity: Steward Agent
- CradleTek loop owner: `StackManager`
- StoreRegistry role: service resolver and container only

## Canonical Stage Sequence

1. Cryptic custody provides source-domain state.
2. SoulFrame projects bounded working-state.
3. AgentiCore performs bounded cognition.
4. A return candidate is emitted.
5. Governance adjudicates `Approved`, `Rejected`, or `Deferred`.
6. Only `Approved` authorizes Cryptic re-Engrammitization.
7. Only governed approved outcomes authorize Prime derivative publication.
8. CradleTek orchestrates the loop without becoming custody, membrane, or publication law.

## Stage Ownership

- `CradleTek.Mantle.MantleOfSovereigntyService`
  - Cryptic custody and governed re-Engrammitization gate
- `SoulFrame.Host.SoulFrameHostClient`
  - SoulFrame membrane via `ISoulFrameMembrane`
- `AgentiCore.Services.BoundedMembraneWorkerService`
  - bounded membrane worker stage
- `AgentiCore.Services.AgentiCore`
  - governance-cycle cognition service via `IGovernanceCycleCognitionService`
- `EngramGovernance.Services.StewardAgent`
  - governance adjudicator via `IReturnGovernanceAdjudicator`
- `CradleTek.Public.PublicLayerService`
  - governed Prime publication sink and derivative view surface
- `Oan.Cradle.StackManager`
  - loop-composition root
- `Oan.Cradle.StoreRegistry`
  - service resolver and container only

## Runtime Artifacts

The governance-first loop uses these explicit artifacts:

- `GovernanceCycleStartRequest`
- `GovernanceCycleWorkResult`
- `ReturnCandidateReviewRequest`
- `GovernanceDecisionReceipt`
- `GovernedReengrammitizationRequest`
- `GovernedPrimePublicationRequest`
- `GovernanceGoldenPathResult`

## Success Path

The approved path is:

1. `StackManager` invokes `IGovernanceCycleCognitionService`.
2. AgentiCore obtains bounded SoulFrame projection.
3. AgentiCore performs bounded cognition and emits a return candidate.
4. `StackManager` builds `ReturnCandidateReviewRequest`.
5. `StewardAgent` emits `GovernanceDecisionReceipt` with decision `Approved`.
6. `StewardAgent` authorizes:
   - one `GovernedReengrammitizationRequest`
   - one `GovernedPrimePublicationRequest`
7. `MantleOfSovereigntyService` executes the Cryptic re-Engrammitization act.
8. `PublicLayerService` publishes:
   - pointer derivative
   - checked-view derivative
9. `StackManager` returns `GovernanceGoldenPathResult`.

## Rejection Path

The rejected path is:

1. candidate is reviewed by `StewardAgent`
2. decision receipt is emitted with decision `Rejected`
3. no Cryptic mutation occurs
4. no Prime publication occurs
5. `StackManager` returns a result with no downstream mutation receipts

## Deferred Path

The deferred path is:

1. candidate is reviewed by `StewardAgent`
2. decision receipt is emitted with decision `Deferred`
3. candidate is persisted for later steward or operator review
4. no automatic requeue occurs
5. no Cryptic mutation occurs
6. no Prime publication occurs

## No-Go Boundaries

These actions are forbidden in the Golden Path:

- no direct candidate path to `ReengrammitizeAsync`
- no direct candidate path to Prime publication
- no direct membrane state to Prime publication
- no CradleTek ownership of custody law
- no CradleTek ownership of membrane law
- no AgentiCore authority to mutate Cryptic custody directly
- no SoulFrame authority to publish Prime derivatives directly

## Operational Completion Condition

Core stack v1 is operational when one repeatable runtime path can:

- start from source-domain custody
- project through SoulFrame
- run bounded AgentiCore cognition
- emit a return candidate
- produce an auditable governance decision
- trigger Cryptic re-Engrammitization only on approval
- emit pointer and checked-view Prime outputs only after approval
- run under CradleTek orchestration through `StackManager`
- pass build, tests, and hygiene

## Current Implementation State

The governance-first Golden Path is now implemented as the active v1 operational loop in code and covered by:

- governance adjudication tests
- Golden Path integration tests
- build, test, and hygiene verification
