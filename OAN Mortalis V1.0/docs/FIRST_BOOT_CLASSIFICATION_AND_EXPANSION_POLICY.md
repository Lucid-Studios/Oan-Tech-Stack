# FIRST_BOOT_CLASSIFICATION_AND_EXPANSION_POLICY

## Purpose

This document defines the lawful first-boot classes, activation states, and expansion limits for the active `OAN Mortalis V1.0` stack.

It exists to answer one narrow question before further first-run orchestration work:

- what kinds of first boot are lawful, what state are they in, and when may subordinate governed expansion become possible

This document is policy-only. It does not itself wire runtime behavior.

Companion documents:

- `docs/SANCTUARY_FIRST_BOOT_PROTOCOL.md`
- `docs/PROTECTED_INTAKE_CLASSIFICATION_CONTRACT.md`
- `docs/FIRST_BOOT_INTERNAL_GOVERNING_CME_FORMATION.md`
- `docs/SANCTUARY_FOUNDING_RUNTIME_BRIEF.md`
- `docs/MOS_ACTUAL_FIRST_VESSEL_AND_REPAIR_LAW.md`

## Core Sentence

First boot is lawful only as `PersonalSolitary` or `CorporateGoverned`, and classification alone never implies expansion eligibility.

The broader operational ceremony for lawful first boot is defined separately in:

- `docs/SANCTUARY_FIRST_BOOT_PROTOCOL.md`

## Lawful Boot Classes

The lawful first-boot classes are:

- `PersonalSolitary`
- `CorporateGoverned`

No third boot class is defined in this phase.

### PersonalSolitary

`PersonalSolitary` may:

- classify as a lawful first-boot mode
- form internal governance
- proceed through founding and readiness law

`PersonalSolitary` may not:

- widen into subordinate agentic expansion
- gain swarm rights
- gain multi-agent personification through first-boot classification alone

### CorporateGoverned

`CorporateGoverned` may:

- classify as a lawful first-boot mode
- form internal governance
- later gain bounded internal expansion rights

`CorporateGoverned` may only gain:

- `ExpansionRights = InternalGovernedOnly`
- `SwarmEligibility = AllowedAfterBondedConfirmation`

after all of the following are true:

- `Steward` formed
- `Father` formed
- `Mother` formed
- triadic cross-witness completed
- bonded confirmation completed

## Boot Activation State

Boot classification and boot activation are not the same thing.

The lawful activation model is:

- `Classified`
- `GovernanceForming`
- `TriadicActive`
- `BondedConfirmed`
- `ExpansionEligible`

### State Meaning

#### Classified

The runtime knows which boot class applies, but no expansion or governance readiness should be inferred from that fact alone.

#### GovernanceForming

Internal governing CME formation may begin under first-boot law.

#### TriadicActive

`Steward`, `Father`, and `Mother` have each lawfully formed and triadic cross-witness has become live.

#### BondedConfirmed

The necessary bonded authority context exists for later narrow reveal and governed expansion checks.

#### ExpansionEligible

Only `CorporateGoverned` may ever lawfully enter this state, and only after:

- `TriadicActive`
- bonded confirmation

`PersonalSolitary` must never become `ExpansionEligible`.

## Expansion Policy

`ExpansionRights` are:

- `None`
- `InternalGovernedOnly`

`SwarmEligibility` is:

- `Denied`
- `AllowedAfterBondedConfirmation`

### Hard Rules

- `PersonalSolitary` always starts with `ExpansionRights = None`
- `PersonalSolitary` always starts with `SwarmEligibility = Denied`
- `CorporateGoverned` starts with `ExpansionRights = None`
- `CorporateGoverned` starts with `SwarmEligibility = Denied`
- `CorporateGoverned` may later gain `InternalGovernedOnly`
- no boot class gains public or unconstrained expansion rights in this phase

### Personal Swarm Mitigation

If:

- `BootClass == PersonalSolitary`
- requested expansion count `> 1`

then the correct policy result is:

- `Quarantine`

Meaning:

- retain for bounded observation or review only
- do not progress
- do not assign expansion rights

## Governing CME Order

First boot must preserve this order:

1. `Steward`
2. `Father`
3. `Mother`
4. triadic cross-witness
5. internal governance active
6. bonded confirmation
7. only then possible subordinate governed expansion

No office may lawfully skip that order.

## Visibility Model

Informational jurisdiction is bounded by office.

### Steward

May see:

- custody class
- provenance class
- reveal-eligibility state

### Father

May see:

- structural identity relations
- governance obligations
- Cryptic-side protected formation posture

### Mother

May see:

- continuity and care obligations
- Prime-side reveal posture
- outward admissibility posture

No single office alone may widen protected reveal or expansion rights.

## Future Host Placement

The future runtime host seam for first-boot sequencing is reserved as:

- `FirstBootGovernanceOrchestrator`

It will later sequence:

- boot classification
- protected-intake masking
- governing-CME formation requests
- triadic cross-witness
- bonded confirmation
- expansion-rights assignment

This pass does not wire that orchestrator into the live runtime.

## Negative Law

This phase must not:

- create a third first-boot class
- grant `PersonalSolitary` any swarm-capable right
- treat `CorporateGoverned` classification as automatic expansion eligibility
- widen Prime reveal by default
- move protected source matter directly into downstream custody
- introduce local seeded LLM participation

## Success Condition

This policy layer is successful when the repo can say:

- first boot has only two lawful classes
- activation state is separate from classification
- no lawful expansion occurs before triadic activation and bonded confirmation
- personal swarm attempts are quarantined, not progressed
- future runtime work has an explicit constitutional target for first-run orchestration
