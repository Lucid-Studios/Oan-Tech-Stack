# TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY

## Purpose

This note fixes the end-to-end telemetry taxonomy for the active
`OAN Mortalis V1.1.1` line.

It exists because the active repo already behaves as if a telemetry taxonomy
exists, but that taxonomy has so far been expressed indirectly through:

- doctrine
- `.audit/state` standing surfaces
- `.audit/runs` bundle layout
- retention and compaction policy
- audit practice

The purpose of this note is not to invent a second telemetry world.
It is to make the current one legible.

## Governing Compression

The governing correction is:

> `bundle` names packaging, not semantic carrier class.

That means:

- a `receipt` may be packaged inside a run bundle
- a `ledger` may be snapshotted into a state surface
- a `witness` may be emitted through a report readout
- a `candidate_packet` may be retained inside a daily bundle

The packaging does not redefine the semantic class.

The second governing correction is:

> every emitted telemetry surface should be readable through one shared
> identity tuple rather than through filename habit alone

## Semantic Class Versus Packaging Class

Semantic class answers:

- what the artifact means

Packaging class answers:

- how the artifact is wrapped, persisted, or exposed

No report surface, bundle directory, or retained pointer may infer semantic
meaning from package shape alone.

## Semantic Carrier Classes

The active semantic carrier classes are:

- `standing_surface`
  A current posture or standing projection.
  It reports present truth but is not itself event proof.
- `appendix_packet`
  Raw transport or handoff material awaiting later consumption.
- `receipt`
  Proof that a lawful event, transition, or bounded action occurred.
- `ledger`
  A continuity-bearing accumulation across a session, thread, lane, or other
  bounded line of activity.
- `witness`
  Observation, review, or burden-bearing confirmation that something was seen,
  checked, or held under law.
- `envelope`
  A bounded outward carrier for seam crossing without reopening the full
  interior artifact.
- `summary_digest`
  A derived read-only condensation for human or reporting use.
- `candidate_packet`
  Review-only material prepared for discernment, carry-forward, or later
  admission, but not yet admitted or inherited.

## Packaging Classes

The active packaging classes are:

- `state_surface`
  A rolling current-state file or equivalent standing exposure.
- `run_bundle`
  A bounded per-run or per-event packaging root under `.audit/runs/`.
- `daily_bundle`
  A compaction root that preserves daily continuity without retaining every
  raw hourly transport artifact forever.
- `report_readout`
  A human-readable or tool-readable derived summary surface.

## Telemetry Groupoids

The active telemetry surfaces should now be read through these orthogonal
groupoids:

- `domain`
  `Sanctuary -> CradleTek -> SoulFrame -> AgentiCore`
- `spline`
  `Install -> Build -> Run -> Rest -> Exit`
- `semanticClass`
  `standing_surface -> appendix_packet -> receipt -> ledger -> witness -> envelope -> summary_digest -> candidate_packet`
- `authorityClass`
  `transport -> evidence -> candidate -> admitted -> inherited`
- `continuityClass`
  `event -> session -> thread -> line -> sibling-line`
- `retentionClass`
  `rolling_state -> hourly_raw -> daily_compacted -> pinned_review -> durable`
- `packageClass`
  `state_surface -> run_bundle -> daily_bundle -> report_readout`

No single axis may substitute for another.

In particular:

- `authorityClass` is not implied by `packageClass`
- `continuityClass` is not implied by filename
- `semanticClass` is not implied by the word `bundle`
- `domain` placement does not by itself determine retention burden

## Required Identity Tuple

Every emitted telemetry surface should now be readable as:

- `{ domain, spline, semanticClass, authorityClass, continuityClass, retentionClass, packageClass }`

Where the current implementation has enough room, emitted telemetry should also
preserve:

- `subjectKey`
- `continuityKey`
- `witnessStatus`
- `sourceSurface`
- `lastLawfulTransition`

These optional fields do not replace the required identity tuple.

## Current Crosswalk

### Companion-Tool Telemetry

The current companion-tool lane in
`COMPANION_TOOL_TELEMETRY_LANE.md` already distinguishes:

- emitted `.audit/state` surfaces
- emitted bundle surfaces under `.audit/runs/`

Under this taxonomy:

- the last-run `.json` file is typically packageClass `state_surface`
- the emitted run directory is packageClass `run_bundle`
- the carried material remains bounded audit evidence and does not gain build
  authority merely because it is retained

### Source-Bucket Report Consumption

The current source-bucket report-consumption lane already gives the strongest
semantic split in the active repo:

> raw appendices are transport, consumed summaries are bounded formation,
> promoted carry-forward notes are admission, and long-term archives are
> inheritance.

Under this taxonomy:

- raw appendices are normally semanticClass `appendix_packet` with
  authorityClass `transport`
- consumption receipts remain semanticClass `receipt`
- standing summaries remain semanticClass `standing_surface` or
  `summary_digest` depending on whether they project current state or derived
  review
- simulated-GEL candidate packets remain semanticClass `candidate_packet`

### Runtime Workbench Session Law

The current runtime workbench law already fixes:

- the workbench-session ledger as a minimal membrane
- receipt chains as the early continuity spine

Under this taxonomy:

- the workbench-session ledger remains semanticClass `ledger`
- receipt chains remain semanticClass `receipt` until later admission law says
  otherwise
- witness remains separate from authorization

## Interpretation Consequences

The practical reading rules are:

- no telemetry surface should be named or reported only as a `bundle` without
  its semantic class being knowable
- no state surface should masquerade as an event receipt
- no receipt chain should be treated as autobiographical, canonical, or
  inherited merely because it survived retention
- no report readout should silently promote evidence into admission
- no daily bundle should redefine the standing of the semantic objects it
  carries

## Existing Bundle Pointer Rule

The active repo already uses many retained `last...Bundle` pointers in the
automation cycle state and retention scripts.

This note does not require mass renaming of existing `...Bundle` pointers.

Instead, it fixes their meaning:

- those pointers identify retained packaging roots or retained bundle-shaped
  outputs
- they do not by themselves tell the full semantic class of the carried
  artifact

Any future readout surface should therefore report:

- package class
- semantic class

as separate fields whenever possible.

## Relation To Later Readouts

Later read-only report surfaces such as:

- `line-audit-report`
- `line-readiness-report`
- `line-braid-report`
- `line-trace-readout`

should read this taxonomy rather than invent a private one.

The first fixed read-only schema for that family now lives in:

- `LINE_AUDIT_REPORT_SCHEMA.md`

That means later readouts must remain:

- line-local unless lawfully widened
- read-only unless explicitly reclassed
- honest about current authority and retention class

## Non-Goals

This note does not:

- create new runtime authority
- promote transport into admission
- replace existing lane-specific law notes
- require immediate renaming of every emitted bundle path
- claim that `V1.2.1` currently has the same active telemetry burden as
  `V1.1.1`

## Working Summary

The active repo already has telemetry law.
It now also has one telemetry taxonomy.

That means:

- semantic carrier class is now explicit
- packaging class is now explicit
- telemetry groupoids are now explicit
- every emitted surface can be read through one identity tuple instead of
  through filename habit alone
