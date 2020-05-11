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
        private static readonly XmlResovle resovle;
        static SqlDbContext()
        {
            resovle = new XmlResovle();
            resovle.Load(@"E:\SqlBatis\SqlBatis.Test", "*.xml");          
        }
        public IDbQuery<StudentDto> Students { get => new DbQuery<StudentDto>(this); }
      
        protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
        {
            builder.Connection = new SqlConnection("Data Source=192.168.31.33;Initial Catalog=test;User ID=sa;Password=yangche!1234;Pooling=true");
            builder.XmlResovle = resovle;
            builder.DbContextType = DbContextType.SqlServer;
            return builder;
        }
       
    }
}
