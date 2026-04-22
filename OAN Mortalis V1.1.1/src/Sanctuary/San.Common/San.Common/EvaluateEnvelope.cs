using System.Text.Json.Serialization;

namespace San.Common
{
    /// <summary>
    /// Host transport surface for evaluation results.
    /// </summary>
    public sealed class EvaluateEnvelope
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = string.Empty;

        [JsonPropertyName("theater_id")]
        public string TheaterId { get; set; } = string.Empty;

        [JsonPropertyName("decision")]
        public string Decision { get; set; } = string.Empty;

        [JsonPropertyName("accepted")]
        public bool? Accepted { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        [JsonPropertyName("return_surface")]
        public GovernedSeedReturnSurfaceContext? ReturnSurfaceContext { get; set; }

        [JsonPropertyName("outbound_object")]
        public GovernedSeedOutboundObjectContext? OutboundObjectContext { get; set; }

        [JsonPropertyName("outbound_lane")]
        public GovernedSeedOutboundLaneContext? OutboundLaneContext { get; set; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }

        [JsonPropertyName("governance_state")]
        public string? GovernanceState { get; set; }

        [JsonPropertyName("governance_trace")]
        public string? GovernanceTrace { get; set; }

        [JsonPropertyName("duplex_ptr")]
        public string? DuplexResponseHash { get; set; }
    }
}
