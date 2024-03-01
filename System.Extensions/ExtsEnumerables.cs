using System.Collections.Generic;
using System.Linq;

namespace System.Exts
{
    public static partial class Extensions
    {
        public static Array CreateArray(this IEnumerable<object> source, Type type) =>
            source.ToArray().CreateArray(type);

        public static Array CreateArray(this object[] arr, Type type)
        {
            var result = Array.CreateInstance(type, arr.Length);
            for (int i = 0; i < arr.Length; i++)
                result.SetValue(arr[i], i);
            return result;
        }

        public static bool IsEqual(this byte[] byteArr1, byte[] byteArr2) =>
            byteArr1.Length == byteArr2.Length && byteArr1.All((d, i) => d == byteArr2[i]);

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

        public static T AddAndReturn<T>(this List<T> list, T t)
        {
            list.Add(t);
            return t;
        }

        //public static bool ContainsRange<T>(this IEnumerable<T> source, IEnumerable<T> range) =>
        //    throw new NotImplementedException();

        //public static bool ContainsRange<T, TResult>(this IEnumerable<T> source, Expression<Func<T, TResult>> selector, IEnumerable<TResult> range) =>
        //    throw new NotImplementedException();
    }
}
