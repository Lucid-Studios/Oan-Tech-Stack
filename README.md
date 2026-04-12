# OAN Tech Stack

`OAN Tech Stack` is the active engineering workspace for the **OAN Mortalis** build line.

This repository currently carries:
- the active executable line: `OAN Mortalis V1.1.1`
- the install-first side-by-side sibling line: `OAN Mortalis V1.2.1`
- the governed build contracts that define family, dependency, and workspace rules

This is not just a code repository.  
It is a **governed build surface** with explicit law, audit, and verification posture.

## Current Build Posture

- `OAN Mortalis V1.1.1/` — **Active executable truth** and current build/test target  
- `OAN Mortalis V1.2.1/` — Install-first sibling line being formed in parallel  
- `Build Contracts/` — Crosscutting governance and workspace constitution

**V1.1.1 remains the active runtime line.**

## Local LLM Runtime (Important)

The full operational build **requires a local hosted LLM runtime and associated model space** that is currently **withheld** from this public repository for security and testing reasons.

This means:
- The repository will **not build completely** out of the box for external users.
- Core symbolic layers (AgentiCore, SLI, SoulFrame) can still compile and run in public-core mode.
- Full CradleTek + resident LLM integration is air-gapped until operational release.

We will release the complete operational set (including local LLM integration guides) once internal testing is complete.

## Architecture Read

```mermaid
flowchart LR
  OP["Operator / Developer"] 
  AC["AgentiCore"] 
  SLI["SLI"] 
  SF["SoulFrame"] 
  CT["CradleTek"]

  OP --> AC --> SLI --> SF --> CT

Working family ownership in the active line is:

- `AgentiCore.*`
  agent runtime ownership
- `SLI.*`
  symbolic protocol and runtime ownership
- `SoulFrame.*`
  operator and identity-facing workflow ownership
- `CradleTek.*`
  infrastructure and substrate ownership
- `Oan.*`
  umbrella stack composition and stack-level contracts

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
powershell -ExecutionPolicy Bypass -File .\OAN Mortalis V1.1.1\tools\verify-private-corpus.ps1
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
