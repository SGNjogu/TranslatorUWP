using Newtonsoft.Json;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class CustomTagService : ICustomTagService
    {
        private const string CustomTagListEndpoint = "api/v2/Organizations";

        private readonly IHttpClientProvider _httpClientProvider;
        public CustomTagService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<List<CustomTag>> GetOrganizationCustomTags(int organizationId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{CustomTagListEndpoint}/{organizationId}/CustomTags");
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<CustomTag>>(content);
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
