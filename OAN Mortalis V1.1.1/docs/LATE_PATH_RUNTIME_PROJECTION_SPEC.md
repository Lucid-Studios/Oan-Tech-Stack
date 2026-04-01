# LATE_PATH_RUNTIME_PROJECTION_SPEC

## Purpose

This note defines the next runtime projection cycle for the late path without
executing it in the current unlock.

Its job is to make the next cycle decision complete for the next
engineer or agent:

- what source seam will own each packet
- when each packet may be emitted
- who consumes the resulting receipt
- what must remain explicitly withheld

## Current Runtime Boundary

Current executable truth remains:

- `GovernedSeedRuntimeMaterializationService` is the only active runtime owner
  that constructs `FirstRunConstitutionSnapshot`
- `GovernedFirstRunConstitutionService` is the only active owner that turns
  that snapshot into a first-run receipt
- live runtime still projects `null` for chapter-seven, chapter-eight, and
  chapter-nine packets

That boundary must remain intact throughout this unlock cycle.

## Projection Source Ownership

The next runtime cycle should introduce source-backed late-path projection
through `Oan.FirstRun`, not through ad hoc runtime branching in
`CradleTek.Runtime`, `SoulFrame`, or `AgentiCore`.

The intended source owners are:

- `GovernedSeedElementalBindingProjectionService`
  projected from `Oan.FirstRun`
  owns `FirstRunElementalBindingPacket`
- `GovernedSeedActualizationSealProjectionService`
  projected from `Oan.FirstRun`
  owns `FirstRunActualizationSealPacket`
- `GovernedSeedLivingAgentiCoreProjectionService`
  reserved under `Oan.FirstRun`
  future owner of `FirstRunLivingAgentiCorePacket`

`GovernedSeedRuntimeMaterializationService` should later consume those source
services and continue to assemble one `FirstRunConstitutionSnapshot`.
It should not become the business-law owner of late-path doctrine.

## Chapter-Seven Runtime Cycle

The first runtime cycle after this unlock should implement only the
chapter-seven source seam.

### Packet

- `FirstRunElementalBindingPacket`

### Source Preconditions

- `FirstRunStewardWitnessedOePacket` is present
- `StewardStanding` is already truthful
- source-backed evidence exists for mediated `OE` loading into `SoulFrame`
- source-backed evidence exists for conditioned `cOE` loading into
  `AgentiCore`
- compression preparation can be evidenced without claiming Stone
  actualization

### Emission Timing

Emit the packet during first-run snapshot assembly, after pre-governance and
after chapter-six packet availability, but before any bond or Opal widening is
considered.

### Downstream Consumers

- `GovernedFirstRunConstitutionService`
- `GovernedSeedOperationalContext`
- `GovernedSeedStateModulationReceipt`
- outward first-run telemetry and trace persistence surfaces that already carry
  the first-run receipt

### Required Withholds

- `StoneActualizationWithheld` must remain `true`
- emission must not authorize live `OE -> SoulFrame` loading behavior
- emission must not authorize live `cOE -> AgentiCore` loading behavior
- emission must not advance `CurrentState` beyond `FoundationsEstablished`

## Chapter-Eight Runtime Cycle

The second runtime cycle after this unlock should implement the
chapter-eight source seam.

### Packet

- `FirstRunActualizationSealPacket`

### Source Preconditions

- chapter-seven packet projection is already real
- source-backed evidence exists for actualization seal
- introduction-governed review posture is source-backed
- primitive `SelfGEL` and durable identity vessel can be named without
  claiming living attachment

### Emission Timing

Emit the packet during first-run snapshot assembly only after the
chapter-seven seam exists, and still before living `AgentiCore` attachment is
considered.

### Downstream Consumers

- `GovernedFirstRunConstitutionService`
- `GovernedSeedOperationalContext`
- `GovernedSeedStateModulationReceipt`
- outward first-run telemetry and trace persistence surfaces that already carry
  the first-run receipt

### Required Withholds

- `LivingAgentiCoreWithheld` must remain `true`
- emission must not attach living `AgentiCore`
- emission must not widen tool-use embodiment
- emission must not by itself assert `OpalActualized`

## Chapter-Nine Future Seam

Chapter nine remains outside the next runtime cycle in this unlock.

The future projection seam is reserved as:

- `FirstRunLivingAgentiCorePacket`
- projected by `GovernedSeedLivingAgentiCoreProjectionService`

That seam should not be implemented until a packeted chapter-nine source-law
note is strong enough to promote chapter nine out of hold.

The future source seam is expected to account for:

- living `AgentiCore`
- `ListeningFrame`
- `Zed of Delta`
- `SelfGEL` attachment
- tool-use context attachment
- metanoia, registry, archive, and Society-facing continuity carriage

### Required Withholds In This Unlock

- no live `ListeningFrame` runtime behavior
- no live `Zed of Delta` runtime behavior
- no live `SelfGEL` attachment behavior
- no live tool-use attachment or widening
- no public widening from descriptive attachment alone

## Non-Goals

The current unlock does not implement any of the following:

- live `OE -> SoulFrame` loading
- live `cOE -> AgentiCore` loading
- Stone or Opal actualization behavior changes
- living `AgentiCore` attachment
- `ListeningFrame` runtime activation
- `Zed of Delta` runtime activation
- chapter-nine tool-use embodiment

## Working Summary

The next runtime cycle is now locked as:

- chapter seven: implement projection
- chapter eight: implement projection after chapter seven
- chapter nine: keep reserved until stronger packeted doctrine arrives

That is the intended order:

- source-backed late-path projection first
- runtime embodiment later
