# Next Implementation Chapter: Post-Participation Execution from Participation Packet

Status: Proposed - Structural Chapter Candidate

## Purpose

Advance the governed runtime beyond participation authorization by teaching it
how to evaluate what a packet-bearing occupant or participant may lawfully
execute, enact, or expose as service behavior.

This chapter begins only after the runtime carries one stable:

* `GovernedSeedPostAdmissionParticipationPacket`

The governing invariant is:

> no downstream execution, service behavior, or post-participation action may occur over anything less than a complete post-admission participation packet

---

## Current Achieved State

After `2d37205`, `8be7cdb`, and `95e13cb`, the runtime now carries:

* post-admission participation as a runtime-visible seam
* one stable `GovernedSeedPostAdmissionParticipationPacket` body aggregating
  that seam

This means the next layer no longer has to infer from scattered occupancy and
role-participation fields. It may reason from one complete participation-layer
body.

---

## Why This Chapter Exists

A candidate that has reached the participation packet is still not yet
executing service behavior or operational action.

It is only packet-confirmed as something that may:

* remain at admission
* wait for domain occupancy
* occupy a domain lawfully
* participate in a role lawfully
* return to binding-pending
* refuse

The next question is narrower and more serious:

> what may an occupancy-authorized or role-participation-authorized packet now actually execute, enact, or expose as service behavior?

---

## Required Input

This chapter reasons only over:

* `GovernedSeedPostAdmissionParticipationPacket`

No looser input is admissible.

The packet is the minimum truthful body because it preserves:

* the admission/binding packet that entered participation
* domain occupancy truth
* role participation truth
* unified participation disposition
* participation receipt trace

---

## First Unified Intent

The first pass should remain unified.

Initial code seam:

* `GovernedSeedPostParticipationExecutionContracts.cs`
* `GovernedSeedPostParticipationExecutionService.cs`

Execution authorization and service-behavior authorization should first be
discovered in one service.

They may later split only if the runtime body demands separate lifecycles.

---

## First Execution Questions

The unified service should answer, at minimum:

* does the participation packet carry a disposition compatible with actual
  execution?
* is domain occupancy already authorized?
* is role participation already authorized where role-bearing action is
  required?
* do standing and revalidation remain intact at execution-bearing scope?
* does the packet preserve attribution strongly enough for actual service
  behavior or operational action?

---

## First Service-Behavior Questions

The unified service should also answer:

* what service behavior is lawful within the admitted and occupied scope?
* what remains prohibited despite participation authorization?
* is the requested action trying to outrun the packet's current scope?
* would execution expand service authority beyond what participation lawfully
  established?

---

## First Disposition Family

The first unified seam should likely return one of:

* `RemainAtParticipationPacket`
* `ExecutionPending`
* `ServiceBehaviorAuthorized`
* `ExecutionAuthorized`
* `ReturnToParticipationPending`
* `Refuse`

These are intentionally narrow and discovery-friendly.

---

## Invariants

### Required Input Invariant

> no downstream execution, service behavior, or post-participation action may occur over anything less than a complete post-admission participation packet

### Occupancy Invariant

No execution may be authorized unless occupancy or participation authorization
already supports it.

### Role Invariant

No role-bearing execution may occur without lawful role participation.

### Scope Invariant

Execution must not silently expand beyond the packet's admitted,
revalidated, and attributable scope.

### Refusal Invariant

Any packet that is incomplete, non-revalidated, non-attributable, or
scope-expanding must not advance.

---

## Suggested First Contracts

Initial contract family:

* `GovernedSeedExecutionEligibilityAssessment`
* `GovernedSeedServiceBehaviorAssessment`
* `GovernedSeedPostParticipationExecutionAssessment`
* `GovernedSeedPostParticipationExecutionReceipt`

These should expose only:

* execution truth
* service-behavior truth
* unified disposition truth

They should not re-perform earlier packet construction or participation logic.

---

## Suggested First Service

* `GovernedSeedPostParticipationExecutionService`

Input:

* `GovernedSeedPostAdmissionParticipationPacket`

Output:

* execution eligibility assessment
* service behavior assessment
* unified assessment
* unified receipt

The first pass should remain conservative:

* no hidden promotion
* no execution without occupancy or participation authorization
* no silent scope expansion
* no service behavior from incomplete or contaminated packet state

---

## Witness Plan

### Focused tests

* incomplete participation packet refuses
* occupancy-authorized but execution-thin packet yields `ExecutionPending`
* participation-authorized packet may yield `ServiceBehaviorAuthorized`
* fully clean packet may yield `ExecutionAuthorized`
* scope-expanding packet returns `ReturnToParticipationPending` or `Refuse`

### Integration tests

* runtime can carry the participation packet into the execution seam
* execution receipt is materialized into runtime-facing surfaces
* refusal, pending, and authorized outcomes preserve packet trace

---

## Outcome Target

At the end of this chapter, the governed runtime should be able to:

* accept a `GovernedSeedPostAdmissionParticipationPacket`
* determine whether the packet remains at participation, returns to
  participation-pending, refuses, authorizes service behavior, or authorizes
  actual execution
* emit a truthful execution-layer receipt for downstream use

---

## Shortest Compression

> the participation packet now says what the admitted packet may lawfully do as
> an occupant or participant; the next step is to define what that participant
> may actually execute or enact.

---

## After The Note

Then the first code pass should be:

* `GovernedSeedPostParticipationExecutionContracts.cs`
* `GovernedSeedPostParticipationExecutionService.cs`

That is the clean next descent.
