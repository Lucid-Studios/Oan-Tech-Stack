namespace San.Common;

public enum PrivateDomainServiceWitnessKind
{
    RegionalPosture = 0,
    LocalServiceWitness = 1,
    PrivateDomainWitness = 2,
    WitnessWithheld = 3
}

public enum PrivateDomainServiceOperationDisposition
{
    Attested = 0,
    Deferred = 1,
    Refused = 2
}

public enum PrivateDomainServiceAxisKind
{
    Actor = 0,
    Action = 1,
    Instrument = 2,
    Method = 3,
    Locality = 4,
    StandingContext = 5,
    ContinuityBurden = 6
}

public sealed record PrivateDomainServiceWitnessRequest(
    string RequestHandle,
    DomainAdmissionRecord DomainAdmissionRecord,
    string ActorHandle,
    string ActionHandle,
    string InstrumentHandle,
    string MethodHandle,
    string LocalityHandle,
    string StandingContextHandle,
    IReadOnlyList<string> ContinuityBurdenHandles,
    IReadOnlyList<string> ResultReceiptHandles,
    bool ActionExecutionRequested,
    bool CradleLocalGovernanceEnactmentRequested,
    DateTimeOffset TimestampUtc);

public sealed record PrivateDomainServiceWitnessReceipt(
    string ReceiptHandle,
    string RequestHandle,
    string DomainAdmissionRecordHandle,
    PrivateDomainServiceWitnessKind WitnessKind,
    PrivateDomainServiceOperationDisposition Disposition,
    string ActorHandle,
    string ActionHandle,
    string InstrumentHandle,
    string MethodHandle,
    string LocalityHandle,
    string StandingContextHandle,
    IReadOnlyList<string> ContinuityBurdenHandles,
    IReadOnlyList<string> ResultReceiptHandles,
    bool ActorAxisPresent,
    bool ActionAxisPresent,
    bool InstrumentAxisPresent,
    bool MethodAxisPresent,
    bool LocalityAxisPresent,
    bool StandingContextAxisPresent,
    bool ContinuityBurdenAxisPresent,
    bool RelationAttested,
    bool ActionExecutionWithheld,
    bool CradleLocalGovernanceEnactmentWithheld,
    bool CustodialMemoryOnly,
    bool CandidateOnly,
    IReadOnlyList<string> ConstraintCodes,
    string ReasonCode,
    string LawfulBasis,
    DateTimeOffset TimestampUtc);

public static class PrivateDomainServiceWitnessEvaluator
{
    public static PrivateDomainServiceWitnessReceipt Evaluate(
        PrivateDomainServiceWitnessRequest request,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DomainAdmissionRecord);

        if (string.IsNullOrWhiteSpace(receiptHandle))
        {
            throw new ArgumentException("Receipt handle must be provided.", nameof(receiptHandle));
        }

        if (string.IsNullOrWhiteSpace(request.RequestHandle))
        {
            throw new ArgumentException("Request handle must be provided.", nameof(request));
        }

        var actorPresent = HasToken(request.ActorHandle);
        var actionPresent = HasToken(request.ActionHandle);
        var instrumentPresent = HasToken(request.InstrumentHandle);
        var methodPresent = HasToken(request.MethodHandle);
        var localityPresent = HasToken(request.LocalityHandle);
        var standingPresent = HasToken(request.StandingContextHandle);
        var continuityBurdenHandles = NormalizeTokens(request.ContinuityBurdenHandles);
        var resultReceiptHandles = NormalizeTokens(request.ResultReceiptHandles);
        var continuityBurdenPresent = continuityBurdenHandles.Count > 0;
        var acceptedDomain = request.DomainAdmissionRecord.Decision == DomainRoleAdmissionDecisionKind.Accept;
        var localAxesPresent = actorPresent &&
                               actionPresent &&
                               instrumentPresent &&
                               methodPresent &&
                               localityPresent;
        var privateAxesPresent = localAxesPresent &&
                                 standingPresent &&
                                 continuityBurdenPresent;
        var actionExecutionRefused = request.ActionExecutionRequested;
        var governanceEnactmentRefused = request.CradleLocalGovernanceEnactmentRequested;

