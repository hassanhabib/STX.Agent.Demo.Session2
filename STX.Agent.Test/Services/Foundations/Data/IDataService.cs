// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Models;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Foundations.Data
{
    public interface IDataService
    {
        ValueTask<AgentContext> RetrieveContextAsync();
        ValueTask AddEntryAsync(DataEntry entry);
        ValueTask<AgentState> RetrieveStateAsync();
        ValueTask UpdateStateAsync(AgentState state);
        ValueTask SetFinalOutputAsync(string output);
    }
}
