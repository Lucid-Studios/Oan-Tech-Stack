# CODEX_AUTOMATION_ONCE_INTENT_CONTRACT

## Purpose

This document defines the truthful contract for repo-emitted Codex automation intent envelopes.

The purpose of the envelope is to preserve a future-bearing automation object without overstating current app capabilities.

## Why This Contract Exists

The orchestration layer needs a way to carry:

- thread-grounded intent
- bucket targets
- subject-predicate-action structure
- delayed release timing
- and wait posture

before a future movement event begins.

The current Codex automation surface is recurring-first, not natively one-shot.

So the repo must preserve one-shot intent honestly.

## Contract Rule

A Codex once-intent envelope is not itself proof that a native app automation already exists.

It is a truthful, structured request surface that preserves:

- what should run
- where it should run
- when it may first run
- what repo truth must still hold before release

## Required Fields

At minimum, an envelope should preserve:

- `instructionId`
- `sourceThreadLabel`
- `sourceCommit`
- `sourceBranch`
- `targetBucketIds`
- `subjectPredicateActionSets`
- `requestedDelayMinutes`
- `earliestEligibleRunUtc`
- `pollingIntervalSeconds`
- `pollingWindowMinutes`
- `movementAdmissibilityState`
- `codexAutomationSupportState`
- `materializationNextAction`

## Support State Rule

The envelope must explicitly say whether native one-shot support exists.

Current truthful states are:

- `intent-envelope-only`
- `requires-materialization`
- `natively-supported`

The repo must currently use `intent-envelope-only` unless the app truly supports native one-shot scheduling.

## Prompt Shape

The envelope may include a suggested Codex automation prompt.

That prompt should preserve:

- the target buckets
- the subject-predicate-action sets
- repo-truth gating
- expected receipts or outputs

It must not silently widen scope beyond the original instruction.

## Contact Rule

The envelope is a contact surface between:

- repo master-thread truth
- and future Codex automation execution

It does not grant authority by itself.

It simply preserves a lawful handoff object.
