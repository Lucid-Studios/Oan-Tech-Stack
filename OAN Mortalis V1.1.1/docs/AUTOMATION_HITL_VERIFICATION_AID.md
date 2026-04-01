# AUTOMATION_HITL_VERIFICATION_AID

## Purpose

This note fixes the shared `HITL` verification aid carried by the current
automation bucket chain.

It exists so the automation line can stop cleanly at `HITL` without forcing the
operator to rediscover the review grammar each time.

## Admission Marker

The current build-facing marker is:

- `automation-hitl-verification-aid: admitted-operator-aid-bounded`

That means:

- the aid is admitted for the active automation line now
- the aid is an operator support surface, not a machine ratification surface
- the aid may be emitted across multiple automation artifacts without widening
  runtime or publication authority

## Emission Surfaces

The shared aid currently rides the same bucket chain used by the bounded
automation line.

It is emitted into:

- tasking status surfaces
- long-form task-map run surfaces
- workspace bucket status surfaces
- local notification bundles
- master-thread orchestration status surfaces
- master-thread instruction and intent envelopes
- source-bucket request and return review surfaces

## Review Sequence

The review sequence is fixed as:

- `received`
- `understood`
- `admissible`
- `actionable`
- `withheld_or_escalated`

Those are ordered operator checks, not machine self-approval stages.

## Outcomes

The allowed operator outcomes are:

- `admit`
- `clarify`
- `defer`
- `refuse`
- `escalate`

## Authority Boundary

Automation may prepare the `hitlVerificationAid` packet whenever a return,
barrier, or posture owes `HITL`.

Automation may not execute the confirmation.

Only the `Operator` may confirm direct `HITL` admission.
