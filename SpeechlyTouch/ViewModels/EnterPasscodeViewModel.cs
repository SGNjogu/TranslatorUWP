using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Settings;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class EnterPasscodeViewModel : ObservableObject
    {
        private string _enteredPasscode;
        public string EnteredPasscode
        {
            get { return _enteredPasscode; }
            set
            {
                SetProperty(ref _enteredPasscode, value);
                ValidatePasscode();
            }
        }

        private Visibility _errorMessageVisibility;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set { SetProperty(ref _errorMessageVisibility, value); }
        }

        private readonly ISettingsService _settings;

        public EnterPasscodeViewModel(ISettingsService settings)
        {
            StrongReferenceMessenger.Default.Register<PasscodeMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _settings = settings;
            ErrorMessageVisibility = Visibility.Collapsed;
        }

        private async void ValidatePasscode()
        {
            if (EnteredPasscode.Length == 4)
            {
                if (EnteredPasscode.Equals(_settings.Passcode))
                {
                    InputPane.GetForCurrentView().TryHide();
                    StrongReferenceMessenger.Default.Send(new NavigationMessage { LoadSettingsView = true });
                }
                else
                    ErrorMessageVisibility = Visibility.Visible;

                await Task.Delay(5000);
                ErrorMessageVisibility = Visibility.Collapsed;
            }
        }

        private void HandleMessage(PasscodeMessage message)
        {
            if(message.ClearPasscodeValues)
                EnteredPasscode = "";
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() => { StrongReferenceMessenger.Default.Send(new PasscodeMessage { ClosePasscodeDialogs = true }); }));
            }
        }

        private RelayCommand _showChangePasscodeCommand = null;
        public RelayCommand ShowChangePasscodeCommand
        {
            get
            {
                return _showChangePasscodeCommand ?? (_showChangePasscodeCommand = new RelayCommand(() => { StrongReferenceMessenger.Default.Send(new PasscodeMessage { ResetPasscode = true }); }));
            }
        }
    }
}
