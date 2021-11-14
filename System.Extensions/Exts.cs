using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Exts
{
    public static class Extensions
    {
        public static string GetDefaultSqlString<TValue>(TValue value) =>
            value switch
            {
                //string str => $"'{str}'",
                //int i => i.ToString(),
                //char ch => $"'{ch}'",
                decimal d => d.ToString(CultureInfo.InvariantCulture),
                Guid guid => $"'{guid}'",
                //DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss}'",
                bool b => $"{(b ? 1 : 0)}",
                _ => value?.ToString()
            };

        public static SqlDbType DefaultMap<TValue>(TValue value) =>
           DefaultMap(typeof(TValue) == typeof(object) ? value.GetType() : typeof(TValue));

        internal static Dictionary<Type, SqlDbType> SqlDbTypes = new Dictionary<Type, SqlDbType>()
        {
            {typeof(bool), SqlDbType.Bit},
            {typeof(DateTime), SqlDbType.DateTime},
            {typeof(decimal), SqlDbType.Decimal},
            {typeof(double), SqlDbType.Float},
            {typeof(float), SqlDbType.Float},
            {typeof(int), SqlDbType.Int},
            {typeof(Guid), SqlDbType.UniqueIdentifier},
            {typeof(string), SqlDbType.NVarChar},
            {typeof(byte), SqlDbType.TinyInt},
            {typeof(byte[]), SqlDbType.Binary}
        };

        public static SqlDbType DefaultMap(Type type) =>
            SqlDbTypes.ContainsKey(type)
            ? SqlDbTypes[type]
            : type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? DefaultMap(Nullable.GetUnderlyingType(type))
                    : throw new NotImplementedException($"{type.Name} is not implemented");

        public static StringBuilder TrimEnd(this StringBuilder sb, bool leftOneWhiteSpace = false)
        {
            if (sb == null || sb.Length == 0)
                return sb;

            int i = sb.Length - 1;
            for (; i >= 0; i--)
                if (!char.IsWhiteSpace(sb[i]))
                    break;

            if (i >= sb.Length - 1 || (leftOneWhiteSpace && i == sb.Length - 2))
                return sb;

            if (leftOneWhiteSpace)
                sb.Remove(i + 2, sb.Length - i);
            else sb.Remove(i + 1, sb.Length - i - 1);

            return sb;
        }

        public static string WithAlpha(this string str) =>
            !string.IsNullOrEmpty(str) && str.Length != 0 && str[0] != '@' ? $"@{str}" : str;

        public static string WithAliasBrackets(this string str) =>
            !string.IsNullOrEmpty(str) && str.Length != 0 && str[0] == '[' && str[^1] == ']' ? str : $"[{str}]";

        public static StringBuilder TrimIfLastCharacterIs(this StringBuilder sb, char character)
        {
            if (sb == null || sb.Length == 0)
                return sb;

            int i = sb.Length - 1;
            while (i > 0 && char.IsWhiteSpace(sb[i]))
                i--;

            if (i == 0 || sb[i] != character)
                return sb;

            return sb.TrimEnd();
        }

        public static string GetStringAfter(this string str, string what) => str.LastIndexOf(what) switch
        {
            var index when index != -1 => str[(index + 1)..],
            _ => str
        };

        public static string GetEscapedString(this string str, bool includeEdges = true) => includeEdges
            ? str.Replace("'", "''")
            : str.Replace("'", "''", 1, str.Length - 1);

        public static string Replace(this string str, string oldValue, string newValue, int fromIndex, int toIndex)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if (i > fromIndex && i + oldValue.Length < toIndex &&
                    str.Substring(i, oldValue.Length) is { } s &&
                    s == oldValue)
                {
                    builder.Append(newValue);
                    i += oldValue.Length - 1;
                }
                else builder.Append(str[i]);
            }

            return builder.ToString();
        }

        public static string RemoveUnnecessaryNewlines(this string str) =>
            string.Join(Environment.NewLine, str.Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s) && s != Environment.NewLine));

        public static IEnumerable<ITuple> ToTuples<T>(this IEnumerable<T> source) =>
            source.Select(s => s as ITuple).Where(s => s != null).ToArray();

        public static int GetAge(this DateTime birthDate, DateTime? deathDate = null)
        {
            var end = deathDate ?? DateTime.Now;
            int age = end.Year - birthDate.Year;
            if (birthDate > end.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Returns string After prevoiusString from text
        /// </summary>
        /// <param name="previousString"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetAfterString(this string text, string previousString) =>
            text.IndexOf(previousString) switch
            {
                var index and >= 0 => text.Substring(index + previousString.Length),
                _ => string.Empty
            };

        /// <summary>
        /// Returns string Before nextString from text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="nextString"></param>
        /// <returns></returns>
        public static string GetBeforeString(this string text, string nextString) =>
            text.IndexOf(nextString) switch
            {
                var index and >= 0 => text.Substring(0, index),
                _ => string.Empty
            };

        /// <summary>
        /// Returns string Before nextString and After previousString
        /// </summary>
        /// <param name="text"></param>
        /// <param name="previousString"></param>
        /// <param name="nextString"></param>
        /// <returns></returns>
        public static string GetBetweenString(this string text, string previousString, string nextString)
        {
            int startIndex = text.IndexOf(previousString);
            if (startIndex < 0 || startIndex == text.Length - 1)
                return string.Empty;

            startIndex += previousString.Length;

            int endIndex = text.IndexOf(nextString, startIndex);
            if (endIndex < 0)
                return string.Empty;

            var result = text.Substring(startIndex, endIndex - startIndex);
            return result;
        }

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static object GetProperty(this ExpandoObject expando, string propertyName) =>
            expando is IDictionary<string, object> dic ? dic[propertyName] : throw new Exception();

        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute =
                type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            bool nameContainsAnonymousType = type.FullName != null && type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsGenericType(this Type type, Type interfaceType) =>
            type.UnderlyingSystemType.Name == interfaceType.Name || type
                .GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

        public static bool IsGenericType(this PropertyInfo property, Type interfaceType) =>
            property.PropertyType.UnderlyingSystemType.Name == interfaceType.Name
            || property.PropertyType
                .GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

        public static bool HasParameterlessConstructor<T>() =>
            typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null) != null;

        public static int DateDiff(string interval, DateTime starting, DateTime ending) =>
           throw new NotImplementedException();

        //public static bool ContainsRange<T>(this IEnumerable<T> source, IEnumerable<T> range) =>
        //    throw new NotImplementedException();

        //public static bool ContainsRange<T, TResult>(this IEnumerable<T> source, Expression<Func<T, TResult>> selector, IEnumerable<TResult> range) =>
        //    throw new NotImplementedException();

        public static string GetUniqueNameByFirstChar(this string name, HashSet<string> alreadyExistedNames)
        {
            var ch = char.ToLower(name.First(char.IsLetter));
            int i = 1;
            string str = $"{ch}{i}";
            while (!alreadyExistedNames.Add(str))
                str = $"{ch}{++i}";
            return str;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T val)
        {
            foreach (var s in source)
                yield return s;

            yield return val;
        }

        public static IEnumerable<T> Concat<T>(this T val, IEnumerable<T> source)
        {
            yield return val;

            foreach (var s in source)
                yield return s;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        public static IEnumerable<T> AsEnumerable<T>(this T val)
        {
            yield return val;
        }

        public static bool IsDefault(this object argument)
        {
            var argumentType = argument.GetType();
            if (argumentType.IsValueType)
            {
                object obj = Activator.CreateInstance(argument.GetType());
                return obj.Equals(argument);
            }

            return false;
        }

        public static Array CreateArray(this IEnumerable<object> source, Type type) =>
            source.ToArray().CreateArray(type);

        public static Array CreateArray(this object[] arr, Type type)
        {
            var result = Array.CreateInstance(type, arr.Length);
            for (int i = 0; i < arr.Length; i++)
                result.SetValue(arr[i], i);
            return result;
        }

        public static IEnumerable<(TResult1, TResult2)> SelectManyWithItem<T, TResult1, TResult2>(
            this IEnumerable<T> source, Func<T, IEnumerable<TResult1>> func, Func<T, TResult2> itemSelector) =>
            from s in source
            from result1 in func(s)
            select (result1, itemSelector(s));

        public static bool All<T>(this IEnumerable<T> source, Func<T, int, bool> func)
        {
            int i = 0;
            foreach (var s in source)
            {
                if (!func(s, i))
                    return false;
                i++;
            }

            return true;
        }

        public static bool HasSameProperties(this object t, object other) => t.GetType().GetProperties().All(prop =>
            prop.PropertyType.Namespace != "System" ||
            (prop.GetValue(t) == null && prop.GetValue(other) == null) ||
            (prop.GetValue(t) is { } firstVal && prop.GetValue(other) is { } secondVal &&
             (firstVal == secondVal || firstVal.Equals(secondVal) ||
              (firstVal is byte[] byteArr1 && secondVal is byte[] byteArr2 && byteArr1.IsEqual(byteArr2)))
            ));

        public static bool IsEqual(this byte[] byteArr1, byte[] byteArr2) =>
            byteArr1.Length == byteArr2.Length && byteArr1.All((d, i) => d == byteArr2[i]);

        public static T AddAndReturn<T>(this List<T> list, T t)
        {
            list.Add(t);
            return t;
        }

        public static Type GetTupleType(this IEnumerable<Type> types) =>
            types.GetCreateTupleMethod()?.ReturnType ?? throw new Exception("Generic Tuple was not created");

        public static MethodInfo GetCreateTupleMethod(this IEnumerable<Type> types, Type withOneParameterAtFirst = null)
        {
            var typesArr = (withOneParameterAtFirst != null ? withOneParameterAtFirst.Concat(types) : types).ToArray();
            if (typesArr.Length <= 7)
                return typeof(Tuple).GetMethods()
                    .FirstOrDefault(method =>
                        method.Name == "Create" && method.GetParameters().Length == typesArr.Length)
                    ?.MakeGenericMethod(typesArr)
                ?? throw new Exception("Tuple Method can not be created");

            var eightTupleType = typesArr.Skip(7).GetTupleType();
            return typeof(Tuple).GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Create" && method.GetParameters().Length == 8)
                ?.MakeGenericMethod(typesArr.Take(7).Concat(eightTupleType).ToArray())
                   ?? throw new Exception("Generic Tuple was not created");
        }

        public static T GetMaxValue<T>()
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (type == typeof(string))
                return (T)(object)"ZZZZZZZZZZZ";
            if (type == typeof(Guid) || type == typeof(Guid?))
                return (T)(object)Guid.NewGuid();//does not return maxValue

            return (T)type.GetField("MaxValue")?.GetValue(null);
        }

        public static T GetMinValue<T>()
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (type == typeof(string))
                return (T)(object)string.Empty;
            if (type == typeof(Guid) || type == typeof(Guid?))
                return (T)(object)Guid.Empty;

            return (T)type.GetField("MinValue")?.GetValue(null);
        }
    }
}
