# LOCAL_AUTOMATION_TASKING_SURFACE

## Purpose

This document defines the formal tasking surface for the local governed automation cycle.

It exists so the automation is not merely "running somewhere."

It must always be possible to say:

- which tasks exist
- which authority owns them
- what completes them
- what makes them escalate
- where their live status is written

## Governing Contract

The machine-readable contract lives in:

- `build/local-automation-tasking.json`

The live applied status surfaces are:

- `.audit/state/local-automation-tasking-status.json`
- `.audit/state/local-automation-tasking-status.md`
- `.audit/state/local-automation-active-task-map-run.json`

Those live surfaces apply the current automation state onto the formal task definitions below.

## Long-Form Task Maps

The tasking surface also carries bounded long-form maps.

Current active map:

- `Automation Maturation Map 06`

Next eligible map:

- `Automation Maturation Map 07`

Time-dilation rule:

- if the active map completes earlier than expected, the agentic working group may pull work only from the next declared map
- no pull-forward may skip beyond that next map
- blocked or HITL-required posture prevents pull-forward

This keeps acceleration lawful without turning early completion into uncontrolled scope widening.

## Active Long-Form Run Law

Each active long-form run must work inside one bounded review window.

The current run window closes at the next declared release-candidate cadence unless a narrower window is declared by policy.

Within that window the automation may:

- consider `3` exploratory model structures
- preserve those structures as bounded run phases
- collapse them into `1` final fourth structure before the window ends

That means the automation may stretch inside one active map, but it may not remain indefinitely exploratory.

The live active-run surface is:

- `.audit/state/local-automation-active-task-map-run.json`

The active-run bundle root is:

- `.audit/runs/long-form-task-maps/`

## Formal Tasks

### Release Candidate Cycle

- Owner: `Automation`
- Authority: `Mechanical only`
- Cadence: every `6` hours
- Purpose: run the governed release-candidate conveyor and emit the latest candidate evidence bundle
- Completion signal: a valid `build-evidence-manifest.json` exists in the newest release-candidate bundle
- Escalates when:
  - the local cycle becomes `blocked`
  - the release-candidate run cannot emit a valid bundle

### Daily HITL Digest

- Owner: `Automation`
- Authority: `Mechanical only`
- Cadence: every `24` hours
- Purpose: generate the single bounded daily review surface for HITL
- Completion signal: the latest digest bundle contains both JSON and Markdown review artifacts
- Escalates when:
  - the digest posture requires immediate HITL
  - the digest bundle cannot be produced

### Promotion Watch

- Owner: `Shared`
- Authority: `Automation may classify; HITL decides promotion`
- Trigger: latest digest posture
- Purpose: determine whether automation may continue stretching, whether HITL is required, or whether the system is blocked
- Completion signal: the current posture is reflected in the task-status surface
- Escalates when:
  - the recommended action becomes review-required-before-promotion
  - the posture becomes `blocked`

### Scheduler Watch

- Owner: `Automation`
- Authority: `Machine-local only`
- Trigger: Windows scheduled task registration and next-run clock
- Purpose: ensure the local automation cycle is actually scheduled and not merely manually provable
- Completion signal: the scheduled task is registered and exposes a next run time
- Escalates when:
  - the scheduled task is not registered
  - the scheduled task has no next run time

## First Long-Form Task Set

### Automation Maturation Map 01

- Goal: make the current local automation cycle report transitions, first scheduled execution truth, and status freshness without requiring manual probing
- Expected review windows: `2`
- Advanced tasks:
  - `Notification Surface`
  - `First Scheduled Run Capture`
  - `Status Freshness Reconciliation`

Current live output surfaces carried forward from this map:

- transition-triggered notifications land under `.audit/runs/notifications/`
- the last evaluated notification state lives at `.audit/state/local-automation-notification-last-run.json`

### Automation Maturation Map 02

- Goal: deepen unattended evidence quality without crossing into ungated promotion authority
- Expected review windows: `2`
- Advanced tasks:
  - `Delta Summary Surface`
  - `Artifact Retention Pruning`
  - `Blocked Escalation Bundle`

Current live output surfaces for this map:

