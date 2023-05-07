using Newtonsoft.Json;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class UsageTrackingService : IUsageTrackingService
    {
        private const string UsageTrackingEndpoint = "api/v2/PackageOrderUsageEndpoints/GetUsage";
        private const string UserUsageLimitEndpoint = "api/v2/Users/";


        private readonly IHttpClientProvider _httpClientProvider;

        public UsageTrackingService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<OrganizationUsageLimit> GetOrganizationUsageLimit(int organizationId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{UsageTrackingEndpoint}/{organizationId}");
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<OrganizationUsageLimit>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserUsageLimit> GetUserUsageLimit(int userId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{UserUsageLimitEndpoint}/{userId}/Limits");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserUsageLimit>(content);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && content.Contains("does not have limits"))
                {
                    return null;
                }
                else
                {
                    throw new Exception(content);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
