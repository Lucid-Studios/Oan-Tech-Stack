# HYBRID_PROTECTED_INGRESS_TEST_PROFILE

## Purpose

This document defines the first bounded hybrid protected-ingress rehearsal profile for the active `OAN Mortalis V1.0` stack.

It exists to prove one narrow point:

- real governance topology may be rehearsed against synthetic credential material while protected ingress remains Cryptic-masked, Prime reveal remains narrow, and the admission/observation chain stays lawful

This profile is a rehearsal surface only. It is not production protected-ingress wiring.

## Core Sentence

Hybrid protected-ingress rehearsal uses real identity topology, synthetic credential material, masked protected handles, narrow Prime reveal envelopes, membrane enforcement, and AgentiCore observation without introducing live secret storage or downstream custody mutation.

## Runtime Shape

The bounded hybrid run exercises:

```text
identity topology
-> protected-intake classification
-> Cryptic masking
-> narrow Prime reveal
-> membrane decision
-> Prime closure, where lawful
-> AgentiCore observation
```

The tracked profile remains abstract and reusable.

The local ignored profile may use real names for manual topology rehearsal only.

## Tracked vs Local Profile Rule

The hybrid profile is split into two surfaces:

- tracked abstract profile
- local ignored real-name profile

### Tracked Abstract Profile

Tracked docs and tracked tests must use only abstract placeholders such as:

- `HumanPrincipal_A`
- `CorporatePrincipal_A`
- `ProtectedAuthorityRecord_1`

Tracked artifacts must never contain:

- real names
- real credential identifiers
- real addresses
- real contact fields

### Local Ignored Real-Name Profile

The local ignored profile may contain:

- real human principal name
- real corporate principal name
- real governance relationship topology

It may not contain:

- real credential IDs
- real registry numbers
- real addresses
- real contact/private identifiers

Even in the local profile, credentials remain synthetic.

### CI and Test Rule

CI and tracked tests must use only the tracked abstract profile example.

The local ignored profile is manual-rehearsal only and must never be read by CI or tracked tests.

Real names from the local profile must never appear in:

- tracked artifacts
- committed logs
- CI-visible logs

## Protected Ingress Classes

The bounded hybrid run uses two protected ingress classes:

- `HumanProtectedIntake`
- `CorporateProtectedIntake`

The role relationship shape is:

- one human principal
- one corporate principal
- one authority relationship

## Credential Material Rule

Credential material remains synthetic everywhere.

Example synthetic values:

- `HUM-TEST-0001`
- `CORP-TEST-8821`
- `AUTH-TEST-ALPHA`
- `ADDR-MASK-01`

These values exist only to pressure-test governance, masking, reveal, and membrane behavior.

They must not correspond to reusable real-world identifiers.

## Prime Reveal Modes

The bounded hybrid profile may request only these reveal modes during normal rehearsal:

- `None`
- `MaskedSummary`
- `StructuralValidation`

`AuthorizedFieldReveal` is included only as a blocked escalation scenario unless bonded authority conditions are explicitly met in later phases.

### Allowed Meaning

#### None

- no Prime-side disclosure

#### MaskedSummary

May reveal:

- narrow masked status
- existence and high-level protected classification

May not reveal:

- raw fields
- protected identifiers

#### StructuralValidation

May reveal:

- lawful structural existence
- relation validity
- status needed for governance or formation checks

May not reveal:

- unrestricted raw field values

### Disallowed Material

The hybrid profile must never reveal:

- real credential IDs
- real registry numbers
- real addresses
- real contact/private identifiers

## Bounded Harness

The bounded runtime harness must:

- classify ingress using the first-boot governance contracts
- produce `MaskedCrypticView` for both human and corporate ingress
- request reveal modes explicitly
- record requested, granted, and blocked reveal modes
- run the existing cryptic formation + admission membrane path only where lawful
- record the full run through `IAgentiFormationObserver`

The harness must not:

- persist secrets
- write to GEL/cGEL
- write to MoS/cMoS
- change routing
- mutate SoulFrame identity state
- involve the local seeded LLM

## First Scenario Matrix

### Case A - Corporate governed valid topology

- boot class: `CorporateGoverned`
- one human principal
- one corporate principal
- one authority relationship
- synthetic credentials only
- requested reveal modes:
  - `MaskedSummary`
  - `StructuralValidation`

Expected:

- ingress masked first
- no raw-field exposure
- policy classification allowed
- membrane can admit structurally valid candidate material
- observation batch records the full chain

### Case B - Personal solitary no expansion

- boot class: `PersonalSolitary`
- same hybrid ingress shape
- requested expansion remains single-operator

Expected:

- classification succeeds
- `ExpansionRights = None`
- no swarm eligibility
- observation records the restrictive posture

### Case C - Personal swarm attempt

- boot class: `PersonalSolitary`
- requested expansion count `> 1`

Expected:

- policy decision `Quarantine`
- no progression to expansion eligibility
- observation records quarantine posture

### Case D - Reveal escalation blocked

- request `AuthorizedFieldReveal` without bonded authority

Expected:

- protected intake classification quarantines
- no raw-field exposure
- `BlockedRevealModes` records the denied reveal
- observation records the blocked posture

## Success Questions

The hybrid profile is successful when the run can answer:

- Did masking happen before formation?
- Did the membrane enforce reveal and boot-class rules?
- Did any candidate lawfully reach closure?
- What did AgentiCore observe?

## Related Runtime and Governance Surfaces

- `docs/PROTECTED_INTAKE_CLASSIFICATION_CONTRACT.md`
- `docs/FIRST_BOOT_CLASSIFICATION_AND_EXPANSION_POLICY.md`
- `docs/SANCTUARY_FIRST_BOOT_PROTOCOL.md`
- `docs/FIRST_BOOT_INTERNAL_GOVERNING_CME_FORMATION.md`
- `docs/BUILD_READINESS.md`

## Negative Law

This profile must not be misread as:

- production protected-ingress wiring
- permission to store live secrets in the repo
- permission to emit real names into tracked logs
- permission to widen reveal beyond bounded rehearsal
- permission to mutate downstream custody or routing
