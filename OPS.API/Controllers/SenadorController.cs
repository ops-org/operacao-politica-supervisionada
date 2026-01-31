using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core.DTOs;
using OPS.Core.Repositories;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class SenadorController : Controller
    {

        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }
        private readonly SenadorRepository _senadorRepository;

        public SenadorController(IConfiguration configuration, IWebHostEnvironment env, SenadorRepository senadorRepository)
        {
            Environment = env;
            Configuration = configuration;
            _senadorRepository = senadorRepository;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            return await _senadorRepository.Consultar(id);
        }

        [HttpPost("Lista")]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            return await _senadorRepository.Lista(filtro);
        }


        [HttpPost("Pesquisa")]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro)
        {
            return await _senadorRepository.Pesquisa(filtro);
        }

        [HttpPost]
        [Route("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await _senadorRepository.Lancamentos(request);
        }

        [HttpGet]
        [Route("TipoDespesa")]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            return await _senadorRepository.TipoDespesa();
        }

        [HttpGet("Documento/{id:int}")]
        public async Task<ActionResult<DocumentoDetalheDTO>> Documento(int id)
        {
            var result = await _senadorRepository.Documento(id);

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            var result = await _senadorRepository.DocumentosDoMesmoDia(id);

            return Ok(result);
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            var result = await _senadorRepository.DocumentosDaSubcotaMes(id);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            return await _senadorRepository.GastosPorAno(id);
        }

        [HttpGet("{id:int}/GastosComPessoalPorAno")]
        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            return await _senadorRepository.GastosComPessoalPorAno(id);
        }

        [HttpGet]
        [Route("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            return await _senadorRepository.MaioresNotas(id);
        }

        [HttpGet]
        [Route("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            return await _senadorRepository.MaioresFornecedores(id);
        }

        [HttpGet]
        [Route("SenadoResumoMensal")]
        public async Task<dynamic> SenadoResumoMensal()
        {
            return await _senadorRepository.SenadoResumoMensal();
        }

        [HttpGet]
        [Route("SenadoResumoAnual")]
        public async Task<dynamic> SenadoResumoAnual()
        {
            return await _senadorRepository.SenadoResumoAnual();
        }

        [HttpGet("Imagem/{id}")]
        public VirtualFileResult Imagem(string id)
        {
            if (!string.IsNullOrEmpty(Environment.ContentRootPath))
            {
                var file = @"images/senador/" + id + ".jpg";
                var filePath = System.IO.Path.Combine(Environment.ContentRootPath, file);

                if (System.IO.File.Exists(filePath))
                {
                    return File(file, "image/jpeg");
                }
            }

            return File(@"images/sem_foto.jpg", "image/jpeg");
        }

        [HttpGet]
        [Route("Lotacao")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lotacao()
        {
            return await _senadorRepository.Lotacao();
        }

        [HttpGet]
        [Route("Cargo")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Cargo()
        {
            return await _senadorRepository.Cargo();
        }

        [HttpGet]
        [Route("Categoria")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Categoria()
        {
            return await _senadorRepository.Categoria();
        }

        [HttpGet]
        [Route("Vinculo")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Vinculo()
        {
            return await _senadorRepository.Vinculo();
        }

        [HttpPost]
        [Route("Remuneracao")]
        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            return await _senadorRepository.Remuneracao(request);
        }

        [HttpGet]
        [Route("Remuneracao/{id:int}")]
        public async Task<dynamic> Remuneracao(int id)
        {
            return await _senadorRepository.Remuneracao(id);
        }
    }
}
