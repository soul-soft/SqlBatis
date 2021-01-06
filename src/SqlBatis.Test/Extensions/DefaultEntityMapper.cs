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
        public static MethodInfo StringConvertJsonMethod = typeof(MyConvertMethod).GetMethod(nameof(StringConvertJson));
        public static string CharArrayConvertString(IDataRecord record, int i)
        {
            if (record.IsDBNull(i))
            {
                return default;
            }
            return record.GetString(i).Trim();
        }

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
    public class MyEntityMapper : DbEntityMapperProvider
    {
        protected override MethodInfo MatchDataRecordConvertMethod(Type returnType, Type entityMemberType, DbFieldInfo fieldInfo)
        {
            //如果是char
            if (fieldInfo.TypeName == "char")
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
