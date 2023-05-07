using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Logging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Settings;
using SpeechlyTouch.Services.UsageTracking;
using SpeechlyTouch.Services.Versioning;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private Visibility _progressRingVisibility;
        public Visibility ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set { SetProperty(ref _progressRingVisibility, value); }
        }

        private readonly ISettingsService _settings;
        private readonly IAuthService _authService;
        private readonly IAppVersionService _appVersionService;
        private readonly ICrashlytics _crashlytics;
        private readonly IUsageService _usageService;

        public LoginViewModel
            (
            ISettingsService settings,
            IAuthService authService,
            IAppVersionService appVersionService,
            ICrashlytics crashlytics,
            IUsageService usageService
            )
        {
            _settings = settings;
            _authService = authService;
            _crashlytics = crashlytics;
            _usageService = usageService;
            _appVersionService = appVersionService;
            ProgressRingVisibility = Visibility.Collapsed;
        }

        public async Task Login()
        {
            ProgressRingVisibility = Visibility.Visible;
            try
            {
                Models.AuthenticationObject authenticationObject = await _authService.Login();
                Microsoft.Identity.Client.AuthenticationResult authenticationResult = authenticationObject.AuthResult;
                Models.SpeechlyUserData speechlyUser = authenticationObject.SpeechlyUserData;
                if (authenticationResult != null)
                {
                    if (speechlyUser.CanLogIn)
                    {
                        _settings.IsUserLoggedIn = true;
                        StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseLoginView = true, CheckSetupStatus = true });

                        //Update organization settings for the users organization when the user logs in the first time
                        await _authService.GetOrganizationSettings();
                        await _authService.UpdateBackendLanguages();
                        await _authService.GetOrganizationTags();
                        await _authService.GetCustomTags();
                        await _authService.UpdateOrgQuestions();
                        await _appVersionService.FetchAppVersion();
                        await _usageService.GetUsageLimits();
                        StrongReferenceMessenger.Default.Send(new OrganizationSettingsMessage { ReloadOrganizationSettings = true });
                    }
                    else
                    {
                        //Logout
                        if (
                            !speechlyUser.HasSpeeclyAccount ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeactivated) ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeleted) ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDisabled) ||
                            speechlyUser.ErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountNotFound)
                            )
                        {
                            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseLoginView = true });

                            StrongReferenceMessenger.Default.Send(new NavigationMessage { ShowLoginFailedView = true });
                        }
                        else
                        {
                            if (!speechlyUser.HasValidSpeechlyCoreLicense)
                            {
                                StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseLoginView = true });

                                StrongReferenceMessenger.Default.Send(new NavigationMessage { ShowInvalidLicenseView = true });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            ProgressRingVisibility = Visibility.Collapsed;
        }

        private RelayCommand _signInCommand = null;
        public RelayCommand SignInCommand
        {
            get
            {
                return _signInCommand ?? (_signInCommand = new RelayCommand(async () => { await Login(); }));
            }
        }
    }
}
