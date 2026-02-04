using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class EstadoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public EstadoControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Estado_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/estado");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        // Only GET /estado is currently available in EstadoController.
    }
}
