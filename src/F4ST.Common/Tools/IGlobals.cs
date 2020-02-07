using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace F4ST.Common.Tools
{
    public interface IGlobals
    {
        Task<string> RequestUrl(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000,
            ContentType contentType = ContentType.Json);

        Task<T> RequestUrl<T>(string baseUrl, string path, Method method, object postData = null,
            Dictionary<string, string> headers = null, int timeout = 10_000,
            ContentType contentType = ContentType.Json);
    }
}