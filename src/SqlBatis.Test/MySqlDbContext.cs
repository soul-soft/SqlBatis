using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace SqlBatis.Test
{
    public class MysqlDbContext : DbContext
    {
        public IDbQuery<StudentDto> Students { get => new DbQuery<StudentDto>(this); }
        public MysqlDbContext(DbContextBuilder builder)
            :base(builder)
        {

        }
    }
}
