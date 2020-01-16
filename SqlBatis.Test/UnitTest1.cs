using NUnit.Framework;
using SqlBatis.Attributes;
using SqlBatis.DbContexts;
using SqlBatis.Expressions;
using SqlBatis.Expressions.Resovles;
using SqlBatis.Queryables;
using SqlBatis.XmlResovles;
using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using Dapper;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace SqlBatis.Test
{
    public class MysqlDbConrext : DbContext
    {
        public readonly IDbQuery<Student> Students;
        protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
        {
            ILoggerFactory factory = LoggerFactory.Create(b => { b.AddConsole(); b.AddDebug(); b.SetMinimumLevel(LogLevel.Debug); });
            builder.Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;");
            builder.XmlResovle = null;
            builder.Logger = factory.CreateLogger<MysqlDbConrext>();
            return builder;
        }
        public MysqlDbConrext(IXmlResovle resovle)
        {
            Students = new DbQuery<Student>(this);
        }
    }
    [Table("student")]
    public class Student
    {
        [Column("id", ColumnKey.Primary, true)]
        public int? Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("age")]
        public int Age { get; set; }
    }
    public class StudentGroup
    {
        public int Cfe { get; set; }
        public int Count { get; set; }
    }

    [Function]
    public static class Func
    {
        public static T COUNT<T>(T column) => default;
        public static string GROUP_CONCAT<T>(T column) => default;
        public static string CONCAT(params object[] columns) => default;
        public static string REPLACE(string column, string oldstr, string newstr) => default;

    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }
        [Test]
        public void Test2()
        {

            var db = new MysqlDbConrext(null);
            db.Open();

            var row = db.Students.Where(a => a.Age == 5)
                .Update(new Student
                {
                    Id = 20,
                    Age = 90,
                    Name = "zs"
                });
        }

        [Test]
        public void Test1()
        {
            var c = new { c = "ggg" };
            Expression<Func<Student, bool>> expression = s => s.Id != null && Operator.NotContains(s.Name, c.c);
            var resovle = new BooleanExpressionResovle(expression).Resovle();
            Assert.Pass();
        }
      
      
    }
}