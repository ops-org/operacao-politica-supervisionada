using Dapper;
using OPS.Core.DTO;
using Serilog;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OPS.Core.DAO
{
    public class EstadoRepository: BaseRepository
    {
        public EstadoRepository(ILogger logger, IDbConnection connection) : base(logger, connection)
        {
        }

        public async Task<IEnumerable<DropDownDTO>> Consultar()
        {
            var sql = "SELECT id, concat(nome, ' (', sigla, ')') as text FROM estado order by nome;";
            IEnumerable<DropDownDTO> lista = await _connection.QueryAsync<DropDownDTO>(sql);

            return lista;
        }
    }
}