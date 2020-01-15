using SqlBatis.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SqlBatis.Expressions.Resovles
{
    public class BooleanExpressionResovle : ExpressionResovle
    {
        private readonly string _prefix = "@";

        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public BooleanExpressionResovle(Expression expression)
            : base(expression)
        {
            _parameters = new Dictionary<string, object>();
        }

        public BooleanExpressionResovle(Expression expression, Dictionary<string, object> parameters)
            : base(expression)
        {
            _parameters = parameters;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression?.NodeType == ExpressionType.Parameter)
            {
                SetParameterName(node);
            }
            else
            {
                SetParameterValue(node);
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Operator))
            {
                if (node.Arguments.Count == 2)
                {
                    _textBuilder.Append("(");
                    SetParameterName(node.Arguments[0] as MemberExpression);
                    var type = Operator.ResovleExpressionType(node.Method.Name);
                    _textBuilder.Append($" {type} ");
                    var value = VisitConstantValue(node.Arguments[1]);
                    if (node.Method.Name == nameof(Operator.StartsWith) || node.Method.Name == nameof(Operator.NotStartsWith))
                    {
                        SetParameterValue(Expression.Constant($"{value}%", typeof(string)));
                    }
                    else if (node.Method.Name == nameof(Operator.StartsWith) || node.Method.Name == nameof(Operator.NotStartsWith))
                    {
                        SetParameterValue(Expression.Constant($"{value}%", typeof(string)));
                    }
                    else if (node.Method.Name == nameof(Operator.Contains) || node.Method.Name == nameof(Operator.NotContains))
                    {
                        SetParameterValue(Expression.Constant($"{value}%", typeof(string)));
                    }
                    else
                    {
                        SetParameterValue(Expression.Constant(value));
                    }
                    _textBuilder.Append(")");
                }
            }
            else if (node.Method.DeclaringType.GetCustomAttribute(typeof(FunctionAttribute), true) != null)
            {
                var function = new FunctionExpressionResovle(node).Resovle();
                _textBuilder.Append(function);
            }
            else
            {
                SetParameterValue(node);
            }
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _textBuilder.Append("(");
            Visit(node.Left);
            if (node.Right is ConstantExpression right && right.Value == null && (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual))
            {
                _textBuilder.AppendFormat(" {0}", node.NodeType == ExpressionType.Equal ? "IS NULL" : "IS NOT NULL");
            }
            else
            {
                _textBuilder.Append($" {Operator.ResovleExpressionType(node.NodeType)} ");
                Visit(node.Right);
            }
            _textBuilder.Append(")");
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            SetParameterValue(node);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                _textBuilder.AppendFormat("{0} ", Operator.ResovleExpressionType(ExpressionType.Not));
                Visit(node.Operand);
            }
            else
            {
                Visit(node.Operand);
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            SetParameterValue(node);
            return node;
        }

        private void SetParameterName(MemberExpression expression)
        {
            var name = TableInfoCache
                .GetColumnName(expression.Member.DeclaringType, expression.Member.Name);
            _textBuilder.Append(name);
        }

        private void SetParameterValue(Expression expression)
        {
            var value = VisitConstantValue(expression);
            var parameterName = $"P_{_parameters.Count}";
            _parameters.Add(parameterName, value);
            _textBuilder.Append($"{_prefix}{parameterName}");
        }
    }
}
