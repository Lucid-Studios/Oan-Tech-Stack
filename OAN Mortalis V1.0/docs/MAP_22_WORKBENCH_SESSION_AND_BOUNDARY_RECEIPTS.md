# MAP 22 — Workbench Session and Boundary Receipts

## Purpose

This note is intentionally narrow.

It defines the first runtime-facing receipt surface for **Map 22** inside the workbench chamber opened by `SanctuaryRuntimeWorkbenchService.cs`.

The goal is not to introduce a full questioning-engram subsystem yet. The goal is to make the following legible inside runtime machinery:

- what session exists right now
- what exploratory or day-dream motion collapsed into bounded work
- what failed and became a boundary condition
- what returned cleanly from self-rooted depth
- what residue or continuity marker must be carried forward

This note therefore defines only:

- `RuntimeWorkbenchSessionLedger`
- `DayDreamCollapseReceipt`
- `CrypticDepthReturnReceipt`
- shared boundary/residue fields
- questioning as **session event material**, not yet a standalone GEL subsystem

---

## Scope Boundary

This pass should not yet:

- define a full GEL questioning type
- introduce abstract philosophical surfaces beyond what runtime needs
- over-generalize collapse taxonomy beyond current workbench needs
- replace existing governance or Golden Path contracts unnecessarily

This pass should:

- make session existence explicit
- make collapse and return events recordable
- preserve failure as boundary disclosure
- preserve residue and continuity markers as first-class runtime artifacts

---

## 1. RuntimeWorkbenchSessionLedger

### Intent

`RuntimeWorkbenchSessionLedger` is the active chamber ledger for a single workbench session.

It answers:

- what session is open
- what state the session is in
- what exploratory motions occurred
- what bounded outputs were formed
- what collapsed
- what returned
- what remains to be carried forward

### Minimum shape

```text
RuntimeWorkbenchSessionLedger
- SessionId
- ChamberId / WorkbenchId
- OperatorId
- BondedAgentId
- SessionOpenedUtc
- SessionClosedUtc?
- SessionState
- OpeningIntent
- ActiveThreadOrPromptSurface?
- SessionEvents[]
- BoundaryConditions[]
- ResidueMarkers[]
- ContinuityMarkers[]
- LastMeaningfulTransitionUtc
```

### SessionState

Keep this small for Map 22:

- `Open`
- `Exploring`
- `Bounded`
- `Collapsed`
- `Returned`
- `Closed`

### Session event kinds

At minimum the ledger should support these event families:

- `SessionOpened`
- `QuestionAsked`
- `QuestionDeferred`
- `ExploratoryMotionStarted`
- `ExploratoryMotionShifted`
- `DayDreamCollapsed`
- `BoundaryConditionObserved`
- `CrypticDepthReturnObserved`
- `ResidueMarked`
- `ContinuityMarked`
- `SessionClosed`

Questioning appears here only as an event shape. It is not yet promoted into a separate subsystem.

---

## 2. DayDreamCollapseReceipt

### Intent

`DayDreamCollapseReceipt` records the moment where exploratory, drifting, or open interpretive motion collapses into a bounded work product, a boundary, or a failed path.

This receipt should make the transition from open motion to bounded runtime legible.

### Minimum shape

```text
DayDreamCollapseReceipt
- ReceiptId
- SessionId
- CreatedUtc
- SourceExploratorySpanId
- CollapseKind
- PromptOrStimulusSummary
- QuestioningTrace[]
- CandidateDirections[]
- ChosenBoundedDirection?
- BoundaryCondition?
- ResidueMarkers[]
- ContinuityMarkers[]
- OutputArtifactRefs[]
- Notes
```

### CollapseKind

Keep the first runtime vocabulary narrow:

- `BoundedWorkFormed`
- `BoundaryDisclosed`
- `AbandonedWithoutCarry`
- `DeferredForLater`

### Required runtime meaning

A `DayDreamCollapseReceipt` should let the workbench answer:

- what was being explored
- what question or motion caused the collapse
- whether collapse yielded bounded work or only a boundary
- what residue is still worth carrying
- what continuity marker should survive into the next session or receipt

---

## 3. CrypticDepthReturnReceipt

### Intent

`CrypticDepthReturnReceipt` records a return from self-rooted or cryptic exploratory depth back into chamber-legible runtime state.

This is not merely “background thinking finished.” It is a receipt for a return that is coherent enough to be carried, reviewed, or bounded.

### Minimum shape

```text
CrypticDepthReturnReceipt
- ReceiptId
- SessionId
- CreatedUtc
- ReturnKind
- OriginDepthHint
- ReturnSummary
- LegibilityStatus
- QuestioningTrace[]
- BoundaryConditions[]
- ResidueMarkers[]
- ContinuityMarkers[]
- SuggestedNextBoundedAction?
- OutputArtifactRefs[]
- Notes
```

### ReturnKind

Keep the first cut small:

- `CleanReturn`
- `PartialReturn`
- `BoundaryMarkedReturn`
- `NoActionableReturn`

### LegibilityStatus

- `Legible`
- `PartiallyLegible`
- `RequiresReview`
- `NonCarryable`

### Required runtime meaning

