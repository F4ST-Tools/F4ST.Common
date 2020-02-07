using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
    
namespace F4ST.Common.Extensions
{
    public static class EnumExt
    {
        public static string GetEnumName<T>(this T v)
            where T : struct, IConvertible
        {
            var enumType = typeof(T);

            if (!enumType.IsEnum) return "";

            var field = v.GetType().GetField(v.ToString());

            if (field == null)
                return v.ToString();

            var attributes =
                (DisplayAttribute) field.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            return attributes != null ? attributes.GetName() : v.ToString();
        }

        public static Dictionary<int, string> GetEnumList<T>(this T v)
            where T:struct, IConvertible
        {
            var items = new Dictionary<int, string>();
            var enumType = typeof(T);

            if (!enumType.IsEnum) return items;

            var source = Enum.GetValues(typeof(T));
            var displayAttributeType = typeof(DisplayAttribute);

            foreach (var value in source)
            {
                var field = value.GetType().GetField(value.ToString());

                if (field == null) continue;

                var attributes =
                    (DisplayAttribute) field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                items.Add((int) value, attributes != null ? attributes.GetName() : value.ToString());
            }

            return items;
        }

        public static Dictionary<K, string> GetEnumList<T, K>(this T v)
            where T : struct, IConvertible
        {
            var items = new Dictionary<K, string>();
            var enumType = typeof(T);

            if (!enumType.IsEnum) return items;

            var source = Enum.GetValues(typeof(T));
            var displayAttributeType = typeof(DisplayAttribute);

            foreach (var value in source)
            {
                var field = value.GetType().GetField(value.ToString());

                if (field == null) continue;

                var attributes =
                    (DisplayAttribute) field.GetCustomAttributes(displayAttributeType, false).FirstOrDefault();
                items.Add((K) value, attributes != null ? attributes.GetName() : value.ToString());
            }

            return items;
        }

        public static T GetEnumByName<T>(string name, T defaultValue)
        {
            T item;
            try
            {
                item = (T) Enum.Parse(typeof(T),
                    Enum.GetNames(typeof(T))
                        .First(e => String.Equals(e, name, StringComparison.CurrentCultureIgnoreCase)));
            }
            catch (Exception)
            {
                item = defaultValue;
            }

            return item;
        }

        public static T GetEnumFromInt<T>(this T v, int number)
            where T : struct, IConvertible
        {
            if (Enum.IsDefined(typeof(T), number))
            {
                return (T) Enum.ToObject(typeof(T), number);
            }
            else
            {
                return default(T);
            }
        }

        public static T GetEnumByIndex<T>(this T v, int index, T defaultValue)
            where T : struct, IConvertible
        {
            if (index < 0)
                return defaultValue;

            var items = Enum.GetNames(typeof(T));
            if (index > items.Length)
                return defaultValue;

            return (T) Enum.Parse(typeof(T),
                Enum.GetNames(typeof(T)).GetValue(index).ToString());
        }
    }
}