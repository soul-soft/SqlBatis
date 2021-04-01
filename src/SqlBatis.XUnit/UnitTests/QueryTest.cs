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
                var query = new
                {
                    Keywords = (string)null
                };
                var b1 = new SqlBuilder();
                b1.Where("name like @Keywords", query.Keywords != null)
                  .Where("is_del=true")
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
            SqlBatisSettings.IgnoreDbCommandInvalidParameters = true;
            _context.Query("select * from student");
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

        [Fact(DisplayName = "忽略列")]
        public void Ignore()
        {
            var list = _context.From<StudentDto>()
                .Ignore(a => a.StuGender)
                .Select();

        }
        [Fact(DisplayName = "表连接1")]
        public void Join2()
        {
            var list = _context.From<StudentDto, SchoolDto>()
                .Join((a, b) => a.SchId == b.Id)
                .Select((a, b) => new
                {
                    a.Id,
                    a.StuName,
                    b.SchName
                });

        }
        [Fact(DisplayName = "表连接2")]
        public void Join3()
        {
            var list = _context.From<StudentDto, SchoolDto, AddressDto>()
               .Join<StudentDto, SchoolDto>((a, b) => a.SchId == b.Id)
               .Join<StudentDto, AddressDto>((a, c) => a.AddrId == c.Id)
               .Select((a, b, c) => new
               {
                   a.Id,
                   a.StuName,
                   b.SchName,
                   c.AddrName
               });
        }

        [Fact(DisplayName = "表连接3")]
        public void Join4()
        {
            try
            {
                var sum = _context.From<StudentDto>().Sum(a => a.Id);
            }
            catch (Exception e)
            {

                throw;
            }
            
            var flag = _context.From<StudentDto>().Where(a => a.Id > 0).Exists();
            var list1 = _context.From<StudentDto>().Select(s=>new StudentDto 
            {
                Id=s.Id,
                StuName=s.StuName
            });
            var list2 = _context.From<StudentDto>().Select(s => new StudentDto
            {
                Id = s.Id,
                StuName = s.StuName,
                StuGender=s.StuGender
            });
        }
    }
}
