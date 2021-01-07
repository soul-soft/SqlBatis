using MySql.Data.MySqlClient;
using NUnit.Framework;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBatis.Test
{
    public class StuName
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    public class UnitTests
    {

        [Test]
        public async Task Test()
        {
            var connectionString = @"Data Source=127.0.0.1;Initial Catalog=mytest;User ID=sa;Password=p";
            var connection = new SqlConnection(connectionString);
            //设置默认的转换器
            var context = new DbContext(new DbContextBuilder
            {
                Connection = connection,
                DbContextType = DbContextType.SqlServer2008,
            });

            try
            {
                context.BeginTransaction();
                var data = context.From<MessagesDto>().Page(2, 2).SelectMany();
                context.CommitTransaction();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }
    }

}