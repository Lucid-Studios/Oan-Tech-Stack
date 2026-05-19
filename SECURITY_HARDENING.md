# Security Hardening

## Purpose

This document defines the public repository hardening posture for Project
Sanctuary.

## Public Surface Rule

The public repository must remain a sparse, non-executable test-release shell
until a deliberate release gate admits new public material. It should not
contain implementation material, build recipes, datasets, fixtures, automation,
runtime payloads, private topology, or local environment assumptions.

## CI Sources

Reusable GitHub workflows should be pinned to reviewed immutable refs when used.
Repository-local checks should enforce the public surface rule.

## Intake Rule

Public issues and pull requests must not include sensitive private material. If
a disclosure requires technical detail that could help reconstruct the private
build, route it to private maintainer review instead.

## Current Public Surface

Allowed tracked surfaces:

- `.github/` metadata and public-surface checks;
- top-level security, support, conduct, contribution, and release posture files.

Disallowed tracked surfaces:

- source code and project files;
- build, test, release, deployment, or automation scripts;
- datasets, fixtures, symbolic payloads, generated audit material, or runtime
  artifacts;
- internal architecture contracts and working ledgers.
