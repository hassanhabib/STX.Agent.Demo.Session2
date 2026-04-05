// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STX.Agent.Test.Models
{
    public class DecisionOutput
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }

        [JsonPropertyName("tool")]
        public string? Tool { get; set; }

        [JsonPropertyName("arguments")]
        public Dictionary<string, object>? Arguments { get; set; }
    }
}
