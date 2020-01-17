using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;
using SqlBatis.Attributes;
using System.Linq.Expressions;

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
                if (!(t.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() is TableAttribute table))
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
                    if (!(item.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() is ColumnAttribute column))
                    {
                        column = new ColumnAttribute()
                        {
                            IsIgnore = false,
                            IsIdentity = false,
                            Key = ColumnKey.None,
                            ColumnName = item.Name,
                        };
                    }
                    if (!column.IsIgnore)
                    {
                        if (string.IsNullOrEmpty(column.ColumnName))
                        {
                            column.ColumnName = item.Name;
                        }
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
