# LINE_AUDIT_REPORT_SCHEMA

## Purpose

This note defines the first read-only schema for `line-audit-report` in the
active `OAN Mortalis V1.1.1` line.

The report exists to read the line truthfully through:

- current build posture
- current doctrine braid
- current verification status
- current telemetry taxonomy
- current declared telemetry surfaces

It is a readout surface.
It is not a mutation surface.

## Governing Compression

The governing rule is:

> `line-audit-report` must read the line through declared taxonomy and
> declared build truth rather than by inventing a private reporting worldview.

That means:

- it reads `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`
- it reads current line readiness and braid surfaces
- it reports what the record supports
- it does not infer standing from absence
- it does not normalize, heal, or repair missing fields

## Read-Only Boundary

`line-audit-report` is:

- line-local
- audit-facing
- read-only
- report-shaped

It is not:

- an `OAN.*` tool
- a repair assistant
- a mutation helper
- an operator surface
- a runtime authority

The first root implementation of this read-only surface now lives in:

- `tools/Get-LineAuditReport.ps1`

## Admission Marker

The current schema marker is:

- `line-audit-report-schema: frame-now`

That means:

- the schema is fixed now
- the first root read-only implementation is now admitted
- the schema does not itself authorize a CLI to infer beyond the record

## Required Inputs

The report may read from declared line-local surfaces such as:

- `BUILD_READINESS.md`
- `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`
- `V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md`
- `COMPANION_TOOL_TELEMETRY_LANE.md`
- `SOURCE_BUCKET_REPORT_CONSUMPTION_LANE.md`
- `.audit/state/*.json`
- `.audit/state/*.md`

The report may also read current build/test/hygiene results when they are
available to the caller.

The report must not:

- expose the private corpus path
- widen into cross-line mutation
- imply runtime authority from telemetry presence

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

## `reportIdentity`

The report identity fields are:

- `reportSurfaceName`
  Must be `line-audit-report`.
- `reportClass`
  Must remain read-only and report-facing.
- `generatedAtUtc`
  The time the report was emitted.
- `sourceLine`
  The line being read.

## `lineIdentity`

The line identity fields are:

- `lineName`
- `lineRoot`
  May be logical rather than absolute in outward rendering.
- `solutionPath`
- `activeExecutableTruthStatus`
- `siblingRelation`

## `linePosture`

The line posture fields are:

- `stateClass`
- `currentObjective`
- `buildabilityStatus`
- `authorityPosture`
- `readOnlyStanding`

## `verificationStatus`

The verification fields are:

- `buildStatus`
- `testStatus`
- `hygieneStatus`
- `diffCheckStatus`
- `auditCount`
- `integrationCount`
- `verificationSource`

If a verification field is not currently available, it must be reported as
`unavailable` rather than silently assumed successful.

## `doctrineBraid`

The doctrine braid fields are:

- `readinessSurface`
- `groupoidAuditSurface`
- `telemetryTaxonomySurface`
- `keyLaneSurfaces`
- `braidStatus`

`keyLaneSurfaces` should at minimum include the currently declared telemetry
lanes that materially shape the line read.

## `telemetryTaxonomy`

The taxonomy section must restate the declared classes used by the report:

- `semanticClasses`
- `packageClasses`
- `authorityClasses`
- `continuityClasses`
- `retentionClasses`
- `domainGroupoid`
- `splineGroupoid`

The report must not invent new classes during emission.

## `telemetrySurfaceInventory`

The inventory is a list of declared telemetry surfaces.

Each item must preserve the taxonomy tuple:

- `surfaceName`
- `domain`
- `spline`
- `semanticClass`
- `authorityClass`
- `continuityClass`
- `retentionClass`
- `packageClass`

Each item should also preserve, when available:

- `declaredBy`
- `stateSurfacePath`
- `runSurfacePath`
- `summarySurfacePath`
- `notes`

If an inventory field cannot be derived from the current declared record, it
must be marked:

- `undeclared`
  when the line has not named it
- `unavailable`
  when the line names it but current data is absent

## `warnings`

Warnings are current issues that matter to line interpretation.

Warnings may include:

- failed verification
- missing declared surfaces
- contradictory state
- unresolved braid tension

Warnings must not include speculative interpretation beyond the declared
record.

## `knownNoise`

Known noise is different from active warning.

It exists so the report can preserve current tolerated noise such as known
diff-check warnings without misclassing them as novel breakage every time.

Each known-noise entry should preserve:

- `noiseClass`
- `currentDisposition`
- `sourceSurface`

## `unavailableOrUndeclared`

This section must list any fields the report intentionally could not fill.

The report must prefer:

- explicit `undeclared`
- explicit `unavailable`

over:

- implicit omission
- success-by-absence
- heuristic repair

## Markdown Readout Order

If the future CLI renders markdown, the minimal order should be:

1. line identity and posture
2. verification summary
3. doctrine braid summary
4. telemetry taxonomy summary
5. telemetry surface inventory
6. warnings
7. known noise
8. unavailable or undeclared fields

## JSON Readout Rule

If the future CLI renders JSON, it should preserve the same section order and
field names as closely as practical.

The JSON readout must remain a structured witness, not a second semantic model.

## Non-Goals

This schema does not:

- implement the CLI
- authorize new telemetry mutation
- promote telemetry into runtime standing
- replace lane-specific law
- widen `line-audit-report` into cross-line authority

## Working Summary

`line-audit-report` is now schema-defined as a read-only line witness.

That means:

- it reads declared build truth
- it reads declared telemetry taxonomy
- it reports semantic class separately from package class
- it stays honest about missing or undeclared fields
