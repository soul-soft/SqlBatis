using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SqlBatis.Expressions;

namespace SqlBatis.Queryables
{
    /// <summary>
    /// 基础操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbQueryable<T> : DbQueryable, IDbQueryable<T>
    {
        #region fields
        private readonly DbTableMetaInfo _tableMetaInfo;
        private readonly IReadOnlyList<DbColumnMetaInfo> _columns;
        private readonly string _parameterPrefix = "@";
        private bool _ignoreAllNullColumns = false;
        public DbQueryable(IDbContext context)
            : base(context, true)
        {
            _tableMetaInfo = SqlBatisSettings.DbMetaInfoProvider.GetTable(typeof(T));
            _columns = SqlBatisSettings.DbMetaInfoProvider.GetColumns(typeof(T));
            AppendViewName(_tableMetaInfo.TableName);
        }
        #endregion

        #region resovles
        /// <summary>
        /// 获取并发列的值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object GetConcurrencyColumnValue(Type type)
        {
            if (type.IsValueType)
            {
                return Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            }
            else
            {
                return Guid.NewGuid().ToString("N");
            }
        }
        /// <summary>
        /// 默认查询字段列表表达式
        /// </summary>
        /// <returns></returns>
        protected Expression GetDefaultSelectColumnsExpression()
        {
            var ignores = BuildIgnoreExpression();
            var columns = _columns
                .Where(a => !a.IsNotMapped)
                .Where(a => !ignores.Contains(a.ColumnName))
                .Select(s => s.ColumnName != s.CsharpName ? $"{s.ColumnName} AS {s.CsharpName}" : s.CsharpName);
            var expression = string.Join(",", columns);
            return Expression.Constant(expression);
        }
        /// <summary>
        /// 实体属性转换成字典
        /// </summary>
        /// <param name="entity"></param>
        private void EntityToDictionary<Entity>(Entity entity)
        {
            var serializer = DbContextBehavior.GetEntityToDictionaryHandler(typeof(Entity));
            var values = serializer(entity);
            foreach (var item in values)
            {
                if (_parameters.ContainsKey(item.Key))
                {
                    _parameters[item.Key] = item.Value;
                }
                else
                {
                    _parameters.Add(item.Key, item.Value);
                }
            }
        }
        private void EntitiesToDictionary<Entity>(IEnumerable<Entity> entities)
        {
            var serializer = DbContextBehavior.GetEntityToDictionaryHandler(typeof(Entity));
            foreach (var entity in entities)
            {
                var values = serializer(entity);
                foreach (var iitem in values)
                {
                    if (_parameters.ContainsKey(iitem.Key))
                    {
                        (_parameters[iitem.Key] as List<object>).Add(iitem.Value);
                    }
                    else
                    {
                        var list = new List<object>
                        {
                            iitem.Value
                        };
                        _parameters.Add(iitem.Key, list);
                    }
                }
            }
        }
        /// <summary>
        /// 护理空值列
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        private List<DbColumnMetaInfo> IgnoreAllNullColumns(IReadOnlyList<DbColumnMetaInfo> columns)
        {
            var temps = new List<DbColumnMetaInfo>();
            foreach (var item in columns)
            {
                if (!_parameters.ContainsKey(item.CsharpName))
                {
                    continue;
                }
                if (_ignoreAllNullColumns)
                {
                    if (_parameters[item.CsharpName] == null)
                    {
                        _parameters.Remove(item.CsharpName);
                        continue;
                    }
                }
                temps.Add(item);
            }
            return temps;
        }
        /// <summary>
        /// 构建新增命令
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private string BuildInsertCommand(bool identity)
        {
            var table = GetViewName();
            var ignores = BuildIgnoreExpression();
            var columns = _columns
                .Where(a => !ignores.Contains(a.ColumnName))//忽略列
                .Where(a => !a.IsNotMapped)//非映射列
                .Where(a => !a.IsIdentity)//非自增列
                .Where(a => !a.IsComplexType)//非计算列
                .Where(a => !a.IsDefault || (_parameters.ContainsKey(a.CsharpName) && _parameters[a.CsharpName] != null))
                .ToList();
            var insertcolumns = IgnoreAllNullColumns(columns);
            //并发检查列
            if (_columns.Any(a => a.IsConcurrencyCheck))
            {
                var checkColumn = _columns.Where(a => a.IsConcurrencyCheck).First();
                if (!_parameters.ContainsKey(checkColumn.CsharpName) || _parameters[checkColumn.CsharpName] == null)
                {
                    if (_parameters.ContainsKey(checkColumn.CsharpName))
                        _parameters[checkColumn.CsharpName] = GetConcurrencyColumnValue(checkColumn.CsharpType);
                    else
                        _parameters.Add(checkColumn.CsharpName, GetConcurrencyColumnValue(checkColumn.CsharpType));
                }
            }
            var columnNames = string.Join(",", insertcolumns.Select(s => s.ColumnName));
            var parameters = string.Join(",", insertcolumns.Select(s => $"{_parameterPrefix}{s.CsharpName}"));
            var sql = $"INSERT INTO {table}({columnNames}) VALUES ({parameters})";
            if (identity)
            {
                if (_context.DbContextType == DbContextType.Sqlite)
                    sql = $"{sql};SELECT LAST_INSERT_ROWID()";
                else
                    sql = $"{sql};SELECT @@IDENTITY";

            }
            return sql;
        }

        /// <summary>
        /// 构建批量新增命令
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        private string BuildBatchInsertCommand(IEnumerable<T> entitys)
        {
            var table = GetViewName();
            var filters = BuildIgnoreExpression();
            var intcolumns = _columns
                .Where(a => !a.IsComplexType)
                .Where(a => !filters.Contains(a.ColumnName))
                .Where(a => !a.IsNotMapped)
                .Where(a => !a.IsIdentity)
                .ToList();
            var columnNames = string.Join(",", intcolumns.Select(s => s.ColumnName));
            if (_context.DbContextType == DbContextType.Mysql)
            {
                var buffer = new StringBuilder();
                buffer.Append($"INSERT INTO {table}({columnNames}) VALUES ");
                var serializer = DbContextBehavior.GetEntityToDictionaryHandler(typeof(T));
                var list = entitys.ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var values = serializer(item);
                    buffer.Append('(');
                    for (var j = 0; j < intcolumns.Count; j++)
                    {
                        var column = intcolumns[j];
                        var value = values[column.CsharpName];
                        if (value == null)
                        {
                            buffer.Append(column.IsDefault ? "DEFAULT" : "NULL");
                        }
                        else if (column.CsharpType == typeof(bool) || column.CsharpType == typeof(bool?))
                        {
                            buffer.Append(Convert.ToBoolean(value) == true ? 1 : 0);
                        }
                        else if (column.CsharpType == typeof(Guid) || column.CsharpType == typeof(Guid?))
                        {
                            buffer.Append($"'{value}'");
                        }
                        else if (column.CsharpType == typeof(byte[]))
                        {
                            var bytes = value as byte[];
                            var hexstr = string.Join("", bytes.Select(s => Convert.ToString(s, 16).PadLeft(2, '0')));
                            buffer.Append($"0x{hexstr}");
                        }
                        else if (column.CsharpType == typeof(DateTime) || column.CsharpType == typeof(DateTime?))
                        {
                            buffer.Append($"'{value}'");
                        }
                        else if (column.CsharpType.IsValueType || (Nullable.GetUnderlyingType(column.CsharpType)?.IsValueType == true))
                        {
                            buffer.Append(value);
                        }
                        else
                        {
                            var str = CheckSql(value.ToString());
                            buffer.Append($"'{str}'");
                        }
                        if (j + 1 < intcolumns.Count)
                        {
                            buffer.Append(',');
                        }
                    }
                    buffer.Append(')');
                    if (i + 1 < list.Count)
                    {
                        buffer.Append(',');
                    }
                }
                return buffer.Remove(buffer.Length - 1, 0).ToString();
            }
            throw new NotImplementedException();
        }
        /// <summary>
        /// 构建更新命令
        /// </summary>
        /// <returns></returns>
        private string BuildUpdateCommand()
        {
            var table = GetViewName();
            var builder = new StringBuilder();
            var expressions = _expressions.GetSetExpressions();
            if (expressions.Any())
            {
                var where = BuildWhereExpression();
                foreach (var item in expressions)
                {
                    var column = new GroupExpressionResovle(_isSingleTable, item.Column).Resovle();
                    var expression = new BooleanExpressionResovle(_isSingleTable, item.Expression, _parameters).Resovle();
                    builder.Append($"{column} = {expression},");
                }
                var sql = $"UPDATE {table} SET {builder.ToString().Trim(',')}{where}";
                return sql;
            }
            else
            {
                var filters = BuildIgnoreExpression();
                var where = BuildWhereExpression();
                var columns = _columns
                    .Where(a => !filters.Contains(a.ColumnName))
                    .Where(a => !a.IsComplexType)
                    .Where(a => !a.IsPrimaryKey)
                    .Where(a => !a.IsNotMapped)
                    .Where(a => !a.IsIdentity)
                    .Where(a => !a.IsConcurrencyCheck)
                    .ToList();
                var updatecolumns = IgnoreAllNullColumns(columns);
                if (string.IsNullOrEmpty(where))
                {
                    var primaryKey = _columns.Where(a => a.IsPrimaryKey).FirstOrDefault();
                    if (primaryKey == null)
                    {
                        throw new MissingPrimaryKeyException("primary key is required");
                    }
                    where = $" WHERE {primaryKey.ColumnName} = {_parameterPrefix}{primaryKey.CsharpName}";
                }
                var setsql = updatecolumns.Select(s => $"{s.ColumnName} = {_parameterPrefix}{s.CsharpName}");
                var sql = $"UPDATE {table} SET {string.Join(",", setsql)}";
                //并发检查
                if (_columns.Any(a => a.IsConcurrencyCheck))
                {
                    var checkColumn = _columns.Where(a => a.IsConcurrencyCheck).FirstOrDefault();
                    if (!_parameters.ContainsKey(checkColumn.CsharpName) || _parameters[checkColumn.CsharpName] == null)
                    {
                        throw new ArgumentNullException(checkColumn.CsharpName);
                    }
                    sql += $",{checkColumn.ColumnName} = {_parameterPrefix}New{checkColumn.CsharpName}";
                    where += $" AND {checkColumn.ColumnName} = {_parameterPrefix}{checkColumn.CsharpName}";
                    _parameters.Add($"New{checkColumn.CsharpName}", GetConcurrencyColumnValue(checkColumn.CsharpType));
                }
                sql = $"{sql}{where}";
                return sql;
            }
        }
        /// <summary>
        /// 构建删除命令
        /// </summary>
        /// <returns></returns>
        private string BuildDeleteCommand(bool byPrimaryKey=false)
        {
            var table = GetViewName();
            var where = string.Empty;
            if (byPrimaryKey)
            {
                var primaryKey = _columns.Where(a => a.IsPrimaryKey).FirstOrDefault();
                if (primaryKey == null)
                {
                    throw new MissingPrimaryKeyException("primary key is required");
                }
                foreach (var item in _parameters.Keys)
                {
                    if (item!= primaryKey.CsharpName)
                    {
                        _parameters.Remove(item);
                    }
                }
                var opt = _parameters[primaryKey.CsharpName] is List<object> ? Operator.ResovleExpressionType("IN") : "=";
                where = $" WHERE {primaryKey.ColumnName} {opt} {_parameterPrefix}{primaryKey.CsharpName}";
            }
            else
            {
                where = BuildWhereExpression();
            }
            var sql = $"DELETE FROM {table}{where}";
            return sql;
        }
        /// <summary>
        /// 防止注入检测
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string CheckSql(string sql)
        {
            var buffer = new StringBuilder();
            for (int i = 0; i < sql.Length; i++)
            {
                var ch = sql[i];
                if (ch == '\'' || ch == '-' || ch == '\\' || ch == '*' || ch == '@')
                {
                    buffer.Append('\\');
                }
                buffer.Append(ch);
            }
            return buffer.ToString();
        }
        #endregion

