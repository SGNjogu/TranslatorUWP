using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SpeechlyTouch.ViewModels
{
    public class SessionDetailsViewModel : ObservableObject
    {
        private Session _currentSession;
        public Session CurrentSession
        {
            get { return _currentSession; }
            set { SetProperty(ref _currentSession, value); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        private bool IsCopyPasteEnabled { get; set; }
        private readonly IDataService _dataService;
        private readonly ICrashlytics _crashlytics;
        private bool IsSessionOpen { get; set; }

        public SessionDetailsViewModel(IDataService dataService, ICrashlytics crashlytics)
        {
            _dataService = dataService;
            _crashlytics = crashlytics;

            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            Initialize();
        }

        private void HandleMessage(NavigationMessage message)
        {
            if (message.CloseSessionDetails && IsSessionOpen)
            {
                NavigateBack();
            }
            if (message.ResetSessionDetails)
                IsSessionOpen = false;
        }

        private async void Initialize()
        {
            try
            {
                var organizationData = await _dataService.GetOneOrganizationSettingsAsync();

                if (organizationData != null)
                {
                    IsCopyPasteEnabled = organizationData.CopyPasteEnabled;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task GetSessionDetails(Session session)
        {
            IsSessionOpen = true;
            StrongReferenceMessenger.Default.Send(new NavigationMessage { HideShellTitleView = true });
            IsLoading = true;

            try
            {
                CurrentSession = session;

                var transcriptionsList = new List<Chat>();

                var transcriptions = await _dataService.GetSessionTranscriptions(session.ID);
                var organizationSettings = await _dataService.GetOneOrganizationSettingsAsync();

                foreach (var transcription in transcriptions)
                {
                    string message = $"Original: {transcription.OriginalText} \nTranslated: {transcription.TranslatedText}";
                    var chat = new Chat();
                    chat.OriginalMessage = transcription.OriginalText;
                    chat.TranslatedMessage = transcription.TranslatedText;
                    chat.Person = transcription.ChatUser;
                    chat.IsCopyPasteEnabled = organizationSettings.CopyPasteEnabled;

                    if (string.IsNullOrEmpty(chat.Person) || chat.Person.Equals("Person One") || chat.Person.Equals("Person one"))
                    {
                        chat.IsPersonOne = true;
                        chat.OriginalMessageISO = session.SourceLangISO;
                        chat.TranslatedMessageISO = session.TargetLangIso;
                    }
                    else
                    {
                        chat.IsPersonOne = false;
                        chat.OriginalMessageISO = session.TargetLangIso;
                        chat.TranslatedMessageISO = session.SourceLangISO;
                    }

                    chat.Date = transcription.ChatTime;
                    chat.Message = message;
                    chat.IsComplete = true;
                    transcriptionsList.Add(chat);
                }

                StrongReferenceMessenger.Default.Send(new RecognizedChatMessage { IsChatList = true, ChatList = transcriptionsList, IsCopyPasteEnabled = true });
                StrongReferenceMessenger.Default.Send(new AudioPlayerMessage { Session = CurrentSession, IsShowingAudioPlayer = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            IsLoading = false;
        }

        private void NavigateBack()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadSessionHistory = true, HideShellTitleView = false });
        }

        private RelayCommand _goBackCommand = null;
        public RelayCommand GoBackCommand
        {
            get
            {
                return _goBackCommand ?? (_goBackCommand = new RelayCommand(() => { NavigateBack(); }));
            }
        }
    }
}
