using NUnit.Framework;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace SqlBatis.Test
{
    public class UnitTests
    {

        [Test]
        public void FFFFFFFFFFF()
        {
            var connectionString = @"Data Source=114.55.84.249;Initial Catalog=juzhen_mes_chengbang;User ID=sa;Password=p@ssw0rd";
            var connection = new SqlConnection(connectionString);
            var context = new DbContext(new DbContextBuilder
            {
                Connection = connection,
                DbContextType = DbContextType.SqlServer,
                EntityMapper = new DefaultEntityMapper()
            });
            try
            {
                //var (list,count) = context.From<SysUserDto>().OrderBy(A=>A.Id).Page(1, 2).SelectMany();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }


    }

}