# SLI_HITL_HOLD_WITNESS_TOKEN_CLASS_LAW

## Purpose

This note defines the first witness-token classes required for lawful exit
from `HitlHold` in `V1.2.1`.

It exists so `HitlHold` is not treated as decorative pause, vague human
oversight, or a state that may silently release once operational pressure
rises.

Its job in this line is to fix:

- the first token classes
- acknowledgement versus assent
- the first exit sufficiency rules
- bounded stewardship release limits

## Governing Compression

`HitlHold` may not lawfully release itself.

The first line-local read is:

> exit from `HitlHold` is a witnessed act that must carry tokenized proof of
> who saw, who judged, what exit was authorized, and under which standing.

## Install-First Boundary

`V1.2.1` is not yet claiming live witnessed release automation.

What is admitted now is:

- line-local witness-token doctrine
- a first passive token-class contract family

What remains withheld is:

- automatic hold release
- mission-time token issuance
- full human-assent routing

## Core Distinction

The line-local distinction to preserve is:

- acknowledgement is not assent

### Acknowledgement

Acknowledgement proves that a witness saw the hold and recognized its burden.

It does not authorize resumed motion by itself.

### Assent

Assent proves that a witness judged one named exit from `HitlHold` to be
lawfully admissible.

The later install-assent corridor may preserve this same acknowledgement versus
assent distinction for localized agreement predicates.

It is not a `HitlHold` exit-token family, and no install agreement assent may
counterfeit a hold-release token.

## First Token Class Family

The first token family now reads:

- `AckToken`
- `AssentToken`
- `StewardAttestationToken`
- `EscalationTransferToken`
- `MotherFatherReviewToken`
- `QuarantineContinuationToken`
- `RefusalClosureToken`
- `RepresentToken`

These tokens are carried line-locally in:

- `src/SLI/SLI.Runtime/HitlHoldWitnessTokenContracts.cs`

## First Exit Family

The first bounded hold exits now read:

- `StewardReview`
- `MotherFatherReview`
- `GovernedReturn`
- `Refusal`
- `Quarantine`

## Sufficiency Read

The line-local sufficiency read is:

- `HitlHold -> StewardReview` requires bounded stewardship witness
- `HitlHold -> MotherFatherReview` requires witnessed escalation transfer
- `HitlHold -> GovernedReturn` requires explicit release assent and reviewed standing
- `HitlHold -> Refusal` requires refusal-bearing closure witness
- `HitlHold -> Quarantine` requires containment continuation witness

## Steward Boundary

`StewardAttestationToken` stays bounded.

It may support release only while:

- the matter remains inside stewardship competence
- no constitutional standing change is being claimed
- no human-assent-only burden applies

## Hard Prohibitions

1. No `AckToken` by itself may authorize release.
2. No stewardship token may counterfeit constitutional release.
3. No `HitlHold` token is reusable by default across unrelated holds.
4. No non-releasable hold class may be softened by convenience.

## Implementation Consequences

This note now governs the first passive `HitlHold` witness-token family in
`V1.2.1`.

The carried batch gives the line:

- typed token classes
- typed witness roles
- typed hold exits
- typed hold classes

It does not yet claim:

- live token issuance
- runtime sufficiency adjudication
- automatic witness release lanes
