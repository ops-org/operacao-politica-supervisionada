using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace OPS.Core.DAO
{
    public class EstadoDao
    {
        public async Task<dynamic> Consultar()
        {
            using (AppDb banco = new AppDb())
            {
                var lstRetorno = new List<dynamic>();

                DbDataReader reader = await banco.ExecuteReaderAsync("SELECT id, sigla, nome FROM estado order by nome;");
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new
                    {
                        id = reader.GetFieldValue<uint>(0),
                        tokens = new[] { reader.GetFieldValue<string>(1) },
                        text = string.Format("{0} ({1})", reader.GetFieldValue<string>(2), reader.GetFieldValue<string>(1))
                    });
                }
                reader.Close();

                return lstRetorno;
            }
        }
    }
}