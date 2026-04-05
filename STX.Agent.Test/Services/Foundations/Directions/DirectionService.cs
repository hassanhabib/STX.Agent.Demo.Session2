// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Brokers.Tools;
using STX.Agent.Test.Models;
using STX.Agent.Test.Services.Foundations.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Foundations.Directions
{
    public class DirectionService : IDirectionService
    {
        private readonly IToolBroker toolBroker;
        private readonly IDataService dataService;

        public DirectionService(
            IToolBroker toolBroker,
            IDataService dataService)
        {
            this.toolBroker = toolBroker;
            this.dataService = dataService;
        }

        public async ValueTask<ValidationResult?> ExecuteActionAsync(
            DecisionOutput decision,
            AgentState state)
        {
            switch (decision.Action)
            {
                case "return":
                    await this.dataService.SetFinalOutputAsync(decision.Payload.ToString());
                    return null;

                case "self":
                    await this.dataService.AddEntryAsync(new DataEntry
                    {
                        Type = "self",
                        Content = decision.Payload.ToString()
                    });
                    return null;

                case "tool":
                    return await ExecuteToolAsync(decision, state);

                default:
                    throw new InvalidOperationException($"Unknown action: {decision.Action}");
            }
        }

        private async ValueTask<ValidationResult?> ExecuteToolAsync(
            DecisionOutput decision,
            AgentState state)
        {
            if (string.IsNullOrEmpty(decision.Tool))
            {
                throw new InvalidOperationException("Tool action requires a tool name");
            }

            Dictionary<string, object> arguments = decision.Arguments ?? new Dictionary<string, object>();

            if (!arguments.ContainsKey("json") && state.LastPayload != null)
            {
                arguments["json"] = state.LastPayload;
            }

            string result = await this.toolBroker.ExecuteToolAsync(decision.Tool, arguments);

            await this.dataService.AddEntryAsync(new DataEntry
            {
                Type = "tool_response",
                Content = result
            });

            return ParseValidationResult(result);
        }

        private ValidationResult ParseValidationResult(string toolResponse)
        {
            try
            {
                JsonDocument doc = JsonDocument.Parse(toolResponse);
                JsonElement root = doc.RootElement;

                bool isValid = root.TryGetProperty("valid", out JsonElement validProp) && 
                    validProp.GetBoolean();

                string? error = root.TryGetProperty("error", out JsonElement errorProp) 
                    ? errorProp.GetString() 
                    : (root.TryGetProperty("message", out JsonElement msgProp) ? msgProp.GetString() : null);

                return new ValidationResult
                {
                    IsValid = isValid,
                    Error = error
                };
            }
            catch
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Error = "Failed to parse validation result"
                };
            }
        }
    }
}
