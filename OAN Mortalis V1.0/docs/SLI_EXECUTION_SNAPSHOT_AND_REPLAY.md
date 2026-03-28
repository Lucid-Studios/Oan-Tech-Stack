# SLI Execution Snapshot & Replay

## Overview
The **SliExecutionSnapshot** provides an **internal-only**, deterministic JSON serialization of the SLI runtime execution state. This includes projected execution data, actualization packets, jurisdiction transitions, trace logs, and intermediate results.

The **SliExecutionReplay** allows this snapshot to be loaded for inspection, debugging, and verification **without reconstructing live services** (such as databases, external inference devices, or other heavy dependencies).

### Core Principles
- **No Production Continuity**: Snapshots are opt-in, primarily used for CI, debugging, and governance artifacts. They do not automatically form "memory" or disrupt existing production pipelines.
- **Projected State Only**: Represents an immutable result projection (DTOs) from `SliExecutionContext` and `LispExecutionResult`. It does NOT contain live object states (`Action`, generic `Task`, external contexts) or the raw operator registry/symbol table.
- **Deterministic JSON**: Uses `System.Text.Json` with strictly defined internal records, sorted collections (`IReadOnlyList` over `Dictionary` where applicable), and string enum converters to guarantee that identical runtime shapes yield identical JSON hashes.

## Contracts
The core surface resides in `SLI.Engine/Runtime`:

### `SliExecutionSnapshot.cs`
Defines the `SliExecutionSnapshot` record and supporting projection models (e.g., `ActualizationPacketSnapshot`, `LocalityShardSnapshot`, `ZedThetaCandidateSnapshot`).

### `SliExecutionSnapshotFactory.cs`
A static factory responsible for mapping the robust real-world properties of a `SliExecutionContext` and a `LispExecutionResult` into the simplified immutable DTOs for JSON storage.

```csharp
var snapshot = SliExecutionSnapshotFactory.CreateForCognition(context, executionResult);
string json = SliExecutionSnapshotFactory.Serialize(snapshot);
```

### `SliExecutionReplay.cs`
Provides a simple path to deserialize a retained string and reconstruct an inspection graph.

```csharp
var replay = SliExecutionReplay.ReplayFromSnapshot(json);
// replay.Snapshot.ActualizationPacket.Disposition == SliActualizationDisposition.Obstructed
```

## JSON Model Structure
Every property follows `camelCase` naming conventions and `System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` is inherently applied by the factory's default config. 

Key nodes captured:
- **`traceId`**: The originating trace execution identifier.
- **`decision`** / **`decisionBranch`**: Control-flow choices chosen by the active shard.
- **`cleaveResidue`**: Unreached or deferred branching intent.
- **`traceLines`** / **`candidateBranches`** / **`prunedBranches`**: Complete internal trace lists.
- **`localityShards`**: Reduced states for `acting`, `witnessing`, and `adjacent-ingestion` shards.
- **`localityRelationEvents`** & **`localityObstructions`**: A ledger of successful or halted interactions across instances.
- **`actualizationWebbingEvents`**: A ledger of the `bloom`, `seal`, `witness`, `ingest`, and `commit` execution hooks.
- **`actualizationPacket`**: The final projected reality update instruction with disposition status and residues.
- **`zedThetaCandidate`**: The identity boundary and packet authorization payload configured for persistence.
- **`liveRuntimeRun`**: Metrics around the broader distributed topology operation.

## Usage in Testing
An expanded test suite inside `SliExecutionSnapshotTests.cs` verifies:
- Round-trip serial stability.
- Accurate obstruction reflection inside snapshotted DTOs.
- Reduced dependencies mapping accurately from runtime to DTO records.
