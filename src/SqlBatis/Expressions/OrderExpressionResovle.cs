using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBatis.Expressions
{
    /// <summary>
    /// 排序表达式解析
    /// </summary>
    public class OrderExpressionResovle : ExpressionResovle
    {
        private readonly List<string> _list = new List<string>();

        private readonly string _asc = string.Empty;

        private readonly bool _single;

        private readonly Expression _expression;

        public OrderExpressionResovle(bool single, Expression expression, bool asc)
            : base(single)
        {
            _expression = expression;
            _single = single;
            if (!asc)
            {
                _asc = " DESC";
            }
        }

        protected override Expression VisitNew(NewExpression node)
        {
            foreach (var item in node.Arguments)
            {
                Visit(item);
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var result = new FunctionExpressionResovle(_single, node).Resovle();
            _list.Add($"{result}{_asc}");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var name = GetDbColumnNameAsAlias(node);
            _list.Add($"{name}{_asc}");
            return node;
        }

        public override string Resovle()
        {
            Visit(_expression);
            return string.Join(",", _list);
        }
    }
}
