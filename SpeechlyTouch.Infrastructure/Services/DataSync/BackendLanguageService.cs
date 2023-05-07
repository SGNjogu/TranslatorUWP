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
    public class BackendLanguageService : IBackendLanguageService
    {
        private const string BackendLanguagesEndpoint = "api/v2/Users";
        private readonly IHttpClientProvider _httpClientProvider;

        public BackendLanguageService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        /// <summary>
        /// Method to get approved Languages for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<BackendLanguage>> GetUserLanguages(int? userId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{BackendLanguagesEndpoint}/{userId}/Languages"); ;

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<BackendLanguage>>(content);
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
