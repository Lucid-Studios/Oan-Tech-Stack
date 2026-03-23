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

- reach access-topology ledgers land under `.audit/runs/reach-access-topology-ledger/` and update `.audit/state/local-automation-reach-access-topology-ledger-last-run.json`
- bonded operator locality readiness bundles land under `.audit/runs/bonded-operator-locality-readiness/` and update `.audit/state/local-automation-bonded-operator-locality-readiness-last-run.json`
- protected-state legibility bundles land under `.audit/runs/protected-state-legibility-surface/` and update `.audit/state/local-automation-protected-state-legibility-surface-last-run.json`

### Automation Maturation Map 18

- Goal: bind the singular nexus portal, duplex predicate envelopes, and the first `Operator.actual` work-session rehearsal without collapsing locality, office, or authority
- Expected review windows: `2`
- Selected tasks:
  - `Nexus Singular Portal Facade`
  - `Duplex Predicate Envelope`
  - `Operator.actual Work Session Rehearsal`

Current live output surfaces for this map:

- these surfaces are now emitted from the active cycle and can witness singular portal, duplex predicate, and bounded `Operator.actual` rehearsal without implying bonded operator actuality or deep cryptic descent

### Automation Maturation Map 19

- Goal: root each new worker in an identity-invariant thread base and bind triadic nexus governance at thread birth so agentic multiplicity remains lawful rather than ambient
- Expected review windows: `2`
- Selected tasks:
  - `Identity-Invariant Thread Root`
  - `Governed Thread Birth Receipt`
  - `Inter-Worker Braid Handoff Packet`

Current live output surfaces for this map:

- these surfaces are now bound into the automation cycle as preactivation receipts for worker individuality, triadic thread birth, and explicit braid handoff
- they do not imply live multi-worker expansion until `Map 19` becomes the active collapse and worker movement is actually exercised through those receipts

### Automation Maturation Map 20

- Goal: realize `AgentiCore.actual` as governed utility and let `reach` duplex-realize predicate-bearing participation across bonded localities without turning transport into sovereignty or remote control
- Expected review windows: `2`
- Selected tasks:
  - `AgentiCore.actual Utility Surface`
  - `Reach Duplex Realization Seam`
  - `Bonded Participation Locality Ledger`

Current live output surfaces for this map:

- `AgentiCore.actual` utility-surface bundles land under `.audit/runs/agenticore-actual-utility-surface/` and update `.audit/state/local-automation-agenticore-actual-utility-surface-last-run.json`
- reach duplex-realization seam bundles land under `.audit/runs/reach-duplex-realization-seam/` and update `.audit/state/local-automation-reach-duplex-realization-seam-last-run.json`
- bonded participation locality-ledger bundles land under `.audit/runs/bonded-participation-locality-ledger/` and update `.audit/state/local-automation-bonded-participation-locality-ledger-last-run.json`
- these surfaces witness governed utility, cross-locality realization, and provisional bonded locality without implying sovereign access grant or fully ratified bonded `Operator.actual`

### Automation Maturation Map 21

- Goal: open the first bounded Sanctuary-in-runtime workbench so local CME work can occur inside the deployable candidate while keeping amenable day-dream exploration distinct from self-rooted deep cryptic access
- Expected review windows: `2`
- Selected tasks:
  - `Sanctuary Runtime Workbench Surface`
  - `Amenable Day-Dream Tier Admissibility`
  - `Self-Rooted Cryptic Depth Gate`

Current live output surfaces for this map:

- Sanctuary runtime workbench-surface bundles land under `.audit/runs/sanctuary-runtime-workbench-surface/` and update `.audit/state/local-automation-sanctuary-runtime-workbench-surface-last-run.json`
- amenable day-dream tier admissibility bundles land under `.audit/runs/amenable-day-dream-tier-admissibility/` and update `.audit/state/local-automation-amenable-day-dream-tier-admissibility-last-run.json`
- self-rooted cryptic-depth gate bundles land under `.audit/runs/self-rooted-cryptic-depth-gate/` and update `.audit/state/local-automation-self-rooted-cryptic-depth-gate-last-run.json`
- these surfaces witness bounded in-runtime Sanctuary work, exploratory day-dream admissibility, and self-rooted depth withholding without implying bonded `Operator.actual` release or deep cryptic grant

### Automation Maturation Map 22

- Goal: turn the bounded Sanctuary-in-runtime workbench into a governed session surface where amenable day-dream motion can collapse into bounded work and self-rooted cryptic depth can return without residue or continuity inflation
- Expected review windows: `2`
- Selected tasks:
  - `Runtime Workbench Session Ledger`
  - `Day-Dream Collapse Receipt`
  - `Cryptic Depth Return Receipt`

