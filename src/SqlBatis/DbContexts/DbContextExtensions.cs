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
