# BUILD_READINESS

## Purpose

This document records the build posture of `OAN Mortalis V1.1.1` as an independent sibling build line.

It answers:

1. Is the line independently buildable right now?
2. What has been carried forward into the new line?
3. What must still mature before promotion?

## Scope

Active line-local solution:

- `OAN Mortalis V1.1.1/Oan.sln`

Assessment date:

- March 27, 2026

## Current Verified State

Verified from the `OAN Mortalis V1.1.1` folder root:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\test.ps1 -Configuration Release
powershell -ExecutionPolicy Bypass -File .\tools\verify-private-corpus.ps1
```

Verified result:

- build succeeded
- tests succeeded
- hygiene succeeded
- `85` tests passed across `2` test assemblies

Solution shape:

- `20` source projects
- `2` test projects
- operational tree normalized under:
  - `src/Sanctuary`
  - `src/TechStack`
  - `tests/Sanctuary`

## Carried Public Surface

The initial carried law surface is limited to:

- standing and gate-law contracts
- cryptic derivation directives, result shapes, and receipts
- protected execution receipts and act-family vocabulary
- predicate-mint contracts
- `Oan.Nexus.Control` as the Sanctuary-native governing interface layer where Prime, Cryptic, and Steward posture is braided into transition decisions
- `Oan.PrimeCryptic.Services` as the Sanctuary-native always-on Prime/Cryptic broker layer
- `Oan.HostedLlm` as the Sanctuary-native Prime-hosted seed service layer, so governed hosted seed inference no longer lives in the old `V1.0` cognition-host stack
- `Oan.Trace.Persistence` as the Sanctuary-native duplex pointer and GEL telemetry layer, so traceable outward persistence no longer depends on the tiny old `Data.Cryptic` and `Telemetry.GEL` helper projects
- `Oan.Runtime.Materialization` as the Sanctuary-side receipt and envelope assembly helper, so `CradleTek.Runtime` orchestrates flow without becoming a return-shaping sink
- `Oan.State.Modulation` as the governance-readable modulation layer for CME state across industrial through government bands
- `SLI.Lisp` as the Sanctuary-native hosted Cryptic module bundle, so Lisp now stands as an explicit symbolic runtime medium rather than a vague future helper language
- minimum governed membrane and headless transport envelopes
- `CradleTek.Custody` as the dormant custody seam for `GEL/cGEL`, `GOA/cGOA`, `MoS/cMoS`, and `OE/SelfGEL` handle bootstrap
- `CradleTek.Mantle` as the explicit `MoS` seam carried forward from the old CradleTek family, so `MoS`, `OE/cOE`, and `SelfGEL` bootstrap now come from the mantle itself through a source-shaped seam instead of anonymous handle minting or a generic service layer
- `CradleTek.Mantle` now also carries the old append-only `MoS` storage law forward as a braided opal-engram seat, so collapsed CME storage is structural in `V1.1.1` and not only implied by bootstrap receipts
- `CradleTek.Memory` as the first neutral memory carry-forward slice, preserving engram query and self-resolution vocabulary from the old CradleTek layer without pulling old cognition-host coupling or filesystem-probing resolver behavior into the new line
- lawful `CradleTek.Memory` source contracts for engram-corpus and root-atlas loading, so future resolver/cleaver work can depend on shaped snapshot sources instead of probing repo-local `corpus_index` or `public_root` artifacts
- source-backed `CradleTek.Memory` resolver and root ontological cleaver behavior rebuilt on top of those lawful snapshot sources, so old ranking and lexical resolution behavior can return without restoring repo-relative probing
- `CradleTek.Memory` now consumes Sanctuary lexical cue services instead of owning its own query-cue and lexeme engines, so memory remains substrate-shaped while Sanctuary owns ingress-oriented lexical interpretation
- `CradleTek.Mantle` now projects presented validation handles from cryptic `cSelfGEL` handles, so `CradleTek.Memory` no longer owns custody-side cooling behavior
- `SoulFrame.Membrane` now mediates a bounded memory context built from lawful engram-corpus and root-atlas sources, so the cleaned `CradleTek.Memory` layer is part of the living stack instead of a sidecar capability
- the minimal `SoulFrame.Identity` law is now rebuilt into the SoulFrame bootstrap receipt itself, so `V1.1.1` carries the lawful SoulFrame identity seat without needing the old `V1.0` identity registry and context services
- placeholder `Class1.cs` and empty bootstrap marker scaffolding has now been stripped from the line, with audit coverage preventing that residue from creeping back into `src/`
- explicit `cGOA` and `cMoS` first-route hold surfaces in `CradleTek.Custody`, so protected hold posture points at custody law rather than anonymous handles
- `SoulFrame.Bootstrap` as the per-CME bootstrap seam that instantiates a CPU-only SoulFrame posture before cognition
- a thin `SoulFrame.Membrane` mediation layer between `CradleTek.Runtime` and `AgentiCore`
- SoulFrame projection, return-intake, and stewardship receipts now shape the live slice without widening SoulFrame into orchestration or Prime publication
- typed protected-residue evidence now enters from `SLI.Ingestion`, so SoulFrame protected-hold routing no longer depends on a late string heuristic alone
- SoulFrame protected-hold routing now emits first-route `cGOA/cMoS` decisions as explicit membrane receipts and governance-readable modulation fields

The first live runtime vertical slice in this line now proves:

- truthful cryptic-floor refusal when no lawful predicate landing surface exists
- machine-legible predicate mint as the first bounded outward form
- Sanctuary-native Prime/Cryptic residency is explicit and currently CPU-only with no target-bounded lane claimed
- bootstrap admission is now nexus-mediated before SoulFrame membrane activation, so instantiation legality is asked explicitly rather than inferred from successful flow
- bootstrap denial now fails closed at the resident field, so an unlawful membrane wake produces a refusal-bearing vertical slice instead of silently continuing
- `Oan.Nexus.Control` now computes a braided posture snapshot plus explicit transition request and decision, so legality is no longer only distributed across bootstrap, membrane, and modulation
- `Oan.Runtime.Materialization` now mints a minimal braided operational context for the vertical slice, so modular surfaces can read one lawful situational surface instead of reconstructing posture from scattered receipts
- nexus now distinguishes `hold`, `return-path-only`, and `archive-admissible` postures instead of smearing all non-hold outcomes into one return shape
- `CradleTek.Runtime` now delegates receipt hydration and envelope assembly to `Oan.Runtime.Materialization`, keeping orchestration thinner and Sanctuary-side shaping explicit
- governance-readable state modulation exists even when the active slice never leaves bootstrap-ready posture
- CradleTek custody and SoulFrame bootstrap are explicit in the returned vertical slice
- SoulFrame bootstrap now carries an explicit mantle receipt, so `MoS` / `OE/cOE` / `SelfGEL` bootstrap is visible as a first-class CradleTek layer instead of being buried inside raw custody handles or treated like a background service
- SoulFrame bootstrap now also carries a detached identity seat with operator-bond handle, opal-engram seat linkage, and integrity hash, so the old `SoulFrame.Identity` model is preserved as bootstrap law rather than as a separate runtime service family
- the mantle receipt now states that `MoS` is the custody and upkeep seat for governing and Operator-bound `OE/cOE`, with Father governing the cryptic side, Mother governing the Prime side, and the mantle remaining the exclusive in-use model recovery seat for operators and customers
- the mantle receipt now also declares the `OE/SelfGEL` and `cOE/cSelfGEL` groupoids inside one CME mantle seat, so the presented and protected are explicitly braided within `MoS` rather than merely colocated by convention
- the mantle receipt now carries a structural braided opal-engram seat with an append-only public/protected ledger plus a SoulFrame snapshot request, so `MoS` already knows how a CME stages from and collapses back into protected storage without depending on the old `V1.0` host models
- lexical query-cue classification and ontological lexeme normalization now live in Sanctuary `SLI.Ingestion`, which `CradleTek.Memory` consumes rather than reproduces locally
- resident bootstrap memory sources now stand up `CradleTek.Memory` from lawful in-memory corpus and atlas snapshots labeled as the `Lucid Research Corpus`, so the live line does not need repo-relative probing to bring memory online
- SoulFrame now emits a bounded memory context inside the personification-among-work surface, so memory summaries and symbolic roots enter the active slice through a mediated membrane seam rather than sitting beside the nexus branch
- `AgentiCore` now consumes that inline SoulFrame memory plane in its capability and derivation posture, so active cognition no longer behaves as if personified memory were absent from the act path
- `Oan.Runtime.Materialization` now also mints a governed CME formation context inside `OperationalContext`, so the inner-system branch can distinguish civic, local-family, bonded-special-case, and parental-child special-case formation lanes without flattening them into one runtime noun
- the first executable civic ladder is now present as bounded ledger grammar inside that formation context, so `Talents`, `Skills`, `Abilities`, `Education`, bounded `Jobs`, and `Career` continuity can mature in code as lawful state without dragging the old `V1.0` jobs-board posture forward
- local family forms now remain `Civil` and locally governed, while child-facing and bonded CME prompts escalate into `SpecialCases`, preserving the intended separation between unbounded civic forms and special-case best-practice lanes
- Prime/Cryptic residency now carries an explicit hosted `SLI.Lisp` bundle receipt, so the resident field can say plainly that C# is hosting a Cryptic symbolic module environment over SLI without pretending Prime and Cryptic are one runtime
- the active SLI floor now requires the canonical hosted Lisp bundle set to be present before predicate landing can proceed, which makes the new `SLI.Lisp` surface part of executable truth instead of passive doctrine
- `OperationalContext` now carries explicit `PrimeToCrypticTransit` and `CrypticToPrimeTransit` contracts plus hosted request/return packets, so the C# Prime host and hosted Lisp/Cryptic medium no longer relate by implication alone and modulation can mirror both interconnect and packet handles directly
- `AgentiCore` now consults a governed Sanctuary-hosted LLM seed receipt before cryptic floor progression, so Prime-side hosted seed refusal or needs-more-information posture can withhold progression explicitly instead of living only in the old `V1.0` host path
- the hosted seed receipt now mints a direct seeded transit packet for `SLI.Engine`, so the cryptic floor consumes a governed Prime-hosted seed carrier instead of acting on raw input alone
- all terminal-side seed ingress now routes through a SoulFrame `LowMind.SF` packet before `AgentiCore` uptake, so prompt input, tool access, and data access can share one lawful ingress seam even while current runtime entry still defaults to prompt-class traffic
- `CradleTek.Host`, `CradleTek.Runtime`, and the headless runtime now expose explicit prompt, tool-access, and data-access entrypoints, so higher-order ingress no longer exists only as an internal route classification and can be invoked as a real caller surface
- the hosted seed request, seeded transit packet, `OperationalContext`, and modulation receipt now mirror the `LowMind.SF` route handle and route kind, so Prime-hosted seed work and hosted cryptic floor work consume the same SoulFrame ingress decision instead of re-deriving it locally
- `SLI.Engine` now treats the `LowMind.SF` route handle as required ingress law and distinguishes direct prompt transit from higher-order EC transit in its cryptic-floor readiness trace
- `AgentiCore` now mints an explicit `HighMind` uptake context after hosted-seed evaluation, so EC uptake staging stands as its own post-SoulFrame seam instead of being implied by the hosted-seed receipt or the final predicate path
- that `HighMind` context now mirrors the inline SoulFrame memory handle, `LowMind.SF` route handle, and hosted-seed receipt/state into `OperationalContext` and modulation, so the inner-system branch can read EC uptake posture directly without reconstructing it from scattered cognition receipts
- `OperationalContext` and modulation now mirror the hosted LLM seed service, receipt, and packet handles, so the Prime-hosted seed surface is part of the inner-system branch instead of an invisible adjunct
- `Oan.HostedLlm` now includes a localhost runtime provider seam, so the governed Prime-hosted seed service can consume the running Hosted LLM inference service over `127.0.0.1` while keeping refusal law, packet law, and receipt shaping inside `V1.1.1`
- Sanctuary now performs a first engrammitization pass at ingress before the active CradleTek body wakes, so external terminal input crosses an explicit Obsidian Wall receipt before LowMind routing, hosted-seed evaluation, HighMind uptake, and cryptic-floor transit can proceed
- that Sanctuary ingress receipt now mirrors through the LowMind route, hosted-seed request and seeded transit packets, prime-to-cryptic transit packet, `OperationalContext`, and modulation, so the line can prove that raw prompt authority terminated before EC uptake rather than merely implying it
- the materialized evaluation envelope now receives a duplex pointer handle and GEL telemetry record through `Oan.Trace.Persistence`, so accepted and refused outcomes both leave a lawful outward trace without reviving the old `Data.Cryptic` or `Telemetry.GEL` helper projects
- SoulFrame now emits bounded projection, return-intake, and stewardship receipts, including first collapse-readiness posture without direct custody mutation
- SoulFrame stewardship now asks `Oan.Nexus.Control` for collapse-readiness, protected-hold class, and review disposition instead of carrying that policy as a local switch block
- SoulFrame now resolves protected first-route posture toward `cGOA`, `cMoS`, or split first-route holding using typed ingress evidence and explicit custody hold surfaces
- SoulFrame hold routing now asks `Oan.Nexus.Control` for review escalation and evidence disposition, so protected routing remains local while hold-review legality is braided at the Sanctuary interface layer
- SoulFrame now emits a bounded situational context for the active membrane path, so `Oan.Nexus.Control` adjudicates from one lawful membrane surface instead of peeking across full stewardship and hold receipts
- modulation now mirrors the minimal operational context minted from the Nexus braid, so governance reads one exposed situational surface instead of a second local reconstruction
- `SituationalContext` remains the personification-among-work surface while `OperationalContext` remains the inner-system surface, and modulation now reads them as distinct authorities instead of collapsing one into the other
- `SituationalContext` now carries the mediated memory context inline, keeping actionable memory participation inside the personification-among-work surface while `OperationalContext` remains the inner-system read surface
- the host `EvaluateEnvelope` now carries a minimal return-surface context built from those two contexts, so return/archive-facing consumers can read lawful outward posture without reopening the full payload first
- `CradleTek.Host` and the headless entrypoint can now expose that minimal return-surface context directly, so outer consumers can stay on the bounded outward surface instead of deserializing the full evidence payload by default
- the host envelope now also carries a slimmer outbound-object context derived from `ReturnSurfaceContext`, so later archive/return/publication helpers can stay on one bounded outward object instead of peeking back into the full payload or re-deriving lifecycle posture
- `CradleTek.Host` and the headless entrypoint can now expose that slimmer outbound-object context directly too, so outward lifecycle consumers can stay on the most reduced lawful object that still preserves archive/return/hold posture
- the host envelope now also carries a minimal outbound-lane context derived from the outbound object, so later archive/return/publication helpers can ask the simplest lawful question, which lane is admitted now, without reinterpreting broader outward state
- protected execution, derivation, and governance receipts on the same return

Current role split emerging from the carried line:

- `SoulFrame` is the working stewardship layer, so most per-CME stewardship labor should accumulate there rather than in `CradleTek` or `AgentiCore`
- `Mother` and `Father` should remain governance-facing offices instead of becoming sinks for everyday runtime work

The normalized operational tree now makes that split visible in the line itself:

- Sanctuary-native resident and governing services live under `src/Sanctuary`
- stack family runtime ownership lives under `src/TechStack`
- Sanctuary-facing audit and integration proofs live under `tests/Sanctuary`

## Deliberately Excluded

The bootstrap line does not carry:

- compatibility shims
- production stubs and mock engines
- transitional out-of-solution surfaces
- duplicated publishers
- broad automation writer swarms
- narrative Prime projection
- old filesystem-probing `CradleTek.Memory` resolver services that still assume repo-relative `corpus_index` or `public_root/GEL.ndjson` discovery

## Promotion Gate

This line is promotable only when all of the following are true:

- folder-local build, test, and hygiene stay green
- no canned success paths or compatibility shims exist in the line
- no inherited `P0` or active-solution `P1` findings are reproduced in the new line
- one truthful runtime vertical slice runs end to end
- predicate mint is the first outward machine-legible return form
- line-local docs remain truthful to the code

## V1.0 Status

The executable retirement queue for `V1.0` is now closed in code.

`V1.0` is now archived in the repository as a reference-only historical line.

It is no longer part of the active engineering or mutation surface, and no current `V1.1.1` runtime law depends on it.
