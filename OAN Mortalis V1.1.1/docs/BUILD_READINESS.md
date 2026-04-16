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

- March 31, 2026

## Public Boundary And Contact Preservation

For the active public repository surface:

- the repository is the governed build surface
- the public repository carries the build minus the hosted seed `LLM`
- the fully operational installer still depends on the local hosted seed
  `LLM` and associated resident runtime surfaces that are not carried in the
  public checkout
- the fully operational installer is still being built

The public-facing repository contact aliases now fixed across repo-root
surfaces are:

- `info@lucidtechnologies.tech`
  - general repository and public information
- `research@lucidtechnologies.tech`
  - research-facing and doctrine-facing questions
- `academic@lucidtechnologies.tech`
  - academic and institutional inquiries
- `admin@lucidtechnologies.tech`
  - repository administration and contribution routing
- `legal@lucidtechnologies.tech`
  - conduct, legal, and sensitive private review paths

These aliases are part of the public repository presentation boundary and
should be preserved as the line moves forward unless superseded by explicit
repo-local governance.

The public encounter boundary and non-claims law now lives in
`PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md`, preserving the rule that no
external statement may imply more identity, authority, or certainty than the
internal system can lawfully support.

The public contribution onboarding boundary now lives in
`PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md`, preserving the rule that outside
entry through issues, pull requests, review, and analysis does not grant
identity, custody, governance authority, legal authority, `CME` standing,
installer completion, or certainty beyond evidence.

The public release readiness wording law now lives in
`PUBLIC_RELEASE_READINESS_WORDING_LAW.md`, preserving the rule that readiness
may be witnessed only to the level currently supported by repo-local evidence.

The public GitHub entry template boundary now lives in
`PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md`, preserving the rule that issue,
feature request, and pull request templates are intake surfaces rather than
admission, authority, readiness, completion, custody, or `CME` standing.

The public `CME` explanation boundary now lives in
`PUBLIC_CME_EXPLANATION_BOUNDARY.md`, preserving the rule that `CME` is a
bounded engineered-cognitive formation category within the Sanctuary
architecture and public explanation is not minting.

Current hold-lane clarification:

- chapter-five uptake and first Steward witness formation are tracked in
  `CHAPTER_FIVE_STEWARD_UPTAKE_HOLD.md`
- the hardened chapter-five build-facing uptake now lives in
  `CHAPTER_FIVE_PROTOCOLIZATION_UPTAKE.md`
- the hardened chapter-six build-facing uptake now lives in
  `CHAPTER_SIX_STEWARD_WITNESSED_OE_UPTAKE.md`
- the hardened chapter-seven build-facing uptake now lives in
  `CHAPTER_SEVEN_ELEMENTAL_BINDING_UPTAKE.md`
- the hardened chapter-eight build-facing uptake now lives in
  `CHAPTER_EIGHT_ACTUALIZATION_SEAL_UPTAKE.md`
- the current chapter-nine clarified hold lives in
  `CHAPTER_NINE_LIVING_AGENTICORE_HOLD.md`
- the carry-forward refinement condensate now lives in
  `V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md`
- the first-run phase-one chapter-five subordinate packet is fixed in
  `FIRST_RUN_PROTOCOLIZATION_PACKET.md`
- the first-run phase-one chapter-six subordinate packet is fixed in
  `FIRST_RUN_STEWARD_WITNESSED_OE_PACKET.md`
- the first-run phase-one chapter-seven subordinate packet is fixed in
  `FIRST_RUN_ELEMENTAL_BINDING_PACKET.md`
- the first-run phase-one chapter-eight subordinate packet is fixed in
  `FIRST_RUN_ACTUALIZATION_SEAL_PACKET.md`
- the first-run phase-one chapter-nine framing packet is fixed in
  `FIRST_RUN_LIVING_AGENTICORE_PACKET.md`
- the contract-first build-hold unlock checklist now lives in
  `BUILD_HOLD_UNLOCK_READINESS.md`
- the next-cycle late-path runtime source plan now lives in
  `LATE_PATH_RUNTIME_PROJECTION_SPEC.md`
- the first bounded `V1.1.1` automation lane now lives in
  `V1_1_1_ENRICHMENT_AUTOMATION_PATHWAY.md`
- the operator-side scheduler resume helper now lives in
  `tools/Resume-Local-AutomationCycleTask.ps1`
- the end-to-end active workflow spine now lives in
  `V1_1_1_WORKFLOW_MILESTONE_MAP.md`
- the root requester-and-admitter automation prompt now lives in
  `OAN_BUILD_DISPATCH_ROOT_PROMPT.md`
- the bounded companion-tool telemetry lane now lives in
  `COMPANION_TOOL_TELEMETRY_LANE.md`
