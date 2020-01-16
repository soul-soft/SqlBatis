using Microsoft.Extensions.Logging;
using SqlBatis.DbContexts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SqlBatis
{
    public class DbContextBuilder
    {
        public IXmlResovle XmlResovle { get; set; }
        public ILogger Logger { get; set; }
        public IDbConnection Connection { get; set; }
        public DbContextType DbContextType { get; set; }
        public ITypeMapper TypeMapper { get; set; }
    }
}
