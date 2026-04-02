# SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW

## Purpose

This note defines the lawful difference between:

- the seed lane's own disposition
- the active build lane's bounded admission decision

Those are related, but they are not the same thing.

## Core Rule

The seed lane may remain `Deferred` while the active build still treats seeded
governance as `admitted-local-bounded`.

That is lawful only when all of the following are true:

- the seed runtime is `ready`
- the seed lane is not `Rejected`
- the unresolved seam is the local preflight profile being explicitly routed to
  research
- the build uses seeded governance only as bounded interpretation and gate
  support, not as promotion or runtime-widening authority

## Why This Exists

The current line already proved two separate truths:

- the local seed runtime is healthy and callable
- the local LLM preflight suite remains a research-routed refinement seam

Those truths do not require the build to stay blocked.

They require the build to remain honest about scope.

So the governing distinction is:

- `disposition`
  what the seed lane says about its own full preflight completion
- `buildAdmissionState`
  whether the active `V1.1.1` build may proceed under bounded seeded
  interpretation now

## Current Admitted Case

The current admitted bounded case is:

- `readyState = ready`
- `disposition = Deferred`
- `dispositionReason = seed-preflight-routed-to-research`
- `buildAdmissionState = admitted-local-bounded`

That means:

- build may continue through seeded-governance-dependent gates
- build may not claim that seed preflight is complete
- build may not widen runtime, CME, or release authority from this alone

## Non-Goals

This law does not:

- promote the seed lane to release authority
- treat research-routed preflight as passed
- authorize seed-LLM execution
- widen `SoulFrame`, `AgentiCore`, `ListeningFrame`, or tool-use authority

## Build Effect

When `buildAdmissionState = admitted-local-bounded`, downstream build gates may:

- stop asking to "bring seeded governance to ready state"
- proceed to the next bounded readiness gate
- preserve the seed lane's own `Deferred` truth in receipts and notices

When `buildAdmissionState` is anything else, downstream build gates must still:

- clarify
- withhold
- or defer

instead of silently proceeding.
