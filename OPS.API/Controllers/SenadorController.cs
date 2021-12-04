using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core;
using OPS.Core.DAO;
using OPS.Core.DTO;
using System.Threading.Tasks;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[CacheOutput(ServerTimeSpan = 43200 /* 12h */)]
    public class SenadorController : Controller
    {

        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }

        SenadorDao dao;

        public SenadorController(IConfiguration configuration, IWebHostEnvironment env)
        {
            Environment = env;
            Configuration = configuration;

            dao = new SenadorDao();
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<dynamic> Consultar(int id)
        {
            return await dao.Consultar(id);
        }

        [HttpPost("Lista")]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Lista(FiltroParlamentarDTO filtro)
        {
            return await dao.Lista(filtro);
        }


        [HttpPost("Pesquisa")]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro)
        {
            return await dao.Pesquisa(filtro);
        }

        [HttpPost]
        [Route("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await dao.Lancamentos(request);
        }

        [HttpGet]
        [Route("TipoDespesa")]
        //[CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            return await dao.TipoDespesa();
        }

        [HttpGet]
        [Route("{id:int}/GastosPorAno")]
        public async Task<dynamic> GastosPorAno(int id)
        {
            return await dao.GastosPorAno(id);
        }

        //[HttpGet]
        //[Route("Documento/{id:int}")]
        //public async Task<dynamic> Documento(int id)
        //{
        //	return await dao.Documento(id);
        //}

        [HttpGet]
        [Route("{id:int}/MaioresNotas")]
        public async Task<dynamic> MaioresNotas(int id)
        {
            return await dao.MaioresNotas(id);
        }

        [HttpGet]
        [Route("{id:int}/MaioresFornecedores")]
        public async Task<dynamic> MaioresFornecedores(int id)
        {
            return await dao.MaioresFornecedores(id);
        }

        [HttpGet]
        [Route("SenadoResumoMensal")]
        public async Task<dynamic> SenadoResumoMensal()
        {
            return await dao.SenadoResumoMensal();
        }

        [HttpGet]
        [Route("SenadoResumoAnual")]
        public async Task<dynamic> SenadoResumoAnual()
        {
            return await dao.SenadoResumoAnual();
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
            return await dao.Lotacao();
        }

        [HttpGet]
        [Route("Cargo")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Cargo()
        {
            return await dao.Cargo();
        }

        [HttpGet]
        [Route("Categoria")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Categoria()
        {
            return await dao.Categoria();
        }

        [HttpGet]
        [Route("Vinculo")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Vinculo()
        {
            return await dao.Vinculo();
        }

        [HttpPost]
        [Route("Remuneracao")]
        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            return await dao.Remuneracao(request);
        }

        [HttpGet]
        [Route("Remuneracao/{id:int}")]
        public async Task<dynamic> Remuneracao(int id)
        {
            return await dao.Remuneracao(id);
        }
    }
}
