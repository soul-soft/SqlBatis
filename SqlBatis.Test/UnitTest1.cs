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
using System.Data.SqlClient;

namespace SqlBatis.Test
{
    public class MysqlDbConrext : DbContext
    {
        public readonly IDbQuery<Student> Students;
        protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
        {
            ILoggerFactory factory = LoggerFactory.Create(b => { b.AddConsole(); b.AddDebug(); b.SetMinimumLevel(LogLevel.Debug); });
            //builder.Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;");
            builder.Connection = new SqlConnection("Data Source=192.168.31.33;Initial Catalog=test;User ID=sa;Password=yangche!1234;Pooling=true");
            builder.XmlResovle = null;
            builder.Logger = factory.CreateLogger<MysqlDbConrext>();
            return builder;
        }
        public MysqlDbConrext(IXmlResovle resovle)
        {
            Students = new DbQuery<Student>(this);
        }
    }
  
    public class SqlDbConrext : DbContext
    {
        public readonly IDbQuery<Student> Students;
        private readonly static ILoggerFactory _loggerFactory
            = LoggerFactory.Create(b => { b.AddConsole(); b.AddDebug(); b.SetMinimumLevel(LogLevel.Debug); });
        protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
        {            
            builder.Connection = new SqlConnection("Data Source=192.168.31.33;Initial Catalog=test;User ID=sa;Password=yangche!1234;Pooling=true");
            builder.XmlResovle = null;
            builder.DbContextType = DbContextType.SqlServer;
            builder.Logger = _loggerFactory.CreateLogger<MysqlDbConrext>();
            return builder;
        }
        public SqlDbConrext(IXmlResovle resovle)
        {
            Students = new DbQuery<Student>(this);
        }
    }
    
    [Table("student")]
    public class Student
    {
        [Column("Id", ColumnKey.Primary, true)]
        public int? Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Age")]
        public int Age { get; set; }
        [Column("IsDelete")]
        public bool? IsDelete { get; set; }
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
            var db = new SqlDbConrext(null);
            db.Open();

            var (list,count) = db.Students
                .GroupBy(a=>a.Age)
                .Page(1,2)
                .SelectMany(s=>new
                {
                    s.Age,
                    Count = Func.COUNT(1)
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