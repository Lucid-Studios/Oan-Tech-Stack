# Security Hardening

## Purpose

This document locks the minimum repository hardening rules required before live protected human or corporate intake may be introduced into the repository's test and runtime surfaces.

## Immutable CI Sources

- Reusable GitHub workflows must be pinned to immutable refs such as reviewed commit SHAs or signed tags.
- Moving refs such as `@main` must not be used for CI execution.

## Live Hardened Surface

The live hardened surface includes tracked files under:

- `.github/`
- `Build Contracts/`
- `docs/`
- `Modules/`
- `OAN Mortalis V1.1.1/`
- top-level build, test, and security policy files

Rules:

- tracked files in the live hardened surface must not contain workstation-specific absolute filesystem paths
- tracked files in the live hardened surface must not contain secrets, private corpus roots, or raw protected intake artifacts
- placeholder/example files must remain obviously placeholder-only

## Protected Intake Gate

Live protected intake may not be introduced until:

- reusable workflow refs are pinned to immutable refs
- live-surface path hygiene is green
- the private-corpus hygiene check is green

Private human and corporate intake must remain Cryptic-masked by default and must not be disclosed through tracked files, CI output, or harness logs without explicit lawful reveal policy.

## Archive Boundary

Historical `OAN Mortalis` archives are outside this repository and inert with respect to the active build.

Rules:

- archive content is excluded from live protected-intake surfaces
- automation, CI, and LLM harnesses must not treat the archive as live runtime or admissible protected-intake source material unless explicitly requested
- historical path artifacts may exist in external archives, but they do not satisfy live-surface hygiene standards and must not be imported into active surfaces
