# STACK_ROOT_RENAMING_MIGRATION_PLAN

## Purpose

This note defines the governed migration from the older `OAN`-root code and
repository presentation model toward the clarified Sanctuary-root stack truth.

It exists so the stack can migrate naming law without treating a repo rename
or namespace correction as an impulsive cleanup event.

## Corrected Naming Truth

The active architectural truth is now:

- `Sanctuary` is the stack root and constitutional host substrate
- `OAN` is a downstream application, game, or domain built from that stack

That means the foundational family map is:

- `San.*`
- `Ctk.*`
- `Sfr.*`
- `Acr.*`
- `SLI.*`

`Oan.*` is not the forward foundational namespace root.

## Immediate Forward Freeze

From this slice forward:

- no new foundational namespace may use `Oan.*`
- no new foundational project may use an `Oan.*` identity
- new Sanctuary-root code should use `San.*`
- new CradleTek code should use `Ctk.*`
- new SoulFrame code should use `Sfr.*`
- new AgentiCore code should use `Acr.*`

Legacy `Oan.*` source and project surfaces remain temporary migration holds
only.

They are governed by the line-local allowlist:

- `build/legacy-oan-namespace-allowlist.json`

## Why This Migration Exists

The rename is not cosmetic.

It corrects a false teaching surface where:

- the repo name
- the root folder language
- the source-family names
- the architecture docs

would otherwise continue to imply that `OAN` is the foundational stack root.

That is no longer truthful.

## Growth Read

The corrected host and family picture is:

- `San.*` is the first lawful runtime habitat and constitutional host
- `Sfr.*` and `Acr.*` may operate natively inside Sanctuary in bounded form
- `Ctk.*` is a major extension family for remote, distributed, hosted, or
  outward runtime embodiment
- `OAN` may later stand as a downstream application or game surface built from
  these families

## Migration Phases

### Phase 1: Naming Constitution

Seat the naming truth in governance and line-local docs.

Deliverables:

- family constitution corrected
- topology note corrected
- migration plan written

### Phase 2: Forward Freeze

Stop new naming drift.

Deliverables:

- `legacy-oan-namespace-allowlist.json`
- audit checks that no new `Oan.*` namespace or project surface appears

### Phase 3: Code and Project Re-rooting

Migrate legacy `Oan.*` source and project names into `San.*` families as
bounded governed slices.

Expected candidates include:

- `Oan.Common` -> `San.Common`
- `Oan.FirstRun` -> `San.FirstRun`
- `Oan.HostedLlm` -> `San.HostedLlm`
- `Oan.Nexus.Control` -> `San.Nexus.Control`
- `Oan.Runtime.*` -> `San.Runtime.*`

### Phase 4: Folder and Solution Truth

Migrate physical folder surfaces so they stop implying the older ontology.

Expected targets include:

- `src/San/`
- `src/Ctk/`
- `src/Sfr/`
- `src/Acr/`

The current `src/Sanctuary/` folder remains a transitional staging root until
that move is executed lawfully.

### Phase 5: Repo and Workspace Truth

Only after code and docs tell the new truth cleanly should the wider repo and
workspace names be renamed.

This includes:

- repository name
- root folder name
- linked doc references
- any CI, automation, or onboarding surfaces that depend on those names

## Verification Checklist

Before a later physical rename event:

- build passes
- tests pass
- hygiene passes
- forward naming freeze remains green
- no new foundational `Oan.*` surfaces have appeared
- docs tell the Sanctuary-root truth consistently

## Non-Goals

This slice does not:

- rename the repository yet
- rename the root folder yet
- rename every legacy `Oan.*` project immediately
- pretend that the migration is complete because the law is now explicit

## One-Line Compression

> Sanctuary is the foundational stack root, OAN is a downstream application
> identity, and the repo now freezes new `Oan.*` drift while it prepares a
> governed rename and re-rooting migration.
