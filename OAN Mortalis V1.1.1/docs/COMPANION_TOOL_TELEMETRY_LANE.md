# COMPANION_TOOL_TELEMETRY_LANE

## Purpose

This note defines how companion-tool telemetry may lawfully enter the active
`OAN Mortalis V1.1.1` automation lane.

The current companion surfaces are:

- `Holographic Data Tool`
- `Trivium Forum`

They are companion tool lanes.
They are not build authority.

## Current Admission Marker

The active marker is:

- `companion-tool-telemetry: admitted-optional-bounded`

That means:

- companion-tool telemetry may be logged into the local automation lane now
- companion-tool telemetry may inform readiness reading and work cadence
- companion-tool telemetry may not silently widen runtime authority or replace
  repo-local executable truth

## Ingress Rule

Companion-tool telemetry enters the build lane only as bounded audit evidence.

That means:

- emitted `.audit/state` surfaces may be summarized into local `.audit` logs
- external absolute paths should be reduced to logical tool labels wherever
  possible
- missing telemetry must be logged as missing, not silently smoothed over

This lane follows `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`.

That means:

- the `.audit/state` file is a packaging surface, not the semantic definition
  of the carried telemetry
- the `.audit/runs` directory is a packaging surface, not a standing upgrade
  of the carried evidence

## Current Tool Read

The current intended read is:

- `Holographic Data Tool`
  may contribute bounded local automation telemetry when its `.audit` lane is
  present
- `Trivium Forum`
  may contribute bounded conference and tool-formation telemetry when its own
  `.audit` lane is present

Until `Trivium Forum` emits a matching `.audit` surface, the active line should
record that the repo exists while the automation telemetry lane is still
forming.

## Build Relation

The governing tools for this lane are:

- `tools/Write-CompanionToolTelemetry.ps1`
- `tools/Invoke-CompanionToolTelemetry.ps1`
- `tools/Invoke-Local-Automation-Cycle.ps1`
- `tools/Write-V111-EnrichmentPathway.ps1`

The emitted state surface is:

- `.audit/state/local-automation-companion-tool-telemetry-last-run.json`

The emitted bundle surface is:

- `.audit/runs/companion-tool-telemetry/`

## Non-Goals

This lane does not:

- promote companion tools into runtime authority
- treat external tool telemetry as a substitute for `V1.1.1` build proof
- imply `SoulFrame`, `AgentiCore`, `ListeningFrame`, or tool-use widening
- require `.hopng` or forum telemetry for the bounded local build lane to stay
  open

## Working Summary

Companion-tool telemetry is now admitted as bounded support evidence.

That means:

- `Holographic Data Tool` telemetry may flow into the local build logs now
- `Trivium Forum` telemetry may join the same lane as soon as its `.audit`
  surface is emitted
- the build lane stays truthful whether a companion tool is fully receipted,
  partially receipted, or still forming
