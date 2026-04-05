// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Models;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Foundations.Directions
{
    public interface IDirectionService
    {
        ValueTask<ValidationResult?> ExecuteActionAsync(
            DecisionOutput decision,
            AgentState state);
    }
}
