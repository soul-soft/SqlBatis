using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SqlBatis
{
    /// <summary>
    /// 实体转换映射器
    /// </summary>
    public interface IEntityMapperProvider
    {
        /// <summary>
        /// 获取实体序列化转换器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <returns></returns>
        Func<IDataRecord, T> GetSerializer<T>(IDataRecord record);
        /// <summary>
        /// 获取动态实体列化转换器
        /// </summary>
        /// <returns></returns>
        Func<IDataRecord, dynamic> GetSerializer(IDataRecord record);
        /// <summary>
        /// 获取参数解码器
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Func<object, Dictionary<string, object>> GetDeserializer(Type type);
    }

    /// <summary>
    /// 默认实体映射器
    /// </summary>
    public class EntityMapperProvider : IEntityMapperProvider
    {
        /// <summary>
        /// 序列化器
        /// </summary>
        private readonly ConcurrentDictionary<SerializerKey, object> _serializers
            = new ConcurrentDictionary<SerializerKey, object>();

        /// <summary>
        /// 参数解序列化器
        /// </summary>
        private readonly ConcurrentDictionary<Type, Func<object, Dictionary<string, object>>> _deserializers
            = new ConcurrentDictionary<Type, Func<object, Dictionary<string, object>>>();

        /// <summary>
        /// 获取实体序列化器的hashkey
        /// </summary>
        private struct SerializerKey : IEquatable<SerializerKey>
        {
            private string[] Names { get; set; }
            private Type Type { get; set; }
            public override bool Equals(object obj)
            {
                return obj is SerializerKey && Equals((SerializerKey)obj);
            }
            public bool Equals(SerializerKey other)
            {
                if (Type != other.Type)
                {
                    return false;
                }
                else if (Names == other.Names)
                {
                    return true;
                }
                else if (Names.Length != other.Names.Length)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < Names.Length; i++)
                    {
                        if (Names[i] != other.Names[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            public override int GetHashCode()
            {
                return Type.GetHashCode();
            }
            public SerializerKey(Type type, string[] names)
            {
                Type = type;
                Names = names;
            }
        }

        /// <summary>
        /// 获取实体序列化器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <returns></returns>
        public Func<IDataRecord, T> GetSerializer<T>(IDataRecord record)
        {
            string[] names = new string[record.FieldCount];
            for (int i = 0; i < record.FieldCount; i++)
            {
                names[i] = record.GetName(i);
            }
            var key = new SerializerKey(typeof(T), names.Length == 1 ? null : names);
            var handler = _serializers.GetOrAdd(key, k =>
             {
                 return CreateTypeSerializerHandler<T>(record);
             });
            return handler as Func<IDataRecord, T>;
        }
        /// <summary>
        /// 获取动态实体序列化器
        /// </summary>
        public Func<IDataRecord, dynamic> GetSerializer(IDataRecord record)
        {
            return (reader) =>
            {
                dynamic obj = new System.Dynamic.ExpandoObject();
                var row = (IDictionary<string, object>)obj;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = GetDynamicValue(record, i);
                    row.Add(name, value);
                }
                return row;
            };
        }
        /// <summary>
        /// 获取实体解构器
        /// </summary>
        public Func<object, Dictionary<string, object>> GetDeserializer(Type type)
        {
            if (type == typeof(Dictionary<string, object>))
            {
                return (object param) => param as Dictionary<string, object>;
            }
            var handler = _deserializers.GetOrAdd(type, t =>
            {
                return CreateTypeDeserializerHandler(type);
            });
            return handler;
        }


        #region 一组可以被重写的策略

        /// <summary>
        /// 查找构造函数如果存在带参构造则获取参数最多的构造器进行实体的创建
        /// </summary>
        protected virtual ConstructorInfo FindConstructor(Type csharpType)
        {
            var constructor = csharpType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                var constructors = csharpType.GetConstructors();
                constructor = constructors.Where(a => a.GetParameters().Length == constructors.Max(s => s.GetParameters().Length)).FirstOrDefault();
            }
            return constructor;
        }

        /// <summary>
        /// 获取参数在DataReader中的顺序
        /// </summary>
        protected virtual DbFieldInfo FindTypeParameter(DbFieldInfo[] dataInfos, ParameterInfo parameterInfo)
        {
            foreach (var item in dataInfos)
            {
                if (item.DataName.Equals(parameterInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                else if (item.DataName.Replace("_", "").Equals(parameterInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取成员信息
        /// </summary>
        protected virtual MemberInfo FindTypeMember(MemberInfo[] properties, DbFieldInfo dataInfo)
        {
            foreach (var item in properties)
            {
                if (item.Name.Equals(dataInfo.DataName, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                else if (item.Name.Equals(dataInfo.DataName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取映射实体成员的转换方法
        /// </summary>
        protected virtual MethodInfo FindTypeMethod(Type returnType, Type memberType)
        {
            if (GetUnderlyingType(memberType) == typeof(bool))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToBooleanMethod : MemberMapperMethod.ToBooleanNullableMethod;
            }
            if (GetUnderlyingType(memberType).IsEnum)
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToEnumMethod.MakeGenericMethod(memberType) : MemberMapperMethod.ToEnumNullableMethod.MakeGenericMethod(GetUnderlyingType(memberType));
            }
            if (GetUnderlyingType(memberType) == typeof(char))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToCharMethod : MemberMapperMethod.ToCharNullableMethod;
            }
            if (memberType == typeof(string))
            {
                return MemberMapperMethod.ToStringMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(Guid))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToGuidMethod : MemberMapperMethod.ToGuidNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(DateTime))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToDateTimeMethod : MemberMapperMethod.ToDateTimeNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(byte) || GetUnderlyingType(memberType) == typeof(sbyte))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToByteMethod : MemberMapperMethod.ToByteNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(short) || GetUnderlyingType(memberType) == typeof(ushort))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToIn16Method : MemberMapperMethod.ToIn16NullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(int) || GetUnderlyingType(memberType) == typeof(uint))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToIn32Method : MemberMapperMethod.ToIn32NullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(long) || GetUnderlyingType(memberType) == typeof(ulong))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToIn64Method : MemberMapperMethod.ToIn64NullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(float))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToFloatMethod : MemberMapperMethod.ToFloatNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(double))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToDoubleMethod : MemberMapperMethod.ToDoubleNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(decimal))
            {
                return !IsNullableType(memberType) ? MemberMapperMethod.ToDecimalMethod : MemberMapperMethod.ToDecimalNullableMethod;
            }
            return MemberMapperMethod.ToObjectMethod;
        }

        /// <summary>
        /// 获取动态映射值
        /// </summary>
        protected virtual object GetDynamicValue(IDataRecord record, int i)
        {
            return record.GetValue(i);
        }
        #endregion

        #region 内部私有方法
        /// <summary>
        /// 创建动态方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Func<object, Dictionary<string, object>> CreateTypeDeserializerHandler(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var methodName = $"{type.Name}Deserializer{Guid.NewGuid():N}";
            var dynamicMethod = new DynamicMethod(methodName, typeof(Dictionary<string, object>), new Type[] { typeof(object) }, type, true);
            var generator = dynamicMethod.GetILGenerator();
            LocalBuilder entityLocal1 = generator.DeclareLocal(typeof(Dictionary<string, object>));
            LocalBuilder entityLocal2 = generator.DeclareLocal(type);
            generator.Emit(OpCodes.Newobj, typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, entityLocal1);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, type);
            generator.Emit(OpCodes.Stloc, entityLocal2);
            foreach (var item in properties)
            {
                generator.Emit(OpCodes.Ldloc, entityLocal1);
                generator.Emit(OpCodes.Ldstr, item.Name);
                generator.Emit(OpCodes.Ldloc, entityLocal2);
                generator.Emit(OpCodes.Callvirt, item.GetGetMethod());
                if (item.PropertyType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, item.PropertyType);
                }
                var addMethod = typeof(Dictionary<string, object>).GetMethod(nameof(Dictionary<string, object>.Add), new Type[] { typeof(string), typeof(object) });
                generator.Emit(OpCodes.Callvirt, addMethod);
            }
            generator.Emit(OpCodes.Ldloc, entityLocal1);
            generator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<object, Dictionary<string, object>>)) as Func<object, Dictionary<string, object>>;
        }
        /// <summary>
        /// 创建动态方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <returns></returns>
        private Func<IDataRecord, T> CreateTypeSerializerHandler<T>(IDataRecord record)
        {
            var type = typeof(T);
            var methodName = $"Serializer{Guid.NewGuid():N}";
            var dynamicMethod = new DynamicMethod(methodName, type, new Type[] { typeof(IDataRecord) }, type, true);
            var generator = dynamicMethod.GetILGenerator();
            LocalBuilder local = generator.DeclareLocal(type);
            var dataInfos = new DbFieldInfo[record.FieldCount];
            for (int i = 0; i < record.FieldCount; i++)
            {
                var dataname = record.GetName(i);
                var datatype = record.GetFieldType(i);
                var typename = record.GetDataTypeName(i);
                dataInfos[i] = new DbFieldInfo(i, typename, datatype, dataname);
            }
            if (dataInfos.Length == 1 && (type.IsValueType || type == typeof(string) || type == typeof(object)))
            {
                var convertMethod = FindTypeMethod(type, type);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, 0);
                if (convertMethod.IsVirtual)
                    generator.Emit(OpCodes.Callvirt, convertMethod);
                else
                    generator.Emit(OpCodes.Call, convertMethod);
                if (type == typeof(object) && convertMethod.ReturnType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, convertMethod.ReturnType);
                }
                generator.Emit(OpCodes.Stloc, local);
                generator.Emit(OpCodes.Ldloc, local);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(Func<IDataRecord, T>)) as Func<IDataRecord, T>;
            }
            var constructor = FindConstructor(type);
            if (constructor.GetParameters().Length > 0)
            {
                var parameters = constructor.GetParameters();
                var locals = new LocalBuilder[parameters.Length];
                for (int i = 0; i < locals.Length; i++)
                {
                    locals[i] = generator.DeclareLocal(parameters[i].ParameterType);
                }
                for (int i = 0; i < locals.Length; i++)
                {
                    var item = FindTypeParameter(dataInfos, parameters[i]);
                    if (item == null)
                    {
                        continue;
                    }
                    var convertMethod = FindTypeMethod(type, parameters[i].ParameterType);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, item.Ordinal);
                    if (convertMethod.IsVirtual)
                        generator.Emit(OpCodes.Callvirt, convertMethod);
                    else
                        generator.Emit(OpCodes.Call, convertMethod);
                    generator.Emit(OpCodes.Stloc, locals[i]);
                }
                for (int i = 0; i < locals.Length; i++)
                {
                    generator.Emit(OpCodes.Ldloc, locals[i]);
                }
                generator.Emit(OpCodes.Newobj, constructor);
                generator.Emit(OpCodes.Stloc, local);
                generator.Emit(OpCodes.Ldloc, local);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(Func<IDataRecord, T>)) as Func<IDataRecord, T>;
            }
            else
            {
                var properties = type.GetProperties();
                generator.Emit(OpCodes.Newobj, constructor);
                generator.Emit(OpCodes.Stloc, local);
                foreach (var item in dataInfos)
                {
                    var property = FindTypeMember(properties, item) as PropertyInfo;
                    if (property == null)
                    {
                        continue;
                    }
                    var convertMethod = FindTypeMethod(type, property.PropertyType);
                    if (convertMethod == null)
                    {
                        continue;
                    }
                    int i = record.GetOrdinal(item.DataName);
                    generator.Emit(OpCodes.Ldloc, local);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    if (convertMethod.IsVirtual)
                        generator.Emit(OpCodes.Callvirt, convertMethod);
                    else
                        generator.Emit(OpCodes.Call, convertMethod);
                    generator.Emit(OpCodes.Callvirt, property.GetSetMethod());
                }
                generator.Emit(OpCodes.Ldloc, local);
                generator.Emit(OpCodes.Ret);
                return dynamicMethod.CreateDelegate(typeof(Func<IDataRecord, T>)) as Func<IDataRecord, T>;
            }
        }
        /// <summary>
        /// 获取类型的非Nullable类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type GetUnderlyingType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType ?? type;
        }

        /// <summary>
        /// 判断是否是可以为null的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsNullableType(Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region 定义一组成员实体转函数
        /// <summary>
        /// 定义映射到成员的转换器
        /// </summary>
        static class MemberMapperMethod
        {
            #region Method Field
            public static MethodInfo ToObjectMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToObject));
            public static MethodInfo ToByteMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToByte));
            public static MethodInfo ToIn16Method = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToInt16));
            public static MethodInfo ToIn32Method = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToInt32));
            public static MethodInfo ToIn64Method = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToInt64));
            public static MethodInfo ToFloatMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToFloat));
            public static MethodInfo ToDoubleMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToDouble));
            public static MethodInfo ToDecimalMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToDecimal));
            public static MethodInfo ToBooleanMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToBoolean));
            public static MethodInfo ToCharMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToChar));
            public static MethodInfo ToStringMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToString));
            public static MethodInfo ToDateTimeMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToDateTime));
            public static MethodInfo ToEnumMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToEnum));
            public static MethodInfo ToGuidMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToGuid));
            #endregion

            #region NullableMethod Field
            public static MethodInfo ToByteNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToByteNullable));
            public static MethodInfo ToIn16NullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToInt16Nullable));
            public static MethodInfo ToIn32NullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToInt32Nullable));
            public static MethodInfo ToIn64NullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToInt64Nullable));
            public static MethodInfo ToFloatNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToFloatNullable));
            public static MethodInfo ToDoubleNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToDoubleNullable));
            public static MethodInfo ToBooleanNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToBooleanNullable));
            public static MethodInfo ToDecimalNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToDecimalNullable));
            public static MethodInfo ToCharNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToCharNullable));
            public static MethodInfo ToDateTimeNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToDateTimeNullable));
            public static MethodInfo ToEnumNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToEnumNullable));
            public static MethodInfo ToGuidNullableMethod = typeof(MemberMapperMethod).GetMethod(nameof(MemberMapperMethod.ConvertToGuidNullable));
            #endregion

            #region Define Convert
            public static object ConvertToObject(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetValue(i);
                }
                catch
                {
                    throw ThrowException<object>(dr, i);
                }
            }

            public static byte ConvertToByte(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    var result = dr.GetByte(i);
                    return result;
                }
                catch
                {
                    throw ThrowException<byte>(dr, i);
                }
            }

            public static short ConvertToInt16(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetInt16(i);
                }
                catch
                {
                    throw ThrowException<short>(dr, i);
                }
            }

            public static int ConvertToInt32(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetInt32(i);
                }
                catch
                {
                    throw ThrowException<int>(dr, i);
                }
            }

            public static long ConvertToInt64(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetInt64(i);
                }
                catch
                {
                    throw ThrowException<long>(dr, i);
                }
            }

            public static float ConvertToFloat(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetFloat(i);
                }
                catch
                {
                    throw ThrowException<float>(dr, i);
                }
            }

            public static double ConvertToDouble(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetDouble(i);
                }
                catch
                {
                    throw ThrowException<double>(dr, i);
                }
            }

            public static bool ConvertToBoolean(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetBoolean(i);
                }
                catch
                {
                    throw ThrowException<bool>(dr, i);
                }
            }

            public static decimal ConvertToDecimal(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetDecimal(i);
                }
                catch
                {
                    throw ThrowException<bool>(dr, i);
                }
            }

            public static char ConvertToChar(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    var result = dr.GetChar(i);
                    return result;
                }
                catch
                {
                    throw ThrowException<char>(dr, i);
                }
            }

            public static string ConvertToString(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetString(i);
                }
                catch
                {
                    throw ThrowException<string>(dr, i);
                }
            }

            public static DateTime ConvertToDateTime(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    return dr.GetDateTime(i);
                }
                catch
                {
                    throw ThrowException<DateTime>(dr, i);
                }
            }

            public static T ConvertToEnum<T>(IDataRecord dr, int i) where T : struct
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    var value = dr.GetValue(i);
                    if (Enum.TryParse(value.ToString(), out T result)) return result;
                    return default;
                }
                catch
                {
                    throw ThrowException<T>(dr, i);
                }
            }

            public static Guid ConvertToGuid(IDataRecord dr, int i)
            {
                try
                {
                    if (dr.IsDBNull(i))
                    {
                        return default;
                    }
                    var result = dr.GetGuid(i);
                    return result;
                }
                catch
                {
                    throw ThrowException<Guid>(dr, i);
                }
            }

            #endregion

            #region Define Nullable Convert
            public static byte? ConvertToByteNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToByte(dr, i);
            }
            public static short? ConvertToInt16Nullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToInt16(dr, i);
            }
            public static int? ConvertToInt32Nullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToInt32(dr, i);
            }
            public static long? ConvertToInt64Nullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToInt64(dr, i);
            }
            public static float? ConvertToFloatNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToFloat(dr, i);
            }
            public static double? ConvertToDoubleNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToDouble(dr, i);
            }
            public static bool? ConvertToBooleanNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToBoolean(dr, i);
            }
            public static decimal? ConvertToDecimalNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToDecimal(dr, i);
            }
            public static char? ConvertToCharNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToChar(dr, i);
            }
            public static DateTime? ConvertToDateTimeNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToDateTime(dr, i);
            }
            public static T? ConvertToEnumNullable<T>(IDataRecord dr, int i) where T : struct
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToEnum<T>(dr, i);
            }
            public static Guid? ConvertToGuidNullable(IDataRecord dr, int i)
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return ConvertToGuid(dr, i);
            }
            #endregion


            #region Exception
            private static Exception ThrowException<T>(IDataRecord dr, int i)
            {
                var inner = new InvalidCastException($"Column of [{dr.GetFieldType(i)}][{dr.GetName(i)}] = [{dr.GetValue(i)}] was not recognized as a valid {typeof(T)}.");
                return new InvalidCastException($"Unable to cast object of type '{dr.GetFieldType(i).Name}' to type '{typeof(T)}'.", inner);
            }
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// DataReader中的行信息
    /// </summary>
    public class DbFieldInfo
    {
        public string TypeName { get; set; }
        public Type DataType { get; set; }
        public string DataName { get; set; }
        public int Ordinal { get; set; }
        public DbFieldInfo(int ordinal, string typeName, Type dataType, string dataName)
        {
            Ordinal = ordinal;
            TypeName = typeName;
            DataType = dataType;
            DataName = dataName;
        }
    }
}