        #region sync       
        public int Count(int? commandTimeout = null)
        {
            var sql = BuildCountCommand();
            return _context.ExecuteScalar<int>(sql, _parameters, commandTimeout);
        }

        public int Count<TResult>(Expression<Func<T, TResult>> expression)
        {
            var sql = BuildCountCommand(expression);
            return _context.ExecuteScalar<int>(sql, _parameters);
        }

        public int Insert<Entity>(Entity entity)
        {
            EntityToDictionary(entity);
            var sql = BuildInsertCommand(false);
            return _context.Execute(sql, _parameters);
        }

        public int InsertReturnId<Entity>(Entity entity)
        {
            EntityToDictionary(entity);
            var sql = BuildInsertCommand(true);
            return _context.ExecuteScalar<int>(sql, _parameters);
        }

        public int Insert<Entity>(IEnumerable<Entity> entitys)
        {
            if (entitys == null || !entitys.Any())
            {
                return 0;
            }
            else if (_context.IsTransactioned)
            {
                return entitys.Select(s => Insert(s)).Sum();
            }
            else
            {
                try
                {
                    _context.BeginTransaction();
                    var count = entitys.Select(s => Insert(s)).Sum();
                    _context.CommitTransaction();
                    return count;
                }
                catch
                {
                    _context.RollbackTransaction();
                    throw;
                }
            }
        }

