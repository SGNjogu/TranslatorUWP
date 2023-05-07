using Newtonsoft.Json;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class PlaybackUsageService : IPlaybackUsageService
    {
        private const string PlaybackUsageTrackingEndpoint = "api/v2/UsageTraking/Playback/Create";
        private readonly IHttpClientProvider _httpClientProvider;

        public PlaybackUsageService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<PlaybackUsageTracking> Create(PlaybackUsageTracking playbackUsage, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(PlaybackUsageTrackingEndpoint, new StringContent(JsonConvert.SerializeObject(playbackUsage), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<PlaybackUsageTracking>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
