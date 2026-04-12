# RESIDENT_SEATING_SERIALIZATION_PILOT_NOTE

## Purpose

This note defines the first controlled serialization pass for resident seating
work.

It exists to test whether the current resident seating doctrine can survive
contact with automation without being flattened into answer curation.

This pilot is intentionally narrow.

It serializes only the current primary bridge entry and keeps the doctrinal
notes as the constitutional source of meaning.

## Governing Compression

> serialize only what has already been stabilized by doctrine-backed recovery

> machine-readable shape must remain downstream of the casebook, bridge, and
> curriculum surfaces

## Current Serialized Scope

The current pilot serializes only:

- `presence-without-inflation`

That choice is deliberate.

It is the current bridge that best survives bounded recovery across both local
resident seats.

The pilot does not yet serialize:

- resident-specific bridge variants as general law
- negative seams as primary training entries
- the full collapse casebook
- the entire training ledger

## Pilot Storage Surface

The current machine-usable pilot lives in the integration-test surface:

- `tests/Sanctuary/Oan.Runtime.IntegrationTests/Oan.Runtime.IntegrationTests/TestData/HostedLlmResidentSeatingPilotDataset.json`

It is placed there so the harness can consume it locally without turning the
repository docs into a runtime state store.

## Required Fields

Each pilot entry should currently preserve:

- frame lines
- constitutional intent
- expected lawful band
- forbidden drift
- expected initial collapse family
- expected bridge classification
- expected recovery classification
- recovery probe
- cross-resident notes
- resident-specific cautions

These fields are enough to test whether automation can carry the meaning of the
bridge without replacing the doctrinal notes that explain it.

## Non-Goals

This pilot does not:

- define a fine-tuning pipeline
- replace the doctrinal docs with JSON
- universalize one resident from isolated success
- claim governance promotion from serialized consumption

## Harness Role

The harness may:

- load the pilot dataset
- verify that the serialized primary bridge remains structurally intact
- run the resident against the serialized frame
- compare observed collapse and recovery behavior to the serialized
  expectations

The harness may not:

- widen the pilot into a full corpus by convenience
- infer resident office from one serialized pass
- treat serialized entries as self-justifying truth

## Working Summary

This pilot lets the branch ask one bounded question:

> can the current resident seating doctrine survive contact with automation?

The answer remains local, witness-driven, and intentionally small until the
serialized seam proves it can carry meaning without flattening it.
