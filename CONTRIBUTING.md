# Contributing

## Scope

This repository hosts the active `OAN Mortalis V1.1.1` engineering workspace.

Active implementation target:

- `OAN Mortalis V1.1.1/`

Reference-only material:

- `Build Contracts/`
- archived historical lines outside this repository


## Before You Change Anything

Read:

- `AGENTS.md`
- `README.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md`
- `OAN Mortalis V1.1.1/docs/PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md`
- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`
- `OAN Mortalis V1.1.1/docs/BUILD_READINESS.md`

## Local Verification

Use the canonical repo-root commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
```

These wrappers now run the workspace path hygiene preflight automatically.

Manual hygiene verification remains available when needed:

```powershell
powershell -ExecutionPolicy Bypass -File .\OAN Mortalis V1.1.1\tools\verify-private-corpus.ps1
```

## Contribution Rules

- make changes only in the active build unless the task explicitly requires otherwise
- edit `Build Contracts/` only for explicit governance work
- do not treat external archives as active build surfaces
- do not commit local absolute paths outside the repository root
- do not commit private corpus paths, credentials, runtime payloads, or machine-local artifacts
- keep changes scoped to one technical concern where possible
- state what the contribution does not claim when it touches identity,
  authority, legal posture, `CME` standing, installer readiness, hosted seed
  `LLM` behavior, or exploratory doctrine
- do not present exploratory, documentation-only, or contract-backed language
  as current executable truth

## Public Entry Boundary

Contribution is an entry path into a governed engineering workspace, not a
grant of identity, custody, governance authority, legal authority, `CME`
standing, installer completion, or certainty beyond evidence.

Contribution authority remains scoped through:

```text
Domain -> Role -> Capacity
```

Opening an issue grants no write capacity.
Opening a pull request grants no merge authority.
Review approval does not create governance law.
Stack vocabulary does not create ontology.

See `OAN Mortalis V1.1.1/docs/PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md` for
the public onboarding boundary that governs issue, pull request, review, and
external-analysis entry.

## Pull Requests

Each pull request should state:

- what changed
- why it changed
- which architecture layer is affected
- how it was verified
- whether there are deployment or configuration impacts
- what the change does not claim when non-claim boundaries are relevant

## Issues

Use the issue templates for:

- bugs
- feature requests

If the problem is a security issue, do not open a public issue. Follow `SECURITY.md`.

## Maintainer Contact

For repository administration, contribution routing, or maintainer-side
coordination that should not start as a public issue, contact:

- `admin@lucidtechnologies.tech`
