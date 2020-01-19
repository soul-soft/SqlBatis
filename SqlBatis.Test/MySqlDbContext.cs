using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace SqlBatis.Test
{
    public class MysqlDbContext : DbContext
    {
        public IDbQuery<Student> Students { get => new DbQuery<Student>(this); }
        private static readonly IXmlResovle resovle;
        static MysqlDbContext()
        {
            resovle = new XmlResovle();
            resovle.Load(@"E:\SqlBatis\SqlBatis.Test", "*.xml");
        }
        protected override void OnLogging(string message, IDataParameterCollection parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            Debug.WriteLine($"============================{DateTime.Now}================================");
            Debug.WriteLine(message);
            if (parameter != null)
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
            builder.Connection = new MySql.Data.MySqlClient.MySqlConnection("server=47.110.55.16;user id=root;password=Yangche51!1234;database=test;");
            builder.XmlResovle = resovle;
            return builder;
        }
    }
}
