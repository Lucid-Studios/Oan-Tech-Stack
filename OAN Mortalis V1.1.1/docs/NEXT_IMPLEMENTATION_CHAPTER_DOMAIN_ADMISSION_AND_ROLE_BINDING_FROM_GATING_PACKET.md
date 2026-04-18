# Next Implementation Chapter: Domain Admission and Role Binding from Gating Packet

Status: Proposed - Structural Chapter Candidate

## Purpose

Advance the governed runtime beyond approachability by teaching it how to evaluate whether a packet-bearing candidate may be:

* actually admitted into a domain
* lawfully bound to a role within that admitted domain

This chapter begins only after the runtime carries one stable:

* `GovernedSeedDomainRoleGatingPacket`

The governing invariant is:

> no domain-bearing admission or role-binding reasoning may occur over anything less than a complete domain/role gating packet

---

## Current Achieved State

After `f2c87c3` and `b30a01f`, the runtime now carries:

* pre-domain governance as a packet
* domain/role approachability as a runtime-visible gate
* one stable `GovernedSeedDomainRoleGatingPacket` body aggregating that gate

This means the next seam no longer has to infer from distributed fields. It may reason from one complete approachability-layer body.

---

## Why This Chapter Exists

A candidate that has reached the gating packet is still not yet:

* domain-admitted
* role-bound

It is only packet-confirmed as something that may approach those thresholds.

The next question is narrower and more serious:

> when does a gating-packet candidate cross from approachability into actual domain admission, and when may that admission lawfully become bound role?

---

## Required Input

This chapter reasons only over:

* `GovernedSeedDomainRoleGatingPacket`

No looser input is admissible.

The packet is the minimum truthful body because it preserves:

* originating pre-domain governance
* domain eligibility truth
* role eligibility truth
* unified gating disposition
* gating receipt trace

---

## First Unified Intent

The first pass should remain unified.

Initial code seam:

* `GovernedSeedDomainAdmissionRoleBindingContracts.cs`
* `GovernedSeedDomainAdmissionRoleBindingService.cs`

Domain admission and role binding should first be discovered in one service.

They may later split only if the runtime body demands separate lifecycles.

---

## First Domain Admission Questions

The unified service should answer, at minimum:

* does the gating packet carry a disposition compatible with actual admission?
* is domain eligibility already satisfied?
* does the packet remain free of cryptic authority contamination?
* is standing-consistent carry preserved at domain-bearing scope?
* is burden attributable strongly enough to admit the candidate into a domain?

---

## First Role Binding Questions

The unified service should also answer:

* does the packet expose enough role-relevant structure to bind a role?
* does the role remain lawful inside the admitted domain?
* can responsibility attach at role scope without ambiguity?
* is role binding premature even if domain admission is possible?

---

## First Disposition Family

The first unified seam should likely return one of:

* `RemainAtGatingPacket`
* `DomainAdmittedRolePending`
* `DomainAndRoleBound`
* `ReturnToPreDomainCarry`
* `Refuse`

These are intentionally narrow and discovery-friendly.

---

## Invariants

### Required Input Invariant

> no domain-bearing admission or role-binding reasoning may occur over anything less than a complete domain/role gating packet

### Domain Invariant

No candidate may be domain-admitted if the packet still contains cryptic authority bleed or unresolved standing inconsistency.

### Role Invariant

Role binding may only be considered after actual domain admission is at least minimally satisfied.

### Refusal Invariant

Any packet that is incomplete, contaminated, or non-attributable at domain-bearing scope must not advance.

---

## Suggested First Contracts

Initial contract family:

* `GovernedSeedDomainAdmissionAssessment`
* `GovernedSeedRoleBindingAssessment`
* `GovernedSeedDomainAdmissionRoleBindingAssessment`
* `GovernedSeedDomainAdmissionRoleBindingReceipt`

These should expose only:

* admission truth
* role-binding truth
* unified disposition truth

They should not re-perform earlier packet construction or approachability logic.

---

## Suggested First Service

* `GovernedSeedDomainAdmissionRoleBindingService`

Input:

* `GovernedSeedDomainRoleGatingPacket`

Output:

* domain admission assessment
* role binding assessment
* unified assessment
* unified receipt

The first pass should remain conservative:

* no hidden promotion
* no role without admission
* no domain admission from contaminated packet state
* no silent fallback

---

## Witness Plan

### Focused tests

* incomplete gating packet refuses
* contaminated gating packet refuses
* domain-admissible but role-incomplete packet yields `DomainAdmittedRolePending`
* fully clean packet yields `DomainAndRoleBound`
* recoverable packet may return `ReturnToPreDomainCarry`

### Integration tests

* runtime can carry the gating packet into the admission/binding seam
* admission/binding receipt is materialized into runtime-facing surfaces
* refusal and return dispositions preserve packet trace

---

## Outcome Target

At the end of this chapter, the governed runtime should be able to:

* accept a `GovernedSeedDomainRoleGatingPacket`
* determine whether the candidate remains at the gating layer, returns to pre-domain carry, is refused, becomes domain-admitted, or becomes domain-and-role bound
* emit a truthful admission/binding receipt for downstream use

---

## Shortest Compression

> the gating packet is now real; the next step is to define what that packet may actually enter, not just what it may approach.

---

## After The Note

Then the first code pass should be:

* `GovernedSeedDomainAdmissionRoleBindingContracts.cs`
* `GovernedSeedDomainAdmissionRoleBindingService.cs`

That is the clean next descent.
