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
    public class LoginFailedViewModel : ObservableObject
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
        public LoginFailedViewModel(ISettingsService settings, IAuthService authService)
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

            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountNotFound))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyAccountNotFoundErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyAccountNotFoundErrorActionMessage");
            }
            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeactivated))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyAccountIsDeactivatedErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyAccountIsDeactivatedErrorActionMessage");
            }
            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDisabled))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyAccountIsDisabledErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyAccountIsDisabledErrorActionMessage");
            }
            if (authenticationErrorMessage == EnumsConverter.ConvertToString(LoginErrorMessage.TalaAccountIsDeleted))
            {
                ErrorMessage = _resourceLoader.GetString("SpeechlyAccountIsDeletedErrorMessage");
                ErrorActionMessage = _resourceLoader.GetString("SpeechlyAccountIsDeletedErrorActionMessage");
            }
        }

        private void RetryLogin()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseLoginFailedView = true });
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
