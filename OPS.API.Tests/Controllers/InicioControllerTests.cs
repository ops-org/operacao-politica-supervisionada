using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class InicioControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public InicioControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ParlamentarResumoGastos_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/inicio/ParlamentarResumoGastos");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Busca_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/inicio/Busca?value=teste");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Importacao_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/inicio/Importacao");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
    }
}
