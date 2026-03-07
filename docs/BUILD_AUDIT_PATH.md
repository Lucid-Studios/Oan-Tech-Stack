# BUILD_AUDIT_PATH

## Purpose

This document defines the first audit-first maturation lane for the active OAN Tech Stack runtime.

The audit lane exists to answer one question:

**What did the build and runtime actually do, what did they emit, and what remained empty, withheld, or pointerized?**

The governing rule is:

**No silent green.**

A passing step must state:

- what it ran
- what it produced
- what payload it carried
- whether any output was intentionally pointerized or withheld
- why anything was empty

## Scope

This phase adds audit visibility around the existing runtime.

It does not:

- change custody law
- widen SoulFrame
- widen AgentiCore
- introduce new runtime authority lanes
- substitute audit evidence for governance truth

Audit remains outside the core runtime meaning path.

## Audit Bundle Model

Each audit run emits one bundle under:

```text
.audit/runs/<runId>/
```

Each bundle contains:

- `run.json`
- `events.ndjson`
- `projects.json`
- `tests.json`
- `payloads.json`
- `summary.md`
- `logs/`

## Bundle Files

### `run.json`

Runtime provenance for the audit run.

Must include:

- `runId`
- `startedAtUtc`
- `completedAtUtc`
- `repoRoot`
- `solutionPath`
- `configuration`
- `commitSha`
- `branch`
- `worktreeState`
- `toolchain`
- `scriptDigests`
- `outputDigests`
- `hopngValidationStatus`

### `events.ndjson`

Append-only audit step events.

The first event set is:

- `BUILD_RUN_STARTED`
- `PROJECT_BUILD_STARTED`
- `PROJECT_BUILD_COMPLETED`
- `TEST_RUN_STARTED`
- `TEST_RUN_COMPLETED`
- `VERIFY_RUN_STARTED`
- `VERIFY_RUN_COMPLETED`
- `SUBSYSTEM_AUDIT_STARTED`
- `SUBSYSTEM_AUDIT_COMPLETED`
- `PAYLOAD_WITNESS_CAPTURED`
- `AUDIT_RUN_COMPLETED`

### `projects.json`

Per-project compile evidence.

For v1, this is allowed to be a truthful summary of solution-level build evidence rather than a fully isolated per-project compile timer.

If a field is not individually measured, it must say so.

### `tests.json`

Per-test-project audit probes.

These should capture:

- project identity
- result
- duration
- result source
- payload classification
- why the result is summary-only if detailed payloads are not emitted

### `payloads.json`

Payload witness records for important build and runtime steps.

Allowed classifications:

- `payload_present`
- `pointer_only`
- `summary_only`
- `empty_by_policy`
- `empty_by_observation`
- `empty_by_design`
- `deferred`
- `denied`
- `dropped_error`
- `unimplemented`

### `summary.md`

Human-readable rollup of:

- provenance
- step outcomes
- durations
- payload classifications
- follow-up observations

## Provenance Rules

Audit provenance must remain separate from deterministic runtime outputs.

The audit lane records:

- commit SHA
- branch or ref
- worktree clean or dirty state
- build configuration
- script identity digests
- toolchain versions
- optional HDT lane status
- bundle output digests

Audit provenance must not mutate deterministic runtime products.

## Malformed or Missing Evidence

Audit may fail safe, but it must not invent success.

Rules:

- malformed evidence never implies completion
- missing evidence never implies payload presence
- summary-only evidence must say that it is summary-only
- unimplemented or withheld outputs must be named explicitly

## Pass Order

### Pass 0. Baseline Truth

Deliver one full build and runtime audit bundle.

Exit gate:

- one full build, test, and hygiene sequence completed
- structured bundle emitted
- step timing visible
- payload classifications visible

### Pass 1. CradleTek

Audit the application and composition fabric first.

Questions:

- what does CradleTek own
- what is orchestration only
- what leaks into custody, membrane, or publication
- what real outputs it produces

### Pass 2. SoulFrame

Audit membrane outputs and return-candidate behavior without widening the membrane.

### Pass 3. AgentiCore

Audit bounded cognition payload truth.

### Pass 4. Governance, storage, and publication connective tissue

Audit decision outputs, downstream acts, publication lanes, retries, and recoveries.

### Pass 5. SLI

Audit symbolic paths only after the core runtime path is payload-proven.

## Per-Subsystem Questions

Each subsystem pass must answer:

1. Ownership
2. Inputs
3. Outputs
4. Payload truth
5. Placement

## Required Scripts

The first audit lane is exposed through:

- `tools/Invoke-Build-Audit.ps1`
- `tools/Invoke-Subsystem-Audit.ps1`

The build audit wraps:

- `build.ps1`
- `test.ps1`
- `OAN Mortalis V1.0/tools/verify-private-corpus.ps1`

## Initial Truthfulness Constraint

This phase is not allowed to hide uncertainty behind green build status.

If the audit lane only has solution-level build evidence for a project, it must say:

- result is based on solution build evidence
- warning counts or fine-grained timings were not individually measured

That is acceptable in v1.

Silent omission is not.

## Success Criteria

This phase is successful when:

- every build run emits a structured audit bundle
- every important step has payload classification
- empty outputs are explained
- subsystem audits can be run in order from CradleTek through SLI
- the repo can distinguish what is real, what is pointerized, what is withheld, and what is still scaffolding
