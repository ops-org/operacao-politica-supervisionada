using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OPS.API.Middleware
{
    public class MarkdownNegotiationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MarkdownNegotiationMiddleware> _logger;

        public MarkdownNegotiationMiddleware(RequestDelegate next, ILogger<MarkdownNegotiationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalResponseStream = context.Response.Body;
            MemoryStream? responseBody = null;
            try
            {
                responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                // Check if client accepts markdown
                var acceptHeader = context.Request.Headers["Accept"].FirstOrDefault();
                if (!string.IsNullOrEmpty(acceptHeader) && acceptHeader.Contains("text/markdown") &&
                    context.Response.ContentType?.Contains("application/json") == true)
                {
                    // Reset response body to read the content
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var jsonContent = await new StreamReader(responseBody).ReadToEndAsync();

                    try
                    {
                        // Convert JSON to markdown
                        var markdownContent = ConvertJsonToMarkdown(jsonContent);

                        // Update response
                        context.Response.Body = originalResponseStream;
                        context.Response.ContentType = "text/markdown";
                        context.Response.ContentLength = Encoding.UTF8.GetByteCount(markdownContent);

                        await context.Response.WriteAsync(markdownContent);
                        return;
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger?.LogWarning(jsonEx, "Failed to parse JSON for markdown conversion");
                        // Fall back to original response
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Unexpected error during markdown conversion");
                        // Fall back to original response
                    }
                }

                // If no markdown conversion needed, copy original response
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalResponseStream);
            }
            finally
            {
                // Ensure original response stream is always restored
                if (context.Response.Body != originalResponseStream)
                {
                    context.Response.Body = originalResponseStream;
                }

                // Dispose MemoryStream properly
                if (responseBody != null)
                {
                    await responseBody.DisposeAsync();
                }
            }
        }

        private string ConvertJsonToMarkdown(string jsonContent)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;

                // Pre-allocate StringBuilder capacity based on JSON content size
                var estimatedCapacity = Math.Max(jsonContent.Length * 2, 1024);
                var markdown = new StringBuilder(estimatedCapacity);
                markdown.AppendLine("# API Response");
                markdown.AppendLine();

                ConvertElementToMarkdown(root, markdown, 0);

                return markdown.ToString();
            }
            catch (JsonException jsonEx)
            {
                _logger?.LogWarning(jsonEx, "JSON parsing failed, falling back to raw JSON");
                return "# API Response\n\n```json\n" + jsonContent + "\n```";
            }
        }

        private void ConvertElementToMarkdown(JsonElement element, StringBuilder markdown, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        markdown.AppendLine($"{indent}**{property.Name}**: ");
                        ConvertElementToMarkdown(property.Value, markdown, indentLevel + 1);
                        markdown.AppendLine();
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        markdown.AppendLine($"{indent}[{index}]:");
                        ConvertElementToMarkdown(item, markdown, indentLevel + 1);
                        markdown.AppendLine();
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    var stringValue = element.GetString();
                    if (System.Uri.TryCreate(stringValue, System.UriKind.Absolute, out _))
                    {
                        markdown.AppendLine($"{indent}[{stringValue}]({stringValue})");
                    }
                    else
                    {
                        markdown.AppendLine($"{indent}{stringValue}");
                    }
                    break;

                default:
                    markdown.AppendLine($"{indent}{element}");
                    break;
            }
        }
    }
}
