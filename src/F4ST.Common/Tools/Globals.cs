using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using F4ST.Common.Extensions;
using Newtonsoft.Json;
using RestSharp;

namespace F4St.Common.Tools
{
    public class Globals : IGlobals
    {
        private static readonly HttpClient _httpClient;

        static Globals()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> RequestUrl(string baseUrl, string path, Method method, object postData = null,
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
            catch
            {
                //
            }

            return retValue;
        }

        public async Task<T> RequestUrl<T>(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000, ContentType contentType = ContentType.Json/*,
            CancellationToken cancellationToken = default*/)
        {
            var res = await RequestUrl(baseUrl, path, method, postData, headers, timeout, contentType);//, cancellationToken);
            return string.IsNullOrWhiteSpace(res)
                ? default
                : JsonConvert.DeserializeObject<T>(res);
        }
    }

    public enum ContentType
    {
        Json,
        XWwwFormUrlencoded
    }
}