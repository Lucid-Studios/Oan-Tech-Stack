using System.Security.Cryptography;
using System.Text;
using CradleTek.Host.Interfaces;
using Hdt.Core.Artifacts;
using Hdt.Core.Models;
using Hdt.Core.Security;
using Hdt.Core.Services;
using Oan.Common;

namespace Oan.Cradle;

public sealed class LocalHdtHopngArtifactService : IHopngArtifactService
{
    private const string Signer = "CradleTek Governed Transit";
    private const string KeyId = "cradletek-governed-hopng";

    private readonly string? _explicitOutputRoot;
    private readonly ArtifactJsonStore _jsonStore = new();
    private readonly HopngArtifactBuilder _builder = new();
    private readonly HopngArtifactLoader _loader = new();
    private readonly HopngArtifactValidator _validator = new();
    private readonly GovernedProjectionDerivationService _projectionDerivation = new();
    private readonly TemporalPhaseStackService _phaseStack = new();
    private readonly Ed25519SignatureService _signatureService = new();

    public LocalHdtHopngArtifactService(string? explicitOutputRoot = null)
    {
        _explicitOutputRoot = explicitOutputRoot;
    }

    public string ContainerName => "cradletek-hopng";

    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<GovernedHopngArtifactReceipt> EmitAsync(
        GovernedHopngEmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var artifactHandle = GovernedHopngArtifactKeys.CreateArtifactHandle(request.LoopKey, request.Profile);
        var outputRoot = ResolveOutputRoot();
        var artifactDirectory = Path.Combine(outputRoot, GovernedHopngArtifactKeys.CreateArtifactDirectoryName(request.LoopKey, request.Profile));
        Directory.CreateDirectory(artifactDirectory);

        try
        {
            var newArtifact = _builder.Create(new NewHopngRequest(
                OutputDirectory: artifactDirectory,
                Name: GovernedHopngArtifactKeys.GetProfileSlug(request.Profile),
                Signer: Signer,
                KeyId: KeyId,
                DisplayName: $"{request.Profile} {request.LoopKey}",
                ArtifactId: CreateArtifactId(request.LoopKey, request.Profile),
                PrivateKeyPath: null,
                PrivateKeyOutputPath: Path.Combine(outputRoot, "_keys", $"{KeyId}.ed25519.private.key"),
                PublicKeyOutputPath: Path.Combine(outputRoot, "_keys", $"{KeyId}.ed25519.public.key")));

            var artifact = EnrichArtifact(newArtifact, request);
            var validation = _validator.Validate(artifact.Layout.ManifestPath);

            var outcome = validation.Errors.Count == 0
                ? GovernedHopngArtifactOutcome.Created
                : GovernedHopngArtifactOutcome.FailedValidation;
            var projectionSummary = request.Profile == GovernedHopngArtifactProfile.GoverningTrafficEvidence
                ? BuildProjectionSummary(artifact, validation)
                : BuildPhaseSummary(artifact, validation);

            return Task.FromResult(new GovernedHopngArtifactReceipt(
                ArtifactHandle: artifactHandle,
                LoopKey: request.LoopKey,
                CandidateId: request.CandidateId,
                CandidateProvenance: request.CandidateProvenance,
                Profile: request.Profile,
                Stage: request.Stage,
                Outcome: outcome,
                IssuedBy: Signer,
                TimestampUtc: DateTimeOffset.UtcNow,
                ArtifactId: artifact.Manifest.ArtifactId,
                ManifestPath: artifact.Layout.ManifestPath,
                ProjectionPath: artifact.Layout.ProjectionPath,
                ValidationSummary: $"validation-errors:{validation.Errors.Count}",
                ProfileSummary: projectionSummary,
                FailureCode: outcome == GovernedHopngArtifactOutcome.Created ? null : "hopng-validation-failed"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new GovernedHopngArtifactReceipt(
                ArtifactHandle: artifactHandle,
                LoopKey: request.LoopKey,
                CandidateId: request.CandidateId,
                CandidateProvenance: request.CandidateProvenance,
                Profile: request.Profile,
                Stage: request.Stage,
                Outcome: GovernedHopngArtifactOutcome.Failed,
                IssuedBy: Signer,
                TimestampUtc: DateTimeOffset.UtcNow,
                ArtifactId: null,
                ManifestPath: null,
                ProjectionPath: null,
                ValidationSummary: null,
                ProfileSummary: null,
                FailureCode: $"hopng-emission-failed:{ex.GetType().Name}"));
        }
    }

    private LoadedHopngArtifact EnrichArtifact(LoadedHopngArtifact artifact, GovernedHopngEmissionRequest request)
    {
        WritePhase2Sidecars(artifact, request);
        if (request.Profile == GovernedHopngArtifactProfile.GovernanceTelemetryPhaseStack)
        {
            WritePhase3Sidecars(artifact, request);
        }

        RewriteManifestAndTrust(artifact, request);
        return _loader.Load(artifact.Layout.ManifestPath);
    }

    private void WritePhase2Sidecars(LoadedHopngArtifact artifact, GovernedHopngEmissionRequest request)
    {
        var artifactId = artifact.Manifest.ArtifactId;
        var universes = new UniverseLayerSet
        {
            ArtifactId = artifactId,
            Universes =
            [
                CreateUniverse("governance-decision", "decision-visible"),
                CreateUniverse("collapse-routing", "collapse-audit"),
                CreateUniverse("publication-state", "publication-audit")
            ]
        };
        var gluing = new GluingManifest
        {
            ArtifactId = artifactId,
            Relations =
            [
                new GluingRelation
                {
                    RelationId = "decision-routes-collapse",
                    SourceUniverseId = "governance-decision",
                    TargetUniverseId = "collapse-routing",
                    RelationType = "governance-routing",
                    RequiredForFormation = true
                },
                new GluingRelation
                {
                    RelationId = "decision-authorizes-publication",
                    SourceUniverseId = "governance-decision",
                    TargetUniverseId = "publication-state",
                    RelationType = "governance-publication",
                    RequiredForFormation = true
                },
                new GluingRelation
                {
                    RelationId = "publication-completes-loop",
                    SourceUniverseId = "publication-state",
                    TargetUniverseId = "collapse-routing",
                    RelationType = "loop-completion",
                    RequiredForFormation = false
                }
            ]
        };
        var projectionRules = new ProjectionRules
        {
            ArtifactId = artifactId,
            Rules =
            [
                new ProjectionRule
                {
                    RuleId = "decision-visible",
                    SourceUniverseId = "governance-decision",
                    TargetProjectionRole = "projection-surface",
                    MappingType = "decision-summary",
                    Precedence = 1
                },
                new ProjectionRule
                {
                    RuleId = "collapse-audit",
                    SourceUniverseId = "collapse-routing",
                    TargetProjectionRole = "audit-surface",
                    MappingType = "collapse-audit",
                    Precedence = 2
                },
                new ProjectionRule
                {
                    RuleId = "publication-audit",
                    SourceUniverseId = "publication-state",
                    TargetProjectionRole = "audit-surface",
                    MappingType = "publication-audit",
                    Precedence = 3
                }
            ]
        };
        var legibility = new LegibilityProfile
        {
            ArtifactId = artifactId,
            RequiredUniverses = ["governance-decision", "collapse-routing", "publication-state"],
            RequiredRelations = ["decision-routes-collapse", "decision-authorizes-publication"],
            ProjectionIntegrityRequired = true
        };

        _jsonStore.WriteCanonical(artifact.Layout.UniverseLayerPath, universes);
        _jsonStore.WriteCanonical(artifact.Layout.GluingManifestPath, gluing);
        _jsonStore.WriteCanonical(artifact.Layout.ProjectionRulesPath, projectionRules);
        _jsonStore.WriteCanonical(artifact.Layout.LegibilityProfilePath, legibility);
    }

    private void WritePhase3Sidecars(LoadedHopngArtifact artifact, GovernedHopngEmissionRequest request)
    {
        var orderedEntries = request.JournalEntries
            .OrderBy(entry => entry.Timestamp)
            .ToArray();
        var artifactId = artifact.Manifest.ArtifactId;
        var cadenceMs = CalculateCadenceMs(orderedEntries);
        var observedDurationMs = Math.Max(1, orderedEntries.Length * cadenceMs);
        var eventSlices = new List<EventSlice>(orderedEntries.Length);
        var sharedEvidence = BuildProtectedEvidenceReferences(request, request.Snapshot);

        for (var index = 0; index < orderedEntries.Length; index++)
        {
            var entry = orderedEntries[index];
            var partialSnapshot = GovernanceLoopStateModel.Project(request.LoopKey, orderedEntries.Take(index + 1).ToArray());
            var sliceTime = new DateTimeOffset(entry.Timestamp, TimeSpan.Zero);
            var universeStates = BuildUniverseStates(partialSnapshot, request.CollapseRoutingDecision);
            eventSlices.Add(new EventSlice
            {
                EventSliceId = $"event-{index + 1:D4}",
                ArtifactId = artifactId,
                N = index + 1,
                TimestampStartUtc = sliceTime,
                TimestampEndUtc = sliceTime,
                RawStartN = index + 1,
                RawEndN = index + 1,
                RawSliceSpan = 1,
                ObservedEventCount = 1,
                UniverseStates = universeStates,
                ProtectedEvidenceRefs = sharedEvidence,
                SliceDigest = ComputeSliceDigest(request.LoopKey, index + 1, universeStates)
            });
        }

        var eventSliceSet = new EventSliceSet
        {
            ArtifactId = artifactId,
            ObservedSet = new ObservedSetHeader
            {
                ObservedSetId = $"observed-{CreateArtifactId(request.LoopKey, request.Profile)}",
                ArtifactId = artifactId,
                ObservedDurationMs = observedDurationMs,
                BaseSliceCadenceMs = cadenceMs,
                RawSliceCount = Math.Max(1, orderedEntries.Length),
                ObservedEventCount = orderedEntries.Length,
                EventGroupingMode = "fixed_raw_count",
                EventGroupingSizeRawSlices = 1,
                ProtectedEvidenceRefs = sharedEvidence,
                PrimeSafeInspectionMode = "metadata_only",
                DataCustodyMode = "protected_external"
            },
            Slices = eventSlices
        };
        var phasePolicy = new PhasePolicy
        {
            ArtifactId = artifactId,
            RawCadenceMs = cadenceMs,
            EventGroupingMode = "fixed_raw_count",
            EventGroupingSizeRawSlices = 1,
            PhaseWindowMode = "fixed_event_count",
            PhaseWindowSizeEventSlices = 2,
            ComparisonHorizonRawSlices = 2,
            AggregationPolicies = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["pressure"] = "mean",
                ["drift"] = "delta",
                ["bloom"] = "latest"
            },
            PrimeSafeInspectionMode = "metadata_only",
            PrivilegedInspectionMode = "full_payload"
        };
        var opticalChannels = new OpticalChannelsDefinition
        {
            ArtifactId = artifactId,
            RequiredChannels = ["pressure", "drift", "bloom"],
            ReservedChannels = ["force", "opacity", "hue", "saturation"],
            Channels =
            [
                CreateChannel("pressure", "governance-pressure"),
                CreateChannel("drift", "governance-drift"),
                CreateChannel("bloom", "governance-bloom")
            ]
        };

        _jsonStore.WriteCanonical(artifact.Layout.EventSlicePath, eventSliceSet);
        _jsonStore.WriteCanonical(artifact.Layout.PhasePolicyPath, phasePolicy);
        _jsonStore.WriteCanonical(artifact.Layout.OpticalChannelsPath, opticalChannels);

        var reloaded = _loader.Load(artifact.Layout.ManifestPath);
        var phaseSlices = new PhaseSliceSet
        {
            ArtifactId = artifactId,
            Slices = _phaseStack.DeriveExpectedPhaseSlices(reloaded)
        };
        _jsonStore.WriteCanonical(artifact.Layout.PhaseSlicePath, phaseSlices);
    }

