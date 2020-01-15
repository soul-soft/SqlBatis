using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute:Attribute
    {
        internal string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public ColumnKey Key { get; set; }
        public bool IsIgnore { get; set; }
        public bool IsIdentity { get; set; }
        public ColumnAttribute(string name = null, ColumnKey key = ColumnKey.None, bool isIdentity = false, bool ignore = false)
        {
            ColumnName = name;
            Key = key;
            IsIgnore = ignore;
            IsIdentity = isIdentity;
        }
    }
}
