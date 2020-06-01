using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace SqlBatis.Test
{
    public class DefaultEntityMapperProvider : EntityMapperProvider
    {
        static class ConvertMethod
        {
            public static MethodInfo ConvertToBooleanMethod = typeof(ConvertMethod).GetMethod(nameof(ConvertToBoolean));
            public static bool ConvertToBoolean(IDataRecord record, int i)
            {
                if (record.IsDBNull(i))
                {
                    return false;
                }
                return record.GetValue(i).ToString() == "Ok";
            }
        }

        protected override MethodInfo FindTypeMethod(Type returnType, Type memberType)
        {
            if (typeof(Student2Dot) == returnType && memberType == typeof(bool))
            {
                return ConvertMethod.ConvertToBooleanMethod;
            }
            return base.FindTypeMethod(returnType, memberType);
        }
    }
}
