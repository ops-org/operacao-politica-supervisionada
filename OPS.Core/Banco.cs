using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace OPS.Core
{
	public class Banco : IDisposable
	{
		private bool _mBeginTransaction;
		private MySqlConnection _mConnection;

		private List<MySqlParameter> _mParametros;
		private MySqlTransaction _mTransaction;

		public Banco()
		{
			Initializer(Padrao.ConnectionString);
		}

		public Banco(string connectionString)
		{
			Initializer(connectionString);
		}

		public long Rows { get; set; }
		public long LastInsertedId { get; set; }

		public void Dispose()
		{
			try
			{
				if (_mBeginTransaction)
					_mTransaction.Rollback();
			}
			catch
			{
				// ignored
			}

			_mConnection.Close();
			_mConnection.Dispose();
			_mConnection = null;
		}

		private void Initializer(string connectionString)
		{
			_mParametros = new List<MySqlParameter>();
			_mConnection = new MySqlConnection(connectionString);
			_mConnection.Open();

			//ExecuteNonQuery("set @@session.time_zone = '-02:00'"); //Horário de verão
			//ExecuteNonQuery("set @@session.time_zone = '-03:00'");

			_mBeginTransaction = false;
		}

		public bool ExecuteNonQuery(string sql, int timeOut = 60)
		{
			using (var command = _mConnection.CreateCommand())
			{
				if (_mBeginTransaction)
					command.Transaction = _mTransaction;

				if (_mParametros.Count > 0)
				{
					command.Parameters.AddRange(_mParametros.ToArray());
					_mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;


				Rows = command.ExecuteNonQuery();

				LastInsertedId = command.LastInsertedId;
			}

			return true;
		}

		public object ExecuteScalar(string sql, int timeOut = 60)
		{
			object retorno;

			using (var command = _mConnection.CreateCommand())
			{
				if (_mBeginTransaction)
					command.Transaction = _mTransaction;

				if (_mParametros.Count > 0)
				{
					command.Parameters.AddRange(_mParametros.ToArray());
					_mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				retorno = command.ExecuteScalar();
			}

			return retorno;
		}

		public MySqlDataReader ExecuteReader(string sql, int timeOut = 60)
		{
			MySqlDataReader reader;

			using (var command = _mConnection.CreateCommand())
			{
				if (_mBeginTransaction)
					command.Transaction = _mTransaction;

				if (_mParametros.Count > 0)
				{
					command.Parameters.AddRange(_mParametros.ToArray());
					_mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				reader = command.ExecuteReader();
			}

			return reader;
		}

		public async Task<DbDataReader> ExecuteReaderAsync(string sql, int timeOut = 60)
		{
			using (var command = _mConnection.CreateCommand())
			{
				if (_mBeginTransaction)
					command.Transaction = _mTransaction;

				if (_mParametros.Count > 0)
				{
					command.Parameters.AddRange(_mParametros.ToArray());
					_mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				return await command.ExecuteReaderAsync();
			}
		}

		public DataTable GetTable(string sql, int timeOut = 60)
		{
			DataTable table;

			using (var command = _mConnection.CreateCommand())
			{
				if (_mBeginTransaction)
					command.Transaction = _mTransaction;

				if (_mParametros.Count > 0)
				{
					command.Parameters.AddRange(_mParametros.ToArray());
					_mParametros.Clear();
				}

				command.CommandText = sql;
				command.CommandTimeout = timeOut;

				using (var adaper = new MySqlDataAdapter(command))
				{
					using (var data = new DataSet())
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
			_mBeginTransaction = true;
			_mTransaction = _mConnection.BeginTransaction();
		}

		public void CommitTransaction()
		{
			_mBeginTransaction = false;

			try
			{
				_mTransaction.Commit();
			}
			catch
			{
				_mTransaction.Rollback();
			}
		}

		public void RollBackTransaction()
		{
			_mBeginTransaction = false;
			_mTransaction.Rollback();
		}

		public void AddParameter(string name, object value)
		{
			_mParametros.Add(new MySqlParameter(name, value));
		}

		public string ParametersHash()
		{
			var lst = _mParametros.Select(x => x.Value?.ToString() ?? string.Empty);
			var str = string.Join(",", lst);

			return Utils.Hash(str);
		}

		public void ClearParameters()
		{
			_mParametros.Clear();
		}
	}
}