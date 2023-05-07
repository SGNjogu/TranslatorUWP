using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Settings;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace SpeechlyTouch.ViewModels
{
    public class ReEnterPasscodeViewModel : ObservableObject
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

        private string _enterPasscodeCountDown;
        public string EnterPasscodeCountDown
        {
            get { return _enterPasscodeCountDown; }
            set { SetProperty(ref _enterPasscodeCountDown, value); }
        }

        private DateTime _dtEnteredPasscodeCountDown;
        private readonly ISettingsService _settings;

        public ReEnterPasscodeViewModel(ISettingsService settings)
        {
            StrongReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
            _settings = settings;
        }

        private void ValidatePasscode()
        {
            if (EnteredPasscode.Equals(_settings.Passcode))
            {
                StrongReferenceMessenger.Default.Send(new NavigationMessage { ReLoadSettingsView = true });
            }
        }

        private void CountDownReEnteredPasscode()
        {
            EnteredPasscode = "";
            _dtEnteredPasscodeCountDown = DateTime.Now.AddSeconds(Constants.ReEnterPasscodeTimeoutSeconds);

            Task.Run(async () =>
            {
                double elapsedSeconds = _dtEnteredPasscodeCountDown.Subtract(DateTime.Now).TotalSeconds;

                while (elapsedSeconds > 0)
                {
                    elapsedSeconds = _dtEnteredPasscodeCountDown.Subtract(DateTime.Now).TotalSeconds;
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        EnterPasscodeCountDown = $"{(int)elapsedSeconds}";
                    });
                }
            });
        }

        private void HandleMessage(NavigationMessage message)
        {
            if(message.CloseSettingsView)
                CountDownReEnteredPasscode();
        }

        private RelayCommand _navigateToHomeCommand = null;
        public RelayCommand NavigateToHomeCommand
        {
            get
            {
                return _navigateToHomeCommand ?? (_navigateToHomeCommand = new RelayCommand(() => { CountDownReEnteredPasscode(); }));
            }
        }
    }
}
