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
    public class UserFeedbackService : IUserFeedbackService
    {
        private const string UserFeedbackEndpoint = "api/v2/UserFeedback";
        private readonly IHttpClientProvider _httpClientProvider;

        public UserFeedbackService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<UserFeedback> Create(UserFeedback userFeedback, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(UserFeedbackEndpoint, new StringContent(JsonConvert.SerializeObject(userFeedback), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserFeedback>(content);
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
