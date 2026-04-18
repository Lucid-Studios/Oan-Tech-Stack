# Next Implementation Chapter: Domain Admission / Role Binding Packet

## Proposed - Structural Chapter Candidate

## Purpose

Advance the governed runtime beyond admission/binding as a runtime event by
materializing one stable carried body for that seam.

This chapter exists so that downstream participation, occupancy, and
post-admission governance work may reason over one complete
domain-admission/role-binding object instead of scattered runtime assessments
and receipts.

The governing invariant is:

> no downstream governance or participation reasoning may occur over anything less than a complete admission/binding packet

---

## Current Achieved State

After `0fc08e9` and `d9a871f`, the live runtime now carries actual
domain-admission and role-binding truth as a first-class event.

The running slice can now say:

* this gating packet entered the admission seam
* this is whether domain admission was granted
* this is whether role remained pending or became bound
* this is the resulting admission/binding disposition

That runtime truth is now carried through:

* control-layer admission/binding evaluation
* runtime service threading
* runtime materialization
* runtime-readable body and state modulation

What is still missing is one stable carried body for the
admission/binding result itself.

---

## Why This Chapter Exists

The unified admission/binding seam now emits:

* `GovernedSeedDomainAdmissionAssessment`
* `GovernedSeedRoleBindingAssessment`
* `GovernedSeedDomainAdmissionRoleBindingAssessment`
* `GovernedSeedDomainAdmissionRoleBindingReceipt`

These are sufficient to witness the seam, but they are still distributed
runtime surfaces.

Before opening downstream participation, occupancy, or post-admission role work,
the runtime should first aggregate those surfaces into one carried body.

---

## Packet Shape

The initial packet should likely carry:

* `GovernedSeedDomainRoleGatingPacket`
* `GovernedSeedDomainAdmissionAssessment`
* `GovernedSeedRoleBindingAssessment`
* `GovernedSeedDomainAdmissionRoleBindingAssessment`
* `GovernedSeedDomainAdmissionRoleBindingReceipt`

This packet becomes the stable admission-bearing body for downstream use.

---

## Intent

The admission/binding packet should make the current seam result:

* transportable
* inspectable
* replayable
* attributable
* suitable for carry-forward continuity

It should not add new admission or role-binding logic.

It should only aggregate already-seated runtime truth into one stable body.

---

## First Descent

The first descent of this chapter should be:

* `GovernedSeedDomainAdmissionRoleBindingPacketContracts.cs`
* `GovernedSeedDomainAdmissionRoleBindingPacketMaterializationService.cs`

Then thread that packet through the live runtime body.

---

## Invariants

### Packet completeness invariant

> no downstream governance or participation reasoning may occur over anything less than a complete admission/binding packet

### Non-reperformance invariant

The packet may carry admission/binding truth, but it must not re-run
admission or role-binding logic.

### Carry-forward invariant

The packet must preserve:

* originating domain/role gating packet identity
* domain-admission truth
* role-binding truth
* unified admission/binding disposition
* admission/binding receipt trace

---

## What This Enables Next

Once the packet exists, the next clean seam becomes:

* downstream post-admission participation
* role-bearing execution or domain occupancy reasoning
* audit/replay/carry-forward of actual admission state

That next seam should reason over the packet, not over distributed runtime
fields.

---

## Witness Plan

### Focused tests

* packet materializes only when full admission/binding outputs exist
* packet preserves admission/binding disposition
* packet preserves originating domain/role gating packet identity
* packet remains available in runtime-facing surfaces

### Integration tests

* live runtime emits the packet after the unified admission/binding seam
* downstream surfaces can reference the packet as one body
* refusal, return, pending, and bound dispositions remain preserved in packet
  form

---

## Outcome Target

At the end of this chapter, the runtime should carry:

* the distributed admission/binding seam surfaces
* and one stable `GovernedSeedDomainAdmissionRoleBindingPacket`

so that downstream governance and participation work may begin from one truthful
carried object.

---

## Shortest Compression

> domain admission and role binding are now runtime truth; the next step is to
> give that truth a body before downstream participation and post-admission work
> begin.
