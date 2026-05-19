# AGENTS.md

## Public Surface Instruction

This repository is handled as the public test-release shell for Project
Sanctuary. Agents working here must preserve the difference between:

- public product identity;
- archived OAN Tech Stack lineage;
- future release candidates;
- private build truth.

The correct posture is: make the release page clear, coherent, and safe without
reconstructing the private implementation.

## Style Standard

Use the same public language across root documents:

- project name: Project Sanctuary;
- maintainer body: Lucid Studios, Department of Agentic Research and
  Development;
- product role: software service layer for extended cognitive management;
- core surfaces: Cradle Technology, Soul Frame, Agentic Core;
- cognition term: Engineered Cognition;
- lineage posture: Carl Gustav Jung-inspired/theological framing and
  IUTT-inspired distinction discipline, both bounded as methodology rather than
  proof.

## Allowed Work

Allowed changes are limited to:

- release-page polish;
- project-scope clarification;
- release-boundary wording;
- non-claim clarification;
- responsible disclosure routing;
- issue and pull request hygiene;
- repository metadata that supports future public release review.

## Disallowed Work

Do not add or restore:

- build, install, test, release, or deployment commands;
- workflow files that execute product build or release candidates;
- private corpus paths, local absolute paths, secrets, tokens, logs, or
  machine-specific configuration;
- runtime payload names, hosted model payloads, fixtures, datasets, generated
  audit material, or reconstruction inputs;
- implementation-bearing schemas, source layouts, private architecture
  contracts, or reproducible operating sequences;
- issue or pull request prompts that ask contributors to provide exact
  reproduction commands for private build surfaces.

## Review Standard

A public change is acceptable when it improves Project Sanctuary's release-page
clarity, preserves non-claim discipline, and avoids giving enough detail to
reconstruct private implementation.

When uncertain, reduce specificity and route the material to private maintainer
review.

## Local Build Truth

Local folders on the maintainer machine are private unless explicitly published.
Do not infer that local working organization, private corpora, or runtime
folders should be exposed here.

This public repository should not be used as a map of the maintainer's private
workspace.

## Verification

Before closeout, check that modified public files do not introduce private
paths, operational commands, build recipes, release automation, or
implementation-bearing reconstruction detail.

If a technical verification was performed privately, summarize the result
without publishing exact local commands, payload paths, or private environment
details.
