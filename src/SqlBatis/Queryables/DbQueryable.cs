using SqlBatis.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SqlBatis.Queryables
{
    public abstract class DbQueryable
    {
        #region fields
        protected readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        protected readonly bool _isSingleTable = true;
        private string _viewName = string.Empty;
        private string _lockname = string.Empty;
        private readonly PageData _page = new PageData();
        protected readonly IDbContext _context;
        internal readonly DbExpressionCollection _expressions = new DbExpressionCollection();
        public DbQueryable(IDbContext context, bool isSingleTable)
        {
            _context = context;
            _isSingleTable = isSingleTable;
        }
        #endregion

        #region resovles     
        protected string BuildWhereExpression()
        {
            var builder = new StringBuilder();
            var expressions = _expressions.GetWhereExpressions();
            var first = true;
            foreach (var expression in expressions)
            {
                var result = new BooleanExpressionResovle(_isSingleTable, expression.Expression, _parameters).Resovle();
                if (first)
                {
                    first = false;
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
            var expressions = _expressions.GetGroupExpressions();
            foreach (var item in expressions)
            {
                var result = new GroupExpressionResovle(_isSingleTable, item.Expression).Resovle();
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
            var expressions = _expressions.GetHavingExpressions();
            var first = true;
            foreach (var item in expressions)
            {
                var result = new BooleanExpressionResovle(_isSingleTable, item.Expression, _parameters).Resovle();
                if (first)
                {
                    first = false;
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
            var expressions = _expressions.GetOrderExpressions();
            var first = true;
            foreach (var item in expressions)
            {
                if (first)
                {
                    first = false;
                    buffer.Append($" ORDER BY ");
                }
                var result = new OrderExpressionResovle(_isSingleTable, item.Expression, item.Asc).Resovle();
                buffer.Append(result);
                buffer.Append(',');
            }
            return buffer.ToString().Trim(',');
        }
        protected List<string> BuildIgnoreExpression()
        {
            var result = new List<string>();
            var expressions = _expressions.GetIgnoreExpressions();
            foreach (var item in expressions)
            {
                var list = new IgnoreExpressionResovle(item.Expression).Resovles();
                result.AddRange(list);
            }
            return result;
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
                        var offset = $"WHERE RowNumber > {_page.Index}";
                        sql = $"SELECT TOP {_page.Count} * FROM (SELECT {column},{rownumber} FROM {_lockname}{table}{where}{group}{having}) AS t {offset}";
                    }
                    else
                    {
                        var offset = $" OFFSET {_page.Index} ROWS FETCH NEXT {_page.Count} ROWS ONLY";
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

        #region protected
        protected void SetPage(int index, int count)
        {
            _page.Index = index;
            _page.Count = count;
        }
        protected void AppendViewName(string viewName)
        {
            if (_viewName.Length > 0)
            {
                _viewName += $" {viewName}";
            }
            else
            {
                _viewName = viewName;
            }
        }
        protected void SetLockName(string lockname)
        {
            _lockname = lockname;
        }
        protected string GetViewName()
        {
            return _viewName;
        }
        protected void AddWhereExpression(Expression expression)
        {
            _expressions.Add(new DbWhereExpression(expression));
        }
        protected void AddOrderExpression(Expression expression, bool asc)
        {
            _expressions.Add(new DbOrderExpression(expression, asc));
        }
        protected void AddGroupExpression(Expression expression)
        {
            _expressions.Add(new DbGroupExpression(expression));
        }
        protected void AddHavingExpression(Expression expression)
        {
            _expressions.Add(new DbHavingExpression(expression));
        }
        protected void AddIgnoreExpression(Expression expression)
        {
            _expressions.Add(new DbIgnoreExpression(expression));
        }
        #endregion

        #region class
        class PageData
        {
            public int Index { get; set; } = -1;
            public int Count { get; set; }
        }
        #endregion
    }
}
