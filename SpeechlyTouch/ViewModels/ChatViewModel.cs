using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        private ObservableCollection<Chat> _chats;
        public ObservableCollection<Chat> Chats
        {
            get { return _chats; }
            set
            {
                SetProperty(ref _chats, value);
                ChatViewChats = Chats;
            }
        }

        public static ObservableCollection<Chat> ChatViewChats { get; set; }

        private Visibility _audioPlayerVisibility;
        public Visibility AudioPlayerVisibility
        {
            get { return _audioPlayerVisibility; }
            set { SetProperty(ref _audioPlayerVisibility, value); }
        }

        /// <summary>
        /// This property binds to the visibility of the copy and share buttons, and their menu items
        /// </summary>
        private Visibility _copyAllAndShareVisibility;
        public Visibility CopyAllAndShareVisibility
        {
            get { return _copyAllAndShareVisibility; }
            set { SetProperty(ref _copyAllAndShareVisibility, value); }
        }

        private double _metaDataFontSize;
        public double MetaDataFontSize
        {
            get { return _metaDataFontSize; }
            set { SetProperty(ref _metaDataFontSize, value); }
        }

        private double _transcriptionsFontSize = 14;
        public double TranscriptionsFontSize
        {
            get { return _transcriptionsFontSize; }
            set
            {
                SetProperty(ref _transcriptionsFontSize, value);
                MetaDataFontSize = TranscriptionsFontSize - 3;
            }
        }

        private bool IsCopyPasteEnabled { get; set; }
        private bool IsAudioPlayerEnabled { get; set; }
        public static bool IsShareAll { get; set; } = false;
        private readonly IDataService _dataService;
        public readonly IAuthService _authService;
        private readonly ICrashlytics _crashlytics;
        private readonly ISettingsService _settingsService;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IDialogService _dialogService;
        private ResourceLoader _resourceLoader;

        public ImmersiveReaderDialog immersiveReaderDialog;

        public ChatViewModel
            (
            IDataService dataService,
            IAuthService authService,
            ICrashlytics crashlytics,
            ISettingsService settingsService,
            IAppAnalytics appAnalytics,
            IDialogService dialogService
            )
        {
            _resourceLoader = ResourceLoader.GetForCurrentView();
            _crashlytics = crashlytics;
            _settingsService = settingsService;
            _appAnalytics = appAnalytics;
            _dialogService = dialogService;
            CopyAllAndShareVisibility = Visibility.Collapsed;
            AudioPlayerVisibility = Visibility.Collapsed;
            ChatViewChats = new ObservableCollection<Chat>();
            Chats = new ObservableCollection<Chat>();
            StrongReferenceMessenger.Default.Register<RecognizedChatMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<PartialChatMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<AudioPlayerMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _dataService = dataService;
            _authService = authService;
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                TranscriptionsFontSize = _settingsService.TranscriptionsFontSize;

                var organizationSettings = await _dataService.GetOneOrganizationSettingsAsync();

                if (organizationSettings != null)
                {
                    IsCopyPasteEnabled = organizationSettings.CopyPasteEnabled;
                    IsAudioPlayerEnabled = organizationSettings.HistoryAudioPlaybackEnabled;
                    if (IsCopyPasteEnabled)
                        CopyAllAndShareVisibility = Visibility.Visible;
                    else
                        CopyAllAndShareVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void HandleMessage(AudioPlayerMessage message)
        {
            if (message.IsShowingAudioPlayer && IsAudioPlayerEnabled)
                AudioPlayerVisibility = Visibility.Visible;
            else
                AudioPlayerVisibility = Visibility.Collapsed;
        }

        private async void HandleMessage(NavigationMessage message)
        {
            try
            {
                if (message.InitializeSession)
                {
                    AudioPlayerVisibility = Visibility.Collapsed;
                    Chats = new ObservableCollection<Chat>();
                }
                if (message.CloseImmersiveReader)
                {
                    immersiveReaderDialog.tbInfo.Text = "";
                    immersiveReaderDialog.ImmersiveReaderWebView.Source = new Uri("about:blank");
                    _dialogService.HideDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void HandleMessage(RecognizedChatMessage message)
        {
            try
            {
                if (message.IsChatList)
                {
                    Chats = new ObservableCollection<Chat>(message.ChatList);
                }
                else if (Chats.Any() && message.Chat.OriginalMessage != Chats.LastOrDefault().OriginalMessage)
                {
                    var lastTranscription = Chats.Last();
                    if ((Chats.All(c => c.OffsetInTicks != message.Chat.OffsetInTicks) && message.Chat.OriginalMessage != lastTranscription.OriginalMessage) || !Chats.Any())
                    {
                        message.Chat.IsComplete = true;
                        Chats.Add(message.Chat);
                        await HandleChatItem(message);
                    }
                    else if (Chats.Any(s => s.OffsetInTicks == message.Chat.OffsetInTicks) && message.Chat.OriginalMessage != lastTranscription.OriginalMessage)
                    {
                        var exisitingChatItem = Chats.FirstOrDefault(s => s.OffsetInTicks == message.Chat.OffsetInTicks);

                        if (exisitingChatItem != null)
                        {
                            exisitingChatItem.Message = message.Chat.Message;
                            exisitingChatItem.OriginalMessage = message.Chat.OriginalMessage;
                            exisitingChatItem.TranslatedMessage = message.Chat.TranslatedMessage;
                            exisitingChatItem.Duration = message.Chat.Duration;
                            exisitingChatItem.Person = message.Chat.Person;
                            exisitingChatItem.IsComplete = true;
                        }
                    }

                    StrongReferenceMessenger.Default.Send(new ChatScrollToMessage { ScrollTo = message.Chat });
                    await HandleChatItem(message);
                }
                else if (Chats != null && !Chats.Any()) //This gets visited only the first time, when there are no chats, and this is not from History.
                {
                    message.Chat.IsComplete = true; //prevent pre defined questions from being deleted
                    Chats.Add(message.Chat);
                    await HandleChatItem(message);
                }
                else if (Chats.Any() && message.Chat.OriginalMessage == Chats.LastOrDefault().OriginalMessage)
                {
                    Chats.LastOrDefault().IsComplete = true;
                }

                List<Chat> newChats = Chats.ToList();
                if (newChats.Any() && newChats.Any(c => c.IsComplete == false))
                {
                    newChats.RemoveAll(c => c.IsComplete == false);
                    Chats = new ObservableCollection<Chat>(newChats);
                    StrongReferenceMessenger.Default.Send(new ChatScrollToMessage { ScrollTo = Chats.LastOrDefault() });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void HandleMessage(PartialChatMessage message)
        {
            try
            {
                if (Chats.Any() && Chats.Any(s => s.OffsetInTicks == message.Chat.OffsetInTicks))
                {
                    var exisitingChatItem = Chats.FirstOrDefault(s => s.OffsetInTicks == message.Chat.OffsetInTicks);

                    if (exisitingChatItem != null)
                    {
                        exisitingChatItem.Message = message.Chat.Message;
                        exisitingChatItem.OriginalMessage = message.Chat.OriginalMessage;
                        exisitingChatItem.TranslatedMessage = message.Chat.TranslatedMessage;
                        exisitingChatItem.Duration = message.Chat.Duration;
                        exisitingChatItem.Person = message.Chat.Person;
                        exisitingChatItem.IsComplete = false;
                    }
                }
                else
                {
                    message.Chat.IsComplete = false;
                    Chats.Add(message.Chat);
                    StrongReferenceMessenger.Default.Send(new ChatScrollToMessage { ScrollTo = message.Chat });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task HandleChatItem(RecognizedChatMessage message)
        {
            try
            {
                string person;

                if (message.Chat.IsPersonOne)
                    person = _resourceLoader.GetString("ChatViewPersonOne");
                else
                    person = _resourceLoader.GetString("ChatViewPersonTwo");

                var rawTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var chatTime = DateTimeOffset.FromUnixTimeSeconds(rawTime).ToLocalTime();

                var transcription = new Transcription
                {
                    Timestamp = chatTime.ToString("H:mm"),
                    ChatTime = chatTime.DateTime,
                    ChatUser = person,
                    Sentiment = "Neutral",
                    OriginalText = message.Chat.OriginalMessage,
                    SessionId = message.SessionId,
                    SyncedToServer = false,
                    TranslatedText = message.Chat.TranslatedMessage,
                    TranslationSeconds = message.Chat.Duration.TotalSeconds
                };

                await _dataService.AddItemAsync<Transcription>(transcription);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task ShowImmersiveReader()
        {
           await _dialogService.ShowDialog(immersiveReaderDialog);
        }
        private async void CopyAll()
        {
            try
            {
                var user = await _settingsService.GetUser();

                if (Chats != null && Chats.Any())
                {
                    string copyText = string.Empty;

                    foreach (var item in Chats)
                    {
                        string text = $"{item.Person}\n{item.Message}\n";
                        copyText = $"{copyText}\n{text}";
                    }

                    DataPackage dataPackage = new DataPackage();
                    dataPackage.RequestedOperation = DataPackageOperation.Copy;
                    dataPackage.SetText(copyText);
                    Clipboard.SetContent(dataPackage);
                }

                _appAnalytics.CaptureCustomEvent("Copy Events",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Copy Whole Session" }
                   });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void ShareAll()
        {
            var user = await _settingsService.GetUser();

            IsShareAll = true;
            DataTransferManager.ShowShareUI();
            _appAnalytics.CaptureCustomEvent("History Page Events",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Single Session Shared" }
                   });
        }

        private void IncreaseFontSize()
        {
            if (TranscriptionsFontSize <= 34)
            {
                TranscriptionsFontSize += 2;
                _settingsService.TranscriptionsFontSize = TranscriptionsFontSize;
            }
        }

        private void DecreaseFontSize()
        {
            if (TranscriptionsFontSize >= 10)
            {
                TranscriptionsFontSize -= 2;
                _settingsService.TranscriptionsFontSize = TranscriptionsFontSize;
            }
        }

        private RelayCommand _copyAllCommand = null;
        public RelayCommand CopyAllCommand
        {
            get
            {
                return _copyAllCommand ?? (_copyAllCommand = new RelayCommand(() => { CopyAll(); }));
            }
        }

        private RelayCommand _shareAllCommand = null;
        public RelayCommand ShareAllCommand
        {
            get
            {
                return _shareAllCommand ?? (_shareAllCommand = new RelayCommand(() => { ShareAll(); }));
            }
        }

        private RelayCommand _zoomOut = null;

        public RelayCommand ZoomOut
        {
            get
            {
                return _zoomOut ?? (_zoomOut = new RelayCommand(() => { DecreaseFontSize(); }));
            }
        }

        private RelayCommand _zoomIn = null;

        public RelayCommand ZoomIn
        {
            get
            {
                return _zoomIn ?? (_zoomIn = new RelayCommand(() => { IncreaseFontSize(); }));
            }
        }
    }
}
