using SqlBatis.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SqlBatis.Queryables
{
    internal class PageData
    {
        public int Index { get; set; } = -1;
        public int Count { get; set; }
    }

    internal class OrderExpression
    {
        public bool Asc { get; set; } = true;
        public Expression Expression { get; set; }
    }

    internal class SetExpression
    {
        public Expression Column { get; set; }
        public Expression Expression { get; set; }
    }

    public abstract class Queryable
    {
        #region fields
        protected readonly bool _isSingleTable = true;
        private StringBuilder _viewName;
        protected readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        internal readonly PageData _page = new PageData();
        protected string _lockname = string.Empty;
        protected readonly IDbContext _context = null;
        protected readonly List<Expression> _whereExpressions = new List<Expression>();
        internal readonly List<OrderExpression> _orderExpressions = new List<OrderExpression>();
        protected readonly List<Expression> _groupExpressions = new List<Expression>();
        protected readonly List<Expression> _havingExpressions = new List<Expression>();
        public Queryable(IDbContext context, bool isSingleTable)
        {
            _context = context;
            _isSingleTable = isSingleTable;
        }
        #endregion

        #region resovles
        protected string GetSingleTableName<T>()
        {
            return SqlBatisSettings.DbMetaInfoProvider.GetTable(typeof(T)).TableName;
        }
        protected List<DbColumnMetaInfo> GetSingleTableColumnMetaInfos<T>()
        {
            return SqlBatisSettings.DbMetaInfoProvider.GetColumns(typeof(T));
        }
        protected void SetViewName(string viewName)
        {
            _viewName = new StringBuilder(viewName);
        }
        protected void AddViewName(string viewName)
        {
            if (_viewName == null)
            {
                _viewName = new StringBuilder();
            }
            if (_viewName.Length > 0)
            {
                _viewName.Append(" ");
            }
            _viewName.Append(viewName);
        }
        protected string GetViewName()
        {
            return _viewName.ToString();
        }
        protected string BuildWhereExpression()
        {
            var builder = new StringBuilder();
            foreach (var expression in _whereExpressions)
            {
                var result = new BooleanExpressionResovle(_isSingleTable, expression, _parameters).Resovle();
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
        protected string BuildGroupExpression()
        {
            var buffer = new StringBuilder();
            foreach (var item in _groupExpressions)
            {
                var result = new GroupExpressionResovle(_isSingleTable, item).Resovle();
                buffer.Append($"{result},");
            }
            var sql = string.Empty;
            if (buffer.Length > 0)
            {
                buffer.Remove(buffer.Length - 1, 1);
                sql = $" GROUP BY {buffer}";
            }
            return sql;
        }
        protected string BuildHavingExpression()
        {
            var buffer = new StringBuilder();
            foreach (var item in _havingExpressions)
            {
                var result = new BooleanExpressionResovle(_isSingleTable, item, _parameters).Resovle();
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
        protected string BuildOrderExpression()
        {
            var buffer = new StringBuilder();
            foreach (var item in _orderExpressions)
            {
                if (item == _orderExpressions.First())
                {
                    buffer.Append($" ORDER BY ");
                }
                var result = new OrderExpressionResovle(_isSingleTable, item.Expression, item.Asc).Resovle();
                buffer.Append(result);
                buffer.Append(",");
            }
            return buffer.ToString().Trim(',');
        }
        protected string BuildCountCommand(Expression expression = null)
        {
            var table = _viewName.ToString();
            var column = "COUNT(1)";
            var where = BuildWhereExpression();
            var group = BuildGroupExpression();
            if (group.Length > 0)
            {
                column = group.Remove(0, 10);
            }
            else if (expression != null)
            {
                column = new SelectExpressionResovle(_isSingleTable, expression).Resovle();
            }
            var sql = $"SELECT {column} FROM {table}{where}{group}";
            if (group.Length > 0)
            {
                sql = $"SELECT COUNT(1) FROM ({sql}) as t";
                return sql;
            }
            return sql;
        }
        protected string BuildSumCommand(Expression expression)
        {
            var table = _viewName.ToString();
            var column = $"SUM({new SelectExpressionResovle(_isSingleTable, expression).Resovle()})";
            var where = BuildWhereExpression();
            var sql = $"SELECT {column} FROM {table}{where}";
            return sql;
        }
        protected string BuildSelectCommand(Expression expression)
        {
            var table = _viewName.ToString();
            var column = new SelectExpressionResovle(_isSingleTable, expression).Resovle();
            var where = BuildWhereExpression();
            var group = BuildGroupExpression();
            var having = BuildHavingExpression();
            var orderBy = BuildOrderExpression();
            string sql;
            if (_context.DbContextType == DbContextType.SqlServer2008 || _context.DbContextType == DbContextType.SqlServer2012)
            {
                if (_lockname != string.Empty)
                {
                    _lockname = $" WITH({_lockname})";
                }
                //第一页
                if (_page.Index == 0)
                {
                    sql = $"SELECT TOP {_page.Count} {column} FROM {table}{_lockname}{where}{group}{having}{orderBy}";
                }
                else if (_page.Index > 0)//大于一页
                {
                    if (orderBy == string.Empty)//如果未指定排序
                    {
                        orderBy = " ORDER BY (SELECT 1)";
                    }
                    if (_context.DbContextType == DbContextType.SqlServer2008)
                    {
                        var rownumber = $"ROW_NUMBER() OVER ({orderBy}) AS RowNumber";
                        var offset = $"WHERE RowNumber > {_page.Count * (_page.Index - 1)}";
                        sql = $"SELECT TOP {_page.Count} * FROM (SELECT {column},{rownumber} FROM {_lockname}{table}{where}{group}{having}) AS t {offset}";
                    }
                    else
                    {
                        var offset = $" OFFSET {(_page.Index - 1) * _page.Count} ROWS FETCH NEXT {_page.Count} ROWS ONLY";
                        sql = $"SELECT {column} FROM {_lockname}{table}{where}{group}{having}{orderBy}{offset}";
                    }
                }
                else//不分页
                {
                    sql = $"SELECT {column} FROM {_lockname}{table}{where}{group}{having}{orderBy}";
                }
            }
            else
            {
                var offset = _page.Index > 0 || _page.Count > 0 ? $" LIMIT {_page.Index},{_page.Count}" : string.Empty;
                sql = $"SELECT {column} FROM {table}{where}{group}{having}{orderBy}{offset}{_lockname}";
            }
            return sql;
        }
        protected string BuildExistsCommand()
        {
            var table = _viewName.ToString();
            var where = BuildWhereExpression();
            var group = BuildGroupExpression();
            var having = BuildHavingExpression();
            if (_context.DbContextType != DbContextType.Mysql)
            {
                return $"SELECT 1 WHERE EXISTS(SELECT 1 FROM {table}{where}{group}{having})";
            }
            else
            {
                return $"SELECT EXISTS(SELECT 1 FROM {table}{where}{group}{having}) as flag";
            }
        }
        #endregion
    }
}
