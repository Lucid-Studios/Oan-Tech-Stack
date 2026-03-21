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
- `tools/Write-Promotion-GateBundle.ps1`
- `tools/Write-CiArtifactConcordance.ps1`
- `tools/Write-Release-RatificationRehearsal.ps1`
- `tools/Write-FirstPublish-IntentClosure.ps1`
- `tools/Write-Seeded-PromotionReview.ps1`
- `tools/Write-Release-HandshakeSurface.ps1`
- `tools/Write-Publish-RequestEnvelope.ps1`
- `tools/Write-PostPublish-EvidenceLoop.ps1`
- `tools/Write-SeedBraid-EscalationLane.ps1`
- `tools/Write-PublishedRuntime-Receipt.ps1`
- `tools/Write-Artifact-AttestationSurface.ps1`
- `tools/Write-PostPublish-DriftWatch.ps1`
- `tools/Write-OperationalPublication-Ledger.ps1`
- `tools/Write-ExternalConsumer-Concordance.ps1`
- `tools/Write-PostPublish-GovernanceLoop.ps1`
- `tools/Write-SchedulerExecution-Receipt.ps1`
- `tools/Write-UnattendedInterval-Concordance.ps1`
- `tools/Write-StaleSurface-ContradictionWatch.ps1`
- `tools/Write-UnattendedProof-Collapse.ps1`
- `tools/Write-DormantWindow-Ledger.ps1`
- `tools/Write-SilentCadence-Integrity.ps1`
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
- `.audit/state/local-automation-promotion-gate-last-run.json`
- `.audit/state/local-automation-ci-concordance-last-run.json`
- `.audit/state/local-automation-release-ratification-last-run.json`
- `.audit/state/local-automation-first-publish-intent-last-run.json`
- `.audit/state/local-automation-seeded-promotion-review-last-run.json`
- `.audit/state/local-automation-release-handshake-last-run.json`
- `.audit/state/local-automation-publish-request-envelope-last-run.json`
- `.audit/state/local-automation-post-publish-evidence-last-run.json`
- `.audit/state/local-automation-seed-braid-escalation-last-run.json`
- `.audit/state/local-automation-published-runtime-receipt-last-run.json`
- `.audit/state/local-automation-artifact-attestation-last-run.json`
- `.audit/state/local-automation-post-publish-drift-watch-last-run.json`
- `.audit/state/local-automation-operational-publication-ledger-last-run.json`
- `.audit/state/local-automation-external-consumer-concordance-last-run.json`
- `.audit/state/local-automation-post-publish-governance-loop-last-run.json`
- `.audit/state/local-automation-publication-cadence-ledger-last-run.json`
- `.audit/state/local-automation-downstream-runtime-observation-last-run.json`
- `.audit/state/local-automation-multi-interval-governance-braid-last-run.json`
- `.audit/state/local-automation-scheduler-execution-receipt-last-run.json`
- `.audit/state/local-automation-unattended-interval-concordance-last-run.json`
- `.audit/state/local-automation-stale-surface-contradiction-watch-last-run.json`
- `.audit/state/local-automation-unattended-proof-collapse-last-run.json`
- `.audit/state/local-automation-dormant-window-ledger-last-run.json`
- `.audit/state/local-automation-silent-cadence-integrity-last-run.json`

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

## Unattended Continuity Lane

The local cycle now carries a bounded unattended continuity lane beyond scheduler registration.

That lane may:

- collapse the first real scheduler proof when it actually appears
- count dormant-consistent cadence windows without confusing them for execution proof
- verify that stable `candidate-ready` stretches remain quiet until a real review edge or contradiction arrives

That lane may not:

- narrate scheduler proof before a real scheduled run exists
- convert dormant waiting into faux readiness beyond current evidence
- treat quiet cadence as permission to skip the daily review edge

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

The seeded lane now carries a ready-on-call posture:

