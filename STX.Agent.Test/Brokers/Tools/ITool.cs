// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.Tools
{
    public interface ITool
    {
        string Name { get; }
        string Description { get; }
        Dictionary<string, string> Arguments { get; }
        ValueTask<string> ExecuteAsync(Dictionary<string, object> arguments);
    }
}
