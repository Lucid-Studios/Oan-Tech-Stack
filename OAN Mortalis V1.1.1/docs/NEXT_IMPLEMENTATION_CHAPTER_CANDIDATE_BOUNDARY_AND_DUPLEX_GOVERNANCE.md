# NEXT_IMPLEMENTATION_CHAPTER_CANDIDATE_BOUNDARY_AND_DUPLEX_GOVERNANCE

Status: Proposed - Structural Chapter Candidate

## Purpose

Advance the governed pre-domain host loop beyond checkpointing by:

- constraining Lisp-originated outputs to `candidate-only` contracts
- separating whole candidates into `Prime-governed` and
  `Cryptic-governed` surfaces
- evaluating whether any separated candidate is eligible to move toward
  `domain-bearing participation`

This chapter exists to preserve the core invariant:

> the host must inspect holistically, then separate what belongs to Prime
> governance from what belongs to Cryptic governance before admission may even
> be considered

## Current Achieved State

After `6859b58`, the runtime possesses a first governed pre-domain host loop.

Current properties:

- `C#` owns the pre-domain checkpoint loop
- `Lisp` remains proposal-shaping only
- the host can:
  - inspect cryptic holding
  - run `form-or-cleave`
  - emit receipted carry/collapse outcomes
- the runtime gates on `PrimeSeedPreDomainStanding`
- the running slice is re-rooted to `San.*` in the touched Sanctuary body

This means the host can now checkpoint candidate cognition without allowing
the cryptic side to self-promote into authority.

## What Is Still Missing

The current host loop can checkpoint a candidate body, but it cannot yet:

- formally constrain Lisp outputs to candidate/suggestion/observation surfaces
- inspect a candidate as one whole and then split it into lawful duplex
  governance surfaces
- decide whether any separated candidate is eligible to move toward admitted
  domain-bearing participation

That missing mechanics layer is the focus of this chapter.

## Chapter Order

This chapter proceeds in three required descents:

1. `candidate-only proposal boundary`
2. `whole-to-duplex separation`
3. `pre-domain admission gate`

This order is mandatory.

Admission must not be reasoned over an unsplit candidate body.

## 1. Candidate-Only Proposal Boundary

### Intent

Make it impossible, at the type and contract level, for Lisp-originated outputs
to arrive as authority-bearing material.

### Lisp May Provide

- candidate proposals
- cryptic holding mutation proposals
- resonance observations
- descendant proposals
- collapse suggestions

### Lisp May Not Provide

- standing assertions
- permission validity
- domain admission
- role binding
- final action disposition

### Candidate Contract Family

Initial contract family to seat:

- `IGovernedSeedCandidateProposal`
- `IGovernedSeedCrypticHoldingMutationProposal`
- `IGovernedSeedResonanceObservation`
- `IGovernedSeedDescendantProposal`
- `IGovernedSeedCollapseSuggestion`

Shared envelope:

- `GovernedSeedCandidateEnvelope`

### Invariant

Lisp-originated outputs must always enter the host as:

- candidate
- observation
- suggestion
- proposal

Never as:

- permission
- standing
- action
- admission
- authority

## 2. Whole-to-Duplex Separation

### Intent

Teach the host to receive a candidate as one pre-domain body, inspect it
holistically, and then separate it into:

- `Prime-governed surface`
- `Cryptic-governed surface`

### Why This Exists

A candidate may contain, in one body:

- invariant-relevant material
- admission-relevant material
- responsibility-bearing implications
- unfinished cryptic formation
- resonance findings
- partial descendants
- trace-form residue

These must not remain collapsed into one unsplit candidate object once the host
has inspected them.

### Prime Side Carries

- invariant-relevant concerns
- admission-relevant structure
- role/domain gating implications
- responsibility-bearing burden markers
- governance-bearing material

### Cryptic Side Carries

- unfinished thought
- resonance groupings
- partial-form candidates
- bloom-form residues
- trace-form residues
- hold-worthy but not-yet-admissible constructs

### Separation Contracts

- `GovernedSeedPrimeCandidateView`
- `GovernedSeedCrypticCandidateView`
- `GovernedSeedCandidateSeparationAssessment`
- `PrimeCrypticDuplexGovernanceReceipt`

### Separation Service

- `GovernedSeedCandidateSeparationService`

### Invariant

Cryptic-bearing material must never inherit Prime authority by proximity.

