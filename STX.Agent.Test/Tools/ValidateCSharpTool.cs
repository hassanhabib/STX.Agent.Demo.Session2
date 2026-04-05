// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE AS LONG AS SOFTWARE FUNDS ARE DONATED TO THE POOR
// ---------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using STX.Agent.Test.Brokers.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace STX.Agent.Test.Tools
{
    public class ValidateCSharpTool : ITool
    {
        public string Name => "validate_csharp";

        public string Description =>
            "Validates if C# code is syntactically and semantically correct using Roslyn compiler";

        public Dictionary<string, string> Arguments => new()
        {
            { "code", "string - The C# code to validate" }
        };

        public ValueTask<string> ExecuteAsync(Dictionary<string, object> arguments)
        {
            if (!arguments.TryGetValue("code", out object? codeValue))
            {
                return ValueTask.FromResult(JsonSerializer.Serialize(new
                {
                    valid = false,
                    error = "Missing 'code' argument"
                }));
            }

            string code = codeValue.ToString() ?? string.Empty;

            try
            {
                // Parse the code into a syntax tree
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

                // Check for syntax errors
                var diagnostics = syntaxTree.GetDiagnostics().ToList();
                var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                if (errors.Any())
                {
                    var errorMessages = errors.Select(e => 
                        $"{e.Id}: {e.GetMessage()} at {e.Location.GetLineSpan().StartLinePosition}");

                    return ValueTask.FromResult(JsonSerializer.Serialize(new
                    {
                        valid = false,
                        hasSyntaxErrors = true,
                        errorCount = errors.Count,
                        errors = errorMessages,
                        message = $"C# code has {errors.Count} syntax error(s)"
                    }));
                }

                // Create a compilation to check semantic errors
                var compilation = CSharpCompilation.Create(
                    "ValidationAssembly",
                    syntaxTrees: new[] { syntaxTree },
                    references: new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
                    },
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                var semanticDiagnostics = compilation.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToList();

                if (semanticDiagnostics.Any())
                {
                    var errorMessages = semanticDiagnostics.Select(e =>
                        $"{e.Id}: {e.GetMessage()}");

                    return ValueTask.FromResult(JsonSerializer.Serialize(new
                    {
                        valid = false,
                        hasSemanticErrors = true,
                        errorCount = semanticDiagnostics.Count,
                        errors = errorMessages,
                        message = $"C# code has {semanticDiagnostics.Count} semantic error(s)"
                    }));
                }

                // Code is valid!
                return ValueTask.FromResult(JsonSerializer.Serialize(new
                {
                    valid = true,
                    hasSyntaxErrors = false,
                    hasSemanticErrors = false,
                    message = "C# code is syntactically and semantically valid"
                }));
            }
            catch (System.Exception ex)
            {
                return ValueTask.FromResult(JsonSerializer.Serialize(new
                {
                    valid = false,
                    error = $"Validation failed: {ex.Message}"
                }));
            }
        }
    }
}
