using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;
using SqlBatis.Attributes;

namespace SqlBatis.Expressions
{
    public static class TableInfoCache
    {
        private static readonly ConcurrentDictionary<Type, TableAttribute> _tables
            = new ConcurrentDictionary<Type, TableAttribute>();

        private static readonly ConcurrentDictionary<Type, List<ColumnAttribute>> _columns
            = new ConcurrentDictionary<Type, List<ColumnAttribute>>();

        public static TableAttribute GetTable(Type type)
        {
            return _tables.GetOrAdd(type, (t) =>
            {
                var table = t.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
                if (table == null)
                {
                    table = new TableAttribute()
                    {
                        Name = type.Name,
                    };
                }
                return table;
            });
        }

        public static List<ColumnAttribute> GetColumns(Type type)
        {
            return _columns.GetOrAdd(type, t =>
             {
                 var list = new List<ColumnAttribute>();
                 var properties = type.GetProperties();
                 foreach (var item in properties)
                 {
                     var column = item.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
                     if (column == null)
                     {
                         column = new ColumnAttribute()
                         {
                             IsIgnore = true,
                             IsIdentity = false,
                             Key = ColumnKey.None,
                             ColumnName = item.Name,
                         };
                     }
                     if (!column.IsIgnore)
                     {
                         column.PropertyName = item.Name;
                         list.Add(column);
                     }
                 }
                 return list;
             });
        }
       
        public static string GetColumnName(Type type, string propertyName)
        {
            var columns = GetColumns(type);
            return columns.Where(a => a.PropertyName == propertyName)
                .FirstOrDefault()?.ColumnName ?? propertyName;
        }

    }

}
