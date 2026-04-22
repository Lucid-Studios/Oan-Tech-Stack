using San.Common;
using SLI.Lisp;

namespace SLI.Engine;

public sealed record CrypticDuplexPostureEvaluation(
    DuplexFieldPacket EmittedPacket,
    PrimeClosureEligibilityKind AdvisoryClosureEligibility,
    bool PrimeClosureIssued,
    string OutcomeCode,
    string GovernanceTrace,
    RtmeDuplexProjectionReceipt Receipt);

public interface ICrypticDuplexPostureEvaluator
{
    CrypticDuplexPostureEvaluation Evaluate(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectionContribution> contributions,
        string receiptHandle);
}

public sealed class CrypticDuplexPostureEvaluator : ICrypticDuplexPostureEvaluator
{
    private readonly IRtmeDuplexPostureEngine _postureEngine;

    public CrypticDuplexPostureEvaluator()
        : this(new RtmeDuplexPostureEngine())
    {
    }

    public CrypticDuplexPostureEvaluator(IRtmeDuplexPostureEngine postureEngine)
    {
        _postureEngine = postureEngine ?? throw new ArgumentNullException(nameof(postureEngine));
    }

    public CrypticDuplexPostureEvaluation Evaluate(
        DuplexFieldPacket packet,
        IReadOnlyList<RtmeProjectionContribution> contributions,
        string receiptHandle)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var result = _postureEngine.Advance(packet, contributions, receiptHandle);

        return new CrypticDuplexPostureEvaluation(
            EmittedPacket: result.EmittedPacket,
            AdvisoryClosureEligibility: result.Receipt.AdvisoryClosureEligibility,
            PrimeClosureIssued: result.Receipt.PrimeClosureIssued,
            OutcomeCode: result.Receipt.OutcomeCode,
            GovernanceTrace: "rtme-duplex-posture-projected-field-only",
            Receipt: result.Receipt);
    }
}