    private void RewriteManifestAndTrust(LoadedHopngArtifact artifact, GovernedHopngEmissionRequest request)
    {
        var manifest = artifact.Manifest with
        {
            Sidecars = BuildSidecars(artifact, request.Profile),
            FileDigests = BuildFileDigests(artifact, request.Profile),
            VisibilityPolicy = artifact.Manifest.VisibilityPolicy with
            {
                CrypticReferences = BuildVisibilityReferences(request, artifact)
            }
        };
        _jsonStore.WriteCanonical(artifact.Layout.ManifestPath, manifest);

        var hashSidecar = new HashSidecar
        {
            ArtifactId = manifest.ArtifactId,
            ManifestCanonicalSha256 = ArtifactHashing.ComputeSha256(artifact.Layout.ManifestPath),
            ArtifactSetSha256 = ArtifactHashing.ComputeArtifactSetSha256(
                manifest.FileDigests,
                ArtifactHashing.ComputeSha256(artifact.Layout.ManifestPath)),
            FileDigests = manifest.FileDigests
        };
        _jsonStore.WriteCanonical(artifact.Layout.HashPath, hashSidecar);

        var privateKeyPath = Path.Combine(Path.GetDirectoryName(artifact.Layout.PrivateKeyPath) ?? artifact.Layout.DirectoryPath, $"{KeyId}.ed25519.private.key");
        var keyMaterial = _signatureService.CreateOrLoad(privateKeyPath, privateKeyPath);
        var hashBytes = File.ReadAllBytes(artifact.Layout.HashPath);
        var signature = _signatureService.Sign(keyMaterial.PrivateKeyBase64, hashBytes);
        _jsonStore.WriteCanonical(artifact.Layout.SignaturePath, new SignatureSidecar
        {
            ArtifactId = manifest.ArtifactId,
            KeyId = KeyId,
            SignedUtc = DateTimeOffset.UtcNow,
            SignedObjectSha256 = ArtifactHashing.ComputeSha256(hashBytes),
            SignatureBase64 = Convert.ToBase64String(signature)
        });
    }