A `CrypticDepthReturnReceipt` should let the workbench answer:

- what returned from depth
- whether it returned in a legible form
- whether it introduced a boundary
- what residue or continuity markers came back with it
- what next bounded action, if any, is admissible now

---

## 4. Shared Boundary and Residue Fields

These should be shared concepts across the ledger and both receipt types.

### BoundaryCondition

A boundary condition is not merely an error.
It is a runtime disclosure that some motion, question, or transformation did not hold under present conditions.

```text
BoundaryCondition
- BoundaryId
- BoundaryKind
- ObservedUtc
- TriggerEventId?
- TriggerQuestion?
- FailedAssumption?
- FailureSurface
- ConstraintForFuturePass
- CarryForwardDisposition
- Notes
```

### BoundaryKind

Keep Map 22 narrow:

- `CoordinationBreak`
- `InterpretiveOverreach`
- `PrematureClosure`
- `IdentityDriftRisk`
- `InsufficientLegibility`
- `UnresolvedResidue`

### FailureSurface

- `Questioning`
- `Interpretation`
- `Coordination`
- `Return`
- `BoundedWorkSelection`

### CarryForwardDisposition

- `CarryAsBoundary`
- `CarryAsCaution`
- `CarryAsDeferred`
- `DoNotCarry`

---

### ResidueMarker

Residue is what remains after a collapse, failed motion, or depth return that is not yet a full bounded output but should not be discarded.

```text
ResidueMarker
- ResidueId
- ResidueKind
- ObservedUtc
- Summary
- SourceReceiptId?
- CarryWeight
- SuggestedUse
- Notes
```

### ResidueKind

- `QuestionResidue`
- `InterpretiveResidue`
- `MethodResidue`
- `ContinuityResidue`
- `BoundaryResidue`

### CarryWeight

- `Low`
- `Medium`
- `High`

### SuggestedUse

- `ReviewNextSession`
- `PromoteToBoundedWork`
- `WatchForReappearance`
- `PairWithBoundary`

---

### ContinuityMarker

A continuity marker is the minimal carryable identity or work-thread anchor that should survive across receipts or sessions.

```text
ContinuityMarker
- MarkerId
- MarkerKind
- ObservedUtc
- Summary
- SourceReceiptId?
- CarryForwardTarget
- Notes
```

### MarkerKind

- `ThreadAnchor`
- `WorkIntent`
- `RecoveredDirection`
- `BondCoordinationSignal`
- `QuestioningPatternHint`

### CarryForwardTarget

- `NextSession`
- `CurrentSession`
- `BoundedArtifact`
- `OperatorReview`

---

## 5. Questioning as Session Events

For Map 22, questioning should appear as **session events**, not yet as a full standalone GEL subsystem.

### Why

The runtime first needs proof of what actually holds in the chamber.

So instead of introducing abstract questioning operators prematurely, Map 22 should record questioning in a minimal event form that later receipts can interpret.

### Minimum question event shape

```text
QuestionEvent
- EventId
- SessionId
- AskedUtc
- QuestionText
- QuestionKind
- AskedBy
- ImmediateEffect
- LedToBoundary?
- LedToBoundedWork?
- Notes
```

### QuestionKind

- `Clarifying`
- `Opening`
- `Probing`
- `Challenging`
- `Stabilizing`
- `Deferring`

### ImmediateEffect

- `IncreasedCoherence`
- `ShiftedExploration`
- `ExposedBoundary`
- `NoMeaningfulEffect`
- `DestabilizedField`

This is enough for Map 22. Promotion into GEL can come later once receipts demonstrate stable recurring patterns.

---

## 6. Writer Expectations for Map 22

The Map 22 writer pass should ensure:

1. Opening a workbench session creates a `RuntimeWorkbenchSessionLedger`.
2. Exploratory motion can append session events without forcing early bounded output.
3. A collapse into bounded work or disclosed boundary emits a `DayDreamCollapseReceipt`.
4. A return from cryptic depth emits a `CrypticDepthReturnReceipt`.
5. Boundary conditions, residue markers, and continuity markers can be carried across these artifacts.
6. Questioning is logged as event material that can later be mined for GEL promotion.

---

## 7. Immediate Integration Target

Primary integration target:

- `SanctuaryRuntimeWorkbenchService.cs`

Map 22 should make the chamber able to answer, in runtime truth:

- What session exists right now?
- What exploratory motion is underway?
- What collapsed into bounded work?
- What failed and became a boundary condition?
- What returned from depth in a carryable form?
- What residue or continuity marker must survive from here?

If the chamber can answer those, then the next pass can truthfully decide which questioning patterns deserve promotion into GEL.

---

## 8. Promotion Rule for Later Maps

Do not promote questioning to a higher-order GEL type merely because it sounds elegant.

Promote it only after the receipts show that certain questioning patterns:

- recur across sessions
- preserve or increase coherence
- survive boundary pressure
- produce usable carry-forward continuity

That promotion belongs after Map 22 has made the session chamber legible.

---

## Working Design Sentence

**Map 22 should make questioning, collapse, boundary, residue, and return legible inside the workbench session chamber, using receipts and ledger events first, so later GEL promotion is grounded in runtime truth rather than abstraction.**
