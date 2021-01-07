using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace SqlBatis.Test
{
    /// <summary>
    /// 1.定义转换器
    /// </summary>
    public static class MyConvertMethod
    {
        public static MethodInfo CharArrayConvertStringMethod = typeof(MyConvertMethod).GetMethod(nameof(CharArrayConvertString));
        public static MethodInfo StringConvertJsonMethod = typeof(MyConvertMethod).GetMethod(nameof(StringConvertJson));
        /// <summary>
        /// 参数必须得有(IDataRecord record, int i)
        /// </summary>
        /// <param name="record">必须的</param>
        /// <param name="i">必须的</param>
        /// <returns></returns>
        public static string CharArrayConvertString(IDataRecord record, int i)
        {
            if (record.IsDBNull(i))
            {
                return default;
            }
            return record.GetString(i).Trim();
        }
        /// <summary>
        /// 泛型方法
        /// 参数必须得有(IDataRecord record, int i)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static T StringConvertJson<T>(IDataRecord record, int i)
        {
            if (record.IsDBNull(i))
            {
                return default;
            }
            var json = record.GetString(i);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
    }
    /// <summary>
    /// 重写转换器匹配函数
    /// </summary>
    public class MyDbEntityMapperProvider : DbEntityMapperProvider
    {
        protected override MethodInfo MatchDataRecordConvertMethod(Type returnType, Type entityMemberType, DbFieldInfo fieldInfo)
        {
            //如果是char
            if (fieldInfo.TypeName == "nchar"|| fieldInfo.TypeName == "nvarchar")
            {
                return MyConvertMethod.CharArrayConvertStringMethod;
            }
            if (entityMemberType.IsClass&&entityMemberType!=typeof(string))
            {
                return MyConvertMethod.StringConvertJsonMethod.MakeGenericMethod(entityMemberType);
            }
            //否则使用群主默认的
            return base.MatchDataRecordConvertMethod(returnType, entityMemberType, fieldInfo);
        }
    }
}
