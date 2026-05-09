using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OPS.API.Middleware;
using Xunit;

namespace OPS.API.Tests.Middleware
{
    public class MarkdownNegotiationMiddlewareTests
    {
        private readonly Mock<ILogger<MarkdownNegotiationMiddleware>> _loggerMock;

        public MarkdownNegotiationMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<MarkdownNegotiationMiddleware>>();
        }

        [Fact]
        public async Task InvokeAsync_WithMarkdownAcceptHeader_ConvertsJsonToMarkdown()
        {
            // Arrange
            var context = CreateHttpContext("text/markdown, application/json");
            var responseBody = "{\"test\": \"value\"}";

            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(responseBody);
            });

            var middleware = new MarkdownNegotiationMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("text/markdown", context.Response.ContentType);
            var response = await GetResponseText(context);
            Assert.Contains("# API Response", response);
        }

        [Fact]
        public async Task InvokeAsync_WithoutMarkdownAcceptHeader_ReturnsOriginalResponse()
        {
            // Arrange
            var context = CreateHttpContext("application/json");
            var responseBody = "{\"test\": \"value\"}";

            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(responseBody);
            });

            var middleware = new MarkdownNegotiationMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("application/json", context.Response.ContentType);
            var response = await GetResponseText(context);
            Assert.Equal(responseBody, response);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidJson_FallsBackToOriginalResponse()
        {
            // Arrange
            var context = CreateHttpContext("text/markdown");
            var invalidJson = "{ invalid json }";

            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(invalidJson);
            });

            var middleware = new MarkdownNegotiationMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("text/markdown", context.Response.ContentType);
            var response = await GetResponseText(context);
            Assert.Equal("# API Response\n\n```json\n{ invalid json }\n```", response);
        }

        [Fact]
        public async Task InvokeAsync_WithNullAcceptHeader_ReturnsOriginalResponse()
        {
            // Arrange
            var context = CreateHttpContext(null);
            var responseBody = "{\"test\": \"value\"}";

            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(responseBody);
            });

            var middleware = new MarkdownNegotiationMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("application/json", context.Response.ContentType);
            var response = await GetResponseText(context);
            Assert.Equal(responseBody, response);
        }

        [Fact]
        public async Task InvokeAsync_WithNonJsonContentType_ReturnsOriginalResponse()
        {
            // Arrange
            var context = CreateHttpContext("text/markdown");
            var responseBody = "<html>test</html>";

            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync(responseBody);
            });

            var middleware = new MarkdownNegotiationMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("text/html", context.Response.ContentType);
            var response = await GetResponseText(context);
            Assert.Equal(responseBody, response);
        }

        [Fact]
        public async Task InvokeAsync_WithException_RestoresOriginalStream()
        {
            // Arrange
            var context = CreateHttpContext("text/markdown");
            var originalStream = context.Response.Body;

            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync("{\"test\": \"value\"}");
                throw new InvalidOperationException("Test exception");
            });

            var middleware = new MarkdownNegotiationMiddleware(next, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
            Assert.Equal(originalStream, context.Response.Body);
        }

        private static HttpContext CreateHttpContext(string acceptHeader)
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            if (acceptHeader != null)
            {
                context.Request.Headers.Add("Accept", acceptHeader);
            }

            return context;
        }

        private static async Task<string> GetResponseText(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
