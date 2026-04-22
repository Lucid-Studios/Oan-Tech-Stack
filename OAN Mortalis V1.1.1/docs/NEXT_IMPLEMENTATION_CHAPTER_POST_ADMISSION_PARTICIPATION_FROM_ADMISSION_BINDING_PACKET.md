# Next Implementation Chapter: Post-Admission Participation from Admission/Binding Packet

## Proposed - Structural Chapter Candidate

## Purpose

Advance the governed runtime beyond actual admission/binding by teaching it how
to evaluate what an admitted-and-bound packet may lawfully participate in,
occupy, or execute.

This chapter begins only after the runtime carries one stable:

* `GovernedSeedDomainAdmissionRoleBindingPacket`

The governing invariant is:

> no downstream participation, occupancy, or post-admission reasoning may occur over anything less than a complete domain admission / role-binding packet

---

## Current Achieved State

After `0fc08e9`, `d9a871f`, and `03acdde`, the runtime now carries:

* actual domain-admission and role-binding truth as a first-class event
* one stable `GovernedSeedDomainAdmissionRoleBindingPacket` body aggregating
  that truth

This means the next seam no longer has to infer from distributed runtime
fields. It may reason from one complete admission-bearing body.

---

## Why This Chapter Exists

A candidate that has reached the admission/binding packet is still not yet
participating as an occupant or actor inside the admitted body.

It is only packet-confirmed as something that has:

* been actually domain-admitted or refused
* remained role-pending or become role-bound
* been witnessed and carried as one admission-bearing packet

The next question is narrower and more operational:

> what may an admitted-and-bound packet now lawfully participate in, occupy, or execute?

---

## Required Input

This chapter reasons only over:

* `GovernedSeedDomainAdmissionRoleBindingPacket`

No looser input is admissible.

The packet is the minimum truthful body because it preserves:

* originating domain/role gating packet identity
* domain-admission truth
* role-binding truth
* unified admission/binding disposition
* admission/binding receipt trace

---

## First Unified Intent

The first pass should remain unified.

Initial code seam:

* `GovernedSeedPostAdmissionParticipationContracts.cs`
* `GovernedSeedPostAdmissionParticipationService.cs`

Participation, occupancy, and role-bearing execution should first be
discovered in one service.

They may later split only if the runtime body demands separate lifecycles.

---

## First Participation Questions

The unified service should answer, at minimum:

* does the admission/binding packet disposition permit participation at all?
* what operational participation follows actual domain admission?
* what occupancy is now permitted inside the admitted body?
* what remains prohibited even after admission?
* what distinguishes admitted from occupancy-ready?
* what distinguishes occupancy-ready from role-participation-ready?

---

## First Disposition Family

The first unified seam should likely return one of:

* `RemainAtAdmissionPacket`
* `DomainOccupancyPending`
* `DomainOccupancyAuthorized`
* `RoleParticipationAuthorized`
* `ReturnToBindingPending`
* `Refuse`

These are intentionally narrow and discovery-friendly.

---

## Invariants

### Required Input Invariant

> no downstream participation, occupancy, or post-admission reasoning may occur over anything less than a complete domain admission / role-binding packet

### Admission Invariant

No participation may occur without actual domain admission.

### Role Invariant

No role-bearing participation may occur without actual role binding.

### Standing Invariant

Participation and occupancy may not outrun current standing or revalidation.

### Attribution Invariant

Post-admission participation must preserve attribution and receipt.

### Scope Invariant

Post-admission participation must not self-expand scope without a new gate.

---

## Suggested First Contracts

Initial contract family:

* `GovernedSeedPostAdmissionParticipationAssessment`
* `GovernedSeedDomainOccupancyAssessment`
* `GovernedSeedRoleParticipationAssessment`
* `GovernedSeedPostAdmissionParticipationReceipt`

These should expose only:

* participation truth
* occupancy truth
* role-participation truth
* unified disposition truth

They should not re-perform earlier admission or packet-construction logic.

---

## Suggested First Service

* `GovernedSeedPostAdmissionParticipationService`

Input:

* `GovernedSeedDomainAdmissionRoleBindingPacket`

Output:

* domain occupancy assessment
* role participation assessment
* unified participation assessment
* unified participation receipt

The first pass should remain conservative:

* no hidden promotion
* no role participation without role binding
* no occupancy from non-admitted packet state
* no silent scope expansion
* no silent fallback

---

## Witness Plan

### Focused tests

* incomplete admission/binding packet refuses
* non-admitted packet refuses participation
* domain-admitted but role-pending packet yields `DomainOccupancyPending` or
  `DomainOccupancyAuthorized`
* fully clean admitted-and-bound packet yields `RoleParticipationAuthorized`
* recoverable packet may return `ReturnToBindingPending`

### Integration tests

* runtime can carry the admission/binding packet into the post-admission seam
* participation receipt is materialized into runtime-facing surfaces
* refusal and return dispositions preserve packet trace

---

## Outcome Target

At the end of this chapter, the governed runtime should be able to:

* accept a `GovernedSeedDomainAdmissionRoleBindingPacket`
* determine whether the packet remains at the admission layer, becomes
  occupancy-ready, becomes role-participation-ready, returns to binding-pending,
  or refuses
* emit a truthful post-admission participation receipt for downstream use

---

## Shortest Compression

> the packet now says what the candidate is admitted into; the next step is to
> define what that admitted packet may lawfully do.
