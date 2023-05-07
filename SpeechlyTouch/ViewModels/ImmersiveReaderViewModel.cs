using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using SpeechlyTouch.Messages;
using SpeechlyTouch.Services.AuditTracking;
using SpeechlyTouch.Services.Settings;
using System;
using System.Collections.Generic;

namespace SpeechlyTouch.ViewModels
{
    public class ImmersiveReaderViewModel : ObservableObject
    {
        private string _immersiveReaderViewmessage;
        public string ImmersiveReaderViewmessage
        {
            get { return _immersiveReaderViewmessage; }
            set { SetProperty(ref _immersiveReaderViewmessage, value); }
        }

        private Uri _sourceUri;
        public Uri SourceUri
        {
            get { return _sourceUri; }
            set { SetProperty(ref _sourceUri, value); }
        }

        private readonly ISettingsService _settings;
        private readonly IAppAnalytics _appAnalytics;
        public ImmersiveReaderViewModel(ISettingsService settings, IAppAnalytics appAnalytics)
        {
            _settings = settings;
            _appAnalytics = appAnalytics;
            StrongReferenceMessenger.Default.Register<ImmersiveReaderNavigationMessage>(this, (r, m) =>
            {
                HandleMessage(m);
            });
        }

        private async void HandleMessage(ImmersiveReaderNavigationMessage m)
        {
            ImmersiveReaderViewmessage = m.Message;
            SourceUri = m.SourceUri;

            if (!string.IsNullOrWhiteSpace(SourceUri.AbsoluteUri))
            {
                var user = await _settings.GetUser();

                _appAnalytics.CaptureCustomEvent("Immersive Reader Launched",
                   new Dictionary<string, string> {
                        {"User", user?.UserEmail },
                        {"Organisation", user?.Organization },
                        {"App Version", Constants.GetSoftwareVersion() }
                   });
            }
        }

        private RelayCommand _closeDialogCommand = null;
        public RelayCommand CloseDialogCommand
        {
            get
            {
                return _closeDialogCommand ?? (_closeDialogCommand = new RelayCommand(() =>
                {
                    ImmersiveReaderViewmessage = "";
                    SourceUri = new Uri("about:blank");
                    StrongReferenceMessenger.Default.Send(new NavigationMessage { CloseImmersiveReader = true, HideShellTitleView = true });
                }));
            }
        }
    }
}
