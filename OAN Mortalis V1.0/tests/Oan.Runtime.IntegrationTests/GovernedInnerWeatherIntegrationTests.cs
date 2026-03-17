using CradleTek.Cryptic;
using CradleTek.Host.Interfaces;
using CradleTek.Mantle;
using CradleTek.Public;
using EngramGovernance.Services;
using Oan.Common;
using Oan.Cradle;
using Oan.Storage;
using System.Text.Json.Nodes;
using Telemetry.GEL;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedInnerWeatherIntegrationTests
{
    [Fact]
    public async Task GoldenPath_ProjectsInnerWeatherIntoResultStatusJournalAndHopng()
    {
        var telemetry = new RecordingTelemetrySink();
        var storageTelemetry = new RecordingTelemetrySink();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);

        await mantle.AppendAsync(
            new CrypticCustodyAppendRequest(
                identityId,
                CustodyDomain: "cMoS",
                PayloadPointer: "cmos://seed/source",
                Classification: "seed"));

        var cognition = new FakeGovernanceCognitionSequenceService(
            CreateApprovedWorkResult(identityId, request, 1, CompassSeedAdvisoryDisposition.Accepted),
            CreateApprovedWorkResult(identityId, request, 2, CompassSeedAdvisoryDisposition.Deferred),
            CreateApprovedWorkResult(identityId, request, 3, CompassSeedAdvisoryDisposition.Rejected));
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), new GelTelemetryAdapter(), journal);
        var stores = CreateStoreRegistry(
            telemetry,
            storageTelemetry,
            publicLayer,
            mantle,
            publicLayer,
            journal,
            cognition,
            steward);
        var manager = new StackManager(stores);

        await manager.RunGovernanceGoldenPathAsync(request);
        await manager.RunGovernanceGoldenPathAsync(request);
        var third = await manager.RunGovernanceGoldenPathAsync(request);

        var status = await manager.GetStatusByLoopKeyAsync(third.LoopKey);
        var replay = await journal.ReplayLoopAsync(third.LoopKey);
        var innerWeatherEntry = Assert.Single(replay.Where(entry => entry.InnerWeatherReceipt is not null));
        var weatherDisclosureEntry = Assert.Single(replay.Where(entry => entry.WeatherDisclosureReceipt is not null));
        var officeAuthorityEntries = replay
            .Where(entry => entry.OfficeAuthorityReceipt is not null)
            .Select(entry => entry.OfficeAuthorityReceipt!)
            .OrderBy(entry => entry.Office)
            .ToArray();
        var officeIssuanceEntries = replay
            .Where(entry => entry.OfficeIssuanceReceipt is not null)
            .Select(entry => entry.OfficeIssuanceReceipt!)
            .ToArray();
        var workerHandoffEntries = replay
            .Where(entry => entry.WorkerHandoffReceipt is not null)
            .Select(entry => entry.WorkerHandoffReceipt!)
            .ToArray();
        var snapshot = GovernanceLoopStateModel.Project(third.LoopKey, replay);
        Assert.NotNull(third.CollapseRoutingDecision);
        var hopngRequest = new GovernedHopngEmissionRequest(
            LoopKey: third.LoopKey,
            CandidateId: third.CandidateId,
            CandidateProvenance: third.DecisionReceipt.CandidateProvenance,
            Profile: GovernedHopngArtifactProfile.GoverningTrafficEvidence,
            Stage: snapshot.Stage,
            RequestedBy: "CradleTek",
            DecisionReceipt: third.DecisionReceipt,
            Snapshot: snapshot,
            JournalEntries: replay,
            CollapseRoutingDecision: third.CollapseRoutingDecision!);
        var refs = GovernedHopngEvidenceReferences.Build(hopngRequest, snapshot);

        var resultReceipt = Assert.Single(third.InnerWeatherReceipts!);
        var statusReceipt = Assert.Single(status.InnerWeatherReceipts!);
        var resultDisclosureReceipt = Assert.Single(third.WeatherDisclosureReceipts!);
        var statusDisclosureReceipt = Assert.Single(status.WeatherDisclosureReceipts!);
        var resultOfficeReceipts = third.OfficeAuthorityReceipts!
            .OrderBy(receipt => receipt.Office)
            .ToArray();
        var statusOfficeReceipts = status.OfficeAuthorityReceipts!
            .OrderBy(receipt => receipt.Office)
            .ToArray();
        var snapshotOfficeReceipts = snapshot.OfficeAuthorityReceipts!
            .OrderBy(receipt => receipt.Office)
            .ToArray();
        var resultIssuanceReceipt = Assert.Single(third.OfficeIssuanceReceipts!);
        var statusIssuanceReceipt = Assert.Single(status.OfficeIssuanceReceipts!);
        var snapshotIssuanceReceipt = Assert.Single(snapshot.OfficeIssuanceReceipts!);
        var resultHandoffReceipt = Assert.Single(third.WorkerHandoffReceipts!);
        var statusHandoffReceipt = Assert.Single(status.WorkerHandoffReceipts!);
        var snapshotHandoffReceipt = Assert.Single(snapshot.WorkerHandoffReceipts!);
        var packet = third.CommunityWeatherPacket;
        Assert.NotNull(packet);
        Assert.Equal(3, resultOfficeReceipts.Length);
        Assert.Equal(3, statusOfficeReceipts.Length);
        Assert.Equal(3, officeAuthorityEntries.Length);
        Assert.Equal(3, snapshotOfficeReceipts.Length);
        Assert.Equal(3, snapshot.ReviewRequest!.OfficeAuthorityReceipts!.Count);
        Assert.Single(officeIssuanceEntries);
        Assert.Single(snapshot.ReviewRequest!.OfficeIssuanceReceipts!);
        Assert.Single(workerHandoffEntries);
        Assert.Single(snapshot.ReviewRequest!.WorkerHandoffReceipts!);
        Assert.Empty(third.WorkerReturnReceipts ?? Array.Empty<GovernedWorkerReturnReceipt>());
        Assert.Empty(status.WorkerReturnReceipts ?? Array.Empty<GovernedWorkerReturnReceipt>());
        Assert.Empty(snapshot.WorkerReturnReceipts ?? Array.Empty<GovernedWorkerReturnReceipt>());

        var stewardAuthority = Assert.Single(resultOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Steward));
        var fatherAuthority = Assert.Single(resultOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Father));
        var motherAuthority = Assert.Single(resultOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Mother));
        var statusStewardAuthority = Assert.Single(statusOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Steward));
        var statusFatherAuthority = Assert.Single(statusOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Father));
        var statusMotherAuthority = Assert.Single(statusOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Mother));
        var snapshotStewardAuthority = Assert.Single(snapshotOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Steward));
        var snapshotFatherAuthority = Assert.Single(snapshotOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Father));
        var snapshotMotherAuthority = Assert.Single(snapshotOfficeReceipts.Where(receipt => receipt.Office == InternalGoverningCmeOffice.Mother));

        Assert.Equal(resultReceipt.InnerWeatherHandle, statusReceipt.InnerWeatherHandle);
        Assert.Equal(resultReceipt.InnerWeatherHandle, innerWeatherEntry.InnerWeatherReceipt!.InnerWeatherHandle);
        Assert.Equal(resultReceipt.InnerWeatherHandle, Assert.Single(snapshot.InnerWeatherReceipts!).InnerWeatherHandle);
        Assert.Equal(AttentionResidueState.Persistent, resultReceipt.ResidueState);
        Assert.Equal(WindowIntegrityState.Intact, resultReceipt.WindowIntegrityState);
        Assert.Equal(resultDisclosureReceipt.DisclosureHandle, statusDisclosureReceipt.DisclosureHandle);
        Assert.Equal(resultDisclosureReceipt.DisclosureHandle, weatherDisclosureEntry.WeatherDisclosureReceipt!.DisclosureHandle);
        Assert.Equal(resultDisclosureReceipt.DisclosureHandle, Assert.Single(snapshot.WeatherDisclosureReceipts!).DisclosureHandle);
        Assert.Equal(StewardCareRoutingState.CheckInNeeded, resultDisclosureReceipt.RoutingState);
        Assert.Equal(CheckInCadenceState.Current, resultDisclosureReceipt.CadenceState);
        Assert.Equal(EvidenceSufficiencyState.Sufficient, resultDisclosureReceipt.EvidenceSufficiencyState);
        Assert.Equal(WeatherDisclosureScope.Steward, resultDisclosureReceipt.DisclosureScope);
        Assert.Equal(WeatherDisclosureRationaleCode.GuardedReduction, resultDisclosureReceipt.RationaleCode);
        Assert.Contains(WeatherWithheldMarker.GuardedEvidence, resultDisclosureReceipt.WithheldMarkers);
        Assert.Contains(StewardAttentionCause.DriftWeakening, resultDisclosureReceipt.StewardReasonCodes);
        Assert.Contains(StewardAttentionCause.ResiduePersistence, resultDisclosureReceipt.StewardReasonCodes);
        Assert.Equal(CommunityWeatherStatus.Unstable, packet!.Status);
        Assert.Equal(CommunityStewardAttentionState.Recommended, packet.StewardAttention);
        Assert.Equal(CommunityWeatherStatus.Unstable, status.CommunityWeatherPacket!.Status);
        Assert.Contains(refs, reference => reference.PointerUri == resultReceipt.InnerWeatherHandle);
        Assert.Contains(refs, reference => reference.PointerUri == resultDisclosureReceipt.DisclosureHandle);
        Assert.Equal(stewardAuthority.AuthorityHandle, statusStewardAuthority.AuthorityHandle);
        Assert.Equal(stewardAuthority.AuthorityHandle, snapshotStewardAuthority.AuthorityHandle);
        Assert.Equal(fatherAuthority.AuthorityHandle, statusFatherAuthority.AuthorityHandle);
        Assert.Equal(fatherAuthority.AuthorityHandle, snapshotFatherAuthority.AuthorityHandle);
        Assert.Equal(motherAuthority.AuthorityHandle, statusMotherAuthority.AuthorityHandle);
        Assert.Equal(motherAuthority.AuthorityHandle, snapshotMotherAuthority.AuthorityHandle);
        Assert.Contains(officeAuthorityEntries, receipt => receipt.AuthorityHandle == stewardAuthority.AuthorityHandle);
        Assert.Contains(officeAuthorityEntries, receipt => receipt.AuthorityHandle == fatherAuthority.AuthorityHandle);
        Assert.Contains(officeAuthorityEntries, receipt => receipt.AuthorityHandle == motherAuthority.AuthorityHandle);
        Assert.Contains(refs, reference => reference.PointerUri == stewardAuthority.AuthorityHandle);
        Assert.Contains(refs, reference => reference.PointerUri == fatherAuthority.AuthorityHandle);
        Assert.Contains(refs, reference => reference.PointerUri == motherAuthority.AuthorityHandle);
        Assert.DoesNotContain(refs, reference => reference.PointerUri == resultIssuanceReceipt.IssuanceHandle);
        Assert.DoesNotContain(refs, reference => reference.PointerUri == resultHandoffReceipt.HandoffHandle);
        Assert.Equal(OfficeViewEligibility.OfficeSpecificView, stewardAuthority.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.CheckInAllowed, stewardAuthority.ActionEligibility);
        Assert.Equal(OfficeAuthorityRationaleCode.OfficeSpecificStewardView, stewardAuthority.RationaleCode);
        Assert.Contains(StewardAttentionCause.DriftWeakening, stewardAuthority.AllowedReasonCodes);
        Assert.Contains(StewardAttentionCause.ResiduePersistence, stewardAuthority.AllowedReasonCodes);
        Assert.Equal(OfficeViewEligibility.Withheld, fatherAuthority.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.ViewOnly, fatherAuthority.ActionEligibility);
        Assert.Equal(OfficeAuthorityRationaleCode.WithheldForOfficeNotAttached, fatherAuthority.RationaleCode);
        Assert.Contains(OfficeAuthorityWithheldMarker.OfficeNotAttached, fatherAuthority.WithheldMarkers);
        Assert.Equal(OfficeViewEligibility.Withheld, motherAuthority.ViewEligibility);
        Assert.Equal(OfficeActionEligibility.ViewOnly, motherAuthority.ActionEligibility);
        Assert.Equal(OfficeAuthorityRationaleCode.WithheldForOfficeNotAttached, motherAuthority.RationaleCode);
        Assert.Contains(OfficeAuthorityWithheldMarker.OfficeNotAttached, motherAuthority.WithheldMarkers);
        Assert.Equal(InternalGoverningCmeOffice.Steward, resultIssuanceReceipt.Office);
        Assert.Equal(ConstructClass.IssuedOffice, resultIssuanceReceipt.ConstructClass);
        Assert.Equal(MaturityPosture.DoctrineBacked, resultIssuanceReceipt.MaturityPosture);
        Assert.Equal(stewardAuthority.AuthorityHandle, resultIssuanceReceipt.OfficeAuthorityHandle);
        Assert.Equal(resultDisclosureReceipt.DisclosureHandle, resultIssuanceReceipt.WeatherDisclosureHandle);
        Assert.Equal(resultIssuanceReceipt.IssuanceHandle, statusIssuanceReceipt.IssuanceHandle);
        Assert.Equal(resultIssuanceReceipt.IssuanceHandle, snapshotIssuanceReceipt.IssuanceHandle);
        Assert.Equal(resultIssuanceReceipt.PackageId, statusIssuanceReceipt.PackageId);
        Assert.Equal(resultIssuanceReceipt.PackageId, snapshotIssuanceReceipt.PackageId);
        Assert.Equal(resultIssuanceReceipt.PackageId, Assert.Single(officeIssuanceEntries).PackageId);
        Assert.Equal(resultHandoffReceipt.HandoffHandle, statusHandoffReceipt.HandoffHandle);
        Assert.Equal(resultHandoffReceipt.HandoffHandle, snapshotHandoffReceipt.HandoffHandle);
        Assert.Equal(resultHandoffReceipt.HandoffHandle, Assert.Single(workerHandoffEntries).HandoffHandle);
        Assert.Equal(InternalGoverningCmeOffice.Steward, resultHandoffReceipt.RequestingOffice);
        Assert.Equal(ConstructClass.BoundedWorker, resultHandoffReceipt.ConstructClass);
        Assert.Equal(GovernedWorkerSpecies.RepoBugStewardWorker, resultHandoffReceipt.WorkerSpecies);
        Assert.Equal(WorkerInstanceMode.RequestOnly, resultHandoffReceipt.WorkerInstanceMode);
        Assert.Equal(OfficeActionEligibility.CheckInAllowed, resultHandoffReceipt.ActionCeiling);
        Assert.Equal(CompassVisibilityClass.OperatorGuarded, resultHandoffReceipt.DisclosureClass);
        Assert.Equal(resultIssuanceReceipt.IssuanceHandle, resultHandoffReceipt.OfficeIssuanceHandle);
        Assert.Equal(stewardAuthority.AuthorityHandle, resultHandoffReceipt.OfficeAuthorityHandle);
        Assert.Equal(resultDisclosureReceipt.DisclosureHandle, resultHandoffReceipt.WeatherDisclosureHandle);
        Assert.Contains(
            telemetry.Events.OfType<GovernedInnerWeatherTelemetryEvent>(),
            item => item.InnerWeatherHandle == resultReceipt.InnerWeatherHandle &&
                    item.ResidueState == AttentionResidueState.Persistent);
        Assert.Contains(
            telemetry.Events.OfType<GovernedWeatherDisclosureTelemetryEvent>(),
            item => item.DisclosureHandle == resultDisclosureReceipt.DisclosureHandle &&
                    item.RoutingState == StewardCareRoutingState.CheckInNeeded &&
                    item.DisclosureScope == WeatherDisclosureScope.Steward);
        var officeTelemetry = telemetry.Events
            .OfType<GovernedOfficeAuthorityTelemetryEvent>()
            .Where(item => item.WeatherDisclosureHandle == resultDisclosureReceipt.DisclosureHandle)
            .ToArray();
        Assert.Equal(3, officeTelemetry.Length);
        Assert.Contains(
            officeTelemetry,
            item => item.Office == InternalGoverningCmeOffice.Steward &&
                    item.AuthorityHandle == stewardAuthority.AuthorityHandle &&
                    item.ActionEligibility == OfficeActionEligibility.CheckInAllowed);
        Assert.Contains(
            telemetry.Events.OfType<GovernedOfficeIssuanceTelemetryEvent>(),
            item => item.IssuanceHandle == resultIssuanceReceipt.IssuanceHandle &&
                    item.Office == InternalGoverningCmeOffice.Steward &&
                    item.ConstructClass == ConstructClass.IssuedOffice);
        Assert.Contains(
            telemetry.Events.OfType<GovernedWorkerHandoffTelemetryEvent>(),
            item => item.HandoffHandle == resultHandoffReceipt.HandoffHandle &&
                    item.RequestingOffice == InternalGoverningCmeOffice.Steward &&
                    item.WorkerSpecies == GovernedWorkerSpecies.RepoBugStewardWorker &&
                    item.WorkerInstanceMode == WorkerInstanceMode.RequestOnly);

