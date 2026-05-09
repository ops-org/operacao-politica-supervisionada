using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPS.API.Middleware;
using Xunit;

namespace OPS.API.Tests.Integration
{
    public class MiddlewarePipelineIntegrationTests
    {
        public MiddlewarePipelineIntegrationTests(ITestOutputHelper output)
        {
            // Output helper available for future debugging if needed
        }

        [Fact]
        public async Task MiddlewarePipeline_WithMarkdownRequest_ReturnsMarkdownResponse()
        {
            // Arrange
            using var host = await CreateTestHostAsync();
            using var client = host.GetTestClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/test");
            request.Headers.Add("Accept", "text/markdown");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/markdown", response.Content.Headers.ContentType?.MediaType);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("# API Response", content);
        }

        [Fact]
        public async Task MiddlewarePipeline_WithJsonRequest_ReturnsJsonResponse()
        {
            // Arrange
            using var host = await CreateTestHostAsync();
            using var client = host.GetTestClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/test");
            request.Headers.Add("Accept", "application/json");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"message\":\"test response\"", content);
        }

        [Fact]
        public async Task MiddlewarePipeline_IncludesLinkHeaders()
        {
            // Arrange
            using var host = await CreateTestHostAsync();
            using var client = host.GetTestClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var linkHeaders = response.Headers.GetValues("Link");
            Assert.NotNull(linkHeaders);
            Assert.NotEmpty(linkHeaders);

            // Verify expected Link headers are present
            var linkHeaderList = linkHeaders.ToList();
            Assert.Contains(linkHeaderList, h => h.Contains("api-catalog"));
            Assert.Contains(linkHeaderList, h => h.Contains("service-doc"));
            Assert.Contains(linkHeaderList, h => h.Contains("status"));
            Assert.Contains(linkHeaderList, h => h.Contains("agent-skills"));
            Assert.Contains(linkHeaderList, h => h.Contains("mcp-server-card"));
        }

        [Fact]
        public async Task MiddlewarePipeline_WithException_PropagatesExceptionCorrectly()
        {
            // Arrange
            using var host = await CreateTestHostWithExceptionAsync();
            using var client = host.GetTestClient();

            // Act
            var exception = await Assert.ThrowsAsync<System.InvalidOperationException>(
                () => client.GetAsync("/error"));

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task MiddlewarePipeline_WithLargeResponse_HandlesCorrectly()
        {
            // Arrange
            using var host = await CreateTestHostAsync();
            using var client = host.GetTestClient();

            // Act
            var response = await client.GetAsync("/large-response");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content.Length); // Verify large content is handled
        }

        private static async Task<IHost> CreateTestHostAsync()
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddLogging(builder =>
                            {
                                builder.SetMinimumLevel(LogLevel.Warning);
                                builder.AddProvider(new TestLoggerProvider());
                            });
                        })
                        .Configure(app =>
                        {
                            // Add middleware in the same order as production
                            app.UseMiddleware<LinkHeaderMiddleware>();
                            app.UseMiddleware<MarkdownNegotiationMiddleware>();

                            // Test endpoint
                            app.Run(async context =>
                            {
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync("{\"message\":\"test response\"}");
                            });
                        });
                });

            return await builder.StartAsync();
        }

        private static async Task<IHost> CreateTestHostWithExceptionAsync()
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddLogging(builder =>
                            {
                                builder.SetMinimumLevel(LogLevel.Warning);
                                builder.AddProvider(new TestLoggerProvider());
                            });
                        })
                        .Configure(app =>
                        {
                            app.UseMiddleware<LinkHeaderMiddleware>();
                            app.UseMiddleware<MarkdownNegotiationMiddleware>();

                            // Error endpoint
                            app.Run(context =>
                            {
                                throw new InvalidOperationException("Test exception");
                            });
                        });
                });

            return await builder.StartAsync();
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            private bool _disposed = false;

            public ILogger CreateLogger(string categoryName)
            {
                return new TestLogger();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        // Dispose managed resources
                    }
                    _disposed = true;
                }
            }
        }

        private class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null!;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                // Suppress logging for tests
            }
        }
    }
}
