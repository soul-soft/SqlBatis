using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SqlBatis
{
    public class DapperDbContext : IDbContext
    {
        readonly IDbConnection _connection;

        IDbTransaction _transaction;

        public bool IsTransactioned => _transaction != null;

        public DbContextState DbContextState { get; private set; } = DbContextState.Closed;

        public DbContextType DbContextType { get; private set; } = DbContextType.Mysql;

        public DapperDbContext(DbContextBuilder builder)
        {
            _connection = builder.Connection;
            DbContextType = builder.DbContextType;
        }

        public void BeginTransaction()
        {
            Open();
            _transaction = _connection.BeginTransaction();
        }
        public void BeginTransaction(IsolationLevel level)
        {
            Open();
            _transaction = _connection.BeginTransaction(level);
        }
        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
                DbContextState = DbContextState.Commit;
            }
        }
        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
                DbContextState = DbContextState.Rollback;
            }
        }
        public int Execute(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.Execute(sql, parameter, _transaction, commandTimeout, commandType);
        }
        public Task<int> ExecuteAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.ExecuteAsync(sql, parameter, _transaction, commandTimeout, commandType);
        }
        public T ExecuteScalar<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.ExecuteScalar<T>(sql, parameter, _transaction, commandTimeout, commandType);
        }
        public Task<T> ExecuteScalarAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.ExecuteScalarAsync<T>(sql, parameter, _transaction, commandTimeout, commandType);
        }
        public IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.Query(sql, parameter, _transaction, false, commandTimeout, commandType);
        }
        public IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.Query<T>(sql, parameter, _transaction, false, commandTimeout, commandType);
        }
        public async Task<List<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var list = await _connection.QueryAsync(sql, parameter, _transaction, commandTimeout, commandType);
            return list.AsList();
        }
        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _connection.QueryAsync<T>(sql, parameter, _transaction, commandTimeout, commandType);
        }
        public IDbGridReader QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var reader = _connection.QueryMultiple(sql, parameter, _transaction, commandTimeout, commandType);
            return new DapperDbMultipleResult(reader);
        }

        void IDbContext.BeginTransaction()
        {
            Open();
            _transaction = _connection.BeginTransaction();
        }

        void IDbContext.BeginTransaction(IsolationLevel level)
        {
            Open();
            _transaction = _connection.BeginTransaction(level);
        }

        public void Close()
        {
            try
            {
                _transaction?.Dispose();
                _connection?.Close();
            }
            finally
            {
                DbContextState = DbContextState.Closed;
            }
        }

        public void Open()
        {
            if (DbContextState == DbContextState.Closed)
            {
                _connection.Open();
                DbContextState = DbContextState.Open;
            }
        }

        public async Task OpenAsync()
        {
            if (DbContextState == DbContextState.Closed)
            {
                await (_connection as System.Data.Common.DbConnection).OpenAsync();
                DbContextState = DbContextState.Open;
            }
        }

        public void Dispose()
        {
            RollbackTransaction();
            System.GC.SuppressFinalize(this);
            Close();
        }

        ~DapperDbContext()
        {
            Dispose();
        }
    }
}