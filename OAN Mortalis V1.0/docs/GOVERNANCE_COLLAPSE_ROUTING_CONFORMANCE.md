# GOVERNANCE_COLLAPSE_ROUTING_CONFORMANCE

## Purpose

This document records the current governance and collapse-routing conformance state against the CME lifecycle model.

It is an evidence document, not a new constitutional source.

Its job is to answer five questions:

1. what collapse outputs are actually distinguishable today
2. where routing decisions are made today
3. which dispositions are explicit, implied, or missing
4. which runtime objects currently carry routing semantics
5. which direct paths remain forbidden

This document must now be read together with `docs/CME_CRYPTIC_FIRST_COLLAPSE_CLARIFICATION.md`.

## Current Routing Path

The current runtime path is:

1. AgentiCore emits a bounded governance-cycle candidate payload and return-candidate pointer.
2. SoulFrame receives the return candidate and emits a collapse-evaluation receipt shape.
3. Governance adjudication in `StewardAgent` decides `Approved`, `Rejected`, or `Deferred`.
4. Approved outcomes may produce:
   - a governed re-engrammitization request
   - a governed Prime publication request
5. `MantleOfSovereigntyService` re-engrammitizes approved residue into `cMoS` under the current coarse `retain_in_mos` runtime label.
6. `PublicLayerService` publishes authorized Prime derivative lanes.
7. Deferred outcomes enter the deferred-review backlog.
8. Rejected and deferred outcomes suppress downstream mutation and publication.

## Current Code Surfaces

The live routing semantics currently sit across:

- `src/SoulFrame.Host/SoulFrameHostClient.cs`
- `src/Oan.Common/SoulFrameMembraneContracts.cs`
- `src/AgentiCore/Services/AgentiCore.cs`
- `src/Oan.Common/GovernanceGoldenPathContracts.cs`
- `src/EngramGovernance/Services/StewardAgent.cs`
- `src/CradleTek.Mantle/MantleOfSovereigntyService.cs`
- `src/CradleTek.Public/PublicLayerService.cs`
- `src/Oan.Cradle/StackManager.cs`

## Distinguishable Collapse Outputs Today

The current runtime can already distinguish these outputs:

- bounded return-candidate submission
- governance decision receipt
- approved re-engrammitization request
- approved Prime publication request
- deferred-review backlog item
- Prime pointer publication
- Prime checked-view publication

What it cannot yet distinguish as first-class collapse objects:

- Dream-seed eligibility as its own runtime class
- explicit discard object or discard receipt
- explicit `cGoA` routing object inside the Golden Path

## Where Routing Decisions Are Made Today

### SoulFrame

SoulFrame currently decides and exposes:

- that the return is a candidate-collapse evaluation
- whether review is required
- whether custody routing is directly allowed
- whether Prime publication is directly allowed

SoulFrame does not perform the final routing.

### StewardAgent

`StewardAgent` currently decides:

- `Approved`
- `Rejected`
- `Deferred`

And from that decision it determines whether:

- re-engrammitization is authorized
- Prime publication is authorized
- deferred review backlog entry is created

Outside the Golden Path, `StewardAgent.RouteResidueAsync(...)` also shows residue routing logic for:

- `GoA`
- `cGoA`
- `Discard`

That proves the concepts exist in code, but not yet as an explicit Golden Path collapse-disposition vocabulary.

### StackManager

`StackManager` currently composes the approved downstream acts and ensures:

- rejected and deferred paths do not continue to mutation/publication
- approved paths move through re-engrammitization and publication under replay/idempotency law
- pending recovery resumes only lawful missing downstream acts

`StackManager` is the orchestration root, not the policy author.

## Disposition Truth Table

