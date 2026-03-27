# V1.0 Retirement Gate

## Purpose

This document freezes the retirement judgment that moved the old `V1.0` line into archived reference-only status beneath the active `V1.1.1` build.

The governing rule is simple:

- carry forward only law-bearing seams that still matter to current executable truth
- retire old glue, duplicated ownership, and compatibility-heavy runtime surfaces
- archive `V1.0` only after the remaining seams below are judged explicitly

## Already Re-Expressed In V1.1.1

These `V1.0` surfaces no longer need to remain active authorities:

- `CradleTek.Public`
- `CradleTek.Cryptic`
- `CradleTek.Runtime`
- `CradleTek.CognitionHost`
- `SoulFrame.Host`
- `Oan.Cradle`
- `Oan.SoulFrame`
- `Oan.Sli`
- `Oan.Fgs`
- `OAN.Core`
- broad compatibility glue and placeholder transport layers

These laws have already been re-expressed in `V1.1.1` through Sanctuary resident services, SoulFrame mediation, the hosted Lisp bundle, the hosted LLM seed, and the bounded outward ladder.

## Re-Expressed Final Utility Seams

The last small utility seams worth carrying from `V1.0` have now been re-expressed as Sanctuary-native trace and persistence surfaces:

- old `Data.Cryptic/CrypticPointerStore.cs`
- old `Telemetry.GEL/GelTelemetryAdapter.cs`

Their new lawful seat is:

- `src/Sanctuary/Oan.Trace.Persistence`

They now serve the outward ladder of `V1.1.1` directly by recording duplex pointer handles and telemetry records from the materialized evaluation envelope, without reviving old storage or host glue.

## Remaining Judgment Queue

Only doctrine-rich, code-poor material remains for reference review before archive.

No further `V1.0` code family is currently required as an active engineering dependency of `V1.1.1`.

## SoulFrame.Identity Judgment

`SoulFrame.Identity` no longer needs to remain a separate code family.

Its only law-bearing core has now been re-expressed directly in the `V1.1.1` SoulFrame bootstrap receipt as a detached identity seat carrying:

- SoulFrame handle
- CME handle
- opal-engram seat handle
- operator-bond handle
- `SelfGEL` / `cSelfGEL` pairing
- default runtime policy
- detached attachment state
- integrity hash

The old registry, context wrapper, and factory/service posture are retired as implementation shape, not carried authority.

## EngramGovernance Judgment

`EngramGovernance` does not need to remain an active code family.

Its prior model surfaces now land in one of three places:

- `OEDecisionEntry` and related decision posture map to the current nexus, governance, and protected-path receipts already carried by `V1.1.1`
- `RootEngramRecord` and related root/atlas semantics map to the lawful atlas and memory-source contracts already rebuilt in `CradleTek.Memory`
- `EngramTelemetry` now maps to the Sanctuary-native trace layer in `Oan.Trace.Persistence`

The remaining `EngramCompassState` and constructor/bootstrap materials are not current runtime dependencies. They belong to:

- future `ListeningFrame` / `Compass` work in `AgentiCore`
- GEL or doctrine where the semantics are still maturing

So the old `EngramGovernance` services, ledger writer, steward agent, and bootstrap stack are retired as implementation shape, not carried authority.

## Archive Condition

`V1.0` is ready to become reference-only when all of the following are true:

- `V1.1.1` build, test, and hygiene remain green
- CME formation and scope law are present enough to stand without `V1.0` office glue
- Lisp/Cryptic and Prime/C# runtime separation is explicit in `V1.1.1`
- the remaining `SoulFrame.Identity` and `EngramGovernance` judgment queue is explicitly closed
- no active runtime behavior in `V1.1.1` still requires `V1.0` code to explain it

## Practical Read

`V1.0` is no longer the active architectural teacher.

Its remaining role is archived reference review, not a live dependency chain.

Archive status:

- `V1.0` remains in the repository for historical review
- `V1.0` is no longer an active mutation surface
- `V1.1.1` is the sole active build line
