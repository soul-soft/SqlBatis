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
    /// 默认实体映射器
    /// </summary>
    public class DbEntityMapperProvider
    {
        #region 内部属性
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
        #endregion

        #region 一组可以被重写的策略
        /// <summary>
        /// 获取实体序列化器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <returns></returns>
        public virtual Func<IDataRecord, T> GetEntityMapper<T>(IDataRecord record)
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
        public virtual Func<IDataRecord, dynamic> GetEntityMapper()
        {
            return (reader) =>
            {
                var expando = new System.Dynamic.ExpandoObject();
                var entity = (IDictionary<string, dynamic>)expando;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    var value = GetEntityMemberDynamicValue(reader, i);
                    entity.Add(name, value);
                }
                return entity;
            };
        }

        /// <summary>
        /// 获取实体解构器
        /// </summary>
        public virtual Func<object, Dictionary<string, object>> GetDeserializer(Type type)
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

        /// <summary>
        /// 获取参数最多的构造器
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        protected virtual ConstructorInfo MatchEntityConstructor(Type entityType)
        {
            var constructor = entityType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                var constructors = entityType.GetConstructors();
                var maxLength = constructors.Max(s => s.GetParameters().Length);
                constructor = constructors
                    .Where(a => a.GetParameters().Length == maxLength)
                    .FirstOrDefault();
            }
            return constructor;
        }

        /// <summary>
        /// 获取参数在DataReader中的顺序
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <param name="fieldInfos"></param>
        /// <returns></returns>
        protected virtual DbFieldInfo MatchEntityConstructorParameter(ParameterInfo parameterInfo, DbFieldInfo[] fieldInfos)
        {
            foreach (var item in fieldInfos)
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
        /// 获取实体的成员信息
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="fieldInfo">数据字段信息</param>
        /// <returns></returns>
        protected virtual MemberInfo MatchEntityPropertyInfo(Type entityType, DbFieldInfo fieldInfo)
        {
            var properties = entityType.GetProperties();
            foreach (var item in properties)
            {
                if (item.Name.Equals(fieldInfo.DataName, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                else if (item.Name.Equals(fieldInfo.DataName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取映射实体成员的转换方法
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityMemberType">实体的成员类型</param>
        /// <param name="fieldInfo">数据库字信息</param>
        /// <returns></returns>
        protected virtual MethodInfo MatchDataRecordConvertMethod(Type entityType, Type entityMemberType, DbFieldInfo fieldInfo)
        {
            if (GetUnderlyingType(entityMemberType) == typeof(bool))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToBooleanMethod
                    : DataRecordConvertMethods.ToBooleanNullableMethod;
            }
            if (GetUnderlyingType(entityMemberType).IsEnum)
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToEnumMethod.MakeGenericMethod(entityMemberType)
                    : DataRecordConvertMethods.ToEnumNullableMethod.MakeGenericMethod(GetUnderlyingType(entityMemberType));
            }
            if (GetUnderlyingType(entityMemberType) == typeof(char))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToCharMethod
                    : DataRecordConvertMethods.ToCharNullableMethod;
            }
            if (entityMemberType == typeof(string))
            {
                return DataRecordConvertMethods.ToStringMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(Guid))
            {
                return !IsNullableType(entityMemberType) ? DataRecordConvertMethods.ToGuidMethod
                    : DataRecordConvertMethods.ToGuidNullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(DateTime))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToDateTimeMethod
                    : DataRecordConvertMethods.ToDateTimeNullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(byte) || GetUnderlyingType(entityMemberType) == typeof(sbyte))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToByteMethod
                    : DataRecordConvertMethods.ToByteNullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(short) || GetUnderlyingType(entityMemberType) == typeof(ushort))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToIn16Method
                    : DataRecordConvertMethods.ToIn16NullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(int) || GetUnderlyingType(entityMemberType) == typeof(uint))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToIn32Method
                    : DataRecordConvertMethods.ToIn32NullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(long) || GetUnderlyingType(entityMemberType) == typeof(ulong))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToIn64Method
                    : DataRecordConvertMethods.ToIn64NullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(float))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToFloatMethod
                    : DataRecordConvertMethods.ToFloatNullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(double))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToDoubleMethod
                    : DataRecordConvertMethods.ToDoubleNullableMethod;
            }
            if (GetUnderlyingType(entityMemberType) == typeof(decimal))
            {
                return !IsNullableType(entityMemberType)
                    ? DataRecordConvertMethods.ToDecimalMethod
                    : DataRecordConvertMethods.ToDecimalNullableMethod;
            }
            return DataRecordConvertMethods.ToObjectMethod;
        }

        /// <summary>
        /// 获取动态映射值
        /// </summary>
        protected virtual dynamic GetEntityMemberDynamicValue(IDataRecord record, int i)
        {
            if (record.IsDBNull(i))
            {
                return null;
            }
            return record.GetValue(i);
        }
        #endregion

        #region 内部私有方法
        /// <summary>
        /// 创建动态方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Func<object, Dictionary<string, object>> CreateTypeDeserializerHandler(Type type)
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
            if (type == typeof(object) || dataInfos.Length == 1 && (type.IsValueType || type == typeof(string)))
            {
                var convertMethod = MatchDataRecordConvertMethod(type, type, dataInfos[0]);
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
            var constructor = MatchEntityConstructor(type);
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
                    var item = MatchEntityConstructorParameter(parameters[i], dataInfos);
                    if (item == null)
                    {
                        continue;
                    }
                    var convertMethod = MatchDataRecordConvertMethod(type, parameters[i].ParameterType, item);
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
                generator.Emit(OpCodes.Newobj, constructor);
                generator.Emit(OpCodes.Stloc, local);
                foreach (var item in dataInfos)
                {
                    var property = MatchEntityPropertyInfo(type, item) as PropertyInfo;
                    if (property == null)
                    {
                        continue;
                    }
                    var convertMethod = MatchDataRecordConvertMethod(type, property.PropertyType, item);
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
        private static Type GetUnderlyingType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType ?? type;
        }
        /// <summary>
        /// 判断是否是null的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsNullableType(Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region SerializerKey
        /// <summary>
        /// 获取实体序列化器的hashkey
        /// </summary>
        struct SerializerKey : IEquatable<SerializerKey>
        {
            private string[] Columns { get; set; }
            
            private Type Type { get; set; }
            
            public override bool Equals(object obj)=> obj is SerializerKey key && Equals(key);
            
            public bool Equals(SerializerKey other)
            {
                if (Type != other.Type)
                    return false;
                else if (Columns.Length != other.Columns.Length)
                    return false;
                else
                    for (int i = 0; i < Columns.Length; i++)
                        if (Columns[i] != other.Columns[i])
                            return false;
                return true;
            }
           
            public override int GetHashCode()=> Type.GetHashCode();
            
            public SerializerKey(Type type, string[] names)
            {
                Type = type;
                Columns = names;
            }
        }
        #endregion
    }

    /// <summary>
    /// 定义映射到成员的转换器
    /// </summary>
    public class DataRecordConvertMethods
    {
        #region Method Field
        internal static MethodInfo ToObjectMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToObject));
        internal static MethodInfo ToByteMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToByte));
        internal static MethodInfo ToIn16Method = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToInt16));
        internal static MethodInfo ToIn32Method = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToInt32));
        internal static MethodInfo ToIn64Method = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToInt64));
        internal static MethodInfo ToFloatMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToFloat));
        internal static MethodInfo ToDoubleMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToDouble));
        internal static MethodInfo ToDecimalMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToDecimal));
        internal static MethodInfo ToBooleanMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToBoolean));
        internal static MethodInfo ToCharMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToChar));
        internal static MethodInfo ToStringMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToString));
        internal static MethodInfo ToTrimStringMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToTrimString));
        internal static MethodInfo ToDateTimeMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToDateTime));
        internal static MethodInfo ToEnumMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToEnum));
        internal static MethodInfo ToGuidMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToGuid));
        #endregion

        #region NullableMethod Field
        internal static MethodInfo ToByteNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToByteNullable));
        internal static MethodInfo ToIn16NullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToInt16Nullable));
        internal static MethodInfo ToIn32NullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToInt32Nullable));
        internal static MethodInfo ToIn64NullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToInt64Nullable));
        internal static MethodInfo ToFloatNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToFloatNullable));
        internal static MethodInfo ToDoubleNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToDoubleNullable));
        internal static MethodInfo ToBooleanNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToBooleanNullable));
        internal static MethodInfo ToDecimalNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToDecimalNullable));
        internal static MethodInfo ToCharNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToCharNullable));
        internal static MethodInfo ToDateTimeNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToDateTimeNullable));
        internal static MethodInfo ToEnumNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToEnumNullable));
        internal static MethodInfo ToGuidNullableMethod = typeof(DataRecordConvertMethods).GetMethod(nameof(DataRecordConvertMethods.ConvertToGuidNullable));
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

        public static string ConvertToTrimString(IDataRecord dr, int i)
        {
            try
            {
                if (dr.IsDBNull(i))
                {
                    return default;
                }
                return dr.GetString(i).Trim();
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

        #region Throw Exception
        private static Exception ThrowException<T>(IDataRecord dr, int i)
        {
            var column = dr.GetName(i);
            var fieldType = dr.GetFieldType(i);
            return new InvalidCastException($"Unable to cast object of type '{fieldType}' to type '{typeof(T)}' at the column '{column}'.");
        }
        #endregion
    }

    /// <summary>
    /// DataReader中的行信息
    /// </summary>
    public class DbFieldInfo
    {
        /// <summary>
        /// 数据库类型名称
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 数据库对应的C#属性类型
        /// </summary>
        public Type DataType { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string DataName { get; set; }
        /// <summary>
        /// 列序列
        /// </summary>
        public int Ordinal { get; set; }
        /// <summary>
        /// 数据库字段描述
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="typeName"></param>
        /// <param name="dataType"></param>
        /// <param name="dataName"></param>
        public DbFieldInfo(int ordinal, string typeName, Type dataType, string dataName)
        {
            Ordinal = ordinal;
            TypeName = typeName;
            DataType = dataType;
            DataName = dataName;
        }
    }
}
