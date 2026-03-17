# CradleTek Prime Runtime Workflow

## Purpose

Operate the hosted SoulFrame runtime as infrastructure outside the repository while keeping the OAN cognition stack deterministic and symbolic-first.

## Pre-requisites

1. Windows host with PowerShell.
2. Python 3 available.
3. Runtime root write access at `<CRADLETEK_RUNTIME_ROOT>`.
4. Model weights staged manually outside repo at `<CRADLETEK_RUNTIME_ROOT>\models\seed.gguf` (or another `*.gguf` in the models folder, or a custom path in config).

## Bootstrap

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1
```

Optional explicit root:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\windows-bootstrap.ps1 -CradleTekRoot <CRADLETEK_RUNTIME_ROOT>
```

Bootstrap performs:

1. Runtime directory creation under `<CRADLETEK_RUNTIME_ROOT>`.
2. Hyper-V and CPU virtualization checks.
3. Python virtual environment setup.
4. Flask dependency install.
5. `llama.cpp` clone/build/install.
6. Inference service deploy and startup.

Bootstrap resolves an existing `*.gguf` in the models folder automatically when `seed.gguf` is absent.

## Runtime Preflight

```powershell
powershell -ExecutionPolicy Bypass -File scripts\runtime-preflight.ps1
```

Use the preflight command before live inference runs to distinguish:

1. `ready-for-inference`
2. `ready-for-model-drop`
3. `runtime-binary-missing`
4. `runtime-binary-and-model-missing`

## Start Runtime

```powershell
powershell -ExecutionPolicy Bypass -File tools\cradletek\start_runtime.ps1
```

This starts the hosted API service and writes a PID file to:

```text
<CRADLETEK_RUNTIME_ROOT>\runtime\soulframe_runtime.pid
```

## Build/Test Stack

```powershell
dotnet build Oan.sln
dotnet test Oan.sln
```

## Stop Runtime

```powershell
powershell -ExecutionPolicy Bypass -File tools\cradletek\stop_runtime.ps1
```

If the service is bootable but the model asset is absent, the runtime health surface now reports `ready-for-model-drop` instead of treating stub output as inference readiness.

## Repository Purity Rules

Do not place these in repository history:

1. VM disks/images.
2. Model weights.
3. Runtime logs and caches.
4. Container layers.

These remain external runtime infrastructure owned by CradleTek.

## Windows Runtime Guide

See:

```text
docs/runtime/WINDOWS_RUNTIME.md
```
