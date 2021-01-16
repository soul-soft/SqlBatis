using SqlBatis;
using System.Data;
using SqlBatis.Queryables;
using Microsoft.Extensions.Logging;

namespace SqlBatis.XUnit
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class MyDbContext : DbContext
    {
         private readonly ILogger<MyDbContext> _logger;

         public MyDbContext(DbContextBuilder builder,ILogger<MyDbContext> logger)
            :base(builder)
         {
            _logger = logger;
         }
         
         protected override IDbCommand CreateDbCommand(string sql, object parameter, int? commandTimeout = null, CommandType? commandType = null)
         {
             _logger.LogInformation(sql);
             return base.CreateDbCommand(sql, parameter, commandTimeout, commandType);
         }
               
        /// <summary>
        /// 
        /// </summary>
        public IDbQueryable<StudentDto> Student
        {
            get
            {
                return new DbQueryable<StudentDto>(this);
            
            }
        }
    
    }
}



