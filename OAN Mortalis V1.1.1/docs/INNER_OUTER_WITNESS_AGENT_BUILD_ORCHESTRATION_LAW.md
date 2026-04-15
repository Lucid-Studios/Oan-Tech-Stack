# INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW

## Purpose

This note defines the first repo-seated multi-agent build topology for the
active `OAN Mortalis V1.1.1` line.

It exists so bounded agent collaboration can plug into the current
master-thread dispatch law rather than bypassing it.

The working model is:

- one `Integrator`
- one `InnerBuilder`
- one `OuterBuilder`
- one `Witness`

This is a build topology.

It is not a peer-merge social model, and it is not a scheduler-first autonomy
claim.

## Governing Contract

This law is subordinate to:

- `MASTER_THREAD_BUCKET_ORCHESTRATION_LAW.md`
- `WORKSPACE_BUCKET_GROUP_SYSTEM.md`
- `LOCAL_AUTOMATION_TASKING_SURFACE.md`
- `GOVERNED_BUILD_AUTOMATION_CONVEYOR.md`

The machine-readable policy surface for this law lives at:

- `build/agent-work-lanes.json`

The active authority remains repo-local executable truth.

## Lane Topology

The build now names four bounded lane classes:

### `Integrator`

The `Integrator` is the only truth-integration authority.

It may reconcile all lane returns, but it should not compete with worker write
scopes during an active slice.

Only the `Integrator` may mutate shared readiness, carry-forward, and
orchestration surfaces.

### `InnerBuilder`

The `InnerBuilder` handles:

- doctrine
- contracts
- audit law
- `GEL`
- `SelfGEL`
- first-run substrate

### `OuterBuilder`

The `OuterBuilder` handles:

- runtime surfaces
- harnesses
- API seams
- hosted `LLM` surfaces
- outward evidence bodies

### `Witness`

The `Witness` handles:

- tests
- audit validation
- barrier classification
- evidence-boundary enforcement

The `Witness` does not integrate shared readiness or carry-forward truth.

## Bucket Target Rule

These lanes target existing workspace buckets.

They do not invent new top-level buckets.

The current lane-to-bucket mapping is:

- `InnerBuilder`
  - `cradletek-cryptic-substrate`
  - `soulframe-office-governance`
  - doctrine and contract portions of `oan-runtime-composition`
- `OuterBuilder`
  - `agenticore-runtime-harness`
  - `sli-lisp-topology`
  - runtime and harness portions of `oan-runtime-composition`
- `Witness`
  - `build-governance-automation`
  - the `tests/Sanctuary/*` and audit-bearing portions of
    `oan-runtime-composition`
- `Integrator`
  - may reconcile all existing buckets without replacing them

## Shared Truth Rule

Shared cross-lane truth surfaces are `Integrator`-owned.

At minimum, those shared surfaces include:

- `BUILD_READINESS.md`
- `V1_1_1_CARRY_FORWARD_LEDGER.md`
- `V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md`
- `INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW.md`
- `build/agent-work-lanes.json`

Worker output without a lawful return receipt is not admissible build
authority.

Receipted worker output may be reviewed, narrowed, held, refused, escalated,
or admitted by the `Integrator`.

It may not silently rewrite shared repo truth.

## Request Envelope

Each bounded lane request must carry:

- `lane`
- `sliceId`
- `subject`
- `predicate`
- `actions`
- `targetBuckets`
- `ownedWriteScope`
- `acceptanceChecks`

These fields preserve the subject-predicate-action grammar that the
master-thread layer already uses.

## Return Receipt

Each bounded lane return must carry:

- `lane`
- `sliceId`
- `touchedSurfaces`
- `writeScopeSatisfied`
- `buildStatus`
- `testStatus`
- `barrierKinds`
- `recommendedDisposition`
- `evidenceArtifacts`
- `notes`

Returns are integrated by slice.

They are not integrated as a continuous unreceipted stream.

## Shared Verification Lane

The repo-root `build.ps1` and `test.ps1` wrappers target the same line-local
solution graph and shared `obj/bin` mutation surfaces.

That means parallel build work may remain lawful at the lane level while direct
verification execution must remain line-serialized.

The root wrappers therefore acquire one shared line verification lock through:

- `tools/Use-LineVerificationLock.ps1`

before invoking `dotnet build` or `dotnet test`.

This keeps the `Witness` verification lane parallel-admissible in planning
while preventing overlapping verification writes against the same line-local
build body.

## First Operational Model

The first implementation is manual subagent orchestration only.

That means:

- the `Integrator` spawns or directs bounded worker agents
- each worker receives one slice with one owned write scope
- the `Witness` reviews the return against declared checks
- the `Integrator` chooses exactly one disposition:
  - `Admit`
  - `Hold`
  - `Narrow`
  - `Return`
  - `Refuse`
  - `Escalate`

The first implementation explicitly does not admit:

- peer-to-peer worker integration
- shared writes
- autonomous worker promotion
- native scheduling dependence
- a separate condensation lane

## Barrier Law

`Witness` blocks at barriers.

The witness lane must block movement when any of these appear:

- overlapping write scope
- build failure
- test failure
- hygiene failure
- evidence-class collapse such as cloud and local proof being treated as one
  class
- no-commit and heat-law violation
- `CME`, office, or governing authority overclaim
- new tool, lane, or domain entry beyond current admission
- irreversible action without explicit gate truth

In this first slice, hygiene failure is classified under the existing contract
barrier grammar rather than adding a separate hygiene barrier enum.

## Non-Goals

This slice does not:

- create a scheduler-driven automation fabric
- widen current admission into automation-first orchestration
- make worker lanes sovereign planners
- let workers co-author shared truth surfaces directly
- claim that agent collaboration is already runtime standing or `CME` proof
