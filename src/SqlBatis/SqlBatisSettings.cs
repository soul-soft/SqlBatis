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
        /// 数据库元信息提供程序
        /// </summary>
        public static IDbMetaInfoProvider DbMetaInfoProvider { get; set; }
            = new AnnotationDbMetaInfoProvider();
        /// <summary>
        /// xml命令配置
        /// </summary>
        public static IXmlCommandsProvider XmlCommandsProvider { get; set; }
            = new XmlCommandsProvider();
    }
}
