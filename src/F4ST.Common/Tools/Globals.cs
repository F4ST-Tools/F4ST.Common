﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using F4ST.Common.Extensions;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using RestSharp;

namespace F4ST.Common.Tools
{
    public static class Globals
    {
        private static readonly HttpClient _httpClient;

        static Globals()
        {
            _httpClient = new HttpClient();
        }

        public static async Task<string> RequestUrl(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000, ContentType contentType = ContentType.Json/*,
            CancellationToken cancellationToken = default(CancellationToken)*/)
        {
            var retValue = string.Empty;
            try
            {
                if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
                    baseUrl = $"http://{baseUrl}";
                /*var client = new RestClient(baseUrl)
                {
                    Timeout = timeout
                };

                var request = new RestRequest(path, method);

                if (postData != null)
                {
                    request.Parameters.Clear();

                    if (contentType == ContentType.Json)
                    {
                        request.AddHeader("Accept", "application/json");
                        request.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(postData),
                            ParameterType.RequestBody);
                    }

                    if (contentType == ContentType.XWwwFormUrlencoded)
                    {
                        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                        request.AddObject(postData);
                    }
                }

                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        request.AddHeader(header.Key, header.Value);
                    }
                }

                var response = await client.ExecuteAsync(request, cancellationToken);

                if (response?.ErrorException != null)
                    return retValue;

                if (response != null)
                    retValue = response.Content;*/

                HttpMethod httpMethod;
                switch (method)
                {
                    case Method.POST:
                        httpMethod = HttpMethod.Post;
                        break;
                    case Method.GET:
                        httpMethod = HttpMethod.Get;
                        break;
                    case Method.PUT:
                        httpMethod = HttpMethod.Put;
                        break;
                    case Method.DELETE:
                        httpMethod = HttpMethod.Delete;
                        break;
                    case Method.OPTIONS:
                        httpMethod = HttpMethod.Options;
                        break;
                    /*case Method.PATCH:
                        httpMethod = HttpMethod.Patch;
                        break;*/
                    case Method.HEAD:
                        httpMethod = HttpMethod.Head;
                        break;
                    default:
                        httpMethod = HttpMethod.Get;
                        break;
                }

                var message = new HttpRequestMessage(httpMethod, new Uri(new Uri(baseUrl), path));

                if (postData != null)
                {
                    if (contentType == ContentType.Json)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8,
                            "application/json");
                    }

                    if (contentType == ContentType.XWwwFormUrlencoded)
                    {
                        //var items = HttpUtility.ParseQueryString(request.Body);
                        message.Content = new FormUrlEncodedContent(postData.ToDictionary<string>());
                    }
                }

                if (headers != null && headers.Count > 0)
                {
                    foreach (var header in headers)
                    {
                        message.Headers.Add(header.Key, header.Value);
                    }
                }

                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(timeout);

                var res = await _httpClient.SendAsync(message, cancellationToken.Token);

                if (res == null)
                    return retValue;

                res.EnsureSuccessStatusCode();

                if (!res.IsSuccessStatusCode)
                {
                    var cnt = string.Empty;
                    if (res.Content != null)
                        cnt = await res.Content?.ReadAsStringAsync();

                    return retValue;
                }

                retValue = await res.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {
                Debugger.Log(1, "F4ST.Common", e.Message);
            }

            return retValue;
        }

        public static async Task<T> RequestUrl<T>(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000, ContentType contentType = ContentType.Json/*,
            CancellationToken cancellationToken = default*/)
        {
            var res = await RequestUrl(baseUrl, path, method, postData, headers, timeout, contentType);//, cancellationToken);
            return string.IsNullOrWhiteSpace(res)
                ? default
                : JsonConvert.DeserializeObject<T>(res);
        }

        /// <summary>
        /// Get implemented object of Interface
        /// </summary>
        /// <typeparam name="T">Interface</typeparam>
        /// <returns>List of objects</returns>
        public static IEnumerable<T> GetImplementedInterfaceOf<T>()
        {
            var res = new List<T>();

            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            var items = runtimeAssemblyNames
                .Select(Assembly.Load)
                .SelectMany(a => a.ExportedTypes)
                .Where(t => typeof(T).IsAssignableFrom(t));

            foreach (var item in items)
            {
                if (item.IsInterface)
                    continue;
                res.Add((T)Activator.CreateInstance(item));
            }

            return res;
        }

        /// <summary>
        /// Get class type with Attribute
        /// </summary>
        /// <typeparam name="T">Attribute</typeparam>
        /// <returns>List of class types</returns>
        public static IEnumerable<Type> GetClassTypeWithAttribute<T>()
            where T : Attribute
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            var items = runtimeAssemblyNames
                .Select(Assembly.Load)
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.GetCustomAttributes<T>().Any());

            return items;
        }

        /// <summary>
        /// اجرای متود و منتظر بودم برای پاسخ
        /// </summary>
        /// <param name="sender">کلاس اجرا کننده</param>
        /// <param name="targetMethod">متود مربوطه</param>
        /// <param name="args">پارامترها</param>
        /// <param name="haveResult">آیا مقدار بازگشتی دارد یا خیر</param>
        /// <returns>مقدار بازگشتی متود</returns>
        public static object RunMethod(object sender, MethodInfo targetMethod, object[] args, bool haveResult)
        {
            var res = targetMethod.Invoke(sender, parameters: args);
            if (!haveResult)
                return null;

            var returnType = targetMethod.ReturnType;
            if (returnType != typeof(Task) &&
                !(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)))
            {
                return res;
            }

            var task = (Task)res;
            AsyncHelpers.RunSync(() => task);

            if (returnType == typeof(Task))
            {
                return null;
            }

            var result = task.GetType().GetProperty("Result")?.GetValue(task, null);
            return result;
        }

    }

    public enum ContentType
    {
        Json,
        XWwwFormUrlencoded
    }
}