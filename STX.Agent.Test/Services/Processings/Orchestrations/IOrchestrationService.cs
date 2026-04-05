// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Threading.Tasks;

namespace STX.Agent.Test.Services.Processings.Orchestrations
{
    public interface IOrchestrationService
    {
        ValueTask RunAsync();
    }
}
