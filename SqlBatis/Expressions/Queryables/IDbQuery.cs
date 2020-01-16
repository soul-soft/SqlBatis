using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlBatis.Queryables
{
    public interface IDbQuery<T>
    {
        int Count(int? commandTimeout = null);
        Task<int> CountAsync(int? commandTimeout = null);
        int Count<TResult>(Expression<Func<T,TResult>> expression);
        Task<int> CountAsync<TResult>(Expression<Func<T, TResult>> expression);
        int Delete(int? commandTimeout = null);
        Task<int> DeleteAsync(int? commandTimeout = null);
        int Delete(Expression<Func<T, bool>> expression);
        Task<int> DeleteAsync(Expression<Func<T, bool>> expression);
        bool Exists(int? commandTimeout = null);
        Task<bool> ExistsAsync(int? commandTimeout = null);
        bool Exists(Expression<Func<T, bool>> expression);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);
        int Update(int? commandTimeout = null);
        Task<int> UpdateAsync(int? commandTimeout = null);
        int Update(T entity);
        Task<int> UpdateAsync(T entity);
        int Insert(T entity);
        Task<int> InsertAsync(T entity);
        int InsertReturnId(T entity);
        Task<int> InsertReturnIdAsync(T entity);
        int Insert(IEnumerable<T> entitys);
        Task<int> InsertAsync(IEnumerable<T> entitys);
        IEnumerable<T> Select(int? commandTimeout = null);
        Task<IEnumerable<T>> SelectAsync(int? commandTimeout = null);
        (IEnumerable<T>, int) SelectMany(int? commandTimeout = null);
        Task<(IEnumerable<T>, int)> SelectManyAsync(int? commandTimeout = null);
        IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        (IEnumerable<TResult>, int) SelectMany<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        T Single(int? commandTimeout = null);
        Task<T> SingleAsync(int? commandTimeout = null);
        TResult Single<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        Task<TResult> SingleAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        IDbQuery<T> Filter<TResult>(Expression<Func<T, TResult>> column);
        IDbQuery<T> Set<TResult>(Expression<Func<T, TResult>> column, TResult value, bool condition = true);
        IDbQuery<T> Set<TResult>(Expression<Func<T, TResult>> column, Expression<Func<T, TResult>> expression, bool condition = true);
        IDbQuery<T> Take(int count);
        IDbQuery<T> Skip(int index, int count);
        IDbQuery<T> Page(int index, int count);
        IDbQuery<T> With(string lockname);
        IDbQuery<T> Having(Expression<Func<T, bool>> expression, bool condition = true);
        IDbQuery<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression);
        IDbQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression);
        IDbQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression);     
        IDbQuery<T> Where(Expression<Func<T, bool>> expression, bool condition = true);
    }
}
