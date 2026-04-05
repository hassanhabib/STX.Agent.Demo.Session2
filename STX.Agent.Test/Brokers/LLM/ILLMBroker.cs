// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.LLM
{
    public interface ILLMBroker
    {
        ValueTask<string> CallAsync(
            string endpoint, 
            string apiKey, 
            string model, 
            string prompt);

        ValueTask<string> CallWithPrefixAsync(
            string endpoint,
            string apiKey,
            string model,
            string prompt,
            string assistantPrefix);
    }
}
