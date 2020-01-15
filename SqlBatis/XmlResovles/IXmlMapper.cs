using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    public interface IXmlMapper
    {
        IEnumerable<T> ExecuteQuery<T>(int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(int? commandTimeout = null, CommandType? commandType = null);
        int ExecuteNonQuery(int? commandTimeout = null, CommandType? commandType = null);
        Task<int> ExecuteNonQueryAsync(int? commandTimeout = null, CommandType? commandType = null);
        T ExecuteScalar<T>(int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(int? commandTimeout = null, CommandType? commandType = null);
    }
}
