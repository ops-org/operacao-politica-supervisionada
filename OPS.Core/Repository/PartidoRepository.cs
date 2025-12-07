using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using OPS.Core.DTO;

namespace OPS.Core.Repository
{
    public class PartidoRepository : BaseRepository
    {
        public PartidoRepository(IDbConnection connection) : base(connection)
        {
        }

        public async Task<IEnumerable<DropDownDTO>> Consultar()
        {
            var sql = "SELECT id, nome as text, concat(sigla, ' - ', legenda) as helpText, concat('/img/partidos/', imagem) as image FROM partido order by nome;";
            IEnumerable<DropDownDTO> lista = await _connection.QueryAsync<DropDownDTO>(sql);

            return lista;
        }
    }
}