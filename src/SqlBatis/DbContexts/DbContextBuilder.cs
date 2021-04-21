using SqlBatis.Expressions;
using System.Data;

namespace SqlBatis
{
    public class DbContextBuilder
    {
        /// <summary>
        /// 设置要托管的数据库连接
        /// </summary>
        public IDbConnection Connection { get; set; }
        /// <summary>
        /// 设置数据库类型
        /// </summary>
        public DbContextType DbContextType { get; set; }
        /// <summary>
        /// 上下文行为
        /// </summary>
        public IDbContextBehavior DbContextBehavior { get; set; } = new DbContextBehavior();
    }
}
