# Summary

Brief description of the change.

# Motivation

Why this change is required.

# Implementation

Key changes made.

# Architecture Impact

Affected subsystem:

- CradleTek
- SoulFrame
- AgentiCore
- SLI Engine
- Hosted LLM
- Infrastructure
- Crosscutting

# Testing

Describe how the change was verified.

Minimum expected when applicable:

- `powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release`
- `powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release`
- `powershell -ExecutionPolicy Bypass -File .\OAN Mortalis V1.0\tools\verify-private-corpus.ps1`

# Deployment Notes

Configuration changes or runtime requirements.

# Hygiene Check

- [ ] No local absolute paths outside the repository root were introduced
- [ ] No private corpus paths were committed
- [ ] Changes are scoped to the active build or intentionally documented otherwise
