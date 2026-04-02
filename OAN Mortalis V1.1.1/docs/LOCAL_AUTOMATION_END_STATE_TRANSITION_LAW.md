# LOCAL_AUTOMATION_END_STATE_TRANSITION_LAW

## Purpose

This document defines the temporal close law for the governed local automation
lane in `OAN Mortalis V1.1.1`.

Its job is to answer one question plainly:

- when a run ends, what may lawfully happen next

This is not the same question as cadence.

Cadence describes when a wake may occur.
Close law describes whether a next wake is admissible at all.

The active redesign treats close law as primary and scheduler timing as
secondary.

## Constitutional Correction

The old scheduler-first model says:

- time passes
- therefore the worker wakes

The corrected model says:

- the worker closes in a known end state
- therefore the next wake may or may not be armed

That means the worker, not the wall clock, becomes the authority over lawful
continuation.

The scheduler remains important, but only as a transport surface for the next
admitted wake.

## Core Principles

### Closure Before Forward Scheduling

Nothing about next-run truth may be written before end-state truth is known.

The lane must first decide what happened.
Only then may it decide what comes next.

So:

- finish state first
- next wake second
- receipt of next-run truth last

### Exactly One Future Wake

The main worker is a single-flight lane.

At most one future main-worker wake may be armed at a time.

That future wake must be the consequence of the most recent closed state, not
an inherited repeating timer.

### Distinct Offices

The automation braid is split into three offices:

- `main worker`
  Performs one governed build pass and closes in one terminal state.
- `hourly watchdog`
  Reflects, detects stale or crashed state, and decides whether recovery,
  rearm, or escalation is lawful.
- `daily HITL digest`
  Reports the last twenty-four hours of lane truth to the operator.

These offices must not be blurred into one another.

## Main Worker Terminal States

Every main-worker run must close in exactly one terminal state:

- `continue`
- `pause-hitl`
- `done`
- `fault-recoverable`

No other terminal state may arm the next wake implicitly.

### Continue

Meaning:

- the run closed lawfully
- the objective is still unfinished
- no explicit `HITL` stop is owed yet

Consequence:

- arm exactly one next main-worker wake
- write the next wake only after the close receipt is fixed

### Pause-HITL

Meaning:

- the run reached a review boundary that requires explicit operator admission

Consequence:

- do not arm the next main-worker wake
- write a `pause_notice`
- leave recovery and eventual resume to operator-cleared reconstitution

### Done

Meaning:

- the current objective is complete within its admitted scope

Consequence:

- do not arm the next main-worker wake
- write a completed receipt
- leave the lane retired until a new objective is admitted

### Fault-Recoverable

Meaning:

- the run did not close healthy enough to rearm itself
- but the failure is still considered recoverable by governed inspection

Consequence:

- do not arm the next main-worker wake directly
- emit fault evidence
- hand recovery disposition to the hourly watchdog

## Rearm Law

Rearm is part of close ceremony.

It is not a background assumption.

That means:

- if terminal state is `continue`, rearm exactly one next wake
- if terminal state is `pause-hitl`, do not rearm
- if terminal state is `done`, do not rearm
- if terminal state is `fault-recoverable`, watchdog owns recovery disposition

The main worker must never publish future wake truth before its present receipt
is closed.

## Watchdog Law

The hourly watchdog is the true heartbeat of the lane.

Its purpose is not to do the main build work.
Its purpose is to:

- detect stale state
- detect crash residue
- detect missing lawful rearm
- confirm that a paused lane is still paused lawfully
- confirm that a completed lane is still complete lawfully
- escalate drift when continuity no longer holds

The watchdog runs hourly whether or not the main worker is paused, complete, or
idle.

The watchdog may:

- rearm a missing `continue` wake when recovery law says it should exist
- leave a paused or done lane untouched
- escalate when state is contradictory or unrecoverable

The watchdog may not:

- fabricate a continue posture
- override an explicit `pause-hitl`
- convert `done` into renewed work without new admission

## Daily HITL Digest Law

The daily digest remains the human-facing governance surface.

Its purpose is to report:

- pauses
- recoveries
- continuations
- completions
- unresolved drift

It does not own main-worker rearm and it does not replace the hourly watchdog.

## Resume Law

Resume is not mere re-enable.

Resume means:

- read present lane truth
- confirm the pause or fault surface has been lawfully cleared
- decide whether a next wake is admissible
- arm exactly one next wake if and only if that decision is affirmative

So resume is a reconstitution act, not a timer toggle.

## Status Vocabulary

The lane must distinguish:

- `healthy-awaiting-rearm`
- `healthy-armed`
- `paused-hitl`
- `done-retired`
- `fault-recoverable`
- `drift-detected`
- `scheduler-unregistered`

The old repeating-task language is not sufficient by itself.

In particular:

- `missing next run` is not always failure
- `disabled` is not always defect
- silence may mean rest, pause, done, or crash

The watchdog and status surfaces must tell those apart.

## Present Truth And Target Truth

Present implementation truth:

- the lane still uses a scheduler-first repeating trigger as temporary transport
- explicit `HITL` and blocked postures already pause the scheduled task
- the 24-hour digest already exists

Target constitutional truth:

- the main worker becomes one-shot single-flight
- the next wake is armed only after clean close
- the hourly watchdog becomes mandatory
- the daily digest remains the operator governance lane

Until the script migration lands, any surface that still speaks in repeating
timer terms should be treated as temporary transport law, not final temporal
law.
