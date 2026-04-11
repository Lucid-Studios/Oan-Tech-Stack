# VERBATIM_INGRESS_AND_SYMBOL_PRESERVATION_LAW

## Purpose

This note defines the first lawful intake office for `Sanctuary.GEL`.

It exists to ensure that raw text and symbols arrive as witnessed form before
any root mapping, predicate formation, or symbolic transformation occurs.

## Governing Compression

> no symbol should be reduced to plain text until the system has recorded what
> kind of thing it was.
>
> verbatim ingress is evidence before it is interpretation.

The `UTF8_RESERVED_SET_AND_CANONICAL_SYMBOL_LAW.md` note now governs the
reserved-set office above this note.

This note therefore preserves exact ingress and symbol-bearing evidence, but it
does not itself freeze a full explicit reserved-set inventory.

## Verbatim Ingress Office

The first intake office is verbatim preservation.

Its work is to preserve:

- exact incoming order
- exact UTF-8 content
- source witness
- ingress timing
- ingress scope

It is not allowed to:

- normalize silently
- classify implicitly
- correct misspelling invisibly
- collapse symbols into plain text

## Required Verbatim Fields

Every verbatim ingress record should carry at minimum:

- `ingress_id`
- `raw_utf8`
- `raw_token_stream`
- `source_witness`
- `ingress_timestamp`
- `origin_universe`

## Required Symbol Fields

Every symbol-bearing unit should carry at minimum:

- `normalized_utf8`
- `unicode_codepoints[]`
- `render_form`
- `symbol_class`
- `semantic_role`

## Symbol Classes

The minimum symbol classes in this phase are:

- `lexical`
- `operator`
- `delimiter`
- `structural`
- `ornamental`
- `domain-specific`
- `unknown`

`unknown` must not be auto-reduced.
It must remain explicitly witnessed until later classification or refusal.

## Malformation Discipline

This phase must preserve malformed, misspelled, or corrupted ingress as
evidence before any candidate normalization path is proposed.

That means:

- raw malformed form remains visible
- candidate normalized form must be explicit
- confidence may be proposed but not implied
- silent correction is forbidden

## Refusal Law

The following must force refusal, hold, or non-promotion:

- malformed UTF-8 stripped before record
- normalization without witness
- symbol class omitted
- raw ingress lost after normalization
- ambiguity collapsed by convenience

## Explicit Non-Grants

This note does not grant:

- root authority
- predicate authority
- automatic correction
- active semantic closure
- runtime trust

## Working Summary

`Sanctuary.GEL` now has a first intake office that preserves verbatim form and
symbol identity before interpretation.

That keeps symbol loss, hidden correction, and pre-classification reduction out
of the lawful intake path.
