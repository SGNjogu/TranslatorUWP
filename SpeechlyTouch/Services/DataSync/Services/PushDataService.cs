using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreBulkTranscriptions = SpeechlyTouch.Core.Domain.BulkTranscriptions;
using CoreDevice = SpeechlyTouch.Core.Domain.Device;
using CorePlaybackUsageTracking = SpeechlyTouch.Core.Domain.PlaybackUsageTracking;
using CoreSession = SpeechlyTouch.Core.Domain.Session;
using CoreSessionTag = SpeechlyTouch.Core.Domain.SessionTag;
using CoreTranscription = SpeechlyTouch.Core.Domain.Transcription;
using CoreUserFeedback = SpeechlyTouch.Core.Domain.UserFeedback;
using CoreQuestion = SpeechlyTouch.Core.Domain.UserQuestion;
using SpeechlyTouch.Enums;

namespace SpeechlyTouch.Services.DataSync.Services
{
    public class PushDataService : IPushDataService
    {
        private readonly IDataService _dataService;
        private readonly ICrashlytics _crashlytics;
        private readonly ISessionService _sessionService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IDeviceService _deviceService;
        private readonly IAzureStorageService _azureStorageService;
        private readonly ISettingsService _settingsService;
        private readonly IAuthService _authService;
        private readonly IUserFeedbackService _userFeedbackService;
        private readonly IPlaybackUsageService _playbackUsageService;
        private readonly IUsageTrackingService _usageTrackingService;
        private readonly IQuestionsService _questionsService;
        private BackgroundWorker BackgroundWorkerClient;
        private BackgroundWorker PlaybackUsageBackgroundWorkerClient;

        public PushDataService
            (
            IDataService dataServiceSession,
            ICrashlytics crashlytics,
            ISessionService sessionService,
            ITranscriptionService transcriptionService,
            IDeviceService deviceService,
            IAzureStorageService azureStorageService,
            ISettingsService settingsService,
            IAuthService authService,
            IUserFeedbackService userFeedbackService,
            IPlaybackUsageService playbackUsageService,
            IUsageTrackingService usageTrackingService,
            IQuestionsService questionsService
            )
        {
            _dataService = dataServiceSession;
            _crashlytics = crashlytics;
            _sessionService = sessionService;
            _transcriptionService = transcriptionService;
            _deviceService = deviceService;
            _azureStorageService = azureStorageService;
            _settingsService = settingsService;
            _authService = authService;
            _playbackUsageService = playbackUsageService;
            _userFeedbackService = userFeedbackService;
            _usageTrackingService = usageTrackingService;
            _questionsService = questionsService;

            BackgroundWorkerClient = new BackgroundWorker();
            BackgroundWorkerClient.DoWork += DoWork;
            BackgroundWorkerClient.RunWorkerCompleted += RunWorkerCompleted;

            PlaybackUsageBackgroundWorkerClient = new BackgroundWorker();
            PlaybackUsageBackgroundWorkerClient.DoWork += PlaybackUsageBackgroundWorkerClient_DoWork;
        }

        private async void PlaybackUsageBackgroundWorkerClient_DoWork(object sender, DoWorkEventArgs e)
        {
            await SyncPlaybackUsage().ConfigureAwait(false);
        }

        public void BeginPlaybackUsageSync()
        {
            try
            {
                PlaybackUsageBackgroundWorkerClient.RunWorkerAsync();
            }
            catch (Exception) { }
        }

        public void BeginDataSync()
        {
            try
            {
                BackgroundWorkerClient.RunWorkerAsync();
            }
            catch (Exception) { }
        }

        public void CancelDataSync()
        {
            BackgroundWorkerClient.CancelAsync();
        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            await SyncDatabase().ConfigureAwait(false);
        }

