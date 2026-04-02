# V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY

## Purpose

This note defines the first bounded automation pathway for `OAN Mortalis
V1.1.1` after the contract-first build-hold unlock.

It is the line-local automation answer to one practical question:

- how do we move from the admitted candidate state into the first truthful
  `V1.1.1` enrichment lane, carry full-body work toward a production-pre-release
  form, and then stop cleanly for seed-LLM wrinkle testing

This pathway is a build and evidence lane.
It is not a runtime widening lane.

## Current Admission Marker

The current build-facing marker is:

- `v111-enrichment-automation: admitted-local-bounded`
- `companion-tool-telemetry: admitted-optional-bounded`
- `source-bucket-federation-control-plane: admitted-local-bounded`
- `source-bucket-federation-cycle: admitted-local-mechanical`
- `source-bucket-return-intake: admitted-local-mechanical`
- `automation-hitl-verification-aid: admitted-operator-aid-bounded`

That means:

- the lane is admitted for local build work now
- the lane is still bounded by current runtime law
- the lane must pause before seed-LLM wrinkle testing and before any later
  live widening claims

## Entry Conditions

The enrichment pathway opens only after the current automation spine has
already produced all of the following:

- runtime deployability envelope at `deployable-candidate-ready`
- Sanctuary runtime readiness at `bounded-working-state-ready`
- runtime work-surface admissibility at `provisional-runtime-work` or stronger
  bounded internal allowance
- runtime workbench session ledger at
  `runtime-workbench-session-ledger-ready`
- seeded governance at `buildAdmissionState = admitted-local-bounded`

That admission law is fixed in:

- `SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW.md`
- `RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md`
- `DISCERNMENT_AND_ADMISSIBILITY_LAW.md`

This means the seed lane may remain `Deferred` for its own advisory posture
while the active build still treats it as locally admitted, but only when the
bounded build-admission law says that is lawful.

It also means the first admitted runtime surface before any wider EC claim is
the bounded governance-and-analysis workbench, not full habitation.

If seeded governance reaches `ready` while downstream pathway surfaces still
leave build admission unresolved, that is a `clarify` seam.

The lane must refresh gate truth before continuing.

Those are the minimum proof surfaces required before the line may honestly say
that end-to-end enrichment work is open.

## Path Shape

The pathway is intentionally simple:

1. admit the candidate and contract-first unlock evidence
2. open bounded full-body work for `V1.1.1`
3. shape that work toward a production-pre-release form
4. pause for seed-LLM wrinkle testing
5. review the result before any broader runtime or public widening

The active phase order is therefore:

- `candidate-evidence`
- `bounded-full-body-work`
- `production-pre-release-form`
- `seed-llm-pause-seam`

## Pause Seam

The pause seam is not optional.

The pathway must carry an explicit seed-LLM wrinkle-test hold:

- build and automation may prepare the line for that test
- build and automation may not silently skip over it
- the seed-LLM test is the first intentional pause after the production-pre-release
  form is shaped

The active pause statement is:

- `seed-llm pause seam: required`

## Orchestration Boundary

The enrichment pathway is local-first.

That means:

- local enrichment work may continue while master-thread orchestration is still
  awaiting a publishable master thread
- external release or wider bucket motion remains held by the current
  master-thread publication law
- the automation lane must say that split plainly rather than blending local
  progress with external release authority

## Optional `.hopng` Support

The current `.hopng` and `Holographic Data Tool` relation remains:

- `.hopng: optional-bounded`

That means:

- `.hopng` may support inspection, comparison, and bounded witness work
- `.hopng` does not become constitutive runtime authority in this lane
- the enrichment pathway must never claim success only because a `.hopng`
  surface exists

## Companion Tool Telemetry

Companion-tool telemetry may now flow into the local build logs through the
bounded companion telemetry lane.

The current companion surfaces are:

- `Holographic Data Tool`
- `Trivium Forum`

The governing relation is:

- `Holographic Data Tool`
  may contribute bounded `.audit` telemetry now when it emits it
- `Trivium Forum`
  may contribute bounded multi-party conference telemetry through its own
  truthful `.audit` lane

## HITL Verification Aid

