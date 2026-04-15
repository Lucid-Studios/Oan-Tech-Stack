using San.Common;
using SLI.Lisp;

namespace SLI.Engine;

public sealed record CrypticDuplexBraidEvaluation(
    DuplexFieldPacket EmittedPacket,
    RtmeDuplexProjectionReceipt ProjectionReceipt,
    RtmeDuplexBraidSessionSnapshot BraidSnapshot,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PrimeClosureIssued,
    string OutcomeCode,
    string GovernanceTrace);

public interface ICrypticDuplexBraidEvaluator
{
    CrypticDuplexBraidEvaluation EvaluateBraid(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string receiptHandle);
}

public sealed class CrypticDuplexBraidEvaluator : ICrypticDuplexBraidEvaluator
{
    private readonly IRtmeDuplexBraidEngine _braidEngine;

    public CrypticDuplexBraidEvaluator()
        : this(new RtmeDuplexBraidEngine())
    {
    }

    public CrypticDuplexBraidEvaluator(IRtmeDuplexBraidEngine braidEngine)
    {
        _braidEngine = braidEngine ?? throw new ArgumentNullException(nameof(braidEngine));
    }

    public CrypticDuplexBraidEvaluation EvaluateBraid(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectedLineInput> lines,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var result = _braidEngine.AdvanceBraid(packet, lines, receiptHandle);

        return new CrypticDuplexBraidEvaluation(
            EmittedPacket: result.EmittedPacket,
            ProjectionReceipt: result.ProjectionReceipt,
            BraidSnapshot: result.BraidSnapshot,
            AdvisoryClosureEligibility: result.BraidSnapshot.AdvisoryClosureEligibility,
            PrimeClosureIssued: result.BraidSnapshot.PrimeClosureIssued,
            OutcomeCode: result.BraidSnapshot.OutcomeCode,
            GovernanceTrace: "rtme-duplex-braid-projected-field-only");
    }
}
