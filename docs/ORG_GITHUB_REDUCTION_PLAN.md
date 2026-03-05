# ORG_GITHUB_REDUCTION_PLAN

## Purpose

This document defines the minimal intended contents of the special `Lucid-Studios/.github` repository after project code has been migrated into dedicated repositories such as `Lucid-Studios/Oan-Tech-Stack`.

The goal is to preserve:

- a clean Lucid Studios organization profile
- shared community-health defaults where useful
- clear separation between organization presentation and project operations

## Target State

`Lucid-Studios/.github` should function as an organization metadata repository only.

It should not be used for:

- project source code
- build/test workflows for individual repositories
- project issue tracking
- project README files
- runtime documentation
- solution files
- project-specific templates

## Minimal Recommended Structure

### Smallest Acceptable Structure

```text
profile/
  README.md
```

This is sufficient if the only purpose of the special repo is the Lucid Studios organization profile page.

### Recommended Minimal Professional Structure

```text
profile/
  README.md

.github/
  ISSUE_TEMPLATE/
    config.yml

CODE_OF_CONDUCT.md
CONTRIBUTING.md
SECURITY.md
SUPPORT.md
pull_request_template.md
```

This version preserves a clean public org profile and a small shared policy surface for repositories that do not override those defaults.

## File-by-File Plan

### 1. `profile/README.md`

Keep.

Purpose:

- present Lucid Studios as an organization
- describe the organization at a high level
- link users to the correct project repositories

Recommended sections:

- organization identity
- core engineering areas
- featured repositories
- contribution guidance
- security/contact links

Do not include:

- project-specific bug reporting instructions
- branch workflow details for a single repository
- active build details from `Oan-Tech-Stack`

### 2. `.github/ISSUE_TEMPLATE/config.yml`

Optional.

Keep only if you want org-wide issue redirection or default contact links.

Purpose:

- disable blank issues on the special repo if desired
- direct users to the correct project repositories
- direct security issues to the correct channel

Recommended behavior:

- no project bug intake on the special repo
- point users to `Lucid-Studios/Oan-Tech-Stack/issues` for OAN Tech Stack work

### 3. `pull_request_template.md`

Optional.

Keep only if you want a generic Lucid Studios PR template that applies across repositories lacking their own template.

Requirement:

- content must be generic and organization-wide
- no project-specific architectural sections unless they truly apply across all repos

### 4. `CODE_OF_CONDUCT.md`

Optional but recommended.

Purpose:

- provide baseline conduct expectations across public repositories

### 5. `CONTRIBUTING.md`

Optional but recommended.

Purpose:

- provide generic guidance on filing issues, opening PRs, and repository etiquette

Must remain generic.

### 6. `SECURITY.md`

Recommended.

Purpose:

- define how to report vulnerabilities for Lucid Studios repositories

### 7. `SUPPORT.md`

Optional.

Purpose:

- tell users where to ask questions
- separate support requests from bug reports

## Content To Remove From `Lucid-Studios/.github`

The following should not remain in the special repo:

- OAN Tech Stack source
- OAN Tech Stack workflows
- OAN Tech Stack issue templates
- OAN Tech Stack runtime or build docs
- OAN Tech Stack `README.md`
- solution files and scripts
- project-specific branch or release processes

These now belong in project repositories, especially:

- `Lucid-Studios/Oan-Tech-Stack`

## Recommended Redirect Language

If org-wide issue config is used, the special repo should direct users like this:

- OAN Tech Stack issues: `Lucid-Studios/Oan-Tech-Stack/issues`
- OAN Tech Stack pull requests: `Lucid-Studios/Oan-Tech-Stack/pulls`
- security concerns: Lucid Studios security contact path

## Sample Content Guidance

### Sample `profile/README.md` Shape

```text
# Lucid Studios

Lucid Studios develops symbolic, runtime, and governance-oriented software systems.

## Featured Repositories
- Oan-Tech-Stack

## Contributing
Please open issues and pull requests in the relevant project repository.

## Security
Please use the organization security policy for responsible disclosure.
```

### Sample `.github/ISSUE_TEMPLATE/config.yml` Shape

```yaml
blank_issues_enabled: false
contact_links:
  - name: OAN Tech Stack Issues
    url: https://github.com/Lucid-Studios/Oan-Tech-Stack/issues
    about: Report project-specific issues in the project repository.
```

## Execution Plan

1. Backup current contents of `Lucid-Studios/.github` if needed.
2. Keep or create `profile/README.md`.
3. Keep only generic shared policy files.
4. Remove all project-specific code and docs from the special repo.
5. Add redirect-oriented issue config if desired.
6. Verify that org profile renders correctly.
7. Verify that project issues are opened in `Lucid-Studios/Oan-Tech-Stack`.

## Decision Rule

If a file exists primarily to support one repository, it should not live in `Lucid-Studios/.github`.

If a file exists to describe Lucid Studios as an organization or to define shared defaults across repositories, it may remain.

## Final Intended Outcome

After reduction, `Lucid-Studios/.github` should behave like an organization presentation and community-health repository.

`Lucid-Studios/Oan-Tech-Stack` should behave like the real project repository for:

- code
- issues
- pull requests
- workflows
- build verification
- runtime and engineering documentation
