using System.Text.Json.Serialization;
using Oan.Common;

namespace CradleTek.CognitionHost.Models;

public sealed class GoldenCodeCompassProjection
{
    [JsonPropertyName("active_basin")]
    public required CompassDoctrineBasin ActiveBasin { get; init; }

    [JsonPropertyName("competing_basin")]
    public required CompassDoctrineBasin CompetingBasin { get; init; }

    [JsonPropertyName("anchor_state")]
    public required CompassAnchorState AnchorState { get; init; }

    [JsonPropertyName("self_touch_class")]
    public required CompassSelfTouchClass SelfTouchClass { get; init; }

    [JsonPropertyName("oe_coe_posture")]
    public required CompassOeCoePosture OeCoePosture { get; init; }

    public static GoldenCodeCompassProjection FromCandidateReceipt(ZedThetaCandidateReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);

        return new GoldenCodeCompassProjection
        {
            ActiveBasin = receipt.ActiveBasin,
            CompetingBasin = receipt.CompetingBasin,
            AnchorState = receipt.AnchorState,
            SelfTouchClass = receipt.SelfTouchClass,
            OeCoePosture = receipt.OeCoePosture
        };
    }
}
