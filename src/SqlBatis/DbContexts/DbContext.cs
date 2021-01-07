using SqlBatis.Queryables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlBatis
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 是否是事物的
        /// </summary>
        bool IsTransactioned { get; }
        /// <summary>
        /// 数据库上下文状态
        /// </summary>
        DbContextState DbContextState { get; }
        /// <summary>
        /// 数据库上下文类型
        /// </summary>
        DbContextType DbContextType { get; }
        /// <summary>
        /// 开启事务会话
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// 开启事务会话
        /// </summary>
        /// <param name="level">事务隔离级别</param>
        void BeginTransaction(IsolationLevel level);
        /// <summary>
        /// 关闭连接和事务
        /// </summary>
        void Close();
        /// <summary>
        /// 提交当前事务会话
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// 执行多结果集查询，返回IMultiResult
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        IDbGridReader QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行单结果集查询，并返回dynamic类型的结果集
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行单结果集查询，并返回dynamic类型的结果集
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<List<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行单结果集查询，并返回T类型的结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行单结果集查询，并返回T类型的结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行无结果集查询，并返回受影响的行数
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        int Execute(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行无结果集查询，并返回受影响的行数
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行无结果集查询，并返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        T ExecuteScalar<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行无结果集查询，并返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<T> ExecuteScalarAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        void Open();
        /// <summary>
        /// 异步打开数据库连接
        /// </summary>
        /// <returns></returns>
        Task OpenAsync();
        /// <summary>
        /// 回滚当前事务会话
        /// </summary>
        void RollbackTransaction();
    }
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class DbContext : IDbContext
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        public bool IsTransactioned { get => _transaction != null; }
        public DbContextState DbContextState { get; private set; } = DbContextState.Closed;
        public DbContextType DbContextType { get; } = DbContextType.Mysql;
        public DbContext(DbContextBuilder builder)
        {
            _connection = builder.Connection;
            DbContextType = builder.DbContextType;
        }

        public virtual IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType))
            {
                var list = new List<dynamic>();
                using (var reader = cmd.ExecuteReader())
                {
                    var handler = SqlBatisSettings.DbEntityMapperProvider.GetSerializer();
                    while (reader.Read())
                    {
                        list.Add(handler(reader));
                    }
                    return list;
                }
            }
        }
        public virtual async Task<List<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var list = new List<dynamic>();
                    var handler = SqlBatisSettings.DbEntityMapperProvider.GetSerializer();
                    while (reader.Read())
                    {
                        list.Add(handler(reader));
                    }
                    return list;
                }
            }
        }
        public virtual IDbGridReader QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType);
            return new DbGridReader(cmd);
        }
        public virtual IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType))
            {
                var list = new List<T>();
                using (var reader = cmd.ExecuteReader())
                {
                    var handler = SqlBatisSettings.DbEntityMapperProvider.GetSerializer<T>(reader);
                    while (reader.Read())
                    {
                        list.Add(handler(reader));
                    }
                    return list;
                }
            }
        }
        public virtual async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var list = new List<T>();
                    var handler = SqlBatisSettings.DbEntityMapperProvider.GetSerializer<T>(reader);
                    while (await reader.ReadAsync())
                    {
                        list.Add(handler(reader));
                    }
                    return list;
                }
            }
        }
        public virtual int Execute(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType))
            {
                return cmd.ExecuteNonQuery();
            }
        }
        public virtual async Task<int> ExecuteAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public virtual T ExecuteScalar<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType))
            {
                var result = cmd.ExecuteScalar();
                if (result is DBNull || result == null)
                {
                    return default;
                }
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        public virtual async Task<T> ExecuteScalarAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                var result = await cmd.ExecuteScalarAsync();
                if (result is DBNull || result == null)
                {
                    return default;
                }
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        public virtual void BeginTransaction()
        {
            Open();
            _transaction = _connection.BeginTransaction();
        }
        public virtual void BeginTransaction(IsolationLevel level)
        {
            Open();
            _transaction = _connection.BeginTransaction(level);
        }
        public virtual void Close()
        {
            try
            {
                _transaction?.Dispose();
                _connection?.Close();
            }
            finally
            {
                DbContextState = DbContextState.Closed;
            }
        }
        public virtual void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
                DbContextState = DbContextState.Commit;
            }
        }
        public virtual void Open()
        {
            if (DbContextState == DbContextState.Closed)
            {
                _connection.Open();
                DbContextState = DbContextState.Open;
            }
        }
        public virtual async Task OpenAsync()
        {
            if (DbContextState == DbContextState.Closed)
            {
                await (_connection as DbConnection).OpenAsync();
                DbContextState = DbContextState.Open;
            }
        }
        public virtual void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
                DbContextState = DbContextState.Rollback;
            }
        }
        /// <summary>
        /// 创建DbCommand
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        protected virtual IDbCommand CreateDbCommand(string sql, object parameter, int? commandTimeout = null, CommandType? commandType = null)
        {
            Open();
            var cmd = _connection.CreateCommand();
            var dbParameters = new List<IDbDataParameter>();
            cmd.Transaction = _transaction;
            cmd.CommandText = sql;
            if (commandTimeout.HasValue)
            {
                cmd.CommandTimeout = commandTimeout.Value;
            }
            if (commandType.HasValue)
            {
                cmd.CommandType = commandType.Value;
            }
            if (parameter is IDbDataParameter)
            {
                dbParameters.Add(parameter as IDbDataParameter);
            }
            else if (parameter is IEnumerable<IDbDataParameter> parameters)
            {
                dbParameters.AddRange(parameters);
            }
            else if (parameter is Dictionary<string, object> keyValues)
            {
                foreach (var item in keyValues)
                {
                    var param = CreateDbDataParameter(cmd, item.Key, item.Value);
                    dbParameters.Add(param);
                }
            }
            else if (parameter != null)
            {
                var handler = SqlBatisSettings.DbEntityMapperProvider.GetDeserializer(parameter.GetType());
                var values = handler(parameter);
                foreach (var item in values)
                {
                    var param = CreateDbDataParameter(cmd, item.Key, item.Value);
                    dbParameters.Add(param);
                }
            }
            if (dbParameters.Count > 0)
            {
                //处理in查询
                var options = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline;
                foreach (IDataParameter item in dbParameters)
                {
                    var pattern = $@"in\s+([\@,\:,\?]?{item.ParameterName})";
                    if (cmd.CommandText.IndexOf("in", StringComparison.OrdinalIgnoreCase) > -1 && Regex.IsMatch(cmd.CommandText, pattern, options))
                    {
                        var name = Regex.Match(cmd.CommandText, pattern, options).Groups[1].Value;
                        var list = new List<object>();
                        if (item.Value is IEnumerable<object> || item.Value is Array || item.Value is IEnumerable)
                        {
                            list = (item.Value as IEnumerable).Cast<object>().Where(a => a != null && a != DBNull.Value).ToList();
                        }
                        else if (item.Value != DBNull.Value)
                        {
                            list.Add(item.Value);
                        }
                        if (list.Count > 0)
                        {
                            var plist = new List<string>();
                            for (int i = 0; i < list.Count; i++)
                            {
                                plist.Add($"{name}{i}");
                                var key = $"{item.ParameterName}{i}";
                                var param = CreateDbDataParameter(cmd, key, list[i]);
                                cmd.Parameters.Add(param);
                            }
                            cmd.CommandText = Regex.Replace(cmd.CommandText, name, $"({string.Join(",", plist)})");
                        }
                        else
                        {
                            cmd.CommandText = Regex.Replace(cmd.CommandText, name, $"(SELECT 1 WHERE 1 = 0)");
                        }
                    }
                    else if (Regex.IsMatch(cmd.CommandText, $@"([\@,\:,\?]+{item.ParameterName})", options))
                    {
                        cmd.Parameters.Add(item);
                    }
                }
            }
            return cmd;
        }
        /// <summary>
        /// 创建DbDataParameter
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual IDbDataParameter CreateDbDataParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Dispose()
        {
            RollbackTransaction();
            Close();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 析构
        /// </summary>
        ~DbContext()
        {
            Dispose();
        }
    }
}
