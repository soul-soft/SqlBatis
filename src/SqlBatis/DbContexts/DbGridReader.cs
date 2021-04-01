using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBatis
{
    /// <summary>
    /// DataReader多个结果集
    /// </summary>
    public interface IDbGridReader : IDisposable
    {
        /// <summary>
        /// 返回当前dynamic类型结果集
        /// </summary>
        /// <returns></returns>
        List<dynamic> Read();
        /// <summary>
        /// 异步返回当前dynamic类型结果集
        /// </summary>
        /// <returns></returns>
        Task<List<dynamic>> ReadAsync();
        /// <summary>
        /// 返回当前T结果集
        /// </summary>
        /// <typeparam name="T">结果集类型</typeparam>
        /// <returns></returns>
        List<T> Read<T>();
        /// <summary>
        ///  异步返回当前T类型结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> ReadAsync<T>();
        /// <summary>
        /// 返回当前dynamic类型结果
        /// </summary>
        /// <returns></returns>
        object ReadFirst();
        /// <summary>
        /// 异步返回当前dynamic类型结果
        /// </summary>
        /// <returns></returns>
        Task<object> ReadFirstAsync();
        /// <summary>
        /// 返回当前T类型结果
        /// </summary>
        /// <typeparam name="T">结果集类型</typeparam>
        /// <returns></returns>
        T ReadFirst<T>();
        /// <summary>
        /// 异步返回当前T类型结果
        /// </summary>
        /// <typeparam name="T">结果集类型</typeparam>
        /// <returns></returns>
        Task<T> ReadFirstAsync<T>();
    }

    internal class DbGridReader : IDbGridReader
    {
        private bool _disposed = false;
        private readonly IDataReader _reader = null;
        private readonly IDbCommand _command = null;
        ~DbGridReader()
        {
            Dispose();
        }
        internal DbGridReader(IDbCommand command)
        {
            _command = command;
            _reader = command.ExecuteReader();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            try { _reader?.Close(); } catch { }
            try {_reader?.Dispose(); } catch { }
            try {_command?.Dispose(); } catch { }
            GC.SuppressFinalize(this);
        }

        public T ReadFirst<T>()
        {
            return Read<T>().FirstOrDefault();
        }

        public async Task<T> ReadFirstAsync<T>()
        {
            return (await ReadAsync<T>()).FirstOrDefault();
        }

        public object ReadFirst()
        {
            return Read<object>().FirstOrDefault();
        }

        public async Task<object> ReadFirstAsync()
        {
            return (await ReadAsync<object>()).FirstOrDefault();
        }

        public async Task<List<dynamic>> ReadAsync()
        {
            var handler = SqlBatisSettings.DbEntityMapperProvider.GetEntityMapper();
            var list = new List<dynamic>();
            while (await (_reader as DbDataReader).ReadAsync())
            {
                list.Add(handler(_reader));
            }
            NextResult();
            return list;
        }

        public List<dynamic> Read()
        {
            var handler = SqlBatisSettings.DbEntityMapperProvider.GetEntityMapper();
            var list = new List<dynamic>();
            while (_reader.Read())
            {
                list.Add(handler(_reader));
            }
            NextResult();
            return list;
        }

        public List<T> Read<T>()
        {
            var handler = SqlBatisSettings.DbEntityMapperProvider.GetEntityMapper<T>(_reader);
            var list = new List<T>();
            while (_reader.Read())
            {
                list.Add(handler(_reader));
            }
            NextResult();
            return list;
        }

        public async Task<List<T>> ReadAsync<T>()
        {
            var handler = SqlBatisSettings.DbEntityMapperProvider.GetEntityMapper<T>(_reader);
            var list = new List<T>();
            while (await (_reader as DbDataReader).ReadAsync())
            {
                list.Add(handler(_reader));
            }
            NextResult();
            return list;
        }

        public void NextResult()
        {
            if (!_reader.NextResult())
            {
                Dispose();
            }
        }
    }
}

