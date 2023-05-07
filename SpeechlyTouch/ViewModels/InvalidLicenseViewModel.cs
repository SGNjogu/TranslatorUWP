using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Helpers;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Auth;
using SpeechlyTouch.Services.Settings;
using Windows.ApplicationModel.Resources;

namespace SpeechlyTouch.ViewModels
{
    public class InvalidLicenseViewModel : ObservableObject
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        private string _errorActionMessage;
        public string ErrorActionMessage
        {
            get { return _errorActionMessage; }
            set { SetProperty(ref _errorActionMessage, value); }
        }

        private readonly ISettingsService _settings;
        private readonly IAuthService _authService;
        private ResourceLoader _resourceLoader;

        public InvalidLicenseViewModel(ISettingsService settings, IAuthService authService)
        {
            _settings = settings;
            _authService = authService;
            _resourceLoader = ResourceLoader.GetForCurrentView();
        }

        public void InitializeMessages()
        {
            if (string.IsNullOrEmpty(_authService.ErrorMessage))
                return;

            var authenticationErrorMessage = _authService.ErrorMessage;

            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseIsExpired))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyLicenseHasExpiredErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyLicenseHasExpiredErrorActionMessage");
            }
            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseIsInActive))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyLicenseIsInactiveErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyLicenseIsInactiveErrorActionMessage");
            }
            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseIsSuspended))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyLicenseIsSuspendedErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyLicenseIsSuspendedErrorActionMessage");
            }
            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaCoreLicenseNotFound))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyLicenseNotFoundErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyLicenseNotFoundErrorActionMessage");
            }
        }

        private async void RetryLogin()
        {
            await _authService.Logout();
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseInvalidLicenseView = true });
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() => { RetryLogin(); }));
            }
        }
    }
}
