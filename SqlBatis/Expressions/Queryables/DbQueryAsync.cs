using SqlBatis.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using SqlBatis.Expressions.Resovles;
using SqlBatis.Expressions;
using System.Threading.Tasks;

namespace SqlBatis.Queryables
{
    public partial class DbQuery<T>
    {
        public Task<int> CountAsync(int? commandTimeout = null)
        {
            var sql = ResovleCount();
            return _context.ExecuteScalarAsync<int>(sql, _parameters, commandTimeout);
        }

        public Task<int> CountAsync<TResult>(Expression<Func<T, TResult>> expression)
        {
            _countExpression = expression;
            return CountAsync();
        }

        public Task<int> DeleteAsync(int? commandTimeout = null)
        {
            var sql = ResovleDelete();
            return _context.ExecuteNonQueryAsync(sql, _parameters, commandTimeout);
        }

        public Task<int> DeleteAsync(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return DeleteAsync();
        }

        public Task<bool> ExistsAsync(int? commandTimeout = null)
        {
            var sql = ResovleExists();
            return _context.ExecuteScalarAsync<bool>(sql, _parameters, commandTimeout);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> expression)
        {
            Where(expression);
            return ExistsAsync();
        }

        public Task<int> UpdateAsync(int? commandTimeout = null)
        {
            if (_setExpressions.Count > 0)
            {
                var sql = ResolveUpdate();
                return _context.ExecuteNonQueryAsync(sql, _parameters, commandTimeout);
            }
            return default;
        }

        public Task<int> UpdateAsync(T entity)
        {
            ResovleParameter(entity);
            var sql = ResolveUpdate();
            return _context.ExecuteNonQueryAsync(sql, _parameters);
        }

        public Task<int> InsertAsync(T entity)
        {
            ResovleParameter(entity);
            var sql = ResovleInsert();
            return _context.ExecuteNonQueryAsync(sql, _parameters);
        }

        public async Task<int> InsertAsync(IEnumerable<T> entitys)
        {
            var count = 0;
            foreach (var item in entitys)
            {
                count += await InsertAsync(item);
            }
            return count;
        }

        public Task<int> InsertReturnIdAsync(T entity)
        {
            ResovleParameter(entity);
            var sql = ResovleInsert() + ";SELECT LAST_INSERT_ID()";
            return _context.ExecuteScalarAsync<int>(sql, _parameters);
        }

        public Task<IEnumerable<T>> SelectAsync(int? commandTimeout = null)
        {
            var sql = ResolveSelect();
            return _context.ExecuteQueryAsync<T>(sql, _parameters, commandTimeout);
        }

        public async Task<(IEnumerable<T>, int)> SelectManyAsync(int? commandTimeout = null)
        {
            var sql1 = ResolveSelect();
            var sql2 = ResovleCount();
            using (var multi = _context.ExecuteMultiQuery($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.GetListAsync<T>();
                var count = await multi.GetAsync<int>();
                return (list, count);
            }
        }

        public Task<IEnumerable<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            _selectExpression = expression;
            var sql = ResolveSelect();
            return _context.ExecuteQueryAsync<TResult>(sql, _parameters, commandTimeout);
        }
        public async Task<(IEnumerable<TResult>, int)> SelectManyAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            _selectExpression = expression;
            var sql1 = ResolveSelect();
            var sql2 = ResovleCount();
            using (var multi = _context.ExecuteMultiQuery($"{sql1};{sql2}", _parameters, commandTimeout))
            {
                var list = await multi.GetListAsync<TResult>();
                var count = await multi.GetAsync<int>();
                return (list, count);
            }
        }
        public async Task<T> SingleAsync(int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(commandTimeout)).FirstOrDefault();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<T, TResult>> expression, int? commandTimeout = null)
        {
            Take(1);
            return (await SelectAsync(expression, commandTimeout)).FirstOrDefault();
        }
    }
}
