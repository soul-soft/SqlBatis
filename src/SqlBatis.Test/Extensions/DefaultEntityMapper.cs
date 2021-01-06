using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace SqlBatis.Test
{
    public static class MyConvertMethod
    {
        public static MethodInfo CharArrayConvertStringMethod = typeof(MyConvertMethod).GetMethod(nameof(CharArrayConvertString));
        /// <summary>
        /// 处理sqlserver中的char数组中的结尾空格
        /// </summary>
        /// <param name="record"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string CharArrayConvertString(IDataRecord record, int i)
        {
            if (record.IsDBNull(i))
            {
                return default;
            }
            return record.GetString(i).Trim();
        }
    }
    public class MyEntityMapper : DbEntityMapper
    {
        protected override MethodInfo MatchDataRecordConvertMethod(Type returnType, Type memberType, DbFieldInfo fieldInfo)
        {
            //实现讲字符串转换成bool
            if (fieldInfo.TypeName == "char")
            {
                return MyConvertMethod.CharArrayConvertStringMethod;
            }
            return base.MatchDataRecordConvertMethod(returnType, memberType, fieldInfo);
        }
    }
}
