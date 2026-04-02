# MASTER_THREAD_BUCKET_ORCHESTRATION_LAW

## Purpose

This document defines the repo-local orchestration layer for the active `OAN Mortalis V1.1.1` workspace.

It exists so the build can move from:

- bucket awareness
- cycle awareness
- and task-map awareness

into:

- commit-grounded instruction issuance
- delayed bucket-scoped automation intent
- and lawful wait-to-movement release

## Master Thread Source

The local repo build is the master thread source.

That means orchestration must ground itself in:

- current branch and commit on `codex/git-workflow`
- current repo cleanliness
- current automation posture
- current task-board posture
- current workspace-bucket posture

The orchestration layer must not issue work from conversational drift alone.

It must compile work from repo-truth.

## What Orchestration Adds

Awareness tells the system what is true now.

Orchestration tells the system:

- what to do next
- where to do it
- when it may move
- and what must still wait

This layer is therefore a governed instruction conveyor.

It is not a sovereign planner.

## Root Dispatch

The active root dispatch surface for this layer is `OAN Build Dispatch`.

Its governing prompt is carried at:

- `OAN_BUILD_DISPATCH_ROOT_PROMPT.md`

That prompt exists to keep the root layer honest about four things:

- it requests and admits work rather than doing every kind of work itself
- it reconciles build truth before it publishes downstream need
- it accepts only receipted bucket returns as build authority
- it preserves the bounded `seed-llm` and actionable-surface hold lines

The root dispatch prompt does not supersede repo-local executable truth.

It compiles the current truth into a stable requester-and-admitter posture.

## Subject-Predicate-Action Sets

Master-thread instructions are expressed as bounded subject-predicate-action sets.

At minimum, each instruction names:

- `subject`
  the build surface, bucket, or construct under action
- `predicate`
  the present condition, function, or relation that makes the action meaningful
- `actions`
  the bounded transforms, verifications, or receipts that should occur

These sets keep orchestration structured and traceable.

They must not collapse into free-form task text without losing their governing shape.

## Bucket Target Rule

Every instruction must target one or more declared workspace buckets.

Buckets remain hard-separated construct surfaces.

Orchestration may work across buckets only when:

- the repo truth supports the instruction
- the targeted buckets are explicitly named
- the instruction remains bounded in scope

## Temporal Zones

The orchestration layer distinguishes at least three temporal zones:

1. present preparation
2. future-directed task construction
3. transition into active movement

The system may author a future-bearing instruction during a turn without treating it as already moving.

## Commit-Grounded Delay Rule

Run intent may be authored during a turn.

Execution intent must remain delayed until at least one minute after the governing commit is published to `codex/git-workflow`.

This delay exists to preserve a lawful separation between:

- proposal
- publication
- and execution

The orchestration layer must not think and act in the same breath.

## Weighted Wait And Polling

An instruction may carry weighted wait posture.

That means it may state:

- minimum delay after publish
- polling interval
- polling window
- commit-dependency posture

Weighted wait is not failure.

Polling is not movement.

Commit observation is not movement admissibility.

## Wait-To-Movement Rule

The orchestration layer may release an instruction from wait toward movement only when:

- the commit is observed on the governing branch
- the required delay has elapsed
- repo posture remains lawful
- targeted bucket posture remains admissible

Only then may the movement-phase layer traverse the work.

## Clarify Seam Rule

The orchestration layer must prefer `clarify` before forced continuation when
its own governing state surfaces materially disagree.

The current live example is:

- seeded governance may report `ready`
- while a downstream build surface still asks to `bring-seeded-governance-to-ready-state`

That is not a valid continue signal.

It is a gate-truth mismatch.

When that seam appears, the root dispatcher must:

- classify the next action as `clarify`
- refresh gate truth first
- only then publish new downstream requests or release movement intent

## Lifecycle

The lawful instruction lifecycle is:

- `prepared`
- `weighted-wait`
- `polling`
- `commit-observed`
- `released`
- `movement`
- `settling`
- `completed`
- `blocked`
- `failed`
- `superseded`

The layer must tell the truth about waiting, release, and blockage rather than flattening them into generic queued work.

## Codex Automation Boundary

The current Codex automation surface does not yet provide truthful native one-shot scheduling.

Because of that, the repo may emit a structured Codex automation intent envelope.

It must not falsely claim that the app has already materialized a native run-once automation when only the envelope exists.

In the current state:

- the repo may prepare one-shot automation intent
- the repo may enumerate and structure that intent
- the repo must still name that seam as an intent envelope until native one-shot automation exists

## Receipts

Each orchestration instruction should preserve at least:

- source branch
- source commit
- source thread label
- target buckets
- subject-predicate-action sets
- earliest eligible run time
- wait and polling posture
- effective lifecycle state
- completion or failure outcome

## Federation Signaling Participation

This repo-local orchestration layer participates in a broader Lucid automation
federation.

That broader federation is documented in the `Documentation Repo` as
cross-lane architecture and doctrine, while this note remains the local build
law for the active repo.

For local orchestration, that means the layer should be able to emit not only
repo-local status but also bounded signaling objects such as:

- receipts
- dependency notices
- readiness notices
- pause notices
- forward conditioning notices

The local action grammar should prefer:

- `admit`
- `continue`
- `clarify`
- `suspend`
- `escalate`
- `return`

so waiting, blockage, continuation, and barrier-crossing remain distinct.

Low-level expected action states do not require repeated `HITL` merely because
movement continues.

Contract barriers still do.

## Non-Goals

This layer does not:

- replace the governed build cycle
- replace the workspace bucket system
- replace office or admissibility law
- authorize work merely because a commit exists
- pretend native Codex run-once automations already exist when they do not
