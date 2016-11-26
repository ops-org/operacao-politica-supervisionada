using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Core
{
	public class Banco : IDisposable
	{
		private Boolean mBeginTransaction;
		private MySqlConnection mConnection;
		private MySqlTransaction mTransaction;

		private List<MySqlParameter> mParametros;

		public Banco()
		{
			//MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();
			//conn_string.Server = "mysql873.umbler.com";
			//conn_string.Port = 41890;
			//conn_string.UserID = "ops-auditoria";
			//conn_string.Password = "W5dv2nU*{W";
			//conn_string.Database = "ops-auditoria";

			//var x = conn_string.ToString();

			mParametros = new List<MySqlParameter>();
			mConnection = new MySqlConnection(Padrao.ConnectionString);
			mConnection.Open();

			//ExecuteNonQuery("set @@session.time_zone = '-02:00'"); //Horário de verão
			//ExecuteNonQuery("set @@session.time_zone = '-03:00'");

			mBeginTransaction = false;
		}

		public Int64 Rows { get; set; }
		public Int64 LastInsertedId { get; set; }

		public Boolean ExecuteNonQuery(String sql, Int32 timeOut = 60)
		{
			using (MySqlCommand command = mConnection.CreateCommand())
			{
				if (mBeginTransaction)
					command.Transaction = mTransaction;

				if (mParametros.Count > 0)
				{
					command.Parameters.AddRange(mParametros.ToArray());
					mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				Rows = command.ExecuteNonQuery();

				LastInsertedId = command.LastInsertedId;
			}

			return true;
		}

		public Object ExecuteScalar(String sql, Int32 timeOut = 60)
		{
			Object retorno = null;

			using (MySqlCommand command = mConnection.CreateCommand())
			{
				if (mBeginTransaction)
					command.Transaction = mTransaction;

				if (mParametros.Count > 0)
				{
					command.Parameters.AddRange(mParametros.ToArray());
					mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

                retorno = command.ExecuteScalar();
			}

			return retorno;
		}

		public MySqlDataReader ExecuteReader(String sql, Int32 timeOut = 60)
		{
			MySqlDataReader reader;

			using (MySqlCommand command = mConnection.CreateCommand())
			{
				if (mBeginTransaction)
					command.Transaction = mTransaction;

				if (mParametros.Count > 0)
				{
					command.Parameters.AddRange(mParametros.ToArray());
					mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				reader = command.ExecuteReader();
			}

			return reader;
		}

		public DataTable GetTable(String sql, Int32 timeOut = 60)
		{
			DataTable table = null;

			using (MySqlCommand command = mConnection.CreateCommand())
			{
				if (mBeginTransaction)
					command.Transaction = mTransaction;

				if (mParametros.Count > 0)
				{
					command.Parameters.AddRange(mParametros.ToArray());
					mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				using (MySqlDataAdapter adaper = new MySqlDataAdapter(command))
				{
					using (DataSet data = new DataSet())
					{
						data.EnforceConstraints = false;
						adaper.Fill(data);
						table = data.Tables[0];
					}
				}
			}

			return table;
		}

		public void BeginTransaction()
		{
			mBeginTransaction = true;
			mTransaction = mConnection.BeginTransaction();
		}

		public void CommitTransaction()
		{
			mBeginTransaction = false;

			try
			{
				mTransaction.Commit();
			}
			catch
			{
				mTransaction.Rollback();
			}
		}

		public void RollBackTransaction()
		{
			mBeginTransaction = false;
			mTransaction.Rollback();
		}

		public void AddParameter(String name, Object value)
		{
			mParametros.Add(new MySqlParameter(name, value));
		}

		public void Dispose()
		{
			try
			{
				if (mBeginTransaction)
				{
					mTransaction.Rollback();
				}
			}
			catch { }

			mConnection.Close();
			mConnection.Dispose();
			mConnection = null;
		}
	}
}
