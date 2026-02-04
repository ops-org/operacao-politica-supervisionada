using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OPS.API.Tests;
using Shouldly;
using Xunit;

namespace OPS.API.Tests.Controllers
{
    public class DeputadoEstadualControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DeputadoEstadualControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_DeputadoEstadualById_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-estadual/1");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_DocumentoDetalhe_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-estadual/Documento/1904690");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_AssembleiaResumoMensal_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-estadual/ResumoMensal");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_AssembleiaResumoAnual_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-estadual/ResumoAnual");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_GastosPorAno_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-estadual/1/GastosPorAno");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_TipoDespesa_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/deputado-estadual/TipoDespesa");

            // Assert
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        }
    }
}
