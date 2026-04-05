// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Brokers.Tools;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace STX.Agent.Test.Tools
{
    public class ValidateJsonTool : ITool
    {
        public string Name => "validate_json";

        public string Description =>
            "Validates if a JSON string has valid structure with 'id' and 'name' fields";

        public Dictionary<string, string> Arguments => new()
        {
            { "json", "string - The JSON string to validate" }
        };

        public ValueTask<string> ExecuteAsync(Dictionary<string, object> arguments)
        {
            if (!arguments.TryGetValue("json", out object? jsonValue))
            {
                return ValueTask.FromResult(JsonSerializer.Serialize(new
                {
                    valid = false,
                    error = "Missing 'json' argument"
                }));
            }

            string jsonString = jsonValue.ToString() ?? string.Empty;

            try
            {
                JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;

                bool hasId = root.TryGetProperty("id", out _);
                bool hasName = root.TryGetProperty("name", out _);

                var result = new
                {
                    valid = hasId && hasName,
                    hasId,
                    hasName,
                    message = hasId && hasName
                        ? "JSON is valid with id and name"
                        : $"JSON missing: {(hasId ? "" : "id")}{(!hasId && !hasName ? ", " : "")}{(hasName ? "" : "name")}"
                };

                return ValueTask.FromResult(JsonSerializer.Serialize(result));
            }
            catch (JsonException ex)
            {
                return ValueTask.FromResult(JsonSerializer.Serialize(new
                {
                    valid = false,
                    error = $"Invalid JSON: {ex.Message}"
                }));
            }
        }
    }
}
