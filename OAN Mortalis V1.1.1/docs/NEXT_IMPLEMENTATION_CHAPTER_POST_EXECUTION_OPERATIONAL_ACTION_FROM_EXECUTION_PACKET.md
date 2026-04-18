# Next Implementation Chapter: Post-Execution Operational Action from Execution Packet

Status: Proposed - Structural Chapter Candidate

## Purpose

Advance the governed runtime beyond post-participation execution by teaching it
how to evaluate what an execution-authorized packet may actually externalize,
instantiate, invoke, or commit as operational action.

This chapter begins only after the runtime carries one stable:

* `GovernedSeedPostParticipationExecutionPacket`

The governing invariant is:

> no downstream operational action or post-execution reasoning may occur over anything less than a complete post-participation execution packet

---

## Current Achieved State

After `808505a` and `88eff12`, the runtime now carries:

* post-participation execution as a runtime-visible seam
* one stable `GovernedSeedPostParticipationExecutionPacket` body aggregating
  that seam

This means the next layer no longer has to infer across scattered execution
surfaces. It may reason from one complete execution-layer body.

---

## Why This Chapter Exists

A candidate that has reached the execution packet is still not yet at actual
operational action.

It is only packet-confirmed as something that may:

* authorize service behavior
* authorize execution
* preserve execution truth in one carried body

The next question is narrower and more serious:

> what execution-authorized packet may now lawfully externalize, instantiate, invoke, or commit as operational action?

The first critical distinction at this seam is:

> authorized effect is not yet committed effect

That separation must remain explicit.

---

## Required Input

This chapter reasons only over:

* `GovernedSeedPostParticipationExecutionPacket`

No looser input is admissible.

The packet is the minimum truthful body because it preserves:

* originating participation-layer identity
* service behavior truth
* execution authorization truth
* unified execution disposition
* execution receipt trace

---

## First Unified Intent

The first pass should remain unified.

Initial code seam:

* `GovernedSeedPostExecutionOperationalActionContracts.cs`
* `GovernedSeedPostExecutionOperationalActionService.cs`

Operational action and service effect should first be discovered in one
service.

They may later split only if the runtime body demands separate lifecycles.

---

## First Operational Action Questions

The unified service should answer, at minimum:

* does the execution packet carry a disposition compatible with operational
  action?
* is execution authorization already satisfied?
* is explicit service authorization present where externalized effect is
  required?
* do standing, revalidation, attribution, and scope remain intact at the point
  of action?
* what remains prohibited even after execution authorization?

---

## First Disposition Family

The first unified seam should likely return one of:

* `RemainAtExecutionPacket`
* `OperationalActionPending`
* `ServiceEffectAuthorized`
* `OperationalActionCommitted`
* `ReturnToExecutionPending`
* `Refuse`

These are intentionally narrow and discovery-friendly.

`ServiceEffectAuthorized` and `OperationalActionCommitted` must not collapse
into one state.
The first means the packet may lawfully externalize a bounded effect.
The second means the effect has actually crossed into committed consequence and
must therefore emit a stronger receipt surface.

---

## Invariants

### Required Input Invariant

> no downstream operational action or post-execution reasoning may occur over anything less than a complete post-participation execution packet

### Authorization Invariant

No operational action may occur without execution authorization.

### Service Effect Invariant

No externalized service effect may occur without explicit service authorization.

### Scope Invariant

Operational action must not outrun standing, revalidation, attribution, or
explicit scope.

### Commitment Invariant

Committed operational action must emit receipt and trace, and must not
self-expand domain, role, or service scope without a new gate.

### Commitment Boundary Invariant

No irreversible, externally meaningful, or propagated effect may be treated as
committed merely because execution authorization exists.
Commitment must remain its own witnessed threshold.

---

## Suggested First Contracts

Initial contract family:

* `GovernedSeedServiceEffectAssessment`
* `GovernedSeedOperationalActionAssessment`
* `GovernedSeedPostExecutionOperationalActionAssessment`
* `GovernedSeedPostExecutionOperationalActionReceipt`

These should expose only:

* service-effect truth
* operational-action truth
* unified disposition truth

They should not re-perform earlier packet construction or execution logic.

The first pass may also need an explicit commitment-support family, even if it
remains internal to the unified seam:

* `GovernedSeedOperationalActionCommitIntent`
* `GovernedSeedOperationalActionCommitAssessment`
* `GovernedSeedOperationalActionCommitReceipt`

These exist to prevent operational action from silently skipping from
authorization to committed consequence.

---

## Suggested First Service

* `GovernedSeedPostExecutionOperationalActionService`

Input:

* `GovernedSeedPostParticipationExecutionPacket`

Output:

* service-effect assessment
* operational-action assessment
* unified assessment
* unified receipt

The first pass should remain conservative:

* no hidden commitment
* no operational action without execution authorization
* no externalized effect without explicit service authorization
* no irreversible action without explicit commitment assessment
* no silent fallback

---

## Witness Plan

### Focused tests

* incomplete execution packet refuses
* service-authorized but not action-ready packet yields `ServiceEffectAuthorized`
* execution-ready but not commit-ready packet yields `OperationalActionPending`
* fully clean packet yields `OperationalActionCommitted`
* recoverable packet yields `ReturnToExecutionPending`

### Integration tests

* runtime can carry the execution packet into the operational-action seam
* operational-action receipt is materialized into runtime-facing surfaces
* refusal and return dispositions preserve packet trace

---

## Outcome Target

At the end of this chapter, the governed runtime should be able to:

* accept a `GovernedSeedPostParticipationExecutionPacket`
* determine whether the candidate remains at execution, returns to
  execution-pending, is refused, authorizes service effect, or commits
  operational action
* emit a truthful operational-action receipt for downstream use

---

## Shortest Compression

> the execution packet is now real; the next step is to define what that
> packet may actually externalize or commit as operational action.

---

## After the Note

Then the first code pass should be:

* `GovernedSeedPostExecutionOperationalActionContracts.cs`
* `GovernedSeedPostExecutionOperationalActionService.cs`

That is the clean next descent.
