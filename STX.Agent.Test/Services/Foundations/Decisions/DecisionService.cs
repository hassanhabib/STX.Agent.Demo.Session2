// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Brokers.LLM;
using STX.Agent.Test.Brokers.Logging;
using STX.Agent.Test.Brokers.Tools;
using STX.Agent.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Foundations.Decisions
{
    public class DecisionService : IDecisionService
    {
        private readonly ILLMBroker llmBroker;
        private readonly IToolBroker toolBroker;
        private readonly ILoggingBroker loggingBroker;

        public DecisionService(
            ILLMBroker llmBroker,
            IToolBroker toolBroker,
            ILoggingBroker loggingBroker)
        {
            this.llmBroker = llmBroker;
            this.toolBroker = toolBroker;
            this.loggingBroker = loggingBroker;
        }

        public async ValueTask<(string RawOutput, DecisionOutput Decision)> MakeDecisionAsync(
            AgentContext context,
            AgentState state)
        {
            Configuration config = ExtractConfiguration(context);
            string prompt = BuildPrompt(context, state);

            const int maxJsonRetries = 2;
            string? lastError = null;

            for (int attempt = 0; attempt < maxJsonRetries; attempt++)
            {
                string finalPrompt = prompt;

                if (attempt > 0 && lastError != null)
                {
                    finalPrompt = prompt + 
                        $"\n\n⚠️ CRITICAL: Your previous response was INVALID JSON.\n" +
                        $"Error: {lastError}\n\n" +
                        $"You MUST return ONLY valid JSON. No markdown, no explanations, no code blocks.\n" +
                        $"Start your response with just the opening brace.";
                }

                // Use assistant prefix to constrain output to JSON only
                string rawOutput = await this.llmBroker.CallWithPrefixAsync(
                    config.Endpoint,
                    config.ApiKey,
                    config.Model,
                    finalPrompt,
                    "{");  // Force LLM to continue from opening brace

                try
                {
                    DecisionOutput? decision = JsonSerializer.Deserialize<DecisionOutput>(rawOutput);

                    if (decision == null)
                    {
                        lastError = "Deserialization returned null";
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(decision.Action))
                    {
                        lastError = "Missing 'action' field";
                        continue;
                    }

                    return (rawOutput, decision);
                }
                catch (JsonException ex)
                {
                    lastError = ex.Message;

                    if (attempt < maxJsonRetries - 1)
                    {
                        this.loggingBroker.LogWarning(
                            $"JSON parse failed (attempt {attempt + 1}/{maxJsonRetries}): {ex.Message}");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Decision output is not valid JSON after {maxJsonRetries} attempts. " +
                            $"Last error: {ex.Message}\n\nRaw output:\n{rawOutput}", ex);
                    }
                }
            }

            throw new InvalidOperationException(
                $"Decision output failed validation after {maxJsonRetries} attempts. Last error: {lastError}");
        }

        private Configuration ExtractConfiguration(AgentContext context)
        {
            DataEntry? configEntry = context.Data.FirstOrDefault(d => d.Type == "config");

            if (configEntry == null)
            {
                throw new InvalidOperationException("No configuration found in Data");
            }

            Dictionary<string, string>? configData = 
                JsonSerializer.Deserialize<Dictionary<string, string>>(configEntry.Content);

            if (configData == null)
            {
                throw new InvalidOperationException("Invalid configuration format");
            }

            return new Configuration
            {
                Endpoint = configData["endpoint"],
                ApiKey = configData["apiKey"],
                Model = configData["model"]
            };
        }

        private string BuildPrompt(AgentContext context, AgentState state)
        {
            List<DataEntry> instructions = context.Data.Where(d => d.Type == "instruction").ToList();
            List<DataEntry> userPrompts = context.Data.Where(d => d.Type == "user_prompt").ToList();
            List<DataEntry> validationFeedback = context.Data.Where(d => d.Type == "validation_feedback").ToList();

            var promptParts = new List<string>();

            promptParts.Add("SYSTEM INSTRUCTIONS:");
            foreach (DataEntry instruction in instructions)
            {
                promptParts.Add(instruction.Content);
            }

            promptParts.Add($"\nCURRENT STATE:");
            promptParts.Add(JsonSerializer.Serialize(new
            {
                step = state.Step,
                retryCount = state.RetryCount,
                isValid = state.IsValid
            }));

            if (validationFeedback.Count > 0)
            {
                promptParts.Add("\n⚠️ VALIDATION FEEDBACK:");
                foreach (DataEntry feedback in validationFeedback)
                {
                    promptParts.Add(feedback.Content);
                }
            }

            IEnumerable<ITool> tools = this.toolBroker.GetAllTools();
            if (tools.Any())
            {
                promptParts.Add("\nAVAILABLE TOOLS:");
                foreach (ITool tool in tools)
                {
                    promptParts.Add($"[TOOL] {JsonSerializer.Serialize(new { tool.Name, tool.Description, tool.Arguments })}");
                }
            }

            promptParts.Add("\nUSER REQUEST:");
            foreach (DataEntry prompt in userPrompts)
            {
                promptParts.Add(prompt.Content);
            }

            promptParts.Add("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            promptParts.Add("CRITICAL OUTPUT INSTRUCTIONS:");
            promptParts.Add("");
            promptParts.Add("The opening brace '{' is ALREADY provided.");
            promptParts.Add("You MUST start your response with the FIRST JSON FIELD.");
            promptParts.Add("");
            promptParts.Add("CORRECT Response:");
            promptParts.Add("  \"action\": \"self\",");
            promptParts.Add("  \"payload\": {\"id\": 1, \"name\": \"John Doe\"},");
            promptParts.Add("  \"tool\": null,");
            promptParts.Add("  \"arguments\": null");
            promptParts.Add("}");
            promptParts.Add("");
            promptParts.Add("WRONG Response (DO NOT DO THIS):");
            promptParts.Add("{  ← DO NOT include this!");
            promptParts.Add("  \"action\": \"self\",");
            promptParts.Add("  ...");
            promptParts.Add("}");
            promptParts.Add("");
            promptParts.Add("Your response combines with '{' to form: { + your_response");
            promptParts.Add("");
            promptParts.Add("DO NOT:");
            promptParts.Add("- Start with '{'");
            promptParts.Add("- Write explanations before or after");
            promptParts.Add("- Use markdown code blocks");
            promptParts.Add("");
            promptParts.Add("STATE-BASED ACTIONS:");
            promptParts.Add("");
            promptParts.Add("* If state.step == 'generate':");
            promptParts.Add("  → action MUST be 'self'");
            promptParts.Add("  → payload MUST contain REAL data (e.g., id: 1, name: \"John Doe\")");
            promptParts.Add("  → Do NOT use placeholders like 'user_input_here'");
            promptParts.Add("");
            promptParts.Add("* If state.step == 'validate':");
            promptParts.Add("  → action MUST be 'tool'");
            promptParts.Add("  → Choose the appropriate validation tool:");
            promptParts.Add("    • Use 'validate_json' for JSON data");
            promptParts.Add("    • Use 'validate_csharp' for C# code");
            promptParts.Add("  → Set tool to the chosen validation tool name");
            promptParts.Add("  → Set arguments with the data to validate");
            promptParts.Add("");
            promptParts.Add("* If state.step == 'return':");
            promptParts.Add("  → action MUST be 'return'");
            promptParts.Add("  → payload MUST be the validated final output");
            promptParts.Add("");
            promptParts.Add("Remember: Start DIRECTLY with the field name, NOT with '{'.");

            return string.Join("\n", promptParts);
        }

        private class Configuration
        {
            public string Endpoint { get; set; } = string.Empty;
            public string ApiKey { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
        }
    }
}
