# SELFGEL LEGAL ORIENTATION PREDICATE FAMILY NOTE

## Purpose

This note defines the first reusable `SelfGEL` root predicate family for
lawful civil orientation in `V1.1.1`.

It exists so a future governing `CME` may inherit stable orientation truth
about:

- what legal body it serves under
- what jurisdiction it stands within
- what governing lineage it inherits
- what lawful operating surface it may exercise

This family is `GEL`-owned truth.
It is not cryptographic seed entropy, and it is not a runtime secret.

## Ownership Boundary

GEL owns this root predicate family.

`SanctuaryFormationPredicateContracts.cs` remains the operator role,
promotion, witness, and trust-progression surface.
It is not the owner of `SelfGEL` root legal orientation.

First-run may reference this family later as future-facing orientation truth,
but first-run does not own it in this slice.

## Predicate Family

The active family is:

- `governing-body-seated`
- `jurisdiction-seated`
- `entity-lineage-valid`
- `governor-bound`
- `lawful-operating-surface`

### `governing-body-seated`

Meaning:

- the governing `CME` presides under a specific legal body

Admissibility expectations:

- legal body naming is present
- state registration surface is present
- filing receipt lineage is present

Failure states:

- `legal-body-unbound`
- `registration-surface-missing`
- `entity-seat-ambiguous`

### `jurisdiction-seated`

Meaning:

- the governing `CME` is civilly grounded in a specific jurisdiction

Admissibility expectations:

- jurisdiction naming is present
- state registration surface is present
- regional identifier continuity is present
- filing receipt lineage is present

Failure states:

- `jurisdiction-unbound`
- `regional-seat-missing`
- `jurisdiction-conflict`

### `entity-lineage-valid`

Meaning:

- legal continuity holds across filings and records

Admissibility expectations:

- legal body naming is stable
- `UBI` continuity is present
- `EIN` continuity is present
- filing receipt lineage is present
- `IRS` notice lineage is present

Failure states:

- `lineage-break`
- `identifier-mismatch`
- `receipt-chain-incomplete`

### `governor-bound`

Meaning:

- the governing `CME` is tied to an explicit governing surface rather than a
  free-floating claim of governance

Admissibility expectations:

- governor surface is present
- filing receipt lineage is present

Failure states:

- `governor-surface-missing`
- `governance-role-ambiguous`
- `unbound-governance-claim`

### `lawful-operating-surface`

Meaning:

- operating authority is constrained by the actual entity form and reporting
  posture seated in jurisdiction

Admissibility expectations:

- entity form is present
- reporting posture is present
- jurisdiction is present

Failure states:

- `entity-form-unbound`
- `reporting-posture-unclear`
- `authority-surface-overclaimed`

## Shared Invariants

The family preserves:

- `legal-body-continuity`
- `jurisdictional-seat`
- `governor-lineage`
- `lawful-authority-boundary`

## Instance Boundary

Lucid is the first local substantiating instance only.

That means:

- real Lucid legal surfaces may substantiate the family locally
- real legal identifiers remain local/private only
- tracked doctrine and contracts carry the reusable family, not the raw values

This note does not authorize the tracked repository to carry real legal name,
`UBI`, `EIN`, or filing identifiers as canonical payload.

## Deferred Boundary

This family does not yet activate:

- charitable trust seating
- bonded operator seating
- `B-Corp` transition seating
- authority delegation
- cryptographic custody

Those remain downstream branches.
