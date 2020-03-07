using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace F4ST.Common.Extensions
{
    /// <summary>
    /// Extension for objects
    /// </summary>
    public static class ObjectExt
    {
        /// <summary>
        /// ForEach for IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">IEnumerable list</param>
        /// <param name="action">Action for raise</param>
        public static void ForEach<T>(this IEnumerable<T> source,
            Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        /// <summary>
        /// Convert object to dictionary
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="obj">Object to convert</param>
        /// <returns>Dictionary of properties</returns>
        public static Dictionary<string, TValue> ToDictionary<TValue>(this object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public static bool IsList(this Type o)
        {
            return o.IsGenericType && o.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static bool IsIEnumerable(this Type o)
        {
            return o.IsGenericType && 
                   (o.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                    o.GetInterface("IEnumerable") != null);
        }

        public static Array ConvertToArray<T>(this IEnumerable<T> obj, Type destType)
        {
            var sourceObj = obj as T[] ?? obj.ToArray();
            var items = Array.CreateInstance(destType, sourceObj.Count());
            for (var i = 0; i < sourceObj.Count(); i++)
            {
                items.SetValue(Convert.ChangeType(sourceObj.ElementAt(i), destType), i);
            }

            return items;
        }

        public static IList ConvertTo<T>(this IEnumerable<T> obj, Type destType)
        {
            var t = typeof(List<>).MakeGenericType(destType);
            var res = (IList)Activator.CreateInstance(t);

            foreach (var item in obj)
            {
                res.Add(Convert.ChangeType(item, destType));
            }

            return res;
        }

        public static object ConvertToEnum(this string obj, Type destType)
        {
            return Enum.Parse(destType, obj);
        }

        public static object ConvertToType<T>(this object obj, Type destType)
        {
            object res = null;
            if (destType.IsGenericType &&
                destType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                res = ConvertToType<T>(obj, destType.GenericTypeArguments[0]);
            }

            if (destType.IsEnum)
            {
                var v = obj;
                if (obj.GetType().IsArray)
                    v = ((IEnumerable<T>)obj).FirstOrDefault();

                res = ConvertToEnum(v?.ToString() ?? "0", destType);
            }

            if (destType.IsList() || destType.IsIEnumerable())
            {
                res = ConvertTo(obj as IEnumerable<T>, destType.GenericTypeArguments[0]);
            }

            if (destType.IsArray)
            {
                res = ConvertToArray(obj as IEnumerable<T>, destType.GetElementType());
            }

            if (res == null)
            {
                var v = obj;
                if (obj.GetType().IsArray)
                    v = ((IEnumerable<T>)obj).FirstOrDefault();

                res = Convert.ChangeType(v, destType);
            }

            return res;
        }

        public static void SetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda,
            TValue value)
        {
            if (!(memberLamda.Body is MemberExpression memberSelectorExpression))
                return;

            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
            {
                var str = memberSelectorExpression.ToString();
                SetPropertyValue(target, str.Substring(str.IndexOf('.') + 1), value);

                //property.SetValue(target, value, null);
            }
        }

        public static object SetPropertyValue(object src, string propName, object value)
        {
            if (src == null) //throw new ArgumentException("Value cannot be null.", "src");
                return value;

            if (propName == null) //throw new ArgumentException("Value cannot be null.", "propName");
                return value;

            if (propName.Contains(".")) //complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return SetPropertyValue(GetPropertyValue(src, temp[0]), temp[1], value);
            }

            var prop = src.GetType().GetProperty(propName);
            if (prop != null)
            {
                prop.SetValue(src, value);
            }

            return value;
        }

        public static TValue GetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda)
        {
            if (!(memberLamda.Body is MemberExpression memberSelectorExpression))
                return default;

            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
            {
                var str = memberSelectorExpression.ToString();
                var value = GetPropertyValue(target, str.Substring(str.IndexOf('.') + 1));

                return (TValue)value;
            }

            return default;
        }

        public static TValue GetPropertyValue<T, TValue>(this T target, string propName)
        {
            if (string.IsNullOrWhiteSpace(propName))
                return default;

            var value = GetPropertyValue(target, propName);

            return (TValue)value;
        }

        public static object GetPropertyValue(object src, string propName)
        {
            if (src == null) //throw new ArgumentException("Value cannot be null.", "src");
                return null;

            if (propName == null) //throw new ArgumentException("Value cannot be null.", "propName");
                return null;

            if (propName.Contains(".")) //complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }

            var prop = src.GetType().GetProperty(propName);
            return prop != null ? prop.GetValue(src, null) : null;
        }

        public static bool IsValid<T>(this T model, out Dictionary<string, string> results)
            where T : class, new()
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(model, context, validationResults, true);

            results = validationResults.ToDictionary(v => v.MemberNames.FirstOrDefault(), v => v.ErrorMessage);
            return isValid;
        }

        public static bool IsValidList<T>(this IEnumerable<T> models, out Dictionary<string, string> results)
            where T : class, new()
        {
            results = null;
            var isValid = true;
            foreach (var model in models)
            {
                isValid = model.IsValid(out results);

                if (!isValid)
                    break;
            }

            return isValid;
        }

        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerable source)
        {
            // Note: firstItem parameter is unused and is just for resolving type of T
            foreach (var item in source)
            {
                yield return (T)item;
            }
        }

        public static string PropertyName<T>(this Expression<Func<T>> expression)
        {
            var propertyInfo = (expression.Body as MemberExpression)?.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }
            return propertyInfo.Name;
        }

        // code adjusted to prevent horizontal overflow
        public static string PropertyName<T, TProperty>(this Expression<Func<T, TProperty>> exp)
        {
            if (!TryFindMemberExpression(exp.Body, out var memberExp))
                return string.Empty;

            var memberNames = new Stack<string>();
            do
            {
                memberNames.Push(memberExp.Member.Name);
            }
            while (TryFindMemberExpression(memberExp.Expression, out memberExp));

            return string.Join(".", memberNames.ToArray());
        }

        // code adjusted to prevent horizontal overflow
        private static bool TryFindMemberExpression(Expression exp, out MemberExpression memberExp)
        {
            memberExp = exp as MemberExpression;
            if (memberExp != null)
            {
                // heyo! that was easy enough
                return true;
            }

            // if the compiler created an automatic conversion,
            // it'll look something like...
            // obj => Convert(obj.Property) [e.g., int -> object]
            // OR:
            // obj => ConvertChecked(obj.Property) [e.g., int -> long]
            // ...which are the cases checked in IsConversion
            if (IsConversion(exp) && exp is UnaryExpression expression)
            {
                memberExp = expression.Operand as MemberExpression;
                if (memberExp != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsConversion(Expression exp)
        {
            return (
                exp.NodeType == ExpressionType.Convert ||
                exp.NodeType == ExpressionType.ConvertChecked
            );
        }

        public static Expression<Func<TModel, TToProperty>> Cast<TModel, TFromProperty, TToProperty>(this Expression<Func<TModel, TFromProperty>> expression)
        {
            Expression converted = Expression.Convert(expression.Body, typeof(TToProperty));

            return Expression.Lambda<Func<TModel, TToProperty>>(converted, expression.Parameters);
        }

        public static Expression GetExpression<T>(this Expression<Func<T, bool>> exp)
        {
            return exp;
        }

        public static string ToBase64(this object obj)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T ToObject<T>(this string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return (T)new BinaryFormatter().Deserialize(ms);
            }
        }
    }
}