        public int InsertBatch(IEnumerable<T> entities, int? commandTimeout = null)
        {
            var count = 0;
            if (entities == null || !entities.Any())
            {
                return count;
            }
            var sql = BuildBatchInsertCommand(entities);
            return _context.Execute(sql, _parameters, commandTimeout);
        }
        public int Update(int? commandTimeout = null)
        {
            if (_expressions.Any(a => a.ExpressionType == DbExpressionType.Set))
            {
                var sql = BuildUpdateCommand();
                return _context.Execute(sql, _parameters, commandTimeout);
            }
            return default;
        }

        public int Update<Entity>(Entity entity)
        {
            EntityToDictionary(entity);
            var sql = BuildUpdateCommand();
            var row = _context.Execute(sql, _parameters);
            if (_columns.Any(a => a.IsConcurrencyCheck) && row == 0)
            {
                throw new DbUpdateConcurrencyException("更新失败：数据版本不一致");
            }
            return row;
        }
        public int DeleteBatch(IEnumerable<T> entities)
        {
            EntitiesToDictionary<T>(entities);
            var sql = BuildDeleteCommand(true);
            return _context.Execute(sql, _parameters);
        }
        public int Delete(T entity)
        {
            EntityToDictionary(entity);
            var sql = BuildDeleteCommand(true);
            return _context.Execute(sql, _parameters);
        }

