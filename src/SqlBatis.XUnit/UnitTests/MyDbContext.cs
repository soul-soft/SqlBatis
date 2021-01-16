using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace SqlBatis.XUnit
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextBuilder builder)
            : base(builder)
        {

        }

        protected override IDbCommand CreateDbCommand(string sql, object parameter, int? commandTimeout = null, CommandType? commandType = null)
        {
            Trace.WriteLine("================Command===================");
            Trace.WriteLine(sql);
            Trace.WriteLine("===========================================");
            return base.CreateDbCommand(sql, parameter, commandTimeout, commandType);
        }
    }
}
