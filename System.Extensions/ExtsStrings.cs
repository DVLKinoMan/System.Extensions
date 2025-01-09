using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Exts
{
    public static partial class Extensions
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
                _ => value == null ? throw new ArgumentNullException(nameof(value)) : value.ToString()!
            };

        public static SqlDbType DefaultMap<TValue>(TValue value) =>
            DefaultMap(typeof(TValue) == typeof(object)
                ? (value == null ? throw new ArgumentNullException(nameof(value)) : value.GetType())
                : typeof(TValue));

        internal static Dictionary<Type, SqlDbType> SqlDbTypes = new()
        {
            { typeof(int), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(bool), SqlDbType.Bit },
            { typeof(DateTime), SqlDbType.DateTime },
            { typeof(decimal), SqlDbType.Decimal },
            { typeof(double), SqlDbType.Float },
            { typeof(float), SqlDbType.Float },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(string), SqlDbType.NVarChar },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(byte[]), SqlDbType.Binary }
        };

        public static SqlDbType DefaultMap(Type type) =>
            SqlDbTypes.TryGetValue(type, out SqlDbType value)
                ? value
                : type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    ? DefaultMap(Nullable.GetUnderlyingType(type)!)
                    : throw new NotImplementedException($"{type.Name} is not implemented");

        public static StringBuilder TrimEnd(this StringBuilder sb, bool leftOneWhiteSpace = false)
        {
            if (sb.Length == 0)
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
            if (sb.Length == 0)
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
            string.Join(Environment.NewLine,
                str.Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s) && s != Environment.NewLine));

        /// <summary>
        /// Returns string After prevoiusString from text
        /// </summary>
        /// <param name="previousString"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetAfterString(this string text, string previousString) =>
            text.IndexOf(previousString) switch
            {
                var index and >= 0 => text[(index + previousString.Length)..],
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
                var index and >= 0 => text[..index],
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

            var result = text[startIndex..endIndex];
            return result;
        }

        public static string GetUniqueNameByFirstChar(this string name, HashSet<string> alreadyExistedNames)
        {
            var ch = char.ToLower(name.First(char.IsLetter));
            int i = 1;
            string str = $"{ch}{i}";
            while (!alreadyExistedNames.Add(str))
                str = $"{ch}{++i}";
            return str;
        }
    }
}