| Disposition | Current Status | Evidence | Notes |
| --- | --- | --- | --- |
| `retain_in_mos` | explicit but coarse | `MantleOfSovereigntyService.ReengrammitizeAsync(...)` appends approved residue into `cMoS` | The current runtime name is useful, but in the corrected model it must not be read as raw close -> `MoS`. It currently corresponds more closely to first protected retention via `cMoS`. |
| `route_to_cgoa` | implied / partial | `StewardAgent.RouteResidueAsync(...)` supports `cGoA` targeting outside the Golden Path | The concept exists, but it is not yet a first-class Golden Path collapse-disposition object. |
| `eligible_for_dream_seed` | missing | no explicit runtime routing object or destination | Still doctrinal only. |
| `discard_transient` | implied / partial | `StewardAgent.RouteResidueAsync(...)` supports `Discard`; rejected/deferred paths also suppress mutation/publication | There is no explicit discard receipt or collapse-disposition object yet, and the corrected model now prefers distinguishing `cGoA` contextual residue from `cMoS` autobiographical residue before treating discard as the governing idea. |
| `defer_review` | explicit | governance `Deferred` decision, deferred-review record, deferred backlog, later review actions | This is the strongest current non-custody/non-publication retention path. |

## Current Runtime Objects Carrying Routing Meaning

The routing semantics currently live in these objects:

- `SoulFrameCollapseEvaluation`
- `SoulFrameReturnIntakeReceipt`
- `ReturnCandidateReviewRequest`
- `GovernanceDecisionReceipt`
- `GovernedReengrammitizationRequest`
- `GovernedPrimePublicationRequest`
- `DeferredReviewRecord`
- `GovernanceActReceipt`

What is still missing is an explicit collapse-disposition object that names, in one runtime vocabulary, where first protected substance is going under the corrected Cryptic-first model.

That gap is now only partly closed for the two routing paths that are already fully real in coarse form:

- `RetainInMos`
- `DeferReview`

## Forbidden Paths Already Enforced

The following remain out of law today:

- no direct `SelfGEL -> cGoA` write
- no direct `SelfGEL -> MoS/cMoS` write
- no direct active runtime residue -> Prime publication
- no deferred path -> mutation or publication
- no rejected path -> mutation or publication

These are currently enforced by the division between:

- AgentiCore candidate output
- SoulFrame collapse evaluation
- governance adjudication
- approved downstream gates

## Conformance Assessment

The current runtime does not yet have the full explicit collapse-disposition lattice from the CME lifecycle model.

What it does have is:

- explicit but coarse `retain_in_mos`
- explicit `defer_review`
- explicit minimal runtime collapse-disposition vocabulary for those two paths
- partial `discard_transient`
- partial `route_to_cgoa`
- absent `eligible_for_dream_seed`

So the collapse model is no longer purely aspirational, but it is still only partly named in runtime contracts.

Under the corrected Cryptic-first collapse reading:

- the current `retain_in_mos` name must be treated as transitional and coarse
- first protected collapse still lands Cryptic-first
- the runtime does not yet explicitly distinguish first `cGoA` peel, `cMoS` protected autobiographical holding, later `cGoA` enrichment, and later `MoS` promotion

## Recommended Next Cut

The next bounded implementation cut should not implement all five dispositions at once.

The strongest current reality has now been made explicit first:

1. `retain_in_mos`
2. `defer_review`

The next candidate, if the code is already close enough, is not raw discard first but truthful first-route vocabulary for:

3. `route_to_cgoa`
4. `route_to_cmos`

That keeps the runtime honest under the corrected model by naming first protected destinations before later Dream or `MoS` promotion, and by preventing `retain_in_mos` from being misread as raw close -> `MoS`.

## Completion Condition

This conformance line is successful when the runtime can truthfully say, using explicit contract vocabulary, whether CME-formed substance is first being:

- routed into `cGoA`
- routed into `cMoS`
- deferred for review

and only later, after deeper curation, whether it is:

- promoted toward `MoS`
- marked Dream-eligible

and when the code only claims as explicit what it already truly performs.
