using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis
{
    public interface IXmlResovle
    {
        string Resolve<T>(string id, T parameter);
        string Resolve(string id);
    }
}
