using SqlBatis.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using SqlBatis.Queryables;

namespace SqlBatis.DbContexts
{
    public static class DbContextExtension
    {
        public static IDbQuery<T> From<T>(this IDbContext context)
        {
            return new DbQuery<T>(context);
        }
    
    }
}
