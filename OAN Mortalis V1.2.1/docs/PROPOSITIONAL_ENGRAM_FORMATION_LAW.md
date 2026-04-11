# PROPOSITIONAL_ENGRAM_FORMATION_LAW

## Purpose

This note defines the first lawful formation of propositional engrams inside
`Sanctuary.GEL`.

It exists to fix the boundary between a predicate candidate and a witnessed
semantic holding.

## Governing Compression

> propositions tell `GEL` what the world seems to be.
>
> a propositional engram is a witnessed semantic holding, not a mere
> predicate candidate.

## Formation Preconditions

A propositional engram may form only when:

- membrane-retained structure already exists
- predicate references are explicit
- universe binding is explicit
- witness is present
- continuity is not broken

A propositional engram may never form directly from raw ingress.

## Constructor Eligibility

Propositional engrams may stand only at:

- `Basic`
- `Intermediate`
- `Advanced`
- `Master`

No full propositional engram may stand as a complete `Root`.

## Canonical Fields

Every propositional engram should carry at minimum:

- `proposition_id`
- `engram_id`
- `constructor_class`
- `categorical_class = propositional`
- `predicate_refs[]`
- `truth_state`
- `universe_binding`
- `source_landing_ref`
- `continuity_refs[]`
- `contradiction_refs[]`
- `witness_refs[]`

In this phase, `truth_state` is limited to:

- `admitted`
- `provisional`

## Refusal Rules

The following must force refusal, hold, or non-formation:

- direct formation from raw ingress
- missing witness
- missing universe binding
- unresolved predicate ambiguity advanced as closure
- a `Root` object presented as a full proposition

## Explicit Non-Grants

This note does not grant:

- procedural standing
- execution authority
- runtime law
- service authorization
- predicate promotion
- engram minting

## Working Summary

`Sanctuary.GEL` now fixes the first lawful boundary between candidate meaning
and witnessed propositional standing.

That standing remains bounded, witnessed, non-procedural, and non-runtime.
