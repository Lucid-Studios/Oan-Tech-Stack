# PRODUCTION_FILE_AND_FOLDER_TOPOLOGY

## Purpose

This note fixes the production-facing file and folder flow for
`OAN Mortalis V1.2.1`.

It also corrects the umbrella family at the beginning of the new line so the
production form does not inherit a naming claim that belongs to a later tool.

## Core Rule

For `V1.2.1`:

- `San.*` is the umbrella stack composition and stack-level contract family
- `Sanctuary` is the parent container in the architectural sense
- `OAN.*` is reserved for the future `OAN` tool and must not be used early as
  the umbrella family of the line

In compact form:

> Sanctuary is the parent container.  
> `San.*` is the umbrella family of the line.  
> `OAN.*` is reserved for the later tool.

## Production Target Topology

The target production topology for `V1.2.1` is:

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

## Migration Read

This means the first family correction for the new line is:

- `src/Sanctuary/Oan.* -> src/San/`
- `namespace Oan.* -> namespace San.*`

while:

- `SLI.*`, `CradleTek.*`, `SoulFrame.*`, `AgentiCore.*`, and `GEL.*` keep
  their family truth
- `OAN.*` remains withheld until the actual `OAN` tool exists

## Immediate Consequence

When the first real source-family carry forward begins in `V1.2.1`:

- do not create new `Oan.*` projects
- create `San.*` stack-level projects instead
- treat the older `Oan.*` family as historical carry-forward debt from
  `V1.1.1`, not as the naming truth of the new line
