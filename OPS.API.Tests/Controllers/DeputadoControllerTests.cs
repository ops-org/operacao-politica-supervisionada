using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class DeputadoControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DeputadoControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_DeputadoById_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/1");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_GastosComPessoalPorAno_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/1/GastosComPessoalPorAno");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_FuncionarioPesquisa_ReturnsSuccess()
        {
            // Arrange
            var filtro = new
            {
                draw = 1,
                start = 0,
                length = 10,
                search = new { value = "", regex = false }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/deputado-federal/FuncionarioPesquisa", filtro);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_Funcionarios_ReturnsSuccess()
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
            var response = await _client.PostAsJsonAsync("/deputado-federal/Funcionarios", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_FuncionariosAtivosPorDeputado_ReturnsSuccess()
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
            var response = await _client.PostAsJsonAsync("/deputado-federal/1/FuncionariosAtivos", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_FuncionariosPorDeputado_ReturnsSuccess()
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
            var response = await _client.PostAsJsonAsync("/deputado-federal/1/FuncionariosHistorico", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_ResumoPresenca_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/1/ResumoPresenca");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_CamaraResumoMensal_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/ResumoMensal");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_CamaraResumoAnual_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/ResumoAnual");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_FrequenciaPorDeputado_ReturnsSuccess()
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
            var response = await _client.PostAsJsonAsync("/deputado-federal/Frequencia/1", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_Frequencia_ReturnsSuccess()
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
            var response = await _client.PostAsJsonAsync("/deputado-federal/Frequencia", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_GrupoFuncional_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/GrupoFuncional");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_Cargo_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/Cargo");

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
            var response = await _client.PostAsJsonAsync("/deputado-federal/Remuneracao", request);

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_RemuneracaoById_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-federal/Remuneracao/1");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
    }
}
