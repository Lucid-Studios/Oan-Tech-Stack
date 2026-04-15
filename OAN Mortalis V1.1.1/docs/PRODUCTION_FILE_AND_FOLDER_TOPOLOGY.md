# PRODUCTION FILE AND FOLDER TOPOLOGY

## Purpose

This note fixes the production-facing file and folder flow for
`OAN Mortalis V1.1.1`.

It exists to keep four things distinct:

- workspace envelope
- active line root
- source-family ownership
- transitional folder staging that should not be copied blindly into future
  templates

The original stack sketch is still useful as a body-plan memory. It is not the
final source-tree authority for the production build.

## Governing Read

Use this note with:

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`
- `docs/BUILD_READINESS.md`
- `docs/FIRST_RUN_CONSTITUTION.md`

The family constitution names ownership.
This note names where that ownership should live in the file tree.

## Core Rule

`Sanctuary` is the constitutional host family and the first lawful runtime
habitat.

The current folder name `src/Sanctuary/` is still transitional because it does
not yet tell the same truth as the forward family prefix `San.*`.

That means the source tree should not permanently imply:

- `src/Sanctuary/` is the final production root for `San.*`
- `Oan.*` is the lasting foundational owner of Sanctuary-root services
- every project under the governed chamber belongs to one final source family
  merely because it currently stages under the same folder

In compact form:

> constitutional host truth is not the same thing as a legacy staging root.

## Current V1.1.1 Layout

The active line currently uses:

```text
OAN Mortalis V1.1.1/
  .audit/
  build/
  docs/
  src/
    Sanctuary/
      Oan.*
      SLI.*
    TechStack/
      AgentiCore/
      CradleTek/
      GEL/
      SoulFrame/
  tests/
    Sanctuary/
  tools/
```

This layout is lawful for the active line, but it contains one important
transitional compromise:

- `src/Sanctuary/` currently contains legacy `Oan.*` project roots that must be
  re-rooted toward `San.*`

The projects beneath it are not proof that `Oan.*` is the enduring
foundational owner.

## Production Interpretation

The production read should be:

### Workspace envelope

The wider local workspace may be read as `Sanctuary` in the architectural
sense.

That means the workspace can contain:

- documentation memory surfaces
- runtime state surfaces
- automation surfaces
- the active executable line

But that architectural reading should not leak into source ownership.

### Active line root

`OAN Mortalis V1.1.1/` remains the active executable line root.

It owns:

- line-local build policy
- line-local docs
- line-local source
- line-local tests
- line-local audit state

### Source-family ownership

Source ownership is determined by family and project role:

- `San.*` for Sanctuary-root constitutional host and stack-level composition
- `SLI.*` for symbolic grammar and symbolic runtime
- `Ctk.*` for CradleTek habitation, custody, and extension ownership
- `Sfr.*` for SoulFrame operator and relational membrane ownership
- `Acr.*` for AgentiCore runtime-core ownership
- `GEL.*` remains a bounded supporting domain surface until stronger family
  placement is fixed
- `Oan.*` remains a legacy migration hold, not a forward foundational family

### Transitional staging roots

The current roots:

- `src/Sanctuary/`
- `src/TechStack/`

should be read as repository organization aids for the active line, not as the
final metaphysical claim about ownership.

## Production Target Topology

The target production topology for future templating is:

```text
<LineRoot>/
  .audit/
  build/
  docs/
  src/
    San/
      San.Common/
      San.FirstRun/
      San.HostedLlm/
      San.Nexus.Control/
      San.PrimeCryptic.Services/
      San.Runtime.Headless/
      San.Runtime.Materialization/
      San.State.Modulation/
      San.Trace.Persistence/
    SLI/
      SLI.Engine/
      SLI.Ingestion/
      SLI.Lisp/
    Acr/
      Acr.Core/
    Ctk/
      Ctk.Custody/
      Ctk.Host/
      Ctk.Mantle/
      Ctk.Memory/
      Ctk.Runtime/
    GEL/
      GEL.Contracts/
    Sfr/
      Sfr.Bootstrap/
      Sfr.Membrane/
  tests/
    Sanctuary/
  tools/
```

This target retires the older `Oan.*` root and the transitional
`src/Sanctuary/` staging root together.

## Template Rule For Future Designs

Future line templates should inherit these rules:

### 1. Do not use a runtime chamber name as the primary source-family root

Do not default to:

- `src/Sanctuary/...` as a permanent legacy staging root
- `src/House/...`
- `src/Chamber/...`

when the projects are really owned by family prefixes like `San.*`, `SLI.*`,
`Ctk.*`, `Sfr.*`, or `Acr.*`.

### 2. Use family-first roots for family-owned code

If the family prefix is stable, the source root should reflect it.

Examples:

- `src/San/`
- `src/SLI/`
- `src/Ctk/`
- `src/Sfr/`
- `src/Acr/`

### 3. Keep grouped substrate families grouped only when it improves flow

Grouped substrate roots may remain when they improve readability, but they
should not erase family distinction or continue to imply older foundational
ownership.

### 4. Keep tests line-local and chamber-readable

`tests/Sanctuary/` is acceptable because it describes the active runtime line
under test rather than claiming source-family ownership.

### 5. Keep docs and build policy line-local

The future template should continue to use:

- `docs/` for line doctrine and implementation-facing law
- `build/` for machine-readable line policy
- `.audit/` for receipts, last-run state, and bounded operational witness

## Migration Rule

This note does not require an immediate folder move.

For the current line, the safe order is:

1. fix the production topology contract
2. treat `src/Sanctuary/` and legacy `Oan.*` roots as transitional in doctrine
   and review
3. avoid creating new long-lived projects under `src/Sanctuary/` unless the
   project is explicitly a migration-facing bridge that will later be re-rooted
4. move physical folders only when solution/project references can be updated
   as one governed slice

## What Should Stay Stable Now

The following should remain stable for the active line right now:

- repo root workflow
- line-local `build/`, `docs/`, `.audit/`, `tests/`, and `tools/`
- `src/TechStack/CradleTek/`
- `src/TechStack/SoulFrame/`
- `src/TechStack/AgentiCore/`
- `src/TechStack/GEL/`

The roots that should now be treated as explicitly transitional are:

- `src/Sanctuary/`
- the legacy `Oan.*` project roots that still live beneath it

## Immediate Build Consequence

When new projects or folders are added:

- use family ownership first
- use chamber language only for runtime or test surfaces
- do not treat `Oan.*` as the physical owner of Sanctuary-root host code
  forever

If a future move is made, the safest first re-root is:

- `src/Sanctuary/Oan.* -> src/San/`
- `src/Sanctuary/SLI.* -> src/SLI/`

That move should be performed as one governed migration slice rather than by
gradual naming drift.
