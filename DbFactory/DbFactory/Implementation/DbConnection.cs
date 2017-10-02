using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data.Sql;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;

namespace DbFactory.Implementation
{
    /// <summary>
    /// Implements IDbConnection
    /// </summary>
    public class DbConnection : IDbConnection, IDisposable
    {
        private string connectionString;

        private IDbCommand cmd = new SqlCommand();

        private SqlConnection connection;

        private SqlTransaction transaction;

        public bool isTransaction = false;

        /// <summary>
        /// Get the connection string
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        private string GetConnectionString(string connectionStringName)
        {
            try
            {
                string name = String.Empty;
                string provider = String.Empty;
                string connectionString = String.Empty;

                ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;

                foreach (ConnectionStringSettings connection in connections)
                {
                    name = connection.Name;
                    provider = connection.ProviderName;
                    connectionString = connection.ConnectionString;

                    if (!String.IsNullOrEmpty(connectionString) && !String.IsNullOrEmpty(name) && name.ToLower().Equals(connectionStringName.ToLower()))
                    {
                        return connectionString;
                    }
                }
                return String.Empty;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Modifified constructor (passing the connection string)
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="IsTransaction"></param>
        public DbConnection(string ConnectionString, bool IsTransaction = false)
        {
            try
            {
                this.connectionString = GetConnectionString(ConnectionString);
                this.connection = new SqlConnection(connectionString);
                cmd.Connection = this.connection;

                this.Open();

                if (IsTransaction)
                {
                    this.transaction = this.connection.BeginTransaction();
                    cmd.Transaction = transaction;
                    isTransaction = true;
                }
            }
            catch
            {
                this.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute SQL Query (return INT)
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            try
            {
                return Convert.ToInt32(ExecuteSqlServerCommand((int)ReturnTypeEnum.Integer));
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Execute SQL Query (return INT, TABLE IDENTITY)
        /// </summary>
        /// <returns></returns>
        public int ExecuteScalar()
        {
            try
            {
                return Convert.ToInt32(ExecuteSqlServerCommand((int)ReturnTypeEnum.Scalar));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Execute SQL Query (return an IDataReader object 
        /// </summary>
        /// <returns></returns>
        public IDataReader ExecuteReader()
        {
            try
            {
                return (IDataReader)ExecuteSqlServerCommand((int)ReturnTypeEnum.Reader);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Execute SQL Query (return an DataSet object)
        /// </summary>
        /// <returns></returns>
        public DataSet GetDataSet()
        {
            try
            {
                return (DataSet)ExecuteSqlServerCommand((int)ReturnTypeEnum.DataSet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Execute SQL Query (return an DataTable object)
        /// </summary>
        /// <returns></returns>
        public DataTable GetDataTable()
        {
            try
            {
                return (DataTable)ExecuteSqlServerCommand((int)ReturnTypeEnum.DataTable);
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// Execute SQL Query (return an Object)
        /// </summary>
        /// <param name="ReturnType"></param>
        /// <returns></returns>
        private object ExecuteSqlServerCommand(int ReturnType)
        {
            try
            {
                this.Open();

                object retorno;

                if (ReturnType == (int)ReturnTypeEnum.DataTable)
                {
                    DataTable table = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = (SqlCommand)cmd;
                    adapter.Fill(table);

                    retorno = table;
                }
                else if (ReturnType == (int)ReturnTypeEnum.DataSet)
                {
                    DataSet set = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    adapter.SelectCommand = (SqlCommand)cmd;
                    adapter.Fill(set);
                    retorno = set;
                }
                else if (ReturnType == (int)ReturnTypeEnum.Scalar)
                {
                    retorno = Convert.ToInt32(cmd.ExecuteScalar());
                }
                else if (ReturnType == (int)ReturnTypeEnum.Reader)
                {
                    retorno = cmd.ExecuteReader();
                }
                else
                {
                    retorno = cmd.ExecuteNonQuery();
                }

                return retorno;
            }
            catch 
            {
                throw;
            }
            finally
            {
                if (!isTransaction)
                    this.Dispose();
            }
        }

        /// <summary>
        //     Gets or sets the text command to run against the data source.
        /// </summary>
        public string CommandText
        {
            get
            {
                return cmd.CommandText;
            }
            set
            {
                cmd.CommandText = value;
            }
        }

        /// <summary>
        //     Specifies how a command string is interpreted.
        /// </summary>
        public CommandType CommandType
        {
            get
            {
                return cmd.CommandType;
            }
            set
            {
                cmd.CommandType = value;
            }
        }

        /// <summary>
        /// Open database connection
        /// </summary>
        private void Open()
        {
            try
            {
                if (connection.State.Equals(ConnectionState.Closed) || connection.State.Equals(ConnectionState.Broken))
                {
                    if (String.IsNullOrEmpty(connection.ConnectionString))
                        connection.ConnectionString = this.connectionString;

                    connection.Open();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        private void Close()
        {
            try
            {
                if (!connection.State.Equals(ConnectionState.Closed)) {
                    connection.Close();
                    connection.Dispose();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Commit the Transaction
        /// </summary>
        public void DbTransCommit()
        {
            if (!(this.transaction == null))
                this.transaction.Commit();
        }

        /// <summary>
        /// Rollback the Transaction
        /// </summary>
        public void DbTransRollback()
        {
            if (!(this.transaction == null))
                this.transaction.Rollback();
        }

        /// <summary>
        /// Dispose the SqlTransaction Object
        /// </summary>
        private void DbTransDispose()
        {
            if (transaction != null)
                transaction.Dispose();
        }

        /// <summary>
        /// Add a SQL Parameter
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="ParamValue"></param>
        public void AddParameter(string ParamName, object ParamValue)
        {
            cmd.Parameters.Add(new SqlParameter(ParamName, ParamValue));
        }

        /// <summary>
        /// Add a SQL Parameter
        /// </summary>
        /// <param name="sqlParameter"></param>
        public void AddParameter(SqlParameter sqlParameter)
        {
            cmd.Parameters.Add(sqlParameter);
        }

        /// <summary>
        /// Add a range SQL Parameters
        /// </summary>
        /// <param name="sqlParameter"></param>
        public void AddParameters(SqlParameter[] sqlParameter)
        {
            ClearParameters();
            foreach (SqlParameter sqlparam in sqlParameter)
            {
                cmd.Parameters.Add(sqlparam);
            }
        }

        /// <summary>
        /// Clear command parameters
        /// </summary>
        public void ClearParameters()
        {
            cmd.Parameters.Clear();
        }

        /// <summary>
        /// Dispose All objects
        /// </summary>
        public void Dispose()
        {
            this.DbTransDispose();
            this.Close();
        }

    }
}