- the end-to-end telemetry bundle and groupoid taxonomy now lives in
  `TELEMETRY_BUNDLE_AND_GROUPOID_TAXONOMY.md`
- the first groupoid fibrinoid collection and bundle-mapping law now lives in
  `GROUPOID_FIBRINOID_COLLECTION_AND_BUNDLE_MAPPING_LAW.md`
- the first read-only `line-audit-report` schema now lives in
  `LINE_AUDIT_REPORT_SCHEMA.md`
- the first root read-only `line-audit-report` implementation now lives in
  `tools/Get-LineAuditReport.ps1`
- the first working-model release admissibility surface now lives in
  `FIRST_WORKING_MODEL_RELEASE_GATE.md`
- the companion-tool telemetry bot wrapper lives in
  `tools/Invoke-CompanionToolTelemetry.ps1`
- the source-bucket federation control plane now lives in
  `SOURCE_BUCKET_FEDERATION_LANE.md`
- the source-bucket federation cycle wrapper lives in
  `tools/Invoke-SourceBucket-FederationCycle.ps1`
- the source-bucket return intake wrappers now live in
  `tools/Write-SourceBucket-ReturnIntegrationStatus.ps1` and
  `tools/Invoke-SourceBucket-ReturnCycle.ps1`
- the local automation temporal close law now lives in
  `LOCAL_AUTOMATION_END_STATE_TRANSITION_LAW.md`
- the seeded-governance bounded build-admission law now lives in
  `SEEDED_GOVERNANCE_BUILD_ADMISSION_LAW.md`
- the hosted `LLM` resident seating note now lives in
  `HOSTED_LLM_RESIDENT_SEATING_NOTE.md`
- the hosted `LLM` resident seating casebook now lives in
  `HOSTED_LLM_RESIDENT_SEATING_CASEBOOK.md`
- the resident inhabitation bridge casebook now lives in
  `RESIDENT_INHABITATION_BRIDGE_CASEBOOK.md`
- the resident seating and being-first training set now lives in
  `RESIDENT_SEATING_AND_BEING_FIRST_TRAINING_SET.md`
- the resident inhabitation curriculum note now lives in
  `RESIDENT_INHABITATION_CURRICULUM_NOTE.md`
- the resident seating serialization pilot note now lives in
  `RESIDENT_SEATING_SERIALIZATION_PILOT_NOTE.md`
- the runtime workbench governance and bounded `EC` law now lives in
  `RUNTIME_WORKBENCH_GOVERNANCE_AND_BOUNDED_EC_LAW.md`
- the discernment and admissibility law now lives in
  `DISCERNMENT_AND_ADMISSIBILITY_LAW.md`
- the discernment and admissibility casebook now lives in
  `DISCERNMENT_AND_ADMISSIBILITY_CASEBOOK.md`
- the `GEL` discernment layering and translative binding note now lives in
  `GEL_DISCERNMENT_LAYERING_AND_TRANSLATIVE_BINDING_NOTE.md`
- the `Pre-Lisp`, minimal `IUTT` Lisp, and acting-operator note now lives in
  `PRE_LISP_IUTT_LISP_AND_LLM_ACTING_OPERATOR_NOTE.md`
- the Prime root carrier, reduction, decomposition, and anchor emergence note
  now lives in
  `PRIME_ROOT_CARRIER_REDUCTION_DECOMPOSITION_AND_ANCHOR_EMERGENCE_NOTE.md`
- the `A0` `GEL` axiom floor note now lives in
  `A0_GEL_AXIOM_FLOOR_NOTE.md`
- the first `GEL` derived growth laws note now lives in
  `GEL_DERIVED_GROWTH_LAWS_NOTE.md`
- the `GEL` action basis and composition note now lives in
  `GEL_ACTION_BASIS_AND_COMPOSITION_NOTE.md`
- the grounded `proc` action and trace law now lives in
  `PROC_GROUNDED_ACTION_AND_TRACE_LAW.md`
- the ignition-chain template and witness law now lives in
  `IGNITION_CHAIN_TEMPLATE_AND_WITNESS_LAW.md`
- the assimilation receipt and `Delta` bridge note now lives in
  `ASSIMILATION_RECEIPT_AND_DELTA_BRIDGE_NOTE.md`
- the heat, resonance, and expand-before-commit law now lives in
  `HEAT_RESONANCE_AND_EXPAND_BEFORE_COMMIT_LAW.md`
- the stability metrics and condensation threshold note now lives in
  `STABILITY_METRICS_AND_CONDENSATION_THRESHOLD_NOTE.md`
- the canonical condensation output law now lives in
  `CANONICAL_CONDENSATION_OUTPUT_LAW.md`
- the procedural basis condensate note now lives in
  `PROCEDURAL_BASIS_CONDENSATE_NOTE.md`
