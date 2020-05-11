using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace SqlBatis.Test
{
    public class SqlDbContext : DbContext
    {
        private static readonly XmlCommandsProvider resovle;
        public SqlDbContext(DbContextBuilder builder):base(builder)
        {
        }
        public IDbQuery<StudentDto> Students { get => new DbQuery<StudentDto>(this); }
      
      
       
    }
}
