# WORKSPACE_RULES

This active build follows the canonical workspace rules defined in:

- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`

## Active Build Interpretation

For `OAN Mortalis V1.0`, the rules are:

1. `Build Contracts/` is the root of truth for architecture, naming, layering, governance, persistence, and determinism.
2. `OAN Mortalis V0.1 Archive/` is read-only reference material only.
3. `OAN Mortalis V1.0/` is the only active build target.

## Write Boundary

Allowed write surface:

- `OAN Mortalis V1.0/**`

Forbidden write surface:

- `Build Contracts/**`
- `OAN Mortalis V0.1 Archive/**`
- any external path outside the repository root

## External Governing Documentation Surface

The `Documentation Repo` is an active governing documentation surface for the OAN build cycle, but it is not part of this repository's allowed write boundary.

In this workspace:

- the active build repo remains the executable source of present truth
- the `Documentation Repo` may be consulted as stabilized conceptual truth and theory digestion
- governed build work should ground itself in both surfaces when the `Documentation Repo` is available
- tracked files in this repo must reference that external surface only by logical label, never by machine-specific absolute path

Unless explicitly directed, work in this repository must not write to the external documentation surface.

## Local-Only Foundations

Foundational private corpus material may be indexed from a local external root, including:

- the Lucid Technologies foundational publications corpus

That root must remain:

- local-only
- ignored by git
- absent from tracked managed files

Use the active leak-scan tool before commit or publish:

```powershell
powershell -ExecutionPolicy Bypass -File tools\verify-private-corpus.ps1
```
