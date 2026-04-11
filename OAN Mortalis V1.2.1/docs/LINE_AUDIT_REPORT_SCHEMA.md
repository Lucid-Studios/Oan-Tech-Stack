# LINE_AUDIT_REPORT_SCHEMA

## Purpose

This note defines the first read-only schema for `line-audit-report` in the
install-first `OAN Mortalis V1.2.1` line.

It exists so the sibling line can be read through one lawful report shape
before later runtime telemetry thickens.

## Governing Compression

The governing rule is:

> `line-audit-report` must describe this line through declared taxonomy and
> declared build truth, not through heuristic completion.

That means:

- the report is line-local
- the report is read-only
- the report may say `undeclared` or `unavailable`
- the report must not heal gaps by inference

## Read-Only Boundary

`line-audit-report` is:

- line-local
- audit-facing
- read-only
- report-shaped

It is not:

- an `OAN.*` tool
- a mutation helper
- a runtime authority
- an operator surface

The first root implementation of this read-only surface now lives in:

- `tools/Get-LineAuditReport.ps1`

## Admission Marker

The current schema marker is:

- `line-audit-report-schema: frame-now`

## Top-Level Schema

The minimal top-level readout shape is:

- `reportIdentity`
- `lineIdentity`
- `linePosture`
- `verificationStatus`
- `doctrineBraid`
- `telemetryTaxonomy`
- `telemetrySurfaceInventory`
- `warnings`
- `knownNoise`
- `unavailableOrUndeclared`

## Inventory Rule

Each telemetry inventory item must preserve:

- `surfaceName`
- `domain`
- `spline`
- `semanticClass`
- `authorityClass`
- `continuityClass`
- `retentionClass`
- `packageClass`

If the current install-first line has not yet declared or emitted a field, the
report must say:

- `undeclared`
- `unavailable`

rather than silently omitting it or filling it by convenience.

## Install-First Consequence

Because `V1.2.1` is still install-first:

- the report may legitimately carry a thin or empty telemetry inventory
- the report should still carry line identity, posture, doctrine braid, and
  taxonomy sections
- missing runtime telemetry must be named honestly rather than treated as
  failure or success by silence

## Relation To Taxonomy

`line-audit-report` in this line must read:

- `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`

before it reports telemetry classes.

## Working Summary

`V1.2.1` now shares the same read-only `line-audit-report` schema as the
active line, while preserving the install-first right to report many telemetry
fields as still undeclared or unavailable.
