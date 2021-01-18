using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    /// <summary>
    /// 修改数据时并发冲突
    /// </summary>
    public class DbUpdateConcurrencyException : Exception
    {
        public DbUpdateConcurrencyException(string message)
            : base(message)
        {

        }
    }
}
