using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OPS.API.Middleware
{
    public class LinkHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LinkHeaderMiddleware> _logger;

        public LinkHeaderMiddleware(RequestDelegate next, ILogger<LinkHeaderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                try
                {
                    // Add Link headers for agent discovery
                    var linkHeaders = new List<string>
                    {
                        "</.well-known/api-catalog>; rel=\"api-catalog\"",
                        "</scalar>; rel=\"service-doc\"",
                        "</health>; rel=\"status\"",
                        "</.well-known/agent-skills/index.json>; rel=\"agent-skills\"",
                        "</.well-known/mcp/server-card.json>; rel=\"mcp-server-card\""
                    };

                    foreach (var linkHeader in linkHeaders)
                    {
                        context.Response.Headers.Append("Link", linkHeader);
                    }

                    _logger.LogDebug("Added {Count} Link headers to response", linkHeaders.Count);
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add Link headers to response");
                    return Task.CompletedTask;
                }
            });

            await _next(context);
        }
    }
}
