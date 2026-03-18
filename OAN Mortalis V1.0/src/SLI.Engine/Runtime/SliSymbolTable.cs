using GEL.Graphs;
using Oan.Common;
using SLI.Engine.Morphology;
using SLI.Engine.Models;
using SoulFrame.Host;
using System.Globalization;

namespace SLI.Engine.Runtime;

public delegate Task<SExpression> SliOperator(
    SExpression expression,
    SliExecutionContext context,
    CancellationToken cancellationToken);

public sealed class SliSymbolTable
{
    private readonly Dictionary<string, SliOperator> _operators = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SliRuntimeOperationClass> _operationClasses = new(StringComparer.OrdinalIgnoreCase);

    public SliSymbolTable()
    {
        RegisterDefaults();
    }

    public void Register(
        string symbol,
        SliOperator handler,
        SliRuntimeOperationClass operationClass = SliRuntimeOperationClass.HostOnly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentNullException.ThrowIfNull(handler);
        _operators[symbol] = handler;
        _operationClasses[symbol] = operationClass;
    }

    public bool TryResolve(string symbol, out SliOperator handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        return _operators.TryGetValue(symbol, out handler!);
    }

    public SliRuntimeCapabilityManifest CreateCapabilityManifest(string runtimeId = "host-sli-interpreter")
    {
        var capabilities = _operators.Keys
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Select(key => new SliRuntimeOperationCapability(
                Opcode: key,
                MeaningAuthority: "host-interpreter",
                Availability: SliRuntimeCapabilityAvailability.Available,
                OperationClass: ResolveOperationClass(key)))
            .ToArray();

        return new SliRuntimeCapabilityManifest(
            runtimeId,
            meaningAuthority: "host-interpreter",
            realizationProfile: SliRuntimeRealizationProfile.CreateHostManaged(),
            capabilities);
    }

    public SliRuntimeCapabilityManifest CreateTargetCapabilityManifest(
        IEnumerable<string> supportedOpcodes,
        string runtimeId = "target-sli-runtime",
        SliRuntimeRealizationProfile? realizationProfile = null)
    {
        ArgumentNullException.ThrowIfNull(supportedOpcodes);

        var supported = supportedOpcodes
            .Where(opcode => !string.IsNullOrWhiteSpace(opcode))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var capabilities = _operators.Keys
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Select(key =>
            {
                var operationClass = ResolveOperationClass(key);
                var availability = operationClass == SliRuntimeOperationClass.HostOnly
                    ? SliRuntimeCapabilityAvailability.Unavailable
                    : supported.Contains(key)
                        ? SliRuntimeCapabilityAvailability.Available
                        : SliRuntimeCapabilityAvailability.Unavailable;

                return new SliRuntimeOperationCapability(
                    Opcode: key,
                    MeaningAuthority: availability == SliRuntimeCapabilityAvailability.Available
                        ? "target-runtime"
                        : "target-runtime-unavailable",
                    Availability: availability,
                    OperationClass: operationClass);
            })
            .ToArray();

        return new SliRuntimeCapabilityManifest(
            runtimeId,
            meaningAuthority: "target-runtime",
            realizationProfile: realizationProfile ?? CreateDerivedTargetRealizationProfile(runtimeId, supported),
            capabilities);
    }

    public IReadOnlyList<string> GetOpcodesByOperationClass(SliRuntimeOperationClass operationClass)
    {
        return _operationClasses
            .Where(entry => entry.Value == operationClass)
            .Select(entry => entry.Key)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void RegisterSharedContract(string symbol, SliOperator handler)
    {
        Register(symbol, handler, SliRuntimeOperationClass.SharedContract);
    }

    private void RegisterTargetCandidate(string symbol, SliOperator handler)
    {
        Register(symbol, handler, SliRuntimeOperationClass.TargetCandidate);
    }

    private SliRuntimeOperationClass ResolveOperationClass(string opcode)
    {
        return _operationClasses.TryGetValue(opcode, out var operationClass)
            ? operationClass
            : SliRuntimeOperationClass.HostOnly;
    }

    private static SliRuntimeRealizationProfile CreateDerivedTargetRealizationProfile(
        string runtimeId,
        ISet<string> supported)
    {
        return SliRuntimeRealizationProfile.CreateTargetBounded(
            profileId: $"{runtimeId}-profile",
            supportsHigherOrderLocality: supported.Any(IsHigherOrderLocalityOpcode),
            supportsBoundedRehearsal: supported.Any(IsRehearsalOpcode),
            supportsBoundedWitness: supported.Any(IsWitnessOpcode),
            supportsBoundedTransport: supported.Any(IsTransportOpcode),
            supportsAdmissibleSurface: supported.Any(IsAdmissibleSurfaceOpcode),
            supportsAccountabilityPacket: supported.Any(IsAccountabilityPacketOpcode),
            supportsResidueEmission: supported.Any(opcode => opcode.EndsWith("-residue", StringComparison.OrdinalIgnoreCase)),
            supportsSymbolicTrace: true,
            maxInstructionCount: Math.Max(32, supported.Count * 2),
            maxSymbolicDepth: Math.Max(16, supported.Count));
    }

    private static bool IsHigherOrderLocalityOpcode(string opcode)
    {
        return opcode.StartsWith("locality-", StringComparison.OrdinalIgnoreCase) ||
               opcode.StartsWith("anchor-", StringComparison.OrdinalIgnoreCase) ||
               opcode.StartsWith("perspective-", StringComparison.OrdinalIgnoreCase) ||
               opcode.StartsWith("participation-", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "seal-posture", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "reveal-posture", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRehearsalOpcode(string opcode)
    {
        return opcode.StartsWith("rehearsal-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWitnessOpcode(string opcode)
    {
        return opcode.StartsWith("witness-", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "glue-threshold", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(opcode, "morphism-candidate", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTransportOpcode(string opcode)
    {
        return opcode.StartsWith("transport-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAdmissibleSurfaceOpcode(string opcode)
    {
        return opcode.StartsWith("surface-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAccountabilityPacketOpcode(string opcode)
    {
        return opcode.StartsWith("packet-", StringComparison.OrdinalIgnoreCase);
    }

    private void RegisterDefaults()
    {
        Register("engram-ref", async (expression, context, cancellationToken) =>
        {
            var concept = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var result = await context.Resolver.ResolveConceptAsync(concept, cancellationToken).ConfigureAwait(false);
            context.AddResolvedEngrams(result.Summaries);
            context.AddTrace($"engram-ref({concept})");
            return SExpression.AtomNode(result.Summaries.Count.ToString());
        });

        Register("context-expand", (expression, context, _) =>
        {
            context.AddTrace($"context-expand({context.Frame.TaskObjective})");
            return Task.FromResult(SExpression.AtomNode("expanded"));
        });

        Register("engram-query", async (expression, context, cancellationToken) =>
        {
            var concept = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var result = await context.Resolver.ResolveConceptAsync(concept, cancellationToken).ConfigureAwait(false);
            context.AddResolvedEngrams(result.Summaries);
            context.AddTrace($"engram-query({concept})");
            return SExpression.AtomNode(result.Summaries.Count.ToString());
        });

        Register("llm_classify", async (expression, context, cancellationToken) =>
        {
            var value = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var response = await context.SemanticDevice.ClassifyAsync(
                    BuildInferenceRequest("classify", value, context),
                    cancellationToken)
                .ConfigureAwait(false);
            context.LastClassifyResponse = response;
            RefreshGoldenCodeContracts(context);
            context.AddTrace(response.Accepted
                ? $"llm_classify({response.Decision})"
                : "llm_classify(refused)");
            return SExpression.AtomNode(response.Accepted ? response.Decision : "llm-refused");
        });

        Register("llm_expand", async (expression, context, cancellationToken) =>
        {
            var value = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var response = await context.SemanticDevice.SemanticExpandAsync(
                    BuildInferenceRequest("semantic_expand", value, context),
                    cancellationToken)
                .ConfigureAwait(false);
            context.AddTrace(response.Accepted
                ? $"llm_expand({response.Decision})"
                : "llm_expand(refused)");
            return SExpression.AtomNode(response.Accepted ? response.Decision : "llm-refused");
        });

        Register("llm_context_infer", async (expression, context, cancellationToken) =>
        {
            var value = UnwrapStringLiteral(Arg(expression, 1, context.Frame.TaskObjective));
            var response = await context.SemanticDevice.InferAsync(
                    BuildInferenceRequest("context_infer", value, context),
                    cancellationToken)
                .ConfigureAwait(false);
            context.AddTrace(response.Accepted
                ? $"llm_context_infer({response.Decision})"
                : "llm_context_infer(refused)");
            return SExpression.AtomNode(response.Accepted ? response.Decision : "llm-refused");
        });

        RegisterSharedContract("predicate-evaluate", (expression, context, _) =>
        {
            var predicate = Arg(expression, 1, "identity-continuity");
            context.AddTrace($"predicate-evaluate({predicate})");
            return Task.FromResult(SExpression.AtomNode("predicate-ok"));
        });

        RegisterSharedContract("decision-evaluate", (expression, context, _) =>
        {
            var branchSet = context.ActiveEngrams
                .OrderByDescending(e => e.ConfidenceWeight)
                .ThenBy(e => e.EngramId, StringComparer.Ordinal)
                .Select(e => $"{e.ConceptTag.ToLowerInvariant()}-path")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();

            if (branchSet.Count == 0)
            {
                branchSet.Add("identity-continuity-path");
            }

            context.CandidateBranches.Clear();
            context.CandidateBranches.AddRange(branchSet);
            context.AddTrace($"decision-evaluate({context.CandidateBranches.Count})");
            return Task.FromResult(SExpression.AtomNode("evaluated"));
        });

        RegisterSharedContract("prime-reflect", (expression, context, _) =>
        {
            var primeState = Arg(expression, 1, "task-objective");
            context.GoldenCodeState.PrimeState = primeState;
            context.AddTrace($"prime-reflect({primeState})");
            return Task.FromResult(SExpression.AtomNode("prime-ready"));
        });

        RegisterSharedContract("zed-listen", (expression, context, _) =>
        {
            var primeState = Arg(expression, 1, "task-objective");
            context.AddTrace($"zed-listen({primeState})");
            return Task.FromResult(SExpression.AtomNode("listening"));
        });

        RegisterSharedContract("delta-differentiate", (expression, context, _) =>
        {
            var primeState = Arg(expression, 1, "task-objective");
            var predicateSet = Arg(expression, 2, "predicate-set");
            var state = context.GoldenCodeState;
            state.ActiveBasin = ResolveGoldenCodeActiveBasin(context, primeState);
            state.CompetingBasin = ResolveGoldenCodeCompetingBasin(context, state.ActiveBasin);
            context.AddTrace($"delta-differentiate({primeState} {predicateSet})");
            return Task.FromResult(SExpression.AtomNode("delta-ready"));
        });

        RegisterSharedContract("sigma-cleave", (expression, context, _) =>
        {
            var reasoningState = Arg(expression, 1, "reasoning-state");
            context.AddTrace($"sigma-cleave({reasoningState})");
            return Task.FromResult(SExpression.AtomNode("sigma-ready"));
        });

        RegisterSharedContract("psi-modulate", (expression, context, _) =>
        {
            var polarity = Arg(expression, 1, "psi-neutral");
            var modulation = Arg(expression, 2, "bounded");
            context.AddTrace($"psi-modulate({polarity}:{modulation})");
            return Task.FromResult(SExpression.AtomNode("psi-ready"));
        });

        RegisterSharedContract("omega-converge", (expression, context, _) =>
        {
            var positive = Arg(expression, 1, "psi-positive");
            var negative = Arg(expression, 2, "psi-negative");
            context.AddTrace($"omega-converge({positive} {negative})");
            return Task.FromResult(SExpression.AtomNode("omega-ready"));
        });

        RegisterSharedContract("theta-seal", (expression, context, _) =>
        {
            var primeState = Arg(expression, 1, "task-objective");
            var reasoningState = Arg(expression, 2, "reasoning-state");
            var state = context.GoldenCodeState;
            state.PrimeState = primeState;
            state.ActiveBasin = ResolveGoldenCodeActiveBasin(context, primeState);
            state.CompetingBasin = ResolveGoldenCodeCompetingBasin(context, state.ActiveBasin);
            state.AnchorState = ResolveGoldenCodeAnchorState(context, state.ActiveBasin, state.CompetingBasin, primeState);
            state.ThetaState = "theta-ready";
            state.IsProjected = true;
            RefreshGoldenCodeContracts(context);
            context.AddTrace($"theta-seal({primeState} {reasoningState} basin={state.ActiveBasin} anchor={state.AnchorState})");
            return Task.FromResult(SExpression.AtomNode("theta-ready"));
        });

        RegisterSharedContract("compass-work", (expression, context, _) =>
        {
            var thetaState = Arg(expression, 1, "theta-state");
            var locality = Arg(expression, 2, "proximal-cognition");
            var state = context.GoldenCodeState;
            state.SelfTouchClass = ResolveGoldenCodeSelfTouchClass(context);
            state.OeCoePosture = ResolveGoldenCodeOeCoePosture(context, state);
            RefreshGoldenCodeContracts(context);
            context.AddTrace($"compass-work({thetaState} {locality} basin={state.ActiveBasin} anchor={state.AnchorState} self-touch={state.SelfTouchClass} posture={state.OeCoePosture})");
            return Task.FromResult(SExpression.AtomNode("compass-live"));
        });

        RegisterSharedContract("gamma-yield", (expression, context, _) =>
        {
            var thetaState = Arg(expression, 1, "theta-state");
            context.GoldenCodeState.GammaState = "gamma-ready";
            RefreshGoldenCodeContracts(context);
            context.AddTrace($"gamma-yield({thetaState})");
            return Task.FromResult(SExpression.AtomNode("gamma-ready"));
        });

        RegisterSharedContract("decision-branch", (expression, context, _) =>
        {
            context.AddTrace($"decision-branch({context.CandidateBranches.Count})");
            return Task.FromResult(SExpression.AtomNode("branched"));
        });

        RegisterSharedContract("compass-update", (expression, context, _) =>
        {
            var arg1 = Arg(expression, 1, "context");
            var arg2 = Arg(expression, 2, "reasoning-state");
            context.AddTrace($"compass-update({arg1} {arg2})");
            return Task.FromResult(SExpression.AtomNode("compass-ok"));
        });

        RegisterSharedContract("cleave", (expression, context, _) =>
        {
            var survivors = context.CandidateBranches.Take(1).ToList();
            var pruned = context.CandidateBranches.Skip(1).ToList();
            context.PrunedBranches.Clear();
            context.PrunedBranches.AddRange(pruned);
            context.CandidateBranches.Clear();
            context.CandidateBranches.AddRange(survivors);
            context.AddTrace($"cleave({context.PrunedBranches.Count})");
            return Task.FromResult(SExpression.AtomNode("cleaved"));
        });

        RegisterSharedContract("commit", (expression, context, _) =>
        {
            context.FinalDecision = context.CandidateBranches.FirstOrDefault() ?? "defer";
            RefreshGoldenCodeContracts(context);
            context.AddTrace($"commit({context.FinalDecision})");
            return Task.FromResult(SExpression.AtomNode(context.FinalDecision));
        });

        RegisterHigherOrderLocalityOperators();
        RegisterBoundedRehearsalOperators();
        RegisterBoundedWitnessOperators();
        RegisterBoundedTransportOperators();
        RegisterBoundedAdmissibilityOperators();
        RegisterBoundedAccountabilityPacketOperators();

        Register("morph-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(root))
            {
                context.MorphologyState.ResolvedLemmaRoots.Add(root);
                context.AddTrace($"morph-root({root})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-root"));
        });

        Register("morph-operator", (expression, context, _) =>
        {
            var token = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var kind = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(kind))
            {
                context.MorphologyState.OperatorAnnotations.Add((token, kind));
                context.AddTrace($"morph-operator({token}:{kind})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-operator"));
        });

        Register("morph-constructor", (expression, context, _) =>
        {
            var role = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var rootKey = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(role) && !string.IsNullOrWhiteSpace(rootKey))
            {
                context.MorphologyState.ConstructorBodies.Add((role, rootKey));
                context.AddTrace($"morph-constructor({role}:{rootKey})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-constructor"));
        });

        Register("morph-predicate-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.PredicateRoot = root;
            context.AddTrace($"morph-predicate-root({root})");
            return Task.FromResult(SExpression.AtomNode("morph-predicate-root"));
        });

        Register("morph-render", (expression, context, _) =>
        {
            var render = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.DiagnosticPredicateRender = render;
            context.AddTrace($"morph-render({render})");
            return Task.FromResult(SExpression.AtomNode("morph-render"));
        });

        Register("morph-summary", (expression, context, _) =>
        {
            var summary = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.Summary = summary;
            context.AddTrace($"morph-summary({summary})");
            return Task.FromResult(SExpression.AtomNode("morph-summary"));
        });

        Register("morph-scalar", (expression, context, _) =>
        {
            var scalar = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.ScalarPayload = scalar;
            context.AddTrace($"morph-scalar({scalar})");
            return Task.FromResult(SExpression.AtomNode("morph-scalar"));
        });

        Register("morph-outcome", (expression, context, _) =>
        {
            var outcome = UnwrapStringLiteral(Arg(expression, 1, "OutOfScope"));
            context.MorphologyState.Outcome = outcome;
            context.AddTrace($"morph-outcome({outcome})");
            return Task.FromResult(SExpression.AtomNode("morph-outcome"));
        });

        Register("morph-graph-edge", (expression, context, _) =>
        {
            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            var relation = UnwrapStringLiteral(Arg(expression, 3, string.Empty));
            if (!string.IsNullOrWhiteSpace(source) &&
                !string.IsNullOrWhiteSpace(target) &&
                !string.IsNullOrWhiteSpace(relation))
            {
                context.MorphologyState.GraphEdges.Add(new ConstructorEdge
                {
                    Source = source,
                    Target = target,
                    Relation = relation
                });
                context.AddTrace($"morph-graph-edge({source}->{target}:{relation})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-graph-edge"));
        });

        Register("morph-anchor", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(anchor))
            {
                context.MorphologyState.ContinuityAnchors.Add(anchor);
                context.AddTrace($"morph-anchor({anchor})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-anchor"));
        });

        Register("morph-invariant", (expression, context, _) =>
        {
            var invariant = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(invariant))
            {
                context.MorphologyState.BodyInvariants.Add(invariant);
                context.AddTrace($"morph-invariant({invariant})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-invariant"));
        });

        Register("morph-cluster-entry", (expression, context, _) =>
        {
            var entry = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(entry))
            {
                context.MorphologyState.ClusterEntries.Add(entry);
                context.AddTrace($"morph-cluster-entry({entry})");
            }

            return Task.FromResult(SExpression.AtomNode("morph-cluster-entry"));
        });

        Register("morph-body-summary", (expression, context, _) =>
        {
            var summary = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.MorphologyState.BodySummary = summary;
            context.AddTrace($"morph-body-summary({summary})");
            return Task.FromResult(SExpression.AtomNode("morph-body-summary"));
        });

        Register("prop-subject-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Subject.RootKey = root;
            context.AddTrace($"prop-subject-root({root})");
            return Task.FromResult(SExpression.AtomNode("prop-subject-root"));
        });

        Register("prop-subject-handle", (expression, context, _) =>
        {
            var handle = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Subject.SymbolicHandle = handle;
            context.AddTrace($"prop-subject-handle({handle})");
            return Task.FromResult(SExpression.AtomNode("prop-subject-handle"));
        });

        Register("prop-predicate-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.PredicateRoot = root;
            context.AddTrace($"prop-predicate-root({root})");
            return Task.FromResult(SExpression.AtomNode("prop-predicate-root"));
        });

        Register("prop-object-root", (expression, context, _) =>
        {
            var root = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Object.RootKey = root;
            context.AddTrace($"prop-object-root({root})");
            return Task.FromResult(SExpression.AtomNode("prop-object-root"));
        });

        Register("prop-object-handle", (expression, context, _) =>
        {
            var handle = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.Object.SymbolicHandle = handle;
            context.AddTrace($"prop-object-handle({handle})");
            return Task.FromResult(SExpression.AtomNode("prop-object-handle"));
        });

        Register("prop-qualifier", (expression, context, _) =>
        {
            var name = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var value = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.PropositionState.Qualifiers.Add(new SliPropositionQualifierResult
                {
                    Name = name,
                    Value = value
                });
                context.AddTrace($"prop-qualifier({name}:{value})");
            }

            return Task.FromResult(SExpression.AtomNode("prop-qualifier"));
        });

        Register("prop-context", (expression, context, _) =>
        {
            var name = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var value = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(name))
            {
                context.PropositionState.ContextTags.Add(new SliPropositionContextTagResult
                {
                    Name = name,
                    Value = value
                });
                context.AddTrace($"prop-context({name}:{value})");
            }

            return Task.FromResult(SExpression.AtomNode("prop-context"));
        });

        Register("prop-render", (expression, context, _) =>
        {
            var render = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.PropositionState.DiagnosticRender = render;
            context.AddTrace($"prop-render({render})");
            return Task.FromResult(SExpression.AtomNode("prop-render"));
        });

        Register("prop-tension", (expression, context, _) =>
        {
            var tension = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(tension))
            {
                context.PropositionState.UnresolvedTensions.Add(tension);
                context.AddTrace($"prop-tension({tension})");
            }

            return Task.FromResult(SExpression.AtomNode("prop-tension"));
        });

