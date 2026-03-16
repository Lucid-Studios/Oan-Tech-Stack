# SLI Runtime Semantic Ownership Transfer

## Purpose

This note defines the transfer of semantic authority for SLI execution from the current host-side C# interpreter stack to a target SLI/Lisp runtime, while preserving a C# membrane for legality, transport, telemetry, conditioning, and return.

It exists to prevent a false migration story in which the stack verbally announces a target Lisp runtime while the host interpreter quietly remains the hidden mind.

## Root Statement

A legal SLI program must mean what the target SLI/Lisp runtime says it means.

The host membrane may validate, reject, quarantine, trace, receipt, condition, transport, and return a legal SLI program. It may not silently inherit, reinterpret, or substitute semantic authority for that program.

## Governing Lines

> The membrane may judge legality, but it may not judge meaning.
>
> Unavailable target semantics are a service failure, not a host fallback opportunity.
>
> If the target runtime is absent, the system must refuse migrated cognitive service rather than let the host interpreter remain the hidden mind.

## Current Truth

Today, runtime meaning is still materially owned by the host-side stack through:

- `src/SLI.Engine/LispBridge.cs`
- `src/SLI.Engine/Runtime/SliInterpreter.cs`
- `src/SLI.Engine/Runtime/SliSymbolTable.cs`

This means the host remains interpreter-authority today, even where Lisp-authored composition has become meaningful.

## Target Truth

The target SLI/Lisp runtime becomes the sole authority for:

- operation semantics
- evaluation semantics
- composite expansion semantics
- higher-order cognition-stage semantics
- residue-production semantics
- hot `c*` reasoning semantics

The C# layer becomes membrane-authority only.

## Authority Map

### Target SLI/Lisp Runtime Owns

- operation semantics
- evaluation semantics
- composite expansion semantics
- higher-order cognition-stage semantics
- residue-production semantics
- hot `c*` reasoning semantics

### C# Membrane Owns

- legality checks
- IR validation
- issuance envelopes
- host/device transport
- trace capture
- residue capture
- conditioning receipts
- Prime boundary support
- collapse return mediation
- observability
- fault containment

### Shared Contract Owns

- SLI IR and opcode surface
- operand schemas
- value schemas
- residue schemas
- trace event schemas
- result packet schemas
- runtime capability manifest

## Boundary Law

The membrane may:

- reject illegal programs
- quarantine unsupported or unsafe programs
- receipt and trace execution
- validate capability manifest compatibility
- mediate host/device exchange and lawful return

The membrane may not:

- reinterpret a legal program's meaning
- silently substitute host semantics for target semantics
- invent cognition results when target semantics are unavailable
- downgrade target absence into covert host execution
- treat receipt, transport, or transit centrality as permission to mutate governing control surfaces directly

If the target runtime is unavailable for a migrated semantic lane, the lawful outcomes are:

- explicit refusal
- explicit unavailable status
- explicit degraded non-cognitive service, if such a path is separately defined and truthfully labeled

The unlawful outcome is silent host semantic fallback.

## Self-State Law

Semantic ownership transfer must preserve the self-state membrane:

- the target runtime operates over hot `cSelfGEL`
- the membrane validates against cooled `SelfGEL` where continuity law requires it
- unresolved contradiction becomes residue, obstruction, or deferred admissibility state
- the membrane does not author self-state; it witnesses and mediates it

This means target cognition does not bypass SoulFrame, and SoulFrame does not secretly remain the cognition engine.

## Relationship To CradleTek And SoulFrame

This transfer does not make the target runtime the whole stack.

- CradleTek remains service governance, TSR policy, lifecycle, posture, attestation support, and ingress or egress permission owner
- SoulFrame remains the continuity membrane, self-state mediator, legality witness, and collapse-return intake boundary
- Sanctuary telemetry remains witness and service support, not hidden semantic authorship
- AgentiCore remains the verified operative process instantiated inside the protected runtime field

The connective C# layer therefore remains a typed control-law surface, not a hidden mutation shortcut into governance.

## Relationship To The Target Substrate

The first practical target substrate for this transfer is a harnessed graphics card.

That substrate relation is defined in:

- `docs/GPU_CRYPTIC_HARNESS_MODEL.md`

The important order is:

1. semantic ownership transfer
2. shared IR and capability manifest
3. device-safe target subset
4. harnessed GPU residency
5. refusal when target semantics are unavailable

The stack must not invert that order by using the device as branding while the host interpreter still owns meaning.

## Migration Phases

### Phase 1

Define:

- IR surface
- opcode manifest
- value schemas
- residue schemas
- trace schemas
- capability manifest

### Phase 2

Mirror a bounded subset of operation semantics on the target runtime.

### Phase 3

For that migrated subset, reduce the host C# layer to:

- verification-only behavior
- transport-only behavior
- telemetry and receipt behavior

### Phase 4

Remove semantic authority from the host interpreter for migrated operations.

### Phase 5

Refuse service when the target runtime is unavailable for migrated semantic lanes.

This final phase is what makes the transfer mechanically real.

## Proof Obligations

The stack should not claim this transfer as complete until all of the following are provable:

- the same legal IR yields the same result on target with no host semantic fallback
- the membrane may block illegal operations but may not alter legal-operation meaning
- trace and residue survive host-device round-trip intact
- conditioning receipts remain membrane-authored, not cognition-authored
- self-validation polling remains membrane-mediated
- target absence produces explicit refusal, not silent host execution
- capability manifest mismatches surface as incompatibility, not as host reinterpretation

## Implementation Notes

This note does not say the current target runtime already exists.

It says the architecture becomes true only when semantic ownership migrates away from the current host interpreter stack and into the target SLI/Lisp runtime.

Until then:

- current host interpretation remains current truth
- membrane language should be used aspirationally and carefully
- host fallback must not be quietly normalized into the final architecture

## Compression Line

C# may remain the membrane, but it must cease to remain the mind.
