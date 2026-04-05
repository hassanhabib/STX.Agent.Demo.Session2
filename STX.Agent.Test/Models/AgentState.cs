// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

namespace STX.Agent.Test.Models
{
    public class AgentState
    {
        public string Step { get; set; } = "generate";
        public string? LastPayload { get; set; }
        public bool IsValid { get; set; } = false;
        public string? ValidationError { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public int IterationCount { get; set; } = 0;
    }
}
