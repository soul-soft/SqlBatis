using System.Data;

namespace SqlBatis
{
    public class DbContextBuilder
    {
        public IDbConnection Connection { get; set; }
        public DbContextType DbContextType { get; set; }
    }
}
