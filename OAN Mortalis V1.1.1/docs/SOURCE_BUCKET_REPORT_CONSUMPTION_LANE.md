# Source-Bucket Report Consumption Lane

## Purpose

This note defines the root-side report-consumption lane for the active
`OAN Mortalis V1.1.1` line.

The lane exists so source-bucket automation does not keep widening into:

- raw hourly telemetry overgrowth
- thread proliferation
- long-term archive sprawl
- counterfeit continuity claims

The lane consumes bounded source-bucket appendices and authoritative state
surfaces into continuity-aware summaries, carry-forward candidates, and
simulated-GEL candidate packets.

## Admission Marker

This is an active build-side control-plane note for `V1.1.1`.

It complements:

- `SOURCE_BUCKET_FEDERATION_LANE.md`
- `BUILD_READINESS.md`
- the source-bucket request and return contracts

It does not replace source-bucket local audits.

## Core Law

The governing law is:

> raw appendices are transport, consumed summaries are bounded formation,
> promoted carry-forward notes are admission, and long-term archives are
> inheritance.

This means:

- bucket lane-watch and working-post emissions remain short-retention
  transport by default
- root consumption decides what changed materially
- only discerned survivors move toward documentation or simulated-GEL
  carry-forward
- no direct shortcut is allowed from raw delta to GEL candidate

This lane also follows `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`.

That means:

- raw appendices remain semantic transport even when they are retained in
  bundle-shaped form
- consumed receipts, summaries, and candidate packets keep their semantic class
  even when they share nearby packaging roots

## Contract Surface

The lane is governed by:

- `source-bucket-report-appendix-contract.json`
- `source-bucket-thread-continuity-contract.json`
- `source-bucket-report-consumption.json`

The root consumer writes:

- `.audit/state/current-source-bucket-standing-summary.json`
- `.audit/state/current-source-bucket-standing-summary.md`
- `.audit/state/current-candidate-gel-items.json`
- `.audit/state/current-candidate-gel-items.md`
- `.audit/runs/report-consumption/<bundle-id>/consumption-receipt.json`
- `.audit/runs/report-consumption/<bundle-id>/delta-since-last-summary.json`
- `.audit/runs/report-consumption-daily/YYYY-MM-DD/bucket-standing-summary.json`
- `.audit/runs/report-consumption-daily/YYYY-MM-DD/bucket-standing-summary.md`
- `.audit/runs/report-consumption-daily/YYYY-MM-DD/candidate-gel-items.json`
- `.audit/runs/report-consumption-daily/YYYY-MM-DD/candidate-gel-items.md`
- `.audit/runs/report-consumption-daily/YYYY-MM-DD/simulated-gel-review-packet.md`

## Continuity Law

The same continuity-bearing thread should be reused by default when:

- `continuityKey` is unchanged
- `discourseOffice` is unchanged
- subject identity remains the same
- no contradiction requires separate adjudication
- no lawful successor transition is required

The lane opens a successor only when:

- subject identity materially changes
- `discourseOffice` changes
- a true continuity break occurs
- the prior thread is sealed and succeeded

This keeps:

- appendices hourly
- summaries bounded
- threads continuity-bearing
- GEL candidates refined rather than improvised

## Standing Classes

The root consumer uses these standing classes:

- `transport_only`
- `consumed_no_material_change`
- `consumed_delta`
- `carry_forward_candidate`
- `gel_research_candidate`
- `pinned_for_review`

Material change exists only when one of these changes:

- lane status
- next lawful action
- blocker set
- milestone
- build or test counts materially
- repo clean or dirty standing
- source-bucket request, return, or handshake state
- contradiction or drift posture

## Promotion Gate

The consumer may move:

- `transport_only -> consumed_no_material_change`
- `transport_only -> consumed_delta`
- `consumed_delta -> carry_forward_candidate`

only after one discernment pass.

The consumer may move:

- `carry_forward_candidate -> gel_research_candidate`

only when:

- discernment status is `stable_enough_for_review`
- stability has been witnessed across at least two consumed windows
- one of those windows is the noon-to-noon full research boundary
- contradiction state is not `contradiction_detected`

The lane explicitly forbids:

- `consumed_delta -> gel_research_candidate`

## Retention And Compaction

Retention defaults are:

- raw appendices: keep `72` hours after successful consumption
- daily compacted bundles: keep `30` days
- carry-forward, GEL-candidate, and pinned evidence: retain until review
  clears them

Additional compaction law:

- for any `(bucketLabel, continuityKey, reportClass)` tuple with no material
  delta, keep only the latest raw appendix even within the `72` hour window
- delta-bearing raw appendices remain until daily compaction completes unless
  pinned

The intent is signal preservation without archive swarm.

## Schedule

The authoritative cadence is owned by Windows Task Scheduler under
`OAN Tech Stack`.

- `:00` every hour local time:
  root build work and source-bucket hourly emissions
- `:30` every hour local time:
  report consumption, continuity reconciliation, and compaction
- `12:30 PM` local time every day:
  the `:30` consumer runs in full research mode over the closed noon-to-noon
  window

## Documentation Repo Relation

`Documentation Repo` is not the raw telemetry sink.

This lane feeds `Documentation Repo` only with:

- consumed carry-forward candidates
- simulated-GEL candidate packets
- pinned review material when review context is required

Raw hourly telemetry remains local transport and short-retention evidence.

## Non-Goals

This lane must not:

- archive every hourly appendix forever
- self-authorize inheritance
- self-authorize executable truth
- bypass build admission
- treat repeated noise as continuity simply because it survived storage
