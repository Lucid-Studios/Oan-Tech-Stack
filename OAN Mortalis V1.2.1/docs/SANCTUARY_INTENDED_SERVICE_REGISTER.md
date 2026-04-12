# SANCTUARY_INTENDED_SERVICE_REGISTER

## Purpose

This note defines the first non-authorizing register for surfaces that may
someday move in `V1.2.1`.

It exists to keep three things distinct:

- service intention
- service authorization
- live service activity

## Status Grammar

Use exactly one current status per service entry:

- `planned`
  The service is named for later motion but has no installed standing yet.
- `installed-disabled`
  The structural seat exists, but the service is not authorized to run.
- `templated-disabled`
  A template exists for the service, but no lawful activation has occurred.
- `authorized`
  The service has passed its activation gate and may lawfully run.

In this first register, `authorized` exists only as reserved grammar and is
used by zero actual entries.

## Entry Schema

Each service entry in this register must include exactly:

- surface name
- origin
- current status
- activation gate
- prospective authorizer
- refusal/pause/shutdown law surfaces
- witness path
- notes

Allowed origin values in this first register are:

- `root`
- `template`
- `local-proposal-future`

## First Register Entries

### `SanctuaryID.RTME` Hosted Lisp Service

- Surface name: `SanctuaryID.RTME` hosted Lisp service
- Origin: `template`
- Current status: `templated-disabled`
- Activation gate: lift of `FIRST_WORKING_MODEL_RELEASE_GATE.md` beyond the
  current `hold` plus explicit live `SLI.Engine -> SLI.Lisp ->
  SanctuaryID.RTME` service binding beneath the named seam law
- Prospective authorizer: later issued `SanctuaryID.GoA` service authority
  beneath the release gate
- Refusal/pause/shutdown law surfaces:
  `SANCTUARY_LISP_RTME_SERVICE_LAW.md`,
  `SEAM_REFUSAL_AND_RETURN_LAW.md`,
  `FIRST_WORKING_MODEL_RELEASE_GATE.md`
- Witness path:
  `SANCTUARYID_RTME_SKELETON.md`,
  `FIRST_WORKING_MODEL_TRACE_PATH.md`,
  `tools/Get-LineAuditReport.ps1`
- Notes: the service-form template office `Sanctuary.RTME` may exist without
  this hosted service being active

### Governed Hosted Cryptic Bundle Residency Surface Beneath `SLI.Lisp`

- Surface name: governed hosted cryptic bundle residency surface beneath
  `SLI.Lisp`
- Origin: `template`
- Current status: `installed-disabled`
- Activation gate: lift of `FIRST_WORKING_MODEL_RELEASE_GATE.md` beyond the
  current `hold` plus lawful live binding beneath
  `SLI_ENGINE_LISP_BINDING_CONTRACT.md`,
  `SEAM_REFUSAL_AND_RETURN_LAW.md`, and
  `LISP_CSHARP_BINDING_SCHEMA.md`
- Prospective authorizer: later issued `SanctuaryID.RTME` service authority
  beneath the release gate
- Refusal/pause/shutdown law surfaces:
  `SLI_LISP_CSHARP_TRANSLATIONAL_MEMBRANE_LAW.md`,
  `SEAM_REFUSAL_AND_RETURN_LAW.md`,
  `FIRST_WORKING_MODEL_RELEASE_GATE.md`
- Witness path:
  `FIRST_WORKING_MODEL_TRACE_PATH.md`,
  `LISP_CSHARP_BINDING_SCHEMA.md`,
  `tools/Get-LineAuditReport.ps1`
- Notes: bundle residency may be described and installed in structure without
  becoming active hosted motion

The RTME hosted service-lift corridor may later describe lawful transition
between these disabled postures and hosted motion.

In this slice:

- statuses remain `templated-disabled` or `installed-disabled`
- no entry becomes `authorized`
- service lift remains future motion only
- no site-bound pre-Cradle authorization receipt may move a service entry into
  `authorized`

## Explicit Exclusions

The following are not service entries in this register:

- read-only root tools such as `line-audit-report`
- passive library surfaces such as `San.Common` and `SLI.Runtime`
- non-service seam notes, shell notes, or trace notes

These may describe, witness, or constrain services.
They are not service entries themselves.

## Explicit Non-Grants

This note does not authorize:

- activation
- scheduling
- orchestration
- service interaction
- local sovereignty over service motion

Service intention is not service authorization.
Presence is not activation.

## Working Summary

Possible motion is now visible without becoming permissible.
The first register defines lawful statuses and witness paths.
No actual service entry is authorized in this phase.
