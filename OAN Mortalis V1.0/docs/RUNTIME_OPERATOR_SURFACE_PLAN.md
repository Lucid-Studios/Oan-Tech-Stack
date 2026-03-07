# Runtime Operator Surface Plan v1

## Summary

This phase adds the first human-usable operator surface over the internal governance-loop control plane. It is intentionally local, CLI-first, and thin: it exposes journal-first status, deferred review, and pending-recovery actions without adding new custody, membrane, or publication authority.

The operator surface exists to answer one practical question:

When the governance loop is running under real conditions, how does a steward or operator inspect it, review deferred work, resume recoverable failures, and trust the result?

## Scope

This phase includes:

- `Oan.Runtime.Headless` as the local operator shell
- command groups for `status`, `deferred`, and `recovery`
- JSON success output on `stdout`
- JSON failure output on `stderr`
- canonical local exit codes
- shared loop-key normalization via `GovernanceLoopKeys.Create(...)`

This phase excludes:

- remote or multi-process admin surfaces
- direct journal reads or writes from the CLI
- policy logic inside the CLI
- direct custody, membrane, or publication actions outside the live control-plane contracts

## Commands

Read-only commands:

- `status --loop-key <key>`
- `status --candidate-id <guid> --provenance <value>`
- `deferred list`
- `deferred get --loop-key <key>`
- `recovery list`

Actuation commands:

- `deferred annotate --loop-key <key> --reviewed-by <id> --annotation <text>`
- `deferred approve --loop-key <key> --reviewed-by <id> --rationale <code> [--annotation <text>]`
- `deferred reject --loop-key <key> --reviewed-by <id> --rationale <code> [--annotation <text>]`
- `recovery resume --loop-key <key> --requested-by <id> --reason <text>`
- `recovery retry-lane --loop-key <key> --lane pointer|checked-view --requested-by <id> --reason <text>`

Compatibility:

- `evaluate`

## Response Contract

Success envelope:

- `ok`
- `command`
- `timestamp`
- optional `loopKey`
- `result`

Failure envelope:

- `ok`
- `command`
- `timestamp`
- optional `loopKey`
- `errorCode`
- `message`
- `exitCode`

The CLI must clearly distinguish:

- loop not found
- loop found but failed-safe due to malformed evidence
- loop found but action invalid from current typed state

## Exit Codes

- `0` success
- `2` invalid arguments
- `3` not found
- `4` invalid state or unlawful action
- `5` runtime failure
- `6` malformed journal or failed-safe evidence condition

## Authority Boundary

The CLI is a thin shell over:

- `IGovernanceLoopStatusReader`
- `IDeferredReviewQueue`
- `IPendingRecoveryCoordinator`

It must not:

- reconstruct loop state itself
- parse journal files directly
- make governance decisions itself
- widen custody or membrane semantics
- bypass governed publication or re-engrammitization gates

## Locality

This surface is explicitly local and process-scoped in v1. It is suitable for steward/operator control on a single runtime host, but it does not claim cross-process or distributed execution guarantees.

## Completion Condition

This phase is complete when:

- an operator can query loop status by loop key or candidate/provenance
- an operator can list and inspect deferred items
- an operator can annotate, approve, and reject deferred items through the lawful review queue
- an operator can list pending recovery and explicitly resume or retry a publication lane
- malformed evidence surfaces as explicit failed-safe output
- `evaluate` remains unchanged

Future extension:

- richer custody-aware operator telemetry remains deferred to the visibility lattice defined in `docs/OPERATOR_TELEMETRY_VISIBILITY_LATTICE.md`

## Visibility Lattice Conformance

The operator surface is now bound to the future visibility lattice as a conformance item, not as a full implementation phase.

### Model claim

Operator telemetry must remain custody-aware and tiered by disclosure class rather than drifting into default content visibility.

### Current code surface

- `IGovernanceLoopStatusReader`
- `IDeferredReviewQueue`
- `IPendingRecoveryCoordinator`
- `GovernanceLoopStatusView`
- `GovernanceDecisionView`
- `PublicationLaneStatusView`
- the local CLI surface in `src/Oan.Runtime.Headless`

### Proven now

- the current operator surface is descriptive only
- no control-plane query surface widens custody, membrane, or publication authority
- malformed evidence already fails safe instead of silently appearing complete
- the current CLI remains Prime-safe by default because it exposes status and workflow metadata only

### Missing now

The current status models do not yet explicitly carry visibility-lattice metadata such as:

- visibility class
- consent state
- governed access state
- privileged access state
- protection or classification posture
- disclosure eligibility summary

### Next cut

The next bounded operator-surface conformance cut should add explicit visibility-lattice metadata fields to the internal status and read models and validate that the default CLI surface remains Prime-safe.

That cut must still avoid:

- protected content previews
- HDT-backed runtime projection
- new authority paths through the operator shell
- deeper privileged or classified handling logic
