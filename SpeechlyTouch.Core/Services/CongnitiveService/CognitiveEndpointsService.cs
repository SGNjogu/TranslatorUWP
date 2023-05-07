using Newtonsoft.Json;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeechlyTouch.Core.Services.CongnitiveService
{
    public class CognitiveEndpointsService : ICognitiveEndpointsService
    {
        private const string CognitiveServiceEndpointBase = "api/v2/CognitiveServices";
        private readonly IHttpClientProvider _httpClientProvider;

        public CognitiveEndpointsService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        /// <summary>
        /// Method to Allocate Cognitive Services Endpoint Id
        /// </summary>
        /// <returns>Return the Endpoint allocated</returns>
        public async Task<CognitiveServiceEndpoint> AllocateCognitiveEndpointId(string token)
        {
            try
            {
                HttpClient client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{CognitiveServiceEndpointBase}/Allocate");

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var endpoint = JsonConvert.DeserializeObject<CognitiveServiceEndpoint>(content);

                    return endpoint;
                }

                throw new Exception(response.Content.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Allocate Cognitive Services Endpoint Id
        /// </summary>
        /// <returns>Return the Endpoint allocated</returns>
        public async Task<bool> DeallocateCognitiveEndpointId(string endpointId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{CognitiveServiceEndpointBase}/Deallocate/{endpointId}");

                var content = await response.Content.ReadAsStringAsync();

                bool endpointDeallocated = false;

                if (response.IsSuccessStatusCode)
                {
                    endpointDeallocated = JsonConvert.DeserializeObject<bool>(content);

                    return endpointDeallocated;
                }

                throw new Exception(response.Content.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
