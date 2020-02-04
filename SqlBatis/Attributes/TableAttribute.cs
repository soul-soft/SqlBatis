using System;

namespace SqlBatis.Attributes
{
    /// <summary>
    /// 表名映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        internal string Name { get; set; }
        public TableAttribute(string name = null)
        {
            Name = name;
        }
    }
}
