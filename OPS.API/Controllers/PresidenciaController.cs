using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPS.Core;
using OPS.Core.Repository;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PresidenciaController : Controller
    {
        PresidenciaRepository dao;
        public PresidenciaController()
        {
            dao = new PresidenciaRepository();
        }


        [HttpPost("Lancamentos")]
        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            return await dao.Lancamentos(request);
        }

        [HttpGet("TipoDespesa")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> TipoDespesa()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM pr_despesa_tipo ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id = reader["id"].ToString(),
                            text = reader["descricao"].ToString(),
                        });
                    }
                }
                return lstRetorno;
            }
        }

        [HttpPost("Servidor")]
        // [CacheOutput(ClientTimeSpan = 43200 /* 12h */, ServerTimeSpan = 43200 /* 12h */)]
        public async Task<dynamic> Servidor(MultiSelectRequest filtro)
        {
            return await dao.Servidor(filtro);
        }

    }
}
