using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace OPS.Core
{
    public class AppDb : IDisposable
    {
        private bool _mBeginTransaction;
        private MySqlConnection _mConnection;
        private MySqlCommand _mCommand;

        private List<MySqlParameter> _mParametros;
        private MySqlTransaction _mTransaction;

        public AppDb()
        {
            Initializer(Padrao.ConnectionString);
        }

        public AppDb(string connectionString)
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
            if (_mCommand == null)
                _mCommand = _mConnection.CreateCommand();
            else
                _mCommand.Parameters.Clear();

            if (_mBeginTransaction)
                _mCommand.Transaction = _mTransaction;

            if (_mParametros.Count > 0)
            {
                _mCommand.Parameters.AddRange(_mParametros.ToArray());
                _mParametros.Clear();
            }

            _mCommand.CommandText = sql;
            _mCommand.CommandTimeout = timeOut;

            Rows = _mCommand.ExecuteNonQuery();

            LastInsertedId = _mCommand.LastInsertedId;

            return true;
        }

        public object ExecuteScalar(string sql, int timeOut = 600)
        {
            if (_mCommand == null)
                _mCommand = _mConnection.CreateCommand();
            else
                _mCommand.Parameters.Clear();

            if (_mBeginTransaction)
                _mCommand.Transaction = _mTransaction;

            if (_mParametros.Count > 0)
            {
                _mCommand.Parameters.AddRange(_mParametros.ToArray());
                _mParametros.Clear();
            }

            _mCommand.CommandText = sql;
            _mCommand.CommandTimeout = timeOut;

            return _mCommand.ExecuteScalar();
        }

        public MySqlDataReader ExecuteReader(string sql, int timeOut = 600)
        {
            if (_mCommand == null)
                _mCommand = _mConnection.CreateCommand();
            else
                _mCommand.Parameters.Clear();

            if (_mBeginTransaction)
                _mCommand.Transaction = _mTransaction;

            if (_mParametros.Count > 0)
            {
                _mCommand.Parameters.AddRange(_mParametros.ToArray());
                _mParametros.Clear();
            }

            _mCommand.CommandText = sql;
            _mCommand.CommandTimeout = timeOut;

            return _mCommand.ExecuteReader();
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string sql, int timeOut = 600)
        {
            if (_mCommand == null)
                _mCommand = _mConnection.CreateCommand();
            else
                _mCommand.Parameters.Clear();

            if (_mBeginTransaction)
                _mCommand.Transaction = _mTransaction;

            if (_mParametros.Count > 0)
            {
                _mCommand.Parameters.AddRange(_mParametros.ToArray());
                _mParametros.Clear();
            }

            _mCommand.CommandText = sql;
            _mCommand.CommandTimeout = timeOut;

            return await _mCommand.ExecuteReaderAsync();
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="data"></param>
        /// <returns>Number of rows affected</returns>
        public List<Dictionary<string, object>> ExecuteDict(string sql, int timeOut = 600)
        {
            if (_mCommand == null)
                _mCommand = _mConnection.CreateCommand();
            else
                _mCommand.Parameters.Clear();

            if (_mBeginTransaction)
                _mCommand.Transaction = _mTransaction;

            if (_mParametros.Count > 0)
            {
                _mCommand.Parameters.AddRange(_mParametros.ToArray());
                _mParametros.Clear();
            }

            _mCommand.CommandText = sql;
            _mCommand.CommandTimeout = timeOut;

            // Execute the query
            List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>();
            using (DbDataReader reader = _mCommand.ExecuteReader())
            {

                // If we have rows, add them to the list
                if (reader.HasRows)
                {
                    // Add each row to the rows list
                    while (reader.Read())
                    {
                        Dictionary<string, object> Row = new Dictionary<string, object>(reader.FieldCount);
                        for (int i = 0; i < reader.FieldCount; ++i)
                            Row.Add(reader.GetName(i), reader.GetValue(i));

                        Rows.Add(Row);
                    }
                }
            }

            // Return Rows
            return Rows;
        }

        public DataTable GetTable(string sql, int timeOut = 600)
        {
            DataTable table;

            if (_mCommand == null)
                _mCommand = _mConnection.CreateCommand();
            else
                _mCommand.Parameters.Clear();

            if (_mBeginTransaction)
                _mCommand.Transaction = _mTransaction;

            if (_mParametros.Count > 0)
            {
                _mCommand.Parameters.AddRange(_mParametros.ToArray());
                _mParametros.Clear();
            }

            _mCommand.CommandText = sql;
            _mCommand.CommandTimeout = timeOut;

            using (var adaper = new MySqlDataAdapter(_mCommand))
            {
                using (var data = new DataSet())
                {
                    data.EnforceConstraints = false;
                    adaper.Fill(data);
                    table = data.Tables[0];
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
            catch (Exception)
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

        public void ResetConnection()
        {
            _mCommand = null;
            _mConnection = new MySqlConnection(Padrao.ConnectionString);
            _mConnection.Open();

            _mBeginTransaction = false;
        }
    }
}