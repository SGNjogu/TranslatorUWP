using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Enums;
using SpeechlyTouch.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechlyTouch.ViewModels
{
    public class ErrorViewModel : ObservableObject
    {
        private string _notificationMessage;
        public string NotificationMessage
        {
            get { return _notificationMessage; }
            set { SetProperty(ref _notificationMessage, value); }
        }

        private SettingsType _settingsType;
        public SettingsType SettingsType
        {
            get { return _settingsType; }
            set { SetProperty(ref _settingsType, value); }
        }

        public ErrorViewModel()
        {
            StrongReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
        }

        private void HandleMessage(ErrorMessage m)
        {
            if(m.SettingsType == SettingsType.Questions)
            {
               SettingsType = SettingsType.Questions;
            }
            if (m.SettingsType == SettingsType.Profile)
            {
                SettingsType = SettingsType.Profile;
            }
            if (m.SettingsType == SettingsType.Language)
            {
                SettingsType = SettingsType.Language;
            }
            if (m.SettingsType == SettingsType.Devices)
            {
                SettingsType = SettingsType.Devices;
            }
        }

        private void CloseErrorDialog()
        {
            StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true, DisplayDialog = false  });
        }

        private void SaveSettings()
        {
            if(SettingsType == SettingsType.Questions)
            {
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true, DisplayDialog = false, SettingsType = SettingsType.Questions });
                
            }
            if (SettingsType == SettingsType.Language)
            {
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true, DisplayDialog = false, SettingsType = SettingsType.Language });

            }
            if (SettingsType == SettingsType.Profile)
            {
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true, DisplayDialog = false, SettingsType = SettingsType.Profile });

            }
            if (SettingsType == SettingsType.Devices)
            {
                StrongReferenceMessenger.Default.Send(new ErrorMessage { EnableNavigation = true, DisplayDialog = false, SettingsType = SettingsType.Devices });

            }
        }

        private RelayCommand _saveSettingsCommand = null;
        public RelayCommand SaveSettingsCommand
        {
            get
            {
                return _saveSettingsCommand ?? (_saveSettingsCommand = new RelayCommand(() => { SaveSettings(); }));
            }
        }

        private RelayCommand _closeErrorDialogCommand = null;
        public RelayCommand CloseErrorDialogCommand
        {
            get
            {
                return _closeErrorDialogCommand ?? (_closeErrorDialogCommand = new RelayCommand(() => { CloseErrorDialog(); }));
            }
        }

      
    }
}
