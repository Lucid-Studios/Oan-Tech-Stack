# CRYPTIC_CUSTODY_SOULFRAME_MEMBRANE_SPEC

## Purpose

This document defines the next contract extraction after `GEL.Contracts`:

- Cryptic custody contracts
- SoulFrame membrane contracts
- Prime derivative contracts

The goal is to make source-domain versus derivative-domain law explicit in code before the next large storage or service split.

## Constitutional Trigger

Source doctrine:

- `docs/PRIME_CRYPTIC_DATA_TOPOLOGY.md`
- `docs/SYSTEM_ONTOLOGY.md`
- `docs/STACK_AUTHORITY_AND_MUTATION_LAW.md`

Decision rule:

- Cryptic originates identity-bearing inscription
- Prime expresses processed derivatives
- SoulFrame governs lawful transformation between them

## Current Gap

The repo has a coarse plane split, but not a lawful custody or membrane split.

Current evidence:

- [`src/Oan.Common/IPlaneStores.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\Oan.Common\IPlaneStores.cs)
  - distinguishes public and cryptic appends, but not sovereignty, membrane law, or governed re-Engrammitization
- [`src/CradleTek.Cryptic/CrypticLayerService.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\CradleTek.Cryptic\CrypticLayerService.cs)
  - models cryptic storage as pointer storage only
- [`src/CradleTek.Mantle/MantleOfSovereigntyService.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\CradleTek.Mantle\MantleOfSovereigntyService.cs)
  - already hints at shadow and restore semantics, but not full custody law
- [`src/SoulFrame.Host/SoulFrameSession.cs`](D:\OAN%20Tech%20Stack\OAN%20Mortalis%20V1.0\src\SoulFrame.Host\SoulFrameSession.cs)
  - models session behavior, but not mitigated self-state membrane transfer

That means the repo still lacks explicit interfaces for:

- governed Cryptic inscription
- guarded read from sovereign custody
- SoulFrame-mediated mitigation and projection
- collapse-return intake as a new Cryptic-side act
- Prime derivative publication as non-sovereign output

## Target Contract Zones

### 1. Cryptic Source Domain Contracts

These contracts define the source-domain acts that may originate or renew continuity-bearing state.

Required acts:

- append or inscribe into protected custody
- guarded read by governed handle
- governed re-Engrammitization
- restore or reconstruct by admitted identity handle

These contracts must not be implemented by Prime publication surfaces.

### 2. SoulFrame Membrane Contracts

These contracts define lawful transformation between source-domain custody and operational use.

Required acts:

- project mitigated working state outward from Cryptic custody
- receive collapse or operational residue as intake material
- validate whether intake is admissible for re-Engrammitization
- emit derivative-safe payloads without disclosing sovereign substance

These contracts must not collapse into raw storage or raw publication.

### 3. Prime Derivative Contracts

These contracts define processed output behavior only.

Required acts:

- pointerize
- redact
- encrypt for release
- publish checked derivative outputs
- expose Prime-safe read models

These contracts must not mutate or reconstruct Cryptic sovereign substance.

## Proposed Interfaces

The exact names may still be adjusted, but the contract shapes should be this narrow.

### Cryptic Custody

#### `ICrypticCustodyStore`

Purpose:

- own append and guarded read against source-domain custody

Minimum operations:

- `AppendAsync(...)`
- `ReadGuardedAsync(...)`
- `RestoreAsync(...)`

Law:

- append is a sovereign act
- read is guarded, not ambient
- restore is governed by identity handle and policy

#### `ICrypticReengrammitizationGate`

Purpose:

- represent the lawful intake path for return from operational collapse into Cryptic custody

Minimum operations:

- `ReengrammitizeAsync(...)`
- `CanAdmitAsync(...)`

Law:

- return is not raw write-back
- return is a new Cryptic-side inscription decision

### SoulFrame Membrane

#### `ISoulFrameMembrane`

Purpose:

- govern mitigated projection from sovereign custody into operational use

Minimum operations:

- `ProjectMitigatedAsync(...)`
- `ReceiveReturnIntakeAsync(...)`

Law:

- projection yields mitigated working state
- membrane receives return material without directly committing it

#### `ISelfStateProjection`

Purpose:

- define a policy-neutral shape for SoulFrame-mediated working self-state

Law:

- operational
- non-sovereign
- admissible to AgentiCore
- not equivalent to raw Cryptic custody

### Prime Derivative

#### `IPrimeDerivativePublisher`

Purpose:

- publish checked, processed, Prime-safe outputs

Minimum operations:

- `PublishPointerAsync(...)`
- `PublishRedactedAsync(...)`
- `PublishEncryptedAsync(...)`

Law:

- may express derivatives
- may not unveil or reconstruct Cryptic

#### `IPrimeDerivativeView`

Purpose:

- expose Prime-safe read models for hosted service use

Law:

- read-only derivative access
- no sovereign authority

## Extraction Target

The first extraction should be contract-only.

Do not move implementations yet.

Create or sharpen only the interfaces and neutral request or response shapes required to encode:

- source-domain custody
- SoulFrame membrane transfer
- Prime derivative publication

## Suggested Placement

Preferred placement for the first pass:

- shared neutral contract interfaces in `Oan.Common` only if they stay truly policy-neutral
- otherwise a dedicated contract surface such as `SoulFrame.Contracts` or `CradleTek.Custody.Contracts`

Decision rule:

- if the interface is cross-family and policy-neutral, it may live in a shared contract surface
- if the interface smuggles sovereignty-specific or membrane-specific law, it should live in the owning family contract surface

## Forbidden Shortcuts

Do not let this pass drift into:

- implementation movement
- storage backend extraction
- governance rewrite
- Prime and Cryptic repository merge or duplication
- direct Prime write access to Cryptic stores
- direct AgentiCore ownership of sovereign identity custody

## Acceptance Criteria

This contract extraction is complete when:

- the repo has explicit custody and membrane interfaces
- Prime derivative publication is separated from Cryptic inscription in contract form
- return to Cryptic custody is modeled as re-Engrammitization, not write-back
- SoulFrame has an explicit membrane role in code law
- the next implementation split can be aimed at source-domain, membrane, or derivative-domain surfaces without ambiguity

## Current Implementation Status

This first extraction now exists in code through:

- `src/Oan.Common/CrypticCustodyContracts.cs`
- `src/Oan.Common/SoulFrameMembraneContracts.cs`
- `src/Oan.Common/PrimeDerivativeContracts.cs`

Current concrete bindings:

- `CradleTek.Mantle.MantleOfSovereigntyService`
  - `ICrypticCustodyStore`
  - `ICrypticReengrammitizationGate`
- `SoulFrame.Host.SoulFrameHostClient`
  - `ISoulFrameMembrane`
- `CradleTek.Public.PublicLayerService`
  - `IPrimeDerivativePublisher`
  - `IPrimeDerivativeView`
- `Oan.Storage.PrimeDerivativePublisherAdapter`
  - compatibility derivative publisher over legacy public plane storage
- `Oan.Sli.RoutingEngine`
  - now consumes `IPrimeDerivativePublisher` for the active Prime publication path rather than reaching directly through `IPublicPlaneStores`

Compatibility note:

- `Oan.Common/IPlaneStores.cs` remains present as a legacy coarse-grain compatibility surface
- it is no longer the preferred extension point for new custody or membrane work
- the active headless runtime now routes Prime publication through a derivative-specific contract lane
