using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class FornecedorControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public FornecedorControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_FornecedorById_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/fornecedor/1");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_Consulta_ReturnsSuccess()
        {
            // Arrange
            var filtro = new
            {
                cnpj = "",
                nome = "teste"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/fornecedor/Consulta", filtro);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_RecebimentosPorAno_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/fornecedor/1/RecebimentosPorAno");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_MaioresGastos_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/fornecedor/1/MaioresGastos");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
    }
}
