using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SqlBatis
{
    public interface IMultiResult : IDisposable
    {
        List<dynamic> GetList();
        Task<List<dynamic>> GetListAsync();
        List<T> GetList<T>();
        Task<List<T>> GetListAsync<T>();
        dynamic Get();
        Task<dynamic> GetAsync();
        T Get<T>();
        Task<T> GetAsync<T>();
    }

    public class MultiResult : IMultiResult
    {
        private readonly IDataReader _reader = null;
        private readonly IDbCommand _command = null;

        private readonly ITypeMapper _mapper = null;

        public MultiResult(IDbCommand command, ITypeMapper mapper)
        {
            _command = command;
            _reader = command.ExecuteReader();
            _mapper = mapper;
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _command?.Dispose();
        }

        public T Get<T>()
        {
            return GetList<T>().FirstOrDefault();
        }

        public async Task<T> GetAsync<T>()
        {
            return (await GetListAsync<T>()).FirstOrDefault();
        }

        public dynamic Get()
        {
            return GetList().FirstOrDefault();
        }
       
        public async Task<dynamic> GetAsync()
        {
            return (await GetListAsync()).FirstOrDefault();
        }
      
        public async Task<List<dynamic>> GetListAsync()
        {
            var handler = TypeConvert.GetSerializer();
            var list = new List<dynamic>();
            while (await (_reader as DbDataReader).ReadAsync())
            {
                list.Add(handler(_reader));
            }
            _reader.NextResult();
            return list;
        }
      
        public List<dynamic> GetList()
        {
            var handler = TypeConvert.GetSerializer();
            var list = new List<dynamic>();
            while (_reader.Read())
            {
                list.Add(handler(_reader));
            }
            _reader.NextResult();
            return list;
        }
      
        public List<T> GetList<T>()
        {
            var handler = TypeConvert.GetSerializer<T>(_mapper, _reader);
            var list = new List<T>();
            while (_reader.Read())
            {
                list.Add(handler(_reader));
            }
            _reader.NextResult();
            return list;
        }

        public async Task<List<T>> GetListAsync<T>()
        {
            var handler = TypeConvert.GetSerializer<T>(_mapper, _reader);
            var list = new List<T>();
            while (await (_reader as DbDataReader).ReadAsync())
            {
                list.Add(handler(_reader));
            }
            _reader.NextResult();
            return list;
        }
    }

}