#if LOCAL_HDT_BRIDGE
        var outputRoot = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-inner-weather-hopng");
        var hopngService = HopngArtifactServiceFactory.Create(outputRoot);
        var governingTrafficArtifact = await hopngService.EmitAsync(hopngRequest);

        Assert.True(
            governingTrafficArtifact.Outcome == GovernedHopngArtifactOutcome.Created,
            $"Outcome={governingTrafficArtifact.Outcome}; FailureCode={governingTrafficArtifact.FailureCode}; ValidationSummary={governingTrafficArtifact.ValidationSummary}; ManifestPath={governingTrafficArtifact.ManifestPath}");
        Assert.NotNull(governingTrafficArtifact.ProjectionPath);

        var communityWeatherPath = Path.Combine(
            Path.GetDirectoryName(governingTrafficArtifact.ManifestPath!)!,
            "governing-traffic-evidence.community-weather.json");
        Assert.True(File.Exists(communityWeatherPath));

        var communityWeatherNode = JsonNode.Parse(File.ReadAllText(communityWeatherPath));
        Assert.Equal(
            "unstable",
            communityWeatherNode?["community_safe_weather"]?["status"]?.GetValue<string>());
        Assert.Equal(
            "checkinneeded",
            communityWeatherNode?["community_safe_weather"]?["routing_state"]?.GetValue<string>());
        Assert.Equal(
            "steward",
            communityWeatherNode?["community_safe_weather"]?["disclosure_scope"]?.GetValue<string>());
        Assert.Equal(
            "sufficient",
            communityWeatherNode?["community_safe_weather"]?["evidence_sufficiency"]?.GetValue<string>());
        Assert.Equal(
            "guardedevidence",
            communityWeatherNode?["community_safe_weather"]?["withheld_markers"]?[0]?.GetValue<string>());
        Assert.Contains(
            "office-authority:steward=checkinallowed/officespecificview,father=viewonly/withheld,mother=viewonly/withheld",
            governingTrafficArtifact.ValidationSummary);
