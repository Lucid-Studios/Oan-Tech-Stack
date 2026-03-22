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

The live active map, next eligible map, and queued batch are reported in:

- `.audit/state/local-automation-tasking-status.json`
- `.audit/state/local-automation-tasking-status.md`

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
- the carried-forward CME formation and office ledger writes `.audit/state/local-automation-cme-formation-office-ledger-last-run.json` and its bundle root under `.audit/runs/cme-formation-office-ledger/`

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
- Advanced tasks:
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
- Advanced tasks:
  - `Published Runtime Receipt`
  - `Artifact Attestation Surface`
  - `Post-Publish Drift Watch`

Current live output surfaces for this map:

- published runtime receipts land under `.audit/runs/published-runtime-receipts/` and update `.audit/state/local-automation-published-runtime-receipt-last-run.json`
- artifact attestations land under `.audit/runs/artifact-attestations/` and update `.audit/state/local-automation-artifact-attestation-last-run.json`
- post-publish drift watch bundles land under `.audit/runs/post-publish-drift-watch/` and update `.audit/state/local-automation-post-publish-drift-watch-last-run.json`

### Automation Maturation Map 08

- Goal: consolidate the first real publication loop into a stable operational governance surface once live publication is observed
- Expected review windows: `2`
- Advanced tasks:
  - `Operational Publication Ledger`
  - `External Consumer Concordance`
  - `Post-Publish Governance Loop`

Current live output surfaces for this map:

- operational publication ledger bundles land under `.audit/runs/operational-publication-ledger/` and update `.audit/state/local-automation-operational-publication-ledger-last-run.json`
- external consumer concordance bundles land under `.audit/runs/external-consumer-concordance/` and update `.audit/state/local-automation-external-consumer-concordance-last-run.json`
- post-publish governance loop bundles land under `.audit/runs/post-publish-governance-loop/` and update `.audit/state/local-automation-post-publish-governance-loop-last-run.json`

### Automation Maturation Map 09

- Goal: operationalize the first multi-interval publication cycle once the initial publication loop is no longer singular
- Expected review windows: `2`
- Advanced tasks:
  - `Publication Cadence Ledger`
  - `Downstream Runtime Observation`
  - `Multi-Interval Governance Braid`

Current live output surfaces for this map:

- publication cadence ledger bundles land under `.audit/runs/publication-cadence-ledger/` and update `.audit/state/local-automation-publication-cadence-ledger-last-run.json`
- downstream runtime observation bundles land under `.audit/runs/downstream-runtime-observation/` and update `.audit/state/local-automation-downstream-runtime-observation-last-run.json`
- multi-interval governance braid bundles land under `.audit/runs/multi-interval-governance-braid/` and update `.audit/state/local-automation-multi-interval-governance-braid-last-run.json`

### Automation Maturation Map 10

- Goal: prove unattended scheduler execution across real intervals and watch dormant surfaces for contradiction without confusing honesty for failure
- Expected review windows: `2`
- Selected tasks:
  - `Scheduler Execution Receipt`
  - `Unattended Interval Concordance`
  - `Stale Surface Contradiction Watch`

Current live output surfaces for this map:

- scheduler execution receipt bundles land under `.audit/runs/scheduler-execution-receipts/` and update `.audit/state/local-automation-scheduler-execution-receipt-last-run.json`
- unattended interval concordance bundles land under `.audit/runs/unattended-interval-concordance/` and update `.audit/state/local-automation-unattended-interval-concordance-last-run.json`
- stale-surface contradiction watch bundles land under `.audit/runs/stale-surface-contradiction-watch/` and update `.audit/state/local-automation-stale-surface-contradiction-watch-last-run.json`

### Automation Maturation Map 11

- Goal: carry unattended waiting truth across real cadence windows, collapse scheduler proof when it arrives, and prove stable silence is integrity rather than neglect
- Expected review windows: `2`
- Selected tasks:
  - `Unattended Proof Collapse`
  - `Dormant Window Ledger`
  - `Silent Cadence Integrity`

Current live output surfaces for this map:

- unattended proof collapse bundles land under `.audit/runs/unattended-proof-collapse/` and update `.audit/state/local-automation-unattended-proof-collapse-last-run.json`
- dormant window ledger bundles land under `.audit/runs/dormant-window-ledger/` and update `.audit/state/local-automation-dormant-window-ledger-last-run.json`
- silent cadence integrity bundles land under `.audit/runs/silent-cadence-integrity/` and update `.audit/state/local-automation-silent-cadence-integrity-last-run.json`

### Automation Maturation Map 12

- Goal: let the active long-form run witness real unattended evidence, advance its exploratory structures lawfully, and collapse at the window edge without pretending scheduler proof that has not arrived
- Expected review windows: `2`
- Selected tasks:
  - `Long-Form Phase Witness`
  - `Long-Form Window Boundary`
  - `Autonomous Long-Form Collapse`

