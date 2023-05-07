using System.Net.Http;

namespace SpeechlyTouch.Core.Services.HttpClientProvider
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public HttpClient SpeechlyBackendClient { get; set; }
        public HttpClientProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpClient GetBackendApiClient(string token)
        {
            if (SpeechlyBackendClient == null || SpeechlyBackendClient.DefaultRequestHeaders.Authorization == null)
            {
                SpeechlyBackendClient = new HttpClient();
#if RELEASE
                SpeechlyBackendClient.BaseAddress = new System.Uri("https://speechly-api.azurewebsites.net/");
#else
                SpeechlyBackendClient.BaseAddress = new System.Uri("https://speechly-api.azurewebsites.net/");
#endif
                SpeechlyBackendClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return SpeechlyBackendClient;
        }
    }
}
