using Hydra.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Services
{

    public enum HttpRequestType
    {
        Get,
        Post,
        Put,
        Delete
    }

    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResponseObject> SendAsync(string url,
                                                    HttpRequestType method,
                                                    object? body = null,
                                                    Dictionary<string, string>? headers = null,
                                                    Guid id = default,
                                                    string actionName = "")
        {
            var responseObj = new ResponseObject()
                .SetActionName(actionName)
                .SetId(id);

            try
            {
                using var requestMessage = new HttpRequestMessage();

                // Method set et
                requestMessage.Method = method switch
                {
                    HttpRequestType.Get => HttpMethod.Get,
                    HttpRequestType.Post => HttpMethod.Post,
                    HttpRequestType.Put => HttpMethod.Put,
                    HttpRequestType.Delete => HttpMethod.Delete,
                    _ => throw new ArgumentOutOfRangeException(nameof(method), "Desteklenmeyen HTTP metodu")
                };

                requestMessage.RequestUri = new Uri(url);

                // Header varsa ekle
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }

                // Body varsa ve post/put ise json olarak set et
                if (body != null && (method == HttpRequestType.Post || method == HttpRequestType.Put))
                {
                    requestMessage.Content = JsonContent.Create(body);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    responseObj.SetSuccess(false)
                               .AddExtraMessage(new ResponseObjectMessage("Hata", $"HTTP {(int)response.StatusCode}", false));
                    return responseObj;
                }

                var result = response.Content.Headers.ContentLength > 0
                    ? await response.Content.ReadFromJsonAsync<object>()
                    : null;

                responseObj.SetSuccess(true)
                           .SetData(result)
                           .UseDefaultMessages();

                return responseObj;
            }
            catch (Exception ex)
            {
                responseObj.SetSuccess(false)
                           .AddExtraMessage(new ResponseObjectMessage("Hata", ex.Message, false));

                return responseObj;
            }
        }

    }
}
