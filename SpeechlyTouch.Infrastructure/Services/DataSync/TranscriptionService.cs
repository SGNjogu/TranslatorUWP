using Newtonsoft.Json;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class TranscriptionService : ITranscriptionService
    {
        private const string TranscriptionsEndpoint = "api/v2/Transcriptions";
        private const string SessionTranscriptionsEndpoint = "api/v2/Transcriptions/List";
        private const string BulkTranscriptionsEndpoint = "api/v2/CreateBulkTranscriptions";
        private readonly IHttpClientProvider _httpClientProvider;

        public TranscriptionService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<Transcription> Create(Transcription transcription, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(TranscriptionsEndpoint, new StringContent(JsonConvert.SerializeObject(transcription), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<Transcription>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Transcription>> CreateBulk(BulkTranscriptions transcriptions, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(BulkTranscriptionsEndpoint, new StringContent(JsonConvert.SerializeObject(transcriptions), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<Transcription>>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Methdo to get transcriptions for a specific session
        /// </summary>
        /// <param name="sessionId">Takes in the sessionId</param>
        /// <returns>Returns a list of transcriptions</returns>
        public async Task<IEnumerable<Transcription>> GetTranscriptionsForSession(int sessionId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{SessionTranscriptionsEndpoint}/{sessionId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Transcription>>(content);
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
