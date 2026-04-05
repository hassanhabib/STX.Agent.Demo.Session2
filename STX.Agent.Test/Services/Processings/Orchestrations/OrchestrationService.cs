// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Brokers.Logging;
using STX.Agent.Test.Models;
using STX.Agent.Test.Services.Foundations.Data;
using STX.Agent.Test.Services.Foundations.Decisions;
using STX.Agent.Test.Services.Foundations.Directions;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Processings.Orchestrations
{
    public class OrchestrationService : IOrchestrationService
    {
        private readonly IDataService dataService;
        private readonly IDecisionService decisionService;
        private readonly IDirectionService directionService;
        private readonly ILoggingBroker loggingBroker;
        private readonly int maxIterations;

        public OrchestrationService(
            IDataService dataService,
            IDecisionService decisionService,
            IDirectionService directionService,
            ILoggingBroker loggingBroker,
            int maxIterations = 10)
        {
            this.dataService = dataService;
            this.decisionService = decisionService;
            this.directionService = directionService;
            this.loggingBroker = loggingBroker;
            this.maxIterations = maxIterations;
        }

        public async ValueTask RunAsync()
        {
            this.loggingBroker.LogInformation("Agent orchestration started");

            AgentState state = await this.dataService.RetrieveStateAsync();

            for (int iteration = 0; iteration < this.maxIterations; iteration++)
            {
                this.loggingBroker.LogInformation(
                    $"Iteration {iteration + 1}/{this.maxIterations} - Step: {state.Step}, Retry: {state.RetryCount}");

                await AddStateToData(state);

                if (state.ValidationError != null && state.Step == "generate")
                {
                    await AddValidationFeedback(state);
                }

                AgentContext context = await this.dataService.RetrieveContextAsync();

                (string rawOutput, DecisionOutput decision) = 
                    await this.decisionService.MakeDecisionAsync(context, state);

                this.loggingBroker.LogInformation($"Decision: {decision.Action}");

                EnforceActionBasedOnState(decision, state);

                await this.dataService.AddEntryAsync(new DataEntry
                {
                    Type = "decision",
                    Content = $"Action: {decision.Action}, Payload: {decision.Payload}"
                });

                string previousPayload = state.LastPayload ?? string.Empty;
                string previousStep = state.Step;

                ValidationResult? validationResult = 
                    await this.directionService.ExecuteActionAsync(decision, state);

                UpdateState(state, decision, validationResult);

                if (previousStep == "generate" && 
                    state.Step == "validate" && 
                    state.RetryCount > 0 && 
                    previousPayload == state.LastPayload)
                {
                    throw new InvalidOperationException(
                        "Agent repeated the same invalid output without improvement");
                }

                await this.dataService.UpdateStateAsync(state);

                context = await this.dataService.RetrieveContextAsync();

                if (state.Step == "return" && decision.Action == "return" && context.FinalOutput != null)
                {
                    this.loggingBroker.LogInformation(
                        $"Agent completed successfully in {iteration + 1} iteration(s), Retries: {state.RetryCount}");
                    break;
                }
            }

            AgentContext finalContext = await this.dataService.RetrieveContextAsync();

            if (finalContext.FinalOutput == null)
            {
                this.loggingBroker.LogWarning("Max iterations reached without final output");
            }
        }

        private async ValueTask AddStateToData(AgentState state)
        {
            string stateJson = JsonSerializer.Serialize(new
            {
                step = state.Step,
                lastPayload = state.LastPayload,
                isValid = state.IsValid,
                validationError = state.ValidationError,
                retryCount = state.RetryCount,
                maxRetries = state.MaxRetries
            });

            await this.dataService.AddEntryAsync(new DataEntry
            {
                Type = "state",
                Content = stateJson
            });
        }

        private async ValueTask AddValidationFeedback(AgentState state)
        {
            string feedbackJson = JsonSerializer.Serialize(new
            {
                validation_error = state.ValidationError,
                retry_count = state.RetryCount,
                message = $"Previous attempt failed validation. You MUST fix the error: {state.ValidationError}"
            });

            await this.dataService.AddEntryAsync(new DataEntry
            {
                Type = "validation_feedback",
                Content = feedbackJson
            });
        }

        private void EnforceActionBasedOnState(DecisionOutput decision, AgentState state)
        {
            switch (state.Step)
            {
                case "generate":
                    if (decision.Action != "self")
                    {
                        this.loggingBroker.LogWarning(
                            $"Guardrail: Forcing action from '{decision.Action}' to 'self' (generate step)");
                        decision.Action = "self";
                    }
                    break;

                case "validate":
                    if (decision.Action != "tool")
                    {
                        this.loggingBroker.LogWarning(
                            $"Guardrail: Forcing action from '{decision.Action}' to 'tool' (validate step)");
                        decision.Action = "tool";

                        // Only set default tool if none specified
                        if (string.IsNullOrEmpty(decision.Tool))
                        {
                            decision.Tool = "validate_json";  // Default fallback
                        }
                    }
                    break;

                case "return":
                    if (decision.Action != "return")
                    {
                        this.loggingBroker.LogWarning(
                            $"Guardrail: Forcing action from '{decision.Action}' to 'return' (return step)");
                        decision.Action = "return";

                        if (state.LastPayload != null)
                        {
                            decision.Payload = JsonDocument.Parse(state.LastPayload).RootElement;
                        }
                    }
                    break;
            }
        }

        private void UpdateState(AgentState state, DecisionOutput decision, ValidationResult? result)
        {
            if (state.Step == "generate" && decision.Action == "self")
            {
                state.LastPayload = decision.Payload.ToString();
                state.Step = "validate";
                return;
            }

            if (state.Step == "validate")
            {
                if (result != null && result.IsValid)
                {
                    state.IsValid = true;
                    state.ValidationError = null;
                    state.Step = "return";
                }
                else
                {
                    state.IsValid = false;
                    state.ValidationError = result?.Error ?? "Unknown validation error";
                    state.RetryCount++;

                    if (state.RetryCount >= state.MaxRetries)
                    {
                        throw new InvalidOperationException(
                            $"Validation failed after {state.MaxRetries} retries. " +
                            $"Last error: {state.ValidationError}");
                    }

                    state.Step = "generate";
                }
                return;
            }
        }
    }
}
