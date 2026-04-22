# WORKSPACE RULES (MANDATORY)

## 1) Build Contracts are the root of truth
**Folder:** `Build Contracts/`  
**Rule:** Any task that affects architecture, interfaces, naming, layering, persistence rules, determinism, or governance **must first be checked against Build Contracts**. If a model proposes anything that conflicts, it must **stop and ask** or **revise to comply**.

> "Before making any changes, read and obey Build Contracts. If a requested change conflicts with contracts, do not implement—report the conflict."

---

## 2) Historical archives are read-only reference
**Folders:** external historical lines and archives when available
**Rule:** This is a **reference dataset** only. No edits, no builds, no "quick fixes," no refactors.  
Historical lines exist for concept/prototype mining, migration mapping, and provenance checks.

When info is needed from a historical line:
- Quote file paths
- Extract minimal relevant snippets
- Propose a `V1.1.1`-native reimplementation

---

## 3) v1.1.1 is the active build target
**Folder:** `OAN Mortalis V1.1.1/`  
**Rule:** All implementation work happens here. All repo-root build/test commands run here. All new code lands here unless a task is explicitly reference-only or archival.

---

## Path Allowlist
- **Allowed implementation write paths:** `OAN Mortalis V1.1.1/**`
- **Allowed governance write paths:** `Build Contracts/**` only when the task is explicitly architecture, naming, layering, dependency, determinism, persistence, or governance work
- **Allowed read paths:** `Build Contracts/**`, `OAN Mortalis V1.1.1/**`, available historical lines for reference only

## Forbidden write paths
- historical lines and external archives
- Anything else in `Unity Projects/**` outside the active build line
