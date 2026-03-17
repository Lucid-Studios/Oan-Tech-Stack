# SEED_LLM_GOVERNED_EMISSION_PROTOCOL

This note defines the first governed emission protocol for seed-facing inference surfaces.

The purpose is operational legibility before broader local-seed participation:

- the host must not infer whether the model is blocked, conflicted, still working, or complete
- non-terminal or malformed emissions must resolve to explicit refusal or error, not silent waiting
- semantic output must be separated from governance posture

## Canonical States

The v1 state vocabulary is:

- `READY`
- `WORKING`
- `HEARTBEAT`
- `QUERY`
- `NEEDS_MORE_INFORMATION`
- `UNRESOLVED_CONFLICT`
- `REFUSAL`
- `ERROR`
- `COMPLETE`
- `HALT`

## Envelope Shape

Seed-governed responses use a bounded envelope:

```json
{
  "decision": "label:equation",
  "payload": "equation-structure",
  "confidence": 0.82,
  "governance": {
    "state": "QUERY",
    "trace": "response-ready",
    "content": "equation-structure"
  }
}
```

The request-side protocol declaration is:

```json
{
  "governance_protocol": {
    "version": "seed-governed-emission-v1",
    "require_state_envelope": true,
    "require_trace": true,
    "require_terminal_state": true,
    "allow_legacy_fallback": false,
    "allowed_states": [
      "READY",
      "WORKING",
      "HEARTBEAT",
      "QUERY",
      "NEEDS_MORE_INFORMATION",
      "UNRESOLVED_CONFLICT",
      "REFUSAL",
      "ERROR",
      "COMPLETE",
      "HALT"
    ]
  }
}
```

## Parser Law

For non-streaming inference responses:

- `QUERY`, `NEEDS_MORE_INFORMATION`, `UNRESOLVED_CONFLICT`, `REFUSAL`, `ERROR`, `COMPLETE`, and `HALT` are terminal
- `READY`, `WORKING`, and `HEARTBEAT` are non-terminal and must not be accepted as final HTTP response states
- missing or unknown state tokens are invalid governed emissions
- missing traces are invalid when the protocol requires traces

When the governed protocol is required and validation fails, the client must surface:

- explicit refusal/error telemetry
- explicit fallback response with `ERROR` state
- no silent wait on undefined output

## Compatibility Boundary

Legacy result-only responses remain parseable only when the caller does not require the governed protocol.

That compatibility exists to avoid pretending every remote endpoint has already upgraded. It must not be used for seed-governed live-wire paths.
