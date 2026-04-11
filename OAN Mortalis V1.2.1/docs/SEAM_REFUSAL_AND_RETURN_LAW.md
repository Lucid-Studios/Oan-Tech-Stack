# SEAM_REFUSAL_AND_RETURN_LAW

## Purpose

This note defines refusal and return law at the named
`SLI.Engine -> SLI.Lisp -> SanctuaryID.RTME` seam.

Its job is to make refusal structured, observable, and receipted before any
later admission shell or binding schema appears.

## Governing Compression

The seam must be able to say no.

That refusal may not:

- disappear silently
- widen authority
- imply failure of the whole line
- masquerade as a successful binding attempt

Refusal is a lawful result.
It is not a missing event.

## Refusal Stages

The seam now carries three refusal or withhold stages.

### Pre-Binding Refusal

Pre-binding refusal occurs before a lawful
`SLI.Engine.PredicateLandingRequest` exists.

Typical reasons include:

- invalid or incomplete passport
- disallowed membrane decision
- disallowed `ProductClass`
- missing lineage or source trace needed to form a lawful carrier

At this stage, refusal prevents seam approach entirely.

### Engine-Entry Refusal Or Withhold

Engine-entry refusal or withhold occurs after a
`PredicateLandingRequest` exists but before the seam is lawfully passed
forward.

Typical reasons include:

- missing `SanctuaryGelHandle`
- missing `IssuedRtmeHandle`
- missing `RouteHandle`
- route-kind mismatch
- refusal or withhold from engine-side admissibility review

At this stage, `SLI.Engine.CrypticFloorEvaluation` is the current bounded
engine-side witness surface.

### Hosted-Bundle Or `RTME`-Side Refusal Or Withhold

Hosted-bundle or `RTME`-side refusal or withhold occurs after engine-side
readiness has been considered but before lawful hosted residency or later
issued-runtime admission can be claimed.

Typical reasons include:

- canonical floor-set not ready
- hosted bundle profile incompatibility
- bundle handle or issued-runtime posture inconsistent with the request

At this stage, `SLI.Lisp.HostedCrypticLispBundleReceipt` is the current bounded
hosted-side witness surface.

## Observable And Receipted Refusal

No refusal at this seam may be a silent drop.

Every refusal or withhold must be:

- observable
- attributable to a stage
- attributable to a reason class
- returnable to witness and gate surfaces

The current line does not yet materialize a dedicated refusal receipt type.
It fixes the minimum future receipt burden now so later implementation cannot
silently improvise it.

## Minimum Refusal Receipt Fields

Any later refusal receipt at this seam must minimally preserve:

- refusal stage
- refusal code
- trace id
- source office
- return destination

These are conceptual minimums for future implementation.
They are not optional decoration.

## Return Law

Seam refusal returns to witness and gate surfaces.

It does not return as:

- silent disappearance
- automatic retry without witness
- continuity widening
- operator ingress by convenience

The return destination must remain legible enough that the refusal can be seen
by later witness and admissibility surfaces rather than being buried inside a
private engine or hosted-runtime pocket.

## Working Summary

`V1.2.1` now fixes refusal law at the named binding seam in three stages:

- pre-binding refusal
- engine-entry refusal or withhold
- hosted-bundle or `RTME`-side refusal or withhold

The current read is:

- refusal must be observable and receipted
- no silent drop is lawful here
- refusal returns to witness and gate surfaces
- refusal does not widen standing
