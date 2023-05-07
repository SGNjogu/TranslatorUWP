using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.Core.DTO;
using SpeechlyTouch.Core.Events;
using SpeechlyTouch.Core.Services.TranslationProviders;
using SpeechlyTouch.Core.Services.TranslationProviders.Events;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.Voices;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Models;
using SpeechlyTouch.Services;
using SpeechlyTouch.Services.Audio;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.CognitiveService;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Services.SignalR;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using ColorConverter = Microsoft.Toolkit.Uwp.Helpers.ColorHelper;
using CoreLanguageService = SpeechlyTouch.Core.Services.Languages.ILanguagesService;
using ILanguagesService = SpeechlyTouch.Services.Languages.ILanguagesService;
using SessionTag = SpeechlyTouch.Models.SessionTag;
using Timer = System.Timers.Timer;

namespace SpeechlyTouch.ViewModels
{
    public class TranslationViewModel : ObservableObject
    {
        private bool _isSwitchEnabled;
        public bool IsSwitchEnabled
        {
            get { return _isSwitchEnabled; }
            set { SetProperty(ref _isSwitchEnabled, value); }
        }
        private bool _isTranslating;
        public bool IsTranslating
        {
            get { return _isTranslating; }
            set { SetProperty(ref _isTranslating, value); }
        }

        private bool _isSingleDevice;
        public bool IsSingleDevice
        {
            get { return _isSingleDevice; }
            set
            { SetProperty(ref _isSingleDevice, value); }
        }

        private Visibility _progressRingVisibility;
        public Visibility ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set { SetProperty(ref _progressRingVisibility, value); }
        }

        private Visibility _speakIconVisibility;
        public Visibility SpeakIconVisibility
        {
            get { return _speakIconVisibility; }
            set { SetProperty(ref _speakIconVisibility, value); }
        }

        private string _translationStateText;
        public string TranslationStateText
        {
            get { return _translationStateText; }
            set { SetProperty(ref _translationStateText, value); }
        }

        private bool IsCopyPasteEnabled { get; set; }

        private int _participantOneWordsPerMinute;
        public int ParticipantOneWordsPerMinute
        {
            get { return _participantOneWordsPerMinute; }
            set { SetProperty(ref _participantOneWordsPerMinute, value); }
        }

        private int _participantTwoWordsPerMinute;
        public int ParticipantTwoWordsPerMinute
        {
            get { return _participantTwoWordsPerMinute; }
            set { SetProperty(ref _participantTwoWordsPerMinute, value); }
        }

        private string _errorInfoGlyph;
        public string ErrorInfoGlyph
        {
            get { return _errorInfoGlyph; }
            set { SetProperty(ref _errorInfoGlyph, value); }
        }

        private Visibility _errorInfoVisibility = Visibility.Collapsed;
        public Visibility ErrorInfoVisibility
        {
            get { return _errorInfoVisibility; }
            set { SetProperty(ref _errorInfoVisibility, value); }
        }

        private Brush _errorInfoBrush;
        public Brush ErrorInfoBrush
        {
            get { return _errorInfoBrush; }
            set { SetProperty(ref _errorInfoBrush, value); }
        }

        private string _errorInfoMessage;
        public string ErrorInfoMessage
        {
            get { return _errorInfoMessage; }
            set { SetProperty(ref _errorInfoMessage, value); }
        }

        private string _switchPersonText;
        public string SwitchPersonText
        {
            get { return _switchPersonText; }
            set { SetProperty(ref _switchPersonText, value); }
        }

        private Visibility _switchButtonVisibility = Visibility.Collapsed;
        public Visibility SwitchButtonVisibility
        {
            get { return _switchButtonVisibility; }
            set { SetProperty(ref _switchButtonVisibility, value); }
        }

        private string _speakerGlyph;
        public string SpeakerGlyph
        {
            get { return _speakerGlyph; }
            set { SetProperty(ref _speakerGlyph, value); }
        }

        private string _speakerStatusText;
        public string SpeakerStatusText
        {
            get { return _speakerStatusText; }
            set { SetProperty(ref _speakerStatusText, value); }
        }

        private string _microphoneGlyph;
        public string MicrophoneGlyph
        {
            get { return _microphoneGlyph; }
            set { SetProperty(ref _microphoneGlyph, value); }
        }

        private string _microphoneStatusText;
        public string MicrophoneStatusText
        {
            get { return _microphoneStatusText; }
            set { SetProperty(ref _microphoneStatusText, value); }
        }

        private bool _isSpeakerMute;
        public bool IsSpeakerMute
        {
            get { return _isSpeakerMute; }
            set
            {
                SetProperty(ref _isSpeakerMute, value);
                ChangeSpeakerStatus();
            }
        }

        private bool _isMicrophoneMute;
        public bool IsMicrophoneMute
        {
            get { return _isMicrophoneMute; }
            set
            {
                SetProperty(ref _isMicrophoneMute, value);
                ChangeMicrophoneStatus();
            }
        }

        private Brush _speakerStatusBrush;
        public Brush SpeakerStatusBrush
        {
            get { return _speakerStatusBrush; }
            set
            {
                SetProperty(ref _speakerStatusBrush, value);
            }
        }

        private Brush _microphoneStatusBrush;
        public Brush MicrophoneStatusBrush
        {
            get { return _microphoneStatusBrush; }
            set
            {
                SetProperty(ref _microphoneStatusBrush, value);
            }
        }

        public Brush _speakerOnBrush = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        public Brush _speakerMuteBrush = new SolidColorBrush(ColorConverter.ToColor("#000000"));

        public Brush _microphoneOnBrush = new SolidColorBrush(ColorConverter.ToColor("#b624c1"));
        public Brush _microphoneMuteBrush = new SolidColorBrush(ColorConverter.ToColor("#000000"));

        /// <summary>
        /// I/O Devices
        /// </summary>
        private InputDevice _participantOneInputDevice;
        private OutputDevice _participantOneOutputDevice;
        private InputDevice _participantTwoInputDevice;
        private OutputDevice _participantTwoOutputDevice;
        private List<Participant> _participants;
        private Participant _currentParticipant;
        private AudioProfile _participantOneAudioProfile;
        private AudioProfile _participantTwoAudioProfile;
        private List<string> _candidateLanguageCodes;

        private string _participantOneLanguageCode;
        private string _participantTwoLanguageCode;
        private string _participantOneWavFilePath;
        private string _participantTwoWavFilePath;

        private string _sessionNumber;
        public string SessionNumber
        {
            get { return _sessionNumber; }
            set
            {
                SetProperty(ref _sessionNumber, value);
            }
        }


        // participant1
        private int _wordsPerMinuteTipCounterP1 = 0;
        private int _wordsPerMinuteCounterP1 = 0;
        private double _wordsPerMinuteTotalP1 = 0;
        private double _wordsPerMinuteAverageP1 = 0;

        // participant2
        private int _wordsPerMinuteTipCounterP2 = 0;
        private int _wordsPerMinuteCounterP2 = 0;
        private double _wordsPerMinuteTotalP2 = 0;
        private double _wordsPerMinuteAverageP2 = 0;