    private static List<SidecarReference> BuildSidecars(LoadedHopngArtifact artifact, GovernedHopngArtifactProfile profile)
    {
        var sidecars = new List<SidecarReference>
        {
            Sidecar("layer-map", "oan.hopng_layer_map", artifact.Layout.LayerMapPath),
            Sidecar("trust-envelope", "oan.hopng_trust_envelope", artifact.Layout.TrustEnvelopePath),
            Sidecar("transform-history", "oan.hopng_transform_history", artifact.Layout.TransformHistoryPath),
            Sidecar("depth-field", "oan.hopng_depth_field", artifact.Layout.DepthFieldPath),
            Sidecar("hash", "oan.hopng_hash_set", artifact.Layout.HashPath),
            Sidecar("signature", "oan.hopng_signature", artifact.Layout.SignaturePath),
            Sidecar("universe-layer", "oan.hopng_universe_layer", artifact.Layout.UniverseLayerPath),
            Sidecar("gluing-manifest", "oan.hopng_gluing_manifest", artifact.Layout.GluingManifestPath),
            Sidecar("projection-rules", "oan.hopng_projection_rules", artifact.Layout.ProjectionRulesPath),
            Sidecar("legibility-profile", "oan.hopng_legibility_profile", artifact.Layout.LegibilityProfilePath)
        };

        if (profile == GovernedHopngArtifactProfile.GovernanceTelemetryPhaseStack)
        {
            sidecars.Add(Sidecar("event-slices", "oan.hopng_event_slice", artifact.Layout.EventSlicePath));
            sidecars.Add(Sidecar("phase-slices", "oan.hopng_phase_slice", artifact.Layout.PhaseSlicePath));
            sidecars.Add(Sidecar("phase-policy", "oan.hopng_phase_policy", artifact.Layout.PhasePolicyPath));
            sidecars.Add(Sidecar("optical-channels", "oan.hopng_optical_channels", artifact.Layout.OpticalChannelsPath));
        }

        return sidecars;
    }

