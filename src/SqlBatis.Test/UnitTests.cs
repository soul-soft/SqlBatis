using MySql.Data.MySqlClient;
using NUnit.Framework;
using System;
using System.Data.SqlClient;
using System.Linq;

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
        public void FFFFFFFFFFF()
        {
            var connectionString = @"server=127.0.0.1;user id=root;password=1024;database=sqlbatis;";
            var connection = new MySqlConnection(connectionString);
            var context = new DbContext(new DbContextBuilder
            {
                Connection = connection,
                DbContextType = DbContextType.SqlServer2012,
            });
            try
            {
                var list = context.From<StudentDto>()
                     .Select();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }


    }

}