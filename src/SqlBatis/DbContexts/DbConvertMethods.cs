using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis
{
    internal class DbConvertMethods
    {
        #region Method Field
        internal static MethodInfo ToObjectMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToObject));
        internal static MethodInfo ToByteMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToByte));
        internal static MethodInfo ToIn16Method = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToInt16));
        internal static MethodInfo ToIn32Method = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToInt32));
        internal static MethodInfo ToIn64Method = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToInt64));
        internal static MethodInfo ToFloatMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToFloat));
        internal static MethodInfo ToDoubleMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToDouble));
        internal static MethodInfo ToDecimalMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToDecimal));
        internal static MethodInfo ToBooleanMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToBoolean));
        internal static MethodInfo ToCharMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToChar));
        internal static MethodInfo ToStringMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToString));
        internal static MethodInfo ToTrimStringMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToTrimString));
        internal static MethodInfo ToDateTimeMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToDateTime));
        internal static MethodInfo ToEnumMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToEnum));
        internal static MethodInfo ToGuidMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToGuid));
        #endregion

        #region NullableMethod Field
        internal static MethodInfo ToByteNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToByteNullable));
        internal static MethodInfo ToIn16NullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToInt16Nullable));
        internal static MethodInfo ToIn32NullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToInt32Nullable));
        internal static MethodInfo ToIn64NullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToInt64Nullable));
        internal static MethodInfo ToFloatNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToFloatNullable));
        internal static MethodInfo ToDoubleNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToDoubleNullable));
        internal static MethodInfo ToBooleanNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToBooleanNullable));
        internal static MethodInfo ToDecimalNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToDecimalNullable));
        internal static MethodInfo ToCharNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToCharNullable));
        internal static MethodInfo ToDateTimeNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToDateTimeNullable));
        internal static MethodInfo ToEnumNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToEnumNullable));
        internal static MethodInfo ToGuidNullableMethod = typeof(DbConvertMethods).GetMethod(nameof(DbConvertMethods.ConvertToGuidNullable));
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
}
