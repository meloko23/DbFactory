using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace DbFactory
{
    public interface IDbConnection
    {
        void AddParameter(string ParamName, object ParamValue);
        void AddParameter(SqlParameter sqlParameter);
        void AddParameters(SqlParameter[] sqlParameter);
        void ClearParameters();
        string CommandText { get; set; }
        CommandType CommandType { get; set; }
        void DbTransCommit();
        void DbTransRollback();
        void Dispose();
        int ExecuteNonQuery();
        IDataReader ExecuteReader();
        int ExecuteScalar();
        DataSet GetDataSet();
        DataTable GetDataTable();
    }
}
