# SLI_ESCALATION_STATE_LAW

## Purpose

This note defines the first line-local escalation state law carried into
`V1.2.1`.

It exists so the sibling line does not treat `SLI` as loose routing,
decorative review language, or an untyped bridge between stewardship and
constitutional review.

Its job in this line is narrower than full runtime realization.

It fixes:

- the first bounded escalation states
- the first lawful transition grammar
- the `HitlHold` burden at state level
- the distinction between `Refusal`, `Quarantine`, and `GovernedReturn`

## Governing Compression

`SLI` escalation is lawful motion across standing layers, not generic routing.

The first line-local read is:

> `SLI` may transport provenance, admissibility, burden, and review posture
> across the install-first sibling line, but only through explicit state,
> jurisdiction, and witnessed transition law.

## Install-First Boundary

`V1.2.1` is not yet claiming the full live civic stack.

What is admitted now is:

- line-local escalation state doctrine
- a first passive escalation contract family
- a bounded transition policy

What remains withheld is:

- live mission escalation orchestration
- issued office release handling
- automatic `HitlHold` release
- fully bound governed-return landing

## First Line-Local State Family

The first state family now reads:

- `LocalResolve`
- `StewardReview`
- `StewardEscalate`
- `MotherFatherReview`
- `HitlHold`
- `Refusal`
- `Quarantine`
- `GovernedReturn`

These states are carried line-locally in:

- `src/SLI/SLI.Runtime/SLI.Runtime.csproj`
- `src/SLI/SLI.Runtime/EscalationStateContracts.cs`
- `src/SLI/SLI.Runtime/EscalationTransitionPolicy.cs`

## Transition Grammar

The first bounded transition grammar carried in this line is:

- `LocalResolve -> LocalResolve | StewardReview | HitlHold | Quarantine`
- `StewardReview -> LocalResolve | StewardEscalate | HitlHold | Quarantine | Refusal`
- `StewardEscalate -> MotherFatherReview | HitlHold`
- `MotherFatherReview -> GovernedReturn | HitlHold | Quarantine | Refusal`
- `HitlHold -> StewardReview | MotherFatherReview | Refusal | Quarantine | GovernedReturn`
- `Refusal -> GovernedReturn`
- `Quarantine -> StewardReview | MotherFatherReview | HitlHold | GovernedReturn`
- `GovernedReturn -> LocalResolve | StewardReview | Refusal | Quarantine`

The line-local guardrail is:

> `HitlHold` may not lawfully release itself.

## Refusal And Quarantine

`Refusal` and `Quarantine` remain distinct.

- `Refusal` is denied motion or non-admissibility.
- `Quarantine` is containment while admissibility remains unresolved or
  dangerous.

The sibling line may not flatten them into one generic stop state.

## Governed Return

`GovernedReturn` is not a generic callback.

It is the downward re-entry of reviewed standing.

Use:

- `SLI_GOVERNED_RETURN_RECEIPT_FAMILY_LAW.md`

when the question is what kind of reviewed receipt lands after state-lawful
return is issued.

## Hard Prohibitions

1. No direct `LocalResolve -> GovernedReturn`.
2. No direct `MotherFatherReview -> LocalResolve`.
3. No `HitlHold` release without explicit witnessed basis.
4. No silent collapse of `Refusal` into `Quarantine`.
5. No silent collapse of `Quarantine` into ordinary local motion.

## Implementation Consequences

This note now governs the first passive `SLI` state family in `V1.2.1`.

The carried batch is bounded on purpose.

It gives the sibling line:

- typed escalation states
- typed jurisdiction posture
- bounded transition requests and decisions
- a first fail-closed transition policy

It does not yet claim:

- full runtime state orchestration
- token sufficiency
- receipt-family landing

