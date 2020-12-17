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
        public virtual IDbQueryable<T> From<T>()
        {
            return new DbQueryable<T>(this);
        }
        public virtual IDbQueryable<T1, T2> From<T1, T2>()
        {
            return new DbQueryable<T1, T2>(this);
        }
        public virtual IDbQueryable<T1, T2, T3> From<T1, T2, T3>()
        {
            return new DbQueryable<T1, T2, T3>(this);
        }
        public virtual IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType))
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
        public virtual async Task<List<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
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
            var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType);
            return new DbMultipleResult(cmd);
        }
        public virtual IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType))
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
            using (var cmd = CreateDbCommand(sql, parameter, commandTimeout, commandType) as DbCommand)
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
            _transaction = Connection.BeginTransaction();
        }
        public virtual void BeginTransaction(IsolationLevel level)
        {
            Open();
            _transaction = Connection.BeginTransaction(level);
        }
        public virtual void Close()
        {
            try
            {
                _transaction?.Dispose();
            }
            catch
            {
            }
            try
            {
                Connection?.Close();
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
                Connection.Open();
                DbContextState = DbContextState.Open;
            }
        }
        public virtual async Task OpenAsync()
        {
            if (DbContextState == DbContextState.Closed)
            {
                await (Connection as DbConnection).OpenAsync();
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
        protected virtual IDbCommand CreateDbCommand(string sql, object parameter, int? commandTimeout = null, CommandType? commandType = null)
        {
            Open();
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
                    var param = CreateDbDataParameter(cmd, item.Key, item.Value);
                    dbParameters.Add(param);
                }
            }
            else if (parameter != null)
            {
                var handler = GlobalSettings.EntityMapperProvider.GetDeserializer(parameter.GetType());
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
                        if (list.Count() > 0)
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
        protected virtual IDbDataParameter CreateDbDataParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }
        public virtual void Dispose()
        {
            RollbackTransaction();
            Close();
        }
        ~DbContext()
        {
            Dispose();
        }
    }
}
