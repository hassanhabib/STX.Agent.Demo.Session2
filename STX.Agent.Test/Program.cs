// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Brokers.Data;
using STX.Agent.Test.Brokers.LLM;
using STX.Agent.Test.Brokers.Logging;
using STX.Agent.Test.Brokers.Tools;
using STX.Agent.Test.Models;
using STX.Agent.Test.Services.Foundations.Data;
using STX.Agent.Test.Services.Foundations.Decisions;
using STX.Agent.Test.Services.Foundations.Directions;
using STX.Agent.Test.Services.Processings.Orchestrations;
using STX.Agent.Test.Tools;
using System.Text.Json;

namespace STX.Agent.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Brokers
            IDataBroker dataBroker = new DataBroker();
            ILLMBroker llmBroker = new LLMBroker();
            ILoggingBroker loggingBroker = new LoggingBroker();
            IToolBroker toolBroker = new ToolBroker();

            // Register Tools
            toolBroker.RegisterTool(new ValidateJsonTool());
            toolBroker.RegisterTool(new ValidateCSharpTool());

            // Initialize Foundation Services
            IDataService dataService = new DataService(dataBroker);
            IDecisionService decisionService = new DecisionService(llmBroker, toolBroker, loggingBroker);
            IDirectionService directionService = new DirectionService(toolBroker, dataService);

            // Initialize Processing Service
            IOrchestrationService orchestrationService = new OrchestrationService(
                dataService,
                decisionService,
                directionService,
                loggingBroker,
                maxIterations: 10);

            // Setup Data
            await dataService.AddEntryAsync(new DataEntry
            {
                Type = "config",
                Content = JsonSerializer.Serialize(new
                {
                    endpoint = "http://192.168.0.196:3000/v1",
                    apiKey = "pllm_22a01582ee3c9c44a459fc486c7760f5429dc87d952ebffe",
                    model = "mistral-7b-instruct-v0.2.Q8_0"
                })
            });

            await dataService.AddEntryAsync(new DataEntry
            {
                Type = "instruction",
                Content = @"You are the Decision component of an AI agent system.

You are given Data which includes:
* user input
* previous outputs
* available tools
* tool results

Your job is to decide the next action.

Available actions:
* 'return': return final result
* 'self': refine or generate data
* 'tool': call a tool

Policy:
* Check available tools before performing self-actions
* Prefer tools when they match the task (e.g., validation)
* You MAY solve tasks yourself if confident

Rules:
* Return ONLY valid JSON
* Do NOT include explanations
* Do NOT include markdown

Output format:
{
  ""action"": ""return"" | ""self"" | ""tool"",
  ""payload"": ""..."",
  ""tool"": ""..."",
  ""arguments"": { }
}"
            });

            await dataService.AddEntryAsync(new DataEntry
            {
                Type = "user_prompt",
                Content = "Generate a JSON object for a student with id " +
                "with type string and name with type string and dob with type date"
            });

            // Run Agent
            await orchestrationService.RunAsync();

            // Display Results
            AgentContext finalContext = await dataService.RetrieveContextAsync();

            Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      FINAL OUTPUT                         ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

            if (finalContext.FinalOutput != null)
            {
                Console.WriteLine(finalContext.FinalOutput);
            }
            else
            {
                Console.WriteLine("No final output generated.");
            }

            Console.WriteLine($"\nTotal data entries: {finalContext.Data.Count}");
        }
    }
}
