using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBatis.Expressions
{
    public class IgnoreExpressionResovle : ExpressionResovle
    {
        private readonly List<string> _list = new List<string>();
     
        private readonly Expression _expression;
    
        public IgnoreExpressionResovle(Expression expression)
            : base(true)
        {
            _expression = expression;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            foreach (var item in node.Arguments)
            {
                Visit(item);
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var name = GetDbColumnNameAsAlias(node);
            _list.Add(name);
            return node;
        }

        public List<string> Resovles()
        {
            Visit(_expression);
            return _list;
        }

        public override string Resovle()
        {
            throw new NotImplementedException();
        }
    }
}