- if the local seed host is already healthy, automation uses it directly
- if the host is cold but the runtime is provisioned, automation may attempt to stand it up before deferring
- if the runtime is not provisioned, the lane must report that directly instead of collapsing all failure into generic host unavailability

## Promotion Evidence Lane

The local cycle now carries a bounded promotion-evidence lane.

That lane may:

- prepare a promotion gate bundle from the latest digest and version decision
- compare local published artifacts against declared CI workflow truth
- rehearse the release ratification checklist without implying actual publication

That lane may not:

- promote versions automatically
- claim CI proof that is not locally present
- convert rehearsal into publication authority

## Release Handshake Lane

The local cycle now carries a bounded release-handshake lane between promotion evidence and any future publish request.

That lane may:

- declare first publish intent from current deployable and version truth
- let the seed produce an advisory promotion review
- collapse promotion evidence into one handshake surface that says what is ready, deferred, or still blocked

That lane may not:

- infer first publish scope beyond declared deployables
- let seeded review substitute for ratification
- imply that a handshake surface is itself a publication act

## Bounded Publish Lane

The local cycle now carries a bounded publish-preparation lane beyond the release handshake.

That lane may:

- prepare a publish request envelope without implying that ratification already happened
- keep a post-publish evidence loop in an explicitly waiting posture until publication is real
- expose a seed-braid escalation surface for post-publish contradiction handling

That lane may not:

- narrate a publish request as if it were an executed publication
- invent post-publish evidence before a published artifact exists
- let seed-braid escalation substitute for HITL in publication or contradiction resolution

## Publication Observation Lane

The local cycle now carries a bounded publication-observation lane beyond the publish-preparation seam.

That lane may:

- record that no real publication receipt has yet been observed
- keep artifact attestation waiting until a real receipt exists
- keep post-publish drift watch dormant until a live publication can actually drift

That lane may not:

- fabricate a published runtime receipt
- claim artifact concordance without a real published artifact
- narrate post-publish drift before the stack has crossed into live publication

## Operational Publication Consolidation Lane

The local cycle now carries a bounded operational-publication lane beyond the first observation seam.

That lane may:

- keep an operational publication ledger in a truthful pre-publication posture until real publication is observed
- compare the bounded publication chain against what an external consumer surface would actually receive
- braid post-publication governance into one bounded loop without implying ratified publication

That lane may not:

- fabricate an operational publication ledger before the first real receipt exists
- claim external concordance without a real consumer-observable surface
- let the governance loop substitute for publication ratification or post-publication human review

## Multi-Interval Publication Cadence Lane

The local cycle now carries a bounded multi-interval cadence lane after the first operational publication loop is formalized.

That lane may:

- keep a publication cadence ledger truthful about whether the stack has actually crossed into repeated intervals
- observe downstream runtime surfaces across more than one interval without inferring extra authority
- braid seeded governance and HITL review across repeated intervals without improvising publication law

That lane may not:

- fabricate repeated cadence before a second governed interval exists
- claim downstream runtime continuity from a single observation window
- let the multi-interval braid substitute for ratification, contradiction handling, or governed publication receipts

## Unattended Scheduler Witness Lane

The local cycle now carries a bounded unattended scheduler witness lane beyond mere scheduler registration.

That lane may:

- capture a real scheduler execution receipt when an unattended run has actually occurred
- compare unattended scheduler truth against the active release-candidate cycle without mistaking manual runs for autonomous proof
- watch dormant publication-adjacent surfaces for ordering contradictions while the stack is still waiting on real-world intervals

That lane may not:

- fabricate unattended proof from scheduler registration alone
- treat dormant-but-consistent surfaces as failure just because the world has not yet advanced
- let contradiction watch invent publication or cadence transitions that the evidence chain has not actually crossed

## Operational Bias

The active automation posture is:

> automate the stretch, verify the truth, and only stop where the stack itself declares a real gate

This keeps the build moving without lying about what is known.
