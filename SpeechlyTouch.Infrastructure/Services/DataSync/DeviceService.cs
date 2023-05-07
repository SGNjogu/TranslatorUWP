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
    public class DeviceService : IDeviceService
    {
        private const string DevicesEndpoint = "api/v2/Devices";
        private const string SessionDevicesEndpoint = "api/v2/Devices/List";
        private readonly IHttpClientProvider _httpClientProvider;

        public DeviceService(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        public async Task<Device> Create(Device device, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);
                HttpResponseMessage response = await client.PostAsync(DevicesEndpoint, new StringContent(JsonConvert.SerializeObject(device), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<Device>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to get devices for a specific sessions
        /// </summary>
        /// <param name="sessionId">Takes in the sessionId</param>
        /// <returns>Returns a list of devices</returns>
        public async Task<IEnumerable<Device>> GetDevicesForSession(int sessionId, string token)
        {
            try
            {
                var client = _httpClientProvider.GetBackendApiClient(token);

                HttpResponseMessage response = await client.GetAsync($"{SessionDevicesEndpoint}/{sessionId}"); ;

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Device>>(content);
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
