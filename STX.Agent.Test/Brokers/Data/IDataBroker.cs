// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Models;
using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.Data
{
    public interface IDataBroker
    {
        ValueTask<AgentContext> GetContextAsync();
        ValueTask AddDataEntryAsync(DataEntry entry);
        ValueTask<AgentState> GetStateAsync();
        ValueTask UpdateStateAsync(AgentState state);
        ValueTask SetFinalOutputAsync(string output);
    }
}