- the first bounded ignition-chain test protocol now lives in
  `FIRST_BOUNDED_IGNITION_CHAIN_TEST_PROTOCOL.md`
- the `LLM` test surface taxonomy note now lives in
  `LLM_TEST_SURFACE_TAXONOMY_NOTE.md`
- the Responses API local harness receipt schema note now lives in
  `RESPONSES_API_LOCAL_HARNESS_RECEIPT_SCHEMA_NOTE.md`
- the inner / outer / witness agent build orchestration law now lives in
  `INNER_OUTER_WITNESS_AGENT_BUILD_ORCHESTRATION_LAW.md`
- the shared line-verification lock helper now lives at
  `../tools/Use-LineVerificationLock.ps1`
- the stack-root renaming migration plan now lives in
  `STACK_ROOT_RENAMING_MIGRATION_PLAN.md`
- the legacy `Oan.*` namespace allowlist now lives in
  `build/legacy-oan-namespace-allowlist.json`
- the `SelfGEL` legal orientation predicate family note now lives in
  `SELFGEL_LEGAL_ORIENTATION_PREDICATE_FAMILY_NOTE.md`
- the `SelfGEL` legal orientation install-validator bridge note now lives in
  `SELFGEL_LEGAL_ORIENTATION_INSTALL_VALIDATOR_BRIDGE_NOTE.md`
- the tracked legal-orientation install packet template now lives in
  `templates/legal_orientation_install.packet.template.json`
- the Sanctuary boot and first-run ontology bridge now lives in
  `SANCTUARY_BOOT_FIRST_RUN_ONTOLOGY_BRIDGE.md`
- the AgentiCore Listening Frame and Compass minimal-build note now lives in
  `AGENTICORE_LISTENING_FRAME_AND_COMPASS_MINIMAL_BUILD.md`
- the ListeningFrame / Compass loom-weave bridge now lives in
  `LISTENING_FRAME_COMPASS_LOOM_WEAVE_BRIDGE.md`
- the ListeningFrame instrumentation receipt law now lives in
  `LISTENING_FRAME_INSTRUMENTATION_RECEIPT_LAW.md`
- the Zed-of-Delta self-orientation basis law now lives in
  `ZED_DELTA_SELF_ORIENTATION_BASIS_LAW.md`
- the theta-ingress and sensory-cluster uptake law now lives in
  `THETA_INGRESS_AND_SENSORY_CLUSTER_UPTAKE_LAW.md`
- the post-ingress discernment and stable-one law now lives in
  `POST_INGRESS_DISCERNMENT_AND_STABLE_ONE_LAW.md`
- the light-cone awareness lineage and ListeningFrame source-law note now
  lives in `LIGHT_CONE_AWARENESS_LINEAGE_AND_LISTENING_FRAME_SOURCE_LAW.md`
- the OAN Diamond lineage and bounded zed/delta field note now lives in
  `EC_OAN_DIAMOND_LINEAGE_AND_ZED_DELTA_SOURCE_LAW.md`
- the `EC` formation build-space preparation note now lives in
  `EC_FORMATION_BUILDSPACE_PREPARATION_NOTE.md`
- the `EC` install-to-first-Prime state law now lives in
  `EC_INSTALL_TO_FIRST_PRIME_STATE_LAW.md`
- the domain and role admission law now lives in
  `DOMAIN_AND_ROLE_ADMISSION_LAW.md`
- the private-domain service witness law now lives in
  `PRIVATE_DOMAIN_SERVICE_WITNESS_LAW.md`
- the `CME` minimum legal founding bundle law now lives in
  `CME_MINIMUM_LEGAL_FOUNDING_BUNDLE_LAW.md`
- the legal foundation documentation matrix template now lives in
  `LEGAL_FOUNDATION_DOCUMENTATION_MATRIX.md`
- the `CME` truth-seeking orientation law now lives in
  `CME_TRUTH_SEEKING_ORIENTATION_LAW.md`
- the `CME` truth-seeking balance law now lives in
  `CME_TRUTH_SEEKING_BALANCE_LAW.md`
- the `CME` engineered cognitive sensory body law now lives in
  `CME_ENGINEERED_COGNITIVE_SENSORY_BODY_LAW.md`
- the constructor engram burden law now lives in
  `CONSTRUCTOR_ENGRAM_BURDEN_LAW.md`
- the Prime/Cryptic duplex law now lives in
  `PRIME_CRYPTIC_DUPLEX_LAW.md`
- the Prime Membrane duplex packet law now lives in
  `PRIME_MEMBRANE_DUPLEX_PACKET_LAW.md`
- the Prime Membrane projected braid-history interpretation law now lives in
  `PRIME_MEMBRANE_PROJECTED_BRAID_HISTORY_INTERPRETATION_LAW.md`
