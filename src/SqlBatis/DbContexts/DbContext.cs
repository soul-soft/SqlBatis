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
        /// 数据库上下文状态
        /// </summary>
        DbContextState DbContextState { get; }
        /// <summary>
        /// 数据库连接
        /// </summary>
        IDbConnection Connection { get; }
        /// <summary>
        /// 数据库上下文类型
        /// </summary>
        DbContextType DbContextType { get; }
        /// <summary>
        /// 获取一个xml执行器
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="id">命令id</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        IXmlQuery From<T>(string id, T parameter) where T : class;
        /// <summary>
        /// 获取一个xml执行器
        /// </summary>
        /// <param name="id">命令id</param>
        /// <returns></returns>
        IXmlQuery From(string id);
        /// <summary>
        /// 获取一个linq执行器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDbQuery<T> From<T>();
        /// <summary>
        /// 开启事务会话
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// 异步开启事务会话
        /// </summary>
        /// <returns></returns>
        Task BeginTransactionAsync();
        /// <summary>
        /// 开启事务会话
        /// </summary>
        /// <param name="level">事务隔离级别</param>
        void BeginTransaction(IsolationLevel level);
        /// <summary>
        /// 异步开启事务会话
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        Task BeginTransactionAsync(IsolationLevel level);
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
        IDbMultipleResult QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
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
        Task<IEnumerable<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
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
        public DbContextState DbContextState { get; private set; } = DbContextState.Closed;
        protected IDbTransaction _transaction;
        public IDbConnection Connection { get; }
        public DbContextType DbContextType { get; } = DbContextType.Mysql;
        public DbContext(DbContextBuilder builder)
        {
            Connection = builder.Connection;
            DbContextType = builder.DbContextType;
        }
        public virtual IXmlQuery From<T>(string id, T parameter) where T : class
        {
            var sql = GlobalSettings.XmlCommandsProvider.Build(id, parameter);
            var deserializer = GlobalSettings.EntityMapperProvider.GetDeserializer(typeof(T));
            var values = deserializer(parameter);
            return new XmlQuery(this, sql, values);
        }
        public virtual IXmlQuery From(string id)
        {
            var sql = GlobalSettings.XmlCommandsProvider.Build(id);
            return new XmlQuery(this, sql);
        }
        public virtual IDbQuery<T> From<T>()
        {
            return new DbQuery<T>(this);
        }
        public virtual IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType))
            {
                var list = new List<dynamic>();
                using (var reader = cmd.ExecuteReader())
                {
                    var handler = GlobalSettings.EntityMapperProvider.GetSerializer();
                    while (reader.Read())
                    {
                        list.Add(handler(reader));
                    }
                    return list;
                }
            }
        }
        public virtual async Task<IEnumerable<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var list = new List<dynamic>();
                    var handler = GlobalSettings.EntityMapperProvider.GetSerializer();
                    while (reader.Read())
                    {
                        list.Add(handler(reader));
                    }
                    return list;
                }
            }
        }
        public virtual IDbMultipleResult QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var cmd = CreateCommand(sql, parameter, commandTimeout, commandType);
            return new DbMultipleResult(cmd);
        }
        public virtual IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType))
            {
                var list = new List<T>();
                using (var reader = cmd.ExecuteReader())
                {
                    var handler = GlobalSettings.EntityMapperProvider.GetSerializer<T>(reader);
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
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var list = new List<T>();
                    var handler = GlobalSettings.EntityMapperProvider.GetSerializer<T>(reader);
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
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType))
            {
                return cmd.ExecuteNonQuery();
            }
        }
        public virtual async Task<int> ExecuteAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                return await cmd.ExecuteNonQueryAsync();
            }
        }
        public virtual T ExecuteScalar<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType))
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
            using (var cmd = CreateCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
            {
                var result = await cmd.ExecuteScalarAsync();
                if (result is DBNull || result == null)
                {
                    return default;
                }
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        public virtual async Task BeginTransactionAsync()
        {
            await Task.Run(() =>
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Open();
                }
                _transaction = Connection.BeginTransaction();
            });
        }
        public virtual async Task BeginTransactionAsync(IsolationLevel level)
        {
            await Task.Run(() =>
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Open();
                }
                _transaction = Connection.BeginTransaction(level);
            });
        }
        public virtual void BeginTransaction()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Open();
            }
            _transaction = Connection.BeginTransaction();
        }
        public virtual void BeginTransaction(IsolationLevel level)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Open();
            }
            _transaction = Connection.BeginTransaction(level);
        }
        public virtual void Close()
        {
            _transaction?.Dispose();
            Connection?.Close();
            DbContextState = DbContextState.Closed;
        }
        public virtual void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                DbContextState = DbContextState.Commit;
            }
        }
        public virtual void Open()
        {
            Connection?.Open();
            DbContextState = DbContextState.Open;
        }
        public virtual async Task OpenAsync()
        {
            await (Connection as DbConnection).OpenAsync();
            DbContextState = DbContextState.Open;
        }
        public virtual void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                DbContextState = DbContextState.Rollback;
            }
        }
        protected virtual IDbCommand CreateCommand(string sql, object parameter, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Open();
            }
            var cmd = Connection.CreateCommand();
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
                    var param = CreateParameter(cmd, item.Key, item.Value);
                    dbParameters.Add(param);
                }
            }
            else if (parameter != null)
            {
                var handler = GlobalSettings.EntityMapperProvider.GetDeserializer(parameter.GetType());
                var values = handler(parameter);
                foreach (var item in values)
                {
                    var param = CreateParameter(cmd, item.Key, item.Value);
                    dbParameters.Add(param);
                }
            }
            if (dbParameters.Count > 0)
            {
                foreach (IDataParameter item in dbParameters)
                {
                    if (item.Value == null)
                    {
                        item.Value = DBNull.Value;
                    }
                    var pattern = $@"in\s+([\@,\:,\?]?{item.ParameterName})";
                    var options = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline;
                    if (cmd.CommandText.IndexOf("in", StringComparison.OrdinalIgnoreCase) != -1 && Regex.IsMatch(cmd.CommandText, pattern, options))
                    {
                        var name = Regex.Match(cmd.CommandText, pattern, options).Groups[1].Value;
                        var list = new List<object>();
                        if (item.Value is IEnumerable<object> || item.Value is Array || item.Value is IEnumerable)
                        {
                            list = (item.Value as IEnumerable).Cast<object>().Where(a => a != null && a != DBNull.Value).ToList();
                        }
                        else
                        {
                            list.Add(item.Value);
                        }
                        if (list.Count() > 0)
                        {
                            cmd.CommandText = Regex.Replace(cmd.CommandText, name, $"({string.Join(",", list.Select(s => $"{name}{list.IndexOf(s)}"))})");
                            foreach (var iitem in list)
                            {
                                var key = $"{item.ParameterName}{list.IndexOf(iitem)}";
                                var param = CreateParameter(cmd, key, iitem);
                                cmd.Parameters.Add(param);
                            }
                        }
                        else
                        {
                            cmd.CommandText = Regex.Replace(cmd.CommandText, name, $"(SELECT 1 WHERE 1 = 0)");
                        }
                    }
                    else
                    {
                        cmd.Parameters.Add(item);
                    }
                }
            }
            return cmd;
        }
        protected virtual IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
        public virtual void Dispose()
        {
            if (DbContextState == DbContextState.Open)
            {
                RollbackTransaction();
            }
            Close();
        }
    }

}
