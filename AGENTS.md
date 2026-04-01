# AGENTS.md

## Mission

This repository is the active engineering workspace for `OAN Mortalis V1.1.1`.

Treat it as:

- the source of truth for active build and test work
- a governed workspace with explicit family, naming, and dependency contracts
- a repository that may consult a private local reference corpus without ever exposing its filesystem path in tracked history

## Workspace Scope

Primary active surface:

- `OAN Mortalis V1.1.1/`

Reference governance:

- `Build Contracts/`

Archived historical lines:

- historical lines now live outside this repository
- do not treat external archives as active build surfaces

Do not treat any external archive as an editable active build surface unless explicitly directed.

## Architectural Constitution

Follow these governing documents before inventing new structure:

- `Build Contracts/Crosscutting/FAMILY_CONSTITUTION.md`
- `Build Contracts/Crosscutting/GLOSSARY_CONTRACT.md`
- `Build Contracts/Crosscutting/DEPENDENCY_CONTRACT.md`
- `OAN Mortalis V1.1.1/docs/BUILD_READINESS.md`
- `OAN Mortalis V1.1.1/docs/V1_1_1_CARRY_FORWARD_LEDGER.md`
- `OAN Mortalis V1.1.1/docs/V1_0_RETIREMENT_GATE.md`
- `OAN Mortalis V1.1.1/docs/GOVERNED_BUILD_AUTOMATION_CONVEYOR.md`
- `OAN Mortalis V1.1.1/docs/LOCAL_AUTOMATION_TASKING_SURFACE.md`
- `OAN Mortalis V1.1.1/docs/MASTER_THREAD_BUCKET_ORCHESTRATION_LAW.md`
- `OAN Mortalis V1.1.1/docs/WORKSPACE_BUCKET_GROUP_SYSTEM.md`
- `Build Contracts/Crosscutting/WORKSPACE_RULES.md`


- `OAN Mortalis V1.1.1/docs/FIRST_RUN_CONSTITUTION.md`

## Family Model

Use the constitutional family model:

- `Oan.*` for umbrella stack composition and stack-level contracts
- `CradleTek.*` for infrastructure and substrate ownership
- `SoulFrame.*` for operator and identity-facing workflow ownership
- `AgentiCore.*` for agent runtime ownership
- `SLI.*` for symbolic protocol and runtime ownership across the stack

Do not force all active work into `Oan.*`.

## Build Behavior

Canonical local commands from repo root:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
```

Expected preflight behavior:

- run workspace path hygiene before build or test
- fail if tracked managed files reveal:
  - the resolved private corpus path
  - external absolute paths outside the repo root

Manual hygiene command:

```powershell
powershell -ExecutionPolicy Bypass -File .\OAN Mortalis V1.1.1\tools\verify-private-corpus.ps1
```

## Documentation Governance Discipline

Treat the `Documentation Repo` as an active governing documentation surface when it is available locally.

Secondary tooling extension rule:

- treat `Antigravity` as a documentation-layer extension for cross-platform communication, repository orientation, and secondary analysis
- keep `Codex` in this workspace as the build master and active implementation authority
- do not let `Antigravity` supersede repo-local executable truth, build verification, or governed mutation flow
- require any `Antigravity`-derived recommendation to be validated against current repo state before implementation
- do not treat `Antigravity` as an independent mutation authority or alternate build surface

For agent work in this repository:

- ground implementation in current repo-local executable truth first
- consult the `Documentation Repo` for stabilized conceptual truth and theory digestion when available
- when work touches automation, seeded governance, or CME office formation, consult current repo-local telemetry under `.audit/state/` when it exists before inferring office, continuity, or readiness
- when work touches bucket orchestration or delayed automation handoff, consult the master-thread orchestration surfaces under `.audit/state/master-thread-orchestration-*.json` before issuing new instruction intent
- never hard-code an external documentation-repo path into tracked files
- state plainly when the external documentation surface is unavailable in a working session

Evidence-led revision rule:

- do not revise documents merely because a revision cycle elapsed
- revise only the documents affected by code maturation, runtime telemetry, package promotion, doctrine correction, theory growth, or architectural seam discovery
- leave unaffected documents stable

Exploration rule:

- when building beyond the current known world, distinguish clearly between implemented and verified, fallback-only, doctrine-defined, contract-backed, and exploratory horizon work
- do not present exploratory structure as current executable truth

## Private Reference Corpus

The repository may use a private local reference corpus identified only as:

- `Lucid Research Corpus`

Resolution order:

1. explicit tool argument
2. ignored repo-local config at `OAN Mortalis V1.1.1/.local/private_corpus_root.txt`
3. `OAN_REFERENCE_CORPUS` environment variable

Rules:

- never commit the resolved corpus path
- never print the resolved corpus path into tracked docs, configs, or code
- use logical source labels only
- keep build outputs and repository history path-independent

## Edit Rules

- prefer changes in `OAN Mortalis V1.1.1/` unless the task is explicitly governance-oriented
- treat `Build Contracts/` as controlled architectural governance
- do not introduce new ambiguous top-level families
- do not create new stack composition roots besides the canonical `Oan.Runtime.Headless`
- if a shim is introduced, document the true owner and intended removal path

## Verification Rules

Before closeout on meaningful changes:

- run `build.ps1` or justify why build was not run
- run `test.ps1` or justify why tests were not run
- run `verify-private-corpus.ps1`

If the task is documentation-only, still run the hygiene check when docs or build surfaces were touched.
