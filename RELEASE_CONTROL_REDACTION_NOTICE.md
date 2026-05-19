# Release Control Redaction Notice

Date: 2026-04-26
Updated: 2026-05-19
Scope: public repository and public issue tracker surface
Status: bare release shell

## Summary

The public OAN Tech Stack repository has been reduced to a minimal release
shell. Older public documentation, examples, and descriptive strata have been
removed from this repository so they do not confuse future readers or appear to
be part of the next current release.

Those materials were preserved through Codex Mirror archive custody rather than
left intermingled with the release home.

## Repository Treatment

The current public tree intentionally retains only:

- root repository identity;
- contribution and security guidance;
- public issue and pull request hygiene;
- release-control notice;
- agent instructions for preserving the public/private boundary;
- workflow checks that prevent accidental reintroduction of implementation or
  private material before a deliberate release gate.

The current public tree intentionally excludes:

- historical public docs and examples;
- implementation source;
- project and solution files;
- build, test, release, deployment, and automation scripts;
- datasets, fixtures, corpora, symbolic payloads, and generated audit material;
- private topology, corpus lineage, model-specific operations, local paths, and
  machine-specific assumptions.

## Archive Treatment

Historical and nonessential materials belong in Codex Mirror:

<https://github.com/Lucid-Studios/Codex-Mirror>

Archive custody preserves lineage. It does not make archived material current
release truth.

## Future Release Rule

Any future republication into this repository requires a clean release review.
Material should be added because it belongs to the next public release, not
because it survived in an older public surface.

Future release candidates must preserve:

- clear current-version posture;
- explicit non-claims;
- private-path and secret hygiene;
- no accidental build or runtime leakage;
- separation between archive lineage and current release body.

## Non-Claims

This repository's sparse state does not publish an implementation, grant
runtime authority, assert CME.Actual, assert Sanctuary.Actual, or certify
production readiness.
