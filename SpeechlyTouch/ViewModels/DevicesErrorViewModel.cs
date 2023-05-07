using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;

namespace SpeechlyTouch.ViewModels
{
    public class DevicesErrorViewModel : ObservableObject
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                SetProperty(ref _errorMessage, value);
            }
        }

        public DevicesErrorViewModel()
        {
            StrongReferenceMessenger.Default.Register<DevicesMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
        }

        private void HandleMessage(DevicesMessage message)
        {
            if (message.ShowDevicesErrorDialog)
                ErrorMessage = message.DevicesErrorMessage;
        }

        void CloseDialog()
        {
            StrongReferenceMessenger.Default.Send(new DevicesMessage { CloseDevicesErrorDialog = true });
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() => { CloseDialog(); }));
            }
        }
    }
}
