using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Settings;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class InitialSetupPasscodeViewModel : ObservableObject
    {
        private string _enteredPasscode;
        public string EnteredPasscode
        {
            get { return _enteredPasscode; }
            set { SetProperty(ref _enteredPasscode, value); }
        }

        private string _enteredPasscodeConfirmation;
        public string EnteredPasscodeConfirmation
        {
            get { return _enteredPasscodeConfirmation; }
            set { SetProperty(ref _enteredPasscodeConfirmation, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        private Visibility _errorMessageVisibility;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set { SetProperty(ref _errorMessageVisibility, value); }
        }


        private readonly ISettingsService _settings;
        private ResourceLoader _resourceLoader;

        public InitialSetupPasscodeViewModel(ISettingsService settings)
        {
            _settings = settings;
            _resourceLoader = ResourceLoader.GetForCurrentView();
            ErrorMessageVisibility = Visibility.Collapsed;
            ErrorMessage = "";
            EnteredPasscode = "";
            EnteredPasscodeConfirmation = "";
        }

        private async Task ValidatePasscode()
        {
            if (EnteredPasscode?.Length < 4 || EnteredPasscodeConfirmation?.Length < 4)
            {
                ErrorMessageVisibility = Visibility.Visible;
                ErrorMessage = _resourceLoader.GetString("PasscodeError_MinimumLength");
            }
            else if (!EnteredPasscode.Equals(EnteredPasscodeConfirmation))
            {
                ErrorMessageVisibility = Visibility.Visible;
                ErrorMessage = _resourceLoader.GetString("PasscodeError_NotMatching");
            }
            else
            {
                _settings.Passcode = EnteredPasscode;
                _settings.IsUserDoneSettingUp = true;
                StrongReferenceMessenger.Default.Send(new NavigationMessage { FinishAppSetup = true });
                EnteredPasscode = "";
                EnteredPasscodeConfirmation = "";
            }

            await Task.Delay(5000);
            ErrorMessageVisibility = Visibility.Collapsed;
        }

        private RelayCommand _finishSetupCommand = null;
        public RelayCommand FinishSetupCommand
        {
            get
            {
                return _finishSetupCommand ?? (_finishSetupCommand = new RelayCommand(async () => { await ValidatePasscode();}));
            }
        }
    }
}