        public int Delete(int? commandTimeout = null)
        {
            var sql = BuildDeleteCommand();
            return _context.Execute(sql, _parameters, commandTimeout);
        }

        public int Delete(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return Delete();
        }

        public bool Exists(int? commandTimeout = null)
        {
            var sql = BuildExistsCommand();
            return _context.ExecuteScalar<bool>(sql, _parameters, commandTimeout);
        }

        public bool Exists(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return Exists();
        }

        public IDbQueryable<T> Set<TResult>(Expression<Func<T, TResult>> column, TResult value, bool condition = true)
        {
            if (condition)
            {
                Expression expression;
                if (value == null)
                {
                    expression = Expression.Constant(DBNull.Value);
                }
                else
                {
                    expression = Expression.Constant(value);
                }
                _expressions.Add(new DbSetExpression(column, expression));
            }
            return this;
        }

        public IDbQueryable<T> Set<TResult>(Expression<Func<T, TResult>> column, Expression<Func<T, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                _expressions.Add(new DbSetExpression(column, expression));
            }
            return this;
        }

        public IDbQueryable<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression)
        {
            AddGroupExpression(expression);
            return this;
        }

        public IDbQueryable<T> Having(Expression<Func<T, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                AddHavingExpression(expression);
            }
            return this;
        }

        public IDbQueryable<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                AddOrderExpression(expression, true);
            }
            return this;
        }

        public IDbQueryable<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                AddOrderExpression(expression, false);
            }
            return this;
        }
        public IDbQueryable<T> Ignore(bool ignoreAllNullColumns = true)
        {
            _ignoreAllNullColumns = ignoreAllNullColumns;
            return this;
        }

        public IDbQueryable<T> Ignore<TResult>(Expression<Func<T, TResult>> column, bool condition = true)
        {
            if (condition)
            {
                AddIgnoreExpression(column);
            }
            return this;
        }

        public IDbQueryable<T> Page(int index, int count, bool condition = true)
        {
            if (condition)
            {
                Skip((index - 1) * count, count);
            }
            return this;
        }

        public IDbQueryable<T> With(string lockname)
        {
            SetLockName($" {lockname}");
            return this;
        }

        public IEnumerable<T> Select(int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(GetDefaultSelectColumnsExpression());
            return _context.Query<T>(sql, _parameters, commandTimeout);
        }

        public (IEnumerable<T>, int) SelectMany(int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(GetDefaultSelectColumnsExpression());
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = multi.Read<T>();
                var count = multi.ReadFirst<int>();
                return (list, count);
            }
        }
        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> expression = null, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression??GetDefaultSelectColumnsExpression());
            return _context.Query<TResult>(sql, _parameters, commandTimeout);
        }

        public (IEnumerable<TResult>, int) SelectMany<TResult>(Expression<Func<T, TResult>> expression = null, int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(expression ?? GetDefaultSelectColumnsExpression());
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = multi.Read<TResult>();
                var count = multi.ReadFirst<int>();
                return (list, count);
            }
        }

        public T Single(int? commandTimeout = null)
        {
            Take(1);
            return Select(commandTimeout).FirstOrDefault();
        }

        public TResult Single<TResult>(Expression<Func<T, TResult>> expression = null, int? commandTimeout = null)
        {
            Take(1);
            return Select(expression, commandTimeout).FirstOrDefault();
        }

        public IDbQueryable<T> Skip(int index, int count, bool condition = true)
        {
            if (condition)
            {
                SetPage(index, count);
            }
            return this;
        }

        public IDbQueryable<T> Take(int count, bool condition = true)
        {
            if (condition)
            {
                Skip(0, count);
            }
            return this;
        }

        public IDbQueryable<T> Where(Expression<Func<T, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                AddWhereExpression(expression);
            }
            return this;
        }

        #endregion

        #region async
        public Task<int> CountAsync(int? commandTimeout = null)
        {
            var sql = BuildCountCommand();
            return _context.ExecuteScalarAsync<int>(sql, _parameters, commandTimeout);
        }

        public Task<int> CountAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            var sql = BuildCountCommand(expression);
            return _context.ExecuteScalarAsync<int>(sql, _parameters);
        }

        public Task<int> DeleteAsync(int? commandTimeout = null)
        {
            var sql = BuildDeleteCommand();
            return _context.ExecuteAsync(sql, _parameters, commandTimeout);
        }

        public Task<int> DeleteAsync(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return DeleteAsync();
        }
        public Task<int> DeleteAsync(T entity)
        {
            EntityToDictionary(entity);
            var sql = BuildDeleteCommand(true);
            return _context.ExecuteAsync(sql, _parameters);
        }
        public Task<int> DeleteBatchAsync(IEnumerable<T> entities)
        {
            EntitiesToDictionary<T>(entities);
            var sql = BuildDeleteCommand(true);
            return _context.ExecuteAsync(sql, _parameters);
        }
        public Task<bool> ExistsAsync(int? commandTimeout = null)
        {
            var sql = BuildExistsCommand();
            return _context.ExecuteScalarAsync<bool>(sql, _parameters, commandTimeout);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return ExistsAsync();
        }

        public Task<int> UpdateAsync(int? commandTimeout = null)
        {
            if (_expressions.Any(a => a.ExpressionType == DbExpressionType.Set))
            {
                var sql = BuildUpdateCommand();
                return _context.ExecuteAsync(sql, _parameters, commandTimeout);
            }
            return default;
        }

        public async Task<int> UpdateAsync<Entity>(Entity entity)
        {
            EntityToDictionary(entity);
            var sql = BuildUpdateCommand();
            var row = await _context.ExecuteAsync(sql, _parameters);
            if (_columns.Any(a => a.IsConcurrencyCheck) && row == 0)
            {
                throw new DbUpdateConcurrencyException("更新失败：数据版本不一致");
            }
            return row;
        }

        public Task<int> InsertAsync<Entity>(Entity entity)
        {
            EntityToDictionary(entity);
            var sql = BuildInsertCommand(false);
            return _context.ExecuteAsync(sql, _parameters);
        }

        public async Task<int> InsertAsync<Entity>(IEnumerable<Entity> entitys)
        {
            int count = 0;
            if (entitys == null || !entitys.Any())
            {
                return 0;
            }
            else if (_context.IsTransactioned)
            {
                foreach (var item in entitys)
                {
                    await InsertAsync(item);
                    count++;
                }
            }
            else
            {
                try
                {
                    _context.BeginTransaction();
                    foreach (var item in entitys)
                    {
                        await InsertAsync(item);
                        count++;
                    }
                    _context.CommitTransaction();
                    return count;
                }
                catch
                {
                    _context.RollbackTransaction();
                    throw;
                }
            }
            return count;
        }

        public async Task<int> InsertBatchAsync(IEnumerable<T> entitys, int? commandTimeout = null)
        {
            var count = 0;
            if (entitys == null || !entitys.Any())
            {
                return count;
            }
            var sql = BuildBatchInsertCommand(entitys);
            return await _context.ExecuteAsync(sql, _parameters, commandTimeout);
        }

        public Task<int> InsertReturnIdAsync<Entity>(Entity entity)
        {
            EntityToDictionary(entity);
            var sql = BuildInsertCommand(true);
            return _context.ExecuteScalarAsync<int>(sql, _parameters);
        }

        public Task<IEnumerable<T>> SelectAsync(int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(GetDefaultSelectColumnsExpression());
            return _context.QueryAsync<T>(sql, _parameters, commandTimeout);
        }

        public async Task<(IEnumerable<T>, int)> SelectManyAsync(int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(GetDefaultSelectColumnsExpression());
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.ReadAsync<T>();
                var count = await multi.ReadFirstAsync<int>();
                return (list, count);
            }
        }

        public Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> expression = null, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression ?? GetDefaultSelectColumnsExpression());
            return _context.QueryAsync<TResult>(sql, _parameters, commandTimeout);
        }

        public async Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T, TResult>> expression = null, int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(expression ?? GetDefaultSelectColumnsExpression());
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.ReadAsync<TResult>();
                var count = await multi.ReadFirstAsync<int>();
                return (list, count);
            }
        }

        public async Task<T> SingleAsync(int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(commandTimeout)).FirstOrDefault();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<T, TResult>> expression = null, int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(expression, commandTimeout)).FirstOrDefault();
        }


        #endregion
    }
}