        var witnessKind = DetermineWitnessKind(
            acceptedDomain,
            localAxesPresent,
            privateAxesPresent,
            actionExecutionRefused,
            governanceEnactmentRefused);
        var disposition = DetermineDisposition(
            acceptedDomain,
            privateAxesPresent,
            actionExecutionRefused,
            governanceEnactmentRefused);
        var relationAttested = acceptedDomain &&
                               privateAxesPresent &&
                               !actionExecutionRefused &&
                               !governanceEnactmentRefused;

        return new PrivateDomainServiceWitnessReceipt(
            ReceiptHandle: receiptHandle.Trim(),
            RequestHandle: request.RequestHandle.Trim(),
            DomainAdmissionRecordHandle: request.DomainAdmissionRecord.RecordHandle,
            WitnessKind: witnessKind,
            Disposition: disposition,
            ActorHandle: NormalizeHandle(request.ActorHandle),
            ActionHandle: NormalizeHandle(request.ActionHandle),
            InstrumentHandle: NormalizeHandle(request.InstrumentHandle),
            MethodHandle: NormalizeHandle(request.MethodHandle),
            LocalityHandle: NormalizeHandle(request.LocalityHandle),
            StandingContextHandle: NormalizeHandle(request.StandingContextHandle),
            ContinuityBurdenHandles: continuityBurdenHandles,
            ResultReceiptHandles: resultReceiptHandles,
            ActorAxisPresent: actorPresent,
            ActionAxisPresent: actionPresent,
            InstrumentAxisPresent: instrumentPresent,
            MethodAxisPresent: methodPresent,
            LocalityAxisPresent: localityPresent,
            StandingContextAxisPresent: standingPresent,
            ContinuityBurdenAxisPresent: continuityBurdenPresent,
            RelationAttested: relationAttested,
            ActionExecutionWithheld: true,
            CradleLocalGovernanceEnactmentWithheld: true,
            CustodialMemoryOnly: true,
            CandidateOnly: true,
            ConstraintCodes: DetermineConstraintCodes(
                acceptedDomain,
                actorPresent,
                actionPresent,
                instrumentPresent,
                methodPresent,
                localityPresent,
                standingPresent,
                continuityBurdenPresent,
                relationAttested,
                actionExecutionRefused,
                governanceEnactmentRefused),
            ReasonCode: DetermineReasonCode(
                acceptedDomain,
                localAxesPresent,
                privateAxesPresent,
                actionExecutionRefused,
                governanceEnactmentRefused),
            LawfulBasis: DetermineLawfulBasis(witnessKind, disposition),
            TimestampUtc: MaxTimestamp(request.TimestampUtc, request.DomainAdmissionRecord.TimestampUtc));
    }

    private static PrivateDomainServiceWitnessKind DetermineWitnessKind(
        bool acceptedDomain,
        bool localAxesPresent,
        bool privateAxesPresent,
        bool actionExecutionRefused,
        bool governanceEnactmentRefused)
    {
        if (!acceptedDomain || actionExecutionRefused || governanceEnactmentRefused)
        {
            return PrivateDomainServiceWitnessKind.WitnessWithheld;
        }

        if (privateAxesPresent)
        {
            return PrivateDomainServiceWitnessKind.PrivateDomainWitness;
        }

        return localAxesPresent
            ? PrivateDomainServiceWitnessKind.LocalServiceWitness
            : PrivateDomainServiceWitnessKind.RegionalPosture;
    }

    private static PrivateDomainServiceOperationDisposition DetermineDisposition(
        bool acceptedDomain,
        bool privateAxesPresent,
        bool actionExecutionRefused,
        bool governanceEnactmentRefused)
    {
        if (actionExecutionRefused || governanceEnactmentRefused)
        {
            return PrivateDomainServiceOperationDisposition.Refused;
        }

        if (!acceptedDomain || !privateAxesPresent)
        {
            return PrivateDomainServiceOperationDisposition.Deferred;
        }

        return PrivateDomainServiceOperationDisposition.Attested;
    }

    private static IReadOnlyList<string> DetermineConstraintCodes(
        bool acceptedDomain,
        bool actorPresent,
        bool actionPresent,
        bool instrumentPresent,
        bool methodPresent,
        bool localityPresent,
        bool standingPresent,
        bool continuityBurdenPresent,
        bool relationAttested,
        bool actionExecutionRefused,
        bool governanceEnactmentRefused)
    {
        var constraints = new List<string>
        {
            "private-domain-service-witness-requires-accepted-domain-admission",
            "private-domain-service-structured-accountability-grammar",
            "private-domain-service-attests-relation-not-action-execution",
            "private-domain-service-custodial-memory-only",
            "private-domain-service-cradle-governance-enactment-withheld"
        };

        if (acceptedDomain)
        {
            constraints.Add("private-domain-service-domain-admission-accepted");
        }
        else
        {
            constraints.Add("private-domain-service-domain-admission-not-accepted");
        }

        AddMissingAxisConstraint(constraints, actorPresent, "actor");
        AddMissingAxisConstraint(constraints, actionPresent, "action");
        AddMissingAxisConstraint(constraints, instrumentPresent, "instrument");
        AddMissingAxisConstraint(constraints, methodPresent, "method");
        AddMissingAxisConstraint(constraints, localityPresent, "locality");
        AddMissingAxisConstraint(constraints, standingPresent, "standing-context");
        AddMissingAxisConstraint(constraints, continuityBurdenPresent, "continuity-burden");

        if (actionExecutionRefused)
        {
            constraints.Add("private-domain-service-action-execution-refused");
        }

        if (governanceEnactmentRefused)
        {
            constraints.Add("private-domain-service-cradle-local-governance-enactment-refused");
        }

        constraints.Add(relationAttested
            ? "private-domain-service-private-relation-attested"
            : "private-domain-service-relation-not-attested");

        return constraints;
    }

    private static string DetermineReasonCode(
        bool acceptedDomain,
        bool localAxesPresent,
        bool privateAxesPresent,
        bool actionExecutionRefused,
        bool governanceEnactmentRefused)
    {
        if (actionExecutionRefused)
        {
            return "private-domain-service-action-execution-refused";
        }

        if (governanceEnactmentRefused)
        {
            return "private-domain-service-cradle-local-governance-enactment-refused";
        }

        if (!acceptedDomain)
        {
            return "private-domain-service-domain-admission-not-accepted";
        }

        if (privateAxesPresent)
        {
            return "private-domain-service-private-relation-attested";
        }

        return localAxesPresent
            ? "private-domain-service-private-axis-incomplete"
            : "private-domain-service-accountability-axis-incomplete";
    }

    private static string DetermineLawfulBasis(
        PrivateDomainServiceWitnessKind witnessKind,
        PrivateDomainServiceOperationDisposition disposition)
    {
        if (witnessKind == PrivateDomainServiceWitnessKind.PrivateDomainWitness &&
            disposition == PrivateDomainServiceOperationDisposition.Attested)
        {
            return "private-domain service witness may attest relation only when accepted domain admission and all accountability axes are present, while action execution and cradle-local governance enactment remain withheld.";
        }

        if (disposition == PrivateDomainServiceOperationDisposition.Refused)
        {
            return "private-domain service witness must refuse requests that turn custodial provenance into action execution or cradle-local governance enactment.";
        }

        return "private-domain service witness must defer when accepted domain admission or accountability axes are incomplete, preserving visible posture without pretending private relation is fully attested.";
    }

    private static void AddMissingAxisConstraint(
        ICollection<string> constraints,
        bool axisPresent,
        string axisName)
    {
        if (!axisPresent)
        {
            constraints.Add($"private-domain-service-{axisName}-axis-missing");
        }
    }

    private static IReadOnlyList<string> NormalizeTokens(
        IReadOnlyList<string>? tokens)
    {
        return (tokens ?? Array.Empty<string>())
            .Where(static token => !string.IsNullOrWhiteSpace(token))
            .Select(static token => token.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static token => token, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeHandle(string? handle)
        => string.IsNullOrWhiteSpace(handle) ? string.Empty : handle.Trim();

    private static bool HasToken(string? value)
        => !string.IsNullOrWhiteSpace(value);

    private static DateTimeOffset MaxTimestamp(
        DateTimeOffset first,
        DateTimeOffset second) =>
        first >= second ? first : second;
}
