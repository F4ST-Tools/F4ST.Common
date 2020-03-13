using System;
using System.Security.Cryptography;
using System.Text;

namespace F4ST.Common.Extensions
{
    public static class StringExt
    {
        /// <summary>
        /// Convert string to Md5
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <returns>Md5 string</returns>
        public static string ToMd5(this string text)
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(text));

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Convert string to Base64
        /// </summary>
        /// <param name="plainText">String to convert</param>
        /// <returns>Base64 string</returns>
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        
        /// <summary>
        /// Convert Base64 to string
        /// </summary>
        /// <param name="base64EncodedData">Base64 strign</param>
        /// <returns>String</returns>
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Mask part of string to special character
        /// </summary>
        /// <param name="source">Original text</param>
        /// <param name="maskValue">Mask character</param>
        /// <param name="start">Start position</param>
        /// <param name="count">Count of character to overwrite by mask</param>
        /// <returns>Masked string</returns>
        public static string Masked(this string source, char maskValue, int start, int count)
        {
            var firstPart = source.Substring(0, start);
            var lastPart = source.Substring(start + count);
            var middlePart = new string(maskValue, count);

            return firstPart + middlePart + lastPart;
        }

        /// <summary>
        /// Mask string to special character
        /// </summary>
        /// <param name="source">Original string</param>
        /// <param name="maskValue">Mask character</param>
        /// <returns>Masked string</returns>
        public static string Masked(this string source, char maskValue = '*')
        {
            return source.Masked(maskValue, 1, source.Length - 2);
        }

        /// <summary>
        /// If string is null, set string.Empty
        /// </summary>
        /// <param name="value">Original string</param>
        /// <returns>String</returns>
        public static string SetEmptyIfNull(this string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value;

        /// <summary>
        /// Set default value if string is null or empty
        /// </summary>
        /// <param name="value">Original string</param>
        /// <param name="defaultValue">Default value to set</param>
        /// <returns>String</returns>
        public static string SetDefaultIfEmpty(this string value, string defaultValue = "-") =>
            string.IsNullOrWhiteSpace(value) ? defaultValue : value;

        /// <summary>
        /// Safe substring for handle exception
        /// </summary>
        /// <param name="text">String to sub string</param>
        /// <param name="startIndex">Start position</param>
        /// <param name="length">Length of character</param>
        /// <returns>Sub stringed text</returns>
        public static string SafeSubString(this string text, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (length > text.Length)
                length = text.Length - startIndex;

            if (startIndex > length)
                return text;

            return text.Substring(startIndex, length);
        }

        /// <summary>
        /// Sub string by start index and end index
        /// </summary>
        /// <param name="value">Original string</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        /// <returns>Sub stringed text</returns>
        public static string SubStr(this string value, int startIndex, int endIndex)=>
            value.Substring(startIndex, (endIndex - startIndex + 1));

    }
}