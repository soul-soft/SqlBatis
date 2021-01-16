using System;
using Xunit;

namespace SqlBatis.XUnit
{
    public class QueryTest : BaseTest
    {
        [Fact(DisplayName = "获取单个")]
        public void SqlBuilder()
        {
            try
            {
                var b1 = new SqlBuilder();
                b1.Where("id>@Id")
                  .Where("score>@Score")
                  .Join("student_name as b on a.id=b.sid");
                var p = new { Age = 1, Score = 20 };
                var tmp1 = b1.Build("select * from student as a /**join**/ /**where**/ ");
                var sql = tmp1.RawSql;
                var tmp2 = b1.Build("select count(1) from student as a /**join**/ /**where**/ ");
            }
            catch (Exception e)
            {

                throw;
            }

        }
        [Fact(DisplayName = "获取单个")]
        public void Single()
        {

            var p = new { b = new { id = 444 } };
            var data = _context.From<StudentDto>()
                .Where(a => a.Id == p.b.id)
                .Single();
        }

        [Fact(DisplayName = "分页")]
        public void Page()
        {
            var (list1, count1) = _context.From<StudentDto>()
                .Page(1, 2)
                .SelectMany();
            var (list2, count2) = _context.From<StudentDto>()
                .Page(2, 2)
                .SelectMany();
        }

        [Fact(DisplayName = "R锁")]
        public void Lock()
        {
            var (list1, count1) = _context.From<StudentDto>()
                .With("FOR UPDATE")
                .Page(1, 2)
                .SelectMany();
            var (list2, count2) = _context.From<StudentDto>()
                .With("FOR UPDATE")
                .Page(2, 2)
                .SelectMany();
        }
    }
}
