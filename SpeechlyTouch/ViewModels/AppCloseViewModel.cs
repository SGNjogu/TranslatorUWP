using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using Windows.ApplicationModel.Core;

namespace SpeechlyTouch.ViewModels
{
    public class AppCloseViewModel : ObservableObject
    {

        public void Dismiss()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseConfirmAppCloseDialog = true });
        }

        public void CloseApp()
        {
            CoreApplication.Exit();
        }

        private RelayCommand _dismissAppCloseCommand = null;

        public RelayCommand DismissAppCloseCommand
        {
            get
            {
                return _dismissAppCloseCommand ?? (_dismissAppCloseCommand = new RelayCommand(() => { Dismiss(); }));
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