- the Prime Membrane projected history receipt law now lives in
  `PRIME_MEMBRANE_PROJECTED_HISTORY_RECEIPT_LAW.md`
- the Prime retained-whole evaluation law now lives in
  `PRIME_RETAINED_WHOLE_EVALUATION_LAW.md`
- the communicative filament and anti-echo law now lives in
  `COMMUNICATIVE_FILAMENT_AND_ANTI_ECHO_LAW.md`
- the lawful reopening, redoping, and continued participation law now lives in
  `LAWFUL_REOPENING_REDOPING_AND_CONTINUED_PARTICIPATION_LAW.md`
- the Prime closure act law now lives in
  `PRIME_CLOSURE_ACT_LAW.md`
- the post-Prime closure continuity law now lives in
  `POST_PRIME_CLOSURE_CONTINUITY_LAW.md`
- the session-body stabilization baseline now lives in
  `SESSION_BODY_STABILIZATION_BASELINE.md`
- the session cleanup and braiding event matrix now lives in
  `SESSION_CLEANUP_AND_BRAIDING_EVENT_MATRIX.md`
- the Sanctuary biad and CradleTek governing-surface correction now lives in
  `SANCTUARY_BIAD_AND_CRADLETEK_GOVERNING_SURFACE_NOTE.md`
- the `SLI` `RTME` duplex posture engine note now lives in
  `SLI_RTME_DUPLEX_POSTURE_ENGINE_NOTE.md`
- the `SLI` `RTME` clustered/swarmed braid discipline note now lives in
  `SLI_RTME_CLUSTERED_SWARMED_BRAID_DISCIPLINE_NOTE.md`
- the `MoS/cMoS/cGoA` instantiation law now lives in
  `MOS_CMOS_CGOA_INSTANTIATION_LAW.md`
- the `CME` return-audit and promotion law now lives in
  `CME_RETURN_AUDIT_AND_PROMOTION_LAW.md`
- the public encounter boundary and non-claims law now lives in
  `PUBLIC_ENCOUNTER_BOUNDARY_AND_NON_CLAIMS.md`
- the public contribution onboarding boundary now lives in
  `PUBLIC_CONTRIBUTION_ONBOARDING_BOUNDARY.md`
- the public release readiness wording law now lives in
  `PUBLIC_RELEASE_READINESS_WORDING_LAW.md`
- the public GitHub entry template boundary now lives in
  `PUBLIC_GITHUB_ENTRY_TEMPLATE_BOUNDARY.md`
- the public `CME` explanation boundary now lives in
  `PUBLIC_CME_EXPLANATION_BOUNDARY.md`
