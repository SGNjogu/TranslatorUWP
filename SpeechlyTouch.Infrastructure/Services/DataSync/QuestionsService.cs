using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Enum;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UserQuestion = SpeechlyTouch.Core.Domain.UserQuestion;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class QuestionsService : IQuestionsService
    {
        private const string UserQuestionsEndpoint = "/api/v2/Users";
        private const string UserQuestions = "userQuestions";
        private const string OrganizationQuestions = "organizationQuestions";


        private readonly IHttpClientProvider _httpClientProvider;

        public QuestionsService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<List<UserQuestion>> GetUserQuestions(int userId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{UserQuestionsEndpoint}/{userId}/Questions");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseObj = JObject.Parse(content);
                    var userQuestions = responseObj[UserQuestions].ToObject<List<UserQuestion>>();
                    foreach (var userQuestion in userQuestions)
                    {
                        userQuestion.QuestionType = (int)QuestionType.UserQuestion;

                    }
                    var organizationQuestions = responseObj[OrganizationQuestions].ToObject<List<UserQuestion>>();
                    foreach (var userQuestion in organizationQuestions)
                    {
                        userQuestion.QuestionType = (int)QuestionType.OrganizationQuestion;

                    }
                    userQuestions.AddRange(organizationQuestions);
                    return userQuestions;
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserQuestion>CreateUserQuestion(int userId,UserQuestion userQuestion, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync($"{UserQuestionsEndpoint}/{userId}/Questions", new StringContent(JsonConvert.SerializeObject(userQuestion), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserQuestion>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> DeleteUserQuestion(int userId, UserQuestionsDTO userQuestionDTO, string token)
        {
            
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                var convertedJson = JsonConvert.SerializeObject(userQuestionDTO);
                var sendObject = JObject.Parse(convertedJson);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri($"{client.BaseAddress}{UserQuestionsEndpoint}/{userId}/Questions"),
                    Content = new StringContent(sendObject.ToString(), Encoding.UTF8, "application/json")
                };

                var responseMessage = await client.SendAsync(request);
                if (responseMessage.IsSuccessStatusCode == true)
                {
                    return true;
                }
                else return false;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }

}
