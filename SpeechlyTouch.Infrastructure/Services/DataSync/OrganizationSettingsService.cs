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
    public class OrganizationSettingsService : IOrganizationSettingsService
    {
        private const string OrganizationSettingsEndpoint = "api/v2/OrganizationSettings";
        private const string OrganizationTagsEndpoint = "api/v2/OrganizationTags/List";

        private readonly IHttpClientProvider _httpClientProvider;
        public OrganizationSettingsService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<OrganizationSettings> GetUserOrganizationSettings(int organizationId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{OrganizationSettingsEndpoint}/{organizationId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<OrganizationSettings>(content);
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<OrganizationTag>> GetUserOrganizationTags(int organizationId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{OrganizationTagsEndpoint}/{organizationId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<OrganizationTag>>(content);
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
