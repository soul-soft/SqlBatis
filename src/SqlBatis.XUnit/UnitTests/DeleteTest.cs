using System;
using Xunit;

namespace SqlBatis.XUnit
{
    public class DeleteTest : BaseTest
    {
        [Fact(DisplayName = "»ù±¾É¾³ý")]
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
    }
}
