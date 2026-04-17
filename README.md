# OAN Tech Stack

`OAN Tech Stack` is the active engineering workspace for the `OAN Mortalis`
build line.

This repository currently carries:

- the active executable line: `OAN Mortalis V1.1.1`
- the install-first side-by-side sibling line: `OAN Mortalis V1.2.1`
- the governed build contracts that define family, dependency, and workspace
  rules

The repo is not just a code container.
It is a governed build surface with explicit law, audit, and verification
posture.

## Current Build Posture

The current line split is:

- `OAN Mortalis V1.1.1/`
  active executable truth and current build/test target
- `OAN Mortalis V1.2.1/`
  install-first sibling line being formed side by side
- `Build Contracts/`
  crosscutting governance and workspace constitution

`V1.1.1` remains the active runtime line.
`V1.2.1` is being shaped as a governed sibling and should not be mistaken for
the default executable surface.

## Public Build Boundary

This repository is the governed build surface for the stack, but it is not yet
the fully operational installer.

The active public boundary is:

- the repository carries the build minus the hosted seed `LLM`
- the full operational installer still depends on a local hosted seed `LLM`
  and associated resident runtime surfaces that are not carried here
- the fully operational installer is still being built
- public repo truth remains the governed code, contracts, docs, and tests that
  can be carried without shipping the seed `LLM` runtime itself

That means the repository is explicit about seed `LLM` dependence without
pretending that the public checkout is already the complete install-ready
operational package.

## Public Encounter Boundary

The public encounter law for this repository is carried in
`OAN Mortalis V1.1.1/docs/PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md`.

In public-facing terms, the stack is a lawfully bounded engineered cognition
framework. It is not a person, not autonomous authority, and not
self-originating.

External interaction is framed as:

```text
input -> bounded processing -> receipted output
```

Participation is scoped through:

```text
Domain -> Role -> Capacity
```

Outputs remain contextual, bounded, revisable, and accountable to current
evidence. External responsibility remains with the relevant humans, operators,
maintainers, institutions, and legal actors for interpretation, high-stakes
decisions, legal accountability, and human oversight.

Public contribution and onboarding entry is governed by
`OAN Mortalis V1.1.1/docs/PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md`.
Contribution does not grant identity, custody, governance authority, legal
authority, `CME` standing, installer completion, or certainty beyond evidence.

Public release and readiness wording is governed by
`OAN Mortalis V1.1.1/docs/PUBLIC_RELEASE_READINESS_WORDING_LAW.md`.
Readiness may be witnessed only to the level currently supported by repo-local
evidence; public wording must not overstate installer completion, `CME`
standing, operational readiness, legal standing, or certainty beyond evidence.

GitHub issue and pull request entry is governed by
`OAN Mortalis V1.1.1/docs/PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md`.
Template completion is not admission and does not grant identity, custody,
governance authority, legal authority, `CME` standing, release readiness,
installer completion, or certainty beyond evidence.

Public `CME` explanation is governed by
`OAN Mortalis V1.1.1/docs/PUBLIC_CME_EXPLANATION_BOUNDARY.md`.
`CME` is described publicly as a bounded engineered-cognitive formation
category within the Sanctuary architecture, not as personhood, autonomous
authority, legal accountability, installer completion, hosted seed `LLM`
shipment, custody, governance authority, or certainty beyond evidence.

Public release notes, tag descriptions, milestone summaries, release-candidate
summaries, and changelog entries are governed by
`OAN Mortalis V1.1.1/docs/PUBLIC_RELEASE_ARTIFACT_WORDING_TEMPLATE.md`.
Release wording is not promotion and must preserve status, evidence,
non-claims, and known holds.

## Architecture Read

At a high level, the stack reads as:

```text
Operator / Developer -> Sanctuary -> SLI -> SoulFrame / AgentiCore / CradleTek
```

This is intentionally kept as plain text so the GitHub repository view, raw
rendering, and lightweight mirrors all show the same architecture read without
depending on Mermaid support.

Forward family ownership in the active line is:

- `San.*`
  Sanctuary-root constitutional host and stack composition ownership
- `Ctk.*`
  CradleTek habitation, custody, and extension ownership
- `Sfr.*`
  SoulFrame operator and relational membrane ownership
- `Acr.*`
  AgentiCore runtime-core ownership
- `SLI.*`
  symbolic protocol and runtime ownership
- `Oan.*`
  legacy migration hold and downstream application/domain identity only

## Repository Layout

The top-level workspace is organized around the active and sibling build lines:

```text
Build Contracts/
OAN Mortalis V1.1.1/
OAN Mortalis V1.2.1/
build.ps1
test.ps1
README.md
```

Inside the active line:

