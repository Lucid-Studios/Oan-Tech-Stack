# OPERATOR_TELEMETRY_VISIBILITY_LATTICE

## Summary

This document defines the future custody-aware visibility lattice for live CME telemetry.

The purpose of this lattice is to let a steward or operator see:

- that the runtime is alive
- where telemetry is flowing
- what class of protected material is present
- how much pressure, load, or routing activity exists
- what disclosure boundary is active

without defaulting to protected content reveal.

This is a planning anchor for later implementation.

It does not change current runtime authority, custody law, or publication law.

## Core Law

Operator telemetry surfaces may reveal the existence, posture, health, routing, and access class of protected CME data without revealing protected contents unless the active viewer, environment, and governing policy authorize that disclosure.

This means the operator surface must prioritize:

- state
- volume
- class
- posture
- authorization boundary

before content.

## Architectural Placement

The visibility lattice sits above the current internal control plane and below any future richer operator display system.

It must project from:

- journal-first runtime evidence
- typed loop/control state
- policy and authorization state
- custody-aware storage posture

It must not become:

- a second truth store
- a direct custody inspection surface
- a bypass around governance or access controls
- an implied content-authority layer

## Relation To Current Stack

This lattice extends the current runtime reading:

- Sanctuary provides the collective nervous system of SLI
- `OE` and `cOE` preserve individuated CME continuity and identity custody
- SoulFrame mediates CME-specific protected self-state
- AgentiCore actualizes bounded working cognition
- Prime and Cryptic remain asymmetrical source/derivative domains

Therefore the operator surface must project:

- evidence of protected activity
- evidence of state class
- evidence of routing and health

without collapsing the distinction between:

- custody and projection
- visibility and authority
- content and evidence of content

## Visibility Tiers

### Tier 1. Prime-Safe Operator Surface

Default operational view.

May show:

- health
- counts
- state classes
- routing pressure
- protection posture
- consented visibility summaries
- governed-view availability

Must not show:

- raw protected contents
- cryptic payload structure
- privileged-only data

### Tier 2. Consented Operator Panel View

Scoped operator view over the operator's own bounded private context.

May show:

- additional scoped detail on the operator's own material
- selected previews where policy and consent allow
- deeper summaries of recent working-state and collapse behavior

Must remain:

- policy-bound
- session-bound
- auditable

### Tier 3. Governed Secure Access

Review-driven secure inspection tier.

May show:

- deeper redaction-aware telemetry
- secure-case inspection views
- governed review material

Requires:

- explicit authorization
- audit trail
- reason code
- session-bound access

### Tier 4. Court And Attorney Privileged View

Separate privileged policy class.

This is not just "more access."

It is a distinct disclosure posture with its own:

- handling rules
- visibility rules
- retention rules
- escalation requirements

### Tier 5. Secret And Top-Secret Protected Stores

Highest posture tier.

Default representation should minimize disclosure and may show only:

- presence
- classification posture
- custody state
- integrity state
- access denial when the environment or authorization is insufficient

## Store Classes And Projection Targets

The lattice should be able to project state over these runtime surfaces:

- `SelfGEL`
- `cSelfGEL`
- `GoA`
- `cGoA`
- `MoS`
- `cMoS`

But by default it should project them as lawful telemetry objects rather than raw content objects.

## Displayable Metadata

For each protected store class, the lattice should eventually expose metadata such as:

- `store_id`
- `store_class`
- `health_state`
- `load_state`
- `coherence_state`
- `recent_ingest_count`
- `recent_collapse_count`
- `protected_payload_count`
- `visibility_class`
- `consent_state`
- `governed_access_state`
- `privileged_access_state`
- `classification_posture`
- `last_audit_event`
- `drift_state`
- `pressure_state`

These fields are intended to make protected runtime activity observable without default content disclosure.

## Payload Classification Expectations

The visibility lattice should project payload posture using the same truth discipline already established elsewhere in the stack.

Relevant classes include:

- `payload_present`
- `pointer_only`
- `summary_only`
- `empty_by_policy`
- `empty_by_observation`
- `empty_by_design`
- `deferred`
- `denied`
- `dropped_error`
- `unimplemented`

The operator surface should show which class is active and why.

## HDT Role

The Holographic Data Tool is a strong future candidate for rendering this lattice because it can provide lawful projection without requiring raw protected disclosure.

In that future role, HDT may render:

- `SelfGEL` health fields
- `cSelfGEL` protection posture
- routing pressure into `cGoA`, `MoS`, Dream-seed, discard, and defer-review lanes
- visibility bands by protection class
- drift, coherence, and load over time
- access-gate overlays
- consent and governance state
- privileged-hold indicators

However:

- HDT remains a projection surface, not runtime authority
- HDT must not override policy or authorization
- no `.hopng` artifact becomes constitutive runtime authority in this phase

## Enforcement Rule

Access tiering must not be only a UI concern.

It must be enforced by:

- runtime policy
- audit receipts
- authorization and session state
- environment controls
- disclosure rules

The operator projection layer may only reveal what those systems already authorize.

## Safeguards

The visibility lattice must not:

- treat query as authority
- reveal protected content by default
- widen custody semantics into the operator surface
- conflate privileged access with routine operator access
- collapse Prime-safe and Cryptic disclosure classes
- make HDT or any projection surface the source of truth

## Suggested Future Runtime Objects

Examples only for later implementation:

- `TelemetryVisibilityTier`
- `TelemetryDisclosureClass`
- `ProtectedStoreVisibilityView`
- `ProtectedStoreHealthView`
- `ProtectedRoutingPressureView`
- `GovernedAccessWindow`
- `PrivilegedDisclosureReceipt`

These belong to a later implementation phase, not this document.

## Implementation Order For Later Phases

When this lattice is implemented, the recommended order is:

1. internal query/read models over protected store posture
2. Prime-safe operator summaries
3. consented operator views
4. governed secure review views
5. privileged disclosure policy classes
6. optional HDT-backed richer projection surfaces

## Completion Condition

This planning line is successful when the future operator surface can show the posture, health, routing, and access class of protected CME data without treating visibility as permission or protected presence as content disclosure.
