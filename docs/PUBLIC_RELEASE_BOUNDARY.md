# Public Release Boundary

The public repository is a descriptive boundary, not an engineering workspace.

## Allowed Public Detail

- plain-language purpose;
- safety and accountability principles;
- citizen science education and stewardship posture;
- disclosure rules;
- high-level custodial cognition safety framing;
- high-level end-user endpoint and typed application layer framing;
- non-claim language around familiarity, authority, continuity, and custody;
- non-executable examples of acceptable public wording;
- contact paths and contribution hygiene.

## Withheld Detail

- source code and project structure;
- build, test, release, deployment, or automation steps;
- private architecture contracts;
- datasets, fixtures, corpora, or generated artifacts;
- private vocabulary maps and symbolic payloads;
- model-specific operating detail;
- local environment paths or machine-specific assumptions.
- private lab records, control surfaces, and implementation-bearing review
  ledgers.

## Review Standard

A public artifact should answer:

1. What is the general purpose?
2. What boundaries govern the claim?
3. What is intentionally not being published?
4. Who should be contacted for private review?
5. What education or stewardship boundary applies?

It should not answer:

1. How do I build it?
2. What are the internal schemas?
3. What files, scripts, or payloads are required?
4. What private data or model configuration was used?

## Publication Surface Rule

Public material may explain why a boundary exists. It must not expose enough
detail to bypass, reconstruct, or operate the private boundary.

Acceptable public wording stays at the level of:

- purpose;
- posture;
- review criteria;
- non-claims;
- disclosure routing.

Unacceptable public wording becomes:

- a source map;
- a build recipe;
- an operational checklist;
- a private vocabulary key;
- a claim of readiness, authority, personhood, or deployment.
