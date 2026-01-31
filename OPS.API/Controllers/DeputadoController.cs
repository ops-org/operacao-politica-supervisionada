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
    [Route("deputado-federal")]
    // [CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class DeputadoController : Controller
    {
        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }
        private readonly DeputadoRepository _deputadoRepository;

        public DeputadoController(IConfiguration configuration, IWebHostEnvironment env, DeputadoRepository deputadoRepository)
        {
            Environment = env;
            Configuration = configuration;
            _deputadoRepository = deputadoRepository;
        }

        [HttpGet("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            return await _deputadoRepository.Consultar(id);
        }

        [HttpPost("Lista")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            return await _deputadoRepository.Lista(filtro);
        }

        [HttpPost("Pesquisa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            return await _deputadoRepository.Pesquisa(filtro);
        }

        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await _deputadoRepository.Lancamentos(request);
        }

        [HttpGet("TipoDespesa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            return await _deputadoRepository.TipoDespesa();
        }

        [HttpPost("FuncionarioPesquisa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> FuncionarioPesquisa(MultiSelectRequest filtro)
        {
            return await _deputadoRepository.FuncionarioPesquisa(filtro);
        }

        [HttpPost("Funcionarios")]
        public async Task<dynamic> Funcionarios(DataTablesRequest request)
        {
            return await _deputadoRepository.Funcionarios(request);
        }

        [HttpPost("{id:int}/FuncionariosAtivos")]
        public async Task<dynamic> FuncionariosAtivosPorDeputado(int id, DataTablesRequest request)
        {
            return await _deputadoRepository.FuncionariosAtivosPorDeputado(id, request);
        }

        [HttpPost("{id:int}/FuncionariosHistorico")]
        public async Task<dynamic> FuncionariosPorDeputado(int id, DataTablesRequest request)
        {
            return await _deputadoRepository.FuncionariosPorDeputado(id, request);
        }

        [HttpGet("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            return await _deputadoRepository.GastosPorAno(id);
        }

        [HttpGet("{id:int}/GastosComPessoalPorAno")]
        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            return await _deputadoRepository.GastosComPessoalPorAno(id);
        }

        [HttpGet("Documento/{id:int}")]
        public async Task<ActionResult<DocumentoDetalheDTO>> Documento(int id)
        {
            var result = await _deputadoRepository.Documento(id);

            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }

        [HttpGet("{id:int}/DocumentosDoMesmoDia")]
        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            var result = await _deputadoRepository.DocumentosDoMesmoDia(id);

            return Ok(result);
        }

        [HttpGet("{id:int}/DocumentosDaSubcotaMes")]
        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            var result = await _deputadoRepository.DocumentosDaSubcotaMes(id);

            return Ok(result);
        }

        [HttpGet("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            return await _deputadoRepository.MaioresNotas(id);
        }

        [HttpGet("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            return await _deputadoRepository.MaioresFornecedores(id);
        }

        [HttpGet("{id:int}/ResumoPresenca")]
        public async Task<dynamic> ResumoPresenca(int id)
        {
            return await _deputadoRepository.ResumoPresenca(id);
        }

        [HttpGet("CamaraResumoMensal")]
        public async Task<dynamic> CamaraResumoMensal()
        {
            return await _deputadoRepository.CamaraResumoMensal();
        }

        [HttpGet("CamaraResumoAnual")]
        public async Task<dynamic> CamaraResumoAnual()
        {
            return await _deputadoRepository.CamaraResumoAnual();
        }

        [HttpPost("Frequencia/{id:int}")]
        public async Task<dynamic> Frequencia(int id, DataTablesRequest request)
        {
            return await _deputadoRepository.Frequencia(id, request);
        }

        [HttpPost("Frequencia")]
        public async Task<dynamic> Frequencia(DataTablesRequest request)
        {
            return await _deputadoRepository.Frequencia(request);
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

        [HttpGet]
        [Route("GrupoFuncional")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> GrupoFuncional()
        {
            return await _deputadoRepository.GrupoFuncional();
        }

        [HttpGet]
        [Route("Cargo")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Cargo()
        {
            return await _deputadoRepository.Cargo();
        }

        [HttpPost]
        [Route("Remuneracao")]
        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            return await _deputadoRepository.Remuneracao(request);
        }

        [HttpGet]
        [Route("Remuneracao/{id:int}")]
        public async Task<dynamic> Remuneracao(int id)
        {
            return await _deputadoRepository.Remuneracao(id);
        }

        [HttpGet]
        [Route("{id:int}/CustoAnual")]
        public async Task<dynamic> CustoAnual(int id)
        {
            return await _deputadoRepository.CustoAnual(id);
        }
    }
}
