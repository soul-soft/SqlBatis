using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBatis.Queryables
{
    public interface IDbQueryable<T>
    {
        int Delete(int? commandTimeout = null);
        bool Exists(int? commandTimeout = null);
        bool Exists(Expression<Func<T, bool>> expression);
        int Update(int? commandTimeout = null);
        int Update(T entity);
        int Insert(T entity);
        int Insert(IEnumerable<T> entitys);
        IDbQueryable<T> Filter<TResult>(Expression<Func<T, TResult>> column);
        IDbQueryable<T> Set<TResult>(Expression<Func<T, TResult>> column, TResult value, bool condition = true);
        IDbQueryable<T> Set<TResult>(Expression<Func<T, TResult>> column, Expression<Func<T, TResult>> expression, bool condition = true);
        IDbQueryable<T> Take(int count);
        IDbQueryable<T> Skip(int index, int count);
        IDbQueryable<T> Page(int index, int count);
        IDbQueryable<T> Having(Expression<Func<T, bool>> expression);
        IDbQueryable<T> GroupBy<TResult>(Expression<Func<T, TResult>> expression);
        IDbQueryable<T> OrderBy<TResult>(Expression<Func<T, TResult>> expression);
        IDbQueryable<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> expression);
        IEnumerable<T> Select(int? commandTimeout = null);
        IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        T Single(int? commandTimeout = null);
        TResult Single<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null);
        IDbQueryable<T> Where(Expression<Func<T, bool>> expression, bool condition = true);
    }
}