        private async Task SyncDatabase()
        {
            try
            {
                await SyncUserQuestions();
                await SyncDeletedUserQuestions();

                var userSettings = await _settingsService.GetUser();

                var usage = await _usageTrackingService.GetOrganizationUsageLimit(userSettings.OrganizationId, _authService.IdToken);
                bool hasUnlimitedLicense = usage.LicensingType.ToLower() == "postpaid";

                if (usage.StorageLimitExceeded == true && !hasUnlimitedLicense)
                    return;

                var unsyncedSessions = await UnsyncedSessions();

                if (unsyncedSessions == null || !unsyncedSessions.Any())
                    return;

                foreach (var session in unsyncedSessions)
                {
                    var uploadedSession = await UploadSession(session);

                    if (uploadedSession != null)
                    {
                        await UpdateSessionLocally(session);
                        await SyncTranscriptions(localSessionID: session.ID, apiSessionId: uploadedSession.Id);
                        await SyncSessionTags(localSessionID: session.ID, apiSessionId: uploadedSession.Id);
                        await SyncDevices(localSessionID: session.ID, apiSessionId: uploadedSession.Id);
                        await SyncSessionFeedback(session.SessionNumber);

                        bool allModelsSynced = await CheckSyncStatus(session.ID);
                        var currentSession = await _dataService.GetOneSessionAsync(session.ID);

                        if (allModelsSynced)
                        {
                            currentSession.AllModelsSyncedToServer = true;
                            await SendEmail(session);
                        }
                        else
                        {
                            currentSession.AllModelsSyncedToServer = false;
                        }

                        await _dataService.UpdateItemAsync<Session>(currentSession);
                    }

                }
                StrongReferenceMessenger.Default.Send(new HistoryMessage { RefreshHistory = true });

                await UpdateSessionsWithoutAllModelsSynced();
                await SyncFeedbackRatings();
                await SyncPlaybackUsage();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateSessionsWithoutAllModelsSynced()
        {
            var allSessions = await _dataService.GetSessionsAsync();
            var UnsyncedSessions = allSessions.Where(s => s.AllModelsSyncedToServer == false);
            foreach (Session session in UnsyncedSessions)
            {
                session.SyncText = "Not Synced";
                await _dataService.UpdateItemAsync<Session>(session);
            }
        }

        public async Task<CoreSession> UploadSession(Session session)
        {
            CoreSession uploadedSession = null;

            try
            {
                bool isUploaded = await UploadWavFile(session).ConfigureAwait(true);

                var sessionUrl = _azureStorageService.GetUrl($"{session.StartTime}.mp3", Constants.AzureStorageConnectionString, Constants.ConvertedRecordingsContainer);
                session.SessionUrl = sessionUrl;

                if (isUploaded)
                    uploadedSession = await _sessionService.Create(new CoreSession
                    {
                        UserId = session.UserId,
                        EndTime = session.EndTime,
                        OrganizationId = session.OrganizationId,
                        SoftwareVersion = session.SoftwareVersion,
                        SourceLangISO = session.SourceLangISO,
                        StartTime = session.StartTime,
                        TargetLangIso = session.TargetLangIso,
                        Duration = session.Duration,
                        ClientType = Helpers.EnumsConverter.ConvertToString(ClientType.IOT),
                        SessionUrl = session.SessionUrl,
                        SessionNumber = session.SessionNumber,
                        BillableSeconds = session.BillableSeconds,
                        CustomTags = session.CustomTags,
                        SessionName = session.SessionName
                    }, _authService.IdToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            return await Task.FromResult(uploadedSession);
        }

        private async Task<bool> UploadWavFile(Session session)
        {
            var userSettings = await _settingsService.GetUser();
            try
            {
                if (userSettings.DataConsentStatus)
                {
                    var waveFilePath = Path.Combine(Constants.GetRecordingsPath(), $"{session.StartTime.ToString()}.wav");
                    var waveFilePathTranslated = Path.Combine(Constants.GetRecordingsPath(), $"{session.StartTime.ToString()}_Translated.wav");
                    bool isWavFileUploaded = false;
                    bool isTranslatedWavFileUploaded = false;
                    bool areAllAudioFilesUploaded;

                    if (File.Exists(waveFilePath))
                    {
                        isWavFileUploaded = await _azureStorageService.UploadFile(waveFilePath, $"{session.StartTime.ToString()}.wav", Constants.AzureStorageConnectionString, Constants.RecordingsContainer);
                        if (isWavFileUploaded)
                            await DeleteLocalAudioFile(waveFilePath);
                    }

                    if (File.Exists(waveFilePathTranslated))
                    {
                        isTranslatedWavFileUploaded = await _azureStorageService.UploadFile(waveFilePathTranslated, $"{session.StartTime.ToString()}_Translated.wav", Constants.AzureStorageConnectionString, Constants.RecordingsContainer);
                        if (isTranslatedWavFileUploaded)
                            await DeleteLocalAudioFile(waveFilePathTranslated);
                    }

                    if (session.IsSingleDeviceSession)
                    {
                        areAllAudioFilesUploaded = isWavFileUploaded;
                    }
                    else
                    {
                        areAllAudioFilesUploaded = isWavFileUploaded && isTranslatedWavFileUploaded;
                    }

                    return areAllAudioFilesUploaded;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            return false;
        }

        private async Task SyncTranscriptions(int localSessionID, int apiSessionId)
        {
            try
            {
                var unsyncedTranscriptions = await UnsyncedTranscriptions(localSessionID);

                if (unsyncedTranscriptions != null && unsyncedTranscriptions.Any())
                {
                    foreach (var transcription in unsyncedTranscriptions)
                    {
                        transcription.SessionId = apiSessionId;
                    }

                    var transcriptionsJsonString = JsonConvert.SerializeObject(unsyncedTranscriptions);
                    var transcriptionsList = JsonConvert.DeserializeObject<List<CoreTranscription>>(transcriptionsJsonString);

                    await _transcriptionService.CreateBulk(new CoreBulkTranscriptions
                    {
                        SessionId = apiSessionId,
                        Transcriptions = transcriptionsList
                    }, _authService.IdToken);

                    foreach (var transcription in unsyncedTranscriptions)
                    {
                        await UpdateTranscriptionLocally(transcription, localSessionID);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncDevices(int localSessionID, int apiSessionId)
        {
            try
            {
                var unsyncedDevices = await UnsyncedDevices(localSessionID);

                if (unsyncedDevices != null && unsyncedDevices.Any())
                {
                    foreach (var device in unsyncedDevices)
                    {
                        device.SessionId = apiSessionId;

                        await _deviceService.Create(new CoreDevice
                        {
                            SessionId = apiSessionId,
                            Certified = device.Certified,
                            Name = device.Name
                        }, _authService.IdToken);

                        await UpdateDeviceLocally(device, localSessionID);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncSessionTags(int localSessionID, int apiSessionId)
        {
            try
            {
                var unsyncedSessionTags = await UnsyncedSessionTags(localSessionID);

                if (unsyncedSessionTags != null && unsyncedSessionTags.Any())
                {
                    foreach (var sessionTag in unsyncedSessionTags)
                    {
                        sessionTag.SessionId = apiSessionId;

                        await _sessionService.CreateSessionTag(new CoreSessionTag
                        {
                            SessionId = apiSessionId,
                            OrganizationId = sessionTag.OrganizationId,
                            OrganizationTagId = sessionTag.OrganizationTagId,
                            TagValue = sessionTag.TagValue
                        }, _authService.IdToken);

                        await UpdateSessionTagLocally(sessionTag, localSessionID);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncSessionFeedback(string sessionNumber)
        {
            try
            {
                var feedbacks = await _dataService.GetFeedbackRatingsAsync();
                var userFeedback = feedbacks.FirstOrDefault(f => f.SessionNumber == sessionNumber);
                if (userFeedback != null)
                {
                    var uploadedFeedback = await _userFeedbackService.Create(new CoreUserFeedback
                    {
                        SessionNumber = userFeedback.SessionNumber,
                        Rating = userFeedback.Rating,
                        ReasonForRating = userFeedback.ReasonForRating,
                        Comment = userFeedback.Comment,
                        FeedbackType = userFeedback.FeedbackType
                    }, _authService.IdToken);

                    if (uploadedFeedback != null)
                        await DeleteUserFeedbackLocally(userFeedback);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncFeedbackRatings()
        {
            try
            {
                var unsyncedFeedbackRatings = await UnsyncedFeedbackRatings();

                if (unsyncedFeedbackRatings != null && unsyncedFeedbackRatings.Any())
                {
                    foreach (var userFeedback in unsyncedFeedbackRatings)
                    {
                        await _userFeedbackService.Create(new CoreUserFeedback
                        {
                            SessionNumber = userFeedback.SessionNumber,
                            Rating = userFeedback.Rating,
                            ReasonForRating = userFeedback.ReasonForRating,
                            Comment = userFeedback.Comment,
                            FeedbackType = userFeedback.FeedbackType
                            
                        }, _authService.IdToken);

                        await DeleteUserFeedbackLocally(userFeedback);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncPlaybackUsage()
        {
            try
            {
                var unsynced = await UnsyncedPlaybackUsage();

                if (unsynced != null && unsynced.Any())
                {
                    foreach (var usage in unsynced)
                    {
                        await _playbackUsageService.Create(new CorePlaybackUsageTracking
                        {
                            SessionNumber = usage.SessionNumber,
                            PlaybackSeconds = usage.PlaybackSeconds,
                            UserId = usage.UserId
                        }, _authService.IdToken);

                        await DeletePlaybackUsageLocally(usage);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncUserQuestions()
        {
            try
            {
                var unsyncedQuestions = await UnsyncedUserQuestions();
                if (!unsyncedQuestions.Any())
                    return;
                var user = await _settingsService.GetUser();

                foreach (var question in unsyncedQuestions)
                {
                    var uploadedUserQuestion = await _questionsService.CreateUserQuestion((int)user.UserIntID,new CoreQuestion
                    {
                        Question = question.Question,
                        LanguageCode = question.LanguageCode
                    }, _authService.IdToken);
                    var userQuestions = await _questionsService.GetUserQuestions((int)user.UserIntID, _authService.IdToken);
                    var que = userQuestions.Where(x => x.Question == question.Question && x.LanguageCode == question.LanguageCode).Count();
                    if (que > 1)
                    {
                        var duplicated = userQuestions.Where(x => x.Question == question.Question && x.LanguageCode == question.LanguageCode);
                        var duplicatedIDs = duplicated.Select(x => x.Id);
                        var localQuestions = await _dataService.GetOrgQuestionsAsync();
                        var localIDs = localQuestions.Select(x => x.QuestionID);
                        var uniqueIDs = duplicatedIDs.Except(localIDs);
                        var applyID = uniqueIDs.FirstOrDefault();
                        await UpdateQuestionLocaly(question, applyID);
                    }
                    else
                    {
                        var updatedQuestion = userQuestions.Where(x => x.Question == question.Question && x.LanguageCode == question.LanguageCode).FirstOrDefault();
                        await UpdateQuestionLocaly(question, updatedQuestion.Id);
                    }
                }
                    
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateQuestionLocaly(OrgQuestions orgQuestion, int uniqueID)
        {
            try
            {
                var questions = await _dataService.GetOrgQuestionsAsync();
                var availableQuestions = questions.Where(x => x.ID == orgQuestion.ID);
                OrgQuestions updatedOrgQuestion = new OrgQuestions()
                {
                      ID = orgQuestion.ID,
                      Question = orgQuestion.Question,
                      LanguageCode = orgQuestion.LanguageCode,
                      Shortcut = orgQuestion.Shortcut,
                      QuestionStatus = (int)UserQuestionStatus.Available,
                      SyncedToServer = true,
                      QuestionID = uniqueID,
                      QuestionType = orgQuestion.QuestionType,
                };

                await _dataService.UpdateItemAsync<OrgQuestions>(updatedOrgQuestion);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncDeletedUserQuestions()
        {
            try
            {
                var unsyncedDeletedQuestions = await UnsyncedDeletedUserQuestions();
                if (!unsyncedDeletedQuestions.Any())
                    return;
                var user = await _settingsService.GetUser();

                foreach (var question in unsyncedDeletedQuestions)
                {
                    List<int> questionID = new List<int>()
                    {
                        question.QuestionID,
                    };
                    var questionDeleted = await _questionsService.DeleteUserQuestion((int)user.UserIntID, new UserQuestionsDTO
                    {
                        QuestionIDs = questionID
                    }, _authService.IdToken);

                    if (questionDeleted)
                        await DeleteUserQuestionLocally(question);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do something when background work is completed
            if (e.Cancelled)
            {
                Debug.WriteLine("DATAPUSH WAS CANCELLED");
            }
        }

        private async Task<List<Session>> UnsyncedSessions()
        {
            List<Session> sessions = null;
            try
            {
                sessions = await _dataService.GetSessionsAsync();
                sessions = sessions.Where(s => s.AllModelsSyncedToServer == false).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(sessions);
        }

        private async Task<List<Transcription>> UnsyncedTranscriptions(int localSessionId)
        {
            List<Transcription> transcriptions = null;
            try
            {
                transcriptions = await _dataService.GetTranscriptionsAsync();
                transcriptions = transcriptions.Where(s => s.SyncedToServer == false && s.SessionId == localSessionId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(transcriptions);
        }

        private async Task<List<Device>> UnsyncedDevices(int localSessionId)
        {
            List<Device> devices = null;
            try
            {
                devices = await _dataService.GetDevicesAsync();
                devices = devices.Where(s => s.SyncedToServer == false && s.SessionId == localSessionId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(devices);
        }

        private async Task<List<SessionTag>> UnsyncedSessionTags(int localSessionId)
        {
            List<SessionTag> sessionTags = null;

            try
            {
                sessionTags = await _dataService.GetSessionTags(localSessionId);
                sessionTags = sessionTags.Where(s => s.SyncedToServer == false && s.SessionId == localSessionId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(sessionTags);
        }

        private async Task<List<UserFeedback>> UnsyncedFeedbackRatings()
        {
            List<UserFeedback> ratings = null;
            try
            {
                ratings = await _dataService.GetFeedbackRatingsAsync();
                ratings = ratings.Where(s => s.SyncedToServer == false).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(ratings);
        }

        private async Task<List<PlaybackUsage>> UnsyncedPlaybackUsage()
        {
            List<PlaybackUsage> playbackUsages = null;
            try
            {
                playbackUsages = await _dataService.GetPlaybackUsageAsync();
                playbackUsages = playbackUsages.Where(s => s.SyncedToServer == false).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(playbackUsages);
        }

        private async Task<List<OrgQuestions>> UnsyncedUserQuestions()
        {
            List<OrgQuestions> userQuestions = null;
            try
            {
                userQuestions = await _dataService.GetOrgQuestionsAsync();
                userQuestions = userQuestions.Where(s => s.SyncedToServer == false && s.QuestionStatus == (int)UserQuestionStatus.Available).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(userQuestions);
        }
        private async Task<List<OrgQuestions>> UnsyncedDeletedUserQuestions()
        {
            List<OrgQuestions> userQuestions = null;
            try
            {
                userQuestions = await _dataService.GetOrgQuestionsAsync();
                userQuestions = userQuestions.Where(s => s.SyncedToServer == true && s.QuestionStatus == (int)UserQuestionStatus.Deleted).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(userQuestions);
        }


        private async Task UpdateSessionLocally(Session session)
        {
            try
            {
                session.SyncedToServer = true;
                session.SyncText = "";
                await _dataService.UpdateItemAsync<Session>(session);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task DeleteLocalAudioFile(string waveFilePath)
        {
            try
            {
                if (File.Exists(waveFilePath))
                {
                    File.Delete(waveFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateTranscriptionLocally(Transcription transcription, int localSessionID)
        {
            try
            {
                transcription.SyncedToServer = true;
                transcription.SessionId = localSessionID;
                await _dataService.UpdateItemAsync<Transcription>(transcription);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateDeviceLocally(Device device, int localSessionID)
        {
            try
            {
                device.SyncedToServer = true;
                device.SessionId = localSessionID;
                await _dataService.UpdateItemAsync<Device>(device);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateSessionTagLocally(SessionTag sessionTag, int localSessionID)
        {
            try
            {
                sessionTag.SyncedToServer = true;
                sessionTag.SessionId = localSessionID;
                await _dataService.UpdateItemAsync<SessionTag>(sessionTag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task DeleteUserFeedbackLocally(UserFeedback userFeedback)
        {
            try
            {
                await _dataService.DeleteItemAsync<UserFeedback>(userFeedback);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task DeleteUserQuestionLocally(OrgQuestions userQuestion)
        {
            try
            {
                await _dataService.DeleteItemAsync<OrgQuestions>(userQuestion);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task DeletePlaybackUsageLocally(PlaybackUsage playbackUsage)
        {
            try
            {
                await _dataService.DeleteItemAsync<PlaybackUsage>(playbackUsage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task<bool> CheckSyncStatus(int sessionId)
        {
            //Check Devices
            var unsyncedDevices = await UnsyncedDevices(sessionId);

            //Check Transcriptions
            var unsyncedTranscriptions = await UnsyncedTranscriptions(sessionId);

            //Check Session
            var session = await _dataService.GetOneSessionAsync(sessionId);
            SyncStatus currentSyncStatus;
            var existingSyncStatus = await ExistingSyncStatus(sessionId);

            if (existingSyncStatus.Count < 1 && session != null)
            {
                SyncStatus syncStatus = new SyncStatus()
                {
                    LocalSessionId = sessionId,
                    SessionSync = session.SyncedToServer,
                    TranscriptionsSync = unsyncedTranscriptions.Count < 1 ? true : false,
                    DevicesSync = unsyncedDevices.Count < 1 ? true : false
                };
                currentSyncStatus = await _dataService.AddItemAsync<SyncStatus>(syncStatus).ConfigureAwait(false);
            }
            else
            {
                var firstSyncStatus = existingSyncStatus.FirstOrDefault();
                firstSyncStatus.SessionSync = session.SyncedToServer;
                firstSyncStatus.TranscriptionsSync = unsyncedTranscriptions.Count < 1 ? true : false;
                firstSyncStatus.DevicesSync = unsyncedDevices.Count < 1 ? true : false;

                currentSyncStatus = await _dataService.UpdateItemAsync<SyncStatus>(firstSyncStatus);
            }

            return SyncStatus(currentSyncStatus);
        }

        private async Task<List<SyncStatus>> ExistingSyncStatus(int sessionId)
        {
            IEnumerable<SyncStatus> syncStatusList = await _dataService.GetSyncStatusAsync();
            return syncStatusList.Where(s => s.LocalSessionId == sessionId).ToList();
        }

        private bool SyncStatus(SyncStatus currentSyncStatus)
        {
            if (!currentSyncStatus.SessionSync || !currentSyncStatus.TranscriptionsSync || !currentSyncStatus.DevicesSync)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task<bool> SendEmail(Session session)
        {
            try
            {
                if (session.EmailingAddress != null)
                {
                    return await _sessionService.CreateSessionEmail(session.SessionNumber, session.EmailingAddress, _authService.IdToken);
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return false;
        }
    }
}
