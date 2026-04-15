# Architecture Frame

## Purpose

This document gives the compact architecture read for the active
Sanctuary-root stack.

It is intentionally smaller than the detailed line-local doctrine. It exists
to keep project placement, build rules, and family naming pointed in the same
direction.

## Active Build Line

The active executable line is:

- `OAN Mortalis V1.1.1/Oan.sln`

The current line still carries legacy project identities, especially `Oan.*`,
`CradleTek.*`, `SoulFrame.*`, and `AgentiCore.*`.

Those names are migration surfaces, not final proof of foundational ownership.

## Family Architecture

The target family frame is:

- `San.*`
  - Sanctuary constitutional habitat and foundational stack composition
- `Ctk.*`
  - CradleTek habitation, custody, extension, and runtime distribution
- `Sfr.*`
  - SoulFrame relational, membrane, projection, and interface substrate
- `Acr.*`
  - AgentiCore identity and governance-capable cognition-core machinery
- `SLI.*`
  - transversal symbolic protocol and runtime
- `Oan.*`
  - downstream product/application identity or legacy migration hold only

Current legacy examples map as:

- `Oan.Runtime.Headless` -> target `San.Runtime.Headless`
- `Oan.Common` -> target `San.Common`
- `Oan.FirstRun` -> target `San.FirstRun`
- `CradleTek.Runtime` -> target `Ctk.Runtime`
- `SoulFrame.Membrane` -> target `Sfr.Membrane`
- `AgentiCore` -> target `Acr.Core`

## Composition Rule

There is one foundational stack composition root.

Current migration root:

- `Oan.Runtime.Headless`

Target root:

- `San.Runtime.Headless`

No family-local runtime may present itself as a second foundational stack root.

## Hard Rules

- **No Unity in the active build line:** active projects are .NET console or
  library surfaces unless a later governed surface explicitly admits otherwise.
- **No globals as architecture:** components should be injected, passed, or
  surfaced through explicit contracts.
- **No duplicate enum ownership:** enums and receipt shapes should live in the
  owning contract family.
- **No silent path dependence:** build and test surfaces must remain free of
  private corpus paths and external absolute paths.
- **No new foundational `Oan.*`:** current `Oan.*` surfaces are governed legacy
  holds until a rename slice retires them.
- **No alternate stack roots:** new hosts, workbenches, or service surfaces must
  not imply a second foundational composition root.

## Reference Documents

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`
- `OAN Mortalis V1.1.1/docs/PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md`
- `OAN Mortalis V1.1.1/docs/STACK_ROOT_RENAMING_MIGRATION_PLAN.md`