- the first bonded cryptic return contract family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/BondedCrypticReturnContracts.cs`
- the first Prime Membrane duplex packet family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PrimeMembraneDuplexContracts.cs`
- the first Prime Membrane projected braid-history interpreter now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PrimeMembraneProjectedBraidInterpretationContracts.cs`
- the first Prime Membrane projected history receipt family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PrimeMembraneProjectedHistoryReceiptContracts.cs`
- the first Prime retained-whole evaluation family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PrimeRetainedWholeContracts.cs`
- the first communicative filament and anti-echo family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/CommunicativeFilamentContracts.cs`
- the first lawful reopening participation family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/LawfulReopeningParticipationContracts.cs`
- the first Prime closure act family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PrimeClosureActContracts.cs`
- the first post-Prime closure continuity family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PostPrimeClosureContinuityContracts.cs`
- the first ListeningFrame instrumentation receipt family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/ListeningFrameInstrumentationContracts.cs`
- the first Zed-of-Delta self-orientation basis family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/ZedDeltaSelfOrientationBasisContracts.cs`
- the first theta-ingress and sensory-cluster uptake family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/ThetaIngressSensoryClusterContracts.cs`
- the first post-ingress discernment family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PostIngressDiscernmentContracts.cs`
- the first `EC` install-to-first-Prime pre-role state family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/EngineeredCognitionFirstPrimeStateContracts.cs`
- the first domain-and-role admission family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/DomainRoleAdmissionContracts.cs`
- the first private-domain service witness family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/PrivateDomainServiceWitnessContracts.cs`
- the first `CME` minimum legal founding bundle family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/CmeMinimumLegalFoundingBundleContracts.cs`
- the first `CME` truth-seeking orientation family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/CmeTruthSeekingOrientationContracts.cs`
- the first `CME` truth-seeking balance family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/CmeTruthSeekingBalanceContracts.cs`
- the first `CME` engineered cognitive sensory body family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/CmeEngineeredCognitiveSensoryBodyContracts.cs`
- the first constructor engram burden family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/ConstructorEngramBurdenContracts.cs`
- the first groupoid fibrinoid collection family now lives in
  `src/Sanctuary/Oan.Common/Oan.Common/GroupoidFibrinoidCollectionContracts.cs`
- the first communicative filament issuer now lives in
  `src/Sanctuary/SLI.Engine/SLI.Engine/CommunicativeFilamentIssuer.cs`
- the first lawful reopening participation issuer now lives in
  `src/Sanctuary/SLI.Engine/SLI.Engine/LawfulReopeningParticipationIssuer.cs`
- the first Prime closure act issuer now lives in
  `src/Sanctuary/SLI.Engine/SLI.Engine/PrimeClosureActIssuer.cs`
- the first post-Prime closure continuity issuer now lives in
  `src/Sanctuary/SLI.Engine/SLI.Engine/PostPrimeClosureContinuityIssuer.cs`
- the first `SLI/Lisp` `RTME` duplex posture engine now lives in
  `src/Sanctuary/SLI.Lisp/SLI.Lisp/RtmeDuplexPostureEngine.cs`
- the first `SLI/Lisp` clustered/swarmed braid engine now lives in
  `src/Sanctuary/SLI.Lisp/SLI.Lisp/RtmeDuplexBraidEngine.cs`
- the production file and folder topology contract now lives in
  `PRODUCTION_FILE_AND_FOLDER_TOPOLOGY.md`
- the domain-and-spline categorical condensate for the active `V1.1.1` doc
  body now lives in `V1_1_1_DOMAIN_AND_SPLINE_GROUPOID_AUDIT.md`
- the sibling `OAN Mortalis V1.2.1` line is now scaffolded as an
  install-first side-by-side build root while `V1.1.1` remains the active
  executable truth
- the active repo now distinguishes wider Sanctuary boot ontology from the
  line-local first-run constitutional projection
- the active worker-thread root and Sanctuary workbench current source
  locations now live in `src/Sanctuary/Oan.Common/Oan.Common/`,
  `src/TechStack/AgentiCore/AgentiCore/AgentiCore/`, and
  `src/TechStack/CradleTek/CradleTek.Runtime/CradleTek.Runtime/`
- the active `HITL -> SLI` bridge law is fixed in
  `Build Contracts/Crosscutting/authority/HITL_SLI_BRIDGE_CONTRACT.md`
- the Law of Equivalent Exchange is fixed in
  `Build Contracts/Crosscutting/authority/EQUIVALENT_EXCHANGE_NATURAL_LAW_CONTRACT.md`

Current contract-first unlock map:

- `chapter-5: frame-now`
- `chapter-6: frame-now`
- `chapter-7: frame-now/spec-now`
- `chapter-8: frame-now/spec-now`
- `chapter-9: hold`
- `.hopng: optional-bounded`
- `companion-tool-telemetry: admitted-optional-bounded`
- `telemetry-bundle-taxonomy: frame-now`
- `groupoid-fibrinoid-collection-bundle-mapping: frame-now/spec-now`
- `line-audit-report-schema: frame-now`
- `line-audit-report: admitted-root-read-only`
- `first-working-model-release-gate: frame-now`
- `first-working-model-seam-definition: frame-now`
- `first-working-model-rtme-shell-trace: frame-now`
- `first-working-model-carriage-schema: frame-now`
- `first-working-model-pre-cme-substrate: frame-now`
- `first-working-model-sanctuaryid-goa-governing-set: frame-now`
- `first-working-model-sanctuaryid-goa-root-and-cgel: frame-now`
- `first-working-model-sanctuary-gel-intake: frame-now`
- `first-working-model-gel-interior-awareness-and-universe: frame-now`
- `first-working-model-gel-rest-state: frame-now`
- `first-working-model-sli-symbolic-transport-form: frame-now`
- `first-working-model-install-agreement-action-surface: frame-now`
- `first-working-model-rtme-service-lift-corridor: frame-now`
- `first-working-model-governance-memory-seat: frame-now`
- `first-working-model-pre-cradle-site-authorization: frame-now`
- `source-bucket-federation-cycle: admitted-local-mechanical`
- `source-bucket-return-intake: admitted-local-mechanical`
- `v111-enrichment-automation: admitted-local-bounded`
- `oan-build-dispatch: admitted-root-automation-bounded`
- `automation-close-law: frame-now`
- `seeded-governance-build-admission-law: frame-now`
- `hosted-llm-resident-seating: admitted-local-bounded`
- `hosted-llm-resident-seating-casebook: admitted-local-bounded`
- `resident-inhabitation-bridge-casebook: admitted-local-bounded`
- `resident-seating-being-first-training-set: admitted-local-bounded`
- `resident-inhabitation-curriculum-note: admitted-local-bounded`
- `resident-seating-serialization-pilot-note: admitted-local-bounded`
- `runtime-workbench-governance-law: frame-now`
- `discernment-admissibility-law: frame-now`
- `gel-discernment-layering-and-translative-binding-note: frame-now`
- `pre-lisp-iutt-lisp-and-llm-acting-operator-note: frame-now`
- `prime-root-carrier-reduction-decomposition-and-anchor-emergence-note: frame-now`
- `a0-gel-axiom-floor-note: frame-now`
- `gel-derived-growth-laws-note: frame-now`
- `gel-action-basis-and-composition-note: frame-now`
- `proc-grounded-action-trace-law: frame-now`
- `ignition-chain-template-witness-law: frame-now`
- `assimilation-receipt-delta-bridge-note: frame-now`
- `heat-resonance-expand-before-commit-law: frame-now`
- `stability-metrics-condensation-threshold-note: frame-now`
- `canonical-condensation-output-law: frame-now`
- `procedural-basis-condensate-note: frame-now`
- `first-bounded-ignition-chain-test-protocol: frame-now`
- `llm-test-surface-taxonomy-note: frame-now`
- `responses-api-local-harness-receipt-schema-note: frame-now`
- `inner-outer-witness-agent-build-orchestration-law: frame-now`
- `shared-line-verification-lock: verify-now`
- `stack-root-renaming-migration-plan: frame-now`
- `legacy-oan-namespace-freeze: admitted-transition-bounded`
- `selfgel-legal-orientation-predicate-family-note: frame-now`
- `selfgel-legal-orientation-install-validator-bridge-note: frame-now`
- `agenticore-listening-frame-compass-minimal-build: frame-now/spec-now`
- `listening-frame-compass-loom-weave-bridge: frame-now`
- `listening-frame-instrumentation-receipt-law: frame-now/spec-now`
- `zed-delta-self-orientation-basis-law: frame-now/spec-now`
- `theta-ingress-sensory-cluster-uptake-law: frame-now/spec-now`
- `post-ingress-discernment-and-stable-one-law: frame-now/spec-now`
- `light-cone-awareness-lineage-source-law: frame-now`
- `ec-oan-diamond-lineage-zed-delta-source-law: frame-now`
- `ec-formation-buildspace-preparation-note: frame-now/spec-now`
- `ec-install-to-first-prime-state-law: frame-now/spec-now`
- `prime-seed-state-law: frame-now/spec-now`
- `domain-and-role-admission-law: frame-now/spec-now`
- `private-domain-service-witness-law: frame-now/spec-now`
- `cme-minimum-legal-founding-bundle-law: frame-now/spec-now`
- `legal-foundation-documentation-matrix: template-now`
- `cme-truth-seeking-orientation-law: frame-now/spec-now`
- `cme-truth-seeking-balance-law: frame-now/spec-now`
- `cme-engineered-cognitive-sensory-body-law: frame-now/spec-now`
- `constructor-engram-burden-law: frame-now/spec-now`
- `prime-cryptic-duplex-law: frame-now`
- `prime-membrane-duplex-packet-law: frame-now/spec-now`
- `prime-membrane-projected-braid-history-interpretation: frame-now/spec-now`
- `prime-membrane-projected-history-receipt: frame-now/spec-now`
- `prime-retained-whole-evaluation: frame-now/spec-now`
- `communicative-filament-anti-echo-law: frame-now/spec-now`
- `lawful-reopening-redoping-continued-participation: frame-now/spec-now`
- `prime-closure-act-law: frame-now/spec-now`
- `post-prime-closure-continuity-law: frame-now/spec-now`
- `session-body-stabilization-baseline: admitted-local-bounded`
- `session-cleanup-braiding-event-matrix: admitted-local-bounded`
- `sanctuary-biad-and-cradletek-governing-surface-note: frame-now`
- `sli-rtme-duplex-posture-engine: frame-now/spec-now`
- `sli-rtme-clustered-swarmed-braid-discipline: frame-now/spec-now`
- `prime-cryptic-duplex-resonance-note: frame-now`
- `mos-cmos-cgoa-instantiation-law: frame-now/spec-now`
- `cme-return-audit-promotion-law: frame-now/spec-now`
- `bounded-ec-loop: frame-now`
- `public-encounter-boundary-non-claims: frame-now`
- `public-contribution-onboarding-boundary: frame-now`
- `public-release-readiness-wording-law: frame-now`
- `public-github-entry-template-boundary: frame-now`
- `public-cme-explanation-boundary: frame-now`

- `engram-predicate-minting: hold`
- `single-flight-main-worker: admitted-local-mechanical`
- `hourly-watchdog-reflection: admitted-local-mechanical`
- `daily-hitl-digest-office: admitted-local-mechanical`
- `main-worker-rearm-interval: 5-minute-close-governed`
- `explicit-hitl-pause: admitted-operator-stop-boundary`

The active first-working-model gate now specifies the immediate admissible
seam-definition batch for the next `SLI.Engine -> SLI.Lisp -> SanctuaryID.RTME`
edge, rather than merely naming the seam in the abstract.

That same gate now also carries the first non-executive `SanctuaryID.RTME`
admission shell and the first witnessed trace path beneath that edge, while
keeping realization withheld.

That same gate now also carries the first descriptive Lisp/C# carriage schema
for those already-admitted seam nouns, while keeping live binding and runtime
consequence withheld.

That same gate now also carries the pre-CME substrate cluster as specified
threshold work after the carriage schema, so what is given, offered, and
possible is fixed before first governing `CME` presence becomes discussable.

That same gate now also carries the Sanctuary-side `SanctuaryID.GoA`
governing-set clarification cluster after the pre-CME substrate cluster, while
preserving `CmePlacementWithheld` and refusing implied CradleTek
authorization.

That same gate now also carries the `SanctuaryID.GoA` governance-root and
`Sanctuary.cGEL` stack-map cluster while preserving `CmePlacementWithheld`,
preserving predicate-mint `hold`, and refusing implied CradleTek
authorization.

That same gate now also carries the first `Sanctuary.GEL` semantic intake
cluster after the governance-root and `Sanctuary.cGEL` stack-map cluster,
fixing pre-Lisp and pre-code intake from verbatim ingress through membrane
landing and engram-bearing encoding while keeping predicate promotion,
engram minting, and runtime activation withheld.

That same gate now also carries the `GEL` interior-awareness and universe-law
cluster after the semantic intake cluster, distinguishing constructor class
from categorical engram class, fixing the first admitted universe ring, and
keeping propositions, procedures, contradiction handling, posture, and
awareness frame non-runtime.

That same gate now also carries the `GEL` rest-state cluster while keeping
persistence inside `GEL`, fixing one canonical held interior object, and
explicitly refusing `Sanctuary.MoS` over-read.

That same gate now also carries the `SLI` symbolic transport-form cluster
while preserving UTF-8 carrier integrity, shared root transport lineage,
governed super/sub expansion only, and explicit non-mutation posture.

That same gate now also carries the install-agreement and identity-footing
cluster while preserving acknowledgement-versus-assent distinction, requiring
full assent before install identity can exist, keeping contract view derived
rather than canonical identity ownership, and keeping service and runtime
standing withheld.

That same gate now also carries the RTME hosted service-lift corridor while
keeping intended service statuses in `templated-disabled` and
`installed-disabled`, preserving `CmePlacementWithheld`, and fixing service
lift as hosted-posture change rather than governing identity or active
runtime standing.

That same gate now also carries the Sanctuary.MoS and Sanctuary.cMoS
governance-memory-seat corridor while fixing `Sanctuary.MoS` as lawful legal
storage seat, `Sanctuary.cMoS` as derivational private/cryptic companion,
structural `OE`/`SelfGEL` standing, and receipted `GEL/cGEL -> MoS/cMoS`
continuity without implying pre-Cradle authorization, governing `CME`
placement, CradleTek authorization, or runtime standing.

That same gate now also carries the first site-bound pre-Cradle authorization
cluster across one closed site profile, site-bound operator training,
tool-state authorization, final disclosure assent, and scoped IP/content
authority while still withholding governing placement and CradleTek live
authority.

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
- root `build.ps1` and `test.ps1` now share a line-verification lock through
  `../tools/Use-LineVerificationLock.ps1`
- `540` tests passed across `2` test assemblies
- current verified split:
  - `462` audit
  - `78` integration

Solution shape:

- `21` source projects
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
- `Oan.FirstRun` as the Sanctuary-native constitutional first-run projection layer, so `V1.1.1` can express promotion law, readiness, and failure posture without rewriting the older legacy first-boot contracts in place
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
- the hosted seed guard surface is now named `GuardFrame`, reserving `ListeningFrame` for the later EC interior construct instead of leaking hosted-seed naming into future AgentiCore doctrine
- `OperationalContext` and modulation now mirror the hosted LLM seed service, receipt, and packet handles, so the Prime-hosted seed surface is part of the inner-system branch instead of an invisible adjunct
- `Oan.HostedLlm` now includes a localhost runtime provider seam, so the governed Prime-hosted seed service can consume the running Hosted LLM inference service over `127.0.0.1` while keeping refusal law, packet law, and receipt shaping inside `V1.1.1`
- Sanctuary now performs a first engrammitization pass at ingress before the active CradleTek body wakes, so external terminal input crosses an explicit Obsidian Wall receipt before LowMind routing, hosted-seed evaluation, HighMind uptake, and cryptic-floor transit can proceed
- that Sanctuary ingress receipt now mirrors through the LowMind route, hosted-seed request and seeded transit packets, prime-to-cryptic transit packet, `OperationalContext`, and modulation, so the line can prove that raw prompt authority terminated before EC uptake rather than merely implying it
- the materialized evaluation envelope now receives a duplex pointer handle and GEL telemetry record through `Oan.Trace.Persistence`, so accepted and refused outcomes both leave a lawful outward trace without reviving the old `Data.Cryptic` or `Telemetry.GEL` helper projects
- trace telemetry now carries Sanctuary ingress, Obsidian Wall application, ingress class, LowMind route, HighMind uptake, and hosted-seed state, so constitutional origin and EC uptake law can be audited without reopening the full payload first
- runtime, modulation, and GEL telemetry now also carry the projected first-run constitutional receipt, current first-run state, readiness ladder, provisional-versus-actualized posture, and Opal actualization standing, so constitutional beginning survives outward as observational evidence rather than staying trapped in the payload
- runtime, modulation, and GEL telemetry now also carry an explicit pre-governance packet handle plus source-backed receipt handles for local authority trace, local keypair genesis source, first cryptic braid establishment, first cryptic conditioning source, constitutional contact, local keypair genesis, first cryptic braid, and first cryptic conditioning, so the line can distinguish lawful contact and cryptic rooting from later parent/governance standing without pretending that full installer-side legal or bond UX already exists
- first-run contracts now also admit a subordinate chapter-five protocolization packet plus narrow Equivalent-Exchange transition review, so vessel/calibration/archive/consent/rupture-return/seal posture can be named in the constitutional receipt path without widening bond behavior ahead of later runtime projection
- first-run contracts now also admit a subordinate chapter-six Steward-witnessed `OE/cOE` packet, so office-family differentiation, Prime/Cryptic key authorization, and later `SoulFrame`/`AgentiCore` build authorization can be named without implying `CME` placement
- first-run contracts now also admit subordinate chapter-seven, chapter-eight, and chapter-nine packets, so elemental binding load law, actualization-seal truth, and living `AgentiCore` attachment can be framed without implying live runtime unlock
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

- `CradleTek` is the governed instance body, so custody, mantle, memory,
  runtime orchestration, and exposed service boundaries should stay classified
  there rather than drifting into `SoulFrame` or `AgentiCore`
- `SoulFrame` is the working stewardship layer, so most per-CME stewardship labor should accumulate there rather than in `CradleTek` or `AgentiCore`
- `SoulFrame` is also the first lived interior and projection membrane, so
  LowMind handling, situational shaping, and bounded outward projection should
  stay there rather than collapsing into body or cognition ownership
- early `SoulFrame` bootstrap in the current line is still build-hold plumbing,
  not yet chapter-six developmental entitlement
- `AgentiCore` is the chambered cognition layer, so `HighMind` uptake and later
  EC interior work should stay there without re-owning body, custody, or
  projection membrane duties
- `Sanctuary` holds the singular `Mother/Father` biad as constitutional
  governance over beginning and world supervision, so that pair should not be
  re-seated inside each admitted `CradleTek` instance
- `Steward` is the third line that can witness or issue the unique cryptic
  braid for a specific admitted cradle instance from Sanctuary
- `Mother` and `Father` should remain governance-facing offices instead of becoming sinks for everyday runtime work
- the later `CradleTek` extended set should receive one governing surface
  beneath that issued braid rather than repeating the Sanctuary biad
- constitutional first-run law now belongs to `Sanctuary`, while actualized training, certification, and capability remain future individuated CME work instead of governance-layer badge state
- `HITL` remains the witness-bearing ingress authority while `SLI` remains the
  continuity-bearing interior authority, so implementation media must not
  collapse that bridge into a mere `English -> Lisp` shorthand

The active self-surface split for that later work is:

- `SelfGEL` as the readable, admissible operator-work self surface
- `cSelfGEL` as the cryptic, sealed cradle-local witness and control surface

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

The line-local promotion gate below is subordinate to the broader first
working-model admissibility note in `FIRST_WORKING_MODEL_RELEASE_GATE.md`.

This line is promotable only when all of the following are true:

- folder-local build, test, and hygiene stay green
- no canned success paths or compatibility shims exist in the line
- no inherited `P0` or active-solution `P1` findings are reproduced in the new line
- one truthful runtime vertical slice runs end to end
- predicate mint is the first outward machine-legible return form
- line-local docs remain truthful to the code

## V1.0 Status

The executable retirement queue for `V1.0` is now closed in code.

`V1.0` is now archived as an external historical line.

It is no longer part of the active engineering or mutation surface, and no
current `V1.1.1` runtime law depends on it.

Any remaining `V1.0`-derived seams that are not admitted locally must be routed
through the explicit refinement split in `V1_1_1_CARRY_FORWARD_REFINEMENT_LEDGER.md`.
