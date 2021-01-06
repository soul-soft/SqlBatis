using MySql.Data.MySqlClient;
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
            var connectionString = @"server=127.0.0.1;user id=root;password=1024;database=sqlbatis;";
            var connection = new MySqlConnection(connectionString);
            var context = new DbContext(new DbContextBuilder
            {
                Connection = connection,
                DbContextType = DbContextType.Mysql,
                DbEntityMapper = new MyEntityMapper()
            });
            try
            {
               var list = context.From<Student, StudentSchool>()
                    .LeftJoin((a, b) => a.Sid == b.Id && b.Id != 1)
                    .Select((a,b)=> new
                    {
                        a.StuName,
                        b.SchName
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
        }


    }

}