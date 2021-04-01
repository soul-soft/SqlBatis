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
        public static bool AllowConstantExpressionResultIsNull { get; set; } = true;
       
        /// <summary>
        /// 是否忽略DbCommand中的无效参数
        /// </summary>
        public static bool IgnoreDbCommandInvalidParameters { get; set; } = false;
        
        /// <summary>
        /// 数据库元信息提供程序
        /// </summary>
        public static IDbMetaInfoProvider DbMetaInfoProvider { get; set; }
            = new AnnotationDbMetaInfoProvider();
        
        /// <summary>
        /// 实体映射器
        /// </summary>
        public static DbEntityMapperProvider DbEntityMapperProvider { get; set; }
            = new DbEntityMapperProvider();
    }
}
