# AGENTS.md

## Mission

This repository is a public descriptive surface only.

Agents working here must preserve a documentation-first release posture. Do not
reintroduce private implementation work, buildable source, internal architecture
contracts, release automation, datasets, test fixtures, or environment-specific
material.

## Allowed Work

Allowed changes are limited to:

- public explanatory documents;
- non-executable examples;
- release boundary wording;
- security and disclosure guidance;
- issue and pull request hygiene;
- repository metadata that supports public documentation review.

## Disallowed Work

Do not add:

- `src/`, `tests/`, build roots, release roots, or generated audit roots;
- source code, project files, solution files, scripts, or notebooks;
- private corpus references, local absolute paths, credentials, tokens, or logs;
- implementation recipes, schemas, fixtures, payloads, or reproducible operating
  sequences;
- model-specific, operator-specific, identity, custody, or runtime payload
  material.

## Review Standard

A public change is acceptable only when it helps a reader understand the
technology's purpose, governance posture, or disclosure boundary without giving
them enough information to reconstruct the private build.

When in doubt, publish less detail and route the material to private review.

## Verification

Before closeout, run or rely on the public surface check and link check. A valid
public branch should have no build/test command to run.