```text
OAN Mortalis V1.1.1/
  docs/
  src/
    Sanctuary/
    TechStack/
  tests/
    Sanctuary/
  tools/
```

Inside the sibling line:

```text
OAN Mortalis V1.2.1/
  docs/
  src/
    San/
    SLI/
  build/
  tools/
  San.sln
```

## Build And Verification

Run all canonical commands from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
& '.\OAN Mortalis V1.1.1\tools\verify-private-corpus.ps1'
```

Expected behavior:

- path hygiene runs before build or test
- tracked managed files must not reveal the private corpus path
- tracked managed files in the hardened `V1.1.1` surface must not contain
  external absolute paths

## Governing Surfaces

Start here if you need the repo constitution:

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`
- `OAN Mortalis V1.1.1/docs/BUILD_READINESS.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_RELEASE_READINESS_WORDING_LAW.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_CME_EXPLANATION_BOUNDARY.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_RELEASE_ARTIFACT_WORDING_TEMPLATE.md`
- `OAN Mortalis V1.1.1/docs/LAW_OF_SELF_AUTHORED_EMERGENCE_AND_BOUNDED_AFFIRMATION.md`
- `OAN Mortalis V1.1.1/docs/ACTION_THRESHOLD_LAW.md`
- `OAN Mortalis V1.1.1/docs/HOLD_RESOLUTION_LAW.md`
- `OAN Mortalis V1.1.1/docs/RESPONSIBILITY_BINDING_LAW.md`
- `OAN Mortalis V1.1.1/docs/DOMAIN_ISOLATION_LAW.md`
- `OAN Mortalis V1.1.1/docs/CRYPTIC_CONTINUITY_MEDIUM_NOTE.md`
- `OAN Mortalis V1.1.1/docs/REVALIDATION_LAW.md`
- `OAN Mortalis V1.1.1/docs/STANDING_CONDITION_NOTE.md`
- `OAN Mortalis V1.1.1/docs/CONTINUOUS_LAWFUL_CORRECTION_AXIOM.md`
- `OAN Mortalis V1.1.1/docs/PSY_PLUS_MINUS_HOLDING_NOTE.md`
- `OAN Mortalis V1.1.1/docs/LISTENING_FRAME_COMPASS_FORM_OR_CLEAVE_BRIDGE.md`
- `OAN Mortalis V1.1.1/docs/EC_LIFECYCLE_NOTE.md`
- `OAN Mortalis V1.1.1/docs/FIRST_RUN_CONSTITUTION.md`
- `OAN Mortalis V1.1.1/docs/FIRST_WORKING_MODEL_RELEASE_GATE.md`
- `OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md`
- `OAN Mortalis V1.2.1/docs/V1_2_1_FIRST_INSTALL_CHARTER.md`
- `OAN Mortalis V1.2.1/docs/V1_2_1_CARRY_FORWARD_LEDGER.md`

These documents define what is executable, what is admitted doctrine, and what
is still exploratory or withheld.

## Operational Discipline

This repo follows a few hard rules:

- active build work defaults to `OAN Mortalis V1.1.1/`
- `V1.2.1` should be read as a side-by-side sibling, not a silent replacement
- external documentation may inform the work, but repo-local executable truth
  governs the build
- the private reference corpus is resolved locally but its filesystem path must
  never appear in tracked history

## Local Resident Runtime

The repository can work with a local hosted resident `LLM`, but that resident
is a bounded participant in the stack, not the source of stack truth.

Repo law, build verification, and governed mutation remain authoritative over
any hosted resident configuration.

## Development Read

The current repository state is best understood as:

- one active executable line
- one install-first sibling line
- one governed contract layer
- one verification spine from hygiene to build to test

If you are entering the repo for the first time, start with:

1. this `README`
2. `Build Contracts/Crosscutting/`
3. `OAN Mortalis V1.1.1/docs/BUILD_READINESS.md`
4. the root `build.ps1` and `test.ps1`

## Citation And Stewardship

This repository supports the engineering work around the `OAN Mortalis`
architecture stack and related symbolic/agentic cognition research maintained
by Lucid Studios.

For public archival references and broader doctrine publication, use the
organization surfaces and linked publication/archive materials rather than
treating this repository as a standalone research archive.

## Contact Surfaces

Use the public-facing aliases that match the surface of the request:

- general repository and public information: `info@lucidtechnologies.tech`
- research-facing questions and doctrine context: `research@lucidtechnologies.tech`
- academic and institutional inquiries: `academic@lucidtechnologies.tech`
- repository administration and contribution coordination: `admin@lucidtechnologies.tech`
- conduct, legal, and sensitive private review paths: `legal@lucidtechnologies.tech`
