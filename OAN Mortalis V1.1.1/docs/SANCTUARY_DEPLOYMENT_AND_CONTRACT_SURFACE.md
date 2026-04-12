# SANCTUARY_DEPLOYMENT_AND_CONTRACT_SURFACE

## Purpose

This note records the implementation-facing Sanctuary deployment and contract
surface for `OAN Mortalis V1.1.1`.

It is not the authoritative doctrine or legal-admin surface.
It is the active build subset that implementation work may depend on now.

## Governing Read

`V1.1.1` inherits a narrow executable posture:

- default deployment is local-bound
- first download or fetch is not remote research consent
- protected, operator, and organization-controlled data remain local unless a
  separate remote layer is explicitly invoked
- organization-managed deployment is a distinct contract class from individual
  operator use
- runtime enforcement must not outrun stabilized doctrine

## Executable Rules

### Local-Bound Default

The default build posture is:

- local binding first
- local execution first
- no implied provider-visible research lane
- no implied protected-data upload lane

### First Download Rule

The first download or fetch must be treated as:

- license delivery
- entitlement verification
- integrity checking
- update retrieval

It must not be treated as:

- research consent
- support-side data disclosure
- protected-data processing consent

### Remote Layer Activation

Remote support or research is a separate layer.

Implementation work must model it explicitly rather than inferring it from:

- local install
- ordinary runtime use
- default telemetry presence

### Organization-Managed Distinction

Organization-managed deployment activates additional burdens around:

- administration
- protected-data handling
- localization and residency
- access control and audit posture

That distinction must remain visible in typed contract records.

Those typed contract records remain a derived implementation view.

They are not the canonical install identity owner and do not replace localized
agreement assent topology.

## Source Discipline

The authoritative doctrine and legal-admin reading lives in the Documentation
Repo under the Sanctuary deployment, contract-layer, and formation surfaces.

Tracked files in this repo must not hard-code external absolute paths to those
materials.

## Current Implementation Target

For this milestone, `V1.1.1` may carry:

- typed contract records in `GEL.Contracts`
- implementation-facing documentation
- tests proving the contract and formation surfaces remain lawful

For this milestone, `V1.1.1` must not carry:

- premature runtime enforcement in `CradleTek`, `SoulFrame`, `AgentiCore`, or
  `Oan.HostedLlm`
- silent widening of deployment or data posture beyond the typed contract
  surface

## Working Summary

The current implementation truth is:

- local-bound first
- explicit remote activation only
- protected and organization-controlled data stay local by default
- organization management is distinct from individual operation
- doctrine remains upstream of runtime enforcement
