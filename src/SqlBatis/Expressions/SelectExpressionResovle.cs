using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBatis.Expressions
{
    public class SelectExpressionResovle : ExpressionResovle
    {
        private readonly bool _single;

        private readonly List<string> _list = new List<string>();

        private readonly Expression _expression;
      
        public SelectExpressionResovle(bool single, Expression expression)
         : base(single)
        {
            _single = single;
            _expression = expression;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            for (int i = 0; i < node.Bindings.Count; i++)
            {
                var item = node.Bindings[i] as MemberAssignment;

                if (item.Expression is MemberExpression mExp)
                {
                    var name = GetDbColumnNameAsAlias(mExp);
                    _list.Add($"{name} AS {item.Member.Name}");
                }
                else if (item.Expression is MethodCallExpression)
                {
                    var expression = new FunctionExpressionResovle(_single, item.Expression).Resovle();
                    _list.Add($"{expression} AS {item.Member.Name}");
                }
            }
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var item = node.Arguments[i];
                var column = node.Members[i].Name;
                if (item is MemberExpression member)
                {
                    var name = GetDbColumnNameAsAlias(member);
                    if (name != column)
                    {
                        _list.Add($"{name} AS {column}");
                    }
                    else
                    {
                        _list.Add(name);
                    }
                }
                else if (item is MethodCallExpression)
                {
                    var expression = new FunctionExpressionResovle(_single, item).Resovle();
                    _list.Add($"{expression} AS {column}");
                }
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var name = GetDbColumnNameAsAlias(node);
            _list.Add(name);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var result = new FunctionExpressionResovle(_single, node).Resovle();
            _list.Add($"{result} AS expr");
            return node;
        }        
        public override string Resovle()
        {
            if (_expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value.ToString();
            }
            Visit(_expression);
            return string.Join(",", _list);
        }
    }
}
