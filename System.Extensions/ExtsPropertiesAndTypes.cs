using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Exts
{
    public static partial class Extensions
    {
        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute =
                type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length != 0;
            bool nameContainsAnonymousType = type.FullName != null && type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsTuple(this Type type)
            => type.Namespace == "System" 
            && type.Name.StartsWith("ValueTuple`", StringComparison.Ordinal);
        
        public static bool IsRecord(this Type type)
            => type.GetMethods().Any(m => m.Name == "<Clone>$");

        public static bool IsCustomClass(this Type type)
            => type.Namespace != "System"
                    && type.IsClass;

        public static bool IsCustomStruct(this Type type)
            => type.Namespace != "System"
                    && type.IsValueType;

        public static void AddProperty(ExpandoObject expando, string propertyName, object? propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object?>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static object? GetProperty(this ExpandoObject expando, string propertyName) =>
            expando is IDictionary<string, object?> dic ? dic[propertyName] : throw new Exception();

        public static bool HasParameterlessConstructor<T>() =>
            typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null) != null;

        public static object? CreateRecordInstance(this Type recordType, object[] values)
        {
            var properties = recordType.GetProperties();
            var constructorParams = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
                constructorParams[i] = values[i];
            return Activator.CreateInstance(recordType, constructorParams);
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

        public static bool IsDefault(this object argument)
        {
            var argumentType = argument.GetType();
            if (argumentType.IsValueType)
            {
                object? obj = Activator.CreateInstance(argument.GetType());
                return obj != null && obj.Equals(argument);
            }

            return false;
        }

        public static bool HasSameProperties(this object t, object other) => t.GetType().GetProperties().All(prop =>
            prop.PropertyType.Namespace != "System" ||
            (prop.GetValue(t) == null && prop.GetValue(other) == null) ||
            (prop.GetValue(t) is { } firstVal && prop.GetValue(other) is { } secondVal &&
             (firstVal == secondVal || firstVal.Equals(secondVal) ||
              (firstVal is byte[] byteArr1 && secondVal is byte[] byteArr2 && byteArr1.IsEqual(byteArr2)))
            ));

        public static T GetMaxValue<T>()
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (type == typeof(string))
                return (T)(object)"ZZZZZZZZZZZ";
            if (type == typeof(Guid) || type == typeof(Guid?))
                return (T)(object)Guid.NewGuid();//does not return maxValue

            return (T)type.GetField("MaxValue")?.GetValue(null)!;
        }

        public static T GetMinValue<T>()
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (type == typeof(string))
                return (T)(object)string.Empty;
            if (type == typeof(Guid) || type == typeof(Guid?))
                return (T)(object)Guid.Empty;

            return (T)type.GetField("MinValue")?.GetValue(null)!;
        }
    }
}
