using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace SqlBatis.Test
{
    public class SqlDbContext : DbContext
    {
        private bool _isopen = false;
        public SqlDbContext(DbContextBuilder builder):base(builder)
        {
        }
        public IDbQuery<StudentDto> Students { get => new DbQuery<StudentDto>(this); }

        public override IEnumerable<dynamic> Query(string sql, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (Connection.State==ConnectionState.Closed)
            {
                _isopen = true;
                Connection.Open();
            }
            var result = base.Query(sql, parameter, commandTimeout, commandType);
            if (_isopen)
            {
                Connection.Close();
            }
            return result;
        }
    }
}
