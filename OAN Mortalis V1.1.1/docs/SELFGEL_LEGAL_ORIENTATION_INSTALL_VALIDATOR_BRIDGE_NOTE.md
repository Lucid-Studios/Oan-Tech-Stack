# SELFGEL LEGAL ORIENTATION INSTALL VALIDATOR BRIDGE NOTE

## Purpose

This note defines the install-validator bridge for the `SelfGEL` legal
orientation family in `V1.1.1`.

It exists to keep four things distinct:

- the reusable `GEL`-owned root predicate family
- the local-only legal evidence pool that may substantiate it
- the future install-validator packet shape
- the first-run bridge, which remains doctrine-only in this slice

## Legal Evidence Pool

The future local evidence pool is named `LegalEvidencePool`.

Its abstract field set is:

- `legal_name`
- `state_registration_surface`
- `jurisdiction`
- `ubi`
- `ein`
- `filing_receipt_handles`
- `irs_notice_handle`
- `governor_surface`
- `entity_form`
- `reporting_posture`
- `evidence_mixture_class`

The evidence mixture class for this bridge is:

- `mixed-legal-surfaces-local-only`

The real packet is organization-controlled, local-only protected data.
It is not tracked canonical repository payload.

## Root Predicate Family

The future install-validator packet may substantiate this root family:

- `governing-body-seated`
- `jurisdiction-seated`
- `entity-lineage-valid`
- `governor-bound`
- `lawful-operating-surface`

`GEL` owns this root family.
First-run may later reference it, but does not own it in this slice.

## Deferred Branches

The install-validator bridge names the following deferred branches only:

- `charitable-trust-seat`
- `bonded-operator-seat`
- `b-corp-transition-seat`
- `authority-delegation`
- `cryptographic-custody`

These branches are named for future seating only.
They are not activated here.

## Local Input Boundary

The tracked placeholder template is:

- `docs/templates/legal_orientation_install.packet.template.json`

The ignored real local packet is:

- `OAN Mortalis V1.1.1/.local/legal_orientation_install.packet.json`

The tracked template carries only abstract fields and logical source labels.

Real legal identifiers remain local/private only.

## First-Run Bridge

Primitive `SelfGEL` may later inherit this `GEL`-owned legal-orientation root
family.

In this slice:

- the bridge into first-run is doctrine-only
- no first-run state-ladder change is authorized
- no live first-run receipt or runtime contract change is authorized

## Non-Secret Boundary

The legal evidence pool may substantiate legal seed formation, but it is not
cryptographic seed entropy.
In plain terms, it is not cryptographic seed entropy.

Legal evidence anchors lawful provenance and civil orientation.
Cryptographic secrecy must still come from separate private entropy and custody
material.
