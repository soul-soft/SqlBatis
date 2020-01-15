using NUnit.Framework;
using SqlBatis.Attributes;
using SqlBatis.DbContexts;
using SqlBatis.Expressions;
using SqlBatis.Expressions.Resovles;
using SqlBatis.XmlResovles;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SqlBatis.Test
{
    [Table("student")]
    public class Student
    {
        [Column("id",ColumnKey.Primary,true)]
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
            var resovle = new XmlResovle();
            resovle.Load(@"E:\SqlBatis\SqlBatis.Test\Student.xml");
            var db = new DbContext(new MySql.Data.MySqlClient.MySqlConnection("server=47.110.55.16;user id=root;password=Yangche51!1234;database=test;"), resovle);
            db.Open();
            db.BeginTransaction();

            //db.From<Student>().Update();

            var stu = db.From<Student>().Insert(new Student()
            {
                Age=12,Name="zs"
            });

            var arr = new int?[] { };

            var list = db.From<Student>()
                 .Where(a => Operator.In(a.Id, arr))
                 .Select();

            db.CommitTransaction();
            //var list1 = db.From("list-dynamic", new { Age = (int?)2 })
            //    .ExecuteQuery<Student>();

            //var list2 = db.From("list-dynamic", new { Age = (int?)null })
            //  .ExecuteQuery<Student>();
            var columns1 = TableInfoCache.GetColumns(typeof(Student));
            var stop = new Stopwatch();
            stop.Start();
            for (int i = 0; i < 100000; i++)
            {
                var columns = TableInfoCache.GetColumns(typeof(Student));
            }
            stop.Stop();
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