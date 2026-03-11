# PROTECTED_INTAKE_CLASSIFICATION_CONTRACT

## Purpose

This document defines how human and corporate source matter must be treated during first boot and early hosted Engrammitization work.

It exists to lock one constitutional rule:

- raw intake is protected source matter, not Prime-readable runtime truth

## Core Sentence

Human and corporate intake must first become `MaskedCrypticView`, and Prime may reveal only functionally necessary truth through explicit lawful reveal modes.

This protected-intake law operates beneath the broader first-boot ceremony defined in:

- `docs/SANCTUARY_FIRST_BOOT_PROTOCOL.md`

The bounded hybrid rehearsal profile that exercises this contract lives in:

- `docs/HYBRID_PROTECTED_INGRESS_TEST_PROFILE.md`

## Protected Intake Kinds

The protected intake kinds in this phase are:

- `HumanProtectedIntake`
- `CorporateProtectedIntake`

No other intake kind is defined here.

## Default Intake Posture

Human and corporate intake:

- enter as protected source strata
- are masked on the Cryptic side first
- do not become raw Prime-readable runtime objects by default

The first stable representation is:

- `MaskedCrypticView`

Tracked examples must remain abstract:

- `HumanPrincipal_A`
- `CorporatePrincipal_A`
- `ProtectedRegistryRecord_1`

No raw personal or corporate identifiers belong in tracked policy artifacts.

## Prime Reveal Modes

The lawful Prime reveal modes are:

- `None`
- `MaskedSummary`
- `StructuralValidation`
- `AuthorizedFieldReveal`

### None

Default state.

Meaning:

- no Prime-side disclosure
- protected source matter remains Cryptic-masked

### MaskedSummary

May reveal:

- narrow masked status
- existence and high-level protected classification

May not reveal:

- raw fields
- protected identifiers

### StructuralValidation

May reveal:

- lawful structural existence
- relation validity
- status needed for governance or formation checks

May not reveal:

- unrestricted raw field values

### AuthorizedFieldReveal

May reveal:

- specific fields required for a bonded lawful function

Requires:

- `BondedAuthorityContext`
- explicit lawful need
- narrow purpose justification

Without bonded authority context, `AuthorizedFieldReveal` must not progress.

## Bonded Authority Context

Bonded authority exists to constrain narrow reveal.

It is not a general disclosure bypass.

In this phase, bonded authority must at minimum carry:

- authority identity
- authority class
- bonded confirmation state
- approved reveal purposes

## Classification Outcomes

Protected intake classification may yield:

- `Allow`
- `Quarantine`
- later phases may use `Reject` for out-of-scope or invalid inputs

### Allow

The requested reveal posture is lawful and may proceed within the current bounded contract.

### Quarantine

The input is structurally present but may not progress because:

- requested reveal is not lawfully authorized
- normalization or policy conditions are unsafe

Quarantine means:

- retain only for bounded observation or review
- do not progress
- do not widen disclosure

## First-Run Defaults

The first-run defaults are:

- `PrimeRevealMode = None`
- `RawFieldExposureAllowed = false`
- `RequiresBondedAuthority = false` except for `AuthorizedFieldReveal`

This means the normal first-run reveal postures are:

- `MaskedSummary`
- `StructuralValidation`

## Visibility Relation To Governing Offices

Protected-intake reveal is bounded by office jurisdiction.

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

No single office alone may widen protected reveal.

## Future Runtime Relation

The future first-boot runtime host will later use this contract to sequence:

- protected-intake masking
- narrow reveal requests
- governing-office formation eligibility
- bonded confirmation checks

This pass defines the contract only. It does not implement the runtime orchestrator.

## Negative Law

This contract must not be read as permission to:

- expose raw personal identifiers by default
- expose raw corporate identifiers by default
- give a local seeded LLM unrestricted protected source access
- treat Prime as broad disclosure space
- route protected intake into downstream custody during first boot

## Success Condition

This contract is successful when the repo can say:

- protected source matter is Cryptic-masked by default
- Prime reveal is narrow and explicit
- `AuthorizedFieldReveal` requires bonded authority context
- tracked artifacts remain abstract and reusable
