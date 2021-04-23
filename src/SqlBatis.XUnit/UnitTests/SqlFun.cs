using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.XUnit
{
    [SqlBatis.Attributes.Function]
    public static class SqlFun
    {
        public static T2 IF<T1, T2>(T1 column, T2 v1, T2 v2)
        {
            return default;
        }

        public static bool ISNULL<T1>(T1 t1)
        {
            return default;
        }

        public static T1 COUNT<T1>(T1 t1)
        {
            return default;
        }
    }
}
