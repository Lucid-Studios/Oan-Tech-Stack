# Contributing

## Scope

This repository hosts the active `OAN Mortalis V1.0` engineering workspace.

Active implementation target:

- `OAN Mortalis V1.0/`

Reference-only material:

- `Build Contracts/`
- `OAN Mortalis V0.1 Archive/`

## Before You Change Anything

Read:

- `AGENTS.md`
- `OAN Mortalis V1.0/docs/WORKSPACE_RULES.md`
- `OAN Mortalis V1.0/docs/BUILD_READINESS.md`

## Local Verification

Use the canonical repo-root commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
```

These wrappers now run the workspace path hygiene preflight automatically.

Manual hygiene verification remains available when needed:

```powershell
powershell -ExecutionPolicy Bypass -File .\OAN Mortalis V1.0\tools\verify-private-corpus.ps1
```

## Contribution Rules

- make changes only in the active build unless the task explicitly requires otherwise
- do not edit `Build Contracts/`
- do not edit `OAN Mortalis V0.1 Archive/`
- do not commit local absolute paths outside the repository root
- do not commit private corpus paths, credentials, runtime payloads, or machine-local artifacts
- keep changes scoped to one technical concern where possible

## Pull Requests

Each pull request should state:

- what changed
- why it changed
- which architecture layer is affected
- how it was verified
- whether there are deployment or configuration impacts

## Issues

Use the issue templates for:

- bugs
- feature requests

If the problem is a security issue, do not open a public issue. Follow `SECURITY.md`.
