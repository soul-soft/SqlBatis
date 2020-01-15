using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Name { get; set; }
        public TableAttribute(string name = null)
        {
            Name = name;
        }
    }
}
