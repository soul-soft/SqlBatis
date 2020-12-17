using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// 数据库上下文状态
        /// </summary>
        DbContextState DbContextState { get; }
        /// <summary>
        /// 数据库连接
        /// </summary>
        IDbConnection Connection { get; }
        /// <summary>
        /// 数据库上下文类型
        /// </summary>
        DbContextType DbContextType { get; }
        /// <summary>
        /// 获取一个xml执行器
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="id">命令id</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        IXmlQuery From<T>(string id, T parameter) where T : class;
        /// <summary>
        /// 获取一个xml执行器
        /// </summary>
        /// <param name="id">命令id</param>
        /// <returns></returns>
        IXmlQuery From(string id);
        /// <summary>
        /// 获取一个linq执行器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDbQueryable<T> From<T>();
        /// <summary>
        /// 开启事务会话
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// 开启事务会话
        /// </summary>
        /// <param name="level">事务隔离级别</param>
        void BeginTransaction(IsolationLevel level);
        /// <summary>
        /// 关闭连接和事务
        /// </summary>
        void Close();
        /// <summary>
        /// 提交当前事务会话
        /// </summary>
        void CommitTransaction();
        /// <summary>
        /// 执行多结果集查询，返回IMultiResult
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        IDbMultipleResult QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行单结果集查询，并返回dynamic类型的结果集
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行单结果集查询，并返回dynamic类型的结果集
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<List<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行单结果集查询，并返回T类型的结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行单结果集查询，并返回T类型的结果集
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行无结果集查询，并返回受影响的行数
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        int Execute(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行无结果集查询，并返回受影响的行数
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 执行无结果集查询，并返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        T ExecuteScalar<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 异步执行无结果集查询，并返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql命令</param>
        /// <param name="parameter">参数</param>
        /// <param name="commandTimeout">超时时间</param>
        /// <param name="commandType">命令类型</param>
        /// <returns></returns>
        Task<T> ExecuteScalarAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null);
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        void Open();
        /// <summary>
        /// 异步打开数据库连接
        /// </summary>
        /// <returns></returns>
        Task OpenAsync();
        /// <summary>
        /// 回滚当前事务会话
        /// </summary>
        void RollbackTransaction();
    }
}
