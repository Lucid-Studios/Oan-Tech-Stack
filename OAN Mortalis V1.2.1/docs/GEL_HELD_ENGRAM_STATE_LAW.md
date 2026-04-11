# GEL_HELD_ENGRAM_STATE_LAW

## Purpose

This note defines what it means for an interior semantic object to be lawfully
held at rest inside `Sanctuary.GEL`.

It exists to keep held state distinct from promotion, activation, and storage
admission.

## Governing Compression

> held is not promoted, and persisted is not activated.
>
> `held`, `admitted`, `persisted`, and `promoted` are distinct notions.

## Held-State Office

`held` means the object may remain lawfully inside `GEL`.

It does not mean:

- promoted
- activated
- executable
- seated in `Sanctuary.MoS`

## Rest-State Vocabulary

The exact rest-state vocabulary admitted in this phase is:

- `held`
- `admitted`
- `provisional`
- `refused`

## Minimum Fields

A lawfully held object may not omit:

- `held_id`
- `constructor_class`
- `rest_state`
- `witness_chain[]`
- `continuity_links[]`

## Unresolved-But-Holdable Conditions

The following may still be lawfully held:

- unresolved ambiguity
- unresolved correspondence
- posture-bound contradiction

## Refusal Rules

The following may not be lawfully held:

- missing witness chain
- broken continuity
- missing constructor class
- implicit promotion read
- implicit `Sanctuary.MoS` storage-seat read

## Explicit Non-Grants

This note does not grant:

- promotion
- engram minting
- runtime activation
- service authorization
- `Sanctuary.MoS` standing

## Working Summary

`Sanctuary.GEL` now has an admitted held-state law.

That law fixes what may remain at rest, what may remain unresolved, and what
may not be silently over-read as promotion or storage standing.
