using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace F4St.Common.Tools
{
    public static class Global
    {
        public static async Task<string> RequestUrl(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000, ContentType contentType = ContentType.Json,
            CancellationToken cancellationToken = default)
        {
            var retValue = string.Empty;
            try
            {
                if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
                    baseUrl = $"http://{baseUrl}";
                var client = new RestClient(baseUrl)
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

                if (response != null /*&& response?.StatusCode == HttpStatusCode.OK*/)
                    retValue = response.Content;

            }
            catch
            {
                //
            }

            return retValue;
        }

        public static async Task<T> RequestUrl<T>(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000, ContentType contentType = ContentType.Json,
            CancellationToken cancellationToken = default)
        {
            var res = await RequestUrl(baseUrl, path, method, postData, headers, timeout, contentType, cancellationToken);
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