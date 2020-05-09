using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using Org.BouncyCastle.Math.Field;
using SqlBatis.Attributes;
using SqlBatis.Expressions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace SqlBatis.Test
{


    public class Tests
    {
        //public MysqlDbContext db = new MysqlDbContext();
        public SqlDbContext db = new SqlDbContext();

        [SetUp]
        public void Setup()
        {
            //db.Open();
        }

        [Test]
        public void TestInsert()
        {
            var row = db.Students
                .Filter(a => a.Id)
                .Insert(new StudentDto()
                {
                    Name = "zs"
                });
            var id = db.Students
                .Filter(a => a.Id)
                .InsertReturnId(new StudentDto()
                {
                    Name = "zs"
                });

        }

        [Test]
        public void TestUpdate()
        {
            var row = db.Students
                .Filter(a => a.Id)
                .Where(a => a.Id == 1)
                .Update(new StudentDto()
                {
                    Age = 90,
                    IsDelete = false,
                    Name = "zs"
                });
            row = db.Students
               .Set(a => a.IsDelete, true)
               .Set(a => a.Age, a => a.Age + 1)
               .Where(a => a.Id == 3)
               .Update();
        }

        [Test]
        public void TestDelete()
        {
            var row = db.Students
                .Where(a => a.Id == 1)
                .Delete();
        }

        [Test]
        public void TestCount()
        {
            var count = db.Students
                .Where(a => a.Age > 3)
                .Count();
        }

        [Test]
        public void TestExists()
        {
            var flag1 = db.Students
                .Where(a => a.Age > 3)
                .Exists();

            var flag2 = db.Students
               .Where(a => a.Age < 3)
               .Exists();
        }

        [Test]
        public void TestDynamic()
        {
            var p = new { Id = (int?)1, Age = (int?)2 };

            var list = db.Students
                .Where(a => a.Id == p.Id, p.Id != null)
                .Where(a => a.Age == p.Age, p.Age != null)
                .Select();

        }

        [Test]
        public void TestGroup()
        {
            var list = db.Students
                .GroupBy(a => a.Age)
                .Select(s => new
                {
                    s.Age,
                    Id = Func.COUNT(1)
                });
        }

        [Test]
        public void TestSkip()
        {
            var list1 = db.Students
                .Skip(1, 10)
                .Select();

            var list2 = db.Students
              .Take(10)
              .Select();
        }

        [Test]
        public void TestPage()
        {
            var list1 = db.Students
                .Page(1, 2)
                .SelectMany();

            var list2 = db.Students
               .OrderBy(a => new { a.Id, a.IsDelete })
               .Page(2, 2)
               .SelectMany();
        }
        [Test]
        public void TestSelect()
        {
            var arr = new int?[] { 1, 2, 3 };

            var list1 = db.Students
                .Where(a => Operator.In(a.Id, arr))
                .Select().ToList();
            var list2 = db.Students
                .Where(a => Operator.Contains(a.Name, "zs"))
                .Select().ToList();
            var list3 = db.Students
                .Where(a => a.IsDelete == true)
                .Select().ToList();
        }

        [Test]
        public void TestXml()
        {
            var row = db.From("sutdent.add", new StudentDto()
            {
                Name = "xml",
                Age = 90
            }).ExecuteNonQuery();

            var p = new { Id = (int?)1, Index = 1, Count = 10 };
            var list = db.From("sutdent.list-dynamic", p)
                .ExecuteQuery<StudentDto>()
                .ToList();
        }

        [Test]
        public void TestExpression()
        {

            var expr = "(Age != null) && (Id > 0)";
            var context = new ExpressionActivator();
            var result = context.Create<P>(expr);
            var flag1 = result.Func(new P { Id = 2, Age = null });
            var flag2 = result.Func(new P { Id = 2, Age = 2 });
        }

        [Test]
        public void TestTypeConvert()
        {
            var deserializer = EmitConvert.GetDeserializer(typeof(StudentDto));

            Dictionary<string, object> keyvalues = deserializer(new StudentDto
            {
                Id = 10,
                Age = 10,
                IsDelete = true,
                Name = "zs"
            });

            var cmd = db.Connection.CreateCommand();
            cmd.CommandText = "select * from Student";
            var reader = cmd.ExecuteReader();
            var serializer = EmitConvert.GetSerializer<StudentDto>(new EntityMapper(), reader);
            while (reader.Read())
            {
                StudentDto student = serializer(reader);
            }
        }

        [Test]
        public void TestXmlresolve()
        {
            object c = 10;
            var xmlresovle = new XmlResovle();
            xmlresovle.Load(@"E:\SqlBatis\SqlBatis.Test\Student.xml");
            var db = new DbContext(new DbContextBuilder
            {
                Connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;port=3306;user id=root;password=1024;database=test;"),
                XmlResovle = xmlresovle,
            });
            db.Open();
            try
            {
                //var count = db.From<Student>().Get(1);
                var multi = db.From("student.list",new { Id=(int?)null})
                    .ExecuteMultiQuery();
                var list = multi.GetList();
                var count = multi.Get();
            }
            catch (Exception e)
            {

                throw;
            }
           
        }
        public object Funcff()
        {
            var reader = new MySqlCommand().ExecuteReader();
            return FiniteFields(reader,0);
        }
        static long? FiniteFields(IDataRecord record,int i)
        {
            return 90;
        }

    }



    class P
    {
        public int Id { get; set; }
        public int? Age { get; set; }//Age type must be int?
    }

    public class Student
    {
        [Column("id")]
        [PrimaryKey]
        public int Id { get; set; }
        [Column("stu_name")]
        public string Name { get; set; }
        [Column("create_time")]
        [Default]
        public DateTime CreateTime { get; set; }
    }

    public class StudentDto
    {
        public int? Id { get; set; }
        [Column("stu_name")]
        public string Name { get; set; }
        public int? Age { get; set; }
        public bool? IsDelete { get; set; }
    }

}