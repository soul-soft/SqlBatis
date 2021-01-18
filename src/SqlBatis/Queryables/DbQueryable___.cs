using SqlBatis.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlBatis.Queryables
{
    public class DbQueryable<T1, T2, T3> : DbQueryable, IDbQueryable<T1, T2, T3>
    {
        #region fields
        private readonly List<string> _tables = new List<string>();
        public DbQueryable(IDbContext context)
            : base(context, false)
        {

        }
        #endregion

        public int Count(int? commandTimeout = null)
        {
            var sql = BuildCountCommand();
            return _context.ExecuteScalar<int>(sql, _parameters, commandTimeout);
        }

        public int Count<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            var sql = BuildCountCommand(expression);
            return _context.ExecuteScalar<int>(sql, _parameters);
        }

        public Task<int> CountAsync(int? commandTimeout = null)
        {
            var sql = BuildCountCommand();
            return _context.ExecuteScalarAsync<int>(sql, _parameters, commandTimeout);
        }

        public Task<int> CountAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            var sql = BuildCountCommand(expression);
            return _context.ExecuteScalarAsync<int>(sql, _parameters);
        }


        public IDbQueryable<T1, T2, T3> GroupBy<TResult>(Expression<Func<T1, T2, T3, TResult>> expression)
        {
            AddGroupExpression(expression);
            return this;
        }

        public IDbQueryable<T1, T2, T3> Having(Expression<Func<T1, T2, T3, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                AddHavingExpression(expression);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3> OrderBy<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                AddOrderExpression(expression, true);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3> OrderByDescending<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                AddOrderExpression(expression, false);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3> Page(int index, int count, bool condition = true)
        {
            if (condition)
            {
                Skip((index - 1) * count, count);
            }
            return this;
        }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression);
            return _context.Query<TResult>(sql, _parameters, commandTimeout);
        }

        public Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression);
            return _context.QueryAsync<TResult>(sql, _parameters, commandTimeout);
        }

        public (IEnumerable<TResult>, int) SelectMany<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(expression);
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = multi.Read<TResult>();
                var count = multi.ReadFirst<int>();
                return (list, count);
            }
        }

        public async Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, int? commandTimeout = null)
        {
            var sql1 = BuildSelectCommand(expression);
            var sql2 = BuildCountCommand();
            using (var multi = _context.QueryMultiple($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.ReadAsync<TResult>();
                var count = await multi.ReadFirstAsync<int>();
                return (list, count);
            }
        }

        public TResult Single<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return Select(expression, commandTimeout).FirstOrDefault();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(expression, commandTimeout)).FirstOrDefault();
        }

        public IDbQueryable<T1, T2, T3> Skip(int index, int count, bool condition = true)
        {
            if (condition)
            {
                SetPage(index, count);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3> Take(int count, bool condition = true)
        {
            if (condition)
            {
                Skip(0, count);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                AddWhereExpression(expression);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3> With(string lockname)
        {
            SetLockName($" {lockname}");
            return this;
        }

        private IDbQueryable<T1, T2, T3> Join<V1, V2>(Expression<Func<V1, V2, bool>> expression, string joinType)
        {
            var resovle = new BooleanExpressionResovle(_isSingleTable, expression, _parameters);
            var onExpression = resovle.Resovle();
            var table1Name = resovle.GetDbTableNameAsAlias(typeof(V1));
            var table2Name = resovle.GetDbTableNameAsAlias(typeof(V2));
            joinType = string.Format("{0} JOIN", joinType);
            if (_tables.Count == 0)
            {
                _tables.Add(table1Name);
                _tables.Add(table2Name);
                SetViewName(string.Format("{0} {1} {2} ON {3}", table1Name, joinType, table2Name, onExpression));
            }
            else if (_tables.Exists(a => table1Name == a))
            {
                _tables.Add(table2Name);
                SetViewName(string.Format("{0} {1} ON {2}", joinType, table2Name, onExpression));
            }
            else
            {
                _tables.Add(table1Name);
                SetViewName(string.Format("{0} {1} ON {2}", joinType, table1Name, onExpression));
            }
            return this;
        }
        public IDbQueryable<T1, T2, T3> Join(Expression<Func<T1, T2, T3, bool>> expression)
        {
            var resovle = new BooleanExpressionResovle(_isSingleTable, expression, _parameters);
            var onExpression = resovle.Resovle();
            var table1Name = resovle.GetDbTableNameAsAlias(typeof(T1));
            var table2Name = resovle.GetDbTableNameAsAlias(typeof(T2));
            var table3Name = resovle.GetDbTableNameAsAlias(typeof(T3));
            SetViewName(string.Format("{0} JOIN {1} JOIN {2} ON {3}", table1Name, table2Name, table3Name, onExpression));
            return this;
        }
        public IDbQueryable<T1, T2, T3> Join<V1, V2>(Expression<Func<V1, V2, bool>> expression)
        {
            Join(expression, "INNER");
            return this;
        }

        public IDbQueryable<T1, T2, T3> LeftJoin<V1, V2>(Expression<Func<V1, V2, bool>> expression)
        {
            Join(expression, "LEFT");
            return this;
        }

        public IDbQueryable<T1, T2, T3> RightJoin<V1, V2>(Expression<Func<V1, V2, bool>> expression)
        {
            Join(expression, "RIGHT");
            return this;
        }
    }
}
