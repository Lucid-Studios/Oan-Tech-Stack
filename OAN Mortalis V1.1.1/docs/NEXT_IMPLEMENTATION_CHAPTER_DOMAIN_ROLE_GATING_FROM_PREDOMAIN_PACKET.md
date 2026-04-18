# NEXT_IMPLEMENTATION_CHAPTER_DOMAIN_ROLE_GATING_FROM_PREDOMAIN_PACKET

Status: Proposed - Structural Chapter Candidate

## Purpose

Advance the governed host beyond pre-domain governance by teaching it how to
evaluate whether a packet-bearing candidate may lawfully approach:

- domain-bearing participation
- role-bearing participation

This chapter begins only after the pre-domain governance chain has been
aggregated into one stable runtime body:

- `GovernedSeedPreDomainGovernancePacket`

The governing invariant is:

> no domain/role gating may occur over anything less than a complete
> pre-domain governance packet

## Current Achieved State

After `ecb224c`, the runtime now carries:

- the distributed pre-domain receipt chain
- one stable `GovernedSeedPreDomainGovernancePacket` body that aggregates that
  chain

This means the next gate no longer has to infer across scattered receipts. It
can reason over one complete pre-domain governance body.

## Why This Chapter Exists

A candidate that has survived the governed pre-domain host loop is still not
yet domain-bearing or role-bearing.

It is only:

- bounded
- inspected
- split into Prime/Cryptic surfaces
- given a pre-domain admission disposition

The next question is narrower and more serious:

> does this packet contain enough clean, attributable, Prime-governed material
> to lawfully approach domain admission and role binding?

## Input Body

This chapter reasons only over:

- `GovernedSeedPreDomainGovernancePacket`

No looser input is admissible.

The packet is the minimum truthful body because it preserves, together:

- candidate boundary
- holding inspection
- form-or-cleave checkpoint
- duplex separation
- pre-domain admission disposition
- host loop result

## First Gating Intent

The first pass should remain unified.

Initial code seam:

- `GovernedSeedDomainRoleGatingContracts.cs`
- `GovernedSeedDomainRoleGatingService.cs`

Domain and role should be evaluated together first as one discovery seam.

They may later split into separate services only if the runtime body demands
it.

## First Gating Questions

The unified gate should answer, at minimum:

### 1. Domain Eligibility

- does the packet contain sufficient Prime-governed admission structure?
- is the packet free of cryptic authority bleed?
- is the current pre-domain disposition consistent with forward movement?
- is the packet still standing-consistent and revalidation-consistent?

### 2. Role Eligibility

- does the packet expose enough role-relevant structure to bind a lawful role?
- is responsibility attributable at the required scope?
- does the packet remain domain-clean at the point of possible role binding?

## First Disposition Family

The first unified gate should likely return one of:

- `RemainPreDomain`
- `DomainAdmissibleRoleIncomplete`
- `DomainAndRoleAdmissible`
- `CrypticOnlyCarry`
- `Refuse`

These are intentionally narrow and discovery-friendly.

## Invariants

### Required Input Invariant

> no domain/role gating may occur over anything less than a complete
> pre-domain governance packet

### Prime Invariant

Cryptic-bearing material must not gain domain/role-bearing authority by
proximity to the packet.

### Role Invariant

Role binding may only be considered after domain admissibility is at least
minimally satisfied.

### Refusal Invariant

Any packet that is incomplete, standing-inconsistent, or
cryptic-authority-contaminated must not advance.

## Suggested First Contracts

Initial contract family:

- `GovernedSeedDomainRoleGatingAssessment`
- `GovernedSeedDomainEligibilityAssessment`
- `GovernedSeedRoleEligibilityAssessment`
- `GovernedSeedDomainRoleGatingReceipt`

These should expose only:

- gating input truth
- gating result truth
- disposition truth

They should not re-perform earlier governance stages.

## Suggested First Service

- `GovernedSeedDomainRoleGatingService`

Input:

- `GovernedSeedPreDomainGovernancePacket`

Output:

- domain eligibility assessment
- role eligibility assessment
- unified gating assessment
- unified gating receipt

The first pass should remain conservative:

- no hidden promotion
- no shortcutting over incomplete packet state
- no cryptic-originated authority inheritance

## Witness Plan

### Focused Gate Tests

- incomplete packet cannot enter domain/role gating
- packet with cryptic authority bleed is refused
- packet with domain-clean but role-incomplete structure yields
  `DomainAdmissibleRoleIncomplete`
- only fully clean packet yields `DomainAndRoleAdmissible`

### Integration Tests

- runtime can carry packet into the domain/role gate
- gate receipt is materialized into runtime-facing surfaces
- refusal and remain-pre-domain outcomes preserve packet trace

## Outcome Target

At the end of this chapter, the governed host should be able to:

- accept a `GovernedSeedPreDomainGovernancePacket`
- determine whether it remains pre-domain, carries cryptic-only, refuses, or
  may approach domain/role-bearing participation
- emit a truthful domain/role gating receipt for downstream use

## Shortest Compression

> the pre-domain chain now has a body; the next step is to teach the host what
> a packet-bearing candidate may lawfully approach.
