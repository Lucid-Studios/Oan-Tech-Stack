# WORKSPACE_BUCKET_GROUP_SYSTEM

## Purpose

This document defines a repo-local bucket grouping system for the active `OAN Mortalis V1.1.1` workspace while the bucket policy remains staged under `OAN Mortalis V1.1.1/Automation/`.

It exists as a build-aware analogue to tool-space folder grouping.

The current tool surface can separate conversations and threads, but it does not yet give the stack a first-class way to keep hard-separated buckets of work while still reporting one shared build picture across them.

This bucket system supplies that missing middle layer.

It lets the repo say:

- which work buckets exist
- which folders and families belong to them
- which buckets are touched right now
- which buckets carry deployable or authoritative projects
- what the current automation posture is while those buckets remain separate

## Governing Contract

The machine-readable bucket contract lives in:

- `Automation/workspace-bucket-groups.json`

The live bucket-awareness surfaces are:

- `.audit/state/workspace-bucket-status.json`
- `.audit/state/workspace-bucket-status.md`

The live state is written by:

- `tools/Write-Workspace-BucketStatus.ps1`

The local automation cycle may refresh that state so bucket awareness stays live rather than manual-only.

## What A Bucket Is

A bucket is a bounded work-awareness surface.

It is not:

- an independent stack
- an authority boundary by itself
- a replacement for family law
- a license to ignore shared build truth

A bucket is simply a durable way to keep:

- paths
- projects
- families
- docs
- and active work

coherently grouped without collapsing the whole repo into one undifferentiated work mass.

## Shared Build Awareness Rule

Buckets may remain hard-separated for day-to-day construct work.

They must still report back into one shared build-awareness surface.

That means the bucket system must preserve at least:

- current repo worktree cleanliness
- current automation posture
- current long-form map posture when available
- authoritative and promotable project counts per bucket
- deployable presence per bucket
- active changed-path visibility per bucket

## Overlap Rule

Buckets may overlap where the stack itself is cross-cutting.

For example:

- doctrine may touch both `SLI` and Documentation Repo research
- governance docs may touch both build automation and office law

Overlap is lawful when it preserves clarity.

Overlap must not be used to blur:

- family ownership
- authority boundaries
- or executable truth

## Current Bucket Set

The initial bucket contract defines these work groups:

- `Build Governance And Automation`
- `CradleTek And Cryptic Substrate`
- `SoulFrame And Office Governance`
- `AgentiCore Runtime Harness`
- `SLI And Lisp Topology`
- `OAN Runtime Composition`
- `Documentation And Research`

These are intended to be:

- strong enough to separate construct needs
- broad enough to preserve whole-stack awareness

## Why This Exists

Without this layer, the workspace is forced into an unhealthy choice:

- either keep everything together and lose local separation
- or separate work manually and lose shared build awareness

This system avoids that collapse.

It lets the build keep one truthful picture while work remains meaningfully bucketed.

## Non-Goals

This system does not:

- replace the canonical family model
- replace the automation tasking surface
- replace the deployables or maturity manifests
- claim the app tool space already supports cross-folder build awareness natively

It is a repo-local compensating structure until richer tool-space grouping exists.
