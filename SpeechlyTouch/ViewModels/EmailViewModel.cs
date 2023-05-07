using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.Settings;
using Windows.UI.Xaml;

namespace SpeechlyTouch.ViewModels
{
    public class EmailViewModel : ObservableObject
    {
        private string _providedEmailAddress;
        public string ProvidedEmailAddress
        {
            get { return _providedEmailAddress; }
            set
            {
                SetProperty(ref _providedEmailAddress, value);
            }
        }

        private Visibility _errorMessageVisibility;
        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set { SetProperty(ref _errorMessageVisibility, value); }
        }

        private readonly ISettingsService _settings;
        private readonly ISessionService _sessionService;

        public EmailViewModel(ISettingsService settings, ISessionService sessionService)
        {
            _settings = settings;
            _sessionService = sessionService;
            ErrorMessageVisibility = Visibility.Collapsed;
        }

        public void SendEmail()
        {
            if (IsValidEmail(ProvidedEmailAddress))
            {
                string providedEmail = ProvidedEmailAddress.Clone().ToString();
                StrongReferenceMessenger.Default.Send(new EmailMessage { CloseEmailPopup = true, SendEmail = true, EmailingAddress = providedEmail });
                ProvidedEmailAddress = null;
            }
            else
            {
                ErrorMessageVisibility = Visibility.Visible;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() => { StrongReferenceMessenger.Default.Send(new EmailMessage { CloseEmailPopup = true }); }));
            }
        }

        private RelayCommand _sendEmailCommand = null;
        public RelayCommand SendEmailCommand
        {
            get
            {
                return _sendEmailCommand ?? (_sendEmailCommand = new RelayCommand(() => SendEmail()));
            }
        }
    }
}
