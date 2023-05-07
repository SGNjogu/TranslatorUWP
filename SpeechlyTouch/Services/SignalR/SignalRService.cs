using Microsoft.AppCenter.Crashes;
using Microsoft.AspNetCore.SignalR.Client;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpeechlyTouch.Services.SignalR
{
    public class SignalRService : ISignalRService
    {
        private HubConnection _connection = null;
        public event Action<SignalRTranslateMessage> SignalRMessageReceived;
        public string ConnectionId { get; private set; } = string.Empty;

        private readonly IHttpClientProvider _httpClientProvider;
        private readonly ICrashlytics _crashlytics;
        private const string SignalREndpointBase = "hubs/translation";

        public SignalRService(IHttpClientProvider httpClientProvider, ICrashlytics crashlytics)
        {
            _httpClientProvider = httpClientProvider;
            _crashlytics = crashlytics;
        }
        private void Initialize(string token)
        {
            var client = _httpClientProvider.GetBackendApiClient(token);

            _connection = new HubConnectionBuilder()
                .WithUrl($"{client.BaseAddress}{SignalREndpointBase}")
                .Build();
        }

        public async Task ConnectSignalR(string token)
        {
            try
            {
                if (_connection == null)
                    Initialize(token);

                await _connection.StartAsync();
                ConnectionId = _connection.ConnectionId;
                _connection.On<SignalRTranslateMessage>("receivedmesage", (signalRTranslateMessage) => SignalRMessageReceived?.Invoke(signalRTranslateMessage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task DisconnectSignalR()
        {
            try
            {
                if(_connection != null)
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                throw ex;
            }
        }

        public async Task SendSignalRMessage(SignalRTranslateMessage signalRTranslateMessage)
        {
            try
            {
                signalRTranslateMessage.ConnectionId = _connection.ConnectionId;

                if (!string.IsNullOrEmpty(signalRTranslateMessage.ConnectionId))
                    await _connection.SendAsync("SendTranslationHubMessage", signalRTranslateMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                throw ex;
            }
        }
    }
}
