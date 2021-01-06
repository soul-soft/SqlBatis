using System;
using System.Collections.Generic;
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
    public partial class DbQueryable<T> : Queryable, IDbQueryable<T>
    {
        #region fields
        protected Expression _filterExpression = null;
        private readonly List<SetExpression> _setExpressions = new List<SetExpression>();
        public DbQueryable(IDbContext context)
            : base(context, true)
        {
            SetViewName(GetSingleTableName<T>());
        }
        #endregion

        #region resovles
        /// <summary>
        /// 默认查询字段列表表达式
        /// </summary>
        /// <returns></returns>
        protected Expression DefaultColumnsExpression()
        {
            var filters = new IgnoreExpressionResovle(_filterExpression).Resovles();
            var columns = GetSingleTableColumnMetaInfos<T>()
                .Where(a => !filters.Contains(a.ColumnName) && !a.IsNotMapped)
                .Select(s => s.ColumnName != s.CsharpName ? $"{s.ColumnName} AS {s.CsharpName}" : s.CsharpName);
            var expression = string.Join(",", columns);
            return Expression.Constant(expression);
        }
        /// <summary>
        /// 实体属性转换成字典
        /// </summary>
        /// <param name="entity"></param>
        private void EntityToDbParameters(T entity)
        {
            var serializer = DbEntityMapper.GetDeserializer(typeof(T));
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
        /// <summary>
        /// 构建新增命令
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private string BuildInsertCommand(bool identity)
        {
            var table = GetViewName();
            var filters = new IgnoreExpressionResovle(_filterExpression).Resovles();
            var columns = GetSingleTableColumnMetaInfos<T>();
            var intcolumns = columns
                .Where(a => !filters.Contains(a.ColumnName) && !a.IsNotMapped && !a.IsIdentity)
                .Where(a => !a.IsComplexType)
                .Where(a => !a.IsDefault || (_parameters.ContainsKey(a.CsharpName) && _parameters[a.CsharpName] != null));//如果是默认字段
            var columnNames = string.Join(",", intcolumns.Select(s => s.ColumnName));
            var parameterNames = string.Join(",", intcolumns.Select(s => $"@{s.CsharpName}"));
            var sql = $"INSERT INTO {table}({columnNames}) VALUES ({parameterNames})";
            if (identity)
            {
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
            var filters = new IgnoreExpressionResovle(_filterExpression).Resovles();
            var columns = GetSingleTableColumnMetaInfos<T>()
                .Where(a => !a.IsComplexType).ToList();
            var intcolumns = columns
                .Where(a => !filters.Contains(a.ColumnName) && !a.IsNotMapped && !a.IsIdentity)
                .ToList();
            var columnNames = string.Join(",", intcolumns.Select(s => s.ColumnName));
            if (_context.DbContextType == DbContextType.Mysql)
            {
                var buffer = new StringBuilder();
                buffer.Append($"INSERT INTO {table}({columnNames}) VALUES ");
                var serializer = DbEntityMapper.GetDeserializer(typeof(T));
                var list = entitys.ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var values = serializer(item);
                    buffer.Append("(");
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
                            buffer.Append(",");
                        }
                    }
                    buffer.Append(")");
                    if (i + 1 < list.Count)
                    {
                        buffer.Append(",");
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
            if (_setExpressions.Count > 0)
            {
                var where = BuildWhereExpression();
                foreach (var item in _setExpressions)
                {
                    var column = new BooleanExpressionResovle(_isSingleTable, item.Column, _parameters).Resovle();
                    var expression = new BooleanExpressionResovle(_isSingleTable, item.Expression, _parameters).Resovle();
                    builder.Append($"{column} = {expression},");
                }
                var sql = $"UPDATE {table} SET {builder.ToString().Trim(',')}{where}";
                return sql;
            }
            else
            {
                var filters = new IgnoreExpressionResovle(_filterExpression).Resovles();
                var where = BuildWhereExpression();
                var columns = GetSingleTableColumnMetaInfos<T>();
                var updcolumns = columns
                    .Where(a => !filters.Contains(a.ColumnName))
                    .Where(a => !a.IsComplexType)
                    .Where(a => !a.IsIdentity && !a.IsPrimaryKey && !a.IsNotMapped)
                    .Where(a => !a.IsConcurrencyCheck)
                    .Select(s => $"{s.ColumnName} = @{s.CsharpName}");
                if (string.IsNullOrEmpty(where))
                {
                    var primaryKey = columns.Where(a => a.IsPrimaryKey).FirstOrDefault()
                        ?? columns.First();
                    where = $" WHERE {primaryKey.ColumnName} = @{primaryKey.CsharpName}";
                    if (columns.Exists(a => a.IsConcurrencyCheck))
                    {
                        var checkColumn = columns.Where(a => a.IsConcurrencyCheck).FirstOrDefault();
                        where += $" AND {checkColumn.ColumnName} = @{checkColumn.CsharpName}";
                    }
                }
                var sql = $"UPDATE {table} SET {string.Join(",", updcolumns)}";
                if (columns.Exists(a => a.IsConcurrencyCheck))
                {
                    var checkColumn = columns.Where(a => a.IsConcurrencyCheck).FirstOrDefault();
                    sql += $",{checkColumn.ColumnName} = @New{checkColumn.CsharpName}";
                    if (checkColumn.CsharpType.IsValueType)
                    {
                        var version = Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                        _parameters.Add($"New{checkColumn.CsharpName}", version);
                    }
                    else
                    {
                        var version = Guid.NewGuid().ToString("N");
                        _parameters.Add($"New{checkColumn.CsharpName}", version);
                    }
                }
                sql += where;
                return sql;
            }
        }
        /// <summary>
        /// 构建删除命令
        /// </summary>
        /// <returns></returns>
        private string BuildDeleteCommand()
        {
            var table = GetViewName();
            var where = BuildWhereExpression();
            var sql = $"DELETE FROM {table}{where}";
            return sql;
        }
        /// <summary>
        /// 防止注入检测
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string CheckSql(string sql)
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
    }
    /// <summary>
    /// 同步linq查询
    /// </summary>
    public partial class DbQueryable<T>
    {
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

        public int Insert(T entity)
        {
            EntityToDbParameters(entity);
            var sql = BuildInsertCommand(false);
            return _context.Execute(sql, _parameters);
        }

        public int InsertReturnId(T entity)
        {
            EntityToDbParameters(entity);
            var sql = BuildInsertCommand(true);
            return _context.ExecuteScalar<int>(sql, _parameters);
        }

        public int Insert(IEnumerable<T> entitys, int? commandTimeout = null)
        {
            if (entitys == null || entitys.Count() == 0)
            {
                return 0;
            }
            var sql = BuildBatchInsertCommand(entitys);
            return _context.Execute(sql, _parameters, commandTimeout);
        }

        public int Update(int? commandTimeout = null)
        {
            if (_setExpressions.Count > 0)
            {
                var sql = BuildUpdateCommand();
                return _context.Execute(sql, _parameters, commandTimeout);
            }
            return default;
        }

        public int Update(T entity)
        {
            EntityToDbParameters(entity);
            var sql = BuildUpdateCommand();
            var row = _context.Execute(sql, _parameters);
            if (GetSingleTableColumnMetaInfos<T>().Exists(a => a.IsConcurrencyCheck) && row == 0)
            {
                throw new DbUpdateConcurrencyException("更新失败：数据版本不一致");
            }
            return row;
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
                _setExpressions.Add(new SetExpression
                {
                    Column = column,
                    Expression = Expression.Constant(value)
                });
            }
            return this;
        }

        public IDbQueryable<T> Set<TResult>(Expression<Func<T, TResult>> column, Expression<Func<T, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                _setExpressions.Add(new SetExpression
                {
                    Column = column,
                    Expression = expression
                });
            }
            return this;
        }

        public IDbQueryable<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression)
        {
            _groupExpressions.Add(expression);
            return this;
        }

        public IDbQueryable<T> Having(Expression<Func<T, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                _havingExpressions.Add(expression);
            }
            return this;
        }

        public IDbQueryable<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                _orderExpressions.Add(new OrderExpression
                {
                    Asc = true,
                    Expression = expression
                });
            }
            return this;
        }

        public IDbQueryable<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                _orderExpressions.Add(new OrderExpression
                {
                    Asc = false,
                    Expression = expression
                });
            }
            return this;
        }

        public IDbQueryable<T> Ignore<TResult>(Expression<Func<T, TResult>> column)
        {
            _filterExpression = column;
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
            _lockname = $" {lockname}";
            return this;
        }

        public IEnumerable<T> Select(int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(DefaultColumnsExpression());
            return _context.Query<T>(sql, _parameters, commandTimeout);
        }

        public (IEnumerable<T>, int) SelectMany(int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(DefaultColumnsExpression());
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = multi.GetList<T>();
                var count = multi.Get<int>();
                return (list, count);
            }
        }
        public TResult Sum<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSumCommand(expression);
            return _context.ExecuteScalar<TResult>(sql, _parameters, commandTimeout);
        }
        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression);
            return _context.Query<TResult>(sql, _parameters, commandTimeout);
        }

        public (IEnumerable<TResult>, int) SelectMany<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(expression);
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = multi.GetList<TResult>();
                var count = multi.Get<int>();
                return (list, count);
            }
        }

        public T Single(int? commandTimeout = null)
        {
            Take(1);
            return Select(commandTimeout).FirstOrDefault();
        }

        public TResult Single<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return Select(expression, commandTimeout).FirstOrDefault();
        }

        public IDbQueryable<T> Skip(int index, int count, bool condition = true)
        {
            if (condition)
            {
                _page.Index = index;
                _page.Count = count;
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
                _whereExpressions.Add(expression);
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
            if (_setExpressions.Count > 0)
            {
                var sql = BuildUpdateCommand();
                return _context.ExecuteAsync(sql, _parameters, commandTimeout);
            }
            return default;
        }

        public async Task<int> UpdateAsync(T entity)
        {
            EntityToDbParameters(entity);
            var sql = BuildUpdateCommand();
            var row = await _context.ExecuteAsync(sql, _parameters);
            if (GetSingleTableColumnMetaInfos<T>().Exists(a => a.IsConcurrencyCheck) && row == 0)
            {
                throw new DbUpdateConcurrencyException("更新失败：数据版本不一致");
            }
            return row;
        }

        public Task<int> InsertAsync(T entity)
        {
            EntityToDbParameters(entity);
            var sql = BuildInsertCommand(false);
            return _context.ExecuteAsync(sql, _parameters);
        }

        public async Task<int> InsertAsync(IEnumerable<T> entitys, int? commandTimeout = null)
        {
            if (entitys == null || entitys.Count() == 0)
            {
                return 0;
            }
            var sql = BuildBatchInsertCommand(entitys);
            return await _context.ExecuteAsync(sql, _parameters, commandTimeout);
        }

        public Task<int> InsertReturnIdAsync(T entity)
        {
            EntityToDbParameters(entity);
            var sql = BuildInsertCommand(true);
            return _context.ExecuteScalarAsync<int>(sql, _parameters);
        }

        public async Task<TResult> SumAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSumCommand(expression);
            return await _context.ExecuteScalarAsync<TResult>(sql, _parameters, commandTimeout);
        }

        public Task<IEnumerable<T>> SelectAsync(int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(DefaultColumnsExpression());
            return _context.QueryAsync<T>(sql, _parameters, commandTimeout);
        }

        public async Task<(IEnumerable<T>, int)> SelectManyAsync(int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(DefaultColumnsExpression());
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.GetListAsync<T>();
                var count = await multi.GetAsync<int>();
                return (list, count);
            }
        }

        public Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression);
            return _context.QueryAsync<TResult>(sql, _parameters, commandTimeout);
        }

        public async Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(expression);
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.GetListAsync<TResult>();
                var count = await multi.GetAsync<int>();
                return (list, count);
            }
        }

        public async Task<T> SingleAsync(int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(commandTimeout)).FirstOrDefault();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(expression, commandTimeout)).FirstOrDefault();
        }
        #endregion
    }

}
