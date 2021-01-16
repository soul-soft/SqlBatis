using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis.Expressions
{
    internal enum DbExpressionType
    {
        Where,
        Order,
        Group,
        Having,
        Set
    }
    internal class DbExpression
    {
        public readonly Expression Expression;
        public readonly DbExpressionType ExpressionType;
        public DbExpression(Expression expression, DbExpressionType expressionType)
        {
            Expression = expression;
            ExpressionType = expressionType;
        }
    }
    internal class DbExpressionCollection : ICollection<DbExpression>
    {
        private readonly ICollection<DbExpression> _list = new List<DbExpression>();
        public IEnumerable<DbWhereExpression> GetWhereExpressions()
        {
            return _list.Where(a => a.ExpressionType == DbExpressionType.Where).Select(s=>s as DbWhereExpression);
        }
        public IEnumerable<DbOrderExpression> GetOrderExpressions()
        {
            return _list.Where(a => a.ExpressionType == DbExpressionType.Order).Select(s => s as DbOrderExpression);
        }
        public IEnumerable<DbGroupExpression> GetGroupExpressions()
        {
            return _list.Where(a => a.ExpressionType == DbExpressionType.Group).Select(s => s as DbGroupExpression);
        }
        public IEnumerable<DbHavingExpression> GetHavingExpressions()
        {
            return _list.Where(a => a.ExpressionType == DbExpressionType.Having).Select(s => s as DbHavingExpression);
        }
        public IEnumerable<DbSetExpression> GetSetExpressions()
        {
            return _list.Where(a => a.ExpressionType == DbExpressionType.Set).Select(s => s as DbSetExpression);
        }
        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public void Add(DbExpression item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(DbExpression item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(DbExpression[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DbExpression> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool Remove(DbExpression item)
        {
            return _list.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
    }
    internal class DbWhereExpression : DbExpression
    {
        public DbWhereExpression(Expression expression)
            : base(expression, DbExpressionType.Where)
        {

        }
    }
    internal class DbOrderExpression : DbExpression
    {
        public bool Asc = false;
        public DbOrderExpression(Expression expression,bool asc)
            : base(expression, DbExpressionType.Order)
        {
            Asc = asc;
        }
    }
    internal class DbGroupExpression : DbExpression
    {
        public DbGroupExpression(Expression expression)
            : base(expression, DbExpressionType.Group)
        {
        }
    }
    internal class DbHavingExpression : DbExpression
    {
        public DbHavingExpression(Expression expression)
            : base(expression, DbExpressionType.Having)
        {
        }
    }
    internal class DbSetExpression : DbExpression
    {
        public Expression Column;
        public DbSetExpression(Expression column,Expression expression)
            : base(expression, DbExpressionType.Set)
        {
            Column = column;
        }
    }
   
}
