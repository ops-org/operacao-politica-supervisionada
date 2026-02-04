using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class PartidoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public PartidoControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Partido_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/partido");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        // Only GET /partido is currently available in PartidoController.
    }
}
