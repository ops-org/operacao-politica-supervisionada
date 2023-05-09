﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using OPS.Core.DTO;

namespace OPS.Core.DAO
{
    public class EstadoRepository: BaseRepository
    {
        public EstadoRepository(IDbConnection connection) : base(connection)
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