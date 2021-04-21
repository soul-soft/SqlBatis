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
        public static IDbQueryable<T> Queryable<T>(this IDbContext context)
        {
            return new DbQueryable<T>(context);
        }
        public static IDbQueryable<T1, T2> From<T1, T2>(this IDbContext context)
        {
            return new DbQueryable<T1, T2>(context);
        }
        public static IDbQueryable<T1, T2> Queryable<T1, T2>(this IDbContext context)
        {
            return new DbQueryable<T1, T2>(context);
        }
        public static IDbQueryable<T1, T2, T3> From<T1, T2, T3>(this IDbContext context)
        {
            return new DbQueryable<T1, T2, T3>(context);
        }
        public static IDbQueryable<T1, T2, T3> Queryable<T1, T2, T3>(this IDbContext context)
        {
            return new DbQueryable<T1, T2, T3>(context);
        }
        public static IDbQueryable<T1, T2, T3, T4> From<T1, T2, T3, T4>(this IDbContext context)
        {
            return new DbQueryable<T1, T2, T3, T4>(context);
        }
        public static IDbQueryable<T1, T2, T3, T4> Queryable<T1, T2, T3, T4>(this IDbContext context)
        {
            return new DbQueryable<T1, T2, T3, T4>(context);
        }
        public static int Insert<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).Insert(entity);
        }
        public static Task<int> InsertAsync<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).InsertAsync(entity);
        }
        public static int InsertBatch<T>(this IDbContext context, IEnumerable<T> entities, int? commandTimeout = null)
        {
            return new DbQueryable<T>(context).InsertBatch(entities, commandTimeout);
        }
        public static Task<int> InsertBatchAsync<T>(this IDbContext context, IEnumerable<T> entities, int? commandTimeout = null)
        {
            return new DbQueryable<T>(context).InsertBatchAsync(entities, commandTimeout);
        }
        public static int InsertReturnId<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).InsertReturnId(entity);
        }
        public static Task<int> InsertReturnIdAsync<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).InsertReturnIdAsync(entity);
        }
        public static int Update<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).Update(entity);
        }
        public static Task<int> UpdateAsync<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).UpdateAsync(entity);
        }
        public static int Delete<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).Delete(entity);
        }
        public static Task<int> DeleteAsync<T>(this IDbContext context, T entity)
        {
            return new DbQueryable<T>(context).DeleteAsync(entity);
        }
        public static int DeleteBatch<T>(this IDbContext context, IEnumerable<T> entities)
        {
            return new DbQueryable<T>(context).DeleteBatch(entities);
        }
        public static Task<int> DeleteBatchAsync<T>(this IDbContext context, IEnumerable<T> entities)
        {
            return new DbQueryable<T>(context).DeleteBatchAsync(entities);
        }
    }
}
