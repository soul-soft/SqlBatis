using MySql.Data.MySqlClient;
using NUnit.Framework;
using SqlBatis.Attributes;
using SqlBatis.Expressions;
using SqlBatis.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SqlBatis.Test
{
    public class UnitTests
    {

        [Test]
        public void TestInsert()
        {
            var builder = new DbContextBuilder
            {
                Connection = new MySqlConnection("server=127.0.0.1;port=3306;user id=root;password=1024;database=test;"),
            };
            using (var db = new DbContext(builder))
            {
                var arr = new List<int>() { 1, 2 }.ToArray();
                var list = db.From<Student>()
                    .Where(a => arr.Contains(a.Id))
                    .Select();
             
            }
        }

    }
    public class Student
    {
        public int Id { get; set; }
    }
}