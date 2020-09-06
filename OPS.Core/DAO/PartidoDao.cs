using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace OPS.Core.DAO
{
	public class PartidoDao
	{
		public async Task<dynamic> Consultar()
		{
			using (AppDb banco = new AppDb())
			{
				var lstRetorno = new List<dynamic>();

				DbDataReader reader = await banco.ExecuteReaderAsync("SELECT id, sigla, nome FROM partido order by nome;");
			    while (await reader.ReadAsync())
                {
					lstRetorno.Add(new
					{
						id = reader.GetFieldValue<uint>(0),
						tokens = new[] { reader.GetFieldValue<string>(1), reader.GetFieldValue<string>(2) },
						text = reader.GetFieldValue<string>(2)
					});
				}
			    reader.Close();

                return lstRetorno;
			}
		}
	}
}