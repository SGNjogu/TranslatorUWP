using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using SpeechlyTouch.Core.Services.AudioProfileService;
using SpeechlyTouch.Core.Services.CongnitiveService;
using SpeechlyTouch.Core.Services.HttpClientProvider;
using SpeechlyTouch.Core.Services.TranslationProviders;
using SpeechlyTouch.Core.Services.TranslationProviders.Interfaces;
using SpeechlyTouch.Core.Services.Voices;
using SpeechlyTouch.Core.Utils;
using SpeechlyTouch.DataService.Interfaces;
using SpeechlyTouch.Infrastructure.Services.DataSync;
using SpeechlyTouch.Infrastructure.Services.Email;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Services.Audio;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Bluetooth;
using SpeechlyTouch.Services.CognitiveService;
using SpeechlyTouch.Services.Connectivity;
using SpeechlyTouch.Services.DataSync.Interfaces;
using SpeechlyTouch.Services.DataSync.Services;
using SpeechlyTouch.Services.FlagLanguage;
using SpeechlyTouch.Services.KeyVault;
using SpeechlyTouch.Services.Languages;
using SpeechlyTouch.Services.Popup;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Services.SignalR;
using SpeechlyTouch.Services.UsageTracking;
using SpeechlyTouch.Services.WiFiNetworks;
using SpeechlyTouch.ViewModels;
using System;
using System.Diagnostics;
using System.Net.Http;
using CoreLanguageService = SpeechlyTouch.Core.Services.Languages.ILanguagesService;
using IInternationalization = SpeechlyTouch.Services.Internationalization.IInternationalizationService;
using InternationalizationUtil = SpeechlyTouch.Services.Internationalization.InternationalizationService;
using MicrosoftLanguagesService = SpeechlyTouch.Core.Services.Languages.MicrosoftLanguagesService;

