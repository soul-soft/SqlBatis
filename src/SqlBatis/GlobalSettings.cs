using SqlBatis.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public static class GlobalSettings
    {
        /// <summary>
        /// 数据库元信息提供程序
        /// </summary>
        public static IDatabaseMetaInfoProvider DatabaseMetaInfoProvider { get; set; }
            = new DatabaseMetaInfoProvider();

    }
}
