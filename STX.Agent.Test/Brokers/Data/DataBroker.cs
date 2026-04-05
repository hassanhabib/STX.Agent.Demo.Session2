// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Models;
using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.Data
{
    public class DataBroker : IDataBroker
    {
        private readonly AgentContext context;
        private AgentState state;

        public DataBroker()
        {
            this.context = new AgentContext();
            this.state = new AgentState();
        }

        public ValueTask<AgentContext> GetContextAsync() =>
            ValueTask.FromResult(this.context);

        public ValueTask AddDataEntryAsync(DataEntry entry)
        {
            this.context.Data.Add(entry);

            return ValueTask.CompletedTask;
        }

        public ValueTask<AgentState> GetStateAsync() =>
            ValueTask.FromResult(this.state);

        public ValueTask UpdateStateAsync(AgentState state)
        {
            this.state = state;

            return ValueTask.CompletedTask;
        }

        public ValueTask SetFinalOutputAsync(string output)
        {
            this.context.FinalOutput = output;

            return ValueTask.CompletedTask;
        }
    }
}
