# Sanctuary Deployment Layer Contract

## Purpose

This contract fixes the authority split for Sanctuary deployment, contract
layering, and formation-bearing deployment posture.

## Authority Split

The authoritative surfaces are:

- Documentation Repo architecture notes for doctrinal deployment and formation
  reading
- Documentation Repo legal-admin notes for contract-stack, protected-data, IP,
  and localization baseline

The inheriting implementation surfaces are:

- `OAN Mortalis V1.1.1` implementation-facing Sanctuary deployment note
- `GEL.Contracts` typed contract records
- tests that prove those records remain truthful to the upstream doctrine

## Controlled Inheritance Rule

`V1.1.1` may inherit only:

- the executable subset needed by build and test work
- typed contract records
- implementation-facing summaries

`V1.1.1` must not silently inherit:

- counsel-facing clause language as executable truth
- wider legal theory than the build can currently represent
- runtime enforcement claims that doctrine has not yet stabilized

## Runtime Restraint Rule

For the current milestone:

- runtime enforcement in `CradleTek.Host`, `CradleTek.Runtime`,
  `Oan.HostedLlm`, `SoulFrame`, and `AgentiCore` is out of scope
- typed contracts may be added
- documentation and tests may be added
- enforcement must wait until the doctrine and typed contract surface have
  settled

## Path Discipline

Tracked files inside the active build repo must not hard-code an external
Documentation Repo path.

Logical source naming is allowed.
External absolute-path dependence in tracked managed files is not.

## Working Summary

The current law is:

- doctrine and legal-admin remain upstream
- `V1.1.1` inherits only the executable subset
- `GEL.Contracts` is the first machine-readable Sanctuary contract home
- runtime behavior must not outpace contract and doctrine stabilization
