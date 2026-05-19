# AGENTS.md

## Public Surface Instruction

This repository is handled as a bare public release shell. Agents working here
must preserve the difference between the current sparse release floor, archived
lineage, and private build truth.

The correct posture is: keep the shell clean until a deliberate release gate
admits new public material.

## Allowed Work

Allowed changes are limited to:

- release-shell hygiene;
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

A public change is acceptable when it preserves the clean release shell, clarifies
boundary posture, or prepares for a future reviewed public release without
giving enough detail to reconstruct private implementation.

When uncertain, reduce specificity and route the material to private maintainer
review.

## Local Build Truth

Local folders on the maintainer machine are private unless explicitly published.
Do not infer that local working organization, private corpora, or runtime folders
should be exposed here.

This public repository should not be used as a map of the maintainer's private
workspace.

## Verification

Before closeout, check that modified public files do not introduce private
paths, operational commands, build recipes, release automation, or
implementation-bearing reconstruction detail.

If a technical verification was performed privately, summarize the result
without publishing exact local commands, payload paths, or private environment
details.
