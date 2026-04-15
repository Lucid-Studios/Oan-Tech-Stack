# Responses API Local Harness Receipt Schema Note

## Purpose

This note defines the first governed receipt schema for
`ResponsesApiLocalHarness`.

It exists so the first local harness runs can land as disciplined evidence
instead of loose experiment output.

This slice is doctrine and placeholder-contract only.

## Core Read

`ResponsesApiLocalHarness` is the first-priority `LLM` test surface because the
runtime, receipts, and no-commit-under-heat behavior remain under local
control.

That means each local harness run should land through one governed receipt
schema.

## Required Receipt Fields

The first local harness receipt must carry:

- `surface-class`
- `runtime-class`
- `input-slice-id`
- `heat-commit-state`
- `shell-observations`
- `chain-outcome`
- `receipt-outcome`
- `refusal-recovery-state`
- `evidence-class`

This is the first bounded field set.

## Runtime Class

The first runtime classes are:

- `ObservationOnly`
- `ToolMediatedLocalExecution`
- `ShellMediatedLocalExecution`

These classes distinguish how the local harness was allowed to act.

They do not change the evidence boundary.

## Shared Witness Families

The receipt remains coupled to the already seated witness families:

- `field-state-receipt`
- `ignition-chain-receipt`
- `verification-trace`

That means the local harness receipt does not replace earlier receipts.
It braids them into one local evidence body.

## Outcome Grammar

The first receipt-outcome grammar is:

- `Witnessed`
- `Hold`
- `Refused`
- `Recovered`

The first refusal/recovery grammar is:

- `None`
- `Refusal`
- `Recovery`
- `RefusalThenRecovery`

## Binding Rules

The `chain-outcome` field must bind to the bounded ignition-chain protocol.

The `heat-commit-state` field must bind to the expand-before-commit law.

The `evidence-class` field must remain local-harness specific.

## Evidence Boundary

Local harness evidence remains a distinct evidence class.

It is not interchangeable with:

- Codex cloud task evidence
- general GPT comparison evidence

Local harness receipts may not be promoted into `CME` baseline proof.

## Boundary

This note does not create the runtime harness.
This note does not execute a local Responses API run.
This note does not authorize cloud and local evidence to merge into one class.

It only defines the first lawful landing body for local harness evidence.
