using Minimal.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Reflection
{
    public static class ReflectionAccessor
    {
        public static bool IsTaggedWith(this Type type, Type taggingInterface) => type.GetInterfaces().Any(i => i == taggingInterface);

        public static List<PropertyInfo> GetQuickFilterProps(Type type)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return props.Where(p => IsQuickFilterProp(p)).ToList();
        }

        public static bool IsQuickFilterProp(this PropertyInfo propInfo)
        {
            var flags = GetFlagPropAttributes(propInfo);

            return flags.Contains(PropFlags.QuickFilter);
        }

        public static List<string> GetFlagPropAttributes(PropertyInfo propInfo)
        {
            List<string> Flags = new();

            // Get instance of the attribute.
            var propFlags = propInfo.GetCustomAttributes(typeof(PropFlagAttribute));
            foreach (PropFlagAttribute propFlag in propFlags)
                Flags.Add(propFlag.Flag);

            return Flags;
        }

        public static bool IsRefIgnore(this PropertyInfo propInfo)
        {
            var flags = GetFlagPropAttributes(propInfo);

            return flags.Contains(PropFlags.REFIGNORE);
        }

        public static Type? GetFKRefType(this PropertyInfo propInfo)
        {
            var propRef = propInfo.GetCustomAttribute(typeof(FKRefTypeAttribute));

            return (propRef as FKRefTypeAttribute)?.RefType;
        }

        public static object? ChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            }
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }

            if (conversionType == typeof(DateOnly))
            {
                return DateOnly.Parse(value.ToString()!);
            }

            return Convert.ChangeType(value, conversionType);
        }

        public static T Clone<T>(T original)
        {
            T newObject = (T)Activator.CreateInstance(original!.GetType())!;

            foreach (var originalProp in original.GetType().GetProperties())
            {
                originalProp.SetValue(newObject, originalProp.GetValue(original));
            }

            return newObject!;
        }

    }
}
