# LISP_CSHARP_BINDING_SCHEMA

## Purpose

This note defines the first doctrine-only carrier schema between the current
Lisp and C# seam nouns already admitted in `V1.2.1`.

It exists to preserve lawful standing, refusal visibility, and receipt
legibility across language surfaces without granting runtime motion.

> This schema governs carriage, not consequence.

## Governing Read

Use this note with:

- `SLI_ENGINE_LISP_BINDING_CONTRACT.md`
- `SEAM_REFUSAL_AND_RETURN_LAW.md`
- `SANCTUARYID_RTME_SKELETON.md`
- `FIRST_WORKING_MODEL_TRACE_PATH.md`
- `SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md`

## Scope

This schema is restricted to already admitted seam nouns only.

The current carriers in scope are:

- `San.Common.SymbolicEnvelope`
- `San.Common.MembraneDecision`
- `SLI.Engine.PredicateLandingRequest`
- `SLI.Engine.CrypticFloorEvaluation`
- `SLI.Lisp.HostedCrypticLispBundleReceipt`
- `SLI.Runtime.RecompositionCandidate`
- `SLI.Runtime.RecompositionCandidateEvaluationDecision`
- seam refusal receipt as defined conceptually in
  `SEAM_REFUSAL_AND_RETURN_LAW.md`

The current line still does not materialize a separate
`San.Common.MembraneDecisionResult`.
This schema therefore remains faithful to the actually carried
`San.Common.MembraneDecision` surface instead of teaching a tidier future noun.

## Preservation-Critical Fields

### Symbolic Passport And Decision Standing

The following `SymbolicEnvelope` fields are identity-bearing and may not be
dropped, inferred, or renamed into ambiguity:

- `Origin`
- `Family`
- `ProductClass`
- `Intent`
- `Admissibility`
- `ContradictionState`
- `MaterializationEligibility`
- `PersistenceEligibility`
- `TraceId`

`MembraneDecision` is itself preservation-critical standing.

Its current value must remain explicit as one of:

- `Accept`
- `Transform`
- `Defer`
- `Refuse`
- `Collapse`

### Seam Carrier And Engine Witness

The following `PredicateLandingRequest` fields are preservation-critical:

- `Envelope`
- `MembraneDecision`
- `SanctuaryGelHandle`
- `IssuedRtmeHandle`
- `RouteHandle`
- `RouteKind`

The following `CrypticFloorEvaluation` fields are preservation-critical:

- `PredicateLandingReady`
- `Disposition`
- `OutcomeCode`
- `GovernanceTrace`
- `Envelope`

### Hosted Receipt Standing

The following `HostedCrypticLispBundleReceipt` fields are receipt-bearing and
must remain legible:

- `BundleHandle`
- `BundleProfile`
- `HostedByIssuedRuntime`
- `CrypticCarrierKind`
- `InterconnectProfile`
- `ModuleNames`
- `HostedExecutionOnly`
- `CanonicalFloorSetReady`
- `TimestampUtc`

### Candidate And Evaluation Standing

The following `RecompositionCandidate` fields remain standing-bearing for seam
approach:

- `CandidateId`
- `QueryId`
- `Disposition`
- `Sources`
- `RequiresMembraneReentry`
- `CreatedAtUtc`

Within `Sources`, the following provenance fields remain preservation-critical:

- `ProductId`
- `ReceiptId`
- `WitnessSnapshotId`
- `SourceTraceId`
- `Lane`
- `Family`
- `ProductClass`
- `Admissibility`
- `ContradictionState`
- `ReceivedAtUtc`

The following `RecompositionCandidateEvaluationDecision` fields are
preservation-critical:

- `Outcome`
- `BurdenEvaluations`
- `OutcomeCode`
- `GovernanceTrace`
- `EligibleForLaterOperatorRealization`
- `RequiresFurtherCleaveReview`
- `EvaluatedAtUtc`

### Refusal And Return Continuity

The conceptual refusal receipt defined by current seam law must preserve:

- refusal stage
- refusal code
- trace id
- source office
- return destination

## Allowed Translation Operations

Lawful translation may do only the following:

- map Lisp keyword carriers into C# named record or property carriers when the
  source noun remains explicit
- adapt list or vector surfaces into `IReadOnlyList<T>` when element meaning is
  preserved exactly
- adapt field ordering when the carrier remains a named field carrier rather
  than a positional reinterpretation
- preserve explicit optional-handle absence as explicit absence when that
  absence was already present in the source
- carry `Transform` only as an explicit membrane decision or as an explicitly
  re-passported envelope after lawful membrane work

Allowed translation may shape.
It may not reinterpret.

## Forbidden Translation Operations

The schema forbids:

- inventing `San.Common.MembraneDecisionResult`
- silently inferring missing passport fields
- silently inferring missing handles
- collapsing refusal into `null`, `false`, empty collections, or opaque host
  exceptions
- substituting generic success/failure for `OutcomeCode` or `GovernanceTrace`
- host-side widening of `CandidateOnly`, `RetainCandidate`, or
  `EligibleForLaterOperatorRealization` into runtime permission
- treating hosted module contents as engine-side carrier material
- outcome substitution by convenience

## Refusal Visibility And Receipt Continuity

Refusal and withhold must remain visible across the membrane.

That means:

- a Lisp-side refusal may not become host-side absence
- a C#-side refusal may not become Lisp-side non-response
- `CrypticFloorEvaluation` with `Disposition == Refuse` or
  `Disposition == Withhold` remains a named bounded outcome
- `HostedCrypticLispBundleReceipt` remains a named receipt and may not be
  flattened into generic success
- seam refusal receipt remains a named refusal and may not be flattened into
  generic failure noise

A receipted path in Lisp must remain receipted and legible in C#.
A refusal in Lisp must remain visible as refusal in C#.

## Explicit Non-Grants

This schema does not authorize:

- execution
- realization
- operator motion
- `RTME` wake
- admission expansion
- host inference
- service binding
- semantic widening by convenience

It does not create new result types, new runtime offices, or new powers.

## Working Summary

The seam nouns now have a documented carrier mapping.
Their identity-bearing and receipt-bearing fields are named.
Refusal remains visible across translation.
Nothing in this schema can be read as new runtime permission.