    private static List<ArtifactFileDigest> BuildFileDigests(LoadedHopngArtifact artifact, GovernedHopngArtifactProfile profile)
    {
        var digests = new List<ArtifactFileDigest>
        {
            FileDigest("projection", artifact.Layout.ProjectionPath),
            FileDigest("layer-map", artifact.Layout.LayerMapPath),
            FileDigest("trust-envelope", artifact.Layout.TrustEnvelopePath),
            FileDigest("transform-history", artifact.Layout.TransformHistoryPath),
            FileDigest("depth-field", artifact.Layout.DepthFieldPath),
            FileDigest("universe-layer", artifact.Layout.UniverseLayerPath),
            FileDigest("gluing-manifest", artifact.Layout.GluingManifestPath),
            FileDigest("projection-rules", artifact.Layout.ProjectionRulesPath),
            FileDigest("legibility-profile", artifact.Layout.LegibilityProfilePath)
        };

        if (profile == GovernedHopngArtifactProfile.GovernanceTelemetryPhaseStack)
        {
            digests.Add(FileDigest("event-slices", artifact.Layout.EventSlicePath));
            digests.Add(FileDigest("phase-slices", artifact.Layout.PhaseSlicePath));
            digests.Add(FileDigest("phase-policy", artifact.Layout.PhasePolicyPath));
            digests.Add(FileDigest("optical-channels", artifact.Layout.OpticalChannelsPath));
        }

        return digests;
    }

    private static UniverseLayer CreateUniverse(string universeId, string projectionRole)
    {
        return new UniverseLayer
        {
            UniverseId = universeId,
            Modality = "governed-telemetry",
            NeutralPlane = 0d,
            ProjectionRole = projectionRole,
            CoordinateFrame = new CoordinateFrame
            {
                XAxis = "x",
                YAxis = "y",
                ZAxis = "t",
                Units = "normalized"
            }
        };
    }

