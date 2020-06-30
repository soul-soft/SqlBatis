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
                db.From<Student>().Insert(new Student[]{ new  Student
                {
                    IsDel = new byte[] { 1, 1, 2 }
                } });

            }
        }

    }
    public class Student
    {
        public int Id { get; set; }
        [Column("is_del")]
        public byte[] IsDel { get; set; }
    }
}