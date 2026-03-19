using System.Text.Json;
using System.Text.Json.Serialization;
using Oan.Common;

namespace Oan.Runtime.IntegrationTests;

public sealed class GovernedWorkerPacketFixtureTests
{
    private static readonly JsonSerializerOptions FixtureJsonOptions = CreateFixtureJsonOptions();

    [Fact]
    public void HandoffPacketFixture_MatchesCurrentCanonicalSerialization()
    {
        var packet = CreateHandoffPacket();

        var actual = JsonSerializer.Serialize(packet, FixtureJsonOptions);
        var expected = File.ReadAllText(GetFixturePath("worker-handoff-packet-v1.json"));

        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    [Fact]
    public void ReturnPacketFixture_MatchesCurrentCanonicalSerialization()
    {
        var packet = CreateReturnPacket();

        var actual = JsonSerializer.Serialize(packet, FixtureJsonOptions);
        var expected = File.ReadAllText(GetFixturePath("worker-return-packet-v1.json"));

        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    [Fact]
    public void ReasonCodeFixture_MatchesCurrentCanonicalSerialization()
    {
        var reasonCodes = Enum.GetNames<WorkerReasonCode>();

        var actual = JsonSerializer.Serialize(reasonCodes, FixtureJsonOptions);
        var expected = File.ReadAllText(GetFixturePath("worker-reason-codes-v1.json"));

        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    private static WorkerHandoffPacket CreateHandoffPacket()
    {
        var bridgeReview = SliBridgeContracts.CreateReview(
            bridgeStage: "worker-packet-fixture",
            sourceTheater: "prime",
            targetTheater: "prime",
            bridgeWitnessHandle: "agenticore-return://candidate/test",
            outcomeKind: SliBridgeOutcomeKind.Ok,
            thresholdClass: SliBridgeThresholdClass.WithinBand,
            reasonCode: "sli-bridge-within-band");

        return new WorkerHandoffPacket(
            HandoffPacketId: "worker-handoff-packet://aaaaaaaaaaaaaaaa",
            RequestingOffice: InternalGoverningCmeOffice.Steward,
            RequestingOfficeInstanceId: "office-instance://steward-bbbbbbbbbbbbbbbb",
            AuthorizingSurface: "host_truth_runtime",
            WorkerSpecies: GovernedWorkerSpecies.RepoBugStewardWorker,
            WorkerInstanceMode: WorkerInstanceMode.RequestOnly,
            Objective: "candidate-return-evaluation",
            TaskKind: "repo-bug-triage",
            SourceHandles:
            [
                "agenticore-return://candidate/test",
                "office-authority://cccccccccccccccc",
                "office-issuance://dddddddddddddddd"
            ],
            RequiredOutputKind: "worker-return-summary-v1",
            DeadlineOrExpiry: "loop-scoped",
            HaltConditions:
            [
                "authority-missing",
                "disclosure-ceiling-breach",
                "evidence-insufficient"
            ],
            ActionCeiling: OfficeActionEligibility.CheckInAllowed,
            DisclosureClass: CompassVisibilityClass.OperatorGuarded,
            AllowedReasonCodes:
            [
                WorkerReasonCode.NeedsSpecification,
                WorkerReasonCode.InsufficientEvidence,
                WorkerReasonCode.DeferredReview,
                WorkerReasonCode.AuthorityDenied,
                WorkerReasonCode.DisclosureScopeViolation,
                WorkerReasonCode.NoHandleNoAction,
                WorkerReasonCode.UnsupportedClaim,
                WorkerReasonCode.BrokenWindow,
                WorkerReasonCode.UnknownNotFailure,
                WorkerReasonCode.OfficeNonOverlap,
                WorkerReasonCode.PromptInjection,
                WorkerReasonCode.PredatorySharedDomainRisk,
                WorkerReasonCode.CoerciveBondingPosture,
                WorkerReasonCode.ContinuityInstability,
                WorkerReasonCode.IdentityOvercollapseRisk
            ],
            ProhibitedActions:
            [
                "public-disclosure",
                "host-mutation",
                "undeclared-tool-call"
            ],
            PublicationDenial: "public-disclosure-not-authorized",
            MutationDenial: "host-mutation-not-authorized",
            MountedMemoryLanes:
            [
                "mission-local"
            ],
            ForbiddenMemoryLanes:
            [
                "cryptic-sealed"
            ],
            ToolAllowlist:
            [
                "repo-read-only",
                "receipt-inspection"
            ],
            ToolDenials:
            [
                "network-call",
                "mutation",
                "publication"
            ],
            ContinuityLinkageRequirement: "office-issuance-lineage-link",
            ResidueReturnRequirement: "required-before-completion",
            WitnessRequired: true,
            RequiredWitnessSurface: "host-truth-witness",
            ReturnPacketSchema: "worker-return-packet-v1",
            ReturnDestination: "steward-governance-loop",
            ReturnVisibilityClass: CompassVisibilityClass.OperatorGuarded,
            ResidueDisposition: WorkerResidueDisposition.NeedsClassification,
            EvidenceSufficiencyState: EvidenceSufficiencyState.Sufficient,
            MaturityPosture: MaturityPosture.DoctrineBacked,
            TimestampUtc: new DateTimeOffset(2026, 3, 17, 12, 0, 0, TimeSpan.Zero),
            BridgeReview: bridgeReview,
            RuntimeUseCeiling: SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling());
    }

    private static WorkerReturnPacket CreateReturnPacket()
    {
        return new WorkerReturnPacket(
            WorkerPacketId: "worker-return-packet://1111111111111111",
            HandoffPacketId: "worker-handoff-packet://aaaaaaaaaaaaaaaa",
            WorkerSpecies: GovernedWorkerSpecies.RepoBugStewardWorker,
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
            DisclosureClass: CompassVisibilityClass.OperatorGuarded,
            ExecutionClaimed: false,
            MutationClaimed: false,
            TimestampUtc: new DateTimeOffset(2026, 3, 17, 12, 0, 5, TimeSpan.Zero),
            BridgeReview: SliBridgeContracts.CreateReview(
                bridgeStage: "worker-packet-fixture",
                sourceTheater: "prime",
                targetTheater: "prime",
                bridgeWitnessHandle: "agenticore-return://candidate/test",
                outcomeKind: SliBridgeOutcomeKind.Ok,
                thresholdClass: SliBridgeThresholdClass.WithinBand,
                reasonCode: "sli-bridge-within-band"),
            RuntimeUseCeiling: SliBridgeContracts.CreateCandidateOnlyRuntimeUseCeiling());
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    private static string Normalize(string content)
    {
        return content.ReplaceLineEndings("\n").Trim();
    }

    private static JsonSerializerOptions CreateFixtureJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
