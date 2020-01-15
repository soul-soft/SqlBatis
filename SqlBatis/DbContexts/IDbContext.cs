using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    public interface IDbContext
    {
        IXmlMapper From<T>(string id, T parameter);
        IXmlMapper From(string id);
        void BeginTransaction();
        void BeginTransaction(IsolationLevel level);
        void Close();
        void CommitTransaction();
        IEnumerable<T> ExecuteQuery<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
        int ExecuteNonQuery(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<int> ExecuteNonQueryAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
        T ExecuteScalar<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null);
        void Open();
        Task OpenAsync();
        void RollbackTransaction();

    }
}
