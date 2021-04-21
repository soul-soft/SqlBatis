using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SqlBatis
{
    public class DaprDbContext : DbContext
    {
        static DaprDbContext()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }
        public static bool MatchNamesWithUnderscores
        {
            get
            {
                return Dapper.DefaultTypeMap.MatchNamesWithUnderscores;
            }
            set
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = value;
            }
        }
        public DaprDbContext
            (DbContextBuilder builder) : base(builder)
        {

        }
        public override int Execute(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.Execute(sql, parameter, Transaction, commandTimeout, commandType);
        }
        public override Task<int> ExecuteAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.ExecuteAsync(sql, parameter, Transaction, commandTimeout, commandType);
        }
        public override object ExecuteScalar(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.ExecuteScalar(sql, parameter, Transaction, commandTimeout, commandType);
        }
        public override T ExecuteScalar<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.ExecuteScalar<T>(sql, parameter, Transaction, commandTimeout, commandType);
        }
        public override Task<T> ExecuteScalarAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.ExecuteScalarAsync<T>(sql, parameter, Transaction, commandTimeout, commandType);
        }
        public override IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.Query(sql, parameter, Transaction, false, commandTimeout, commandType);
        }
        public override IEnumerable<T> Query<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.Query<T>(sql, parameter, Transaction, false, commandTimeout, commandType);
        }
        public override async Task<List<dynamic>> QueryAsync(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var list = await Connection.QueryAsync(sql, parameter, Transaction, commandTimeout, commandType);
            return list.AsList();
        }
        public override Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Connection.QueryAsync<T>(sql, parameter, Transaction, commandTimeout, commandType);
        }
        public override IDbGridReader QueryMultiple(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var reader = Connection.QueryMultiple(sql, parameter, Transaction, commandTimeout, commandType);
            return new DapperDbMultipleResult(reader);
        }
    }
}