Prime-bearing material must never flatten unfinished cognition prematurely.

## 3. Pre-Domain Admission Gate

### Intent

After whole-to-duplex separation, determine whether any separated candidate is
eligible to move toward admitted domain-bearing participation.

### This Gate May Result In

- remain pre-domain
- carry cryptic-only
- refuse
- prepare for domain/role gating

### Admission Gate Contracts

- `PrimeSeedPreDomainAdmissionAssessment`
- `DomainRoleEligibilityAssessment`
- `PreDomainAdmissionGateReceipt`

### Admission Gate Service

- `PrimeSeedPreDomainAdmissionGateService`

### Gate Criteria

At minimum, the admission gate must ensure:

- Prime compliance remains intact
- no cryptic-originated authority bleed exists
- role/domain prerequisites are either satisfied or explicitly absent
- permission is still candidate-only until lawfully advanced
- responsibility can be attributed if the candidate progresses

### Invariant

Admission may only be reasoned over duplex-separated material.

## New Receipts

This chapter introduces a new receipt family so the split remains auditable.

Target receipts:

- `GovernedSeedCandidateBoundaryReceipt`
- `GovernedSeedCandidateSeparationReceipt`
- `PrimeCrypticDuplexGovernanceReceipt`
- `PrimeSeedPreDomainAdmissionGateReceipt`

Each receipt should preserve:

- source candidate reference
- current standing reference
- separation basis
- retained Prime material summary
- retained Cryptic material summary
- refused residue summary
- next disposition

## Suggested Ownership

### `San.Common`

Owns:

- contracts
- enums
- receipts
- assessments

### `SLI.Engine`

Owns:

- candidate separation service
- cryptic proposal handling utilities
- resonance observation handling

### `San.Nexus.Control`

Owns:

- pre-domain admission gate
- orchestration updates to the governed host loop

### `San.Runtime.Materialization`

Owns:

- materialization of duplex and admission receipts into runtime-facing surfaces

## Host Loop Extension

The current host loop performs:

- holding inspection
- `form-or-cleave`
- carry/collapse receipting

The next host loop should perform:

1. receive candidate envelope
2. verify pre-domain standing
3. inspect cryptic holding
4. run `form-or-cleave`
5. run whole-to-duplex separation
6. run pre-domain admission gate
7. emit:
   - boundary receipt
   - separation receipt
   - duplex receipt
   - admission gate receipt
8. choose disposition:
   - remain pre-domain
   - carry cryptic-only
   - refuse
   - prepare for next domain/role gate

## First Files to Implement

Initial file skeleton:

- `GovernedSeedCandidateBoundaryContracts.cs`
- `GovernedSeedCandidateSeparationContracts.cs`
- `GovernedSeedCandidateSeparationService.cs`
- `PrimeSeedPreDomainAdmissionGateContracts.cs`
- `PrimeSeedPreDomainAdmissionGateService.cs`

## Witness Plan

### Candidate Boundary Tests

- Lisp-originated proposals cannot set standing
- Lisp-originated proposals cannot set permission validity
- Lisp-originated proposals cannot bind domain or role
- Lisp-originated proposals cannot set final action disposition

### Duplex Separation Tests

- mixed candidate input separates into Prime and Cryptic views correctly
- cryptic-only residues do not appear on the Prime side
- prime-relevant burden markers survive separation
- no authority-bearing fields are derived from cryptic-only suggestion surfaces

### Admission Gate Tests

- unsplit candidate cannot be admitted
- duplex-separated candidate with insufficient Prime surface remains pre-domain
- cryptic-rich but authority-empty candidate is carried, not promoted
- only properly separated and standing-consistent candidates may be prepared for
  next domain/role gating

## Core Invariant Of The Chapter

> The host must inspect holistically, then separate what belongs to Prime
> governance from what belongs to Cryptic governance before admission may even
> be considered.

## Outcome Target

At the end of this chapter, the governed host should be able to:

- receive a candidate as one pre-domain body
- prevent proposal surfaces from self-promoting into authority
- split that candidate into Prime and Cryptic governance surfaces lawfully
- receipt the split explicitly
- determine whether the candidate remains pre-domain or may move toward
  domain-bearing participation

## Shortest Compression

> The next build step is to make proposal boundaries explicit, split whole
> candidates into Prime and Cryptic governance surfaces, and only then allow
> admission reasoning to begin.
