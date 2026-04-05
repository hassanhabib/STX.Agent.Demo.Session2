// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace STX.Agent.Test.Brokers.LLM
{
    public class LLMBroker : ILLMBroker
    {
        private readonly HttpClient httpClient;

        public LLMBroker()
        {
            this.httpClient = new HttpClient();
        }

        public async ValueTask<string> CallAsync(
            string endpoint,
            string apiKey,
            string model,
            string prompt)
        {
            return await CallWithPrefixAsync(endpoint, apiKey, model, prompt, null);
        }

        public async ValueTask<string> CallWithPrefixAsync(
            string endpoint,
            string apiKey,
            string model,
            string prompt,
            string assistantPrefix)
        {
            string requestUrl = $"{endpoint.TrimEnd('/')}/chat/completions";

            var messages = new System.Collections.Generic.List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = prompt
                }
            };

            // Add assistant prefix if provided
            if (!string.IsNullOrEmpty(assistantPrefix))
            {
                messages.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = assistantPrefix
                });
            }

            var request = new ChatCompletionRequest
            {
                Model = model,
                Messages = messages.ToArray(),
                Stop = !string.IsNullOrEmpty(assistantPrefix) 
                    ? new[] { "```", "\n\n", "\n\nThis", "\n\nSince", "\n\nBased", "\n\nNote" }
                    : null
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = JsonContent.Create(request)
            };

            if (!string.IsNullOrEmpty(apiKey))
            {
                httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
            }

            HttpResponseMessage response = await this.httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody);

            if (chatResponse?.Choices == null || chatResponse.Choices.Length == 0)
            {
                throw new InvalidOperationException("No response from LLM");
            }

            string content = chatResponse.Choices[0].Message.Content;

            // If we used a prefix, prepend it to the response for complete JSON
            if (!string.IsNullOrEmpty(assistantPrefix))
            {
                // Remove leading opening brace if LLM includes it anyway
                content = content.TrimStart();
                if (content.StartsWith("{"))
                {
                    content = content.Substring(1).TrimStart();
                }

                content = assistantPrefix + content;
            }

            return content;
        }

        private class ChatCompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public ChatMessage[] Messages { get; set; } = Array.Empty<ChatMessage>();

            [JsonPropertyName("stop")]
            public string[]? Stop { get; set; }
        }

        private class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private class ChatCompletionResponse
        {
            [JsonPropertyName("choices")]
            public ChatChoice[] Choices { get; set; } = Array.Empty<ChatChoice>();
        }

        private class ChatChoice
        {
            [JsonPropertyName("message")]
            public ChatMessage Message { get; set; } = new();
        }
    }
}