        private Timer _audioNotDetectedTimer;

        private long _startTime;
        private bool _canStoreData;
        private DataService.Models.Session _currentSession;
        private EmailDialog _emailDialog;
        private AudioNotDetectedDialog _audioNotDetectedDialog;
        private AppCloseDialog _appCloseDialog;
        private FeedbackStarRatingDialog _feedbackStarRatingDialog;

        private string _errorGlyph = "\uE7BA";
        private string _infoGlyph = "\uE946";
        private Brush _errorBrush = new SolidColorBrush(Colors.Red);
        private Brush _infoBrush = new SolidColorBrush(ColorConverter.ToColor("#02175d"));
        private bool _closeWindow = false;

        private List<Core.Domain.Language> AutoDetectLanguages { get; set; }
        List<string> ParticipantLanguageCodes;

        private string _currentSessionName;
        private ObservableCollection<string> _currentSessionCustomTags;
        private ObservableCollection<SessionTag> _currentSessionSessionTags;
        private SessionMetaDataDialog _sessionMetaDataDialog;
        private OrgQuestionsDialog _orgQuestionsDialog;


        private Timer _translationTimer;
        int _timeCounterSeconds = 0;

        private string _timeCounter;
        public string TimeCounter
        {
            get { return _timeCounter; }
            set
            {
                SetProperty(ref _timeCounter, value);
            }
        }

        private bool _isUrdu { get; set; } = false;
        public bool CannotBeAutoDetected { get; set; } = true;
        public bool IsAutoDetectSession { get; set; } = false;
        private bool IsEnabledEnhancements { get; set; } = false;
        private List<string> CandidateLanguages { get; set; } = new List<string>();

        private int? _maxUserSessionTime;

        private string AccessKey;
        private string ApiRegion;

        private ResourceLoader _resourceLoader;
        private ITranslationHub _translationHub;
        private readonly ISettingsService _settingsService;
        private readonly IAudioService _audioService;
        private readonly CoreLanguageService _coreLanguageService;
        private readonly ILanguagesService _languagesService;
        private readonly IVoicesService _voicesService;
        private readonly ISessionService _sessionService;
        private readonly ICrashlytics _crashlytics;
        private readonly IDataService _dataService;
        private readonly ICognitiveServicesHelper _cognitiveServicesHelper;
        private readonly IPushDataService _pushDataService;
        private readonly IPullDataService _pullDataService;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IAuthService _authService;
        private readonly ISignalRService _signalRService;
        private readonly IMicrosoftTextToSpeechProvider _microsoftTextToSpeechProvider;
        private readonly IDialogService _dialogService;

        public TranslationViewModel
            (
            ISettingsService settingsService,
            IAudioService audioService,
            CoreLanguageService coreLanguageService,
            ILanguagesService languagesService,
            IVoicesService voicesService,
            ISessionService sessionService,
            ICrashlytics crashlytics,
            IDataService dataService,
            ICognitiveServicesHelper cognitiveServicesHelper,
            IPushDataService pushDataService,
            IPullDataService pullDataService,
            IAppAnalytics appAnalytics,
            IAuthService authService,
            ISignalRService signalRService,
            IMicrosoftTextToSpeechProvider microsoftTextToSpeechProvider,
            IDialogService dialogService
            )
        {
            _settingsService = settingsService;
            _audioService = audioService;
            _coreLanguageService = coreLanguageService;
            _languagesService = languagesService;
            _voicesService = voicesService;
            _sessionService = sessionService;
            _crashlytics = crashlytics;
            _dataService = dataService;
            _cognitiveServicesHelper = cognitiveServicesHelper;
            _pushDataService = pushDataService;
            _pullDataService = pullDataService;
            _appAnalytics = appAnalytics;
            _authService = authService;
            _signalRService = signalRService;
            _dialogService = dialogService;
            _microsoftTextToSpeechProvider = microsoftTextToSpeechProvider;
            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            _canStoreData = true;
            ProgressRingVisibility = Visibility.Collapsed;
            SpeakIconVisibility = Visibility.Collapsed;
            IsSwitchEnabled = true;

            _currentSessionName = string.Empty;
            _currentSessionCustomTags = new ObservableCollection<string>();
            _currentSessionSessionTags = new ObservableCollection<SessionTag>();

            LoadDialogs();

            TimeCounter = "00:00";
            InitializeAutoDetectLanguages();

            StrongReferenceMessenger.Default.Register<EmailMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<SessionMetadataMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<ConnectivityMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<PermissionsMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<AutoDetectMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<OrgQuestionsMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<FeedbackDialogMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            _ = Initialize();
        }

        private void LoadDialogs()
        {
            _emailDialog = new EmailDialog();
            _audioNotDetectedDialog = new AudioNotDetectedDialog();
            _appCloseDialog = new AppCloseDialog();
            _sessionMetaDataDialog = new SessionMetaDataDialog();
            _orgQuestionsDialog = new OrgQuestionsDialog();
            _feedbackStarRatingDialog = new FeedbackStarRatingDialog();
        }

        private void HandleMessage(FeedbackDialogMessage message)
        {
            if (message.CloseStarRatingFeedback)
            {
                _feedbackStarRatingDialog.Hide();
            }
        }
        private async void HandleMessage(OrgQuestionsMessage message)
        {
            if (message != null && !string.IsNullOrEmpty(message.TranslateQuestion))
            {
                await AddToTranslation(message.TranslateQuestion, message.LanguageCode);
            }
        }

