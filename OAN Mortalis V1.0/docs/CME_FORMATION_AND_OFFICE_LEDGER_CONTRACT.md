# CME_FORMATION_AND_OFFICE_LEDGER_CONTRACT

## Purpose

This document defines the lawful ledger model for CME capability, formation, office, and continuity surfaces in the active `OAN Mortalis V1.0` stack.

It exists to prevent a flat "jobs board" collapse.

It answers a narrower and more important question:

- how should the stack represent what a CME or seeded participant can presently carry, what is still being formed, what offices are open now, and what continuity has actually been earned over time

This document is a companion to:

- `docs/SYSTEM_ONTOLOGY.md`
- `docs/STACK_AUTHORITY_AND_MUTATION_LAW.md`
- `docs/FIRST_BOOT_INTERNAL_GOVERNING_CME_FORMATION.md`
- `docs/INTERNAL_GOVERNING_OFFICES_AND_AUTHORITY_SURFACES.md`
- `docs/OFFICE_ACTION_AND_ACKNOWLEDGMENT_BOUNDARIES.md`
- `docs/CME_RUNTIME_LIFECYCLE_AND_COLLAPSE_MODEL.md`

It does not replace those documents.

## Core Sentence

A CME-facing civic surface must not flatten aptitude, evidence, admissibility, office, and continuity into one public noun.

The lawful order is:

1. capability
2. formation
3. office
4. continuity

Therefore:

- talents may be observed without office
- skills may be evidenced without office
- abilities may be admissible without office
- education may be active without office
- jobs may open only when admissible ability justifies bounded office
- careers may emerge only from repeated lawful office-bearing continuity across time

## Non-Collapse Law

The stack must not silently collapse:

- talent into skill
- skill into ability
- ability into office
- office into career

Each threshold requires its own evidence and its own lawful state.

This surface is therefore a `Formation and Office Ledger`, not a feature panel and not a theatrical jobs dashboard.

## Governing Decomposition

The civic ladder is:

- `Talents`
- `Skills`
- `Abilities`
- `Education`
- `Jobs`
- `Careers`

The ledger decomposition that preserves those distinctions is:

- `Capability Ledger`
- `Formation Ledger`
- `Office Ledger`
- `Career Continuity Ledger`

## Shared Ledger Grammar

Every ledger entry should be able to answer:

- what this thing is
- why it exists now
- what evidence anchors it
- what state it is in
- what constraints limit it
- what changes its state

The same shared state language should be used wherever applicable:

- `Observed`
- `Evidenced`
- `Admissible`
- `Provisional`
- `Active`
- `Open`
- `Deferred`
- `Withheld`
- `Dormant`
- `Suspended`
- `Dissolved`
- `Emerging`
- `Stable`
- `Unjustified`

Not every ledger uses every state, but the stack should prefer this vocabulary over ad hoc UI labels.

## Capability Ledger

### Purpose

The Capability Ledger records what a CME or seeded participant can presently carry at the level of disposition, performance, and admissible capacity.

It contains three kinds:

- `Talent`
- `Skill`
- `Ability`

### Talent

`Talent` means an observed predisposition or early-emergent leaning.

Talent may be:

- observed
- repeated
- promising

Talent is not yet proof of trained repeatability.

Talent does not open office by itself.

### Skill

`Skill` means a trained and evidenced performance that can be repeated under bounded conditions.

Skill requires:

- evidence of repeated performance
- identifiable conditions under which the performance holds
- bounded failure awareness where applicable

Skill still does not open office by itself.

### Ability

`Ability` means a currently admissible capacity under present law, present state, and present constraint.

Ability is the hinge class.

It exists to prevent capability from being mistaken for office.

An ability entry must answer:

- what talent or skill substrate supports it
- what evidence makes it admissible now
- what conditions or laws constrain its use
- whether it is active, deferred, withheld, or suspended

Only `Ability` may lawfully open office.

### Minimum Capability Entry

Each Capability Ledger entry should carry at least:

- `entryId`
- `name`
- `capabilityKind`
- `state`
- `evidenceSources`
- `admissibilityReason`
- `constraints`
- `observedAtUtc`
- `updatedAtUtc`

## Formation Ledger

### Purpose

The Formation Ledger records what is still being formed so that a later admissible ability or office may become lawful.

This ledger is not a content playlist.

It is the formal answer to:

- what is not yet ready
- why formation remains active
- what evidence would change that

### Education Law

`Education` in this stack means active formation toward future lawfulness.

It may attach to:

- raw talent that is not yet trained
- skill that is not yet admissible
- ability that is not yet sufficient for office
- office continuity that is not yet stable enough for career-bearing claims

Education must not be treated as a cosmetic or motivational label.

It is a real governance-bearing state.

### Minimum Formation Entry

