using SqlBatis.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using SqlBatis.Expressions.Resovles;
using SqlBatis.Expressions;

namespace SqlBatis.Queryables
{
    public class MysqlQueryable<T> : IDbQueryable<T>
    {
        #region fields

        private readonly Dictionary<string, object> _parameters
            = new Dictionary<string, object>();

        private readonly PageData _page = new PageData();

        private readonly IDbContext _context = null;

        private Expression _filterExpression = null;
     
        private readonly List<Expression> _whereExpressions = new List<Expression>();

        private readonly List<SetExpression> _setExpressions = new List<SetExpression>();

        private readonly List<OrderExpression> _orderExpressions = new List<OrderExpression>();

        private Expression _selectExpression = null;

        private Expression _groupExpression = null;

        private Expression _havingExpression = null;

        public MysqlQueryable(IDbContext context)
        {
            _context = context;
        }
        #endregion

        #region resovle
        private void ResovleParameter(T entity)
        {
            var serializer = TypeConvert.GetDeserializer(typeof(T));
            var values = serializer(entity);
            foreach (var item in values)
            {
                _parameters.Add(item.Key, item.Value);
            }
        }
      
        private string ResolveSelect()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var columns = ResolveColumns();
            var where = ResolveWhere();
            var group = ResolveGroup();
            var having = ResolveHaving();
            var order = ResolveOrder();
            var limit = ResovleLimit();
            var sql = $"SELECT {columns} FROM {table}{where}{group}{having}{order}{limit}";
            return sql;
        }

