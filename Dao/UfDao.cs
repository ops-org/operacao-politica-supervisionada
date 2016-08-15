using MySql.Data.MySqlClient;
using OPS.Core;
using System.Collections.Generic;
using System.Text;

namespace OPS.Dao
{
	public class UfDao
	{
		//internal dynamic Pesquisa(FiltroDropDownDTO filtro)
		//{
		//	using (Banco banco = new Banco())
		//	{
		//		var strSql = new StringBuilder();
		//		strSql.Append("SELECT SQL_CALC_FOUND_ROWS ideCadastro, txNomeParlamentar FROM parlamentares ");
		//		if (!string.IsNullOrEmpty(filtro.q))
		//		{
		//			strSql.AppendFormat("WHERE txNomeParlamentar LIKE @q ", filtro.q);
		//			banco.AddParameter("@q", "%" + filtro.q + "%");
		//		}
		//		strSql.AppendFormat("ORDER BY txNomeParlamentar ");
		//		strSql.AppendFormat("LIMIT {0},{1}; ", ((filtro.page ?? 1) - 1) * filtro.count, filtro.count);

		//		strSql.Append("SELECT FOUND_ROWS(); ");

		//		var lstRetorno = new List<dynamic>();
		//		using (MySqlDataReader reader = banco.ExecuteReader(strSql.ToString()))
		//		{
		//			while (reader.Read())
		//			{
		//				lstRetorno.Add(new
		//				{
		//					id = reader[0].ToString(),
		//					text = reader[1].ToString(),
		//				});
		//			}

		//			reader.NextResult();
		//			reader.Read();

		//			return new
		//			{
		//				total_count = reader[0],
		//				results = lstRetorno
		//			};
		//		}
		//	}
		//}
	}
}