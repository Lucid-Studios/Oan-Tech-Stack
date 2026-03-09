# SEED_LEMMA_ROOT_SET

## Purpose

This document defines the first curated lemma basis for the seed `GEL`.

It exists to prove that:

- the active English intake path can land on stable lemma roots
- the first seed `GEL` can begin from a bounded semantic basis rather than a giant lexical dump
- symbolic constructor and reserved-domain law can be applied to a trusted root subset before broader symbolic contextualization generation begins

The canonical machine-readable artifact for this pass is:

- `public_root/seed/SeedLemmaRoots.json`

This document is the policy and curation note that explains that artifact.

## Canonical Lemma Policy

The seed set follows the active lemma-root constitution:

- lemmas are lowercase canonical root anchors
- inflected forms are examples, not identity
- surface words do not override lemma identity
- phrasal verbs are excluded unless the current intake path already resolves them stably as one root
- polysemous English forms are admitted only when the current intake path resolves them to one stable seed root without heuristic guessing
- ambiguous lemma candidates are deferred, not forced into the seed

The intake path remains:

1. English input
2. lemma landing
3. root lookup
4. operator/body compatibility
5. only later broader `EngramDraft` expansion

The first translation proof lane does not mutate this seed manifest.

It uses:

- the canonical `350`-root seed set as the primary atlas-facing basis
- a test-scoped narrative overlay only for missing roots in the fixed three-sentence fixture

That overlay is supplementary and non-canonical. It exists only to prove the first bounded English intake -> constructor body -> `EngramDraft` lane without contaminating the seed `GEL` foundation.

## Seed Selection Criteria

The first seed is curated from the current canonical candidate pool in:

- `public_root/GEL.ndjson`

It is not imported from external concept dictionaries.

Selection rules:

- structural usefulness wins over raw frequency alone
- all admitted lemmas already exist in the current candidate pool
- every admitted lemma has a deterministic bootstrap symbolic core handle
- every admitted lemma has a stable primary category
- reserved-domain collisions are either excluded or admitted only as explicit `bridge-only` roots

## Manifest Structure

Each admitted seed entry in `SeedLemmaRoots.json` carries:

- `lemma`
- `symbolicCore`
- `primaryCategory`
- `secondaryCategories`
- `domainDescriptor`
- `operatorCompatibility`
- `variantExamples`
- `reservedDomainStatus`
- `disciplinaryReservations`

For this phase:

- `symbolicCore` uses deterministic bootstrap handles of the form `atlas.core::<lemma>`
- `reservedDomainStatus` may be only:
  - `none`
  - `bridge-only`
- `disciplinaryReservations` is empty for `none` roots and explicit for `bridge-only` roots

## Category Quotas

The admitted set contains exactly `350` lemmas:

- `50` action roots
- `50` relation roots
- `50` transformation roots
- `50` state roots
- `50` measurement roots
- `50` classification roots
- `50` observation roots

Secondary categories are descriptive only and do not count toward the quota totals.

## Operator Compatibility Policy

The first seed uses only these compatibility values:

- `core-only`
- `prefix-capable`
- `suffix-capable`
- `prefix-suffix-capable`

These values describe whether the current canonical atlas surface and its observed lexical variants already support lawful constructor expansion for a lemma root.

They do not authorize symbolic contextualization generation yet.

## Reserved-Domain Policy

The seed set must preserve the active symbolic constitution:

- grammar operator space is globally reserved
- root cores are atlas-owned
- disciplinary domains are reserved by default
- governance/meta symbols remain reserved
- experimental extension space remains separate

Admitted seed roots must therefore be either:

- `none`
  - general-root-safe for this phase
- `bridge-only`
  - lawful only with explicit disciplinary reservation recorded in the manifest

Roots that collide with disciplinary or governance domains are excluded from the manifest and listed below as `excluded-this-phase`.

## Admitted Primary Sets

### Action

`accept`, `act`, `add`, `admit`, `advise`, `apply`, `ask`, `build`, `call`, `change`, `choose`, `claim`, `collect`, `complete`, `create`, `decide`, `define`, `deliver`, `describe`, `develop`, `discover`, `divide`, `drive`, `establish`, `evaluate`, `explain`, `form`, `guide`, `handle`, `identify`, `improve`, `include`, `infer`, `interpret`, `join`, `learn`, `manage`, `move`, `navigate`, `operate`, `organize`, `produce`, `protect`, `receive`, `record`, `remove`, `respond`, `solve`, `support`, `validate`

### Relation

