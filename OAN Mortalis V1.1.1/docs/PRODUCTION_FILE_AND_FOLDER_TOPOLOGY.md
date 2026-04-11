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

`Sanctuary` is a runtime chamber and governed access layer.

It is not the final production source-owning family root.

That means the source tree should not permanently imply:

- `Sanctuary` owns `Oan.*`
- `Sanctuary` owns `SLI.*`
- every project under the governed chamber belongs to one source family because
  it happens to run inside that chamber

In compact form:

> runtime chamber is not source ownership.

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

- `src/Sanctuary/` is currently a chambered staging root, not a canonical
  source-family owner

The projects beneath it are governed by their family prefixes, not by the
folder name `Sanctuary`.

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

- `Oan.*` for umbrella stack composition and stack-level contracts
- `SLI.*` for symbolic grammar and symbolic runtime
- `CradleTek.*` for infrastructure and substrate ownership
- `SoulFrame.*` for operator and identity-facing workflow ownership
- `AgentiCore.*` for agent runtime ownership
- `GEL.*` remains a bounded supporting domain surface until stronger family
  placement is fixed

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
    Oan/
      Oan.Common/
      Oan.FirstRun/
      Oan.HostedLlm/
      Oan.Nexus.Control/
      Oan.PrimeCryptic.Services/
      Oan.Runtime.Headless/
      Oan.Runtime.Materialization/
      Oan.State.Modulation/
      Oan.Trace.Persistence/
    SLI/
      SLI.Engine/
      SLI.Ingestion/
      SLI.Lisp/
    TechStack/
      AgentiCore/
        AgentiCore/
      CradleTek/
        CradleTek.Custody/
        CradleTek.Host/
        CradleTek.Mantle/
        CradleTek.Memory/
        CradleTek.Runtime/
      GEL/
        GEL.Contracts/
      SoulFrame/
        SoulFrame.Bootstrap/
        SoulFrame.Membrane/
  tests/
    Sanctuary/
  tools/
```

This target keeps the current `TechStack/` grouping where it still helps line
readability, but it removes `Sanctuary` as a misleading source-owning root.

## Template Rule For Future Designs

Future line templates should inherit these rules:

### 1. Do not use a runtime chamber name as the primary source-family root

Do not default to:

- `src/Sanctuary/...`
- `src/House/...`
- `src/Chamber/...`

when the projects are really owned by family prefixes like `Oan.*`, `SLI.*`,
or `CradleTek.*`.

### 2. Use family-first roots for family-owned code

If the family prefix is stable, the source root should reflect it.

Examples:

- `src/Oan/`
- `src/SLI/`

### 3. Keep grouped substrate families grouped only when it improves flow

`TechStack/` may remain as a grouping root when it improves substrate and
runtime readability, but it should not erase family distinction.

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
2. treat `src/Sanctuary/` as transitional in doctrine and review
3. avoid creating new long-lived projects under `src/Sanctuary/` unless the
   project is explicitly a chamber-facing `Oan.*` bridge that will later be
   re-rooted
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

The only root that should now be treated as explicitly transitional is:

- `src/Sanctuary/`

## Immediate Build Consequence

When new projects or folders are added:

- use family ownership first
- use chamber language only for runtime or test surfaces
- do not treat `Sanctuary` as the physical owner of `Oan.*` and `SLI.*`
  forever

If a future move is made, the safest first re-root is:

- `src/Sanctuary/Oan.* -> src/Oan/`
- `src/Sanctuary/SLI.* -> src/SLI/`

That move should be performed as one governed migration slice rather than by
gradual naming drift.