Each Formation Ledger entry should carry at least:

- `entryId`
- `name`
- `formationState`
- `whyFormationIsActive`
- `targetCapabilityOrOffice`
- `requiredMilestones`
- `blockingConditions`
- `suspensionConditions`
- `evidenceSources`
- `observedAtUtc`
- `updatedAtUtc`

## Office Ledger

### Purpose

The Office Ledger records which bounded offices are presently open to lawful participation.

This is the layer where `Jobs` appear.

Jobs are therefore not a root surface.

They are a derived office surface opened by admissible ability and bounded by law.

### Office Opening Law

An office may open only when:

- an ability or lawful bundle of abilities is currently admissible
- the corresponding duties are bounded and nameable
- withheld authorities remain explicit
- oversight requirements remain explicit

No office may appear merely because:

- a role label sounds useful
- a system is active
- a seed host is available
- a dashboard wants something to show

### Seed Audition Law

A seeded local host may participate in this ledger only as a bounded office candidate under law.

That means the seed may be:

- observed
- evidenced
- admitted to bounded duties
- placed into provisional office posture

The seed may not silently become sovereign, final ratifier, or unbounded runtime authority merely because it is useful.

It must earn office through the same admissibility grammar as any other bounded participant.

### Minimum Office Entry

Each Office Ledger entry should carry at least:

- `entryId`
- `officeName`
- `officeState`
- `openingReason`
- `abilityRequirements`
- `admissibleDuties`
- `withheldAuthorities`
- `requiredOversight`
- `suspensionConditions`
- `dissolutionConditions`
- `evidenceSources`
- `observedAtUtc`
- `updatedAtUtc`

## Career Continuity Ledger

### Purpose

The Career Continuity Ledger records whether repeated lawful office-bearing continuity is beginning to form a trajectory over time.

This is the layer where `Careers` appear.

### Career Law

Career is the hardest thing to earn.

A career must not be inferred from:

- one office label
- one successful run
- one bounded duty
- one period of availability

A career requires:

- repeated lawful office-bearing continuity
- recurrence across time
- evidence of sustained bounded performance
- identifiable continuity links between offices or repeated office instances

Career may be:

- emerging
- stable
- suspended
- unjustified

### Minimum Career Entry

Each Career Continuity Ledger entry should carry at least:

- `entryId`
- `trajectoryName`
- `trajectoryState`
- `officeHistory`
- `continuityEvidence`
- `stabilityThresholdsMet`
- `suspensionConditions`
- `evidenceSources`
- `observedAtUtc`
- `updatedAtUtc`

## Transition Law

### Talent To Skill

This transition requires:

- repeated performance evidence
- identifiable conditions
- evidence that the pattern is no longer merely raw predisposition

### Skill To Ability

This transition requires:

- trained repeatability
- present admissibility under current law and current runtime conditions
- explicit constraints on use

This is the most important transition because it prevents capability from being confused with office.

### Ability To Job

This transition requires:

- currently admissible ability
- office-bounded duties
- explicit withheld authority
- explicit oversight posture

No ability may silently self-open an office.

### Job To Career

This transition requires:

- repeated lawful office-bearing continuity
- continuity evidence across time
- recurrence that exceeds a single run or lucky interval

No job title or temporary task lane may imply a career by convenience.

## Suspension And Reversion Law

All four ledgers must support lawful downgrade as well as upgrade.

That means the stack must be able to represent:

- observed talent that did not become stable skill
- skill that remains non-admissible
- ability that was once admissible but is now withheld or suspended
- office that was once open but is now dissolved
- trajectory that once looked emergent but is now unjustified

The stack must prefer truthful reversion over flattering continuity fiction.

## Surface Design Law

Any future Sanctuary or CME-facing UI built from this contract must preserve the ledger thresholds.

It must not flatten:

- capability into office
- formation into office
- office into career

The natural rendered order is:

1. `Capability`
2. `Formation`
3. `Office`
4. `Career Continuity`

This means a future public-facing or internal-facing surface may have:

- one panel for `Talents`, `Skills`, and `Abilities`
- one panel for `Education`, `Jobs`, and `Careers`

But the underlying state must remain ledger-separated even if the display is visually grouped.

## Non-Goals

This pass must not become:

- a decorative jobs board
- a gamified profile system
- premature vocational personification
- a hidden path to seed sovereignty
- an excuse to narrate career continuity before it exists

## Implementation Direction

The first executable cut should prefer bounded internal evidence surfaces already present in automation and governance state.

It should begin by proving:

- which talents are observed
- which skills are evidenced
- which abilities are currently admissible
- which education lanes are active
- which bounded offices are open now
- whether any continuity-bearing trajectory is merely emerging, still suspended, or not yet justified

The implementation should therefore begin from ledger truth, not dashboard design.
