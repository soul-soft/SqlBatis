using SqlBatis.Queryables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    /// <summary>
    /// 扩展IDbContext
    /// </summary>
    public static class DbContextExtensions
    {
        public static IXmlQuery From<T>(this IDbContext context, string id, T parameter) where T : class
        {
            var sql = SqlBatisSettings.XmlCommandsProvider.Build(id, parameter);
            var deserializer = SqlBatisSettings.DbEntityMapperProvider.GetDeserializer(typeof(T));
            var values = deserializer(parameter);
            return new XmlQuery(context, sql, values);
        }
        public static IXmlQuery From(this IDbContext context, string id)
        {
            var sql = SqlBatisSettings.XmlCommandsProvider.Build(id);
            return new XmlQuery(context, sql);
        }
        public static IDbQueryable<T> From<T>(this IDbContext context)
        {
            return new DbQueryable<T>(context);
        }
        public static IDbQueryable<T1, T2> From<T1, T2>(this IDbContext context)
        {
            return new DbQueryable<T1, T2>(context);
        }
        public static IDbQueryable<T1, T2, T3> From<T1, T2, T3>(this IDbContext context)
        {
            return new DbQueryable<T1, T2, T3>(context);
        }
    }
}
