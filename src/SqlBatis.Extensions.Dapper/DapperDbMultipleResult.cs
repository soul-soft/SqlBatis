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

        public object ReadFirst()
        {
            return _reader.ReadFirst();
        }

        public T ReadFirst<T>()
        {
            return _reader.ReadFirst<T>();
        }

        public Task<object> ReadFirstAsync()
        {
            return _reader.ReadFirstAsync();
        }

        public Task<T> ReadFirstAsync<T>()
        {
            return _reader.ReadFirstAsync<T>();
        }

        public List<dynamic> Read()
        {
            return _reader.Read(false).AsList();
        }

        public List<T> Read<T>()
        {
            return _reader.Read<T>(false).AsList();
        }

        public async Task<List<dynamic>> ReadAsync()
        {
            var list = await _reader.ReadAsync(false);
            return list.AsList();
        }

        public async Task<List<T>> ReadAsync<T>()
        {
            var list = await _reader.ReadAsync<T>(false);
            return list.AsList();
        }
    }
}