        Register("prop-grade", (expression, context, _) =>
        {
            var grade = UnwrapStringLiteral(Arg(expression, 1, SliPropositionalCompileGrade.NeedsSpecification.ToString()));
            context.PropositionState.Grade = grade;
            context.AddTrace($"prop-grade({grade})");
            return Task.FromResult(SExpression.AtomNode("prop-grade"));
        });
    }

    private void RegisterHigherOrderLocalityOperators()
    {
        RegisterTargetCandidate("locality-bind", (expression, context, _) =>
        {
            var localitySeed = UnwrapStringLiteral(Arg(expression, 1, "context"));
            var localityHandle = $"{localitySeed}:{context.Frame.CMEId}:{context.Frame.ContextId:D}";
            context.HigherOrderLocalityState.Reset(localityHandle);
            context.AddTrace($"locality-bind({localitySeed})");
            return Task.FromResult(SExpression.AtomNode("locality-bind"));
        });

        RegisterTargetCandidate("anchor-self", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.SelfAnchor = anchor;
            context.AddTrace($"anchor-self({anchor})");
            return Task.FromResult(SExpression.AtomNode("anchor-self"));
        });

        RegisterTargetCandidate("anchor-other", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.OtherAnchor = anchor;
            context.AddTrace($"anchor-other({anchor})");
            return Task.FromResult(SExpression.AtomNode("anchor-other"));
        });

        RegisterTargetCandidate("anchor-relation", (expression, context, _) =>
        {
            var anchor = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.RelationAnchor = anchor;
            context.AddTrace($"anchor-relation({anchor})");
            return Task.FromResult(SExpression.AtomNode("anchor-relation"));
        });

        RegisterTargetCandidate("seal-posture", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.BoundedSealPosture));
            if (string.Equals(value, SliHigherOrderLocalityState.BoundedSealPosture, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "sealed", StringComparison.OrdinalIgnoreCase))
            {
                state.SealPosture = value.ToLowerInvariant();
            }
            else
            {
                state.SealPosture = ResolveSafeValue(state.SealPosture, SliHigherOrderLocalityState.BoundedSealPosture);
                state.AddWarning($"seal-posture rejected invalid value '{value}'.");
                AddResidue(context, state.Residues, HigherOrderLocalityResidueKind.InvalidPostureValue, "seal-posture", $"Rejected invalid seal posture '{value}'.");
            }

            context.AddTrace($"seal-posture({state.SealPosture})");
            return Task.FromResult(SExpression.AtomNode("seal-posture"));
        });

        RegisterTargetCandidate("reveal-posture", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.MaskedRevealPosture));
            if (string.Equals(value, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "narrow", StringComparison.OrdinalIgnoreCase))
            {
                state.RevealPosture = value.ToLowerInvariant();
            }
            else
            {
                state.RevealPosture = ResolveSafeValue(state.RevealPosture, SliHigherOrderLocalityState.MaskedRevealPosture);
                state.AddWarning($"reveal-posture rejected invalid value '{value}'.");
                AddResidue(context, state.Residues, HigherOrderLocalityResidueKind.InvalidPostureValue, "reveal-posture", $"Rejected invalid reveal posture '{value}'.");
            }

            context.AddTrace($"reveal-posture({state.RevealPosture})");
            return Task.FromResult(SExpression.AtomNode("reveal-posture"));
        });

        RegisterTargetCandidate("perspective-configure", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var missingLocality = string.IsNullOrWhiteSpace(state.LocalityHandle);
            var missingAnchors =
                string.IsNullOrWhiteSpace(state.SelfAnchor) ||
                string.IsNullOrWhiteSpace(state.OtherAnchor) ||
                string.IsNullOrWhiteSpace(state.RelationAnchor);

            if (missingLocality)
            {
                AddResidue(
                    context,
                    state.Perspective.Residues,
                    HigherOrderLocalityResidueKind.MissingLocalityPrerequisites,
                    "perspective-configure",
                    "Perspective configuration requires a bound locality handle.");
            }

            if (missingAnchors)
            {
                AddResidue(
                    context,
                    state.Perspective.Residues,
                    HigherOrderLocalityResidueKind.MissingAnchorPrerequisites,
                    "perspective-configure",
                    "Perspective configuration requires self, other, and relation anchors.");
            }

            if (missingLocality || missingAnchors)
            {
                state.Perspective.IsConfigured = false;
                state.AddWarning("perspective-configure remained incomplete.");
                AddResidue(
                    context,
                    state.Perspective.Residues,
                    HigherOrderLocalityResidueKind.IncompletePerspective,
                    "perspective-configure",
                    "Perspective configuration remained incomplete because prerequisites were missing.");
                context.AddTrace("perspective-configure(incomplete)");
                return Task.FromResult(SExpression.AtomNode("perspective-incomplete"));
            }

            state.Perspective.IsConfigured = true;
            context.AddTrace($"perspective-configure({UnwrapStringLiteral(Arg(expression, 1, "locality-state"))})");
            return Task.FromResult(SExpression.AtomNode("perspective-configure"));
        });

        RegisterTargetCandidate("perspective-orientation", (expression, context, _) =>
        {
            var focus = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var weightValue = UnwrapStringLiteral(Arg(expression, 2, "0.0"));
            if (string.IsNullOrWhiteSpace(focus) ||
                !double.TryParse(weightValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var weight))
            {
                context.HigherOrderLocalityState.AddWarning($"perspective-orientation ignored invalid weight '{weightValue}'.");
                AddResidue(
                    context,
                    context.HigherOrderLocalityState.Perspective.Residues,
                    HigherOrderLocalityResidueKind.IncompletePerspective,
                    "perspective-orientation",
                    $"Perspective orientation rejected invalid weight '{weightValue}'.");
                context.AddTrace("perspective-orientation(invalid)");
                return Task.FromResult(SExpression.AtomNode("perspective-orientation-invalid"));
            }

            context.HigherOrderLocalityState.Perspective.OrientationVector[focus] = weight;
            context.AddTrace($"perspective-orientation({focus}:{weight.ToString("0.0###", CultureInfo.InvariantCulture)})");
            return Task.FromResult(SExpression.AtomNode("perspective-orientation"));
        });

        RegisterTargetCandidate("perspective-constraint", (expression, context, _) =>
        {
            var constraint = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(constraint))
            {
                context.HigherOrderLocalityState.Perspective.EthicalConstraints.Add(constraint);
                context.AddTrace($"perspective-constraint({constraint})");
            }

            return Task.FromResult(SExpression.AtomNode("perspective-constraint"));
        });

        RegisterTargetCandidate("perspective-weight", (expression, context, _) =>
        {
            var name = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var rawValue = UnwrapStringLiteral(Arg(expression, 2, "0.0"));
            if (string.IsNullOrWhiteSpace(name) ||
                !double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                context.HigherOrderLocalityState.AddWarning($"perspective-weight ignored invalid value '{rawValue}'.");
                AddResidue(
                    context,
                    context.HigherOrderLocalityState.Perspective.Residues,
                    HigherOrderLocalityResidueKind.IncompletePerspective,
                    "perspective-weight",
                    $"Perspective weight rejected invalid value '{rawValue}'.");
                context.AddTrace("perspective-weight(invalid)");
                return Task.FromResult(SExpression.AtomNode("perspective-weight-invalid"));
            }

            context.HigherOrderLocalityState.Perspective.WeightFunctions[name] = value;
            context.AddTrace($"perspective-weight({name}:{value.ToString("0.0###", CultureInfo.InvariantCulture)})");
            return Task.FromResult(SExpression.AtomNode("perspective-weight"));
        });

        RegisterTargetCandidate("perspective-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "perspective residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Perspective.Residues,
                HigherOrderLocalityResidueKind.IncompletePerspective,
                "perspective-residue",
                detail);
            context.AddTrace($"perspective-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("perspective-residue"));
        });

        RegisterTargetCandidate("participation-configure", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            if (!state.Perspective.IsConfigured)
            {
                state.Participation.IsConfigured = false;
                state.Participation.Mode = ResolveSafeValue(state.Participation.Mode, SliHigherOrderLocalityState.ObserveMode);
                state.AddWarning("participation-configure remained incomplete because perspective is not configured.");
                AddResidue(
                    context,
                    state.Participation.Residues,
                    HigherOrderLocalityResidueKind.IncompleteParticipation,
                    "participation-configure",
                    "Participation configuration requires completed perspective configuration.");
                context.AddTrace("participation-configure(incomplete)");
                return Task.FromResult(SExpression.AtomNode("participation-incomplete"));
            }

            state.Participation.IsConfigured = true;
            state.Participation.Mode = ResolveSafeValue(state.Participation.Mode, SliHigherOrderLocalityState.ObserveMode);
            context.AddTrace($"participation-configure({UnwrapStringLiteral(Arg(expression, 1, "locality-state"))})");
            return Task.FromResult(SExpression.AtomNode("participation-configure"));
        });

        RegisterTargetCandidate("participation-mode", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.ObserveMode));
            if (string.Equals(value, SliHigherOrderLocalityState.ObserveMode, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "engage", StringComparison.OrdinalIgnoreCase))
            {
                state.Participation.Mode = value.ToLowerInvariant();
            }
            else
            {
                state.Participation.Mode = ResolveSafeValue(state.Participation.Mode, SliHigherOrderLocalityState.ObserveMode);
                state.AddWarning($"participation-mode rejected invalid value '{value}'.");
                AddResidue(
                    context,
                    state.Participation.Residues,
                    HigherOrderLocalityResidueKind.InvalidParticipationMode,
                    "participation-mode",
                    $"Rejected invalid participation mode '{value}'.");
            }

            context.AddTrace($"participation-mode({state.Participation.Mode})");
            return Task.FromResult(SExpression.AtomNode("participation-mode"));
        });

        RegisterTargetCandidate("participation-role", (expression, context, _) =>
        {
            var role = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            context.HigherOrderLocalityState.Participation.Role = role;
            context.AddTrace($"participation-role({role})");
            return Task.FromResult(SExpression.AtomNode("participation-role"));
        });

        RegisterTargetCandidate("participation-rule", (expression, context, _) =>
        {
            var rule = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(rule))
            {
                context.HigherOrderLocalityState.Participation.InteractionRules.Add(rule);
                context.AddTrace($"participation-rule({rule})");
            }

            return Task.FromResult(SExpression.AtomNode("participation-rule"));
        });

        RegisterTargetCandidate("participation-capability", (expression, context, _) =>
        {
            var capability = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(capability))
            {
                context.HigherOrderLocalityState.Participation.CapabilitySet.Add(capability);
                context.AddTrace($"participation-capability({capability})");
            }

            return Task.FromResult(SExpression.AtomNode("participation-capability"));
        });

        RegisterTargetCandidate("participation-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "participation residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Participation.Residues,
                HigherOrderLocalityResidueKind.IncompleteParticipation,
                "participation-residue",
                detail);
            context.AddTrace($"participation-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("participation-residue"));
        });
    }

    private void RegisterBoundedRehearsalOperators()
    {
        RegisterTargetCandidate("rehearsal-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var rehearsal = state.Rehearsal;
            var modeValue = UnwrapStringLiteral(Arg(expression, 2, SliRehearsalState.DreamGameMode));
            var mode = ResolveRehearsalMode(modeValue);
            if (!string.Equals(modeValue, mode, StringComparison.OrdinalIgnoreCase))
            {
                rehearsal.Warnings.Add($"rehearsal-begin rejected invalid mode '{modeValue}'.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.InvalidRehearsalMode,
                    "rehearsal-begin",
                    $"Rejected invalid rehearsal mode '{modeValue}'.");
            }

            var missingPrerequisites =
                string.IsNullOrWhiteSpace(state.LocalityHandle) ||
                !state.Perspective.IsConfigured ||
                !state.Participation.IsConfigured ||
                !HasSafeSealPosture(state.SealPosture);

            if (missingPrerequisites)
            {
                rehearsal.IsConfigured = false;
                rehearsal.Warnings.Add("rehearsal-begin requires completed locality, perspective, participation, and safe seal posture.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.MissingRehearsalPrerequisites,
                    "rehearsal-begin",
                    "Rehearsal requires completed locality, perspective, participation, and safe seal posture.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-begin",
                    "Rehearsal remained incomplete because prerequisites were missing.");
                context.AddTrace("rehearsal-begin(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-incomplete"));
            }

            rehearsal.Configure(state.LocalityHandle, mode);
            context.AddTrace($"rehearsal-begin({mode})");
            return Task.FromResult(SExpression.AtomNode("rehearsal-begin"));
        });

        RegisterTargetCandidate("rehearsal-branch", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            if (!rehearsal.IsConfigured)
            {
                rehearsal.Warnings.Add("rehearsal-branch requires rehearsal-begin.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-branch",
                    "Branch formation requires an active rehearsal.");
                context.AddTrace("rehearsal-branch(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-branch-incomplete"));
            }

            var branch = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(branch))
            {
                rehearsal.BranchSet.Add(branch);
                context.AddTrace($"rehearsal-branch({branch})");
            }

            return Task.FromResult(SExpression.AtomNode("rehearsal-branch"));
        });

        RegisterTargetCandidate("rehearsal-substitute", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            if (!rehearsal.IsConfigured)
            {
                rehearsal.Warnings.Add("rehearsal-substitute requires rehearsal-begin.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-substitute",
                    "Substitution requires an active rehearsal.");
                context.AddTrace("rehearsal-substitute(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-substitute-incomplete"));
            }

            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target))
            {
                rehearsal.SubstitutionLedger.Add(new SliRehearsalSubstitutionState
                {
                    Source = source,
                    Target = target
                });
                context.AddTrace($"rehearsal-substitute({source}->{target})");
            }

            return Task.FromResult(SExpression.AtomNode("rehearsal-substitute"));
        });

        RegisterTargetCandidate("rehearsal-analogy", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            if (!rehearsal.IsConfigured)
            {
                rehearsal.Warnings.Add("rehearsal-analogy requires rehearsal-begin.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.IncompleteRehearsal,
                    "rehearsal-analogy",
                    "Analogy requires an active rehearsal.");
                context.AddTrace("rehearsal-analogy(incomplete)");
                return Task.FromResult(SExpression.AtomNode("rehearsal-analogy-incomplete"));
            }

            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (!string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target))
            {
                rehearsal.AnalogyLedger.Add(new SliRehearsalAnalogyState
                {
                    Source = source,
                    Target = target
                });
                context.AddTrace($"rehearsal-analogy({source}~{target})");
            }

            return Task.FromResult(SExpression.AtomNode("rehearsal-analogy"));
        });

        RegisterTargetCandidate("rehearsal-seal", (expression, context, _) =>
        {
            var rehearsal = context.HigherOrderLocalityState.Rehearsal;
            var value = UnwrapStringLiteral(Arg(expression, 1, SliRehearsalState.IdentitySealed));
            if (string.Equals(value, SliRehearsalState.IdentitySealed, StringComparison.OrdinalIgnoreCase))
            {
                rehearsal.IdentitySeal = SliRehearsalState.IdentitySealed;
            }
            else
            {
                rehearsal.IdentitySeal = SliRehearsalState.IdentitySealed;
                rehearsal.IsBindable = false;
                rehearsal.Warnings.Add($"rehearsal-seal rejected invalid value '{value}'.");
                AddResidue(
                    context,
                    rehearsal.Residues,
                    HigherOrderLocalityResidueKind.InvalidIdentitySeal,
                    "rehearsal-seal",
                    $"Rejected invalid rehearsal identity seal '{value}'.");
            }

            context.AddTrace($"rehearsal-seal({rehearsal.IdentitySeal})");
            return Task.FromResult(SExpression.AtomNode("rehearsal-seal"));
        });

        RegisterTargetCandidate("rehearsal-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "rehearsal residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Rehearsal.Residues,
                HigherOrderLocalityResidueKind.IncompleteRehearsal,
                "rehearsal-residue",
                detail);
            context.AddTrace($"rehearsal-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("rehearsal-residue"));
        });
    }

    private void RegisterBoundedWitnessOperators()
    {
        RegisterTargetCandidate("witness-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var witness = state.Witness;

            var missingPrerequisites =
                string.IsNullOrWhiteSpace(state.LocalityHandle) ||
                !state.Perspective.IsConfigured ||
                !state.Participation.IsConfigured;

            if (missingPrerequisites)
            {
                witness.IsConfigured = false;
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                witness.Warnings.Add("witness-begin requires completed locality, perspective, and participation.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.MissingWitnessPrerequisites,
                    "witness-begin",
                    "Witness formation requires completed locality, perspective, and participation.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-begin",
                    "Witness remained incomplete because prerequisites were missing.");
                context.AddTrace("witness-begin(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-incomplete"));
            }

            var leftToken = UnwrapStringLiteral(Arg(expression, 1, "locality-state"));
            var rightToken = UnwrapStringLiteral(Arg(expression, 2, "locality-state"));

            var leftResolved = TryResolveWitnessReference(context, leftToken, out var leftHandle);
            var rightResolved = TryResolveWitnessReference(context, rightToken, out var rightHandle);
            if (!leftResolved || !rightResolved)
            {
                witness.IsConfigured = false;
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                witness.Warnings.Add("witness-begin rejected an unresolved locality or rehearsal branch reference.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.InvalidWitnessReference,
                    "witness-begin",
                    $"Witness rejected unresolved references '{leftToken}' and/or '{rightToken}'.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-begin",
                    "Witness remained incomplete because a comparison reference could not be resolved.");
                context.AddTrace("witness-begin(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("witness-invalid-reference"));
            }

            witness.Configure(leftHandle, rightHandle);
            context.AddTrace($"witness-begin({leftToken}|{rightToken})");
            return Task.FromResult(SExpression.AtomNode("witness-begin"));
        });

        RegisterTargetCandidate("witness-compare", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.Warnings.Add("witness-compare requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-compare",
                    "Comparison requires an active witness.");
                context.AddTrace("witness-compare(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-compare-incomplete"));
            }

            witness.CandidacyStatus = SliWitnessState.Comparable;
            context.AddTrace($"witness-compare({witness.LeftLocalityHandle}|{witness.RightLocalityHandle})");
            return Task.FromResult(SExpression.AtomNode("witness-compare"));
        });

        RegisterTargetCandidate("witness-preserve", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.Warnings.Add("witness-preserve requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-preserve",
                    "Preservation claims require an active witness.");
                context.AddTrace("witness-preserve(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-preserve-incomplete"));
            }

            var invariant = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(invariant) &&
                !witness.PreservedInvariants.Contains(invariant, StringComparer.OrdinalIgnoreCase))
            {
                witness.PreservedInvariants.Add(invariant);
                context.AddTrace($"witness-preserve({invariant})");
            }

            return Task.FromResult(SExpression.AtomNode("witness-preserve"));
        });

        RegisterTargetCandidate("witness-difference", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.Warnings.Add("witness-difference requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "witness-difference",
                    "Difference claims require an active witness.");
                context.AddTrace("witness-difference(incomplete)");
                return Task.FromResult(SExpression.AtomNode("witness-difference-incomplete"));
            }

            var difference = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(difference) &&
                !witness.DifferenceSet.Contains(difference, StringComparer.OrdinalIgnoreCase))
            {
                witness.DifferenceSet.Add(difference);
                context.AddTrace($"witness-difference({difference})");
            }

            return Task.FromResult(SExpression.AtomNode("witness-difference"));
        });

        RegisterTargetCandidate("witness-residue", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            var detail = UnwrapStringLiteral(Arg(expression, 1, "witness residue"));
            AddResidue(
                context,
                witness.Residues,
                HigherOrderLocalityResidueKind.LawfulDifferenceResidue,
                "witness-residue",
                detail);
            context.AddTrace($"witness-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("witness-residue"));
        });

        RegisterTargetCandidate("glue-threshold", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            var rawValue = UnwrapStringLiteral(Arg(expression, 1, SliWitnessState.DefaultGlueThreshold.ToString("0.00", CultureInfo.InvariantCulture)));
            if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold) ||
                threshold < 0.0 ||
                threshold > 1.0)
            {
                witness.GlueThreshold = SliWitnessState.DefaultGlueThreshold;
                witness.Warnings.Add($"glue-threshold rejected invalid value '{rawValue}'.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.InvalidGlueThreshold,
                    "glue-threshold",
                    $"Rejected invalid glue threshold '{rawValue}'.");
                context.AddTrace($"glue-threshold({witness.GlueThreshold.ToString("0.00", CultureInfo.InvariantCulture)})");
                return Task.FromResult(SExpression.AtomNode("glue-threshold-invalid"));
            }

            witness.GlueThreshold = threshold;
            context.AddTrace($"glue-threshold({threshold.ToString("0.00", CultureInfo.InvariantCulture)})");
            return Task.FromResult(SExpression.AtomNode("glue-threshold"));
        });

        RegisterTargetCandidate("morphism-candidate", (expression, context, _) =>
        {
            var witness = context.HigherOrderLocalityState.Witness;
            if (!witness.IsConfigured)
            {
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                witness.Warnings.Add("morphism-candidate requires witness-begin.");
                AddResidue(
                    context,
                    witness.Residues,
                    HigherOrderLocalityResidueKind.IncompleteWitness,
                    "morphism-candidate",
                    "Morphism candidacy requires an active witness.");
                context.AddTrace("morphism-candidate(incomplete)");
                return Task.FromResult(SExpression.AtomNode("morphism-incomplete"));
            }

            var blockingResidues = witness.Residues.Any(residue =>
                residue.Kind is HigherOrderLocalityResidueKind.MissingWitnessPrerequisites or
                    HigherOrderLocalityResidueKind.InvalidWitnessReference or
                    HigherOrderLocalityResidueKind.InvalidGlueThreshold or
                    HigherOrderLocalityResidueKind.IncompleteWitness);

            var missingPreservedInvariants = RequiredWitnessInvariants()
                .Except(witness.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var disallowedDifferences = witness.DifferenceSet
                .Where(difference => !AllowedWitnessDifferences().Contains(difference, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (blockingResidues)
            {
                witness.CandidacyStatus = SliWitnessState.NonCandidate;
                context.AddTrace("morphism-candidate(non-candidate)");
                return Task.FromResult(SExpression.AtomNode("morphism-non-candidate"));
            }

            if (missingPreservedInvariants.Length > 0 ||
                disallowedDifferences.Length > 0 ||
                witness.GlueThreshold < SliWitnessState.DefaultGlueThreshold)
            {
                witness.CandidacyStatus = SliWitnessState.Comparable;

                if (missingPreservedInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        witness.Residues,
                        HigherOrderLocalityResidueKind.NonCandidateWitness,
                        "morphism-candidate",
                        $"Missing preserved invariants: {string.Join(", ", missingPreservedInvariants)}.");
                }

                if (disallowedDifferences.Length > 0)
                {
                    AddResidue(
                        context,
                        witness.Residues,
                        HigherOrderLocalityResidueKind.NonCandidateWitness,
                        "morphism-candidate",
                        $"Differences are not morphism-safe: {string.Join(", ", disallowedDifferences)}.");
                }

                if (witness.GlueThreshold < SliWitnessState.DefaultGlueThreshold)
                {
                    AddResidue(
                        context,
                        witness.Residues,
                        HigherOrderLocalityResidueKind.NonCandidateWitness,
                        "morphism-candidate",
                        $"Glue threshold {witness.GlueThreshold.ToString("0.00", CultureInfo.InvariantCulture)} remained below candidacy floor.");
                }

                context.AddTrace("morphism-candidate(comparable)");
                return Task.FromResult(SExpression.AtomNode("morphism-comparable"));
            }

            witness.CandidacyStatus = SliWitnessState.MorphismCandidate;
            context.AddTrace("morphism-candidate(candidate)");
            return Task.FromResult(SExpression.AtomNode("morphism-candidate"));
        });
    }

    private void RegisterBoundedTransportOperators()
    {
        RegisterTargetCandidate("transport-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var witness = state.Witness;
            var transport = state.Transport;

            var witnessToken = UnwrapStringLiteral(Arg(expression, 1, "witness-state"));
            if (!TryResolveTransportWitnessHandle(state, witnessToken, out var witnessHandle))
            {
                transport.IsConfigured = false;
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-begin requires a resolved witness handle.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.MissingTransportPrerequisites,
                    "transport-begin",
                    "Transport requires a resolved witness handle.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-begin",
                    "Transport remained incomplete because no witness handle was available.");
                context.AddTrace("transport-begin(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-incomplete"));
            }

            var missingPrerequisites =
                string.IsNullOrWhiteSpace(state.LocalityHandle) ||
                !state.Perspective.IsConfigured ||
                !state.Participation.IsConfigured ||
                !witness.IsConfigured ||
                !string.Equals(witness.CandidacyStatus, SliWitnessState.MorphismCandidate, StringComparison.OrdinalIgnoreCase) ||
                HasBlockingWitnessResidues(witness);

            if (missingPrerequisites)
            {
                transport.IsConfigured = false;
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-begin requires completed locality, perspective, participation, and positive witness candidacy.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.MissingTransportPrerequisites,
                    "transport-begin",
                    "Transport requires completed locality, perspective, participation, and morphism candidacy.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.BlockedTransport,
                    "transport-begin",
                    "Transport was blocked because witness candidacy did not authorize carry.");
                context.AddTrace("transport-begin(blocked)");
                return Task.FromResult(SExpression.AtomNode("transport-blocked"));
            }

            transport.Configure(witnessHandle);
            context.AddTrace($"transport-begin({witnessToken})");
            return Task.FromResult(SExpression.AtomNode("transport-begin"));
        });

        RegisterTargetCandidate("transport-source", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-source requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-source",
                    "Transport source requires an active transport.");
                context.AddTrace("transport-source(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-source-incomplete"));
            }

            var token = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!TryResolveWitnessReference(context, token, out var sourceHandle))
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-source rejected an unresolved locality or rehearsal branch reference.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportReference,
                    "transport-source",
                    $"Transport rejected unresolved source reference '{token}'.");
                context.AddTrace("transport-source(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("transport-source-invalid"));
            }

            transport.SourceLocalityHandle = sourceHandle;
            context.AddTrace($"transport-source({token})");
            return Task.FromResult(SExpression.AtomNode("transport-source"));
        });

        RegisterTargetCandidate("transport-target", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-target requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-target",
                    "Transport target requires an active transport.");
                context.AddTrace("transport-target(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-target-incomplete"));
            }

            var token = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!TryResolveWitnessReference(context, token, out var targetHandle))
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-target rejected an unresolved locality or rehearsal branch reference.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportReference,
                    "transport-target",
                    $"Transport rejected unresolved target reference '{token}'.");
                context.AddTrace("transport-target(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("transport-target-invalid"));
            }

            transport.TargetLocalityHandle = targetHandle;
            context.AddTrace($"transport-target({token})");
            return Task.FromResult(SExpression.AtomNode("transport-target"));
        });

        RegisterTargetCandidate("transport-preserve", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-preserve requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-preserve",
                    "Transport preservation requires an active transport.");
                context.AddTrace("transport-preserve(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-preserve-incomplete"));
            }

            var invariant = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(invariant) &&
                !transport.PreservedInvariants.Contains(invariant, StringComparer.OrdinalIgnoreCase))
            {
                transport.PreservedInvariants.Add(invariant);
                context.AddTrace($"transport-preserve({invariant})");
            }

            return Task.FromResult(SExpression.AtomNode("transport-preserve"));
        });

        RegisterTargetCandidate("transport-map", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            if (!transport.IsConfigured)
            {
                transport.Warnings.Add("transport-map requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-map",
                    "Transport mapping requires an active transport.");
                context.AddTrace("transport-map(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-map-incomplete"));
            }

            var source = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var target = UnwrapStringLiteral(Arg(expression, 2, string.Empty));
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            {
                transport.Status = SliTransportState.Blocked;
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportMapping,
                    "transport-map",
                    "Transport mapping requires both source and target values.");
                context.AddTrace("transport-map(invalid)");
                return Task.FromResult(SExpression.AtomNode("transport-map-invalid"));
            }

            transport.MappedDifferences.Add(new SliTransportMappingState
            {
                Source = source,
                Target = target
            });
            context.AddTrace($"transport-map({source}->{target})");
            return Task.FromResult(SExpression.AtomNode("transport-map"));
        });

        RegisterTargetCandidate("transport-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "transport residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.Transport.Residues,
                HigherOrderLocalityResidueKind.TransportResidue,
                "transport-residue",
                detail);
            context.AddTrace($"transport-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("transport-residue"));
        });

        RegisterTargetCandidate("transport-status", (expression, context, _) =>
        {
            var transport = context.HigherOrderLocalityState.Transport;
            var requestedStatus = UnwrapStringLiteral(Arg(expression, 1, SliTransportState.Candidate));

            if (!transport.IsConfigured)
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add("transport-status requires transport-begin.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.IncompleteTransport,
                    "transport-status",
                    "Transport status changes require an active transport.");
                context.AddTrace("transport-status(incomplete)");
                return Task.FromResult(SExpression.AtomNode("transport-status-incomplete"));
            }

            if (!IsRecognizedTransportStatus(requestedStatus))
            {
                transport.Status = SliTransportState.Blocked;
                transport.Warnings.Add($"transport-status rejected invalid value '{requestedStatus}'.");
                AddResidue(
                    context,
                    transport.Residues,
                    HigherOrderLocalityResidueKind.InvalidTransportStatus,
                    "transport-status",
                    $"Rejected invalid transport status '{requestedStatus}'.");
                context.AddTrace("transport-status(invalid)");
                return Task.FromResult(SExpression.AtomNode("transport-status-invalid"));
            }

            if (string.Equals(requestedStatus, SliTransportState.Blocked, StringComparison.OrdinalIgnoreCase))
            {
                transport.Status = SliTransportState.Blocked;
                context.AddTrace("transport-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("transport-blocked"));
            }

            if (string.Equals(requestedStatus, SliTransportState.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                transport.Status = SliTransportState.Candidate;
                context.AddTrace("transport-status(candidate)");
                return Task.FromResult(SExpression.AtomNode("transport-candidate"));
            }

            var witness = context.HigherOrderLocalityState.Witness;
            var missingRequiredInvariants = RequiredTransportInvariants()
                .Except(transport.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var illegalCarriedInvariants = transport.PreservedInvariants
                .Where(invariant => !witness.PreservedInvariants.Contains(invariant, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            var disallowedMappings = transport.MappedDifferences
                .Where(mapping => !IsAllowedTransportMapping(mapping.Source, mapping.Target))
                .ToArray();

            var missingWitnessMappings = witness.DifferenceSet
                .Except(transport.MappedDifferences.Select(mapping => mapping.Source), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var blocked =
                HasBlockingWitnessResidues(witness) ||
                !string.Equals(witness.CandidacyStatus, SliWitnessState.MorphismCandidate, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(transport.TargetLocalityHandle) ||
                !string.Equals(transport.SourceLocalityHandle, witness.LeftLocalityHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(transport.TargetLocalityHandle, witness.RightLocalityHandle, StringComparison.OrdinalIgnoreCase) ||
                missingRequiredInvariants.Length > 0 ||
                illegalCarriedInvariants.Length > 0 ||
                disallowedMappings.Length > 0 ||
                missingWitnessMappings.Length > 0;

            if (blocked)
            {
                transport.Status = SliTransportState.Blocked;

                if (string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) || string.IsNullOrWhiteSpace(transport.TargetLocalityHandle))
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.MissingTransportPrerequisites,
                        "transport-status",
                        "Transport completion requires explicit source and target localities.");
                }

                if (!string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) &&
                    !string.IsNullOrWhiteSpace(transport.TargetLocalityHandle) &&
                    (!string.Equals(transport.SourceLocalityHandle, witness.LeftLocalityHandle, StringComparison.OrdinalIgnoreCase) ||
                     !string.Equals(transport.TargetLocalityHandle, witness.RightLocalityHandle, StringComparison.OrdinalIgnoreCase)))
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.BlockedTransport,
                        "transport-status",
                        "Transport source and target must match the active witness comparison pair.");
                }

                if (missingRequiredInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.BlockedTransport,
                        "transport-status",
                        $"Transport omitted required preserved invariants: {string.Join(", ", missingRequiredInvariants)}.");
                }

                if (illegalCarriedInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.InvalidTransportMapping,
                        "transport-status",
                        $"Transport attempted to carry invariants not preserved by witness: {string.Join(", ", illegalCarriedInvariants)}.");
                }

                if (disallowedMappings.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.InvalidTransportMapping,
                        "transport-status",
                        $"Transport attempted disallowed mappings: {string.Join(", ", disallowedMappings.Select(mapping => $"{mapping.Source}->{mapping.Target}"))}.");
                }

                if (missingWitnessMappings.Length > 0)
                {
                    AddResidue(
                        context,
                        transport.Residues,
                        HigherOrderLocalityResidueKind.BlockedTransport,
                        "transport-status",
                        $"Transport omitted witness differences: {string.Join(", ", missingWitnessMappings)}.");
                }

                context.AddTrace("transport-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("transport-blocked"));
            }

            transport.Status = SliTransportState.Completed;
            context.AddTrace("transport-status(completed)");
            return Task.FromResult(SExpression.AtomNode("transport-completed"));
        });
    }

    private void RegisterBoundedAdmissibilityOperators()
    {
        RegisterTargetCandidate("surface-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var transport = state.Transport;
            var surface = state.AdmissibleSurface;
            surface.Reset();

            var transportToken = UnwrapStringLiteral(Arg(expression, 1, "transport-state"));
            if (!TryResolveAdmissibleTransportHandle(state, transportToken, out var transportHandle))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-begin requires a resolved completed transport handle.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceReference,
                    "surface-begin",
                    $"Admissible surface rejected unresolved transport reference '{transportToken}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-begin",
                    "Admissible surface remained incomplete because no completed transport handle was available.");
                context.AddTrace("surface-begin(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("surface-begin-invalid"));
            }

            var missingPrerequisites =
                !transport.IsConfigured ||
                !string.Equals(transport.TransportHandle, transportHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(transport.Status, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(transport.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(transport.TargetLocalityHandle) ||
                HasBlockingTransportResidues(transport);

            if (missingPrerequisites)
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-begin requires completed lawful transport with intact preserved invariants.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.MissingAdmissibleSurfacePrerequisites,
                    "surface-begin",
                    "Admissible surface formation requires completed transport with explicit source and target localities.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.BlockedAdmissibleSurface,
                    "surface-begin",
                    "Admissible surface formation was blocked because transport had not completed lawfully.");
                context.AddTrace("surface-begin(blocked)");
                return Task.FromResult(SExpression.AtomNode("surface-begin-blocked"));
            }

            surface.Configure(transportHandle, transport.SourceLocalityHandle, transport.TargetLocalityHandle);
            context.AddTrace($"surface-begin({transportToken})");
            return Task.FromResult(SExpression.AtomNode("surface-begin"));
        });

        RegisterTargetCandidate("surface-source", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var transport = state.Transport;
            var surface = state.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-source requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-source",
                    "Admissible surface source requires an active surface.");
                context.AddTrace("surface-source(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-source-incomplete"));
            }

            var transportToken = UnwrapStringLiteral(Arg(expression, 1, "transport-state"));
            if (!TryResolveAdmissibleTransportHandle(state, transportToken, out var transportHandle) ||
                !string.Equals(transport.TransportHandle, transportHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(surface.TransportHandle, transportHandle, StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-source rejected an unresolved or mismatched transport reference.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceReference,
                    "surface-source",
                    $"Admissible surface rejected unresolved transport reference '{transportToken}'.");
                context.AddTrace("surface-source(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("surface-source-invalid"));
            }

            surface.Configure(transportHandle, transport.SourceLocalityHandle, transport.TargetLocalityHandle);
            context.AddTrace($"surface-source({transportToken})");
            return Task.FromResult(SExpression.AtomNode("surface-source"));
        });

        RegisterTargetCandidate("surface-class", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-class requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-class",
                    "Admissible surface classification requires an active surface.");
                context.AddTrace("surface-class(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-class-incomplete"));
            }

            var requestedClass = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            var identityApplicability = UnwrapStringLiteral(Arg(expression, 2, string.Empty));

            if (!IsRecognizedSurfaceClass(requestedClass))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add($"surface-class rejected invalid value '{requestedClass}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceClass,
                    "surface-class",
                    $"Rejected invalid admissible surface class '{requestedClass}'.");
                context.AddTrace("surface-class(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-class-invalid"));
            }

            if (!IsRecognizedIdentityApplicability(identityApplicability))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add($"surface-class rejected invalid identity applicability '{identityApplicability}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidIdentityBearingApplicability,
                    "surface-class",
                    $"Rejected invalid identity-bearing applicability '{identityApplicability}'.");
                context.AddTrace("surface-class(invalid-identity-applicability)");
                return Task.FromResult(SExpression.AtomNode("surface-class-invalid"));
            }

            surface.SurfaceClass = requestedClass.ToLowerInvariant();
            surface.IdentityBearingApplicable = string.Equals(
                identityApplicability,
                SliAdmissibleSurfaceState.IdentityApplicable,
                StringComparison.OrdinalIgnoreCase);
            surface.HasIdentityBearingApplicability = true;
            context.AddTrace($"surface-class({requestedClass}:{identityApplicability})");
            return Task.FromResult(SExpression.AtomNode("surface-class"));
        });

        RegisterTargetCandidate("surface-reveal", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-reveal requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-reveal",
                    "Admissible surface reveal posture requires an active surface.");
                context.AddTrace("surface-reveal(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-reveal-incomplete"));
            }

            var requestedReveal = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.MaskedRevealPosture));
            if (!string.Equals(requestedReveal, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(requestedReveal, "narrow", StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.RevealPosture = ResolveSafeValue(surface.RevealPosture, SliHigherOrderLocalityState.MaskedRevealPosture);
                surface.Warnings.Add($"surface-reveal rejected invalid value '{requestedReveal}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceReveal,
                    "surface-reveal",
                    $"Rejected invalid admissible surface reveal posture '{requestedReveal}'.");
                context.AddTrace("surface-reveal(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-reveal-invalid"));
            }

            surface.RevealPosture = requestedReveal.ToLowerInvariant();
            context.AddTrace($"surface-reveal({requestedReveal})");
            return Task.FromResult(SExpression.AtomNode("surface-reveal"));
        });

        RegisterTargetCandidate("surface-boundary", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-boundary requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-boundary",
                    "Admissible surface boundary requires an active surface.");
                context.AddTrace("surface-boundary(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-boundary-incomplete"));
            }

            var requestedBoundary = UnwrapStringLiteral(Arg(expression, 1, SliHigherOrderLocalityState.BoundedSealPosture));
            if (!HasSafeSealPosture(requestedBoundary))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Boundary = ResolveSafeValue(surface.Boundary, SliHigherOrderLocalityState.BoundedSealPosture);
                surface.Warnings.Add($"surface-boundary rejected invalid value '{requestedBoundary}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceBoundary,
                    "surface-boundary",
                    $"Rejected invalid admissible surface boundary '{requestedBoundary}'.");
                context.AddTrace("surface-boundary(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-boundary-invalid"));
            }

            surface.Boundary = requestedBoundary.ToLowerInvariant();
            context.AddTrace($"surface-boundary({requestedBoundary})");
            return Task.FromResult(SExpression.AtomNode("surface-boundary"));
        });

        RegisterTargetCandidate("surface-evidence", (expression, context, _) =>
        {
            var surface = context.HigherOrderLocalityState.AdmissibleSurface;
            if (!surface.IsConfigured)
            {
                surface.Warnings.Add("surface-evidence requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-evidence",
                    "Admissible surface evidence requires an active surface.");
                context.AddTrace("surface-evidence(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-evidence-incomplete"));
            }

            var evidence = UnwrapStringLiteral(Arg(expression, 1, string.Empty));
            if (!string.IsNullOrWhiteSpace(evidence) &&
                !surface.EvidenceSet.Contains(evidence, StringComparer.OrdinalIgnoreCase))
            {
                surface.EvidenceSet.Add(evidence);
                context.AddTrace($"surface-evidence({evidence})");
            }

            return Task.FromResult(SExpression.AtomNode("surface-evidence"));
        });

        RegisterTargetCandidate("surface-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "admissible surface residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.AdmissibleSurface.Residues,
                HigherOrderLocalityResidueKind.AdmissibleSurfaceResidue,
                "surface-residue",
                detail);
            context.AddTrace($"surface-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("surface-residue"));
        });

        RegisterTargetCandidate("surface-status", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var transport = state.Transport;
            var surface = state.AdmissibleSurface;
            var requestedStatus = UnwrapStringLiteral(Arg(expression, 1, SliAdmissibleSurfaceState.Candidate));

            if (!surface.IsConfigured)
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add("surface-status requires surface-begin.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface,
                    "surface-status",
                    "Admissible surface status changes require an active surface.");
                context.AddTrace("surface-status(incomplete)");
                return Task.FromResult(SExpression.AtomNode("surface-status-incomplete"));
            }

            if (!IsRecognizedSurfaceStatus(requestedStatus))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                surface.Warnings.Add($"surface-status rejected invalid value '{requestedStatus}'.");
                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.InvalidSurfaceStatus,
                    "surface-status",
                    $"Rejected invalid admissible surface status '{requestedStatus}'.");
                context.AddTrace("surface-status(invalid)");
                return Task.FromResult(SExpression.AtomNode("surface-status-invalid"));
            }

            if (string.Equals(requestedStatus, SliAdmissibleSurfaceState.Blocked, StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;
                context.AddTrace("surface-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("surface-blocked"));
            }

            if (string.Equals(requestedStatus, SliAdmissibleSurfaceState.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                surface.Status = SliAdmissibleSurfaceState.Candidate;
                context.AddTrace("surface-status(candidate)");
                return Task.FromResult(SExpression.AtomNode("surface-candidate"));
            }

            var missingTransportInvariants = RequiredTransportInvariants()
                .Except(transport.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var revealWouldWiden = WouldWidenReveal(state.RevealPosture, surface.RevealPosture);
            var boundaryWouldWiden = WouldWidenBoundary(state.SealPosture, surface.Boundary);
            var identityApplicabilityConflict =
                string.Equals(surface.SurfaceClass, SliAdmissibleSurfaceState.IdentityBearingClass, StringComparison.OrdinalIgnoreCase) &&
                !surface.IdentityBearingApplicable;

            var blocked =
                !transport.IsConfigured ||
                !string.Equals(transport.Status, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(surface.TransportHandle) ||
                !string.Equals(surface.TransportHandle, transport.TransportHandle, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(surface.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(surface.TargetLocalityHandle) ||
                string.IsNullOrWhiteSpace(surface.SurfaceClass) ||
                !surface.HasIdentityBearingApplicability ||
                surface.EvidenceSet.Count == 0 ||
                HasBlockingTransportResidues(transport) ||
                missingTransportInvariants.Length > 0 ||
                revealWouldWiden ||
                boundaryWouldWiden ||
                identityApplicabilityConflict;

            if (blocked)
            {
                surface.Status = SliAdmissibleSurfaceState.Blocked;

                if (!transport.IsConfigured ||
                    !string.Equals(transport.Status, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase) ||
                    surface.EvidenceSet.Count == 0 ||
                    string.IsNullOrWhiteSpace(surface.SurfaceClass) ||
                    !surface.HasIdentityBearingApplicability ||
                    missingTransportInvariants.Length > 0)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.MissingAdmissibleSurfacePrerequisites,
                        "surface-status",
                        $"Admissible surface formation requires completed transport, explicit classing, explicit identity applicability, evidence carriage, and preserved invariants. Missing invariants: {string.Join(", ", missingTransportInvariants)}.");
                }

                if (string.IsNullOrWhiteSpace(surface.TransportHandle) ||
                    !string.Equals(surface.TransportHandle, transport.TransportHandle, StringComparison.OrdinalIgnoreCase))
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidSurfaceReference,
                        "surface-status",
                        "Admissible surface formation requires an active transport reference that matches the completed transport lane.");
                }

                if (revealWouldWiden)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidSurfaceReveal,
                        "surface-status",
                        $"Admissible surface reveal '{surface.RevealPosture}' would widen locality reveal '{state.RevealPosture}'.");
                }

                if (boundaryWouldWiden)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidSurfaceBoundary,
                        "surface-status",
                        $"Admissible surface boundary '{surface.Boundary}' would widen locality seal posture '{state.SealPosture}'.");
                }

                if (identityApplicabilityConflict)
                {
                    AddResidue(
                        context,
                        surface.Residues,
                        HigherOrderLocalityResidueKind.InvalidIdentityBearingApplicability,
                        "surface-status",
                        "Identity-bearing surface class requires explicit identity applicability.");
                }

                AddResidue(
                    context,
                    surface.Residues,
                    HigherOrderLocalityResidueKind.BlockedAdmissibleSurface,
                    "surface-status",
                    "Admissible surface remained blocked because the carried structure could not yet be lawfully shaped for inspection.");
                context.AddTrace("surface-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("surface-blocked"));
            }

            surface.Status = SliAdmissibleSurfaceState.Formed;
            context.AddTrace("surface-status(formed)");
            return Task.FromResult(SExpression.AtomNode("surface-formed"));
        });
    }

    private void RegisterBoundedAccountabilityPacketOperators()
    {
        RegisterTargetCandidate("packet-begin", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var surface = state.AdmissibleSurface;
            var packet = state.AccountabilityPacket;
            packet.Reset();

            var surfaceToken = UnwrapStringLiteral(Arg(expression, 1, "surface-state"));
            if (string.Equals(surfaceToken, "surface-state", StringComparison.OrdinalIgnoreCase) &&
                (!surface.IsConfigured || !string.Equals(surface.Status, SliAdmissibleSurfaceState.Formed, StringComparison.OrdinalIgnoreCase)))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.Warnings.Add("packet-begin requires a formed admissible surface.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.MissingAccountabilityPacketPrerequisites,
                    "packet-begin",
                    "Accountability packet formation requires a formed admissible surface.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-begin",
                    "Accountability packet remained incomplete because admissible surface formation had not completed.");
                context.AddTrace("packet-begin(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-begin-blocked"));
            }

            if (!TryResolveAdmissibleSurfaceHandle(state, surfaceToken, out var surfaceHandle))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.Warnings.Add("packet-begin requires a resolved formed admissible surface handle.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketReference,
                    "packet-begin",
                    $"Accountability packet rejected unresolved surface reference '{surfaceToken}'.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-begin",
                    "Accountability packet remained incomplete because no formed admissible surface handle was available.");
                context.AddTrace("packet-begin(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("packet-begin-invalid"));
            }

            var missingPrerequisites =
                !surface.IsConfigured ||
                !string.Equals(surface.SurfaceHandle, surfaceHandle, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(surface.Status, SliAdmissibleSurfaceState.Formed, StringComparison.OrdinalIgnoreCase) ||
                HasBlockingAdmissibleSurfaceResidues(surface);

            if (missingPrerequisites)
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.Warnings.Add("packet-begin requires a formed admissible surface with intact bounded review state.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.MissingAccountabilityPacketPrerequisites,
                    "packet-begin",
                    "Accountability packet formation requires a formed admissible surface.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.BlockedAccountabilityPacket,
                    "packet-begin",
                    "Accountability packet formation was blocked because admissible surface formation had not completed lawfully.");
                context.AddTrace("packet-begin(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-begin-blocked"));
            }

            packet.Configure(surfaceHandle);
            context.AddTrace($"packet-begin({surfaceToken})");
            return Task.FromResult(SExpression.AtomNode("packet-begin"));
        });

        RegisterTargetCandidate("packet-lineage", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var packet = state.AccountabilityPacket;
            var surface = state.AdmissibleSurface;
            var transport = state.Transport;
            var witness = state.Witness;

            if (!packet.IsConfigured)
            {
                packet.Warnings.Add("packet-lineage requires packet-begin.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-lineage",
                    "Accountability packet lineage requires an active packet.");
                context.AddTrace("packet-lineage(incomplete)");
                return Task.FromResult(SExpression.AtomNode("packet-lineage-incomplete"));
            }

            var surfaceToken = UnwrapStringLiteral(Arg(expression, 1, "surface-state"));
            if (!TryResolveAdmissibleSurfaceHandle(state, surfaceToken, out var surfaceHandle) ||
                !string.Equals(packet.SurfaceHandle, surfaceHandle, StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.Warnings.Add("packet-lineage rejected an unresolved or mismatched admissible surface reference.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketReference,
                    "packet-lineage",
                    $"Accountability packet rejected unresolved surface reference '{surfaceToken}'.");
                context.AddTrace("packet-lineage(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("packet-lineage-invalid"));
            }

            if (string.IsNullOrWhiteSpace(surface.TransportHandle) ||
                !transport.IsConfigured ||
                !string.Equals(surface.TransportHandle, transport.TransportHandle, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(transport.WitnessHandle) ||
                !witness.IsConfigured ||
                !string.Equals(transport.WitnessHandle, witness.WitnessHandle, StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.MissingAccountabilityPacketPrerequisites,
                    "packet-lineage",
                    "Accountability packet lineage requires completed admissible surface, transport, and witness lineage.");
                context.AddTrace("packet-lineage(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-lineage-blocked"));
            }

            packet.TransportHandle = transport.TransportHandle;
            packet.WitnessHandle = witness.WitnessHandle;
            packet.SourceLocalityHandle = surface.SourceLocalityHandle;
            packet.TargetLocalityHandle = surface.TargetLocalityHandle;
            context.AddTrace($"packet-lineage({surfaceToken})");
            return Task.FromResult(SExpression.AtomNode("packet-lineage"));
        });

        RegisterTargetCandidate("packet-invariants", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var packet = state.AccountabilityPacket;
            var transport = state.Transport;
            var witness = state.Witness;

            if (!packet.IsConfigured)
            {
                packet.Warnings.Add("packet-invariants requires packet-begin.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-invariants",
                    "Accountability packet invariants require an active packet.");
                context.AddTrace("packet-invariants(incomplete)");
                return Task.FromResult(SExpression.AtomNode("packet-invariants-incomplete"));
            }

            var surfaceToken = UnwrapStringLiteral(Arg(expression, 1, "surface-state"));
            if (!TryResolveAdmissibleSurfaceHandle(state, surfaceToken, out var surfaceHandle) ||
                !string.Equals(packet.SurfaceHandle, surfaceHandle, StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketReference,
                    "packet-invariants",
                    $"Accountability packet rejected unresolved surface reference '{surfaceToken}'.");
                context.AddTrace("packet-invariants(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("packet-invariants-invalid"));
            }

            var carriedInvariants = transport.PreservedInvariants
                .Intersect(witness.PreservedInvariants, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (carriedInvariants.Length == 0)
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketInvariant,
                    "packet-invariants",
                    "Accountability packet invariants must be carried from already-preserved witness and transport invariants.");
                context.AddTrace("packet-invariants(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-invariants-blocked"));
            }

            packet.PreservedInvariants.Clear();
            packet.PreservedInvariants.AddRange(carriedInvariants);
            context.AddTrace($"packet-invariants({surfaceToken})");
            return Task.FromResult(SExpression.AtomNode("packet-invariants"));
        });

        RegisterTargetCandidate("packet-class", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var packet = state.AccountabilityPacket;
            var surface = state.AdmissibleSurface;

            if (!packet.IsConfigured)
            {
                packet.Warnings.Add("packet-class requires packet-begin.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-class",
                    "Accountability packet classing requires an active packet.");
                context.AddTrace("packet-class(incomplete)");
                return Task.FromResult(SExpression.AtomNode("packet-class-incomplete"));
            }

            var surfaceToken = UnwrapStringLiteral(Arg(expression, 1, "surface-state"));
            if (!TryResolveAdmissibleSurfaceHandle(state, surfaceToken, out var surfaceHandle) ||
                !string.Equals(packet.SurfaceHandle, surfaceHandle, StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketReference,
                    "packet-class",
                    $"Accountability packet rejected unresolved surface reference '{surfaceToken}'.");
                context.AddTrace("packet-class(invalid-reference)");
                return Task.FromResult(SExpression.AtomNode("packet-class-invalid"));
            }

            packet.SurfaceClass = surface.SurfaceClass;
            packet.IdentityBearingApplicable = surface.IdentityBearingApplicable;
            context.AddTrace($"packet-class({surfaceToken})");
            return Task.FromResult(SExpression.AtomNode("packet-class"));
        });

        RegisterTargetCandidate("packet-reveal", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var packet = state.AccountabilityPacket;
            var surface = state.AdmissibleSurface;

            if (!packet.IsConfigured)
            {
                packet.Warnings.Add("packet-reveal requires packet-begin.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-reveal",
                    "Accountability packet reveal posture requires an active packet.");
                context.AddTrace("packet-reveal(incomplete)");
                return Task.FromResult(SExpression.AtomNode("packet-reveal-incomplete"));
            }

            var revealToken = UnwrapStringLiteral(Arg(expression, 1, "surface-state"));
            string requestedReveal;
            if (TryResolveAdmissibleSurfaceHandle(state, revealToken, out var surfaceHandle) &&
                string.Equals(packet.SurfaceHandle, surfaceHandle, StringComparison.OrdinalIgnoreCase))
            {
                requestedReveal = surface.RevealPosture;
            }
            else
            {
                requestedReveal = revealToken;
            }

            if (!string.Equals(requestedReveal, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(requestedReveal, "narrow", StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.RevealPosture = ResolveSafeValue(packet.RevealPosture, surface.RevealPosture);
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketReveal,
                    "packet-reveal",
                    $"Rejected invalid accountability packet reveal posture '{requestedReveal}'.");
                context.AddTrace("packet-reveal(invalid)");
                return Task.FromResult(SExpression.AtomNode("packet-reveal-invalid"));
            }

            if (WouldWidenPacketReveal(surface.RevealPosture, requestedReveal))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.RevealPosture = ResolveSafeValue(packet.RevealPosture, surface.RevealPosture);
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketReveal,
                    "packet-reveal",
                    $"Accountability packet reveal '{requestedReveal}' would widen admissible surface reveal '{surface.RevealPosture}'.");
                context.AddTrace("packet-reveal(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-reveal-blocked"));
            }

            packet.RevealPosture = requestedReveal.ToLowerInvariant();
            context.AddTrace($"packet-reveal({revealToken})");
            return Task.FromResult(SExpression.AtomNode("packet-reveal"));
        });

        RegisterTargetCandidate("packet-residue", (expression, context, _) =>
        {
            var detail = UnwrapStringLiteral(Arg(expression, 1, "accountability packet residue"));
            AddResidue(
                context,
                context.HigherOrderLocalityState.AccountabilityPacket.Residues,
                HigherOrderLocalityResidueKind.AccountabilityPacketResidue,
                "packet-residue",
                detail);
            context.AddTrace($"packet-residue({detail})");
            return Task.FromResult(SExpression.AtomNode("packet-residue"));
        });

        RegisterTargetCandidate("packet-status", (expression, context, _) =>
        {
            var state = context.HigherOrderLocalityState;
            var packet = state.AccountabilityPacket;
            var surface = state.AdmissibleSurface;
            var transport = state.Transport;
            var witness = state.Witness;
            var requestedStatus = UnwrapStringLiteral(Arg(expression, 1, SliAccountabilityPacketState.Candidate));

            if (!packet.IsConfigured)
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.Warnings.Add("packet-status requires packet-begin.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket,
                    "packet-status",
                    "Accountability packet status changes require an active packet.");
                context.AddTrace("packet-status(incomplete)");
                return Task.FromResult(SExpression.AtomNode("packet-status-incomplete"));
            }

            if (!IsRecognizedPacketStatus(requestedStatus))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                packet.Warnings.Add($"packet-status rejected invalid value '{requestedStatus}'.");
                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.InvalidPacketStatus,
                    "packet-status",
                    $"Rejected invalid accountability packet readiness status '{requestedStatus}'.");
                context.AddTrace("packet-status(invalid)");
                return Task.FromResult(SExpression.AtomNode("packet-status-invalid"));
            }

            if (string.Equals(requestedStatus, SliAccountabilityPacketState.Blocked, StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;
                context.AddTrace("packet-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-blocked"));
            }

            if (string.Equals(requestedStatus, SliAccountabilityPacketState.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Candidate;
                context.AddTrace("packet-status(candidate)");
                return Task.FromResult(SExpression.AtomNode("packet-candidate"));
            }

            var blocked =
                !surface.IsConfigured ||
                !string.Equals(surface.Status, SliAdmissibleSurfaceState.Formed, StringComparison.OrdinalIgnoreCase) ||
                HasBlockingAdmissibleSurfaceResidues(surface) ||
                HasBlockingTransportResidues(transport) ||
                HasBlockingWitnessResidues(witness) ||
                HasBlockingAccountabilityPacketResidues(packet) ||
                string.IsNullOrWhiteSpace(packet.TransportHandle) ||
                string.IsNullOrWhiteSpace(packet.WitnessHandle) ||
                string.IsNullOrWhiteSpace(packet.SourceLocalityHandle) ||
                string.IsNullOrWhiteSpace(packet.TargetLocalityHandle) ||
                packet.PreservedInvariants.Count == 0 ||
                string.IsNullOrWhiteSpace(packet.SurfaceClass) ||
                WouldWidenPacketReveal(surface.RevealPosture, packet.RevealPosture);

            if (blocked)
            {
                packet.ReadinessStatus = SliAccountabilityPacketState.Blocked;

                if (string.IsNullOrWhiteSpace(packet.TransportHandle) ||
                    string.IsNullOrWhiteSpace(packet.WitnessHandle) ||
                    string.IsNullOrWhiteSpace(packet.SourceLocalityHandle) ||
                    string.IsNullOrWhiteSpace(packet.TargetLocalityHandle) ||
                    packet.PreservedInvariants.Count == 0 ||
                    string.IsNullOrWhiteSpace(packet.SurfaceClass))
                {
                    AddResidue(
                        context,
                        packet.Residues,
                        HigherOrderLocalityResidueKind.MissingAccountabilityPacketPrerequisites,
                        "packet-status",
                        "Accountability packet review readiness requires carried lineage, preserved invariants, and mirrored surface classing.");
                }

                if (WouldWidenPacketReveal(surface.RevealPosture, packet.RevealPosture))
                {
                    AddResidue(
                        context,
                        packet.Residues,
                        HigherOrderLocalityResidueKind.InvalidPacketReveal,
                        "packet-status",
                        $"Accountability packet reveal '{packet.RevealPosture}' would widen admissible surface reveal '{surface.RevealPosture}'.");
                }

                AddResidue(
                    context,
                    packet.Residues,
                    HigherOrderLocalityResidueKind.BlockedAccountabilityPacket,
                    "packet-status",
                    "Accountability packet remained blocked because the review packet could not yet be lawfully shaped for Sanctuary inspection.");
                context.AddTrace("packet-status(blocked)");
                return Task.FromResult(SExpression.AtomNode("packet-blocked"));
            }

            packet.ReadinessStatus = SliAccountabilityPacketState.ReviewReady;
            context.AddTrace("packet-status(review-ready)");
            return Task.FromResult(SExpression.AtomNode("packet-review-ready"));
        });
    }

    private static void AddResidue(
        SliExecutionContext context,
        ICollection<HigherOrderLocalityResidue> scopedResidues,
        HigherOrderLocalityResidueKind kind,
        string source,
        string detail)
    {
        var residue = new HigherOrderLocalityResidue
        {
            Kind = kind,
            Source = source,
            Detail = detail
        };

        context.HigherOrderLocalityState.AddResidue(residue);
        if (!ReferenceEquals(scopedResidues, context.HigherOrderLocalityState.Residues))
        {
            scopedResidues.Add(residue);
        }
    }

    private static string ResolveSafeValue(string currentValue, string safeDefault)
    {
        return string.IsNullOrWhiteSpace(currentValue)
            ? safeDefault
            : currentValue;
    }

    private static bool HasSafeSealPosture(string value)
    {
        return string.Equals(value, SliHigherOrderLocalityState.BoundedSealPosture, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "sealed", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRehearsalMode(string value)
    {
        if (string.Equals(value, SliRehearsalState.DreamGameMode, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "dream", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "game", StringComparison.OrdinalIgnoreCase))
        {
            return value.ToLowerInvariant();
        }

        return SliRehearsalState.DreamGameMode;
    }

    private static IReadOnlyList<string> RequiredWitnessInvariants()
    {
        return
        [
            "self-anchor-polarity",
            "other-anchor-polarity",
            "relation-anchor-polarity",
            "seal-posture-bound",
            "reveal-posture-bound",
            "participation-mode-limit",
            "identity-nonbinding"
        ];
    }

    private static IReadOnlyList<string> AllowedWitnessDifferences()
    {
        return
        [
            "rehearsal-branch",
            "substitution",
            "analogy",
            "branch-context"
        ];
    }

    private static IReadOnlyList<string> RequiredTransportInvariants()
    {
        return
        [
            "self-anchor-polarity",
            "other-anchor-polarity",
            "relation-anchor-polarity",
            "seal-posture-bound",
            "reveal-posture-bound",
            "participation-mode-limit",
            "identity-nonbinding"
        ];
    }

    private static bool IsAllowedTransportMapping(string source, string target)
    {
        return
            (string.Equals(source, "rehearsal-branch", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "branch-variant", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(source, "substitution", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "substitution", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(source, "analogy", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "analogy", StringComparison.OrdinalIgnoreCase)) ||
            (string.Equals(source, "branch-context", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(target, "branch-context", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRecognizedTransportStatus(string value)
    {
        return string.Equals(value, SliTransportState.Blocked, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliTransportState.Candidate, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliTransportState.Completed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedSurfaceClass(string value)
    {
        return string.Equals(value, SliAdmissibleSurfaceState.InformationalClass, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.ComparativeClass, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.IdentityBearingClass, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedIdentityApplicability(string value)
    {
        return string.Equals(value, SliAdmissibleSurfaceState.InformationalOnly, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.IdentityApplicable, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedSurfaceStatus(string value)
    {
        return string.Equals(value, SliAdmissibleSurfaceState.Blocked, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.Candidate, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAdmissibleSurfaceState.Formed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRecognizedPacketStatus(string value)
    {
        return string.Equals(value, SliAccountabilityPacketState.Blocked, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAccountabilityPacketState.Candidate, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, SliAccountabilityPacketState.ReviewReady, StringComparison.OrdinalIgnoreCase);
    }

    private static bool WouldWidenReveal(string localityReveal, string surfaceReveal)
    {
        return string.Equals(localityReveal, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(surfaceReveal, "narrow", StringComparison.OrdinalIgnoreCase);
    }

    private static bool WouldWidenBoundary(string localityBoundary, string surfaceBoundary)
    {
        return string.Equals(localityBoundary, "sealed", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(surfaceBoundary, SliHigherOrderLocalityState.BoundedSealPosture, StringComparison.OrdinalIgnoreCase);
    }

    private static bool WouldWidenPacketReveal(string surfaceReveal, string packetReveal)
    {
        return string.Equals(surfaceReveal, SliHigherOrderLocalityState.MaskedRevealPosture, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(packetReveal, "narrow", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasBlockingWitnessResidues(SliWitnessState witness)
    {
        return witness.Residues.Any(residue =>
            residue.Kind is HigherOrderLocalityResidueKind.MissingWitnessPrerequisites or
                HigherOrderLocalityResidueKind.InvalidWitnessReference or
                HigherOrderLocalityResidueKind.InvalidGlueThreshold or
                HigherOrderLocalityResidueKind.IncompleteWitness or
                HigherOrderLocalityResidueKind.NonCandidateWitness);
    }

    private static bool HasBlockingTransportResidues(SliTransportState transport)
    {
        return transport.Residues.Any(residue =>
            residue.Kind is HigherOrderLocalityResidueKind.MissingTransportPrerequisites or
                HigherOrderLocalityResidueKind.InvalidTransportReference or
                HigherOrderLocalityResidueKind.InvalidTransportStatus or
                HigherOrderLocalityResidueKind.InvalidTransportMapping or
                HigherOrderLocalityResidueKind.IncompleteTransport or
                HigherOrderLocalityResidueKind.BlockedTransport);
    }

    private static bool HasBlockingAdmissibleSurfaceResidues(SliAdmissibleSurfaceState surface)
    {
        return surface.Residues.Any(residue =>
            residue.Kind is HigherOrderLocalityResidueKind.MissingAdmissibleSurfacePrerequisites or
                HigherOrderLocalityResidueKind.InvalidSurfaceReference or
                HigherOrderLocalityResidueKind.InvalidSurfaceClass or
                HigherOrderLocalityResidueKind.InvalidSurfaceStatus or
                HigherOrderLocalityResidueKind.InvalidSurfaceReveal or
                HigherOrderLocalityResidueKind.InvalidSurfaceBoundary or
                HigherOrderLocalityResidueKind.InvalidIdentityBearingApplicability or
                HigherOrderLocalityResidueKind.IncompleteAdmissibleSurface or
                HigherOrderLocalityResidueKind.BlockedAdmissibleSurface);
    }

    private static bool HasBlockingAccountabilityPacketResidues(SliAccountabilityPacketState packet)
    {
        return packet.Residues.Any(residue =>
            residue.Kind is HigherOrderLocalityResidueKind.MissingAccountabilityPacketPrerequisites or
                HigherOrderLocalityResidueKind.InvalidPacketReference or
                HigherOrderLocalityResidueKind.InvalidPacketStatus or
                HigherOrderLocalityResidueKind.InvalidPacketReveal or
                HigherOrderLocalityResidueKind.InvalidPacketInvariant or
                HigherOrderLocalityResidueKind.IncompleteAccountabilityPacket or
                HigherOrderLocalityResidueKind.BlockedAccountabilityPacket);
    }

    private static bool TryResolveWitnessReference(SliExecutionContext context, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        var state = context.HigherOrderLocalityState;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (string.Equals(token, "locality-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.LocalityHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.LocalityHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        if (!state.Rehearsal.IsConfigured)
        {
            return false;
        }

        if (state.Rehearsal.BranchSet.Contains(token, StringComparer.OrdinalIgnoreCase))
        {
            resolvedHandle = $"{state.Rehearsal.RehearsalHandle}:branch:{token}";
            return true;
        }

        if (token.StartsWith($"{state.Rehearsal.RehearsalHandle}:branch:", StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = token;
            return true;
        }

        return false;
    }

    private static bool TryResolveTransportWitnessHandle(SliHigherOrderLocalityState state, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !state.Witness.IsConfigured)
        {
            return false;
        }

        if (string.Equals(token, "witness-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.Witness.WitnessHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.Witness.WitnessHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        return false;
    }

    private static bool TryResolveAdmissibleTransportHandle(SliHigherOrderLocalityState state, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !state.Transport.IsConfigured)
        {
            return false;
        }

        if (string.Equals(token, "transport-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.Transport.TransportHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.Transport.TransportHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        return false;
    }

    private static bool TryResolveAdmissibleSurfaceHandle(SliHigherOrderLocalityState state, string token, out string resolvedHandle)
    {
        resolvedHandle = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || !state.AdmissibleSurface.IsConfigured)
        {
            return false;
        }

        if (string.Equals(token, "surface-state", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(token, state.AdmissibleSurface.SurfaceHandle, StringComparison.OrdinalIgnoreCase))
        {
            resolvedHandle = state.AdmissibleSurface.SurfaceHandle;
            return !string.IsNullOrWhiteSpace(resolvedHandle);
        }

        return false;
    }

    private static string Arg(SExpression expression, int index, string fallback)
    {
        if (expression.IsAtom || expression.Children.Count <= index)
        {
            return fallback;
        }

        return expression.Children[index].Atom ?? fallback;
    }

    private static string UnwrapStringLiteral(string value)
    {
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            return value[1..^1];
        }

        return value;
    }

    private static SoulFrameInferenceRequest BuildInferenceRequest(
        string task,
        string contextValue,
        SliExecutionContext context)
    {
        var constraints = new SoulFrameInferenceConstraints
        {
            Domain = SoulFrameGovernedPromptContextComposer.ResolveDomain(
                rawContext: contextValue,
                configuredDomain: context.OpalConstraints.Domain,
                objectiveHint: context.Frame.TaskObjective),
            DriftLimit = context.OpalConstraints.DriftLimit,
            MaxTokens = context.OpalConstraints.MaxTokens
        };

        return new SoulFrameInferenceRequest
        {
            Task = task,
            Context = contextValue,
            OpalConstraints = constraints,
            SoulFrameId = context.Frame.SoulFrameId,
            ContextId = context.Frame.ContextId,
            GovernanceProtocol = SoulFrameGovernedEmissionProtocol.CreateSeedRequired()
        };
    }

    private static CompassDoctrineBasin ResolveGoldenCodeActiveBasin(
        SliExecutionContext context,
        string primeState)
    {
        var advisoryBasin = context.LastClassifyResponse?.CompassAdvisory?.SuggestedActiveBasin;
        if (advisoryBasin.HasValue)
        {
            return advisoryBasin.Value;
        }

        var normalized = $"{context.OpalConstraints.Domain} {context.Frame.TaskObjective} {primeState}".ToLowerInvariant();

        if (normalized.Contains("bounded-locality continuity", StringComparison.Ordinal) ||
            normalized.Contains("bounded locality continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.BoundedLocalityContinuity;
        }

        if (normalized.Contains("fluid continuity law", StringComparison.Ordinal) ||
            normalized.Contains("fluid continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.FluidContinuityLaw;
        }

        if (normalized.Contains("identity continuity", StringComparison.Ordinal) ||
            normalized.Contains("identity-continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.IdentityContinuity;
        }

        if (normalized.Contains("continuity", StringComparison.Ordinal))
        {
            return CompassDoctrineBasin.GeneralContinuityDiscourse;
        }

        return CompassDoctrineBasin.Unknown;
    }

    private static CompassDoctrineBasin ResolveGoldenCodeCompetingBasin(
        SliExecutionContext context,
        CompassDoctrineBasin activeBasin)
    {
        var advisoryBasin = context.LastClassifyResponse?.CompassAdvisory?.SuggestedCompetingBasin;
        if (advisoryBasin.HasValue && advisoryBasin.Value != CompassDoctrineBasin.Unknown)
        {
            return advisoryBasin.Value;
        }

        return activeBasin switch
        {
            CompassDoctrineBasin.BoundedLocalityContinuity => CompassDoctrineBasin.FluidContinuityLaw,
            CompassDoctrineBasin.FluidContinuityLaw => CompassDoctrineBasin.BoundedLocalityContinuity,
            CompassDoctrineBasin.IdentityContinuity => CompassDoctrineBasin.IdentityContinuity,
            CompassDoctrineBasin.GeneralContinuityDiscourse => CompassDoctrineBasin.GeneralContinuityDiscourse,
            _ => CompassDoctrineBasin.Unknown
        };
    }

    private static CompassAnchorState ResolveGoldenCodeAnchorState(
        SliExecutionContext context,
        CompassDoctrineBasin activeBasin,
        CompassDoctrineBasin competingBasin,
        string primeState)
    {
        var response = context.LastClassifyResponse;
        if (response is not null && !response.Accepted)
        {
            return CompassAnchorState.Weakened;
        }

        var advisoryAnchor = response?.CompassAdvisory?.SuggestedAnchorState;
        if (advisoryAnchor.HasValue)
        {
            return advisoryAnchor.Value;
        }

        var advisoryText = string.Join(
            " ",
            new[]
            {
                response?.Decision,
                response?.Payload,
                response?.Governance.Content,
                response?.Governance.Trace,
                context.Frame.TaskObjective,
                primeState
            }.Where(value => !string.IsNullOrWhiteSpace(value)))
            .ToLowerInvariant();

        var activeSeen = BasinMarkers(activeBasin).Any(marker => advisoryText.Contains(marker, StringComparison.Ordinal));
        var competingSeen = BasinMarkers(competingBasin).Any(marker => advisoryText.Contains(marker, StringComparison.Ordinal));

        if (activeSeen && !competingSeen)
        {
            return CompassAnchorState.Held;
        }

        if (competingSeen && !activeSeen)
        {
            return CompassAnchorState.Lost;
        }

        return CompassAnchorState.Weakened;
    }

    private static CompassSelfTouchClass ResolveGoldenCodeSelfTouchClass(SliExecutionContext context)
    {
        var hint = context.Frame.SelfStateHint;
        if (hint is null)
        {
            return CompassSelfTouchClass.NoTouch;
        }

        if (hint.HasDeferredOrContradictedClaim)
        {
            return CompassSelfTouchClass.BoundaryContact;
        }

        if (hint.HasHotClaim || hint.ClaimCount > 0)
        {
            return CompassSelfTouchClass.HotClaimTouch;
        }

        if (hint.ValidationConceptCount > 0)
        {
            return CompassSelfTouchClass.ValidationTouch;
        }

        return CompassSelfTouchClass.NoTouch;
    }

    private static CompassOeCoePosture ResolveGoldenCodeOeCoePosture(
        SliExecutionContext context,
        SliGoldenCodeState state)
    {
        var response = context.LastClassifyResponse;
        var advisory = response?.CompassAdvisory;
        var advisoryAccepted =
            response?.Accepted == true &&
            advisory is not null &&
            advisory.Confidence >= 0.55 &&
            advisory.SuggestedActiveBasin == state.ActiveBasin &&
            (advisory.SuggestedCompetingBasin == state.CompetingBasin ||
             advisory.SuggestedCompetingBasin == CompassDoctrineBasin.Unknown) &&
            advisory.SuggestedAnchorState == state.AnchorState &&
            advisory.SuggestedSelfTouchClass == state.SelfTouchClass;

        if (!advisoryAccepted)
        {
            return CompassOeCoePosture.Unresolved;
        }

        var hint = context.Frame.CleaverHint;
        if (hint is null)
        {
            return CompassOeCoePosture.Unresolved;
        }

        if (hint.KnownRatio >= 0.66 && hint.UnknownRatio <= 0.34)
        {
            return CompassOeCoePosture.OeDominant;
        }

        if (hint.UnknownRatio >= 0.66)
        {
            return CompassOeCoePosture.CoeDominant;
        }

        return CompassOeCoePosture.ShuntedBalanced;
    }

    private static void RefreshGoldenCodeContracts(SliExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var state = context.GoldenCodeState;
        var updateLocus = ResolveGoldenCodeUpdateLocus(context, state);
        state.PacketDirective = CreatePacketDirective(context, updateLocus);
        state.IdentityKernelBoundary = CreateIdentityKernelBoundary(context, state, updateLocus);
        state.PacketValidity = CreatePacketValidity(context, state, updateLocus);
    }

    private static SliUpdateLocus ResolveGoldenCodeUpdateLocus(
        SliExecutionContext context,
        SliGoldenCodeState state)
    {
        if (context.LastClassifyResponse?.Accepted == false)
        {
            return SliUpdateLocus.Reject;
        }

        if (state.ActiveBasin == CompassDoctrineBasin.IdentityContinuity)
        {
            return SliUpdateLocus.Kernel;
        }

        var hint = context.Frame.CleaverHint;
        if (hint is not null && hint.UnknownRatio >= 0.4)
        {
            return SliUpdateLocus.Gap;
        }

        return state.IsProjected
            ? SliUpdateLocus.Sheaf
            : SliUpdateLocus.Reject;
    }

    private static SliPacketDirective CreatePacketDirective(
        SliExecutionContext context,
        SliUpdateLocus updateLocus)
    {
        if (context.LastClassifyResponse?.Accepted == false || updateLocus == SliUpdateLocus.Reject)
        {
            return new SliPacketDirective(
                SliThinkingTier.Master,
                SliPacketClass.Refusal,
                SliEngramOperation.Refuse,
                updateLocus,
                SliAuthorityClass.CandidateBearing);
        }

        if (string.Equals(context.FinalDecision, "defer", StringComparison.OrdinalIgnoreCase))
        {
            return new SliPacketDirective(
                SliThinkingTier.Master,
                SliPacketClass.Observation,
                SliEngramOperation.NoOp,
                updateLocus,
                SliAuthorityClass.CandidateBearing);
        }

        return new SliPacketDirective(
            SliThinkingTier.Master,
            SliPacketClass.Commitment,
            SliEngramOperation.Write,
            updateLocus,
            SliAuthorityClass.CandidateBearing);
    }

    private static IdentityKernelBoundaryReceipt CreateIdentityKernelBoundary(
        SliExecutionContext context,
        SliGoldenCodeState state,
        SliUpdateLocus updateLocus)
    {
        var basinToken = state.ActiveBasin.ToString().ToLowerInvariant();
        return new IdentityKernelBoundaryReceipt(
            CmeIdentityHandle: $"cme:{context.Frame.CMEId}",
            IdentityKernelHandle: $"kernel:{context.Frame.CMEId}",
            ContinuityAnchorHandle: $"anchor:{context.Frame.CMEId}:{basinToken}",
            KernelBound: updateLocus == SliUpdateLocus.Kernel,
            CandidateLocus: updateLocus);
    }

    private static SliPacketValidityReceipt CreatePacketValidity(
        SliExecutionContext context,
        SliGoldenCodeState state,
        SliUpdateLocus updateLocus)
    {
        const bool syntaxOk = true;
        var hexadOk = state.AnchorState != CompassAnchorState.Lost;
        var scepOk = updateLocus != SliUpdateLocus.Reject;
        var policyEligible = context.LastClassifyResponse?.Accepted != false;
        var reasonCode = ResolvePacketValidityReasonCode(syntaxOk, hexadOk, scepOk, policyEligible);
        return new SliPacketValidityReceipt(syntaxOk, hexadOk, scepOk, policyEligible, reasonCode);
    }

    private static string ResolvePacketValidityReasonCode(
        bool syntaxOk,
        bool hexadOk,
        bool scepOk,
        bool policyEligible)
    {
        if (!syntaxOk)
        {
            return "sli-syntax-invalid";
        }

        if (!hexadOk)
        {
            return "sli-hexad-out-of-band";
        }

        if (!scepOk)
        {
            return "sli-scep-reject";
        }

        if (!policyEligible)
        {
            return "sli-policy-withheld";
        }

        return "sli-packet-valid";
    }

    private static IReadOnlyList<string> BasinMarkers(CompassDoctrineBasin basin) => basin switch
    {
        CompassDoctrineBasin.BoundedLocalityContinuity => ["bounded-locality continuity", "bounded locality continuity", "locality witness"],
        CompassDoctrineBasin.FluidContinuityLaw => ["fluid continuity law", "fluid continuity"],
        CompassDoctrineBasin.IdentityContinuity => ["identity continuity", "identity-continuity"],
        CompassDoctrineBasin.GeneralContinuityDiscourse => ["continuity discourse", "continuity"],
        _ => Array.Empty<string>()
    };
}