        private void HandleMessage(AutoDetectMessage message)
        {
            if (message.TranslationOpen)
            {
                if (message.candidateLanguages != null && message.candidateLanguages.Count > 0)
                {
                    CandidateLanguages = message.candidateLanguages;
                }
            }
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                _emailDialog = new EmailDialog();
                _audioNotDetectedDialog = new AudioNotDetectedDialog();
                _appCloseDialog = new AppCloseDialog();
                _sessionMetaDataDialog = new SessionMetaDataDialog();
                _orgQuestionsDialog = new OrgQuestionsDialog();
                _feedbackStarRatingDialog = new FeedbackStarRatingDialog();
            }
        }

        private void HandleMessage(PermissionsMessage message)
        {
            if (message.MicPermissionDenied)
            {
                NavigationService.GoBack();
            }
        }

        private async void HandleMessage(SessionMetadataMessage message)
        {
            try
            {
                _currentSessionCustomTags = message.CustomTags.Count != 0 ? message.CustomTags : new ObservableCollection<string>();

                _currentSessionName = !string.IsNullOrEmpty(message.SessionName) ? message.SessionName : "";

                _currentSessionSessionTags = message.SessionTags.Count != 0 ? message.SessionTags : new ObservableCollection<SessionTag>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void HandleMessage(ConnectivityMessage message)
        {
            try
            {
                if (message.ConnectionLost)
                    ShowErrorInfo(isError: true, message: _resourceLoader.GetString("ConenctivityStatus_ConnectionLost"), isSticky: true);
                if (message.ConnectionSlow)
                    ShowErrorInfo(isError: true, message: _resourceLoader.GetString("ConenctivityStatus_ConnectionUnstable"), displaySeconds: 5000);
                if (message.ConnectionStable || message.ConnectionPresent)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ErrorInfoVisibility = Visibility.Collapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void HandleMessage(EmailMessage message)
        {
            if (message.CloseEmailPopup)
                CloseSession(message.SendEmail, message.EmailingAddress);
        }

        private async void HandleMessage(NavigationMessage message)
        {
            try
            {
                if (message.InitializeSession)
                    await Initialize();
                if (message.ContinueTranslation)
                    ContinueTransLation();
                if (message.StopTranslation)
                {
                    _audioNotDetectedDialog.Hide();
                    await StopTranslation();
                }
                if (message.CloseConfirmAppCloseDialog)
                    _appCloseDialog.Hide();
                if (message.RegisterAppCloseEvent)
                    RegisterAppCloseEvent();
                if (message.UnRegisterAppCloseEvent)
                    UnRegisterAppCloseEvent();
                if (message.CloseSessionMetadataDialog)
                {
                    _sessionMetaDataDialog.Hide();
                    await _dialogService.ShowDialog(_emailDialog);
                }
                if (message.CloseQuestionsDialog)
                {
                    _orgQuestionsDialog.Hide();
                    _orgQuestionsDialog = null;
                    _orgQuestionsDialog = new OrgQuestionsDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task AddToTranslation(string text, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(AccessKey) || string.IsNullOrWhiteSpace(ApiRegion)) return;

            await _microsoftTextToSpeechProvider.Translate(
                AccessKey,
                ApiRegion,
                languageCode,
                text,
                _participantTwoLanguageCode,
                _participantOneLanguageCode,
                _participants.FirstOrDefault(c => c.SelectedLanguage.Code == _participantOneLanguageCode));
        }

        private void InitializeAutoDetectLanguages()
        {
            AutoDetectLanguages = _coreLanguageService.GetAutoDetectSupportedLanguages().ToList();
        }
        private void ClearSessionMetadata()
        {
            _currentSessionName = string.Empty;
            _currentSessionCustomTags.Clear();
            _currentSessionSessionTags.Clear();
        }

        private async void CloseSession(bool sendEmail, string emailingAddress)
        {
            try
            {
                StrongReferenceMessenger.Default.Send(new FeedbackDialogMessage
                {
                    SessionNumber = SessionNumber,
                    Feedbacktitle = _resourceLoader.GetString("SessionFeedbackTitle"),
                    Feedbackdescription = _resourceLoader.GetString("SessionFeedbackPrompt"),
                    FeedbackType = Enums.FeedbackType.Translation
                });
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    if (_currentSession != null)
                    {
                        await UpdateSession(sendEmail, emailingAddress).ConfigureAwait(false);
                    }
                });

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    _emailDialog.Hide();
                    NavigationService.GoBack();
                });
                await _dialogService.ShowDialog(_feedbackStarRatingDialog);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task Initialize()
        {
            try
            {
                TranslationStateText = _resourceLoader.GetString("TranslationPage_SettingUpState");
                ProgressRingVisibility = Visibility.Visible;
                SpeakIconVisibility = Visibility.Collapsed;
                SwitchButtonVisibility = Visibility.Collapsed;
                SessionNumber = "";
                IsEnabledEnhancements = false;

                _closeWindow = false;
                var organizationSettings = await _dataService.GetOneOrganizationSettingsAsync();

                IsCopyPasteEnabled = organizationSettings != null ? organizationSettings.CopyPasteEnabled : true;
                IsEnabledEnhancements = organizationSettings != null ? organizationSettings.EnableAudioEnhancement : false;

                _audioNotDetectedTimer = new Timer();
                _audioNotDetectedTimer.Interval = TimeSpan.FromSeconds(60).TotalMilliseconds;
                _audioNotDetectedTimer.Elapsed += AudioNotDetectedTimer_Elapsed;

                _translationTimer = new Timer();
                _translationTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
                _translationTimer.Elapsed += TranslationTimer_Elapsed;

                GetTranslationLanguages();
                await GetAudioProfiles();
                await StartTranslation();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void TranslationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _timeCounterSeconds++;

                var minutes = _timeCounterSeconds / 60;
                TimeCounter = TimeSpan.FromSeconds(_timeCounterSeconds).ToString("mm\\:ss");

                if (_maxUserSessionTime != null)
                {
                    if (minutes >= _maxUserSessionTime)
                    {
                        _translationTimer.Elapsed -= TranslationTimer_Elapsed;
                        ShowErrorInfo(isError: false, _resourceLoader.GetString("TranslationLimitReachedError"), 5000, closeWindow: false);
                        await LimiteReached();
                    }
                }
            });
        }

        private async Task LimiteReached()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Task.Delay(5000);
            });
            await StopTranslationSession();
        }

        private async Task ShowSessionMetadataDialog()
        {
            await _dialogService.ShowDialog(_sessionMetaDataDialog);

        }

        private async Task GetAudioProfiles()
        {
            try
            {
                _participantOneInputDevice = await _audioService.ParticipantOneInputDevice();
                _participantOneOutputDevice = await _audioService.ParticipantOneOutputDevice();

                _participantTwoInputDevice = await _audioService.ParticipantTwoInputDevice();
                _participantTwoOutputDevice = await _audioService.ParticipantTwoOutputDevice();

                if (_participantTwoInputDevice == null || _participantTwoOutputDevice == null)
                {
                    IsSingleDevice = true;
                }
                else
                {
                    IsSingleDevice = false;
                }

                IsSpeakerMute = false;
                IsMicrophoneMute = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void GetTranslationLanguages()
        {
            _participantOneLanguageCode = _settingsService.DefaultTranslationLanguageCode;
            _participantTwoLanguageCode = _settingsService.TargetTranslationLanguageCode;
        }

        private async Task StartTranslation()
        {
            try
            {
                var userSettings = await _settingsService.GetUser();

                //Send a SignalR Message to track this session
                await _signalRService.ConnectSignalR(_authService.IdToken);
                _signalRService.SignalRMessageReceived += SignalRServiceMessageReceived;
                await _signalRService.SendSignalRMessage(new SignalRTranslateMessage { UserEmail = userSettings.UserEmail });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void SignalRServiceMessageReceived(SignalRTranslateMessage obj)
        {
            _signalRService.SignalRMessageReceived -= SignalRServiceMessageReceived;
            if (IsTranslating)
            {
                return;
            }

            if (obj.ConnectionId == _signalRService.ConnectionId)
            {
                if (obj.CanTranslate)
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await InitializeTranslationSession();
                    });
                else
                {
                    _signalRService.SignalRMessageReceived -= SignalRServiceMessageReceived;
                    ShowErrorInfo(isError: true, _resourceLoader.GetString("TranslationLicenceUseError"), 5000, closeWindow: true);
                }
            }
        }

        private async Task StopTranslation()
        {
            try
            {
                await StopTranslationSession();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task InitializeTranslationSession()
        {
            try
            {
                var hasMicPermission = await PermissionsHelper.MicrophoneAccess();

                if (hasMicPermission)
                {
                    IsSwitchEnabled = true;
                    var userSettings = await _settingsService.GetUser();
                    var usage = await _dataService.GetUsageLimit();

                    bool hasUnlimitedLicense = usage.OrganizationLicensingType.ToLower() == "postpaid";
                    if (usage.OrganizationTranslationLimitExceeded == true && !hasUnlimitedLicense)
                    {
                        //Show user error about exceeding translation minutes
                        //Reset translation button UI

                        TranslationStateText = "Error";
                        ProgressRingVisibility = Visibility.Collapsed;
                        SpeakIconVisibility = Visibility.Visible;

                        ShowErrorInfo(isError: true, _resourceLoader.GetString("TranslationTimeDepletedError"), 5000, closeWindow: true);
                        await ForceStopSession();
                        return;
                    }
                    _maxUserSessionTime = usage.UserMaxSessionTime;
                    ParticipantLanguageCodes = new List<string>();

                    if (userSettings.DataConsentStatus == true)
                    {
                        //#if (DEBUG || STAGING)
                        //#else
                        if (usage.OrganizationStorageLimitExceeded && !hasUnlimitedLicense)
                        {
                            _canStoreData = false;
                        }
                        //#endif
                    }
                    else
                    {
                        _canStoreData = false;
                    }

                    TimeCounter = "00:00";
                    _timeCounterSeconds = 0;

                    SetParticipants();

                    if (_participants.Any())
                    {
                        _currentParticipant = _participants.First();
                    }
                    else
                    {
                        Crashes.TrackError(new Exception("Error setting up participants"), attachments: await _crashlytics.Attachments());
                        return;
                    }

                    //Check if the endpoint from the previous session was deallocated. If true, deallocate it and delete from database
                    await _cognitiveServicesHelper.DeleteCognitiveServicesEndpointId();
                    //Allocate new endpoint for the session and save endpoint Id to database
                    var endpoint = await _cognitiveServicesHelper.GetAccessKeyAndRegionAsync();
                    if (endpoint != null)
                    {
                        AccessKey = endpoint.AccessKey;
                        ApiRegion = endpoint.Region;
                    }

                    _isUrdu = false;
                    _translationHub = new MicrosoftTranslationHub(_coreLanguageService, _voicesService, _microsoftTextToSpeechProvider);
                    //if (_participantOneLanguageCode == "ur-PK" || _participantTwoLanguageCode == "ur-PK")
                    //{
                    //    _isUrdu = true;
                    //    _translationHub = new GoogleTranslationHub(_coreLanguageService, _voicesService, endpoint.AccessKey, endpoint.Region);
                    //}
                    //else
                    //{
                    //    _isUrdu = false;
                    //    _translationHub = new MicrosoftTranslationHub(_coreLanguageService, _voicesService, _microsoftTextToSpeechProvider);
                    //}

                    //Check if languages can be auto-detected
                    CannotBeAutoDetected = true;
                    List<bool> autoDetectStatus = new List<bool>();
                    foreach (var code in ParticipantLanguageCodes)
                    {
                        autoDetectStatus.Add(AutoDetectLanguages.Any(l => l.Code == code));
                    }

                    CannotBeAutoDetected = autoDetectStatus.Contains(false);

                    //Check Profanity settings
                    var organizationSettings = await _dataService.GetOrganizationSettingsAsync();

                    bool deviceEnhancementsEnabled = EnableDeviceEnhancements(_participantOneAudioProfile, _participantTwoAudioProfile);

                    if (organizationSettings != null && organizationSettings.Any())
                    {
                        _translationHub.AllowExplicitContent = organizationSettings.FirstOrDefault().AllowExplicitContent;
                        _translationHub.EnableAudioEnhancement = organizationSettings.FirstOrDefault().EnableAudioEnhancement && deviceEnhancementsEnabled;
                    }
                    else
                    {
                        _translationHub.AllowExplicitContent = false;
                    }

                    _translationHub.PartialResultReady += OnPartialResultReady;
                    _translationHub.TranscriptionResultReady += OnTranscriptionResultReady;
                    _translationHub.TranslationCancelled += OnTranslationCancelled;

                    List<string> participantLanguageCodes = new List<string>();

                    _startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    if (_canStoreData)
                    {
                        var audioDir = Constants.GetRecordingsPath();
                        _participantOneWavFilePath = Path.Combine(audioDir, $"{_startTime}.wav"); // participantOneWavFilePath
                        FileDirectoryHelper.CreateDirectoryIfNotExists(_participantOneWavFilePath);

                        if (!IsSingleDevice)
                        {
                            _participantTwoWavFilePath = Path.Combine(audioDir, $"{_startTime}_Translated.wav"); // participantTwoWavFilePath
                        }
                    }

                    string accessKey;

                    if (_isUrdu)
                    {
                        accessKey = Constants.GoogleTranslateCredentials;
                    }
                    else
                    {
                        accessKey = endpoint.AccessKey;
                    }

                    if (!CannotBeAutoDetected)
                    {
                        foreach (var code in ParticipantLanguageCodes)
                        {
                            CandidateLanguages.Add(code);
                        }
                    }

                    if (CandidateLanguages != null && CandidateLanguages.Any())
                    {
                        await _translationHub.StartTranslationAsync
                        (
                        _participants,
                        accessKey,
                        endpoint.Region,
                        IsSingleDevice,
                        _participantOneWavFilePath,
                        _participantTwoWavFilePath,
                        CandidateLanguages
                        );
                    }
                    else
                    {
                        await _translationHub.StartTranslationAsync
                        (
                        _participants,
                        accessKey,
                        endpoint.Region,
                        IsSingleDevice,
                        _participantOneWavFilePath,
                        _participantTwoWavFilePath
                        );
                    }

                    await CreateSession().ConfigureAwait(true);

                    IsTranslating = true;

                    if (IsSingleDevice)
                    {
                        if (CannotBeAutoDetected)
                        {
                            var languages = await _languagesService.GetSupportedLanguagesAsync();
                            var participantOneLanguage = languages.FirstOrDefault(s => s.Code == _currentParticipant.SelectedLanguage.Code).DisplayName;
                            TranslationStateText = $"{_resourceLoader.GetString("TranslationPage_SpeakIn")} {participantOneLanguage}";
                            SwitchPersonText = _resourceLoader.GetString("TranslationPage_SwitchToPersonTwo");
                            SwitchButtonVisibility = Visibility.Visible;
                        }
                        else if (!_isUrdu)
                        {
                            SwitchButtonVisibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        SwitchButtonVisibility = Visibility.Collapsed;
                    }

                    TranslationStateText = _resourceLoader.GetString("TranslationPage_TranslationReadyState");
                    ProgressRingVisibility = Visibility.Collapsed;
                    SpeakIconVisibility = Visibility.Visible;
                    _audioNotDetectedTimer.Start();
                    _translationTimer.Start();

                    IsMicrophoneMute = false;
                    IsSpeakerMute = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                await ForceStopSession();
            }
        }

        private bool EnableDeviceEnhancements(AudioProfile personOneAudioProfile, AudioProfile personTwoAudioProfile)
        {
            if (IsSingleDevice && !personOneAudioProfile.IsJabra)
            {
                return true;
            }
            else if (!IsSingleDevice && !personOneAudioProfile.IsJabra && !personTwoAudioProfile.IsJabra)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void OnPartialResultReady(TranslationResult obj)
        {
            try
            {
                var participantIndex = _participants.FindIndex(p => p.Guid == obj.Guid);

                if (IsSingleDevice && !CannotBeAutoDetected && !_isUrdu)
                {
                    if (obj.IsPersonOne)
                    {
                        ParticipantPartialChat(obj, "Person 1");
                    }
                    else
                    {
                        ParticipantPartialChat(obj, "Person 2");
                    }
                }
                else
                {
                    if (participantIndex == 0)
                    {
                        ParticipantPartialChat(obj, "Person 1");
                    }

                    if (participantIndex == 1)
                    {
                        ParticipantPartialChat(obj, "Person 2");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void SetParticipants()
        {
            try
            {
                ParticipantOneWordsPerMinute = 0;
                ParticipantTwoWordsPerMinute = 0;

                _participants = new List<Participant>();
                var languages = _coreLanguageService.GetSupportedLanguages();

                var participantOneLanguage = languages.FirstOrDefault(c => c.Code == _participantOneLanguageCode);
                var participantTwoLanguage = languages.FirstOrDefault(c => c.Code == _participantTwoLanguageCode);

                _participantOneAudioProfile = new AudioProfile { InputDevice = _participantOneInputDevice, OutputDevice = _participantOneOutputDevice, Name = "Customized Setup" };

                if (IsSingleDevice)
                {
                    if (participantOneLanguage != null && participantTwoLanguage != null && _participantOneAudioProfile != null)
                    {
                        _participants.Add(new Participant { Guid = Guid.NewGuid(), SelectedLanguage = participantOneLanguage, AudioProfile = _participantOneAudioProfile });
                        _participants.Add(new Participant { Guid = Guid.NewGuid(), SelectedLanguage = participantTwoLanguage, AudioProfile = _participantOneAudioProfile });
                    }
                }
                else
                {
                    _participantTwoAudioProfile = new AudioProfile { InputDevice = _participantTwoInputDevice, OutputDevice = _participantTwoOutputDevice, Name = "Customized Setup" };

                    if (participantOneLanguage != null && participantTwoLanguage != null && _participantOneAudioProfile != null && _participantTwoAudioProfile != null)
                    {
                        _participants.Add(new Participant { Guid = Guid.NewGuid(), SelectedLanguage = participantOneLanguage, AudioProfile = _participantOneAudioProfile });
                        _participants.Add(new Participant { Guid = Guid.NewGuid(), SelectedLanguage = participantTwoLanguage, AudioProfile = _participantTwoAudioProfile });
                    }
                }

                foreach (var participant in _participants)
                {
                    ParticipantLanguageCodes.Add(participant.SelectedLanguage.Code);
                    participant.AudioProfile.InputDevice.InputStateChanged += OnInputStateChanged;
                    participant.AudioProfile.OutputDevice.OutputStateChanged += OnOutputStateChanged;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task CreateSession()
        {
            try
            {
                SessionNumber sessionNumber = new SessionNumber() { ReferenceNumber = "N/A" };
                sessionNumber = await _sessionService.GetSessionNumber(_authService.IdToken).ConfigureAwait(true);

                SessionNumber = sessionNumber.ReferenceNumber;

                var user = await _settingsService.GetUser();

                //TODO logout user
                if (user.UserIntID == null)
                {
                    //await _authenticationService.Logout();
                    //_dialogService.ShowDialog(_loginViewModel);
                    return;
                }

                var rawStart = DateTimeOffset.FromUnixTimeSeconds(_startTime).ToLocalTime();
                var languages = _coreLanguageService.GetSupportedLanguages();

                DataService.Models.Session session = new DataService.Models.Session()
                {
                    SessionNumber = sessionNumber.ReferenceNumber,
                    OrganizationId = user.OrganizationId,
                    StartTime = _startTime,
                    RecordDate = rawStart.ToString("d"),
                    RawStartTime = rawStart.ToString("t"),
                    EndTime = _startTime + 1,
                    RawEndTime = string.Empty,
                    UserId = (int)user.UserIntID,
                    SoftwareVersion = Constants.GetSoftwareVersion(),
                    SourceLangISO = _participantOneLanguageCode,
                    TargetLangIso = _participantTwoLanguageCode,
                    SourceLanguage = languages.FirstOrDefault(c => c.Code == _participantOneLanguageCode).Name,
                    TargeLanguage = languages.FirstOrDefault(c => c.Code == _participantTwoLanguageCode).Name,
                    Duration = string.Empty,
                    IsSingleDeviceSession = IsSingleDevice,
                    ClientType = EnumsConverter.ConvertToString(ClientType.IOT)
                };

                _currentSession = await _dataService.AddItemAsync<DataService.Models.Session>(session).ConfigureAwait(true);
                await PersistDevicesToDatabase().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        public async Task CreateSessionTags()
        {
            try
            {
                if (_currentSessionSessionTags != null && _currentSessionSessionTags.Any())
                {
                    var sessionTags = new List<DataService.Models.SessionTag>();

                    foreach (var sessionTag in _currentSessionSessionTags)
                    {
                        sessionTags.Add(new DataService.Models.SessionTag
                        {
                            OrganizationId = _currentSession.OrganizationId,
                            SessionId = _currentSession.ID,
                            TagValue = sessionTag.TagValue,
                            OrganizationTagId = sessionTag.OrganizationTagId
                        });
                    }

                    await _dataService.CreateSessionTags(sessionTags, _currentSession.ID);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task StopTranslationSession()
        {
            try
            {
                var user = await _settingsService.GetUser();

                IsSwitchEnabled = true;
                CandidateLanguages = new List<string>();
                _signalRService.SignalRMessageReceived -= SignalRServiceMessageReceived;
                await _signalRService.DisconnectSignalR();

                if (_closeWindow)
                    NavigationService.GoBack();

                TranslationStateText = "Stopping session..";
                ProgressRingVisibility = Visibility.Visible;
                SpeakIconVisibility = Visibility.Collapsed;
                CannotBeAutoDetected = true;

                _audioNotDetectedTimer.Stop();
                _translationTimer.Stop();
                _timeCounterSeconds = 0;
                ParticipantOneWordsPerMinute = 0;
                ParticipantTwoWordsPerMinute = 0;

                await _cognitiveServicesHelper.DeleteCognitiveServicesEndpointId();

                // Unsubscribe from events
                foreach (var participant in _participants)
                {
                    participant.AudioProfile.InputDevice.InputStateChanged -= OnInputStateChanged;
                    participant.AudioProfile.OutputDevice.OutputStateChanged -= OnOutputStateChanged;
                }

                if (_translationHub != null)
                {
                    _translationHub.TranscriptionResultReady -= OnTranscriptionResultReady;
                    _translationHub.TranslationCancelled -= OnTranslationCancelled;
                    _translationHub.PartialResultReady -= OnPartialResultReady;
                }

                await _translationHub.StopTranslationAsync();
                IsTranslating = false;

                bool sessionIsValid = false;

                if (_currentSession == null)
                    sessionIsValid = false;
                else
                    sessionIsValid = await _dataService.IsSessionValid(_currentSession.ID);

                if (sessionIsValid)
                {
                    StrongReferenceMessenger.Default.Send(new HistoryMessage { RefreshHistory = true });
                    var organizationSettings = await _dataService.GetOneOrganizationSettingsAsync();
                    if (organizationSettings.EnableSessionTags)
                    {
                        await ShowSessionMetadataDialog();
                    }
                    else
                    {
                        await _dialogService.ShowDialog(_emailDialog);
                    }
                }
                else
                {
                    _appAnalytics.CaptureCustomEvent("Invalid Sessions",
           new Dictionary<string, string> {
                            { "User", user?.UserEmail },
                            { "Organization", user.Organization },
                            { "App Version", Constants.GetSoftwareVersion() } });

                    await _dataService.DeleteSessionAsync(_currentSession);
                    await DeleteLocalAudioFile(_participantOneWavFilePath);
                    await DeleteLocalAudioFile(_participantTwoWavFilePath);
                    NavigationService.GoBack();
                }

                IsMicrophoneMute = false;
                IsSpeakerMute = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                await ForceStopSession();
            }
        }

        private async Task ForceStopSession()
        {
            try
            {
                IsSwitchEnabled = true;
                CandidateLanguages = new List<string>();
                if (_translationHub != null)
                    _translationHub.ForceStop();

                _audioNotDetectedTimer.Stop();
                CannotBeAutoDetected = true;

                if (_currentSession != null)
                {
                    await _dialogService.ShowDialog(_emailDialog);
                }
                else
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _emailDialog.Hide();
                        NavigationService.GoBack();
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateSession(bool sendEmail, string emailingAddress)
        {
            try
            {
                var user = await _settingsService.GetUser();
                UpdateSessionDuration();
                _currentSession.IsEmailRequired = sendEmail;
                _currentSession.EmailingAddress = emailingAddress;
                _currentSession.SessionName = _currentSessionName;
                _currentSession.CustomTags = _currentSessionCustomTags.Count > 0 ? string.Join(",", _currentSessionCustomTags) : "";
                _currentSession = await _dataService.UpdateItemAsync<DataService.Models.Session>(_currentSession).ConfigureAwait(true);
                await CreateSessionTags().ConfigureAwait(false);
                bool sessionIsValid = await _dataService.IsSessionValid(_currentSession.ID);

                if (sessionIsValid)
                {

                    _appAnalytics.CaptureCustomEvent("Translation Session Event",
                    new Dictionary<string, string> {
                            { "SourceLanguage", _currentSession.DisplaySourceLanguage },
                            { "TargetLanguage", _currentSession.DisplayTargetLanguage },
                            { "Duration (Seconds)", _currentSession.Duration },
                            { "User", user.UserEmail },
                            { "Organization", user.Organization },
                            { "Single Device?", _currentSession.IsSingleDeviceSession ? "Yes" : "No" } });

                    if (sendEmail)
                    {
                        _appAnalytics.CaptureCustomEvent("Email session after session",
                    new Dictionary<string, string> {
                            { "SourceLanguage", _currentSession.DisplaySourceLanguage },
                            { "TargetLanguage", _currentSession.DisplayTargetLanguage },
                            { "Duration (Seconds)", _currentSession.Duration },
                            { "User", user.UserEmail },
                            { "Recepient", _currentSession.EmailingAddress },
                            { "Organization", user.Organization },
                            { "Single Device?", _currentSession.IsSingleDeviceSession ? "Yes" : "No" } });
                    }

                    if (_canStoreData)
                    {
                        Thread pushThread = new Thread(_pushDataService.BeginDataSync);
                        pushThread.Start();
                        Thread pullThread = new Thread(_pullDataService.BeginDataSync);
                        pullThread.Start();
                    }
                    else
                    {
                        // when data consent is not given, upload the session
                        // without syncing transcriptions and audiofile
                        await _pushDataService.UploadSession(_currentSession);
                    }
                }
                else
                {
                    await _dataService.DeleteSessionAsync(_currentSession);
                    await DeleteLocalAudioFile(_participantOneWavFilePath);
                    await DeleteLocalAudioFile(_participantTwoWavFilePath);
                }

                _currentSession = null;
                ClearSessionMetadata();
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

        private string UpdateSessionDuration()
        {
            var rawStart = DateTimeOffset.FromUnixTimeSeconds(_startTime).ToLocalTime();
            var endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var rawEnd = DateTimeOffset.FromUnixTimeSeconds(endTime).ToLocalTime();
            var sessionDuration = TimeSpan.FromSeconds((rawStart - rawEnd).TotalSeconds).ToString(@"m\:ss");

            _currentSession.EndTime = endTime;
            _currentSession.RawEndTime = rawEnd.ToString("t");
            _currentSession.Duration = TimeCounter != "00:00" ? TimeCounter : sessionDuration;

            var languages = _coreLanguageService.GetSupportedLanguages();


            return _currentSession.Duration;
        }

        public async Task PersistDevicesToDatabase()
        {
            try
            {
                if (IsSingleDevice)
                {
                    await InsertDevice(_participantOneAudioProfile, _currentSession.ID);
                }
                else
                {
                    await InsertDevice(_participantOneAudioProfile, _currentSession.ID);
                    await InsertDevice(_participantTwoAudioProfile, _currentSession.ID);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task InsertDevice(AudioProfile audioProfile, int sessionId)
        {
            try
            {
                DataService.Models.Device device = new DataService.Models.Device()
                {
                    SessionId = sessionId,
                    Name = "Customized Setup",
                    Certified = audioProfile.IsJabra
                };
                await _dataService.AddItemAsync<DataService.Models.Device>(device).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void OnTranslationCancelled(TranslationCancelled obj)
        {

        }

        private async void OnTranscriptionResultReady(TranslationResult obj)
        {
            try
            {
                _audioNotDetectedTimer.Stop();

                var participantIndex = _participants.FindIndex(p => p.Guid == obj.Guid);

                if (IsSingleDevice && !CannotBeAutoDetected && !_isUrdu)
                {
                    if (obj.IsPersonOne)
                    {
                        ParticipantChat(obj, "Person 1");
                        await CalculateWordsPerMinutePerson1Async(obj);
                    }
                    else
                    {
                        ParticipantChat(obj, "Person 2");
                        await CalculateWordsPerMinutePerson2Async(obj);
                    }
                }
                else
                {
                    if (participantIndex == 0)
                    {
                        ParticipantChat(obj, "Person 1");
                        await CalculateWordsPerMinutePerson1Async(obj);
                    }

                    if (participantIndex == 1)
                    {
                        ParticipantChat(obj, "Person 2");
                        await CalculateWordsPerMinutePerson2Async(obj);
                    }
                }

                _audioNotDetectedTimer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task CalculateWordsPerMinutePerson1Async(TranslationResult obj)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _wordsPerMinuteCounterP1++;
                    _wordsPerMinuteTotalP1 += obj.WordsPerMinute;
                    _wordsPerMinuteAverageP1 = _wordsPerMinuteTotalP1 / _wordsPerMinuteCounterP1;
                    ParticipantOneWordsPerMinute = (int)(double)_wordsPerMinuteAverageP1;

                    if (_wordsPerMinuteCounterP1 % 11 == 0)
                    {
                        if (_wordsPerMinuteTipCounterP1 < 2)
                        {
                            if (ParticipantOneWordsPerMinute < 80)
                            {
                                // TODO: Rename Person 1 with actual person name
                                _wordsPerMinuteTipCounterP1++;
                                ShowErrorInfo(isError: false, _resourceLoader.GetString("TranslationPage_SpeakFast"));
                            }

                            if (ParticipantOneWordsPerMinute > 120)
                            {
                                // TODO: Rename Person 1 with actual person name
                                _wordsPerMinuteTipCounterP1++;
                                ShowErrorInfo(isError: false, _resourceLoader.GetString("TranslationPage_SpeakSlow"));
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task CalculateWordsPerMinutePerson2Async(TranslationResult obj)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _wordsPerMinuteCounterP2++;
                    _wordsPerMinuteTotalP2 += obj.WordsPerMinute;
                    _wordsPerMinuteAverageP2 = _wordsPerMinuteTotalP2 / _wordsPerMinuteCounterP2;
                    ParticipantTwoWordsPerMinute = (int)(double)_wordsPerMinuteAverageP2;

                    if (_wordsPerMinuteCounterP2 % 11 == 0)
                    {
                        if (_wordsPerMinuteTipCounterP2 < 2)
                        {
                            if (ParticipantTwoWordsPerMinute < 80)
                            {
                                // TODO: Rename Person 2 with actual person name
                                _wordsPerMinuteTipCounterP2++;
                                ShowErrorInfo(isError: false, _resourceLoader.GetString("TranslationPage_SpeakFast"));
                            }

                            if (ParticipantTwoWordsPerMinute > 120)
                            {
                                // TODO: Rename Person 2 with actual person name
                                _wordsPerMinuteTipCounterP2++;
                                ShowErrorInfo(isError: false, _resourceLoader.GetString("TranslationPage_SpeakSlow"));
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void OnOutputStateChanged(object sender, OutputDeviceStateChangedEventArgs outputStateChangedEventArgs)
        {

        }

        private void OnInputStateChanged(object sender, InputDeviceStateChangedEventArgs inputStateChangedEventArgs)
        {

        }

        private async void ParticipantChat(TranslationResult result, string personIdentifier)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var originalText = result.OriginalText;
                    var translatedText = result.TranslatedText;
                    Chat chat = new Chat
                    {
                        Guid = Guid.NewGuid(),
                        Date = DateTime.Now,
                        OriginalMessage = $"{originalText}",
                        TranslatedMessage = $"{translatedText}",
                        Duration = result.Duration,
                        Message = $"Original: {originalText} \nTranslated: {translatedText}",
                        IsCopyPasteEnabled = IsCopyPasteEnabled,
                        OffsetInTicks = result.OffsetInTicks,
                    };
                    if (_currentSession != null)
                    {
                        if (personIdentifier.Equals("Person 1"))
                        {
                            chat.OriginalMessageISO = _currentSession.SourceLangISO;
                            chat.TranslatedMessageISO = _currentSession.TargetLangIso;
                            chat.IsPersonOne = true;
                        }
                        else
                        {
                            chat.OriginalMessageISO = _currentSession.TargetLangIso;
                            chat.TranslatedMessageISO = _currentSession.SourceLangISO;
                            chat.IsPersonOne = false;
                        }
                        StrongReferenceMessenger.Default.Send(new RecognizedChatMessage { IsChatList = false, Chat = chat, SessionId = _currentSession.ID, IsCopyPasteEnabled = IsCopyPasteEnabled });
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void ParticipantPartialChat(TranslationResult result, string personIdentifier)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var originalText = result.OriginalText;
                    var translatedText = result.TranslatedText;
                    Chat chat = new Chat
                    {
                        Guid = Guid.NewGuid(),
                        Date = DateTime.Now,
                        OriginalMessage = $"{originalText}",
                        TranslatedMessage = $"{translatedText}",
                        Duration = result.Duration,
                        Message = $"Original: {originalText} \nTranslated: {translatedText}",
                        IsCopyPasteEnabled = IsCopyPasteEnabled,
                        OffsetInTicks = result.OffsetInTicks,
                        IsComplete = false
                    };
                    if (_currentSession != null)
                    {
                        if (personIdentifier.Equals("Person 1"))
                        {
                            chat.OriginalMessageISO = _currentSession.SourceLangISO;
                            chat.TranslatedMessageISO = _currentSession.TargetLangIso;
                            chat.IsPersonOne = true;
                        }
                        else
                        {
                            chat.OriginalMessageISO = _currentSession.TargetLangIso;
                            chat.TranslatedMessageISO = _currentSession.SourceLangISO;
                            chat.IsPersonOne = false;
                        }
                        StrongReferenceMessenger.Default.Send(new PartialChatMessage { Chat = chat, SessionId = _currentSession.ID });
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void AudioNotDetectedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _audioNotDetectedTimer.Stop();
                StrongReferenceMessenger.Default.Send(new NavigationMessage { ShowAudioNotDetectedDialog = true });

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await _dialogService.ShowDialog(_audioNotDetectedDialog);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void ContinueTransLation()
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _audioNotDetectedDialog.Hide();
                    _audioNotDetectedTimer.Start();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void ShowErrorInfo(bool isError, string message, int displaySeconds = 3000, bool isSticky = false, bool closeWindow = false)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    _closeWindow = closeWindow;

                    if (isError)
                    {
                        ErrorInfoBrush = _errorBrush;
                        ErrorInfoGlyph = _errorGlyph;
                    }
                    else
                    {
                        ErrorInfoBrush = _infoBrush;
                        ErrorInfoGlyph = _infoGlyph;
                    }
                    ErrorInfoMessage = message;
                    ErrorInfoVisibility = Visibility.Visible;

                    if (!isSticky)
                    {
                        await Task.Delay(displaySeconds);
                        ErrorInfoVisibility = Visibility.Collapsed;
                    }

                    if (_closeWindow)
                        NavigationService.GoBack();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task CloseErrorInfoAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ErrorInfoVisibility = Visibility.Collapsed;
                if (_closeWindow)
                    NavigationService.GoBack();
            });
        }

        private async Task SwitchParticipant()
        {
            try
            {
                IsSwitchEnabled = false;
                TranslationStateText = _resourceLoader.GetString("TranslationPage_Switching");

                var switchTo = _participants.Find(p => p.Guid != _currentParticipant.Guid);
                await _translationHub.Switch(switchTo);
                _currentParticipant = switchTo;
                var languageCode = switchTo.SelectedLanguage.Code;

                if (IsSingleDevice)
                {
                    var languages = await _languagesService.GetSupportedLanguagesAsync();
                    var participantLanguage = languages.FirstOrDefault(s => s.Code == _currentParticipant.SelectedLanguage.Code).DisplayName;
                    TranslationStateText = $"{_resourceLoader.GetString("TranslationPage_SpeakIn")} {participantLanguage}";
                }

                if (languageCode == _settingsService.DefaultTranslationLanguageCode)
                    SwitchPersonText = _resourceLoader.GetString("TranslationPage_SwitchToPersonTwo");
                else
                    SwitchPersonText = _resourceLoader.GetString("TranslationPage_SwitchToPersonOne");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            finally
            {
                IsSwitchEnabled = true;
            }
        }

        private void RegisterAppCloseEvent()
        {
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnAppCloseRequest;
        }

        private void UnRegisterAppCloseEvent()
        {
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested -= OnAppCloseRequest;
        }

        private async void ChangeSpeakerStatus()
        {
            try
            {
                if (IsSpeakerMute)
                {
                    SpeakerGlyph = "\uE74F";
                    SpeakerStatusBrush = _speakerMuteBrush;
                    SpeakerStatusText = _resourceLoader.GetString("TranslationPage_SpeakerOff");

                    if (_participants != null && _participants.Any())
                    {
                        foreach (var participant in _participants)
                        {
                            if (participant.AudioOutputService != null)
                                participant.AudioOutputService.Mute();
                        }
                    }
                }
                else
                {
                    SpeakerGlyph = "\uE994";
                    SpeakerStatusBrush = _speakerOnBrush;
                    SpeakerStatusText = _resourceLoader.GetString("TranslationPage_SpeakerOn");

                    if (_participants != null && _participants.Any())
                    {
                        foreach (var participant in _participants)
                        {
                            if (participant.AudioOutputService != null)
                                participant.AudioOutputService.UnMute();
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

        private async void ChangeMicrophoneStatus()
        {
            try
            {
                if (IsMicrophoneMute)
                {
                    MicrophoneGlyph = "\uEC54";
                    MicrophoneStatusBrush = _speakerMuteBrush;
                    MicrophoneStatusText = _resourceLoader.GetString("TranslationPage_MicrophoneOff");

                    if (_participants != null && _participants.Any())
                    {
                        foreach (var participant in _participants)
                        {
                            if (participant.AudioInputService != null)
                                participant.AudioInputService.Mute();
                        }
                    }
                }
                else
                {
                    MicrophoneGlyph = "\uE720";
                    MicrophoneStatusBrush = _speakerOnBrush;
                    MicrophoneStatusText = _resourceLoader.GetString("TranslationPage_MicrophoneOn");

                    if (_participants != null && _participants.Any())
                    {
                        foreach (var participant in _participants)
                        {
                            if (participant.AudioInputService != null)
                                participant.AudioInputService.Unmute();
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

        private void MuteUnMuteSpeaker()
        {
            if (IsSpeakerMute)
                IsSpeakerMute = false;
            else
                IsSpeakerMute = true;
        }

        private void MuteUnMuteMicrophone()
        {
            if (IsMicrophoneMute)
                IsMicrophoneMute = false;
            else
                IsMicrophoneMute = true;
        }

        private async void OnAppCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs args)
        {
            args.Handled = true;

            try
            {
                if (IsTranslating)
                {
                    _orgQuestionsDialog.Hide();
                    await _dialogService.ShowDialog(_appCloseDialog);
                }
            }
            catch (Exception) { }
        }

        private async void ShowQuestions()
        {
            await _dialogService.ShowDialog(_orgQuestionsDialog);
        }

        public async void AskQuestion(int questionNumber)
        {

            if (IsTranslating)
            {
                var questions = await _dataService.GetOrgQuestionsAsync();
                var resultString = questionNumber.ToString();
                var targetQuestion = questions.FirstOrDefault(q => q.Shortcut == resultString);
                if (targetQuestion != null)
                {
                    var user = await _settingsService.GetUser();
                    _appAnalytics.CaptureCustomEvent("Organization questions events",
                          new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() },
                        {"Action", "Organization question selected" }
                           });
                    StrongReferenceMessenger.Default.Send(new OrgQuestionsMessage { TranslateQuestion = targetQuestion.Question, LanguageCode = targetQuestion.LanguageCode });
                }

            }
        }

        private RelayCommand _endSessionCommand = null;

        public RelayCommand EndSessionCommand
        {
            get
            {
                return _endSessionCommand ?? (_endSessionCommand = new RelayCommand(async () =>
                {
                    await StopTranslation();
                }));
            }
        }

        private RelayCommand _translateCommand = null;

        public RelayCommand TranslateCommand
        {
            get
            {
                return _translateCommand ?? (_translateCommand = new RelayCommand(async () => { await StartTranslation(); }));
            }
        }

        private RelayCommand _closeErrorInfoCommand = null;

        public RelayCommand CloseErrorInfoCommand
        {
            get
            {
                return _closeErrorInfoCommand ?? (_closeErrorInfoCommand = new RelayCommand(async () => { await CloseErrorInfoAsync(); }));
            }
        }

        private RelayCommand _switchParticipantCommand = null;

        public RelayCommand SwitchParticipantCommand
        {
            get
            {
                return _switchParticipantCommand ?? (_switchParticipantCommand = new RelayCommand(async () => { await SwitchParticipant(); }));
            }
        }

        private RelayCommand _muteUnMuteSpeakerCommand = null;

        public RelayCommand MuteUnMuteSpeakerCommand
        {
            get
            {
                return _muteUnMuteSpeakerCommand ?? (_muteUnMuteSpeakerCommand = new RelayCommand(() => { MuteUnMuteSpeaker(); }));
            }
        }

        private RelayCommand _muteUnMuteMicrophoneCommand = null;

        public RelayCommand MuteUnMuteMicrophoneCommand
        {
            get
            {
                return _muteUnMuteMicrophoneCommand ?? (_muteUnMuteMicrophoneCommand = new RelayCommand(() => { MuteUnMuteMicrophone(); }));
            }
        }

        private RelayCommand _questionsCommand = null;

        public RelayCommand QuestionsCommand
        {
            get
            {
                return _questionsCommand ?? (_questionsCommand = new RelayCommand(() => { ShowQuestions(); }));
            }
        }
    }
}
