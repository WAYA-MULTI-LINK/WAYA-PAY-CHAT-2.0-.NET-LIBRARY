using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WayaPay.Net.SDK
{
    public static class HttpConnection
    {
        public static HttpClient CreateClient(bool isProduction)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var baseURL = isProduction ? BaseConstants.WayPayBaseEndPointProduction : BaseConstants.WayPayBaseEndPointStaging;
            var client = new HttpClient()
            {
                BaseAddress = new Uri(baseURL)
            };

            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(BaseConstants.ContentTypeHeaderJson));

            client.DefaultRequestHeaders.Add("cache-control", "no-cache");

            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BaseConstants.AuthorizationHeaderType, secretKey);

            return client;
        }
    }
}