The enrichment pathway now carries one shared `hitlVerificationAid` operator-aid
packet across the current automation chain as fixed in
`AUTOMATION_HITL_VERIFICATION_AID.md`.

That aid is emitted into:

- tasking status surfaces
- workspace bucket status surfaces
- local notification bundles
- master-thread orchestration status and instruction surfaces

The review sequence is fixed as:

- `received`
- `understood`
- `admissible`
- `actionable`
- `withheld_or_escalated`

Automation may prepare that aid whenever `HITL` is needed.
Only the `Operator` may execute the confirmation.

Missing companion telemetry must still be logged as missing rather than being
smoothed over by assumption.

## Root Dispatch

The active root automation prompt for this lane now lives in:

- `OAN_BUILD_DISPATCH_ROOT_PROMPT.md`
- `V1_1_1_WORKFLOW_MILESTONE_MAP.md`

That dispatcher must treat seeded-governance admission mismatch as a
`clarify` seam before publishing downstream requests.

The companion telemetry note is:

- `COMPANION_TOOL_TELEMETRY_LANE.md`

## Source-Bucket Federation

The active line now also carries a line-local source-bucket federation control
plane.

That plane lets `V1.1.1`:

- publish bounded build needs into its own outbox
- target source buckets by logical label
- wait for lawful return receipts
- integrate only what survives the request/return loop

The first declared source buckets are:

- `IUTT SLI & Lisp`
- `Latex Styles`
- `Trivium Forum`
- `Holographic Data Tool`

The first active matrix-driven request publication is narrower:

- `IUTT SLI & Lisp`
  active request publication admitted now
- `Trivium Forum`
  active request publication admitted now

The current declared-but-idle listening buckets remain:

- `Latex Styles`
- `Holographic Data Tool`

The governing note is:

- `SOURCE_BUCKET_FEDERATION_LANE.md`

## Tooling Surface

The governing automation tools for this pathway are:

- `tools/Invoke-Local-Automation-Cycle.ps1`
- `tools/Resume-Local-AutomationCycleTask.ps1`
- `tools/Invoke-CompanionToolTelemetry.ps1`
- `tools/Write-CompanionToolTelemetry.ps1`
- `tools/New-SourceBucket-WorkRequest.ps1`
- `tools/Write-SourceBucket-FederationStatus.ps1`
- `tools/Invoke-SourceBucket-FederationCycle.ps1`
- `tools/Write-SourceBucket-ReturnIntegrationStatus.ps1`
- `tools/Invoke-SourceBucket-ReturnCycle.ps1`

The temporal close-law target for this tooling surface is:

- `LOCAL_AUTOMATION_END_STATE_TRANSITION_LAW.md`

So the current short scheduler loop should be treated as temporary transport.

The target form is:

- single-flight main worker
- hourly watchdog reflection
- daily `HITL` digest governance
- `tools/Write-V111-EnrichmentPathway.ps1`

The unattended cycle now runs every `5` minutes while the release-candidate
and daily `HITL` checkpoints keep their broader cadence. When the posture
becomes `hitl-required` or `blocked`, the scheduled task must pause and await
explicit operator resumption.
- `tools/Invoke-V111-EnrichmentPathway.ps1`

The emitted audit surface is:

- `.audit/state/local-automation-companion-tool-telemetry-last-run.json`
- `.audit/state/source-bucket-request-index.json`
- `.audit/state/source-bucket-return-index.json`
- `.audit/state/source-bucket-return-integration-status.json`
- `.audit/state/source-bucket-federation-status.json`
- `.audit/state/local-automation-v111-enrichment-pathway-last-run.json`

## Non-Goals

This pathway does not do any of the following by itself:

- live `OE -> SoulFrame` loading
- live `cOE -> AgentiCore` loading
- actualized `CME` attachment
- live `ListeningFrame` behavior
- tool-use widening
- automatic seed-LLM execution
- `.hopng` authority elevation

## Working Summary

The first `V1.1.1` enrichment automation lane is now:

- locally admitted
- contract-first
- full-body-work capable
- production-pre-release forming
- explicitly paused before seed-LLM wrinkle testing