    private static OpticalChannelDefinition CreateChannel(string channelId, string meaning)
    {
        return new OpticalChannelDefinition
        {
            ChannelId = channelId,
            CanonicalMeaning = meaning,
            Required = true,
            DerivedOnly = false,
            UsageMode = "governance-telemetry"
        };
    }

    private static SidecarReference Sidecar(string role, string schema, string path)
    {
        return new SidecarReference
        {
            Role = role,
            Schema = schema,
            SchemaVersion = "0.1.0",
            Path = Path.GetFileName(path),
            Required = true
        };
    }

    private static ArtifactFileDigest FileDigest(string role, string path)
    {
        return new ArtifactFileDigest
        {
            Role = role,
            Path = Path.GetFileName(path),
            Sha256 = ArtifactHashing.ComputeSha256(path)
        };
    }

    private static List<CrypticReference> BuildVisibilityReferences(GovernedHopngEmissionRequest request, LoadedHopngArtifact artifact)
    {
        return GovernedHopngEvidenceReferences.Build(request, request.Snapshot)
            .Select(reference => new CrypticReference
            {
                Id = reference.RefId,
                PointerUri = reference.PointerUri,
                Policy = "governed-hopng-evidence",
                Summary = reference.Summary
            })
            .ToList();
    }

    private static List<ProtectedEvidenceReference> BuildProtectedEvidenceReferences(
        GovernedHopngEmissionRequest request,
        GovernanceLoopStateSnapshot snapshot)
    {
        return GovernedHopngEvidenceReferences.Build(request, snapshot)
            .Select(reference => new ProtectedEvidenceReference
            {
                RefId = reference.RefId,
                PointerUri = reference.PointerUri,
                DigestSha256 = ArtifactHashing.ComputeSha256(Encoding.UTF8.GetBytes(reference.PointerUri)),
                Summary = reference.Summary
            })
            .ToList();
    }

    private static Dictionary<string, TemporalUniverseState> BuildUniverseStates(
        GovernanceLoopStateSnapshot snapshot,
        CmeCollapseRoutingDecision? collapseRoutingDecision)
    {
        return new Dictionary<string, TemporalUniverseState>(StringComparer.Ordinal)
        {
            ["governance-decision"] = BuildDecisionUniverse(snapshot),
            ["collapse-routing"] = BuildCollapseUniverse(snapshot, collapseRoutingDecision),
            ["publication-state"] = BuildPublicationUniverse(snapshot)
        };
    }

    private static TemporalUniverseState BuildDecisionUniverse(GovernanceLoopStateSnapshot snapshot)
    {
        var decision = snapshot.DecisionReceipt?.Decision;
        var pressure = decision switch
        {
            GovernanceDecision.Approved => 1.0,
            GovernanceDecision.Deferred => 0.5,
            GovernanceDecision.Rejected => 0.0,
            _ => 0.0
        };
        var drift = snapshot.Stage switch
        {
            GovernanceLoopStage.GovernanceDecisionDeferred => 0.5,
            GovernanceLoopStage.PendingRecovery => 1.0,
            GovernanceLoopStage.LoopFailed => 1.0,
            _ => 0.0
        };
        var bloom = snapshot.DecisionReceipt is null
            ? 0.0
            : snapshot.DecisionReceipt.AuthorizedDerivativeLanes switch
            {
                GovernedPrimeDerivativeLane.Pointer | GovernedPrimeDerivativeLane.CheckedView => 1.0,
                GovernedPrimeDerivativeLane.Neither => 0.0,
                _ => 0.5
            };

        return new TemporalUniverseState
        {
            Pressure = pressure,
            Drift = drift,
            Bloom = bloom
        };
    }

    private static TemporalUniverseState BuildCollapseUniverse(
        GovernanceLoopStateSnapshot snapshot,
        CmeCollapseRoutingDecision? collapseRoutingDecision)
    {
        var pressure = snapshot.LatestCollapseQualification?.ClassificationConfidence
            ?? collapseRoutingDecision?.ClassificationConfidence
            ?? 0.0;
        var triggerCount = CountFlags(snapshot.LatestCollapseQualification?.ReviewTriggers ?? collapseRoutingDecision?.ReviewTriggers ?? CmeCollapseReviewTrigger.None);
        var drift = triggerCount switch
        {
            <= 0 => 0.0,
            1 => 0.5,
            _ => 1.0
        };
        var residueClass = snapshot.LatestCollapseQualification?.ResidueClass ?? collapseRoutingDecision?.ResidueClass;
        var bloom = residueClass switch
        {
            CmeCollapseResidueClass.AutobiographicalProtected => 1.0,
            CmeCollapseResidueClass.ContextualProtected => 0.5,
            _ => 0.0
        };

        return new TemporalUniverseState
        {
            Pressure = pressure,
            Drift = drift,
            Bloom = bloom
        };
    }

