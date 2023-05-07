using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;

namespace SpeechlyTouch.ViewModels
{
    public class ConsentViewModel : ObservableObject
    {

        private void CloseConsentDialog()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseConsentDialog = true });
        }

        private void AcceptConsent()
        {
            StrongReferenceMessenger.Default.Send(new NavigationMessage { AcceptConsent = true });
        }

        private RelayCommand _closeConsentCommand = null;
        public RelayCommand CloseConsentCommand
        {
            get
            {
                return _closeConsentCommand ?? (_closeConsentCommand = new RelayCommand(() => { CloseConsentDialog(); }));
            }
        }

        private RelayCommand _acceptConsentCommand = null;
        public RelayCommand AcceptConsentCommand
        {
            get
            {
                return _acceptConsentCommand ?? (_acceptConsentCommand = new RelayCommand(() => { AcceptConsent(); }));
            }
        }
    }
}
