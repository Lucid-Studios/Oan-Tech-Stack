# CradleTek HOPNG Governance Telemetry Model

## Purpose

This note defines the first runtime use of `.hopng` artifacts as supplemental governance evidence emitted by the CradleTek transit spine.

## Boundary

This pass does not make `.hopng`:

- a governance decision surface
- a mutation surface
- a replacement for journals, receipts, or typed contracts
- a required runtime dependency for governance completion

This pass does make `.hopng`:

- supplemental evidence
- terminal-loop telemetry output
- auditable even when unavailable

## Root Statement

`.hopng` artifacts are derivative evidence emitted by the CradleTek transit spine. They may witness governance and telemetry, but they may not mutate governance, replace receipts, or become constitutive authority in this pass.

## Emission Window

The first landing emits `.hopng` only when a governance loop reaches:

- `LoopCompleted`
- `PendingRecovery`

No artifact is emitted for:

- intake
- review-request formation
- deferred intermediate state
- rejected pre-terminal return

## First Profiles

The first bounded profiles are:

- `GoverningTrafficEvidence`
- `GovernanceTelemetryPhaseStack`

The first profile witnesses the lawful governance path.

The second profile witnesses the temporal order of terminal governance telemetry.

## Authority Relation

The authoritative surfaces remain:

- journals
- shared actionable-content and mutation contracts
- typed governance decision and act receipts

`.hopng` is derivative of those surfaces.

It is not upstream of them.

## Optional Bridge Posture

The local HDT bridge is:

- optional
- developer-local
- path-independent in tracked history
- disabled by default

If the local bridge is unavailable, CradleTek must still emit explicit `Unavailable` receipts so absence remains auditable.

## Evidence Inputs

The first governance-loop profile may draw from:

- governance journal entries
- governance loop snapshot state
- decision and act receipts
- collapse qualification and routing
- actionable content handles
- request envelope identifiers
- mutation receipt identifiers

Protected evidence remains pointerized.

## Terminal Output Relation

The active output is rooted under runtime telemetry and signed as CradleTek-governed transit evidence.

The visible PNG remains a placeholder carrier in this pass.

The meaning-bearing surface is the sidecar set and the correlated receipts.

## Constitutional Relation

This note should be read with:

- `docs/HOLOGRAPHIC_DATA_TOOL.md`
- `docs/CRADLETEK_GOVERNED_CALL_TRANSIT_MODEL.md`
- `docs/ACTIONABLE_CONTENT_CONTRACT.md`
- `docs/CONTROL_SURFACE_MUTATION_LAW.md`

The relation is:

- HDT defines the artifact substrate
- CradleTek transit law defines who may emit it
- actionable-content law defines what lineage it may point to
- mutation law defines what it may never become

## Compression Line

CradleTek may emit `.hopng` evidence at terminal governance boundaries, but the evidence witnesses authority rather than becoming authority.
