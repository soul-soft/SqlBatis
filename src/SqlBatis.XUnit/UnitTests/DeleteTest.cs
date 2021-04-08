using System;
using Xunit;

namespace SqlBatis.XUnit
{
    public class DeleteTest : BaseTest
    {
        [Fact(DisplayName = "基本删除")]
        public void BaseDelete()
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
            _context.From<StudentDto>().Delete(a => a.Id == id);
            var flag = _context.From<StudentDto>().Exists(a => a.Id == id);
            Assert.False(flag);
        }

        [Fact(DisplayName = "删除实体")]
        public void BaseDeleteEntity()
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
            data.Id = id;
            var row = _context.Delete(data);
            var flag = _context.From<StudentDto>().Exists(a => a.Id == id);
            Assert.False(flag);
        }
        [Fact(DisplayName = "删除实体")]
        public void BaseDeleteEntities()
        {
            var data = new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            };
            var data2 = new StudentDto
            {
                Score = 50.5,
                StuGender = true,
                CreateTime = DateTime.Now,
                Version = Guid.NewGuid().ToString("N"),
                StuName = "zs",
            };
            var id = _context.From<StudentDto>().InsertReturnId(data);
            data.Id = id;
            var id2 = _context.From<StudentDto>().InsertReturnId(data2);
            data2.Id = id2;
            var row = _context.DeleteBatch(new StudentDto[] { data,data2});
            var flag = _context.From<StudentDto>().Exists(a => a.Id == id);
            Assert.False(flag);
        }
    }
}
