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
Qualification criteria for first-route holding are defined in `docs/CME_COLLAPSE_QUALIFICATION_CRITERIA.md`.

## Current Routing Path

The current runtime path is:

1. AgentiCore emits a bounded governance-cycle candidate payload and return-candidate pointer.
2. SoulFrame receives the return candidate and emits a collapse-evaluation receipt shape.
3. Governance adjudication in `StewardAgent` decides `Approved`, `Rejected`, or `Deferred`.
4. `StackManager` converts governance decision plus bounded collapse classification into:
   - first-route holding destination
   - residue class
   - review state
5. Approved autobiographical or `SelfGEL`-identified residue routes to `cMoS` through the live re-engrammitization path.
6. Approved contextual residue routes to protected `cGoA` holding without re-engrammitization.
7. Deferred outcomes still route to `cMoS` or `cGoA` first, then remain visible in the deferred-review backlog under `DeferredReview`.
8. `PublicLayerService` publishes authorized Prime derivative lanes only for approved cases.
9. Rejected outcomes suppress downstream holding and publication.

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
- later `MoS` promotion as distinct from first `cMoS` holding

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

That proves the concepts exist in code, but not yet as the full later-stage routing lattice.

### StackManager

`StackManager` now composes first-route holding and downstream acts and ensures:

- rejected paths do not continue to holding or publication
- approved autobiographical paths move through `cMoS` holding via re-engrammitization and publication under replay/idempotency law
- approved contextual paths move through protected `cGoA` holding before any publication lane work
- deferred paths still hold in `cMoS` or `cGoA`, then remain deferred for later review
- pending recovery resumes only lawful missing downstream acts

`StackManager` is the orchestration root, not the policy author.

## Disposition Truth Table

| Runtime Meaning | Current Status | Evidence | Notes |
| --- | --- | --- | --- |
| `RouteToCMoS` | explicit | `StackManager`, `GovernanceLoopStateModel`, `MantleOfSovereigntyService.ReengrammitizeAsync(...)` | This is the current first-route holding path for approved autobiographical or `SelfGEL`-identified protected residue. It is not raw close -> `MoS`. |
| `RouteToCGoA` | explicit | `StackManager`, `GovernanceLoopStateModel`, `ICrypticCustodyStore.AppendAsync(...)` via `CustodyDomain = "cGoA"` | This is the current first-route holding path for approved or deferred contextual protected residue. |
| `DeferredReview` review state | explicit | governance `Deferred` decision, deferred-review record, deferred backlog, routing decisions with `ReviewState = DeferredReview` | Review is now a separate axis from first-route holding destination. |
| `eligible_for_dream_seed` | missing | no explicit runtime routing object or destination | Still doctrinal only. |
| `discard_transient` | implied / partial | `StewardAgent.RouteResidueAsync(...)` supports `Discard`; rejected/deferred paths also suppress mutation/publication | There is no explicit discard receipt or collapse-disposition object yet, and the corrected model now prefers distinguishing `cGoA` contextual residue from `cMoS` autobiographical residue before treating discard as the governing idea. |

## Current Runtime Objects Carrying Routing Meaning

The routing semantics currently live in these objects:

- `CmeCollapseClassification`
- `CmeCollapseQualificationResult`
- `CmeCollapseReviewState`
- `CmeCollapseRoutingDecision`
- `SoulFrameCollapseEvaluation`
- `SoulFrameReturnIntakeReceipt`
- `ReturnCandidateReviewRequest`
- `GovernanceDecisionReceipt`
- `GovernedReengrammitizationRequest`
- `GovernedPrimePublicationRequest`
- `DeferredReviewRecord`
- `GovernanceActReceipt`

The runtime can now explain not only first protected destination, but also the evidence basis for that destination:

- classification confidence
- evidence flags
- review-trigger evidence
- source subsystem provenance

What is still missing is the later-stage routing lattice beyond first protected hold.

That gap is now already closed for the two first-route holdings that are runtime-real today:

- `RouteToCMoS`
- `RouteToCGoA`

And review posture is now separate:

- `DeferredReview`

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

- explicit `RouteToCMoS`
- explicit `RouteToCGoA`
- explicit `DeferredReview` as a separate review-state axis
- partial `discard_transient`
- absent `eligible_for_dream_seed`

So the collapse model is no longer purely aspirational, but it is still only partly named in runtime contracts.

Under the corrected Cryptic-first collapse reading:

- first protected collapse lands Cryptic-first into `cGoA` or `cMoS`
- review state is layered over that holding decision, not substituted for it
- the runtime does not yet explicitly distinguish later `cGoA` enrichment, discard, Dream eligibility, and later `MoS` promotion

## Recommended Next Cut

The next bounded implementation cut should not implement all five dispositions at once.

The strongest current reality has now been made explicit first:

1. `RouteToCMoS`
2. `RouteToCGoA`
3. `DeferredReview`

That keeps the runtime honest under the corrected model by naming first protected destinations before later Dream or `MoS` promotion.

The next qualification layer is now also explicit:

- route explains **where**
- evidence explains **why**
- review state explains **whether later adjudication is still required**

## Completion Condition

This conformance line is successful when the runtime can truthfully say, using explicit contract vocabulary, whether CME-formed substance is first being:

- routed into `cGoA`
- routed into `cMoS`
- held with `DeferredReview` or without review

and only later, after deeper curation, whether it is:

- promoted toward `MoS`
- marked Dream-eligible

and when the code only claims as explicit what it already truly performs.
