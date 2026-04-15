# Security Policy

## Scope

This policy applies to `Lucid-Studios/Oan-Tech-Stack`.

## Report Security Issues Privately

Do not open public GitHub issues for:

- credential exposure
- private corpus leakage
- sensitive runtime configuration disclosure
- infrastructure abuse paths
- vulnerabilities that should be privately triaged first

Use a private maintainer contact path instead of the public issue tracker.

Private disclosure contacts:

- `admin@lucidtechnologies.tech`
- `legal@lucidtechnologies.tech`

## Sensitive Material

The following must never be committed or posted publicly:

- private corpus roots
- secrets or tokens
- machine-local credentials
- external absolute paths that reveal sensitive local structure
- private runtime payloads or model artifacts

## Temporary Mitigation

If you discover a live exposure:

1. stop propagating the content
2. rotate or isolate affected credentials if relevant
3. remove leaked content from current working surfaces
4. notify maintainers privately

## Non-Security Issues

Normal defects, build failures, and feature requests should go through the standard repository issue templates.
