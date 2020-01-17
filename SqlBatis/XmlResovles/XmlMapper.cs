using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    public interface IXmlMapper
    {
        IMultiResult ExecuteMultiQuery(int? commandTimeout = null, CommandType? commandType = null);
        IEnumerable<dynamic> ExecuteQuery(int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<dynamic>> ExecuteQueryAsync(int? commandTimeout = null, CommandType? commandType = null);
        IEnumerable<T> ExecuteQuery<T>(int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(int? commandTimeout = null, CommandType? commandType = null);
        int ExecuteNonQuery(int? commandTimeout = null, CommandType? commandType = null);
        Task<int> ExecuteNonQueryAsync(int? commandTimeout = null, CommandType? commandType = null);
        T ExecuteScalar<T>(int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(int? commandTimeout = null, CommandType? commandType = null);
    }
    internal class XmlMapper : IXmlMapper
    {
        private readonly string _sql = null;

        private readonly object _param = null;

        private readonly IDbContext _mapper = null;

        public XmlMapper(IDbContext mapper, string sql, object param = null)
        {
            _mapper = mapper;
            _sql = sql;
            _param = param;
        }

        public IMultiResult ExecuteMultiQuery(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteMultiQuery(_sql,_param,commandTimeout,commandType);
        }

        public int ExecuteNonQuery(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteNonQuery(_sql, _param, commandTimeout, commandType);
        }

        public Task<int> ExecuteNonQueryAsync(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteNonQueryAsync(_sql, _param, commandTimeout, commandType);
        }

        public IEnumerable<T> ExecuteQuery<T>(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteQuery<T>(_sql, _param, commandTimeout, commandType);
        }

        public IEnumerable<dynamic> ExecuteQuery(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteQuery(_sql, _param, commandTimeout, commandType);
        }

        public Task<IEnumerable<T>> ExecuteQueryAsync<T>(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteQueryAsync<T>(_sql, _param, commandTimeout, commandType);
        }

        public Task<IEnumerable<dynamic>> ExecuteQueryAsync(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteQueryAsync(_sql, _param, commandTimeout, commandType);
        }

        public T ExecuteScalar<T>(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteScalar<T>(_sql, _param, commandTimeout, commandType);
        }

        public Task<T> ExecuteScalarAsync<T>(int? commandTimeout = null, CommandType? commandType = null)
        {
            return _mapper.ExecuteScalarAsync<T>(_sql, _param, commandTimeout, commandType);
        }
    }
}
