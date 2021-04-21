using SqlBatis.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlBatis.Queryables
{
    public class DbQueryable<T1, T2, T3, T4> : DbQueryable, IDbQueryable<T1, T2, T3, T4>
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

        public int Count<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression)
        {
            var sql = BuildCountCommand(expression);
            return _context.ExecuteScalar<int>(sql, _parameters);
        }

        public Task<int> CountAsync(int? commandTimeout = null)
        {
            var sql = BuildCountCommand();
            return _context.ExecuteScalarAsync<int>(sql, _parameters, commandTimeout);
        }

        public Task<int> CountAsync<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression)
        {
            var sql = BuildCountCommand(expression);
            return _context.ExecuteScalarAsync<int>(sql, _parameters);
        }


        public IDbQueryable<T1, T2, T3, T4> GroupBy<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression)
        {
            AddGroupExpression(expression);
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> Having(Expression<Func<T1, T2, T3, T4, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                AddHavingExpression(expression);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> OrderBy<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                AddOrderExpression(expression, true);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> OrderByDescending<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, bool condition = true)
        {
            if (condition)
            {
                AddOrderExpression(expression, false);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> Page(int index, int count, bool condition = true)
        {
            if (condition)
            {
                Skip((index - 1) * count, count);
            }
            return this;
        }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression);
            return _context.Query<TResult>(sql, _parameters, commandTimeout);
        }

        public Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, int? commandTimeout = null)
        {
            var sql = BuildSelectCommand(expression);
            return _context.QueryAsync<TResult>(sql, _parameters, commandTimeout);
        }

        public (IEnumerable<TResult>, int) SelectMany<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, int? commandTimeout = null)
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

        public async Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, int? commandTimeout = null)
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

        public TResult Single<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return Select(expression, commandTimeout).FirstOrDefault();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(expression, commandTimeout)).FirstOrDefault();
        }

        public IDbQueryable<T1, T2, T3, T4> Skip(int index, int count, bool condition = true)
        {
            if (condition)
            {
                SetPage(index, count);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> Take(int count, bool condition = true)
        {
            if (condition)
            {
                Skip(0, count);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> expression, bool condition = true)
        {
            if (condition)
            {
                AddWhereExpression(expression);
            }
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> With(string lockname)
        {
            SetLockName($" {lockname}");
            return this;
        }

        private IDbQueryable<T1, T2, T3, T4> JoinFormat(Expression<Func<T1, T2, T3, T4, bool>> expression, string joinType)
        {
            var resovle = new BooleanExpressionResovle(_isSingleTable, expression, _parameters);
            var onExpression = resovle.Resovle();
            var alias = resovle.GetTableAlias();
            if (_tables.Count == 0)
            {
                joinType = string.Format(" {0} JOIN ", joinType);
                _tables.AddRange(alias.Select(s => s.Key));
                var viewName = string.Join(joinType, alias.Select(s => $"{s.Value} AS {s.Key}"));
                AppendViewName(string.Format("{0} ON {1}", viewName, onExpression));
            }
            else
            {
                var alia = alias.Where(a => !_tables.Contains(a.Key)).First();
                joinType = string.Format("{0} JOIN ", joinType);
                var viewName = $"{joinType}{alia.Value} AS {alia.Key}";
                AppendViewName(string.Format("{0} ON {1}", viewName, onExpression));
            }
            return this;
        }
        public IDbQueryable<T1, T2, T3, T4> Join(Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            JoinFormat(expression, "INNER");
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            JoinFormat(expression, "LEFT");
            return this;
        }

        public IDbQueryable<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> expression)
        {
            JoinFormat(expression, "RIGHT");
            return this;
        }
    }
}
