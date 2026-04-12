# AGREEMENT_PREDICATE_BUNDLE_LAW

## Purpose

This note defines the first localized agreement-predicate bundle emitted by
the install-agreement action surface in `V1.2.1`.

It exists so agreement structure is carried as predicate-bearing lanes rather
than a single blob-like contract artifact.

## Governing Compression

The bundle is the semantic product of the lawful install-agreement action
surface.

It is not inferred from raw text.
It is not inferred from default convenience.

The preserved distinction is:

- acknowledgement is not assent

## Required Predicate Lanes

The bundle must carry exactly these agreement lanes:

- `service-license-predicate`
- `terms-of-service-predicate`
- `bonded-operator-predicate`
- `cme-lab-notice-predicate`
- `research-data-practice-predicate`
- `access-attachment-profile-predicate`

## Assent Grammar

Each lane may stand in exactly one assent state:

- `Presented`
- `Acknowledged`
- `Assented`
- `Refused`
- `Withheld`

`Acknowledged` and `Assented` must remain distinguishable at bundle level.

## Formation Rule

Localized agreement predicates must be formed from the authorized choice
matrix and lawful agreement surface, not inferred from raw text or default
convenience.

Template presence without adoption or assent remains non-binding.

## Bundle Minimums

The first bundle must preserve:

- predicate lane
- assent state
- template ref
- witness refs
- language dataset id
- locale and jurisdiction
- agreement template lineage

## Explicit Non-Grants

This note does not authorize:

- identity minting by bundle presence alone
- contract-law widening by template presence alone
- service activation
- runtime consequence
- Atlas mutation

## Working Summary

`AgreementPredicateBundle` is now the first localized semantic product of the
install-agreement action surface.

It preserves predicate lanes, assent state, and lineage without granting
identity, service, or mutation standing by itself.
