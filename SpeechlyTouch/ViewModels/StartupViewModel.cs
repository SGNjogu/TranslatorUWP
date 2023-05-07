using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.DataService.Models;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Connectivity;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.Internationalization;
using SpeechlyTouch.Services.KeyVault;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Services.UsageTracking;
using SpeechlyTouch.Services.Versioning;
using SpeechlyTouch.Views.Pages;
using SpeechlyTouch.Views.Popups;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class StartupViewModel : ObservableObject
    {
        private Visibility _isInitializingProgressActive;
        public Visibility IsInitializingProgressActive
        {
            get { return _isInitializingProgressActive; }
            set { SetProperty(ref _isInitializingProgressActive, value); }
        }

        private Visibility _isInitializingError;
        public Visibility IsInitializingError
        {
            get { return _isInitializingError; }
            set { SetProperty(ref _isInitializingError, value); }
        }

        private string _initializationMessage;
        public string InitializationMessage
        {
            get { return _initializationMessage; }
            set { SetProperty(ref _initializationMessage, value); }
        }

        private LoginFailedDialog loginFailedDialog;
        private InvalidLicenseDialog invalidLicenseDialog;
        private LoginDialog loginDialog;
        private AppUpdateDialog appUpdateDialog;

        private readonly IKeyVaultService _keyVaultService;
        private readonly IKeyVaultDataService _keyVaultDataService;
        private readonly ISettingsService _settingsService;
        private readonly IAuthService _authService;
        private readonly IPullDataService _pullDataService;
        private readonly IPushDataService _pushDataService;
        private readonly IConnectivityService _connectivityService;
        private readonly IAppVersionService _appVersionService;
        private readonly IInternationalizationService _internationalizationService;
        private readonly ICrashlytics _crashlytics;
        private readonly IUsageService _usageService;
        private readonly IDialogService _dialogService;

        private BackgroundWorker BackgroundWorkerClient;

        private ResourceLoader _resourceLoader;

        private int RetryCount = 3;

        public StartupViewModel
            (
            IKeyVaultService keyVaultService,
            IKeyVaultDataService keyVaultDataService,
            ISettingsService settingsService,
            IAuthService authService,
            IPullDataService pullDataService,
            IPushDataService pushDataService,
            IConnectivityService connectivityService,
            IAppVersionService appVersionService,
            IInternationalizationService internationalizationService,
            ICrashlytics crashlytics,
            IUsageService usageService,
            IDialogService dialogService
            )
        {
            _keyVaultService = keyVaultService;
            _keyVaultDataService = keyVaultDataService;
            _settingsService = settingsService;
            _authService = authService;
            _pullDataService = pullDataService;
            _pushDataService = pushDataService;
            _connectivityService = connectivityService;
            _appVersionService = appVersionService;
            _internationalizationService = internationalizationService;
            _crashlytics = crashlytics;
            _usageService = usageService;
            _dialogService = dialogService;
            _internationalizationService.LoadApplicationLanguage();

            _resourceLoader = ResourceLoader.GetForViewIndependentUse();
            InitializationMessage = _resourceLoader.GetString("StartUpPageSettingUpMessage");
            IsInitializingError = Visibility.Collapsed;
            IsInitializingProgressActive = Visibility.Visible;

            _connectivityService.ConnectionChangedEvent += OnConnectionStateChanged;

           LoadDialog();

            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            StrongReferenceMessenger.Default.Register<AppUpdateMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            StrongReferenceMessenger.Default.Register<InternationalizationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });

            BackgroundWorkerClient = new BackgroundWorker();
            BackgroundWorkerClient.DoWork += DoWork;

            _ = InitializeAppSecrets();
        }

        private void LoadDialog()
        {
            loginDialog = new LoginDialog();
            loginFailedDialog = new LoginFailedDialog();
            invalidLicenseDialog = new InvalidLicenseDialog();
            appUpdateDialog = new AppUpdateDialog();
        }
        private async void HandleMessage(InternationalizationMessage message)
        {
            if (!string.IsNullOrEmpty(message.LanguageCode))
            {
                await Task.Delay(300);
                loginDialog = new LoginDialog();
                loginFailedDialog = new LoginFailedDialog();
                invalidLicenseDialog = new InvalidLicenseDialog();
                appUpdateDialog = new AppUpdateDialog();
            }
        }

        private void HandleMessage(AppUpdateMessage message)
        {
            if (message.CloseUpdateDialog)
                appUpdateDialog.Hide();
        }

        private void OnConnectionStateChanged(object sender, ConnectionChangedEventArgs args)
        {
            var connectivityMessage = new ConnectivityMessage();

            switch (args.ConnectionState)
            {
                case ConnectionState.ConnectionPresent:
                    connectivityMessage.ConnectionPresent = true;
                    break;
                case ConnectionState.ConnectionLost:
                    connectivityMessage.ConnectionLost = true;
                    break;
                case ConnectionState.ConnectionStable:
                    connectivityMessage.ConnectionStable = true;
                    break;
                case ConnectionState.ConnectionSlow:
                    connectivityMessage.ConnectionSlow = true;
                    break;
                default:
                    break;
            }

            StrongReferenceMessenger.Default.Send(connectivityMessage);
        }

        private async void HandleMessage(NavigationMessage message)
        {
            try
            {
                if (message.CloseLoginView)
                {
                    CloseLoginDialog();
                    StrongReferenceMessenger.Default.Send(new LoginMessage { AccountSwitch = true });
                }

                if (message.CheckSetupStatus)
                {
                    await CheckSetupStatus();
                }

                if (message.CloseLoginFailedView)
                {
                    CloseLoginFailedDialog();
                    await ShowLoginDialog();
                }

                if (message.ShowLoginFailedView)
                {
                    CloseLoginDialog();
                    await ShowLoginFailedDialog();
                }

                if (message.CloseInvalidLicenseView)
                {
                    CloseInvalideLicenseDialog();
                    await ShowLoginDialog();
                }

                if (message.ShowInvalidLicenseView)
                    await ShowInvalidLicenseDialog();

                if (message.ShowLoginDialogView)
                    await ShowLoginDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task CheckLoginStatus()
        {
            try
            {
                var user = await _settingsService.GetUser();
                if (user == null || !user.IsLoggedIn || !user.HasValidLicense)
                {
                    await _authService.Logout();
                   await ShowLoginDialog();
                }
                else
                {
                    await AttemptSilentLogin();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task ShowLoginDialog()
        {
           await _dialogService.ShowDialog(loginDialog);
        }

        private void CloseLoginDialog()
        {
            loginDialog.Hide();
        }

        private async Task CheckSetupStatus()
        {
            try
            {
                if (BackgroundWorkerClient != null)
                {
                    BackgroundWorkerClient.RunWorkerAsync();
                }

                if (_settingsService.IsUserDoneSettingUp)
                {
                    NavigationService.Navigate(typeof(DashboardPage));

                    var appVersion = _appVersionService.CurrentVersion;

                    if (appVersion != null)
                    {
                        if (appVersion.IsUnsupportedVersion || appVersion.IsForcedUpdate || !appVersion.IsLatestVersion)
                        {
                          await  _dialogService.ShowDialog(appUpdateDialog);
                        }
                    }
                }
                else
                {
                    NavigationService.Navigate(typeof(InitialSetupPage));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void CloseLoginFailedDialog()
        {
            loginFailedDialog.Hide();
        }

        private void CloseInvalideLicenseDialog()
        {
            invalidLicenseDialog.Hide();
        }

        private async Task ShowLoginFailedDialog()
        {
            await _dialogService.ShowDialog(loginFailedDialog);
        }

        private async Task ShowInvalidLicenseDialog()
        {
            await _dialogService.ShowDialog(invalidLicenseDialog);
        }

        private async Task AttemptSilentLogin()
        {
            try
            {
                var authResultObject = await _authService.AttemptSilentLogin();
                var authResult = authResultObject?.AuthResult;
                var speechlyuser = authResultObject?.SpeechlyUserData;
                var user = await _settingsService.GetUser();

                if (authResultObject == null || authResult == null || speechlyuser == null || !speechlyuser.CanLogIn)
                {
                    await _authService.Logout();
                   await ShowLoginDialog();
                }
                else
                {
                    _settingsService.IsUserLoggedIn = true;
                    await _authService.GetOrganizationSettings();
                    await _authService.UpdateBackendLanguages();
                    await _authService.GetOrganizationTags();
                    await _authService.GetCustomTags();
                    await _authService.UpdateOrgQuestions();
                    await _usageService.GetUsageLimits();
                    StrongReferenceMessenger.Default.Send(new OrganizationSettingsMessage { ReloadOrganizationSettings = true });
                    StrongReferenceMessenger.Default.Send(new LanguageMessage { UpdateLanguages = true });
                    await _appVersionService.FetchAppVersion();
                    await CheckSetupStatus();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var userSettings = await _settingsService.GetUser();
                if (!userSettings.DataConsentStatus) return;

                Thread pushThread = new Thread(_pushDataService.BeginDataSync);
                pushThread.Start();
                Thread pullThread = new Thread(_pullDataService.BeginDataSync);
                pullThread.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }


        public async Task InitializeAppSecrets()
        {
            try
            {

                IsInitializingError = Visibility.Collapsed;
                IsInitializingProgressActive = Visibility.Visible;

                bool isNetworkConnected = _connectivityService.IsConnectionAvailable();

                if (!isNetworkConnected)
                {
                    IsInitializingProgressActive = Visibility.Collapsed;
                    IsInitializingError = Visibility.Visible;
                    return;
                }

                RetryCount = 3;

                var secrets = await SaveSecrets();

                if (secrets != null)
                {
                    AssignConstants(secrets);
                    await _internationalizationService.GetInternationalizationLanguages();
                    await CheckLoginStatus();
                }
                else if (RetryCount > 0)
                {
                    await InitializeAppSecrets();
                    RetryCount--;
                }
                else
                {
                    IsInitializingProgressActive = Visibility.Collapsed;
                    IsInitializingError = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void AssignConstants(AzureKeyVaultSecrets secrets)
        {
            Constants.AzureKey = secrets.AzureKey;
            Constants.AzureRegion = secrets.AzureRegion;
            Constants.AppCenterClientId = secrets.AppCenterKey;
            Constants.MailGunApiKey = secrets.MailGunApiKey;
            Constants.BusinessAnnualLicenseSku = secrets.BusinessAnnualLicenseSku;
            Constants.BusinessMonthlyLicenseSku = secrets.BusinessMonthlyLicenseSku;
            Constants.ConferenceAnnualLicenseSku = secrets.ConferenceAnnualLicenseSku;
            Constants.ConferenceMonthlyLicenseSku = secrets.ConferenceMonthlyLicenseSku;
            Constants.AB2CClientId = secrets.AB2CClientId;
            Constants.AzureStorageConnectionString = secrets.AzureStorageConnectionString;
            Constants.GoogleTranslateCredentials = secrets.GoogleTranslateCredentials;
        }

        private async Task<AzureKeyVaultSecrets> SaveSecrets()
        {
            AzureKeyVaultSecrets secrets = null;
            try
            {
                secrets = await _keyVaultDataService.Get();
                if (secrets != null)
                {
                    int localId = secrets.ID;
                    var savedDate = secrets.DateCreated;
                    var totalDays = (DateTime.UtcNow - savedDate).Days;
                    if (totalDays >= 7)
                    {
                        secrets = await FetchAppSecrets();
                        secrets.ID = localId;
                        await _keyVaultDataService.Update(secrets);
                    }
                    if (string.IsNullOrEmpty(secrets.GoogleTranslateCredentials))
                    {
                        secrets = await FetchAppSecrets();
                    }
                }
                else
                {
                    await FetchSecrets();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
            return await Task.FromResult(secrets);
        }

        private async Task FetchSecrets()
        {
            var secrets = await FetchAppSecrets();
            await _keyVaultDataService.Create(secrets);
        }

        private async Task<AzureKeyVaultSecrets> FetchAppSecrets()
        {
            try
            {
                var secrets = await _keyVaultService.GetAllSecretsAsync();
                var jsonString = await JsonConverter.ReturnJsonStringFromObject(secrets);
                var azureKeyVaultSecrets = await JsonConverter.ReturnObjectFromJsonString<AzureKeyVaultSecrets>(jsonString);
                azureKeyVaultSecrets.DateCreated = DateTime.UtcNow;
                return azureKeyVaultSecrets;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
                return null;
            }
        }

        public void CloseApp()
        {
            CoreApplication.Exit();
        }

        private RelayCommand _retryCommand = null;
        public RelayCommand RetryCommand
        {
            get
            {
                return _retryCommand ?? (_retryCommand = new RelayCommand(async () => { await InitializeAppSecrets(); }));
            }
        }

        private RelayCommand _closeAppCommand = null;
        public RelayCommand CloseAppCommand
        {
            get
            {
                return _closeAppCommand ?? (_closeAppCommand = new RelayCommand(() => { CloseApp(); }));
            }
        }
    }
}
