using SqlBatis.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using SqlBatis.Expressions.Resovles;
using SqlBatis.Expressions;
using System.Threading.Tasks;

namespace SqlBatis.Queryables
{
    public partial class DbQuery<T> : IDbQuery<T>
    {
        #region fields

        private readonly Dictionary<string, object> _parameters
            = new Dictionary<string, object>();

        private readonly PageData _page = new PageData();

        private string _lockname = string.Empty;

        private readonly IDbContext _context = null;

        private readonly List<Expression> _whereExpressions = new List<Expression>();

        private readonly List<SetExpression> _setExpressions = new List<SetExpression>();

        private readonly List<OrderExpression> _orderExpressions = new List<OrderExpression>();

        private readonly List<Expression> _groupExpressions = new List<Expression>();

        private readonly List<Expression> _havingExpressions = new List<Expression>();

        private Expression _filterExpression = null;

        private Expression _selectExpression = null;

        private Expression _countExpression = null;

        public DbQuery(IDbContext context)
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

        private string ResovleCount()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var column = "COUNT(1)";
            var where = ResolveWhere();
            var group = ResolveGroup();
            if (group.Length > 0)
            {
                column = group.Remove(0, 10);
            }
            else if (_countExpression != null)
            {
                column = new SelectExpressionResovle(_countExpression).Resovle();
            }
            var sql = $"SELECT {column} FROM {table}{where}{group}";
            if (group.Length > 0)
            {
                sql = $"SELECT COUNT(1) FROM ({sql}) as t";
                return sql;
            }
            return sql;
        }

        private string ResolveSelect()
        {
            var table = TableInfoCache.GetTable(typeof(T)).Name;
            var column = ResolveColumns();
            var where = ResolveWhere();
            var group = ResolveGroup();
            var having = ResolveHaving();
            var order = ResolveOrder();
            string sql;
            if (_context.DbContextType == DbContextType.SqlServer)
            {
                if (_lockname != string.Empty)
                {
                    _lockname = $" WITH({_lockname})";
                }
                if (_page.Index == 0)
                {
                    sql = $"SELECT TOP {_page.Count} {column} FROM {table}{_lockname}{where}{group}{having}{order}";
                }
                else if (_page.Index > 0)
                {
                    if (order == string.Empty)
                    {
                        order = " ORDER BY (SELECT 1)";
                    }
                    var limit = $" OFFSET {_page.Index} ROWS FETCH NEXT {_page.Count} ROWS ONLY";
                    sql = $"SELECT {column} FROM {_lockname}{table}{where}{group}{having}{order}{limit}";
                }
                else
                {
                    sql = $"SELECT {column} FROM {_lockname}{table}{where}{group}{having}{order}";
                }
            }
            else
            {
                var limit = _page.Index > 0 ? " LIMIT " : string.Empty;
                sql = $"SELECT {column} FROM {table}{where}{group}{having}{order}{limit}{_lockname}";
            }
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
            var buffer = new StringBuilder();
            foreach (var item in _groupExpressions)
            {
                var result = new GroupExpressionResovle(item).Resovle();
                buffer.Append($"{result},");
            }
            var sql = string.Empty;
            if (buffer.Length > 0)
            {
                buffer.Remove(buffer.Length - 1, 1);
                sql = $" GROUP BY {buffer.ToString()}";
            }
            return sql;
        }

        private string ResolveHaving()
        {
            var buffer = new StringBuilder();
            foreach (var item in _havingExpressions)
            {
                var result = new BooleanExpressionResovle(item, _parameters).Resovle();
                if (item == _havingExpressions.First())
                {
                    buffer.Append($" HAVING {result}");
                }
                else
                {
                    buffer.Append($" AND {result}");
                }
            }
            return buffer.ToString();
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

    }
}
