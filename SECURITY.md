# Security Policy

## Scope

This policy applies to the bare public release shell
`Lucid-Studios/Oan-Tech-Stack`.

## Report Privately

Do not open public issues for:

- credential exposure;
- private corpus or dataset disclosure;
- local path leakage;
- sensitive runtime, operator, identity, custody, or model material;
- implementation details that could enable reconstruction of the private build;
- vulnerabilities or abuse paths that require coordinated handling.

Private disclosure contacts:

- `admin@lucidtechnologies.tech`
- `legal@lucidtechnologies.tech`

## Public Repository Rule

The public repository should currently contain only release-shell custody,
governance posture, contribution/security routing, non-claims, and disclosure
guidance. Historical public material belongs in archive custody. Buildable
materials belong outside this public surface.

## Boundary Reports

Please report privately if public material appears to expose:

- private source or reconstruction detail;
- internal control mechanisms;
- private data, model, or runtime payloads;
- local path or machine-specific information;
- claims that imply deployment readiness, legal authority, custody authority, or
  personified operation.

## Temporary Mitigation

If you discover a live exposure:

1. stop propagating the content;
2. preserve a private note of where the exposure appears;
3. notify maintainers privately;
4. avoid reposting sensitive excerpts in issues, pull requests, comments, or
   public forks.
