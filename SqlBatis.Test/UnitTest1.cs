using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SqlBatis.Attributes;
using SqlBatis.Expressions.Resovles;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace SqlBatis.Test
{
    public class MysqlDbConrext : DbContext
    {
        public readonly IDbQuery<Student> Students;
        private static readonly IXmlResovle resovle;
        static MysqlDbConrext()
        {
            resovle = new XmlResovle();
            resovle.Load(@"E:\SqlBatis\SqlBatis.Test","*.xml");
        }
        protected override void OnLogging(string message, object parameter = null, int? commandTimeout = null, CommandType? commandType = null)
        {
          
        }
        protected override DbContextBuilder OnConfiguring(DbContextBuilder builder)
        {
            ILoggerFactory factory = LoggerFactory.Create(b => { b.AddConsole(); b.AddDebug(); b.SetMinimumLevel(LogLevel.Debug); });
            builder.Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;");
            builder.XmlResovle = null;
            return builder;
        }        
        public MysqlDbConrext()
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
            return builder;
        }
        public SqlDbConrext()
        {
            Students = new DbQuery<Student>(this);
        }
    }

    public class Student
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
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
            
            var db = new MysqlDbConrext();
            db.Open();

            var list = db.Students
                .Filter(a => a.Id)
                .Insert(new Student()
                {
                    Age = 90,
                    IsDelete = false,
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