`above`, `across`, `against`, `align`, `among`, `around`, `associate`, `attach`, `balance`, `before`, `behind`, `below`, `beneath`, `beside`, `between`, `beyond`, `bind`, `bridge`, `by`, `combine`, `connect`, `contain`, `contrast`, `correlate`, `correspond`, `couple`, `depend`, `differ`, `during`, `from`, `inside`, `integrate`, `into`, `link`, `map`, `match`, `mediate`, `merge`, `near`, `of`, `off`, `over`, `pair`, `relate`, `separate`, `through`, `toward`, `under`, `within`, `without`

### Transformation

`adapt`, `alter`, `assemble`, `automate`, `bend`, `blend`, `calibrate`, `clarify`, `compress`, `convert`, `deform`, `derive`, `edit`, `enlarge`, `evolve`, `expand`, `extend`, `fabricate`, `fold`, `generate`, `grow`, `heal`, `increase`, `innovate`, `modulate`, `modify`, `mutate`, `optimise`, `process`, `reconcile`, `reduce`, `refine`, `reform`, `regenerate`, `repair`, `replace`, `reshape`, `restore`, `revise`, `rotate`, `shift`, `simulate`, `transition`, `translate`, `transmute`, `transpose`, `turn`, `update`, `vary`, `weave`

### State

`able`, `absent`, `active`, `alive`, `available`, `aware`, `calm`, `certain`, `clear`, `closed`, `common`, `complex`, `constant`, `current`, `dense`, `direct`, `dynamic`, `empty`, `equal`, `external`, `false`, `finite`, `formal`, `free`, `full`, `general`, `high`, `idle`, `intact`, `internal`, `latent`, `live`, `local`, `neutral`, `normal`, `open`, `possible`, `present`, `private`, `ready`, `real`, `safe`, `stable`, `steady`, `strong`, `true`, `valid`, `visible`, `weak`, `whole`

### Measurement

`amount`, `area`, `average`, `breadth`, `capacity`, `charge`, `count`, `cost`, `degree`, `diameter`, `distance`, `duration`, `energy`, `extent`, `flow`, `force`, `frequency`, `height`, `index`, `level`, `limit`, `load`, `magnitude`, `mass`, `measure`, `number`, `pace`, `period`, `position`, `pressure`, `quantity`, `radius`, `range`, `rate`, `ratio`, `scale`, `score`, `size`, `span`, `speed`, `strength`, `sum`, `temperature`, `time`, `total`, `value`, `velocity`, `volume`, `weight`, `yield`

### Classification

`attribute`, `basis`, `catalogue`, `category`, `character`, `class`, `classify`, `cluster`, `code`, `concept`, `criteria`, `descriptor`, `differentiate`, `distinguish`, `enumerate`, `example`, `family`, `feature`, `file`, `genre`, `group`, `kind`, `label`, `list`, `matrix`, `model`, `name`, `order`, `pattern`, `profile`, `prototype`, `qualify`, `rank`, `represent`, `rule`, `schema`, `scheme`, `select`, `series`, `sort`, `species`, `standard`, `structure`, `symbol`, `tag`, `taxonomy`, `term`, `type`, `unit`, `version`

### Observation

`analyse`, `assess`, `attend`, `audit`, `capture`, `check`, `compare`, `detect`, `estimate`, `examine`, `expect`, `experience`, `explore`, `glimpse`, `hear`, `inspect`, `investigate`, `know`, `listen`, `locate`, `look`, `monitor`, `note`, `notice`, `observe`, `perceive`, `predict`, `probe`, `query`, `question`, `read`, `recognize`, `reflect`, `report`, `review`, `sample`, `scan`, `search`, `see`, `sense`, `signal`, `study`, `survey`, `test`, `trace`, `track`, `verify`, `view`, `watch`, `witness`

## Excluded This Phase

The following current candidate roots are intentionally excluded from the first seed manifest:

| Candidate | Reason |
| --- | --- |
| `theorem` | disciplinary reservation: mathematics |
| `calculus` | disciplinary reservation: mathematics |
| `statute` | disciplinary reservation: law |
| `jurisprudence` | disciplinary reservation: law |
| `diagnosis` | disciplinary reservation: medicine |
| `surgery` | disciplinary reservation: medicine |
| `molecule` | disciplinary reservation: science |
| `quark` | disciplinary reservation: physics |
| `isotope` | disciplinary reservation: physics/science |
| `variable` | unresolved ambiguity plus disciplinary reservation pressure |

These exclusions are not rejection of the roots themselves.

They are a phase-bounded refusal to let the first seed manifest silently absorb disciplinary symbol domains before explicit bridge policy and deeper symbolic governance are ready.

## Immediate Consequence

The first seed `GEL` now has a bounded semantic basis that is:

- lemma-anchored
- category-balanced
- symbolically governed
- reserved-domain aware
- small enough to trust
- large enough to support later `EngramDraft` expansion without lexical chaos
