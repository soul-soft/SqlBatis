using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlBatis.Queryables
{
    /// <summary>
    /// linq 查询
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public interface IDbQueryable<T1, T2>
    {
        /// <summary>
        /// 内连接
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IDbQueryable<T1, T2> Join(Expression<Func<T1, T2, bool>> expression);
        /// <summary>
        /// 左外连接
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IDbQueryable<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> expression);
        /// <summary>
        /// 右外连接
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IDbQueryable<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> expression);
        /// <summary>
        /// count查询
        /// </summary>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        int Count(int? commandTimeout = null);
        /// <summary>
        /// 异步count查询
        /// </summary>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        Task<int> CountAsync(int? commandTimeout = null);
        /// <summary>
        /// count查询
        /// </summary>
        /// <typeparam name="TResult">类型推断</typeparam>
        /// <param name="expression">字段列表</param>
        /// <returns></returns>
        int Count<TResult>(Expression<Func<T1, T2, TResult>> expression);
        /// <summary>
        /// 异步count查询
        /// </summary>
        /// <typeparam name="TResult">类型推断</typeparam>
        /// <param name="expression">字段列表</param>
        /// <returns></returns>
        Task<int> CountAsync<TResult>(Expression<Func<T1, T2, TResult>> expression);
        /// <summary>
        /// select查询
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        IEnumerable<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> expression, int? commandTimeout = null);
        /// <summary>
        /// 异步select查询
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T1, T2, TResult>> expression, int? commandTimeout = null);
        /// <summary>
        /// 分页select查询
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        (IEnumerable<TResult>, int) SelectMany<TResult>(Expression<Func<T1, T2, TResult>> expression, int? commandTimeout = null);
        /// <summary>
        /// 异步分页select查询
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T1, T2, TResult>> expression, int? commandTimeout = null);
        /// <summary>
        /// select查询
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        TResult Single<TResult>(Expression<Func<T1, T2, TResult>> expression, int? commandTimeout = null);
        /// <summary>
        /// 异步select查询
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <returns></returns>
        Task<TResult> SingleAsync<TResult>(Expression<Func<T1, T2, TResult>> expression, int? commandTimeout = null);
        /// <summary>
        /// take查询，从下标为0的行获取count条记录
        /// </summary>
        /// <param name="count">记录个数</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> Take(int count, bool condition = true);
        /// <summary>
        /// skip，从下标为index的行获取count条记录
        /// </summary>
        /// <param name="index">起始下标</param>
        /// <param name="count">记录个数</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> Skip(int index, int count, bool condition = true);
        /// <summary>
        /// page查询，从下标为(index-1)*count的行获取count条记录
        /// </summary>
        /// <param name="index">起始页码</param>
        /// <param name="count">记录个数</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> Page(int index, int count, bool condition = true);
        /// <summary>
        /// 指定读锁
        /// </summary>
        /// <param name="lockname"></param>
        /// <returns></returns>
        IDbQueryable<T1, T2> With(string lockname);
        /// <summary>
        /// where查询，多个where有效使用and连接
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="condition">是否有效</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> Where(Expression<Func<T1, T2, bool>> expression, bool condition = true);
        /// <summary>
        /// having查询，多个having查询有效使用and连接
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="condition">是否有效</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> Having(Expression<Func<T1, T2, bool>> expression, bool condition = true);
        /// <summary>
        /// group查询
        /// </summary>
        /// <typeparam name="TResult">类型推断</typeparam>
        /// <param name="expression">字段列表</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> GroupBy<TResult>(Expression<Func<T1, T2, TResult>> expression);
        /// <summary>
        /// orderby查询，升序
        /// </summary>
        /// <typeparam name="TResult">类型推断</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="condition">是否有效</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> OrderBy<TResult>(Expression<Func<T1, T2, TResult>> expression, bool condition = true);
        /// <summary>
        /// orderby查询，降序
        /// </summary>
        /// <typeparam name="TResult">类型推断</typeparam>
        /// <param name="expression">字段列表</param>
        /// <param name="condition">是否有效</param>
        /// <returns></returns>
        IDbQueryable<T1, T2> OrderByDescending<TResult>(Expression<Func<T1, T2, TResult>> expression, bool condition = true);
    }
}
