# GOVERNED_BUILD_AUTOMATION_CONVEYOR

## Purpose

This document defines the first automation conveyor for the active `OAN Mortalis V1.0` build.

The goal is not to remove human judgment.

The goal is to automate everything that is:

- mechanical
- repeatable
- evidential
- versionable

while stopping cleanly when the stack reaches a declared human gate.

## Core Law

Automation may:

- verify hygiene
- resolve build versions
- classify touched projects
- run build and test
- run build and subsystem audits
- publish declared deployables into candidate artifact bundles
- emit evidence manifests

Automation may not:

- infer maturity from green output alone
- widen authority, publication, or deployable scope by implication
- promote a modular-set change without a declared HITL gate
- treat documentation drift as executable truth

## Declared Truth Surfaces

The conveyor is grounded in:

- `build/family-maturity.json`
- `build/deployables.json`
- `build/version-policy.json`
- `build/hitl-gates.json`

These files declare what the automation is allowed to believe about:

- uneven project maturity
- current deployable surfaces
- version progression
- when human judgment is still mandatory

## Conveyor Shape

1. Resolve version and touched project truth.
2. Run workspace hygiene.
3. Run build audit.
4. Run subsystem audit.
5. Publish only declared deployables.
6. Emit a build evidence manifest.
7. Return one of:
   - `candidate-ready`
   - `hitl-required`
   - `blocked`

## Present Deployable Truth

The current first publish surface is intentionally narrow:

- `src/Oan.Runtime.Headless/Oan.Runtime.Headless.csproj`

Everything else may be buildable and operational without automatically being publishable.

## HITL Boundary

The conveyor is deliberately biased toward progress.

It does not stop merely because work is ambitious or novel.

It stops when a declared gate is crossed, for example:

- modular-set promotion
- new deployable introduction
- unmapped source/test surface changes
- undeclared authority widening
- publication promotion beyond candidate-ready

## Evidence Artifacts

Candidate runs write ignored local evidence under `.audit/runs/`.

Those artifacts are for:

- verification
- comparison
- packaging proof
- promotion review

They are not identity-forming runtime memory.

## Local Trust-Verified Cycle

The repo now carries a local automation cycle for longer unattended stretches:

- release-candidate conveyor cadence: every `6` hours
- mandatory HITL digest cadence: every `24` hours
- blocked status: stop immediately
- `hitl-required` status: keep verification moving, freeze promotion, surface it in the next digest

This cycle is declared in:

- `build/local-automation-cycle.json`

The supporting scripts are:

- `tools/Invoke-Local-Automation-Cycle.ps1`
- `tools/Write-Release-Candidate-Digest.ps1`
- `tools/Write-Local-AutomationNotification.ps1`
- `tools/Write-Local-Automation-TaskStatus.ps1`
- `tools/Install-Local-AutomationCycleTask.ps1`
- `tools/Invoke-Seeded-Build-Governance.ps1`
- `tools/Sync-Local-AutomationScheduler.ps1`
- `tools/Write-CmeFormalization-ConsolidationStatus.ps1`

The stable status surfaces are:

- `.audit/state/local-automation-cycle.json`
- `.audit/state/local-automation-cycle-last-run.json`
- `.audit/state/local-automation-notification-last-run.json`
- `.audit/state/local-automation-tasking-status.json`
- `.audit/state/local-automation-tasking-status.md`
- `.audit/state/local-automation-seeded-governance-last-run.json`
- `.audit/state/local-automation-scheduler-reconciliation-last-run.json`
- `.audit/state/local-automation-cme-consolidation-state.json`

The formal tasking surface is:

- `build/local-automation-tasking.json`
- `docs/LOCAL_AUTOMATION_TASKING_SURFACE.md`

The local cycle is intentionally biased toward continued build progress.

It does not require a human to bless every green pass.

It does require a human to review at least once every `24` hours, or sooner if the stack enters a blocked state.

The cycle now includes a bounded notification seam:

- it stays quiet while posture remains `candidate-ready`
- it emits a local notification bundle when posture transitions into `hitl-required` or `blocked`
- it attempts a best-effort Windows popup without failing the build if the popup channel is unavailable

## Seeded Governance Lane

The local cycle may now braid a seeded local host into unattended build review.

That seeded lane may:

- interpret routine build evidence
- run the local preflight profile
- classify posture as `Accepted`, `Deferred`, or `Rejected`
- contribute advisory provenance to the build surface

That seeded lane may not:

- promote versions
- widen authority
- publish by implication
- overrule a blocked posture

Seeded outputs are evidence artifacts, not autonomous release truth.

## Operational Bias

The active automation posture is:

> automate the stretch, verify the truth, and only stop where the stack itself declares a real gate

This keeps the build moving without lying about what is known.
