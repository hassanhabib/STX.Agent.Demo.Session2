// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Models;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Foundations.Decisions
{
    public interface IDecisionService
    {
        ValueTask<(string RawOutput, DecisionOutput Decision)> MakeDecisionAsync(
            AgentContext context,
            AgentState state);
    }
}