Current live output surfaces for this map:

- long-form phase witness bundles land under `.audit/runs/long-form-phase-witness/` and update `.audit/state/local-automation-long-form-phase-witness-last-run.json`
- long-form window boundary bundles land under `.audit/runs/long-form-window-boundary/` and update `.audit/state/local-automation-long-form-window-boundary-last-run.json`
- autonomous long-form collapse bundles land under `.audit/runs/autonomous-long-form-collapse/` and update `.audit/state/local-automation-autonomous-long-form-collapse-last-run.json`

### Automation Maturation Map 13

- Goal: harvest the first scheduler-proven interval into a governed surface, distinguish scheduled proof from manual continuity, and promote into the next lawful batch without improvisation
- Expected review windows: `2`
- Selected tasks:
  - `Scheduler Proof Harvest`
  - `Interval Origin Clarification`
  - `Queued Task Map Promotion`

Current live output surfaces for this map:

- scheduler proof harvest bundles land under `.audit/runs/scheduler-proof-harvest/` and update `.audit/state/local-automation-scheduler-proof-harvest-last-run.json`
- interval origin clarification bundles land under `.audit/runs/interval-origin-clarification/` and update `.audit/state/local-automation-interval-origin-clarification-last-run.json`
- queued task map promotion bundles land under `.audit/runs/queued-task-map-promotion/` and update `.audit/state/local-automation-queued-task-map-promotion-last-run.json`

### Automation Maturation Map 14

- Goal: reconcile the first scheduler-proven cadence into a stable observed interval chain without confusing observation for publication or maturity
- Expected review windows: `2`
- Selected tasks:
  - `Observed Cadence Ledger`
  - `Manual Overhang Reconciliation`
  - `Scheduler Interval Governance Braid`

Current live output surfaces for this map:

- observed cadence truth remains bounded to the scheduler-proof, interval-origin, and unattended-proof surfaces until its dedicated ledger root is declared
- manual overhang reconciliation remains bounded to the interval-origin and unattended-proof surfaces until its dedicated receipt exists
- scheduler interval governance braid remains expressed through the long-form tasking status until its dedicated bundle root is declared

### Automation Maturation Map 15

- Goal: let seeded governance read quiet intervals for lawful surplus work selection while keeping pull-forward bounded to the next declared map only
- Expected review windows: `2`
- Selected tasks:
  - `Seeded Interval Reflection`
  - `Pause Potential Surface`
  - `Bounded Pull-Forward Selector`

Current live output surfaces for this map:

- seeded interval reflection remains bounded to the seeded governance and scheduler-proof surfaces until its dedicated advisory bundle is declared
- pause potential remains expressed through long-form phase, dormant-window, and cadence integrity surfaces until its dedicated bundle is declared
- bounded pull-forward selection is reflected through `.audit/state/local-automation-active-task-map-selection.json` and `.audit/state/local-automation-tasking-status.json`

### Automation Maturation Map 16

- Goal: prove the current headless deployable can host a bounded Sanctuary working state and expose which runtime work surfaces are presently admissible without narrating bonded participation or deep cryptic descent too early
- Expected review windows: `2`
- Selected tasks:
  - `Runtime Deployability Envelope`
  - `Sanctuary Runtime Readiness Receipt`
  - `Runtime Work Surface Admissibility`

Current live output surfaces for this map:

- runtime deployability envelopes land under `.audit/runs/runtime-deployability-envelope/` and update `.audit/state/local-automation-runtime-deployability-envelope-last-run.json`
- Sanctuary runtime readiness receipts land under `.audit/runs/sanctuary-runtime-readiness/` and update `.audit/state/local-automation-sanctuary-runtime-readiness-last-run.json`
- runtime work-surface admissibility bundles land under `.audit/runs/runtime-work-surface-admissibility/` and update `.audit/state/local-automation-runtime-work-surface-admissibility-last-run.json`

### Automation Maturation Map 17

- Goal: prepare reach-aware boundary legibility and bonded operator locality readiness once bounded Sanctuary runtime work is lawfully present
- Expected review windows: `2`
- Selected tasks:
  - `Reach Access Topology Ledger`
  - `Bonded Operator Locality Readiness`
  - `Protected State Legibility Surface`

Current live output surfaces for this map:

- these surfaces are declared runway only until the next promotion edge lawfully activates them

### Automation Maturation Map 18

- Goal: bind the singular nexus portal, duplex predicate envelopes, and the first `Operator.actual` work-session rehearsal without collapsing locality, office, or authority
- Expected review windows: `2`
- Selected tasks:
  - `Nexus Singular Portal Facade`
  - `Duplex Predicate Envelope`
  - `Operator.actual Work Session Rehearsal`

Current live output surfaces for this map:

- these surfaces are declared runway only until the prior runtime and reach maps collapse lawfully into them

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
