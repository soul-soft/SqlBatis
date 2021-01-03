using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace SqlBatis.Test
{
    public class DefaultEntityMapper : SqlBatis.DefaultEntityMapper
    {
        static class ConvertMethod
        {
            public static MethodInfo CharArrayConvertStringMethod = typeof(ConvertMethod).GetMethod(nameof(CharArrayConvertString));
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

        protected override MethodInfo MatchDataRecordConvertMethod(Type returnType, Type memberType, DbFieldInfo fieldInfo)
        {
            //实现讲字符串转换成bool
            if (fieldInfo.TypeName=="char")
            {
                return ConvertMethod.CharArrayConvertStringMethod;
            }
            return base.MatchDataRecordConvertMethod(returnType, memberType, fieldInfo);
        }
    }
}
