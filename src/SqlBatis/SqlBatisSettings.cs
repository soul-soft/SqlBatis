using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public static class SqlBatisSettings
    {
        /// <summary>
        /// 是否允许常量表达式的结果为：默认不允许将抛出NullReferenceException
        /// </summary>
        public static bool AllowConstantExpressionResultIsNull { get; set; } = false;
       
        /// <summary>
        /// 是否忽略DbCommand中的无效参数
        /// </summary>
        public static bool IgnoreDbCommandInvalidParameters { get; set; } = false;
        
        /// <summary>
        /// 数据库元信息提供程序
        /// </summary>
        internal static IDbMetaInfoProvider DbMetaInfoProvider { get; set; }
            = new AnnotationDbMetaInfoProvider();
    }
}
