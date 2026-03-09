# KNOWN_UNKNOWN_EXTENSION_SEMANTICS

## Purpose

This document defines the first explicit semantics for what the atlas and seed `GEL` know, what they can admit provisionally, and what must be refused.

The goal is to preserve room for lawful extension without collapsing unresolved structure into failure or pretending completion where none exists.

## Canonical Extension States

The active bounded extension states are:

- `Closed`
- `BootstrapClosed`
- `NeedsSpecification`
- `ExtensionCandidate`
- `ProhibitedCollision`

These are contract-backed in:

- `src/GEL.Contracts/Models/SymbolicGovernanceContracts.cs`

## Meaning

### `Closed`

The structure is sufficiently specified and lawful for canonical use.

### `BootstrapClosed`

The structure is lawfully admitted for bootstrap use, but still recognized as an early closure grade rather than a fully mature one.

### `NeedsSpecification`

The structure is admissible enough to preserve, but not sufficiently specified for full canonical closure.

### `ExtensionCandidate`

The structure is unresolved but still lawfully investigable.

It may remain in bounded extension space without being mistaken for canonical atlas closure.

### `ProhibitedCollision`

The structure collides with reserved governance/meta space, reserved disciplinary domains, or other explicit symbolic law and may not be admitted as canonical.

## Governing Rule

Unknown is not the same as failure.

But unknown is also not the same as canonical closure.

The seed `GEL` therefore needs explicit states for:

- stable known structure
- early-but-lawful bootstrap structure
- unresolved but investigable structure
- prohibited structure

## Immediate Consequence

The system can now preserve:

- what is known
- what is provisionally admissible
- what still needs specification
- what may be extended under law
- what must be refused due to collision

That is the minimum semantic discipline required for meaningful later growth.
