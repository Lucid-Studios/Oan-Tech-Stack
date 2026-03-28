# DOCUMENTATION_REPO_GOVERNANCE_UPTAKE_MODEL

## Purpose

This document defines the `Documentation Repo` as an active governing documentation surface for the `OAN Mortalis V1.0` build cycle.

It exists to keep theory, doctrine, and executable implementation from drifting apart as the stack matures.

## Core Placement

The active build repo records:

- executable present truth
- immediate implementation constraints
- current contracts, tests, and runtime behavior

The `Documentation Repo` records:

- stabilized conceptual truth
- theory digestion
- doctrine consolidation
- architecture-seam interpretation after code maturation

Governed build work must consult both surfaces when the `Documentation Repo` is available.

## Governing Rule

The build cycle is:

`build -> observe -> document -> govern -> build again`

This means:

- code maturation may yield theory-relevant telemetry
- package promotion may yield doctrine correction
- runtime seam discovery may yield conceptual consolidation
- future implementation should then ground itself in both the active repo and the stabilized documentation surface

This cycle may be reviewed on a recurring schedule, but the schedule itself does not justify document mutation.

## Anti-Drift Law

Build work should preferentially ground itself in:

- current repo-local code state
- current `Documentation Repo` theory state

It should not infer missing architecture from:

- stale summaries
- cached assumptions
- aspirational prose that has not been reconciled against the living build

## Role Boundaries

The `Documentation Repo` is:

- an active governing documentation surface
- a recipient of theory-relevant implementation telemetry
- a stabilized conceptual memory for future implementation passes

The `Documentation Repo` is not:

- the active build target
- a replacement for executable truth
- a runtime authority surface
- a justification for overriding current code behavior by narrative alone

## Contradiction Handling

When the active build repo and the `Documentation Repo` diverge:

- executable repo-local truth remains authoritative for current runtime behavior
- the divergence must be surfaced as doctrine debt, build debt, or migration debt
- future work should reconcile the two surfaces explicitly rather than silently choosing one

## Locality And Path Discipline

The `Documentation Repo` is treated in this workspace by logical label only.

Tracked files in the active build repo must not:

- hard-code an external absolute documentation-repo path
- depend on a machine-specific external checkout path for local restore, build, or test

The existence or absence of a local documentation checkout may influence operator research posture, but it must not silently distort the executable build path.

## Operational Corollary

Where the code repo records executable truth and immediate implementation constraints, the `Documentation Repo` records stabilized conceptual truth and theory digestion.

Governed build work must consult both when available.

If the `Documentation Repo` is unavailable in a given working session, that absence should be stated plainly and the build should proceed from executable local truth rather than speculative reconstruction.

## Revision Discipline

Documentation revision should be evidence-led and selective.

Only documents affected by governing change should be advanced.

The strongest evidence classes are:

- build and test outcomes
- code maturation
- runtime telemetry
- governance receipts and artifact evidence
- doctrine correction
- theory growth that changes governing interpretation

Unaffected documents should remain stable rather than being refreshed for cadence alone.

## Research Subject Intake

When discussion or seam discovery outpaces current build admission, the active repo may stage a bounded research subject for the `Documentation Repo`.

Such a research subject should:

- name the theme to be researched
- ground itself in current active-repo executable truth first
- state clearly what is doctrine-backed versus still exploratory
- identify review-cleanup targets without implying automatic build mutation

Research subjects are valid intake surfaces for theory digestion and review cleanup.

They are not:

- runtime authority
- executable truth by declaration
- permission to skip later reconciliation against the active build