namespace SpeechlyTouch.Services.Dependency
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary> 
    public class DependencyService
    {
        private IServiceProvider _serviceProvider;
        private ServiceCollection _serviceDescriptors;

        public DashboardViewModel DashboardViewModel
        {
            get
            {
                return _serviceProvider.GetService<DashboardViewModel>();
            }
        }

        public ConsentViewModel ConsentViewModel
        {
            get
            {
                return _serviceProvider.GetService<ConsentViewModel>();
            }
        }

        public ChangePasscodeViewModel ChangePasscodeViewModel
        {
            get
            {
                return _serviceProvider.GetService<ChangePasscodeViewModel>();
            }
        }

        public EnterPasscodeViewModel EnterPasscodeViewModel
        {
            get
            {
                return _serviceProvider.GetService<EnterPasscodeViewModel>();
            }
        }

        public ReEnterPasscodeViewModel ReEnterPasscodeViewModel
        {
            get
            {
                return _serviceProvider.GetService<ReEnterPasscodeViewModel>();
            }
        }

        public SetupPasscodeViewModel SetupPasscodeViewModel
        {
            get
            {
                return _serviceProvider.GetService<SetupPasscodeViewModel>();
            }
        }

        public ShellViewModel ShellViewModel
        {
            get
            {
                return _serviceProvider.GetService<ShellViewModel>();
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return _serviceProvider.GetService<SettingsViewModel>();
            }
        }

        public DevicesViewModel DevicesViewModel
        {
            get
            {
                return _serviceProvider.GetService<DevicesViewModel>();
            }
        }

        public DevicesErrorViewModel DevicesErrorViewModel
        {
            get
            {
                return _serviceProvider.GetService<DevicesErrorViewModel>();
            }
        }

        public ChatViewModel ChatViewModel
        {
            get
            {
                return _serviceProvider.GetService<ChatViewModel>();
            }
        }

        public TranslationViewModel TranslationViewModel
        {
            get
            {
                return _serviceProvider.GetService<TranslationViewModel>();
            }
        }

        public LanguagesViewModel LanguagesViewModel
        {
            get
            {
                return _serviceProvider.GetService<LanguagesViewModel>();
            }
        }

        public ProfileViewModel ProfileViewModel
        {
            get
            {
                return _serviceProvider.GetService<ProfileViewModel>();
            }
        }

        public LoginViewModel LoginViewModel
        {
            get
            {
                return _serviceProvider.GetService<LoginViewModel>();
            }
        }

        public LoginFailedViewModel LoginFailedViewModel
        {
            get
            {
                return _serviceProvider.GetService<LoginFailedViewModel>();
            }
        }

        public InvalidLicenseViewModel InvalidLicenseViewModel
        {
            get
            {
                return _serviceProvider.GetService<InvalidLicenseViewModel>();
            }
        }

        public EmailViewModel EmailViewModel
        {
            get
            {
                return _serviceProvider.GetService<EmailViewModel>();
            }
        }

        public StartupViewModel StartupViewModel
        {
            get
            {
                return _serviceProvider.GetService<StartupViewModel>();
            }
        }

        public HistoryViewModel HistoryViewModel
        {
            get
            {
                return _serviceProvider.GetService<HistoryViewModel>();
            }
        }

        public SessionDetailsViewModel SessionDetailsViewModel
        {
            get
            {
                return _serviceProvider.GetService<SessionDetailsViewModel>();
            }
        }

        public AudioPlayerViewModel AudioPlayerViewModel
        {
            get
            {
                return _serviceProvider.GetService<AudioPlayerViewModel>();
            }
        }

        public AudioNotDetectedViewModel AudioNotDetectedViewModel
        {
            get
            {
                return _serviceProvider.GetService<AudioNotDetectedViewModel>();
            }
        }

        public ImmersiveReaderViewModel ImmersiveReaderViewModel
        {
            get
            {
                return _serviceProvider.GetService<ImmersiveReaderViewModel>();
            }
        }

        public HelpViewModel HelpViewModel
        {
            get
            {
                return _serviceProvider.GetService<HelpViewModel>();
            }
        }

        public AboutViewModel AboutViewModel
        {
            get
            {
                return _serviceProvider.GetService<AboutViewModel>();
            }
        }

        public WhatsNewViewModel WhatsNewViewModel
        {
            get
            {
                return _serviceProvider.GetService<WhatsNewViewModel>();
            }
        }

        public LicenseViewModel LicenseViewModel
        {
            get
            {
                return _serviceProvider.GetService<LicenseViewModel>();
            }
        }

        public FeedbackViewModel FeedbackViewModel
        {
            get
            {
                return _serviceProvider.GetService<FeedbackViewModel>();
            }
        }

        public InitialSetupViewModel InitialSetupViewModel
        {
            get
            {
                return _serviceProvider.GetService<InitialSetupViewModel>();
            }
        }

        public InitialSetupNetworkViewModel InitialSetupNetworkViewModel
        {
            get
            {
                return _serviceProvider.GetService<InitialSetupNetworkViewModel>();
            }
        }

        public AudioDevicesViewModel AudioDevicesViewModel
        {
            get
            {
                return _serviceProvider.GetService<AudioDevicesViewModel>();
            }
        }

        public InitialSetupLanguagesViewModel InitialSetupLanguagesViewModel
        {
            get
            {
                return _serviceProvider.GetService<InitialSetupLanguagesViewModel>();
            }
        }

        public InitialSetupPasscodeViewModel InitialSetupPasscodeViewModel
        {
            get
            {
                return _serviceProvider.GetService<InitialSetupPasscodeViewModel>();
            }
        }

        public AppCloseViewModel AppCloseViewModel
        {
            get
            {
                return _serviceProvider.GetService<AppCloseViewModel>();
            }
        }

        public SessionMetaDataViewModel SessionMetaDataViewModel
        {
            get
            {
                return _serviceProvider.GetService<SessionMetaDataViewModel>();
            }
        }

        public AppUpdateViewModel AppUpdateViewModel
        {
            get
            {
                return _serviceProvider.GetService<AppUpdateViewModel>();
            }
        }


        public AutoDetectionFlagsViewModel AutoDetectionFlagsViewModel
        {
            get
            {
                return _serviceProvider.GetService<AutoDetectionFlagsViewModel>();
            }
        }

        public AutoDetectionLanguagesViewModel AutoDetectionLanguagesViewModel
        {
            get
            {
                return _serviceProvider.GetService<AutoDetectionLanguagesViewModel>();
            }
        }

        public OrgQuestionsViewModel OrgQuestionsViewModel
        {
            get
            {
                return _serviceProvider.GetService<OrgQuestionsViewModel>();
            }
        }

        public FeedbackStarRatingViewModel FeedbackStarRatingViewModel
        {
            get
            {
                return _serviceProvider.GetService<FeedbackStarRatingViewModel>();
            }
        }

        public FeedbackRatingViewModel FeedbackRatingViewModel
        {
            get
            {
                return _serviceProvider.GetService<FeedbackRatingViewModel>();
            }
        }

        public FeedbackSubmittedViewModel FeedbackSubmittedViewModel
        {
            get
            {
                return _serviceProvider.GetService<FeedbackSubmittedViewModel>();
            }
        }

        public NotificationViewModel NotificationViewModel
        {
            get
            {
                return _serviceProvider.GetService<NotificationViewModel>();
            }
        }
        public ErrorViewModel ErrorViewModel
        {
            get
            {
                return _serviceProvider.GetService<ErrorViewModel>();
            }
        }

        public AudioDownloadViewModel AudioDownloadViewModel
        {
            get
            {
                return _serviceProvider.GetService<AudioDownloadViewModel>();
            }
        }

        public AddNewQuestionViewModel AddNewQuestionViewModel
        {
            get
            {
                return _serviceProvider.GetService<AddNewQuestionViewModel>();
            }
        }

        public DependencyService()
        {
            Initialize();
        }

        private async void Initialize()
        {
            _serviceProvider = ConfigureServices();

            try
            {
                var dataService = _serviceProvider.GetService<IDataService>();
                var dataBasePath = await Constants.DatabasePath();
                await dataService.InitializeAsync(dataBasePath);
                InitializeAppCenter();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private IServiceProvider ConfigureServices()
        {
            _serviceDescriptors = new ServiceCollection();

            // Register Interfaces before ViewModels
            RegisterHttpClient();
            RegisterInterfaces();
            RegisterViewModels();

            return _serviceDescriptors.BuildServiceProvider();
        }

        private void RegisterHttpClient()
        {
            _serviceDescriptors.AddHttpClient(HttpClients.SpeechlyBackend, c =>
            {
                c.BaseAddress = new Uri(Constants.BackendAPiEndpoint);
            });

            var factory = _serviceDescriptors.BuildServiceProvider().GetService<IHttpClientFactory>();
            _serviceDescriptors.AddSingleton(typeof(IHttpClientFactory), factory);
            _serviceDescriptors.AddSingleton<IHttpClientProvider, HttpClientProvider>();
        }

        private void RegisterInterfaces()
        {
            try
            {
                // Register Interfaces Here
                _serviceDescriptors.AddSingleton<IAppAnalytics, AppAnalytics>();
                _serviceDescriptors.AddSingleton<IDataService, DataService.Services.DataService>();
                _serviceDescriptors.AddSingleton<Languages.ILanguagesService, LanguagesService>();
                _serviceDescriptors.AddSingleton<ISettingsService, SettingsService>();
                _serviceDescriptors.AddSingleton<ICrashlytics, CrashlyticsConfig>();
                _serviceDescriptors.AddSingleton<IEmailService, EmailService>();
                _serviceDescriptors.AddSingleton<IBluetoothService, BluetoothService>();
                _serviceDescriptors.AddSingleton<IWiFiService, WiFiService>();
                _serviceDescriptors.AddSingleton<IAudioProfileService, AudioProfileService>();
                _serviceDescriptors.AddSingleton<IAudioService, AudioService>();
                _serviceDescriptors.AddSingleton<IAuthService, AuthService>();
                _serviceDescriptors.AddSingleton<IKeyVaultService, KeyVaultService>();
                _serviceDescriptors.AddSingleton<IKeyVaultDataService, KeyVaultDataService>();
                _serviceDescriptors.AddSingleton<ITranslationHub, MicrosoftTranslationHub>();
                _serviceDescriptors.AddSingleton<CoreLanguageService, MicrosoftLanguagesService>();
                _serviceDescriptors.AddSingleton<IVoicesService, MicrosoftVoicesService>();
                _serviceDescriptors.AddSingleton<ISessionService, SessionService>();
                _serviceDescriptors.AddSingleton<ITranscriptionService, TranscriptionService>();
                _serviceDescriptors.AddSingleton<IDeviceService, DeviceService>();
                _serviceDescriptors.AddSingleton<IPullDataService, PullDataService>();
                _serviceDescriptors.AddSingleton<IPushDataService, PushDataService>();
                _serviceDescriptors.AddSingleton<IAzureStorageService, AzureStorageService>();
                _serviceDescriptors.AddSingleton<IUserFeedbackService, UserFeedbackService>();
                _serviceDescriptors.AddSingleton<IPlaybackUsageService, PlaybackUsageService>();
                _serviceDescriptors.AddSingleton<IUsageTrackingService, UsageTrackingService>();
                _serviceDescriptors.AddSingleton<ICognitiveEndpointsService, CognitiveEndpointsService>();
                _serviceDescriptors.AddSingleton<ICognitiveServicesHelper, CognitiveServicesHelper>();
                _serviceDescriptors.AddSingleton<IOrganizationSettingsService, OrganizationSettingsService>();
                _serviceDescriptors.AddSingleton<IConnectivityService, ConnectivityService>();
                _serviceDescriptors.AddSingleton<IAppVersionService, AppVersionService>();
                _serviceDescriptors.AddSingleton<Versioning.IAppVersionService, Versioning.AppVersionService>();
                _serviceDescriptors.AddSingleton<IInternationalizationService, InternationalizationService>();
                _serviceDescriptors.AddSingleton<IInternationalization, InternationalizationUtil>();
                _serviceDescriptors.AddSingleton<ICustomTagService, CustomTagService>();
                _serviceDescriptors.AddSingleton<IUsageService, UsageService>();
                _serviceDescriptors.AddSingleton<ISignalRService, SignalRService>();
                _serviceDescriptors.AddSingleton<IFlagLanguageService, FlagLanguageService>();
                _serviceDescriptors.AddSingleton<IMicrosoftTextToTextTranslator, MicrosoftTextToTextTranslator>();
                _serviceDescriptors.AddSingleton<IMicrosoftStandardVoiceSynthesizer, MicrosoftStandardVoiceSynthesizer>();
                _serviceDescriptors.AddSingleton<IMicrosoftTextToSpeechProvider, MicrosoftTextToSpeechProvider>();
                _serviceDescriptors.AddSingleton<IQuestionsService, QuestionsService>();
                _serviceDescriptors.AddSingleton<IBackendLanguageService, BackendLanguageService>();
                _serviceDescriptors.AddSingleton<IDialogService, DialogService>();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void RegisterViewModels()
        {
            try
            {
                // RegisterViewModels Here
                _serviceDescriptors.AddSingleton<DashboardViewModel>();
                _serviceDescriptors.AddSingleton<ConsentViewModel>();
                _serviceDescriptors.AddSingleton<SettingsViewModel>();
                _serviceDescriptors.AddSingleton<DevicesViewModel>();
                _serviceDescriptors.AddSingleton<ShellViewModel>();
                _serviceDescriptors.AddSingleton<SetupPasscodeViewModel>();
                _serviceDescriptors.AddSingleton<ChangePasscodeViewModel>();
                _serviceDescriptors.AddSingleton<EnterPasscodeViewModel>();
                _serviceDescriptors.AddSingleton<ReEnterPasscodeViewModel>();
                _serviceDescriptors.AddSingleton<TranslationViewModel>();
                _serviceDescriptors.AddSingleton<ChatViewModel>();
                _serviceDescriptors.AddSingleton<LanguagesViewModel>();
                _serviceDescriptors.AddSingleton<ProfileViewModel>();
                _serviceDescriptors.AddSingleton<DevicesErrorViewModel>();
                _serviceDescriptors.AddSingleton<LoginViewModel>();
                _serviceDescriptors.AddSingleton<LoginFailedViewModel>();
                _serviceDescriptors.AddSingleton<InvalidLicenseViewModel>();
                _serviceDescriptors.AddSingleton<StartupViewModel>();
                _serviceDescriptors.AddSingleton<EmailViewModel>();
                _serviceDescriptors.AddSingleton<HistoryViewModel>();
                _serviceDescriptors.AddSingleton<SessionDetailsViewModel>();
                _serviceDescriptors.AddSingleton<AudioPlayerViewModel>();
                _serviceDescriptors.AddSingleton<AudioNotDetectedViewModel>();
                _serviceDescriptors.AddSingleton<ImmersiveReaderViewModel>();
                _serviceDescriptors.AddSingleton<HelpViewModel>();
                _serviceDescriptors.AddSingleton<AboutViewModel>();
                _serviceDescriptors.AddSingleton<WhatsNewViewModel>();
                _serviceDescriptors.AddSingleton<LicenseViewModel>();
                _serviceDescriptors.AddSingleton<FeedbackViewModel>();
                _serviceDescriptors.AddSingleton<InitialSetupViewModel>();
                _serviceDescriptors.AddSingleton<InitialSetupNetworkViewModel>();
                _serviceDescriptors.AddSingleton<AudioDevicesViewModel>();
                _serviceDescriptors.AddSingleton<InitialSetupLanguagesViewModel>();
                _serviceDescriptors.AddSingleton<InitialSetupPasscodeViewModel>();
                _serviceDescriptors.AddSingleton<AppCloseViewModel>();
                _serviceDescriptors.AddSingleton<SessionMetaDataViewModel>();
                _serviceDescriptors.AddSingleton<AppUpdateViewModel>();
                _serviceDescriptors.AddSingleton<AutoDetectionLanguagesViewModel>();
                _serviceDescriptors.AddSingleton<AutoDetectionFlagsViewModel>();
                _serviceDescriptors.AddSingleton<OrgQuestionsViewModel>();
                _serviceDescriptors.AddSingleton<FeedbackSubmittedViewModel>();
                _serviceDescriptors.AddSingleton<FeedbackRatingViewModel>();
                _serviceDescriptors.AddSingleton<FeedbackStarRatingViewModel>();
                _serviceDescriptors.AddSingleton<NotificationViewModel>();
                _serviceDescriptors.AddSingleton<ErrorViewModel>();
                _serviceDescriptors.AddSingleton<AudioDownloadViewModel>();
                _serviceDescriptors.AddSingleton<AddNewQuestionViewModel>();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void InitializeAppCenter()
        {
            // TODO WTS: Add your app in the app center and set your secret here. More at https://docs.microsoft.com/appcenter/sdk/getting-started/uwp
            AppCenter.Start(Constants.AppCenterClientId, typeof(Analytics), typeof(Crashes));
        }
    }
}
