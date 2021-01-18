using System;
using System.Text.RegularExpressions;
using Xunit;

namespace SqlBatis.XUnit
{
    public class InsertTest : BaseTest
    {
        [Fact(DisplayName = "SQL插入")]
        public void ExecuteSql()
        {
            try
            {
                _context.Open();
                var data = new StudentDto
                {
                    Score = 50.5,
                    StuGender = true,
                    CreateTime = DateTime.Now,
                    Version = Guid.NewGuid().ToString("N"),
                    StuName = "zs",
                };
                var sql = @"INSERT INTO student
                (stu_name,stu_gender,score,version,create_time) values 
                (@StuName,@StuGender,@Score,@Version,@CreateTime)";
                var row = _context.Execute(sql, data);
                Assert.Equal(1, row);
            }
            catch (Exception e)
            {

                throw;
            }
           
        }

        [Fact(DisplayName = "基本插入")]
        public void BaseInsert()
        {
            var row = _context.From<StudentDto>().Insert(new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            });
            Assert.Equal(1, row);
        }

        [Fact(DisplayName = "默认值插入")]
        public void DeafultInsert()
        {
            var row = _context.From<StudentDto>().Insert(new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            });
            var entity = _context.From<StudentDto>()
                .OrderByDescending(a => a.Id)
                .Single();
            Assert.NotNull(entity.CreateTime);
        }

        [Fact(DisplayName = "数据版本插入")]
        public void ConcurrencyInsert()
        {
            var row = _context.From<StudentDto>().Insert(new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                StuName = "zs",
                Version="aaaaaa"
            });
            var entity = _context.From<StudentDto>()
                .OrderByDescending(a => a.Id)
                .Single();
            Assert.NotNull(entity.Version);
        }

        [Fact(DisplayName = "忽略部分字段插入")]
        public void IgnoreColumnInsert()
        {
            var row = _context.From<StudentDto>()
            .Ignore(a => a.Version)
            .Insert(new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            });
            var entity = _context.From<StudentDto>()
                .OrderByDescending(a => a.Id)
                .Single();
            Assert.Null(entity.Version);
        }

        [Fact(DisplayName = "忽略所有Null字段插入")]
        public void IgnoreNullColumnInsert()
        {
            var data = new {Age=5,Name="zs" };
            var row = _context.From<StudentDto>()
              //.Ignore(ignoreAllNullColumns: true)
              .Insert(new
              {
                  StuName = "zs",
              });

            var entity = _context.From<StudentDto>()
                .OrderByDescending(a => a.Id)
                .Single();
            Assert.NotNull(entity.StuName);
            Assert.Null(entity.Score);
        }

        [Fact(DisplayName = "返回自增列插入")]
        public void ReturnIdInsert()
        {
            var version = Guid.NewGuid().ToString("N");
            var id = _context.From<StudentDto>().InsertReturnId(new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                Version = version,
                StuName = "zs",
            });
            var entity = _context.From<StudentDto>()
                .Where(a => a.Version == version)
                .Single();
            Assert.Equal(id, entity.Id);
        }

    }
}
