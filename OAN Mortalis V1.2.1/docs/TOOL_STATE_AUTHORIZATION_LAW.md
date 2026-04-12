# TOOL STATE AUTHORIZATION LAW

## Purpose

This note defines site-bound and operator-bound tool-state authorization for
pre-Cradle standing in `V1.2.1`.

It exists so local tool availability is not over-read into service authority,
runtime autonomy, or governing action.

## Governing Compression

> tool-state authorization may support pre-Cradle standing, but it does not
> authorize service activation, runtime autonomy, or governing action.

## Authorization Office

The bounded passive tool-state enum and record now live in:

- `ToolAuthorizationState`
- `ToolStateAuthorizationRecord`

Each tool-state authorization must preserve:

- one tool surface name
- one authorization state
- one site-binding profile
- one operator
- one witness chain

## Existing Seam Dependence

Use this note together with:

- `CRADLETEK_SITE_BINDING_PROFILE_LAW.md`
- `SANCTUARY_INTENDED_SERVICE_REGISTER.md`
- `PRE_CRADLETEK_SITE_AUTHORIZATION_CANDIDATE_LAW.md`

Tool-state authorization may support site-bound standing while every intended
service entry remains disabled.

## Explicit Non-Grants

This note does not grant:

- service activation
- runtime autonomy
- governing `CME` placement
- CradleTek authorization

## Working Summary

`V1.2.1` now fixes one passive tool-state authorization surface for later
pre-Cradle standing without turning tools into autonomous or service-bearing
authority.