        private string ResovleInsert()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var filters = new GroupExpressionResovle(_filterExpression).Resovle().Split(',');
            var columns = TableInfoCache.GetColumns(typeof(T));
            var intcolumns = columns
                .Where(a => !filters.Contains(a.ColumnName))
                .Where(a => !a.IsIdentity);
            var intcolumnNames = string.Join(",", intcolumns.Select(s => s.ColumnName));
            var intcolumnParameters = string.Join(",", intcolumns.Select(s => $"@{s.PropertyName}"));
            var sql = $"INSERT INTO {table}({intcolumnNames}) VALUES ({intcolumnParameters})";
            return sql;
        }

        private string ResolveUpdate()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var builder = new StringBuilder();
            if (_setExpressions.Count > 0)
            {
                var where = ResolveWhere();
                foreach (var item in _setExpressions)
                {
                    var column = new BooleanExpressionResovle(item.Column).Resovle();
                    var expression = new BooleanExpressionResovle(item.Expression, _parameters).Resovle();
                    builder.Append($"{column} = {expression},");
                }
                var sql = $"UPDATE {table} SET {builder.ToString().Trim(',')}{where}";
                return sql;
            }
            else
            {
                var filters = new GroupExpressionResovle(_filterExpression).Resovle().Split(',');
                var where = ResolveWhere();
                var columns = TableInfoCache.GetColumns(typeof(T));
                var updcolumns = columns
                    .Where(a => !a.IsIdentity && a.Key != Attributes.ColumnKey.Primary)
                    .Where(a => !filters.Contains(a.ColumnName))
                    .Select(s => $"{s.ColumnName} = @{s.PropertyName}");
                if (string.IsNullOrEmpty(where))
                {
                    var key = (columns.Where(a => a.Key == Attributes.ColumnKey.Primary).FirstOrDefault()
                        ?? columns.First());
                    where = $" WHERE {key.ColumnName} = @{key.PropertyName}";
                }
                var sql = $"UPDATE {table} SET {string.Join(",", updcolumns)}{where}";
                return sql;
            }
        }

        private string ResovleDelete()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var where = ResolveWhere();
            var sql = $"DELETE FROM {table}{where}";
            return sql;
        }

        private string ResovleExists()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var where = ResolveWhere();
            var group = ResolveGroup();
            var having = ResolveHaving();
            var sql = $"SELECT 1 WHERE EXISTS(SELECT 1 FROM {table}{where}{group}{having})";
            return sql;
        }

        private string ResolveColumns()
        {
            if (_selectExpression == null)
            {
                var filters = new GroupExpressionResovle(_filterExpression).Resovle().Split(',');
                var columns = TableInfoCache.GetColumns(typeof(T))
                    .Where(a => !filters.Contains(a.ColumnName))
                    .Select(s => $"{s.ColumnName} AS {s.PropertyName}");
                return string.Join(",", columns);
            }
            else
            {
                return new SelectExpressionResovle(_selectExpression).Resovle();
            }
        }

        private string ResolveWhere()
        {
            var builder = new StringBuilder();
            foreach (var expression in _whereExpressions)
            {
                var result = new BooleanExpressionResovle(expression, _parameters).Resovle();
                if (expression == _whereExpressions.First())
                {
                    builder.Append($" WHERE {result}");
                }
                else
                {
                    builder.Append($" AND {result}");
                }
            }
            return builder.ToString();
        }

        private string ResolveGroup()
        {
            if (_groupExpression != null)
            {
                var result = new GroupExpressionResovle(_groupExpression).Resovle();
                return $" GROUP BY {result}";
            }
            return string.Empty;
        }

        private string ResolveHaving()
        {
            if (_havingExpression != null)
            {
                var result = new BooleanExpressionResovle(_havingExpression, _parameters).Resovle();
                return $" HAVING {result}";
            }
            return string.Empty;
        }

        private string ResolveOrder()
        {
            var buffer = new StringBuilder();
            foreach (var item in _orderExpressions)
            {
                if (item == _orderExpressions.First())
                {
                    buffer.Append($" ORDER BY ");

                }
                var result = new OrderExpressionResovle(item.Expression, item.Asc).Resovle();
                buffer.Append(result);
                buffer.Append(",");
            }
            return buffer.ToString().Trim(',');
        }

        private string ResovleLimit()
        {
            if (_page.Index != -1)
            {
                return $" LIMIT {_page.Index},{_page.Count}";
            }
            return string.Empty;
        }



        class PageData
        {
            public int Index { get; set; } = -1;
            public int Count { get; set; }
        }

        class OrderExpression
        {
            public bool Asc { get; set; } = true;
            public Expression Expression { get; set; }
        }

        class SetExpression
        {
            public Expression Column { get; set; }
            public Expression Expression { get; set; }
        }
        #endregion

        #region implement

        public int Insert(T entity)
        {
            ResovleParameter(entity);
            var sql = ResovleInsert();
            return _context.ExecuteNonQuery(sql,_parameters);
        }

        public int Insert(IEnumerable<T> entitys)
        {
            var count = 0;
            foreach (var item in entitys)
            {
                count += Insert(item);
            }
            return count;
        }

        public int Update(T entity)
        {
            ResovleParameter(entity);
            var sql = ResolveUpdate();
            return _context.ExecuteNonQuery(sql, _parameters);
        }

        public int Update(int? commandTimeout = null)
        {
            if (_setExpressions.Count > 0)
            {
                var sql = ResolveUpdate();
                return _context.ExecuteNonQuery(sql, _parameters, commandTimeout);
            }
            return default;
        }

        public IDbQueryable<T> Set<TResult>(Expression<Func<T, TResult>> column, TResult value, bool condition = true)
        {
            if (true)
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
            if (true)
            {
                _setExpressions.Add(new SetExpression
                {
                    Column = column,
                    Expression = expression
                });
            }
            return this;
        }

        public int Delete(int? commandTimeout = null)
        {
            var sql = ResovleDelete();
            return _context.ExecuteNonQuery(sql, _parameters, commandTimeout);
        }

        public bool Exists(int? commandTimeout = null)
        {
            var sql = ResovleExists();
            return _context.ExecuteScalar<bool>(sql, _parameters, commandTimeout);
        }

        public bool Exists(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return Exists();
        }

        public IDbQueryable<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression)
        {
            _groupExpression = expression;
            return this;
        }

        public IDbQueryable<T> Having(Expression<Func<T, bool>> expression)
        {
            _havingExpression = expression;
            return this;
        }

        public IDbQueryable<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression)
        {
            _orderExpressions.Add(new OrderExpression
            {
                Asc = true,
                Expression = expression
            });
            return this;
        }

        public IDbQueryable<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression)
        {
            _orderExpressions.Add(new OrderExpression
            {
                Asc = false,
                Expression = expression
            });
            return this;
        }

        public IDbQueryable<T> Filter<TResult>(Expression<Func<T, TResult>> column)
        {
            _filterExpression = column;
            return this;
        }

        public IDbQueryable<T> Page(int index, int count)
        {
            Skip((index - 1) * count, count);
            return this;
        }

        public IEnumerable<T> Select(int? commandTimeout = null)
        {
            var sql = ResolveSelect();
            return _context.ExecuteQuery<T>(sql, _parameters);
        }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            _selectExpression = expression;
            var sql = ResolveSelect();
            return _context.ExecuteQuery<TResult>(sql, _parameters, commandTimeout);
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

        public IDbQueryable<T> Skip(int index, int count)
        {
            _page.Index = index;
            _page.Count = count;
            return this;
        }

        public IDbQueryable<T> Take(int count)
        {
            Skip(0, count);
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
       
    }
}
