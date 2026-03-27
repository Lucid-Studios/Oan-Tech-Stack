# WORKSPACE_RULES

This sibling build line follows the canonical workspace rules defined in:

- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`

## V1.1.1 Interpretation

For `OAN Mortalis V1.1.1`, the rules are:

1. `Build Contracts/` remains the root of truth for architecture, naming, layering, governance, persistence, and determinism.
2. `OAN Mortalis V1.1.1/` is a curated rebuild line with its own folder-local build surface.
3. Other versioned build lines remain outside this line's normal write boundary unless an explicit migration or critical-fix task says otherwise.

## Write Boundary

Allowed write surface:

- `OAN Mortalis V1.1.1/**`

Forbidden write surface:

- `Build Contracts/**`
- `OAN Mortalis V0.1 Archive/**`
- any external path outside the repository root

## Build Surface

Use the line-local entrypoints from this folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
```

Run the line-local hygiene surface before publishing changes:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\verify-private-corpus.ps1
```

## Private Corpus Discipline

Foundational private corpus material may be indexed from a local external root, including:

- the Lucid Technologies foundational publications corpus

That root must remain:

- local-only
- ignored by git
- absent from tracked managed files
