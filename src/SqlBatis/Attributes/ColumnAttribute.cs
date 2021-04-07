using System;

namespace SqlBatis.Attributes
{
    /// <summary>
    /// 字段映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        internal string Name { get; set; }
        internal Type Type { get; set; }
        /// <summary>
        /// 属性字段映射
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="type">字段类型</param>
        public ColumnAttribute(string name = null,Type type=null)
        {
            Name = name;
        }
    }
}
