using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core;
using OPS.Core.DAO;
using OPS.Core.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPS.WebApi
{
    [ApiController]
    [Route("[controller]")]
    [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }

        DeputadoDao dao;

        public DeputadoController(IConfiguration configuration, IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = configuration;

            dao = new DeputadoDao();
        }

        [HttpGet("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            return await dao.Consultar(id);
        }

        [HttpPost("Lista")]
        [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            return await dao.Lista(filtro);
        }

        [HttpPost("Pesquisa")]
        [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            return await dao.Pesquisa(filtro);
        }

        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await dao.Lancamentos(request);
        }

        [HttpGet("TipoDespesa")]
        [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            return await dao.TipoDespesa();
        }

        [HttpPost("Secretarios")]
        public async Task<dynamic> Secretarios(DataTablesRequest request)
        {
            return await dao.Secretarios(request);
        }

        [HttpPost("{id:int}/SecretariosAtivos")]
        public async Task<dynamic> SecretariosAtivosPorDeputado(int id, DataTablesRequest request)
        {
            return await dao.SecretariosAtivosPorDeputado(id, request);
        }

        [HttpPost("{id:int}/SecretariosHistorico")]
        public async Task<dynamic> SecretariosPorDeputado(int id, DataTablesRequest request)
        {
            return await dao.SecretariosPorDeputado(id, request);
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            return await dao.GastosPorAno(id);
        }

        [HttpGet("Documento/{id:int}")]
        public async Task<dynamic> Documento(int id)
        {
            var result = await dao.Documento(id);

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            var result = await dao.DocumentosDoMesmoDia(id);

            return Ok(result);
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            var result = await dao.DocumentosDaSubcotaMes(id);

            return Ok(result);
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            return await dao.MaioresNotas(id);
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            return await dao.MaioresFornecedores(id);
        }

        [HttpGet("{id:int}/ResumoPresenca")]
        public async Task<dynamic> ResumoPresenca(int id)
        {
            return await dao.ResumoPresenca(id);
        }

        [HttpGet("CamaraResumoMensal")]
        public async Task<dynamic> CamaraResumoMensal()
        {
            return await dao.CamaraResumoMensal();
        }

        [HttpGet("CamaraResumoAnual")]
        public async Task<dynamic> CamaraResumoAnual()
        {
            return await dao.CamaraResumoAnual();
        }

        [HttpPost("Frequencia/{id:int}")]
        public async Task<dynamic> Frequencia(int id, DataTablesRequest request)
        {
            return await dao.Frequencia(id, request);
        }

        [HttpPost("Frequencia")]
        public async Task<dynamic> Frequencia(DataTablesRequest request)
        {
            return await dao.Frequencia(request);
        }

        [HttpGet("Imagem/{id}")]
        public VirtualFileResult Imagem(string id)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
            {
                var file = @"images/depfederal/" + id + ".jpg";
                var filePath = System.IO.Path.Combine(Environment.ContentRootPath, file);

                if (System.IO.File.Exists(filePath))
                {
                    return File(file, "image/jpeg");
                }
            }

            return File(@"images/sem_foto.jpg", "image/jpeg");
        }
    }
}
