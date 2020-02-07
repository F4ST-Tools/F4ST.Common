using System;
using System.Collections.Generic;
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
    }
}