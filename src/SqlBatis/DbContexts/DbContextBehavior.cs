using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SqlBatis
{

    /// <summary>
    /// 提供数据转换能力
    /// </summary>
    public class DbContextBehavior
    {
        #region 内部属性
        /// <summary>
        /// 序列化器
        /// </summary>
        private static readonly ConcurrentDictionary<SerializerKey, object> _serializers
            = new ConcurrentDictionary<SerializerKey, object>();

        /// <summary>
        /// 参数解序列化器
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object, Dictionary<string, object>>> _deserializers
            = new ConcurrentDictionary<Type, Func<object, Dictionary<string, object>>>();
        #endregion

        #region 一组可以被重写的策略
        /// <summary>
        /// 获取实体序列化器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <returns></returns>
        internal Func<IDataRecord, T> GetDataReaderEntityHandler<T>(IDataRecord record)
        {
            var names = new StringBuilder();
            if (record.FieldCount>1)
            {
                for (int i = 0; i < record.FieldCount; i++)
                {
                    names.Append(record.GetName(i));
                    if (i+1!= record.FieldCount)
                    {
                        names.Append('.');
                    }
                }
            }
            var key = new SerializerKey(typeof(T), names.ToString());
            var handler = _serializers.GetOrAdd(key, k =>
            {
                 return CreateEntityBindHandler<T>(record);
            });
            return handler as Func<IDataRecord, T>;
        }

        /// <summary>
        /// 获取动态实体序列化器
        /// </summary>
        internal Func<IDataRecord, dynamic> GetDataReaderDynamicHandler()
        {
            return (reader) =>
            {
                var expando = new System.Dynamic.ExpandoObject();
                var entity = (IDictionary<string, dynamic>)expando;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    if (reader.IsDBNull(i))
                    {
                        return null;
                    }
                    var value = reader.GetValue(i);
                    entity.Add(name, value);
                }
                return entity;
            };
        }

        /// <summary>
        /// 获取类成员到字典的一个转换器
        /// </summary>
        internal static Func<object, Dictionary<string, object>> GetEntityToDictionaryHandler(Type type)
        {
            if (type == typeof(Dictionary<string, object>))
            {
                return (object param) => param as Dictionary<string, object>;
            }
            var handler = _deserializers.GetOrAdd(type, t =>
            {
                return CreateEntityUnBindHandler(type);
            });
            return handler;
        }
       
        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual T ChangeType<T>(object value)
        {
            if (value is null || value is DBNull) return default;
            if (value is T t) return t;
            var type = typeof(T);
            type = GetUnderlyingType(type);
            if (type.IsEnum)
            {
                if (value is float || value is double || value is decimal)
                {
                    value = Convert.ChangeType(value, Enum.GetUnderlyingType(type), System.Globalization.CultureInfo.InvariantCulture);
                }
                return (T)Enum.ToObject(type, value);
            }
            return (T)Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture);
        }
        
        /// <summary>
        /// 创建数据库参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual KeyValuePair<string,object> CreateDbCommandParameter(string name, object value)
        {
            return new KeyValuePair<string, object>(name,value);
        }
        
        /// <summary>
        /// 获取构造器
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        protected virtual ConstructorInfo FindEntityConstructor(Type entityType)
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
        /// 获取参数
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="recordField"></param>
        /// <returns></returns>
        protected virtual ParameterInfo FindEntityConstructorParameter(ConstructorInfo constructor, DataReaderField recordField)
        {
            foreach (var item in constructor.GetParameters())
            {
                if (recordField.DataName.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                else if (recordField.DataName.Replace("_", "").Equals(item.Name, StringComparison.OrdinalIgnoreCase))
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
        protected virtual MemberInfo FindEntityMember(Type entityType, DataReaderField fieldInfo)
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
        /// <param name="memberType">成员类型</param>
        /// <param name="recordField">数据库字段</param>
        /// <returns></returns>
        protected virtual MethodInfo FindConvertMethod(Type entityType, Type memberType, DataReaderField recordField)
        {
            if (GetUnderlyingType(memberType) == typeof(bool))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToBooleanMethod
                    : DbConvertMethods.ToBooleanNullableMethod;
            }
            if (GetUnderlyingType(memberType).IsEnum)
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToEnumMethod.MakeGenericMethod(memberType)
                    : DbConvertMethods.ToEnumNullableMethod.MakeGenericMethod(GetUnderlyingType(memberType));
            }
            if (GetUnderlyingType(memberType) == typeof(char))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToCharMethod
                    : DbConvertMethods.ToCharNullableMethod;
            }
            if (memberType == typeof(string))
            {
                return DbConvertMethods.ToStringMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(Guid))
            {
                return !IsNullableType(memberType) ? DbConvertMethods.ToGuidMethod
                    : DbConvertMethods.ToGuidNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(DateTime))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToDateTimeMethod
                    : DbConvertMethods.ToDateTimeNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(byte) || GetUnderlyingType(memberType) == typeof(sbyte))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToByteMethod
                    : DbConvertMethods.ToByteNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(short) || GetUnderlyingType(memberType) == typeof(ushort))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToIn16Method
                    : DbConvertMethods.ToIn16NullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(int) || GetUnderlyingType(memberType) == typeof(uint))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToIn32Method
                    : DbConvertMethods.ToIn32NullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(long) || GetUnderlyingType(memberType) == typeof(ulong))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToIn64Method
                    : DbConvertMethods.ToIn64NullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(float))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToFloatMethod
                    : DbConvertMethods.ToFloatNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(double))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToDoubleMethod
                    : DbConvertMethods.ToDoubleNullableMethod;
            }
            if (GetUnderlyingType(memberType) == typeof(decimal))
            {
                return !IsNullableType(memberType)
                    ? DbConvertMethods.ToDecimalMethod
                    : DbConvertMethods.ToDecimalNullableMethod;
            }
            return DbConvertMethods.ToObjectMethod;
        }

        #endregion

        #region 内部私有方法
        /// <summary>
        /// 创建动态方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Func<object, Dictionary<string, object>> CreateEntityUnBindHandler(Type type)
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
        private Func<IDataRecord, T> CreateEntityBindHandler<T>(IDataRecord record)
        {
            var type = typeof(T);
            var methodName = $"Serializer{Guid.NewGuid():N}";
            var dynamicMethod = new DynamicMethod(methodName, type, new Type[] { typeof(IDataRecord) }, type, true);
            var generator = dynamicMethod.GetILGenerator();
            LocalBuilder local = generator.DeclareLocal(type);
            var dataInfos = new DataReaderField[record.FieldCount];
            for (int i = 0; i < record.FieldCount; i++)
            {
                var dataname = record.GetName(i);
                var datatype = record.GetFieldType(i);
                var typename = record.GetDataTypeName(i);
                dataInfos[i] = new DataReaderField(i, typename, datatype, dataname);
            }
            if (type == typeof(object) || dataInfos.Length == 1 && (type.IsValueType || type == typeof(string)))
            {
                var convertMethod = FindConvertMethod(type, type, dataInfos[0]);
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
            var constructor = FindEntityConstructor(type);
            if (constructor.GetParameters().Length > 0)
            {
                var parameters = constructor.GetParameters().ToList();
                var locals = new LocalBuilder[parameters.Count];
                for (int i = 0; i < locals.Length; i++)
                {
                    locals[i] = generator.DeclareLocal(parameters[i].ParameterType);
                }
                foreach (var item in dataInfos)
                {
                    var parameter = FindEntityConstructorParameter(constructor, item);
                    if (parameter == null)
                    {
                        continue;
                    }
                    int i = parameters.IndexOf(parameter);
                    var convertMethod = FindConvertMethod(type, parameter.ParameterType, item);
                    if (convertMethod==null)
                    {
                        continue;
                    }
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
                    var property = FindEntityMember(type, item) as PropertyInfo;
                    if (property == null)
                    {
                        continue;
                    }
                    var convertMethod = FindConvertMethod(type, property.PropertyType, item);
                    if (convertMethod == null)
                    {
                        continue;
                    }
                    generator.Emit(OpCodes.Ldloc, local);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, item.Ordinal);
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
            return Nullable.GetUnderlyingType(type) ?? type;
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
    }

    /// <summary>
    /// 获取实体序列化器的hashkey
    /// </summary>
    internal struct SerializerKey : IEquatable<SerializerKey>
    {
        private string _columns;

        private Type Type { get; set; }

        public override bool Equals(object obj) => obj is SerializerKey key && Equals(key);

        public bool Equals(SerializerKey other)
        {
            if (Type != other.Type)
                return false;
            else if (_columns.Length != other._columns.Length)
                return false;
            else if (_columns != other._columns)
                return false;
            return true;
        }

        public override int GetHashCode() => Type.GetHashCode();

        public SerializerKey(Type type, string names)
        {
            Type = type;
            _columns = names;
        }
    }
    /// <summary>
    /// DataReader中的行信息
    /// </summary>
   
    public class DataReaderField
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
        public DataReaderField(int ordinal, string typeName, Type dataType, string dataName)
        {
            Ordinal = ordinal;
            TypeName = typeName;
            DataType = dataType;
            DataName = dataName;
        }
    }
}
