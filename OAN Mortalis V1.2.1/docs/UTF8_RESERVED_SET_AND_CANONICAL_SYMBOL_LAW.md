# UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW

## Purpose

This note defines the UTF-8 reserved-set office and canonical carrier
preservation law for Atlas-derived symbolic transport.

It exists to ensure that carrier identity is preserved exactly before any
symbolic interpretation, transport reduction, or contextual meaning claim is
attempted.

## Governing Compression

> UTF-8 preservation precedes symbolic interpretation.
>
> Transport readiness preserves root ancestry and formation law; it does not
> guarantee local semantic realization.

## Reserved-Set Office

This office governs the preservation burden for symbolic carriers.

Its work is to ensure that every incoming carrier keeps:

- exact raw UTF-8
- normalized UTF-8 with witness
- exact codepoint sequence
- display form
- symbol class
- carrier identity trace

It is not allowed to:

- silently normalize for convenience
- erase carrier identity during interpretation
- freeze a full explicit reserved-set inventory in this phase
- treat preservation success as local semantic realization

## Preservation Rules

Every incoming carrier in this phase should preserve at minimum:

- `raw_utf8`
- `normalized_utf8`
- `unicode_codepoints[]`
- `display_form`
- `symbol_class`
- `carrier_identity_ref`
- `witness_refs[]`

Preservation must occur before:

- transport reduction
- root-symbol assignment
- formation extension
- contextual split discussion

Symbolic interpretation may classify a carrier.
It may not erase or replace carrier identity.

## Governance-Only Scope

This slice governs the reserved-set office only.

It does not yet freeze or enumerate a full explicit reserved-set inventory.

That wider inventory remains a later law surface once transport governance is
stable enough to name it without bluff.

## Refusal Rules

The following must force refusal, hold, or non-promotion:

- UTF-8 loss before record
- convenience normalization without witness
- codepoint sequence omitted
- display form lost after reduction
- carrier identity erased by interpretation
- malformed ingress treated as if preservation succeeded

## Explicit Non-Grants

This note does not grant:

- a full frozen reserved-set inventory
- Atlas delta candidacy
- Atlas mutation
- predicate promotion
- engram minting
- operator realization
- runtime activation

## Working Summary

`SLI` now has a UTF-8 and canonical-carrier preservation note for symbolic
transport.

That note keeps carrier identity exact before interpretation while explicitly
refusing to confuse preservation governance with local semantic realization or
Atlas update authority.
