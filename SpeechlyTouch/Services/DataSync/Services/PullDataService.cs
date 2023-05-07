using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Core.Services.Languages;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.Settings;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreSession = SpeechlyTouch.Core.Domain.Session;

namespace SpeechlyTouch.Services.DataSync.Services
{
    public class PullDataService : IPullDataService
    {
        private readonly IDataService _dataService;
        private readonly ISettingsService _settingsService;
        private readonly ICrashlytics _crashlytics;
        private readonly ISessionService _sessionService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IDeviceService _deviceService;
        private readonly IAuthService _authService;
        private readonly ILanguagesService _languagesService;

        private BackgroundWorker BackgroundWorkerClient;
        public PullDataService
            (
            IDataService dataService,
            ISettingsService settingsService,
            ICrashlytics crashlytics,
            ISessionService sessionService,
            ITranscriptionService transcriptionService,
            IDeviceService deviceService,
            IAuthService authService,
            ILanguagesService languagesService
            )
        {
            _dataService = dataService;
            _settingsService = settingsService;
            _crashlytics = crashlytics;
            _sessionService = sessionService;
            _transcriptionService = transcriptionService;
            _authService = authService;
            _languagesService = languagesService;
            _deviceService = deviceService;

            BackgroundWorkerClient = new BackgroundWorker();
            BackgroundWorkerClient.DoWork += DoWork;
            BackgroundWorkerClient.RunWorkerCompleted += RunWorkerCompleted;
        }

        public void BeginDataSync()
        {
            BackgroundWorkerClient.RunWorkerAsync();
        }

        public void CancelDataSync()
        {
            BackgroundWorkerClient.CancelAsync();
        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            var userSettings = await _settingsService.GetUser();
            await SyncDatabase(userSettings).ConfigureAwait(false);
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do something when background work is completed
            if (e.Cancelled)
            {
                Debug.WriteLine("DATAPULL WAS CANCELLED");
            }
        }

        public async Task SyncDatabase(User userSettings)
        {
            try
            {
                var sessionsCount = await _dataService.GetSessionCountAsync();
                var sessionsList = await _sessionService.GetSessionsForUser((int)userSettings.UserIntID, (int)userSettings.OrganizationId, _authService.IdToken);

                if (sessionsCount == 0)
                {
                    foreach (var session in sessionsList)
                    {
                        await SyncSession(session, userSettings);
                    }
                    StrongReferenceMessenger.Default.Send(new HistoryMessage { RefreshHistory = true });
                }
                else if (sessionsList.Count() > sessionsCount)
                {
                    var localSessions = await _dataService.GetSessionsAsync();
                    foreach (var session in sessionsList)
                    {
                        var existingSession = localSessions.FirstOrDefault(s => s.StartTime == session.StartTime);
                        if (existingSession == null)
                        {
                            await SyncSession(session, userSettings);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncSession(CoreSession session, User userSettings)
        {
            var localSession = await SaveSessionLocally(session, userSettings);
            await SyncTranscriptions(session.Id, localSession.ID);
            await SyncSessionTags(session.Id, localSession.ID);
            await SyncDevices(session.Id, localSession.ID);
        }

        private async Task<Session> SaveSessionLocally(CoreSession session, User userSettings)
        {
            try
            {
                //TODO Implement Language service before pulling sessions from the backend
                var languages = _languagesService.GetSupportedLanguages();
                var rawStart = DateTimeOffset.FromUnixTimeSeconds(session.StartTime).ToLocalTime();
                var rawEnd = DateTimeOffset.FromUnixTimeSeconds(session.EndTime).ToLocalTime();
                var timeDuration = rawEnd - rawStart;
                string duration;

                if (string.IsNullOrEmpty(session.Duration))
                    duration = timeDuration.ToString(@"m\:ss");
                else
                    duration = session.Duration;

                return await _dataService.AddItemAsync<Session>(new Session
                {
                    UserId = (int)userSettings.UserIntID,
                    SourceLanguage = languages.Where(s => s.Code == session.SourceLangISO).FirstOrDefault()?.Name,
                    SourceLangISO = session.SourceLangISO,
                    TargeLanguage = languages.Where(s => s.Code == session.TargetLangIso).FirstOrDefault()?.Name,
                    TargetLangIso = session.TargetLangIso,
                    SyncedToServer = true,
                    SyncText = "",
                    StartTime = session.StartTime,
                    RecordDate = rawStart.ToString("d"),
                    RawStartTime = rawStart.ToString("t"),
                    EndTime = session.EndTime,
                    RawEndTime = rawEnd.ToString("t"),
                    Duration = duration,
                    ClientType = session.ClientType,
                    AllModelsSyncedToServer = true,
                    SessionNumber = session.SessionNumber,
                    BillableSeconds = session.BillableSeconds,
                    SoftwareVersion = session.SoftwareVersion,
                    CustomTags = session.CustomTags,
                    SessionName = session.SessionName
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        private async Task SyncTranscriptions(int sessionId, int localSessionId)
        {
            try
            {
                var transcriptions = await _transcriptionService.GetTranscriptionsForSession(sessionId, _authService.IdToken);

                foreach (var transcription in transcriptions)
                {
                    await _dataService.AddItemAsync<Transcription>(new Transcription
                    {
                        ChatTime = transcription.ChatTime,
                        ChatUser = transcription.ChatUser,
                        Timestamp = transcription.ChatTime.ToString("H:mm"),
                        OriginalText = transcription.OriginalText,
                        SessionId = localSessionId,
                        SyncedToServer = true,
                        TranslatedText = transcription.TranslatedText,
                        Sentiment = transcription.Sentiment,
                        TranslationSeconds = transcription.TranslationSeconds
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncDevices(int sessionId, int localSessionId)
        {
            try
            {
                var devices = await _deviceService.GetDevicesForSession(sessionId, _authService.IdToken);

                foreach (var device in devices)
                {
                    await _dataService.AddItemAsync<Device>(new Device
                    {
                        Name = device.Name,
                        Certified = device.Certified,
                        SessionId = localSessionId,
                        SyncedToServer = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncSessionTags(int sessionId, int localSessionId)
        {
            try
            {
                var sessionTags = await _sessionService.GetSessionTags(sessionId, _authService.IdToken);

                if (sessionTags != null && sessionTags.Any())
                {
                    foreach (var sessionTag in sessionTags)
                    {
                        await _dataService.AddItemAsync<DataService.Models.SessionTag>(new DataService.Models.SessionTag
                        {
                            SessionId = localSessionId,
                            SyncedToServer = true,
                            OrganizationId = sessionTag.OrganizationId,
                            OrganizationTagId = sessionTag.OrganizationTagId,
                            TagValue = sessionTag.TagValue
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }
    }
}
