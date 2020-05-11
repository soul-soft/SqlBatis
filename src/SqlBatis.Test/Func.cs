using SqlBatis.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.Test
{
    [Function]
    public static class Func
    {
        public static T COUNT<T>(T column) => default;
        public static string GROUP_CONCAT<T>(T column) => default;
        public static string CONCAT(params object[] columns) => default;
        public static string REPLACE(string column, string oldstr, string newstr) => default;

    }
}
