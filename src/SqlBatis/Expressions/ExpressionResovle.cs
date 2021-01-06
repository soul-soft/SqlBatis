using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SqlBatis.Expressions
{
    /// <summary>
    /// 数据表达式解析基类
    /// </summary>
    public abstract class ExpressionResovle : ExpressionVisitor
    {
        /// <summary>
        /// 是否单表操作
        /// </summary>
        private readonly bool _singleTable;
        /// <summary>
        /// 表别名
        /// </summary>
        private readonly Dictionary<Type, string> _tableAliasNames
            = new Dictionary<Type, string>();
        /// <summary>
        /// 数据库表达式解析基类
        /// </summary>
        /// <param name="singleTable">是否单表操作</param>
        protected ExpressionResovle(bool singleTable)
        {
            _singleTable = singleTable;
        }

        /// <summary>
        /// 解析常量表达式，并返回值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object VisitExpressionValue(Expression expression)
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

        /// <summary>
        /// 获取数据库字段名转换成别名
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected string GetDbColumnNameAsAlias(MemberExpression expression)
        {
            var tableType = expression.Member.DeclaringType;
            var fieldName = expression.Member.Name;
            var columns = SqlBatisSettings.DbMetaInfoProvider.GetColumns(tableType);
            var column = columns.Where(a => a.CsharpName == fieldName)
                .FirstOrDefault().ColumnName;
            if (_singleTable)
            {
                return column;
            }
            var aliasName = (expression.Expression as ParameterExpression).Name;
            if (!_tableAliasNames.ContainsKey(tableType))
            {
                _tableAliasNames.Add(tableType, aliasName);
            }
            return $"{aliasName}.{column}";
        }

        /// <summary>
        /// 获取数据库表转换成别名名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetDbTableNameAsAlias(Type type)
        {
            var tableName = SqlBatisSettings.DbMetaInfoProvider.GetTable(type).TableName;
            if (_singleTable)
            {
                return tableName;
            }
            var aliasName = _tableAliasNames[type];
            return $"{tableName} AS {aliasName}";
        }

        /// <summary>
        /// 解析出一个字符串
        /// </summary>
        /// <returns></returns>
        public abstract string Resovle();
    }
}
