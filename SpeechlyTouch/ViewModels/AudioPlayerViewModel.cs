using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class AudioPlayerViewModel : ObservableObject
    {
        private Visibility _loaderVisibility;
        public Visibility LoaderVisibility
        {
            get { return _loaderVisibility; }
            set { SetProperty(ref _loaderVisibility, value); }
        }

        private Visibility _errorVisibility;
        public Visibility ErrorVisibility
        {
            get { return _errorVisibility; }
            set { SetProperty(ref _errorVisibility, value); }
        }

        private Visibility _downloadButtonVisibility;
        public Visibility DownloadButtonVisibility
        {
            get { return _downloadButtonVisibility; }
            set { SetProperty(ref _downloadButtonVisibility, value); }
        }

        private Uri _audioFileSource;
        public Uri AudioFileSource
        {
            get { return _audioFileSource; }
            set { SetProperty(ref _audioFileSource, value); }
        }

        private TimeSpan _position;
        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                SetProperty(ref _position, value);
                OnPositionChanged(Position);
            }
        }

        private Session _selectedSession;
        private ResourceLoader _resourceLoader;
        private readonly ICrashlytics _crashlytics;
        private readonly ISettingsService _settingsService;
        private readonly IAzureStorageService _azureStorageService;

        public AudioPlayerViewModel
            (
            ICrashlytics crashlytics,
            ISettingsService settingsService,
            IAzureStorageService azureStorageService)
        {
            _crashlytics = crashlytics;
            _settingsService = settingsService;
            _azureStorageService = azureStorageService;
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            StrongReferenceMessenger.Default.Register<AudioPlayerMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<DownloadMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            ErrorVisibility = Visibility.Collapsed;
            DownloadButtonVisibility = Visibility.Collapsed;
        }

        private void HandleMessage(DownloadMessage message)
        {
            if(message.DownloadButtonVisibility == Visibility.Visible)
            {
                DownloadButtonVisibility = Visibility.Visible;
            }
            if (message.DownloadButtonVisibility == Visibility.Collapsed)
            {
                DownloadButtonVisibility = Visibility.Collapsed;
            }
        }

        private async void HandleMessage(AudioPlayerMessage message)
        {
            if (message.IsShowingAudioPlayer && message.Session != null)
            {
                _selectedSession = message.Session;
                await LoadAudio();
            }
        }

        private async Task LoadAudio()
        {
            LoaderVisibility = Visibility.Visible;
            ErrorVisibility = Visibility.Collapsed;

            try
            {
                string uri = GetWaveFileUri();
                string fileName = $"{_selectedSession.StartTime}.wav";
                string defaultPlaybackLanguage = GetDefaultPlaybackLanguage();

                if (defaultPlaybackLanguage.Contains("2"))
                {
                    fileName = $"{_selectedSession.StartTime}_Translated.wav";
                    var newUri = uri.Remove(uri.Length - 4, 4);
                    uri = $"{newUri}_Translated.wav";
                }

                bool audioExists = await CheckIfAudioExists(uri, fileName).ConfigureAwait(true);

                // Check if blob exists the second time (if blob does not exist) without appending '_Translated.wav'
                // This check is for legacy apps that did not save two audio files so no '_Translated.wav'
                if (!audioExists && fileName != $"{_selectedSession.StartTime}.wav")
                {
                    uri = GetWaveFileUri();
                    fileName = $"{_selectedSession.StartTime}.wav";
                    audioExists = await CheckIfAudioExists(uri, fileName).ConfigureAwait(true);
                }

                if (audioExists)
                {
                    bool isUrl = IsUrl(uri);
                    bool fileExists = await TryGetAudioFile(uri);

                    if (isUrl && !fileExists)
                    {
                        ErrorVisibility = Visibility.Visible;
                        LoaderVisibility = Visibility.Collapsed;
                        return;
                    }

                    AudioFileSource = new Uri(uri);
                    DownloadButtonVisibility = Visibility.Visible;
                }
                else
                {
                    ErrorVisibility = Visibility.Visible;
                    LoaderVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                ErrorVisibility = Visibility.Visible;
            }

            LoaderVisibility = Visibility.Collapsed;
        }

        private string GetDefaultPlaybackLanguage()
        {
            string defaultPlaybackLanguage = _settingsService.DefaultPlaybackLanguage;
            string language = _resourceLoader.GetString($"Language");

            if (defaultPlaybackLanguage == $"{language} 1")
            {
                return defaultPlaybackLanguage;
            }
            else if (defaultPlaybackLanguage == $"{language} 2")
            {
                return defaultPlaybackLanguage;
            }
            else
            {
                _settingsService.DefaultPlaybackLanguage = $"{language} 1";
                return _settingsService.DefaultPlaybackLanguage;
            }
        }

        private string GetWaveFileUri()
        {
            var audioDir = Constants.GetRecordingsPath();
            var waveFilePath = Path.Combine(audioDir, $"{_selectedSession.StartTime}.wav");

            if (_selectedSession.SyncedToServer)
            {
                return $"{Constants.RecordingsURL}{_selectedSession.StartTime}.wav";
            }
            else
            {
                return waveFilePath;
            }
        }

        private async Task<bool> CheckIfAudioExists(string uri, string audioName)
        {
            if (IsUrl(uri))
            {
                try
                {
                    bool blobExists = await _azureStorageService.CheckIfBlobExists(audioName, Constants.AzureStorageConnectionString, Constants.RecordingsContainer);

                    return blobExists;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Crashes.TrackError(ex, attachments: _crashlytics.Attachments().Result);
                }
            }

            return CheckAudioFileExists(audioName);
        }

        public bool IsUrl(string uri)
        {
            return uri != null && uri.Contains("https://");
        }

        private bool CheckAudioFileExists(string audioName)
        {
            var waveFilePath = Path.Combine(Constants.GetRecordingsPath(), audioName);
            if (File.Exists(waveFilePath))
            {
                return true;
            }
            return false;
        }

        private async Task<bool> TryGetAudioFile(string uri)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(uri);
                webRequest.Method = "HEAD";
                var response = await webRequest.GetResponseAsync();
                return response.ContentLength > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void OnPositionChanged(TimeSpan timeSpan)
        {
            try
            {
                if (_selectedSession != null)
                {
                    var startTime = DateTimeUtility.ReturnDateTimeFromlongSeconds(_selectedSession.StartTime);
                    var endTime = DateTimeUtility.ReturnDateTimeFromlongSeconds(_selectedSession.EndTime);
                    var difference = TimeSpan.FromTicks((endTime - startTime).Ticks);

                    if (difference > timeSpan)
                    {
                        var currentPosition = (timeSpan * 100) / difference;
                        StrongReferenceMessenger.Default.Send(new ChatScrollToMessage { IsPercentage = true, Percentage = currentPosition });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void DownloadAudio()
        {
            if(AudioFileSource != null && _selectedSession.SessionNumber != null)
            {
                StrongReferenceMessenger.Default.Send(new DownloadMessage { Uri = AudioFileSource, SessionNumber = _selectedSession.SessionNumber, DownloadButtonVisibility = Visibility.Collapsed });
            }
        }

        private RelayCommand _downloadAudioCommand = null;
        public RelayCommand DownloadAudioCommand
        {
            get
            {
                return _downloadAudioCommand ?? (_downloadAudioCommand = new RelayCommand( () => {  DownloadAudio(); }));
            }
        }

       
    }
}