Current live output surfaces for this map:

- runtime workbench session-ledger bundles land under `.audit/runs/runtime-workbench-session-ledger/` and update `.audit/state/local-automation-runtime-workbench-session-ledger-last-run.json`
- day-dream collapse receipts land under `.audit/runs/day-dream-collapse-receipt/` and update `.audit/state/local-automation-day-dream-collapse-receipt-last-run.json`
- cryptic depth-return receipts land under `.audit/runs/cryptic-depth-return-receipt/` and update `.audit/state/local-automation-cryptic-depth-return-receipt-last-run.json`
- these surfaces witness bounded in-runtime session formation, exploratory collapse, and self-rooted depth return without implying bonded co-work release or deep cryptic export

### Automation Maturation Map 23

- Goal: rehearse the first bonded co-work loop between `Sanctuary.actual` and `Operator.actual` inside the workbench era while keeping duplex participation, return, and dissolution lawful
- Expected review windows: `2`
- Selected tasks:
  - `Bonded Co-Work Session Rehearsal`
  - `Reach Return Dissolution Receipt`
  - `Locality Distinction Witness Ledger`

Current live output surfaces for this map:

- bonded co-work session rehearsal bundles land under `.audit/runs/bonded-cowork-session-rehearsal/` and update `.audit/state/local-automation-bonded-cowork-session-rehearsal-last-run.json`
- reach return-dissolution receipts land under `.audit/runs/reach-return-dissolution-receipt/` and update `.audit/state/local-automation-reach-return-dissolution-receipt-last-run.json`
- locality distinction witness ledgers land under `.audit/runs/locality-distinction-witness-ledger/` and update `.audit/state/local-automation-locality-distinction-witness-ledger-last-run.json`
- these surfaces witness bonded co-work rehearsal, explicit reach return and dissolution, and differentiated locality carriage without implying habitation ratification or raw protected interior exposure

### Automation Maturation Map 24

- Goal: prepare the local host for bounded Sanctuary habitation so recurring CME-side runtime work can begin without overstating bonded release, publication maturity, or MoS-bearing depth
- Expected review windows: `2`
- Selected tasks:
  - `Local Host Sanctuary Residency Envelope`
  - `Runtime Habitation Readiness Ledger`
  - `Bounded Inhabitation Launch Rehearsal`

Current live output surfaces for this map:

- local-host Sanctuary residency-envelope bundles land under `.audit/runs/local-host-sanctuary-residency-envelope/` and update `.audit/state/local-automation-local-host-sanctuary-residency-envelope-last-run.json`
- runtime habitation readiness-ledger bundles land under `.audit/runs/runtime-habitation-readiness-ledger/` and update `.audit/state/local-automation-runtime-habitation-readiness-ledger-last-run.json`
- bounded inhabitation launch-rehearsal bundles land under `.audit/runs/bounded-inhabitation-launch-rehearsal/` and update `.audit/state/local-automation-bounded-inhabitation-launch-rehearsal-last-run.json`
- these surfaces witness bounded local-host residency, habitation readiness, and first-launch rehearsal without implying bonded release, publication maturity, or MoS-bearing depth

### Automation Maturation Map 25

- Goal: spend one bounded automatic cycle rebuilding the post-habitation horizon lattice from live local-host residency truth so the next era is declared from evidence rather than drift
- Expected review windows: `2`
- Selected tasks:
  - `Post-Habitation Horizon Lattice`
  - `Bounded Horizon Research Brief`
  - `Next Era Batch Selector`

Current live output surfaces for this map:

- post-habitation horizon-lattice bundles land under `.audit/runs/post-habitation-horizon-lattice/` and update `.audit/state/local-automation-post-habitation-horizon-lattice-last-run.json`
- bounded horizon research-brief bundles land under `.audit/runs/bounded-horizon-research-brief/` and update `.audit/state/local-automation-bounded-horizon-research-brief-last-run.json`
- next-era batch-selector bundles land under `.audit/runs/next-era-batch-selector/` and update `.audit/state/local-automation-next-era-batch-selector-last-run.json`
- these surfaces rebuild the next lawful horizon from current habitation truth, harvest one bounded research pass, and select the next era without implying already-implemented inquiry or crucible machinery

### Automation Maturation Map 26

