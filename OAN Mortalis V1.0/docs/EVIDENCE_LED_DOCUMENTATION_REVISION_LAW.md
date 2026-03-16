# EVIDENCE_LED_DOCUMENTATION_REVISION_LAW

## Purpose

This document defines how governed documentation revision should occur as the active build matures.

It exists to keep the documentation cycle evidence-led rather than cadence-led, even when revisions are performed on a recurring schedule.

## Core Rule

Documentation should advance because governing truth changed, not merely because time elapsed.

A periodic revision cycle may trigger review work, but it must not justify speculative edits by itself.

## Selective Audit Rule

Only documents affected by one or more of the following should be audited and revised:

- code maturation
- package promotion
- contract change
- runtime telemetry
- governance receipts or artifact evidence
- theory growth
- doctrine correction
- architecture-seam discovery
- contradiction between executable truth and stabilized conceptual truth

Documents that were not affected by governing change should remain stable.

## Agent Exploration Rule

Agents should use the revision cycle to explore what needs to exist next without rewriting the repo as though those future structures already exist.

When exploring beyond current implementation, agents must distinguish clearly between:

- implemented and verified
- implemented on fallback or unavailable paths only
- doctrine-defined but not yet contract-backed
- contract-backed but not yet runtime-exercised
- exploratory horizon work not yet admitted into the active build

Unknown future structure should be proposed as explicit seam, debt, or follow-up work rather than filled in as present truth.

## Evidence Inputs

The revision cycle should preferentially ground itself in:

- the current active repo code state
- current test and build outcomes
- current audit outputs
- current runtime evidence and governance telemetry
- current `Documentation Repo` theory state when that surface is available

It should not ground itself in:

- stale summaries
- cached model memory
- repetitive reformulation with no new evidence
- speculative completion of architecture that the build has not yet taught

## Output Expectations

A lawful documentation revision cycle should produce one or more of the following:

- updated doctrine where governing truth changed
- updated readiness debt where implementation lags doctrine
- updated migration notes where older terms or placements remain in code
- updated implementation targets where new seams become actionable
- explicit statement that no governed documentation changes were warranted for unaffected surfaces

## Build Discipline Relation

This law is part of build discipline, but it is not a build gate in the same sense as restore, build, test, or path hygiene.

Its purpose is to improve exploration quality and reduce drift by making documentation revision:

- selective
- evidence-led
- auditable
- non-speculative

## Documentation Repo Relation

The `Documentation Repo` is an active governing documentation surface for stabilized conceptual truth and theory digestion.

When it is available, agents should use it with the active repo as a dual-source governance posture:

- active repo for executable present truth
- `Documentation Repo` for stabilized conceptual truth

When it is unavailable, agents should say so plainly and continue from the active repo's executable truth rather than inventing a missing conceptual state.
