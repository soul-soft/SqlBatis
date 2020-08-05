using MySql.Data.MySqlClient;
using NUnit.Framework;
using SqlBatis.Attributes;
using Dapper;
using SqlBatis.Expressions;
using SqlBatis.Test.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SqlBatis.Test
{
    public class UnitTests
    {

        [Test]
        public void TestInsertAsync()
        {
            var builder = new DbContextBuilder
            {
                Connection = new MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
            };
            GlobalSettings.XmlCommandsProvider.Load(@"D:\SqlBatis\src\SqlBatis.Test\Student.xml");
            try
            {
                var sql = "insert into advert_banners(banner_img,banner_sort,banner_group) values(@img,@sort,@group)";
                var list = new List<int>();
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i = 0; i < 30; i++)
                {
                    Regex.IsMatch(sql, $@"([\@,\:,\?]+{"fff"})");
                }
                stopwatch.Stop();
                list = list.Distinct().ToList();
                Debug.Write("ºÄÊ±£º"+stopwatch.ElapsedMilliseconds);
                using (var db = new DbContext(builder))
                {
                    var ff = db.Execute("insert into advert_banners(banner_img,banner_sort,banner_group) values(@img,@sort,@group)", new { img=(string)null,sort=20, group=1 });
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        [Test]
        public void TestDynamicConvert()
        {
            var builder = new DbContextBuilder
            {
                Connection = new MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
            };
            try
            {
                var db = new DbContext(builder);
                var list = db.Query("select balance from  student");
                foreach (var item in list)
                {
                    decimal b = item.balance;
                }
            }
            catch (Exception e)
            {

                throw;
            }
           
        }
    }
    public class Student
    {
        public int Id { get; set; }
        [Column("stu_name")]
        public byte[] IsDel { get; set; }
    }
   
}