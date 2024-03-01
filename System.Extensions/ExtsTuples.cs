using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Exts
{
    public static partial class Extensions 
    { 
        public static IEnumerable<ITuple> ToTuples<T>(this IEnumerable<T> source) =>
            source.Select(s => s as ITuple).Where(s => s != null).ToArray()!;

        public static Type GetTupleType(this IEnumerable<Type> types) =>
            types.GetCreateTupleMethod()?.ReturnType ?? throw new Exception("Generic Tuple was not created");

        public static MethodInfo GetCreateTupleMethod(this IEnumerable<Type> types, Type? withOneParameterAtFirst = null)
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

    }
}
