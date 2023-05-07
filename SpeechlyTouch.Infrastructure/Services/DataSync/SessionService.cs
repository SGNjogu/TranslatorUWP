using Newtonsoft.Json;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.DataSync
{
    public class SessionService : ISessionService
    {
        private const string SessionsEndpoint = "api/v2/Sessions";
        private const string UserSessionsEndpoint = "api/v2/Sessions/List";
        private const string SessionNumberEndpoint = "api/v2/Sessions/GenerateSessionNumber";
        private const string SessionEmailingEndpoint = "api/v2/Sessions/EmailSession";
        private const string SessionTagsCreateEndpoint = "api/v2/SessionTags";
        private const string SessionTagsGetEndpoint = "api/v2/SessionTags/List/sessionId:int?sessionId=";


        private readonly IHttpClientProvider _httpClientProvider;
        public SessionService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        /// <summary>
        /// Uploads a Session
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>Returns backend sessionId</returns>
        public async Task<Session> Create(Session session, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(SessionsEndpoint, new StringContent(JsonConvert.SerializeObject(session), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<Session>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to get sessions for a specific user
        /// </summary>
        /// <param name="userId">Takes in the userId</param>
        /// /// <param name="organizationId">Takes in the userId</param>
        /// <returns>Returns a list of sessions</returns>
        public async Task<IEnumerable<Session>> GetSessionsForUser(int userId, int organizationId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{UserSessionsEndpoint}?organizationId={organizationId}&userId={userId}&paginate=false");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Session>>(content);
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets a custom session number for the session
        /// </summary>
        /// <returns></returns>
        public async Task<SessionNumber> GetSessionNumber(string token)
        {
            SessionNumber sessionNumber = new SessionNumber() { ReferenceNumber = "N/A" };

            try
            {
                HttpClient client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync(SessionNumberEndpoint);
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    sessionNumber = JsonConvert.DeserializeObject<SessionNumber>(content);
                }
            }
            catch (Exception ex) { throw ex; }

            return sessionNumber;
        }

        /// <summary>
        /// Method to create an export of the session details of the selected session
        /// and email them to the specified email address
        /// </summary>
        /// <param name="sessionNumber">Session</param>
        /// /// <param name="emailAddress">Session</param>
        public async Task<bool> CreateSessionEmail(string sessionNumber, string emailAddress, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{SessionEmailingEndpoint}?SessionNumber={sessionNumber}&Email={emailAddress}");
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Creates a session custom tag
        /// </summary>
        /// <param name="sessionTag"></param>
        /// <param name="token"></param>
        /// <returns>Session Tag</returns>
        public async Task<SessionTag> CreateSessionTag(SessionTag sessionTag, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(SessionTagsCreateEndpoint, new StringContent(JsonConvert.SerializeObject(sessionTag), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    sessionTag = JsonConvert.DeserializeObject<SessionTag>(content);
                }
            }
            catch (Exception ex) { throw ex; }

            return sessionTag;
        }

        /// <summary>
        /// Gets all session tags
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="token"></param>
        /// <returns>Session Tag</returns>
        public async Task<List<SessionTag>> GetSessionTags(int sessionId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.GetAsync($"{SessionTagsGetEndpoint}{sessionId}");
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<SessionTag>>(content);
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
