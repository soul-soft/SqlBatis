using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace SqlBatis
{
    public class DapperDbMultipleResult : IDbGridReader
    {
        readonly SqlMapper.GridReader _reader;

        public DapperDbMultipleResult(SqlMapper.GridReader reader)
        {
            _reader = reader;
        }
        public void Dispose()
        {
            _reader?.Dispose();
            GC.SuppressFinalize(this);
        }

        public object Get()
        {
            return _reader.Read(false);
        }

        public T Get<T>()
        {
            return _reader.ReadFirst<T>();
        }

        public Task<object> GetAsync()
        {
            return _reader.ReadFirstAsync();
        }

        public Task<T> GetAsync<T>()
        {
            return _reader.ReadFirstAsync<T>();
        }

        public List<dynamic> GetList()
        {
            return _reader.Read(false).AsList();
        }

        public List<T> GetList<T>()
        {
            return _reader.Read<T>(false).AsList();
        }

        public async Task<List<dynamic>> GetListAsync()
        {
            var list = await _reader.ReadAsync(false);
            return list.AsList();
        }

        public async Task<List<T>> GetListAsync<T>()
        {
            var list = await _reader.ReadAsync<T>(false);
            return list.AsList();
        }
    }
}
