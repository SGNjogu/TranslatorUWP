using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Core.Domain;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services;
using SpeechlyTouch.Services.Audio;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Connectivity;
using SpeechlyTouch.Services.FlagLanguage;
using SpeechlyTouch.Services.Internationalization;
using SpeechlyTouch.Services.Languages;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Views.ContentControls;
using SpeechlyTouch.Views.Pages;
using SpeechlyTouch.Views.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Language = SpeechlyTouch.Models.Language;

namespace SpeechlyTouch.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        public List<Language> Languages;

        private ObservableCollection<Language> _quickViewLanguages;
        public ObservableCollection<Language> QuickViewLanguages
        {
            get { return _quickViewLanguages; }
            set { SetProperty(ref _quickViewLanguages, value); }
        }

        private string _languageSearchText;
        public string LanguageSearchText
        {
            get { return _languageSearchText; }
            set
            {
                SetProperty(ref _languageSearchText, value);
                SearchLanguage(LanguageSearchText);
            }
        }

        public bool IsSearching { get; set; }

        private Visibility _noResultsVisibility;
        public Visibility NoResultsVisibility
        {
            get { return _noResultsVisibility; }
            set { SetProperty(ref _noResultsVisibility, value); }
        }

        private bool _isSelectionEnabled = true;
        public bool IsSelectionEnabled
        {
            get { return _isSelectionEnabled; }
            set { SetProperty(ref _isSelectionEnabled, value); }
        }

        private Language _selectedTargetLanguage;
        public Language SelectedTargetLanguage
        {
            get { return _selectedTargetLanguage; }
            set
            {
                SetProperty(ref _selectedTargetLanguage, value);
                if (SelectedTargetLanguage != null)
                {
                    HandleSelectedLanguage(SelectedTargetLanguage);
                }
            }
        }

        private List<DataService.Models.InternationalizationLanguage> InternationalizationLanguages { get; set; }

        private InputDevice SelectedInputDevice { get; set; }
        private OutputDevice SelectedOutputDevice { get; set; }

        private ConsentDialog consentDialog;
        private EnterPasscodeDialog enterPasscodeDialog;
        private SetupPasscodeDialog setupPasscodeDialog;
        private ChangePasscodeDialog changePasscodeDialog;
        private DevicesErrorDialog devicesErrorDialog;
        private LanguageDetectionWizard languageDetectionWizard;

        private bool IsDevicesErrorDialogOpened { get; set; }

        private readonly ILanguagesService _languagesService;
        private readonly ISettingsService _settings;
        private readonly IAudioService _audioService;
        private readonly IConnectivityService _connectivityService;
        private readonly IDataService _dataService;
        private readonly IInternationalizationService _internationalizationService;
        private readonly IFlagLanguageService _flagLanguageService;
        private ResourceLoader _resourceLoader;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IDialogService _dialogService;
        public DashboardViewModel
            (
            ILanguagesService languagesService,
            ISettingsService settings,
            IAudioService audioService,
            IConnectivityService connectivityService,
            IDataService dataService,
            IInternationalizationService internationalizationService,
            IFlagLanguageService flagLanguageService,
            IAppAnalytics appAnalytics,
            IDialogService dialogService
            )
        {
            StrongReferenceMessenger.Default.Register<PasscodeMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<DevicesMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
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

            _languagesService = languagesService;
            _settings = settings;
            _audioService = audioService;
            _connectivityService = connectivityService;
            _dataService = dataService;
            _internationalizationService = internationalizationService;
            _flagLanguageService = flagLanguageService;
            _appAnalytics = appAnalytics;
            LanguageSearchText = "";
            NoResultsVisibility = Visibility.Collapsed;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            IsSelectionEnabled = true;
            _dialogService = dialogService;

            enterPasscodeDialog = new EnterPasscodeDialog();
            setupPasscodeDialog = new SetupPasscodeDialog();
            changePasscodeDialog = new ChangePasscodeDialog();
            consentDialog = new ConsentDialog();
            devicesErrorDialog = new DevicesErrorDialog();


            LoadAudioDevices();
            RegisterShare();
            _ = InitializeIntlLanguages();
        }

        private async void HandleMessage(AutoDetectMessage message)
        {
            if (message.CloseAutoDetectPopups && message.candidateLanguages != null && message.candidateLanguages.Any())
            {
                CloseLanguageDetectionWizard();
                var assignedTargetLanguage = message.candidateLanguages.FirstOrDefault();
                SelectedTargetLanguage = Languages.FirstOrDefault(l => l.Code == assignedTargetLanguage);
                await ShowConsentDialog().ConfigureAwait(true);
                StrongReferenceMessenger.Default.Send<AutoDetectMessage>(new AutoDetectMessage { TranslationOpen = true, candidateLanguages = message.candidateLanguages });
            }
            else if (message.CloseAutoDetectPopups)
            {
                CloseLanguageDetectionWizard();
            }
        }

        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                enterPasscodeDialog = new EnterPasscodeDialog();
                setupPasscodeDialog = new SetupPasscodeDialog();
                changePasscodeDialog = new ChangePasscodeDialog();
                consentDialog = new ConsentDialog();
                devicesErrorDialog = new DevicesErrorDialog();
            }
        }

        private async Task InitializeIntlLanguages()
        {
            InternationalizationLanguages = await _dataService.GetInternationalizationLanguages();
        }

        private void RegisterShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest dataRequest = args.Request;
            dataRequest.Data.Properties.Title = "Tala" + _resourceLoader.GetString("TouchShareText");

            if (ChatViewModel.IsShareAll)
            {
                if (ChatViewModel.ChatViewChats != null && ChatViewModel.ChatViewChats.Any())
                {
                    string shareText = string.Empty;

                    foreach (var item in ChatViewModel.ChatViewChats)
                    {
                        string text = $"{item.Person}\n{item.Message}\n";
                        shareText = $"{shareText}\n{text}";
                    }

                    dataRequest.Data.SetText(shareText);
                }
                else
                {
                    dataRequest.Data.SetText(_resourceLoader.GetString("DataRequestMessage"));
                }
                ChatViewModel.IsShareAll = false;
            }

            if (ChatView.IsSingleChat)
            {
                if (ChatView.Chat != null)
                {
                    string shareText = $"{ChatView.Chat.Person}\n{ChatView.Chat.Message}\n";

                    dataRequest.Data.SetText(shareText);
                }
                else
                {
                    dataRequest.Data.SetText(_resourceLoader.GetString("DataRequestMessage"));
                }
                ChatView.IsSingleChat = false;
            }

            if (HistoryViewModel.IsFileShare)
            {
                try
                {
                    dataRequest.Data.SetStorageItems(new[] { HistoryViewModel.File }, false);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                HistoryViewModel.IsFileShare = false;
            }
        }


        private async void LoadLanguages()
        {
            Languages = await _languagesService.GetSupportedLanguagesAsync();

            string quickViewLanguages = _settings.QuickViewLanguages;

            if (string.IsNullOrEmpty(quickViewLanguages))
            {
                var quickViewList = new List<Language>()
                {
                    Languages.FirstOrDefault(l => l.Code == "fr-FR"),
                    Languages.FirstOrDefault(l => l.Code == "es-ES"),
                    Languages.FirstOrDefault(l => l.Code == "it-IT"),
                    Languages.FirstOrDefault(l => l.Code == "nl-NL"),
                    Languages.FirstOrDefault(l => l.Code == "sv-SE"),
                };

                quickViewList.RemoveAll(l => l == null);

                if(quickViewList.Count() > 4)
                {
                    QuickViewLanguages = new ObservableCollection<Language>(quickViewList);
                }
                else
                {
                    QuickViewLanguages = new ObservableCollection<Language>(Languages.Take(5));
                }
            }
            else
            {
                var quickLanguagesList = await JsonConverter.ReturnObjectFromJsonString<List<string>>(quickViewLanguages);
                GetQuickViewLanguages(quickLanguagesList);
            }
        }

        private void GetQuickViewLanguages(List<string> quickViewLanguagesList)
        {
            QuickViewLanguages = new ObservableCollection<Language>();

            foreach (var item in quickViewLanguagesList)
            {
                var language = Languages.Find(s => s.Code == item);

                if (language != null)
                {
                    QuickViewLanguages.Add(language);
                    var index = Languages.IndexOf(language);
                    var position = QuickViewLanguages.Count;
                    Languages.Remove(language);
                    Languages.Insert(position - 1, language);
                }
            }
        }

        private void SearchLanguage(string searchText)
        {
            NoResultsVisibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(searchText))
            {
                IsSearching = false;
                LoadLanguages();
                return;
            }

            IsSearching = true;

            List<Language> filteredLanguages = new List<Language>();

            for (int i = 0; i < Languages.Count; i++)
            {
                var language = Languages[i];
                if (
                    language.Code.ToLower().Contains(searchText.ToLower())
                    || language.DisplayName.ToLower().Contains(searchText.ToLower())
                    || language.EnglishName.ToLower().Contains(searchText.ToLower())
                    )
                {
                    filteredLanguages.Add(language);
                }
            }

            QuickViewLanguages = new ObservableCollection<Language>(filteredLanguages);

            if (QuickViewLanguages.Any())
                NoResultsVisibility = Visibility.Collapsed;
            else
                NoResultsVisibility = Visibility.Visible;
        }

        private async Task ShowConsentDialog()
        {
            CloseErrorDialogIfOpen();

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            bool isConnectionAvailable = _connectivityService.IsConnectionAvailable();

            if (!isConnectionAvailable)
            {
                StrongReferenceMessenger.Default.Send(new DevicesMessage { ShowDevicesErrorDialog = true, DevicesErrorMessage = resourceLoader.GetString("ConenctivityStatus_ConnectionLost") });
                return;
            }

            if (SelectedInputDevice == null || SelectedOutputDevice == null)
            {
                StrongReferenceMessenger.Default.Send(new DevicesMessage { ShowDevicesErrorDialog = true, DevicesErrorMessage = resourceLoader.GetString("DevicesErrorDialog_NoDeviceError") });
                return;
            }

            if (!_settings.IsCheckedSingleDevice)
            {
                var participantTwoSelectedInputDevice = await _audioService.ParticipantTwoInputDevice();
                var participantTwoSelectedOutputDevice = await _audioService.ParticipantTwoOutputDevice();

                if (participantTwoSelectedInputDevice == null || participantTwoSelectedOutputDevice == null)
                {
                    StrongReferenceMessenger.Default.Send(new DevicesMessage { ShowDevicesErrorDialog = true, DevicesErrorMessage = resourceLoader.GetString("DevicesErrorDialog_DualDeviceError") });
                    return;
                }
            }

            if (SelectedTargetLanguage == null)
            {
                StrongReferenceMessenger.Default.Send(new DevicesMessage { ShowDevicesErrorDialog = true, DevicesErrorMessage = resourceLoader.GetString("DashboardPage_TargetLanguageError") });
                return;
            }

            if (string.IsNullOrEmpty(_settings.DefaultTranslationLanguageCode))
            {
                StrongReferenceMessenger.Default.Send(new DevicesMessage { ShowDevicesErrorDialog = true, DevicesErrorMessage = resourceLoader.GetString("DashboardPage_DefaultLanguageError") });
                return;
            }

            CloseErrorDialogIfOpen();

            StrongReferenceMessenger.Default.Send(new NavigationMessage { InitializeSession = true });
            StrongReferenceMessenger.Default.Send(new AudioPlayerMessage { IsShowingAudioPlayer = false });
            SelectedTargetLanguage = null;
            NavigationService.Navigate(typeof(TranslationPage));
        }

        void CloseErrorDialogIfOpen()
        {
            if (SelectedInputDevice != null && SelectedOutputDevice != null)
                if (IsDevicesErrorDialogOpened)
                {
                    IsDevicesErrorDialogOpened = false;
                    _dialogService.HideDialog();
                }
        }

        private void CloseDeviceErrorDialog()
        {
            IsDevicesErrorDialogOpened = false;
            _dialogService.HideDialog();
        }

        private void CloseConsentDialog()
        {
            _dialogService.HideDialog();
        }

        private void AcceptConsent()
        {
            CloseConsentDialog();
            StrongReferenceMessenger.Default.Send(new NavigationMessage { InitializeSession = true });
            StrongReferenceMessenger.Default.Send(new AudioPlayerMessage { IsShowingAudioPlayer = false });
            NavigationService.Navigate(typeof(TranslationPage));
        }

        private async Task ShowEnterPasscodeDialog()
        {
            bool passcodeIsSet = !string.IsNullOrEmpty(_settings.Passcode);

            if (passcodeIsSet)
               await _dialogService.ShowDialog(enterPasscodeDialog);
            else
                await _dialogService.ShowDialog(setupPasscodeDialog);

            StrongReferenceMessenger.Default.Send(new PasscodeMessage { ClearPasscodeValues = true });
        }

        private async Task ShowChangePasscodeDialog()
        {
            ClosePasscodeDialogs();
            await _dialogService.ShowDialog(changePasscodeDialog);
        }

        private async void LoadAudioDevices()
        {
            await LoadAudioInputDevice();
            await LoadAudioOutputDevice();
            CloseErrorDialogIfOpen();
        }

        private async Task LoadAudioInputDevice()
        {
            SelectedInputDevice = await _audioService.ParticipantOneInputDevice();
            CloseErrorDialogIfOpen();
        }

        private async Task LoadAudioOutputDevice()
        {
            SelectedOutputDevice = await _audioService.ParticipantOneOutputDevice();
            CloseErrorDialogIfOpen();
        }

        private async void HandleSelectedLanguage(Language language)
        {
            _settings.TargetTranslationLanguageCode = language.Code;
            var splitlanguageCode = GetLanguageCode(SelectedTargetLanguage);
            if (_settings.ApplicationLanguageCode != language.Code &&
                _settings.IsEnabledAutoLanguageSwitch &&
                _settings.ApplicationLanguageCode != splitlanguageCode &&
                _settings.ApplicationLanguageCode.Substring(0, 2) !=
                splitlanguageCode && InternationalizationLanguages.Any())
            {
                if (_settings.ApplicationLanguageCode != GetLanguageCode(SelectedTargetLanguage))
                {
                    IsSelectionEnabled = false;
                    var selectedLanguage = SelectedTargetLanguage;
                    var languageCode = GetLanguageCode(language);
                    if (!string.IsNullOrEmpty(languageCode))
                    {
                        _internationalizationService.SetAppLanguage(languageCode);
                        await Task.Delay(300);
                        Frame dashboardFrame = Window.Current.Content as Frame;
                        dashboardFrame.CacheSize = 0;
                        dashboardFrame.BackStack.Clear();
                        dashboardFrame.Navigate(typeof(DashboardPage));
                        Window.Current.Content = dashboardFrame;

                        //Re-initialize dialogs
                        enterPasscodeDialog = new EnterPasscodeDialog();
                        setupPasscodeDialog = new SetupPasscodeDialog();
                        changePasscodeDialog = new ChangePasscodeDialog();
                        consentDialog = new ConsentDialog();
                        devicesErrorDialog = new DevicesErrorDialog();

                        StrongReferenceMessenger.Default.Send(new InternationalizationMessage { LanguageCode = languageCode, IsDashboardChange = true });
                        await Task.Delay(500);
                        SelectedTargetLanguage = selectedLanguage;
                        StrongReferenceMessenger.Default.Send(new ScrollToMessage { Index = QuickViewLanguages.IndexOf(SelectedTargetLanguage) });
                    }
                }
            }
            IsSelectionEnabled = true;
        }

        private string GetLanguageCode(Language language)
        {
            var code = InternationalizationLanguages.FirstOrDefault(l => l.Code == language.Code)?.Code;
            if (!string.IsNullOrEmpty(code))
            {
                return code;
            }
            else
            {
                var firstCode = language.Code.Substring(0, 2);
                if (!string.IsNullOrEmpty(firstCode))
                    return firstCode;
            }
            return string.Empty;
        }

        private void HandleMessage(NavigationMessage message)
        {
            if (message.LoadSettingsView)
            {
                ClosePasscodeDialogs();
                NavigationService.Navigate(typeof(ShellView));
            }

            if (message.LoadQuickViewLanguages)
                LoadLanguages();

            if (message.CloseConsentDialog)
                CloseConsentDialog();

            if (message.AcceptConsent)
                AcceptConsent();
        }

        private void HandleMessage(DevicesMessage message)
        {
            if (
                message.ReloadAudioInputDevices ||
                message.ReloadAudioOuputDevices ||
                message.ReloadDashboardAudioDevices
                )
                LoadAudioDevices();
            if (message.CloseDevicesErrorDialog)
                CloseDeviceErrorDialog();
            if (message.ShowDevicesErrorDialog)
                ShowDevicesErrorDialog();
        }

        private async void ShowDevicesErrorDialog()
        {
            try
            {
                if (!IsDevicesErrorDialogOpened && devicesErrorDialog != null)
                {
                    IsDevicesErrorDialogOpened = true;
                   await _dialogService.ShowDialog(devicesErrorDialog);
                }
            }
            catch { }
        }

        private async void HandleMessage(PasscodeMessage message)
        {
            if (message.ClosePasscodeDialogs)
                ClosePasscodeDialogs();
            if (message.ResetPasscode)
              await  ShowChangePasscodeDialog();
        }

        private void ClosePasscodeDialogs()
        {
            _dialogService.HideDialog();
        }

        private void CloseLanguageDetectionWizard()
        {
            _dialogService.HideDialog();
        }

        private async void ShowlanguageDetectionWizard()
        {
            var user = await _settings.GetUser();
            languageDetectionWizard = new LanguageDetectionWizard { DataContext = new LanguageDetectionWizardViewModel(_flagLanguageService) };
            _appAnalytics.CaptureCustomEvent("Auto Detect Wizard Used",
                    new Dictionary<string, string>
                    {
                        {"User", user.UserEmail },
                        {"Organization", user.Organization },
                        {"App Version", Constants.GetSoftwareVersion() }
                    });
            await _dialogService.ShowDialog(languageDetectionWizard);
        }

        private RelayCommand _showConsentCommand = null;
        public RelayCommand ShowConsentCommand
        {
            get
            {
                return _showConsentCommand ?? (_showConsentCommand = new RelayCommand(async () => { await ShowConsentDialog(); }));
            }
        }

        private RelayCommand _showEnterPasscodeCommand = null;
        public RelayCommand ShowEnterPasscodeCommand
        {
            get
            {
                return _showEnterPasscodeCommand ?? (_showEnterPasscodeCommand = new RelayCommand(async () => { await ShowEnterPasscodeDialog(); }));
            }
        }

        private RelayCommand _launchAutoDetectCommand = null;
        public RelayCommand LaunchAutoDetectCommand
        {
            get
            {
                return _launchAutoDetectCommand ?? (_launchAutoDetectCommand = new RelayCommand(() => { ShowlanguageDetectionWizard(); }));
            }
        }
    }
}
