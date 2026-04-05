// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.Tools
{
    public interface IToolBroker
    {
        void RegisterTool(ITool tool);
        ValueTask<string> ExecuteToolAsync(string toolName, Dictionary<string, object> arguments);
        IEnumerable<ITool> GetAllTools();
    }
}