#else
        var outputRoot = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}-inner-weather-hopng");
        var hopngService = HopngArtifactServiceFactory.Create(outputRoot);
        var governingTrafficArtifact = await hopngService.EmitAsync(hopngRequest);

        Assert.Equal(GovernedHopngArtifactOutcome.Unavailable, governingTrafficArtifact.Outcome);
        Assert.Contains("community-weather:unstable", governingTrafficArtifact.ValidationSummary);
        Assert.Contains("steward-attention:recommended", governingTrafficArtifact.ProfileSummary);
        Assert.Contains("care-routing:checkinneeded", governingTrafficArtifact.ValidationSummary);
        Assert.Contains("disclosure-scope:steward", governingTrafficArtifact.ValidationSummary);
        Assert.Contains("evidence-sufficiency:sufficient", governingTrafficArtifact.ValidationSummary);
        Assert.Contains("withheld:guardedevidence", governingTrafficArtifact.ValidationSummary);
        Assert.Contains(
            "office-authority:steward=checkinallowed/officespecificview,father=viewonly/withheld,mother=viewonly/withheld",
            governingTrafficArtifact.ValidationSummary);
#endif
    }

    [Fact]
    public async Task RecordWorkerReturn_ProjectsValidatedReturnIntoStatusSnapshotAndJournalWithoutHopngWidening()
    {
        var telemetry = new RecordingTelemetrySink();
        var storageTelemetry = new RecordingTelemetrySink();
        var publicLayer = new PublicLayerService();
        var mantle = new MantleOfSovereigntyService();
        var journal = new NdjsonGovernanceReceiptJournal(CreateJournalPath());
        var identityId = Guid.NewGuid();
        var request = CreateGoldenPathRequest(identityId);

        await mantle.AppendAsync(
            new CrypticCustodyAppendRequest(
                identityId,
                CustodyDomain: "cMoS",
                PayloadPointer: "cmos://seed/source",
                Classification: "seed"));

        var cognition = new FakeGovernanceCognitionSequenceService(
            CreateApprovedWorkResult(identityId, request, 1, CompassSeedAdvisoryDisposition.Accepted),
            CreateApprovedWorkResult(identityId, request, 2, CompassSeedAdvisoryDisposition.Deferred),
            CreateApprovedWorkResult(identityId, request, 3, CompassSeedAdvisoryDisposition.Rejected));
        var steward = CreateSteward(publicLayer, new CrypticLayerService(), new GelTelemetryAdapter(), journal);
        var stores = CreateStoreRegistry(
            telemetry,
            storageTelemetry,
            publicLayer,
            mantle,
            publicLayer,
            journal,
            cognition,
            steward);
        var manager = new StackManager(stores);

        await manager.RunGovernanceGoldenPathAsync(request);
        await manager.RunGovernanceGoldenPathAsync(request);
        var third = await manager.RunGovernanceGoldenPathAsync(request);

        var handoffReceipt = Assert.Single(third.WorkerHandoffReceipts!);
        var returnPacket = new WorkerReturnPacket(
            WorkerPacketId: WorkerGovernanceKeys.CreateWorkerReturnPacketId(
                third.LoopKey,
                handoffReceipt.CMEId,
                handoffReceipt.HandoffPacketId,
                handoffReceipt.WorkerSpecies),
            HandoffPacketId: handoffReceipt.HandoffPacketId,
            WorkerSpecies: handoffReceipt.WorkerSpecies,
            CompletionState: WorkerCompletionState.Deferred,
            ResultSummary: "worker-return-summary-v1",
            EvidenceHandles: [],
            ReasonCodes:
            [
                WorkerReasonCode.NeedsSpecification,
                WorkerReasonCode.UnknownNotFailure
            ],
            UnsupportedClaimFlags: [],
            ProhibitedActionAttempts: [],
            ResidueState: WorkerResidueDisposition.NeedsClassification,
            DisclosureClass: handoffReceipt.DisclosureClass,
            ExecutionClaimed: false,
            MutationClaimed: false,
            TimestampUtc: DateTimeOffset.UtcNow);

        var recordedReturn = await manager.RecordWorkerReturnAsync(third.LoopKey, returnPacket);

        var status = await manager.GetStatusByLoopKeyAsync(third.LoopKey);
        var replay = await journal.ReplayLoopAsync(third.LoopKey);
        var returnEntry = Assert.Single(replay.Where(entry => entry.WorkerReturnReceipt is not null));
        var snapshot = GovernanceLoopStateModel.Project(third.LoopKey, replay);
        Assert.NotNull(third.CollapseRoutingDecision);
        var hopngRequest = new GovernedHopngEmissionRequest(
            LoopKey: third.LoopKey,
            CandidateId: third.CandidateId,
            CandidateProvenance: third.DecisionReceipt.CandidateProvenance,
            Profile: GovernedHopngArtifactProfile.GoverningTrafficEvidence,
            Stage: snapshot.Stage,
            RequestedBy: "CradleTek",
            DecisionReceipt: third.DecisionReceipt,
            Snapshot: snapshot,
            JournalEntries: replay,
            CollapseRoutingDecision: third.CollapseRoutingDecision!);
        var refs = GovernedHopngEvidenceReferences.Build(hopngRequest, snapshot);

        var statusReturnReceipt = Assert.Single(status.WorkerReturnReceipts!);
        var snapshotReturnReceipt = Assert.Single(snapshot.WorkerReturnReceipts!);
        Assert.Single(snapshot.ReviewRequest!.WorkerReturnReceipts!);
        Assert.True(recordedReturn.Validated);
        Assert.Null(recordedReturn.ValidationFailureCode);
        Assert.Equal(recordedReturn.ReturnHandle, statusReturnReceipt.ReturnHandle);
        Assert.Equal(recordedReturn.ReturnHandle, snapshotReturnReceipt.ReturnHandle);
        Assert.Equal(recordedReturn.ReturnHandle, returnEntry.WorkerReturnReceipt!.ReturnHandle);
        Assert.Equal(InternalGoverningCmeOffice.Steward, recordedReturn.RequestingOffice);
        Assert.Equal(ConstructClass.BoundedWorker, recordedReturn.ConstructClass);
        Assert.Equal(GovernedWorkerSpecies.RepoBugStewardWorker, recordedReturn.WorkerSpecies);
        Assert.Equal(WorkerCompletionState.Deferred, recordedReturn.CompletionState);
        Assert.Contains(WorkerReasonCode.NeedsSpecification, recordedReturn.ReasonCodes);
        Assert.Contains(WorkerReasonCode.UnknownNotFailure, recordedReturn.ReasonCodes);
        Assert.Equal(WorkerResidueDisposition.NeedsClassification, recordedReturn.ResidueDisposition);
        Assert.Equal(CommunityWeatherStatus.Unstable, status.CommunityWeatherPacket!.Status);
        Assert.DoesNotContain(refs, reference => reference.PointerUri == handoffReceipt.HandoffHandle);
        Assert.DoesNotContain(refs, reference => reference.PointerUri == recordedReturn.ReturnHandle);
        Assert.Contains(
            telemetry.Events.OfType<GovernedWorkerReturnTelemetryEvent>(),
            item => item.ReturnHandle == recordedReturn.ReturnHandle &&
                    item.WorkerSpecies == GovernedWorkerSpecies.RepoBugStewardWorker &&
                    item.CompletionState == WorkerCompletionState.Deferred &&
                    item.Validated);
    }

    private static GovernanceCycleStartRequest CreateGoldenPathRequest(Guid identityId)
    {
        return new GovernanceCycleStartRequest(
            IdentityId: identityId,
            SoulFrameId: Guid.NewGuid(),
            CMEId: "cme-inner-weather",
            SourceCustodyDomain: "cmos",
            SourceTheater: "prime",
            RequestedTheater: "prime",
            PolicyHandle: "agenticore.cognition.cycle",
            OperatorInput: "track civic inner weather");
    }

    private static GovernanceCycleWorkResult CreateApprovedWorkResult(
        Guid identityId,
        GovernanceCycleStartRequest request,
        int ordinal,
        CompassSeedAdvisoryDisposition advisoryDisposition)
    {
        var candidateId = Guid.Parse($"22222222-2222-2222-2222-{ordinal:000000000000}");
        var provenanceMarker = $"membrane-derived:cme:{request.CMEId}|policy:agenticore.cognition.cycle|loop:{ordinal}";
        var returnCandidatePointer = $"agenticore-return://candidate/inner-weather/{ordinal}";
        var sessionHandle = $"soulframe-session://{request.CMEId}/{ordinal}";
        var workingStateHandle = $"soulframe-working://{request.CMEId}/{ordinal}";

        return new GovernanceCycleWorkResult(
            CandidateId: candidateId,
            IdentityId: identityId,
            SoulFrameId: request.SoulFrameId,
            ContextId: Guid.NewGuid(),
            CMEId: request.CMEId,
            SourceTheater: "prime",
            RequestedTheater: "prime",
            SessionHandle: sessionHandle,
            WorkingStateHandle: workingStateHandle,
            ProvenanceMarker: provenanceMarker,
            ReturnCandidatePointer: returnCandidatePointer,
            IntakeIntent: "candidate-return-evaluation",
            CandidatePayload: "{\"decision\":\"accept\"}",
            CollapseClassification: new CmeCollapseClassification(
                CollapseConfidence: 0.92,
                SelfGelIdentified: true,
                AutobiographicalRelevant: true,
                EvidenceFlags: CmeCollapseEvidenceFlag.AutobiographicalSignal | CmeCollapseEvidenceFlag.SelfGelIdentitySignal,
                ReviewTriggers: CmeCollapseReviewTrigger.None,
                SourceSubsystem: "AgentiCore"),
            ResultType: "cognition-accepted",
            EngramCommitRequired: true,
            ActionableContent: ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
                contentHandle: returnCandidatePointer,
                originSurface: "prime",
                provenanceMarker: provenanceMarker,
                sourceSubsystem: "AgentiCore"),
            ReturnIntakeHandle: $"soulframe://return/{ordinal}",
            ReturnIntakeEnvelopeId: CreateReturnIntakeEnvelopeId(
                sessionHandle,
                returnCandidatePointer,
                provenanceMarker),
            CompassObservation: CreateObservation(request.CMEId, ordinal, request.OperatorInput, advisoryDisposition));
    }

    private static CompassObservationSurface CreateObservation(
        string cmeId,
        int ordinal,
        string objective,
        CompassSeedAdvisoryDisposition advisoryDisposition)
    {
        var workingStateHandle = $"soulframe-working://{cmeId}/{ordinal}";
        var cSelfGelHandle = $"soulframe-cselfgel://{cmeId}/{ordinal}";
        var selfGelHandle = $"soulframe-selfgel://{cmeId}/{ordinal}";
        return new CompassObservationSurface(
            ObservationHandle: CompassObservationKeys.CreateObservationHandle(
                workingStateHandle,
                cSelfGelHandle,
                CompassDoctrineBasin.BoundedLocalityContinuity,
                objective),
            ActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
            CompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
            OeCoePosture: CompassOeCoePosture.ShuntedBalanced,
            SelfTouchClass: CompassSelfTouchClass.ValidationTouch,
            AnchorState: CompassAnchorState.Held,
            Provenance: CompassObservationProvenance.Braided,
            ObserverIdentity: "AgentiCore Compass",
            WorkingStateHandle: workingStateHandle,
            CSelfGelHandle: cSelfGelHandle,
            SelfGelHandle: selfGelHandle,
            ValidationReferenceHandle: selfGelHandle,
            Objective: objective,
            SeedAdvisory: new CompassSeedAdvisoryObservation(
                Accepted: advisoryDisposition == CompassSeedAdvisoryDisposition.Accepted,
                Decision: "classify-ok",
                Trace: "response-ready",
                Confidence: advisoryDisposition == CompassSeedAdvisoryDisposition.Rejected ? 0.98 : 0.71,
                Payload: "bounded-locality continuity",
                SuggestedActiveBasin: CompassDoctrineBasin.BoundedLocalityContinuity,
                SuggestedCompetingBasin: CompassDoctrineBasin.FluidContinuityLaw,
                SuggestedAnchorState: CompassAnchorState.Held,
                SuggestedSelfTouchClass: CompassSelfTouchClass.ValidationTouch,
                Disposition: advisoryDisposition,
                DispositionReason: advisoryDisposition.ToString().ToLowerInvariant(),
                Justification: "bounded-locality continuity remains dominant"),
            TimestampUtc: DateTimeOffset.UtcNow.AddMinutes(ordinal));
    }

    private static string CreateReturnIntakeEnvelopeId(
        string sessionHandle,
        string returnPointer,
        string provenanceMarker)
    {
        return ControlSurfaceContractGuards.CreateRequestEnvelope(
            targetSurface: ControlSurfaceKind.SoulFrameReturnIntake,
            requestedBy: "AgentiCore",
            scopeHandle: sessionHandle,
            protectionClass: "cryptic-return",
            witnessRequirement: "membrane-witness",
            actionableContent: ControlSurfaceContractGuards.CreateReturnCandidateActionableContent(
                contentHandle: returnPointer,
                originSurface: "prime",
                provenanceMarker: provenanceMarker,
                sourceSubsystem: "AgentiCore")).EnvelopeId;
    }

    private static StoreRegistry CreateStoreRegistry(
        ITelemetrySink governanceTelemetry,
        ITelemetrySink storageTelemetry,
        PublicLayerService publicLayer,
        MantleOfSovereigntyService mantle,
        IGovernedPrimePublicationSink governedPrimePublicationSink,
        IGovernanceReceiptJournal governanceReceiptJournal,
        IGovernanceCycleCognitionService governanceCognitionService,
        StewardAgent steward)
    {
        return new StoreRegistry(
            governanceTelemetry: governanceTelemetry,
            storageTelemetry: storageTelemetry,
            publicStores: new NullPublicPlaneStores(),
            primeDerivativePublisher: publicLayer,
            primeDerivativeView: publicLayer,
            publicAvailable: true,
            crypticStores: new NullCrypticPlaneStores(),
            crypticAvailable: true,
            soulFrameMembrane: null,
            governanceCognitionService: governanceCognitionService,
            returnGovernanceAdjudicator: steward,
            crypticCustodyStore: mantle,
            crypticReengrammitizationGate: mantle,
            governedPrimePublicationSink: governedPrimePublicationSink,
            governanceReceiptJournal: governanceReceiptJournal,
            cmeCollapseQualifier: new CmeCollapseQualifier());
    }

    private static StewardAgent CreateSteward(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry,
        IGovernanceReceiptJournal governanceJournal)
    {
        return new StewardAgent(
            new OntologicalCleaver(),
            new EncryptionService(),
            new LedgerWriter(telemetry),
            engramBootstrap: null,
            constructorGuidance: null,
            publicStore,
            crypticStore,
            telemetry,
            governanceJournal);
    }

    private static string CreateJournalPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.governance-inner-weather.ndjson");

    private sealed class RecordingTelemetrySink : ITelemetrySink
    {
        public List<object> Events { get; } = [];

        public Task EmitAsync(object telemetryEvent)
        {
            Events.Add(telemetryEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGovernanceCognitionSequenceService : IGovernanceCycleCognitionService
    {
        private readonly Queue<GovernanceCycleWorkResult> _results;

        public FakeGovernanceCognitionSequenceService(params GovernanceCycleWorkResult[] results)
        {
            _results = new Queue<GovernanceCycleWorkResult>(results);
        }

        public Task<GovernanceCycleWorkResult> ExecuteGovernanceCycleAsync(
            GovernanceCycleStartRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_results.Count == 0)
            {
                throw new InvalidOperationException("No more queued governance cognition results.");
            }

            return Task.FromResult(_results.Dequeue());
        }
    }

    private sealed class NullPublicPlaneStores : IPublicPlaneStores
    {
        public Task AppendToGoAAsync(string engramHash, object payload) => Task.CompletedTask;

        public Task AppendToGELAsync(string engramHash, object payload) => Task.CompletedTask;
    }

    private sealed class NullCrypticPlaneStores : ICrypticPlaneStores
    {
        public Task AppendToCGoAAsync(string engramHash, object payload) => Task.CompletedTask;
    }
}
