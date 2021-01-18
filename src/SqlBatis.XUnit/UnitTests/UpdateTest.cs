using SqlBatis.Expressions;
using System;
using System.Data;
using Xunit;

namespace SqlBatis.XUnit
{
    public class UpdateTest : BaseTest
    {
        [Fact(DisplayName = "基本修改")]
        public void BaseUpdate()
        {
            var data = new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            };
            _context.From<StudentDto>().Insert(data);
            var entity = _context.From<StudentDto>().Where(a => a.Version == data.Version).Single();
            entity.Score = 1;
            entity.StuName = "BaseUpdate";
            entity.StuGender = false;
            var row = _context.From<StudentDto>().Update(entity);
            Assert.Equal(1, row);
        }

        [Fact(DisplayName = "并发检查修改")]
        public void ConcurrencyCheckUpdate()
        {
            try
            {
                var data = new StudentDto
                {
                    Score = 50.5,
                    StuGender = true,
                    CreateTime = DateTime.Now,
                    Version = Guid.NewGuid().ToString("N"),
                    StuName = "zs",
                };
                _context.From<StudentDto>().Insert(data);
                var entity = _context.From<StudentDto>().Where(a => a.Version == data.Version).Single();
                entity.Score = 1;
                entity.StuName = "ConcurrencyCheckUpdate";
                entity.StuGender = false;
                entity.Version = Guid.NewGuid().ToString("N");//修改实体的版本号将导致异常
                var row = _context.From<StudentDto>().Update(entity);
                Assert.Equal(1, row);
            }
            catch (Exception e)
            {
                Assert.True(e is DbUpdateConcurrencyException);
            }

        }

        [Fact(DisplayName = "忽略字段修改")]
        public void IgnoreColumnUpdate()
        {
            var data = new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            };
            _context.From<StudentDto>().Insert(data);
            var entity = _context.From<StudentDto>().Where(a => a.Version == data.Version).Single();
            entity.Score = 1;
            entity.StuName = "IgnoreUpdate";
            entity.StuGender = false;
            var row = _context.From<StudentDto>()
                .Ignore(a => a.Score)
                .Update(entity);
            var newentity = _context.From<StudentDto>().Where(a => a.Id == entity.Id).Single();
            Assert.Equal(newentity.Score, data.Score);
        }
       
        [Fact(DisplayName = "忽略空值字段修改")]
        public void IgnoreAllNullColumnUpdate()
        {
            var data = new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            };
            _context.From<StudentDto>().Insert(data);
            var entity = _context.From<StudentDto>().Where(a => a.Version == data.Version).Single();
            var updateEntity = new  
            {
                Id=entity.Id,
                StuName="aaa",
                data.Version
            };
            var row = _context.From<StudentDto>()
              //.Ignore(ignoreAllNullColumns: true)
              .Update(updateEntity);
            var queryEntity = _context.From<StudentDto>().Where(a => a.Id == entity.Id).Single();
            Assert.Equal("aaa", queryEntity.StuName);
            Assert.Equal(data.StuGender, queryEntity.StuGender);
        }
        
        [Fact(DisplayName = "表达式修改")]
        public void ExpressionUpdate()
        {
            var data = new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            };
            var id = _context.From<StudentDto>().InsertReturnId(data);
            var row = _context.From<StudentDto>()
                .Set(a => a.Score, a => a.Score + 0.5)
                .Where(a => a.Id == id)
                .Update();
            var entity = _context.From<StudentDto>().Where(a => a.Id == id).Single();
            Assert.Equal(entity.Score, data.Score+0.5);
        }
    }
}
