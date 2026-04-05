// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Collections.Generic;

namespace STX.Agent.Test.Models
{
    public class AgentContext
    {
        public List<DataEntry> Data { get; } = new();
        public string? FinalOutput { get; set; }
    }
}
