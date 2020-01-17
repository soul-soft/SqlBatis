using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SqlBatis
{
    public class Operator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool In<T>(T column, IEnumerable<T> list) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool In<T>(T column, params T[] values) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]     
        public static bool NotIn<T>(T column, IEnumerable<T> enumerable) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool NotIn<T>(T column, params T[] values) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool Contains(string column, string text) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool NotContains(string column, string text) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool StartsWith(string column, string text) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool NotStartsWith(string column, string text) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool EndsWith(string column, string text) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool NotEndsWith(string column, string text) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool Regexp(string column, string regexp) => default;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("样式", "IDE0060:删除未使用的参数", Justification = "<挂起>")]
        public static bool NotRegexp(string column, string regexp) => default;
        public static string ResovleExpressionType(ExpressionType type)
        {
            var condition = string.Empty;
            switch (type)
            {
                case ExpressionType.Add:
                    condition = "+";
                    break;
                case ExpressionType.Subtract:
                    condition = "-";
                    break;
                case ExpressionType.Multiply:
                    condition = "*";
                    break;
                case ExpressionType.Divide:
                    condition = "/";
                    break;
                case ExpressionType.Modulo:
                    condition = "%";
                    break;
                case ExpressionType.Equal:
                    condition = "=";
                    break;
                case ExpressionType.NotEqual:
                    condition = "<>";
                    break;
                case ExpressionType.GreaterThan:
                    condition = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    condition = ">=";
                    break;
                case ExpressionType.LessThan:
                    condition = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    condition = "<=";
                    break;
                case ExpressionType.OrElse:
                    condition = "OR";
                    break;
                case ExpressionType.AndAlso:
                    condition = "AND";
                    break;
                case ExpressionType.Not:
                    condition = "NOT";
                    break;
            }
            return condition;
        }
        public static string ResovleExpressionType(string type)
        {
            switch (type)
            {
                case nameof(Operator.In):
                    type = "IN";
                    break;
                case nameof(Operator.NotIn):
                    type = "NOT IN";
                    break;
                case nameof(Operator.Contains):
                case nameof(Operator.StartsWith):
                case nameof(Operator.EndsWith):
                    type = "LIKE";
                    break;
                case nameof(Operator.NotContains):
                case nameof(Operator.NotStartsWith):
                case nameof(Operator.NotEndsWith):
                    type = "NOT LIKE";
                    break;
                case nameof(Operator.Regexp):
                    type = "REGEXP";
                    break;
                case nameof(Operator.NotRegexp):
                    type = "NOT REGEXP";
                    break;
            }
            return type;
        }
    }
}
