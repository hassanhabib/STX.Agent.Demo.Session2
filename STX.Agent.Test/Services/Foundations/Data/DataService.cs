// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using STX.Agent.Test.Brokers.Data;
using STX.Agent.Test.Models;
using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Foundations.Data
{
    public class DataService : IDataService
    {
        private readonly IDataBroker dataBroker;

        public DataService(IDataBroker dataBroker)
        {
            this.dataBroker = dataBroker;
        }

        public async ValueTask<AgentContext> RetrieveContextAsync() =>
            await this.dataBroker.GetContextAsync();

        public async ValueTask AddEntryAsync(DataEntry entry) =>
            await this.dataBroker.AddDataEntryAsync(entry);

        public async ValueTask<AgentState> RetrieveStateAsync() =>
            await this.dataBroker.GetStateAsync();

        public async ValueTask UpdateStateAsync(AgentState state) =>
            await this.dataBroker.UpdateStateAsync(state);

        public async ValueTask SetFinalOutputAsync(string output) =>
            await this.dataBroker.SetFinalOutputAsync(output);
    }
}
