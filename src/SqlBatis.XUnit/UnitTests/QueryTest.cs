using System;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBatis.XUnit
{
    public class QueryTest : BaseTest
    {
        [Fact(DisplayName = "获取单个")]
        public void SqlBuilder()
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
        [Fact(DisplayName = "获取单个")]
        public void Single()
        {
            _context.Query("select * from student");
            var p = new { b = new { id = 444 } };
            var data = _context.From<StudentDto>()
                .Where(a => a.Id == p.b.id)
                .Single();
        }
        [Fact(DisplayName = "匿名类型查询映射")]
        public void AnTypeQuery()
        {
            var a1 = AnTypeQuery2(s => new
            {
                s.Id,
                s.SchId,
                s.AddrId,
            });
            var a2 = AnTypeQuery2(s => new
            {
                s.AddrId,
                s.SchId,
                s.Id,
            });
            var a3 = AnTypeQuery2(s => new
            {
                s.Id,
                s.AddrId
            });
            var a4 = AnTypeQuery2(s => new
            {
                s.Id,
                s.StuName
            });
        }
        public T AnTypeQuery2<T>(Func<StudentDto, T> func)
        {
            return _context.Query<T>("select id,sch_id,addr_id from student order by id desc").FirstOrDefault();
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
        [Fact(DisplayName = "函数")]
        public void SQLFUNC()
        {
            var list = _context.From<StudentDto>()
                .Ignore(a => a.StuGender)
                .Select(s =>SqlFun.COUNT(1));

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
               .Join((a, b, c) => a.SchId == b.Id)
               .Join((a, b, c) => a.AddrId == c.Id)
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
            var flag = _context.From<StudentDto>().Where(a => a.Id > 0).Exists();
            var list1 = _context.From<StudentDto>().Select(s => new StudentDto
            {
                Id = s.Id,
                StuName = s.StuName
            });
            var list2 = _context.From<StudentDto>().Select(s => new StudentDto
            {
                Id = s.Id,
                StuName = s.StuName,
                StuGender = s.StuGender
            });
        }
        [Fact(DisplayName = "in查询")]
        public void InQuery()
        {
            var arr = new int[] { 449,450};
            var list1 = _context.From<StudentDto>()
                .Where(a=>Operator.In(a.Id,arr))
                .Select();
           
        }

        [Fact(DisplayName = "指定返回类型")]
        public async Task CustomerResultType()
        {
            var arr = new int[] { 449, 450 };
            var list1 = _context.From<StudentDto>()
                .Where(a => Operator.In(a.Id, arr))
                .Select<SubSutdent>();
            var s1 = _context.From<StudentDto>()
               .Where(a => Operator.In(a.Id, arr))
               .Single<SubSutdent>();
            var list2 = await _context.From<StudentDto>()
               .Where(a => Operator.In(a.Id, arr))
               .SelectAsync<SubSutdent>();
            var s2 = await _context.From<StudentDto>()
             .Where(a => Operator.In(a.Id, arr))
             .SingleAsync<SubSutdent>();

        }
    }

    public class SubSutdent
    {
        public int Id { get; set; }
        public string StuName { get; set; }
    }
}
