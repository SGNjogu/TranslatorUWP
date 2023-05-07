using System.Net.Http;

namespace SpeechlyTouch.Core.Services.HttpClientProvider
{
    public interface IHttpClientProvider
    {
        HttpClient GetBackendApiClient(string token);
    }
}