    private static TemporalUniverseState BuildPublicationUniverse(GovernanceLoopStateSnapshot snapshot)
    {
        var publishedLaneCount = CountFlags(snapshot.PublishedLanes);
        var pressure = publishedLaneCount switch
        {
            <= 0 => 0.0,
            1 => 0.5,
            _ => 1.0
        };
        var drift = snapshot.Stage == GovernanceLoopStage.PendingRecovery || snapshot.FailureStage is not null
            ? 1.0
            : 0.0;
        var bloom = snapshot.Stage == GovernanceLoopStage.LoopCompleted ? 1.0 : 0.25;

        return new TemporalUniverseState
        {
            Pressure = pressure,
            Drift = drift,
            Bloom = bloom
        };
    }

    private static int CountFlags<T>(T value) where T : Enum
    {
        var raw = Convert.ToUInt64(value);
        var count = 0;
        while (raw > 0)
        {
            count += (int)(raw & 1UL);
            raw >>= 1;
        }

        return count;
    }

    private static string ComputeSliceDigest(
        string loopKey,
        int sequence,
        IReadOnlyDictionary<string, TemporalUniverseState> universeStates)
    {
        var builder = new StringBuilder();
        builder.Append(loopKey).Append('|').Append(sequence);
        foreach (var pair in universeStates.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append('|')
                .Append(pair.Key)
                .Append(':')
                .Append(pair.Value.Pressure.ToString("F3"))
                .Append(':')
                .Append(pair.Value.Drift.ToString("F3"))
                .Append(':')
                .Append(pair.Value.Bloom.ToString("F3"));
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string ResolveOutputRoot()
    {
        if (!string.IsNullOrWhiteSpace(_explicitOutputRoot))
        {
            return Path.GetFullPath(_explicitOutputRoot);
        }

        var repoRoot = ResolveRepoRoot();
        var localOverridePath = Path.Combine(repoRoot, ".local", "hopng_output_root.txt");
        if (File.Exists(localOverridePath))
        {
            var localOverride = File.ReadAllText(localOverridePath).Trim();
            if (!string.IsNullOrWhiteSpace(localOverride))
            {
                return Path.GetFullPath(localOverride);
            }
        }

        var envOverride = Environment.GetEnvironmentVariable("OAN_HOPNG_OUTPUT_ROOT");
        if (!string.IsNullOrWhiteSpace(envOverride))
        {
            return Path.GetFullPath(envOverride);
        }

        return Path.Combine(repoRoot, "runtime", "telemetry", "hopng");
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Oan.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private static string CreateArtifactId(string loopKey, GovernedHopngArtifactProfile profile)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{loopKey}|{profile}"));
        return Convert.ToHexString(hash).ToLowerInvariant()[..32];
    }

    private static int CalculateCadenceMs(IReadOnlyList<GovernanceJournalEntry> entries)
    {
        if (entries.Count < 2)
        {
            return 1;
        }

        var deltas = new List<int>();
        for (var index = 1; index < entries.Count; index++)
        {
            var delta = (int)Math.Max(1, (entries[index].Timestamp - entries[index - 1].Timestamp).TotalMilliseconds);
            if (delta > 0)
            {
                deltas.Add(delta);
            }
        }

        return deltas.Count == 0 ? 1 : deltas.Min();
    }

    private string BuildProjectionSummary(LoadedHopngArtifact artifact, Hdt.Core.Validation.ValidationResult validation)
    {
        var projection = _projectionDerivation.Derive(artifact, validation);
        return $"projection:{projection.Status};issues:{projection.Issues.Count}";
    }

    private string BuildPhaseSummary(LoadedHopngArtifact artifact, Hdt.Core.Validation.ValidationResult validation)
    {
        var render = _phaseStack.Render(artifact, validation, view: "prime");
        return $"phase-stack:{render.Status};slices:{render.PhaseSliceCount};issues:{render.Issues.Count}";
    }
}
