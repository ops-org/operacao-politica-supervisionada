using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class SenadorControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public SenadorControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_SenadorById_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/1");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_GastosComPessoalPorAno_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/1/GastosComPessoalPorAno");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }


        [Fact]
        public async Task Get_SenadoResumoMensal_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/ResumoMensal");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_SenadoResumoAnual_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/ResumoAnual");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Lotacao_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/Lotacao");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Categoria_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/Categoria");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Vinculo_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/Vinculo");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Cargo_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/Cargo");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_Remuneracao_ReturnsSuccess()
        {
            // Arrange
            var request = new
            {
                draw = 1,
                start = 0,
                length = 10,
                search = new { value = "", regex = false }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/senador/Remuneracao", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_RemuneracaoById_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/senador/Remuneracao/1");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
    }
}
