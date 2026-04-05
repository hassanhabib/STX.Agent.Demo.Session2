// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using System;

namespace STX.Agent.Test.Brokers.Logging
{
    public class LoggingBroker : ILoggingBroker
    {
        public void LogInformation(string message) =>
            Console.WriteLine($"[INFO] {message}");

        public void LogWarning(string message) =>
            Console.WriteLine($"[WARNING] {message}");

        public void LogError(Exception exception) =>
            Console.WriteLine($"[ERROR] {exception.Message}");

        public void LogError(string message) =>
            Console.WriteLine($"[ERROR] {message}");
    }
}
