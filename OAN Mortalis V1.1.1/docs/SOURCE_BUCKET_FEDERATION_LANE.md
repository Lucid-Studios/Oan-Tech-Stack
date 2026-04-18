# SOURCE_BUCKET_FEDERATION_LANE

## Purpose

This note defines the line-local source-bucket federation lane for
`OAN Mortalis V1.1.1`.

It exists so the active build can:

- publish bounded build needs
- target specialized external work buckets by label
- ingest only lawful return receipts
- and keep build mutation authority inside the active line

This lane is a control plane.

It is not a cross-repo mutation license.

## Admission Marker

The current build-facing marker is:

- `source-bucket-federation-control-plane: admitted-local-bounded`
- `source-bucket-federation-cycle: admitted-local-mechanical`

That means:

- the control plane is admitted for local build use now
- requests may be published from the active line
- integration and execution still depend on lawful return receipts
- final executable widening remains gated by the current build and `HITL` law

The mechanical federation cycle now runs inside the active build lane to:

- read the versioned touchpoint matrix
- publish bounded source-bucket work requests into the line-local outbox
- refresh the request index and federation status surfaces
- and stop short of any direct cross-repo mutation or runtime widening

## Line-Local Scope

This control plane lives inside:

- `OAN Mortalis V1.1.1/Automation/`
- `OAN Mortalis V1.1.1/.audit/`

It reads the existing repo-root automation telemetry as upstream standing, but
it keeps its own request and return loop line-local to the active build.

The upstream read surfaces are:

- repo-root master-thread orchestration status
- repo-root local automation tasking status
- repo-root `V1.1.1` enrichment pathway state
- repo-root companion-tool telemetry state

That split matters.

The repo-root surfaces remain the current upstream telemetry truth.
The active line-local federation lane remains the place where
`V1.1.1` publishes bounded source-bucket needs.

## Source Buckets

The first declared source buckets are labels only:

- `IUTT SLI & Lisp`
- `Latex Styles`
- `Trivium Forum`
- `Holographic Data Tool`

These labels are intentionally logical rather than filesystem-bearing.

The active line may name them, route to them, and condition on their receipts
without hard-coding external local paths into tracked build history.

Current intended specialization is:

- `IUTT SLI & Lisp`
  research and cognition-law intake
- `Latex Styles`
  publication and pedagogy formatting law
- `Trivium Forum`
  multi-party conference and operator workflow law
- `Holographic Data Tool`
  braid, artifact, and engram support law

The first active request publication is intentionally narrower than the full
declared bucket list.

Current matrix-driven publication scope is:

- `IUTT SLI & Lisp`
  active research-handoff cluster present now
- `Trivium Forum`
  active workflow-governance handoff cluster present now

Current declared-but-idle listening buckets are:

- `Latex Styles`
  declared and bounded, but no active build-handoff request currently emitted
- `Holographic Data Tool`
  declared and bounded, but no active build-handoff request currently emitted

## Dispatch Law

Build acts here as lawful requester and admitting surface.

That means Build may:

- detect bounded build need from current line truth
- compile that need into a request bundle
- publish the request into its own outbox
- wait for source-bucket return receipts
- integrate only what survives that loop

Build must not:

- mutate external source buckets directly
- infer integration authority from raw external repo drift
- widen runtime law merely because a source bucket produced output

## Request Outbox

The line-local request outbox is:

- `OAN Mortalis V1.1.1/.audit/runs/source-bucket-work-requests/`

Each bundle should preserve:

- `request.json`
- `request.md`

The machine request contract lives at:

- `OAN Mortalis V1.1.1/Automation/source-bucket-work-request-contract.json`

Each request names at minimum:

- `requestId`
- `targetBucketLabel`
- `buildSurface`
- `subject`
- `predicate`
- `actions`
- `neededReturnClass`
- `evidenceLinks`
- `admissibilityClass`
- `requiredReceipts`
- `hitlState`
- `withholdRules`

This keeps the request surface in a bounded subject-predicate-action form.

## Listener And Return Law

Each source bucket should answer through its own lawful local lane and return
receipts rather than through ambient conversation.

The line-local return contract lives at:

- `OAN Mortalis V1.1.1/Automation/source-bucket-return-contract.json`

The build-local return inbox and status surfaces now live at:

- `OAN Mortalis V1.1.1/.audit/runs/source-bucket-returns/`
- `OAN Mortalis V1.1.1/.audit/state/source-bucket-return-index.json`
- `OAN Mortalis V1.1.1/.audit/state/source-bucket-return-integration-status.json`

The expected first listener states are:

- `received`
- `understood`
- `admissible`
- `actionable`
- `withheld_or_escalated`

Those states align to the current shared `HITL` verification grammar.

Build ingests only return receipts and their bounded artifacts.
It does not ingest unreceipted external drift as mutation authority.

The current build-side return intake posture is:

- `source-bucket-return-intake: admitted-local-mechanical`

## Three Layers

The active federation lane is split into three layers:

1. `federation`
   publish requests, ingest receipts, classify bucket posture
2. `integration`
   decide whether the return is `frame-now`, `spec-now`, `implement-now`, or
   `hold`
3. `execution`
   mutate the active line only after return receipts are admitted

No layer may bypass the layer before it.

## First Executable Target

The first executable target supported by this lane is:

- `Oan.Runtime.Headless`

The current success posture is still bounded:

- boot locally
- reach the admitted actionable work surface
- preserve the `seed-llm` pause seam
- avoid wider runtime or `CME` claims

This lane therefore supports executable completion.
It does not silently authorize later embodiment claims.

## HITL Boundary

The shared `HITL` verification aid remains fixed in:

- `AUTOMATION_HITL_VERIFICATION_AID.md`

That means source-bucket returns may prepare:

- `received`
- `understood`
- `admissible`
- `actionable`
- `withheld_or_escalated`

Automation may prepare that review surface.
Only the `Operator` may confirm direct `HITL` admission.

## Non-Goals

This lane does not:

- authorize cross-repo mutation by Build
- treat publication pedagogy as executable truth
- widen live `ListeningFrame` or tool-use authority
- treat optional telemetry as constitutive runtime proof
- place live `CME` embodiment from source-bucket output alone
