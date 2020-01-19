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
        private static readonly IXmlResovle resovle;
        static SqlDbContext()
        {
            resovle = new XmlResovle();
            resovle.Load(@"E:\SqlBatis\SqlBatis.Test", "*.xml");          
        }
        public IDbQuery<Student> Students { get => new DbQuery<Student>(this); }

        protected override void OnLogging(string message, IDataParameterCollection parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            Debug.WriteLine($"============================{DateTime.Now}================================");
            Debug.WriteLine(message);
            if (parameter!=null)
            {
                foreach (IDataParameter item in parameter)
                {
                    Debug.WriteLine($"{item.ParameterName}={item.Value}");
                }
            }
            Debug.WriteLine("==============================================================================");
        }
        protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
        {
            builder.Connection = new SqlConnection("Data Source=192.168.31.33;Initial Catalog=test;User ID=sa;Password=yangche!1234;Pooling=true");
            builder.XmlResovle = resovle;
            builder.DbContextType = DbContextType.SqlServer;
            return builder;
        }
       
    }
}
