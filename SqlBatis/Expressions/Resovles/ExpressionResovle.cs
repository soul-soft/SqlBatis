using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SqlBatis.Expressions.Resovles
{
    public abstract class ExpressionResovle : ExpressionVisitor
    {
        protected ExpressionResovle(Expression expression)
        {
            _expression = expression;
        }

        protected readonly Expression _expression = null;

        protected readonly StringBuilder _textBuilder = new StringBuilder();

        public object VisitConstantValue(Expression expression)
        {
            var names = new Stack<string>();
            var exps = new Stack<Expression>();
            var mifs = new Stack<MemberInfo>();
            if (expression is ConstantExpression constant)
                return constant.Value;
            else if (expression is MemberExpression)
            {
                var temp = expression;
                object value = null;
                while (temp is MemberExpression memberExpression)
                {
                    names.Push(memberExpression.Member.Name);
                    exps.Push(memberExpression.Expression);
                    mifs.Push(memberExpression.Member);
                    temp = memberExpression.Expression;
                }
                foreach (var name in names)
                {
                    var exp = exps.Pop();
                    var mif = mifs.Pop();
                    if (exp is ConstantExpression cex)
                        value = cex.Value;
                    if (mif is PropertyInfo pif)
                        value = pif.GetValue(value);
                    else if (mif is FieldInfo fif)
                        value = fif.GetValue(value);
                }
                return value;
            }
            else
            {
                return Expression.Lambda(expression).Compile().DynamicInvoke();
            }
        }

        public virtual string Resovle()
        {
            Visit(_expression);
            return _textBuilder.ToString();
        }
    }
}
