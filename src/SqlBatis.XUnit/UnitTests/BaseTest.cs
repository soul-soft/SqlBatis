using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.XUnit
{
    public class BaseTest
    {
        protected IDbContext _context = null;
       
        public BaseTest()
        {
            _context = new MyDbContext(new DbContextBuilder
            {
                Connection = new MySqlConnector.MySqlConnection("server=127.0.0.1;user id=root;password=1024;database=test;"),
                DbContextType = DbContextType.Mysql
            });
        }
    }
}
