using SqlBatis.Queryables;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.Test.Extensions
{
    public class DefaultDbContext : DbContext
    {
        public DefaultDbContext(DbContextBuilder builder)
            : base(builder)
        {

        }
        public IDbQueryable<StudentDto> Students =>
            new DbQueryable<StudentDto>(this);
    }
}
