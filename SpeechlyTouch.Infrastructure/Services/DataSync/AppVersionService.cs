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
    public class AppVersionService : IAppVersionService
    {
        private const string AppVersionsEndpoint = "api/v2/AppVersions/List";
        private readonly IHttpClientProvider _httpClientProvider;

        public AppVersionService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<IEnumerable<AppVersion>> GetAppVersions(int appType, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{AppVersionsEndpoint}/{appType}"); ;

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<AppVersion>>(content);
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
