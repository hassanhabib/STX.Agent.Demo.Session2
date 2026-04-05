// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.Tools
{
    public class ToolBroker : IToolBroker
    {
        private readonly Dictionary<string, ITool> tools;

        public ToolBroker()
        {
            this.tools = new Dictionary<string, ITool>();
        }

        public void RegisterTool(ITool tool) =>
            this.tools[tool.Name] = tool;

        public async ValueTask<string> ExecuteToolAsync(
            string toolName,
            Dictionary<string, object> arguments)
        {
            if (!this.tools.TryGetValue(toolName, out ITool? tool))
            {
                throw new InvalidOperationException($"Tool '{toolName}' not found");
            }

            return await tool.ExecuteAsync(arguments);
        }

        public IEnumerable<ITool> GetAllTools() =>
            this.tools.Values.ToList();
    }
}