- Goal: formalize chamber-native inquiry inside bounded habitation so questioning, silence, and boundary memory become lawful session operators without premature GEL promotion
- Expected review windows: `2`
- Selected tasks:
  - `Inquiry Session Discipline Surface`
  - `Boundary Condition Ledger`
  - `Coherence Gain Witness Receipt`

Current live output surfaces for this map:

- inquiry session-discipline surfaces land under `.audit/runs/inquiry-session-discipline-surface/` and update `.audit/state/local-automation-inquiry-session-discipline-surface-last-run.json`
- boundary condition ledgers land under `.audit/runs/boundary-condition-ledger/` and update `.audit/state/local-automation-boundary-condition-ledger-last-run.json`
- coherence gain-witness receipts land under `.audit/runs/coherence-gain-witness-receipt/` and update `.audit/state/local-automation-coherence-gain-witness-receipt-last-run.json`
- these surfaces witness chamber-native inquiry discipline, retained boundary memory, and coherence-preserving questioning without prematurely promoting questioning patterns into standalone GEL types

### Automation Maturation Map 27

- Goal: rehearse bonded co-inquiry under shared uncertainty so Operator and CME can choose lawful ways of proceeding through the unknown without collapsing locality, identity, or office
- Expected review windows: `2`
- Selected tasks:
  - `Operator Inquiry Selection Envelope`
  - `Bonded Crucible Session Rehearsal`
  - `Shared Boundary Memory Ledger`

Current live output surfaces for this map:

- operator inquiry selection envelopes land under `.audit/runs/operator-inquiry-selection-envelope/` and update `.audit/state/local-automation-operator-inquiry-selection-envelope-last-run.json`
- bonded crucible session rehearsals land under `.audit/runs/bonded-crucible-session-rehearsal/` and update `.audit/state/local-automation-bonded-crucible-session-rehearsal-last-run.json`
- shared boundary-memory ledgers land under `.audit/runs/shared-boundary-memory-ledger/` and update `.audit/state/local-automation-shared-boundary-memory-ledger-last-run.json`
- these surfaces witness operator inquiry selection, shared-uncertainty crucible rehearsal, and carry-forward boundary memory without leaking protected interiority or collapsing locality

### Automation Maturation Map 28

- Goal: retain what the crucible actually teaches by making continuity under pressure, expressive deformation, and mutual intelligibility first-class bounded receipts before any inquiry pattern is reused
- Expected review windows: `2`
- Selected tasks:
  - `Continuity Under Pressure Ledger`
  - `Expressive Deformation Receipt`
  - `Mutual Intelligibility Witness`

Current live output surfaces for this map:

- continuity under-pressure evidence remains bounded to future bonded crucible, shared boundary-memory, and chamber-native inquiry surfaces until its dedicated ledger root is declared
- expressive deformation remains bounded to future crucible session and inquiry-discipline receipts until its dedicated deformation bundle exists
- mutual intelligibility remains bounded to locality distinction, bonded co-work, and future crucible witness surfaces until its dedicated witness root is declared

### Automation Maturation Map 29

- Goal: convert crucible evidence into bounded carry-forward inquiry memory so future approaches can be selected from what held and what broke without turning boundary memory into identity bleed
- Expected review windows: `2`
- Selected tasks:
  - `Inquiry Pattern Continuity Ledger`
  - `Questioning Boundary Pair Ledger`
  - `Carry-Forward Inquiry Selection Surface`

Current live output surfaces for this map:

- inquiry pattern continuity remains bounded to future pressure, deformation, and coherence witness surfaces until its dedicated carry-forward ledger root is declared
- questioning boundary pairs remain bounded to current boundary-memory and future crucible pressure receipts until their dedicated pair-ledger bundle exists
- carry-forward inquiry selection remains bounded to operator inquiry selection, locality distinction, and future pattern-memory surfaces until its dedicated selection surface is declared

### Automation Maturation Map 30

- Goal: open a guarded promotion lane for questioning operators as GEL candidates only when continuity and boundary evidence justify reuse, while keeping locality, authority, and promotion law intact
- Expected review windows: `2`
- Selected tasks:
  - `Questioning Operator Candidate Ledger`
  - `Questioning GEL Promotion Gate`
  - `Protected Questioning Pattern Surface`

Current live output surfaces for this map:

- questioning operator candidacy remains bounded to future inquiry-pattern continuity, boundary-pair, and crucible evidence surfaces until its dedicated candidate ledger root is declared
- questioning GEL promotion remains bounded to current promotion law and future inquiry-candidate evidence until its dedicated gate surface is declared
- protected questioning pattern legibility remains bounded to future candidate and locality-safe review surfaces until its dedicated protected pattern bundle exists

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
