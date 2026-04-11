# ENGRAM_CORRESPONDENCE_AND_CONTRADICTION_LAW

## Purpose

This note defines how `Sanctuary.GEL` distinguishes lawful plurality,
unresolved correspondence, contradiction, hostile pressure, and malformed
source.

It exists to stop contradiction handling from collapsing into one generic
error bucket.

## Governing Compression

> plurality is not contradiction by default.
>
> no shared-root relation may be treated as identity or contradiction without
> explicit witness.

## Correspondence Office

Correspondence law exists to judge relations between engrams across universe
contexts without forcing collapse.

It may witness:

- lawful plurality
- unresolved relation
- bounded contradiction
- hostile pressure
- malformed source traces

## Contradiction Classes

The contradiction classes admitted in this phase are:

- `plural-not-contradictory`
- `correspondence-unresolved`
- `locally-contradictory`
- `hostile-pressure`
- `malformed-source`

## Canonical Fields

Every correspondence or contradiction record should carry at minimum:

- `correspondence_id`
- `left_engram_ref`
- `right_engram_ref`
- `relation_type`
- `universe_context`
- `contradiction_class`
- `resolution_state`
- `witness_refs[]`

`relation_type` should use the universe-law edge types.

## Refusal Rules

The following must force refusal, hold, or non-collapse:

- plurality treated as contradiction without witness
- contradiction treated as mere parser error
- hostile pressure normalized into ordinary correspondence
- malformed source advanced as lawful proposition without explicit quarantine
- any collapse into a single generic error bucket

## Explicit Non-Grants

This note does not grant:

- forced identity
- forced contradiction
- silent normalization of hostile or malformed ingress
- runtime consequence

## Working Summary

`Sanctuary.GEL` now fixes a first lawful distinction between plurality,
unresolved relation, contradiction, hostile pressure, and malformed source.

That distinction keeps correspondence-bearing meaning from collapsing into
flat error handling.
