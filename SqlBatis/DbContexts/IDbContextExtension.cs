using SqlBatis.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using SqlBatis.Queryables;

namespace SqlBatis.DbContexts
{
    public static class IDbContextExtension
    {
        public static IDbQueryable<T> From<T>(this IDbContext context)
        {
            return new MysqlQueryable<T>(context);
        }
    
    }
}
