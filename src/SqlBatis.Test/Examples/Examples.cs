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
    public class Examples
    {
        private DefaultDbContext db;
        [SetUp]
        public void Setup()
        {
            //db.Open();
        }

        [Test]
        public void TestInsert()
        {
            var row = db.Students
                .Ignore(a => a.Id)
                .Insert(new StudentDto()
                {
                    Name = "zs"
                });
            var id = db.Students
                .Ignore(a => a.Id)
                .InsertReturnId(new StudentDto()
                {
                    Name = "zs"
                });

        }

        [Test]
        public void TestUpdate()
        {
            var row = db.Students
                .Ignore(a => a.Id)
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
                    Id = MysqlFunc.COUNT(1)
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
            }).Execute();

            var p = new { Id = (int?)1, Index = 1, Count = 10 };
            var list = db.From("sutdent.list-dynamic", p)
                .Query<StudentDto>()
                .ToList();
        }

        [Test]
        public void TestExpression()
        {

            var expr = "(Age != null) && (Id > 0)";
            var context = new EvalExpression();
            var result = context.Create<P>(expr);
            var flag1 = result.Func(new P { Id = 2, Age = null });
            var flag2 = result.Func(new P { Id = 2, Age = 2 });
        }

        [Test]
        public void TestTypeConvert()
        {
            var deserializer = new EntityMapperProvider().GetDeserializer(typeof(StudentDto));

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
            var serializer = new EntityMapperProvider()
                .GetSerializer<StudentDto>(reader);
            while (reader.Read())
            {
                StudentDto student = serializer(reader);
            }
        }

        [Test]
        public void TestXmlresolve()
        {
            var stop = new Stopwatch();
            stop.Start();
            var func = GlobalSettings.EntityMapperProvider.GetDeserializer(typeof(Student2Dot));
            for (int i = 0; i < 100000; i++)
            {
                func(new Student2Dot() { id=i,stu_name="ff"+i});
            }
            stop.Stop();
            object c = 10;
            //加载嵌入式配置
            GlobalSettings.EntityMapperProvider = new DefaultEntityMapperProvider();
            //var db = new DbContext(new DbContextBuilder
            //{
            //    Connection = new MySqlConnection("server=127.0.0.1;port=3306;user id=root;password=1024;database=test;"),
            //});
            //db.Logging += Db_Logging;
            //db.Open();
            try
            {
                using (var db = new DbContext(new DbContextBuilder
                {
                    Connection = new MySqlConnection("server=127.0.0.1;port=3306;user id=root;password=1024;database=test;"),
                }))
                {
                    db.Open();
                    db.BeginTransaction();
                    var list = db.Query("select * from student where id=@id",new { id= 35006 });
                    db.CommitTransaction();
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }

        [Test]
        public void CreateIntanceof()
        {
            //反射性能测试
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("time", typeof(DateTime));
            for (int i = 0; i < 2000000; i++)
            {
                var row = table.NewRow();
                row[0] = i;
                row[1] = "name" + i;
                row[2] = DateTime.Now;
                table.Rows.Add(row);
            }
            //自定义的内存数据源，
            var reader = new MemoryDataReader(table);
            var stopwatch = new Stopwatch();
            var func = GlobalSettings.EntityMapperProvider.GetSerializer<Student2Dot>(reader);
            var pops = typeof(Student2Dot).GetProperties();
            stopwatch.Start();
            while (reader.Read())
            {
                #region 反射
                ////反射
                //var student = Activator.CreateInstance(typeof(Student));
                //if (!reader.IsDBNull(0))
                //{
                //    pops[0].SetValue(student, reader.GetInt32(0));
                //}
                //if (!reader.IsDBNull(1))
                //{
                //    pops[1].SetValue(student, reader.GetString(1));
                //}
                //if (!reader.IsDBNull(2))
                //{
                //    pops[2].SetValue(student, reader.GetDateTime(2));
                //}
                #endregion

                #region 手写
                //var stu = new Student();
                //if (!reader.IsDBNull(0))
                //{
                //    stu.id = reader.GetInt32(0);
                //}
                //if (!reader.IsDBNull(1))
                //{
                //    stu.name = reader.GetString(1);
                //}
                //if (!reader.IsDBNull(2))
                //{
                //    stu.time = reader.GetDateTime(2);
                //}
                #endregion

                #region sqlbatis
                //sqlbatis
                //var entity = func(reader);
                #endregion
            }
            stopwatch.Stop();
        }
       
        [Test]
        public void AutoOpenConnectionAsync()
        {
            GlobalSettings.EntityMapperProvider = new DefaultEntityMapperProvider();
            var builder = new DbContextBuilder
            {
                Connection = new MySqlConnection("server=127.0.0.1;port=3306;user id=root;password=1024;database=test;"),
            };
            using (var db = new DbContext(builder))
            {
                //db.Open();
                db.BeginTransaction();
                var list = db.Query("SELECT * FROM `student` a JOIN student_score b on a.id=b.student_id");
                db.CommitTransaction();
            }
        }
    }
    class P
    {
        public int Id { get; set; }
        public int? Age { get; set; }//Age type must be int?
    }
    public class Student2Dot
    {
        public int id { get; set; }

        public string stu_name { get; set; }

        public bool is_del { get; set; }
    }
    public class StudentScore
    {
        public int Id { get; set; }
        public double Score { get; set; }
    }
    
}