- delta summaries land inside each digest bundle as `delta-summary.json` and `delta-summary.md`
- retention pruning writes its last-run state to `.audit/state/local-automation-retention-last-run.json`
- blocked escalation bundles land under `.audit/runs/blocked-escalations/` and update `.audit/state/local-automation-blocked-escalation-last-run.json` when the posture is `blocked`

### Automation Maturation Map 03

- Goal: introduce seeded governance participation into unattended build review, reconcile scheduler/runtime cadence truth, and expose a CME formalization consolidation surface without widening promotion authority
- Expected review windows: `2`
- Advanced tasks:
  - `Seeded Governance Lane`
  - `Scheduler Cadence Reconciliation`
  - `CME Formalization Consolidation Surface`

Current live output surfaces for this map:

- seeded governance bundles land under `.audit/runs/seeded-governance/` and update `.audit/state/local-automation-seeded-governance-last-run.json`
- scheduler reconciliation writes `.audit/state/local-automation-scheduler-reconciliation-last-run.json`
- CME consolidation writes `.audit/state/local-automation-cme-consolidation-state.json` and its paired Markdown surface

### Automation Maturation Map 04

- Goal: close promotion and release rehearsal once seeded governance and runtime cadence are stable
- Expected review windows: `2`
- Advanced tasks:
  - `Promotion Gate Bundle`
  - `CI Artifact Concordance`
  - `Release Ratification Rehearsal`

Current live output surfaces for this map:

- promotion gate bundles land under `.audit/runs/promotion-gates/` and update `.audit/state/local-automation-promotion-gate-last-run.json`
- CI concordance bundles land under `.audit/runs/ci-concordance/` and update `.audit/state/local-automation-ci-concordance-last-run.json`
- release ratification rehearsal bundles land under `.audit/runs/release-ratification/` and update `.audit/state/local-automation-release-ratification-last-run.json`

### Automation Maturation Map 05

- Goal: stabilize first publish intent and seeded promotion review once promotion evidence is consistently reproducible
- Expected review windows: `2`
- Advanced tasks:
  - `Seeded Promotion Review`
  - `First Publish Intent Closure`
  - `Release Handshake Surface`

Current live output surfaces for this map:

- seeded promotion review bundles land under `.audit/runs/seeded-promotion-review/` and update `.audit/state/local-automation-seeded-promotion-review-last-run.json`
- first publish intent bundles land under `.audit/runs/first-publish-intent/` and update `.audit/state/local-automation-first-publish-intent-last-run.json`
- release handshake bundles land under `.audit/runs/release-handshake/` and update `.audit/state/local-automation-release-handshake-last-run.json`

### Automation Maturation Map 06

- Goal: prepare the first bounded publish request and post-publish evidence loop once the handshake surface is stable
- Expected review windows: `2`
- Selected tasks:
  - `Publish Request Envelope`
  - `Post-Publish Evidence Loop`
  - `Seed Braid Escalation Lane`

Current live output surfaces for this map:

- publish request envelopes land under `.audit/runs/publish-request-envelopes/` and update `.audit/state/local-automation-publish-request-envelope-last-run.json`
- post-publish evidence loop bundles land under `.audit/runs/post-publish-evidence/` and update `.audit/state/local-automation-post-publish-evidence-last-run.json`
- seed braid escalation bundles land under `.audit/runs/seed-braid-escalations/` and update `.audit/state/local-automation-seed-braid-escalation-last-run.json`

### Automation Maturation Map 07

- Goal: stabilize live publication execution and the first external evidence loop once a bounded publish request is ratified
- Expected review windows: `2`
- Queued tasks:
  - `Published Runtime Receipt`
  - `Artifact Attestation Surface`
  - `Post-Publish Drift Watch`

## Status Interpretation

The task board must distinguish between:

- `waiting-for-cadence`
- `waiting-for-daily-review`
- `clear-to-continue`
- `hitl-required`
- `blocked`
- `scheduler-unregistered`
- `scheduler-ready`

These are operational postures, not release claims.

## Authority Boundary

This tasking surface does not give automation new authority.

It makes current authority legible.

Automation may continue mechanical work inside its declared cadence.

HITL remains mandatory for:

- modular-set promotion
- new deployable introduction
- authority widening
- publication promotion
- unresolved blocked states
