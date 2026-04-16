# Public Release Artifact Wording Template

## Purpose

This document defines the public wording template for release notes, tag
descriptions, milestone summaries, release-candidate summaries, and changelog
entries.

It applies the public release readiness wording law to actual public release
artifacts.

Release artifacts are witness summaries.
They are not promotion engines.

## Governing Inputs

Use this template with:

- `PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md`
- `PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md`
- `PUBLIC_RELEASE_READINESS_WORDING_LAW.md`
- `PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md`
- `PUBLIC_CME_EXPLANATION_BOUNDARY.md`
- `FIRST_WORKING_MODEL_RELEASE_GATE.md`

Public release artifacts must preserve `pass`, `hold`, and `fail` gate
grammar when they describe readiness, installer state, first-working-model
status, `CME` standing, or public operational posture.

No "mostly ready" reading is lawful release artifact wording.

## Required Release Artifact Sections

Every public release note or tag description that makes a readiness or
architecture claim should include:

- `Status`
- `Evidence`
- `Architecture Layer`
- `What Changed`
- `What This Does Not Claim`
- `Known Holds`
- `Verification`

Short tags may compress these sections, but they must still preserve status,
evidence, non-claims, and holds.

## Status

State the gate posture plainly:

```text
Status: pass | hold | fail
```

Use `hold` when the release artifact is evidence-bearing but one or more
required seams remain intentionally incomplete.

Do not use "mostly ready", "basically complete", "production ready", or
"installer complete" unless the relevant gate has passed for that exact claim.

## Evidence

State the evidence class supporting the artifact:

- documentation seat
- contract seat
- runtime witness
- audit witness
- substrate support
- public explanation
- release-candidate evidence bundle
- build, test, and hygiene evidence

Evidence class must not be flattened into completion.

A documentation seat is not runtime witness.
A contract seat is not live enactment.
A runtime witness is not installer completion.
A substrate support is not `CME` minting.
A public explanation is not ontology.
A release-candidate evidence bundle is not production readiness by itself.

## Architecture Layer

Name the affected layer:

- Sanctuary
- CradleTek
- SoulFrame
- AgentiCore
- SLI
- GEL
- Hosted LLM
- Public Boundary
- Infrastructure
- Crosscutting

This layer name is descriptive.
It does not grant governance authority.

## What Changed

Describe the change in concrete terms:

- docs changed
- contracts changed
- tests or witnesses changed
- runtime behavior changed
- release tooling changed
- public boundary changed

Avoid language that turns a narrow change into whole-system completion.

## What This Does Not Claim

Release artifacts must explicitly preserve non-claims when the artifact touches
`CME`, Prime, runtime, legal, readiness, installer, governance, custody, or
public-boundary language.

Use the following checklist when relevant:

- does not mint `CME` standing
- does not grant personhood or legal personhood
- does not grant autonomous authority
- does not grant legal accountability
- does not grant custody authority
- does not grant governance authority
- does not enact live role authority
- does not complete the public installer
- does not ship the hosted seed `LLM` in the public checkout
- does not convert release-candidate evidence into production readiness
- does not claim certainty beyond current evidence

## Known Holds

Name withheld seams plainly.

Examples:

- hosted seed `LLM` runtime is not shipped in the public checkout
- fully operational installer is still being built
- first-working-model gate remains `hold`
- `CME` explanation is not minting
- founding is not minting
- embodiment is not persona
- return is not promotion
- legal-orientation templates are not legal advice or legal personhood

Known holds are not defects.
They are truthful status boundaries.

## Verification

List only verification that actually ran or evidence that actually exists.

Allowed forms include:

- focused test or audit witness
- full build wrapper
- full test wrapper
- private-corpus hygiene check
- release-candidate evidence bundle
- cited documentation law
- reproduction steps

Do not use planned verification as completed verification.

## Minimal Public Release Note Skeleton

```markdown
# Release / Tag: <name>

## Status

Status: hold

## Evidence

- Evidence class:
- Release gate:
- Build/test/hygiene:

## Architecture Layer

- Layer:
- Stratum:

## What Changed

-

## What This Does Not Claim

-

## Known Holds

-

## Verification

-
```

## Boundary Invariant

Public release artifacts must preserve this rule:

Release wording is not promotion.

No release note, tag description, milestone summary, release-candidate summary,
or changelog entry may imply more identity, authority, readiness, or certainty
than the internal system can lawfully support.

No external statement may imply more identity, authority, or certainty than the internal system can lawfully support.
